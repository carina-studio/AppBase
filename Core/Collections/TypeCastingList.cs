using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CarinaStudio.Collections;

// Class to wrap IList as generic IList<T>.
class TypeCastingList<T> : IList<T>, IReadOnlyList<T>
{
    // Fields.
    readonly IList list;

    
    // Constructor.
    // ReSharper disable once ConvertToPrimaryConstructor
    public TypeCastingList(IList list) =>
        this.list = list;
    

    /// <inheritdoc/>
    public void Add(T element) => 
        this.list.Add(element);

    
    /// <inheritdoc/>
    public void Clear() => 
        this.list.Clear();


    /// <inheritdoc/>
    public bool Contains(T element) =>
        this.list.Contains(element);
    

    /// <inheritdoc/>
    public void CopyTo(T[] array, int arrayIndex)
    {
        var list = this.list;
        var count = list.Count;
        if (count == 0)
            return;
        var elementType = (Type?)null;
        for (var i = count - 1; i >= 0; --i)
        {
            elementType = list[i]?.GetType();
            if (elementType != null)
                break;
        }
        if (elementType == typeof(T))
            list.CopyTo(array, arrayIndex);
        else if (elementType != null)
        {
            if (arrayIndex < 0 || arrayIndex + count > array.Length)
                throw new ArgumentOutOfRangeException(nameof(arrayIndex));
            for (var i = 0; i < count; ++i, ++arrayIndex)
                array[arrayIndex] = (T)list[i]!;
        }
    }
    

    /// <inheritdoc cref="ICollection{T}.Count"/>
    public int Count => this.list.Count;


    /// <inheritdoc/>
    public int IndexOf(T element) =>
        this.list.IndexOf(element);


    /// <inheritdoc/>
    public void Insert(int index, T element) =>
        this.list.Insert(index, element);
    

    /// <inheritdoc/>
    public bool IsReadOnly => this.list.IsReadOnly;


    /// <inheritdoc/>
    public IEnumerator<T> GetEnumerator() =>
        ((IEnumerable)this.list).Cast<T>().GetEnumerator();


    /// <inheritdoc/>
    public bool Remove(T element)
    {
        var index = this.list.IndexOf(element);
        if (index >= 0)
        {
            this.list.RemoveAt(index);
            return true;
        }
        return false;
    }


    /// <inheritdoc/>
    public void RemoveAt(int index) =>
        this.list.RemoveAt(index);


#pragma warning disable CS8600
#pragma warning disable CS8603
    /// <inheritdoc cref="IList{T}.this"/>
    public T this[int index]
    {
        get => (T)this.list[index];
        set => this.list[index] = value;
    }
#pragma warning restore CS8600
#pragma warning restore CS8603


    // Interface implementations.
    IEnumerator IEnumerable.GetEnumerator() =>
        this.list.GetEnumerator();
}