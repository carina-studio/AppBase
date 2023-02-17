using System;
using System.Threading;

namespace CarinaStudio
{
    /// <summary>
    /// <see cref="ObservableValue{T}"/> which caches a local value from source.
    /// </summary>
    /// <typeparam name="T">Type of value.</typeparam>
    public class CachedObservableValue<T> : ObservableValue<T>
    {
        // Observer to source value.
        class WeakObserver : IObserver<T>
        {
            // Fields.
            readonly WeakReference<CachedObservableValue<T>> owner;

            // Constructor.
            public WeakObserver(CachedObservableValue<T> owner) =>
                this.owner = new(owner);
            
            /// <inheritdoc/>
            public void OnCompleted()
            { }

            /// <inheritdoc/>
            public void OnError(Exception error)
            { }

            /// <inheritdoc/>
            public void OnNext(T value)
            {
                if (this.owner.TryGetTarget(out var owner))
                    owner.Value = value;
            }
        }


        // Fields.
        IDisposable? sourceObserverToken;
        SynchronizationContext? sourceSyncContext;
        readonly Func<T>? updateValueFunc;


        /// <summary>
        /// Initialize new <see cref="CachedObservableValue{T}"/> instance.
        /// </summary>
        /// <param name="updateValueFunc">Function to get value from source.</param>
        public CachedObservableValue(Func<T> updateValueFunc) : base(updateValueFunc())
        {
            this.updateValueFunc = updateValueFunc;
#pragma warning disable CA1816
            GC.SuppressFinalize(this);
#pragma warning restore CA1816
        }
        

        /// <summary>
        /// Initialize new <see cref="CachedObservableValue{T}"/> instance.
        /// </summary>
        /// <param name="source">Source value.</param>
        /// <remarks>The source will be observed with weak observer internally. The observer will be unsubscribed in the thread which creates the instance if possible.</remarks>
        public CachedObservableValue(IObservable<T> source)
        {
            this.sourceObserverToken = source.Subscribe(new WeakObserver(this));
            this.sourceSyncContext = SynchronizationContext.Current;
        }
        

        /// <summary>
        /// Finalizer.
        /// </summary>
        ~CachedObservableValue()
        {
            if (this.sourceSyncContext != null)
            {
                this.sourceSyncContext.Post(_ =>
                    this.sourceObserverToken = this.sourceObserverToken.DisposeAndReturnNull(), null);
                this.sourceSyncContext = null;
            }
            else
                this.sourceObserverToken = this.sourceObserverToken.DisposeAndReturnNull();
        }
            

        /// <summary>
        /// Invalidate cached value and update immediately.
        /// </summary>
        public void Invalidate()
        {
            if (this.updateValueFunc != null)
                this.Value = this.updateValueFunc();
        }
    }
}