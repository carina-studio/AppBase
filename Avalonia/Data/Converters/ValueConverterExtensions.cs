using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace CarinaStudio.Data.Converters
{
	/// <summary>
	/// Extensions for <see cref="IValueConverter"/>.
	/// </summary>
	public static class ValueConverterExtensions
	{
#pragma warning disable CS8600
#pragma warning disable CS8603
		/// <summary>
		/// Convert value.
		/// </summary>
		/// <typeparam name="T">Type of target value.</typeparam>
		/// <param name="converter"><see cref="IValueConverter"/>.</param>
		/// <param name="value">Value to convert.</param>
		/// <returns>Converted value.</returns>
		public static T Convert<T>(this IValueConverter converter, object? value) =>
			(T)converter.Convert(value, typeof(T), null, CultureInfo.InvariantCulture);
		

		/// <summary>
		/// Convert value.
		/// </summary>
		/// <typeparam name="T">Type of target value.</typeparam>
		/// <param name="converter"><see cref="IValueConverter"/>.</param>
		/// <param name="value">Value to convert.</param>
		/// <param name="parameter">Conversion parameter.</param>
		/// <returns>Converted value.</returns>
		public static T Convert<T>(this IValueConverter converter, object? value, object? parameter) =>
			(T)converter.Convert(value, typeof(T), parameter, CultureInfo.InvariantCulture);


		/// <summary>
		/// Convert value.
		/// </summary>
		/// <typeparam name="T">Type of target value.</typeparam>
		/// <param name="converter"><see cref="IValueConverter"/>.</param>
		/// <param name="value">Value to convert.</param>
		/// <param name="cultureInfo">Culture info.</param>
		/// <returns>Converted value.</returns>
		public static T Convert<T>(this IValueConverter converter, object? value, CultureInfo cultureInfo) =>
			(T)converter.Convert(value, typeof(T), null, cultureInfo);


		/// <summary>
		/// Convert value.
		/// </summary>
		/// <typeparam name="T">Type of target value.</typeparam>
		/// <param name="converter"><see cref="IValueConverter"/>.</param>
		/// <param name="value">Value to convert.</param>
		/// <param name="parameter">Conversion parameter.</param>
		/// <param name="cultureInfo">Culture info.</param>
		/// <returns>Converted value.</returns>
		public static T Convert<T>(this IValueConverter converter, object? value, object? parameter, CultureInfo cultureInfo) =>
			(T)converter.Convert(value, typeof(T), parameter, cultureInfo);


		/// <summary>
		/// Convert value back.
		/// </summary>
		/// <typeparam name="T">Type of target value.</typeparam>
		/// <param name="converter"><see cref="IValueConverter"/>.</param>
		/// <param name="value">Value to convert back.</param>
		/// <returns>Converted value.</returns>
		public static T ConvertBack<T>(this IValueConverter converter, object? value) =>
			(T)converter.ConvertBack(value, typeof(T), null, CultureInfo.InvariantCulture);


		/// <summary>
		/// Convert value back.
		/// </summary>
		/// <typeparam name="T">Type of target value.</typeparam>
		/// <param name="converter"><see cref="IValueConverter"/>.</param>
		/// <param name="value">Value to convert back.</param>
		/// <param name="cultureInfo">Culture info.</param>
		/// <returns>Converted value.</returns>
		public static T ConvertBack<T>(this IValueConverter converter, object? value, CultureInfo cultureInfo) =>
			(T)converter.ConvertBack(value, typeof(T), null, cultureInfo);


		/// <summary>
		/// Convert value back.
		/// </summary>
		/// <typeparam name="T">Type of target value.</typeparam>
		/// <param name="converter"><see cref="IValueConverter"/>.</param>
		/// <param name="value">Value to convert back.</param>
		/// <param name="parameter">Conversion parameter.</param>
		/// <param name="cultureInfo">Culture info.</param>
		/// <returns>Converted value.</returns>
		public static T ConvertBack<T>(this IValueConverter converter, object? value, object? parameter, CultureInfo cultureInfo) =>
			(T)converter.ConvertBack(value, typeof(T), parameter, cultureInfo);
#pragma warning restore CS8600
#pragma warning restore CS8603
	}
}
