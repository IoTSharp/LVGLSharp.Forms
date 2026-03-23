namespace LVGLSharp.Runtime.MacOs;

public sealed class MacOsFrameBuffer
{
    public MacOsFrameBuffer(int width, int height)
    {
        if (width <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(width), width, "width 必须大于 0。");
        }

        if (height <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(height), height, "height 必须大于 0。");
        }

        Width = width;
        Height = height;
        Argb8888Bytes = GC.AllocateUninitializedArray<byte>(width * height * 4);
    }

    public int Width { get; }

    public int Height { get; }

    public int Stride => Width * 4;

    public byte[] Argb8888Bytes { get; }
}