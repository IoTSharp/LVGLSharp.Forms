using LVGLSharp.Runtime.Headless;
using System.Text;
using static LVGLSharp.Interop.LVGL;

namespace OffscreenDemo;

internal static unsafe class Program
{
    private static void Main(string[] args)
    {
        var options = OffscreenDemoOptions.Parse(args);

        using var view = new OffscreenView(options.Width, options.Height, options.Dpi);
        view.Open();

        var root = view.Root;
        if (root == null)
        {
            throw new InvalidOperationException("Offscreen root 创建失败。");
        }

        var label = lv_label_create(root);
        if (label == null)
        {
            throw new InvalidOperationException("Offscreen label 创建失败。");
        }

        var text = Encoding.UTF8.GetBytes("LVGLSharp Offscreen Snapshot\0");
        fixed (byte* textPtr = text)
        {
            lv_label_set_text(label, textPtr);
        }

        lv_obj_center(label);
        view.RenderFrame();

        Directory.CreateDirectory(Path.GetDirectoryName(options.OutputPath)!);
        view.SavePng(options.OutputPath);
        Console.WriteLine($"Offscreen snapshot saved: {options.OutputPath}");
    }

    private sealed record OffscreenDemoOptions(string OutputPath, int Width, int Height, float Dpi)
    {
        public static OffscreenDemoOptions Parse(string[] args)
        {
            var outputPath = args.Length > 0 && !string.IsNullOrWhiteSpace(args[0])
                ? args[0]
                : Path.Combine(AppContext.BaseDirectory, "offscreen-snapshot.png");

            var width = TryParseInt(args, 1, 480);
            var height = TryParseInt(args, 2, 320);
            var dpi = TryParseFloat(args, 3, 96f);

            return new OffscreenDemoOptions(outputPath, width, height, dpi);
        }

        private static int TryParseInt(string[] args, int index, int fallback)
        {
            return args.Length > index && int.TryParse(args[index], out var value) ? value : fallback;
        }

        private static float TryParseFloat(string[] args, int index, float fallback)
        {
            return args.Length > index && float.TryParse(args[index], out var value) ? value : fallback;
        }
    }
}