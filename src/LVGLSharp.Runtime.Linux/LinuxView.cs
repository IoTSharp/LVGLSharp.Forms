using LVGLSharp;
using LVGLSharp.Interop;
using System;

namespace LVGLSharp.Runtime.Linux;

public unsafe class LinuxView : ViewLifetimeBase
{
    private readonly IView _inner;
    private readonly LinuxHostEnvironment _environment;
    private static LinuxView? s_activeView;

    public LinuxView(string title = "LVGLSharp Linux", int width = 800, int height = 600, float dpi = 96f,
        string fbdev = "/dev/fb0", string indev = "/dev/input/event0", bool borderless = false)
    {
        var detectedWaylandDisplay = LinuxEnvironmentDetector.DetectWaylandDisplay();
        var detectedX11Display = LinuxEnvironmentDetector.DetectX11Display();
        _environment = LinuxEnvironmentDetector.ResolveHostEnvironment(detectedWaylandDisplay, detectedX11Display, fbdev);

        _inner = _environment switch
        {
            LinuxHostEnvironment.Wslg => new WslgView(title, width, height, dpi, detectedX11Display, borderless),
            LinuxHostEnvironment.Wayland => new WaylandView(title, width, height, dpi, detectedWaylandDisplay, detectedX11Display, borderless),
            LinuxHostEnvironment.Sdl => new SdlView(title, width, height, dpi, borderless),
            LinuxHostEnvironment.FrameBuffer => new FrameBufferView(fbdev, indev, dpi),
            LinuxHostEnvironment.Drm => CreateDrmView(dpi),
            LinuxHostEnvironment.Offscreen => new OffscreenView(width, height, dpi),
            LinuxHostEnvironment.X11 => new X11View(title, width, height, dpi, detectedX11Display, borderless),
            _ => throw new InvalidOperationException($"Unsupported Linux view mode: {_environment}"),
        };

        s_activeView = this;
    }

    public override lv_obj_t* Root => _inner.Root;
    public override lv_group_t* KeyInputGroup => _inner.KeyInputGroup;
    public override delegate* unmanaged[Cdecl]<lv_event_t*, void> SendTextAreaFocusCallback => _inner.SendTextAreaFocusCallback;
    public static (int X, int Y) CurrentMousePosition => s_activeView?._inner switch
    {
        X11View => X11View.CurrentMousePosition,
        WaylandView => WaylandView.CurrentMousePosition,
        SdlView => SdlView.CurrentMousePosition,
        _ => (0, 0),
    };

    public static uint CurrentMouseButton => s_activeView?._inner switch
    {
        X11View => X11View.CurrentMouseButton,
        WaylandView => WaylandView.CurrentMouseButton,
        SdlView => SdlView.CurrentMouseButton,
        _ => 0U,
    };

    protected override void OnOpenCore()
    {
        _inner.Open();
    }

    public override void HandleEvents()
    {
        _inner.HandleEvents();
    }

    protected override void RunLoopCore(Action iteration)
    {
        _inner.RunLoop(iteration);
    }

    protected override void OnCloseCore()
    {
        _inner.Close();
    }

    public override void RegisterTextInput(lv_obj_t* textArea)
    {
        _inner.RegisterTextInput(textArea);
    }

    protected override bool CanSkipClose() => false;

    private static DrmView CreateDrmView(float dpi)
    {
        var options = LinuxEnvironmentDetector.GetDefaultDrmOptions(dpi);
        return new DrmView(options.DevicePath, options.ConnectorId, options.Dpi, options.ModePreference);
    }
}
