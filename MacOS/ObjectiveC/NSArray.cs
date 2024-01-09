using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace CarinaStudio.MacOS.ObjectiveC;

// NSArray.
static class NSArray
{
    // Static fields.
    public static readonly Selector? ContainsSelector;
    public static readonly Selector? CountSelector;
    public static readonly Selector? IndexOfSelector;
    public static readonly Selector? InitWithArraySelector;
    public static readonly Selector? InitWithObjectsSelector;
    public static readonly Class? NSArrayClass;
    public static readonly Selector? ObjectAtSelector;
    public static readonly Selector? ObjectEnumeratorSelector;
    
    
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
}

/// <summary>
/// NSArray.
/// </summary>
public class NSArray<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)] T> : NSObject, IList<T> where T : NSObject
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
        public T Current => this.current ?? throw new InvalidOperationException();

        object IEnumerator.Current =>
            this.Current;
        
        // Dispose.
        void IDisposable.Dispose()
        { }

        // Move to next element.
        public bool MoveNext()
        {
            this.current = this.baseEnumerator.NextObject()?.Exchange(it => 
            {
                if (typeof(T) == typeof(NSObject))
                    return (T)it;
                return it.Retain<T>();
            });
            return this.current is not null;
        }

        // Reset.
        void IEnumerator.Reset() =>
            throw new InvalidOperationException();
    }


    /// <summary>
    /// Initialize new <see cref="NSArray{T}"/> instance.
    /// </summary>
    /// <param name="objects">Elements.</param>
    [RequiresDynamicCode(CallConstructorRdcMessage)]
    public NSArray(params T[] objects) : this((IEnumerable<T>)objects)
    { }


    /// <summary>
    /// Initialize new <see cref="NSArray{T}"/> instance.
    /// </summary>
    /// <param name="objects">Elements.</param>
    [RequiresDynamicCode(CallConstructorRdcMessage)]
    public NSArray(IEnumerable<T> objects) : base(Global.Run(() =>
    {
        if (objects is NSArray<T> nsArray)
            return Initialize(NSArray.NSArrayClass!.Allocate(), nsArray);
        return Initialize(NSArray.NSArrayClass!.Allocate(), objects);
    }), true)
    { }


    // Constructor.
    NSArray(IntPtr handle, bool ownsInstance) : base(handle, ownsInstance) =>
        this.VerifyClass(NSArray.NSArrayClass!);
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
    [RequiresDynamicCode(CallConstructorRdcMessage)]
    public bool Contains(T? obj) =>
        this.SendMessage<bool>(NSArray.ContainsSelector!, obj);
    

    /// <summary>
    /// Copy elements from array.
    /// </summary>
    /// <param name="array">Array.</param>
    /// <param name="arrayIndex">Index of first position in <paramref name="array"/> to put copied elements.</param>
    [RequiresDynamicCode(CallMethodRdcMessage)]
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
#pragma warning disable IL3050
    public int Count => this.SendMessage<int>(NSArray.CountSelector!);
#pragma warning restore IL3050


    /// <summary>
    /// Get enumerator.
    /// </summary>
    /// <returns>Enumerator.</returns>
#pragma warning disable IL3050
    public IEnumerator<T> GetEnumerator() =>
        new Enumerator(this.SendMessage<NSEnumerator>(NSArray.ObjectEnumeratorSelector!));
#pragma warning restore IL3050
    

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() =>
        this.GetEnumerator();
    

    /// <summary>
    /// Initialize allocated instance with array.
    /// </summary>
    /// <param name="obj">Handle of allocated instance.</param>
    /// <param name="array">Array.</param>
    /// <returns>Handle of initialized instance.</returns>
    [RequiresDynamicCode(CallMethodRdcMessage)]
    protected static IntPtr Initialize(IntPtr obj, NSArray<T> array) =>
        SendMessage<IntPtr>(obj, NSArray.InitWithArraySelector!, array);
    

    /// <summary>
    /// Initialize allocated instance with initial objects.
    /// </summary>
    /// <param name="obj">Handle of allocated instance.</param>
    /// <param name="objects">Objects.</param>
    /// <returns>Handle of initialized instance.</returns>
    [RequiresDynamicCode(CallMethodRdcMessage)]
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
            return SendMessage<IntPtr>(obj, NSArray.InitWithObjectsSelector!, (IntPtr)handleListPtr, handleList.Length);
    }


    /// <inheritdoc/>
    void IList<T>.Insert(int index, T obj) =>
        throw new InvalidOperationException();


    /// <summary>
    /// Get position of given object in array.
    /// </summary>
    /// <param name="obj">Object to check.</param>
    /// <returns>Index of position of object.</returns>
    [RequiresDynamicCode(CallMethodRdcMessage)]
    public int IndexOf(T obj) =>
        this.SendMessage<int>(NSArray.IndexOfSelector!, obj);
    

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
    public T this[int index]
    {
        [RequiresDynamicCode(GetPropertyRdcMessage)]
        get => this.SendMessage<T>(NSArray.ObjectAtSelector!, index);
    }


    /// <inheritdoc/>
    T IList<T>.this[int index]
    {
        [RequiresDynamicCode(GetPropertyRdcMessage)]
        get => this[index];
        set => throw new InvalidOperationException();
    }


    /// <inheritdoc/>
    public override string ToString() =>
        $"{typeof(T).Name}[{this.Count}]";
}