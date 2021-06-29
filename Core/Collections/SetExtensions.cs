using System;
using System.Collections.Generic;

namespace CarinaStudio.Collections
{
	/// <summary>
	/// Extensions for <see cref="ISet{T}"/>.
	/// </summary>
	public static class SetExtensions
	{
		/// <summary>
		/// Add all given elements to the set.
		/// </summary>
		/// <typeparam name="T">Type of element.</typeparam>
		/// <param name="set"><see cref="ISet{T}"/>.</param>
		/// <param name="elements">Elements to add.</param>
		public static void AddAll<T>(this ISet<T> set, IEnumerable<T> elements)
		{
			foreach (var element in elements)
				set.Add(element);
		}


		/// <summary>
		/// Make <see cref="ISet{T}"/> as read-only.
		/// </summary>
		/// <typeparam name="T">Type of element.</typeparam>
		/// <param name="set"><see cref="ISet{T}"/>.</param>
		/// <returns>Read-only <see cref="ISet{T}"/>.</returns>
		public static ISet<T> AsReadOnly<T>(this ISet<T> set)
		{
			if (set.IsReadOnly)
				return set;
			return new ReadOnlySet<T>(set);
		}
	}
}
