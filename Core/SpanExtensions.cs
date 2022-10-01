using System;
using  System.Runtime.CompilerServices;

namespace CarinaStudio
{
    /// <summary>
    /// Extensions for <see cref="Span{T}"/> and <see cref="ReadOnlySpan{T}"/>.
    /// </summary>
    public static unsafe class SpanExtensions
    {
        /// <summary>
        /// Get address of value referenced by <see cref="Span{T}"/> and perform an action.
        /// </summary>
        /// <param name="span"><see cref="Span{T}"/>.</param>
        /// <param name="action">Action to perform.</param>
        /// <typeparam name="T">Type of value.</typeparam>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Pin<T>(this Span<T> span, Action<IntPtr> action) where T : unmanaged
        {
            ref readonly T valueRef = ref span.GetPinnableReference();
            fixed (T* ptr = &valueRef)
                action((IntPtr)ptr);
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
            ref readonly T valueRef = ref span.GetPinnableReference();
            fixed (T* ptr = &valueRef)
                action((IntPtr)ptr);
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
            ref readonly T valueRef = ref span.GetPinnableReference();
            fixed (T* ptr = &valueRef)
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
            ref readonly T valueRef = ref span.GetPinnableReference();
            fixed (T* ptr = &valueRef)
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
            ref readonly T valueRef = ref span.GetPinnableReference();
            fixed (T* ptr = &valueRef)
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
        public static R Pin<T, R>(this ReadOnlySpan<T> span, Func<IntPtr, R> func) where T : unmanaged
        {
            ref readonly T valueRef = ref span.GetPinnableReference();
            fixed (T* ptr = &valueRef)
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
        public static R Pin<T, R>(this Span<T> span, Func<IntPtr, int, R> func) where T : unmanaged
        {
            ref readonly T valueRef = ref span.GetPinnableReference();
            fixed (T* ptr = &valueRef)
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
            ref readonly T valueRef = ref span.GetPinnableReference();
            fixed (T* ptr = &valueRef)
                return func((IntPtr)ptr, span.Length);
        }
    }
}