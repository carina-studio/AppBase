using System;
using System.Buffers;
using System.Runtime.CompilerServices;

namespace CarinaStudio.Buffers
{
    /// <summary>
    /// Extensions for <see cref="IPinnable"/>.
    /// </summary>
    public static class PinnableExtensions
    {
		/// <summary>
		/// Pin given <see cref="IPinnable"/>, get address of memory and perform action.
		/// </summary>
		/// <param name="pinnable"><see cref="IPinnable"/>.</param>
		/// <param name="action">Method to receive address of memory and perform action.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Pin(this IPinnable pinnable, Action<IntPtr> action) =>
			Pin(pinnable, action);


		/// <summary>
		/// Pin given <see cref="IPinnable"/>, get address of memory and perform action.
		/// </summary>
		/// <param name="pinnable"><see cref="IPinnable"/>.</param>
		/// <param name="elementIndex">Index of first element to pin.</param>
		/// <param name="action">Method to receive address of memory and perform action.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Pin(this IPinnable pinnable, int elementIndex, Action<IntPtr> action)
		{
			using var handle = pinnable.Pin(elementIndex);
			unsafe
			{
				action(new IntPtr(handle.Pointer));
			}
		}


		/// <summary>
		/// Pin given <see cref="IPinnable"/>, get address of memory and generate a value.
		/// </summary>
		/// <param name="pinnable"><see cref="IPinnable"/>.</param>
		/// <param name="func">Function to receive address of memory and generate value.</param>
		/// <returns>Generated value.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static R Pin<R>(this IPinnable pinnable, Func<IntPtr, R> func) =>
			Pin(pinnable, 0, func);


		/// <summary>
		/// Pin given <see cref="IPinnable"/>, get address of memory and generate a value.
		/// </summary>
		/// <param name="pinnable"><see cref="IPinnable"/>.</param>
		/// <param name="elementIndex">Index of first element to pin.</param>
		/// <param name="func">Function to receive address of memory and generate value.</param>
		/// <returns>Generated value.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static R Pin<R>(this IPinnable pinnable, int elementIndex, Func<IntPtr, R> func)
		{
			using var handle = pinnable.Pin(elementIndex);
			unsafe
			{
				return func(new IntPtr(handle.Pointer));
			}
		}
	}
}
