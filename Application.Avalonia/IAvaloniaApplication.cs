using Avalonia.Styling;
using System;
using System.Diagnostics.CodeAnalysis;

namespace CarinaStudio;

/// <summary>
/// <see cref="IApplication"/> which based-on Avalonia.
/// </summary>
public interface IAvaloniaApplication : IApplication
{
	/// <summary>
	/// Get observable resource with given key.
	/// </summary>
	/// <param name="key">Key.</param>
	/// <param name="converter">Value converter.</param>
	/// <returns>Observable value of resource.</returns>
	IObservable<object?> GetResourceObservable(object key, Func<object?, object?>? converter = null);


    /// <summary>
    /// Try finding resource within the object.
    /// </summary>
    /// <param name="key">The resource key.</param>
    /// <param name="theme">Theme used to select theme dictionary.</param>
    /// <param name="value">The value with given key.</param>
    /// <returns>
    /// True if the resource found.
    /// </returns>
    bool TryFindResource(object key, ThemeVariant? theme, [NotNullWhen(true)] out object? value);
}


/// <summary>
/// Extensions for <see cref="IAvaloniaApplication"/>.
/// </summary>
public static class AvaloniaApplicationExtensions
{
	// Get actual theme of given object.
	internal static ThemeVariant? GetActualThemeVariant(IAvaloniaApplication app) => app switch
	{
		IThemeVariantHost host => host.ActualThemeVariant,
		_ => null,
	};


#pragma warning disable CS8601
	/// <summary>
	/// Find resource with given type or use default value.
	/// </summary>
	/// <typeparam name="T">Type of resource.</typeparam>
	/// <param name="app"><see cref="IAvaloniaApplication"/>.</param>
	/// <param name="key">Resource key.</param>
	/// <param name="defaultValue">Default value.</param>
	/// <returns></returns>
	public static T FindResourceOrDefault<T>(this IAvaloniaApplication app, object key, T defaultValue = default) =>
		FindResourceOrDefault(app, key, GetActualThemeVariant(app), defaultValue);


	/// <summary>
	/// Find resource with given type or use default value.
	/// </summary>
	/// <typeparam name="T">Type of resource.</typeparam>
	/// <param name="app"><see cref="IAvaloniaApplication"/>.</param>
	/// <param name="key">Resource key.</param>
	/// <param name="theme">Theme.</param>
	/// <param name="defaultValue">Default value.</param>
	/// <returns></returns>
	public static T FindResourceOrDefault<T>(this IAvaloniaApplication app, object key, ThemeVariant? theme, T defaultValue = default)
	{
		if (app.TryFindResource(key, theme, out var value) && value is T valueT)
			return valueT;
		return defaultValue;
	}
#pragma warning restore CS8601


	/// <summary>
	/// Try finding resource with given type.
	/// </summary>
	/// <typeparam name="T">Type of resource.</typeparam>
	/// <param name="app"><see cref="IAvaloniaApplication"/>.</param>
	/// <param name="key">Resource key.</param>
	/// <param name="res">Found resource.</param>
	/// <returns>True if resource found.</returns>
	public static bool TryFindResource<T>(this IAvaloniaApplication app, object key, [NotNullWhen(true)] out T? res) where T : class =>
		TryFindResource(app, key, GetActualThemeVariant(app), out res);


	/// <summary>
	/// Try finding resource with given type.
	/// </summary>
	/// <typeparam name="T">Type of resource.</typeparam>
	/// <param name="app"><see cref="IAvaloniaApplication"/>.</param>
	/// <param name="key">Resource key.</param>
	/// <param name="theme">Theme.</param>
	/// <param name="res">Found resource.</param>
	/// <returns>True if resource found.</returns>
	public static bool TryFindResource<T>(this IAvaloniaApplication app, object key, ThemeVariant? theme, [NotNullWhen(true)] out T? res) where T : class
	{
		if (app.TryFindResource(key, theme, out var value) && value is T valueT)
		{
			res = valueT;
			return true;
		}
		res = default;
		return false;
	}


	/// <summary>
	/// Try finding resource with given type.
	/// </summary>
	/// <typeparam name="T">Type of resource.</typeparam>
	/// <param name="app"><see cref="IAvaloniaApplication"/>.</param>
	/// <param name="key">Resource key.</param>
	/// <param name="res">Found resource.</param>
	/// <returns>True if resource found.</returns>
	public static bool TryFindResource<T>(this IAvaloniaApplication app, object key, [NotNullWhen(true)] out T? res) where T : struct =>
		TryFindResource(app, key, GetActualThemeVariant(app), out res);


	/// <summary>
	/// Try finding resource with given type.
	/// </summary>
	/// <typeparam name="T">Type of resource.</typeparam>
	/// <param name="app"><see cref="IAvaloniaApplication"/>.</param>
	/// <param name="key">Resource key.</param>
	/// <param name="theme">Theme.</param>
	/// <param name="res">Found resource.</param>
	/// <returns>True if resource found.</returns>
	public static bool TryFindResource<T>(this IAvaloniaApplication app, object key, ThemeVariant? theme, [NotNullWhen(true)] out T? res) where T : struct
	{
		if (app.TryFindResource(key, theme, out var value) && value is T valueT)
		{
			res = valueT;
			return true;
		}
		res = default;
		return false;
	}
}
