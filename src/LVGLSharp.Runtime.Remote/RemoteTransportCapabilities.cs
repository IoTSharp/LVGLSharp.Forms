namespace LVGLSharp.Runtime.Remote;

public sealed record RemoteTransportCapabilities(
    bool SupportsClipboardSync,
    bool SupportsPointerInput,
    bool SupportsKeyboardInput,
    bool SupportsFrameStreaming);