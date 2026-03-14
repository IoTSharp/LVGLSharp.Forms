using LVGLSharp.Interop;

namespace LVGLSharp.Forms
{
    /// <summary>A numeric up/down spinner backed by the LVGL spinbox widget.</summary>
    public class NumericUpDown : Control
    {
        private decimal _value = 0;
        private decimal _minimum = 0;
        private decimal _maximum = 100;
        private int _decimalPlaces = 0;

        public decimal Value
        {
            get => _value;
            set => _value = value;
        }

        public decimal Minimum
        {
            get => _minimum;
            set => _minimum = value;
        }

        public decimal Maximum
        {
            get => _maximum;
            set => _maximum = value;
        }

        public int DecimalPlaces
        {
            get => _decimalPlaces;
            set => _decimalPlaces = value;
        }

        public decimal Increment { get; set; } = 1;
        /// <remarks>Thousands separator formatting is not applied to the LVGL spinbox widget.</remarks>
        public bool ThousandsSeparator { get; set; }
        /// <remarks>Text alignment is not applied to the LVGL spinbox widget.</remarks>
        public HorizontalAlignment TextAlign { get; set; } = HorizontalAlignment.Right;

        internal override unsafe void CreateLvglObject(nint parentHandle)
        {
            _lvglObjectHandle = (nint)lv_spinbox_create((lv_obj_t*)parentHandle);
            ApplyLvglProperties();
            CreateChildrenLvglObjects();
        }
    }
}
