namespace LVGLSharp.Darwing
{
    public readonly struct Size : System.IEquatable<Size>
    {
        public Size(int width, int height)
        {
            Width = width;
            Height = height;
        }

        public int Width { get; }
        public int Height { get; }

        public static Size Empty => new Size(0, 0);

        public bool Equals(Size other) => Width == other.Width && Height == other.Height;

        public override bool Equals(object? obj) => obj is Size other && Equals(other);

        public override int GetHashCode() => System.HashCode.Combine(Width, Height);

        public static bool operator ==(Size left, Size right) => left.Equals(right);

        public static bool operator !=(Size left, Size right) => !left.Equals(right);
    }
}
