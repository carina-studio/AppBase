using System;
using System.Threading;
using System.Threading.Tasks;

namespace CarinaStudio.Threading;

/// <summary>
/// Extensions for <see cref="SynchronizationContext"/>.
/// </summary>
public static class SynchronizationContextExtensions
{
	// Control block of delayed call-back.
	class DelayedCallbackStub(SynchronizationContext synchronizationContext, SendOrPostCallback callback, object? callbackState) : IDelayedCallbackStub
	{
		// Fields
		volatile bool IsCancellable = true;
		volatile bool IsCancelled;
		public readonly SynchronizationContext SynchronizationContext = synchronizationContext;

		/// <inheritdoc/>
		void IDelayedCallbackStub.Callback()
		{
			// ReSharper disable once IdentifierTypo
			if (this.SynchronizationContext is SingleThreadSynchronizationContext stsc
			    && !stsc.ExecutionThread.IsAlive)
			{
				return;
			}
			try
			{
				this.SynchronizationContext.Post(this.CallbackEntry, null);
			}
			catch (ObjectDisposedException)
			{ }
		}
		
		/// <inheritdoc/>
		bool IDelayedCallbackStub.Cancel()
		{
			lock (this)
			{
				if (!this.IsCancellable || this.IsCancelled)
					return false;
				this.IsCancelled = true;
			}
			return true;
		}

		// Entry of call-back.
		void CallbackEntry(object? state)
		{
			lock (this)
			{
				if (this.IsCancelled)
					return;
				this.IsCancellable = false;
			}
			callback(callbackState);
		}
	}


	/// <summary>
	/// Cancel posted delayed call-back.
	/// </summary>
	/// <param name="synchronizationContext"><see cref="SynchronizationContext"/>.</param>
	/// <param name="token">Token returned from <see cref="PostDelayed(SynchronizationContext, SendOrPostCallback, object?, int)"/>.</param>
	/// <returns>True if call-back cancelled successfully.</returns>
	public static bool CancelDelayed(this SynchronizationContext synchronizationContext, object token)
	{
		if (!DelayedCallbacks.TryGetCallbackStub(token, out var callbackStub)
		    || callbackStub is not DelayedCallbackStub delayedCallbackStub
		    || delayedCallbackStub.SynchronizationContext != synchronizationContext)
		{
			return false;
		}
		if (!DelayedCallbacks.Cancel(token))
			return false;
		// ReSharper disable once IdentifierTypo
		if (synchronizationContext is SingleThreadSynchronizationContext stsc)
			return stsc.ExecutionThread.IsAlive;
		return true;
	}


	/// <summary>
	/// Post call-back.
	/// </summary>
	/// <param name="synchronizationContext"><see cref="SynchronizationContext"/>.</param>
	/// <param name="callback">Call-back.</param>
	public static void Post(this SynchronizationContext synchronizationContext, Action callback) => 
		synchronizationContext.Post(_ => callback(), null);


	/// <summary>
	/// Post delayed call-back.
	/// </summary>
	/// <param name="synchronizationContext"><see cref="SynchronizationContext"/>.</param>
	/// <param name="callback">Call-back.</param>
	/// <param name="delayMillis">Delayed time in milliseconds.</param>
	/// <returns>Token of posted delayed call-back.</returns>
	public static object PostDelayed(this SynchronizationContext synchronizationContext, Action callback, int delayMillis) => 
		PostDelayed(synchronizationContext, _ => callback(), null, delayMillis);
	
	
	/// <summary>
	/// Post delayed call-back.
	/// </summary>
	/// <param name="synchronizationContext"><see cref="SynchronizationContext"/>.</param>
	/// <param name="callback">Call-back.</param>
	/// <param name="delay">Delayed time.</param>
	/// <returns>Token of posted delayed call-back.</returns>
	public static object PostDelayed(this SynchronizationContext synchronizationContext, Action callback, TimeSpan delay)
	{
		var ms = delay.TotalMilliseconds;
		if (ms <= int.MaxValue)
			return PostDelayed(synchronizationContext, _ => callback(), null, (int)ms);
		throw new ArgumentException("The delayed time in milliseconds cannot be greater than Int32.MaxValue.");
	}


	/// <summary>
	/// Post delayed call-back.
	/// </summary>
	/// <param name="synchronizationContext"><see cref="SynchronizationContext"/>.</param>
	/// <param name="callback">Call-back.</param>
	/// <param name="state">Custom state pass to call-back.</param>
	/// <param name="delay">Delayed time.</param>
	/// <returns>Token of posted delayed call-back.</returns>
	public static object PostDelayed(this SynchronizationContext synchronizationContext, SendOrPostCallback callback, object? state, TimeSpan delay)
	{
		var ms = delay.TotalMilliseconds;
		if (ms <= int.MaxValue)
			return PostDelayed(synchronizationContext, callback, state, (int)ms);
		throw new ArgumentException("The delayed time in milliseconds cannot be greater than Int32.MaxValue.");
	}


	/// <summary>
	/// Post delayed call-back.
	/// </summary>
	/// <param name="synchronizationContext"><see cref="SynchronizationContext"/>.</param>
	/// <param name="callback">Call-back.</param>
	/// <param name="state">Custom state pass to call-back.</param>
	/// <param name="delayMillis">Delayed time in milliseconds.</param>
	/// <returns>Token of posted delayed call-back.</returns>
	public static object PostDelayed(this SynchronizationContext synchronizationContext, SendOrPostCallback callback, object? state, int delayMillis)
	{
		// check state
		// ReSharper disable once IdentifierTypo
		if (synchronizationContext is SingleThreadSynchronizationContext stsc && !stsc.ExecutionThread.IsAlive)
			throw new ObjectDisposedException(nameof(SingleThreadSynchronizationContext));

		// schedule call-back
		return DelayedCallbacks.Schedule(new DelayedCallbackStub(synchronizationContext, callback, state), delayMillis);
	}


	/// <summary>
	/// Call given call-back synchronously.
	/// </summary>
	/// <param name="synchronizationContext"><see cref="SynchronizationContext"/>.</param>
	/// <param name="callback">Call-back.</param>
	public static void Send(this SynchronizationContext synchronizationContext, Action callback) => synchronizationContext.Send(_ => callback(), null);


	/// <summary>
	/// Call given function and wait for result.
	/// </summary>
	/// <param name="synchronizationContext"><see cref="SynchronizationContext"/>.</param>
	/// <param name="func">Function.</param>
	/// <typeparam name="R">Type of result.</typeparam>
	/// <returns>Result of function.</returns>
#pragma warning disable CS8603
	public static R Send<R>(this SynchronizationContext synchronizationContext, Func<R> func)
	{
		var result = default(R);
		synchronizationContext.Send(_ =>
		{
			result = func();
		}, null);
		return result;
	}
#pragma warning restore CS8603


	/// <summary>
	/// Send action and wait for completion asynchronously.
	/// </summary>
	/// <param name="synchronizationContext"><see cref="SynchronizationContext"/>.</param>
	/// <param name="action">Action.</param>
	/// <param name="cancellationToken">Cancellation token.</param>
	/// <returns>Task of sending and performing action.</returns>
	public static Task SendAsync(this SynchronizationContext synchronizationContext, Action action, CancellationToken cancellationToken = default)
	{
		if (cancellationToken.IsCancellationRequested)
			return Task.FromCanceled(cancellationToken);
		var taskCompletionSource = new TaskCompletionSource();
		var cancellationTokenRegistration = default(CancellationTokenRegistration);
		cancellationTokenRegistration = cancellationToken.Register(() =>
		{
			cancellationTokenRegistration.Dispose();
			taskCompletionSource.TrySetCanceled();
		});
		synchronizationContext.Post(() =>
		{
			try
			{
				if (cancellationToken.IsCancellationRequested)
					taskCompletionSource.TrySetCanceled();
				else
				{
					action();
					taskCompletionSource.TrySetResult();
				}
			}
			catch (Exception ex)
			{
				taskCompletionSource.TrySetException(ex);
			}
			finally
			{
				cancellationTokenRegistration.Dispose();
			}
		});
		return taskCompletionSource.Task;
	}
	
	
	/// <summary>
	/// Send an asynchronous action and wait for completion asynchronously.
	/// </summary>
	/// <param name="synchronizationContext"><see cref="SynchronizationContext"/>.</param>
	/// <param name="asyncAction">Asynchronous action.</param>
	/// <param name="cancellationToken">Cancellation token.</param>
	/// <returns>Task of sending and performing action.</returns>
	public static Task SendAsync(this SynchronizationContext synchronizationContext, Func<Task> asyncAction, CancellationToken cancellationToken = default)
	{
		if (cancellationToken.IsCancellationRequested)
			return Task.FromCanceled(cancellationToken);
		var taskCompletionSource = new TaskCompletionSource();
		var cancellationTokenRegistration = default(CancellationTokenRegistration);
		cancellationTokenRegistration = cancellationToken.Register(() =>
		{
			cancellationTokenRegistration.Dispose();
			taskCompletionSource.TrySetCanceled();
		});
		synchronizationContext.Post(async void () =>
		{
			try
			{
				if (cancellationToken.IsCancellationRequested)
					taskCompletionSource.TrySetCanceled();
				else
				{
					await asyncAction();
					taskCompletionSource.TrySetResult();
				}
			}
			catch (Exception ex)
			{
				taskCompletionSource.TrySetException(ex);
			}
			finally
			{
				cancellationTokenRegistration.Dispose();
			}
		});
		return taskCompletionSource.Task;
	}
	
	
	/// <summary>
	/// Send action and wait for result asynchronously.
	/// </summary>
	/// <param name="synchronizationContext"><see cref="SynchronizationContext"/>.</param>
	/// <param name="func">Function to generate result.</param>
	/// <param name="cancellationToken">Cancellation token.</param>
	/// <returns>Task of sending and performing action.</returns>
	public static Task<R> SendAsync<R>(this SynchronizationContext synchronizationContext, Func<R> func, CancellationToken cancellationToken = default)
	{
		if (cancellationToken.IsCancellationRequested)
			return Task.FromCanceled<R>(cancellationToken);
		var taskCompletionSource = new TaskCompletionSource<R>();
		var cancellationTokenRegistration = default(CancellationTokenRegistration);
		cancellationTokenRegistration = cancellationToken.Register(() =>
		{
			cancellationTokenRegistration.Dispose();
			taskCompletionSource.TrySetCanceled();
		});
		synchronizationContext.Post(() =>
		{
			try
			{
				if (cancellationToken.IsCancellationRequested)
					taskCompletionSource.TrySetCanceled();
				else
				{
					taskCompletionSource.TrySetResult(func());
				}
			}
			catch (Exception ex)
			{
				taskCompletionSource.TrySetException(ex);
			}
			finally
			{
				cancellationTokenRegistration.Dispose();
			}
		});
		return taskCompletionSource.Task;
	}
	
	
	/// <summary>
	/// Send an asynchronous action and wait for result asynchronously.
	/// </summary>
	/// <param name="synchronizationContext"><see cref="SynchronizationContext"/>.</param>
	/// <param name="asyncFunc">Asynchronous function to generate result.</param>
	/// <param name="cancellationToken">Cancellation token.</param>
	/// <returns>Task of sending and performing action.</returns>
	public static Task<R> SendAsync<R>(this SynchronizationContext synchronizationContext, Func<Task<R>> asyncFunc, CancellationToken cancellationToken = default)
	{
		if (cancellationToken.IsCancellationRequested)
			return Task.FromCanceled<R>(cancellationToken);
		var taskCompletionSource = new TaskCompletionSource<R>();
		var cancellationTokenRegistration = default(CancellationTokenRegistration);
		cancellationTokenRegistration = cancellationToken.Register(() =>
		{
			cancellationTokenRegistration.Dispose();
			taskCompletionSource.TrySetCanceled();
		});
		synchronizationContext.Post(async void () =>
		{
			try
			{
				if (cancellationToken.IsCancellationRequested)
					taskCompletionSource.TrySetCanceled();
				else
				{
					taskCompletionSource.TrySetResult(await asyncFunc());
				}
			}
			catch (Exception ex)
			{
				taskCompletionSource.TrySetException(ex);
			}
			finally
			{
				cancellationTokenRegistration.Dispose();
			}
		});
		return taskCompletionSource.Task;
	}
}