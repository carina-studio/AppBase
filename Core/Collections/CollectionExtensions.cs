using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace CarinaStudio.Collections
{
	/// <summary>
	/// Extensions for <see cref="ICollection{T}"/>.
	/// </summary>
	public static class CollectionExtensions
	{
		/// <summary>
		/// Add all given elements to collection.
		/// </summary>
		/// <param name="collection">Collection.</param>
		/// <param name="elements">Elements to add.</param>
		/// <typeparam name="T">Type of element of collection.</typeparam>
		public static void AddAll<T>(this ICollection<T> collection, IEnumerable<T> elements)
		{
			if (collection is List<T> list)
				list.AddRange(elements);
			else if (collection is ObservableList<T> observableList)
				observableList.AddRange(elements);
			else if (collection is SortedObservableList<T> sortedList)
				sortedList.AddAll(elements);
			else
			{
				foreach (var element in elements)
					collection.Add(element);
			}
		}


		/// <summary>
		/// Check whether given collection is empty or not.
		/// </summary>
		/// <typeparam name="T">Type of element of collection.</typeparam>
		/// <param name="collection">Collection to check.</param>
		/// <returns>True if collection is empty.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsEmpty<T>(this ICollection<T> collection) => collection.Count <= 0;


		/// <summary>
		/// Check whether given collection is not empty or not.
		/// </summary>
		/// <typeparam name="T">Type of element of collection.</typeparam>
		/// <param name="collection">Collection to check.</param>
		/// <returns>True if collection is not empty.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsNotEmpty<T>(this ICollection<T>? collection) => collection != null && collection.Count > 0;


		/// <summary>
		/// Check whether given collection is null/empty or not.
		/// </summary>
		/// <typeparam name="T">Type of element of collection.</typeparam>
		/// <param name="collection">Collection to check.</param>
		/// <returns>True if collection is null or empty.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsNullOrEmpty<T>(this ICollection<T>? collection) => collection == null || collection.Count <= 0;


		/// <summary>
		/// Remove elements which match the given condition from collection.
		/// </summary>
		/// <param name="collection">Collection.</param>
		/// <param name="predicate">Function to check condition.</param>
		/// <typeparam name="T">Type of element of collection.</typeparam>
		/// <returns>Number of removed elements.</returns>
		public static int RemoveAll<T>(this ICollection<T> collection, Predicate<T> predicate)
		{
			if (collection is List<T> list)
				return list.RemoveAll(predicate);
			if (collection is ObservableList<T> observableList)
				return observableList.RemoveAll(predicate);
			if (collection is SortedObservableList<T> sortedList)
				return sortedList.RemoveAll(predicate);
			if (collection is IList<T> listInterface)
			{
				var count = 0;
				for (var i = listInterface.Count - 1; i >= 0; --i)
				{
					if (predicate(listInterface[i]))
					{
						listInterface.RemoveAt(i);
						++count;
					}
				}
				return count;
			}
			var elementsToRemove = (List<T>?)null;
			foreach (var element in collection)
			{
				if (predicate(element))
				{
					elementsToRemove ??= new List<T>();
					elementsToRemove.Add(element);
				}
			}
			if (elementsToRemove != null)
			{
				foreach (var element in elementsToRemove)
					collection.Remove(element);
				return elementsToRemove.Count;
			}
			return 0;
		}
	}
}
