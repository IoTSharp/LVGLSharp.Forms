using System.Threading;
using System.Threading.Tasks;

namespace LVGLSharp.Runtime.Remote;

public interface IRemoteTransport
{
    string Name { get; }

    Task SendFrameAsync(RemoteFrame frame, CancellationToken cancellationToken = default);

    Task SendInputAsync(RemoteInputEvent inputEvent, CancellationToken cancellationToken = default);
}