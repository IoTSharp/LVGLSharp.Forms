using LVGLSharp.Interop;

namespace LVGLSharp.Forms
{
    public class TextBox : Control
    {
        public bool Multiline { get; set; }
        public string? PlaceholderText { get; set; }

        internal override unsafe void CreateLvglObject(nint parentHandle)
        {
            _lvglObjectHandle = (nint)lv_textarea_create((lv_obj_t*)parentHandle);
            var obj = (lv_obj_t*)_lvglObjectHandle;
            lv_textarea_set_one_line(obj, !Multiline);
            if (!string.IsNullOrEmpty(Text))
            {
                fixed (byte* ptr = ToUtf8(Text))
                    lv_textarea_set_text(obj, ptr);
            }
            if (!string.IsNullOrEmpty(PlaceholderText))
            {
                fixed (byte* ptr = ToUtf8(PlaceholderText))
                    lv_textarea_set_placeholder_text(obj, ptr);
            }
            Application.CurrentStyleSet.TextBox.Apply(obj);
            ApplyLvglProperties();
            CreateChildrenLvglObjects();
        }
    }
}