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
            var obj = (lv_obj_t*)_lvglObjectHandle;
            if (!string.IsNullOrEmpty(Text))
            {
                fixed (byte* ptr = ToUtf8(Text))
                    lv_dropdown_set_text(obj, ptr);
            }

            var list = lv_dropdown_get_list(obj);
            if (DropDownHeight > 0 && list != null)
            {
                lv_obj_set_style_max_height(list, DropDownHeight, 0);
            }

            Application.CurrentStyleSet.ComboBox.Apply(obj, list);
            ApplyLvglProperties();
            CreateChildrenLvglObjects();
        }
    }
}