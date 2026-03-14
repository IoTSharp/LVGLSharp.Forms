using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace LVGLSharp.Forms
{
    /// <summary>
    /// Provides a read-only view of the forms currently open in the application.
    /// </summary>
    public sealed class FormCollection : ReadOnlyCollection<Form>
    {
        internal FormCollection(IList<Form> list)
            : base(list)
        {
        }
    }
}
