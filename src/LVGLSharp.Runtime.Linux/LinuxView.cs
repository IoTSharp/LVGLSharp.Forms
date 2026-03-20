using LVGLSharp;
using LVGLSharp.Interop;
using System;
using System.IO;
using System.Linq;

namespace LVGLSharp.Runtime.Linux;

public unsafe class LinuxView : IWindow
{
    private enum LinuxViewMode
    {
        X11,
        FrameBuffer,
    }

    private readonly IWindow _inner;
    private readonly LinuxViewMode _mode;

    public LinuxView(string title = "LVGLSharp Linux", int width = 800, int height = 600, float dpi = 96f,
        string fbdev = "/dev/fb0", string indev = "/dev/input/event0", bool borderless = false)
    {
        var detectedDisplay = DetectX11Display();
        _mode = detectedDisplay switch
        {
            not null => LinuxViewMode.X11,
            _ when File.Exists(fbdev) => LinuxViewMode.FrameBuffer,
            _ => LinuxViewMode.X11,
        };

        _inner = _mode switch
        {
            LinuxViewMode.FrameBuffer => new FrameBufferView(fbdev, indev, dpi),
            LinuxViewMode.X11 => new X11View(title, width, height, dpi, detectedDisplay, borderless),
            _ => throw new InvalidOperationException($"Unsupported Linux view mode: {_mode}"),
        };
    }

    public lv_obj_t* Root => _inner.Root;
    public lv_group_t* KeyInputGroup => _inner.KeyInputGroup;
    public delegate* unmanaged[Cdecl]<lv_event_t*, void> SendTextAreaFocusCallback => _inner.SendTextAreaFocusCallback;

    public void Init()
    {
        _inner.Init();
    }

    public void ProcessEvents()
    {
        _inner.ProcessEvents();
    }

    public void StartLoop(Action handle)
    {
        _inner.StartLoop(handle);
    }

    public void Stop()
    {
        _inner.Stop();
    }

    public void AttachTextInput(lv_obj_t* textArea)
    {
        _inner.AttachTextInput(textArea);
    }

    private static string? DetectX11Display()
    {
        const string x11SocketDir = "/tmp/.X11-unix";
        if (Directory.Exists(x11SocketDir))
        {
            var displayEntry = Directory.EnumerateFiles(x11SocketDir, "X*")
                .Select(Path.GetFileName)
                .Select(static name => name is { Length: > 1 } value ? value[1..] : string.Empty)
                .Where(static value => value.Length > 0)
                .OrderBy(static value => value, StringComparer.Ordinal)
                .FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(displayEntry))
            {
                return $":{displayEntry}";
            }
        }

        return null;
    }
}
