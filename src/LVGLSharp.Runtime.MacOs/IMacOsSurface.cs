namespace LVGLSharp.Runtime.MacOs;

public interface IMacOsSurface : IDisposable
{
    string Title { get; }

    int Width { get; }

    int Height { get; }

    float Dpi { get; }

    bool IsCreated { get; }

    void Create();

    void PumpEvents();
}