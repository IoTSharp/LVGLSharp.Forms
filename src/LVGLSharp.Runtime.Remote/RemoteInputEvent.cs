namespace LVGLSharp.Runtime.Remote;

public sealed record RemoteInputEvent(
    RemoteInputEventKind Kind,
    int X,
    int Y,
    uint Buttons,
    uint KeyCode,
    string? Text);