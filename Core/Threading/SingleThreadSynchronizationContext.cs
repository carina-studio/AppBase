using System;
using System.Threading;

namespace CarinaStudio.Threading
{
	/// <summary>
	/// Implementation of <see cref="SynchronizationContext"/> which uses single execution thread to handle posted call-backs. This is thread-safe class.
	/// </summary>
	public class SingleThreadSynchronizationContext : SynchronizationContext, IDisposable
	{
		// Control block of posted call-back.
		class PostedCallback
		{
			public volatile SendOrPostCallback? Callback;
			public volatile object? CallbackState;
			public volatile PostedCallback? Next;
		}


		// Fields.
		readonly Thread executionThread;
		volatile PostedCallback? freePostedCallbackListHead;
		volatile bool isDisposed;
		volatile PostedCallback? postedCallbackListHead;
		volatile PostedCallback? postedCallbackListTail;
		readonly object syncLock = new object();


		/// <summary>
		/// Initialize new <see cref="SingleThreadSynchronizationContext"/> instance.
		/// </summary>
		/// <param name="name">Name.</param>
		/// <param name="isBackground">True to set execution thread as background.</param>
		public SingleThreadSynchronizationContext(string? name = null, bool isBackground = true)
		{
			this.executionThread = new Thread(this.ExecutionThreadProc).Also((it) =>
			{
				it.IsBackground = isBackground;
				it.Name = name;
				it.Start();
			});
		}


		/// <summary>
		/// Dispose the instance.
		/// </summary>
		public void Dispose()
		{
			lock (this.syncLock)
			{
				if (this.isDisposed)
					return;
				this.isDisposed = true;
				Monitor.Pulse(this.syncLock);
			}
		}


		/// <summary>
		/// Get execution thread.
		/// </summary>
		public Thread ExecutionThread { get => this.executionThread; }


		// Entry of execution thread.
		void ExecutionThreadProc()
		{
			// setup
			SynchronizationContext.SetSynchronizationContext(this);

			// execute
			while (true)
			{
				// check state
				if (this.isDisposed)
					break;

				// get next call-back
				var postedCallback = this.syncLock.Lock(() =>
				{
					if (this.postedCallbackListHead != null)
					{
						var postedCallback = this.postedCallbackListHead;
						this.postedCallbackListHead = postedCallback.Next;
						if (postedCallbackListTail == postedCallback)
							this.postedCallbackListTail = null;
						postedCallback.Next = null;
						return postedCallback;
					}
					else
					{
						Monitor.Wait(this.syncLock);
						return null;
					}
				});
				if (postedCallback == null)
					continue;

				// execute call-back
				this.OperationStarted();
				postedCallback.Callback?.Invoke(postedCallback.CallbackState);

				// recycle control block
				postedCallback.Callback = null;
				postedCallback.CallbackState = null;
				lock (this.syncLock)
				{
					postedCallback.Next = this.freePostedCallbackListHead;
					this.freePostedCallbackListHead = postedCallback;
				}

				// complete call-back
				this.OperationCompleted();
			}
		}


		/// <summary>
		/// Get name.
		/// </summary>
		public string? Name { get => this.executionThread.Name; }


		/// <summary>
		/// Post a call-back.
		/// </summary>
		/// <param name="callback">Call-back.</param>
		/// <param name="state">Custom state passed to call-back.</param>
		public override void Post(SendOrPostCallback callback, object? state)
		{
			lock (this.syncLock)
			{
				// check state
				this.ThrowIfDisposed();

				// prepare control block
				var postedCallback = this.freePostedCallbackListHead?.Also((it) =>
				{
					this.freePostedCallbackListHead = it.Next;
					it.Next = null;
				}) ?? new PostedCallback();
				postedCallback.Callback = callback;
				postedCallback.CallbackState = state;

				// enqueue call-back
				if (this.postedCallbackListTail != null)
					this.postedCallbackListTail.Next = postedCallback;
				else
					this.postedCallbackListHead = postedCallback;
				this.postedCallbackListTail = postedCallback;

				// notify execution thread
				Monitor.Pulse(this.syncLock);
			}
		}


		/// <summary>
		/// Send a call-back and wait for execution completed.
		/// </summary>
		/// <param name="callback">Call-back.</param>
		/// <param name="state">Custom state passed to call-back.</param>
		public override void Send(SendOrPostCallback callback, object? state)
		{
			if (Thread.CurrentThread == this.executionThread)
				callback(state);
			else
			{
				var syncLock = new object();
				var exception = (Exception?)null;
				lock (syncLock)
				{
					this.Post((_) =>
					{
						try
						{
							callback(state);
						}
						catch (Exception ex)
						{
							exception = ex;
						}
						finally
						{
							lock (syncLock)
								Monitor.Pulse(syncLock);
						}
					}, null);
					Monitor.Wait(syncLock);
				}
				if (exception != null)
					throw new Exception("Exception occurred while executing call-back.", exception);
			}
		}


		// Throw exception if disposed.
		void ThrowIfDisposed()
		{
			if (this.isDisposed)
				throw new ObjectDisposedException(this.GetType().Name);
		}
	}
}
