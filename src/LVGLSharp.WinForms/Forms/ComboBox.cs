using LVGLSharp.Interop;

namespace LVGLSharp.Forms
{
    public class ComboBox : Control
    {
        public bool FormattingEnabled { get; set; }
        public int DropDownHeight { get; set; }
        public object? FlatStyle { get; set; }
        public bool IntegralHeight { get; set; }
        public int ItemHeight { get; set; }

        internal override unsafe void CreateLvglObject(nint parentHandle)
        {
            _lvglObjectHandle = (nint)lv_dropdown_create((lv_obj_t*)parentHandle);
            ApplyLvglProperties();
            CreateChildrenLvglObjects();
        }
    }
}