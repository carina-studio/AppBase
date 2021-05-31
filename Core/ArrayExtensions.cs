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
	}
}
