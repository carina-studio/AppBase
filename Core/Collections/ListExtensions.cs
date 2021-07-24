using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace CarinaStudio.Collections
{
	/// <summary>
	/// Extensions for <see cref="IList{T}"/>.
	/// </summary>
	public static class ListExtensions
	{
		// Fields.
		[ThreadStatic]
		static Random? random;


		/// <summary>
		/// Make given <see cref="IList{T}"/> as read-only <see cref="IList{T}"/>.
		/// </summary>
		/// <remarks>If <paramref name="list"/> implements <see cref="INotifyCollectionChanged"/> then returned <see cref="IList{T}"/> will also implements <see cref="INotifyCollectionChanged"/>.</remarks>
		/// <typeparam name="T">Type of element.</typeparam>
		/// <param name="list"><see cref="IList{T}"/> to make as read-only.</param>
		/// <returns>Read-only <see cref="IList{T}"/>.</returns>
		public static IList<T> AsReadOnly<T>(this IList<T> list)
		{
			if (list.IsReadOnly)
				return list;
			if (list is INotifyCollectionChanged)
				return new ReadOnlyObservableList<T>(list);
			return new ReadOnlyCollection<T>(list);
		}


		/// <summary>
		/// Use binary-search to find given element.
		/// </summary>
		/// <typeparam name="T">Type of element.</typeparam>
		/// <param name="list">List to find element.</param>
		/// <param name="element">Element to be found.</param>
		/// <param name="comparer"><see cref="IComparer{T}"/> to compare elements.</param>
		/// <returns>Index of found element, or bitwise complement of index of proper position to put element.</returns>
		public static int BinarySearch<T>(this IList<T> list, T element, IComparer<T> comparer) => BinarySearch(list, 0, list.Count, element, comparer.Compare);


		/// <summary>
		/// Use binary-search to find given element.
		/// </summary>
		/// <typeparam name="T">Type of element.</typeparam>
		/// <param name="list">List to find element.</param>
		/// <param name="element">Element to be found.</param>
		/// <param name="comparison">Comparison function.</param>
		/// <returns>Index of found element, or bitwise complement of index of proper position to put element.</returns>
		public static int BinarySearch<T>(this IList<T> list, T element, Comparison<T> comparison) => BinarySearch(list, 0, list.Count, element, comparison);


		/// <summary>
		/// Use binary-search to find given element by key.
		/// </summary>
		/// <typeparam name="T">Type of element.</typeparam>
		/// <typeparam name="TKey">Type of key of element.</typeparam>
		/// <param name="list">List to find element.</param>
		/// <param name="key">Key of element to be found.</param>
		/// <param name="keyGetter">Method to get key from element.</param>
		/// <param name="comparison">Comparison function.</param>
		/// <returns>Index of found element, or bitwise complement of index of proper position to put element.</returns>
		public static int BinarySearch<T, TKey>(this IList<T> list, TKey key, Func<T, TKey> keyGetter, Comparison<TKey> comparison) => BinarySearch(list, 0, list.Count, key, keyGetter, comparison);


		/// <summary>
		/// Use binary-search to find given element.
		/// </summary>
		/// <typeparam name="T">Type of element.</typeparam>
		/// <param name="list">List to find element.</param>
		/// <param name="element">Element to be found.</param>
		/// <returns>Index of found element, or bitwise complement of index of proper position to put element.</returns>
		public static int BinarySearch<T>(this IList<T> list, T element) where T : IComparable<T> => BinarySearch(list, 0, list.Count, element);


		// Binary search.
		static int BinarySearch<T>(IList<T> list, int start, int end, T element, Comparison<T> comparison)
		{
			if (start >= end)
				return ~start;
			var middle = (start + end) / 2;
			var result = comparison(element, list[middle]);
			if (result == 0)
				return middle;
			if (result < 0)
				return BinarySearch(list, start, middle, element, comparison);
			return BinarySearch(list, middle + 1, end, element, comparison);
		}
		static int BinarySearch<T, TKey>(IList<T> list, int start, int end, TKey key, Func<T, TKey> keyGetter, Comparison<TKey> comparison)
		{
			if (start >= end)
				return ~start;
			var middle = (start + end) / 2;
			var result = comparison(key, keyGetter(list[middle]));
			if (result == 0)
				return middle;
			if (result < 0)
				return BinarySearch(list, start, middle, key, keyGetter, comparison);
			return BinarySearch(list, middle + 1, end, key, keyGetter, comparison);
		}
		static int BinarySearch<T>(IList<T> list, int start, int end, T element) where T : IComparable<T>
		{
			if (start >= end)
				return ~start;
			var middle = (start + end) / 2;
			var result = element.CompareTo(list[middle]);
			if (result == 0)
				return middle;
			if (result < 0)
				return BinarySearch(list, start, middle, element);
			return BinarySearch(list, middle + 1, end, element);
		}


		/// <summary>
		/// Copy elements to array.
		/// </summary>
		/// <typeparam name="T">Type of list element.</typeparam>
		/// <param name="list"><see cref="IList{T}"/>.</param>
		/// <param name="index">Index of first element in list to copy.</param>
		/// <param name="array">Array to place copied elements.</param>
		/// <param name="arrayIndex">Index of first position in <paramref name="array"/> to place copied elements.</param>
		/// <param name="count">Number of elements to copy.</param>
		public static void CopyTo<T>(this IList<T> list, int index, T[] array, int arrayIndex, int count)
		{
			if (list is List<T> sysList)
				sysList.CopyTo(index, array, arrayIndex, count);
			else if (list is SortedObservableList<T> sortedList)
				sortedList.CopyTo(index, array, arrayIndex, count);
			else if (count > 0)
			{
				if (index < 0 || index >= list.Count)
					throw new ArgumentOutOfRangeException(nameof(index));
				if (arrayIndex < 0 || arrayIndex >= array.Length)
					throw new ArgumentOutOfRangeException(nameof(arrayIndex));
				if (index + count > list.Count || arrayIndex + count > array.Length)
					throw new ArgumentOutOfRangeException(nameof(count));
				while (count > 0)
				{
					array[arrayIndex++] = list[index++];
					--count;
				}
			}
		}


		/// <summary>
		/// Check whether elements in given <see cref="IList{T}"/> is sorted or not.
		/// </summary>
		/// <typeparam name="T">Type of elements.</typeparam>
		/// <param name="list"><see cref="IList{T}"/>.</param>
		/// <param name="comparer"><see cref="IComparer{T}"/> to check order of elements.</param>
		/// <returns>True if elements in <see cref="IList{T}"/> is sorted.</returns>
		public static bool IsSorted<T>(this IList<T> list, IComparer<T> comparer) => IsSorted(list, comparer.Compare);


		/// <summary>
		/// Check whether elements in given <see cref="IList{T}"/> is sorted or not.
		/// </summary>
		/// <typeparam name="T">Type of elements.</typeparam>
		/// <param name="list"><see cref="IList{T}"/>.</param>
		/// <param name="comparison">Comparison method to check order of elements.</param>
		/// <returns>True if elements in <see cref="IList{T}"/> is sorted.</returns>
		public static bool IsSorted<T>(this IList<T> list, Comparison<T> comparison)
		{
			var count = list.Count;
			if (count > 1)
			{
				var nextElement = list[count - 1];
				for (var i = count - 2; i >= 0; --i)
				{
					var element = list[i];
					if (comparison(element, nextElement) > 0)
						return false;
					nextElement = element;
				}
			}
			return true;
		}


		/// <summary>
		/// Check whether elements in given <see cref="IList{T}"/> is sorted or not.
		/// </summary>
		/// <typeparam name="T">Type of elements.</typeparam>
		/// <param name="list"><see cref="IList{T}"/>.</param>
		/// <returns>True if elements in <see cref="IList{T}"/> is sorted.</returns>
		public static bool IsSorted<T>(this IList<T> list) where T : IComparable<T>
		{
			var count = list.Count;
			if (count > 1)
			{
				var nextElement = list[count - 1];
				for (var i = count - 2; i >= 0; --i)
				{
					var element = list[i];
					if (element.CompareTo(nextElement) > 0)
						return false;
					nextElement = element;
				}
			}
			return true;
		}


		/// <summary>
		/// Select an element from given <see cref="IList{T}"/> randomly.
		/// </summary>
		/// <typeparam name="T">Type of element.</typeparam>
		/// <param name="list"><see cref="IList{T}"/>.</param>
		/// <returns>Element selected from <see cref="IList{T}"/>.</returns>
		public static T SelectRandomElement<T>(this IList<T> list)
		{
			if (list.IsEmpty())
				throw new ArgumentException("Cannot select random element from empty list.");
			if (random == null)
				random = new Random();
			return list[random.Next(0, list.Count)];
		}


		/// <summary>
		/// Shuffle elements in given list.
		/// </summary>
		/// <typeparam name="T">Type of list element.</typeparam>
		/// <param name="list"><see cref="IList{T}"/>.</param>
		public static void Shuffle<T>(this IList<T> list) => Shuffle(list, 0, list.Count);


		/// <summary>
		/// Shuffle elements in given list.
		/// </summary>
		/// <typeparam name="T">Type of list element.</typeparam>
		/// <param name="list"><see cref="IList{T}"/>.</param>
		/// <param name="index">Index of first element in list to shuffle.</param>
		/// <param name="count">Number of elements to shuffle.</param>
		public static void Shuffle<T>(this IList<T> list, int index, int count)
		{
			if (count <= 1)
				return;
			if (index < 0 || index >= list.Count)
				throw new ArgumentOutOfRangeException(nameof(index));
			if (index + count > list.Count)
				throw new ArgumentOutOfRangeException(nameof(count));
			if (random == null)
				random = new Random();
			var remaining = count;
			while (remaining > 0)
			{
				var i = random.Next(index, index + count);
				var j = random.Next(index, index + count);
				while (i == j)
					j = random.Next(index, index + count);
				var temp = list[i];
				list[i] = list[j];
				list[j] = temp;
				--remaining;
			}
		}


		/// <summary>
		/// Copy elements to array.
		/// </summary>
		/// <typeparam name="T">Type of list element.</typeparam>
		/// <param name="list"><see cref="IList{T}"/>.</param>
		/// <param name="index">Index of first element in list to copy.</param>
		/// <param name="count">Number of elements to copy.</param>
		/// <returns>Array of copied elements</returns>
		public static T[] ToArray<T>(this IList<T> list, int index, int count) => new T[count].Also((it) => list.CopyTo(index, it, 0, count));
	}
}
