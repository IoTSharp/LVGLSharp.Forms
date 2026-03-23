namespace LVGLSharp.Runtime.MacOs;

public sealed record MacOsHostDiagnostics(
    string Title,
    int Width,
    int Height,
    float Dpi,
    bool SurfaceCreated,
    bool Initialized,
    bool Running,
    bool FrameBufferAllocated)
{
    public override string ToString()
        => $"Host=MacOs, Title={Title}, Size={Width}x{Height}, Dpi={Dpi:0.##}, SurfaceCreated={SurfaceCreated}, Initialized={Initialized}, Running={Running}, FrameBufferAllocated={FrameBufferAllocated}";
}