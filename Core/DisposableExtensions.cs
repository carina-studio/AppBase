﻿using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace CarinaStudio
{
	/// <summary>
	/// Extensions for <see cref="IDisposable"/>.
	/// </summary>
	public static class DisposableExtensions
	{
		/// <summary>
		/// Call <see cref="IDisposable.Dispose"/> and return null.
		/// </summary>
		/// <typeparam name="T">Type which implements <see cref="IDisposable"/>.</typeparam>
		/// <param name="disposable"><see cref="IDisposable.Dispose"/>.</param>
		/// <returns>Null.</returns>
		public static T? DisposeAndReturnNull<T>(this T? disposable) where T : class, IDisposable
		{
			disposable?.Dispose();
			return null;
		}


#pragma warning disable CS8600
		/// <summary>
		/// Exhange the source <see cref="IDisposable"/> with another one.
		/// </summary>
		/// <typeparam name="T">Type of source <see cref="IDisposable"/>.</typeparam>
		/// <typeparam name="R">Type of result <see cref="IDisposable"/>.</typeparam>
		/// <param name="source">Source <see cref="IDisposable"/>.</param>
		/// <param name="func">Exchanging function.</param>
		/// <returns>Exchanged <see cref="IDisposable"/>.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static R? Exchange<T, R>(this T source, Func<R?> func) where T : class, IDisposable where R : class, IDisposable
		{
			R? result = default;
			try
			{
				result = func();
			}
			finally
			{
				if (!object.ReferenceEquals(source, result))
					source?.Dispose();
			}
			return result;
		}


		/// <summary>
		/// Exhange the source <see cref="IDisposable"/> with another one.
		/// </summary>
		/// <typeparam name="T">Type of source <see cref="IDisposable"/>.</typeparam>
		/// <typeparam name="R">Type of result <see cref="IDisposable"/>.</typeparam>
		/// <param name="source">Source <see cref="IDisposable"/>.</param>
		/// <param name="func">Exchanging function.</param>
		/// <returns>Exchanged <see cref="IDisposable"/>.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static R? Exchange<T, R>(this T source, Func<T, R?> func) where T : class, IDisposable where R : class, IDisposable
		{
			R? result = default;
			try
			{
				result = func(source);
			}
			finally
			{
				if (!object.ReferenceEquals(source, result))
					source?.Dispose();
			}
			return result;
		}
#pragma warning restore CS8600


		/// <summary>
		/// Exhange the source <see cref="IDisposable"/> with another one asynchronously.
		/// </summary>
		/// <typeparam name="T">Type of source <see cref="IDisposable"/>.</typeparam>
		/// <typeparam name="R">Type of result <see cref="IDisposable"/>.</typeparam>
		/// <param name="source">Source <see cref="IDisposable"/>.</param>
		/// <param name="func">Exchanging function.</param>
		/// <returns>Task of exchanging <see cref="IDisposable"/>.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static async Task<R?> ExchangeAsync<T, R>(this T source, Func<Task<R?>> func) where T : class, IDisposable where R : class, IDisposable
		{
			R? result = default;
			try
			{
				result = await func();
			}
			finally
			{
				if (!object.ReferenceEquals(source, result))
				{
					if (source is IAsyncDisposable asyncDisposable)
						await asyncDisposable.DisposeAsync();
					else
						source?.Dispose();
				}
			}
			return result;
		}


		/// <summary>
		/// Exhange the source <see cref="IDisposable"/> with another one asynchronously.
		/// </summary>
		/// <typeparam name="T">Type of source <see cref="IDisposable"/>.</typeparam>
		/// <typeparam name="R">Type of result <see cref="IDisposable"/>.</typeparam>
		/// <param name="source">Source <see cref="IDisposable"/>.</param>
		/// <param name="func">Exchanging function.</param>
		/// <returns>Task of exchanging <see cref="IDisposable"/>.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static async Task<R?> ExchangeAsync<T, R>(this T source, Func<T, Task<R?>> func) where T : class, IDisposable where R : class, IDisposable
		{
			R? result = default;
			try
			{
				result = await func(source);
			}
			finally
			{
				if (!object.ReferenceEquals(source, result))
				{
					if (source is IAsyncDisposable asyncDisposable)
						await asyncDisposable.DisposeAsync();
					else
						source?.Dispose();
				}
			}
			return result;
		}


		/// <summary>
		/// Use the given <see cref="IDisposable"/> to perform action then dispose it before returning from method.
		/// </summary>
		/// <typeparam name="T">Type which implements <see cref="IDisposable"/>.</typeparam>
		/// <param name="disposable"><see cref="IDisposable"/>.</param>
		/// <param name="action">Action to perform.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Use<T>(this T disposable, Action<T> action) where T : IDisposable
		{
			try
			{
				action(disposable);
			}
			finally
			{
				disposable.Dispose();
			}
		}


		/// <summary>
		/// Use the given <see cref="IDisposable"/> to generate value then dispose it before returning from method.
		/// </summary>
		/// <typeparam name="T">Type which implements <see cref="IDisposable"/>.</typeparam>
		/// <typeparam name="R">Type of generated value.</typeparam>
		/// <param name="disposable"><see cref="IDisposable"/>.</param>
		/// <param name="func">Using function.</param>
		/// <returns>Generated value.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static R Use<T, R>(this T disposable, Func<T, R> func) where T : IDisposable
		{
			try
			{
				return func(disposable);
			}
			finally
			{
				disposable.Dispose();
			}
		}


		/// <summary>
		/// Use the given <see cref="IDisposable"/> to generate reference to value then dispose it before returning from method.
		/// </summary>
		/// <typeparam name="T">Type which implements <see cref="IDisposable"/>.</typeparam>
		/// <typeparam name="R">Type of generated reference to value.</typeparam>
		/// <param name="disposable"><see cref="IDisposable"/>.</param>
		/// <param name="func">Using function.</param>
		/// <returns>Generated reference to value.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref R Use<T, R>(this T disposable, RefOutFunc<T, R> func) where T : IDisposable
		{
			try
			{
				return ref func(disposable);
			}
			finally
			{
				disposable.Dispose();
			}
		}


		/// <summary>
		/// Use the given <see cref="IDisposable"/> to perform action asynchronously then dispose it before returning from method.
		/// </summary>
		/// <typeparam name="T">Type which implements <see cref="IDisposable"/>.</typeparam>
		/// <param name="disposable"><see cref="IDisposable"/>.</param>
		/// <param name="action">Action to perform.</param>
		/// <returns>Task of asynchronous action.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static async Task UseAsync<T>(this T disposable, Func<T, Task> action) where T : IDisposable
		{
			try
			{
				await action(disposable);
			}
			finally
			{
				if (disposable is IAsyncDisposable asyncDisposable)
					await asyncDisposable.DisposeAsync();
				else
					disposable.Dispose();
			}
		}


		/// <summary>
		/// Use the given <see cref="IDisposable"/> to generate value asynchronously then dispose it before returning from method.
		/// </summary>
		/// <typeparam name="T">Type which implements <see cref="IDisposable"/>.</typeparam>
		/// <typeparam name="R">Type of generated value.</typeparam>
		/// <param name="disposable"><see cref="IDisposable"/>.</param>
		/// <param name="func">Using function.</param>
		/// <returns>Task of generating value.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static async Task<R> UseAsync<T, R>(this T disposable, Func<T, Task<R>> func) where T : IDisposable
		{
			try
			{
				return await func(disposable);
			}
			finally
			{
				if (disposable is IAsyncDisposable asyncDisposable)
					await asyncDisposable.DisposeAsync();
				else
					disposable.Dispose();
			}
		}
	}
}
