namespace UdpToolkit.Network.Clients
{
    using System.Collections.Generic;

    /// <summary>
    /// Thread-safe wrapper for list.
    /// </summary>
    /// <typeparam name="T">Type.</typeparam>
    public class ThreadSafeList<T> : IList<T>
    {
        private readonly List<T> _list;
        private readonly object _root;

        /// <summary>
        /// Initializes a new instance of the <see cref="ThreadSafeList{T}"/> class.
        /// </summary>
        /// <param name="list">Instance of list.</param>
        internal ThreadSafeList(List<T> list)
        {
            _list = list;
            _root = ((System.Collections.ICollection)list).SyncRoot;
        }

        /// <inheritdoc />
        public int Count
        {
            get
            {
                lock (_root)
                {
                    return _list.Count;
                }
            }
        }

        /// <inheritdoc />
        public bool IsReadOnly
        {
            get
            {
                return ((ICollection<T>)_list).IsReadOnly;
            }
        }

        /// <inheritdoc />
        public T this[int index]
        {
            get
            {
                lock (_root)
                {
                    return _list[index];
                }
            }

            set
            {
                lock (_root)
                {
                    _list[index] = value;
                }
            }
        }

        /// <inheritdoc />
        public void Add(T item)
        {
            lock (_root)
            {
                _list.Add(item);
            }
        }

        /// <inheritdoc />
        public void Clear()
        {
            lock (_root)
            {
                _list.Clear();
            }
        }

        /// <inheritdoc />
        public bool Contains(T item)
        {
            lock (_root)
            {
                return _list.Contains(item);
            }
        }

        /// <inheritdoc />
        public void CopyTo(T[] array, int arrayIndex)
        {
            lock (_root)
            {
                _list.CopyTo(array, arrayIndex);
            }
        }

        /// <inheritdoc />
        public bool Remove(T item)
        {
            lock (_root)
            {
                return _list.Remove(item);
            }
        }

        /// <inheritdoc />
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            lock (_root)
            {
                return _list.GetEnumerator();
            }
        }

        /// <inheritdoc />
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            lock (_root)
            {
                return ((IEnumerable<T>)_list).GetEnumerator();
            }
        }

        /// <inheritdoc />
        public int IndexOf(T item)
        {
            lock (_root)
            {
                return _list.IndexOf(item);
            }
        }

        /// <inheritdoc />
        public void Insert(int index, T item)
        {
            lock (_root)
            {
                _list.Insert(index, item);
            }
        }

        /// <inheritdoc />
        public void RemoveAt(int index)
        {
            lock (_root)
            {
                _list.RemoveAt(index);
            }
        }
    }
}