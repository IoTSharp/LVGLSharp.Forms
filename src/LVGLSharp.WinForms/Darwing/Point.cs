namespace LVGLSharp.Darwing
{
    public readonly struct Point : System.IEquatable<Point>
    {
        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }

        public int X { get; }
        public int Y { get; }

        public static Point Empty => new Point(0, 0);

        public bool Equals(Point other) => X == other.X && Y == other.Y;

        public override bool Equals(object? obj) => obj is Point other && Equals(other);

        public override int GetHashCode() => System.HashCode.Combine(X, Y);

        public static bool operator ==(Point left, Point right) => left.Equals(right);

        public static bool operator !=(Point left, Point right) => !left.Equals(right);
    }
}
