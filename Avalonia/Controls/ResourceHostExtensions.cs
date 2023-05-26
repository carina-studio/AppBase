﻿using Avalonia;
using Avalonia.Controls;
#if !AVALONIA_11_0_0_P4
using Avalonia.Styling;
#endif
using System;
using System.Diagnostics.CodeAnalysis;

namespace CarinaStudio.Controls
{
    /// <summary>
    /// Extensions for <see cref="IResourceHost"/>.
    /// </summary>
    public static class ResourceHostExtensions
    {
		/// <summary>
		/// Bind property to given resource.
		/// </summary>
		/// <param name="target">Target object.</param>
		/// <param name="property">Property to bind.</param>
		/// <param name="resourceKey">Resource key.</param>
		/// <typeparam name="T">Type of target object.</typeparam>
		/// <returns>Token of binding.</returns>
		public static IDisposable BindToResource<T>(this T target, AvaloniaProperty property, object resourceKey) where T : AvaloniaObject, IResourceHost =>
			target.Bind(property, new CachedResource<object?>(target, resourceKey));
		
		
#if !AVALONIA_11_0_0_P4
	    // Get actual theme of given object.
	    static ThemeVariant? GetActualThemeVariant(object obj) => obj switch
	    {
		    IThemeVariantHost host => host.ActualThemeVariant,
		    _ => null,
	    };
#endif

		
#if !AVALONIA_11_0_0_P4
#pragma warning disable CS8601
	    /// <summary>
	    /// Find resource with given type or use default value.
	    /// </summary>
	    /// <typeparam name="T">Type of resource.</typeparam>
	    /// <param name="resourceHost"><see cref="IResourceHost"/>.</param>
	    /// <param name="key">Resource key.</param>
	    /// <param name="defaultValue">Default value.</param>
	    /// <returns></returns>
	    public static T FindResourceOrDefault<T>(this IResourceHost resourceHost, object key, T defaultValue = default) =>
		    FindResourceOrDefault(resourceHost, key, GetActualThemeVariant(resourceHost), defaultValue);

	    
	    /// <summary>
	    /// Find resource with given type or use default value.
	    /// </summary>
	    /// <typeparam name="T">Type of resource.</typeparam>
	    /// <param name="resourceHost"><see cref="IResourceHost"/>.</param>
	    /// <param name="key">Resource key.</param>
	    /// <param name="theme">Theme.</param>
	    /// <param name="defaultValue">Default value.</param>
	    /// <returns></returns>
	    public static T FindResourceOrDefault<T>(this IResourceHost resourceHost, object key, ThemeVariant? theme, T defaultValue = default)
	    {
		    if (resourceHost.TryFindResource(key, theme, out var rawRes) && rawRes is T targetRes)
			    return targetRes;
		    return defaultValue;
	    }
#pragma warning restore CS8601
#else
#pragma warning disable CS8601
		/// <summary>
		/// Find resource with given type or use default value.
		/// </summary>
		/// <typeparam name="T">Type of resource.</typeparam>
		/// <param name="resourceHost"><see cref="IResourceHost"/>.</param>
		/// <param name="key">Resource key.</param>
		/// <param name="defaultValue">Default value.</param>
		/// <returns></returns>
		public static T FindResourceOrDefault<T>(this IResourceHost resourceHost, object key, T defaultValue = default)
		{
			if (resourceHost.TryFindResource(key, out var rawRes) && rawRes is T targetRes)
				return targetRes;
			return defaultValue;
		}
#pragma warning restore CS8601
#endif


#if !AVALONIA_11_0_0_P4
	    /// <summary>
	    /// Try finding resource with given type.
	    /// </summary>
	    /// <typeparam name="T">Type of resource.</typeparam>
	    /// <param name="resourceHost"><see cref="IResourceHost"/>.</param>
	    /// <param name="key">Resource key.</param>
	    /// <param name="res">Found resource.</param>
	    /// <returns>True if resource found.</returns>
	    public static bool TryFindResource<T>(this IResourceHost resourceHost, object key, [NotNullWhen(true)] out T? res) where T : class =>
		    TryFindResource(resourceHost, key, GetActualThemeVariant(resourceHost), out res);


	    /// <summary>
	    /// Try finding resource with given type.
	    /// </summary>
	    /// <typeparam name="T">Type of resource.</typeparam>
	    /// <param name="resourceHost"><see cref="IResourceHost"/>.</param>
	    /// <param name="key">Resource key.</param>
	    /// <param name="theme">Theme.</param>
	    /// <param name="res">Found resource.</param>
	    /// <returns>True if resource found.</returns>
	    public static bool TryFindResource<T>(this IResourceHost resourceHost, object key, ThemeVariant? theme, [NotNullWhen(true)] out T? res) where T : class
	    {
		    if (resourceHost.TryFindResource(key, theme, out var rawRes) && rawRes is T targetRes)
		    {
			    res = targetRes;
			    return true;
		    }
		    res = default;
		    return false;
	    }
#else
		/// <summary>
		/// Try finding resource with given type.
		/// </summary>
		/// <typeparam name="T">Type of resource.</typeparam>
		/// <param name="resourceHost"><see cref="IResourceHost"/>.</param>
		/// <param name="key">Resource key.</param>
		/// <param name="res">Found resource.</param>
		/// <returns>True if resource found.</returns>
		public static bool TryFindResource<T>(this IResourceHost resourceHost, object key, [NotNullWhen(true)] out T? res) where T : class
		{
			if (resourceHost.TryFindResource(key, out var rawRes) && rawRes is T targetRes)
			{
				res = targetRes;
				return true;
			}
			res = default;
			return false;
		}
#endif


#if !AVALONIA_11_0_0_P4
	    /// <summary>
	    /// Try finding resource with given type.
	    /// </summary>
	    /// <typeparam name="T">Type of resource.</typeparam>
	    /// <param name="resourceHost"><see cref="IResourceHost"/>.</param>
	    /// <param name="key">Resource key.</param>
	    /// <param name="res">Found resource.</param>
	    /// <returns>True if resource found.</returns>
	    public static bool TryFindResource<T>(this IResourceHost resourceHost, object key, [NotNullWhen(true)] out T? res) where T : struct =>
		    TryFindResource(resourceHost, key, GetActualThemeVariant(resourceHost), out res);
	    
	    
	    /// <summary>
	    /// Try finding resource with given type.
	    /// </summary>
	    /// <typeparam name="T">Type of resource.</typeparam>
	    /// <param name="resourceHost"><see cref="IResourceHost"/>.</param>
	    /// <param name="key">Resource key.</param>
	    /// <param name="theme">Theme.</param>
	    /// <param name="res">Found resource.</param>
	    /// <returns>True if resource found.</returns>
	    public static bool TryFindResource<T>(this IResourceHost resourceHost, object key, ThemeVariant? theme, [NotNullWhen(true)] out T? res) where T : struct
	    {
		    if (resourceHost.TryFindResource(key, theme, out var rawRes) && rawRes is T targetRes)
		    {
			    res = targetRes;
			    return true;
		    }
		    res = default;
		    return false;
	    }
#else
		/// <summary>
		/// Try finding resource with given type.
		/// </summary>
		/// <typeparam name="T">Type of resource.</typeparam>
		/// <param name="resourceHost"><see cref="IResourceHost"/>.</param>
		/// <param name="key">Resource key.</param>
		/// <param name="res">Found resource.</param>
		/// <returns>True if resource found.</returns>
		public static bool TryFindResource<T>(this IResourceHost resourceHost, object key, [NotNullWhen(true)] out T? res) where T : struct
		{
			if (resourceHost.TryFindResource(key, out var rawRes) && rawRes is T targetRes)
			{
				res = targetRes;
				return true;
			}
			res = default;
			return false;
		}
#endif
	}
}
