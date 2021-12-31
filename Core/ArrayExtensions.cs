using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace CarinaStudio
{
	/// <summary>
	/// Extensions for array.
	/// </summary>
	public static class ArrayExtensions
	{
		/// <summary>
		/// Pin given array, get memory address of array and perform action.
		/// </summary>
		/// <typeparam name="T">Type of array element.</typeparam>
		/// <param name="array">Array.</param>
		/// <param name="action">Method to receive address of array and perform action.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Pin<T>(this T[] array, Action<IntPtr> action)
		{
			var gcHandle = GCHandle.Alloc(array, GCHandleType.Pinned);
			try
			{
				action(gcHandle.AddrOfPinnedObject());
			}
			finally
			{
				gcHandle.Free();
			}
		}


		/// <summary>
		/// Pin given arrays, get memory addresses of arrays and perform action.
		/// </summary>
		/// <typeparam name="T1">Type of 1st array element.</typeparam>
		/// <typeparam name="T2">Type of 2nd array element.</typeparam>
		/// <param name="arrays">Arrays.</param>
		/// <param name="action">Method to receive addresses of arrays and perform action.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Pin<T1, T2>(this (T1[], T2[]) arrays, Action<IntPtr, IntPtr> action)
		{
			var gcHandle2 = new GCHandle();
			var gcHandle1 = GCHandle.Alloc(arrays.Item1, GCHandleType.Pinned);
			try
			{
				gcHandle2 = GCHandle.Alloc(arrays.Item2, GCHandleType.Pinned);
				action(gcHandle1.AddrOfPinnedObject(), gcHandle2.AddrOfPinnedObject());
			}
			finally
			{
				gcHandle1.Free();
				gcHandle2.Free();
			}
		}


		/// <summary>
		/// Pin given array, get memory address of array and generate a value.
		/// </summary>
		/// <typeparam name="T">Type of array element.</typeparam>
		/// <typeparam name="R">Type of generated value.</typeparam>
		/// <param name="array">Array.</param>
		/// <param name="func">Function to receive address of array and generate value.</param>
		/// <returns>Generated value.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static R Pin<T, R>(this T[] array, Func<IntPtr, R> func)
		{
			var gcHandle = GCHandle.Alloc(array, GCHandleType.Pinned);
			try
			{
				return func(gcHandle.AddrOfPinnedObject());
			}
			finally
			{
				gcHandle.Free();
			}
		}


		/// <summary>
		/// Pin given arrays, get memory addresses of arrays and generate a value.
		/// </summary>
		/// <typeparam name="T1">Type of 1st array element.</typeparam>
		/// <typeparam name="T2">Type of 2nd array element.</typeparam>
		/// <typeparam name="R">Type of generated value.</typeparam>
		/// <param name="arrays">Arrays.</param>
		/// <param name="func">Function to receive addresses of arrays and generate value.</param>
		/// <returns>Generated value.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static R Pin<T1, T2, R>(this (T1[], T2[]) arrays, Func<IntPtr, IntPtr, R> func)
		{
			var gcHandle2 = new GCHandle();
			var gcHandle1 = GCHandle.Alloc(arrays.Item1, GCHandleType.Pinned);
			try
			{
				gcHandle2 = GCHandle.Alloc(arrays.Item2, GCHandleType.Pinned);
				return func(gcHandle1.AddrOfPinnedObject(), gcHandle2.AddrOfPinnedObject());
			}
			finally
			{
				gcHandle1.Free();
				gcHandle2.Free();
			}
		}
	}
}
