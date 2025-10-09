using Avalonia.Data.Converters;
using CarinaStudio.Collections;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace CarinaStudio.Data.Converters;

/// <summary>
/// Predefined <see cref="IValueConverter"/>s to convert from <see cref="IComparable"/>s or <see cref="IComparable{T}"/>s.
/// </summary>
public static class ComparableConverters
{
    /// <summary>
    /// Convert from <see cref="IComparable"/> to <see cref="bool"/> if value is greater than parameter.
    /// </summary>
    public static readonly IValueConverter IsGreaterThan = new ComparingConverter(r => r > 0);
    /// <summary>
    /// Convert from <see cref="IComparable"/> to <see cref="bool"/> if value isn't greater than parameter.
    /// </summary>
    public static readonly IValueConverter IsNotGreaterThan = new ComparingConverter(r => r <= 0);
    /// <summary>
    /// Convert from <see cref="IComparable"/> to <see cref="bool"/> if value isn't smaller than parameter.
    /// </summary>
    public static readonly IValueConverter IsNotSmallerThan = new ComparingConverter(r => r >= 0);
    /// <summary>
    /// Convert from <see cref="IComparable"/> to <see cref="bool"/> if value is smaller than parameter.
    /// </summary>
    public static readonly IValueConverter IsSmallerThan = new ComparingConverter(r => r < 0);
    /// <summary>
    /// Select the maximum <see cref="IComparable"/>.
    /// </summary>
    public static readonly IMultiValueConverter Max = new SelectionConverter((x, y, r) => r > 0 ? x : y);
    /// <summary>
    /// Select the minimum <see cref="IComparable"/>.
    /// </summary>
    public static readonly IMultiValueConverter Min = new SelectionConverter((x, y, r) => r < 0 ? x : y);


    // Static fields.
    static readonly Type IComparableType = Type.GetType("System.IComparable`1").AsNonNull();


    // Converter for comparing values.
    class ComparingConverter(Func<int, bool> resultGenerator) : IValueConverter
    {
        // Convert.
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (targetType != typeof(object) && targetType != typeof(bool))
                return null;
            if (value is null || parameter is null)
                return null;
            if (value is IComparable comparable)
            {
                try
                {
                    return resultGenerator(comparable.CompareTo(parameter));
                }
                catch
                {
                    return null;
                }
            }
            var valueType = value.GetType();
            var paramType = parameter.GetType();
#pragma warning disable IL2075
            foreach (var interfaceType in valueType.GetInterfaces())
#pragma warning restore IL2075
            {
                if (interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == IComparableType)
                {
                    if (interfaceType.GenericTypeArguments[0].IsAssignableFrom(paramType))
                    {
#pragma warning disable IL2075
                        var compareToMethod = interfaceType.GetMethod(nameof(IComparable.CompareTo));
                        if (compareToMethod is not null)
                        {
                            var comparingResult = (int)compareToMethod.Invoke(value, [ parameter ])!;
                            return resultGenerator(comparingResult);
                        }
#pragma warning restore IL2075
                    }
                }
            }
            return null;
        }

        // Convert back.
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            null;
    }


    // Converter for selecting value.
    class SelectionConverter(Func<object?, object?, int, object?> selector) : IMultiValueConverter
    {
        // Convert.
        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            var compareToMethod = default(System.Reflection.MethodInfo);
            if (typeof(IComparable).IsAssignableFrom(targetType))
            {
#pragma warning disable IL2070
                compareToMethod = targetType.GetMethod(nameof(IComparable.CompareTo), 0, [ typeof(object) ]).AsNonNull();
#pragma warning restore IL2070
            }
            else
            {
                if (targetType == typeof(object))
                {
                    if (values.IsEmpty() || values[0] is null)
                        return null;
                    targetType = values[0]!.GetType();
                }
                if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == IComparableType)
                {
#pragma warning disable IL2070
                    compareToMethod = targetType.GetMethod(nameof(IComparable.CompareTo)).AsNonNull();
#pragma warning restore IL2070
                }
                else
                {
#pragma warning disable IL2070
                    foreach (var interfaceType in targetType.GetInterfaces())
                    {
#pragma warning restore IL2070
                        if (interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == IComparableType)
                        {
                            if (interfaceType.GenericTypeArguments[0].IsAssignableFrom(targetType))
                            {
#pragma warning disable IL2070
#pragma warning disable IL2075
                                compareToMethod = interfaceType.GetMethod(nameof(IComparable.CompareTo)).AsNonNull();
#pragma warning restore IL2070
#pragma warning restore IL2075
                                break;
                            }
                        }
                    }
                    if (compareToMethod is null)
                        return null;
                }
            }
            var result = (object?)null;
            var args = new object?[1];
            foreach (var value in values)
            {
                if (value == null)
                    continue;
                if (!targetType.IsInstanceOfType(value))
                    continue;
                if (result is null)
                    result = value;
                else
                {
                    args[0] = result;
                    try
                    {
                        var comparisonResult = (int)compareToMethod.Invoke(value, args)!;
                        result = selector(value, result, comparisonResult);
                    }
                    // ReSharper disable once EmptyGeneralCatchClause
                    catch
                    { }
                }
            }
            return result;
        }
    }
}