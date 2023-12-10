using System;
using System.Collections;
using System.Collections.Generic;

namespace CarinaStudio.Collections
{
    /// <summary>
    /// Implementation of <see cref="IList{T}"/> and <see cref="IReadOnlyList{T}"/> which reverses the wrapped <see cref="IList{T}"/>.
    /// </summary>
    /// <typeparam name="T">Type of item.</typeparam>
    class ReversedList<T> : IList<T>, IReadOnlyList<T>
    {
        // Enumerator.
        class Enumerator : IEnumerator<T>
        {
            // Fields.
            readonly IEnumerator<T> baseEnumerator;
            readonly IList<T> list;
            int index = -1;
            
            // Constructor.
            public Enumerator(IList<T> list)
            {
                this.baseEnumerator = list.GetEnumerator();
                this.list = list;
            }

            /// <inheritdoc/>
            public T Current
            {
                get
                {
                    if (this.index < 0)
                        throw new InvalidOperationException();
                    return this.list[this.index];
                }
            }

            /// <inheritdoc/>
            object? IEnumerator.Current => this.Current;

            /// <inheritdoc/>
            public void Dispose()
            {
                this.baseEnumerator.Dispose();
                this.index = -1;
            }

            /// <inheritdoc/>
            public bool MoveNext()
            {
                if (this.index < 0)
                {
                    if (this.baseEnumerator.MoveNext())
                    {
                        this.index = this.list.Count - 1;
                        return true;
                    }
                }
                else
                {
                    if (this.baseEnumerator.MoveNext())
                    {
                        --this.index;
                        return true;
                    }
                }
                this.index = -1;
                return false;
            }

            /// <inheritdoc/>
            public void Reset()
            {
                this.baseEnumerator.Reset();
                this.index = -1;
            }
        }
        
        
        // Fields.
        readonly bool isReadOnly;
        readonly IList<T> list;


        // Constructor.
        public ReversedList(IList<T> list, bool isReadOnly)
        {
            this.isReadOnly = isReadOnly;
            this.list = list;
        }


        /// <inheritdoc/>
        public void Add(T item)
        {
            if (this.isReadOnly)
                throw new InvalidOperationException();
            this.list.Insert(0, item);
        }


        /// <inheritdoc/>
        public void Clear()
        {
            if (this.isReadOnly)
                throw new InvalidOperationException();
            this.list.Clear();
        }


        /// <inheritdoc/>
        public bool Contains(T item) =>
            this.list.Contains(item);


        /// <inheritdoc/>
        public void CopyTo(T[] array, int arrayIndex)
        {
            this.list.CopyTo(array, arrayIndex);
            Array.Reverse(array, arrayIndex, this.list.Count);
        }


        /// <inheritdoc cref="ICollection{T}.Count"/>.
        public int Count => this.list.Count;


        /// <inheritdoc/>
        public IEnumerator<T> GetEnumerator() =>
            new Enumerator(this.list);


        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() =>
            this.GetEnumerator();
        
        
        /// <inheritdoc/>
        public bool IsReadOnly => this.isReadOnly || this.list.IsReadOnly;
        
        
        /// <inheritdoc/>
        public int IndexOf(T item)
        {
            var index = this.list.IndexOf(item);
            return index >= 0 ? this.Count - index - 1 : -1;
        }


        /// <inheritdoc/>
        public void Insert(int index, T item)
        {
            if (this.isReadOnly)
                throw new InvalidOperationException();
            this.list.Insert(this.list.Count - index, item);
        }


        /// <inheritdoc/>
        public bool Remove(T item)
        {
            if (this.isReadOnly)
                throw new InvalidOperationException();
            return this.list.Remove(item);
        }


        /// <inheritdoc/>
        public void RemoveAt(int index)
        {
            if (this.isReadOnly)
                throw new InvalidOperationException();
            this.list.RemoveAt(this.Count - index - 1);
        }


        /// <inheritdoc cref="IList{T}.this"/>
        public T this[int index]
        {
            get => this.list[^(index + 1)];
            set
            {
                if (this.isReadOnly)
                    throw new InvalidOperationException();
                this.list[^(index + 1)] = value;
            }
        }
    }
}