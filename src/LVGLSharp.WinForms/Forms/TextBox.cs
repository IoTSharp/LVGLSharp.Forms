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
            if (Form.SendTextAreaFocusCb != null)
            {
                lv_obj_add_event_cb(obj, Form.SendTextAreaFocusCb, lv_event_code_t.LV_EVENT_FOCUSED, null);
            }

            if (Form.key_inputGroup != null)
            {
                lv_group_add_obj(Form.key_inputGroup, obj);
            }

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