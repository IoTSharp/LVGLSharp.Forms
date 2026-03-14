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
            ApplyLvglProperties();
            CreateChildrenLvglObjects();
        }
    }
}