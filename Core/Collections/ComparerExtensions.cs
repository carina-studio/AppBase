using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace CarinaStudio.Collections;

/// <summary>
/// Extensions for <see cref="IComparer{T}"/>.
/// </summary>
public static class ComparerExtensions
{
	/// <summary>
	/// Create new <see cref="IComparer{T}"/> which generates inverse result of given <see cref="IComparer{T}"/>.
	/// </summary>
	/// <typeparam name="T">Type of value to compare.</typeparam>
	/// <param name="comparer"><see cref="IComparer{T}"/>.</param>
	/// <returns><see cref="IComparer{T}"/> which generates inverse result of given <see cref="IComparer{T}"/>.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static IComparer<T> Invert<T>(this IComparer<T> comparer) => Comparer<T>.Create((x, y) => comparer.Compare(y, x));
}