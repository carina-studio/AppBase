using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace CarinaStudio.Data.Converters
{
    /// <summary>
    /// Predefined <see cref="IValueConverter"/>s to convert from value to <see cref="bool"/>.
    /// </summary>
    public static class ValueToBooleanConverters
    {
        /// <summary>
        /// Convert to True if value is empty string.
        /// </summary>
        public static readonly IValueConverter EmptyStringToTrue = new EmptyStringToBooleanConverterImpl(true);
        /// <summary>
        /// Convert to True if value is non-empty string.
        /// </summary>
        public static readonly IValueConverter NonEmptyStringToTrue = new EmptyStringToBooleanConverterImpl(false);
        /// <summary>
        /// Convert to True if value is non-Null.
        /// </summary>
        public static readonly IValueConverter NonNullToTrue = new NullToBooleanConverterImpl(false);
        /// <summary>
        /// Convert to True if value is Null.
        /// </summary>
        public static readonly IValueConverter NullToTrue = new NullToBooleanConverterImpl(true);


        // Convert from empty/non-empty string to boolean.
        class EmptyStringToBooleanConverterImpl : IValueConverter
        {
            // Fields.
            readonly bool emptyIsTrue;

            // Constructor.
            public EmptyStringToBooleanConverterImpl(bool emptyIsTrue) => this.emptyIsTrue = emptyIsTrue;

            // Convert.
            public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
            {
                if (typeof(object) != targetType && typeof(bool) != targetType)
                    return null;
                if (value == null)
                    return this.emptyIsTrue;
                if (value is string str)
                    return this.emptyIsTrue ? str.Length == 0 : str.Length > 0;
                return null;
            }

            // Convert back.
            public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => null;
        }


        // Convert from null/non-null to boolean.
        class NullToBooleanConverterImpl : IValueConverter
        {
            // Fields.
            readonly bool nullIsTrue;

            // Constructor.
            public NullToBooleanConverterImpl(bool nullIsTrue) => this.nullIsTrue = nullIsTrue;

            // Convert.
            public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
            {
                if (typeof(object) != targetType && typeof(bool) != targetType)
                    return null;
                return this.nullIsTrue ? (value == null) : (value != null);
            }

            // Convert back.
            public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => null;
        }
    }
}
