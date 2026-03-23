using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LVGLSharp.Runtime.Linux;

internal enum LinuxHostEnvironment
{
    Wslg,
    Wayland,
    Sdl,
    X11,
    FrameBuffer,
    Drm,
}

internal static class LinuxEnvironmentDetector
{
    internal static string FormatWaylandWindowTitle(string title, string? detectedWaylandDisplay, string? detectedX11Display)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);

        var preferredWaylandDisplay = GetDiagnosticValue(GetPreferredWaylandDisplay(detectedWaylandDisplay));
        var x11Fallback = GetDiagnosticValue(detectedX11Display);
        return $"{title} [Wayland:{preferredWaylandDisplay}, X11:{x11Fallback}]";
    }

    internal static string FormatWslgWindowTitle(string title, string? detectedDisplay)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);

        var preferredDisplay = GetDiagnosticValue(GetPreferredWslgDisplay(detectedDisplay));
        return $"{title} [WSLg:{preferredDisplay}]";
    }

    internal static string GetWslgDiagnosticSummary(string? detectedDisplay)
    {
        var processDisplay = GetDiagnosticValue(Environment.GetEnvironmentVariable("DISPLAY"));
        var preferredDisplay = GetDiagnosticValue(GetPreferredWslgDisplay(detectedDisplay));
        var waylandDisplay = GetDiagnosticValue(Environment.GetEnvironmentVariable("WAYLAND_DISPLAY"));
        var distroName = GetDiagnosticValue(Environment.GetEnvironmentVariable("WSL_DISTRO_NAME"));

        return $"WSLg DISPLAY={processDisplay}, PreferredX11={preferredDisplay}, WAYLAND={waylandDisplay}, DISTRO={distroName}";
    }

    internal static string GetWaylandDiagnosticSummary(string? detectedWaylandDisplay, string? detectedX11Display)
    {
        var sessionType = GetDiagnosticValue(Environment.GetEnvironmentVariable("XDG_SESSION_TYPE"));
        var processWaylandDisplay = GetDiagnosticValue(Environment.GetEnvironmentVariable("WAYLAND_DISPLAY"));
        var preferredWaylandDisplay = GetDiagnosticValue(GetPreferredWaylandDisplay(detectedWaylandDisplay));
        var runtimeDir = GetDiagnosticValue(Environment.GetEnvironmentVariable("XDG_RUNTIME_DIR"));
        var x11Fallback = GetDiagnosticValue(detectedX11Display);

        return $"Wayland SESSION={sessionType}, WAYLAND={processWaylandDisplay}, PreferredWayland={preferredWaylandDisplay}, XDG_RUNTIME_DIR={runtimeDir}, X11Fallback={x11Fallback}";
    }

    internal static IReadOnlyList<string?> GetX11DisplayCandidates(string? preferredDisplay)
    {
        List<string?> candidates = [];
        HashSet<string> seen = new(StringComparer.Ordinal);

        if (!string.IsNullOrWhiteSpace(preferredDisplay) && seen.Add(preferredDisplay))
        {
            candidates.Add(preferredDisplay);
        }

        foreach (var detectedDisplay in EnumerateSocketDisplays())
        {
            if (seen.Add(detectedDisplay))
            {
                candidates.Add(detectedDisplay);
            }
        }

        return candidates;
    }

    internal static LinuxHostEnvironment ResolveHostEnvironment(string? detectedWaylandDisplay, string? detectedX11Display, string fbdev)
    {
        var explicitHost = GetExplicitHost();
        if (explicitHost is not null)
        {
            return explicitHost.Value;
        }

        if (ShouldUseSdl())
        {
            return LinuxHostEnvironment.Sdl;
        }

        if (IsWslg(detectedX11Display))
        {
            return LinuxHostEnvironment.Wslg;
        }

        if (IsWayland(detectedWaylandDisplay))
        {
            return LinuxHostEnvironment.Wayland;
        }

        if (detectedX11Display is not null)
        {
            return LinuxHostEnvironment.X11;
        }

        if (File.Exists(fbdev))
        {
            return LinuxHostEnvironment.FrameBuffer;
        }

        return LinuxHostEnvironment.X11;
    }

    internal static LinuxDrmOptions GetDefaultDrmOptions(float dpi)
    {
        const string defaultDrmDevice = "/dev/dri/card0";
        return new LinuxDrmOptions(defaultDrmDevice, -1, dpi, DrmView.DrmModePreference.Default);
    }

    internal static bool ShouldUseSdl()
    {
        var explicitHostName = Environment.GetEnvironmentVariable("LVGLSHARP_LINUX_HOST");
        if (!string.IsNullOrWhiteSpace(explicitHostName) &&
            string.Equals(explicitHostName, "sdl", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var useSdl = Environment.GetEnvironmentVariable("LVGLSHARP_USE_SDL");
        return string.Equals(useSdl, "1", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(useSdl, "true", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(useSdl, "yes", StringComparison.OrdinalIgnoreCase);
    }

    private static LinuxHostEnvironment? GetExplicitHost()
    {
        var explicitHostName = Environment.GetEnvironmentVariable("LVGLSHARP_LINUX_HOST");
        if (string.IsNullOrWhiteSpace(explicitHostName))
        {
            return null;
        }

        return explicitHostName.Trim().ToLowerInvariant() switch
        {
            "wslg" => LinuxHostEnvironment.Wslg,
            "wayland" => LinuxHostEnvironment.Wayland,
            "sdl" => LinuxHostEnvironment.Sdl,
            "x11" => LinuxHostEnvironment.X11,
            "fb" or "framebuffer" => LinuxHostEnvironment.FrameBuffer,
            "drm" or "kms" => LinuxHostEnvironment.Drm,
            _ => null,
        };
    }

    internal static string? DetectWaylandDisplay()
    {
        var processWaylandDisplay = Environment.GetEnvironmentVariable("WAYLAND_DISPLAY");
        if (!string.IsNullOrWhiteSpace(processWaylandDisplay))
        {
            return processWaylandDisplay;
        }

        var runtimeDir = Environment.GetEnvironmentVariable("XDG_RUNTIME_DIR");
        if (!string.IsNullOrWhiteSpace(runtimeDir) && Directory.Exists(runtimeDir))
        {
            var socketEntry = Directory.EnumerateFiles(runtimeDir, "wayland-*")
                .Select(Path.GetFileName)
                .Where(static name => !string.IsNullOrWhiteSpace(name))
                .OrderBy(static name => name, StringComparer.Ordinal)
                .FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(socketEntry))
            {
                return socketEntry;
            }
        }

        return null;
    }

    internal static string? DetectX11Display()
    {
        const string x11SocketDir = "/tmp/.X11-unix";
        if (Directory.Exists(x11SocketDir))
        {
            var displayEntry = Directory.EnumerateFiles(x11SocketDir, "X*")
                .Select(Path.GetFileName)
                .Select(static name => name is { Length: > 1 } value ? value[1..] : string.Empty)
                .Where(static value => value.Length > 0)
                .OrderBy(static value => value, StringComparer.Ordinal)
                .FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(displayEntry))
            {
                return $":{displayEntry}";
            }
        }

        return null;
    }

    private static IEnumerable<string> EnumerateSocketDisplays()
    {
        const string x11SocketDir = "/tmp/.X11-unix";
        if (!Directory.Exists(x11SocketDir))
        {
            yield break;
        }

        foreach (var displayEntry in Directory.EnumerateFiles(x11SocketDir, "X*")
            .Select(Path.GetFileName)
            .Select(static name => name is { Length: > 1 } value ? value[1..] : string.Empty)
            .Where(static value => value.Length > 0)
            .OrderBy(static value => value, StringComparer.Ordinal))
        {
            yield return $":{displayEntry}";
        }
    }

    internal static string? GetPreferredWslgDisplay(string? detectedDisplay)
    {
        var processDisplay = Environment.GetEnvironmentVariable("DISPLAY");
        if (!string.IsNullOrWhiteSpace(processDisplay))
        {
            return processDisplay;
        }

        if (!string.IsNullOrWhiteSpace(detectedDisplay))
        {
            return detectedDisplay;
        }

        return Directory.Exists("/mnt/wslg") ? ":0" : null;
    }

    internal static string? GetPreferredWaylandDisplay(string? detectedWaylandDisplay)
    {
        var processWaylandDisplay = Environment.GetEnvironmentVariable("WAYLAND_DISPLAY");
        if (!string.IsNullOrWhiteSpace(processWaylandDisplay))
        {
            return processWaylandDisplay;
        }

        if (!string.IsNullOrWhiteSpace(detectedWaylandDisplay))
        {
            return detectedWaylandDisplay;
        }

        return null;
    }

    private static bool IsWsl()
    {
        if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("WSL_DISTRO_NAME")) ||
            !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("WSL_INTEROP")))
        {
            return true;
        }

        const string procVersionPath = "/proc/version";
        if (!File.Exists(procVersionPath))
        {
            return false;
        }

        var procVersion = File.ReadAllText(procVersionPath);
        return procVersion.Contains("Microsoft", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsWslg(string? detectedDisplay)
    {
        if (!IsWsl())
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("WAYLAND_DISPLAY")) ||
            !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("WSLG_RUNTIME_DIR")) ||
            Directory.Exists("/mnt/wslg"))
        {
            return true;
        }

        return detectedDisplay is not null &&
            !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("DISPLAY"));
    }

    private static bool IsWayland(string? detectedWaylandDisplay)
    {
        if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("WAYLAND_DISPLAY")) ||
            !string.IsNullOrWhiteSpace(detectedWaylandDisplay))
        {
            return true;
        }

        return string.Equals(Environment.GetEnvironmentVariable("XDG_SESSION_TYPE"), "wayland", StringComparison.OrdinalIgnoreCase);
    }

    private static string GetDiagnosticValue(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? "<unset>" : value;
    }
}

internal readonly record struct LinuxDrmOptions(string DevicePath, int ConnectorId, float Dpi, DrmView.DrmModePreference ModePreference);
