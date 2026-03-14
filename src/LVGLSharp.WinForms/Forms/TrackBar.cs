using LVGLSharp.Interop;

namespace LVGLSharp.Forms
{
    public class TrackBar : Control
    {
        private int _minimum = 0;
        private int _maximum = 10;
        private int _value = 0;

        public int Minimum
        {
            get => _minimum;
            set
            {
                _minimum = value;
                UpdateLvglValue();
            }
        }

        public int Maximum
        {
            get => _maximum;
            set
            {
                _maximum = value;
                UpdateLvglValue();
            }
        }

        public int Value
        {
            get => _value;
            set
            {
                _value = value;
                UpdateLvglValue();
            }
        }

        public Orientation Orientation { get; set; } = Orientation.Horizontal;
        public int TickFrequency { get; set; } = 1;
        public TickStyle TickStyle { get; set; } = TickStyle.BottomRight;
        public int SmallChange { get; set; } = 1;
        public int LargeChange { get; set; } = 5;

        private unsafe void UpdateLvglValue()
        {
            if (_lvglObjectHandle == 0) return;
            var obj = (lv_obj_t*)_lvglObjectHandle;
            lv_slider_set_range(obj, _minimum, _maximum);
            lv_slider_set_value(obj, _value, LV_ANIM_OFF);
        }

        internal override unsafe void CreateLvglObject(nint parentHandle)
        {
            _lvglObjectHandle = (nint)lv_slider_create((lv_obj_t*)parentHandle);
            var obj = (lv_obj_t*)_lvglObjectHandle;
            lv_slider_set_range(obj, _minimum, _maximum);
            lv_slider_set_value(obj, _value, LV_ANIM_OFF);
            ApplyLvglProperties();
            CreateChildrenLvglObjects();
        }
    }
}
