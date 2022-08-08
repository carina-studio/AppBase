using System.Collections.Generic;

namespace CarinaStudio.Collections
{
    /// <summary>
    /// Extensions for <see cref="Queue{T}"/>.
    /// </summary>
    public static class QueueExtensions
    {
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
}