namespace LVGLSharp.Darwing
{
    public readonly struct Rectangle : System.IEquatable<Rectangle>
    {
        public Rectangle(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public int X { get; }
        public int Y { get; }
        public int Width { get; }
        public int Height { get; }

        public int Left => X;
        public int Top => Y;
        public int Right => X + Width;
        public int Bottom => Y + Height;

        public bool Equals(Rectangle other) => X == other.X && Y == other.Y && Width == other.Width && Height == other.Height;

        public override bool Equals(object? obj) => obj is Rectangle other && Equals(other);

        public override int GetHashCode() => System.HashCode.Combine(X, Y, Width, Height);

        public static bool operator ==(Rectangle left, Rectangle right) => left.Equals(right);

        public static bool operator !=(Rectangle left, Rectangle right) => !left.Equals(right);
    }
}
