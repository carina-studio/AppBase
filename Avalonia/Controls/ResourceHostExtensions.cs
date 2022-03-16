using Avalonia.Controls;
using System;

namespace CarinaStudio.Controls
{
    /// <summary>
    /// Extensions for <see cref="IResourceHost"/>.
    /// </summary>
    public static class ResourceHostExtensions
    {
		/// <summary>
		/// Try finding resource with given type.
		/// </summary>
		/// <typeparam name="T">Type of resource.</typeparam>
		/// <param name="resourceHost"><see cref="IResourceHost"/>.</param>
		/// <param name="key">Resource key.</param>
		/// <param name="res">Found resource.</param>
		/// <returns>True if resource found.</returns>
		public static bool TryFindResource<T>(this IResourceHost resourceHost, object key, out T? res) where T : class
		{
			if (resourceHost.TryFindResource(key, out var rawRes) && rawRes is T)
			{
				res = (T)rawRes;
				return true;
			}
			res = default;
			return false;
		}


		/// <summary>
		/// Try finding resource with given type.
		/// </summary>
		/// <typeparam name="T">Type of resource.</typeparam>
		/// <param name="resourceHost"><see cref="IResourceHost"/>.</param>
		/// <param name="key">Resource key.</param>
		/// <param name="res">Found resource.</param>
		/// <returns>True if resource found.</returns>
		public static bool TryFindResource<T>(this IResourceHost resourceHost, object key, out T? res) where T : struct
		{
			if (resourceHost.TryFindResource(key, out var rawRes) && rawRes is T)
			{
				res = (T)rawRes;
				return true;
			}
			res = default;
			return false;
		}
	}
}
