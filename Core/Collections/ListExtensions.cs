﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace CarinaStudio.Collections;

/// <summary>
/// Extensions for <see cref="IList{T}"/> and <see cref="IList"/>.
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
	/// <returns>Read-only <see cref="IList{T}"/>, the instance also implements <see cref="IReadOnlyList{T}"/>.</returns>
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
	/// <param name="comparer"><see cref="IComparer{T}"/> to compare elements.</param>
	/// <returns>Index of found element, or bitwise complement of index of proper position to put element.</returns>
	public static int BinarySearch<T>(this IReadOnlyList<T> list, T element, IComparer<T> comparer) => BinarySearch(list, 0, list.Count, element, comparer.Compare);


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
	/// Use binary-search to find given element.
	/// </summary>
	/// <typeparam name="T">Type of element.</typeparam>
	/// <param name="list">List to find element.</param>
	/// <param name="element">Element to be found.</param>
	/// <param name="comparison">Comparison function.</param>
	/// <returns>Index of found element, or bitwise complement of index of proper position to put element.</returns>
	public static int BinarySearch<T>(this IReadOnlyList<T> list, T element, Comparison<T> comparison) => BinarySearch(list, 0, list.Count, element, comparison);
	
	
	/// <summary>
	/// Use binary-search to find given element by key.
	/// </summary>
	/// <typeparam name="T">Type of element.</typeparam>
	/// <typeparam name="TKey">Type of key of element.</typeparam>
	/// <param name="list">List to find element.</param>
	/// <param name="key">Key of element to be found.</param>
	/// <param name="keyGetter">Method to get key from element.</param>
	/// <returns>Index of found element, or bitwise complement of index of proper position to put element.</returns>
	public static int BinarySearch<T, TKey>(this List<T> list, TKey key, Func<T, TKey> keyGetter) where TKey : IComparable<TKey> => BinarySearch((IList<T>)list, 0, list.Count, key, keyGetter, (lhs, rhs) => lhs.CompareTo(rhs));


	/// <summary>
	/// Use binary-search to find given element by key.
	/// </summary>
	/// <typeparam name="T">Type of element.</typeparam>
	/// <typeparam name="TKey">Type of key of element.</typeparam>
	/// <param name="list">List to find element.</param>
	/// <param name="key">Key of element to be found.</param>
	/// <param name="keyGetter">Method to get key from element.</param>
	/// <returns>Index of found element, or bitwise complement of index of proper position to put element.</returns>
	public static int BinarySearch<T, TKey>(this IList<T> list, TKey key, Func<T, TKey> keyGetter) where TKey : IComparable<TKey> => BinarySearch(list, 0, list.Count, key, keyGetter, (lhs, rhs) => lhs.CompareTo(rhs));
	
	
	/// <summary>
	/// Use binary-search to find given element by key.
	/// </summary>
	/// <typeparam name="T">Type of element.</typeparam>
	/// <typeparam name="TKey">Type of key of element.</typeparam>
	/// <param name="list">List to find element.</param>
	/// <param name="key">Key of element to be found.</param>
	/// <param name="keyGetter">Method to get key from element.</param>
	/// <returns>Index of found element, or bitwise complement of index of proper position to put element.</returns>
	public static int BinarySearch<T, TKey>(this IReadOnlyList<T> list, TKey key, Func<T, TKey> keyGetter) where TKey : IComparable<TKey> => BinarySearch(list, 0, list.Count, key, keyGetter, (lhs, rhs) => lhs.CompareTo(rhs));


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
	public static int BinarySearch<T, TKey>(this List<T> list, TKey key, Func<T, TKey> keyGetter, Comparison<TKey> comparison) => BinarySearch((IList<T>)list, 0, list.Count, key, keyGetter, comparison);
	

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
	/// Use binary-search to find given element by key.
	/// </summary>
	/// <typeparam name="T">Type of element.</typeparam>
	/// <typeparam name="TKey">Type of key of element.</typeparam>
	/// <param name="list">List to find element.</param>
	/// <param name="key">Key of element to be found.</param>
	/// <param name="keyGetter">Method to get key from element.</param>
	/// <param name="comparison">Comparison function.</param>
	/// <returns>Index of found element, or bitwise complement of index of proper position to put element.</returns>
	public static int BinarySearch<T, TKey>(this IReadOnlyList<T> list, TKey key, Func<T, TKey> keyGetter, Comparison<TKey> comparison) => BinarySearch(list, 0, list.Count, key, keyGetter, comparison);
	
	
	/// <summary>
	/// Use binary-search to find given element.
	/// </summary>
	/// <typeparam name="T">Type of element.</typeparam>
	/// <param name="list">List to find element.</param>
	/// <param name="element">Element to be found.</param>
	/// <returns>Index of found element, or bitwise complement of index of proper position to put element.</returns>
	public static int BinarySearch<T>(this IList<T> list, T element) where T : IComparable<T> => BinarySearch(list, 0, list.Count, element);
	
	
	/// <summary>
	/// Use binary-search to find given element.
	/// </summary>
	/// <typeparam name="T">Type of element.</typeparam>
	/// <param name="list">List to find element.</param>
	/// <param name="element">Element to be found.</param>
	/// <returns>Index of found element, or bitwise complement of index of proper position to put element.</returns>
	public static int BinarySearch<T>(this IReadOnlyList<T> list, T element) where T : IComparable<T> => BinarySearch(list, 0, list.Count, element);


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
	
	
	// Binary search.
	static int BinarySearch<T>(IReadOnlyList<T> list, int start, int end, T element, Comparison<T> comparison)
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
	static int BinarySearch<T, TKey>(IReadOnlyList<T> list, int start, int end, TKey key, Func<T, TKey> keyGetter, Comparison<TKey> comparison)
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
	static int BinarySearch<T>(IReadOnlyList<T> list, int start, int end, T element) where T : IComparable<T>
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
	/// Case given list to generic typed list.
	/// </summary>
	/// <remarks>If <paramref name="list"/> implements <see cref="INotifyCollectionChanged"/> then returned <see cref="IList{T}"/> will also implements <see cref="INotifyCollectionChanged"/>.</remarks>
	/// <param name="list">List.</param>
	/// <typeparam name="TOut">Type of element.</typeparam>
	/// <returns>Generic typed list.</returns>
	public static IList<TOut> Cast<TOut>(this IList list)
	{
		if (list is IList<TOut> typedList)
			return typedList;
		if (list.Count > 0 && !(list[0] is TOut) && list[0] != null)
			throw new InvalidCastException();
		if (list is INotifyCollectionChanged)
			return new TypeCastingObservableList<TOut>(list);
		return new TypeCastingList<TOut>(list);
	}
	
	
	/// <summary>
	/// Copy elements to array.
	/// </summary>
	/// <typeparam name="T">Type of list element.</typeparam>
	/// <param name="list">The list.</param>
	/// <param name="index">Index of first element in list to copy.</param>
	/// <param name="array">Array to place copied elements.</param>
	/// <param name="arrayIndex">Index of first position in <paramref name="array"/> to place copied elements.</param>
	/// <param name="count">Number of elements to copy.</param>
	public static void CopyTo<T>(this List<T> list, int index, T[] array, int arrayIndex, int count) => CopyTo((IList<T>)list, index, array, arrayIndex, count);


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
	/// Copy elements to array.
	/// </summary>
	/// <typeparam name="T">Type of list element.</typeparam>
	/// <param name="list"><see cref="IList{T}"/>.</param>
	/// <param name="index">Index of first element in list to copy.</param>
	/// <param name="array">Array to place copied elements.</param>
	/// <param name="arrayIndex">Index of first position in <paramref name="array"/> to place copied elements.</param>
	/// <param name="count">Number of elements to copy.</param>
	public static void CopyTo<T>(this IReadOnlyList<T> list, int index, T[] array, int arrayIndex, int count)
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
	/// Get read-only view of range of source list which allows accessing elements from source list directly without copying.
	/// </summary>
	/// <param name="list">Source list.</param>
	/// <param name="start">Start index of range.</param>
	/// <param name="count">Number of elements needed to be included.</param>
	/// <typeparam name="T">Type of element.</typeparam>
	/// <returns>View of range of source list.</returns>
	/// <remarks>The element get from view and <see cref="ICollection{T}.Count"/> of view may be changed if source list has been modified.</remarks>
	public static IList<T> GetRangeView<T>(this List<T> list, int start, int count) => GetRangeView((IList<T>)list, start, count);


	/// <summary>
	/// Get read-only view of range of source list which allows accessing elements from source list directly without copying.
	/// </summary>
	/// <param name="list">Source list.</param>
	/// <param name="start">Start index of range.</param>
	/// <param name="count">Number of elements needed to be included.</param>
	/// <typeparam name="T">Type of element.</typeparam>
	/// <returns>View of range of source list.</returns>
	/// <remarks>The element get from view and <see cref="ICollection{T}.Count"/> of view may be changed if source list has been modified.</remarks>
	public static IList<T> GetRangeView<T>(this IList<T> list, int start, int count) =>
		new ListRangeView<T>(list, start, count);
	
	
	/// <summary>
	/// Get read-only view of range of source list which allows accessing elements from source list directly without copying.
	/// </summary>
	/// <param name="list">Source list.</param>
	/// <param name="start">Start index of range.</param>
	/// <param name="count">Number of elements needed to be included.</param>
	/// <typeparam name="T">Type of element.</typeparam>
	/// <returns>View of range of source list.</returns>
	/// <remarks>The element get from view and <see cref="ICollection{T}.Count"/> of view may be changed if source list has been modified.</remarks>
	public static IReadOnlyList<T> GetRangeView<T>(this IReadOnlyList<T> list, int start, int count) =>
		new ReadOnlyListRangeView<T>(list, start, count);
	
	
	/// <summary>
	/// Check whether the given list is empty or not.
	/// </summary>
	/// <typeparam name="T">Type of element of collection.</typeparam>
	/// <param name="list">List to check.</param>
	/// <returns>True if the list is empty.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsEmpty<T>(this List<T> list) => list.Count <= 0;


	/// <summary>
	/// Check whether the given list is not empty or not.
	/// </summary>
	/// <typeparam name="T">Type of element of collection.</typeparam>
	/// <param name="list">List to check.</param>
	/// <returns>True if the list is not empty.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsNotEmpty<T>([NotNullWhen(true)] this List<T>? list) => list is not null && list.Count > 0;
	
	
	/// <summary>
	/// Check whether given list is null/empty or not.
	/// </summary>
	/// <typeparam name="T">Type of element of collection.</typeparam>
	/// <param name="collection">List to check.</param>
	/// <returns>True if the list is null or empty.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsNullOrEmpty<T>([NotNullWhen(false)] this List<T>? collection) => collection is null || collection.Count <= 0;
	
	
	/// <summary>
	/// Check whether elements in given list is sorted or not.
	/// </summary>
	/// <typeparam name="T">Type of elements.</typeparam>
	/// <param name="list">List.</param>
	/// <param name="comparer"><see cref="IComparer{T}"/> to check order of elements.</param>
	/// <returns>True if elements in the list are sorted.</returns>
	public static bool IsSorted<T>(this List<T> list, IComparer<T> comparer) => IsSorted(list, comparer.Compare);


	/// <summary>
	/// Check whether elements in given <see cref="IList{T}"/> is sorted or not.
	/// </summary>
	/// <typeparam name="T">Type of elements.</typeparam>
	/// <param name="list"><see cref="IList{T}"/>.</param>
	/// <param name="comparer"><see cref="IComparer{T}"/> to check order of elements.</param>
	/// <returns>True if elements in <see cref="IList{T}"/> are sorted.</returns>
	public static bool IsSorted<T>(this IList<T> list, IComparer<T> comparer) => IsSorted(list, comparer.Compare);
	
	
	/// <summary>
	/// Check whether elements in given <see cref="IReadOnlyList{T}"/> is sorted or not.
	/// </summary>
	/// <typeparam name="T">Type of elements.</typeparam>
	/// <param name="list"><see cref="IReadOnlyList{T}"/>.</param>
	/// <param name="comparer"><see cref="IComparer{T}"/> to check order of elements.</param>
	/// <returns>True if elements in <see cref="IReadOnlyList{T}"/> are sorted.</returns>
	public static bool IsSorted<T>(this IReadOnlyList<T> list, IComparer<T> comparer) => IsSorted(list, comparer.Compare);


	/// <summary>
	/// Check whether elements in given <see cref="IList{T}"/> is sorted or not.
	/// </summary>
	/// <typeparam name="T">Type of elements.</typeparam>
	/// <param name="list"><see cref="IList{T}"/>.</param>
	/// <param name="comparison">Comparison method to check order of elements.</param>
	/// <returns>True if elements in <see cref="IList{T}"/> are sorted.</returns>
	public static bool IsSorted<T>(this List<T> list, Comparison<T> comparison) => IsSorted((IList<T>)list, comparison);


	/// <summary>
	/// Check whether elements in given <see cref="IList{T}"/> is sorted or not.
	/// </summary>
	/// <typeparam name="T">Type of elements.</typeparam>
	/// <param name="list"><see cref="IList{T}"/>.</param>
	/// <param name="comparison">Comparison method to check order of elements.</param>
	/// <returns>True if elements in <see cref="IList{T}"/> are sorted.</returns>
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
	/// Check whether elements in given <see cref="IReadOnlyList{T}"/> is sorted or not.
	/// </summary>
	/// <typeparam name="T">Type of elements.</typeparam>
	/// <param name="list"><see cref="IReadOnlyList{T}"/>.</param>
	/// <param name="comparison">Comparison method to check order of elements.</param>
	/// <returns>True if elements in <see cref="IReadOnlyList{T}"/> are sorted.</returns>
	public static bool IsSorted<T>(this IReadOnlyList<T> list, Comparison<T> comparison)
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
	/// Check whether elements in given list is sorted or not.
	/// </summary>
	/// <typeparam name="T">Type of elements.</typeparam>
	/// <param name="list"><see cref="IList{T}"/>.</param>
	/// <returns>True if elements in the list are sorted.</returns>
	public static bool IsSorted<T>(this List<T> list) where T : IComparable<T> => IsSorted((IList<T>)list);


	/// <summary>
	/// Check whether elements in given <see cref="IList{T}"/> is sorted or not.
	/// </summary>
	/// <typeparam name="T">Type of elements.</typeparam>
	/// <param name="list"><see cref="IList{T}"/>.</param>
	/// <returns>True if elements in <see cref="IList{T}"/> are sorted.</returns>
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
	/// Check whether elements in given <see cref="IReadOnlyList{T}"/> is sorted or not.
	/// </summary>
	/// <typeparam name="T">Type of elements.</typeparam>
	/// <param name="list"><see cref="IReadOnlyList{T}"/>.</param>
	/// <returns>True if elements in <see cref="IReadOnlyList{T}"/> are sorted.</returns>
	public static bool IsSorted<T>(this IReadOnlyList<T> list) where T : IComparable<T>
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
	/// Make given list as reversed <see cref="IList{T}"/>.
	/// </summary>
	/// <typeparam name="T">Type of element.</typeparam>
	/// <param name="list">The list to be reversed.</param>
	/// <returns>Reversed <see cref="IList{T}"/>.</returns>
	public static IList<T> Reverse<T>(this List<T> list) => Reverse((IList<T>)list);


	/// <summary>
	/// Make given <see cref="IList{T}"/> as reversed <see cref="IList{T}"/>.
	/// </summary>
	/// <remarks>If <paramref name="list"/> implements <see cref="INotifyCollectionChanged"/> then returned <see cref="IList{T}"/> will also implements <see cref="INotifyCollectionChanged"/>.</remarks>
	/// <typeparam name="T">Type of element.</typeparam>
	/// <param name="list"><see cref="IList{T}"/> to be reversed.</param>
	/// <returns>Reversed <see cref="IList{T}"/>, the instance also implements <see cref="IReadOnlyList{T}"/>.</returns>
	public static IList<T> Reverse<T>(this IList<T> list)
	{
		return list is INotifyCollectionChanged 
			? new ReversedObservableList<T>(list, list.IsReadOnly) 
			: new ReversedList<T>(list, list.IsReadOnly);
	}
	
	
	/// <summary>
	/// Make given <see cref="IReadOnlyList{T}"/> as reversed <see cref="IReadOnlyList{T}"/>.
	/// </summary>
	/// <remarks>If <paramref name="list"/> implements <see cref="INotifyCollectionChanged"/> then returned <see cref="IReadOnlyList{T}"/> will also implements <see cref="INotifyCollectionChanged"/>.</remarks>
	/// <typeparam name="T">Type of element.</typeparam>
	/// <param name="list"><see cref="IReadOnlyList{T}"/> to be reversed.</param>
	/// <returns>Reversed <see cref="IReadOnlyList{T}"/>.</returns>
	public static IReadOnlyList<T> Reverse<T>(this IReadOnlyList<T> list)
	{
		return list is INotifyCollectionChanged 
			? new ReadOnlyReversedObservableList<T>(list) 
			: new ReadOnlyReversedList<T>(list);
	}
	
	
	/// <summary>
	/// Make given <see cref="IList{T}"/> as read-only <see cref="IList{T}"/> which reversed its items.
	/// </summary>
	/// <remarks>If <paramref name="list"/> implements <see cref="INotifyCollectionChanged"/> then returned <see cref="IList{T}"/> will also implements <see cref="INotifyCollectionChanged"/>.</remarks>
	/// <typeparam name="T">Type of element.</typeparam>
	/// <param name="list"><see cref="IList{T}"/> to be reversed.</param>
	/// <returns>Read-only <see cref="IList{T}"/> which reverses its items.</returns>
	public static IList<T> ReverseAsReadOnly<T>(this IList<T> list)
	{
		return list is INotifyCollectionChanged 
			? new ReversedObservableList<T>(list, true) 
			: new ReversedList<T>(list, true);
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
		random ??= new Random();
		return list[random.Next(0, list.Count)];
	}
	
	
	/// <summary>
	/// Select an element from given <see cref="IList{T}"/> randomly.
	/// </summary>
	/// <typeparam name="T">Type of element.</typeparam>
	/// <param name="list"><see cref="IList{T}"/>.</param>
	/// <returns>Element selected from <see cref="IList{T}"/>.</returns>
	public static T SelectRandomElement<T>(this IReadOnlyList<T> list)
	{
		if (list.IsEmpty())
			throw new ArgumentException("Cannot select random element from empty list.");
		random ??= new Random();
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
		random ??= new Random();
		var remaining = count;
		while (remaining > 0)
		{
			var i = random.Next(index, index + count);
			var j = random.Next(index, index + count);
			while (i == j)
				j = random.Next(index, index + count);
			(list[i], list[j]) = (list[j], list[i]);
			--remaining;
		}
	}
	
	
	/// <summary>
	/// Copy elements to array.
	/// </summary>
	/// <typeparam name="T">Type of list element.</typeparam>
	/// <param name="list"><see cref="List{T}"/>.</param>
	/// <param name="index">Index of first element in list to copy.</param>
	/// <param name="count">Number of elements to copy.</param>
	/// <returns>Array of copied elements</returns>
	public static T[] ToArray<T>(this List<T> list, int index, int count) => ToArray((IList<T>)list, index, count);


	/// <summary>
	/// Copy elements to array.
	/// </summary>
	/// <typeparam name="T">Type of list element.</typeparam>
	/// <param name="list"><see cref="IList{T}"/>.</param>
	/// <param name="index">Index of first element in list to copy.</param>
	/// <param name="count">Number of elements to copy.</param>
	/// <returns>Array of copied elements</returns>
	public static T[] ToArray<T>(this IList<T> list, int index, int count) => new T[count].Also(it => list.CopyTo(index, it, 0, count));
	
	
	/// <summary>
	/// Copy elements to array.
	/// </summary>
	/// <typeparam name="T">Type of list element.</typeparam>
	/// <param name="list"><see cref="IReadOnlyList{T}"/>.</param>
	/// <param name="index">Index of first element in list to copy.</param>
	/// <param name="count">Number of elements to copy.</param>
	/// <returns>Array of copied elements</returns>
	public static T[] ToArray<T>(this IReadOnlyList<T> list, int index, int count) => new T[count].Also(it => list.CopyTo(index, it, 0, count));
}


// View of sub range of list.
internal class ListRangeView<T> : IList, IList<T>, IReadOnlyList<T>
{
	// Enumerator.
	class Enumerator : IEnumerator<T>
	{
		// Fields.
		int currentIndex = -1;
		readonly int initCount;
		bool isEnded;
		readonly ListRangeView<T> view;

		// Constructor.
		public Enumerator(ListRangeView<T> view)
		{
			this.initCount = view.Count;
			this.view = view;
		}

		/// <inheritdoc/>
		public T Current
		{
			get
			{
				if (isEnded || this.currentIndex < 0)
					throw new InvalidOperationException();
				if (this.view.Count != this.initCount)
					throw new InvalidOperationException();
				return this.view[this.currentIndex];
			}
		}

		/// <inheritdoc/>
		public void Dispose() =>
			this.isEnded = true;

		/// <inheritdoc/>
		public bool MoveNext()
		{
			if (this.isEnded)
				return false;
			if (this.view.Count != this.initCount)
				throw new InvalidOperationException();
			++this.currentIndex;
			if (this.currentIndex >= this.initCount)
			{
				this.isEnded = true;
				return false;
			}
			return true;
		}

		// Interface implementations.
		object? IEnumerator.Current => this.Current;

		void IEnumerator.Reset()
		{
		}
	}


	// Fields.
	readonly int count;
	readonly IList<T> list;
	readonly int start;


	// Constructor.
	public ListRangeView(IList<T> list, int start, int count)
	{
		if (start < 0)
			throw new ArgumentOutOfRangeException(nameof(start));
		if (count < 0)
			throw new ArgumentOutOfRangeException(nameof(count));
		if ((long)start + count > int.MaxValue)
			throw new ArgumentOutOfRangeException();
		this.count = count;
		this.list = list;
		this.start = start;
	}


	/// <inheritdoc/>
	public bool Contains(T item) =>
		this.IndexOf(item) >= 0;


	/// <inheritdoc/>
	public void CopyTo(T[] array, int arrayIndex)
	{
		var count = this.Count;
		if (count < 0)
			return;
		this.list.CopyTo(this.start, array, arrayIndex, count);
	}


	/// <inheritdoc cref="ICollection{T}.Count"/>
	public int Count
	{
		get
		{
			if (this.start >= this.list.Count)
				return 0;
			return Math.Min(this.count, this.list.Count - this.start);
		}
	}


	/// <inheritdoc/>
	public IEnumerator<T> GetEnumerator() =>
		new Enumerator(this);


	/// <inheritdoc/>
	public int IndexOf(T item)
	{
		var endIndex = Math.Min(this.start + this.count, this.list.Count);
		if (this.list is SortedObservableList<T> sortedList)
		{
			var index = sortedList.IndexOf(item);
			if (index >= this.start && index < this.start + this.count)
				return (index - this.start);
		}
		else
		{
			for (var i = this.start; i < endIndex; ++i)
			{
				if (Equals(item, this.list[i]))
					return (i - this.start);
			}
		}
		return -1;
	}


	/// <inheritdoc cref="ICollection{T}.IsReadOnly"/>
	public bool IsReadOnly => true;


	// Get element.
	public T this[int index]
	{
		get
		{
			if (index < 0 || index >= this.count)
				throw new ArgumentOutOfRangeException(nameof(index));
			return this.list[this.start + index];
		}
	}


	// Interface implementations.
	int IList.Add(object? item) =>
		throw new InvalidOperationException();

	void ICollection<T>.Add(T item) =>
		throw new InvalidOperationException();

	void IList.Clear() =>
		throw new InvalidOperationException();

	void ICollection<T>.Clear() =>
		throw new InvalidOperationException();

	bool IList.Contains(object? item) =>
		item is T e && this.list.Contains(e);

	void ICollection.CopyTo(Array array, int arrayIndex)
	{
		var typedArray = new T[this.Count];
		this.list.CopyTo(typedArray, 0);
		for (var i = 0; i < typedArray.Length; ++i, ++arrayIndex)
			array.SetValue(typedArray[i], arrayIndex);
	}

	IEnumerator IEnumerable.GetEnumerator() =>
		this.list.GetEnumerator();

	int IList.IndexOf(object? item) =>
		item is T e ? this.list.IndexOf(e) : -1;

	void IList.Insert(int index, object? item) =>
		throw new InvalidOperationException();

	void IList<T>.Insert(int index, T item) =>
		throw new InvalidOperationException();

	bool IList.IsFixedSize => false;
	bool ICollection.IsSynchronized => false;

	void IList.Remove(object? item) =>
		throw new InvalidOperationException();

	bool ICollection<T>.Remove(T item) =>
		throw new InvalidOperationException();

	void IList.RemoveAt(int index) =>
		throw new InvalidOperationException();

	void IList<T>.RemoveAt(int index) =>
		throw new InvalidOperationException();

	object ICollection.SyncRoot => this;

	object? IList.this[int index]
	{
		get => this[index];
		set => throw new InvalidOperationException();
	}

	T IList<T>.this[int index]
	{
		get => this[index];
		set => throw new InvalidOperationException();
	}
}


// View of sub range of list.
internal class ReadOnlyListRangeView<T> : IReadOnlyList<T>
{
	// Enumerator.
	class Enumerator : IEnumerator<T>
	{
		// Fields.
		int currentIndex = -1;
		readonly int initCount;
		bool isEnded;
		readonly ReadOnlyListRangeView<T> view;

		// Constructor.
		public Enumerator(ReadOnlyListRangeView<T> view)
		{
			this.initCount = view.Count;
			this.view = view;
		}

		/// <inheritdoc/>
		public T Current
		{
			get
			{
				if (isEnded || this.currentIndex < 0)
					throw new InvalidOperationException();
				if (this.view.Count != this.initCount)
					throw new InvalidOperationException();
				return this.view[this.currentIndex];
			}
		}

		/// <inheritdoc/>
		public void Dispose() =>
			this.isEnded = true;

		/// <inheritdoc/>
		public bool MoveNext()
		{
			if (this.isEnded)
				return false;
			if (this.view.Count != this.initCount)
				throw new InvalidOperationException();
			++this.currentIndex;
			if (this.currentIndex >= this.initCount)
			{
				this.isEnded = true;
				return false;
			}
			return true;
		}

		// Interface implementations.
		object? IEnumerator.Current => this.Current;

		void IEnumerator.Reset()
		{ }
	}


	// Fields.
	readonly int count;
	readonly IReadOnlyList<T> list;
	readonly int start;


	// Constructor.
	public ReadOnlyListRangeView(IReadOnlyList<T> list, int start, int count)
	{
		if (start < 0)
			throw new ArgumentOutOfRangeException(nameof(start));
		if (count < 0)
			throw new ArgumentOutOfRangeException(nameof(count));
		if ((long)start + count > int.MaxValue)
			throw new ArgumentOutOfRangeException();
		this.count = count;
		this.list = list;
		this.start = start;
	}


	/// <inheritdoc cref="ICollection{T}.Count"/>
	public int Count
	{
		get
		{
			if (this.start >= this.list.Count)
				return 0;
			return Math.Min(this.count, this.list.Count - this.start);
		}
	}


	/// <inheritdoc/>
	public IEnumerator<T> GetEnumerator() =>
		new Enumerator(this);


	/// <inheritdoc cref="ICollection{T}.IsReadOnly"/>
	public bool IsReadOnly => true;


	// Get element.
	public T this[int index]
	{
		get
		{
			if (index < 0 || index >= this.count)
				throw new ArgumentOutOfRangeException(nameof(index));
			return this.list[this.start + index];
		}
	}


	// Interface implementation
	IEnumerator IEnumerable.GetEnumerator() =>
		this.list.GetEnumerator();
}