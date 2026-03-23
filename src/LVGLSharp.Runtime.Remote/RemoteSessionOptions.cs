namespace LVGLSharp.Runtime.Remote;

public record RemoteSessionOptions
{
    public string TransportName { get; init; } = "remote";

    public int TargetFrameRate { get; init; } = 30;

    public bool EnableInputForwarding { get; init; } = true;

    public bool EnableClipboardSync { get; init; } = false;

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(TransportName))
        {
            throw new ArgumentException("TransportName 不能为空。", nameof(TransportName));
        }

        if (TargetFrameRate <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(TargetFrameRate), TargetFrameRate, "TargetFrameRate 必须大于 0。");
        }
    }
}