using LVGLSharp.Runtime.Headless;
using SixLabors.ImageSharp.PixelFormats;
using Xunit;
using static LVGLSharp.Interop.LVGL;

namespace LVGLSharp.Headless.Tests;

public unsafe class OffscreenSnapshotTests
{
    [Fact]
    public void RenderSnapshot_UsesConfiguredSizeAndBackground()
    {
        var options = new OffscreenOptions
        {
            Width = 64,
            Height = 32,
            Dpi = 96f,
            BackgroundColor = new Rgba32(0x12, 0x34, 0x56, 0xFF),
        };

        using var view = new OffscreenView(options);
        view.Open();

        var root = view.Root;
        Assert.NotEqual(nint.Zero, (nint)root);

        var image = view.RenderSnapshot();
        using (image)
        {
            Assert.Equal(64, image.Width);
            Assert.Equal(32, image.Height);

            var pixel = image[0, 0];
            Assert.InRange(Math.Abs(pixel.R - options.BackgroundColor.R), 0, 8);
            Assert.InRange(Math.Abs(pixel.G - options.BackgroundColor.G), 0, 4);
            Assert.InRange(Math.Abs(pixel.B - options.BackgroundColor.B), 0, 8);
            Assert.Equal(options.BackgroundColor.A, pixel.A);
        }
    }

    [Fact]
    public void RenderSnapshot_WithLabel_ProducesImage()
    {
        var options = new OffscreenOptions
        {
            Width = 200,
            Height = 80,
            Dpi = 96f,
            BackgroundColor = new Rgba32(255, 255, 255, 255),
        };

        using var view = new OffscreenView(options);
        view.Open();

        var label = lv_label_create(view.Root);
        Assert.NotEqual(nint.Zero, (nint)label);

        var text = "Snapshot Test\0"u8;
        fixed (byte* textPtr = text)
        {
            lv_label_set_text(label, textPtr);
        }

        lv_obj_center(label);

        using var image = view.RenderSnapshot();
        Assert.Equal(options.Width, image.Width);
        Assert.Equal(options.Height, image.Height);
    }
}