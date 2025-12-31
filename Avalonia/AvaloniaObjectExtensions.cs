using Avalonia;
using Avalonia.Controls;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CarinaStudio;

/// <summary>
/// Extensions for <see cref="AvaloniaObject"/>.
/// </summary>
public static class AvaloniaObjectExtensions
{
    /// <summary>
    /// Bind property to given resource.
    /// </summary>
    /// <param name="obj">Target object.</param>
    /// <param name="property">Property to bind.</param>
    /// <param name="resourceHost"><see cref="IResourceHost"/> to find resource.</param>
    /// <param name="resourceKey">Resource key.</param>
    /// <returns>Token of binding.</returns>
    public static IDisposable BindToResource(this AvaloniaObject obj, AvaloniaProperty property, IResourceHost resourceHost, object resourceKey) =>
        obj.Bind(property, new CachedResource<object?>(resourceHost, resourceKey));
    
    
    /// <summary>
    /// Wait for property changing to desired value asynchronously.
    /// </summary>
    /// <param name="obj">Object owns the property.</param>
    /// <param name="property">The property.</param>
    /// <param name="value">Desired value.</param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/> to cancel waiting.</param>
    /// <typeparam name="TObj">Type of object.</typeparam>
    /// <returns>Task of waiting for property value.</returns>
    public static async Task WaitForPropertyChangeAsync<TObj>(this TObj obj, AvaloniaProperty property, object? value, CancellationToken cancellationToken = default) where TObj : AvaloniaObject
    {
        obj.VerifyAccess();
        cancellationToken.ThrowIfCancellationRequested();
        // ReSharper disable once MergeConditionalExpression
        if (value is not null ? value.Equals(obj.GetValue(property)) : obj.GetValue(property) is null)
            return;
        var taskCompletionSource = new TaskCompletionSource();
        await using var ctr = cancellationToken.Register(() => taskCompletionSource.TrySetCanceled());
        using var st = obj.GetObservable(property).Subscribe(newValue =>
        {
            if (!taskCompletionSource.Task.IsCompleted && (value?.Equals(newValue) ?? newValue is null))
                taskCompletionSource.TrySetResult();
        }, skipOnNextDuringSubscription: true);
        await taskCompletionSource.Task;
    }


    /// <summary>
    /// Wait for property changing to desired value asynchronously.
    /// </summary>
    /// <param name="obj">Object owns the property.</param>
    /// <param name="property">The property.</param>
    /// <param name="value">Desired value.</param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/> to cancel waiting.</param>
    /// <typeparam name="TObj">Type of object.</typeparam>
    /// <typeparam name="TValue">Type of property value.</typeparam>
    /// <returns>Task of waiting for property value.</returns>
    public static Task WaitForPropertyChangeAsync<TObj, TValue>(this TObj obj, AvaloniaProperty<TValue> property, TValue? value, CancellationToken cancellationToken = default) where TObj : AvaloniaObject =>
        obj.WaitForPropertyChangeAsync((AvaloniaProperty)property, value, cancellationToken);
}