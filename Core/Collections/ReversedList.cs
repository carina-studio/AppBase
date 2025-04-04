using System;
using System.Collections;
using System.Collections.Generic;

namespace CarinaStudio.Collections;

/// <summary>
/// Implementation of <see cref="IReadOnlyList{T}"/> which reverses the wrapped <see cref="IReadOnlyList{T}"/>.
/// </summary>
/// <typeparam name="T">Type of item.</typeparam>
class ReadOnlyReversedList<T>(IReadOnlyList<T> list) : IReadOnlyList<T>
{
    // Enumerator.
    class Enumerator(IReadOnlyList<T> list) : IEnumerator<T>
    {
        // Fields.
        readonly IEnumerator<T> baseEnumerator = list.GetEnumerator();
        int index = -1;

        /// <inheritdoc/>
        public T Current
        {
            get
            {
                if (this.index < 0)
                    throw new InvalidOperationException();
                return list[this.index];
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
                    this.index = list.Count - 1;
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


    /// <inheritdoc/>.
    public int Count => list.Count;


    /// <inheritdoc/>
    public IEnumerator<T> GetEnumerator() =>
        new Enumerator(list);


    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() =>
        this.GetEnumerator();


    /// <inheritdoc cref="IReadOnlyList{T}.this"/>
    public T this[int index]  => list[^(index + 1)];
}


/// <summary>
/// Implementation of <see cref="IList{T}"/> and <see cref="IReadOnlyList{T}"/> which reverses the wrapped <see cref="IList{T}"/>.
/// </summary>
/// <typeparam name="T">Type of item.</typeparam>
class ReversedList<T>(IList<T> list, bool isReadOnly) : IList<T>, IReadOnlyList<T>
{
    // Enumerator.
    class Enumerator(IList<T> list) : IEnumerator<T>
    {
        // Fields.
        readonly IEnumerator<T> baseEnumerator = list.GetEnumerator();
        int index = -1;

        /// <inheritdoc/>
        public T Current
        {
            get
            {
                if (this.index < 0)
                    throw new InvalidOperationException();
                return list[this.index];
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
                    this.index = list.Count - 1;
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


    /// <inheritdoc/>
    public void Add(T item)
    {
        if (isReadOnly)
            throw new InvalidOperationException();
        list.Insert(0, item);
    }


    /// <inheritdoc/>
    public void Clear()
    {
        if (isReadOnly)
            throw new InvalidOperationException();
        list.Clear();
    }


    /// <inheritdoc/>
    public bool Contains(T item) =>
        list.Contains(item);


    /// <inheritdoc/>
    public void CopyTo(T[] array, int arrayIndex)
    {
        list.CopyTo(array, arrayIndex);
        Array.Reverse(array, arrayIndex, list.Count);
    }


    /// <inheritdoc cref="ICollection{T}.Count"/>.
    public int Count => list.Count;


    /// <inheritdoc/>
    public IEnumerator<T> GetEnumerator() =>
        new Enumerator(list);


    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() =>
        this.GetEnumerator();
    
    
    /// <inheritdoc/>
    public bool IsReadOnly => isReadOnly || list.IsReadOnly;
    
    
    /// <inheritdoc/>
    public int IndexOf(T item)
    {
        var index = list.IndexOf(item);
        return index >= 0 ? this.Count - index - 1 : -1;
    }


    /// <inheritdoc/>
    public void Insert(int index, T item)
    {
        if (isReadOnly)
            throw new InvalidOperationException();
        list.Insert(list.Count - index, item);
    }


    /// <inheritdoc/>
    public bool Remove(T item)
    {
        if (isReadOnly)
            throw new InvalidOperationException();
        return list.Remove(item);
    }


    /// <inheritdoc/>
    public void RemoveAt(int index)
    {
        if (isReadOnly)
            throw new InvalidOperationException();
        list.RemoveAt(this.Count - index - 1);
    }


    /// <inheritdoc cref="IList{T}.this"/>
    public T this[int index]
    {
        get => list[^(index + 1)];
        set
        {
            if (isReadOnly)
                throw new InvalidOperationException();
            list[^(index + 1)] = value;
        }
    }
}