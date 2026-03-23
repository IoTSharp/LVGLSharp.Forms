using LVGLSharp.Runtime.Headless;
using SixLabors.ImageSharp.PixelFormats;
using System.Text;
using static LVGLSharp.Interop.LVGL;

namespace OffscreenDemo;

internal static unsafe class Program
{
    private static void Main(string[] args)
    {
        var options = OffscreenDemoOptions.Parse(args);

        using var view = new OffscreenView(options.ToOffscreenOptions());
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

        var text = Encoding.UTF8.GetBytes($"{options.Text}\0");
        fixed (byte* textPtr = text)
        {
            lv_label_set_text(label, textPtr);
        }

        lv_obj_center(label);

        Directory.CreateDirectory(Path.GetDirectoryName(options.OutputPath)!);
        view.SaveSnapshot();
        Console.WriteLine($"Offscreen snapshot saved: {options.OutputPath}");
    }

    private sealed record OffscreenDemoOptions(string OutputPath, int Width, int Height, float Dpi, string Text, Rgba32 BackgroundColor)
    {
        public static OffscreenDemoOptions Parse(string[] args)
        {
            var outputPath = args.Length > 0 && !string.IsNullOrWhiteSpace(args[0])
                ? args[0]
                : Path.Combine(AppContext.BaseDirectory, "offscreen-snapshot.png");

            var width = TryParseInt(args, 1, 480);
            var height = TryParseInt(args, 2, 320);
            var dpi = TryParseFloat(args, 3, 96f);
            var text = args.Length > 4 && !string.IsNullOrWhiteSpace(args[4])
                ? args[4]
                : "LVGLSharp Offscreen Snapshot";
            var backgroundColor = args.Length > 5 && TryParseColor(args[5], out var parsedColor)
                ? parsedColor
                : new Rgba32(255, 255, 255, 255);

            return new OffscreenDemoOptions(outputPath, width, height, dpi, text, backgroundColor);
        }

        public OffscreenOptions ToOffscreenOptions()
        {
            return new OffscreenOptions
            {
                Width = Width,
                Height = Height,
                Dpi = Dpi,
                OutputPath = OutputPath,
                BackgroundColor = BackgroundColor,
            };
        }

        private static int TryParseInt(string[] args, int index, int fallback)
        {
            return args.Length > index && int.TryParse(args[index], out var value) ? value : fallback;
        }

        private static float TryParseFloat(string[] args, int index, float fallback)
        {
            return args.Length > index && float.TryParse(args[index], out var value) ? value : fallback;
        }

        private static bool TryParseColor(string value, out Rgba32 color)
        {
            color = default;
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            var normalized = value.Trim().TrimStart('#');
            if (normalized.Length != 6 && normalized.Length != 8)
            {
                return false;
            }

            if (!uint.TryParse(normalized, System.Globalization.NumberStyles.HexNumber, null, out var hex))
            {
                return false;
            }

            color = normalized.Length == 6
                ? new Rgba32((byte)((hex >> 16) & 0xFF), (byte)((hex >> 8) & 0xFF), (byte)(hex & 0xFF), 0xFF)
                : new Rgba32((byte)((hex >> 24) & 0xFF), (byte)((hex >> 16) & 0xFF), (byte)((hex >> 8) & 0xFF), (byte)(hex & 0xFF));
            return true;
        }
    }
}