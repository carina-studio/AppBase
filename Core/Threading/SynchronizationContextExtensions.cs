using System;
using System.Diagnostics;
using System.Threading;

namespace CarinaStudio.Threading
{
	/// <summary>
	/// Extensions for <see cref="SynchronizationContext"/>.
	/// </summary>
	public static class SynchronizationContextExtensions
	{
		// Control block of delayed call-back.
		class DelayedCallback
		{
			// Fields
			readonly SendOrPostCallback Callback;
			readonly object? CallbackState;
			public volatile bool IsCancellable = true;
			public volatile bool IsCancelled;
			public volatile DelayedCallback? Next;
			public volatile DelayedCallback? Previous;
			public readonly long ReadyTime;
			public readonly SynchronizationContext SynchronizationContext;

			// Constructor.
			public DelayedCallback(SynchronizationContext synchronizationContext, SendOrPostCallback callback, object? state, long readyTime)
			{
				this.Callback = callback;
				this.CallbackState = state;
				this.ReadyTime = readyTime;
				this.SynchronizationContext = synchronizationContext;
			}

			// Entry of call-back.
			public void CallbackEntry(object? state)
			{
				lock (this)
				{
					if (this.IsCancelled)
						return;
					this.IsCancellable = false;
				}
				this.Callback(this.CallbackState);
			}
		}


		// Fields.
		static volatile DelayedCallback? DelayedCallbackListHead;
		static readonly object DelayedCallbackSyncLock = new();
		static readonly Thread DelayedCallbackThread;
		static readonly Stopwatch DelayedCallbackWatch = new();


		// Initializer.
		static SynchronizationContextExtensions()
		{
			DelayedCallbackThread = new Thread(DelayedCallbackThreadProc)
			{
				IsBackground = true
			};
		}


		/// <summary>
		/// Cancel posted delayed call-back.
		/// </summary>
		/// <param name="synchronizationContext"><see cref="SynchronizationContext"/>.</param>
		/// <param name="token">Token returned from <see cref="PostDelayed(SynchronizationContext, SendOrPostCallback, object?, int)"/>.</param>
		/// <returns>True if call-back cancelled successfully.</returns>
		public static bool CancelDelayed(this SynchronizationContext synchronizationContext, object token)
		{
			if (token is not DelayedCallback delayedCallback)
				throw new ArgumentException("Invalid token.");
			if (delayedCallback.SynchronizationContext != synchronizationContext)
				return false;
			// ReSharper disable once IdentifierTypo
			var stsc = (synchronizationContext as SingleThreadSynchronizationContext);
			var isSyncContextAlive = (stsc == null || stsc.ExecutionThread.IsAlive);
			lock (DelayedCallbackSyncLock)
			{
				if (DelayedCallbackListHead == delayedCallback)
				{
					DelayedCallbackListHead = delayedCallback.Next;
					if (delayedCallback.Next != null)
						delayedCallback.Next.Previous = null;
					delayedCallback.Next = null;
					delayedCallback.IsCancelled = true;
					return isSyncContextAlive;
				}
				if (delayedCallback.Previous != null || delayedCallback.Next != null)
				{
					if (delayedCallback.Previous != null)
						delayedCallback.Previous.Next = delayedCallback.Next;
					if (delayedCallback.Next != null)
						delayedCallback.Next.Previous = delayedCallback.Previous;
					delayedCallback.Previous = null;
					delayedCallback.Next = null;
					delayedCallback.IsCancelled = true;
					return isSyncContextAlive;
				}
			}
			lock (delayedCallback)
			{
				if (delayedCallback.IsCancelled || !delayedCallback.IsCancellable)
					return false;
				delayedCallback.IsCancelled = true;
				return isSyncContextAlive;
			}
		}


		// Entry of delayed call-back thread.
		static void DelayedCallbackThreadProc()
		{
			while (true)
			{
				// select next call-back
				DelayedCallback? delayedCallback = null;
				lock (DelayedCallbackSyncLock)
				{
					// check call-back
					var waitingTime = 0;
					if (DelayedCallbackListHead != null)
					{
						var currentTime = DelayedCallbackWatch.ElapsedMilliseconds;
						var timeDiff = DelayedCallbackListHead.ReadyTime - currentTime;
						if (timeDiff <= 0)
						{
							delayedCallback = DelayedCallbackListHead;
							if (delayedCallback.Next != null)
								delayedCallback.Next.Previous = null;
							DelayedCallbackListHead = delayedCallback.Next;
							delayedCallback.Next = null;
						}
						else if (timeDiff <= int.MaxValue)
							waitingTime = (int)timeDiff;
						else
							waitingTime = int.MaxValue;
					}
					else
						waitingTime = Timeout.Infinite;

					// wait for next call-back
					if (waitingTime != 0)
					{
						Monitor.Wait(DelayedCallbackSyncLock, waitingTime);
						continue;
					}
				}

				// post call-back
				if (delayedCallback != null)
				{
					try
					{
						delayedCallback.SynchronizationContext.Post(delayedCallback.CallbackEntry, null);
					}
					catch (ObjectDisposedException) // ignore posting to disposed context
					{ }
				}
			}
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

			// setup environment
			if (!DelayedCallbackWatch.IsRunning)
			{
				lock (typeof(SynchronizationContextExtensions))
				{
					if (!DelayedCallbackWatch.IsRunning)
					{
						DelayedCallbackWatch.Start();
						DelayedCallbackThread.Start();
					}
				}
			}

			// create delayed call-back
			if (delayMillis < 0)
				delayMillis = 0;
			var readyTime = DelayedCallbackWatch.ElapsedMilliseconds + delayMillis;
			var delayedCallback = new DelayedCallback(synchronizationContext, callback, state, readyTime);

			// enqueue to list or post directly
			if (delayMillis > 0)
			{
				lock (DelayedCallbackSyncLock)
				{
					var prevDelayedCallback = (DelayedCallback?)null;
					var nextDelayedCallback = DelayedCallbackListHead;
					while (nextDelayedCallback != null)
					{
						if (nextDelayedCallback.ReadyTime > readyTime)
							break;
						prevDelayedCallback = nextDelayedCallback;
						nextDelayedCallback = nextDelayedCallback.Next;
					}
					if (nextDelayedCallback != null)
					{
						delayedCallback.Next = nextDelayedCallback;
						nextDelayedCallback.Previous = delayedCallback;
					}
					if (prevDelayedCallback != null)
					{
						prevDelayedCallback.Next = delayedCallback;
						delayedCallback.Previous = prevDelayedCallback;
					}
					else
					{
						DelayedCallbackListHead = delayedCallback;
						Monitor.Pulse(DelayedCallbackSyncLock);
					}
				}
			}
			else
				synchronizationContext.Post(delayedCallback.CallbackEntry, null);
			return delayedCallback;
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
	}
}
