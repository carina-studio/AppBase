﻿using Avalonia;
using Avalonia.Controls;
using Avalonia.Styling;
using Avalonia.Threading;
using System.Diagnostics.CodeAnalysis;

namespace CarinaStudio.Controls
{
    /// <summary>
    /// Extensions for <see cref="IResourceNode"/>.
    /// </summary>
    public static class ResourceNodeExtensions
    {
	    // Get actual theme of given object.
	    internal static ThemeVariant? GetActualThemeVariant(object obj)
	    {
		    if (obj is not AvaloniaObject avaloniaObject || avaloniaObject.CheckAccess())
			    return GetActualThemeVariantInternal(obj);
		    return Dispatcher.UIThread.Invoke(() => GetActualThemeVariantInternal(obj));
	    }
	    static ThemeVariant? GetActualThemeVariantInternal(object obj) =>
		    obj switch
		    {
			    IThemeVariantHost host => host.ActualThemeVariant,
			    _ => null,
		    };



	    /// <summary>
	    /// Try getting resource with given type.
	    /// </summary>
	    /// <typeparam name="T">Type of resource.</typeparam>
	    /// <param name="node"><see cref="IResourceNode"/>.</param>
	    /// <param name="key">Resource key.</param>
	    /// <param name="res">Found resource.</param>
	    /// <returns>True if resource got.</returns>
	    /// <remarks>This is a thread-safe method.</remarks>
	    public static bool TryGetResource<T>(this IResourceNode node, object key, [NotNullWhen(true)] out T? res) where T : class =>
		    TryGetResource(node, GetActualThemeVariant(node), key, out res);


	    /// <summary>
	    /// Try getting resource with given type.
	    /// </summary>
	    /// <typeparam name="T">Type of resource.</typeparam>
	    /// <param name="node"><see cref="IResourceNode"/>.</param>
	    /// <param name="theme">Theme.</param>
	    /// <param name="key">Resource key.</param>
	    /// <param name="res">Found resource.</param>
	    /// <returns>True if resource got.</returns>
	    /// <remarks>This is a thread-safe method.</remarks>
	    public static bool TryGetResource<T>(this IResourceNode node, ThemeVariant? theme, object key, [NotNullWhen(true)] out T? res) where T : class
	    {
		    if (node.TryGetResource(key, theme, out var rawRes) && rawRes is T targetRes)
		    {
			    res = targetRes;
			    return true;
		    }
		    res = default;
		    return false;
	    }


	    /// <summary>
	    /// Try getting resource with given type.
	    /// </summary>
	    /// <typeparam name="T">Type of resource.</typeparam>
	    /// <param name="node"><see cref="IResourceNode"/>.</param>
	    /// <param name="key">Resource key.</param>
	    /// <param name="res">Found resource.</param>
	    /// <returns>True if resource got.</returns>
	    /// <remarks>This is a thread-safe method.</remarks>
	    public static bool TryGetResource<T>(this IResourceNode node, object key, [NotNullWhen(true)] out T? res) where T : struct =>
		    TryGetResource(node, GetActualThemeVariant(node), key, out res);


	    /// <summary>
	    /// Try getting resource with given type.
	    /// </summary>
	    /// <typeparam name="T">Type of resource.</typeparam>
	    /// <param name="node"><see cref="IResourceNode"/>.</param>
	    /// <param name="theme">Theme.</param>
	    /// <param name="key">Resource key.</param>
	    /// <param name="res">Found resource.</param>
	    /// <returns>True if resource got.</returns>
	    /// <remarks>This is a thread-safe method.</remarks>
	    public static bool TryGetResource<T>(this IResourceNode node, ThemeVariant? theme, object key, [NotNullWhen(true)] out T? res) where T : struct
	    {
		    if (node.TryGetResource(key, theme, out var rawRes) && rawRes is T targetRes)
		    {
			    res = targetRes;
			    return true;
		    }
		    res = default;
		    return false;
	    }
    }
}
