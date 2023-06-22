using Avalonia.Controls;
using System;
using System.Collections;

namespace CarinaStudio.Controls
{
	/// <summary>
	/// Extensions for <see cref="ItemsControl"/>.
	/// </summary>
	public static class ItemsControlExtensions
	{
		/// <summary>
		/// Get number of item in <see cref="ItemsControl"/>.
		/// </summary>
		/// <param name="itemsControl"><see cref="ItemsControl"/>.</param>
		/// <returns>Number of items, or 0 if number of items cannot be determined.</returns>
		[Obsolete("Use ItemCount property instead.")]
		public static int GetItemCount(this ItemsControl itemsControl) => itemsControl.Items?.Let(it =>
		{
			if (it is ICollection collection)
				return collection.Count;
			try
			{
				return it.GetType().GetProperty("Count")?.Let(property =>
				{
					if (property.PropertyType == typeof(int))
						return (int)property.GetValue(it).AsNonNull();
					return 0;
				}) ?? 0;
			}
			// ReSharper disable once EmptyGeneralCatchClause
			catch
			{ }
			return 0;
		}) ?? 0;
	}
}
