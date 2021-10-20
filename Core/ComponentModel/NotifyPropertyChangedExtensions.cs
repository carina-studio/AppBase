using System;
using System.ComponentModel;
using System.Threading;

namespace CarinaStudio.ComponentModel
{
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
            volatile bool isDisposed;
            readonly INotifyPropertyChanged target;

            // Constructor.
            public WeakEventHandlerAdapter(INotifyPropertyChanged target, PropertyChangedEventHandler handler)
            {
                this.handlerRef = new WeakReference<PropertyChangedEventHandler>(handler);
                this.target = target;
                target.PropertyChanged += this.OnPropertyChanged;
            }

            // Dispose.
            public void Dispose()
            {
                lock (this)
                {
                    if (this.isDisposed)
                        return;
                    this.isDisposed = true;
                }
                this.target.PropertyChanged -= this.OnPropertyChanged;
            }

            // Entry of PropertyChanged event handler.
            void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
            {
                if (this.handlerRef.TryGetTarget(out var handler) && handler != null)
                    handler(sender, e);
                else
                {
                    var syncContext = SynchronizationContext.Current;
                    if (syncContext != null)
                        syncContext.Post(_ => this.Dispose(), null);
                    else
                        this.Dispose();
                }
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
    }
}
