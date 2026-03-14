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
        public string? Rtf { get; set; }
        public ScrollBars ScrollBars { get; set; } = ScrollBars.Both;

        internal override unsafe void CreateLvglObject(nint parentHandle)
        {
            _lvglObjectHandle = (nint)lv_textarea_create((lv_obj_t*)parentHandle);
            var obj = (lv_obj_t*)_lvglObjectHandle;
            lv_textarea_set_one_line(obj, false);
            if (!string.IsNullOrEmpty(Text))
            {
                fixed (byte* ptr = ToUtf8(Text))
                    lv_textarea_set_text(obj, ptr);
            }
            ApplyLvglProperties();
            CreateChildrenLvglObjects();
        }
    }
}
