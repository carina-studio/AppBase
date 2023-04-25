using Avalonia.Controls;
#if AVALONIA_11_0_0_P6_OR_ABOVE
using Avalonia.Styling;
#endif
using System.Diagnostics.CodeAnalysis;

namespace CarinaStudio.Controls
{
    /// <summary>
    /// Extensions for <see cref="IResourceNode"/>.
    /// </summary>
    public static class ResourceNodeExtensions
    {
#if AVALONIA_11_0_0_P6_OR_ABOVE
	    // Get actual theme of given object.
	    static ThemeVariant? GetActualThemeVariant(object obj) => obj switch
	    {
		    IThemeVariantHost host => host.ActualThemeVariant,
		    _ => null,
	    };
#endif
	    
	    
#if AVALONIA_11_0_0_P6_OR_ABOVE
	    /// <summary>
	    /// Try getting resource with given type.
	    /// </summary>
	    /// <typeparam name="T">Type of resource.</typeparam>
	    /// <param name="node"><see cref="IResourceNode"/>.</param>
	    /// <param name="key">Resource key.</param>
	    /// <param name="res">Found resource.</param>
	    /// <returns>True if resource got.</returns>
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
#else
		/// <summary>
		/// Try getting resource with given type.
		/// </summary>
		/// <typeparam name="T">Type of resource.</typeparam>
		/// <param name="node"><see cref="IResourceNode"/>.</param>
		/// <param name="key">Resource key.</param>
		/// <param name="res">Found resource.</param>
		/// <returns>True if resource got.</returns>
		public static bool TryGetResource<T>(this IResourceNode node, object key, [NotNullWhen(true)] out T? res) where T : class
		{
			if (node.TryGetResource(key, out var rawRes) && rawRes is T targetRes)
			{
				res = targetRes;
				return true;
			}
			res = default;
			return false;
		}
#endif


#if AVALONIA_11_0_0_P6_OR_ABOVE
	    /// <summary>
	    /// Try getting resource with given type.
	    /// </summary>
	    /// <typeparam name="T">Type of resource.</typeparam>
	    /// <param name="node"><see cref="IResourceNode"/>.</param>
	    /// <param name="key">Resource key.</param>
	    /// <param name="res">Found resource.</param>
	    /// <returns>True if resource got.</returns>
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
#else
	    /// <summary>
		/// Try getting resource with given type.
		/// </summary>
		/// <typeparam name="T">Type of resource.</typeparam>
		/// <param name="node"><see cref="IResourceNode"/>.</param>
		/// <param name="key">Resource key.</param>
		/// <param name="res">Found resource.</param>
		/// <returns>True if resource got.</returns>
		public static bool TryGetResource<T>(this IResourceNode node, object key, [NotNullWhen(true)] out T? res) where T : struct
		{
			if (node.TryGetResource(key, out var rawRes) && rawRes is T targetRes)
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
