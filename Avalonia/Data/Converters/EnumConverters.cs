using Avalonia.Data.Converters;
using CarinaStudio.Threading;
using System;
using System.Collections;
using System.Globalization;

namespace CarinaStudio.Data.Converters;

/// <summary>
/// Predefined <see cref="IValueConverter"/>s for enumeration.
/// </summary>
public static class EnumConverters
{
    // Fields.
#if !NET10_0_OR_GREATER
    static IValueConverter? parsing;
    static IValueConverter? values;
#endif


    /// <summary>
    /// Convert from <see cref="String"/> to the enumeration.
    /// </summary>
    /// <remarks>The parameter of <see cref="IValueConverter.Convert(object?, Type, object?, CultureInfo)"/> must be <see cref="Type"/> of enumeration.</remarks>
    public static IValueConverter Parsing
    {
        get
        {
#if NET10_0_OR_GREATER
            field ??= new ParsingConverter();
            return field;
#else
            parsing ??= new ParsingConverter();
            return parsing;
#endif
        }
    }


    /// <summary>
    /// Convert from <see cref="Type"/> to all values of the enumeration.
    /// </summary>
    public static IValueConverter Values
    {
        get
        {
#if NET10_0_OR_GREATER
            field ??= new ValuesConverter();
            return field;
#else
            values ??= new ValuesConverter();
            return values;
#endif
        }
    }


    // Converter to parse value.
    [ThreadSafe]
    class ParsingConverter : IValueConverter
    {
        /// <inheritdoc/>.
        [ThreadSafe]
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
        [ThreadSafe]
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
    [ThreadSafe]
    class ValuesConverter : IValueConverter
    {
        /// <inheritdoc/>.
        [ThreadSafe]
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (targetType != typeof(object) && !typeof(IEnumerable).IsAssignableFrom(targetType))
                return null;
            if (value is not Type type || !type.IsEnum)
                return null;
#if NET7_0_OR_GREATER
            var rawValues = Enum.GetValuesAsUnderlyingType(type);
            var valueCount = rawValues.Length;
            if (valueCount > 0)
            {
                var firstValue = rawValues.GetValue(0);
                var values = new object[valueCount];
                switch (firstValue)
                {
                    case int firstIntValue:
                    {
                        values[0] = Enum.ToObject(type, firstIntValue);
                        for (var i = valueCount - 1; i > 0; --i)
                            values[i] = Enum.ToObject(type, (int)rawValues.GetValue(i)!);
                        break;
                    }
                    case uint firstUintValue:
                    {
                        values[0] = Enum.ToObject(type, firstUintValue);
                        for (var i = valueCount - 1; i > 0; --i)
                            values[i] = Enum.ToObject(type, (uint)rawValues.GetValue(i)!);
                        break;
                    }
                    case byte firstByteValue:
                    {
                        values[0] = Enum.ToObject(type, firstByteValue);
                        for (var i = valueCount - 1; i > 0; --i)
                            values[i] = Enum.ToObject(type, (byte)rawValues.GetValue(i)!);
                        break;
                    }
                    case sbyte firstSbyteValue:
                    {
                        values[0] = Enum.ToObject(type, firstSbyteValue);
                        for (var i = valueCount - 1; i > 0; --i)
                            values[i] = Enum.ToObject(type, (sbyte)rawValues.GetValue(i)!);
                        break;
                    }
                    case short firstShortValue:
                    {
                        values[0] = Enum.ToObject(type, firstShortValue);
                        for (var i = valueCount - 1; i > 0; --i)
                            values[i] = Enum.ToObject(type, (short)rawValues.GetValue(i)!);
                        break;
                    }
                    case ushort firstUshortValue:
                    {
                        values[0] = Enum.ToObject(type, firstUshortValue);
                        for (var i = valueCount - 1; i > 0; --i)
                            values[i] = Enum.ToObject(type, (ushort)rawValues.GetValue(i)!);
                        break;
                    }
                    case long firstLongValue:
                    {
                        values[0] = Enum.ToObject(type, firstLongValue);
                        for (var i = valueCount - 1; i > 0; --i)
                            values[i] = Enum.ToObject(type, (long)rawValues.GetValue(i)!);
                        break;
                    }
                    case ulong firstUlongValue:
                    {
                        values[0] = Enum.ToObject(type, firstUlongValue);
                        for (var i = valueCount - 1; i > 0; --i)
                            values[i] = Enum.ToObject(type, (ulong)rawValues.GetValue(i)!);
                        break;
                    }
                }
                return values;
            }
            return Array.Empty<object>();
#else
            return Enum.GetValues(type);
#endif
        }

        /// <inheritdoc/>.
        [ThreadSafe]
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            null;
    }
}