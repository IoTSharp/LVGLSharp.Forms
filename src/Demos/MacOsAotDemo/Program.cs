using LVGLSharp.Runtime.MacOs;

namespace MacOsAotDemo;

internal static class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        var options = MacOsAotDemoOptions.Parse(args);

        ApplicationConfiguration.Initialize();
        Application.Run(new frmMacOsAotDemo(options.ToViewOptions()));
    }

    private sealed record MacOsAotDemoOptions(string Title, int Width, int Height, float Dpi)
    {
        public static MacOsAotDemoOptions Parse(string[] args)
        {
            var title = args.Length > 0 && !string.IsNullOrWhiteSpace(args[0])
                ? args[0]
                : "LVGLSharp macOS AOT Demo";

            var width = TryParseInt(args, 1, 960);
            var height = TryParseInt(args, 2, 540);
            var dpi = TryParseFloat(args, 3, 96f);

            return new MacOsAotDemoOptions(title, width, height, dpi);
        }

        public MacOsViewOptions ToViewOptions()
        {
            return new MacOsViewOptions
            {
                Title = Title,
                Width = Width,
                Height = Height,
                Dpi = Dpi,
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
    }
}