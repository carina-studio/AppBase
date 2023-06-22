using Avalonia.Data.Converters;
using System;
using System.Diagnostics;
// ReSharper disable CompareOfFloatsByEqualityOperator

namespace CarinaStudio.Data.Converters
{
    /// <summary>
    /// Provide <see cref="IValueConverter"/> to convert from numbers.
    /// </summary>
    public static class NumberConverters
    {
        /// <summary>
        /// <see cref="IValueConverter"/> to convert from negative number to True.
        /// </summary>
        public static readonly IValueConverter IsNegative = new FuncValueConverter<object?, bool>(value =>
        {
            if (value is int intValue)
                return intValue < 0;
            if (value is double doubleValue)
                return doubleValue < -0.0;
            if (value is byte || value is ushort || value is uint || value is ulong)
                return false;
            if (value is IConvertible convertible)
            {
                try
                {
                    return convertible.ToInt64(null) < 0;
                }
                catch
                {
                    Debug.WriteLine($"Unable to convert from '{value}' to number");
                }
            }
            return false;
        });


        /// <summary>
        /// <see cref="IValueConverter"/> to convert from non-negative number to True.
        /// </summary>
        public static readonly IValueConverter IsNonNegative = new FuncValueConverter<object?, bool>(value =>
        {
            if (value is int intValue)
                return intValue >= 0;
            if (value is double doubleValue)
                return doubleValue >= -0.0;
            if (value is byte || value is ushort || value is uint || value is ulong)
                return true;
            if (value is IConvertible convertible)
            {
                try
                {
                    return convertible.ToInt64(null) >= 0;
                }
                catch
                {
                    Debug.WriteLine($"Unable to convert from '{value}' to number");
                }
            }
            return false;
        });


        /// <summary>
        /// <see cref="IValueConverter"/> to convert from non-positive number to True.
        /// </summary>
        public static readonly IValueConverter IsNonPositive = new FuncValueConverter<object?, bool>(value =>
        {
            if (value is int intValue)
                return intValue <= 0;
            if (value is double doubleValue)
                return doubleValue <= 0.0;
            if (value is ulong ulongValue)
                return ulongValue == 0;
            if (value is IConvertible convertible)
            {
                try
                {
                    return convertible.ToInt64(null) <= 0;
                }
                catch
                {
                    Debug.WriteLine($"Unable to convert from '{value}' to number");
                }
            }
            return false;
        });


        /// <summary>
        /// <see cref="IValueConverter"/> to convert from non-zero number to True.
        /// </summary>
        public static readonly IValueConverter IsNonZero = new FuncValueConverter<object?, bool>(value =>
        {
            if (value is int intValue)
                return intValue != 0;
            if (value is double doubleValue)
                return doubleValue != 0.0 && doubleValue != -0.0;
            if (value is ulong ulongValue)
                return ulongValue > 0;
            if (value is bool boolValue)
                return boolValue;
            if (value is IConvertible convertible)
            {
                try
                {
                    return convertible.ToInt64(null) != 0;
                }
                catch
                {
                    Debug.WriteLine($"Unable to convert from '{value}' to number");
                }
            }
            return false;
        });


        /// <summary>
        /// <see cref="IValueConverter"/> to convert from positive number to True.
        /// </summary>
        public static readonly IValueConverter IsPositive = new FuncValueConverter<object?, bool>(value =>
        {
            if (value is int intValue)
                return intValue > 0;
            if (value is double doubleValue)
                return doubleValue > 0.0;
            if (value is ulong ulongValue)
                return ulongValue > 0;
            if (value is IConvertible convertible)
            {
                try
                {
                    return convertible.ToInt64(null) > 0;
                }
                catch
                {
                    Debug.WriteLine($"Unable to convert from '{value}' to number");
                }
            }
            return false;
        });


        /// <summary>
        /// <see cref="IValueConverter"/> to convert from zero number to True.
        /// </summary>
        public static readonly IValueConverter IsZero = new FuncValueConverter<object?, bool>(value =>
        {
            if (value is int intValue)
                return intValue == 0;
            if (value is double doubleValue)
                return doubleValue == 0.0 || doubleValue == -0.0;
            if (value is ulong ulongValue)
                return ulongValue == 0;
            if (value is bool boolValue)
                return !boolValue;
            if (value is IConvertible convertible)
            {
                try
                {
                    return convertible.ToInt64(null) == 0;
                }
                catch
                {
                    Debug.WriteLine($"Unable to convert from '{value}' to number");
                }
            }
            return false;
        });
    }
}