using LVGLSharp.Interop;

namespace LVGLSharp.Forms
{
    public class RadioButton : Control
    {
        private bool? _useVisualStyleBackColor;

        public bool UseVisualStyleBackColor
        {
            get => _useVisualStyleBackColor ?? Application.VisualStylesEnabled;
            set => _useVisualStyleBackColor = value;
        }

        public bool Checked { get; set; }
        /// <remarks>Check alignment is not applied; LVGL checkbox always places the indicator to the left of the label.</remarks>
        public ContentAlignment CheckAlign { get; set; }

        internal override unsafe void CreateLvglObject(nint parentHandle)
        {
            // LVGL does not have a native radio button widget; simulate with a checkbox
            _lvglObjectHandle = (nint)lv_checkbox_create((lv_obj_t*)parentHandle);
            var obj = (lv_obj_t*)_lvglObjectHandle;
            if (!string.IsNullOrEmpty(Text))
            {
                fixed (byte* ptr = ToUtf8(Text))
                    lv_checkbox_set_text(obj, ptr);
            }
            if (Checked)
                lv_obj_add_state(obj, LV_STATE_CHECKED);
            Application.GetStyleSet(UseVisualStyleBackColor).RadioButton.Apply(obj);
            ApplyLvglProperties();
            CreateChildrenLvglObjects();
        }
    }
}
