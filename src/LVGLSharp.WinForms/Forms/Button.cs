using LVGLSharp.Interop;

namespace LVGLSharp.Forms
{
    public class Button : Control
    {
        public bool UseVisualStyleBackColor { get; set; }

        internal override unsafe void CreateLvglObject(nint parentHandle)
        {
            _lvglObjectHandle = (nint)lv_button_create((lv_obj_t*)parentHandle);
            var labelObj = lv_label_create((lv_obj_t*)_lvglObjectHandle);
            lv_obj_center(labelObj);
            fixed (byte* ptr = ToUtf8(Text))
                lv_label_set_text(labelObj, ptr);
            ApplyLvglProperties();
            CreateChildrenLvglObjects();
        }
    }
}