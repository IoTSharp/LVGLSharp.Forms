using SixLabors.ImageSharp.PixelFormats;

namespace LVGLSharp.Runtime.Headless;

public sealed record OffscreenOptions
{
    public static OffscreenOptions Default { get; } = new();

    public int Width { get; init; } = 800;

    public int Height { get; init; } = 600;

    public float Dpi { get; init; } = 96f;

    public Rgba32 BackgroundColor { get; init; } = new(255, 255, 255, 255);

    public string? OutputPath { get; init; }

    public void Validate()
    {
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