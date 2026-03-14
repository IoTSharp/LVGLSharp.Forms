namespace LVGLSharp.Darwing
{
    public readonly struct Color : System.IEquatable<Color>
    {
        public Color(byte r, byte g, byte b, byte a = 255)
        {
            A = a;
            R = r;
            G = g;
            B = b;
        }

        public byte A { get; }
        public byte R { get; }
        public byte G { get; }
        public byte B { get; }

        public static Color Empty => new Color(0, 0, 0, 0);

        public bool Equals(Color other) => A == other.A && R == other.R && G == other.G && B == other.B;

        public override bool Equals(object? obj) => obj is Color other && Equals(other);

        public override int GetHashCode() => System.HashCode.Combine(A, R, G, B);

        public static bool operator ==(Color left, Color right) => left.Equals(right);

        public static bool operator !=(Color left, Color right) => !left.Equals(right);
    }
}
