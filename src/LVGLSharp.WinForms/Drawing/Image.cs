namespace LVGLSharp.Drawing
{
    public sealed class Image : IDisposable
    {
        private static Func<string, IImageSource>? s_factory;
        private IImageSource? _source;

        private Image(IImageSource source)
        {
            ArgumentNullException.ThrowIfNull(source);

            _source = source;
        }

        public int Width
        {
            get
            {
                ThrowIfDisposed();

                return _source!.Width;
            }
        }

        public int Height
        {
            get
            {
                ThrowIfDisposed();

                return _source!.Height;
            }
        }

        /// <summary>
        /// Registers the image factory used by <see cref="Load(string)"/> and <see cref="FromFile(string)"/>.
        /// </summary>
        /// <param name="factory">The image factory.</param>
        public static void RegisterFactory(Func<string, IImageSource> factory)
        {
            ArgumentNullException.ThrowIfNull(factory);

            s_factory = factory;
        }

        public static Image Load(string path)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(path);

            return FromFile(path);
        }

        public static Image FromFile(string path)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(path);

            return new Image(CreateSource(path));
        }

        internal static bool IsFactoryRegistered => s_factory is not null;

        internal byte[] ToLvglArgb8888Bytes()
        {
            ThrowIfDisposed();

            return _source!.ToLvglArgb8888Bytes();
        }

        private void ThrowIfDisposed()
        {
            if (_source is null)
            {
                throw new ObjectDisposedException(nameof(Image));
            }
        }

        private static IImageSource CreateSource(string path)
        {
            if (s_factory is null)
            {
                PlatformRuntimeRegistration.EnsureCurrentPlatformRegistered();
            }

            return s_factory?.Invoke(path)
                ?? throw new InvalidOperationException("No LVGLSharp image runtime has been configured. Reference the matching runtime assembly or call the matching runtime setup method before loading images.");
        }

        public void Dispose()
        {
            _source?.Dispose();
            _source = null;
        }
    }
}