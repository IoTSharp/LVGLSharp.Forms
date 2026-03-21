using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace LVGLSharp.Runtime.Linux;

internal unsafe sealed partial class WaylandInputSource : IDisposable
{
    private const uint SeatCapabilityPointer = 1u;
    private const uint SeatCapabilityKeyboard = 2u;
    private const uint PointerButtonStatePressed = 1u;
    private const uint BtnLeft = 272u;
    private const uint BtnRight = 273u;
    private const uint BtnMiddle = 274u;

    private IntPtr _seat;
    private IntPtr _pointer;
    private IntPtr _keyboard;
    private GCHandle _listenerStateHandle;

    private uint _lastKey;
    private bool _keyPressed;
    private bool _shiftPressed;
    private bool _capsLockEnabled;

    [LibraryImport("libc", EntryPoint = "close")]
    private static partial int Close(int fd);

    private static readonly WaylandNative.WlSeatListener s_seatListener = new(&HandleSeatCapabilities, &HandleSeatName);
    private static readonly WaylandNative.WlPointerListener s_pointerListener = new(&HandlePointerEnter, &HandlePointerLeave, &HandlePointerMotion, &HandlePointerButton, &HandlePointerAxis);
    private static readonly WaylandNative.WlKeyboardListener s_keyboardListener = new(&HandleKeyboardKeymap, &HandleKeyboardEnter, &HandleKeyboardLeave, &HandleKeyboardKey, &HandleKeyboardModifiers);

    public bool SupportsPointer => true;

    public bool SupportsKeyboard => true;

    public bool SupportsTextInput => true;

    public (int X, int Y) CurrentMousePosition { get; private set; }

    public uint CurrentMouseButton { get; private set; }

    public uint CurrentKey => _lastKey;

    public bool IsKeyPressed => _keyPressed;

    public bool IsDisposed { get; private set; }

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
        _shiftPressed = false;
        _capsLockEnabled = false;
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
            _shiftPressed = false;
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

    private void UpdateKeyboardState(uint key, uint state)
    {
        var isPressed = state != 0;

        switch (key)
        {
            case 42 or 54:
                _shiftPressed = isPressed;
                _lastKey = 0;
                _keyPressed = false;
                return;
            case 58 when isPressed:
                _capsLockEnabled = !_capsLockEnabled;
                _lastKey = 0;
                _keyPressed = false;
                return;
        }

        _lastKey = MapLinuxKeyCodeToLvKey(key, _shiftPressed, _capsLockEnabled);
        _keyPressed = isPressed && _lastKey != 0;

        if (!isPressed && _lastKey == 0)
        {
            _keyPressed = false;
        }
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

    private static uint MapLinuxKeyCodeToLvKey(uint keyCode, bool shiftPressed, bool capsLockEnabled)
    {
        var uppercase = shiftPressed ^ capsLockEnabled;

        return keyCode switch
        {
            28 => (uint)LV_KEY_ENTER,
            1 => (uint)LV_KEY_ESC,
            14 => (uint)LV_KEY_BACKSPACE,
            15 => (uint)LV_KEY_NEXT,
            43 => shiftPressed ? (uint)'|' : (uint)'\\',
            12 => shiftPressed ? (uint)'_' : (uint)'-',
            13 => shiftPressed ? (uint)'+' : (uint)'=',
            26 => shiftPressed ? (uint)'{' : (uint)'[',
            27 => shiftPressed ? (uint)'}' : (uint)']',
            39 => shiftPressed ? (uint)':' : (uint)';',
            40 => shiftPressed ? (uint)'"' : (uint)'\'',
            41 => shiftPressed ? (uint)'~' : (uint)'`',
            51 => shiftPressed ? (uint)'<' : (uint)',',
            52 => shiftPressed ? (uint)'>' : (uint)'.',
            53 => shiftPressed ? (uint)'?' : (uint)'/',
            102 => (uint)LV_KEY_HOME,
            107 => (uint)LV_KEY_END,
            111 => (uint)LV_KEY_DEL,
            105 => (uint)LV_KEY_LEFT,
            106 => (uint)LV_KEY_RIGHT,
            103 => (uint)LV_KEY_UP,
            108 => (uint)LV_KEY_DOWN,
            57 => (uint)' ',
            >= 2 and <= 11 => MapDigitKey(keyCode, shiftPressed),
            >= 16 and <= 25 => MapTopRowLetterKey(keyCode, uppercase),
            >= 30 and <= 38 => MapMiddleRowLetterKey(keyCode, uppercase),
            >= 44 and <= 50 => MapBottomRowLetterKey(keyCode, uppercase),
            _ => 0,
        };
    }

    private static uint MapDigitKey(uint keyCode, bool shiftPressed)
    {
        return (keyCode, shiftPressed) switch
        {
            (2, true) => (uint)'!',
            (3, true) => (uint)'@',
            (4, true) => (uint)'#',
            (5, true) => (uint)'$',
            (6, true) => (uint)'%',
            (7, true) => (uint)'^',
            (8, true) => (uint)'&',
            (9, true) => (uint)'*',
            (10, true) => (uint)'(',
            (11, true) => (uint)')',
            _ => (uint)(keyCode == 11 ? '0' : '1' + (keyCode - 2)),
        };
    }

    private static uint MapLetterKey(char baseChar, bool uppercase)
    {
        return uppercase ? (uint)char.ToUpperInvariant(baseChar) : (uint)baseChar;
    }

    private static uint MapTopRowLetterKey(uint keyCode, bool uppercase)
    {
        return keyCode switch
        {
            16 => MapLetterKey('q', uppercase),
            17 => MapLetterKey('w', uppercase),
            18 => MapLetterKey('e', uppercase),
            19 => MapLetterKey('r', uppercase),
            20 => MapLetterKey('t', uppercase),
            21 => MapLetterKey('y', uppercase),
            22 => MapLetterKey('u', uppercase),
            23 => MapLetterKey('i', uppercase),
            24 => MapLetterKey('o', uppercase),
            25 => MapLetterKey('p', uppercase),
            _ => 0,
        };
    }

    private static uint MapMiddleRowLetterKey(uint keyCode, bool uppercase)
    {
        return keyCode switch
        {
            30 => MapLetterKey('a', uppercase),
            31 => MapLetterKey('s', uppercase),
            32 => MapLetterKey('d', uppercase),
            33 => MapLetterKey('f', uppercase),
            34 => MapLetterKey('g', uppercase),
            35 => MapLetterKey('h', uppercase),
            36 => MapLetterKey('j', uppercase),
            37 => MapLetterKey('k', uppercase),
            38 => MapLetterKey('l', uppercase),
            _ => 0,
        };
    }

    private static uint MapBottomRowLetterKey(uint keyCode, bool uppercase)
    {
        return keyCode switch
        {
            44 => MapLetterKey('z', uppercase),
            45 => MapLetterKey('x', uppercase),
            46 => MapLetterKey('c', uppercase),
            47 => MapLetterKey('v', uppercase),
            48 => MapLetterKey('b', uppercase),
            49 => MapLetterKey('n', uppercase),
            50 => MapLetterKey('m', uppercase),
            _ => 0,
        };
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
            inputSource.UpdatePointerPositionFromFixed(surfaceX, surfaceY);
        }
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static void HandlePointerLeave(IntPtr data, IntPtr pointer, uint serial, IntPtr surface)
    {
        var handle = GCHandle.FromIntPtr(data);
        if (handle.Target is WaylandInputSource inputSource)
        {
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
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static void HandleKeyboardKeymap(IntPtr data, IntPtr keyboard, uint format, int fileDescriptor, uint size)
    {
        if (fileDescriptor >= 0)
        {
            _ = Close(fileDescriptor);
        }
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static void HandleKeyboardEnter(IntPtr data, IntPtr keyboard, uint serial, IntPtr surface, IntPtr keys)
    {
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static void HandleKeyboardLeave(IntPtr data, IntPtr keyboard, uint serial, IntPtr surface)
    {
        var handle = GCHandle.FromIntPtr(data);
        if (handle.Target is WaylandInputSource inputSource)
        {
            inputSource._lastKey = 0;
            inputSource._keyPressed = false;
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
    }
}
