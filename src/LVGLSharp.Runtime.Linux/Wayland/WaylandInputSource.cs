using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace LVGLSharp.Runtime.Linux;

internal unsafe sealed class WaylandInputSource : IDisposable
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

    private static readonly WaylandNative.WlSeatListener s_seatListener = new(&HandleSeatCapabilities, &HandleSeatName);
    private static readonly WaylandNative.WlPointerListener s_pointerListener = new(&HandlePointerEnter, &HandlePointerLeave, &HandlePointerMotion, &HandlePointerButton, &HandlePointerAxis);
    private static readonly WaylandNative.WlKeyboardListener s_keyboardListener = new(&HandleKeyboardKeymap, &HandleKeyboardEnter, &HandleKeyboardLeave, &HandleKeyboardKey, &HandleKeyboardModifiers);

    public bool SupportsPointer => true;

    public bool SupportsKeyboard => true;

    public bool SupportsTextInput => false;

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
        _lastKey = MapLinuxKeyCodeToLvKey(key);
        _keyPressed = state != 0 && _lastKey != 0;
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

    private static uint MapLinuxKeyCodeToLvKey(uint keyCode)
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
            57 => (uint)' ',
            >= 2 and <= 11 => (uint)(keyCode == 11 ? '0' : '1' + (keyCode - 2)),
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
