using LVGLSharp;
using LVGLSharp.Interop;
using System;

namespace LVGLSharp.Runtime.Linux;

public unsafe sealed class WaylandView : IView
{
    private readonly WaylandDisplayConnection _connection;
    private readonly WaylandWindow _window;
    private readonly WaylandInputSource _inputSource;
    private readonly WaylandBufferPresenter _bufferPresenter;
    private readonly X11View? _fallbackView;
    private readonly string _diagnosticSummary;
    private bool _initialized;

    public WaylandView(
        string title = "LVGLSharp Wayland",
        int width = 800,
        int height = 600,
        float dpi = 96f,
        string? waylandDisplayName = null,
        string? x11DisplayName = null,
        bool borderless = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);

        _diagnosticSummary = LinuxEnvironmentDetector.GetWaylandDiagnosticSummary(waylandDisplayName, x11DisplayName);
        _connection = new WaylandDisplayConnection(waylandDisplayName, _diagnosticSummary);
        _window = new WaylandWindow(title, width, height, borderless);
        _inputSource = new WaylandInputSource();
        _bufferPresenter = new WaylandBufferPresenter(width, height, dpi);

        if (!string.IsNullOrWhiteSpace(x11DisplayName))
        {
            _fallbackView = new X11View(
                LinuxEnvironmentDetector.FormatWaylandWindowTitle(title, waylandDisplayName, x11DisplayName),
                width,
                height,
                dpi,
                x11DisplayName,
                borderless);
        }
    }

    public static (int X, int Y) CurrentMousePosition => X11View.CurrentMousePosition;

    public static uint CurrentMouseButton => X11View.CurrentMouseButton;

    public lv_obj_t* Root => GetActiveView().Root;

    public lv_group_t* KeyInputGroup => GetActiveView().KeyInputGroup;

    public delegate* unmanaged[Cdecl]<lv_event_t*, void> SendTextAreaFocusCallback => GetActiveView().SendTextAreaFocusCallback;

    public void Init()
    {
        if (_initialized)
        {
            return;
        }

        _connection.Connect();

        if (_fallbackView is not null)
        {
            _fallbackView.Init();
            _initialized = true;
            return;
        }

        _window.InitializeSurface(_connection);
        _initialized = true;
        _connection.ThrowIfDisposed();
        throw CreateNativeHostNotImplementedException();
    }

    public void ProcessEvents()
    {
        GetActiveView().ProcessEvents();
    }

    public void StartLoop(Action handle)
    {
        GetActiveView().StartLoop(handle);
    }

    public void Stop()
    {
        try
        {
            _fallbackView?.Stop();
        }
        finally
        {
            _bufferPresenter.Dispose();
            _inputSource.Dispose();
            _window.Dispose();
            _connection.Dispose();
        }
    }

    public void AttachTextInput(lv_obj_t* textArea)
    {
        GetActiveView().AttachTextInput(textArea);
    }

    public override string ToString() => _fallbackView is null
        ? $"{_diagnosticSummary}, Mode=WaylandSkeleton, Connected={_connection.IsConnected}, Compositor={_connection.HasCompositor}, Shm={_connection.HasSharedMemory}, XdgWmBase={_connection.HasXdgWmBase}, NativeSurface={_window.IsNativeSurfaceInitialized}, XdgSurface={_window.IsXdgSurfaceInitialized}, XdgToplevel={_window.IsXdgToplevelInitialized}"
        : $"{_diagnosticSummary}, Mode=X11Fallback, Connected={_connection.IsConnected}, Compositor={_connection.HasCompositor}, Shm={_connection.HasSharedMemory}, XdgWmBase={_connection.HasXdgWmBase}, NativeSurface={_window.IsNativeSurfaceInitialized}, XdgSurface={_window.IsXdgSurfaceInitialized}, XdgToplevel={_window.IsXdgToplevelInitialized}";

    private IView GetActiveView()
    {
        if (_fallbackView is not null)
        {
            return _fallbackView;
        }

        _connection.ThrowIfDisposed();
        _window.ThrowIfDisposed();
        _inputSource.ThrowIfDisposed();
        _bufferPresenter.ThrowIfDisposed();

        throw CreateNativeHostNotImplementedException();
    }

    private InvalidOperationException CreateNativeHostNotImplementedException()
    {
        return new InvalidOperationException(
            $"Wayland native host is not implemented yet. {_connection.DiagnosticSummary}, Connected={_connection.IsConnected}, ConnectedDisplay={_connection.ConnectedDisplayName ?? "<default>"}, Compositor={_connection.HasCompositor}:{_connection.CompositorVersion}, Shm={_connection.HasSharedMemory}:{_connection.SharedMemoryVersion}, XdgWmBase={_connection.HasXdgWmBase}:{_connection.XdgWmBaseVersion}, Window={_window.Title}({_window.Width}x{_window.Height}), SurfaceReady={_window.HasSurfacePrerequisites}, NativeSurface={_window.IsNativeSurfaceInitialized}, XdgReady={_window.HasXdgShellPrerequisites}, XdgSurface={_window.IsXdgSurfaceInitialized}, XdgToplevel={_window.IsXdgToplevelInitialized}, WindowInit={_window.InitializationSummary ?? "<pending>"}, Pointer={_inputSource.SupportsPointer}, Keyboard={_inputSource.SupportsKeyboard}, TextInput={_inputSource.SupportsTextInput}, Surface={_bufferPresenter.PixelWidth}x{_bufferPresenter.PixelHeight}@{_bufferPresenter.Dpi:0.##}dpi");
    }
}
