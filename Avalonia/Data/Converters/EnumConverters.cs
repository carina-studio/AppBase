using Avalonia.Data.Converters;
using System;
using System.Collections;
using System.Globalization;

namespace CarinaStudio.Data.Converters
{
    /// <summary>
    /// Predefined <see cref="IValueConverter"/>s for enumeration.
    /// </summary>
    public static class EnumConverters
    {
        /// <summary>
        /// Convert from <see cref="String"/> to the enumeration.
        /// </summary>
        /// <remarks>The parameter of <see cref="IValueConverter.Convert(object?, Type, object?, CultureInfo)"/> must be <see cref="Type"/> of enumeration.</remarks>
        public static readonly IValueConverter Parsing = new ParsingConverter();


        /// <summary>
        /// Convert from <see cref="Type"/> to all values of the enumeration.
        /// </summary>
        public static readonly IValueConverter Values = new ValuesConverter();


        // Converter to parse value.
        class ParsingConverter : IValueConverter
        {
            /// <inheritdoc/>.
            public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
            {
                if (targetType != typeof(object) && !targetType.IsEnum)
                    return null;
                if (value == null || parameter is not Type enumType || !enumType.IsEnum)
                    return null;
                if (Enum.TryParse(enumType, value.ToString(), out var e))
                    return e;
                return null;
            }

            /// <inheritdoc/>.
            public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            {
                if (targetType != typeof(object) && targetType != typeof(string))
                    return null;
                if (value is not Enum e)
                    return null;
                return e.ToString();
            }
        }


        // Converter to get values.
        class ValuesConverter : IValueConverter
        {
            /// <inheritdoc/>.
            public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
            {
                if (targetType != typeof(object) && !typeof(IEnumerable).IsAssignableFrom(targetType))
                    return null;
                if (value is not Type type || !type.IsEnum)
                    return null;
                return Enum.GetValues(type);
            }

            /// <inheritdoc/>.
            public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
                null;
        }
    }
}