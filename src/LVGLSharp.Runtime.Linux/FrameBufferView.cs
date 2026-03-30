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
public unsafe class FrameBufferView : ViewLifetimeBase
    {
        [DllImport("lvgl", CallingConvention = CallingConvention.Cdecl, EntryPoint = "lv_linux_fbdev_create")]
        private static extern lv_display_t* lv_linux_fbdev_create_native();

        [DllImport("lvgl", CallingConvention = CallingConvention.Cdecl, EntryPoint = "lv_linux_fbdev_set_file")]
        private static extern lv_result_t lv_linux_fbdev_set_file_native(lv_display_t* disp, byte* file);

        [DllImport("lvgl", CallingConvention = CallingConvention.Cdecl, EntryPoint = "lv_evdev_create")]
        private static extern lv_indev_t* lv_evdev_create_native(lv_indev_type_t indev_type, byte* dev_path);

        static lv_display_t* g_display;
        static lv_indev_t* g_indev;
        static bool g_running;
        static byte[] _timeBuf = new byte[32];
        static int startTick;

        public static lv_obj_t* RootObject { get; set; }
        public static lv_group_t* KeyInputGroupObject { get; set; } = null;
        public static delegate* unmanaged[Cdecl]<lv_event_t*, void> SendTextAreaFocusCallbackCore { get; set; } = null;

        public override lv_obj_t* Root => RootObject;
        public override lv_group_t* KeyInputGroup => KeyInputGroupObject;
        public override delegate* unmanaged[Cdecl]<lv_event_t*, void> SendTextAreaFocusCallback => SendTextAreaFocusCallbackCore;

        private lv_font_t* _fallbackFont;
        private lv_style_t* _defaultFontStyle;
        private SixLaborsFontManager? _fontManager;

        [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
        static unsafe uint my_tick()
        {
            return (uint)(Environment.TickCount - startTick);
        }

        private string _fbdev;
        private string _indev;
        private float _dpi;

        public FrameBufferView(string fbdev = "/dev/fb0", string indev = "/dev/input/event0", float dpi = 72f)
        {
            _fbdev = fbdev;
            _indev = indev;
            _dpi = dpi;
        }

        protected override void OnOpenCore()
        {
            if (g_running)
            {
                return;
            }

            LvglNativeLibraryResolver.EnsureRegistered();
            g_running = true;
            startTick = Environment.TickCount;
            lv_init();
            lv_tick_set_cb(&my_tick);

            g_display = lv_linux_fbdev_create_native();
            fixed (byte* ptr = Encoding.ASCII.GetBytes($"{_fbdev}\0"))
                _ = lv_linux_fbdev_set_file_native(g_display, ptr);

            fixed (byte* ptr = Encoding.ASCII.GetBytes($"{_indev}\0"))
                g_indev = lv_evdev_create_native(lv_indev_type_t.LV_INDEV_TYPE_POINTER, ptr);

            RootObject = lv_scr_act();

            LinuxRuntimeFontHelper.InitializeRuntimeFont(RootObject, _dpi).ApplyTo(
                ref _fallbackFont,
                ref _fontManager,
                ref _defaultFontStyle);
        }

        public override void RegisterTextInput(lv_obj_t* textArea)
        {
            if (textArea == null)
            {
                return;
            }

            lv_obj_t* keyboard = lv_keyboard_create(lv_scr_act());
            lv_obj_set_size(keyboard, 670, 200);
            lv_keyboard_set_textarea(keyboard, textArea);
        }

        protected override void RunLoopCore(Action iteration)
        {
            while (g_running)
            {
                HandleEvents();
                iteration?.Invoke();
                Thread.Sleep(5);
            }
        }

        public override void HandleEvents()
        {
            lv_timer_handler();
        }

        protected override void OnCloseCore()
        {
            g_running = false;

            if (g_indev != null)
            {
                lv_indev_delete(g_indev);
                g_indev = null;
            }

            if (g_display != null)
            {
                lv_display_delete(g_display);
                g_display = null;
            }

            LinuxRuntimeFontHelper.ReleaseRuntimeFont(
                ref _fallbackFont,
                ref _fontManager,
                ref _defaultFontStyle);
            RootObject = null;
            KeyInputGroupObject = null;
            SendTextAreaFocusCallbackCore = null;
        }

        protected override bool CanSkipClose() => !g_running && g_display == null && g_indev == null && RootObject == null;
    }
}
