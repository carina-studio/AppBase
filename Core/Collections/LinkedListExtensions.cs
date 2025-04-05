using System.Collections.Generic;
#if !NET9_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
#endif

namespace CarinaStudio.Collections;

/// <summary>
/// Extension methods for <see cref="LinkedList{T}"/>.
/// </summary>
public static class LinkedListExtensions
{
#if !NET9_0_OR_GREATER
    /// <summary>
    /// Check whether the given linked-list is empty or not.
    /// </summary>
    /// <typeparam name="T">Type of element of linked-list.</typeparam>
    /// <param name="list">Linked-list to check.</param>
    /// <returns>True if the linked-list is empty.</returns>
    /// <remarks>The method is available for target framework before .NET 9.</remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsEmpty<T>(this LinkedList<T> list) => list.Count <= 0;
#endif


#if !NET9_0_OR_GREATER
    /// <summary>
    /// Check whether the given linked-list is not empty or not.
    /// </summary>
    /// <typeparam name="T">Type of element of linked-list.</typeparam>
    /// <param name="list">Linked-list to check.</param>
    /// <returns>True if the linked-list is not empty.</returns>
    /// <remarks>The method is available for target framework before .NET 9.</remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNotEmpty<T>([NotNullWhen(true)] this LinkedList<T>? list) => list is not null && list.Count > 0;
#endif
    
    
#if !NET9_0_OR_GREATER
    /// <summary>
    /// Check whether given linked-list is null/empty or not.
    /// </summary>
    /// <typeparam name="T">Type of element of linked-list.</typeparam>
    /// <param name="list">Linked-list to check.</param>
    /// <returns>True if the linked-list is null or empty.</returns>
    /// <remarks>The method is available for target framework before .NET 9.</remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNullOrEmpty<T>([NotNullWhen(false)] this LinkedList<T>? list) => list is null || list.Count <= 0;
#endif
}