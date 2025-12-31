using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace CarinaStudio.ComponentModel;

/// <summary>
/// Extensions for <see cref="INotifyPropertyChanged"/>.
/// </summary>
public static class NotifyPropertyChangedExtensions
{
    // Adapter of weak event handler.
    class WeakEventHandlerAdapter : IDisposable
    {
        // Fields.
        readonly WeakReference<PropertyChangedEventHandler> handlerRef;
        int isDisposed;
        readonly SynchronizationContext? syncContext;
        readonly INotifyPropertyChanged target;

        // Constructor.
        public WeakEventHandlerAdapter(INotifyPropertyChanged target, PropertyChangedEventHandler handler)
        {
            this.handlerRef = new WeakReference<PropertyChangedEventHandler>(handler);
            this.syncContext = SynchronizationContext.Current;
            this.target = target;
            target.PropertyChanged += this.OnPropertyChanged;
        }

        // Dispose.
        public void Dispose()
        {
            if (Interlocked.Exchange(ref this.isDisposed, 1) != 0)
                return;
            if (this.syncContext != null && this.syncContext != SynchronizationContext.Current)
            {
                try
                {
                    this.syncContext.Post(_ => this.target.PropertyChanged -= this.OnPropertyChanged, null);
                    return;
                }
                // ReSharper disable EmptyGeneralCatchClause
                catch
                { }
                // ReSharper restore EmptyGeneralCatchClause
            }
            this.target.PropertyChanged -= this.OnPropertyChanged;
        }

        // Entry of PropertyChanged event handler.
        void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (this.handlerRef.TryGetTarget(out var handler))
                handler(sender, e);
            else
                this.Dispose();
        }
    }


    /// <summary>
    /// Add weak event handler to <see cref="INotifyPropertyChanged.PropertyChanged"/>.
    /// </summary>
    /// <param name="target"><see cref="INotifyPropertyChanged"/>.</param>
    /// <param name="handler">Event handler.</param>
    /// <returns><see cref="IDisposable"/> which represents added weak event handler. You can call <see cref="IDisposable.Dispose"/> to remove weak event handler explicitly.</returns>
    public static IDisposable AddWeakPropertyChangedEventHandler(this INotifyPropertyChanged target, PropertyChangedEventHandler handler) =>
        new WeakEventHandlerAdapter(target, handler);


    /// <summary>
    /// Wait for the property changing to desired value asynchronously.
    /// </summary>
    /// <param name="obj">Object owns the property.</param>
    /// <param name="propertyName">Name of property.</param>
    /// <param name="checkPropertyValue">Function to check whether the value is desired one or not.</param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/> to cancel waiting.</param>
    /// <typeparam name="T">Type of object owns the property.</typeparam>
    /// <returns>Task of waiting for property value.</returns>
    public static async Task WaitForPropertyChangeAsync<T>(this T obj, string propertyName, Predicate<T> checkPropertyValue, CancellationToken cancellationToken = default) where T : INotifyPropertyChanged
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (checkPropertyValue(obj))
            return;
        var taskCompletionSource = new TaskCompletionSource();
        var propertyChangedHandler = new PropertyChangedEventHandler((_, e) =>
        {
            if (!taskCompletionSource.Task.IsCompleted && e.PropertyName == propertyName && checkPropertyValue(obj))
                taskCompletionSource.TrySetResult();
        });
        obj.PropertyChanged += propertyChangedHandler;
        await using var _ = cancellationToken.Register(() => taskCompletionSource.TrySetCanceled());
        try
        {
            await taskCompletionSource.Task;
        }
        finally
        {
            obj.PropertyChanged -= propertyChangedHandler;
        }
    }
}