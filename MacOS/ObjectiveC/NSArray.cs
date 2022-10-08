using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CarinaStudio.MacOS.ObjectiveC;

/// <summary>
/// NSArray.
/// </summary>
public class NSArray<T> : NSObject, IList<T> where T : NSObject
{
    // Enumerator.
    class Enumerator : IEnumerator<T>
    {
        // Fields.
        readonly NSEnumerator baseEnumerator;
        T? current;

        // Constructor.
        public Enumerator(NSEnumerator baseEnumerator) =>
            this.baseEnumerator = baseEnumerator;
        
        // Current.
        public T Current { get => this.current ?? throw new InvalidOperationException(); }
        object IEnumerator.Current =>
            this.Current;
        
        // Dispose.
        void IDisposable.Dispose()
        { }

        // Move to next element.
        public bool MoveNext()
        {
            this.current = this.baseEnumerator.NextObject().Exchange(it => 
            {
                if (it == null)
                    return null;
                if (typeof(T) == typeof(NSObject))
                    return (T)it;
                return it?.Retain<T>();
            });
            return this.current != null;
        }

        // Reset.
        void IEnumerator.Reset() =>
            throw new InvalidOperationException();
    }


    // Static fields.
    static readonly Selector? ContainsSelector;
    static readonly Selector? CountSelector;
    static readonly Selector? IndexOfSelector;
    static readonly Selector? InitWithArraySelector;
    static readonly Selector? InitWithObjectsSelector;
    static readonly Class? NSArrayClass;
    static readonly Selector? ObjectAtSelector;
    static readonly Selector? ObjectEnumeratorSelector;


    // Static initializer.
    static NSArray()
    {
        if (Platform.IsNotMacOS)
            return;
        NSArrayClass = Class.GetClass("NSArray").AsNonNull();
        ContainsSelector = Selector.FromName("contains:");
        CountSelector = Selector.FromName("count");
        IndexOfSelector = Selector.FromName("indexOf:");
        InitWithArraySelector = Selector.FromName("initWithArray:");
        InitWithObjectsSelector = Selector.FromName("initWithObjects:count:");
        ObjectAtSelector = Selector.FromName("objectAtIndex:");
        ObjectEnumeratorSelector = Selector.FromName("objectEnumerator");
    }


    /// <summary>
    /// Initialize new <see cref="NSArray{T}"/> instance.
    /// </summary>
    /// <param name="objects">Elements.</param>
    public NSArray(params T[] objects) : this((IEnumerable<T>)objects)
    { }


    /// <summary>
    /// Initialize new <see cref="NSArray{T}"/> instance.
    /// </summary>
    /// <param name="objects">Elements.</param>
    public NSArray(IEnumerable<T> objects) : base(Global.Run(() =>
    {
        if (objects is NSArray<T> nsArray)
            return Initialize(NSArrayClass!.Allocate(), nsArray);
        return Initialize(NSArrayClass!.Allocate(), objects);
    }), true)
    { }


    // Constructor.
    NSArray(IntPtr handle, bool ownsInstance) : base(handle, ownsInstance) =>
        this.VerifyClass(NSArrayClass!);
    NSArray(Class cls, IntPtr handle, bool ownsInstance) : base(cls, handle, ownsInstance)
    { }


    /// <inheritdoc/>
    void ICollection<T>.Add(T obj) =>
        throw new InvalidOperationException();
    

    /// <inheritdoc/>
    void ICollection<T>.Clear() =>
        throw new InvalidOperationException();
    

    /// <summary>
    /// Check whether given object is contained in array or not.
    /// </summary>
    /// <param name="obj">Object to check.</param>
    /// <returns>True if given object is contained in array.</returns>
    public bool Contains(T? obj) =>
        this.SendMessage<bool>(ContainsSelector!, obj);
    

    /// <summary>
    /// Copy elements from array.
    /// </summary>
    /// <param name="array">Array.</param>
    /// <param name="arrayIndex">Index of first position in <paramref name="array"/> to put copied elements.</param>
    public void CopyTo(T[] array, int arrayIndex)
    {
        if (arrayIndex < 0 || arrayIndex >= array.Length)
            throw new ArgumentOutOfRangeException(nameof(arrayIndex));
        var count = this.Count;
        if (count <= 0)
            return;
        --count;
        arrayIndex += count;
        if (arrayIndex >= array.Length)
            throw new ArgumentOutOfRangeException(nameof(arrayIndex));
        while (count >= 0)
            array[arrayIndex--] = this[count--];
    }
    

    /// <summary>
    /// Get number of element in array.
    /// </summary>
    public int Count { get => this.SendMessage<int>(CountSelector!); }


    /// <summary>
    /// Get enumerator.
    /// </summary>
    /// <returns>Enumerator.</returns>
    public IEnumerator<T> GetEnumerator() =>
        new Enumerator(this.SendMessage<NSEnumerator>(ObjectEnumeratorSelector!));
    

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() =>
        this.GetEnumerator();
    

    /// <summary>
    /// Initialize allocated instance with array.
    /// </summary>
    /// <param name="obj">Handle of allocated instance.</param>
    /// <param name="array">Array.</param>
    /// <returns>Handle of initialized instance.</returns>
    protected static IntPtr Initialize(IntPtr obj, NSArray<T> array) =>
        NSObject.SendMessage<IntPtr>(obj, InitWithArraySelector!, array);
    

    /// <summary>
    /// Initialize allocated instance with initial objects.
    /// </summary>
    /// <param name="obj">Handle of allocated instance.</param>
    /// <param name="objects">Objects.</param>
    /// <returns>Handle of initialized instance.</returns>
    protected static unsafe IntPtr Initialize(IntPtr obj, IEnumerable<T> objects)
    {
        IntPtr[] handleList;
        if (objects is IList<T> list)
        {
            handleList = new IntPtr[list.Count];
            for (var i = list.Count - 1; i >= 0; --i)
                handleList[i] = list[i].Handle;
        }
        else
        {
            var array = objects.ToArray();
            handleList = new IntPtr[array.Length];
            for (var i = array.Length - 1; i >= 0; --i)
                handleList[i] = array[i].Handle;
        }
        fixed (IntPtr* handleListPtr = handleList)
            return NSObject.SendMessage<IntPtr>(obj, InitWithObjectsSelector!, (IntPtr)handleListPtr, handleList.Length);
    }


    /// <inheritdoc/>
    void IList<T>.Insert(int index, T obj) =>
        throw new InvalidOperationException();


    /// <summary>
    /// Get position of given object in array.
    /// </summary>
    /// <param name="obj">Object to check.</param>
    /// <returns>Index of position of object.</returns>
    public int IndexOf(T obj) =>
        this.SendMessage<int>(IndexOfSelector!, obj);
    

    /// <inheritdoc/>
    bool ICollection<T>.IsReadOnly => true;


    /// <inheritdoc/>
    bool ICollection<T>.Remove(T obj) =>
        throw new InvalidOperationException();
    

    /// <inheritdoc/>
    void IList<T>.RemoveAt(int index) =>
        throw new InvalidOperationException();


    /// <summary>
    /// Get element at given position.
    /// </summary>
    public T this[int index] { get => this.SendMessage<T>(ObjectAtSelector!, index); }


    /// <inheritdoc/>
    T IList<T>.this[int index]
    {
        get => this[index];
        set => throw new InvalidOperationException();
    }


    /// <inheritdoc/>
    public override string ToString() =>
        $"{typeof(T).Name}[{this.Count}]";
}