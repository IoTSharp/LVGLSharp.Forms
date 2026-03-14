using LVGLSharp.Runtime.Windows;

namespace LVGLSharp
{
    internal static partial class WindowHostFactory
    {
        internal static partial IWindow Create(string title, int width, int height)
            => new Win32Window(title, (uint)width, (uint)height);
    }
}
