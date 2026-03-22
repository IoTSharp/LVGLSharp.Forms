using System;
using LVGLSharp.Drawing;
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

        /// <summary>
        /// Enables the default LVGL event bridge for this panel. Passive layout containers can disable it to reduce startup cost.
        /// </summary>
        public bool EnableLvglInputEvents { get; set; } = true;

        internal override unsafe void CreateLvglObject(nint parentHandle)
        {
            _lvglObjectHandle = (nint)lv_obj_create((lv_obj_t*)parentHandle);
            var obj = (lv_obj_t*)_lvglObjectHandle;
            Application.CurrentStyleSet.TransparentPanel.Apply(obj);
            if (EnableLvglInputEvents)
            {
                ApplyLvglProperties();
            }
            else
            {
                ApplyPassiveLvglProperties(obj);
            }

            foreach (var child in Controls)
            {
                child.CreateLvglObject(_lvglObjectHandle);
            }
        }

        private unsafe void ApplyPassiveLvglProperties(lv_obj_t* obj)
        {
            int width = Size.Width > 0 ? Size.Width : LV_SIZE_CONTENT;
            int height = Size.Height > 0 ? Size.Height : LV_SIZE_CONTENT;
            lv_obj_set_size(obj, width, height);
            lv_obj_set_pos(obj, Location.X, Location.Y);

            if (BackColor != Color.Empty)
            {
                lv_obj_set_style_bg_opa(obj, (byte)_lv_opacity_level_t.LV_OPA_COVER, 0);
                lv_obj_set_style_bg_color(obj, lv_color_make(BackColor.R, BackColor.G, BackColor.B), 0);
            }

            if (ForeColor != Color.Empty)
            {
                lv_obj_set_style_text_color(obj, lv_color_make(ForeColor.R, ForeColor.G, ForeColor.B), 0);
            }

            lv_obj_remove_flag(obj, LV_OBJ_FLAG_SCROLLABLE | LV_OBJ_FLAG_SCROLL_ELASTIC | LV_OBJ_FLAG_SCROLL_MOMENTUM | LV_OBJ_FLAG_SCROLL_CHAIN);
            lv_obj_set_scrollbar_mode(obj, LV_SCROLLBAR_MODE_OFF);

            if (!Visible)
            {
                lv_obj_add_flag(obj, LV_OBJ_FLAG_HIDDEN);
            }

            OnHandleCreated(EventArgs.Empty);
        }
    }
}
