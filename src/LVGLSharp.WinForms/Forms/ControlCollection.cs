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

        public ControlCollection()
        {
        }

        internal ControlCollection(Control owner)
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
            ArgumentNullException.ThrowIfNull(item);

            _ctls.Add(item);
            _owner?.HandleChildAdded(item);

            if (_owner?._lvglObjectHandle != 0 && item._lvglObjectHandle == 0)
            {
                item.CreateLvglObject(_owner._lvglObjectHandle);
            }
        }

        public void Add(Control item, int v, int v1)
        {
            if (_owner is TableLayoutPanel tableLayoutPanel)
            {
                tableLayoutPanel.SetCellPosition(item, v, v1);
            }

            Add(item);
        }

        public void Clear()
        {
            foreach (var control in _ctls)
            {
                _owner?.HandleChildRemoved(control);
            }

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

        public void Insert(int index, Control item)
        {
            ArgumentNullException.ThrowIfNull(item);

            _ctls.Insert(index, item);
            _owner?.HandleChildAdded(item);
        }

        public bool Remove(Control item)
        {
            if (!_ctls.Remove(item))
            {
                return false;
            }

            _owner?.HandleChildRemoved(item);
            return true;
        }

        public void RemoveAt(int index)
        {
            var item = _ctls[index];
            _ctls.RemoveAt(index);
            _owner?.HandleChildRemoved(item);
        }

        IEnumerator IEnumerable.GetEnumerator() { return _ctls.GetEnumerator(); }
    }
}