namespace LVGLSharp.Runtime.MacOs;

public sealed class MacOsSurfaceSkeleton : IMacOsSurface
{
    private bool _disposed;

    public MacOsSurfaceSkeleton(MacOsViewOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        options.Validate();

        Title = options.Title;
        Width = options.Width;
        Height = options.Height;
        Dpi = options.Dpi;
    }

    public string Title { get; }

    public int Width { get; }

    public int Height { get; }

    public float Dpi { get; }

    public bool IsCreated { get; private set; }

    public void Create()
    {
        ThrowIfDisposed();
        IsCreated = true;
    }

    public void PumpEvents()
    {
        ThrowIfDisposed();
        if (!IsCreated)
        {
            throw new InvalidOperationException("MacOs surface ÉÐÎ´´´½¨¡£");
        }
    }

    public void Dispose()
    {
        _disposed = true;
        IsCreated = false;
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(MacOsSurfaceSkeleton));
        }
    }
}