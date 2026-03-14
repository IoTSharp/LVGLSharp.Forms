namespace LVGLSharp.Forms
{
    public class ControlEventArgs : EventArgs
    {
        public Control? Control { get; }

        public ControlEventArgs() { }

        public ControlEventArgs(Control? control)
        {
            Control = control;
        }
    }
}