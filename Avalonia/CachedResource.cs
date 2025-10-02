using Avalonia.Controls;
using CarinaStudio.Threading;
using System;

namespace CarinaStudio;

/// <summary>
/// <see cref="IObservable{T}"/> which caches the resource with specific key in resource dictionary.
/// </summary>
/// <typeparam name="T">Type of resource.</typeparam>
public class CachedResource<T> : CachedObservableValue<T>
{
    /// <summary>
    /// Initialize new <see cref="CachedResource{T}"/> instance.
    /// </summary>
    /// <param name="resourceHost">Resource host.</param>
    /// <param name="key">Resource key.</param>
    /// <param name="converter">Resource converter.</param>
    public CachedResource(IResourceHost resourceHost, object key, Func<object?, object?>? converter = null) : base(resourceHost.GetResourceObservable(key, converter).Cast<object?, T>()) =>
        this.ResourceKey = key;


    /// <summary>
    /// Get resource key.
    /// </summary>
    [ThreadSafe]
    public object ResourceKey { get; }
}