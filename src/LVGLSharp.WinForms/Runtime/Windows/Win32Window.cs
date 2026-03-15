using LVGLSharp;
using LVGLSharp.Interop;
using SixLabors.Fonts;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static LVGLSharp.Runtime.Windows.Win32Api;
using SixLaborsSystemFonts = SixLabors.Fonts.SystemFonts;

namespace LVGLSharp.Runtime.Windows
{
    public unsafe class Win32Window : IWindow
    {
        static MSG msg;
        static int Width;
        static int Height;
        static IntPtr g_hwnd;
        static lv_display_t* g_display;
        static IntPtr g_lvbuf;

        static uint g_bufSize = 1024 * 1024 * 4;
        static bool g_running = true;
        static int startTick;
        static int mouseX = 0, mouseY = 0;
        static bool mousePressed = false;
        static byte[] bgraBuf;
        static byte[] _timeBuf = new byte[32];
        static readonly object renderLock = new object();
        static uint last_key_processed;
        static lv_indev_state_t last_key_state_processed = LV_INDEV_STATE_REL;
        static string ime_content = "";
        static bool ignore_next_wmchar = false;
        static volatile int mouseWheelDelta = 0;
        static ConcurrentQueue<uint> key_queue = new ConcurrentQueue<uint>();
        static ConcurrentQueue<string> ime_commit_queue = new ConcurrentQueue<string>();

        public static lv_obj_t* root { get; set; }
        public static lv_group_t* key_inputGroup { get; set; }
        public static delegate* unmanaged[Cdecl]<lv_event_t*, void> SendTextAreaFocusCb { get; set; } = &HandleSendTextAreaFocusCb;

        public lv_obj_t* Root => root;
        public lv_group_t* KeyInputGroup => key_inputGroup;
        public delegate* unmanaged[Cdecl]<lv_event_t*, void> SendTextAreaFocusCallback => SendTextAreaFocusCb;

        static BITMAPINFO _bmi = new BITMAPINFO
        {
            bmiHeader = new BITMAPINFOHEADER
            {
                biSize = (uint)Marshal.SizeOf<BITMAPINFOHEADER>(),
                biPlanes = 1,
                biBitCount = 32,
                biCompression = 0 // BI_RGB
            },
            bmiColors = new uint[256]
        };

        static void ConvertRGB565ToBGRA(byte* src, byte* dst, int pixelCount)
        {
            for (int i = 0; i < pixelCount; i++)
            {
                ushort rgb565 = ((ushort*)src)[i];
                byte r = (byte)(((rgb565 >> 11) & 0x1F) << 3);
                byte g = (byte)(((rgb565 >> 5) & 0x3F) << 2);
                byte b = (byte)((rgb565 & 0x1F) << 3);
                dst[i * 4 + 0] = b; // BGRA
                dst[i * 4 + 1] = g;
                dst[i * 4 + 2] = r;
                dst[i * 4 + 3] = 0xFF;
            }
        }

        [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
        static unsafe void FlushCb(lv_display_t* disp_drv, lv_area_t* area, byte* color_p)
        {
            lock (renderLock)
            {
                int width = area->x2 - area->x1 + 1;
                int height = area->y2 - area->y1 + 1;
                int pixelCount = width * height;

                fixed (byte* pBGRA = bgraBuf)
                {
                    ConvertRGB565ToBGRA((byte*)color_p, pBGRA, pixelCount);

                    _bmi.bmiHeader.biWidth = width;
                    _bmi.bmiHeader.biHeight = -height;
                    _bmi.bmiHeader.biSizeImage = (uint)(width * height * 4);

                    IntPtr hdc = GetDC(g_hwnd);
                    SetDIBitsToDevice(
                        hdc, area->x1, area->y1, (uint)width, (uint)height,
                        0, 0, 0, (uint)height,
                        (IntPtr)pBGRA, ref _bmi, 0);
                    ReleaseDC(g_hwnd, hdc);
                }

                lv_display_flush_ready(disp_drv);
            }
        }

        [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
        static unsafe uint my_tick()
        {
            return (uint)(Environment.TickCount - startTick);
        }

        [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
        static unsafe void MouseReadCb(lv_indev_t* indev_drv, lv_indev_data_t* data)
        {
            data->point.x = mouseX;
            data->point.y = mouseY;
            data->state = mousePressed ? LV_INDEV_STATE_PR : LV_INDEV_STATE_REL;
            data->enc_diff = (short)mouseWheelDelta;
            mouseWheelDelta = 0;
        }

        [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
        static void KeyboardReadCb(lv_indev_t* indev, lv_indev_data_t* data)
        {
            if (ignore_next_wmchar)
            {
                data->key = 0;
                data->state = LV_INDEV_STATE_REL;
                return;
            }

            if (last_key_state_processed == LV_INDEV_STATE_PR)
            {
                data->key = last_key_processed;
                data->state = LV_INDEV_STATE_REL;
                last_key_state_processed = LV_INDEV_STATE_REL;
                return;
            }

            uint new_key;
            if (key_queue.TryDequeue(out new_key))
            {
                data->key = new_key;
                data->state = LV_INDEV_STATE_PR;

                last_key_processed = new_key;
                last_key_state_processed = LV_INDEV_STATE_PR;
            }
            else
            {
                data->key = 0;
                data->state = LV_INDEV_STATE_REL;
            }
        }

        [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
        static unsafe void HandleSendTextAreaFocusCb(lv_event_t* e)
        {
            lv_obj_t* target = (lv_obj_t*)lv_event_get_target(e);

            // 获取 TextArea 坐标
            lv_area_t area;
            lv_obj_get_coords(target, &area);

            // TextArea 屏幕坐标
            int ime_x = area.x1;
            int ime_y = area.y2;

            // 设置 IME 候选框位置
            IntPtr hIMC = ImmGetContext(g_hwnd);
            if (hIMC != IntPtr.Zero)
            {
                COMPOSITIONFORM compForm = new COMPOSITIONFORM();
                compForm.dwStyle = CFS_POINT;
                compForm.ptCurrentPos.x = ime_x;
                compForm.ptCurrentPos.y = ime_y;

                ImmSetCompositionWindow(hIMC, ref compForm);
                ImmReleaseContext(g_hwnd, hIMC);
            }
        }

        static unsafe void DrainImeCommitQueue()
        {
            while (ime_commit_queue.TryDequeue(out var text))
            {
                if (string.IsNullOrEmpty(text) || key_inputGroup == null)
                {
                    continue;
                }

                var inputObj = lv_group_get_focused(key_inputGroup);
                if (inputObj == null)
                {
                    continue;
                }

                var utf8 = Encoding.UTF8.GetBytes(text + "\0");
                fixed (byte* utf8Ptr = utf8)
                {
                    lv_textarea_add_text(inputObj, utf8Ptr);
                }
            }
        }

        static IntPtr MyWndProc(IntPtr hWnd, uint msg, UIntPtr wParam, IntPtr lParam)
        {
            switch (msg)
            {
                case WM_DESTROY:
                    g_running = false;
                    PostQuitMessage(0);
                    break;
                case 0x0005: // WM_SIZE
                    lock (renderLock)
                    {
                        int newWidth = lParam.ToInt32() & 0xFFFF;
                        int newHeight = (lParam.ToInt32() >> 16) & 0xFFFF;
                        Width = newWidth;
                        Height = newHeight;
                        // 更新分辨率
                        lv_display_set_resolution(g_display, Width, Height);
                    }
                    break;
                case 0x0201: // WM_LBUTTONDOWN
                    mousePressed = true;
                    mouseX = (short)(lParam.ToInt32() & 0xFFFF);
                    mouseY = (short)((lParam.ToInt32() >> 16) & 0xFFFF);
                    break;
                case 0x0202: // WM_LBUTTONUP
                    mousePressed = false;
                    mouseX = (short)(lParam.ToInt32() & 0xFFFF);
                    mouseY = (short)((lParam.ToInt32() >> 16) & 0xFFFF);
                    break;
                case 0x0200: // WM_MOUSEMOVE
                    mouseX = (short)(lParam.ToInt32() & 0xFFFF);
                    mouseY = (short)((lParam.ToInt32() >> 16) & 0xFFFF);
                    break;
                case WM_IME_STARTCOMPOSITION:
                    ignore_next_wmchar = true;
                    break;
                case 0x0102: // WM_CHAR
                    if (ignore_next_wmchar)
                        break;

                    key_queue.Enqueue((uint)wParam);
                    break;
                case 0x0101: // WM_KEYUP
                    if (ignore_next_wmchar)
                    {
                        ignore_next_wmchar = false;
                        break;
                    }

                    break;
                case WM_IME_COMPOSITION:
                    {
                        if (((int)lParam & GCS_RESULTSTR) != 0)
                        {
                            IntPtr hIMC = ImmGetContext(hWnd);
                            if (hIMC != IntPtr.Zero)
                            {
                                int size = ImmGetCompositionStringW(hIMC, GCS_RESULTSTR, null, 0);
                                if (size > 0)
                                {
                                    byte[] buffer = new byte[size];
                                    ImmGetCompositionStringW(hIMC, GCS_RESULTSTR, buffer, size);
                                    string result = Encoding.Unicode.GetString(buffer);
                                    ime_content = result;
                                    if (!string.IsNullOrEmpty(result))
                                    {
                                        ime_commit_queue.Enqueue(result);
                                    }
                                    ignore_next_wmchar = true;
                                }
                                ImmReleaseContext(hWnd, hIMC);
                            }
                        }
                        break;
                    }
                case WM_MOUSEWHEEL:
                    {
                        int delta = (short)((wParam.ToUInt64() >> 16) & 0xFFFF);
                        mouseWheelDelta += delta / WHEEL_DELTA;
                        break;
                    }
            }
            return DefWindowProc(hWnd, msg, wParam, lParam);
        }

        private string _title;
        private lv_font_t* _fallbackFont;
        private lv_style_t* _defaultFontStyle;
        private SixLaborsFontManager _fontManager;
        private lv_indev_t* kbd_indev;
        private lv_indev_t* indev;
        private WNDCLASSEX wc;
        private IntPtr wndProcPtr;
        private WndProcDelegate wndProcDelegate;

        public Win32Window(string title, uint width, uint height)
        {
            bgraBuf = new byte[g_bufSize];
            _title = title;
            Width = (int)width;
            Height = (int)height;
        }

        public void Init()
        {
            startTick = Environment.TickCount;

            wndProcDelegate = MyWndProc;
            wndProcPtr = Marshal.GetFunctionPointerForDelegate(wndProcDelegate);

            wc = new WNDCLASSEX
            {
                cbSize = (uint)Marshal.SizeOf<WNDCLASSEX>(),
                style = 0,
                lpfnWndProc = wndProcPtr,
                cbClsExtra = 0,
                cbWndExtra = 0,
                hInstance = GetModuleHandle(null),
                hIcon = IntPtr.Zero,
                hCursor = LoadCursor(IntPtr.Zero, (IntPtr)32512),
                hbrBackground = IntPtr.Zero,
                lpszMenuName = null,
                lpszClassName = "LVGLSharpWin",
                hIconSm = IntPtr.Zero
            };
            RegisterClassEx(ref wc);

            g_hwnd = CreateWindowExW(
                0, "LVGLSharpWin", _title,
                WS_OVERLAPPEDWINDOW,
                100, 100, Width, Height,
                IntPtr.Zero, IntPtr.Zero, GetModuleHandle(null), IntPtr.Zero
            );

            ShowWindow(g_hwnd, 5);
            UpdateWindow(g_hwnd);

            lv_init();

            lv_tick_set_cb(&my_tick);

            g_display = lv_display_create(Width, Height);

            // Mouse
            indev = lv_indev_create();
            lv_indev_set_type(indev, lv_indev_type_t.LV_INDEV_TYPE_POINTER);
            lv_indev_set_read_cb(indev, &MouseReadCb);

            // Keyboard
            kbd_indev = lv_indev_create();
            lv_indev_set_type(kbd_indev, lv_indev_type_t.LV_INDEV_TYPE_KEYPAD);
            lv_indev_set_read_cb(kbd_indev, &KeyboardReadCb);
            key_inputGroup = lv_group_create();
            lv_indev_set_group(kbd_indev, key_inputGroup);

            g_lvbuf = Marshal.AllocHGlobal((int)g_bufSize);
            lv_display_set_buffers(g_display, g_lvbuf.ToPointer(), null, g_bufSize, LV_DISPLAY_RENDER_MODE_FULL);
            lv_display_set_flush_cb(g_display, &FlushCb);

            root = lv_scr_act();
            _fallbackFont = lv_obj_get_style_text_font(root, LV_PART_MAIN);

            _fontManager = new SixLaborsFontManager(
                SixLaborsSystemFonts.Get("Microsoft YaHei"),
                12,
                GetDPI(),
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

            g_running = false;
            Marshal.FreeHGlobal(g_lvbuf);
        }

        public void ProcessEvents()
        {
            DrainImeCommitQueue();
            lv_timer_handler();

            if (PeekMessage(out msg, IntPtr.Zero, 0, 0, 1))
            {
                if (msg.message == 0x0012)
                {
                    g_running = false;
                    return;
                }

                TranslateMessage(ref msg);
                DispatchMessage(ref msg);
            }
        }

        public void Stop()
        {
            g_running = false;

            if (g_hwnd != IntPtr.Zero)
            {
                DestroyWindow(g_hwnd);
            }
        }
    }
}
