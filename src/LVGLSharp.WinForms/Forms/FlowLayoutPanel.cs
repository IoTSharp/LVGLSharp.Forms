using LVGLSharp.Interop;

namespace LVGLSharp.Forms
{
    public class FlowLayoutPanel : Control
    {
        internal override unsafe void CreateLvglObject(nint parentHandle)
        {
            _lvglObjectHandle = (nint)lv_obj_create((lv_obj_t*)parentHandle);
            var obj = (lv_obj_t*)_lvglObjectHandle;
            // Horizontal wrapping flex layout, matching WinForms FlowLayoutPanel default
            lv_obj_set_flex_flow(obj, LV_FLEX_FLOW_ROW_WRAP);
            lv_obj_set_style_pad_all(obj, 0, 0);
            lv_obj_set_style_pad_gap(obj, 0, 0);
            ApplyLvglProperties();
            CreateChildrenLvglObjects();
        }
    }
}