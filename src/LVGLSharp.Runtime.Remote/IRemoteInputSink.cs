namespace LVGLSharp.Runtime.Remote;

internal interface IRemoteInputSink
{
    void PostInput(RemoteInputEvent inputEvent);
}
