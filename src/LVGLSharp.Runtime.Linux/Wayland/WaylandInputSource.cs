using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace LVGLSharp.Runtime.Linux;

internal unsafe sealed partial class WaylandInputSource : IDisposable
{
    private const string XkbCommonLib = "libxkbcommon.so.0";
    private const uint PointerAxisVerticalScroll = 0u;
    private const uint PointerAxisHorizontalScroll = 1u;
    private const int PointerAxisStep = 256;
    private const uint SeatCapabilityPointer = 1u;
    private const uint SeatCapabilityKeyboard = 2u;
    private const uint PointerButtonStatePressed = 1u;
    private const uint BtnLeft = 272u;
    private const uint BtnRight = 273u;
    private const uint BtnMiddle = 274u;
    private const int KeyboardPressedState = 1;
    private const int KeyboardReleasedState = 0;
    private const uint XkbKeymapFormatTextV1 = 1u;

    private IntPtr _seat;
    private IntPtr _pointer;
    private IntPtr _keyboard;
    private GCHandle _listenerStateHandle;
    private IntPtr _xkbContext;
    private IntPtr _xkbKeymap;
    private IntPtr _xkbState;

    private uint _lastKey;
    private bool _keyPressed;
    private int _wheelDiff;
    private int _wheelVerticalRemainder;
    private int _wheelHorizontalRemainder;
    private uint _lastTextKey;
    private long _repeatRate;
    private long _repeatDelay;
    private long _nextRepeatTick;
    private uint _repeatKey;
    private bool _hasPointerFocus;
    private bool _hasKeyboardFocus;

    [LibraryImport("libc", EntryPoint = "close")]
    private static partial int Close(int fd);

    [LibraryImport("libc", EntryPoint = "mmap")]
    private static partial IntPtr Mmap(IntPtr addr, nuint length, uint prot, uint flags, int fd, nint offset);

    [LibraryImport("libc", EntryPoint = "munmap")]
    private static partial int Munmap(IntPtr addr, nuint length);

    [LibraryImport(XkbCommonLib, EntryPoint = "xkb_context_new")]
    private static partial IntPtr XkbContextNew(uint flags);

    [LibraryImport(XkbCommonLib, EntryPoint = "xkb_context_unref")]
    private static partial void XkbContextUnref(IntPtr context);

    [LibraryImport(XkbCommonLib, EntryPoint = "xkb_keymap_new_from_string", StringMarshalling = StringMarshalling.Utf8)]
    private static partial IntPtr XkbKeymapNewFromString(IntPtr context, string keymapString, uint format, uint flags);

    [LibraryImport(XkbCommonLib, EntryPoint = "xkb_keymap_unref")]
    private static partial void XkbKeymapUnref(IntPtr keymap);

    [LibraryImport(XkbCommonLib, EntryPoint = "xkb_state_new")]
    private static partial IntPtr XkbStateNew(IntPtr keymap);

    [LibraryImport(XkbCommonLib, EntryPoint = "xkb_state_unref")]
    private static partial void XkbStateUnref(IntPtr state);

    [LibraryImport(XkbCommonLib, EntryPoint = "xkb_state_update_mask")]
    private static partial uint XkbStateUpdateMask(IntPtr state, uint depressedMods, uint latchedMods, uint lockedMods, uint depressedLayout, uint latchedLayout, uint lockedLayout);

    [LibraryImport(XkbCommonLib, EntryPoint = "xkb_state_key_get_utf32")]
    private static partial uint XkbStateKeyGetUtf32(IntPtr state, uint key);

    [LibraryImport(XkbCommonLib, EntryPoint = "xkb_state_key_get_one_sym")]
    private static partial uint XkbStateKeyGetOneSym(IntPtr state, uint key);

    private static readonly WaylandNative.WlSeatListener s_seatListener = new(&HandleSeatCapabilities, &HandleSeatName);
    private static readonly WaylandNative.WlPointerListener s_pointerListener = new(&HandlePointerEnter, &HandlePointerLeave, &HandlePointerMotion, &HandlePointerButton, &HandlePointerAxis);
    private static readonly WaylandNative.WlKeyboardListener s_keyboardListener = new(&HandleKeyboardKeymap, &HandleKeyboardEnter, &HandleKeyboardLeave, &HandleKeyboardKey, &HandleKeyboardModifiers, &HandleKeyboardRepeatInfo);

    public bool SupportsPointer => true;

    public bool SupportsKeyboard => true;

    public bool SupportsTextInput => true;

    public (int X, int Y) CurrentMousePosition { get; private set; }

    public uint CurrentMouseButton { get; private set; }

    public uint CurrentKey => _lastKey;

    public bool IsKeyPressed => _keyPressed;

    public bool HasKeyboardLayout => _xkbState != IntPtr.Zero;

    public bool HasPointerFocus => _hasPointerFocus;

    public bool HasKeyboardFocus => _hasKeyboardFocus;

    public long RepeatRate => _repeatRate;

    public long RepeatDelay => _repeatDelay;

    public bool IsDisposed { get; private set; }

    public int ConsumeEncoderDiff()
    {
        ThrowIfDisposed();

        var diff = _wheelDiff;
        _wheelDiff = 0;
        return diff;
    }

    public void Initialize(WaylandDisplayConnection connection)
    {
        ArgumentNullException.ThrowIfNull(connection);

        ThrowIfDisposed();
        connection.ThrowIfDisposed();

        if (_seat != IntPtr.Zero)
        {
            return;
        }

        _seat = connection.BindSeat();
        _listenerStateHandle = GCHandle.Alloc(this);
        var listenerState = GCHandle.ToIntPtr(_listenerStateHandle);

        fixed (WaylandNative.WlSeatListener* seatListener = &s_seatListener)
        {
            var result = WaylandNative.AddSeatListener(_seat, seatListener, listenerState);
            if (result != 0)
            {
                throw new InvalidOperationException("Unable to attach Wayland wl_seat listener.");
            }
        }

        connection.Roundtrip();
        connection.DispatchPending();
    }

    public void UpdateMouseState(int x, int y, uint button)
    {
        ThrowIfDisposed();

        CurrentMousePosition = (x, y);
        CurrentMouseButton = button;
    }

    public void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);
    }

    public void Dispose()
    {
        if (_listenerStateHandle.IsAllocated)
        {
            _listenerStateHandle.Free();
        }

        WaylandNative.DestroyProxy(_keyboard);
        WaylandNative.DestroyProxy(_pointer);
        WaylandNative.DestroyProxy(_seat);
        _keyboard = IntPtr.Zero;
        _pointer = IntPtr.Zero;
        _seat = IntPtr.Zero;
        CurrentMousePosition = (0, 0);
        CurrentMouseButton = 0;
        _lastKey = 0;
        _keyPressed = false;
        _repeatKey = 0;
        _nextRepeatTick = 0;
        _repeatRate = 0;
        _repeatDelay = 0;
        _lastTextKey = 0;
        _wheelDiff = 0;
        _wheelVerticalRemainder = 0;
        _wheelHorizontalRemainder = 0;
        _hasPointerFocus = false;
        _hasKeyboardFocus = false;
        ReleaseKeyboardLayout();
        IsDisposed = true;
    }

    private void SetPointerCapabilities(uint capabilities)
    {
        if ((capabilities & SeatCapabilityPointer) != 0)
        {
            if (_pointer == IntPtr.Zero)
            {
                _pointer = WaylandNative.GetSeatPointer(_seat);
                fixed (WaylandNative.WlPointerListener* pointerListener = &s_pointerListener)
                {
                    var result = WaylandNative.AddPointerListener(_pointer, pointerListener, GCHandle.ToIntPtr(_listenerStateHandle));
                    if (result != 0)
                    {
                        throw new InvalidOperationException("Unable to attach Wayland wl_pointer listener.");
                    }
                }
            }
        }
        else if (_pointer != IntPtr.Zero)
        {
            WaylandNative.DestroyProxy(_pointer);
            _pointer = IntPtr.Zero;
            CurrentMouseButton = 0;
            _hasPointerFocus = false;
        }
    }

    private void SetKeyboardCapabilities(uint capabilities)
    {
        if ((capabilities & SeatCapabilityKeyboard) != 0)
        {
            if (_keyboard == IntPtr.Zero)
            {
                _keyboard = WaylandNative.GetSeatKeyboard(_seat);
                fixed (WaylandNative.WlKeyboardListener* keyboardListener = &s_keyboardListener)
                {
                    var result = WaylandNative.AddKeyboardListener(_keyboard, keyboardListener, GCHandle.ToIntPtr(_listenerStateHandle));
                    if (result != 0)
                    {
                        throw new InvalidOperationException("Unable to attach Wayland wl_keyboard listener.");
                    }
                }
            }
        }
        else if (_keyboard != IntPtr.Zero)
        {
            WaylandNative.DestroyProxy(_keyboard);
            _keyboard = IntPtr.Zero;
            _lastKey = 0;
            _keyPressed = false;
            _repeatKey = 0;
            _nextRepeatTick = 0;
            _lastTextKey = 0;
            _hasKeyboardFocus = false;
            ReleaseKeyboardLayout();
        }
    }

    private void UpdatePointerPositionFromFixed(int fixedX, int fixedY)
    {
        CurrentMousePosition = (fixedX / 256, fixedY / 256);
    }

    private void UpdatePointerButton(uint button, uint state)
    {
        CurrentMouseButton = state == PointerButtonStatePressed ? MapPointerButton(button) : 0u;
    }

    private void UpdatePointerAxis(uint axis, int value)
    {
        if (value == 0)
        {
            return;
        }

        switch (axis)
        {
            case PointerAxisVerticalScroll:
                AccumulateAxis(ref _wheelVerticalRemainder, value);
                break;
            case PointerAxisHorizontalScroll:
                AccumulateAxis(ref _wheelHorizontalRemainder, value);
                break;
        }
    }

    private void UpdateKeyboardState(uint key, uint state)
    {
        var isPressed = state == KeyboardPressedState;
        var lvKey = TranslateWaylandKeyToLvKey(key);

        if (!isPressed)
        {
            if (_repeatKey == lvKey)
            {
                _repeatKey = 0;
                _nextRepeatTick = 0;
            }

            _lastKey = lvKey;
            _keyPressed = false;
            return;
        }

        _lastKey = lvKey;
        _lastTextKey = _lastKey >= 32 && _lastKey <= 0x10FFFF ? _lastKey : 0;
        _keyPressed = _lastKey != 0;

        if (_keyPressed && _repeatRate > 0 && _repeatDelay >= 0)
        {
            _repeatKey = _lastKey;
            _nextRepeatTick = Environment.TickCount64 + _repeatDelay;
        }
    }

    public void ReadKeyboardState(out uint key, out bool pressed)
    {
        ThrowIfDisposed();

        key = _lastKey;
        pressed = _keyPressed;

        if (!_hasKeyboardFocus)
        {
            key = 0;
            pressed = false;
            return;
        }

        if (pressed)
        {
            return;
        }

        if (_repeatKey == 0 || _repeatRate <= 0 || Environment.TickCount64 < _nextRepeatTick)
        {
            return;
        }

        key = _repeatKey;
        pressed = true;
        var interval = Math.Max(1L, 1000L / _repeatRate);
        _nextRepeatTick = Environment.TickCount64 + interval;
    }

    private uint TranslateWaylandKeyToLvKey(uint key)
    {
        var xkbKey = key + 8;
        if (_xkbState != IntPtr.Zero)
        {
            var utf32 = XkbStateKeyGetUtf32(_xkbState, xkbKey);
            if (utf32 != 0)
            {
                return utf32;
            }

            var keysym = XkbStateKeyGetOneSym(_xkbState, xkbKey);
            var mappedControlKey = MapKeysymToLvKey(keysym);
            if (mappedControlKey != 0)
            {
                return mappedControlKey;
            }
        }

        return MapFallbackLinuxKeyCodeToLvKey(key);
    }

    private static uint MapPointerButton(uint button)
    {
        return button switch
        {
            BtnLeft => 1u,
            BtnRight => 2u,
            BtnMiddle => 4u,
            _ => 0u,
        };
    }

    private static uint MapFallbackLinuxKeyCodeToLvKey(uint keyCode)
    {
        return keyCode switch
        {
            28 => (uint)LV_KEY_ENTER,
            1 => (uint)LV_KEY_ESC,
            14 => (uint)LV_KEY_BACKSPACE,
            15 => (uint)LV_KEY_NEXT,
            102 => (uint)LV_KEY_HOME,
            107 => (uint)LV_KEY_END,
            111 => (uint)LV_KEY_DEL,
            105 => (uint)LV_KEY_LEFT,
            106 => (uint)LV_KEY_RIGHT,
            103 => (uint)LV_KEY_UP,
            108 => (uint)LV_KEY_DOWN,
            104 => (uint)LV_KEY_PREV,
            109 => (uint)LV_KEY_NEXT,
            57 => (uint)' ',
            _ => 0,
        };
    }

    private static uint MapKeysymToLvKey(uint keysym)
    {
        return keysym switch
        {
            0xFF0D or 0xFF8D => (uint)LV_KEY_ENTER,
            0xFF1B => (uint)LV_KEY_ESC,
            0xFF08 => (uint)LV_KEY_BACKSPACE,
            0xFF09 => (uint)LV_KEY_NEXT,
            0xFE20 => (uint)LV_KEY_PREV,
            0xFF50 => (uint)LV_KEY_HOME,
            0xFF57 => (uint)LV_KEY_END,
            0xFFFF => (uint)LV_KEY_DEL,
            0xFF51 => (uint)LV_KEY_LEFT,
            0xFF53 => (uint)LV_KEY_RIGHT,
            0xFF52 => (uint)LV_KEY_UP,
            0xFF54 => (uint)LV_KEY_DOWN,
            0xFF55 => (uint)LV_KEY_PREV,
            0xFF56 => (uint)LV_KEY_NEXT,
            _ => 0,
        };
    }

    private void UpdateKeyboardModifiers(uint depressed, uint latched, uint locked, uint group)
    {
        if (_xkbState == IntPtr.Zero)
        {
            return;
        }

        _ = XkbStateUpdateMask(_xkbState, depressed, latched, locked, 0u, 0u, group);
    }

    private void UpdateKeyboardRepeatInfo(int rate, int delay)
    {
        _repeatRate = Math.Max(0, rate);
        _repeatDelay = Math.Max(0, delay);

        if (_repeatRate <= 0)
        {
            _repeatKey = 0;
            _nextRepeatTick = 0;
        }
    }

    private void UpdateKeyboardKeymap(int fileDescriptor, uint size)
    {
        try
        {
            if (fileDescriptor < 0 || size == 0)
            {
                return;
            }

            var mapped = Mmap(IntPtr.Zero, size, 0x1u, 0x02u, fileDescriptor, 0);
            if (mapped == IntPtr.Zero || mapped == new IntPtr(-1))
            {
                return;
            }

            try
            {
                var keymapString = Marshal.PtrToStringUTF8(mapped, checked((int)size));
                if (string.IsNullOrWhiteSpace(keymapString))
                {
                    return;
                }

                var context = XkbContextNew(0u);
                if (context == IntPtr.Zero)
                {
                    return;
                }

                var keymap = XkbKeymapNewFromString(context, keymapString, XkbKeymapFormatTextV1, 0u);
                if (keymap == IntPtr.Zero)
                {
                    XkbContextUnref(context);
                    return;
                }

                var state = XkbStateNew(keymap);
                if (state == IntPtr.Zero)
                {
                    XkbKeymapUnref(keymap);
                    XkbContextUnref(context);
                    return;
                }

                ReleaseKeyboardLayout();
                _xkbContext = context;
                _xkbKeymap = keymap;
                _xkbState = state;
            }
            finally
            {
                _ = Munmap(mapped, size);
            }
        }
        finally
        {
            if (fileDescriptor >= 0)
            {
                _ = Close(fileDescriptor);
            }
        }
    }

    private void AccumulateAxis(ref int remainder, int value)
    {
        remainder += value;
        while (remainder >= PointerAxisStep)
        {
            _wheelDiff--;
            remainder -= PointerAxisStep;
        }

        while (remainder <= -PointerAxisStep)
        {
            _wheelDiff++;
            remainder += PointerAxisStep;
        }
    }

    private void ReleaseKeyboardLayout()
    {
        if (_xkbState != IntPtr.Zero)
        {
            XkbStateUnref(_xkbState);
            _xkbState = IntPtr.Zero;
        }

        if (_xkbKeymap != IntPtr.Zero)
        {
            XkbKeymapUnref(_xkbKeymap);
            _xkbKeymap = IntPtr.Zero;
        }

        if (_xkbContext != IntPtr.Zero)
        {
            XkbContextUnref(_xkbContext);
            _xkbContext = IntPtr.Zero;
        }
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static void HandleSeatCapabilities(IntPtr data, IntPtr seat, uint capabilities)
    {
        var handle = GCHandle.FromIntPtr(data);
        if (handle.Target is not WaylandInputSource inputSource)
        {
            return;
        }

        inputSource.SetPointerCapabilities(capabilities);
        inputSource.SetKeyboardCapabilities(capabilities);
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static void HandleSeatName(IntPtr data, IntPtr seat, IntPtr name)
    {
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static void HandlePointerEnter(IntPtr data, IntPtr pointer, uint serial, IntPtr surface, int surfaceX, int surfaceY)
    {
        var handle = GCHandle.FromIntPtr(data);
        if (handle.Target is WaylandInputSource inputSource)
        {
            inputSource._hasPointerFocus = true;
            inputSource.UpdatePointerPositionFromFixed(surfaceX, surfaceY);
        }
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static void HandlePointerLeave(IntPtr data, IntPtr pointer, uint serial, IntPtr surface)
    {
        var handle = GCHandle.FromIntPtr(data);
        if (handle.Target is WaylandInputSource inputSource)
        {
            inputSource._hasPointerFocus = false;
            inputSource.CurrentMouseButton = 0;
        }
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static void HandlePointerMotion(IntPtr data, IntPtr pointer, uint time, int surfaceX, int surfaceY)
    {
        var handle = GCHandle.FromIntPtr(data);
        if (handle.Target is WaylandInputSource inputSource)
        {
            inputSource.UpdatePointerPositionFromFixed(surfaceX, surfaceY);
        }
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static void HandlePointerButton(IntPtr data, IntPtr pointer, uint serial, uint time, uint button, uint state)
    {
        var handle = GCHandle.FromIntPtr(data);
        if (handle.Target is WaylandInputSource inputSource)
        {
            inputSource.UpdatePointerButton(button, state);
        }
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static void HandlePointerAxis(IntPtr data, IntPtr pointer, uint time, uint axis, int value)
    {
        var handle = GCHandle.FromIntPtr(data);
        if (handle.Target is WaylandInputSource inputSource)
        {
            inputSource.UpdatePointerAxis(axis, value);
        }
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static void HandleKeyboardKeymap(IntPtr data, IntPtr keyboard, uint format, int fileDescriptor, uint size)
    {
        var handle = GCHandle.FromIntPtr(data);
        if (handle.Target is WaylandInputSource inputSource && format == XkbKeymapFormatTextV1)
        {
            inputSource.UpdateKeyboardKeymap(fileDescriptor, size);
            return;
        }

        if (fileDescriptor >= 0)
        {
            _ = Close(fileDescriptor);
        }
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static void HandleKeyboardEnter(IntPtr data, IntPtr keyboard, uint serial, IntPtr surface, IntPtr keys)
    {
        var handle = GCHandle.FromIntPtr(data);
        if (handle.Target is WaylandInputSource inputSource)
        {
            inputSource._hasKeyboardFocus = true;
        }
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static void HandleKeyboardLeave(IntPtr data, IntPtr keyboard, uint serial, IntPtr surface)
    {
        var handle = GCHandle.FromIntPtr(data);
        if (handle.Target is WaylandInputSource inputSource)
        {
            inputSource._hasKeyboardFocus = false;
            inputSource._lastKey = 0;
            inputSource._keyPressed = false;
            inputSource._repeatKey = 0;
            inputSource._nextRepeatTick = 0;
            inputSource._lastTextKey = 0;
        }
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static void HandleKeyboardKey(IntPtr data, IntPtr keyboard, uint serial, uint time, uint key, uint state)
    {
        var handle = GCHandle.FromIntPtr(data);
        if (handle.Target is WaylandInputSource inputSource)
        {
            inputSource.UpdateKeyboardState(key, state);
        }
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static void HandleKeyboardModifiers(IntPtr data, IntPtr keyboard, uint serial, uint depressed, uint latched, uint locked, uint group)
    {
        var handle = GCHandle.FromIntPtr(data);
        if (handle.Target is WaylandInputSource inputSource)
        {
            inputSource.UpdateKeyboardModifiers(depressed, latched, locked, group);
        }
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static void HandleKeyboardRepeatInfo(IntPtr data, IntPtr keyboard, int rate, int delay)
    {
        var handle = GCHandle.FromIntPtr(data);
        if (handle.Target is WaylandInputSource inputSource)
        {
            inputSource.UpdateKeyboardRepeatInfo(rate, delay);
        }
    }
}
