using LVGLSharp;
using LVGLSharp.Interop;
using System;

namespace LVGLSharp.Runtime.MacOs;

public unsafe sealed class MacOsView : ViewLifetimeBase
{
    private readonly MacOsViewOptions _options;
    private readonly IMacOsSurface _surface;
    private readonly MacOsFrameBuffer _frameBuffer;
    private bool _running;
    private bool _initialized;

    public MacOsView()
        : this(new MacOsViewOptions())
    {
    }

    public MacOsView(MacOsViewOptions options)
        : this(options, new MacOsSurfaceSkeleton(options))
    {
    }

    internal MacOsView(MacOsViewOptions options, IMacOsSurface surface)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _options.Validate();
        _surface = surface ?? throw new ArgumentNullException(nameof(surface));
        _frameBuffer = new MacOsFrameBuffer(_options.Width, _options.Height);
    }

    public MacOsView(string title = "LVGLSharp MacOs", int width = 800, int height = 600, float dpi = 96f)
        : this(new MacOsViewOptions
        {
            Title = title,
            Width = width,
            Height = height,
            Dpi = dpi,
        })
    {
    }

    public MacOsViewOptions Options => _options;

    public IMacOsSurface Surface => _surface;

    public MacOsFrameBuffer FrameBuffer => _frameBuffer;

    public MacOsHostDiagnostics Diagnostics => new(
        _options.Title,
        _options.Width,
        _options.Height,
        _options.Dpi,
        _surface.IsCreated,
        _initialized,
        _running,
        _frameBuffer.Argb8888Bytes.Length > 0);

    public MacOsHostContext HostContext => new(_options, _surface, _frameBuffer, Diagnostics);

    public override lv_obj_t* Root => null;

    public override lv_group_t* KeyInputGroup => null;

    public override delegate* unmanaged[Cdecl]<lv_event_t*, void> SendTextAreaFocusCallback => null;

    protected override void OnOpenCore()
    {
        LvglNativeLibraryResolver.EnsureRegistered();
        _surface.Create();
        _initialized = true;
        _running = true;

        throw new NotSupportedException(
            $"MacOs runtime skeleton is not implemented yet. Title={_options.Title}, Size={_options.Width}x{_options.Height}, Dpi={_options.Dpi:0.##}." +
            " This host is reserved for a future dedicated macOS runtime.");
    }

    public override void HandleEvents()
    {
        if (_initialized)
        {
            _surface.PumpEvents();
        }
    }

    protected override void RunLoopCore(Action iteration)
    {
        while (_running)
        {
            iteration?.Invoke();
            break;
        }
    }

    protected override void OnCloseCore()
    {
        _running = false;
        _initialized = false;
        _surface.Dispose();
    }

    public override void RegisterTextInput(lv_obj_t* textArea)
    {
    }

    protected override bool CanSkipClose() => !_running && !_initialized;

    public override string ToString() => HostContext.ToString();
}