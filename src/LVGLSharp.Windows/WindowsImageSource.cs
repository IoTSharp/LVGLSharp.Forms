using System.Runtime.InteropServices;

namespace LVGLSharp.Runtime.Windows;

/// <summary>
/// Represents a Windows-backed image source used by `LVGLSharp.Forms`.
/// </summary>
public sealed class WindowsImageSource : IDisposable
{
    private System.Drawing.Image? _image;

    /// <summary>
    /// Loads an image from the specified file path.
    /// </summary>
    /// <param name="path">The file path of the image to load.</param>
    public WindowsImageSource(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

#pragma warning disable CA1416
        _image = System.Drawing.Image.FromFile(path);
#pragma warning restore CA1416
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

#pragma warning disable CA1416
        using var bitmap = new System.Drawing.Bitmap(_image!.Width, _image.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        using (var graphics = System.Drawing.Graphics.FromImage(bitmap))
        {
            graphics.DrawImage(_image, 0, 0, _image.Width, _image.Height);
        }

        var rectangle = new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height);
        var bitmapData = bitmap.LockBits(rectangle, System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
#pragma warning restore CA1416

        try
        {
            var sourceBytes = GC.AllocateUninitializedArray<byte>(Math.Abs(bitmapData.Stride) * bitmap.Height);
            var bgraBytes = GC.AllocateUninitializedArray<byte>(bitmap.Width * bitmap.Height * 4);

            Marshal.Copy(bitmapData.Scan0, sourceBytes, 0, sourceBytes.Length);

            for (var y = 0; y < bitmap.Height; y++)
            {
                var sourceRow = y * Math.Abs(bitmapData.Stride);
                var destinationRow = y * bitmap.Width * 4;

                Buffer.BlockCopy(sourceBytes, sourceRow, bgraBytes, destinationRow, bitmap.Width * 4);
            }

            return bgraBytes;
        }
        finally
        {
#pragma warning disable CA1416
            bitmap.UnlockBits(bitmapData);
#pragma warning restore CA1416
        }
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
            throw new ObjectDisposedException(nameof(WindowsImageSource));
        }
    }
}