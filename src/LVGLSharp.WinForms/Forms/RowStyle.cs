namespace LVGLSharp.Forms
{
    public class RowStyle : TableLayoutStyle
    {
        private SizeType _sizeType;
        private float _height;

        public RowStyle()
        {
        }

        public RowStyle(SizeType sizeType, float height)
        {
            _sizeType = sizeType;
            _height = height;
            SizeType = sizeType;
        }

        public float Height => _height;
    }
}
