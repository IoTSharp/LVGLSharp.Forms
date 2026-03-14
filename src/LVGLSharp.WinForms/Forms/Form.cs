using LVGLSharp;
using LVGLSharp.Darwing;
using LVGLSharp.Interop;
using System;
using System.ComponentModel;

namespace LVGLSharp.Forms
{
    public unsafe class Form : Control
    {
        private IWindow? _window;
        private bool _loadRaised;

        public Form()
        {
        }

        public event EventHandler? Load;

        public static lv_obj_t* root { get; set; }
        public static lv_group_t* key_inputGroup { get; set; }
        public static delegate* unmanaged[Cdecl]<lv_event_t*, void> SendTextAreaFocusCb { get; set; } = null;

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
        }

        protected virtual void OnLoad(EventArgs e)
        {
            if (_loadRaised)
            {
                return;
            }

            _loadRaised = true;
            Load?.Invoke(this, e);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected override void CreateHandle()
        {
            if (_window is not null)
            {
                return;
            }

            base.CreateHandle();

            var displayWidth = ClientSize.Width > 0 ? ClientSize.Width : (Size.Width > 0 ? Size.Width : 800);
            var displayHeight = ClientSize.Height > 0 ? ClientSize.Height : (Size.Height > 0 ? Size.Height : 600);
            Width = displayWidth;
            Height = displayHeight;
            Size = new Size(displayWidth, displayHeight);

            _window = WindowHostFactory.Create(Text, displayWidth, displayHeight);
            _window.Init();

            root = _window.Root;
            key_inputGroup = _window.KeyInputGroup;
            SendTextAreaFocusCb = _window.SendTextAreaFocusCallback;

            _lvglObjectHandle = (nint)root;
            Handle = _lvglObjectHandle;
            OnHandleCreated(EventArgs.Empty);
            Application.RegisterOpenForm(this);

            CreateChildrenLvglObjects();
            OnLoad(EventArgs.Empty);
        }

        public new void Show()
        {
            if (_window is null)
            {
                CreateHandle();
            }

            SetVisibleCore(true);
        }

        /// <summary>
        /// Closes the form.
        /// </summary>
        public void Close()
        {
            DestroyHandle();
        }

        internal void RunMessageLoop(Action? handle = null)
        {
            if (_window is null)
            {
                return;
            }

            _window.StartLoop(handle ?? (() => { }));
            Application.UnregisterOpenForm(this);
        }

        internal void ProcessEventsCore()
        {
            _window?.ProcessEvents();
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected override void DestroyHandle()
        {
            if (_window is null && Handle == 0)
            {
                return;
            }

            _window?.Stop();
            _window = null;
            Application.UnregisterOpenForm(this);
            base.DestroyHandle();
        }

        public new Size ClientSize
        {
            get => base.ClientSize;
            set
            {
                base.ClientSize = value;
                OnClientSizeChanged(EventArgs.Empty);
            }
        }

        protected override void OnClientSizeChanged(EventArgs e)
        {
            base.OnClientSizeChanged(e);
            Size = ClientSize;
            Width = ClientSize.Width;
            Height = ClientSize.Height;
        }

        public SizeF AutoScaleDimensions { get; set; }

        public AutoScaleMode AutoScaleMode { get; set; }
    }
}








