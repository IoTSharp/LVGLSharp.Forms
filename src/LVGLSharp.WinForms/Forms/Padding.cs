namespace LVGLSharp.Forms
{
    public readonly struct Padding
    {
        public Padding(int all)
            : this(all, all, all, all)
        {
        }

        public Padding(int left, int top, int right, int bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }

        public int Left { get; }
        public int Top { get; }
        public int Right { get; }
        public int Bottom { get; }

        public int Horizontal => Left + Right;
        public int Vertical => Top + Bottom;

        public static Padding Empty => new Padding(0);
    }
}