using LVGLSharp;
using LVGLSharp.Interop;
using System;

namespace LVGLSharp.Runtime.Linux;

public unsafe sealed class DrmView : ViewLifetimeBase
{
    private readonly string _devicePath;
    private readonly int _connectorId;
    private readonly float _dpi;
    private bool _running;

    public DrmView(string devicePath = "/dev/dri/card0", int connectorId = -1, float dpi = 96f)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(devicePath);

        _devicePath = devicePath;
        _connectorId = connectorId;
        _dpi = dpi;
    }

    public override lv_obj_t* Root => null;

    public override lv_group_t* KeyInputGroup => null;

    public override delegate* unmanaged[Cdecl]<lv_event_t*, void> SendTextAreaFocusCallback => null;

    protected override void OnOpenCore()
    {
        LvglNativeLibraryResolver.EnsureRegistered();
        _running = true;

        throw new NotSupportedException(
            $"DRM/KMS runtime skeleton is not implemented yet. Device={_devicePath}, Connector={_connectorId}, Dpi={_dpi:0.##}." +
            " This host is reserved for a future Linux DRM/KMS backend.");
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
    }

    public override void RegisterTextInput(lv_obj_t* textArea)
    {
    }

    protected override bool CanSkipClose() => !_running;
}