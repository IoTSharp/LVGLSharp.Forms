using LVGLSharp.Interop;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace LVGLSharp.Forms
{
    public class TextBox : Control
    {
        private int _selectionStart;
        private int _selectionLength;
        private ContextMenuStrip? _defaultContextMenu;
        private GCHandle _keyEventGcHandle;
        private GCHandle _mouseEventGcHandle;
        private UndoRedoStack _undoRedoStack = new();
        private bool _isUpdatingText;
        private bool _isSyncingFromLvgl;

        protected override bool AllowsNativeScrolling => Multiline;

        public bool Multiline { get; set; }
        public string? PlaceholderText { get; set; }

        public int SelectionStart
        {
            get => _selectionStart;
            set
            {
                _selectionStart = value;
                if (_lvglObjectHandle != IntPtr.Zero)
                {
                    unsafe
                    {
                        lv_textarea_set_cursor_pos((lv_obj_t*)_lvglObjectHandle, value);
                    }
                }
            }
        }

        public int SelectionLength
        {
            get => _selectionLength;
            set => _selectionLength = value;
        }

        public string SelectedText
        {
            get
            {
                if (_selectionStart >= 0 && _selectionLength > 0 && !string.IsNullOrEmpty(Text))
                {
                    int start = Math.Min(_selectionStart, Text.Length);
                    int length = Math.Min(_selectionLength, Text.Length - start);
                    return Text.Substring(start, length);
                }
                return string.Empty;
            }
            set
            {
                if (_selectionStart >= 0 && !string.IsNullOrEmpty(Text))
                {
                    string newText = Text.Remove(_selectionStart, Math.Min(_selectionLength, Text.Length - _selectionStart));
                    if (!string.IsNullOrEmpty(value))
                    {
                        newText = newText.Insert(_selectionStart, value);
                    }
                    SetTextWithHistory(newText);
                    _selectionLength = value?.Length ?? 0;
                }
            }
        }

        public ContextMenuStrip? ContextMenuStrip
        {
            get => _defaultContextMenu;
            set => _defaultContextMenu = value;
        }

        public bool CanUndo => _undoRedoStack.CanUndo;
        public bool CanRedo => _undoRedoStack.CanRedo;

        public void Copy()
        {
            if (_selectionLength > 0)
            {
                Clipboard.SetText(SelectedText);
            }
        }

        public void Cut()
        {
            if (_selectionLength > 0)
            {
                Clipboard.SetText(SelectedText);
                SelectedText = string.Empty;
            }
        }

        public void Paste()
        {
            string? clipboardText = Clipboard.GetText();
            if (!string.IsNullOrEmpty(clipboardText))
            {
                if (_selectionLength > 0)
                {
                    SelectedText = clipboardText;
                }
                else
                {
                    if (!string.IsNullOrEmpty(Text))
                    {
                        SetTextWithHistory(Text.Insert(_selectionStart, clipboardText));
                    }
                    else
                    {
                        SetTextWithHistory(clipboardText);
                    }
                }
                
                UpdateLvglText();
            }
        }

        public void SelectAll()
        {
            if (!string.IsNullOrEmpty(Text))
            {
                _selectionStart = 0;
                _selectionLength = Text.Length;
            }
        }

        public void Undo()
        {
            if (!CanUndo) return;

            var action = _undoRedoStack.Undo(Text ?? string.Empty, _selectionStart, _selectionLength);
            if (action != null)
            {
                _isUpdatingText = true;
                Text = action.Text;
                _selectionStart = action.CursorPosition;
                _selectionLength = action.SelectionLength;
                UpdateLvglText();
                _isUpdatingText = false;
            }
        }

        public void Redo()
        {
            if (!CanRedo) return;

            var action = _undoRedoStack.Redo(Text ?? string.Empty, _selectionStart, _selectionLength);
            if (action != null)
            {
                _isUpdatingText = true;
                Text = action.Text;
                _selectionStart = action.CursorPosition;
                _selectionLength = action.SelectionLength;
                UpdateLvglText();
                _isUpdatingText = false;
            }
        }

        public TextBox()
        {
            CreateDefaultContextMenu();
        }

        private void CreateDefaultContextMenu()
        {
            _defaultContextMenu = new ContextMenuStrip();

            var cutItem = new ToolStripMenuItem("剪切(&T)", (s, e) => Cut(), Keys.Control | Keys.X);
            var copyItem = new ToolStripMenuItem("复制(&C)", (s, e) => Copy(), Keys.Control | Keys.C);
            var pasteItem = new ToolStripMenuItem("粘贴(&P)", (s, e) => Paste(), Keys.Control | Keys.V);
            var selectAllItem = new ToolStripMenuItem("全选(&A)", (s, e) => SelectAll(), Keys.Control | Keys.A);
            
            _defaultContextMenu.Items.Add(cutItem);
            _defaultContextMenu.Items.Add(copyItem);
            _defaultContextMenu.Items.Add(pasteItem);
            _defaultContextMenu.Items.Add(ToolStripMenuItem.CreateSeparator());
            _defaultContextMenu.Items.Add(selectAllItem);

            var undoItem = new ToolStripMenuItem("撤销(&U)", (s, e) => Undo(), Keys.Control | Keys.Z);
            var redoItem = new ToolStripMenuItem("重做(&R)", (s, e) => Redo(), Keys.Control | Keys.Y);
            
            _defaultContextMenu.Items.Insert(0, undoItem);
            _defaultContextMenu.Items.Insert(1, redoItem);
            _defaultContextMenu.Items.Insert(2, ToolStripMenuItem.CreateSeparator());

            // 移除 Opening 事件处理 - 改为在 Show 之前更新项状态
        }

        private void UpdateContextMenuItemStates()
        {
            if (_defaultContextMenu == null) return;

            bool hasSelection = _selectionLength > 0;
            bool hasText = !string.IsNullOrEmpty(Text);
            bool hasClipboardText = Clipboard.ContainsText();

            var items = _defaultContextMenu.Items;
            if (items.Count >= 7)
            {
                items[0].Enabled = CanUndo;  // Undo
                items[1].Enabled = CanRedo;  // Redo
                // items[2] is separator
                items[3].Enabled = hasSelection;  // Cut
                items[4].Enabled = hasSelection;  // Copy
                items[5].Enabled = hasClipboardText;  // Paste
                // items[6] is separator
                if (items.Count >= 8)
                    items[7].Enabled = hasText;  // Select All
            }
        }

        private void SetTextWithHistory(string newText)
        {
            if (!_isUpdatingText && newText != Text)
            {
                _undoRedoStack.Push(Text ?? string.Empty, _selectionStart, _selectionLength);
            }
            Text = newText;
        }

        private void UpdateLvglText()
        {
            if (_lvglObjectHandle != IntPtr.Zero)
            {
                unsafe
                {
                    fixed (byte* ptr = ToUtf8(Text ?? string.Empty))
                    {
                        lv_textarea_set_text((lv_obj_t*)_lvglObjectHandle, ptr);
                    }

                    int cursorPosition = Math.Clamp(_selectionStart, 0, Text?.Length ?? 0);
                    lv_textarea_set_cursor_pos((lv_obj_t*)_lvglObjectHandle, cursorPosition);
                }
            }
        }

        internal override unsafe void CreateLvglObject(nint parentHandle)
        {
            _lvglObjectHandle = (nint)lv_textarea_create((lv_obj_t*)parentHandle);
            var obj = (lv_obj_t*)_lvglObjectHandle;
            
            lv_textarea_set_text_selection(obj, new c_bool1(true));
            lv_textarea_set_one_line(obj, !Multiline);
            
            if (Form.SendTextAreaFocusCb != null)
            {
                lv_obj_add_event_cb(obj, Form.SendTextAreaFocusCb, lv_event_code_t.LV_EVENT_FOCUSED, null);
            }

            if (Form.key_inputGroup != null)
            {
                lv_group_add_obj(Form.key_inputGroup, obj);
            }

            AddKeyEventCallback();
            AddMouseEventCallback(); // 添加鼠标事件回调

            if (!string.IsNullOrEmpty(Text))
            {
                fixed (byte* ptr = ToUtf8(Text))
                    lv_textarea_set_text(obj, ptr);
            }
            if (!string.IsNullOrEmpty(PlaceholderText))
            {
                fixed (byte* ptr = ToUtf8(PlaceholderText))
                    lv_textarea_set_placeholder_text(obj, ptr);
            }
            Application.CurrentStyleSet.TextBox.Apply(obj);
            ApplyLvglProperties();
            CreateChildrenLvglObjects();
        }

        private unsafe void AddMouseEventCallback()
        {
            if (_mouseEventGcHandle.IsAllocated)
                _mouseEventGcHandle.Free();
            
            _mouseEventGcHandle = GCHandle.Alloc(this);
            lv_obj_add_event_cb(
                (lv_obj_t*)_lvglObjectHandle,
                &MouseEventCallback,
                lv_event_code_t.LV_EVENT_PRESSED,
                (void*)GCHandle.ToIntPtr(_mouseEventGcHandle));
        }

        [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
        private static unsafe void MouseEventCallback(lv_event_t* e)
        {
            void* userData = lv_event_get_user_data(e);
            if (userData == null) return;

            var gcHandle = GCHandle.FromIntPtr(new IntPtr(userData));
            if (gcHandle.IsAllocated && gcHandle.Target is TextBox textBox)
            {
                lv_obj_t* target = lv_event_get_target_obj(e);
                if (target == null)
                {
                    return;
                }

                // Pointer presses do not automatically update the keypad focus group on X11.
                // Focus the textarea explicitly so subsequent key events are delivered here.
                if (Form.key_inputGroup != null)
                {
                    lv_group_focus_obj(target);
                }

                uint mouseButton = GetCurrentMouseButton();
                if (mouseButton == 2 && textBox.ContextMenuStrip != null)
                {
                    textBox.UpdateContextMenuItemStates();

                    int x = lv_obj_get_x(target);
                    int y = lv_obj_get_y(target);
                    textBox.ContextMenuStrip.Show(textBox, x, y);
                }
            }
        }

        private static uint GetCurrentMouseButton()
        {
            return RuntimeInputState.GetCurrentMouseButton();
        }

        private unsafe void AddKeyEventCallback()
        {
            if (_keyEventGcHandle.IsAllocated)
                _keyEventGcHandle.Free();
            
            _keyEventGcHandle = GCHandle.Alloc(this);
            lv_obj_add_event_cb(
                (lv_obj_t*)_lvglObjectHandle,
                &KeyEventCallback,
                lv_event_code_t.LV_EVENT_KEY,
                (void*)GCHandle.ToIntPtr(_keyEventGcHandle));
        }

        [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
        private static unsafe void KeyEventCallback(lv_event_t* e)
        {
            void* userData = lv_event_get_user_data(e);
            if (userData == null) return;
            
            var gcHandle = GCHandle.FromIntPtr(new IntPtr(userData));
            if (gcHandle.IsAllocated && gcHandle.Target is TextBox textBox)
            {
                uint key = lv_event_get_key(e);
                textBox.HandleKeyPress(key);
            }
        }

        private void HandleKeyPress(uint lvglKey)
        {
            // TODO: 将 LVGL 键码映射到 Keys 枚举
            // LVGL 使用 ASCII 值，需要检测修饰键
            
            bool isCtrlPressed = LVGLSharp.Forms.ModifierKeys.IsControlPressed;
            
            if (isCtrlPressed)
            {
                char keyChar = (char)lvglKey;
                switch (char.ToUpper(keyChar))
                {
                    case 'C':
                        Copy();
                        break;
                    case 'X':
                        Cut();
                        break;
                    case 'V':
                        Paste();
                        break;
                    case 'A':
                        SelectAll();
                        break;
                    case 'Z':
                        Undo();
                        break;
                    case 'Y':
                        Redo();
                        break;
                }
            }
        }

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);

            if (_lvglObjectHandle == IntPtr.Zero || _isSyncingFromLvgl)
            {
                return;
            }

            UpdateLvglText();
        }

        protected override void DispatchLvglEvent(lv_event_code_t code)
        {
            if (code == LV_EVENT_VALUE_CHANGED)
            {
                SyncTextFromLvgl();
                return;
            }

            base.DispatchLvglEvent(code);
        }

        private unsafe void SyncTextFromLvgl()
        {
            if (_lvglObjectHandle == IntPtr.Zero)
            {
                return;
            }

            string lvglText = Marshal.PtrToStringUTF8((nint)lv_textarea_get_text((lv_obj_t*)_lvglObjectHandle)) ?? string.Empty;
            int cursorPosition = (int)lv_textarea_get_cursor_pos((lv_obj_t*)_lvglObjectHandle);

            _selectionStart = Math.Clamp(cursorPosition, 0, lvglText.Length);
            _selectionLength = 0;

            if (string.Equals(Text, lvglText, StringComparison.Ordinal))
            {
                return;
            }

            _isSyncingFromLvgl = true;
            try
            {
                Text = lvglText;
            }
            finally
            {
                _isSyncingFromLvgl = false;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_keyEventGcHandle.IsAllocated)
                    _keyEventGcHandle.Free();
                if (_mouseEventGcHandle.IsAllocated)
                    _mouseEventGcHandle.Free();
            }
            base.Dispose(disposing);
        }
    }
}
