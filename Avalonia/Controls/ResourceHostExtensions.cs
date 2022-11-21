using Avalonia.Controls;
using System.Diagnostics.CodeAnalysis;

namespace CarinaStudio.Controls
{
    /// <summary>
    /// Extensions for <see cref="IResourceHost"/>.
    /// </summary>
    public static class ResourceHostExtensions
    {
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
	}
}
