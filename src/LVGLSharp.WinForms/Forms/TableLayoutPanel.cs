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
            var remaining = Math.Max(0, totalWidth);
            var percentColumns = new List<int>();
            var autoColumns = new List<int>();
            var percentWeightTotal = 0f;

            for (var i = 0; i < columns; i++)
            {
                var style = i < ColumnStyles.Count ? ColumnStyles[i] : null;
                if (style?.SizeType == SizeType.Absolute)
                {
                    widths[i] = Math.Max(0, (int)Math.Round(style.Width));
                    remaining -= widths[i];
                }
                else if (style?.SizeType == SizeType.Percent)
                {
                    percentColumns.Add(i);
                    percentWeightTotal += Math.Max(0f, style.Width);
                }
                else
                {
                    autoColumns.Add(i);
                }
            }

            remaining = Math.Max(0, remaining);

            if (percentColumns.Count > 0)
            {
                var remainingForPercent = remaining;
                var weightRemaining = percentWeightTotal > 0f ? percentWeightTotal : percentColumns.Count;

                for (var i = 0; i < percentColumns.Count; i++)
                {
                    var columnIndex = percentColumns[i];
                    var style = columnIndex < ColumnStyles.Count ? ColumnStyles[columnIndex] : null;
                    var weight = percentWeightTotal > 0f ? Math.Max(0f, style?.Width ?? 0f) : 1f;
                    var isLast = i == percentColumns.Count - 1;
                    var width = isLast || weightRemaining <= 0f
                        ? remainingForPercent
                        : (int)Math.Round(remainingForPercent * (weight / weightRemaining));

                    width = Math.Clamp(width, 0, remainingForPercent);
                    widths[columnIndex] = width;
                    remainingForPercent -= width;
                    weightRemaining -= weight;
                }

                remaining = remainingForPercent;
            }

            if (autoColumns.Count == 0)
            {
                return widths;
            }

            var remainingForAuto = Math.Max(0, remaining);
            for (var i = 0; i < autoColumns.Count; i++)
            {
                var columnIndex = autoColumns[i];
                var slotsLeft = autoColumns.Count - i;
                var width = slotsLeft <= 1 ? remainingForAuto : remainingForAuto / slotsLeft;
                width = Math.Max(0, width);
                if (widths[columnIndex] == 0)
                {
                    widths[columnIndex] = width;
                }

                remainingForAuto -= width;
            }

            return widths;
        }

        private int[] CalculateRowHeights(int rows, int totalHeight)
        {
            var heights = new int[rows];
            var remaining = Math.Max(0, totalHeight);
            var percentRows = new List<int>();
            var autoRows = new List<int>();
            var percentWeightTotal = 0f;

            for (var i = 0; i < rows; i++)
            {
                var style = i < RowStyles.Count ? RowStyles[i] : null;
                if (style?.SizeType == SizeType.Absolute)
                {
                    heights[i] = Math.Max(0, (int)Math.Round(style.Height));
                    remaining -= heights[i];
                }
                else if (style?.SizeType == SizeType.Percent)
                {
                    percentRows.Add(i);
                    percentWeightTotal += Math.Max(0f, style.Height);
                }
                else
                {
                    autoRows.Add(i);
                }
            }

            remaining = Math.Max(0, remaining);

            if (percentRows.Count > 0)
            {
                var remainingForPercent = remaining;
                var weightRemaining = percentWeightTotal > 0f ? percentWeightTotal : percentRows.Count;

                for (var i = 0; i < percentRows.Count; i++)
                {
                    var rowIndex = percentRows[i];
                    var style = rowIndex < RowStyles.Count ? RowStyles[rowIndex] : null;
                    var weight = percentWeightTotal > 0f ? Math.Max(0f, style?.Height ?? 0f) : 1f;
                    var isLast = i == percentRows.Count - 1;
                    var height = isLast || weightRemaining <= 0f
                        ? remainingForPercent
                        : (int)Math.Round(remainingForPercent * (weight / weightRemaining));

                    height = Math.Clamp(height, 0, remainingForPercent);
                    heights[rowIndex] = height;
                    remainingForPercent -= height;
                    weightRemaining -= weight;
                }

                remaining = remainingForPercent;
            }

            if (autoRows.Count == 0)
            {
                return heights;
            }

            var remainingForAuto = Math.Max(0, remaining);
            for (var i = 0; i < autoRows.Count; i++)
            {
                var rowIndex = autoRows[i];
                var slotsLeft = autoRows.Count - i;
                var height = slotsLeft <= 1 ? remainingForAuto : remainingForAuto / slotsLeft;
                height = Math.Max(0, height);
                if (heights[rowIndex] == 0)
                {
                    heights[rowIndex] = height;
                }

                remainingForAuto -= height;
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
                LvglCreateTrace.Before(child);
                try
                {
                    child.CreateLvglObject(_lvglObjectHandle);
                }
                finally
                {
                    LvglCreateTrace.After();
                }
            }
        }
    }
}
