using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace LVGLSharp.Runtime.Remote;

public static class RemoteFrameEncoder
{
    public static byte[] Encode(RemoteFrame frame, RemoteFrameEncoding encoding)
    {
        ArgumentNullException.ThrowIfNull(frame);

        return encoding switch
        {
            RemoteFrameEncoding.Argb8888 => frame.Argb8888Bytes,
            RemoteFrameEncoding.RawRgba32 => ConvertArgbToRgba(frame.Argb8888Bytes),
            RemoteFrameEncoding.Png => EncodePng(frame),
            _ => throw new ArgumentOutOfRangeException(nameof(encoding), encoding, "不支持的帧编码格式。"),
        };
    }

    private static byte[] ConvertArgbToRgba(byte[] argbBytes)
    {
        var rgbaBytes = GC.AllocateUninitializedArray<byte>(argbBytes.Length);
        for (var i = 0; i < argbBytes.Length; i += 4)
        {
            rgbaBytes[i] = argbBytes[i + 1];
            rgbaBytes[i + 1] = argbBytes[i + 2];
            rgbaBytes[i + 2] = argbBytes[i + 3];
            rgbaBytes[i + 3] = argbBytes[i];
        }

        return rgbaBytes;
    }

    private static byte[] EncodePng(RemoteFrame frame)
    {
        using var image = new Image<Rgba32>(frame.Width, frame.Height);
        image.ProcessPixelRows(accessor =>
        {
            var bytes = frame.Argb8888Bytes;
            for (var y = 0; y < frame.Height; y++)
            {
                var row = accessor.GetRowSpan(y);
                var rowOffset = y * frame.Width * 4;
                for (var x = 0; x < frame.Width; x++)
                {
                    var pixelOffset = rowOffset + x * 4;
                    row[x] = new Rgba32(
                        bytes[pixelOffset + 1],
                        bytes[pixelOffset + 2],
                        bytes[pixelOffset + 3],
                        bytes[pixelOffset]);
                }
            }
        });

        using var stream = new MemoryStream();
        image.SaveAsPng(stream);
        return stream.ToArray();
    }
}