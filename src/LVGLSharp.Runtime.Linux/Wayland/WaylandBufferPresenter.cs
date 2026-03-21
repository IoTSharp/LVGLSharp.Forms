using System;

namespace LVGLSharp.Runtime.Linux;

internal sealed class WaylandBufferPresenter : IDisposable
{
    public WaylandBufferPresenter(int pixelWidth, int pixelHeight, float dpi)
    {
        PixelWidth = pixelWidth;
        PixelHeight = pixelHeight;
        Dpi = dpi;
    }

    public int PixelWidth { get; }

    public int PixelHeight { get; }

    public float Dpi { get; }

    public bool IsDisposed { get; private set; }

    public void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);
    }

    public void Dispose()
    {
        IsDisposed = true;
    }
}
