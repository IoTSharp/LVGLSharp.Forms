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
        /// <remarks>Not currently applied to the LVGL slider widget.</remarks>
        public int TickFrequency { get; set; } = 1;
        /// <remarks>Not currently applied to the LVGL slider widget.</remarks>
        public TickStyle TickStyle { get; set; } = TickStyle.BottomRight;
        /// <remarks>Not currently applied to the LVGL slider widget.</remarks>
        public int SmallChange { get; set; } = 1;
        /// <remarks>Not currently applied to the LVGL slider widget.</remarks>
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
            // Apply orientation
            var orient = Orientation == Orientation.Vertical
                ? lv_slider_orientation_t.LV_SLIDER_ORIENTATION_VERTICAL
                : lv_slider_orientation_t.LV_SLIDER_ORIENTATION_HORIZONTAL;
            lv_slider_set_orientation(obj, orient);
            ApplyLvglProperties();
            CreateChildrenLvglObjects();
        }
    }
}
