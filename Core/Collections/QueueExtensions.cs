using System.Collections.Generic;
#if !NET9_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
#endif

namespace CarinaStudio.Collections;

/// <summary>
/// Extensions for <see cref="Queue{T}"/>.
/// </summary>
public static class QueueExtensions
{
#if !NET9_0_OR_GREATER
    /// <summary>
    /// Check whether the given queue is empty or not.
    /// </summary>
    /// <typeparam name="T">Type of element of queue.</typeparam>
    /// <param name="queue">Queue to check.</param>
    /// <returns>True if the queue is empty.</returns>
    /// <remarks>The method is available for target framework before .NET 9.</remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsEmpty<T>(this Queue<T> queue) => queue.Count <= 0;
#endif


#if !NET9_0_OR_GREATER
    /// <summary>
    /// Check whether the given queue is not empty or not.
    /// </summary>
    /// <typeparam name="T">Type of element of queue.</typeparam>
    /// <param name="queue">Queue to check.</param>
    /// <returns>True if the queue is not empty.</returns>
    /// <remarks>The method is available for target framework before .NET 9.</remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNotEmpty<T>([NotNullWhen(true)] this Queue<T>? queue) => queue is not null && queue.Count > 0;
#endif

    
#if !NET9_0_OR_GREATER
    /// <summary>
    /// Check whether given queue is null/empty or not.
    /// </summary>
    /// <typeparam name="T">Type of element of queue.</typeparam>
    /// <param name="queue">Queue to check.</param>
    /// <returns>True if the queue is null or empty.</returns>
    /// <remarks>The method is available for target framework before .NET 9.</remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNullOrEmpty<T>([NotNullWhen(false)] this Queue<T>? queue) => queue is null || queue.Count <= 0;
#endif
 
    
    /// <summary>
    /// Try dequeuing value from queue as given type.
    /// </summary>
    /// <param name="queue"><see cref="Queue{T}"/>.</param>
    /// <param name="value">Dequeued value.</param>
    /// <typeparam name="T">Type of value in queue.</typeparam>
    /// <typeparam name="TOut">Desired type of value.</typeparam>
    /// <returns>True if value dequeued as given type successfully.</returns>
    public static bool TryDequeue<T, TOut>(this Queue<T> queue, out TOut value) where TOut : T
    {
        if (queue.TryPeek(out var rawValue) && rawValue is TOut outValue)
        {
            queue.Dequeue();
            value = outValue;
            return true;
        }
#pragma warning disable CS8601
        value = default;
#pragma warning restore CS8601
        return false;
    }


    /// <summary>
    /// Try peeking value in queue as given type.
    /// </summary>
    /// <param name="queue"><see cref="Queue{T}"/>.</param>
    /// <param name="value">Peeked value.</param>
    /// <typeparam name="T">Type of value in queue.</typeparam>
    /// <typeparam name="TOut">Desired type of value.</typeparam>
    /// <returns>True if value peeked as given type successfully.</returns>
    public static bool TryPeek<T, TOut>(this Queue<T> queue, out TOut value) where TOut : T
    {
        if (queue.TryPeek(out var rawValue) && rawValue is TOut outValue)
        {
            value = outValue;
            return true;
        }
#pragma warning disable CS8601
        value = default;
#pragma warning restore CS8601
        return false;
    }
}