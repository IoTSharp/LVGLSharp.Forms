namespace LVGLSharp.Runtime.Remote;

public sealed record RemoteFrame(int Width, int Height, byte[] Argb8888Bytes)
{
	public int Stride => Width * 4;
}