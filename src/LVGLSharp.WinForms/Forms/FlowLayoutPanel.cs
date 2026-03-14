using LVGLSharp.Interop;

namespace LVGLSharp.Forms
{
    public class FlowLayoutPanel : Control
    {
        internal void PerformFlowLayout()
        {
            if (Controls.Count == 0)
            {
                return;
            }

            var preserveDesignerLayout = false;
            foreach (var child in Controls)
            {
                if (child.Location.X != 0 || child.Location.Y != 0)
                {
                    preserveDesignerLayout = true;
                    break;
                }
            }

            if (preserveDesignerLayout)
            {
                return;
            }

            var availableWidth = ClientSize.Width > 0 ? ClientSize.Width : Size.Width;
            if (availableWidth <= 0)
            {
                return;
            }

            var x = Padding.Left;
            var y = Padding.Top;
            var lineHeight = 0;

            foreach (var child in Controls)
            {
                var childWidth = child.Size.Width;
                var childHeight = child.Size.Height;
                var margin = child.Margin;

                if (x > Padding.Left && x + margin.Left + childWidth + margin.Right > availableWidth - Padding.Right)
                {
                    x = Padding.Left;
                    y += lineHeight;
                    lineHeight = 0;
                }

                child.SetBounds(
                    x + margin.Left,
                    y + margin.Top,
                    childWidth,
                    childHeight,
                    BoundsSpecified.All);

                x += margin.Left + childWidth + margin.Right;
                lineHeight = Math.Max(lineHeight, margin.Top + childHeight + margin.Bottom);
            }
        }

        internal override unsafe void CreateLvglObject(nint parentHandle)
        {
            _lvglObjectHandle = (nint)lv_obj_create((lv_obj_t*)parentHandle);
            var obj = (lv_obj_t*)_lvglObjectHandle;
            lv_obj_set_style_pad_all(obj, 0, 0);
            ApplyLvglProperties();
            PerformFlowLayout();
            CreateChildrenLvglObjects();
        }
    }
}