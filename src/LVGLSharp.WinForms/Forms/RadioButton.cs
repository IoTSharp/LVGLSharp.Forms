using LVGLSharp.Interop;

namespace LVGLSharp.Forms
{
    public class RadioButton : Control
    {
        private bool? _useVisualStyleBackColor;
        private bool _checked;

        public bool UseVisualStyleBackColor
        {
            get => _useVisualStyleBackColor ?? Application.VisualStylesEnabled;
            set => _useVisualStyleBackColor = value;
        }

        public bool Checked
        {
            get => _checked;
            set
            {
                if (_checked == value)
                {
                    return;
                }

                _checked = value;
                UpdateLvglState();
            }
        }
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
            UpdateLvglState();
            Application.GetStyleSet(UseVisualStyleBackColor).RadioButton.Apply(obj);
            ApplyLvglProperties();
            CreateChildrenLvglObjects();
        }

        protected override void OnClick(EventArgs e)
        {
            if (!Checked)
            {
                foreach (var sibling in Parent?.Controls ?? [])
                {
                    if (sibling is RadioButton radioButton && !ReferenceEquals(radioButton, this))
                    {
                        radioButton.Checked = false;
                    }
                }

                Checked = true;
            }

            base.OnClick(e);
        }

        protected override unsafe void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);

            if (_lvglObjectHandle == nint.Zero)
            {
                return;
            }

            fixed (byte* ptr = ToUtf8(Text))
            {
                lv_checkbox_set_text((lv_obj_t*)_lvglObjectHandle, ptr);
            }
        }

        private unsafe void UpdateLvglState()
        {
            if (_lvglObjectHandle == nint.Zero)
            {
                return;
            }

            var obj = (lv_obj_t*)_lvglObjectHandle;
            if (_checked)
            {
                lv_obj_add_state(obj, lv_state_t.LV_STATE_CHECKED);
            }
            else
            {
                lv_obj_clear_state(obj, lv_state_t.LV_STATE_CHECKED);
            }
        }
    }
}
