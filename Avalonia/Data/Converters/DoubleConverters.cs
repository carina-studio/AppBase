using Avalonia.Data.Converters;
using CarinaStudio.Collections;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace CarinaStudio.Data.Converters
{
    /// <summary>
    /// Predefined <see cref="IMultiValueConverter"/> for operations on <see cref="double"/>.
    /// </summary>
    public static class DoubleConverters
    {
        /// <summary>
        /// <see cref="IMultiValueConverter"/> perform addition on <see cref="double"/> values.
        /// </summary>
        public static readonly IMultiValueConverter Addition = new AdditionConverter();
        /// <summary>
        /// <see cref="IMultiValueConverter"/> perform division on <see cref="double"/> values.
        /// </summary>
        public static readonly IMultiValueConverter Division = new DivisionConverter();
        /// <summary>
        /// <see cref="IMultiValueConverter"/> perform Mmltiplication on <see cref="double"/> values.
        /// </summary>
        public static readonly IMultiValueConverter Multiplication = new MultiplicationConverter();
        /// <summary>
        /// <see cref="IMultiValueConverter"/> perform subtraction on <see cref="double"/> values.
        /// </summary>
        public static readonly IMultiValueConverter Subtraction = new SubtractionConverter();


        // Addition.
        class AdditionConverter : IMultiValueConverter
        {
            // Convert.
            public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
            {
                if (typeof(object) != targetType && typeof(double) != targetType)
                    return null;
                var result = 0.0;
                foreach (var value in values)
                {
                    if (value is double doubleValue)
                        result += doubleValue;
                    else if (value is IConvertible convertible)
                        result += convertible.ToDouble(culture);
                }
                return result;
            }
        }


        // Division.
        class DivisionConverter : IMultiValueConverter
        {
            // Convert.
            public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
            {
                if (typeof(object) != targetType && typeof(double) != targetType)
                    return null;
                var result = 0.0;
                var isFirstValueSet = false;
                foreach (var value in values)
                {
                    if (value is double doubleValue)
                    {
                        if (isFirstValueSet)
                            result /= doubleValue;
                        else
                        {
                            result = doubleValue;
                            isFirstValueSet = true;
                        }
                    }
                    else if (value is IConvertible convertible)
                    {
                        if (isFirstValueSet)
                            result /= convertible.ToDouble(culture);
                        else
                        {
                            result = convertible.ToDouble(culture);
                            isFirstValueSet = true;
                        }
                    }
                }
                return result;
            }
        }


        // Multiplication.
        class MultiplicationConverter : IMultiValueConverter
        {
            // Convert.
            public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
            {
                if (typeof(object) != targetType && typeof(double) != targetType)
                    return null;
                var result = 0.0;
                var isFirstValueSet = false;
                foreach (var value in values)
                {
                    if (value is double doubleValue)
                    {
                        if (isFirstValueSet)
                            result *= doubleValue;
                        else
                        {
                            result = doubleValue;
                            isFirstValueSet = true;
                        }
                    }
                    else if (value is IConvertible convertible)
                    {
                        if (isFirstValueSet)
                            result *= convertible.ToDouble(culture);
                        else
                        {
                            result = convertible.ToDouble(culture);
                            isFirstValueSet = true;
                        }
                    }
                }
                return result;
            }
        }


        // Subtraction.
        class SubtractionConverter : IMultiValueConverter
        {
            // Convert.
            public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
            {
                if (typeof(object) != targetType && typeof(double) != targetType)
                    return null;
                var result = 0.0;
                var isFirstValueSet = false;
                foreach (var value in values)
                {
                    if (value is double doubleValue)
                    {
                        if (isFirstValueSet)
                            result -= doubleValue;
                        else
                        {
                            result = doubleValue;
                            isFirstValueSet = true;
                        }
                    }
                    else if (value is IConvertible convertible)
                    {
                        if (isFirstValueSet)
                            result -= convertible.ToDouble(culture);
                        else
                        {
                            result = convertible.ToDouble(culture);
                            isFirstValueSet = true;
                        }
                    }
                }
                return result;
            }
        }
    }
}