using System;
using System.Runtime.CompilerServices;

namespace CarinaStudio
{
    /// <summary>
    /// Extensions for <see cref="Span{T}"/> and <see cref="ReadOnlySpan{T}"/>.
    /// </summary>
    public static unsafe class SpanExtensions
    {
        /// <summary>
        /// Check whether given character sequence is empty or contains whitespaces only or not.
        /// </summary>
        /// <param name="s">Character sequence to check.</param>
        /// <returns>True if the character sequence is empty or contains whitespaces only.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsEmptyOrWhiteSpace(this Span<char> s)
        {
            fixed (char* p = s)
            {
                if (p is null)
                    return true;
                var cPtr = p;
                var count = s.Length;
                while (count > 0)
                {
                    if (!char.IsWhiteSpace(*cPtr))
                        return false;
                    ++cPtr;
                    --count;
                }
                return true;
            }
        }
        
        
        /// <summary>
        /// Check whether given character sequence is empty or contains whitespaces only or not.
        /// </summary>
        /// <param name="s">Character sequence to check.</param>
        /// <returns>True if the character sequence is empty or contains whitespaces only.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsEmptyOrWhiteSpace(this ReadOnlySpan<char> s)
        {
            fixed (char* p = s)
            {
                if (p is null)
                    return true;
                var cPtr = p;
                var count = s.Length;
                while (count > 0)
                {
                    if (!char.IsWhiteSpace(*cPtr))
                        return false;
                    ++cPtr;
                    --count;
                }
                return true;
            }
        }
        
        
        /// <summary>
        /// Check whether given <see cref="Span{T}"/> contains data or not.
        /// </summary>
        /// <param name="span"><see cref="Span{T}"/> to check.</param>
        /// <typeparam name="T">Type of value.</typeparam>
        /// <returns>True if <see cref="Span{T}"/> contains data.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNotEmpty<T>(this Span<T> span) =>
            span.Length > 0;
        
        
        /// <summary>
        /// Check whether given <see cref="ReadOnlySpan{T}"/> contains data or not.
        /// </summary>
        /// <param name="span"><see cref="ReadOnlySpan{T}"/> to check.</param>
        /// <typeparam name="T">Type of value.</typeparam>
        /// <returns>True if <see cref="ReadOnlySpan{T}"/> contains data.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNotEmpty<T>(this ReadOnlySpan<T> span) =>
            span.Length > 0;
        
        
        /// <summary>
        /// Check whether given character sequence contains at least one non-whitespace or not.
        /// </summary>
        /// <param name="s">Character sequence to check.</param>
        /// <returns>True if the character sequence contains contains at least one non-whitespace.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNotWhiteSpace(this Span<char> s)
        {
            fixed (char* p = s)
            {
                if (p is null)
                    return false;
                var cPtr = p;
                var count = s.Length;
                while (count > 0)
                {
                    if (!char.IsWhiteSpace(*cPtr))
                        return true;
                    ++cPtr;
                    --count;
                }
                return false;
            }
        }
        
        
        /// <summary>
        /// Check whether given character sequence contains at least one non-whitespace or not.
        /// </summary>
        /// <param name="s">Character sequence to check.</param>
        /// <returns>True if the character sequence contains contains at least one non-whitespace.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNotWhiteSpace(this ReadOnlySpan<char> s)
        {
            fixed (char* p = s)
            {
                if (p is null)
                    return false;
                var cPtr = p;
                var count = s.Length;
                while (count > 0)
                {
                    if (!char.IsWhiteSpace(*cPtr))
                        return true;
                    ++cPtr;
                    --count;
                }
                return false;
            }
        }
        
        
        /// <summary>
        /// Get address of value referenced by <see cref="Span{T}"/> and perform an action.
        /// </summary>
        /// <param name="span"><see cref="Span{T}"/>.</param>
        /// <param name="action">Action to perform.</param>
        /// <typeparam name="T">Type of value.</typeparam>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Pin<T>(this Span<T> span, Action<IntPtr> action) where T : unmanaged
        {
            fixed (T* ptr = span)
                action((IntPtr)ptr);
        }
        
        
        /// <summary>
        /// Get address of value referenced by <see cref="Span{T}"/> and perform an action.
        /// </summary>
        /// <param name="span"><see cref="Span{T}"/>.</param>
        /// <param name="action">Action to perform.</param>
        /// <typeparam name="T">Type of value.</typeparam>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void PinAs<T>(this Span<T> span, PointerAction<T> action) where T : unmanaged
        {
            fixed (T* ptr = span)
                action(ptr);
        }


        /// <summary>
        /// Get address of value referenced by <see cref="ReadOnlySpan{T}"/> and perform an action.
        /// </summary>
        /// <param name="span"><see cref="ReadOnlySpan{T}"/>.</param>
        /// <param name="action">Action to perform.</param>
        /// <typeparam name="T">Type of value.</typeparam>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Pin<T>(this ReadOnlySpan<T> span, Action<IntPtr> action) where T : unmanaged
        {
            fixed (T* ptr = span)
                action((IntPtr)ptr);
        }
        
        
        /// <summary>
        /// Get address of value referenced by <see cref="ReadOnlySpan{T}"/> and perform an action.
        /// </summary>
        /// <param name="span"><see cref="ReadOnlySpan{T}"/>.</param>
        /// <param name="action">Action to perform.</param>
        /// <typeparam name="T">Type of value.</typeparam>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Pin<T>(this ReadOnlySpan<T> span, PointerAction<T> action) where T : unmanaged
        {
            fixed (T* ptr = span)
                action(ptr);
        }


        /// <summary>
        /// Get address of value referenced by <see cref="Span{T}"/> and perform an action.
        /// </summary>
        /// <param name="span"><see cref="Span{T}"/>.</param>
        /// <param name="action">Action to perform.</param>
        /// <typeparam name="T">Type of value.</typeparam>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Pin<T>(this Span<T> span, Action<IntPtr, int> action) where T : unmanaged
        {
            fixed (T* ptr = span)
                action((IntPtr)ptr, span.Length);
        }


        /// <summary>
        /// Get address of value referenced by <see cref="ReadOnlySpan{T}"/> and perform an action.
        /// </summary>
        /// <param name="span"><see cref="ReadOnlySpan{T}"/>.</param>
        /// <param name="action">Action to perform.</param>
        /// <typeparam name="T">Type of value.</typeparam>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Pin<T>(this ReadOnlySpan<T> span, Action<IntPtr, int> action) where T : unmanaged
        {
            fixed (T* ptr = span)
                action((IntPtr)ptr, span.Length);
        }


        /// <summary>
        /// Get address of value referenced by <see cref="Span{T}"/> and generate value.
        /// </summary>
        /// <param name="span"><see cref="Span{T}"/>.</param>
        /// <param name="func">Function to generate value.</param>
        /// <typeparam name="T">Type of value.</typeparam>
        /// <typeparam name="R">Type of generated value.</typeparam>
        /// <returns>Generated value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static R Pin<T, R>(this Span<T> span, Func<IntPtr, R> func) where T : unmanaged
        {
            fixed (T* ptr = span)
                return func((IntPtr)ptr);
        }
        
        
        /// <summary>
        /// Get address of value referenced by <see cref="Span{T}"/> and generate value.
        /// </summary>
        /// <param name="span"><see cref="Span{T}"/>.</param>
        /// <param name="func">Function to generate value.</param>
        /// <typeparam name="T">Type of value.</typeparam>
        /// <typeparam name="R">Type of generated value.</typeparam>
        /// <returns>Generated value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static R Pin<T, R>(this Span<T> span, PointerInFunc<T, R> func) where T : unmanaged
        {
            fixed (T* ptr = span)
                return func(ptr);
        }


        /// <summary>
        /// Get address of value referenced by <see cref="ReadOnlySpan{T}"/> and generate value.
        /// </summary>
        /// <param name="span"><see cref="ReadOnlySpan{T}"/>.</param>
        /// <param name="func">Function to generate value.</param>
        /// <typeparam name="T">Type of value.</typeparam>
        /// <typeparam name="R">Type of generated value.</typeparam>
        /// <returns>Generated value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static R Pin<T, R>(this ReadOnlySpan<T> span, Func<IntPtr, R> func) where T : unmanaged
        {
            fixed (T* ptr = span)
                return func((IntPtr)ptr);
        }
        
        
        /// <summary>
        /// Get address of value referenced by <see cref="ReadOnlySpan{T}"/> and generate value.
        /// </summary>
        /// <param name="span"><see cref="ReadOnlySpan{T}"/>.</param>
        /// <param name="func">Function to generate value.</param>
        /// <typeparam name="T">Type of value.</typeparam>
        /// <typeparam name="R">Type of generated value.</typeparam>
        /// <returns>Generated value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static R Pin<T, R>(this ReadOnlySpan<T> span, PointerInFunc<T, R> func) where T : unmanaged
        {
            fixed (T* ptr = span)
                return func(ptr);
        }


        /// <summary>
        /// Get address of value referenced by <see cref="Span{T}"/> and generate value.
        /// </summary>
        /// <param name="span"><see cref="Span{T}"/>.</param>
        /// <param name="func">Function to generate value.</param>
        /// <typeparam name="T">Type of value.</typeparam>
        /// <typeparam name="R">Type of generated value.</typeparam>
        /// <returns>Generated value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static R Pin<T, R>(this Span<T> span, Func<IntPtr, int, R> func) where T : unmanaged
        {
            fixed (T* ptr = span)
                return func((IntPtr)ptr, span.Length);
        }


        /// <summary>
        /// Get address of value referenced by <see cref="ReadOnlySpan{T}"/> and generate value.
        /// </summary>
        /// <param name="span"><see cref="ReadOnlySpan{T}"/>.</param>
        /// <param name="func">Function to generate value.</param>
        /// <typeparam name="T">Type of value.</typeparam>
        /// <typeparam name="R">Type of generated value.</typeparam>
        /// <returns>Generated value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static R Pin<T, R>(this ReadOnlySpan<T> span, Func<IntPtr, int, R> func) where T : unmanaged
        {
            fixed (T* ptr = span)
                return func((IntPtr)ptr, span.Length);
        }
    }
}