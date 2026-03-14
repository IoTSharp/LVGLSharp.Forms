using LVGLSharp;
using LVGLSharp.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace LVGLSharp.Runtime.Linux
{
    public unsafe class LinuxView : IWindow
    {
        static lv_display_t* g_display;
        static lv_indev_t* g_indev;
        static uint g_bufSize;
        static bool g_running = true;
        static lv_obj_t* label;
        static byte[] _timeBuf = new byte[32];
        static int startTick;

        public static lv_obj_t* root { get; set; }
        public static lv_group_t* key_inputGroup { get; set; } = null;
        public static delegate* unmanaged[Cdecl]<lv_event_t*, void> SendTextAreaFocusCb { get; set; } = null;

        public lv_obj_t* Root => root;
        public lv_group_t* KeyInputGroup => key_inputGroup;
        public delegate* unmanaged[Cdecl]<lv_event_t*, void> SendTextAreaFocusCallback => SendTextAreaFocusCb;

        private lv_font_t* _fallbackFont;
        private lv_style_t* _defaultFontStyle;
        private SixLaborsFontManager _fontManager;

        [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
        static unsafe uint my_tick()
        {
            return (uint)(Environment.TickCount - startTick);
        }

        private string _fbdev;
        private string _indev;
        private float _dpi;

        public LinuxView(string fbdev = "/dev/fb0", string indev = "/dev/input/event0", float dpi = 72f)
        {
            _fbdev = fbdev;
            _indev = indev;
            _dpi = dpi;
        }

        public void Init()
        {
            startTick = Environment.TickCount;
            lv_init();
            lv_tick_set_cb(&my_tick);

            g_display = lv_linux_fbdev_create();
            fixed (byte* ptr = Encoding.ASCII.GetBytes($"{_fbdev}\0"))
                lv_linux_fbdev_set_file(g_display, ptr);

            fixed (byte* ptr = Encoding.ASCII.GetBytes($"{_indev}\0"))
                g_indev = lv_evdev_create(lv_indev_type_t.LV_INDEV_TYPE_POINTER, ptr);

            root = lv_scr_act();

            _fallbackFont = lv_obj_get_style_text_font(root, LV_PART_MAIN);

            _fontManager = new SixLaborsFontManager(
                "NotoSansSC-Regular.ttf",
                12,
                _dpi,
                _fallbackFont,
                LvglHostDefaults.CreateDefaultFontFallbackGlyphs());

            _defaultFontStyle = LvglHostDefaults.ApplyDefaultFontStyle(root, _fontManager.GetLvFontPtr());
        }

        public void StartLoop(Action handle)
        {
            while (g_running)
            {
                ProcessEvents();
                handle?.Invoke();
                Thread.Sleep(5);
            }
        }

        public void ProcessEvents()
        {
            lv_timer_handler();
        }

        public void Stop()
        {
            g_running = false;
        }
    }
}
