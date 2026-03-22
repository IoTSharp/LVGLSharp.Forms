using LVGLSharp;
using LVGLSharp.Interop;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace LVGLSharp.Runtime.Linux;

public unsafe sealed class WaylandView : ViewLifetimeBase
{
    private static WaylandView? s_activeView;
    private static int s_startTick;

    private readonly WaylandDisplayConnection _connection;
    private readonly WaylandWindow _window;
    private readonly WaylandInputSource _inputSource;
    private readonly WaylandBufferPresenter _bufferPresenter;
    private readonly X11View? _fallbackView;
    private readonly string _diagnosticSummary;
    private lv_display_t* _lvDisplay;
    private lv_indev_t* _mouseIndev;
    private lv_indev_t* _keyboardIndev;
    private lv_indev_t* _wheelIndev;
    private lv_font_t* _fallbackFont;
    private lv_style_t* _defaultFontStyle;
    private lv_obj_t* _root;
    private lv_group_t* _keyInputGroup;
    private SixLaborsFontManager? _fontManager;
    private string? _resolvedSystemFontPath;
    private string? _fontDiagnosticSummary;
    private bool _initialized;
    private bool _running;
    private bool _disposed;

    public WaylandView(
        string title = "LVGLSharp Wayland",
        int width = 800,
        int height = 600,
        float dpi = 96f,
        string? waylandDisplayName = null,
        string? x11DisplayName = null,
        bool borderless = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);

        _diagnosticSummary = LinuxEnvironmentDetector.GetWaylandDiagnosticSummary(waylandDisplayName, x11DisplayName);
        _connection = new WaylandDisplayConnection(waylandDisplayName, _diagnosticSummary);
        _window = new WaylandWindow(title, width, height, borderless);
        _inputSource = new WaylandInputSource();
        _bufferPresenter = new WaylandBufferPresenter(width, height, dpi);

        if (!string.IsNullOrWhiteSpace(x11DisplayName))
        {
            _fallbackView = new X11View(
                LinuxEnvironmentDetector.FormatWaylandWindowTitle(title, waylandDisplayName, x11DisplayName),
                width,
                height,
                dpi,
                x11DisplayName,
                borderless);
        }
    }

    public static (int X, int Y) CurrentMousePosition => s_activeView?._inputSource.CurrentMousePosition ?? (0, 0);

    public static uint CurrentMouseButton => s_activeView?._inputSource.CurrentMouseButton ?? 0U;

    public override lv_obj_t* Root => _fallbackView is not null ? _fallbackView.Root : _root;

    public override lv_group_t* KeyInputGroup => _fallbackView is not null ? _fallbackView.KeyInputGroup : _keyInputGroup;

    public override delegate* unmanaged[Cdecl]<lv_event_t*, void> SendTextAreaFocusCallback => _fallbackView is not null ? _fallbackView.SendTextAreaFocusCallback : null;

    protected override void OnOpenCore()
    {
        _connection.Connect();

        if (_fallbackView is not null)
        {
            _fallbackView.Open();
            _running = true;
            _initialized = true;
            return;
        }

        InitializeLvgl();
        _window.InitializeSurface(_connection);
        _bufferPresenter.InitializeSharedMemory(_connection);
        _inputSource.Initialize(_connection);
        _connection.PumpEvents();
        s_activeView = this;
        _running = true;
        _initialized = true;
    }

    public override void HandleEvents()
    {
        if (_fallbackView is not null)
        {
            _fallbackView.HandleEvents();
            return;
        }

        if (!_initialized)
        {
            return;
        }

        _connection.PumpEvents();
        HandlePendingResize();

        if (_root != null && _bufferPresenter.ConsumeRedrawAfterRelease())
        {
            lv_obj_invalidate(_root);
        }

        lv_timer_handler();
    }

    protected override void RunLoopCore(Action iteration)
    {
        if (_fallbackView is not null)
        {
            _fallbackView.RunLoop(iteration);
            return;
        }

        while (_running)
        {
            HandleEvents();

            if (_window.IsCloseRequested)
            {
                break;
            }

            iteration?.Invoke();
            Thread.Sleep(5);
        }
    }

    protected override void OnCloseCore()
    {
        if (s_activeView == this)
        {
            s_activeView = null;
        }

        _running = false;

        if (_fallbackView is not null)
        {
            _fallbackView.Close();
            _initialized = false;
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

        _fontManager?.Dispose();
        _fontManager = null;
        _resolvedSystemFontPath = null;
        _fontDiagnosticSummary = null;

        _root = null;
        _bufferPresenter.Dispose();
        _inputSource.Dispose();
        _window.Dispose();
        _connection.Dispose();
        _initialized = false;
    }

    protected override bool CanSkipClose() =>
        _fallbackView is null &&
        !_initialized &&
        _lvDisplay == null &&
        _mouseIndev == null &&
        _keyboardIndev == null &&
        _wheelIndev == null &&
        _root == null;

    public override void RegisterTextInput(lv_obj_t* textArea)
    {
        _fallbackView?.RegisterTextInput(textArea);
    }


    public override string ToString() => _fallbackView is null
        ? $"{_diagnosticSummary}, Mode=WaylandSkeleton, Connected={_connection.IsConnected}, Compositor={_connection.HasCompositor}, Shm={_connection.HasSharedMemory}, XdgWmBase={_connection.HasXdgWmBase}, NativeSurface={_window.IsNativeSurfaceInitialized}, XdgSurface={_window.IsXdgSurfaceInitialized}, XdgToplevel={_window.IsXdgToplevelInitialized}, Configure={_window.HasReceivedConfigure}:{_window.LastConfigureSerial}, Ack={_window.HasAcknowledgedConfigure}, Pong={_window.HasRespondedToPing}:{_window.LastPingSerial}, ToplevelSize={_window.LastConfiguredWidth}x{_window.LastConfiguredHeight}, Close={_window.IsCloseRequested}, ShmBuffer={_bufferPresenter.HasSharedMemoryBuffer}:{_bufferPresenter.IsBufferReleased}:{_bufferPresenter.BufferReleaseCount}, Seat={_connection.HasSeat}:{_connection.SeatVersion}, FontPath={_resolvedSystemFontPath ?? "<none>"}, FontDiag={_fontDiagnosticSummary ?? "<unresolved>"}"
        : $"{_diagnosticSummary}, Mode=X11Fallback, Connected={_connection.IsConnected}, Compositor={_connection.HasCompositor}, Shm={_connection.HasSharedMemory}, XdgWmBase={_connection.HasXdgWmBase}, NativeSurface={_window.IsNativeSurfaceInitialized}, XdgSurface={_window.IsXdgSurfaceInitialized}, XdgToplevel={_window.IsXdgToplevelInitialized}, Configure={_window.HasReceivedConfigure}:{_window.LastConfigureSerial}, Ack={_window.HasAcknowledgedConfigure}, Pong={_window.HasRespondedToPing}:{_window.LastPingSerial}, ToplevelSize={_window.LastConfiguredWidth}x{_window.LastConfiguredHeight}, Close={_window.IsCloseRequested}, ShmBuffer={_bufferPresenter.HasSharedMemoryBuffer}, Seat={_connection.HasSeat}:{_connection.SeatVersion}";

    private InvalidOperationException CreateNativeHostNotImplementedException()
    {
        return new InvalidOperationException(
            $"Wayland native host is not implemented yet. {_connection.DiagnosticSummary}, Connected={_connection.IsConnected}, ConnectedDisplay={_connection.ConnectedDisplayName ?? "<default>"}, Compositor={_connection.HasCompositor}:{_connection.CompositorVersion}, Shm={_connection.HasSharedMemory}:{_connection.SharedMemoryVersion}, XdgWmBase={_connection.HasXdgWmBase}:{_connection.XdgWmBaseVersion}, Seat={_connection.HasSeat}:{_connection.SeatVersion}, Window={_window.Title}({_window.Width}x{_window.Height}), SurfaceReady={_window.HasSurfacePrerequisites}, NativeSurface={_window.IsNativeSurfaceInitialized}, XdgReady={_window.HasXdgShellPrerequisites}, XdgSurface={_window.IsXdgSurfaceInitialized}, XdgToplevel={_window.IsXdgToplevelInitialized}, Configure={_window.HasReceivedConfigure}:{_window.LastConfigureSerial}, Ack={_window.HasAcknowledgedConfigure}, Pong={_window.HasRespondedToPing}:{_window.LastPingSerial}, ToplevelSize={_window.LastConfiguredWidth}x{_window.LastConfiguredHeight}, Close={_window.IsCloseRequested}, WindowInit={_window.InitializationSummary ?? "<pending>"}, Pointer={_inputSource.SupportsPointer}:{_inputSource.CurrentMouseButton}@{_inputSource.CurrentMousePosition.X},{_inputSource.CurrentMousePosition.Y}:{_inputSource.HasPointerFocus}, Keyboard={_inputSource.SupportsKeyboard}:{_inputSource.CurrentKey}:{_inputSource.IsKeyPressed}:{_inputSource.HasKeyboardLayout}:{_inputSource.HasKeyboardFocus}, Repeat={_inputSource.RepeatRate}:{_inputSource.RepeatDelay}, TextInput={_inputSource.SupportsTextInput}, Surface={_bufferPresenter.PixelWidth}x{_bufferPresenter.PixelHeight}@{_bufferPresenter.Dpi:0.##}dpi, LvDisplay={_lvDisplay != null}, Root={_root != null}, KeyGroup={_keyInputGroup != null}, FontPath={_resolvedSystemFontPath ?? "<none>"}, FontDiag={_fontDiagnosticSummary ?? "<unresolved>"}, ShmBuffer={_bufferPresenter.HasSharedMemoryBuffer}:{_bufferPresenter.IsBufferReleased}:{_bufferPresenter.BufferReleaseCount}, Flushes={_bufferPresenter.FlushCount}:{_bufferPresenter.LastFlushWidth}x{_bufferPresenter.LastFlushHeight}, SkippedFlushes={_bufferPresenter.SkippedFlushCount}");
    }

    private void HandlePendingResize()
    {
        if (_lvDisplay == null || !_window.HasPendingResize || !_bufferPresenter.IsBufferReleased)
        {
            return;
        }

        if (!_window.TryConsumePendingResize(out var width, out var height))
        {
            return;
        }

        if (!_bufferPresenter.ResizeIfNeeded(_connection, width, height))
        {
            return;
        }

        lv_display_set_resolution(_lvDisplay, width, height);
        lv_display_set_buffers(_lvDisplay, _bufferPresenter.DrawBuffer, null, _bufferPresenter.DrawBufferByteSize, LV_DISPLAY_RENDER_MODE_FULL);

        if (_root != null)
        {
            lv_obj_invalidate(_root);
        }
    }

    private void InitializeLvgl()
    {
        LvglNativeLibraryResolver.EnsureRegistered();
        s_startTick = Environment.TickCount;

        if (!lv_is_initialized())
        {
            lv_init();
        }

        lv_tick_set_cb(&TickCb);

        _bufferPresenter.Initialize();
        _lvDisplay = lv_display_create(_bufferPresenter.PixelWidth, _bufferPresenter.PixelHeight);
        if (_lvDisplay == null)
        {
            throw new InvalidOperationException("LVGL display 创建失败。" );
        }

        lv_display_set_buffers(_lvDisplay, _bufferPresenter.DrawBuffer, null, _bufferPresenter.DrawBufferByteSize, LV_DISPLAY_RENDER_MODE_FULL);
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

        _root = lv_scr_act();
        _keyInputGroup = lv_group_create();
        if (_keyboardIndev != null && _keyInputGroup != null)
        {
            lv_indev_set_group(_keyboardIndev, _keyInputGroup);
        }

        _fallbackFont = lv_obj_get_style_text_font(_root, lv_part_t.LV_PART_MAIN);
        _fontDiagnosticSummary = LinuxSystemFontResolver.GetFontPathDiagnosticSummary();

        _resolvedSystemFontPath = LinuxSystemFontResolver.TryResolveFontPath();
        if (!string.IsNullOrWhiteSpace(_resolvedSystemFontPath))
        {
            _fontManager = new SixLaborsFontManager(
                _resolvedSystemFontPath,
                12,
                _bufferPresenter.Dpi,
                _fallbackFont,
                LvglHostDefaults.CreateDefaultFontFallbackGlyphs());

            _defaultFontStyle = LvglHostDefaults.ApplyDefaultFontStyle(_root, _fontManager.GetLvFontPtr());
        }
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static uint TickCb()
    {
        return (uint)(Environment.TickCount - s_startTick);
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static void FlushCb(lv_display_t* display, lv_area_t* area, byte* pxMap)
    {
        var view = s_activeView;
        if (view is null)
        {
            lv_display_flush_ready(display);
            return;
        }

        view._bufferPresenter.Flush(display, area, view._window.SurfaceProxy);
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
            data->btn_id = 0;
            return;
        }

        var position = view._inputSource.CurrentMousePosition;
        data->point.x = position.X;
        data->point.y = position.Y;
        data->state = view._inputSource.CurrentMouseButton != 0 ? LV_INDEV_STATE_PR : LV_INDEV_STATE_REL;
        data->btn_id = view._inputSource.CurrentMouseButton;
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

        view._inputSource.ReadKeyboardState(out var key, out var pressed);
        data->key = key;
        data->state = pressed ? LV_INDEV_STATE_PR : LV_INDEV_STATE_REL;
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

        data->enc_diff = (short)view._inputSource.ConsumeEncoderDiff();
        data->state = LV_INDEV_STATE_REL;
    }

}
