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
			Pin(pinnable, 0, action);


		/// <summary>
		/// Pin given <see cref="IPinnable"/>s, get addresses of memory and perform action.
		/// </summary>
		/// <param name="pinnables"><see cref="IPinnable"/>s.</param>
		/// <param name="action">Method to receive addresses of memory and perform action.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Pin(this (IPinnable, IPinnable) pinnables, Action<IntPtr, IntPtr> action) =>
			Pin(pinnables, 0, 0, action);


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
		/// Pin given <see cref="IPinnable"/>s, get addresses of memory and perform action.
		/// </summary>
		/// <param name="pinnables"><see cref="IPinnable"/>s.</param>
		/// <param name="elementIndex1">Index of first element of 1st <see cref="IPinnable"/> to pin.</param>
		/// <param name="elementIndex2">Index of first element of 2nd <see cref="IPinnable"/> to pin.</param>
		/// <param name="action">Method to receive addresses of memory and perform action.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Pin(this (IPinnable, IPinnable) pinnables, int elementIndex1, int elementIndex2, Action<IntPtr, IntPtr> action)
		{
			using var handle1 = pinnables.Item1.Pin(elementIndex1);
			using var handle2 = pinnables.Item2.Pin(elementIndex2);
			unsafe
			{
				action(new IntPtr(handle1.Pointer), new IntPtr(handle2.Pointer));
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
		/// Pin given <see cref="IPinnable"/>s, get addresses of memory and generate a value.
		/// </summary>
		/// <param name="pinnables"><see cref="IPinnable"/>s.</param>
		/// <param name="func">Function to receive addresses of memory and generate value.</param>
		/// <returns>Generated value.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static R Pin<R>(this (IPinnable, IPinnable) pinnables, Func<IntPtr, IntPtr, R> func) =>
			Pin(pinnables, 0, 0, func);


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


		/// <summary>
		/// Pin given <see cref="IPinnable"/>s, get addresses of memory and generate a value.
		/// </summary>
		/// <param name="pinnables"><see cref="IPinnable"/>s.</param>
		/// <param name="elementIndex1">Index of first element of 1st <see cref="IPinnable"/> to pin.</param>
		/// <param name="elementIndex2">Index of first element of 2nd <see cref="IPinnable"/> to pin.</param>
		/// <param name="func">Function to receive addresses of memory and generate value.</param>
		/// <returns>Generated value.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static R Pin<R>(this (IPinnable, IPinnable) pinnables, int elementIndex1, int elementIndex2, Func<IntPtr, IntPtr, R> func)
		{
			using var handle1 = pinnables.Item1.Pin(elementIndex1);
			using var handle2 = pinnables.Item2.Pin(elementIndex2);
			unsafe
			{
				return func(new IntPtr(handle1.Pointer), new IntPtr(handle2.Pointer));
			}
		}
	}
}
