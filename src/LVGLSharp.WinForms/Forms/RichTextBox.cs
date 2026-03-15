using LVGLSharp.Interop;

namespace LVGLSharp.Forms
{
    /// <summary>Rich text editor backed by an LVGL textarea widget.</summary>
    public class RichTextBox : Control
    {
        public bool Multiline { get; set; } = true;
        public bool WordWrap { get; set; } = true;
        public bool ReadOnly { get; set; }
        public bool DetectUrls { get; set; }
        /// <remarks>RTF content is not supported; only plain text from the <see cref="Control.Text"/> property is displayed.</remarks>
        public string? Rtf { get; set; }
        /// <remarks>Scrollbar visibility is not directly configurable; LVGL textarea manages its own scrollbars.</remarks>
        public ScrollBars ScrollBars { get; set; } = ScrollBars.Both;

        internal override unsafe void CreateLvglObject(nint parentHandle)
        {
            _lvglObjectHandle = (nint)lv_textarea_create((lv_obj_t*)parentHandle);
            var obj = (lv_obj_t*)_lvglObjectHandle;
            lv_textarea_set_one_line(obj, false);
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
            Application.CurrentStyleSet.RichTextBox.Apply(obj);
            ApplyLvglProperties();
            CreateChildrenLvglObjects();
        }
    }
}
