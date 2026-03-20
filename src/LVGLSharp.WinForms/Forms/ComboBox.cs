using LVGLSharp.Interop;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace LVGLSharp.Forms
{
    public class ComboBox : Control
    {
        private int _selectedIndex = -1;
        private object? _selectedItem;
        private readonly ObservableCollection<object> _items;
        private ComboBoxStyle _dropDownStyle = ComboBoxStyle.DropDown;

        public ComboBox()
        {
            _items = new ObservableCollection<object>();
            _items.CollectionChanged += Items_CollectionChanged;
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                if (_selectedIndex != value)
                {
                    if (value < -1 || value >= _items.Count)
                    {
                        if (value != -1)
                            throw new ArgumentOutOfRangeException(nameof(value));
                    }

                    _selectedIndex = value;
                    _selectedItem = _selectedIndex >= 0 && _selectedIndex < _items.Count ? _items[_selectedIndex] : null;
                    
                    UpdateLvglSelection();
                    OnSelectedIndexChanged(EventArgs.Empty);
                }
            }
        }

        [Browsable(false)]
        [Bindable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public object? SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (_selectedItem != value)
                {
                    int index = -1;
                    if (value != null)
                    {
                        index = _items.IndexOf(value);
                    }
                    SelectedIndex = index;
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Localizable(true)]
        public ObservableCollection<object> Items => _items;

        public override string Text
        {
            get => base.Text;
            set
            {
                base.Text = value;
                if (_lvglObjectHandle != nint.Zero)
                {
                    UpdateLvglText();
                }
            }
        }

        [DefaultValue(ComboBoxStyle.DropDown)]
        public ComboBoxStyle DropDownStyle
        {
            get => _dropDownStyle;
            set => _dropDownStyle = value;
        }

        public bool FormattingEnabled { get; set; }
        
        [DefaultValue(106)]
        public int DropDownHeight { get; set; } = 106;
        
        public FlatStyle FlatStyle { get; set; }
        public bool IntegralHeight { get; set; }
        public int ItemHeight { get; set; }

        public event EventHandler? SelectedIndexChanged;

        protected virtual void OnSelectedIndexChanged(EventArgs e)
        {
            SelectedIndexChanged?.Invoke(this, e);
        }

        private void Items_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (_lvglObjectHandle != nint.Zero)
            {
                UpdateLvglItems();
            }
        }

        internal override unsafe void CreateLvglObject(nint parentHandle)
        {
            _lvglObjectHandle = (nint)lv_dropdown_create((lv_obj_t*)parentHandle);
            var obj = (lv_obj_t*)_lvglObjectHandle;
            
            UpdateLvglItems();
            UpdateLvglSelection();

            var list = lv_dropdown_get_list(obj);
            if (DropDownHeight > 0 && list != null)
            {
                lv_obj_set_style_max_height(list, DropDownHeight, 0);
            }

            Application.CurrentStyleSet.ComboBox.Apply(obj, list);
            ApplyLvglProperties();
            CreateChildrenLvglObjects();
        }

        private unsafe void UpdateLvglItems()
        {
            if (_lvglObjectHandle == nint.Zero) return;

            var obj = (lv_obj_t*)_lvglObjectHandle;
            
            if (_items.Count > 0)
            {
                string optionsText = string.Join("\n", _items.Select(item => item?.ToString() ?? ""));
                fixed (byte* ptr = ToUtf8(optionsText))
                {
                    lv_dropdown_set_options(obj, ptr);
                }
            }
            else
            {
                lv_dropdown_clear_options(obj);
            }
        }

        private unsafe void UpdateLvglSelection()
        {
            if (_lvglObjectHandle == nint.Zero) return;

            var obj = (lv_obj_t*)_lvglObjectHandle;
            if (_selectedIndex >= 0 && _selectedIndex < _items.Count)
            {
                lv_dropdown_set_selected(obj, (ushort)_selectedIndex);
            }
        }

        private unsafe void UpdateLvglText()
        {
            if (_lvglObjectHandle == nint.Zero) return;

            var obj = (lv_obj_t*)_lvglObjectHandle;
            if (!string.IsNullOrEmpty(Text))
            {
                fixed (byte* ptr = ToUtf8(Text))
                    lv_dropdown_set_text(obj, ptr);
            }
        }

        protected override void DispatchLvglEvent(lv_event_code_t code)
        {
            if (code == LV_EVENT_VALUE_CHANGED)
            {
                SyncSelectionFromLvgl();
                return;
            }

            base.DispatchLvglEvent(code);
        }

        private unsafe void SyncSelectionFromLvgl()
        {
            if (_lvglObjectHandle == nint.Zero)
            {
                return;
            }

            int selectedIndex = (int)lv_dropdown_get_selected((lv_obj_t*)_lvglObjectHandle);
            if (selectedIndex < 0 || selectedIndex >= _items.Count)
            {
                selectedIndex = -1;
            }

            if (_selectedIndex == selectedIndex)
            {
                return;
            }

            _selectedIndex = selectedIndex;
            _selectedItem = _selectedIndex >= 0 && _selectedIndex < _items.Count ? _items[_selectedIndex] : null;
            OnSelectedIndexChanged(EventArgs.Empty);
        }
    }
}
