using System;
using System.Runtime.InteropServices;

namespace LVGLSharp.Runtime.Linux;

internal static unsafe partial class WaylandNative
{
    private const string WaylandClientLib = "libwayland-client.so.0";
    private const uint WlShmFormatXrgb8888 = 1;

    private static IntPtr s_waylandClientHandle;
    private static IntPtr s_seatInterface;
    private static IntPtr s_xdgWmBaseInterface;
    private static IntPtr s_xdgSurfaceInterface;
    private static IntPtr s_xdgToplevelInterface;

    [StructLayout(LayoutKind.Sequential)]
    private struct WlInterface
    {
        public IntPtr Name;
        public int Version;
        public int MethodCount;
        public IntPtr Methods;
        public int EventCount;
        public IntPtr Events;
    }

    [StructLayout(LayoutKind.Explicit)]
    internal struct WlArgument
    {
        [FieldOffset(0)] public int I;
        [FieldOffset(0)] public uint U;
        [FieldOffset(0)] public IntPtr S;
        [FieldOffset(0)] public IntPtr O;
        [FieldOffset(0)] public uint N;
        [FieldOffset(0)] public int H;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal readonly struct XdgWmBaseListener
    {
        public readonly delegate* unmanaged[Cdecl]<IntPtr, IntPtr, uint, void> Ping;

        public XdgWmBaseListener(delegate* unmanaged[Cdecl]<IntPtr, IntPtr, uint, void> ping)
        {
            Ping = ping;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    internal readonly struct WlSeatListener
    {
        public readonly delegate* unmanaged[Cdecl]<IntPtr, IntPtr, uint, void> Capabilities;
        public readonly delegate* unmanaged[Cdecl]<IntPtr, IntPtr, IntPtr, void> Name;

        public WlSeatListener(
            delegate* unmanaged[Cdecl]<IntPtr, IntPtr, uint, void> capabilities,
            delegate* unmanaged[Cdecl]<IntPtr, IntPtr, IntPtr, void> name)
        {
            Capabilities = capabilities;
            Name = name;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    internal readonly struct WlPointerListener
    {
        public readonly delegate* unmanaged[Cdecl]<IntPtr, IntPtr, uint, IntPtr, int, int, void> Enter;
        public readonly delegate* unmanaged[Cdecl]<IntPtr, IntPtr, uint, IntPtr, void> Leave;
        public readonly delegate* unmanaged[Cdecl]<IntPtr, IntPtr, uint, int, int, void> Motion;
        public readonly delegate* unmanaged[Cdecl]<IntPtr, IntPtr, uint, uint, uint, uint, void> Button;
        public readonly delegate* unmanaged[Cdecl]<IntPtr, IntPtr, uint, uint, int, void> Axis;
        public readonly IntPtr Frame;
        public readonly IntPtr AxisSource;
        public readonly IntPtr AxisStop;
        public readonly IntPtr AxisDiscrete;
        public readonly IntPtr AxisValue120;
        public readonly IntPtr AxisRelativeDirection;

        public WlPointerListener(
            delegate* unmanaged[Cdecl]<IntPtr, IntPtr, uint, IntPtr, int, int, void> enter,
            delegate* unmanaged[Cdecl]<IntPtr, IntPtr, uint, IntPtr, void> leave,
            delegate* unmanaged[Cdecl]<IntPtr, IntPtr, uint, int, int, void> motion,
            delegate* unmanaged[Cdecl]<IntPtr, IntPtr, uint, uint, uint, uint, void> button,
            delegate* unmanaged[Cdecl]<IntPtr, IntPtr, uint, uint, int, void> axis)
        {
            Enter = enter;
            Leave = leave;
            Motion = motion;
            Button = button;
            Axis = axis;
            Frame = IntPtr.Zero;
            AxisSource = IntPtr.Zero;
            AxisStop = IntPtr.Zero;
            AxisDiscrete = IntPtr.Zero;
            AxisValue120 = IntPtr.Zero;
            AxisRelativeDirection = IntPtr.Zero;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    internal readonly struct WlKeyboardListener
    {
        public readonly delegate* unmanaged[Cdecl]<IntPtr, IntPtr, uint, int, uint, void> Keymap;
        public readonly delegate* unmanaged[Cdecl]<IntPtr, IntPtr, uint, IntPtr, IntPtr, void> Enter;
        public readonly delegate* unmanaged[Cdecl]<IntPtr, IntPtr, uint, IntPtr, void> Leave;
        public readonly delegate* unmanaged[Cdecl]<IntPtr, IntPtr, uint, uint, uint, uint, void> Key;
        public readonly delegate* unmanaged[Cdecl]<IntPtr, IntPtr, uint, uint, uint, uint, uint, void> Modifiers;
        public readonly IntPtr RepeatInfo;

        public WlKeyboardListener(
            delegate* unmanaged[Cdecl]<IntPtr, IntPtr, uint, int, uint, void> keymap,
            delegate* unmanaged[Cdecl]<IntPtr, IntPtr, uint, IntPtr, IntPtr, void> enter,
            delegate* unmanaged[Cdecl]<IntPtr, IntPtr, uint, IntPtr, void> leave,
            delegate* unmanaged[Cdecl]<IntPtr, IntPtr, uint, uint, uint, uint, void> key,
            delegate* unmanaged[Cdecl]<IntPtr, IntPtr, uint, uint, uint, uint, uint, void> modifiers)
        {
            Keymap = keymap;
            Enter = enter;
            Leave = leave;
            Key = key;
            Modifiers = modifiers;
            RepeatInfo = IntPtr.Zero;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    internal readonly struct XdgToplevelListener
    {
        public readonly delegate* unmanaged[Cdecl]<IntPtr, IntPtr, int, int, IntPtr, void> Configure;
        public readonly delegate* unmanaged[Cdecl]<IntPtr, IntPtr, void> Close;

        public XdgToplevelListener(
            delegate* unmanaged[Cdecl]<IntPtr, IntPtr, int, int, IntPtr, void> configure,
            delegate* unmanaged[Cdecl]<IntPtr, IntPtr, void> close)
        {
            Configure = configure;
            Close = close;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    internal readonly struct XdgSurfaceListener
    {
        public readonly delegate* unmanaged[Cdecl]<IntPtr, IntPtr, uint, void> Configure;

        public XdgSurfaceListener(delegate* unmanaged[Cdecl]<IntPtr, IntPtr, uint, void> configure)
        {
            Configure = configure;
        }
    }

    [LibraryImport(WaylandClientLib, EntryPoint = "wl_proxy_marshal_array_flags")]
    private static partial IntPtr WlProxyMarshalArrayFlags(IntPtr proxy, uint opcode, IntPtr interfacePtr, uint version, uint flags, WlArgument* args);

    [LibraryImport(WaylandClientLib, EntryPoint = "wl_proxy_get_version")]
    private static partial uint WlProxyGetVersion(IntPtr proxy);

    [LibraryImport(WaylandClientLib, EntryPoint = "wl_proxy_destroy")]
    private static partial void WlProxyDestroy(IntPtr proxy);

    [LibraryImport(WaylandClientLib, EntryPoint = "wl_proxy_add_listener")]
    private static partial int WlProxyAddXdgWmBaseListener(IntPtr proxy, XdgWmBaseListener* implementation, IntPtr data);

    [LibraryImport(WaylandClientLib, EntryPoint = "wl_proxy_add_listener")]
    private static partial int WlProxyAddXdgSurfaceListener(IntPtr proxy, XdgSurfaceListener* implementation, IntPtr data);

    [LibraryImport(WaylandClientLib, EntryPoint = "wl_proxy_add_listener")]
    private static partial int WlProxyAddXdgToplevelListener(IntPtr proxy, XdgToplevelListener* implementation, IntPtr data);

    [LibraryImport(WaylandClientLib, EntryPoint = "wl_proxy_add_listener")]
    private static partial int WlProxyAddSeatListener(IntPtr proxy, WlSeatListener* implementation, IntPtr data);

    [LibraryImport(WaylandClientLib, EntryPoint = "wl_proxy_add_listener")]
    private static partial int WlProxyAddPointerListener(IntPtr proxy, WlPointerListener* implementation, IntPtr data);

    [LibraryImport(WaylandClientLib, EntryPoint = "wl_proxy_add_listener")]
    private static partial int WlProxyAddKeyboardListener(IntPtr proxy, WlKeyboardListener* implementation, IntPtr data);

    internal static IntPtr BindGlobal(IntPtr registryProxy, WaylandGlobalInfo globalInfo, string interfaceSymbolName)
    {
        if (registryProxy == IntPtr.Zero)
        {
            throw new ArgumentException("Wayland registry proxy cannot be zero.", nameof(registryProxy));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(interfaceSymbolName);

        var interfacePtr = GetRequiredInterfaceSymbol(interfaceSymbolName);
        var interfaceNameUtf8 = Marshal.StringToCoTaskMemUTF8(globalInfo.InterfaceName);
        try
        {
            WlArgument* arguments = stackalloc WlArgument[4];
            arguments[0].U = globalInfo.Name;
            arguments[1].S = interfaceNameUtf8;
            arguments[2].U = globalInfo.Version;
            arguments[3].N = 0;

            var boundProxy = WlProxyMarshalArrayFlags(registryProxy, 0u, interfacePtr, globalInfo.Version, 0u, arguments);
            if (boundProxy == IntPtr.Zero)
            {
                throw new InvalidOperationException($"Unable to bind Wayland global '{globalInfo.InterfaceName}' ({globalInfo.Name}).");
            }

            return boundProxy;
        }
        finally
        {
            Marshal.FreeCoTaskMem(interfaceNameUtf8);
        }
    }

    internal static IntPtr CreateSurface(IntPtr compositorProxy)
    {
        if (compositorProxy == IntPtr.Zero)
        {
            throw new ArgumentException("Wayland compositor proxy cannot be zero.", nameof(compositorProxy));
        }

        var surfaceInterfacePtr = GetRequiredInterfaceSymbol("wl_surface_interface");
        WlArgument* arguments = stackalloc WlArgument[1];
        arguments[0].N = 0;

        var proxyVersion = WlProxyGetVersion(compositorProxy);
        var surfaceProxy = WlProxyMarshalArrayFlags(compositorProxy, 0u, surfaceInterfacePtr, proxyVersion, 0u, arguments);
        if (surfaceProxy == IntPtr.Zero)
        {
            throw new InvalidOperationException("Unable to create Wayland wl_surface.");
        }

        return surfaceProxy;
    }

    internal static IntPtr BindXdgWmBase(IntPtr registryProxy, WaylandGlobalInfo globalInfo)
    {
        return BindGlobal(registryProxy, globalInfo, GetOrCreateXdgWmBaseInterface());
    }

    internal static IntPtr BindSharedMemory(IntPtr registryProxy, WaylandGlobalInfo globalInfo)
    {
        return BindGlobal(registryProxy, globalInfo, "wl_shm_interface", 1u);
    }

    internal static IntPtr BindSeat(IntPtr registryProxy, WaylandGlobalInfo globalInfo)
    {
        return BindGlobal(registryProxy, globalInfo, GetOrCreateSeatInterface(), 1u);
    }

    internal static IntPtr CreateXdgSurface(IntPtr xdgWmBaseProxy, IntPtr surfaceProxy)
    {
        if (xdgWmBaseProxy == IntPtr.Zero)
        {
            throw new ArgumentException("Wayland xdg_wm_base proxy cannot be zero.", nameof(xdgWmBaseProxy));
        }

        if (surfaceProxy == IntPtr.Zero)
        {
            throw new ArgumentException("Wayland wl_surface proxy cannot be zero.", nameof(surfaceProxy));
        }

        WlArgument* arguments = stackalloc WlArgument[2];
        arguments[0].N = 0;
        arguments[1].O = surfaceProxy;

        var proxyVersion = WlProxyGetVersion(xdgWmBaseProxy);
        var xdgSurfaceProxy = WlProxyMarshalArrayFlags(xdgWmBaseProxy, 2u, GetOrCreateXdgSurfaceInterface(), proxyVersion, 0u, arguments);
        if (xdgSurfaceProxy == IntPtr.Zero)
        {
            throw new InvalidOperationException("Unable to create Wayland xdg_surface.");
        }

        return xdgSurfaceProxy;
    }

    internal static IntPtr CreateXdgToplevel(IntPtr xdgSurfaceProxy)
    {
        if (xdgSurfaceProxy == IntPtr.Zero)
        {
            throw new ArgumentException("Wayland xdg_surface proxy cannot be zero.", nameof(xdgSurfaceProxy));
        }

        WlArgument* arguments = stackalloc WlArgument[1];
        arguments[0].N = 0;

        var proxyVersion = WlProxyGetVersion(xdgSurfaceProxy);
        var xdgToplevelProxy = WlProxyMarshalArrayFlags(xdgSurfaceProxy, 1u, GetOrCreateXdgToplevelInterface(), proxyVersion, 0u, arguments);
        if (xdgToplevelProxy == IntPtr.Zero)
        {
            throw new InvalidOperationException("Unable to create Wayland xdg_toplevel.");
        }

        return xdgToplevelProxy;
    }

    internal static void PongXdgWmBase(IntPtr xdgWmBaseProxy, uint serial)
    {
        if (xdgWmBaseProxy == IntPtr.Zero)
        {
            throw new ArgumentException("Wayland xdg_wm_base proxy cannot be zero.", nameof(xdgWmBaseProxy));
        }

        WlArgument* arguments = stackalloc WlArgument[1];
        arguments[0].U = serial;

        var proxyVersion = WlProxyGetVersion(xdgWmBaseProxy);
        _ = WlProxyMarshalArrayFlags(xdgWmBaseProxy, 3u, IntPtr.Zero, proxyVersion, 0u, arguments);
    }

    internal static void AckXdgSurfaceConfigure(IntPtr xdgSurfaceProxy, uint serial)
    {
        if (xdgSurfaceProxy == IntPtr.Zero)
        {
            throw new ArgumentException("Wayland xdg_surface proxy cannot be zero.", nameof(xdgSurfaceProxy));
        }

        WlArgument* arguments = stackalloc WlArgument[1];
        arguments[0].U = serial;

        var proxyVersion = WlProxyGetVersion(xdgSurfaceProxy);
        _ = WlProxyMarshalArrayFlags(xdgSurfaceProxy, 4u, IntPtr.Zero, proxyVersion, 0u, arguments);
    }

    internal static void SetXdgToplevelTitle(IntPtr xdgToplevelProxy, string title)
    {
        if (xdgToplevelProxy == IntPtr.Zero)
        {
            throw new ArgumentException("Wayland xdg_toplevel proxy cannot be zero.", nameof(xdgToplevelProxy));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(title);

        var titleUtf8 = Marshal.StringToCoTaskMemUTF8(title);
        try
        {
            WlArgument* arguments = stackalloc WlArgument[1];
            arguments[0].S = titleUtf8;

            var proxyVersion = WlProxyGetVersion(xdgToplevelProxy);
            _ = WlProxyMarshalArrayFlags(xdgToplevelProxy, 2u, IntPtr.Zero, proxyVersion, 0u, arguments);
        }
        finally
        {
            Marshal.FreeCoTaskMem(titleUtf8);
        }
    }

    internal static void CommitSurface(IntPtr surfaceProxy)
    {
        if (surfaceProxy == IntPtr.Zero)
        {
            throw new ArgumentException("Wayland wl_surface proxy cannot be zero.", nameof(surfaceProxy));
        }

        var proxyVersion = WlProxyGetVersion(surfaceProxy);
        _ = WlProxyMarshalArrayFlags(surfaceProxy, 6u, IntPtr.Zero, proxyVersion, 0u, null);
    }

    internal static IntPtr CreateSharedMemoryPool(IntPtr sharedMemoryProxy, int fileDescriptor, int size)
    {
        if (sharedMemoryProxy == IntPtr.Zero)
        {
            throw new ArgumentException("Wayland wl_shm proxy cannot be zero.", nameof(sharedMemoryProxy));
        }

        WlArgument* arguments = stackalloc WlArgument[3];
        arguments[0].N = 0;
        arguments[1].H = fileDescriptor;
        arguments[2].I = size;

        var proxyVersion = WlProxyGetVersion(sharedMemoryProxy);
        var poolProxy = WlProxyMarshalArrayFlags(sharedMemoryProxy, 0u, GetRequiredInterfaceSymbol("wl_shm_pool_interface"), proxyVersion, 0u, arguments);
        if (poolProxy == IntPtr.Zero)
        {
            throw new InvalidOperationException("Unable to create Wayland wl_shm_pool.");
        }

        return poolProxy;
    }

    internal static IntPtr CreateSharedMemoryBuffer(IntPtr poolProxy, int width, int height, int stride)
    {
        if (poolProxy == IntPtr.Zero)
        {
            throw new ArgumentException("Wayland wl_shm_pool proxy cannot be zero.", nameof(poolProxy));
        }

        WlArgument* arguments = stackalloc WlArgument[7];
        arguments[0].N = 0;
        arguments[1].I = 0;
        arguments[2].I = width;
        arguments[3].I = height;
        arguments[4].I = stride;
        arguments[5].U = WlShmFormatXrgb8888;
        arguments[6].N = 0;

        var proxyVersion = WlProxyGetVersion(poolProxy);
        var bufferProxy = WlProxyMarshalArrayFlags(poolProxy, 0u, GetRequiredInterfaceSymbol("wl_buffer_interface"), proxyVersion, 0u, arguments);
        if (bufferProxy == IntPtr.Zero)
        {
            throw new InvalidOperationException("Unable to create Wayland wl_buffer.");
        }

        return bufferProxy;
    }

    internal static void AttachBuffer(IntPtr surfaceProxy, IntPtr bufferProxy, int x, int y)
    {
        if (surfaceProxy == IntPtr.Zero)
        {
            throw new ArgumentException("Wayland wl_surface proxy cannot be zero.", nameof(surfaceProxy));
        }

        WlArgument* arguments = stackalloc WlArgument[3];
        arguments[0].O = bufferProxy;
        arguments[1].I = x;
        arguments[2].I = y;

        var proxyVersion = WlProxyGetVersion(surfaceProxy);
        _ = WlProxyMarshalArrayFlags(surfaceProxy, 1u, IntPtr.Zero, proxyVersion, 0u, arguments);
    }

    internal static void DamageBuffer(IntPtr surfaceProxy, int x, int y, int width, int height)
    {
        if (surfaceProxy == IntPtr.Zero)
        {
            throw new ArgumentException("Wayland wl_surface proxy cannot be zero.", nameof(surfaceProxy));
        }

        WlArgument* arguments = stackalloc WlArgument[4];
        arguments[0].I = x;
        arguments[1].I = y;
        arguments[2].I = width;
        arguments[3].I = height;

        var proxyVersion = WlProxyGetVersion(surfaceProxy);
        _ = WlProxyMarshalArrayFlags(surfaceProxy, 9u, IntPtr.Zero, proxyVersion, 0u, arguments);
    }

    internal static int AddSeatListener(IntPtr seatProxy, WlSeatListener* listener, IntPtr data)
    {
        if (seatProxy == IntPtr.Zero)
        {
            throw new ArgumentException("Wayland wl_seat proxy cannot be zero.", nameof(seatProxy));
        }

        return WlProxyAddSeatListener(seatProxy, listener, data);
    }

    internal static IntPtr GetSeatPointer(IntPtr seatProxy)
    {
        if (seatProxy == IntPtr.Zero)
        {
            throw new ArgumentException("Wayland wl_seat proxy cannot be zero.", nameof(seatProxy));
        }

        WlArgument* arguments = stackalloc WlArgument[1];
        arguments[0].N = 0;

        var proxyVersion = WlProxyGetVersion(seatProxy);
        var pointerProxy = WlProxyMarshalArrayFlags(seatProxy, 0u, GetRequiredInterfaceSymbol("wl_pointer_interface"), proxyVersion, 0u, arguments);
        if (pointerProxy == IntPtr.Zero)
        {
            throw new InvalidOperationException("Unable to create Wayland wl_pointer.");
        }

        return pointerProxy;
    }

    internal static IntPtr GetSeatKeyboard(IntPtr seatProxy)
    {
        if (seatProxy == IntPtr.Zero)
        {
            throw new ArgumentException("Wayland wl_seat proxy cannot be zero.", nameof(seatProxy));
        }

        WlArgument* arguments = stackalloc WlArgument[1];
        arguments[0].N = 0;

        var proxyVersion = WlProxyGetVersion(seatProxy);
        var keyboardProxy = WlProxyMarshalArrayFlags(seatProxy, 1u, GetRequiredInterfaceSymbol("wl_keyboard_interface"), proxyVersion, 0u, arguments);
        if (keyboardProxy == IntPtr.Zero)
        {
            throw new InvalidOperationException("Unable to create Wayland wl_keyboard.");
        }

        return keyboardProxy;
    }

    internal static int AddPointerListener(IntPtr pointerProxy, WlPointerListener* listener, IntPtr data)
    {
        if (pointerProxy == IntPtr.Zero)
        {
            throw new ArgumentException("Wayland wl_pointer proxy cannot be zero.", nameof(pointerProxy));
        }

        return WlProxyAddPointerListener(pointerProxy, listener, data);
    }

    internal static int AddKeyboardListener(IntPtr keyboardProxy, WlKeyboardListener* listener, IntPtr data)
    {
        if (keyboardProxy == IntPtr.Zero)
        {
            throw new ArgumentException("Wayland wl_keyboard proxy cannot be zero.", nameof(keyboardProxy));
        }

        return WlProxyAddKeyboardListener(keyboardProxy, listener, data);
    }

    internal static void DestroyProxy(IntPtr proxy)
    {
        if (proxy == IntPtr.Zero)
        {
            return;
        }

        WlProxyDestroy(proxy);
    }

    internal static int AddXdgWmBaseListener(IntPtr xdgWmBaseProxy, XdgWmBaseListener* listener, IntPtr data)
    {
        if (xdgWmBaseProxy == IntPtr.Zero)
        {
            throw new ArgumentException("Wayland xdg_wm_base proxy cannot be zero.", nameof(xdgWmBaseProxy));
        }

        return WlProxyAddXdgWmBaseListener(xdgWmBaseProxy, listener, data);
    }

    internal static int AddXdgSurfaceListener(IntPtr xdgSurfaceProxy, XdgSurfaceListener* listener, IntPtr data)
    {
        if (xdgSurfaceProxy == IntPtr.Zero)
        {
            throw new ArgumentException("Wayland xdg_surface proxy cannot be zero.", nameof(xdgSurfaceProxy));
        }

        return WlProxyAddXdgSurfaceListener(xdgSurfaceProxy, listener, data);
    }

    internal static int AddXdgToplevelListener(IntPtr xdgToplevelProxy, XdgToplevelListener* listener, IntPtr data)
    {
        if (xdgToplevelProxy == IntPtr.Zero)
        {
            throw new ArgumentException("Wayland xdg_toplevel proxy cannot be zero.", nameof(xdgToplevelProxy));
        }

        return WlProxyAddXdgToplevelListener(xdgToplevelProxy, listener, data);
    }

    internal static IntPtr GetRequiredInterfaceSymbol(string symbolName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(symbolName);

        var libraryHandle = GetWaylandClientHandle();
        if (!NativeLibrary.TryGetExport(libraryHandle, symbolName, out var symbolAddress) || symbolAddress == IntPtr.Zero)
        {
            throw new InvalidOperationException($"Unable to resolve Wayland interface symbol '{symbolName}'.");
        }

        return symbolAddress;
    }

    private static IntPtr BindGlobal(IntPtr registryProxy, WaylandGlobalInfo globalInfo, IntPtr interfacePtr)
    {
        return BindGlobal(registryProxy, globalInfo, interfacePtr, globalInfo.Version);
    }

    private static IntPtr BindGlobal(IntPtr registryProxy, WaylandGlobalInfo globalInfo, string interfaceSymbolName, uint bindVersion)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(interfaceSymbolName);
        return BindGlobal(registryProxy, globalInfo, GetRequiredInterfaceSymbol(interfaceSymbolName), bindVersion);
    }

    private static IntPtr BindGlobal(IntPtr registryProxy, WaylandGlobalInfo globalInfo, IntPtr interfacePtr, uint bindVersion)
    {
        if (registryProxy == IntPtr.Zero)
        {
            throw new ArgumentException("Wayland registry proxy cannot be zero.", nameof(registryProxy));
        }

        if (interfacePtr == IntPtr.Zero)
        {
            throw new ArgumentException("Wayland interface pointer cannot be zero.", nameof(interfacePtr));
        }

        var targetVersion = bindVersion == 0 ? globalInfo.Version : Math.Min(globalInfo.Version, bindVersion);
        var interfaceNameUtf8 = Marshal.StringToCoTaskMemUTF8(globalInfo.InterfaceName);
        try
        {
            WlArgument* arguments = stackalloc WlArgument[4];
            arguments[0].U = globalInfo.Name;
            arguments[1].S = interfaceNameUtf8;
            arguments[2].U = targetVersion;
            arguments[3].N = 0;

            var boundProxy = WlProxyMarshalArrayFlags(registryProxy, 0u, interfacePtr, targetVersion, 0u, arguments);
            if (boundProxy == IntPtr.Zero)
            {
                throw new InvalidOperationException($"Unable to bind Wayland global '{globalInfo.InterfaceName}' ({globalInfo.Name}).");
            }

            return boundProxy;
        }
        finally
        {
            Marshal.FreeCoTaskMem(interfaceNameUtf8);
        }
    }

    private static IntPtr GetOrCreateSeatInterface()
    {
        return s_seatInterface != IntPtr.Zero
            ? s_seatInterface
            : s_seatInterface = CreateCustomInterface("wl_seat", 1);
    }

    private static IntPtr GetOrCreateXdgWmBaseInterface()
    {
        return s_xdgWmBaseInterface != IntPtr.Zero
            ? s_xdgWmBaseInterface
            : s_xdgWmBaseInterface = CreateCustomInterface("xdg_wm_base", 6);
    }

    private static IntPtr GetOrCreateXdgSurfaceInterface()
    {
        return s_xdgSurfaceInterface != IntPtr.Zero
            ? s_xdgSurfaceInterface
            : s_xdgSurfaceInterface = CreateCustomInterface("xdg_surface", 6);
    }

    private static IntPtr GetOrCreateXdgToplevelInterface()
    {
        return s_xdgToplevelInterface != IntPtr.Zero
            ? s_xdgToplevelInterface
            : s_xdgToplevelInterface = CreateCustomInterface("xdg_toplevel", 6);
    }

    private static IntPtr CreateCustomInterface(string name, int version)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var interfaceStruct = Marshal.AllocHGlobal(sizeof(WlInterface));
        *(WlInterface*)interfaceStruct = new WlInterface
        {
            Name = Marshal.StringToCoTaskMemUTF8(name),
            Version = version,
            MethodCount = 0,
            Methods = IntPtr.Zero,
            EventCount = 0,
            Events = IntPtr.Zero,
        };

        return interfaceStruct;
    }

    private static IntPtr GetWaylandClientHandle()
    {
        if (s_waylandClientHandle != IntPtr.Zero)
        {
            return s_waylandClientHandle;
        }

        s_waylandClientHandle = NativeLibrary.Load(WaylandClientLib);
        return s_waylandClientHandle;
    }
}
