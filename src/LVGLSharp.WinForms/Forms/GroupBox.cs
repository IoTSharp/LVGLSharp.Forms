using LVGLSharp.Interop;

namespace LVGLSharp.Forms
{
    /// <summary>A labelled container, rendered as an LVGL object with a title label.</summary>
    public class GroupBox : Control
    {
        internal override unsafe void CreateLvglObject(nint parentHandle)
        {
            _lvglObjectHandle = (nint)lv_obj_create((lv_obj_t*)parentHandle);
            var obj = (lv_obj_t*)_lvglObjectHandle;
            Application.CurrentStyleSet.GroupBox.Apply(obj);
            // Title label at the top of the group box
            if (!string.IsNullOrEmpty(Text))
            {
                var titleLabel = lv_label_create(obj);
                fixed (byte* ptr = ToUtf8(Text))
                    lv_label_set_text(titleLabel, ptr);
                lv_obj_align(titleLabel, LV_ALIGN_TOP_LEFT, 0, 0);
            }
            ApplyLvglProperties();
            CreateChildrenLvglObjects();
        }
    }
}
