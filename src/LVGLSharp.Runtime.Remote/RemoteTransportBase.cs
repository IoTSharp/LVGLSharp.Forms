using System.Threading;
using System.Threading.Tasks;

namespace LVGLSharp.Runtime.Remote;

public abstract class RemoteTransportBase : IRemoteTransport
{
    protected RemoteTransportBase(string name, RemoteTransportCapabilities capabilities)
    {
        Name = string.IsNullOrWhiteSpace(name)
            ? throw new ArgumentException("名称不能为空。", nameof(name))
            : name;
        Capabilities = capabilities ?? throw new ArgumentNullException(nameof(capabilities));
    }

    public string Name { get; }

    public RemoteTransportCapabilities Capabilities { get; }

    public abstract Task SendFrameAsync(RemoteFrame frame, CancellationToken cancellationToken = default);

    public abstract Task SendInputAsync(RemoteInputEvent inputEvent, CancellationToken cancellationToken = default);

    public override string ToString() => $"{Name} [Clipboard={Capabilities.SupportsClipboardSync}, Pointer={Capabilities.SupportsPointerInput}, Keyboard={Capabilities.SupportsKeyboardInput}, Streaming={Capabilities.SupportsFrameStreaming}]";
}