using LVGLSharp.Interop;

namespace LVGLSharp.Forms
{
    public class CheckBox : Control
    {
        private bool? _useVisualStyleBackColor;

        public bool UseVisualStyleBackColor
        {
            get => _useVisualStyleBackColor ?? Application.VisualStylesEnabled;
            set => _useVisualStyleBackColor = value;
        }

        public bool Checked { get; set; }

        internal override unsafe void CreateLvglObject(nint parentHandle)
        {
            _lvglObjectHandle = (nint)lv_checkbox_create((lv_obj_t*)parentHandle);
            var obj = (lv_obj_t*)_lvglObjectHandle;
            if (!string.IsNullOrEmpty(Text))
            {
                fixed (byte* ptr = ToUtf8(Text))
                    lv_checkbox_set_text(obj, ptr);
            }
            if (Checked)
                lv_obj_add_state(obj, LV_STATE_CHECKED);
            Application.GetStyleSet(UseVisualStyleBackColor).CheckBox.Apply(obj);
            ApplyLvglProperties();
            CreateChildrenLvglObjects();
        }
    }
}