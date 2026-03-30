using System.Text;
using LVGLSharp.Interop;
using LVGLSharp.Runtime.Headless;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Xunit;
using static LVGLSharp.Interop.LVGL;
using static LVGLSharp.Interop.lv_align_t;
using static LVGLSharp.Interop.lv_flex_align_t;
using static LVGLSharp.Interop.lv_flex_flow_t;

namespace LVGLSharp.Headless.Tests;

public unsafe class OffscreenSnapshotChineseTests
{
    [Fact]
    public void RenderSnapshot_WithChineseLabel_ProducesNonEmptyImage()
    {
        var options = CreateOptions(240, 80);
        using var view = CreateOpenView(options);

        var label = lv_label_create(view.Root);
        Assert.NotEqual(nint.Zero, (nint)label);

        SetLabelText(label, "LVGLSharp 快照测试 - Hello世界");
        lv_obj_center(label);
        lv_obj_update_layout(view.Root);

        using var image = view.RenderSnapshot();
        Assert.Equal(options.Width, image.Width);
        Assert.Equal(options.Height, image.Height);

        AssertSnapshotHasInk(image, options.BackgroundColor, minimumInkPixels: 40);
    }

    [Fact]
    public void RenderSnapshot_WithChineseLvglSharpLayout_MatchesSnapshot()
    {
        var options = CreateOptions(360, 220);
        using var view = CreateOpenView(options);

        var surface = CreateContainer(view.Root, 12, 12, options.Width - 24, options.Height - 24, padAll: 0);

        var row1 = CreateContainer(surface, 0, 0, options.Width - 24, 48, padAll: 0);
        var row1Flow = CreateFlowContainer(row1, 8, 6, options.Width - 40, 36);
        var titleLabel = lv_label_create(row1Flow);
        Assert.NotEqual(nint.Zero, (nint)titleLabel);
        SetLabelText(titleLabel, "中文 LVGLSharp 布局快照");

        var statusLabel = lv_label_create(row1Flow);
        Assert.NotEqual(nint.Zero, (nint)statusLabel);
        SetLabelText(statusLabel, "状态：已启用");

        var row2 = CreateContainer(surface, 0, 56, options.Width - 24, 52, padAll: 0);
        var row2Flow = CreateFlowContainer(row2, 8, 6, options.Width - 40, 40);
        var languageLabel = lv_label_create(row2Flow);
        Assert.NotEqual(nint.Zero, (nint)languageLabel);
        SetLabelText(languageLabel, "语言(Language)：");

        var valueLabel = lv_label_create(row2Flow);
        Assert.NotEqual(nint.Zero, (nint)valueLabel);
        SetLabelText(valueLabel, "中文 / English");

        var themeLabel = lv_label_create(row2Flow);
        Assert.NotEqual(nint.Zero, (nint)themeLabel);
        SetLabelText(themeLabel, "主题：浅色");

        var row3 = CreateContainer(surface, 0, 116, options.Width - 24, 92, padAll: 0);
        var row3Flow = CreateFlowContainer(row3, 8, 6, options.Width - 40, 80);
        var infoLabel = lv_label_create(row3Flow);
        Assert.NotEqual(nint.Zero, (nint)infoLabel);
        lv_label_set_long_mode(infoLabel, LV_LABEL_LONG_WRAP);
        lv_obj_set_width(infoLabel, options.Width - 88);
        SetLabelText(infoLabel, "说明：此快照用于验证中文标题、字段行与多行提示文本在 LVGLSharp 布局中的排版是否稳定。\n请检查对齐、换行与字形输出。");

        lv_obj_update_layout(view.Root);

        using var image = view.RenderSnapshot();
        Assert.Equal(options.Width, image.Width);
        Assert.Equal(options.Height, image.Height);

        AssertSnapshotMatches(
            image,
            options.BackgroundColor,
            expectedSignature: "157|7,6,74,47|0111111000/0111111000/0222121210/0111111100/1444432100/0111100000",
            minimumInkPixels: 400);
    }

    [Fact]
    public void RenderSnapshot_WithChineseMixedTextLines_MatchesSnapshot()
    {
        var options = CreateOptions(320, 180);
        using var view = CreateOpenView(options);

        var card = CreateContainer(view.Root, 16, 16, options.Width - 32, options.Height - 32, padAll: 0);
        var flow = CreateFlowContainer(card, 10, 10, options.Width - 52, options.Height - 52);

        var headerLabel = lv_label_create(flow);
        Assert.NotEqual(nint.Zero, (nint)headerLabel);
        SetLabelText(headerLabel, "中文 / English 混排快照");

        var bodyLabel = lv_label_create(flow);
        Assert.NotEqual(nint.Zero, (nint)bodyLabel);
        lv_label_set_long_mode(bodyLabel, LV_LABEL_LONG_WRAP);
        lv_obj_set_width(bodyLabel, options.Width - 84);
        SetLabelText(bodyLabel, "账号(Account)：lvglsharp@test.com\n状态(Status)：已启用(Enabled)\n说明：这是一段用于快照测试的中英文混排文本，请确认数字、符号、英文单词与中文段落在 headless 渲染下保持稳定换行。\n版本(Version)：v9.5.0-preview");

        lv_obj_update_layout(view.Root);

        using var image = view.RenderSnapshot();
        Assert.Equal(options.Width, image.Width);
        Assert.Equal(options.Height, image.Height);

        AssertSnapshotMatches(
            image,
            options.BackgroundColor,
            expectedSignature: "177|8,8,65,38|0000000000/0333300000/0423323200/0454555300/0564442000/0001110000",
            minimumInkPixels: 350);
    }

    private static OffscreenOptions CreateOptions(int width, int height)
        => new()
        {
            Width = width,
            Height = height,
            Dpi = 96f,
            BackgroundColor = new Rgba32(255, 255, 255, 255),
        };

    private static OffscreenView CreateOpenView(OffscreenOptions options)
    {
        var view = new OffscreenView(options);
        view.Open();
        return view;
    }

    private static lv_obj_t* CreateContainer(lv_obj_t* parent, int x, int y, int width, int height, int padAll)
    {
        var container = lv_obj_create(parent);
        Assert.NotEqual(nint.Zero, (nint)container);

        lv_obj_remove_style_all(container);
        lv_obj_set_pos(container, x, y);
        lv_obj_set_size(container, width, height);
        lv_obj_set_style_pad_all(container, padAll, 0);

        return container;
    }

    private static lv_obj_t* CreateFlowContainer(lv_obj_t* parent, int x, int y, int width, int height)
    {
        var container = CreateContainer(parent, x, y, width, height, padAll: 8);
        lv_obj_set_flex_flow(container, LV_FLEX_FLOW_ROW_WRAP);
        lv_obj_set_flex_align(container, LV_FLEX_ALIGN_START, LV_FLEX_ALIGN_CENTER, LV_FLEX_ALIGN_START);
        lv_obj_set_style_pad_row(container, 6, 0);
        lv_obj_set_style_pad_column(container, 10, 0);
        return container;
    }

    private static void SetLabelText(lv_obj_t* label, string text)
    {
        var utf8 = Encoding.UTF8.GetBytes(text + "\0");
        fixed (byte* textPtr = utf8)
        {
            lv_label_set_text(label, textPtr);
        }
    }

    private static void AssertSnapshotHasInk(Image<Rgba32> image, Rgba32 background, int minimumInkPixels)
    {
        var metrics = CollectInkMetrics(image, background);
        Assert.True(
            metrics.InkPixels >= minimumInkPixels,
            $"Snapshot content too small. InkPixels={metrics.InkPixels}, Signature={CreateSnapshotSignature(image, background, metrics)}");
    }

    private static void AssertSnapshotMatches(Image<Rgba32> image, Rgba32 background, string expectedSignature, int minimumInkPixels)
    {
        var metrics = CollectInkMetrics(image, background);
        Assert.True(
            metrics.InkPixels >= minimumInkPixels,
            $"Snapshot content too small. InkPixels={metrics.InkPixels}, Signature={CreateSnapshotSignature(image, background, metrics)}");

        var actualSignature = CreateSnapshotSignature(image, background, metrics);
        Assert.True(
            string.Equals(expectedSignature, actualSignature, StringComparison.Ordinal),
            $"Snapshot signature mismatch. Expected={expectedSignature}; Actual={actualSignature}");
    }

    private static string CreateSnapshotSignature(Image<Rgba32> image, Rgba32 background, InkMetrics metrics)
    {
        const string hexDigits = "0123456789ABCDEF";
        const int columns = 10;
        const int rows = 6;

        var builder = new StringBuilder(96);
        builder.Append(metrics.InkPixels / 32);
        builder.Append('|');
        builder.Append(metrics.Left / 4).Append(',').Append(metrics.Top / 4).Append(',').Append(metrics.Right / 4).Append(',').Append(metrics.Bottom / 4);
        builder.Append('|');

        for (var row = 0; row < rows; row++)
        {
            if (row > 0)
            {
                builder.Append('/');
            }

            var yStart = row * image.Height / rows;
            var yEnd = (row + 1) * image.Height / rows;

            for (var column = 0; column < columns; column++)
            {
                var xStart = column * image.Width / columns;
                var xEnd = (column + 1) * image.Width / columns;
                var inkPixels = 0;

                for (var y = yStart; y < yEnd; y++)
                {
                    for (var x = xStart; x < xEnd; x++)
                    {
                        if (IsInkPixel(image[x, y], background))
                        {
                            inkPixels++;
                        }
                    }
                }

                var cellPixels = Math.Max(1, (xEnd - xStart) * (yEnd - yStart));
                var density = Math.Clamp((inkPixels * 15 + cellPixels / 2) / cellPixels, 0, 15);
                builder.Append(hexDigits[density]);
            }
        }

        return builder.ToString();
    }

    private static InkMetrics CollectInkMetrics(Image<Rgba32> image, Rgba32 background)
    {
        var inkPixels = 0;
        var left = image.Width;
        var top = image.Height;
        var right = -1;
        var bottom = -1;

        for (var y = 0; y < image.Height; y++)
        {
            for (var x = 0; x < image.Width; x++)
            {
                if (!IsInkPixel(image[x, y], background))
                {
                    continue;
                }

                inkPixels++;
                left = Math.Min(left, x);
                top = Math.Min(top, y);
                right = Math.Max(right, x);
                bottom = Math.Max(bottom, y);
            }
        }

        if (inkPixels == 0)
        {
            return new InkMetrics(0, -1, -1, -1, -1);
        }

        return new InkMetrics(inkPixels, left, top, right, bottom);
    }

    private static bool IsInkPixel(Rgba32 pixel, Rgba32 background)
    {
        var diff = Math.Abs(pixel.R - background.R)
                 + Math.Abs(pixel.G - background.G)
                 + Math.Abs(pixel.B - background.B)
                 + Math.Abs(pixel.A - background.A);

        return diff > 24;
    }

    private readonly record struct InkMetrics(int InkPixels, int Left, int Top, int Right, int Bottom);
}
