using System.Threading;
using System.Threading.Tasks;

namespace LVGLSharp.Runtime.Remote.Vnc;

public sealed class VncTransportSkeleton : RemoteTransportBase
{
    public VncTransportSkeleton(VncSessionOptions options)
        : base("vnc", new RemoteTransportCapabilities(
            SupportsClipboardSync: true,
            SupportsPointerInput: true,
            SupportsKeyboardInput: true,
            SupportsFrameStreaming: true))
    {
        Options = options ?? throw new ArgumentNullException(nameof(options));
        Options.Validate();
    }

    public VncSessionOptions Options { get; }

    public override Task SendFrameAsync(RemoteFrame frame, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(frame);
        throw new NotSupportedException($"VNC transport skeleton is not implemented yet. Host={Options.Host}, Port={Options.Port}.");
    }

    public override Task SendInputAsync(RemoteInputEvent inputEvent, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(inputEvent);
        throw new NotSupportedException("VNC transport skeleton does not forward input yet.");
    }
}