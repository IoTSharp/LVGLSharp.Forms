using System;

namespace LVGLSharp.Runtime.Linux;

internal sealed class WaylandWindow : IDisposable
{
    private IntPtr _compositor;
    private IntPtr _surface;
    private IntPtr _xdgWmBase;
    private IntPtr _xdgSurface;
    private IntPtr _xdgToplevel;

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
                WaylandNative.SetXdgToplevelTitle(_xdgToplevel, Title);
                WaylandNative.CommitSurface(_surface);
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
            ? $"Compositor={compositorGlobal.Name}:{compositorGlobal.Version}, Surface={_surface != IntPtr.Zero}, XdgWmBase={_xdgWmBase != IntPtr.Zero}, XdgSurface={_xdgSurface != IntPtr.Zero}, XdgToplevel={_xdgToplevel != IntPtr.Zero}, Borderless={Borderless}"
            : $"Compositor={compositorGlobal.Name}:{compositorGlobal.Version}, Surface={_surface != IntPtr.Zero}, XdgWmBase=<missing>, XdgSurface={_xdgSurface != IntPtr.Zero}, XdgToplevel={_xdgToplevel != IntPtr.Zero}, Borderless={Borderless}";
    }

    public void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);
    }

    public void Dispose()
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
        HasSurfacePrerequisites = false;
        HasXdgShellPrerequisites = false;
        IsNativeSurfaceInitialized = false;
        IsXdgSurfaceInitialized = false;
        IsXdgToplevelInitialized = false;
        InitializationSummary = null;
        IsDisposed = true;
    }
}
