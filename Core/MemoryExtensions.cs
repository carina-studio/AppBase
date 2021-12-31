using System;
using System.Runtime.CompilerServices;

namespace CarinaStudio
{
	/// <summary>
	/// Extensions for <see cref="Memory{T}"/>.
	/// </summary>
	public static class MemoryExtensions
	{
		/// <summary>
		/// Pin given <see cref="Memory{T}"/>, get address of memory and perform action.
		/// </summary>
		/// <typeparam name="T">Type of memory element.</typeparam>
		/// <param name="memory"><see cref="Memory{T}"/>.</param>
		/// <param name="action">Method to receive address of memory and perform action.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Pin<T>(this Memory<T> memory, Action<IntPtr> action)
		{
			using var handle = memory.Pin();
			unsafe
			{
				action(new IntPtr(handle.Pointer));
			}
		}


		/// <summary>
		/// Pin given list of <see cref="Memory{T}"/>, get addresses of memory and perform action.
		/// </summary>
		/// <typeparam name="T1">Type of 1st memory element.</typeparam>
		/// <typeparam name="T2">Type of 2nd memory element.</typeparam>
		/// <param name="memoryList">List of <see cref="Memory{T}"/>.</param>
		/// <param name="action">Method to receive addresses of memory and perform action.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Pin<T1, T2>(this (Memory<T1>, Memory<T2>) memoryList, Action<IntPtr, IntPtr> action)
		{
			using var handle1 = memoryList.Item1.Pin();
			using var handle2 = memoryList.Item2.Pin();
			unsafe
			{
				action(new IntPtr(handle1.Pointer), new IntPtr(handle2.Pointer));
			}
		}


		/// <summary>
		/// Pin given <see cref="Memory{T}"/>, get address of memory and generate a value.
		/// </summary>
		/// <typeparam name="T">Type of memory element.</typeparam>
		/// <typeparam name="R">Type of generated value.</typeparam>
		/// <param name="memory"><see cref="Memory{T}"/>.</param>
		/// <param name="func">Function to receive address of memory and generate value.</param>
		/// <returns>Generated value.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static R Pin<T, R>(this Memory<T> memory, Func<IntPtr, R> func)
		{
			using var handle = memory.Pin();
			unsafe
			{
				return func(new IntPtr(handle.Pointer));
			}
		}


		/// <summary>
		/// Pin given list of <see cref="Memory{T}"/>, get addresses of memory and generate a value.
		/// </summary>
		/// <typeparam name="T1">Type of 1st memory element.</typeparam>
		/// <typeparam name="T2">Type of 2nd memory element.</typeparam>
		/// <typeparam name="R">Type of generated value.</typeparam>
		/// <param name="memoryList">List of <see cref="Memory{T}"/>.</param>
		/// <param name="func">Function to receive addresses of memory and generate value.</param>
		/// <returns>Generated value.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static R Pin<T1, T2, R>(this (Memory<T1>, Memory<T2>) memoryList, Func<IntPtr, IntPtr, R> func)
		{
			using var handle1 = memoryList.Item1.Pin();
			using var handle2 = memoryList.Item2.Pin();
			unsafe
			{
				return func(new IntPtr(handle1.Pointer), new IntPtr(handle2.Pointer));
			}
		}


		/// <summary>
		/// Pin given <see cref="ReadOnlyMemory{T}"/>, get address of memory and perform action.
		/// </summary>
		/// <typeparam name="T">Type of memory element.</typeparam>
		/// <param name="memory"><see cref="ReadOnlyMemory{T}"/>.</param>
		/// <param name="action">Method to receive address of memory and perform action.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Pin<T>(this ReadOnlyMemory<T> memory, Action<IntPtr> action)
		{
			using var handle = memory.Pin();
			unsafe
			{
				action(new IntPtr(handle.Pointer));
			}
		}


		/// <summary>
		/// Pin given list of <see cref="ReadOnlyMemory{T}"/>, get addresses of memory and perform action.
		/// </summary>
		/// <typeparam name="T1">Type of 1st memory element.</typeparam>
		/// <typeparam name="T2">Type of 2nd memory element.</typeparam>
		/// <param name="memoryList">List of <see cref="ReadOnlyMemory{T}"/>.</param>
		/// <param name="action">Method to receive addresses of memory and perform action.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Pin<T1, T2>(this (ReadOnlyMemory<T1>, ReadOnlyMemory<T2>) memoryList, Action<IntPtr, IntPtr> action)
		{
			using var handle1 = memoryList.Item1.Pin();
			using var handle2 = memoryList.Item2.Pin();
			unsafe
			{
				action(new IntPtr(handle1.Pointer), new IntPtr(handle2.Pointer));
			}
		}


		/// <summary>
		/// Pin given <see cref="ReadOnlyMemory{T}"/>, get address of memory and generate a value.
		/// </summary>
		/// <typeparam name="T">Type of memory element.</typeparam>
		/// <typeparam name="R">Type of generated value.</typeparam>
		/// <param name="memory"><see cref="ReadOnlyMemory{T}"/>.</param>
		/// <param name="func">Function to receive address of memory and generate value.</param>
		/// <returns>Generated value.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static R Pin<T, R>(this ReadOnlyMemory<T> memory, Func<IntPtr, R> func)
		{
			using var handle = memory.Pin();
			unsafe
			{
				return func(new IntPtr(handle.Pointer));
			}
		}


		/// <summary>
		/// Pin given list of <see cref="ReadOnlyMemory{T}"/>, get addresses of memory and generate a value.
		/// </summary>
		/// <typeparam name="T1">Type of 1st memory element.</typeparam>
		/// <typeparam name="T2">Type of 2nd memory element.</typeparam>
		/// <typeparam name="R">Type of generated value.</typeparam>
		/// <param name="memoryList">List of <see cref="ReadOnlyMemory{T}"/>.</param>
		/// <param name="func">Function to receive addresses of memory and generate value.</param>
		/// <returns>Generated value.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static R Pin<T1, T2, R>(this (ReadOnlyMemory<T1>, ReadOnlyMemory<T2>) memoryList, Func<IntPtr, IntPtr, R> func)
		{
			using var handle1 = memoryList.Item1.Pin();
			using var handle2 = memoryList.Item2.Pin();
			unsafe
			{
				return func(new IntPtr(handle1.Pointer), new IntPtr(handle2.Pointer));
			}
		}
	}
}
