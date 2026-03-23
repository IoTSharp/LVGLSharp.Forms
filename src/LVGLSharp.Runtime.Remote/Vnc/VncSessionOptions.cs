namespace LVGLSharp.Runtime.Remote.Vnc;

public sealed record VncSessionOptions : RemoteSessionOptions
{
    public int Port { get; init; } = 5900;

    public string Host { get; init; } = "127.0.0.1";

    public string? Password { get; init; }

    public bool ShareExistingSession { get; init; } = true;
        public int Width { get; init; } = 800;
        public int Height { get; init; } = 480;
}