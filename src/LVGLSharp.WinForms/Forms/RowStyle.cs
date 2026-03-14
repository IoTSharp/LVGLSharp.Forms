namespace LVGLSharp.Forms
{
    public class RowStyle : TableLayoutStyle
    {
        private float _height;

        public RowStyle()
        {
        }

        public RowStyle(SizeType sizeType, float height)
        {
            SizeType = sizeType;
            _height = height;
        }

        public float Height => _height;
    }
}
