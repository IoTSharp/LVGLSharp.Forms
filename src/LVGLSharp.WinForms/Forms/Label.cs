using LVGLSharp.Interop;

namespace LVGLSharp.Forms
{
    public class Label : Control
    {
        public ContentAlignment TextAlign { get; set; }

        internal override unsafe void CreateLvglObject(nint parentHandle)
        {
            _lvglObjectHandle = (nint)lv_label_create((lv_obj_t*)parentHandle);
            var obj = (lv_obj_t*)_lvglObjectHandle;
            fixed (byte* ptr = ToUtf8(Text))
                lv_label_set_text(obj, ptr);
            Application.CurrentStyleSet.Label.Apply(obj);
            if (TextAlign != 0)
            {
                lv_obj_set_style_text_align(obj, ToLvglTextAlign(TextAlign), 0);
            }
            ApplyLvglProperties();
            CreateChildrenLvglObjects();
        }

        private static lv_text_align_t ToLvglTextAlign(ContentAlignment alignment)
        {
            return alignment switch
            {
                ContentAlignment.TopCenter or ContentAlignment.MiddleCenter or ContentAlignment.BottomCenter => LV_TEXT_ALIGN_CENTER,
                ContentAlignment.TopRight or ContentAlignment.MiddleRight or ContentAlignment.BottomRight => LV_TEXT_ALIGN_RIGHT,
                _ => LV_TEXT_ALIGN_LEFT,
            };
        }
    }
}