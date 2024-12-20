using System;
using System.Collections;
using System.Collections.Generic;

namespace CarinaStudio;

/// <summary>
/// Collection which collects one or more <see cref="IDisposable"/>s and dispose them automatically when disposing the collection.
/// </summary>
public class DisposableCollection: BaseDisposable, ICollection<IDisposable>
{
    // Fields.
    readonly List<IDisposable> disposables = new();


    /// <summary>
    /// Initialize new <see cref="DisposableCollection"/> instance.
    /// </summary>
    public DisposableCollection()
    { }


    /// <summary>
    /// Initialize new <see cref="DisposableCollection"/> instance.
    /// </summary>
    /// <param name="disposables">Initial <see cref="IDisposable"/>s to be added to the collection.</param>
    public DisposableCollection(params IDisposable[] disposables)
    {
        this.disposables.AddRange(disposables);
    }
    
    
    /// <inheritdoc/>
    public void Add(IDisposable item)
    {
        this.VerifyDisposed();
        this.disposables.Add(item);
    }


    /// <inheritdoc/>
    public void Clear()
    {
        this.VerifyDisposed();
        this.disposables.Clear();
    }


    /// <inheritdoc/>
    public bool Contains(IDisposable item) =>
        this.disposables.Contains(item);


    /// <inheritdoc/>
    public void CopyTo(IDisposable[] array, int arrayIndex) =>
        this.disposables.CopyTo(array, arrayIndex);


    /// <inheritdoc/>
    public int Count => this.disposables.Count;


    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            var disposables = this.disposables;
            for (var i = disposables.Count - 1; i >= 0; --i)
                disposables[i].Dispose();
            disposables.Clear();
        }
    }


    /// <inheritdoc/>
    public IEnumerator<IDisposable> GetEnumerator() =>
        this.disposables.GetEnumerator();

    
    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() =>
        this.GetEnumerator();
    
    
    /// <inheritdoc/>
    bool ICollection<IDisposable>.IsReadOnly => this.IsDisposed;


    /// <inheritdoc/>
    public bool Remove(IDisposable item)
    {
        this.VerifyDisposed();
        return this.disposables.Remove(item);
    }
}