using LVGLSharp;
using LVGLSharp.Drawing;
using LVGLSharp.Interop;
using System;
using System.ComponentModel;

namespace LVGLSharp.Forms
{
    public unsafe class Form : Control
    {
        private IView? _view;
        private bool _loadRaised;

        public Form()
        {
        }

        public event EventHandler? Load;

        public FormBorderStyle FormBorderStyle { get; set; } = FormBorderStyle.Sizable;

        public static lv_obj_t* RootObject { get; set; }
        public static lv_group_t* KeyInputGroupObject { get; set; }
        public static delegate* unmanaged[Cdecl]<lv_event_t*, void> SendTextAreaFocusCallback { get; set; } = null;
        public static delegate* unmanaged[Cdecl]<lv_event_t*, void> SendTextAreaDefocusCallback { get; set; } = null;

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
            if (_view is not null)
            {
                return;
            }

            base.CreateHandle();

            var displayWidth = ClientSize.Width > 0 ? ClientSize.Width : (Size.Width > 0 ? Size.Width : 800);
            var displayHeight = ClientSize.Height > 0 ? ClientSize.Height : (Size.Height > 0 ? Size.Height : 600);
            Width = displayWidth;
            Height = displayHeight;
            Size = new Size(displayWidth, displayHeight);

            var windowOptions = new WindowCreateOptions(Text, displayWidth, displayHeight, FormBorderStyle == FormBorderStyle.None);
            _view = WindowHostFactory.Create(windowOptions);
            _view.Open();

            RootObject = _view.Root;
            KeyInputGroupObject = _view.KeyInputGroup;
            SendTextAreaFocusCallback = _view.SendTextAreaFocusCallback;
            SendTextAreaDefocusCallback = null;
            Application.CurrentStyleSet.Root.Apply(RootObject);

            _lvglObjectHandle = (nint)RootObject;
            Handle = _lvglObjectHandle;
            OnHandleCreated(EventArgs.Empty);
            Application.RegisterOpenForm(this);

            var skipChildCreate = string.Equals(Environment.GetEnvironmentVariable("LVGLSHARP_SKIP_CHILD_CREATE"), "1", StringComparison.OrdinalIgnoreCase);
            if (!skipChildCreate)
            {
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

            OnLoad(EventArgs.Empty);

            if (RootObject != null)
            {
                lv_obj_invalidate(RootObject);
            }
        }

        public new void Show()
        {
            if (_view is null)
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
            if (_view is null)
            {
                return;
            }

            _view.RunLoop(() =>
            {
                handle?.Invoke();
                OnMessageLoopIteration();
            });
            Application.UnregisterOpenForm(this);
        }

        internal void ProcessEventsCore()
        {
            _view?.HandleEvents();
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected override void DestroyHandle()
        {
            if (_view is null && Handle == 0)
            {
                return;
            }

            _view?.Close();
            _view?.Dispose();
            _view = null;
            RootObject = null;
            KeyInputGroupObject = null;
            SendTextAreaFocusCallback = null;
            SendTextAreaDefocusCallback = null;
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

        protected internal virtual void OnMessageLoopIteration()
        {
        }

        public SizeF AutoScaleDimensions { get; set; }

        public AutoScaleMode AutoScaleMode { get; set; }
    }
}
