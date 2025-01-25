using System;

namespace CarinaStudio;

/// <summary>
/// Extensions for <see cref="IObservable{T}"/>.
/// </summary>
public static class ObservableExtensions
{
    // Special implementation of IObservable.
    class ObserverImpl<T> : IObserver<T>
    {
        // Fields.
        readonly Action<T> onNextAction;

        // Constructor.
        public ObserverImpl(Action<T> onNextAction, bool skipOnNext)
        {
            this.onNextAction = onNextAction;
            this.SkipOnNext = skipOnNext;
        }
        public ObserverImpl(Action onNextAction, bool skipOnNext)
        {
            this.onNextAction = _ => onNextAction();
            this.SkipOnNext = skipOnNext;
        }

        // Whether the call of OnNext should be skipped or not.
        public bool SkipOnNext = false;

        // Implementations.
        void IObserver<T>.OnCompleted()
        { }
        void IObserver<T>.OnError(Exception error)
        { }
        void IObserver<T>.OnNext(T value)
        {
            if (!this.SkipOnNext)
                this.onNextAction(value);
        }
    }


    /// <summary>
    /// Cast given <see cref="IObservable{T}"/> to another type of <see cref="IObservable{T}"/>.
    /// </summary>
    /// <param name="observable"><see cref="IObservable{T}"/>.</param>
    /// <typeparam name="TIn">Type of value of source <see cref="IObservable{T}"/>.</typeparam>
    /// <typeparam name="TOut">Type of value of target <see cref="IObservable{T}"/>.</typeparam>
    /// <returns><see cref="IObservable{T}"/>.</returns>
    public static IObservable<TOut> Cast<TIn, TOut>(this IObservable<TIn> observable) =>
        new TypeConvertedObservable<TIn, TOut>(observable);


    /// <summary>
    /// Create <see cref="IObservable{T}"/> which inverts the source <see cref="IObservable{T}"/>.
    /// </summary>
    /// <param name="observable">Source <see cref="IObservable{T}"/>.</param>
    /// <returns><see cref="IObservable{T}"/> with inverted value.</returns>
    public static IObservable<bool> Invert(this IObservable<bool> observable) =>
        new InvertedObservableBoolean(observable);
    

    /// <summary>
    /// Subscribe given action to observe value change.
    /// </summary>
    /// <param name="observable"><see cref="IObservable{T}"/>.</param>
    /// <param name="onNext">Action to observe value change.</param>
    /// <param name="skipOnNextDuringSubscription">True to skip call of <paramref name="onNext"/> during subscription.</param>
    /// <typeparam name="T">Type of value of <see cref="IObservable{T}"/>.</typeparam>
    /// <returns>Token of subscribed observer.</returns>
    public static IDisposable Subscribe<T>(this IObservable<T> observable, Action onNext, bool skipOnNextDuringSubscription = false) 
    {
        var observer = new ObserverImpl<T>(onNext, skipOnNextDuringSubscription);
        var unsubscriber = observable.Subscribe(observer);
        observer.SkipOnNext = false;
        return unsubscriber;
    }
    

    /// <summary>
    /// Subscribe given action to observe value change.
    /// </summary>
    /// <param name="observable"><see cref="IObservable{T}"/>.</param>
    /// <param name="onNext">Action to observe value change.</param>
    /// <param name="skipOnNextDuringSubscription">True to skip call of <paramref name="onNext"/> during subscription.</param>
    /// <typeparam name="T">Type of value of <see cref="IObservable{T}"/>.</typeparam>
    /// <returns>Token of subscribed observer.</returns>
    public static IDisposable Subscribe<T>(this IObservable<T> observable, Action<T> onNext, bool skipOnNextDuringSubscription = false)
    {
        var observer = new ObserverImpl<T>(onNext, skipOnNextDuringSubscription);
        var unsubscriber = observable.Subscribe(observer);
        observer.SkipOnNext = false;
        return unsubscriber;
    }
    

    /// <summary>
    /// Subscribe given function to observe value change.
    /// </summary>
    /// <param name="observable"><see cref="IObservable{T}"/>.</param>
    /// <param name="onNext">Function to observe value change.</param>
    /// <param name="skipOnNextDuringSubscription">True to skip call of <paramref name="onNext"/> during subscription.</param>
    /// <typeparam name="T">Type of value of <see cref="IObservable{T}"/>.</typeparam>
    /// <typeparam name="R">Type returned value of <paramref name="onNext"/>.</typeparam>
    /// <returns>Token of subscribed observer.</returns>
    public static IDisposable Subscribe<T, R>(this IObservable<T> observable, Func<R> onNext, bool skipOnNextDuringSubscription = false)
    {
        var observer = new ObserverImpl<T>(_ => onNext(), skipOnNextDuringSubscription);
        var unsubscriber = observable.Subscribe(observer);
        observer.SkipOnNext = false;
        return unsubscriber;
    }


    /// <summary>
    /// Subscribe given function to observe value change.
    /// </summary>
    /// <param name="observable"><see cref="IObservable{T}"/>.</param>
    /// <param name="onNext">Function to observe value change.</param>
    /// <param name="skipOnNextDuringSubscription">True to skip call of <paramref name="onNext"/> during subscription.</param>
    /// <typeparam name="T">Type of value of <see cref="IObservable{T}"/>.</typeparam>
    /// <typeparam name="R">Type returned value of <paramref name="onNext"/>.</typeparam>
    /// <returns>Token of subscribed observer.</returns>
    public static IDisposable Subscribe<T, R>(this IObservable<T> observable, Func<T, R> onNext, bool skipOnNextDuringSubscription = false)
    {
        var observer = new ObserverImpl<T>(value => onNext(value), skipOnNextDuringSubscription);
        var unsubscriber = observable.Subscribe(observer);
        observer.SkipOnNext = false;
        return unsubscriber;
    }
    

    /// <summary>
    /// Subscribe a weak reference to given observer to observe value change.
    /// </summary>
    /// <param name="observable"><see cref="IObservable{T}"/>.</param>
    /// <param name="observer">Observer.</param>
    /// <typeparam name="T">Type of value of <see cref="IObservable{T}"/>.</typeparam>
    /// <returns>Token of subscribed observer.</returns>
    public static IDisposable WeakSubscribe<T>(this IObservable<T> observable, IObserver<T> observer)
    {
        var weakObserver = new WeakObserver<T>(observer);
        var token = observable.Subscribe(weakObserver);
        weakObserver.SubscriptionToken = token;
        return token;
    }
}