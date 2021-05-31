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
	}
}
