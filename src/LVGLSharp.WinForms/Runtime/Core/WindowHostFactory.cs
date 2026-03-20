namespace LVGLSharp
{
    internal static class WindowHostFactory
    {
        private static Func<WindowCreateOptions, IWindow>? s_factory;

        internal static bool IsRegistered => s_factory is not null;

        internal static void Register(Func<string, int, int, IWindow> factory)
        {
            ArgumentNullException.ThrowIfNull(factory);

            Register(options => factory(options.Title, options.Width, options.Height));
        }

        internal static void Register(Func<WindowCreateOptions, IWindow> factory)
        {
            ArgumentNullException.ThrowIfNull(factory);

            s_factory = factory;
        }

        internal static IWindow Create(string title, int width, int height)
        {
            return Create(new WindowCreateOptions(title, width, height));
        }

        internal static IWindow Create(WindowCreateOptions options)
        {
            if (s_factory is null)
            {
                PlatformRuntimeRegistration.EnsureCurrentPlatformRegistered();
            }

            return s_factory?.Invoke(options)
                ?? throw new InvalidOperationException("No LVGLSharp runtime has been configured. Call `Application.UseRuntime(...)` before `Application.Run(...)`.");
        }
    }

    internal static class RuntimeInputState
    {
        private static Func<uint>? s_currentMouseButtonProvider;
        private static bool s_registered;

        internal static bool IsRegistered => s_registered;

        internal static void RegisterCurrentMouseButtonProvider(Func<uint>? currentMouseButtonProvider)
        {
            s_currentMouseButtonProvider = currentMouseButtonProvider;
            s_registered = true;
        }

        internal static uint GetCurrentMouseButton()
        {
            if (!s_registered)
            {
                PlatformRuntimeRegistration.EnsureCurrentPlatformRegistered();
            }

            return s_currentMouseButtonProvider?.Invoke() ?? 0;
        }
    }
}
