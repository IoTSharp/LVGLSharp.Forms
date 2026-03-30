using LVGLSharp.Runtime.Headless;
using SixLabors.ImageSharp.PixelFormats;
using Xunit;
using static LVGLSharp.Interop.LVGL;

namespace LVGLSharp.Headless.Tests;

public unsafe class OffscreenSnapshotChineseTests
{
    [Fact]
    public void RenderSnapshot_WithChineseLabel_ProducesNonEmptyImage()
    {
        var options = new OffscreenOptions
        {
            Width = 240,
            Height = 80,
            Dpi = 96f,
            BackgroundColor = new Rgba32(255, 255, 255, 255),
        };

        using var view = new OffscreenView(options);
        view.Open();

        var label = lv_label_create(view.Root);
        Assert.NotEqual(nint.Zero, (nint)label);

        // 中文 + 英文混排文本，确保多字节路径在 headless 下也能稳定渲染
        var text = "LVGLSharp 快照测试 - Hello世界\0"u8;
        fixed (byte* textPtr = text)
        {
            lv_label_set_text(label, textPtr);
        }

        lv_obj_center(label);

        using var image = view.RenderSnapshot();
        Assert.Equal(options.Width, image.Width);
        Assert.Equal(options.Height, image.Height);

        // 采样中心区域像素，确认不是纯背景色（简单防空白渲染）
        var sampleX = options.Width / 2;
        var sampleY = options.Height / 2;

        var pixel = image[sampleX, sampleY];
        var bg = options.BackgroundColor;

        var diff = Math.Abs(pixel.R - bg.R)
                  + Math.Abs(pixel.G - bg.G)
                  + Math.Abs(pixel.B - bg.B);

        Assert.True(diff > 0, "Snapshot appears to be blank at center sample pixel.");
    }
}
