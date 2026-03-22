using LVGLSharp.Interop;
using System.ComponentModel;

namespace LVGLSharp.Forms
{
    public class CheckBox : Control
    {
        private bool? _useVisualStyleBackColor;
        private bool _checked;
        private CheckState _checkState = CheckState.Unchecked;

        public CheckBox()
        {
        }

        [DefaultValue(true)]
        public bool UseVisualStyleBackColor
        {
            get => _useVisualStyleBackColor ?? Application.VisualStylesEnabled;
            set => _useVisualStyleBackColor = value;
        }

        [Bindable(true)]
        [SettingsBindable(true)]
        [DefaultValue(false)]
        public bool Checked
        {
            get => _checked;
            set
            {
                if (_checked != value)
                {
                    _checked = value;
                    _checkState = _checked ? CheckState.Checked : CheckState.Unchecked;
                    UpdateLvglState();
                    OnCheckedChanged(EventArgs.Empty);
                }
            }
        }

        [Bindable(true)]
        [DefaultValue(CheckState.Unchecked)]
        public CheckState CheckState
        {
            get => _checkState;
            set
            {
                if (_checkState != value)
                {
                    _checkState = value;
                    _checked = _checkState == CheckState.Checked;
                    UpdateLvglState();
                    OnCheckStateChanged(EventArgs.Empty);
                    if (_checkState != CheckState.Indeterminate)
                    {
                        OnCheckedChanged(EventArgs.Empty);
                    }
                }
            }
        }

        public event EventHandler? CheckedChanged;
        public event EventHandler? CheckStateChanged;

        protected virtual void OnCheckedChanged(EventArgs e)
        {
            CheckedChanged?.Invoke(this, e);
        }

        protected virtual void OnCheckStateChanged(EventArgs e)
        {
            CheckStateChanged?.Invoke(this, e);
        }

        internal override unsafe void CreateLvglObject(nint parentHandle)
        {
            _lvglObjectHandle = (nint)lv_checkbox_create((lv_obj_t*)parentHandle);
            var obj = (lv_obj_t*)_lvglObjectHandle;
            
            if (!string.IsNullOrEmpty(Text))
            {
                fixed (byte* ptr = ToUtf8(Text))
                    lv_checkbox_set_text(obj, ptr);
            }
            
            UpdateLvglState();
            
            Application.GetStyleSet(UseVisualStyleBackColor).CheckBox.Apply(obj);
            ApplyLvglProperties();
            CreateChildrenLvglObjects();
        }

        private unsafe void UpdateLvglState()
        {
            if (_lvglObjectHandle == nint.Zero) return;

            var obj = (lv_obj_t*)_lvglObjectHandle;
            
            if (_checked || _checkState == CheckState.Checked)
            {
                lv_obj_add_state(obj, lv_state_t.LV_STATE_CHECKED);
            }
            else
            {
                lv_obj_clear_state(obj, lv_state_t.LV_STATE_CHECKED);
            }
        }

        protected override void OnClick(EventArgs e)
        {
            // Toggle checked state
            Checked = !Checked;
            base.OnClick(e);
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
                lv_checkbox_set_text((lv_obj_t*)_lvglObjectHandle, ptr);
            }
        }
    }
}
