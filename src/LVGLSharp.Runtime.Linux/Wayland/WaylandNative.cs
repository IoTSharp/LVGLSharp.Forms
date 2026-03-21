using System;
using System.Runtime.InteropServices;

namespace LVGLSharp.Runtime.Linux;

internal static unsafe partial class WaylandNative
{
    private const string WaylandClientLib = "libwayland-client.so.0";

    private static IntPtr s_waylandClientHandle;
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

    [LibraryImport(WaylandClientLib, EntryPoint = "wl_proxy_marshal_array_flags")]
    private static partial IntPtr WlProxyMarshalArrayFlags(IntPtr proxy, uint opcode, IntPtr interfacePtr, uint version, uint flags, WlArgument* args);

    [LibraryImport(WaylandClientLib, EntryPoint = "wl_proxy_get_version")]
    private static partial uint WlProxyGetVersion(IntPtr proxy);

    [LibraryImport(WaylandClientLib, EntryPoint = "wl_proxy_destroy")]
    private static partial void WlProxyDestroy(IntPtr proxy);

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

    internal static void DestroyProxy(IntPtr proxy)
    {
        if (proxy == IntPtr.Zero)
        {
            return;
        }

        WlProxyDestroy(proxy);
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
        if (registryProxy == IntPtr.Zero)
        {
            throw new ArgumentException("Wayland registry proxy cannot be zero.", nameof(registryProxy));
        }

        if (interfacePtr == IntPtr.Zero)
        {
            throw new ArgumentException("Wayland interface pointer cannot be zero.", nameof(interfacePtr));
        }

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
