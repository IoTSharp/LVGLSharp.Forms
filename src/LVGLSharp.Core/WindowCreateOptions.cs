namespace LVGLSharp;

public sealed class WindowCreateOptions
{
    public WindowCreateOptions(string title, int width, int height, bool borderless = false)
    {
        Title = title ?? string.Empty;
        Width = width;
        Height = height;
        Borderless = borderless;
    }

    public string Title { get; }

    public int Width { get; }

    public int Height { get; }

    public bool Borderless { get; }
}
