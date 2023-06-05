using System;

namespace CarinaStudio
{
    /// <summary>
    /// Base class of <see cref="FixedObservableValue{T}"/>.
    /// </summary>
    public abstract class FixedObservableValue
    {
        /// <summary>
        /// Default instance for False.
        /// </summary>
        public static readonly IObservable<bool> False = new FixedObservableValue<bool>(false);
        /// <summary>
        /// Default instance for Null.
        /// </summary>
        public static readonly IObservable<object?> Null = new FixedObservableValue<object?>(null);
        /// <summary>
        /// Default instance for True.
        /// </summary>
        public static readonly IObservable<bool> True = new FixedObservableValue<bool>(true);
        /// <summary>
        /// Default instance for zero.
        /// </summary>
        public static readonly IObservable<double> ZeroInDouble = new FixedObservableValue<double>(0);
        /// <summary>
        /// Default instance for zero.
        /// </summary>
        public static readonly IObservable<int> ZeroInInt32 = new FixedObservableValue<int>(0);
        /// <summary>
        /// Default instance for zero.
        /// </summary>
        public static readonly IObservable<long> ZeroInInt64 = new FixedObservableValue<long>(0);
        /// <summary>
        /// Default instance for zero.
        /// </summary>
        public static readonly IObservable<float> ZeroInSingle = new FixedObservableValue<float>(0);
        
        
        // Constructor.
        internal FixedObservableValue()
        { }
    }
    
    
    /// <summary>
    /// Implementation of <see cref="IObservable{T}"/> with fixed value.
    /// </summary>
    /// <typeparam name="T">Type of value.</typeparam>
    public class FixedObservableValue<T> : FixedObservableValue, IObservable<T>
    {
        /// <summary>
        /// Initialize new <see cref="FixedObservableValue{T}"/> instance.
        /// </summary>
        /// <param name="value">Value.</param>
        public FixedObservableValue(T value) =>
            this.Value = value;
        

        /// <inheritdoc/>
        public IDisposable Subscribe(IObserver<T> observer)
        {
            observer.OnNext(this.Value);
            return EmptyDisposable.Default;
        }


        /// <summary>
        /// Get value.
        /// </summary>
        public T Value { get; }
    }
}