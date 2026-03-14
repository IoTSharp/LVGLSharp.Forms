namespace LVGLSharp.Forms
{
    public class LayoutEventArgs : EventArgs
    {
        public LayoutEventArgs(Control? affectedControl, string? affectedProperty)
        {
            AffectedControl = affectedControl;
            AffectedProperty = affectedProperty;
        }

        public Control? AffectedControl { get; }

        public string? AffectedProperty { get; }
    }
}