using LVGLSharp.Interop;

namespace LVGLSharp.Forms
{
    public class ListBox : Control
    {
        public SelectionMode SelectionMode { get; set; } = SelectionMode.One;
        public DrawMode DrawMode { get; set; } = DrawMode.Normal;
        /// <remarks>Item height customization is not yet applied to the LVGL list widget.</remarks>
        public bool IntegralHeight { get; set; } = true;
        /// <remarks>Item height customization is not yet applied to the LVGL list widget.</remarks>
        public int ItemHeight { get; set; }

        internal override unsafe void CreateLvglObject(nint parentHandle)
        {
            _lvglObjectHandle = (nint)lv_list_create((lv_obj_t*)parentHandle);
            Application.CurrentStyleSet.ListBox.Apply((lv_obj_t*)_lvglObjectHandle);
            ApplyLvglProperties();
            CreateChildrenLvglObjects();
        }
    }
}
