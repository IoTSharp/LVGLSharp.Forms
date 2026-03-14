namespace LVGLSharp.Darwing
{
    public readonly struct SizeF : System.IEquatable<SizeF>
    {
        public SizeF(float width, float height)
        {
            Width = width;
            Height = height;
        }

        public float Width { get; }
        public float Height { get; }

        public bool Equals(SizeF other) => Width.Equals(other.Width) && Height.Equals(other.Height);

        public override bool Equals(object? obj) => obj is SizeF other && Equals(other);

        public override int GetHashCode() => System.HashCode.Combine(Width, Height);

        public static bool operator ==(SizeF left, SizeF right) => left.Equals(right);

        public static bool operator !=(SizeF left, SizeF right) => !left.Equals(right);
    }
}
