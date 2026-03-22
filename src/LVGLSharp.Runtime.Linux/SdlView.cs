using LVGLSharp;
using LVGLSharp.Interop;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace LVGLSharp.Runtime.Linux;

public unsafe sealed partial class SdlView : ViewLifetimeBase
{
    private static SdlView? s_activeView;

    private readonly string _title;
    private int _width;
    private int _height;
    private readonly float _dpi;
    private readonly bool _borderless;

    private readonly SdlInputSource _inputSource = new();
    private readonly SdlBufferPresenter _bufferPresenter;
    private lv_display_t* _lvDisplay;
    private lv_indev_t* _mouseIndev;
    private lv_indev_t* _keyboardIndev;
    private lv_indev_t* _wheelIndev;
    private bool _running;
    private bool _initialized;
    private ulong _lastPresentTick;
    private bool _hasPendingResize;
    private int _pendingWidth;
    private int _pendingHeight;

    private lv_font_t* _fallbackFont;
    private lv_style_t* _defaultFontStyle;
    private SixLaborsFontManager? _fontManager;
    private string? _resolvedSystemFontPath;
    private string? _fontDiagnosticSummary;
    private string? _fontGlyphDiagnosticSummary;
    private lv_obj_t* _root;
    private lv_group_t* _keyInputGroup;
    private GCHandle _selfHandle;
    private lv_obj_t* _focusedTextArea;
    public SdlView(string title = "LVGLSharp SDL", int width = 800, int height = 600, float dpi = 96f, bool borderless = false)
    {
        _title = title;
        _width = width;
        _height = height;
        _dpi = dpi;
        _borderless = borderless;
        _bufferPresenter = new SdlBufferPresenter(width, height, dpi);
    }

    public static (int X, int Y) CurrentMousePosition => SdlInputSource.LastMousePosition;

    public static uint CurrentMouseButton => SdlInputSource.LastMouseButton;

    public override lv_obj_t* Root => _root;

    public override lv_group_t* KeyInputGroup => _keyInputGroup;

    public override delegate* unmanaged[Cdecl]<lv_event_t*, void> SendTextAreaFocusCallback => &HandleTextAreaFocused;

    public delegate* unmanaged[Cdecl]<lv_event_t*, void> SendTextAreaDefocusCallback => &HandleTextAreaDefocused;

    protected override void OnOpenCore()
    {
        LvglNativeLibraryResolver.EnsureRegistered();

        if (!lv_is_initialized())
        {
            lv_init();
        }

        InitializeSdl();
        InitializeLvgl();

        _root = lv_scr_act();
        _keyInputGroup = lv_group_create();
        lv_indev_set_group(_keyboardIndev, _keyInputGroup);
        _fallbackFont = lv_obj_get_style_text_font(_root, lv_part_t.LV_PART_MAIN);
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

            _defaultFontStyle = LvglHostDefaults.ApplyDefaultFontStyle(_root, _fontManager.GetLvFontPtr());
        }

        s_activeView = this;
        _running = true;
        _lastPresentTick = (ulong)Environment.TickCount64;
        _initialized = true;
    }

    public override void HandleEvents()
    {
        if (!_initialized)
        {
            return;
        }

        PollEvents();
        CommitPendingTextInput();
        HandlePendingResize();
        PresentFrame();
    }

    protected override void RunLoopCore(Action iteration)
    {
        while (_running)
        {
            HandleEvents();
            iteration?.Invoke();
        }
    }

    protected override void OnCloseCore()
    {
        if (s_activeView == this)
        {
            s_activeView = null;
        }

        _running = false;

        if (!_initialized &&
            _lvDisplay == null &&
            _mouseIndev == null &&
            _keyboardIndev == null &&
            _wheelIndev == null &&
            _root == null &&
            _keyInputGroup == null)
        {
            TryBeginClose();
            return;
        }

        if (!TryBeginClose())
        {
            return;
        }

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

        if (_keyInputGroup != null)
        {
            lv_group_delete(_keyInputGroup);
            _keyInputGroup = null;
        }

        if (_lvDisplay != null)
        {
            lv_display_delete(_lvDisplay);
            _lvDisplay = null;
        }

        _bufferPresenter.Dispose();

        _fontManager?.Dispose();
        _fontManager = null;
        _resolvedSystemFontPath = null;
        _fontDiagnosticSummary = null;
        _fontGlyphDiagnosticSummary = null;

        _root = null;
        _keyInputGroup = null;
        _focusedTextArea = null;
        _inputSource.Reset();
        _hasPendingResize = false;
        _pendingWidth = 0;
        _pendingHeight = 0;
        _lastPresentTick = 0;

        if (_selfHandle.IsAllocated)
        {
            _selfHandle.Free();
        }

        SdlNative.SDL_StopTextInput();

        _initialized = false;
    }

    protected override bool CanSkipClose() =>
        !_initialized &&
        _lvDisplay == null &&
        _mouseIndev == null &&
        _keyboardIndev == null &&
        _wheelIndev == null &&
        _root == null &&
        _keyInputGroup == null;

    public override void RegisterTextInput(lv_obj_t* textArea)
    {
        UpdateFocusedTextArea(textArea);
    }


    public override string ToString()
    {
        return $"Title={_title}, Window={_bufferPresenter.Window != IntPtr.Zero}:{_width}x{_height}, Renderer={_bufferPresenter.Renderer != IntPtr.Zero}, Texture={_bufferPresenter.Texture != IntPtr.Zero}, Running={_running}, Initialized={_initialized}, LvDisplay={_lvDisplay != null}, Root={_root != null}, KeyGroup={_keyInputGroup != null}, FontPath={_resolvedSystemFontPath ?? "<none>"}, FontDiag={_fontDiagnosticSummary ?? "<unresolved>"}, GlyphDiag={_fontGlyphDiagnosticSummary ?? "<unresolved>"}";
    }

    private void InitializeSdl()
    {
        _bufferPresenter.InitializeWindow(_title, _borderless);
    }

    private void InitializeLvgl()
    {
        _lvDisplay = lv_display_create(_bufferPresenter.PixelWidth, _bufferPresenter.PixelHeight);
        if (_lvDisplay == null)
        {
            throw new InvalidOperationException("LVGL display ´´½¨Ê§°Ü¡£");
        }

        if (!_selfHandle.IsAllocated)
        {
            _selfHandle = GCHandle.Alloc(this);
        }

        var selfPtr = (void*)GCHandle.ToIntPtr(_selfHandle);
        lv_display_set_user_data(_lvDisplay, selfPtr);

        lv_display_set_buffers(_lvDisplay, _bufferPresenter.DrawBuffer, null, _bufferPresenter.DrawBufferByteSize, LV_DISPLAY_RENDER_MODE_FULL);
        lv_display_set_flush_cb(_lvDisplay, &FlushCb);

        _mouseIndev = lv_indev_create();
        lv_indev_set_type(_mouseIndev, LV_INDEV_TYPE_POINTER);
        lv_indev_set_read_cb(_mouseIndev, &MouseReadCb);
        lv_indev_set_display(_mouseIndev, _lvDisplay);
        lv_indev_set_user_data(_mouseIndev, selfPtr);

        _keyboardIndev = lv_indev_create();
        lv_indev_set_type(_keyboardIndev, LV_INDEV_TYPE_KEYPAD);
        lv_indev_set_read_cb(_keyboardIndev, &KeyboardReadCb);
        lv_indev_set_display(_keyboardIndev, _lvDisplay);
        lv_indev_set_user_data(_keyboardIndev, selfPtr);

        _wheelIndev = lv_indev_create();
        lv_indev_set_type(_wheelIndev, LV_INDEV_TYPE_ENCODER);
        lv_indev_set_read_cb(_wheelIndev, &WheelReadCb);
        lv_indev_set_display(_wheelIndev, _lvDisplay);
        lv_indev_set_user_data(_wheelIndev, selfPtr);
    }

    private void PollEvents()
    {
        while (SdlNative.SDL_PollEvent(out var sdlEvent) != 0)
        {
            if (_inputSource.TryHandleEvent(sdlEvent, out var closeRequested, out var resize))
            {
                if (closeRequested)
                {
                    _running = false;
                }

                if (resize is { } size)
                {
                    QueueResize(size.Width, size.Height);
                }
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

        if (!_bufferPresenter.IsReady)
        {
            Thread.Sleep(5);
            return;
        }

        _bufferPresenter.Present();

        Thread.Sleep(5);
    }

    private void QueueResize(int width, int height)
    {
        if (width <= 0 || height <= 0)
        {
            return;
        }

        if (width == _width && height == _height)
        {
            return;
        }

        _pendingWidth = width;
        _pendingHeight = height;
        _hasPendingResize = true;
    }

    private void HandlePendingResize()
    {
        if (!_hasPendingResize)
        {
            return;
        }

        ResizeBuffers(_pendingWidth, _pendingHeight);
        _hasPendingResize = false;
    }

    private void ResizeBuffers(int width, int height)
    {
        if (width <= 0 || height <= 0)
        {
            return;
        }

        _bufferPresenter.ResizeIfNeeded(width, height, _lvDisplay, _root);
        _width = width;
        _height = height;
    }

    private void CommitPendingTextInput()
    {
        CommitEditingKeys();

        var pendingText = _inputSource.ConsumePendingText();
        if (string.IsNullOrEmpty(pendingText) || _focusedTextArea == null)
        {
            return;
        }

        var utf8Bytes = Encoding.UTF8.GetBytes(pendingText + "\0");
        fixed (byte* utf8Ptr = utf8Bytes)
        {
            lv_textarea_add_text(_focusedTextArea, utf8Ptr);
        }
    }

    private void CommitEditingKeys()
    {
        if (_focusedTextArea == null)
        {
            return;
        }

        switch (_inputSource.ConsumeEditingKeyPress())
        {
            case (uint)LV_KEY_BACKSPACE:
                lv_textarea_delete_char(_focusedTextArea);
                break;
            case (uint)LV_KEY_DEL:
                lv_textarea_delete_char_forward(_focusedTextArea);
                break;
            case (uint)LV_KEY_LEFT:
                lv_textarea_cursor_left(_focusedTextArea);
                break;
            case (uint)LV_KEY_RIGHT:
                lv_textarea_cursor_right(_focusedTextArea);
                break;
            case (uint)LV_KEY_UP:
                lv_textarea_cursor_up(_focusedTextArea);
                break;
            case (uint)LV_KEY_DOWN:
                lv_textarea_cursor_down(_focusedTextArea);
                break;
            default:
                return;
        }
    }

    private void UpdateFocusedTextArea(lv_obj_t* textArea)
    {
        _focusedTextArea = textArea;
        if (_focusedTextArea != null)
        {
            SdlNative.SDL_StartTextInput();
        }
        else
        {
            SdlNative.SDL_StopTextInput();
        }
    }

    private static SdlView? GetViewFromDisplay(lv_display_t* display)
    {
        if (display == null)
        {
            return null;
        }

        var userData = lv_display_get_user_data(display);
        return userData == null ? null : (SdlView?)GCHandle.FromIntPtr((IntPtr)userData).Target;
    }

    private static SdlView? GetViewFromIndev(lv_indev_t* indev)
    {
        if (indev == null)
        {
            return null;
        }

        var userData = lv_indev_get_user_data(indev);
        return userData == null ? null : (SdlView?)GCHandle.FromIntPtr((IntPtr)userData).Target;
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static void HandleTextAreaFocused(lv_event_t* e)
    {
        var target = lv_event_get_target_obj(e);
        var view = s_activeView;
        if (view is null)
        {
            return;
        }

        view.UpdateFocusedTextArea(target);
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static void HandleTextAreaDefocused(lv_event_t* e)
    {
        var view = s_activeView;
        if (view is null)
        {
            return;
        }

        view.UpdateFocusedTextArea(null);
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static void FlushCb(lv_display_t* display, lv_area_t* area, byte* pxMap)
    {
        var view = GetViewFromDisplay(display) ?? s_activeView;
        if (view is null)
        {
            lv_display_flush_ready(display);
            return;
        }

        view._bufferPresenter.Flush(display, area, pxMap);
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static void MouseReadCb(lv_indev_t* indev, lv_indev_data_t* data)
    {
        var view = GetViewFromIndev(indev) ?? s_activeView;
        if (view is null)
        {
            data->point.x = 0;
            data->point.y = 0;
            data->state = LV_INDEV_STATE_REL;
            data->enc_diff = 0;
            return;
        }

        data->point.x = view._inputSource.CurrentMousePosition.X;
        data->point.y = view._inputSource.CurrentMousePosition.Y;
        data->state = view._inputSource.IsMousePressed ? LV_INDEV_STATE_PR : LV_INDEV_STATE_REL;
        data->enc_diff = 0;
        data->btn_id = view._inputSource.CurrentMouseButton;
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static void KeyboardReadCb(lv_indev_t* indev, lv_indev_data_t* data)
    {
        var view = GetViewFromIndev(indev) ?? s_activeView;
        if (view is null)
        {
            data->key = 0;
            data->state = LV_INDEV_STATE_REL;
            return;
        }

        data->key = view._inputSource.CurrentKey;
        data->state = view._inputSource.IsKeyPressed ? LV_INDEV_STATE_PR : LV_INDEV_STATE_REL;
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static void WheelReadCb(lv_indev_t* indev, lv_indev_data_t* data)
    {
        var view = GetViewFromIndev(indev) ?? s_activeView;
        if (view is null)
        {
            data->enc_diff = 0;
            data->state = LV_INDEV_STATE_REL;
            return;
        }

        data->enc_diff = (short)view._inputSource.ConsumeWheelDiff();
        data->state = LV_INDEV_STATE_REL;
    }
}