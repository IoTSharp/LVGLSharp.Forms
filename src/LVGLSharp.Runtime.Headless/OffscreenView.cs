using LVGLSharp;
using LVGLSharp.Interop;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace LVGLSharp.Runtime.Headless;

public unsafe sealed class OffscreenView : ViewLifetimeBase
{
    private readonly OffscreenOptions _options;
    private bool _running;
    private bool _initialized;
    private lv_display_t* _lvDisplay;
    private lv_obj_t* _root;
    private lv_group_t* _keyInputGroup;
    private byte* _drawBuffer;
    private uint _drawBufferByteSize;
    private uint* _frameBuffer;
    private GCHandle _selfHandle;

    public OffscreenView()
        : this(OffscreenOptions.Default)
    {
    }

    public OffscreenView(OffscreenOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _options.Validate();
    }

    public OffscreenView(int width, int height, float dpi)
        : this(new OffscreenOptions
        {
            Width = width,
            Height = height,
            Dpi = dpi,
        })
    {
    }

    public OffscreenOptions Options => _options;

    public override lv_obj_t* Root => _root;

    public override lv_group_t* KeyInputGroup => _keyInputGroup;

    public override delegate* unmanaged[Cdecl]<lv_event_t*, void> SendTextAreaFocusCallback => null;

    protected override void OnOpenCore()
    {
        LvglNativeLibraryResolver.EnsureRegistered();

        if (!lv_is_initialized())
        {
            lv_init();
        }

        AllocateBuffers();

        _lvDisplay = lv_display_create(_options.Width, _options.Height);
        if (_lvDisplay == null)
        {
            throw new InvalidOperationException("Offscreen LVGL display ?????");
        }

        if (!_selfHandle.IsAllocated)
        {
            _selfHandle = GCHandle.Alloc(this);
        }

        lv_display_set_user_data(_lvDisplay, (void*)GCHandle.ToIntPtr(_selfHandle));
        lv_display_set_buffers(_lvDisplay, _drawBuffer, null, _drawBufferByteSize, LV_DISPLAY_RENDER_MODE_FULL);
        lv_display_set_flush_cb(_lvDisplay, &FlushCb);

        _root = lv_scr_act();
        ApplyBackgroundStyle();
        _keyInputGroup = lv_group_create();
        _running = true;
        _initialized = true;
    }

    public override void HandleEvents()
    {
        if (!_initialized)
        {
            return;
        }

        lv_timer_handler();
    }

    protected override void RunLoopCore(Action iteration)
    {
        while (_running)
        {
            HandleEvents();
            iteration?.Invoke();
            break;
        }
    }

    protected override void OnCloseCore()
    {
        _running = false;

        if (_keyInputGroup != null)
        {
            lv_group_delete(_keyInputGroup);
            _keyInputGroup = null;
        }

        if (_lvDisplay != null)
        {
            lv_display_delete(_lvDisplay);
            _lvDisplay = null;
        }

        if (_drawBuffer != null)
        {
            NativeMemory.Free(_drawBuffer);
            _drawBuffer = null;
            _drawBufferByteSize = 0;
        }

        if (_frameBuffer != null)
        {
            NativeMemory.Free(_frameBuffer);
            _frameBuffer = null;
        }

        if (_selfHandle.IsAllocated)
        {
            _selfHandle.Free();
        }

        _root = null;
        _initialized = false;
    }

    public override void RegisterTextInput(lv_obj_t* textArea)
    {
    }

    protected override bool CanSkipClose() => !_initialized && !_running && _lvDisplay == null;

    public void RenderFrame()
    {
        if (!_initialized)
        {
            throw new InvalidOperationException("OffscreenView ?????");
        }

        lv_timer_handler();
    }

    public Image<Rgba32> RenderSnapshot()
    {
        RenderFrame();
        return CaptureImage();
    }

    public void RenderSnapshotToPng(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        using var image = RenderSnapshot();
        image.Save(path);
    }

    public Image<Rgba32> CaptureImage()
    {
        if (!_initialized || _frameBuffer == null)
        {
            throw new InvalidOperationException("OffscreenView ????????");
        }

        var image = new Image<Rgba32>(_options.Width, _options.Height, _options.BackgroundColor);
        image.ProcessPixelRows(accessor =>
        {
            for (var y = 0; y < _options.Height; y++)
            {
                var row = accessor.GetRowSpan(y);
                var rowOffset = y * _options.Width;
                for (var x = 0; x < _options.Width; x++)
                {
                    var argb = _frameBuffer[rowOffset + x];
                    row[x] = new Rgba32(
                        (byte)((argb >> 16) & 0xFF),
                        (byte)((argb >> 8) & 0xFF),
                        (byte)(argb & 0xFF),
                        (byte)((argb >> 24) & 0xFF));
                }
            }
        });

        return image;
    }

    public void SavePng(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        using var image = CaptureImage();
        image.Save(path);
    }

    public void SaveSnapshot()
    {
        if (string.IsNullOrWhiteSpace(_options.OutputPath))
        {
            throw new InvalidOperationException("?? OffscreenOptions ??? OutputPath?");
        }

        RenderSnapshotToPng(_options.OutputPath);
    }

    private void ApplyBackgroundStyle()
    {
        if (_root == null)
        {
            return;
        }

        lv_obj_set_style_bg_opa(_root, 255, 0);
        lv_obj_set_style_bg_color(
            _root,
            lv_color_make(_options.BackgroundColor.R, _options.BackgroundColor.G, _options.BackgroundColor.B),
            0);
    }

    private void AllocateBuffers()
    {
        _drawBufferByteSize = checked((uint)(_options.Width * _options.Height * sizeof(ushort)));
        _drawBuffer = (byte*)NativeMemory.AllocZeroed((nuint)_drawBufferByteSize);
        if (_drawBuffer == null)
        {
            throw new OutOfMemoryException("Offscreen draw buffer ?????");
        }

        _frameBuffer = (uint*)NativeMemory.AllocZeroed((nuint)(_options.Width * _options.Height), (nuint)sizeof(uint));
        if (_frameBuffer == null)
        {
            throw new OutOfMemoryException("Offscreen frame buffer ?????");
        }
    }

    private static OffscreenView? GetViewFromDisplay(lv_display_t* display)
    {
        if (display == null)
        {
            return null;
        }

        var userData = lv_display_get_user_data(display);
        return userData == null ? null : (OffscreenView?)GCHandle.FromIntPtr((IntPtr)userData).Target;
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static void FlushCb(lv_display_t* display, lv_area_t* area, byte* pxMap)
    {
        var view = GetViewFromDisplay(display);
        if (view is null || view._frameBuffer == null)
        {
            lv_display_flush_ready(display);
            return;
        }

        var width = area->x2 - area->x1 + 1;
        var height = area->y2 - area->y1 + 1;
        var source = (ushort*)pxMap;

        for (var y = 0; y < height; y++)
        {
            var dstRow = (area->y1 + y) * view._options.Width + area->x1;
            for (var x = 0; x < width; x++)
            {
                var rgb565 = source[(y * width) + x];
                view._frameBuffer[dstRow + x] = ConvertRgb565ToArgb8888(rgb565);
            }
        }

        lv_display_flush_ready(display);
    }

    private static uint ConvertRgb565ToArgb8888(ushort rgb565)
    {
        var r = (byte)(((rgb565 >> 11) & 0x1F) * 255 / 31);
        var g = (byte)(((rgb565 >> 5) & 0x3F) * 255 / 63);
        var b = (byte)((rgb565 & 0x1F) * 255 / 31);
        return 0xFF000000u | ((uint)r << 16) | ((uint)g << 8) | b;
    }
}