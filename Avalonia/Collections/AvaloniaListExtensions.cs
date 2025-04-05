using Avalonia.Collections;
#if !NET9_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
#endif

namespace CarinaStudio.Collections;

/// <summary>
/// Extension methods for <see cref="AvaloniaList{T}"/> and <see cref="IAvaloniaList{T}"/>.
/// </summary>
public static class AvaloniaListExtensions
{
#if !NET9_0_OR_GREATER
    /// <summary>
    /// Check whether the given list is empty or not.
    /// </summary>
    /// <typeparam name="T">Type of element of list.</typeparam>
    /// <param name="list">List to check.</param>
    /// <returns>True if the list is empty.</returns>
    /// <remarks>The method is available for target framework before .NET 9.</remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsEmpty<T>(this AvaloniaList<T> list) => list.Count <= 0;
#endif
    
    
#if !NET9_0_OR_GREATER
    /// <summary>
    /// Check whether the given list is empty or not.
    /// </summary>
    /// <typeparam name="T">Type of element of list.</typeparam>
    /// <param name="list">List to check.</param>
    /// <returns>True if the list is empty.</returns>
    /// <remarks>The method is available for target framework before .NET 9.</remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsEmpty<T>(this IAvaloniaList<T> list) => list.Count <= 0;
#endif
    
    
#if !NET9_0_OR_GREATER
    /// <summary>
    /// Check whether the given list is not empty or not.
    /// </summary>
    /// <typeparam name="T">Type of element of list.</typeparam>
    /// <param name="list">List to check.</param>
    /// <returns>True if the list is not empty.</returns>
    /// <remarks>The method is available for target framework before .NET 9.</remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNotEmpty<T>([NotNullWhen(true)] this AvaloniaList<T>? list) => list is not null && list.Count > 0;
#endif
    
    
#if !NET9_0_OR_GREATER
    /// <summary>
    /// Check whether the given list is not empty or not.
    /// </summary>
    /// <typeparam name="T">Type of element of list.</typeparam>
    /// <param name="list">List to check.</param>
    /// <returns>True if the list is not empty.</returns>
    /// <remarks>The method is available for target framework before .NET 9.</remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNotEmpty<T>([NotNullWhen(true)] this IAvaloniaList<T>? list) => list is not null && list.Count > 0;
#endif
    
    
#if !NET9_0_OR_GREATER
    /// <summary>
    /// Check whether given list is null/empty or not.
    /// </summary>
    /// <typeparam name="T">Type of element of list.</typeparam>
    /// <param name="list">List to check.</param>
    /// <returns>True if the list is null or empty.</returns>
    /// <remarks>The method is available for target framework before .NET 9.</remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNullOrEmpty<T>([NotNullWhen(false)] this AvaloniaList<T>? list) => list is null || list.Count <= 0;
#endif
    
    
#if !NET9_0_OR_GREATER
    /// <summary>
    /// Check whether given list is null/empty or not.
    /// </summary>
    /// <typeparam name="T">Type of element of list.</typeparam>
    /// <param name="list">List to check.</param>
    /// <returns>True if the list is null or empty.</returns>
    /// <remarks>The method is available for target framework before .NET 9.</remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNullOrEmpty<T>([NotNullWhen(false)] this IAvaloniaList<T>? list) => list is null || list.Count <= 0;
#endif
}