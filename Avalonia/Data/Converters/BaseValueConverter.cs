using Avalonia.Data.Converters;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace CarinaStudio.Data.Converters
{
    /// <summary>
    /// Base implementation of <see cref="IValueConverter"/>.
    /// </summary>
    /// <typeparam name="TIn">Type of value.</typeparam>
    /// <typeparam name="TOut">Type of converted value.</typeparam>
    public abstract class BaseValueConverter<TIn, TOut> : IValueConverter
    {
        /// <summary>
        /// Convert value.
        /// </summary>
        /// <param name="value">Value.</param>
        /// <param name="parameter">Parameter.</param>
        /// <param name="culture">Culture.</param>
        /// <returns>Converted value.</returns>
        [return: MaybeNull]
        protected abstract TOut Convert(TIn value, object? parameter, CultureInfo culture);


        /// <inheritdoc/>.
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (targetType != typeof(object) && targetType != typeof(TOut))
                return null;
            if (value is not TIn input)
                return null;
            return this.Convert(input, parameter, culture);
        }


        /// <summary>
        /// Convert value back.
        /// </summary>
        /// <param name="value">Value.</param>
        /// <param name="parameter">Parameter.</param>
        /// <param name="culture">Culture.</param>
        /// <returns>Converted value.</returns>
        [return: MaybeNull]
        protected virtual TIn ConvertBack(TOut value, object? parameter, CultureInfo culture) => default;


        /// <inheritdoc/>.
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (targetType != typeof(object) && targetType != typeof(TIn))
                return null;
            if (value is not TOut input)
                return null;
            return this.ConvertBack(input, parameter, culture);
        }
    }
}