using System;

namespace CarinaStudio
{
    /// <summary>
    /// Extensions for <see cref="IObservable{T}"/>.
    /// </summary>
    public static class ObservableExtensions
    {
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
        /// <typeparam name="T">Type of value of <see cref="IObservable{T}"/>.</typeparam>
        /// <returns>Token of subscribed observer.</returns>
        public static IDisposable Subscribe<T>(this IObservable<T> observable, Action onNext) =>
            observable.Subscribe(new Observer<T>(onNext));
        

        /// <summary>
        /// Subscribe given action to observe value change.
        /// </summary>
        /// <param name="observable"><see cref="IObservable{T}"/>.</param>
        /// <param name="onNext">Action to observe value change.</param>
        /// <typeparam name="T">Type of value of <see cref="IObservable{T}"/>.</typeparam>
        /// <returns>Token of subscribed observer.</returns>
        public static IDisposable Subscribe<T>(this IObservable<T> observable, Action<T> onNext) =>
            observable.Subscribe(new Observer<T>(onNext));
        

        /// <summary>
        /// Subscribe given function to observe value change.
        /// </summary>
        /// <param name="observable"><see cref="IObservable{T}"/>.</param>
        /// <param name="onNext">Function to observe value change.</param>
        /// <typeparam name="T">Type of value of <see cref="IObservable{T}"/>.</typeparam>
        /// <typeparam name="R">Type returned value of <paramref name="onNext"/>.</typeparam>
        /// <returns>Token of subscribed observer.</returns>
        public static IDisposable Subscribe<T, R>(this IObservable<T> observable, Func<R> onNext) =>
            observable.Subscribe(new Observer<T>(_ => onNext()));
        

        /// <summary>
        /// Subscribe given function to observe value change.
        /// </summary>
        /// <param name="observable"><see cref="IObservable{T}"/>.</param>
        /// <param name="onNext">Function to observe value change.</param>
        /// <typeparam name="T">Type of value of <see cref="IObservable{T}"/>.</typeparam>
        /// <typeparam name="R">Type returned value of <paramref name="onNext"/>.</typeparam>
        /// <returns>Token of subscribed observer.</returns>
        public static IDisposable Subscribe<T, R>(this IObservable<T> observable, Func<T, R> onNext) =>
            observable.Subscribe(new Observer<T>(value => onNext(value)));
        

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
}