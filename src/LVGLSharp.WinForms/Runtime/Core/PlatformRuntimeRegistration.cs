using System.Reflection;
using System.Runtime.ExceptionServices;
using LVGLSharp.Drawing;

namespace LVGLSharp
{
    internal static class PlatformRuntimeRegistration
    {
        private const string WindowsRuntimeAssemblyName = "LVGLSharp.Runtime.Windows";
        private const string LinuxRuntimeAssemblyName = "LVGLSharp.Runtime.Linux";
        private const string WindowsWindowTypeName = "LVGLSharp.Runtime.Windows.Win32Window";
        private const string LinuxWindowTypeName = "LVGLSharp.Runtime.Linux.LinuxView";
        private const string WindowsImageSourceTypeName = "LVGLSharp.Runtime.Windows.WindowsImageSource";
        private const string LinuxImageSourceTypeName = "LVGLSharp.Runtime.Linux.LinuxImageSource";
        private const string WindowsCurrentMouseButtonPropertyName = "CurrentMouseButton";
        private static readonly Lock s_syncLock = new();

        internal static void EnsureCurrentPlatformRegistered()
        {
            if (WindowHostFactory.IsRegistered && Image.IsFactoryRegistered && RuntimeInputState.IsRegistered)
            {
                return;
            }

            lock (s_syncLock)
            {
                if (WindowHostFactory.IsRegistered && Image.IsFactoryRegistered && RuntimeInputState.IsRegistered)
                {
                    return;
                }

                if (OperatingSystem.IsWindows())
                {
                    RegisterWindowsRuntime();
                    return;
                }

                if (OperatingSystem.IsLinux())
                {
                    RegisterLinuxRuntime();
                    return;
                }
            }

            throw new PlatformNotSupportedException("LVGLSharp.Forms currently supports only Windows and Linux runtimes.");
        }

        internal static void RegisterWindowsRuntime()
        {
            WindowHostFactory.Register(CreateWindowsWindow);
            RuntimeInputState.RegisterCurrentMouseButtonProvider(CreateWindowsMouseButtonProvider());
            Image.RegisterFactory(CreateWindowsImageSource);
        }

        internal static void RegisterLinuxRuntime()
        {
            WindowHostFactory.Register(CreateLinuxWindow);
            RuntimeInputState.RegisterCurrentMouseButtonProvider(null);
            Image.RegisterFactory(CreateLinuxImageSource);
        }

        private static IWindow CreateWindowsWindow(string title, int width, int height)
        {
            var runtimeType = GetRequiredRuntimeType(WindowsRuntimeAssemblyName, WindowsWindowTypeName, "Windows runtime window");

            return CreateWindow(runtimeType, title, (uint)width, (uint)height);
        }

        private static IWindow CreateLinuxWindow(string title, int width, int height)
        {
            var runtimeType = GetRequiredRuntimeType(LinuxRuntimeAssemblyName, LinuxWindowTypeName, "Linux runtime window");

            return CreateWindow(runtimeType);
        }

        private static IWindow CreateWindow(Type runtimeType, params object[] arguments)
        {
            try
            {
                return (IWindow)(Activator.CreateInstance(runtimeType, arguments)
                    ?? throw new InvalidOperationException($"The runtime type '{runtimeType.FullName}' could not be instantiated."));
            }
            catch (TargetInvocationException ex) when (ex.InnerException is not null)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                throw;
            }
        }

        private static Func<uint> CreateWindowsMouseButtonProvider()
        {
            var runtimeType = GetRequiredRuntimeType(WindowsRuntimeAssemblyName, WindowsWindowTypeName, "Windows runtime mouse state provider");
            var currentMouseButtonProperty = runtimeType.GetProperty(WindowsCurrentMouseButtonPropertyName, BindingFlags.Public | BindingFlags.Static)
                ?? throw new InvalidOperationException($"The runtime type '{runtimeType.FullName}' does not expose the public static '{WindowsCurrentMouseButtonPropertyName}' property.");

            return () => (uint)(currentMouseButtonProperty.GetValue(null) ?? 0u);
        }

        private static IImageSource CreateWindowsImageSource(string path)
        {
            return ReflectionImageSource.Create(GetRequiredRuntimeType(WindowsRuntimeAssemblyName, WindowsImageSourceTypeName, "Windows runtime image source"), path);
        }

        private static IImageSource CreateLinuxImageSource(string path)
        {
            return ReflectionImageSource.Create(GetRequiredRuntimeType(LinuxRuntimeAssemblyName, LinuxImageSourceTypeName, "Linux runtime image source"), path);
        }

        private static Type GetRequiredRuntimeType(string assemblyName, string typeName, string capability)
        {
            var runtimeType = Type.GetType($"{typeName}, {assemblyName}", throwOnError: false);
            if (runtimeType is not null)
            {
                return runtimeType;
            }

            throw new InvalidOperationException($"The {capability} could not be resolved. Ensure the '{assemblyName}' assembly is referenced by the application.");
        }
    }
}