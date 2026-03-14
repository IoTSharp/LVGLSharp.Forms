using LVGLSharp.Interop;

namespace LVGLSharp.Forms
{
    /// <summary>
    /// A generic container panel backed by a transparent LVGL container object.
    /// </summary>
    public class Panel : Control
    {
        /// <remarks>Border styling is not currently applied to the underlying LVGL object; the panel is rendered as a transparent container.</remarks>
        public BorderStyle BorderStyle { get; set; }

        internal override unsafe void CreateLvglObject(nint parentHandle)
        {
            _lvglObjectHandle = (nint)lv_obj_create((lv_obj_t*)parentHandle);
            var obj = (lv_obj_t*)_lvglObjectHandle;
            lv_obj_set_style_pad_all(obj, 0, 0);
            // Remove border and background for a transparent panel
            lv_obj_set_style_border_width(obj, 0, 0);
            lv_obj_set_style_bg_opa(obj, 0, 0);
            ApplyLvglProperties();
            CreateChildrenLvglObjects();
        }
    }
}
