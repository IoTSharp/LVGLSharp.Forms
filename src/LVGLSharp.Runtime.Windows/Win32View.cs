using LVGLSharp;
using LVGLSharp.Interop;
using SixLabors.Fonts;
using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static LVGLSharp.Runtime.Windows.Win32Api;

namespace LVGLSharp.Runtime.Windows
{
    public unsafe class Win32View : ViewLifetimeBase
    {
        static MSG msg;
        static int Width;
        static int Height;
        static IntPtr g_hwnd;
        static lv_display_t* g_display;
        static IntPtr g_lvbuf;
        static lv_obj_t* g_focusSink;

        static uint g_bufSize = 1024 * 1024 * 4;
        static bool g_running;
        static lv_obj_t* label;
        static int startTick;
        static int mouseX = 0, mouseY = 0;
        static bool mousePressed = false;
        static uint mouseButton = 0;
        static byte[] bgraBuf;
        static byte[] _timeBuf = new byte[32];
        static readonly object renderLock = new object();
        static uint last_key_processed;
        static lv_indev_state_t last_key_state_processed = LV_INDEV_STATE_REL;
        static bool ime_composing = false;
        static int pending_ime_char_skips = 0;
        static volatile int mouseWheelDelta = 0;
        static ConcurrentQueue<uint> keyQueue = new ConcurrentQueue<uint>();

        public static lv_obj_t* RootObject { get; set; }
        public static lv_group_t* KeyInputGroupObject { get; set; }
        public static delegate* unmanaged[Cdecl]<lv_event_t*, void> SendTextAreaFocusCallbackCore { get; set; } = &HandleSendTextAreaFocusCb;
        public static uint CurrentMouseButton => mouseButton;
        public static (int X, int Y) CurrentMousePosition => (mouseX, mouseY);
        public override lv_obj_t* Root => RootObject;
        public override lv_group_t* KeyInputGroup => KeyInputGroupObject;
        public override delegate* unmanaged[Cdecl]<lv_event_t*, void> SendTextAreaFocusCallback => SendTextAreaFocusCallbackCore;


        static BITMAPINFO _bmi = new BITMAPINFO
        {
            bmiHeader = new BITMAPINFOHEADER
            {
                biSize = (uint)Marshal.SizeOf(typeof(BITMAPINFOHEADER)),
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
            data->btn_id = mouseButton;
            data->enc_diff = (short)mouseWheelDelta;
            mouseWheelDelta = 0;
        }

        [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
        static void KeyboardReadCb(lv_indev_t* indev, lv_indev_data_t* data)
        {
            if (last_key_state_processed == LV_INDEV_STATE_PR)
            {
                data->key = last_key_processed;
                data->state = LV_INDEV_STATE_REL;
                last_key_state_processed = LV_INDEV_STATE_REL;
                return;
            }

            uint new_key;
            if (keyQueue.TryDequeue(out new_key))
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
            UpdateImeCompositionWindow(target);
        }

        static unsafe lv_obj_t* GetFocusedTextInput()
        {
            if (KeyInputGroupObject == null)
            {
                return null;
            }

            var inputObj = lv_group_get_focused(KeyInputGroupObject);
            return inputObj == g_focusSink ? null : inputObj;
        }

        static unsafe void UpdateImeCompositionWindow(lv_obj_t* target)
        {
            if (target == null)
            {
                return;
            }

            var labelObj = lv_textarea_get_label(target);
            if (labelObj == null)
            {
                return;
            }

            lv_area_t labelArea;
            lv_obj_get_coords(labelObj, &labelArea);

            lv_point_t cursorPoint;
            lv_label_get_letter_pos(labelObj, lv_textarea_get_cursor_pos(target), &cursorPoint);

            var font = lv_obj_get_style_text_font(target, lv_part_t.LV_PART_MAIN);
            int ime_x = labelArea.x1 + cursorPoint.x;
            int ime_y = labelArea.y1 + cursorPoint.y + (font != null ? font->line_height : 0);

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

        static unsafe void CommitImeText(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            var inputObj = GetFocusedTextInput();
            if (inputObj == null)
            {
                return;
            }

            var encoding = Encoding.UTF8;
            int byteCount = encoding.GetByteCount(text);
            byte[]? rentedBuffer = null;
            Span<byte> utf8 = byteCount + 1 <= 256
                ? stackalloc byte[byteCount + 1]
                : (rentedBuffer = ArrayPool<byte>.Shared.Rent(byteCount + 1));

            try
            {
                encoding.GetBytes(text.AsSpan(), utf8);
                utf8[byteCount] = 0;

                fixed (byte* utf8Ptr = utf8)
                {
                    lv_textarea_add_text(inputObj, utf8Ptr);
                }
            }
            finally
            {
                if (rentedBuffer != null)
                {
                    ArrayPool<byte>.Shared.Return(rentedBuffer);
                }
            }
        }

        static string? TryGetCompositionText(IntPtr hIMC, int compositionType)
        {
            int size = ImmGetCompositionStringW(hIMC, compositionType, null, 0);
            if (size <= 0)
            {
                return null;
            }

            byte[] buffer = ArrayPool<byte>.Shared.Rent(size);
            try
            {
                int bytesWritten = ImmGetCompositionStringW(hIMC, compositionType, buffer, size);
                return bytesWritten > 0 ? Encoding.Unicode.GetString(buffer, 0, bytesWritten) : null;
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        static bool PumpPendingMessages()
        {
            bool hadMessages = false;

            while (PeekMessage(out msg, IntPtr.Zero, 0, 0, 1))
            {
                hadMessages = true;

                if (msg.message == 0x0012)
                {
                    g_running = false;
                    break;
                }

                TranslateMessage(ref msg);
                DispatchMessage(ref msg);
            }

            return hadMessages;
        }

        bool ProcessEventsCore()
        {
            bool hadMessages = PumpPendingMessages();
            lv_timer_handler();
            return hadMessages;
        }

        static unsafe bool IsPointInsideObject(lv_obj_t* obj, int x, int y)
        {
            if (obj == null)
            {
                return false;
            }

            lv_area_t area;
            lv_obj_get_coords(obj, &area);
            return x >= area.x1 && x <= area.x2 && y >= area.y1 && y <= area.y2;
        }

        static unsafe void ResetTextInputFocus(int x, int y)
        {
            if (KeyInputGroupObject == null || g_focusSink == null)
            {
                return;
            }

            var focusedObj = lv_group_get_focused(KeyInputGroupObject);
            if (focusedObj == null || focusedObj == g_focusSink || IsPointInsideObject(focusedObj, x, y))
            {
                return;
            }

            lv_textarea_clear_selection(focusedObj);
            lv_obj_clear_state(focusedObj, lv_state_t.LV_STATE_FOCUSED | lv_state_t.LV_STATE_FOCUS_KEY);
            lv_group_focus_obj(g_focusSink);
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
                    mouseButton = 1;
                    mouseX = (short)(lParam.ToInt32() & 0xFFFF);
                    mouseY = (short)((lParam.ToInt32() >> 16) & 0xFFFF);
                    ResetTextInputFocus(mouseX, mouseY);
                    break;
                case 0x0202: // WM_LBUTTONUP
                    mousePressed = false;
                    mouseButton = 0;
                    mouseX = (short)(lParam.ToInt32() & 0xFFFF);
                    mouseY = (short)((lParam.ToInt32() >> 16) & 0xFFFF);
                    break;
                case 0x0204: // WM_RBUTTONDOWN
                    mousePressed = true;
                    mouseButton = 2;
                    mouseX = (short)(lParam.ToInt32() & 0xFFFF);
                    mouseY = (short)((lParam.ToInt32() >> 16) & 0xFFFF);
                    break;
                case 0x0205: // WM_RBUTTONUP
                    mousePressed = false;
                    mouseButton = 0;
                    mouseX = (short)(lParam.ToInt32() & 0xFFFF);
                    mouseY = (short)((lParam.ToInt32() >> 16) & 0xFFFF);
                    break;
                case 0x0200: // WM_MOUSEMOVE
                    mouseX = (short)(lParam.ToInt32() & 0xFFFF);
                    mouseY = (short)((lParam.ToInt32() >> 16) & 0xFFFF);
                    break;
                case WM_IME_STARTCOMPOSITION:
                    UpdateImeCompositionWindow(GetFocusedTextInput());
                    ime_composing = true;
                    break;
                case 0x0102: // WM_CHAR
                    if (ime_composing)
                    {
                        break;
                    }

                    if (pending_ime_char_skips > 0)
                    {
                        pending_ime_char_skips--;
                        break;
                    }

                    keyQueue.Enqueue((uint)wParam);
                    break;
                case WM_IME_ENDCOMPOSITION:
                    ime_composing = false;
                    break;
                case WM_IME_COMPOSITION:
                    {
                        UpdateImeCompositionWindow(GetFocusedTextInput());
                        int compositionFlags = (int)lParam;

                        if ((compositionFlags & GCS_COMPSTR) != 0)
                        {
                            ime_composing = true;
                        }

                        if ((compositionFlags & GCS_RESULTSTR) != 0)
                        {
                            IntPtr hIMC = ImmGetContext(hWnd);
                            if (hIMC != IntPtr.Zero)
                            {
                                string? result = TryGetCompositionText(hIMC, GCS_RESULTSTR);
                                if (!string.IsNullOrEmpty(result))
                                {
                                    CommitImeText(result);
                                    pending_ime_char_skips = Math.Max(pending_ime_char_skips, result.Length);
                                }

                                ime_composing = false;
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
        private readonly bool _borderless;
        private lv_font_t* _fallbackFont;
        private lv_font_t* _defaultFont;
        private lv_style_t* _defaultFontStyle;
        private SixLaborsFontManager _fontManager;
        private lv_indev_t* _keyboardInputDevice;
        private lv_indev_t* _pointerInputDevice;
        private WNDCLASSEX wc;
        private IntPtr wndProcPtr;
        private WndProcDelegate wndProcDelegate;

        public Win32View(string title, uint width, uint height, bool borderless = false)
        {
            bgraBuf = new byte[g_bufSize];
            _title = title;
            Width = (int)width;
            Height = (int)height;
            _borderless = borderless;
        }

        protected override void OnOpenCore()
        {
            LvglNativeLibraryResolver.EnsureRegistered();
            g_running = true;
            startTick = Environment.TickCount;

            wndProcDelegate = MyWndProc;
            wndProcPtr = Marshal.GetFunctionPointerForDelegate(wndProcDelegate);

            wc = new WNDCLASSEX
            {
                cbSize = (uint)Marshal.SizeOf(typeof(WNDCLASSEX)),
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

            var windowRect = new RECT
            {
                left = 0,
                top = 0,
                right = Width,
                bottom = Height,
            };

            int windowStyle = _borderless ? WS_POPUP : WS_OVERLAPPEDWINDOW;
            if (!_borderless)
            {
                AdjustWindowRect(ref windowRect, windowStyle, false);
            }

            g_hwnd = CreateWindowExW(
                0, "LVGLSharpWin", _title,
                windowStyle,
                100,
                100,
                windowRect.right - windowRect.left,
                windowRect.bottom - windowRect.top,
                IntPtr.Zero, IntPtr.Zero, GetModuleHandle(null), IntPtr.Zero
            );

            ShowWindow(g_hwnd, 5);
            UpdateWindow(g_hwnd);

            lv_init();

            lv_tick_set_cb(&my_tick);

            g_display = lv_display_create(Width, Height);

            // Mouse
            _pointerInputDevice = lv_indev_create();
            lv_indev_set_type(_pointerInputDevice, lv_indev_type_t.LV_INDEV_TYPE_POINTER);
            lv_indev_set_read_cb(_pointerInputDevice, &MouseReadCb);

            // Keyboard
            _keyboardInputDevice = lv_indev_create();
            lv_indev_set_type(_keyboardInputDevice, lv_indev_type_t.LV_INDEV_TYPE_KEYPAD);
            lv_indev_set_read_cb(_keyboardInputDevice, &KeyboardReadCb);
            KeyInputGroupObject = lv_group_create();
            lv_indev_set_group(_keyboardInputDevice, KeyInputGroupObject);

            g_lvbuf = Marshal.AllocHGlobal((int)g_bufSize);
            lv_display_set_buffers(g_display, g_lvbuf.ToPointer(), null, g_bufSize, LV_DISPLAY_RENDER_MODE_FULL);
            lv_display_set_flush_cb(g_display, &FlushCb);

            RootObject = lv_scr_act();
            lv_obj_set_flex_flow(RootObject, LV_FLEX_FLOW_COLUMN);
            lv_obj_set_style_pad_all(RootObject, 10, 0);
            lv_obj_remove_flag(RootObject, LV_OBJ_FLAG_SCROLLABLE | LV_OBJ_FLAG_SCROLL_ELASTIC | LV_OBJ_FLAG_SCROLL_MOMENTUM | LV_OBJ_FLAG_SCROLL_CHAIN);
            lv_obj_set_scrollbar_mode(RootObject, LV_SCROLLBAR_MODE_OFF);
            g_focusSink = lv_obj_create(RootObject);
            lv_obj_set_size(g_focusSink, 1, 1);
            lv_obj_add_flag(g_focusSink, lv_obj_flag_t.LV_OBJ_FLAG_HIDDEN | lv_obj_flag_t.LV_OBJ_FLAG_IGNORE_LAYOUT);

            _fallbackFont = lv_obj_get_style_text_font(RootObject, lv_part_t.LV_PART_MAIN);

            _fontManager = new SixLaborsFontManager(SystemFonts.Get("Microsoft YaHei"), 12, GetDPI(), _fallbackFont, [
                61441, 61448, 61451, 61452, 61453, 61457, 61459, 61461, 61465, 61468,
                61473, 61478, 61479, 61480, 61502, 61507, 61512, 61515, 61516, 61517,
                61521, 61522, 61523, 61524, 61543, 61544, 61550, 61552, 61553, 61556,
                61559, 61560, 61561, 61563, 61587, 61589, 61636, 61637, 61639, 61641,
                61664, 61671, 61674, 61683, 61724, 61732, 61787, 61931, 62016, 62017,
                62018, 62019, 62020, 62087, 62099, 62189, 62212, 62810, 63426, 63650
            ]);
            _defaultFont = _fontManager.GetLvFontPtr();

            _defaultFontStyle = (lv_style_t*)NativeMemory.Alloc((nuint)sizeof(lv_style_t));
            NativeMemory.Clear(_defaultFontStyle, (nuint)sizeof(lv_style_t));
            lv_style_init(_defaultFontStyle);
            lv_style_set_text_font(_defaultFontStyle, _defaultFont);

            lv_obj_add_style(RootObject, _defaultFontStyle, 0);
        }
        protected override void RunLoopCore(Action iteration)
        {
            while (g_running)
            {
                bool hadMessages = ProcessEventsCore();
                iteration?.Invoke();

                if (!hadMessages)
                {
                    Thread.Sleep(1);
                }
            }
        }

        public override void HandleEvents()
        {
            ProcessEventsCore();
        }

        protected override void OnCloseCore()
        {
            g_running = false;

            if (_keyboardInputDevice != null)
            {
                lv_indev_delete(_keyboardInputDevice);
                _keyboardInputDevice = null;
            }

            if (_pointerInputDevice != null)
            {
                lv_indev_delete(_pointerInputDevice);
                _pointerInputDevice = null;
            }

            if (KeyInputGroupObject != null)
            {
                lv_group_delete(KeyInputGroupObject);
                KeyInputGroupObject = null;
            }

            if (g_display != null)
            {
                lv_display_delete(g_display);
                g_display = null;
            }

            if (g_lvbuf != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(g_lvbuf);
                g_lvbuf = IntPtr.Zero;
            }

            if (_defaultFontStyle != null)
            {
                lv_style_reset(_defaultFontStyle);
                NativeMemory.Free(_defaultFontStyle);
                _defaultFontStyle = null;
            }

            _fontManager?.Dispose();

            if (g_hwnd != IntPtr.Zero)
            {
                DestroyWindow(g_hwnd);
                g_hwnd = IntPtr.Zero;
            }

            RootObject = null;
            SendTextAreaFocusCallbackCore = &HandleSendTextAreaFocusCb;
            g_focusSink = null;
        }

        protected override bool CanSkipClose() => !g_running && g_hwnd == IntPtr.Zero && g_display == null && g_lvbuf == IntPtr.Zero;

        public override void RegisterTextInput(lv_obj_t* textArea)
        {
        }

    }
}
