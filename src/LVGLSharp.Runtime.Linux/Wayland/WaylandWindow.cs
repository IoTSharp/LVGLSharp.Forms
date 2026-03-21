using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace LVGLSharp.Runtime.Linux;

internal sealed unsafe class WaylandWindow : IDisposable
{
    private IntPtr _compositor;
    private IntPtr _surface;
    private IntPtr _xdgWmBase;
    private IntPtr _xdgSurface;
    private IntPtr _xdgToplevel;
    private GCHandle _shellListenerStateHandle;

    private static readonly WaylandNative.XdgWmBaseListener s_xdgWmBaseListener = new(&HandleXdgWmBasePing);
    private static readonly WaylandNative.XdgSurfaceListener s_xdgSurfaceListener = new(&HandleXdgSurfaceConfigure);
    private static readonly WaylandNative.XdgToplevelListener s_xdgToplevelListener = new(&HandleXdgToplevelConfigure, &HandleXdgToplevelClose);

    public WaylandWindow(string title, int width, int height, bool borderless)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);

        Title = title;
        Width = width;
        Height = height;
        Borderless = borderless;
    }

    public string Title { get; }

    public int Width { get; }

    public int Height { get; }

    public bool Borderless { get; }

    public bool HasSurfacePrerequisites { get; private set; }

    public bool HasXdgShellPrerequisites { get; private set; }

    public bool IsNativeSurfaceInitialized { get; private set; }

    public bool IsXdgSurfaceInitialized { get; private set; }

    public bool IsXdgToplevelInitialized { get; private set; }

    public bool HasReceivedConfigure { get; private set; }

    public bool HasAcknowledgedConfigure { get; private set; }

    public bool HasRespondedToPing { get; private set; }

    public uint LastConfigureSerial { get; private set; }

    public uint LastPingSerial { get; private set; }

    public bool IsCloseRequested { get; private set; }

    public int LastConfiguredWidth { get; private set; }

    public int LastConfiguredHeight { get; private set; }

    public IntPtr SurfaceProxy => _surface;

    public string? InitializationSummary { get; private set; }

    public bool IsDisposed { get; private set; }

    public void InitializeSurface(WaylandDisplayConnection connection)
    {
        ArgumentNullException.ThrowIfNull(connection);

        ThrowIfDisposed();
        connection.ThrowIfDisposed();

        if (!connection.IsConnected)
        {
            throw new InvalidOperationException($"Wayland display is not connected. {connection.DiagnosticSummary}");
        }

        if (!connection.TryGetGlobal("wl_compositor", out var compositorGlobal))
        {
            throw new InvalidOperationException($"Wayland compositor global is unavailable. {connection.DiagnosticSummary}");
        }

        if (IsNativeSurfaceInitialized)
        {
            return;
        }

        _compositor = connection.BindCompositor();
        try
        {
            _surface = WaylandNative.CreateSurface(_compositor);
            HasXdgShellPrerequisites = connection.TryGetGlobal("xdg_wm_base", out var xdgWmBaseGlobal);
            if (HasXdgShellPrerequisites)
            {
                _xdgWmBase = connection.BindXdgWmBase();
                _xdgSurface = WaylandNative.CreateXdgSurface(_xdgWmBase, _surface);
                _xdgToplevel = WaylandNative.CreateXdgToplevel(_xdgSurface);
                EnsureShellListenersAttached();
                WaylandNative.SetXdgToplevelTitle(_xdgToplevel, Title);
                WaylandNative.CommitSurface(_surface);
                connection.PumpEvents();
            }
        }
        catch
        {
            WaylandNative.DestroyProxy(_xdgToplevel);
            WaylandNative.DestroyProxy(_xdgSurface);
            WaylandNative.DestroyProxy(_xdgWmBase);
            WaylandNative.DestroyProxy(_surface);
            WaylandNative.DestroyProxy(_compositor);
            _xdgToplevel = IntPtr.Zero;
            _xdgSurface = IntPtr.Zero;
            _xdgWmBase = IntPtr.Zero;
            _surface = IntPtr.Zero;
            _compositor = IntPtr.Zero;
            throw;
        }

        HasSurfacePrerequisites = true;
        IsNativeSurfaceInitialized = _surface != IntPtr.Zero;
        IsXdgSurfaceInitialized = _xdgSurface != IntPtr.Zero;
        IsXdgToplevelInitialized = _xdgToplevel != IntPtr.Zero;
        InitializationSummary = HasXdgShellPrerequisites
            ? $"Compositor={compositorGlobal.Name}:{compositorGlobal.Version}, Surface={_surface != IntPtr.Zero}, XdgWmBase={_xdgWmBase != IntPtr.Zero}, XdgSurface={_xdgSurface != IntPtr.Zero}, XdgToplevel={_xdgToplevel != IntPtr.Zero}, Configured={HasReceivedConfigure}, Acked={HasAcknowledgedConfigure}, Pong={HasRespondedToPing}, Borderless={Borderless}"
            : $"Compositor={compositorGlobal.Name}:{compositorGlobal.Version}, Surface={_surface != IntPtr.Zero}, XdgWmBase=<missing>, XdgSurface={_xdgSurface != IntPtr.Zero}, XdgToplevel={_xdgToplevel != IntPtr.Zero}, Configured={HasReceivedConfigure}, Acked={HasAcknowledgedConfigure}, Pong={HasRespondedToPing}, Borderless={Borderless}";
    }

    public void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);
    }

    public void Dispose()
    {
        if (_shellListenerStateHandle.IsAllocated)
        {
            _shellListenerStateHandle.Free();
        }

        WaylandNative.DestroyProxy(_xdgToplevel);
        WaylandNative.DestroyProxy(_xdgSurface);
        WaylandNative.DestroyProxy(_xdgWmBase);
        WaylandNative.DestroyProxy(_surface);
        WaylandNative.DestroyProxy(_compositor);
        _xdgToplevel = IntPtr.Zero;
        _xdgSurface = IntPtr.Zero;
        _xdgWmBase = IntPtr.Zero;
        _surface = IntPtr.Zero;
        _compositor = IntPtr.Zero;
        HasSurfacePrerequisites = false;
        HasXdgShellPrerequisites = false;
        IsNativeSurfaceInitialized = false;
        IsXdgSurfaceInitialized = false;
        IsXdgToplevelInitialized = false;
        HasReceivedConfigure = false;
        HasAcknowledgedConfigure = false;
        HasRespondedToPing = false;
        LastConfigureSerial = 0;
        LastPingSerial = 0;
        IsCloseRequested = false;
        LastConfiguredWidth = 0;
        LastConfiguredHeight = 0;
        InitializationSummary = null;
        IsDisposed = true;
    }

    private void EnsureShellListenersAttached()
    {
        if (_xdgWmBase == IntPtr.Zero || _xdgSurface == IntPtr.Zero || _xdgToplevel == IntPtr.Zero)
        {
            throw new InvalidOperationException("Wayland xdg shell proxies are not ready for listener attachment.");
        }

        if (!_shellListenerStateHandle.IsAllocated)
        {
            _shellListenerStateHandle = GCHandle.Alloc(this);
        }

        var listenerState = GCHandle.ToIntPtr(_shellListenerStateHandle);

        fixed (WaylandNative.XdgWmBaseListener* xdgWmBaseListener = &s_xdgWmBaseListener)
        {
            var result = WaylandNative.AddXdgWmBaseListener(_xdgWmBase, xdgWmBaseListener, listenerState);
            if (result != 0)
            {
                throw new InvalidOperationException("Unable to attach Wayland xdg_wm_base listener.");
            }
        }

        fixed (WaylandNative.XdgSurfaceListener* xdgSurfaceListener = &s_xdgSurfaceListener)
        {
            var result = WaylandNative.AddXdgSurfaceListener(_xdgSurface, xdgSurfaceListener, listenerState);
            if (result != 0)
            {
                throw new InvalidOperationException("Unable to attach Wayland xdg_surface listener.");
            }
        }

        fixed (WaylandNative.XdgToplevelListener* xdgToplevelListener = &s_xdgToplevelListener)
        {
            var result = WaylandNative.AddXdgToplevelListener(_xdgToplevel, xdgToplevelListener, listenerState);
            if (result != 0)
            {
                throw new InvalidOperationException("Unable to attach Wayland xdg_toplevel listener.");
            }
        }
    }

    private void UpdateInitializationSummary()
    {
        InitializationSummary = HasXdgShellPrerequisites
            ? $"Surface={_surface != IntPtr.Zero}, XdgWmBase={_xdgWmBase != IntPtr.Zero}, XdgSurface={_xdgSurface != IntPtr.Zero}, XdgToplevel={_xdgToplevel != IntPtr.Zero}, Configured={HasReceivedConfigure}:{LastConfigureSerial}, Acked={HasAcknowledgedConfigure}, Pong={HasRespondedToPing}:{LastPingSerial}, ToplevelSize={LastConfiguredWidth}x{LastConfiguredHeight}, Close={IsCloseRequested}, Borderless={Borderless}"
            : $"Surface={_surface != IntPtr.Zero}, XdgWmBase=<missing>, XdgSurface={_xdgSurface != IntPtr.Zero}, XdgToplevel={_xdgToplevel != IntPtr.Zero}, Configured={HasReceivedConfigure}:{LastConfigureSerial}, Acked={HasAcknowledgedConfigure}, Pong={HasRespondedToPing}:{LastPingSerial}, ToplevelSize={LastConfiguredWidth}x{LastConfiguredHeight}, Close={IsCloseRequested}, Borderless={Borderless}";
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static void HandleXdgWmBasePing(IntPtr data, IntPtr xdgWmBase, uint serial)
    {
        var handle = GCHandle.FromIntPtr(data);
        if (handle.Target is not WaylandWindow window)
        {
            return;
        }

        WaylandNative.PongXdgWmBase(xdgWmBase, serial);
        window.LastPingSerial = serial;
        window.HasRespondedToPing = true;
        window.UpdateInitializationSummary();
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static void HandleXdgSurfaceConfigure(IntPtr data, IntPtr xdgSurface, uint serial)
    {
        var handle = GCHandle.FromIntPtr(data);
        if (handle.Target is not WaylandWindow window)
        {
            return;
        }

        WaylandNative.AckXdgSurfaceConfigure(xdgSurface, serial);
        WaylandNative.CommitSurface(window._surface);
        window.LastConfigureSerial = serial;
        window.HasReceivedConfigure = true;
        window.HasAcknowledgedConfigure = true;
        window.UpdateInitializationSummary();
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static void HandleXdgToplevelConfigure(IntPtr data, IntPtr xdgToplevel, int width, int height, IntPtr states)
    {
        var handle = GCHandle.FromIntPtr(data);
        if (handle.Target is not WaylandWindow window)
        {
            return;
        }

        window.LastConfiguredWidth = width;
        window.LastConfiguredHeight = height;
        window.UpdateInitializationSummary();
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static void HandleXdgToplevelClose(IntPtr data, IntPtr xdgToplevel)
    {
        var handle = GCHandle.FromIntPtr(data);
        if (handle.Target is not WaylandWindow window)
        {
            return;
        }

        window.IsCloseRequested = true;
        window.UpdateInitializationSummary();
    }
}
