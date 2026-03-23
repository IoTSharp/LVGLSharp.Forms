using LVGLSharp.Drawing;
using SixLabors.ImageSharp.PixelFormats;
using System.Runtime.InteropServices;
using ImageSharp = SixLabors.ImageSharp.Image;
using ImageSharpImage = SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgba32>;

namespace LVGLSharp.Runtime.MacOs;

public sealed class MacOsImageSource : IImageSource
{
    private ImageSharpImage? _image;

    public MacOsImageSource(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        _image = ImageSharp.Load<Rgba32>(path);
    }

    public int Width => _image?.Width ?? throw new ObjectDisposedException(nameof(MacOsImageSource));

    public int Height => _image?.Height ?? throw new ObjectDisposedException(nameof(MacOsImageSource));

    public byte[] ToLvglArgb8888Bytes()
    {
        if (_image is null)
        {
            throw new ObjectDisposedException(nameof(MacOsImageSource));
        }

        var rgbaBytes = GC.AllocateUninitializedArray<byte>(Width * Height * Marshal.SizeOf<Rgba32>());
        var bgraBytes = GC.AllocateUninitializedArray<byte>(rgbaBytes.Length);
        _image.CopyPixelDataTo(MemoryMarshal.Cast<byte, Rgba32>(rgbaBytes.AsSpan()));

        for (var offset = 0; offset < rgbaBytes.Length; offset += 4)
        {
            bgraBytes[offset] = rgbaBytes[offset + 2];
            bgraBytes[offset + 1] = rgbaBytes[offset + 1];
            bgraBytes[offset + 2] = rgbaBytes[offset];
            bgraBytes[offset + 3] = rgbaBytes[offset + 3];
        }

        return bgraBytes;
    }

    public void Dispose()
    {
        _image?.Dispose();
        _image = null;
    }
}