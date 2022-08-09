using System;

namespace CarinaStudio
{
    // Implementation of IObservable to convert type of value.
    class TypeConvertedObservable<TIn, TOut> : IObservable<TOut>
    {
        // Fields.
        readonly IObservable<TIn> source;


        // Token of observer.
        class ObserverToken : IDisposable
        {
            // Fields.
            readonly IDisposable observerToken;

            // Constructor.
            public ObserverToken(IObservable<TIn> observable, IObserver<TOut> observer)
            {
                var observerAdapter = new Observer<TIn>(value =>
                {
                    if (value is TOut outValue)
                        observer.OnNext(outValue);
                }, observer.OnCompleted, observer.OnError);
                this.observerToken = observable.Subscribe(observerAdapter);
            }

            // Dispose.
            public void Dispose() =>
                this.observerToken.Dispose();
        }


        // Constructor.
        public TypeConvertedObservable(IObservable<TIn> source) =>
            this.source = source;


        /// <inheritdoc/>
        public IDisposable Subscribe(IObserver<TOut> observer) =>
            new ObserverToken(this.source, observer);
    }
}