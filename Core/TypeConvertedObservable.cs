using System;

namespace CarinaStudio;

// Implementation of IObservable to convert type of value.
class TypeConvertedObservable<TIn, TOut>(IObservable<TIn> source) : IObservable<TOut>
{
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
    

    /// <inheritdoc/>
    public IDisposable Subscribe(IObserver<TOut> observer) =>
        new ObserverToken(source, observer);
}