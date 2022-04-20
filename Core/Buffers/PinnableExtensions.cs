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

		
		/// <summary>
		/// Pin given <see cref="IPinnable"/>, get address of memory and perform action.
		/// </summary>
		/// <typeparam name="TPtr">Type of pointer of pinned memory element.</typeparam>
		/// <param name="pinnable"><see cref="IPinnable"/>.</param>
		/// <param name="action">Method to receive address of memory and perform action.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static unsafe void PinAs<TPtr>(this IPinnable pinnable, PointerAction<TPtr> action) where TPtr : unmanaged =>
			PinAs(pinnable, 0, action);


		/// <summary>
		/// Pin given <see cref="IPinnable"/>s, get addresses of memory and perform action.
		/// </summary>
		/// <typeparam name="TPtr1">Type of 1st pointer of pinned memory element.</typeparam>
		/// <typeparam name="TPtr2">Type of 2nd pointer of pinned memory element.</typeparam>
		/// <param name="pinnables"><see cref="IPinnable"/>s.</param>
		/// <param name="action">Method to receive addresses of memory and perform action.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static unsafe void PinAs<TPtr1, TPtr2>(this (IPinnable, IPinnable) pinnables, PointerAction<TPtr1, TPtr2> action) where TPtr1 : unmanaged where TPtr2 : unmanaged =>
			PinAs(pinnables, 0, 0, action);


		/// <summary>
		/// Pin given <see cref="IPinnable"/>, get address of memory and perform action.
		/// </summary>
		/// <typeparam name="TPtr">Type of pointer of pinned memory element.</typeparam>
		/// <param name="pinnable"><see cref="IPinnable"/>.</param>
		/// <param name="elementIndex">Index of first element to pin.</param>
		/// <param name="action">Method to receive address of memory and perform action.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static unsafe void PinAs<TPtr>(this IPinnable pinnable, int elementIndex, PointerAction<TPtr> action) where TPtr : unmanaged
		{
			using var handle = pinnable.Pin(elementIndex);
			action((TPtr*)handle.Pointer);
		}


		/// <summary>
		/// Pin given <see cref="IPinnable"/>s, get addresses of memory and perform action.
		/// </summary>
		/// <typeparam name="TPtr1">Type of 1st pointer of pinned memory element.</typeparam>
		/// <typeparam name="TPtr2">Type of 2nd pointer of pinned memory element.</typeparam>
		/// <param name="pinnables"><see cref="IPinnable"/>s.</param>
		/// <param name="elementIndex1">Index of first element of 1st <see cref="IPinnable"/> to pin.</param>
		/// <param name="elementIndex2">Index of first element of 2nd <see cref="IPinnable"/> to pin.</param>
		/// <param name="action">Method to receive addresses of memory and perform action.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static unsafe void PinAs<TPtr1, TPtr2>(this (IPinnable, IPinnable) pinnables, int elementIndex1, int elementIndex2, PointerAction<TPtr1, TPtr2> action) where TPtr1 : unmanaged where TPtr2 : unmanaged
		{
			using var handle1 = pinnables.Item1.Pin(elementIndex1);
			using var handle2 = pinnables.Item2.Pin(elementIndex2);
			action((TPtr1*)handle1.Pointer, (TPtr2*)handle2.Pointer);
		}


		/// <summary>
		/// Pin given <see cref="IPinnable"/>, get address of memory and generate a value.
		/// </summary>
		/// <typeparam name="TPtr">Type of pointer of pinned memory element.</typeparam>
		/// <typeparam name="R">Type of generated value.</typeparam>
		/// <param name="pinnable"><see cref="IPinnable"/>.</param>
		/// <param name="func">Function to receive address of memory and generate value.</param>
		/// <returns>Generated value.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static unsafe R PinAs<TPtr, R>(this IPinnable pinnable, PointerInFunc<TPtr, R> func) where TPtr : unmanaged =>
			PinAs(pinnable, 0, func);


		/// <summary>
		/// Pin given <see cref="IPinnable"/>s, get addresses of memory and generate a value.
		/// </summary>
		/// <typeparam name="TPtr1">Type of 1st pointer of pinned memory element.</typeparam>
		/// <typeparam name="TPtr2">Type of 2nd pointer of pinned memory element.</typeparam>
		/// <typeparam name="R">Type of generated value.</typeparam>
		/// <param name="pinnables"><see cref="IPinnable"/>s.</param>
		/// <param name="func">Function to receive addresses of memory and generate value.</param>
		/// <returns>Generated value.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static unsafe R PinAs<TPtr1, TPtr2, R>(this (IPinnable, IPinnable) pinnables, PointerInFunc<TPtr1, TPtr2, R> func) where TPtr1 : unmanaged where TPtr2 : unmanaged =>
			PinAs(pinnables, 0, 0, func);


		/// <summary>
		/// Pin given <see cref="IPinnable"/>, get address of memory and generate a value.
		/// </summary>
		/// <typeparam name="TPtr">Type of pointer of pinned memory element.</typeparam>
		/// <typeparam name="R">Type of generated value.</typeparam>
		/// <param name="pinnable"><see cref="IPinnable"/>.</param>
		/// <param name="elementIndex">Index of first element to pin.</param>
		/// <param name="func">Function to receive address of memory and generate value.</param>
		/// <returns>Generated value.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static unsafe R PinAs<TPtr, R>(this IPinnable pinnable, int elementIndex, PointerInFunc<TPtr, R> func) where TPtr : unmanaged
		{
			using var handle = pinnable.Pin(elementIndex);
			return func((TPtr*)handle.Pointer);
		}


		/// <summary>
		/// Pin given <see cref="IPinnable"/>s, get addresses of memory and generate a value.
		/// </summary>
		/// <typeparam name="TPtr1">Type of 1st pointer of pinned memory element.</typeparam>
		/// <typeparam name="TPtr2">Type of 2nd pointer of pinned memory element.</typeparam>
		/// <typeparam name="R">Type of generated value.</typeparam>
		/// <param name="pinnables"><see cref="IPinnable"/>s.</param>
		/// <param name="elementIndex1">Index of first element of 1st <see cref="IPinnable"/> to pin.</param>
		/// <param name="elementIndex2">Index of first element of 2nd <see cref="IPinnable"/> to pin.</param>
		/// <param name="func">Function to receive addresses of memory and generate value.</param>
		/// <returns>Generated value.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static unsafe R PinAs<TPtr1, TPtr2, R>(this (IPinnable, IPinnable) pinnables, int elementIndex1, int elementIndex2, PointerInFunc<TPtr1, TPtr2, R> func) where TPtr1 : unmanaged where TPtr2 : unmanaged
		{
			using var handle1 = pinnables.Item1.Pin(elementIndex1);
			using var handle2 = pinnables.Item2.Pin(elementIndex2);
			return func((TPtr1*)handle1.Pointer, (TPtr2*)handle2.Pointer);
		}
	}
}
