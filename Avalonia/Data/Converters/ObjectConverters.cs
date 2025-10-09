using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace CarinaStudio.Data.Converters;

/// <summary>
/// Predefined <see cref="IValueConverter"/>s to convert from <see cref="object"/>.
/// </summary>
public static class ObjectConverters
{
    /// <summary>
    /// Convert from <see cref="object"/> to <see cref="bool"/> if value equals to parameter.
    /// </summary>
    public static readonly IValueConverter IsEquivalentTo = new EqualityConverter(true);
    /// <summary>
    /// Convert from <see cref="object"/> to <see cref="bool"/> if value doesn't equal to parameter.
    /// </summary>
    public static readonly IValueConverter IsNotEquivalentTo = new EqualityConverter(false);


    // Converter for equality.
    class EqualityConverter(bool equivalent) : IValueConverter
    {
        // Convert.
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (targetType != typeof(object) && targetType != typeof(bool))
                return null;
            if (equivalent)
                return value?.Equals(parameter) ?? (parameter is null);
            return !value?.Equals(parameter) ?? (parameter is not null);
        }

        // Convert back.
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            null;
    }
}