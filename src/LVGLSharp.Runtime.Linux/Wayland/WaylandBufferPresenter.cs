using LVGLSharp.Interop;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace LVGLSharp.Runtime.Linux;

internal unsafe sealed partial class WaylandBufferPresenter : IDisposable
{
    private const int O_CREAT = 0x0040;
    private const int O_RDWR = 0x0002;
    private const uint PROT_READ = 0x1;
    private const uint PROT_WRITE = 0x2;
    private const uint MAP_SHARED = 0x01;

    private IntPtr _sharedMemory;
    private IntPtr _sharedMemoryPool;
    private IntPtr _sharedMemoryBuffer;
    private int _sharedMemoryFileDescriptor = -1;
    private int _stride;
    private string? _sharedMemoryName;
    private byte* _drawBuffer;
    private uint _drawBufferByteSize;
    private GCHandle _bufferListenerStateHandle;

    private static readonly WaylandNative.WlBufferListener s_bufferListener = new(&HandleBufferRelease);

    [LibraryImport("libc", EntryPoint = "shm_open", StringMarshalling = StringMarshalling.Utf8)]
    private static partial int ShmOpen(string name, int oflag, uint mode);

    [LibraryImport("libc", EntryPoint = "shm_unlink", StringMarshalling = StringMarshalling.Utf8)]
    private static partial int ShmUnlink(string name);

    [LibraryImport("libc", EntryPoint = "ftruncate")]
    private static partial int Ftruncate(int fd, nint length);

    [LibraryImport("libc", EntryPoint = "mmap")]
    private static partial IntPtr Mmap(IntPtr addr, nuint length, uint prot, uint flags, int fd, nint offset);

    [LibraryImport("libc", EntryPoint = "munmap")]
    private static partial int Munmap(IntPtr addr, nuint length);

    [LibraryImport("libc", EntryPoint = "close")]
    private static partial int Close(int fd);

    public WaylandBufferPresenter(int pixelWidth, int pixelHeight, float dpi)
    {
        PixelWidth = pixelWidth;
        PixelHeight = pixelHeight;
        Dpi = dpi;
        IsBufferReleased = true;
    }

    public int PixelWidth { get; private set; }

    public int PixelHeight { get; private set; }

    public float Dpi { get; }

    public byte* DrawBuffer => _drawBuffer;

    public uint DrawBufferByteSize => _drawBufferByteSize;

    public IntPtr SharedMemoryBuffer => _sharedMemoryBuffer;

    public bool IsBufferReleased { get; private set; }

    public uint BufferReleaseCount { get; private set; }

    public uint SkippedFlushCount { get; private set; }

    public uint FlushCount { get; private set; }

    public int LastFlushWidth { get; private set; }

    public int LastFlushHeight { get; private set; }

    public bool HasAllocatedBuffer => _drawBuffer != null;

    public bool HasSharedMemoryBuffer => _sharedMemoryBuffer != IntPtr.Zero;

    public bool IsDisposed { get; private set; }

    public void Initialize()
    {
        ThrowIfDisposed();

        EnsureDrawBuffer();
    }

    public void InitializeSharedMemory(WaylandDisplayConnection connection)
    {
        ArgumentNullException.ThrowIfNull(connection);

        ThrowIfDisposed();
        connection.ThrowIfDisposed();

        if (_sharedMemoryBuffer != IntPtr.Zero)
        {
            return;
        }

        var sharedMemoryProxy = connection.BindSharedMemory();
        try
        {
            _stride = checked(PixelWidth * sizeof(uint));
            var sharedMemorySize = checked(_stride * PixelHeight);
            _sharedMemoryName = $"/lvglsharp-wayland-{Guid.NewGuid():N}";
            _sharedMemoryFileDescriptor = ShmOpen(_sharedMemoryName, O_CREAT | O_RDWR, 0x180);
            if (_sharedMemoryFileDescriptor < 0)
            {
                throw new InvalidOperationException("Unable to create Wayland shared memory object.");
            }

            if (Ftruncate(_sharedMemoryFileDescriptor, sharedMemorySize) != 0)
            {
                throw new InvalidOperationException("Unable to size Wayland shared memory object.");
            }

            _sharedMemory = Mmap(IntPtr.Zero, (nuint)sharedMemorySize, PROT_READ | PROT_WRITE, MAP_SHARED, _sharedMemoryFileDescriptor, 0);
            if (_sharedMemory == IntPtr.Zero || _sharedMemory == new IntPtr(-1))
            {
                throw new InvalidOperationException("Unable to map Wayland shared memory buffer.");
            }

            _sharedMemoryPool = WaylandNative.CreateSharedMemoryPool(sharedMemoryProxy, _sharedMemoryFileDescriptor, sharedMemorySize);
            _sharedMemoryBuffer = WaylandNative.CreateSharedMemoryBuffer(_sharedMemoryPool, PixelWidth, PixelHeight, _stride);
            AttachBufferReleaseListener();
            IsBufferReleased = true;
        }
        finally
        {
            WaylandNative.DestroyProxy(sharedMemoryProxy);
        }
    }

    public bool ResizeIfNeeded(WaylandDisplayConnection connection, int pixelWidth, int pixelHeight)
    {
        ArgumentNullException.ThrowIfNull(connection);

        ThrowIfDisposed();

        if (pixelWidth <= 0 || pixelHeight <= 0)
        {
            return false;
        }

        if (pixelWidth == PixelWidth && pixelHeight == PixelHeight)
        {
            return false;
        }

        ReleaseSharedMemoryResources();
        ReleaseDrawBuffer();

        PixelWidth = pixelWidth;
        PixelHeight = pixelHeight;

        Initialize();
        InitializeSharedMemory(connection);
        return true;
    }

    public void Flush(lv_display_t* display, lv_area_t* area, IntPtr surfaceProxy)
    {
        ThrowIfDisposed();

        if (area != null)
        {
            LastFlushWidth = lv_area_get_width(area);
            LastFlushHeight = lv_area_get_height(area);
        }

        FlushCount++;

        if (!IsBufferReleased)
        {
            SkippedFlushCount++;
            lv_display_flush_ready(display);
            return;
        }

        if (_sharedMemory != IntPtr.Zero && area != null)
        {
            CopyRgb565ToXrgb8888(area);
        }

        if (surfaceProxy != IntPtr.Zero && _sharedMemoryBuffer != IntPtr.Zero)
        {
            WaylandNative.AttachBuffer(surfaceProxy, _sharedMemoryBuffer, 0, 0);
            WaylandNative.DamageBuffer(surfaceProxy, area->x1, area->y1, LastFlushWidth, LastFlushHeight);
            WaylandNative.CommitSurface(surfaceProxy);
            IsBufferReleased = false;
        }

        lv_display_flush_ready(display);
    }

    private void EnsureDrawBuffer()
    {
        if (_drawBuffer != null)
        {
            return;
        }

        _drawBufferByteSize = checked((uint)(PixelWidth * PixelHeight * sizeof(ushort)));
        _drawBuffer = (byte*)NativeMemory.AllocZeroed((nuint)_drawBufferByteSize);
        if (_drawBuffer == null)
        {
            throw new OutOfMemoryException("Wayland draw buffer allocation failed.");
        }
    }

    private void AttachBufferReleaseListener()
    {
        if (_sharedMemoryBuffer == IntPtr.Zero)
        {
            throw new InvalidOperationException("Wayland shared memory buffer is not ready for release listener attachment.");
        }

        if (!_bufferListenerStateHandle.IsAllocated)
        {
            _bufferListenerStateHandle = GCHandle.Alloc(this);
        }

        var listenerState = GCHandle.ToIntPtr(_bufferListenerStateHandle);
        fixed (WaylandNative.WlBufferListener* bufferListener = &s_bufferListener)
        {
            var result = WaylandNative.AddBufferListener(_sharedMemoryBuffer, bufferListener, listenerState);
            if (result != 0)
            {
                throw new InvalidOperationException("Unable to attach Wayland wl_buffer listener.");
            }
        }
    }

    private void CopyRgb565ToXrgb8888(lv_area_t* area)
    {
        var destination = (uint*)_sharedMemory;
        var width = lv_area_get_width(area);
        var height = lv_area_get_height(area);
        var source = (ushort*)_drawBuffer;

        for (var y = 0; y < height; y++)
        {
            var destinationRow = destination + (area->y1 + y) * PixelWidth + area->x1;
            var sourceRow = source + y * width;
            for (var x = 0; x < width; x++)
            {
                destinationRow[x] = ConvertRgb565ToXrgb8888(sourceRow[x]);
            }
        }
    }

    private static uint ConvertRgb565ToXrgb8888(ushort pixel)
    {
        uint r = (uint)((pixel >> 11) & 0x1F);
        uint g = (uint)((pixel >> 5) & 0x3F);
        uint b = (uint)(pixel & 0x1F);

        r = (r << 3) | (r >> 2);
        g = (g << 2) | (g >> 4);
        b = (b << 3) | (b >> 2);

        return 0xFF000000u | (r << 16) | (g << 8) | b;
    }

    public void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);
    }

    public void Dispose()
    {
        ReleaseSharedMemoryResources();
        ReleaseDrawBuffer();

        if (_bufferListenerStateHandle.IsAllocated)
        {
            _bufferListenerStateHandle.Free();
        }

        _stride = 0;
        _drawBufferByteSize = 0;
        FlushCount = 0;
        BufferReleaseCount = 0;
        SkippedFlushCount = 0;
        IsBufferReleased = true;
        LastFlushWidth = 0;
        LastFlushHeight = 0;
        IsDisposed = true;
    }

    private void ReleaseSharedMemoryResources()
    {
        if (_sharedMemory != IntPtr.Zero)
        {
            _ = Munmap(_sharedMemory, (nuint)checked(_stride * PixelHeight));
            _sharedMemory = IntPtr.Zero;
        }

        WaylandNative.DestroyProxy(_sharedMemoryBuffer);
        WaylandNative.DestroyProxy(_sharedMemoryPool);
        _sharedMemoryBuffer = IntPtr.Zero;
        _sharedMemoryPool = IntPtr.Zero;

        if (_sharedMemoryFileDescriptor >= 0)
        {
            _ = Close(_sharedMemoryFileDescriptor);
            _sharedMemoryFileDescriptor = -1;
        }

        if (!string.IsNullOrWhiteSpace(_sharedMemoryName))
        {
            _ = ShmUnlink(_sharedMemoryName);
            _sharedMemoryName = null;
        }

        IsBufferReleased = true;
    }

    private void ReleaseDrawBuffer()
    {
        if (_drawBuffer != null)
        {
            NativeMemory.Free(_drawBuffer);
            _drawBuffer = null;
        }

        _drawBufferByteSize = 0;
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static void HandleBufferRelease(IntPtr data, IntPtr buffer)
    {
        var handle = GCHandle.FromIntPtr(data);
        if (handle.Target is not WaylandBufferPresenter presenter)
        {
            return;
        }

        presenter.IsBufferReleased = true;
        presenter.BufferReleaseCount++;
    }
}
