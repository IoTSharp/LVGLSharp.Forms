using LVGLSharp.Interop;

namespace LVGLSharp.Forms
{
    public class FlowLayoutPanel : Control
    {
        /// <summary>
        /// Keeps legacy behavior where non-zero child coordinates are treated as designer-authored
        /// absolute positions and flow layout is skipped.
        /// </summary>
        public bool PreserveChildLocations { get; set; } = true;

        /// <summary>
        /// true: lay out left-to-right; false: top-to-bottom.
        /// </summary>
        public bool FlowHorizontally { get; set; } = true;

        /// <summary>
        /// Enables wrapping when items exceed the available primary-axis space.
        /// </summary>
        public bool WrapContents { get; set; } = true;

        /// <summary>
        /// Used only for flow progression when a child's Width is zero.
        /// </summary>
        public int AutoMeasureWidth { get; set; }

        /// <summary>
        /// Used only for flow progression when a child's Height is zero.
        /// </summary>
        public int AutoMeasureHeight { get; set; }

        internal void PerformFlowLayout()
        {
            if (Controls.Count == 0)
            {
                return;
            }

            if (PreserveChildLocations)
            {
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
            }

            if (FlowHorizontally)
            {
                PerformHorizontalFlowLayout();
            }
            else
            {
                PerformVerticalFlowLayout();
            }
        }

        private void PerformHorizontalFlowLayout()
        {
            var availableWidth = ClientSize.Width > 0 ? ClientSize.Width : Size.Width;
            if (WrapContents && availableWidth <= 0)
            {
                return;
            }

            var x = Padding.Left;
            var y = Padding.Top;
            var lineHeight = 0;
            var rightLimit = availableWidth - Padding.Right;

            foreach (var child in Controls)
            {
                var actualWidth = child.Size.Width;
                var actualHeight = child.Size.Height;
                var measuredWidth = actualWidth > 0 ? actualWidth : AutoMeasureWidth;
                var measuredHeight = actualHeight > 0 ? actualHeight : AutoMeasureHeight;
                measuredWidth = Math.Max(1, measuredWidth);
                measuredHeight = Math.Max(1, measuredHeight);

                var margin = child.Margin;

                if (WrapContents &&
                    availableWidth > 0 &&
                    x > Padding.Left &&
                    x + margin.Left + measuredWidth + margin.Right > rightLimit)
                {
                    x = Padding.Left;
                    y += lineHeight;
                    lineHeight = 0;
                }

                child.SetBounds(
                    x + margin.Left,
                    y + margin.Top,
                    actualWidth,
                    actualHeight,
                    BoundsSpecified.All);

                x += margin.Left + measuredWidth + margin.Right;
                lineHeight = Math.Max(lineHeight, margin.Top + measuredHeight + margin.Bottom);
            }
        }

        private void PerformVerticalFlowLayout()
        {
            var availableHeight = ClientSize.Height > 0 ? ClientSize.Height : Size.Height;
            if (WrapContents && availableHeight <= 0)
            {
                return;
            }

            var x = Padding.Left;
            var y = Padding.Top;
            var lineWidth = 0;
            var bottomLimit = availableHeight - Padding.Bottom;

            foreach (var child in Controls)
            {
                var actualWidth = child.Size.Width;
                var actualHeight = child.Size.Height;
                var measuredWidth = actualWidth > 0 ? actualWidth : AutoMeasureWidth;
                var measuredHeight = actualHeight > 0 ? actualHeight : AutoMeasureHeight;
                measuredWidth = Math.Max(1, measuredWidth);
                measuredHeight = Math.Max(1, measuredHeight);

                var margin = child.Margin;

                if (WrapContents &&
                    availableHeight > 0 &&
                    y > Padding.Top &&
                    y + margin.Top + measuredHeight + margin.Bottom > bottomLimit)
                {
                    y = Padding.Top;
                    x += lineWidth;
                    lineWidth = 0;
                }

                child.SetBounds(
                    x + margin.Left,
                    y + margin.Top,
                    actualWidth,
                    actualHeight,
                    BoundsSpecified.All);

                y += margin.Top + measuredHeight + margin.Bottom;
                lineWidth = Math.Max(lineWidth, margin.Left + measuredWidth + margin.Right);
            }
        }

        internal override unsafe void CreateLvglObject(nint parentHandle)
        {
            _lvglObjectHandle = (nint)lv_obj_create((lv_obj_t*)parentHandle);
            var obj = (lv_obj_t*)_lvglObjectHandle;
            Application.CurrentStyleSet.LayoutPanel.Apply(obj);
            ApplyLvglProperties();
            PerformFlowLayout();
            CreateChildrenLvglObjects();
        }
    }
}
