using System;
using System.Collections.Generic;
using System.Text;

namespace CarinaStudio.Collections
{
	/// <summary>
	/// Extensions for <see cref="IList{T}"/>.
	/// </summary>
	public static class ListExtensions
	{
		/// <summary>
		/// Find given element by binary-search.
		/// </summary>
		/// <typeparam name="T">Type of element.</typeparam>
		/// <param name="list">List to find element.</param>
		/// <param name="element">Element to be found.</param>
		/// <param name="comparer"><see cref="IComparer{T}"/> to compare elements.</param>
		/// <returns>Index of found element, or bitwise complement of index of proper position to put element.</returns>
		public static int BinarySearch<T>(this IList<T> list, T element, IComparer<T> comparer) => BinarySearch<T>(list, 0, list.Count, element, comparer.Compare);


		/// <summary>
		/// Find given element by binary-search.
		/// </summary>
		/// <typeparam name="T">Type of element.</typeparam>
		/// <param name="list">List to find element.</param>
		/// <param name="element">Element to be found.</param>
		/// <param name="comparison">Comparison function.</param>
		/// <returns>Index of found element, or bitwise complement of index of proper position to put element.</returns>
		public static int BinarySearch<T>(this IList<T> list, T element, Comparison<T> comparison) => BinarySearch<T>(list, 0, list.Count, element, comparison);


		/// <summary>
		/// Find given element by binary-search.
		/// </summary>
		/// <typeparam name="T">Type of element.</typeparam>
		/// <param name="list">List to find element.</param>
		/// <param name="element">Element to be found.</param>
		/// <returns>Index of found element, or bitwise complement of index of proper position to put element.</returns>
		public static int BinarySearch<T>(this IList<T> list, T element) where T : IComparable<T> => BinarySearch<T>(list, 0, list.Count, element);


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
				return BinarySearch<T>(list, start, middle, element, comparison);
			return BinarySearch<T>(list, middle + 1, end, element, comparison);
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
				return BinarySearch<T>(list, start, middle, element);
			return BinarySearch<T>(list, middle + 1, end, element);
		}
	}
}
