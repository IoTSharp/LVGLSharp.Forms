using LVGLSharp;
using LVGLSharp.Interop;
using System;

namespace LVGLSharp.Runtime.MacOs;

public unsafe sealed class MacOsView : ViewLifetimeBase
{
    private readonly string _title;
    private readonly int _width;
    private readonly int _height;
    private readonly float _dpi;
    private bool _running;
    private bool _initialized;

    public MacOsView(string title = "LVGLSharp MacOs", int width = 800, int height = 600, float dpi = 96f)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);

        _title = title;
        _width = width;
        _height = height;
        _dpi = dpi;
    }

    public override lv_obj_t* Root => null;

    public override lv_group_t* KeyInputGroup => null;

    public override delegate* unmanaged[Cdecl]<lv_event_t*, void> SendTextAreaFocusCallback => null;

    protected override void OnOpenCore()
    {
        LvglNativeLibraryResolver.EnsureRegistered();
        _initialized = true;
        _running = true;

        throw new NotSupportedException(
            $"MacOs runtime skeleton is not implemented yet. Title={_title}, Size={_width}x{_height}, Dpi={_dpi:0.##}." +
            " This host is reserved for a future dedicated macOS runtime.");
    }

    public override void HandleEvents()
    {
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
    }

    public override void RegisterTextInput(lv_obj_t* textArea)
    {
    }

    protected override bool CanSkipClose() => !_running && !_initialized;

    public override string ToString() => $"Host=MacOs, Title={_title}, Size={_width}x{_height}, Dpi={_dpi:0.##}, Initialized={_initialized}, Running={_running}";
}