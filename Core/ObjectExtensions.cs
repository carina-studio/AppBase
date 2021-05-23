using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace CarinaStudio
{
	/// <summary>
	/// Extensions for all types.
	/// </summary>
	public static class ObjectExtensions
	{
		/// <summary>
		/// Perform action on the given value, and return it.
		/// </summary>
		/// <typeparam name="T">Type of value.</typeparam>
		/// <param name="value">Given value.</param>
		/// <param name="action">Action to perform on <paramref name="value"/>.</param>
		/// <returns>Value which is same as <paramref name="value"/>.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T Also<T>(this T value, Action<T> action)
		{
			action(value);
			return value;
		}


		/// <summary>
		/// Treat given nullable value as non-null value, or throw <see cref="NullReferenceException"/>.
		/// </summary>
		/// <typeparam name="T">Type of value.</typeparam>
		/// <param name="obj">Given nullable value.</param>
		/// <returns>Non-null value.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T AsNonNull<T>(this T? obj) where T : class => obj ?? throw new NullReferenceException();


		/// <summary>
		/// Perform action on the given value.
		/// </summary>
		/// <typeparam name="T">Type of given value.</typeparam>
		/// <param name="value">Given value.</param>
		/// <param name="action">Action to perform on <paramref name="value"/>.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Let<T>(this T value, Action<T> action) => action(value);


		/// <summary>
		/// Perform action on the given value, and return a custom value.
		/// </summary>
		/// <typeparam name="T">Type of given value.</typeparam>
		/// <typeparam name="R">Type of return value.</typeparam>
		/// <param name="value">Given value.</param>
		/// <param name="action">Action to perform on <paramref name="value"/>.</param>
		/// <returns>Custom return value.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static R Let<T, R>(this T value, Func<T, R> action) => action(value);


		/// <summary>
		/// Acquire lock on given object and generate value after releasing lock.
		/// </summary>
		/// <typeparam name="T">Type of given object.</typeparam>
		/// <typeparam name="R">Type of generated value.</typeparam>
		/// <param name="obj">Object to acquire lock on.</param>
		/// <param name="func">Function to generate value.</param>
		/// <returns>Generated value.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static R Lock<T, R>(this T obj, Func<R> func) where T : class
		{
			Monitor.Enter(obj);
			try
			{
				return func();
			}
			finally
			{
				Monitor.Exit(obj);
			}
		}


		/// <summary>
		/// Acquire lock on given object and generate value after releasing lock.
		/// </summary>
		/// <typeparam name="T">Type of given object.</typeparam>
		/// <typeparam name="R">Type of generated value.</typeparam>
		/// <param name="obj">Object to acquire lock on.</param>
		/// <param name="func">Function to generate value.</param>
		/// <returns>Generated value.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static R Lock<T, R>(this T obj, Func<T, R> func) where T : class
		{
			Monitor.Enter(obj);
			try
			{
				return func(obj);
			}
			finally
			{
				Monitor.Exit(obj);
			}
		}
	}
}
