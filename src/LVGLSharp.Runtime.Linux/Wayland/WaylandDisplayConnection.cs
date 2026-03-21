using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace LVGLSharp.Runtime.Linux;

internal sealed unsafe partial class WaylandDisplayConnection : IDisposable
{
    private const string WaylandClientLib = "libwayland-client.so.0";

    private IntPtr _display;
    private IntPtr _registry;
    private GCHandle _listenerStateHandle;

    [StructLayout(LayoutKind.Sequential)]
    private readonly struct WlRegistryListener
    {
        public readonly delegate* unmanaged[Cdecl]<IntPtr, IntPtr, uint, IntPtr, uint, void> Global;
        public readonly delegate* unmanaged[Cdecl]<IntPtr, IntPtr, uint, void> GlobalRemove;

        public WlRegistryListener(
            delegate* unmanaged[Cdecl]<IntPtr, IntPtr, uint, IntPtr, uint, void> global,
            delegate* unmanaged[Cdecl]<IntPtr, IntPtr, uint, void> globalRemove)
        {
            Global = global;
            GlobalRemove = globalRemove;
        }
    }

    private static readonly WlRegistryListener s_registryListener = new(&HandleRegistryGlobal, &HandleRegistryGlobalRemove);

    [LibraryImport(WaylandClientLib, EntryPoint = "wl_display_connect", StringMarshalling = StringMarshalling.Utf8)]
    private static partial IntPtr WlDisplayConnect(string? name);

    [LibraryImport(WaylandClientLib, EntryPoint = "wl_display_disconnect")]
    private static partial void WlDisplayDisconnect(IntPtr display);

    [LibraryImport(WaylandClientLib, EntryPoint = "wl_display_roundtrip")]
    private static partial int WlDisplayRoundtrip(IntPtr display);

    [LibraryImport(WaylandClientLib, EntryPoint = "wl_display_dispatch_pending")]
    private static partial int WlDisplayDispatchPending(IntPtr display);

    [LibraryImport(WaylandClientLib, EntryPoint = "wl_display_dispatch")]
    private static partial int WlDisplayDispatch(IntPtr display);

    [LibraryImport(WaylandClientLib, EntryPoint = "wl_display_flush")]
    private static partial int WlDisplayFlush(IntPtr display);

    [LibraryImport(WaylandClientLib, EntryPoint = "wl_display_get_registry")]
    private static partial IntPtr WlDisplayGetRegistry(IntPtr display);

    [LibraryImport(WaylandClientLib, EntryPoint = "wl_proxy_add_listener")]
    private static partial int WlProxyAddListener(IntPtr proxy, WlRegistryListener* implementation, IntPtr data);

    public WaylandDisplayConnection(string? requestedDisplayName, string diagnosticSummary)
    {
        DiagnosticSummary = string.IsNullOrWhiteSpace(diagnosticSummary)
            ? throw new ArgumentException("Value cannot be null or whitespace.", nameof(diagnosticSummary))
            : diagnosticSummary;
        RequestedDisplayName = requestedDisplayName;
    }

    public string? RequestedDisplayName { get; }

    public string DiagnosticSummary { get; }

    public string? ConnectedDisplayName { get; private set; }

    public bool HasCompositor { get; private set; }

    public bool HasSharedMemory { get; private set; }

    public bool HasXdgWmBase { get; private set; }

    public bool HasSeat { get; private set; }

    public uint CompositorVersion { get; private set; }

    public uint SharedMemoryVersion { get; private set; }

    public uint XdgWmBaseVersion { get; private set; }

    public uint SeatVersion { get; private set; }

    public IReadOnlyCollection<string> DiscoveredInterfaces => _discoveredInterfaces.AsReadOnly();

    public IReadOnlyCollection<WaylandGlobalInfo> DiscoveredGlobals => _discoveredGlobals.AsReadOnly();

    public IntPtr RegistryProxy => _registry;

    public bool IsConnected => _display != IntPtr.Zero;

    public bool IsDisposed { get; private set; }

    private List<string> _discoveredInterfaces { get; } = [];
    private List<WaylandGlobalInfo> _discoveredGlobals { get; } = [];

    public void Connect()
    {
        ThrowIfDisposed();

        if (IsConnected)
        {
            return;
        }

        _display = WlDisplayConnect(RequestedDisplayName);
        if (_display == IntPtr.Zero)
        {
            throw new InvalidOperationException($"Unable to connect to Wayland display. {DiagnosticSummary}");
        }

        ConnectedDisplayName = RequestedDisplayName;
        InitializeRegistry();
    }

    public void Roundtrip()
    {
        ThrowIfDisposed();
        ThrowIfNotConnected();

        var result = WlDisplayRoundtrip(_display);
        if (result < 0)
        {
            throw new InvalidOperationException($"Wayland roundtrip failed. {DiagnosticSummary}");
        }
    }

    public void DispatchPending()
    {
        ThrowIfDisposed();
        ThrowIfNotConnected();

        var result = WlDisplayDispatchPending(_display);
        if (result < 0)
        {
            throw new InvalidOperationException($"Wayland dispatch pending failed. {DiagnosticSummary}");
        }
    }

    public void Dispatch()
    {
        ThrowIfDisposed();
        ThrowIfNotConnected();

        var result = WlDisplayDispatch(_display);
        if (result < 0)
        {
            throw new InvalidOperationException($"Wayland dispatch failed. {DiagnosticSummary}");
        }
    }

    public void Flush()
    {
        ThrowIfDisposed();
        ThrowIfNotConnected();

        var result = WlDisplayFlush(_display);
        if (result < 0)
        {
            throw new InvalidOperationException($"Wayland flush failed. {DiagnosticSummary}");
        }
    }

    public void PumpEvents()
    {
        Flush();
        DispatchPending();
    }

    public void Disconnect()
    {
        if (!IsConnected)
        {
            ConnectedDisplayName = null;
            return;
        }

        ReleaseRegistry();

        WlDisplayDisconnect(_display);
        _display = IntPtr.Zero;
        ConnectedDisplayName = null;
        ResetGlobals();
    }

    public bool TryGetGlobal(string interfaceName, out WaylandGlobalInfo globalInfo)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(interfaceName);

        for (var index = 0; index < _discoveredGlobals.Count; index++)
        {
            var candidate = _discoveredGlobals[index];
            if (string.Equals(candidate.InterfaceName, interfaceName, StringComparison.Ordinal))
            {
                globalInfo = candidate;
                return true;
            }
        }

        globalInfo = default;
        return false;
    }

    public IntPtr BindCompositor()
    {
        ThrowIfDisposed();
        ThrowIfNotConnected();

        if (_registry == IntPtr.Zero)
        {
            throw new InvalidOperationException($"Wayland registry is unavailable. {DiagnosticSummary}");
        }

        if (!TryGetGlobal("wl_compositor", out var compositorGlobal))
        {
            throw new InvalidOperationException($"Wayland compositor global is unavailable. {DiagnosticSummary}");
        }

        return WaylandNative.BindGlobal(_registry, compositorGlobal, "wl_compositor_interface");
    }

    public IntPtr BindXdgWmBase()
    {
        ThrowIfDisposed();
        ThrowIfNotConnected();

        if (_registry == IntPtr.Zero)
        {
            throw new InvalidOperationException($"Wayland registry is unavailable. {DiagnosticSummary}");
        }

        if (!TryGetGlobal("xdg_wm_base", out var xdgWmBaseGlobal))
        {
            throw new InvalidOperationException($"Wayland xdg_wm_base global is unavailable. {DiagnosticSummary}");
        }

        return WaylandNative.BindXdgWmBase(_registry, xdgWmBaseGlobal);
    }

    public IntPtr BindSharedMemory()
    {
        ThrowIfDisposed();
        ThrowIfNotConnected();

        if (_registry == IntPtr.Zero)
        {
            throw new InvalidOperationException($"Wayland registry is unavailable. {DiagnosticSummary}");
        }

        if (!TryGetGlobal("wl_shm", out var sharedMemoryGlobal))
        {
            throw new InvalidOperationException($"Wayland wl_shm global is unavailable. {DiagnosticSummary}");
        }

        return WaylandNative.BindSharedMemory(_registry, sharedMemoryGlobal);
    }

    public IntPtr BindSeat()
    {
        ThrowIfDisposed();
        ThrowIfNotConnected();

        if (_registry == IntPtr.Zero)
        {
            throw new InvalidOperationException($"Wayland registry is unavailable. {DiagnosticSummary}");
        }

        if (!TryGetGlobal("wl_seat", out var seatGlobal))
        {
            throw new InvalidOperationException($"Wayland wl_seat global is unavailable. {DiagnosticSummary}");
        }

        return WaylandNative.BindSeat(_registry, seatGlobal);
    }

    public void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);
    }

    public void Dispose()
    {
        if (IsDisposed)
        {
            return;
        }

        Disconnect();
        IsDisposed = true;
    }

    private void InitializeRegistry()
    {
        if (_registry != IntPtr.Zero)
        {
            return;
        }

        _registry = WlDisplayGetRegistry(_display);
        if (_registry == IntPtr.Zero)
        {
            throw new InvalidOperationException($"Unable to acquire Wayland registry. {DiagnosticSummary}");
        }

        _listenerStateHandle = GCHandle.Alloc(this);
        fixed (WlRegistryListener* listener = &s_registryListener)
        {
            var result = WlProxyAddListener(_registry, listener, GCHandle.ToIntPtr(_listenerStateHandle));
            if (result != 0)
            {
                ReleaseRegistry();
                throw new InvalidOperationException($"Unable to attach Wayland registry listener. {DiagnosticSummary}");
            }
        }

        Roundtrip();
        DispatchPending();
    }

    private void ReleaseRegistry()
    {
        if (_registry != IntPtr.Zero)
        {
            WaylandNative.DestroyProxy(_registry);
            _registry = IntPtr.Zero;
        }

        if (_listenerStateHandle.IsAllocated)
        {
            _listenerStateHandle.Free();
        }
    }

    private void ResetGlobals()
    {
        _discoveredInterfaces.Clear();
        _discoveredGlobals.Clear();
        RebuildCapabilitiesFromGlobals();
    }

    private void ThrowIfNotConnected()
    {
        if (!IsConnected)
        {
            throw new InvalidOperationException($"Wayland display is not connected. {DiagnosticSummary}");
        }
    }

    private void RecordGlobal(uint name, string interfaceName, uint version)
    {
        var existingIndex = -1;
        for (var index = 0; index < _discoveredGlobals.Count; index++)
        {
            if (_discoveredGlobals[index].Name == name)
            {
                existingIndex = index;
                break;
            }
        }

        var globalInfo = new WaylandGlobalInfo(name, interfaceName, version);
        if (existingIndex >= 0)
        {
            _discoveredGlobals[existingIndex] = globalInfo;
        }
        else
        {
            _discoveredGlobals.Add(globalInfo);
        }

        if (!_discoveredInterfaces.Contains(interfaceName, StringComparer.Ordinal))
        {
            _discoveredInterfaces.Add(interfaceName);
        }

        RebuildCapabilitiesFromGlobals();
    }

    private void RemoveGlobal(uint name)
    {
        var removed = _discoveredGlobals.RemoveAll(candidate => candidate.Name == name);
        if (removed == 0)
        {
            return;
        }

        _discoveredInterfaces.Clear();
        for (var index = 0; index < _discoveredGlobals.Count; index++)
        {
            var interfaceName = _discoveredGlobals[index].InterfaceName;
            if (!_discoveredInterfaces.Contains(interfaceName, StringComparer.Ordinal))
            {
                _discoveredInterfaces.Add(interfaceName);
            }
        }

        RebuildCapabilitiesFromGlobals();
    }

    private void RebuildCapabilitiesFromGlobals()
    {
        HasCompositor = false;
        HasSharedMemory = false;
        HasXdgWmBase = false;
        HasSeat = false;
        CompositorVersion = 0;
        SharedMemoryVersion = 0;
        XdgWmBaseVersion = 0;
        SeatVersion = 0;

        for (var index = 0; index < _discoveredGlobals.Count; index++)
        {
            var globalInfo = _discoveredGlobals[index];
            switch (globalInfo.InterfaceName)
            {
                case "wl_compositor":
                    HasCompositor = true;
                    CompositorVersion = Math.Max(CompositorVersion, globalInfo.Version);
                    break;
                case "wl_shm":
                    HasSharedMemory = true;
                    SharedMemoryVersion = Math.Max(SharedMemoryVersion, globalInfo.Version);
                    break;
                case "xdg_wm_base":
                    HasXdgWmBase = true;
                    XdgWmBaseVersion = Math.Max(XdgWmBaseVersion, globalInfo.Version);
                    break;
                case "wl_seat":
                    HasSeat = true;
                    SeatVersion = Math.Max(SeatVersion, globalInfo.Version);
                    break;
            }
        }
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static void HandleRegistryGlobal(IntPtr data, IntPtr registry, uint name, IntPtr interfaceName, uint version)
    {
        var handle = GCHandle.FromIntPtr(data);
        if (handle.Target is not WaylandDisplayConnection connection)
        {
            return;
        }

        var discoveredInterface = Marshal.PtrToStringUTF8(interfaceName);
        if (string.IsNullOrWhiteSpace(discoveredInterface))
        {
            return;
        }

        connection.RecordGlobal(name, discoveredInterface, version);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static void HandleRegistryGlobalRemove(IntPtr data, IntPtr registry, uint name)
    {
        var handle = GCHandle.FromIntPtr(data);
        if (handle.Target is WaylandDisplayConnection connection)
        {
            connection.RemoveGlobal(name);
        }
    }
}
