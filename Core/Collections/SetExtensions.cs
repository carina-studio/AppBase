using System.Collections.Generic;
#if !NET9_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
#endif

namespace CarinaStudio.Collections;

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


#if !NET10_0_OR_GREATER
	/// <summary>
	/// Make <see cref="ISet{T}"/> as read-only.
	/// </summary>
	/// <typeparam name="T">Type of element.</typeparam>
	/// <param name="set"><see cref="ISet{T}"/>.</param>
	/// <returns>Read-only <see cref="ISet{T}"/>.</returns>
	/// <remarks>The method is available for target framework before .NET 10.</remarks>
	public static ISet<T> AsReadOnly<T>(this ISet<T> set)
	{
		if (set.IsReadOnly)
			return set;
		return new ReadOnlySet<T>(set);
	}
#endif
	
	
#if !NET9_0_OR_GREATER
	/// <summary>
	/// Check whether the given set is empty or not.
	/// </summary>
	/// <typeparam name="T">Type of element of set.</typeparam>
	/// <param name="set">Set to check.</param>
	/// <returns>True if the set is empty.</returns>
	/// <remarks>The method is available for target framework before .NET 9.</remarks>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsEmpty<T>(this HashSet<T> set) => set.Count <= 0;
#endif
	
	
#if !NET9_0_OR_GREATER
	/// <summary>
	/// Check whether the given set is empty or not.
	/// </summary>
	/// <typeparam name="T">Type of element of set.</typeparam>
	/// <param name="set">Set to check.</param>
	/// <returns>True if the set is empty.</returns>
	/// <remarks>The method is available for target framework before .NET 9.</remarks>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsEmpty<T>(this SortedSet<T> set) => set.Count <= 0;
#endif
	
	
#if !NET9_0_OR_GREATER
	/// <summary>
	/// Check whether the given set is empty or not.
	/// </summary>
	/// <typeparam name="T">Type of element of set.</typeparam>
	/// <param name="set">Set to check.</param>
	/// <returns>True if the set is empty.</returns>
	/// <remarks>The method is available for target framework before .NET 9.</remarks>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsEmpty<T>(this ISet<T> set) => set.Count <= 0;
#endif
	
	
#if !NET9_0_OR_GREATER
	/// <summary>
	/// Check whether the given set is not empty or not.
	/// </summary>
	/// <typeparam name="T">Type of element of set.</typeparam>
	/// <param name="set">Set to check.</param>
	/// <returns>True if the set is not empty.</returns>
	/// <remarks>The method is available for target framework before .NET 9.</remarks>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsNotEmpty<T>([NotNullWhen(true)] this HashSet<T>? set) => set is not null && set.Count > 0;
#endif
	
	
#if !NET9_0_OR_GREATER
	/// <summary>
	/// Check whether the given set is not empty or not.
	/// </summary>
	/// <typeparam name="T">Type of element of set.</typeparam>
	/// <param name="set">Set to check.</param>
	/// <returns>True if the set is not empty.</returns>
	/// <remarks>The method is available for target framework before .NET 9.</remarks>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsNotEmpty<T>([NotNullWhen(true)] this SortedSet<T>? set) => set is not null && set.Count > 0;
#endif


#if !NET9_0_OR_GREATER
	/// <summary>
	/// Check whether the given set is not empty or not.
	/// </summary>
	/// <typeparam name="T">Type of element of set.</typeparam>
	/// <param name="set">Set to check.</param>
	/// <returns>True if the set is not empty.</returns>
	/// <remarks>The method is available for target framework before .NET 9.</remarks>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsNotEmpty<T>([NotNullWhen(true)] this ISet<T>? set) => set is not null && set.Count > 0;
#endif
	
	
#if !NET9_0_OR_GREATER
	/// <summary>
	/// Check whether given set is null/empty or not.
	/// </summary>
	/// <typeparam name="T">Type of element of set.</typeparam>
	/// <param name="set">Set to check.</param>
	/// <returns>True if the set is null or empty.</returns>
	/// <remarks>The method is available for target framework before .NET 9.</remarks>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsNullOrEmpty<T>([NotNullWhen(false)] this HashSet<T>? set) => set is null || set.Count <= 0;
#endif
	
	
#if !NET9_0_OR_GREATER
	/// <summary>
	/// Check whether given set is null/empty or not.
	/// </summary>
	/// <typeparam name="T">Type of element of set.</typeparam>
	/// <param name="set">Set to check.</param>
	/// <returns>True if the set is null or empty.</returns>
	/// <remarks>The method is available for target framework before .NET 9.</remarks>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsNullOrEmpty<T>([NotNullWhen(false)] this SortedSet<T>? set) => set is null || set.Count <= 0;
#endif
	

#if !NET9_0_OR_GREATER
	/// <summary>
	/// Check whether given set is null/empty or not.
	/// </summary>
	/// <typeparam name="T">Type of element of set.</typeparam>
	/// <param name="set">Set to check.</param>
	/// <returns>True if the set is null or empty.</returns>
	/// <remarks>The method is available for target framework before .NET 9.</remarks>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsNullOrEmpty<T>([NotNullWhen(false)] this ISet<T>? set) => set is null || set.Count <= 0;
#endif
}
