using System.Collections.Generic;

namespace CarinaStudio.Collections
{
    /// <summary>
    /// Extensions for <see cref="Stack{T}"/>.
    /// </summary>
    public static class StackExtensions
    {
        /// <summary>
        /// Try peeking value in stack as given type.
        /// </summary>
        /// <param name="stack"><see cref="Stack{T}"/>.</param>
        /// <param name="value">Peeked value.</param>
        /// <typeparam name="T">Type of value in stack.</typeparam>
        /// <typeparam name="TOut">Desired type of value.</typeparam>
        /// <returns>True if value peeked as given type successfully.</returns>
        public static bool TryPeek<T, TOut>(this Stack<T> stack, out TOut value) where TOut : T
        {
            if (stack.TryPeek(out var rawValue) && rawValue is TOut outValue)
            {
                value = outValue;
                return true;
            }
#pragma warning disable CS8601
            value = default;
#pragma warning restore CS8601
            return false;
        }


        /// <summary>
        /// Try popping value from stack as given type.
        /// </summary>
        /// <param name="stack"><see cref="Stack{T}"/>.</param>
        /// <param name="value">Popped value.</param>
        /// <typeparam name="T">Type of value in stack.</typeparam>
        /// <typeparam name="TOut">Desired type of value.</typeparam>
        /// <returns>True if value popped as given type successfully.</returns>
        public static bool TryPop<T, TOut>(this Stack<T> stack, out TOut value) where TOut : T
        {
            if (!stack.TryPop(out var rawValue))
            {
#pragma warning disable CS8601
                value = default;
#pragma warning restore CS8601
                return false;
            }
            if (rawValue is TOut outValue)
            {
                value = outValue;
                return true;
            }
            stack.Push(rawValue);
#pragma warning disable CS8601
            value = default;
#pragma warning restore CS8601
            return false;
        }
    }
}