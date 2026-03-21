using System;

namespace LVGLSharp.Runtime.Linux;

internal sealed class WaylandInputSource : IDisposable
{
    public bool SupportsPointer => true;

    public bool SupportsKeyboard => true;

    public bool SupportsTextInput => false;

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
