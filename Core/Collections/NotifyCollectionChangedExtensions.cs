using System;
using System.Collections.Specialized;
using System.Threading;

namespace CarinaStudio.Collections
{
    /// <summary>
    /// Extensions for <see cref="INotifyCollectionChanged"/>.
    /// </summary>
    public static class NotifyCollectionChangedExtensions
    {
        // Adapter of weak event handler.
        class WeakEventHandlerAdapter : IDisposable
        {
            // Fields.
            readonly WeakReference<NotifyCollectionChangedEventHandler> handlerRef;
            int isDisposed;
            readonly SynchronizationContext? syncContext;
            readonly INotifyCollectionChanged target;

            // Constructor.
            public WeakEventHandlerAdapter(INotifyCollectionChanged target, NotifyCollectionChangedEventHandler handler)
            {
                this.handlerRef = new WeakReference<NotifyCollectionChangedEventHandler>(handler);
                this.syncContext = SynchronizationContext.Current;
                this.target = target;
                target.CollectionChanged += this.OnCollectionChanged;
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
                        this.syncContext.Post(_ => this.target.CollectionChanged -= this.OnCollectionChanged, null);
                        return;
                    }
                    // ReSharper disable EmptyGeneralCatchClause
                    catch
                    { }
                    // ReSharper restore EmptyGeneralCatchClause
                }
                this.target.CollectionChanged -= this.OnCollectionChanged;
            }

            // Entry of CollectionChanged event handler.
            void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
            {
                if (this.handlerRef.TryGetTarget(out var handler))
                    handler(sender, e);
                else
                    this.Dispose();
            }
        }


        /// <summary>
        /// Add weak event handler to <see cref="INotifyCollectionChanged.CollectionChanged"/>.
        /// </summary>
        /// <param name="target"><see cref="INotifyCollectionChanged"/>.</param>
        /// <param name="handler">Event handler.</param>
        /// <returns><see cref="IDisposable"/> which represents added weak event handler. You can call <see cref="IDisposable.Dispose"/> to remove weak event handler explicitly.</returns>
        public static IDisposable AddWeakCollectionChangedEventHandler(this INotifyCollectionChanged target, NotifyCollectionChangedEventHandler handler) =>
            new WeakEventHandlerAdapter(target, handler);
    }
}
