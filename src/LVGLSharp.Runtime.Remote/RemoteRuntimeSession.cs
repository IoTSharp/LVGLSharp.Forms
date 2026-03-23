using System;
using System.Threading;
using System.Threading.Tasks;

namespace LVGLSharp.Runtime.Remote;

public sealed class RemoteRuntimeSession
{
    public RemoteRuntimeSession(IRemoteFrameSource frameSource, IRemoteTransport transport, RemoteSessionOptions? options = null)
    {
        FrameSource = frameSource ?? throw new ArgumentNullException(nameof(frameSource));
        Transport = transport ?? throw new ArgumentNullException(nameof(transport));
        Options = options ?? new RemoteSessionOptions { TransportName = transport.Name };
        Options.Validate();
    }

    public IRemoteFrameSource FrameSource { get; }

    public IRemoteTransport Transport { get; }

    public RemoteSessionOptions Options { get; }

    public Task SendFrameAsync(CancellationToken cancellationToken = default)
    {
        var frame = FrameSource.CaptureFrame();
        return Transport.SendFrameAsync(frame, cancellationToken);
    }

    public async Task<bool> ForwardInputAsync(RemoteInputEvent inputEvent, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(inputEvent);

        if (!Options.EnableInputForwarding)
        {
            return false;
        }

        await Transport.SendInputAsync(inputEvent, cancellationToken);
        return FrameSource.TryHandleInput(inputEvent);
    }

    public override string ToString() => $"Transport={Options.TransportName}, Fps={Options.TargetFrameRate}, Input={Options.EnableInputForwarding}";
}