namespace LVGLSharp.Runtime.Remote;

internal interface IRemoteHostedTransport : IRemoteTransport
{
    void Start();

    void Stop();

    void AttachInputSink(IRemoteInputSink inputSink);
}
