using System.Reflection;
using System.Runtime.ExceptionServices;

namespace LVGLSharp.Drawing
{
    internal sealed class ReflectionImageSource : IImageSource
    {
        private readonly object _instance;
        private readonly PropertyInfo _widthProperty;
        private readonly PropertyInfo _heightProperty;
        private readonly MethodInfo _toLvglArgb8888BytesMethod;

        private ReflectionImageSource(object instance, Type sourceType)
        {
            _instance = instance;
            _widthProperty = sourceType.GetProperty(nameof(Width), BindingFlags.Instance | BindingFlags.Public)
                ?? throw new InvalidOperationException($"The runtime image source type '{sourceType.FullName}' does not expose a public '{nameof(Width)}' property.");
            _heightProperty = sourceType.GetProperty(nameof(Height), BindingFlags.Instance | BindingFlags.Public)
                ?? throw new InvalidOperationException($"The runtime image source type '{sourceType.FullName}' does not expose a public '{nameof(Height)}' property.");
            _toLvglArgb8888BytesMethod = sourceType.GetMethod(nameof(ToLvglArgb8888Bytes), BindingFlags.Instance | BindingFlags.Public, binder: null, types: Type.EmptyTypes, modifiers: null)
                ?? throw new InvalidOperationException($"The runtime image source type '{sourceType.FullName}' does not expose a public '{nameof(ToLvglArgb8888Bytes)}' method.");
        }

        public int Width => (int)(_widthProperty.GetValue(_instance) ?? 0);

        public int Height => (int)(_heightProperty.GetValue(_instance) ?? 0);

        internal static IImageSource Create(Type sourceType, string path)
        {
            ArgumentNullException.ThrowIfNull(sourceType);
            ArgumentException.ThrowIfNullOrWhiteSpace(path);

            try
            {
                var instance = Activator.CreateInstance(sourceType, path)
                    ?? throw new InvalidOperationException($"The runtime image source type '{sourceType.FullName}' could not be instantiated.");

                return new ReflectionImageSource(instance, sourceType);
            }
            catch (TargetInvocationException ex) when (ex.InnerException is not null)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                throw;
            }
        }

        public byte[] ToLvglArgb8888Bytes()
        {
            return (byte[])(_toLvglArgb8888BytesMethod.Invoke(_instance, null)
                ?? throw new InvalidOperationException("The runtime image source did not return image data."));
        }

        public void Dispose()
        {
            if (_instance is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}