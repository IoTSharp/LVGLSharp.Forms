using System;

namespace LVGLSharp.Runtime.Remote;

public sealed class RemoteRuntimeSession
{
    public RemoteRuntimeSession(IRemoteFrameSource frameSource, string transportName)
    {
        FrameSource = frameSource ?? throw new ArgumentNullException(nameof(frameSource));
        TransportName = string.IsNullOrWhiteSpace(transportName)
            ? throw new ArgumentException("Value cannot be null or whitespace.", nameof(transportName))
            : transportName;
    }

    public IRemoteFrameSource FrameSource { get; }

    public string TransportName { get; }

    public override string ToString() => $"Transport={TransportName}";
}