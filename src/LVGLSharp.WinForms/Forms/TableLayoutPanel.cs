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

        public void SetColumnSpan(Control control, int span)
        {
        }

        internal override unsafe void CreateLvglObject(nint parentHandle)
        {
            _lvglObjectHandle = (nint)lv_obj_create((lv_obj_t*)parentHandle);
            var obj = (lv_obj_t*)_lvglObjectHandle;
            lv_obj_set_style_pad_all(obj, 0, 0);
            ApplyLvglProperties();
            CreateChildrenLvglObjects();
        }
    }
}
