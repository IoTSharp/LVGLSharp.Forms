namespace LVGLSharp.Forms
{
    public class LayoutEventArgs : EventArgs
    {
        public Control? AffectedControl { get; }
        public string? AffectedProperty { get; }

        public LayoutEventArgs() { }

        public LayoutEventArgs(Control? affectedControl, string? affectedProperty)
        {
            AffectedControl = affectedControl;
            AffectedProperty = affectedProperty;
        }
    }
}