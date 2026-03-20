using LVGLSharp.Interop;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.PixelFormats;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static LVGLSharp.Interop.LVGL;

using ImageSharpImage = SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgba32>;

namespace MusicDemo;

public unsafe sealed class frmMusicDemo : Form
{
    private const int DemoWidth = 478;
    private const int DemoHeight = 271;
    private const uint LvImageHeaderMagic = 0x19;
    private const string ScreenshotResourceName = "MusicDemo.Assets.screenshot1.gif";

    private readonly List<GifFrameDescriptor> _frames = new();

    private GCHandle _selfHandle;
    private lv_obj_t* _imageObject;
    private lv_timer_t* _frameTimer;
    private int _frameIndex;

    public frmMusicDemo()
    {
        Text = "LVGL Music Demo";
        ClientSize = new LVGLSharp.Drawing.Size(DemoWidth, DemoHeight);
        FormBorderStyle = FormBorderStyle.None;
        Load += OnMusicDemoLoad;
    }

    protected override void DestroyHandle()
    {
        CleanupAnimation();
        base.DestroyHandle();
    }

    private void OnMusicDemoLoad(object? sender, EventArgs e)
    {
        ConfigureRoot();
        LoadGifFrames();
        CreateImageView();
        StartAnimation();
    }

    private void ConfigureRoot()
    {
        var root = (lv_obj_t*)Handle;
        lv_obj_set_style_bg_color(root, lv_color_hex(0x000000), 0);
        lv_obj_set_style_bg_opa(root, LV_OPA_COVER, 0);
        lv_obj_set_style_border_width(root, 0, 0);
        lv_obj_set_style_pad_all(root, 0, 0);
        lv_obj_set_style_radius(root, 0, 0);
        lv_obj_remove_flag(root,
            lv_obj_flag_t.LV_OBJ_FLAG_SCROLLABLE |
            lv_obj_flag_t.LV_OBJ_FLAG_SCROLL_ELASTIC |
            lv_obj_flag_t.LV_OBJ_FLAG_SCROLL_MOMENTUM |
            lv_obj_flag_t.LV_OBJ_FLAG_SCROLL_CHAIN);
        lv_obj_set_scrollbar_mode(root, lv_scrollbar_mode_t.LV_SCROLLBAR_MODE_OFF);
    }

    private void LoadGifFrames()
    {
        if (_frames.Count > 0)
        {
            return;
        }

        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(ScreenshotResourceName)
            ?? throw new InvalidOperationException($"Embedded resource not found: {ScreenshotResourceName}");
        using ImageSharpImage gif = SixLabors.ImageSharp.Image.Load<Rgba32>(stream);

        for (int index = 0; index < gif.Frames.Count; index++)
        {
            using ImageSharpImage frameImage = gif.Frames.CloneFrame(index);
            int frameDelayCs = 4;

            if (frameImage.Frames.RootFrame.Metadata.TryGetGifMetadata(out var gifMetadata) &&
                gifMetadata is not null &&
                gifMetadata.FrameDelay > 0)
            {
                frameDelayCs = gifMetadata.FrameDelay;
            }

            _frames.Add(CreateDescriptor(frameImage, checked((uint)(frameDelayCs * 10))));
        }

        if (_frames.Count == 0)
        {
            throw new InvalidOperationException("The embedded music GIF does not contain any frames.");
        }
    }

    private void CreateImageView()
    {
        var root = (lv_obj_t*)Handle;
        _imageObject = lv_image_create(root);
        lv_obj_set_size(_imageObject, DemoWidth, DemoHeight);
        lv_obj_center(_imageObject);
        lv_image_set_inner_align(_imageObject, lv_image_align_t.LV_IMAGE_ALIGN_CENTER);
        ShowFrame(0);
    }

    private void StartAnimation()
    {
        if (_frames.Count <= 1)
        {
            return;
        }

        _selfHandle = GCHandle.Alloc(this);
        _frameTimer = lv_timer_create(&OnFrameTimer, _frames[0].DelayMs, (void*)GCHandle.ToIntPtr(_selfHandle));
    }

    private void ShowFrame(int frameIndex)
    {
        if (_imageObject == null || _frames.Count == 0)
        {
            return;
        }

        _frameIndex = frameIndex % _frames.Count;
        if (_frameIndex < 0)
        {
            _frameIndex += _frames.Count;
        }

        var frame = _frames[_frameIndex];
        lv_image_set_src(_imageObject, frame.Descriptor);

        if (_frameTimer != null)
        {
            lv_timer_set_period(_frameTimer, frame.DelayMs);
        }
    }

    private void AdvanceFrame()
    {
        if (_frames.Count <= 1)
        {
            return;
        }

        ShowFrame((_frameIndex + 1) % _frames.Count);
    }

    private void CleanupAnimation()
    {
        if (_frameTimer != null)
        {
            lv_timer_delete(_frameTimer);
            _frameTimer = null;
        }

        if (_selfHandle.IsAllocated)
        {
            _selfHandle.Free();
        }

        foreach (var frame in _frames)
        {
            frame.Dispose();
        }

        _frames.Clear();
        _imageObject = null;
        _frameIndex = 0;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static void OnFrameTimer(lv_timer_t* timer)
    {
        var userData = lv_timer_get_user_data(timer);
        if (userData == null)
        {
            return;
        }

        var handle = GCHandle.FromIntPtr((nint)userData);
        if (handle.Target is frmMusicDemo form)
        {
            form.AdvanceFrame();
        }
    }

    private static GifFrameDescriptor CreateDescriptor(ImageSharpImage image, uint delayMs)
    {
        byte[] rgbaBytes = GC.AllocateUninitializedArray<byte>(image.Width * image.Height * Unsafe.SizeOf<Rgba32>());
        byte[] bgraBytes = GC.AllocateUninitializedArray<byte>(rgbaBytes.Length);

        image.CopyPixelDataTo(MemoryMarshal.Cast<byte, Rgba32>(rgbaBytes.AsSpan()));
        ConvertRgbaToBgra(rgbaBytes, bgraBytes);

        nuint descriptorSize = (nuint)sizeof(lv_image_dsc_t);
        nuint totalSize = descriptorSize + (nuint)bgraBytes.Length;
        byte* buffer = (byte*)NativeMemory.Alloc(totalSize);
        if (buffer == null)
        {
            throw new OutOfMemoryException("Unable to allocate LVGL image buffer for GIF frame.");
        }

        var descriptor = (lv_image_dsc_t*)buffer;
        byte* data = buffer + descriptorSize;

        fixed (byte* source = bgraBytes)
        {
            Buffer.MemoryCopy(source, data, bgraBytes.Length, bgraBytes.Length);
        }

        *descriptor = new lv_image_dsc_t
        {
            header = new lv_image_header_t
            {
                magic = LvImageHeaderMagic,
                cf = (uint)lv_color_format_t.LV_COLOR_FORMAT_ARGB8888,
                flags = 0,
                w = (uint)image.Width,
                h = (uint)image.Height,
                stride = (uint)(image.Width * 4),
            },
            data_size = (uint)bgraBytes.Length,
            data = data,
        };

        return new GifFrameDescriptor((nint)buffer, descriptor, Math.Max(delayMs, 10u));
    }

    private static void ConvertRgbaToBgra(ReadOnlySpan<byte> rgbaBytes, Span<byte> bgraBytes)
    {
        for (int offset = 0; offset < rgbaBytes.Length; offset += 4)
        {
            bgraBytes[offset] = rgbaBytes[offset + 2];
            bgraBytes[offset + 1] = rgbaBytes[offset + 1];
            bgraBytes[offset + 2] = rgbaBytes[offset];
            bgraBytes[offset + 3] = rgbaBytes[offset + 3];
        }
    }

    private readonly struct GifFrameDescriptor : IDisposable
    {
        public GifFrameDescriptor(nint buffer, lv_image_dsc_t* descriptor, uint delayMs)
        {
            Buffer = buffer;
            Descriptor = descriptor;
            DelayMs = delayMs;
        }

        public nint Buffer { get; }

        public lv_image_dsc_t* Descriptor { get; }

        public uint DelayMs { get; }

        public void Dispose()
        {
            if (Buffer != nint.Zero)
            {
                NativeMemory.Free((void*)Buffer);
            }
        }
    }
}
