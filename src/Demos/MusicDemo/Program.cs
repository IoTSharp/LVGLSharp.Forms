namespace MusicDemo;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        DemoRuntimeConfiguration.Configure();
        Application.Run(new frmMusicDemo());
    }
}
