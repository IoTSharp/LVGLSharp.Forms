using LVGLSharp.Interop;

namespace LVGLSharp.Forms
{
    public class TableLayoutPanel : Control
    {
        public int ColumnCount { get; set; }
        public List<ColumnStyle> ColumnStyles { get; } = new List<ColumnStyle>();
        public int RowCount { get; set; }
        public List<RowStyle> RowStyles { get; } = new List<RowStyle>();

        // Stores the column/row position for each child added via Add(control, col, row)
        private readonly Dictionary<Control, (int col, int row)> _cellPositions = new();
        private readonly Dictionary<Control, int> _columnSpans = new();
        private readonly Dictionary<Control, int> _rowSpans = new();

        public void SetColumnSpan(Control control, int span)
        {
            ArgumentNullException.ThrowIfNull(control);

            _columnSpans[control] = Math.Max(1, span);
        }

        /// <summary>
        /// Sets the number of rows that a child control spans.
        /// </summary>
        public void SetRowSpan(Control control, int span)
        {
            ArgumentNullException.ThrowIfNull(control);

            _rowSpans[control] = Math.Max(1, span);
        }

        internal void SetCellPosition(Control control, int column, int row)
        {
            ArgumentNullException.ThrowIfNull(control);

            _cellPositions[control] = (Math.Max(0, column), Math.Max(0, row));
        }

        internal void PerformTableLayout()
        {
            var columns = Math.Max(1, ColumnCount > 0 ? ColumnCount : 1);
            var rows = Math.Max(1, RowCount > 0 ? RowCount : 1);
            var totalWidth = Math.Max(0, (ClientSize.Width > 0 ? ClientSize.Width : Size.Width) - Padding.Horizontal);
            var totalHeight = Math.Max(0, (ClientSize.Height > 0 ? ClientSize.Height : Size.Height) - Padding.Vertical);

            var columnWidths = CalculateColumnWidths(columns, totalWidth);
            var rowHeights = CalculateRowHeights(rows, totalHeight);

            var columnOffsets = CalculateOffsets(columnWidths, Padding.Left);
            var rowOffsets = CalculateOffsets(rowHeights, Padding.Top);

            var defaultColumn = 0;
            var defaultRow = 0;

            foreach (var child in Controls)
            {
                var position = _cellPositions.TryGetValue(child, out var cell) ? cell : (col: defaultColumn, row: defaultRow);
                var column = Math.Min(position.col, columns - 1);
                var row = Math.Min(position.row, rows - 1);
                var columnSpan = Math.Min(_columnSpans.TryGetValue(child, out var configuredColumnSpan) ? configuredColumnSpan : 1, columns - column);
                var rowSpan = Math.Min(_rowSpans.TryGetValue(child, out var configuredRowSpan) ? configuredRowSpan : 1, rows - row);
                columnSpan = Math.Max(1, columnSpan);
                rowSpan = Math.Max(1, rowSpan);

                var x = columnOffsets[column];
                var y = rowOffsets[row];
                var width = 0;
                for (var i = 0; i < columnSpan; i++)
                {
                    width += columnWidths[column + i];
                }

                var height = 0;
                for (var i = 0; i < rowSpan; i++)
                {
                    height += rowHeights[row + i];
                }

                var margin = child.Margin;

                if (child.Dock == DockStyle.Fill)
                {
                    child.SetBounds(
                        x + margin.Left,
                        y + margin.Top,
                        Math.Max(0, width - margin.Horizontal),
                        Math.Max(0, height - margin.Vertical),
                        BoundsSpecified.All);
                }
                else
                {
                    child.SetBounds(
                        x + margin.Left,
                        y + margin.Top,
                        child.Size.Width,
                        child.Size.Height,
                        BoundsSpecified.All);
                }

                defaultColumn++;
                if (defaultColumn >= columns)
                {
                    defaultColumn = 0;
                    defaultRow = Math.Min(defaultRow + 1, rows - 1);
                }
            }
        }

        private int[] CalculateColumnWidths(int columns, int totalWidth)
        {
            var widths = new int[columns];
            var remaining = totalWidth;
            var autoColumns = 0;

            for (var i = 0; i < columns; i++)
            {
                var style = i < ColumnStyles.Count ? ColumnStyles[i] : null;
                if (style?.SizeType == SizeType.Absolute)
                {
                    widths[i] = (int)style.Width;
                    remaining -= widths[i];
                }
                else if (style?.SizeType == SizeType.Percent)
                {
                    widths[i] = (int)Math.Round(totalWidth * (style.Width / 100f));
                    remaining -= widths[i];
                }
                else
                {
                    autoColumns++;
                }
            }

            var autoWidth = autoColumns > 0 ? Math.Max(0, remaining) / autoColumns : 0;
            for (var i = 0; i < columns; i++)
            {
                if (widths[i] == 0)
                {
                    widths[i] = autoWidth;
                }
            }

            return widths;
        }

        private int[] CalculateRowHeights(int rows, int totalHeight)
        {
            var heights = new int[rows];
            var remaining = totalHeight;
            var autoRows = 0;

            for (var i = 0; i < rows; i++)
            {
                var style = i < RowStyles.Count ? RowStyles[i] : null;
                if (style?.SizeType == SizeType.Absolute)
                {
                    heights[i] = (int)style.Height;
                    remaining -= heights[i];
                }
                else if (style?.SizeType == SizeType.Percent)
                {
                    heights[i] = (int)Math.Round(totalHeight * (style.Height / 100f));
                    remaining -= heights[i];
                }
                else
                {
                    autoRows++;
                }
            }

            var autoHeight = autoRows > 0 ? Math.Max(0, remaining) / autoRows : 0;
            for (var i = 0; i < rows; i++)
            {
                if (heights[i] == 0)
                {
                    heights[i] = autoHeight;
                }
            }

            return heights;
        }

        private static int[] CalculateOffsets(int[] lengths, int start)
        {
            var offsets = new int[lengths.Length];
            var current = start;
            for (var i = 0; i < lengths.Length; i++)
            {
                offsets[i] = current;
                current += lengths[i];
            }

            return offsets;
        }

        internal override unsafe void CreateLvglObject(nint parentHandle)
        {
            _lvglObjectHandle = (nint)lv_obj_create((lv_obj_t*)parentHandle);
            var obj = (lv_obj_t*)_lvglObjectHandle;
            Application.CurrentStyleSet.LayoutPanel.Apply(obj);
            ApplyLvglProperties();
            PerformTableLayout();

            foreach (var child in Controls)
            {
                child.CreateLvglObject(_lvglObjectHandle);
            }
        }
    }
}
