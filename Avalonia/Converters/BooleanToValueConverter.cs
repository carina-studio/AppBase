using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace CarinaStudio.Data.Converters
{
	/// <summary>
	/// <see cref="IValueConverter"/> to convert from <see cref="bool"/> to specific value type.
	/// </summary>
	public class BooleanToValueConverter<TValue> : IValueConverter
	{
		// Fields.
		readonly Func<TValue, TValue, bool>? equalityChecker;


		/// <summary>
		/// Initialize new <see cref="BooleanToValueConverter{TValue}"/> instance.
		/// </summary>
		/// <param name="trueValue">Value converted from True.</param>
		/// <param name="falseValue">Value converted from False.</param>
		/// <param name="equalityChecker">Function to check equality of values.</param>
		public BooleanToValueConverter(TValue trueValue, TValue falseValue, Func<TValue, TValue, bool>? equalityChecker = null)
		{
			this.TrueValue = trueValue;
			this.FalseValue = falseValue;
			this.equalityChecker = equalityChecker;
		}


		/// <inheritdoc/>
		public virtual object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
		{
			if (!targetType.IsAssignableFrom(typeof(TValue)))
				return null;
			if (!(value is bool boolValue) || !boolValue)
				return this.FalseValue;
			return this.TrueValue;
		}


		/// <inheritdoc/>
		public virtual object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
		{
			if (targetType != typeof(bool))
				return null;
			if (!(value is TValue targetValue))
				return false;
			if (this.equalityChecker != null)
				return this.equalityChecker(targetValue, this.TrueValue);
			if (targetValue is IEquatable<TValue> equatable)
				return equatable.Equals(this.TrueValue);
			if (targetValue == null)
				return this.TrueValue == null;
			return targetValue.Equals(this.TrueValue);
		}


		/// <summary>
		/// Get value converted from False.
		/// </summary>
		public TValue FalseValue { get; }


		/// <summary>
		/// Get value converted from True.
		/// </summary>
		public TValue TrueValue { get; }
	}
}
