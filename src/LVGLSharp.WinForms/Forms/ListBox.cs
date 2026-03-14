using LVGLSharp.Interop;

namespace LVGLSharp.Forms
{
    public class ListBox : Control
    {
        public SelectionMode SelectionMode { get; set; } = SelectionMode.One;
        public DrawMode DrawMode { get; set; } = DrawMode.Normal;
        public bool IntegralHeight { get; set; } = true;
        public int ItemHeight { get; set; }

        internal override unsafe void CreateLvglObject(nint parentHandle)
        {
            _lvglObjectHandle = (nint)lv_list_create((lv_obj_t*)parentHandle);
            ApplyLvglProperties();
            CreateChildrenLvglObjects();
        }
    }
}
