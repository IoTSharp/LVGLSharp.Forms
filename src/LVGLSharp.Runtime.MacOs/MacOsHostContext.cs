namespace LVGLSharp.Runtime.MacOs;

public sealed record MacOsHostContext(
    MacOsViewOptions Options,
    IMacOsSurface Surface,
    MacOsFrameBuffer FrameBuffer,
    MacOsHostDiagnostics Diagnostics)
{
    public override string ToString()
        => $"{Diagnostics}; FrameBuffer={FrameBuffer.Width}x{FrameBuffer.Height}@{FrameBuffer.Stride}";
}