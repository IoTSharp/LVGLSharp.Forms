using LVGLSharp.Interop;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;

namespace LVGLSharp
{
    public static class LvglNativeLibraryResolver
    {
        private const string NativeLibraryName = "lvgl";
        private static IntPtr _handle;
        private static int _isRegistered;

        /// <summary>
        /// Registers LVGL native library probing from the application output directory.
        /// </summary>
        public static void EnsureRegistered()
        {
            if (Interlocked.Exchange(ref _isRegistered, 1) != 0)
            {
                return;
            }

            NativeLibrary.SetDllImportResolver(typeof(LVGL).Assembly, Resolve);
        }

        private static IntPtr Resolve(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
        {
            if (!string.Equals(libraryName, NativeLibraryName, StringComparison.Ordinal))
            {
                return IntPtr.Zero;
            }

            if (_handle != IntPtr.Zero)
            {
                return _handle;
            }

            foreach (var candidatePath in GetCandidatePaths())
            {
                if (NativeLibrary.TryLoad(candidatePath, out var handle))
                {
                    _handle = handle;
                    return handle;
                }
            }

            if (NativeLibrary.TryLoad(libraryName, assembly, searchPath, out var fallbackHandle))
            {
                _handle = fallbackHandle;
                return fallbackHandle;
            }

            return IntPtr.Zero;
        }

        private static IEnumerable<string> GetCandidatePaths()
        {
            var baseDirectory = AppContext.BaseDirectory;

            foreach (var fileName in GetLibraryFileNames())
            {
                yield return Path.Combine(baseDirectory, fileName);
            }

            foreach (var runtimeIdentifier in GetCandidateRuntimeIdentifiers())
            {
                foreach (var fileName in GetLibraryFileNames())
                {
                    yield return Path.Combine(baseDirectory, "runtimes", runtimeIdentifier, "native", fileName);
                }
            }
        }

        private static IEnumerable<string> GetCandidateRuntimeIdentifiers()
        {
            if (OperatingSystem.IsWindows())
            {
                yield return RuntimeInformation.ProcessArchitecture switch
                {
                    Architecture.X86 => "win-x86",
                    Architecture.Arm64 => "win-arm64",
                    _ => "win-x64"
                };

                yield break;
            }

            if (OperatingSystem.IsLinux())
            {
                yield return RuntimeInformation.ProcessArchitecture switch
                {
                    Architecture.Arm => "linux-arm",
                    Architecture.Arm64 => "linux-arm64",
                    _ => "linux-x64"
                };
            }
        }

        private static IEnumerable<string> GetLibraryFileNames()
        {
            if (OperatingSystem.IsWindows())
            {
                yield return "lvgl.dll";
                yield break;
            }

            if (OperatingSystem.IsLinux())
            {
                // Prefer the versioned SONAME so we reuse the exact same LVGL image
                // already pulled in by liblvgl_host_x11.so and avoid double-loading.
                yield return "liblvgl.so.9";
                yield return "liblvgl.so";
                yield break;
            }

            yield return NativeLibraryName;
        }
    }
}
