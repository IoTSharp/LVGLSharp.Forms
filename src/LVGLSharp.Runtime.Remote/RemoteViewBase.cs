using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using LVGLSharp.Interop;
using static LVGLSharp.Interop.LVGL;

namespace LVGLSharp.Runtime.Remote;

public abstract unsafe class RemoteViewBase : ViewLifetimeBase, IRemoteInputSink
{
    private readonly IRemoteTransport _transport;
    private readonly RemoteSessionOptions _options;
    private readonly int _width;
    private readonly int _height;
    private readonly bool _publishFrames;
    private readonly object _inputSync = new();
    private readonly Queue<KeyTransition> _pendingKeyTransitions = new();
    private readonly Queue<string> _pendingTextInputs = new();

    private bool _running;
    private bool _initialized;
    private bool _hasPendingFrame = true;
    private bool _hasPublishedFrame;
    private long _lastTick;
    private long _lastFramePublishTick;
    private lv_display_t* _lvDisplay;
    private lv_obj_t* _root;
    private lv_group_t* _keyInputGroup;
    private lv_indev_t* _keyboardIndev;
    private lv_indev_t* _pointerIndev;
    private byte* _drawBuffer;
    private uint _drawBufferByteSize;
    private uint* _frameBuffer;
    private GCHandle _selfHandle;
    private lv_obj_t* _focusedTextArea;
    private int _pointerX;
    private int _pointerY;
    private uint _pointerButton;
    private lv_indev_state_t _pointerState = lv_indev_state_t.LV_INDEV_STATE_RELEASED;
    private uint _currentKey;
    private lv_indev_state_t _currentKeyState = lv_indev_state_t.LV_INDEV_STATE_RELEASED;

    protected RemoteViewBase(IRemoteTransport transport, RemoteSessionOptions options, int width, int height, bool publishFrames = true)
    {
        _transport = transport ?? throw new ArgumentNullException(nameof(transport));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _width = width > 0 ? width : throw new ArgumentOutOfRangeException(nameof(width));
        _height = height > 0 ? height : throw new ArgumentOutOfRangeException(nameof(height));
        _publishFrames = publishFrames;
    }

    public IRemoteTransport Transport => _transport;

    public RemoteSessionOptions Options => _options;

    public sealed override lv_obj_t* Root => _root;

    public sealed override lv_group_t* KeyInputGroup => _keyInputGroup;

    public sealed override delegate* unmanaged[Cdecl]<lv_event_t*, void> SendTextAreaFocusCallback => &HandleTextAreaFocused;

    protected override void OnOpenCore()
    {
        LvglNativeLibraryResolver.EnsureRegistered();

        if (!lv_is_initialized())
        {
            lv_init();
        }

        AllocateBuffers();

        _lvDisplay = lv_display_create(_width, _height);
        if (_lvDisplay == null)
        {
            throw new InvalidOperationException("Remote LVGL display 创建失败。");
        }

        if (!_selfHandle.IsAllocated)
        {
            _selfHandle = GCHandle.Alloc(this);
        }

        var selfPtr = (void*)GCHandle.ToIntPtr(_selfHandle);
        lv_display_set_user_data(_lvDisplay, selfPtr);
        lv_display_set_buffers(_lvDisplay, _drawBuffer, null, _drawBufferByteSize, lv_display_render_mode_t.LV_DISPLAY_RENDER_MODE_FULL);
        lv_display_set_flush_cb(_lvDisplay, &FlushCb);

        _pointerIndev = lv_indev_create();
        lv_indev_set_type(_pointerIndev, lv_indev_type_t.LV_INDEV_TYPE_POINTER);
        lv_indev_set_display(_pointerIndev, _lvDisplay);
        lv_indev_set_user_data(_pointerIndev, selfPtr);
        lv_indev_set_read_cb(_pointerIndev, &PointerReadCb);

        _keyboardIndev = lv_indev_create();
        lv_indev_set_type(_keyboardIndev, lv_indev_type_t.LV_INDEV_TYPE_KEYPAD);
        lv_indev_set_display(_keyboardIndev, _lvDisplay);
        lv_indev_set_user_data(_keyboardIndev, selfPtr);
        lv_indev_set_read_cb(_keyboardIndev, &KeyboardReadCb);

        _root = lv_scr_act();
        lv_obj_set_style_bg_opa(_root, 255, 0);
        lv_obj_set_style_bg_color(_root, lv_color_make(255, 255, 255), 0);
        lv_obj_remove_flag(_root, lv_obj_flag_t.LV_OBJ_FLAG_SCROLLABLE | lv_obj_flag_t.LV_OBJ_FLAG_SCROLL_ELASTIC | lv_obj_flag_t.LV_OBJ_FLAG_SCROLL_MOMENTUM | lv_obj_flag_t.LV_OBJ_FLAG_SCROLL_CHAIN);
        lv_obj_set_scrollbar_mode(_root, lv_scrollbar_mode_t.LV_SCROLLBAR_MODE_OFF);

        _keyInputGroup = lv_group_create();
        lv_indev_set_group(_keyboardIndev, _keyInputGroup);

        if (_transport is IRemoteHostedTransport hostedTransport)
        {
            hostedTransport.AttachInputSink(this);
            hostedTransport.Start();
        }

        _lastTick = Environment.TickCount64;
        _lastFramePublishTick = 0;
        _running = true;
        _initialized = true;
    }

    public sealed override void HandleEvents()
    {
        if (!_initialized)
        {
            return;
        }

        PumpTicks();
        CommitPendingTextInput();
        lv_timer_handler();
        PublishFrameIfNeeded();
    }

    protected override void RunLoopCore(Action iteration)
    {
        var frameDelay = Math.Max(1, 1000 / Math.Max(1, _options.TargetFrameRate));
        while (_running)
        {
            HandleEvents();
            iteration?.Invoke();
            Thread.Sleep(frameDelay);
        }
    }

    protected override void OnCloseCore()
    {
        _running = false;

        if (_transport is IRemoteHostedTransport hostedTransport)
        {
            hostedTransport.Stop();
        }

        if (_keyboardIndev != null)
        {
            lv_indev_delete(_keyboardIndev);
            _keyboardIndev = null;
        }

        if (_pointerIndev != null)
        {
            lv_indev_delete(_pointerIndev);
            _pointerIndev = null;
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

        if (_drawBuffer != null)
        {
            NativeMemory.Free(_drawBuffer);
            _drawBuffer = null;
            _drawBufferByteSize = 0;
        }

        if (_frameBuffer != null)
        {
            NativeMemory.Free(_frameBuffer);
            _frameBuffer = null;
        }

        if (_selfHandle.IsAllocated)
        {
            _selfHandle.Free();
        }

        lock (_inputSync)
        {
            _pendingKeyTransitions.Clear();
            _pendingTextInputs.Clear();
            _currentKey = 0;
            _currentKeyState = lv_indev_state_t.LV_INDEV_STATE_RELEASED;
            _pointerX = 0;
            _pointerY = 0;
            _pointerButton = 0;
            _pointerState = lv_indev_state_t.LV_INDEV_STATE_RELEASED;
        }

        _focusedTextArea = null;
        _root = null;
        _initialized = false;
        _hasPendingFrame = false;
        _hasPublishedFrame = false;
        _lastTick = 0;
        _lastFramePublishTick = 0;
    }

    protected override bool CanSkipClose() =>
        !_initialized &&
        !_running &&
        _lvDisplay == null &&
        _keyboardIndev == null &&
        _pointerIndev == null &&
        _keyInputGroup == null;

    public sealed override void RegisterTextInput(lv_obj_t* textArea)
    {
        if (textArea == null || _keyInputGroup == null)
        {
            return;
        }

        lv_group_add_obj(_keyInputGroup, textArea);
    }

    void IRemoteInputSink.PostInput(RemoteInputEvent inputEvent)
    {
        ArgumentNullException.ThrowIfNull(inputEvent);

        lock (_inputSync)
        {
            switch (inputEvent.Kind)
            {
                case RemoteInputEventKind.PointerMove:
                    _pointerX = inputEvent.X;
                    _pointerY = inputEvent.Y;
                    _pointerButton = GetPrimaryButton(inputEvent.Buttons);
                    break;
                case RemoteInputEventKind.PointerDown:
                    _pointerX = inputEvent.X;
                    _pointerY = inputEvent.Y;
                    _pointerButton = GetPrimaryButton(inputEvent.Buttons);
                    _pointerState = lv_indev_state_t.LV_INDEV_STATE_PRESSED;
                    break;
                case RemoteInputEventKind.PointerUp:
                    _pointerX = inputEvent.X;
                    _pointerY = inputEvent.Y;
                    _pointerButton = GetPrimaryButton(inputEvent.Buttons);
                    _pointerState = lv_indev_state_t.LV_INDEV_STATE_RELEASED;
                    break;
                case RemoteInputEventKind.KeyDown:
                    if (inputEvent.KeyCode != 0)
                    {
                        _pendingKeyTransitions.Enqueue(new KeyTransition(inputEvent.KeyCode, lv_indev_state_t.LV_INDEV_STATE_PRESSED));
                    }
                    break;
                case RemoteInputEventKind.KeyUp:
                    if (inputEvent.KeyCode != 0)
                    {
                        _pendingKeyTransitions.Enqueue(new KeyTransition(inputEvent.KeyCode, lv_indev_state_t.LV_INDEV_STATE_RELEASED));
                    }
                    break;
                case RemoteInputEventKind.TextInput:
                    if (!string.IsNullOrEmpty(inputEvent.Text))
                    {
                        _pendingTextInputs.Enqueue(inputEvent.Text);
                    }
                    break;
            }
        }
    }

    private void AllocateBuffers()
    {
        _drawBufferByteSize = checked((uint)(_width * _height * sizeof(ushort)));
        _drawBuffer = (byte*)NativeMemory.AllocZeroed((nuint)_drawBufferByteSize);
        if (_drawBuffer == null)
        {
            throw new OutOfMemoryException("Remote draw buffer 分配失败。");
        }

        _frameBuffer = (uint*)NativeMemory.AllocZeroed((nuint)(_width * _height), (nuint)sizeof(uint));
        if (_frameBuffer == null)
        {
            throw new OutOfMemoryException("Remote frame buffer 分配失败。");
        }
    }

    private void PumpTicks()
    {
        var now = Environment.TickCount64;
        if (_lastTick == 0)
        {
            _lastTick = now;
            return;
        }

        var diff = now - _lastTick;
        if (diff > 0)
        {
            lv_tick_inc((uint)diff);
            _lastTick = now;
        }
    }

    private void CommitPendingTextInput()
    {
        List<string>? pendingTexts = null;

        lock (_inputSync)
        {
            if (_pendingTextInputs.Count == 0)
            {
                return;
            }

            pendingTexts = new List<string>(_pendingTextInputs.Count);
            while (_pendingTextInputs.Count > 0)
            {
                pendingTexts.Add(_pendingTextInputs.Dequeue());
            }
        }

        if (_focusedTextArea == null)
        {
            return;
        }

        foreach (var pendingText in pendingTexts)
        {
            var utf8Bytes = Encoding.UTF8.GetBytes(pendingText + "\0");
            fixed (byte* utf8Ptr = utf8Bytes)
            {
                lv_textarea_add_text(_focusedTextArea, utf8Ptr);
            }
        }
    }

    private void PublishFrameIfNeeded()
    {
        if (!_publishFrames || _frameBuffer == null)
        {
            return;
        }

        var now = Environment.TickCount64;
        var frameInterval = Math.Max(1, 1000 / Math.Max(1, _options.TargetFrameRate));
        if (_hasPublishedFrame && !_hasPendingFrame)
        {
            return;
        }

        if (_hasPublishedFrame && now - _lastFramePublishTick < frameInterval)
        {
            return;
        }

        var bytes = new byte[_width * _height * 4];
        for (var i = 0; i < _width * _height; i++)
        {
            var argb = _frameBuffer[i];
            var offset = i * 4;
            bytes[offset] = (byte)((argb >> 24) & 0xFF);
            bytes[offset + 1] = (byte)((argb >> 16) & 0xFF);
            bytes[offset + 2] = (byte)((argb >> 8) & 0xFF);
            bytes[offset + 3] = (byte)(argb & 0xFF);
        }

        _transport.SendFrameAsync(new RemoteFrame(_width, _height, bytes)).GetAwaiter().GetResult();
        _hasPendingFrame = false;
        _hasPublishedFrame = true;
        _lastFramePublishTick = now;
    }

    private static uint GetPrimaryButton(uint buttons)
    {
        if ((buttons & 1u) != 0)
        {
            return 1;
        }

        if ((buttons & 2u) != 0)
        {
            return 2;
        }

        if ((buttons & 4u) != 0)
        {
            return 3;
        }

        return 0;
    }

    private static RemoteViewBase? GetViewFromDisplay(lv_display_t* display)
    {
        if (display == null)
        {
            return null;
        }

        var userData = lv_display_get_user_data(display);
        return userData == null ? null : (RemoteViewBase?)GCHandle.FromIntPtr((IntPtr)userData).Target;
    }

    private static RemoteViewBase? GetViewFromIndev(lv_indev_t* indev)
    {
        if (indev == null)
        {
            return null;
        }

        var userData = lv_indev_get_user_data(indev);
        return userData == null ? null : (RemoteViewBase?)GCHandle.FromIntPtr((IntPtr)userData).Target;
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static void HandleTextAreaFocused(lv_event_t* e)
    {
        var target = lv_event_get_target_obj(e);
        if (target == null)
        {
            return;
        }

        var display = lv_obj_get_display(target);
        var view = GetViewFromDisplay(display);
        if (view == null)
        {
            return;
        }

        view._focusedTextArea = target;
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static void FlushCb(lv_display_t* display, lv_area_t* area, byte* pxMap)
    {
        var view = GetViewFromDisplay(display);
        if (view == null || view._frameBuffer == null)
        {
            lv_display_flush_ready(display);
            return;
        }

        var width = area->x2 - area->x1 + 1;
        var height = area->y2 - area->y1 + 1;
        var source = (ushort*)pxMap;

        for (var y = 0; y < height; y++)
        {
            var dstRow = (area->y1 + y) * view._width + area->x1;
            for (var x = 0; x < width; x++)
            {
                var rgb565 = source[(y * width) + x];
                view._frameBuffer[dstRow + x] = ConvertRgb565ToArgb8888(rgb565);
            }
        }

        view._hasPendingFrame = true;
        lv_display_flush_ready(display);
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static void PointerReadCb(lv_indev_t* indev, lv_indev_data_t* data)
    {
        var view = GetViewFromIndev(indev);
        if (view == null)
        {
            data->point.x = 0;
            data->point.y = 0;
            data->state = lv_indev_state_t.LV_INDEV_STATE_RELEASED;
            data->btn_id = 0;
            return;
        }

        lock (view._inputSync)
        {
            data->point.x = view._pointerX;
            data->point.y = view._pointerY;
            data->state = view._pointerState;
            data->btn_id = view._pointerButton;
        }
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static void KeyboardReadCb(lv_indev_t* indev, lv_indev_data_t* data)
    {
        var view = GetViewFromIndev(indev);
        if (view == null)
        {
            data->key = 0;
            data->state = lv_indev_state_t.LV_INDEV_STATE_RELEASED;
            return;
        }

        lock (view._inputSync)
        {
            if (view._pendingKeyTransitions.Count > 0)
            {
                var transition = view._pendingKeyTransitions.Dequeue();
                view._currentKey = transition.KeyCode;
                view._currentKeyState = transition.State;
            }

            data->key = view._currentKey;
            data->state = view._currentKeyState;

            if (view._currentKeyState == lv_indev_state_t.LV_INDEV_STATE_RELEASED)
            {
                view._currentKey = 0;
            }
        }
    }

    private static uint ConvertRgb565ToArgb8888(ushort rgb565)
    {
        var r = (byte)(((rgb565 >> 11) & 0x1F) * 255 / 31);
        var g = (byte)(((rgb565 >> 5) & 0x3F) * 255 / 63);
        var b = (byte)((rgb565 & 0x1F) * 255 / 31);
        return 0xFF000000u | ((uint)r << 16) | ((uint)g << 8) | b;
    }

    private readonly record struct KeyTransition(uint KeyCode, lv_indev_state_t State);
}
