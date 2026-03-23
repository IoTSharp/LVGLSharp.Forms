namespace LVGLSharp.Runtime.MacOs;

public sealed record MacOsViewOptions
{
    public string Title { get; init; } = "LVGLSharp MacOs";

    public int Width { get; init; } = 800;

    public int Height { get; init; } = 600;

    public float Dpi { get; init; } = 96f;

    public void Validate()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(Title);

        if (Width <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(Width), Width, "Width 必须大于 0。");
        }

        if (Height <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(Height), Height, "Height 必须大于 0。");
        }

        if (Dpi <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(Dpi), Dpi, "Dpi 必须大于 0。");
        }
    }
}