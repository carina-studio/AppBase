using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNotEmpty<T>([NotNullWhen(true)] LinkedList<T>? list) => list is not null && list.Count > 0;
#endif
    
    
#if !NET9_0_OR_GREATER
    /// <summary>
    /// Check whether given linked-list is null/empty or not.
    /// </summary>
    /// <typeparam name="T">Type of element of linked-list.</typeparam>
    /// <param name="list">Linked-list to check.</param>
    /// <returns>True if the linked-list is null or empty.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNullOrEmpty<T>([NotNullWhen(false)] LinkedList<T>? list) => list is null || list.Count <= 0;
#endif
}