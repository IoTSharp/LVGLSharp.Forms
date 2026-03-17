using SixLabors.ImageSharp.PixelFormats;
using System.Runtime.InteropServices;
using ImageSharp = SixLabors.ImageSharp.Image;
using ImageSharpImage = SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgba32>;

namespace LVGLSharp.Runtime.Linux;

/// <summary>
/// Represents a Linux-backed image source used by `LVGLSharp.Forms`.
/// </summary>
public sealed class LinuxImageSource : IDisposable
{
    private ImageSharpImage? _image;

    /// <summary>
    /// Loads an image from the specified file path.
    /// </summary>
    /// <param name="path">The file path of the image to load.</param>
    public LinuxImageSource(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        _image = ImageSharp.Load<Rgba32>(path);
    }

    /// <summary>
    /// Gets the pixel width of the image.
    /// </summary>
    public int Width
    {
        get
        {
            ThrowIfDisposed();

            return _image!.Width;
        }
    }

    /// <summary>
    /// Gets the pixel height of the image.
    /// </summary>
    public int Height
    {
        get
        {
            ThrowIfDisposed();

            return _image!.Height;
        }
    }

    /// <summary>
    /// Converts the image into LVGL ARGB8888 byte data.
    /// </summary>
    /// <returns>The converted pixel buffer.</returns>
    public byte[] ToLvglArgb8888Bytes()
    {
        ThrowIfDisposed();

        var rgbaBytes = GC.AllocateUninitializedArray<byte>(Width * Height * Marshal.SizeOf<Rgba32>());
        var bgraBytes = GC.AllocateUninitializedArray<byte>(rgbaBytes.Length);
        _image!.CopyPixelDataTo(MemoryMarshal.Cast<byte, Rgba32>(rgbaBytes.AsSpan()));

        for (var offset = 0; offset < rgbaBytes.Length; offset += 4)
        {
            bgraBytes[offset] = rgbaBytes[offset + 2];
            bgraBytes[offset + 1] = rgbaBytes[offset + 1];
            bgraBytes[offset + 2] = rgbaBytes[offset];
            bgraBytes[offset + 3] = rgbaBytes[offset + 3];
        }

        return bgraBytes;
    }

    /// <summary>
    /// Releases the loaded image.
    /// </summary>
    public void Dispose()
    {
        _image?.Dispose();
        _image = null;
    }

    private void ThrowIfDisposed()
    {
        if (_image is null)
        {
            throw new ObjectDisposedException(nameof(LinuxImageSource));
        }
    }
}