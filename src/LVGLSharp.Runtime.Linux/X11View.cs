using LVGLSharp;
using LVGLSharp.Interop;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace LVGLSharp.Runtime.Linux;

public unsafe partial class X11View : ViewLifetimeBase
{
    private const string X11Lib = "libX11.so.6";

    private const int Success = 0;
    private const int False = 0;
    private const int ZPixmap = 2;
    private const int PropModeReplace = 0;
    private const int XUtf8StringStyle = 4;
    private const int MotifHintsDecorations = 1 << 1;

    private const int Button1 = 1;
    private const int Button4 = 4;
    private const int Button5 = 5;

    private const int KeyPress = 2;
    private const int KeyRelease = 3;
    private const int ButtonPress = 4;
    private const int ButtonRelease = 5;
    private const int MotionNotify = 6;
    private const int DestroyNotify = 17;
    private const int ClientMessage = 33;

    private const long KeyPressMask = 1L << 0;
    private const long KeyReleaseMask = 1L << 1;
    private const long ButtonPressMask = 1L << 2;
    private const long ButtonReleaseMask = 1L << 3;
    private const long PointerMotionMask = 1L << 6;
    private const long ExposureMask = 1L << 15;
    private const long StructureNotifyMask = 1L << 17;

    private const nuint XK_Return = 0xff0d;
    private const nuint XK_KP_Enter = 0xff8d;
    private const nuint XK_Escape = 0xff1b;
    private const nuint XK_BackSpace = 0xff08;
    private const nuint XK_Tab = 0xff09;
    private const nuint XK_ISO_Left_Tab = 0xfe20;
    private const nuint XK_Home = 0xff50;
    private const nuint XK_Left = 0xff51;
    private const nuint XK_Up = 0xff52;
    private const nuint XK_Right = 0xff53;
    private const nuint XK_Down = 0xff54;
    private const nuint XK_End = 0xff57;
    private const nuint XK_Delete = 0xffff;
    private const nuint XK_space = 0x0020;
    private const nuint XK_asciitilde = 0x007e;

    private static readonly object s_renderLock = new();
    private static X11View? s_activeView;

    [StructLayout(LayoutKind.Sequential)]
    private struct XTextProperty
    {
        public IntPtr value;
        public nuint encoding;
        public int format;
        public nuint nitems;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MotifWmHints
    {
        public uint flags;
        public uint functions;
        public uint decorations;
        public int inputMode;
        public uint status;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct XKeyEvent
    {
        public int type;
        public nuint serial;
        public int send_event;
        public IntPtr display;
        public nuint window;
        public nuint root;
        public nuint subwindow;
        public nuint time;
        public int x;
        public int y;
        public int x_root;
        public int y_root;
        public uint state;
        public uint keycode;
        public int same_screen;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct XButtonEvent
    {
        public int type;
        public nuint serial;
        public int send_event;
        public IntPtr display;
        public nuint window;
        public nuint root;
        public nuint subwindow;
        public nuint time;
        public int x;
        public int y;
        public int x_root;
        public int y_root;
        public uint state;
        public uint button;
        public int same_screen;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct XMotionEvent
    {
        public int type;
        public nuint serial;
        public int send_event;
        public IntPtr display;
        public nuint window;
        public nuint root;
        public nuint subwindow;
        public nuint time;
        public int x;
        public int y;
        public int x_root;
        public int y_root;
        public uint state;
        public byte is_hint;
        public int same_screen;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct XDestroyWindowEvent
    {
        public int type;
        public nuint serial;
        public int send_event;
        public IntPtr display;
        public nuint @event;
        public nuint window;
    }

    [StructLayout(LayoutKind.Explicit)]
    private struct XClientMessageData
    {
        [FieldOffset(0)] public long l0;
        [FieldOffset(8)] public long l1;
        [FieldOffset(16)] public long l2;
        [FieldOffset(24)] public long l3;
        [FieldOffset(32)] public long l4;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct XClientMessageEvent
    {
        public int type;
        public nuint serial;
        public int send_event;
        public IntPtr display;
        public nuint window;
        public nuint message_type;
        public int format;
        public XClientMessageData data;
    }

    [StructLayout(LayoutKind.Explicit, Size = 192)]
    private struct XEvent
    {
        [FieldOffset(0)] public int type;
        [FieldOffset(0)] public XKeyEvent xkey;
        [FieldOffset(0)] public XButtonEvent xbutton;
        [FieldOffset(0)] public XMotionEvent xmotion;
        [FieldOffset(0)] public XClientMessageEvent xclient;
        [FieldOffset(0)] public XDestroyWindowEvent xdestroywindow;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct XImageFuncs
    {
        public IntPtr create_image;
        public IntPtr destroy_image;
        public IntPtr get_pixel;
        public IntPtr put_pixel;
        public IntPtr sub_image;
        public IntPtr add_pixel;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct XImage
    {
        public int width;
        public int height;
        public int xoffset;
        public int format;
        public IntPtr data;
        public int byte_order;
        public int bitmap_unit;
        public int bitmap_bit_order;
        public int bitmap_pad;
        public int depth;
        public int bytes_per_line;
        public int bits_per_pixel;
        public nuint red_mask;
        public nuint green_mask;
        public nuint blue_mask;
        public IntPtr obdata;
        public XImageFuncs f;
    }

    [LibraryImport(X11Lib, EntryPoint = "XOpenDisplay", StringMarshalling = StringMarshalling.Utf8)]
    private static partial IntPtr XOpenDisplay(string? displayName);

    [LibraryImport(X11Lib, EntryPoint = "XDefaultScreen")]
    private static partial int XDefaultScreen(IntPtr display);

    [LibraryImport(X11Lib, EntryPoint = "XRootWindow")]
    private static partial nuint XRootWindow(IntPtr display, int screenNumber);

    [LibraryImport(X11Lib, EntryPoint = "XDefaultVisual")]
    private static partial IntPtr XDefaultVisual(IntPtr display, int screenNumber);

    [LibraryImport(X11Lib, EntryPoint = "XDefaultDepth")]
    private static partial int XDefaultDepth(IntPtr display, int screenNumber);

    [LibraryImport(X11Lib, EntryPoint = "XCreateSimpleWindow")]
    private static partial nuint XCreateSimpleWindow(
        IntPtr display,
        nuint parent,
        int x,
        int y,
        uint width,
        uint height,
        uint borderWidth,
        nuint border,
        nuint background);

    [LibraryImport(X11Lib, EntryPoint = "XInternAtom", StringMarshalling = StringMarshalling.Utf8)]
    private static partial nuint XInternAtom(IntPtr display, string atomName, int onlyIfExists);

    [LibraryImport(X11Lib, EntryPoint = "XSetWMProtocols")]
    private static partial int XSetWMProtocols(IntPtr display, nuint window, ref nuint protocols, int count);

    [LibraryImport(X11Lib, EntryPoint = "XSelectInput")]
    private static partial int XSelectInput(IntPtr display, nuint window, long eventMask);

    [LibraryImport(X11Lib, EntryPoint = "XMapWindow")]
    private static partial int XMapWindow(IntPtr display, nuint window);

    [LibraryImport(X11Lib, EntryPoint = "XCreateGC")]
    private static partial IntPtr XCreateGC(IntPtr display, nuint drawable, nuint valuemask, IntPtr values);

    [LibraryImport(X11Lib, EntryPoint = "XCreateImage")]
    private static partial IntPtr XCreateImage(
        IntPtr display,
        IntPtr visual,
        uint depth,
        int format,
        int offset,
        IntPtr data,
        uint width,
        uint height,
        int bitmapPad,
        int bytesPerLine);

    [LibraryImport(X11Lib, EntryPoint = "XPending")]
    private static partial int XPending(IntPtr display);

    [LibraryImport(X11Lib, EntryPoint = "XNextEvent")]
    private static partial int XNextEvent(IntPtr display, out XEvent xevent);

    [LibraryImport(X11Lib, EntryPoint = "XLookupKeysym")]
    private static partial nuint XLookupKeysym(ref XKeyEvent keyEvent, int index);

    [LibraryImport(X11Lib, EntryPoint = "XPutImage")]
    private static partial int XPutImage(
        IntPtr display,
        nuint drawable,
        IntPtr gc,
        IntPtr image,
        int srcX,
        int srcY,
        int destX,
        int destY,
        uint width,
        uint height);

    [LibraryImport(X11Lib, EntryPoint = "XFlush")]
    private static partial int XFlush(IntPtr display);

    [LibraryImport(X11Lib, EntryPoint = "XDestroyWindow")]
    private static partial int XDestroyWindow(IntPtr display, nuint window);

    [LibraryImport(X11Lib, EntryPoint = "XFreeGC")]
    private static partial int XFreeGC(IntPtr display, IntPtr gc);

    [LibraryImport(X11Lib, EntryPoint = "XCloseDisplay")]
    private static partial int XCloseDisplay(IntPtr display);

    [LibraryImport(X11Lib, EntryPoint = "Xutf8TextListToTextProperty")]
    private static partial int Xutf8TextListToTextProperty(IntPtr display, IntPtr list, int count, int style, out XTextProperty textProperty);

    [LibraryImport(X11Lib, EntryPoint = "XSetWMName")]
    private static partial void XSetWMName(IntPtr display, nuint window, ref XTextProperty textProperty);

    [LibraryImport(X11Lib, EntryPoint = "XSetWMIconName")]
    private static partial void XSetWMIconName(IntPtr display, nuint window, ref XTextProperty textProperty);

    [LibraryImport(X11Lib, EntryPoint = "XChangeProperty")]
    private static partial int XChangeProperty(
        IntPtr display,
        nuint window,
        nuint property,
        nuint type,
        int format,
        int mode,
        IntPtr data,
        int elementCount);

    [LibraryImport(X11Lib, EntryPoint = "XFree")]
    private static partial int XFree(IntPtr data);

    private readonly string _title;
    private readonly int _width;
    private readonly int _height;
    private readonly float _dpi;
    private readonly string? _requestedDisplayName;
    private readonly bool _borderless;

    private IntPtr _display;
    private int _screen;
    private nuint _window;
    private IntPtr _gc;
    private nuint _wmDeleteWindow;
    private IntPtr _xImage;
    private uint* _frameBuffer;
    private byte* _drawBuffer;
    private uint _drawBufferByteSize;

    private lv_display_t* _lvDisplay;
    private lv_indev_t* _mouseIndev;
    private lv_indev_t* _keyboardIndev;
    private lv_indev_t* _wheelIndev;

    private int _mouseX;
    private int _mouseY;
    private bool _mousePressed;
    private uint _mouseButton;
    private uint _lastKey;
    private bool _keyPressed;
    private short _wheelDiff;
    private ulong _lastPresentTick;
    private bool _running;
    private bool _initialized;

    private lv_font_t* _fallbackFont;
    private lv_style_t* _defaultFontStyle;
    private SixLaborsFontManager? _fontManager;
    private string? _resolvedSystemFontPath;
    private string? _fontDiagnosticSummary;
    private string? _fontGlyphDiagnosticSummary;

    public X11View(string title = "LVGLSharp X11", int width = 800, int height = 600, float dpi = 96f, string? displayName = null, bool borderless = false)
    {
        _title = title;
        _width = width;
        _height = height;
        _dpi = dpi;
        _requestedDisplayName = displayName;
        _borderless = borderless;
    }

    public static lv_obj_t* RootObject { get; private set; }
    public static lv_group_t* KeyInputGroupObject { get; private set; }
    public static delegate* unmanaged[Cdecl]<lv_event_t*, void> SendTextAreaFocusCallbackCore { get; private set; }
    public static (int X, int Y) CurrentMousePosition => s_activeView is null ? (0, 0) : (s_activeView._mouseX, s_activeView._mouseY);
    public static uint CurrentMouseButton => s_activeView?._mouseButton ?? 0U;

    public override lv_obj_t* Root => RootObject;
    public override lv_group_t* KeyInputGroup => KeyInputGroupObject;
    public override delegate* unmanaged[Cdecl]<lv_event_t*, void> SendTextAreaFocusCallback => SendTextAreaFocusCallbackCore;

    protected override void OnOpenCore()
    {
        LvglNativeLibraryResolver.EnsureRegistered();

        if (!lv_is_initialized())
        {
            lv_init();
        }

        InitializeWindow();
        InitializeLvgl();

        RootObject = lv_scr_act();
        KeyInputGroupObject = lv_group_create();
        lv_indev_set_group(_keyboardIndev, KeyInputGroupObject);
        _fallbackFont = lv_obj_get_style_text_font(RootObject, lv_part_t.LV_PART_MAIN);
        _fontDiagnosticSummary = LinuxSystemFontResolver.GetFontPathDiagnosticSummary();
        _fontGlyphDiagnosticSummary = LinuxSystemFontResolver.GetGlyphDiagnosticSummary();

        _resolvedSystemFontPath = LinuxSystemFontResolver.TryResolveFontPath();
        if (!string.IsNullOrWhiteSpace(_resolvedSystemFontPath))
        {
            _fontManager = new SixLaborsFontManager(
                _resolvedSystemFontPath,
                12,
                _dpi,
                _fallbackFont,
                LvglHostDefaults.CreateDefaultFontFallbackGlyphs());

            _defaultFontStyle = LvglHostDefaults.ApplyDefaultFontStyle(RootObject, _fontManager.GetLvFontPtr());
        }

        SendTextAreaFocusCallbackCore = null;
        s_activeView = this;
        _running = true;
        _lastPresentTick = (ulong)Environment.TickCount64;
        _initialized = true;
    }

    public override void RegisterTextInput(lv_obj_t* textArea)
    {
        // X11 host 暂时只提供硬件键盘路径。
    }

    protected override void RunLoopCore(Action iteration)
    {
        while (_running)
        {
            HandleEvents();
            iteration?.Invoke();
        }
    }

    public override void HandleEvents()
    {
        if (!_initialized)
        {
            return;
        }

        PollEvents();
        PresentFrame();
    }

    protected override void OnCloseCore()
    {
        if (s_activeView == this)
        {
            s_activeView = null;
        }

        _running = false;

        if (_mouseIndev != null)
        {
            lv_indev_delete(_mouseIndev);
            _mouseIndev = null;
        }

        if (_keyboardIndev != null)
        {
            lv_indev_delete(_keyboardIndev);
            _keyboardIndev = null;
        }

        if (_wheelIndev != null)
        {
            lv_indev_delete(_wheelIndev);
            _wheelIndev = null;
        }

        if (KeyInputGroupObject != null)
        {
            lv_group_delete(KeyInputGroupObject);
            KeyInputGroupObject = null;
        }

        if (_lvDisplay != null)
        {
            lv_display_delete(_lvDisplay);
            _lvDisplay = null;
        }

        if (_xImage != IntPtr.Zero)
        {
            DestroyXImage();
        }

        if (_frameBuffer != null)
        {
            NativeMemory.Free(_frameBuffer);
            _frameBuffer = null;
        }

        if (_drawBuffer != null)
        {
            NativeMemory.Free(_drawBuffer);
            _drawBuffer = null;
            _drawBufferByteSize = 0;
        }

        if (_gc != IntPtr.Zero && _display != IntPtr.Zero)
        {
            XFreeGC(_display, _gc);
            _gc = IntPtr.Zero;
        }

        if (_window != 0 && _display != IntPtr.Zero)
        {
            XDestroyWindow(_display, _window);
            _window = 0;
        }

        if (_display != IntPtr.Zero)
        {
            XCloseDisplay(_display);
            _display = IntPtr.Zero;
        }

        _fontManager?.Dispose();
        _fontManager = null;
        _resolvedSystemFontPath = null;
        _fontDiagnosticSummary = null;
        _fontGlyphDiagnosticSummary = null;

        RootObject = null;
        SendTextAreaFocusCallbackCore = null;
        _lastKey = 0;
        _keyPressed = false;
        _wheelDiff = 0;
        _initialized = false;
    }

    protected override bool CanSkipClose() =>
        !_initialized &&
        _display == IntPtr.Zero &&
        _window == 0 &&
        _gc == IntPtr.Zero &&
        _xImage == IntPtr.Zero &&
        _frameBuffer == null &&
        _drawBuffer == null;


    public override string ToString()
    {
        string connectedDisplay = _requestedDisplayName ?? "<default>";

        return $"Title={_title}, Display={connectedDisplay}, Window={_window != 0}:{_width}x{_height}, Running={_running}, Initialized={_initialized}, LvDisplay={_lvDisplay != null}, Root={RootObject != null}, KeyGroup={KeyInputGroupObject != null}, FontPath={_resolvedSystemFontPath ?? "<none>"}, FontDiag={_fontDiagnosticSummary ?? "<unresolved>"}, GlyphDiag={_fontGlyphDiagnosticSummary ?? "<unresolved>"}";
    }

    private void InitializeWindow()
    {
        var displayCandidates = LinuxEnvironmentDetector.GetX11DisplayCandidates(_requestedDisplayName);
        foreach (var displayCandidate in displayCandidates)
        {
            _display = XOpenDisplay(displayCandidate);
            if (_display != IntPtr.Zero)
            {
                break;
            }
        }

        if (_display == IntPtr.Zero)
        {
            var attemptedDisplays = string.Join(", ", displayCandidates.Select(static candidate => candidate ?? "<process-default>"));
            throw new InvalidOperationException($"初始化 X11 host 失败，请检查 X11 环境是否可用。已尝试: {attemptedDisplays}");
        }

        _screen = XDefaultScreen(_display);
        var rootWindow = XRootWindow(_display, _screen);
        _window = XCreateSimpleWindow(_display, rootWindow, 10, 10, (uint)_width, (uint)_height, _borderless ? 0u : 1u, 0, 0);
        if (_window == 0)
        {
            throw new InvalidOperationException("X11 窗口创建失败。");
        }

        _wmDeleteWindow = XInternAtom(_display, "WM_DELETE_WINDOW", False);
        SetWindowTitleUtf8(_title);
        ApplyWindowChrome();
        XSetWMProtocols(_display, _window, ref _wmDeleteWindow, 1);
        XSelectInput(
            _display,
            _window,
            ExposureMask |
            ButtonPressMask |
            ButtonReleaseMask |
            PointerMotionMask |
            KeyPressMask |
            KeyReleaseMask |
            StructureNotifyMask);

        _gc = XCreateGC(_display, _window, 0, IntPtr.Zero);
        if (_gc == IntPtr.Zero)
        {
            throw new InvalidOperationException("X11 图形上下文创建失败。");
        }

        AllocateBuffers();

        var visual = XDefaultVisual(_display, _screen);
        var depth = XDefaultDepth(_display, _screen);
        _xImage = XCreateImage(
            _display,
            visual,
            (uint)depth,
            ZPixmap,
            0,
            (IntPtr)_frameBuffer,
            (uint)_width,
            (uint)_height,
            32,
            _width * sizeof(uint));

        if (_xImage == IntPtr.Zero)
        {
            throw new InvalidOperationException("X11 图像缓冲创建失败。");
        }

        XMapWindow(_display, _window);
        XFlush(_display);
    }

    private void ApplyWindowChrome()
    {
        if (!_borderless || _display == IntPtr.Zero || _window == 0)
        {
            return;
        }

        var motifHintsAtom = XInternAtom(_display, "_MOTIF_WM_HINTS", False);
        if (motifHintsAtom == 0)
        {
            return;
        }

        var hints = new MotifWmHints
        {
            flags = MotifHintsDecorations,
            functions = 0,
            decorations = 0,
            inputMode = 0,
            status = 0,
        };

        unsafe
        {
            XChangeProperty(
                _display,
                _window,
                motifHintsAtom,
                motifHintsAtom,
                32,
                PropModeReplace,
                (IntPtr)(&hints),
                5);
        }
    }

    private void InitializeLvgl()
    {
        _lvDisplay = lv_display_create(_width, _height);
        if (_lvDisplay == null)
        {
            throw new InvalidOperationException("LVGL display 创建失败。");
        }

        lv_display_set_buffers(_lvDisplay, _drawBuffer, null, _drawBufferByteSize, LV_DISPLAY_RENDER_MODE_FULL);
        lv_display_set_flush_cb(_lvDisplay, &FlushCb);

        _mouseIndev = lv_indev_create();
        lv_indev_set_type(_mouseIndev, LV_INDEV_TYPE_POINTER);
        lv_indev_set_read_cb(_mouseIndev, &MouseReadCb);
        lv_indev_set_display(_mouseIndev, _lvDisplay);

        _keyboardIndev = lv_indev_create();
        lv_indev_set_type(_keyboardIndev, LV_INDEV_TYPE_KEYPAD);
        lv_indev_set_read_cb(_keyboardIndev, &KeyboardReadCb);
        lv_indev_set_display(_keyboardIndev, _lvDisplay);

        _wheelIndev = lv_indev_create();
        lv_indev_set_type(_wheelIndev, LV_INDEV_TYPE_ENCODER);
        lv_indev_set_read_cb(_wheelIndev, &WheelReadCb);
        lv_indev_set_display(_wheelIndev, _lvDisplay);
    }

    private void AllocateBuffers()
    {
        _frameBuffer = (uint*)NativeMemory.AllocZeroed((nuint)(_width * _height), (nuint)sizeof(uint));
        if (_frameBuffer == null)
        {
            throw new OutOfMemoryException("X11 framebuffer 分配失败。");
        }

        _drawBufferByteSize = checked((uint)(_width * _height * sizeof(ushort)));
        _drawBuffer = (byte*)NativeMemory.AllocZeroed((nuint)_drawBufferByteSize);
        if (_drawBuffer == null)
        {
            throw new OutOfMemoryException("LVGL draw buffer 分配失败。");
        }
    }

    private void PollEvents()
    {
        while (XPending(_display) > 0)
        {
            XNextEvent(_display, out var ev);

            switch (ev.type)
            {
                case MotionNotify:
                    _mouseX = ev.xmotion.x;
                    _mouseY = ev.xmotion.y;
                    break;
                case ButtonPress:
                    if (ev.xbutton.button == Button1)
                    {
                        _mousePressed = true;
                        _mouseButton = 1;
                        _mouseX = ev.xbutton.x;
                        _mouseY = ev.xbutton.y;
                    }
                    else if (ev.xbutton.button == 3)
                    {
                        _mouseButton = 2;
                        _mouseX = ev.xbutton.x;
                        _mouseY = ev.xbutton.y;
                    }
                    else if (ev.xbutton.button == 2)
                    {
                        _mouseButton = 4;
                        _mouseX = ev.xbutton.x;
                        _mouseY = ev.xbutton.y;
                    }
                    else if (ev.xbutton.button == Button4)
                    {
                        _wheelDiff++;
                    }
                    else if (ev.xbutton.button == Button5)
                    {
                        _wheelDiff--;
                    }
                    break;
                case ButtonRelease:
                    if (ev.xbutton.button == Button1)
                    {
                        _mousePressed = false;
                        _mouseButton = 0;
                        _mouseX = ev.xbutton.x;
                        _mouseY = ev.xbutton.y;
                    }
                    else if (ev.xbutton.button is 2 or 3)
                    {
                        _mouseButton = 0;
                        _mouseX = ev.xbutton.x;
                        _mouseY = ev.xbutton.y;
                    }
                    break;
                case KeyPress:
                    var key = KeysymToLvKey(XLookupKeysym(ref ev.xkey, 0));
                    if (key != 0)
                    {
                        _lastKey = key;
                        _keyPressed = true;
                    }
                    break;
                case KeyRelease:
                    _keyPressed = false;
                    break;
                case ClientMessage:
                    if (ev.xclient.data.l0 == (long)_wmDeleteWindow)
                    {
                        _running = false;
                    }
                    break;
                case DestroyNotify:
                    _running = false;
                    break;
            }
        }
    }

    private void PresentFrame()
    {
        var now = (ulong)Environment.TickCount64;
        var diff = _lastPresentTick == 0 ? 0U : (uint)(now - _lastPresentTick);
        _lastPresentTick = now;

        lv_tick_inc(diff);
        lv_timer_handler();

        if (_display != IntPtr.Zero && _window != 0 && _gc != IntPtr.Zero && _xImage != IntPtr.Zero)
        {
            lock (s_renderLock)
            {
                XPutImage(_display, _window, _gc, _xImage, 0, 0, 0, 0, (uint)_width, (uint)_height);
            }
            XFlush(_display);
        }

        Thread.Sleep(5);
    }

    private void DestroyXImage()
    {
        var image = (XImage*)_xImage;
        image->data = IntPtr.Zero;

        if (image->f.destroy_image != IntPtr.Zero)
        {
            var destroy = (delegate* unmanaged[Cdecl]<XImage*, int>)image->f.destroy_image;
            destroy(image);
        }
        else
        {
            XFree(_xImage);
        }

        _xImage = IntPtr.Zero;
    }

    private void SetWindowTitleUtf8(string title)
    {
        var titlePtr = Marshal.StringToCoTaskMemUTF8(title);
        var titleListPtr = Marshal.AllocHGlobal(IntPtr.Size);
        XTextProperty textProperty = default;

        try
        {
            Marshal.WriteIntPtr(titleListPtr, titlePtr);

            if (Xutf8TextListToTextProperty(_display, titleListPtr, 1, XUtf8StringStyle, out textProperty) == Success)
            {
                XSetWMName(_display, _window, ref textProperty);
                XSetWMIconName(_display, _window, ref textProperty);
            }

            var utf8String = XInternAtom(_display, "UTF8_STRING", False);
            var netWmName = XInternAtom(_display, "_NET_WM_NAME", False);
            var netWmVisibleName = XInternAtom(_display, "_NET_WM_VISIBLE_NAME", False);
            if (utf8String != 0)
            {
                var titleBytes = Encoding.UTF8.GetBytes(title);
                fixed (byte* titleBytesPtr = titleBytes)
                {
                    XChangeProperty(_display, _window, netWmName, utf8String, 8, PropModeReplace, (IntPtr)titleBytesPtr, titleBytes.Length);
                    XChangeProperty(_display, _window, netWmVisibleName, utf8String, 8, PropModeReplace, (IntPtr)titleBytesPtr, titleBytes.Length);
                }
            }
        }
        finally
        {
            if (textProperty.value != IntPtr.Zero)
            {
                XFree(textProperty.value);
            }

            Marshal.FreeHGlobal(titleListPtr);
            Marshal.FreeCoTaskMem(titlePtr);
        }
    }

    private static uint KeysymToLvKey(nuint keysym)
    {
        return keysym switch
        {
            XK_Return or XK_KP_Enter => (uint)LV_KEY_ENTER,
            XK_Escape => (uint)LV_KEY_ESC,
            XK_BackSpace => (uint)LV_KEY_BACKSPACE,
            XK_Tab => (uint)LV_KEY_NEXT,
            XK_ISO_Left_Tab => (uint)LV_KEY_PREV,
            XK_Home => (uint)LV_KEY_HOME,
            XK_End => (uint)LV_KEY_END,
            XK_Delete => (uint)LV_KEY_DEL,
            XK_Left => (uint)LV_KEY_LEFT,
            XK_Right => (uint)LV_KEY_RIGHT,
            XK_Up => (uint)LV_KEY_UP,
            XK_Down => (uint)LV_KEY_DOWN,
            >= XK_space and <= XK_asciitilde => (uint)keysym,
            _ => 0,
        };
    }

    private static uint ConvertRgb565ToX11(ushort pixel)
    {
        uint r = (uint)((pixel >> 11) & 0x1F);
        uint g = (uint)((pixel >> 5) & 0x3F);
        uint b = (uint)(pixel & 0x1F);

        r = (r << 3) | (r >> 2);
        g = (g << 2) | (g >> 4);
        b = (b << 3) | (b >> 2);

        return r | (g << 8) | (b << 16);
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static void FlushCb(lv_display_t* display, lv_area_t* area, byte* pxMap)
    {
        var view = s_activeView;
        if (view is null || view._frameBuffer == null)
        {
            lv_display_flush_ready(display);
            return;
        }

        lock (s_renderLock)
        {
            var width = lv_area_get_width(area);
            var height = lv_area_get_height(area);

            for (var y = 0; y < height; y++)
            {
                var dst = view._frameBuffer + (area->y1 + y) * view._width + area->x1;
                var src = ((ushort*)pxMap) + y * width;
                for (var x = 0; x < width; x++)
                {
                    dst[x] = ConvertRgb565ToX11(src[x]);
                }
            }
        }

        lv_display_flush_ready(display);
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static void MouseReadCb(lv_indev_t* indev, lv_indev_data_t* data)
    {
        var view = s_activeView;
        if (view is null)
        {
            data->point.x = 0;
            data->point.y = 0;
            data->state = LV_INDEV_STATE_REL;
            data->enc_diff = 0;
            return;
        }

        data->point.x = view._mouseX;
        data->point.y = view._mouseY;
        data->state = view._mousePressed ? LV_INDEV_STATE_PR : LV_INDEV_STATE_REL;
        data->enc_diff = 0;
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static void KeyboardReadCb(lv_indev_t* indev, lv_indev_data_t* data)
    {
        var view = s_activeView;
        if (view is null)
        {
            data->key = 0;
            data->state = LV_INDEV_STATE_REL;
            return;
        }

        data->key = view._lastKey;
        data->state = view._keyPressed ? LV_INDEV_STATE_PR : LV_INDEV_STATE_REL;
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static void WheelReadCb(lv_indev_t* indev, lv_indev_data_t* data)
    {
        var view = s_activeView;
        if (view is null)
        {
            data->enc_diff = 0;
            data->state = LV_INDEV_STATE_REL;
            return;
        }

        data->enc_diff = view._wheelDiff;
        view._wheelDiff = 0;
        data->state = LV_INDEV_STATE_REL;
    }
}
