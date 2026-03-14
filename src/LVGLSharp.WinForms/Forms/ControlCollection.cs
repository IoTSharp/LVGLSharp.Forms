using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
namespace LVGLSharp.Forms
{
    


    [ListBindable(false)]
    public class ControlCollection : IList<Control>, ICollection, IEnumerable
    {
        private readonly Control? _owner;
        private Collection<Control> _ctls = new Collection<Control>();

        public ControlCollection(Control? owner = null)
        {
            _owner = owner;
        }

        public Control? this[int index] { get => _ctls[index]; set => _ctls[index] = value; }

        public int Count => _ctls.Count;

        public bool IsReadOnly => false;

        public bool IsSynchronized => false;

        public object SyncRoot => _ctls;

        public void Add(Control item)
        {
            _ctls.Add(item);
            _owner?.RaiseControlAdded(item);
        }

        public void Add(Control item, int v, int v1)
        {
            _ctls.Add(item);
            _owner?.RaiseControlAdded(item);
        }

        public void Clear()
        {
            _ctls.Clear();
        }

        public bool Contains(Control item)
        {
            return _ctls.Contains(item);
        }

        public void CopyTo(Control[] array, int arrayIndex)
        {
            _ctls.CopyTo(array, arrayIndex);
        }

        public void CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<Control> GetEnumerator()
        {
            return _ctls.GetEnumerator();
        }

        public int IndexOf(Control item) => _ctls.IndexOf(item);

        public void Insert(int index, Control item) => _ctls.Insert(index, item);

        public bool Remove(Control item)
        {
            bool removed = _ctls.Remove(item);
            if (removed) _owner?.RaiseControlRemoved(item);
            return removed;
        }

        public void RemoveAt(int index)
        {
            var item = _ctls[index];
            _ctls.RemoveAt(index);
            _owner?.RaiseControlRemoved(item);
        }

        IEnumerator IEnumerable.GetEnumerator() { return _ctls.GetEnumerator(); }
    }
}