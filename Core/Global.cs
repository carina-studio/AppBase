using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace CarinaStudio
{
	/// <summary>
	/// Global utility methods.
	/// </summary>
	public static class Global
	{
		/// <summary>
		/// Result of <see cref="RunCatching{T}(Func{T})"/>.
		/// </summary>
		/// <typeparam name="T">Type of generated value.</typeparam>
		public ref struct RunCatchingResult<T>
		{
			// Constructor.
			internal RunCatchingResult(bool isSuccessful, T result, Exception? ex)
			{
				this.IsSuccessful = isSuccessful;
				this.Exception = ex;
				this.Result = result;
			}

			/// <summary>
			/// Get <see cref="Exception"/> occurred while generating value.
			/// </summary>
			public Exception? Exception { get; }

			/// <summary>
			/// Check whether value generating is successful or not.
			/// </summary>
			public bool IsSuccessful { get; }

			/// <summary>
			/// Get generated value, or default value if failed to generate.
			/// </summary>
			public T Result { get; }
		}


		/// <summary>
		/// Generate a value.
		/// </summary>
		/// <typeparam name="T">Type of generated value.</typeparam>
		/// <param name="func">Function to generate value.</param>
		/// <returns>Generated value.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T Run<T>(Func<T> func) => func();


		/// <summary>
		/// Generate a reference to value.
		/// </summary>
		/// <typeparam name="T">Type of generated value.</typeparam>
		/// <param name="func">Function to generate reference to value.</param>
		/// <returns>Generated reference to value.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref T Run<T>(RefFunc<T> func) => ref func();


#pragma warning disable CS8604
		/// <summary>
		/// Generate value and catch exception if occurred.
		/// </summary>
		/// <typeparam name="T">Type of generated value.</typeparam>
		/// <param name="func">Function to generate value.</param>
		/// <returns><see cref="RunCatchingResult{T}"/> contains the result and exception occurred.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static RunCatchingResult<T> RunCatching<T>(Func<T> func)
		{
			try
			{
				return new RunCatchingResult<T>(true, func(), null);
			}
			catch (Exception ex)
			{
				return new RunCatchingResult<T>(false, default, ex);
			}
		}
#pragma warning restore CS8604


		/// <summary>
		/// Run given action and ignore any error occurred.
		/// </summary>
		/// <param name="action">Action to run.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void RunWithoutError(Action action)
		{
			try
			{
				action();
			}
			catch
			{ }
		}


		/// <summary>
		/// Run given action and ignore any error occurred asynchronously.
		/// </summary>
		/// <param name="action">Action to run.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void RunWithoutErrorAsync(Action action) => Task.Run(() =>
		{
			try
			{
				action();
			}
			catch
			{ }
		});
	}
}
