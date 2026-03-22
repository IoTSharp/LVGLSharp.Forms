using System;
using LVGLSharp.Drawing;
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
            Application.CurrentStyleSet.Label.Apply(obj);
            if (TextAlign != 0)
            {
                lv_obj_set_style_text_align(obj, ToLvglTextAlign(TextAlign), 0);
            }
            ApplyPassiveLvglProperties(obj);
            CreateChildrenLvglObjects();
        }

        protected override unsafe void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);

            if (_lvglObjectHandle == nint.Zero)
            {
                return;
            }

            fixed (byte* ptr = ToUtf8(Text))
            {
                lv_label_set_text((lv_obj_t*)_lvglObjectHandle, ptr);
            }
        }

        private static lv_text_align_t ToLvglTextAlign(ContentAlignment alignment)
        {
            return alignment switch
            {
                ContentAlignment.TopCenter or ContentAlignment.MiddleCenter or ContentAlignment.BottomCenter => LV_TEXT_ALIGN_CENTER,
                ContentAlignment.TopRight or ContentAlignment.MiddleRight or ContentAlignment.BottomRight => LV_TEXT_ALIGN_RIGHT,
                _ => LV_TEXT_ALIGN_LEFT,
            };
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
