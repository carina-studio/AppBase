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
        public static IDisposable Subscribe<T, R>(this IObservable<T> observable, Func<T, R> onNext) =>
            observable.Subscribe(new Observer<T>(value => onNext(value)));
    }
}