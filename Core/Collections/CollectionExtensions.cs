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
	}
}
