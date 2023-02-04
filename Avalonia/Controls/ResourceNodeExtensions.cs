using Avalonia.Controls;
using System.Diagnostics.CodeAnalysis;

namespace CarinaStudio.Controls
{
    /// <summary>
    /// Extensions for <see cref="IResourceNode"/>.
    /// </summary>
    public static class ResourceNodeExtensions
    {
		/// <summary>
		/// Try getting resource with given type.
		/// </summary>
		/// <typeparam name="T">Type of resource.</typeparam>
		/// <param name="node"><see cref="IResourceNode"/>.</param>
		/// <param name="key">Resource key.</param>
		/// <param name="res">Found resource.</param>
		/// <returns>True if resource got.</returns>
		public static bool TryGetResource<T>(this IResourceNode node, object key, [NotNullWhen(true)] out T? res) where T : class =>
			TryGetResource(node, key, null, out res);


		/// <summary>
		/// Try getting resource with given type.
		/// </summary>
		/// <typeparam name="T">Type of resource.</typeparam>
		/// <param name="node"><see cref="IResourceNode"/>.</param>
		/// <param name="key">Resource key.</param>
		/// <param name="theme">Theme.</param>
		/// <param name="res">Found resource.</param>
		/// <returns>True if resource got.</returns>
		public static bool TryGetResource<T>(this IResourceNode node, object key, Avalonia.Styling.ThemeVariant? theme, [NotNullWhen(true)] out T? res) where T : class
		{
			if (node.TryGetResource(key, theme,  out var rawRes) && rawRes is T targetRes)
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
		public static bool TryGetResource<T>(this IResourceNode node, object key, [NotNullWhen(true)] out T? res) where T : struct =>
			TryGetResource(node, key, null, out res);


		/// <summary>
		/// Try getting resource with given type.
		/// </summary>
		/// <typeparam name="T">Type of resource.</typeparam>
		/// <param name="node"><see cref="IResourceNode"/>.</param>
		/// <param name="key">Resource key.</param>
		/// <param name="theme">Theme.</param>
		/// <param name="res">Found resource.</param>
		/// <returns>True if resource got.</returns>
		public static bool TryGetResource<T>(this IResourceNode node, object key, Avalonia.Styling.ThemeVariant? theme, [NotNullWhen(true)] out T? res) where T : struct
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
