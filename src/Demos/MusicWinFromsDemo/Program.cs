using System;

namespace MusicWinFromsDemo
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            ApplicationConfiguration.Initialize();

#if LVGLSHARP_FORMS
            DemoRuntimeConfiguration.Configure();
#endif

            Application.Run(new frmMusicDemo());
        }
    }
}
