namespace LVGLSharp.Runtime.Remote;

public interface IRemoteFrameSource
{
    RemoteFrame CaptureFrame();

    bool TryHandleInput(RemoteInputEvent inputEvent);
}