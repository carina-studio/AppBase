using System;
using System.Threading;
using System.Threading.Tasks;

namespace CarinaStudio.Threading;

/// <summary>
/// Scheduled action which will be performed by specific <see cref="SynchronizationContext"/>. This is a thread-safe class.
/// </summary>
[ThreadSafe]
public class ScheduledAction : ISynchronizable
{
	// Fields.
	readonly object action;
	Action? asyncExecutionCompletedCallback;
	int executionCounter;
	readonly bool isReentrantAllowed;
	volatile object? token;
	readonly Lock syncLock = new();


	/// <summary>
	/// Initialize new <see cref="ScheduledAction"/> instance.
	/// </summary>
	/// <param name="synchronizationContext"><see cref="SynchronizationContext"/> to perform action.</param>
	/// <param name="action">Action.</param>
	/// <param name="allowReentrant">True to allow action being reentrant.</param>
	public ScheduledAction(SynchronizationContext synchronizationContext, Action action, bool allowReentrant = false)
	{
		this.SynchronizationContext = synchronizationContext;
		this.action = action;
		this.isReentrantAllowed = allowReentrant;
	}
	
	
	/// <summary>
	/// Initialize new <see cref="ScheduledAction"/> instance.
	/// </summary>
	/// <param name="synchronizationContext"><see cref="SynchronizationContext"/> to perform action.</param>
	/// <param name="asyncAction">Asynchronous action.</param>
	/// <param name="allowReentrant">True to allow action being reentrant.</param>
	public ScheduledAction(SynchronizationContext synchronizationContext, Func<Task> asyncAction, bool allowReentrant = false)
	{
		this.SynchronizationContext = synchronizationContext;
		this.action = asyncAction;
		this.isReentrantAllowed = allowReentrant;
	}
	
	
	/// <summary>
	/// Initialize new <see cref="ScheduledAction"/> instance.
	/// </summary>
	/// <param name="synchronizationContext"><see cref="SynchronizationContext"/> to perform action.</param>
	/// <param name="asyncAction">Asynchronous action.</param>
	/// <param name="allowReentrant">True to allow action being reentrant.</param>
	public ScheduledAction(SynchronizationContext synchronizationContext, Func<CancellationToken, Task> asyncAction, bool allowReentrant = false)
	{
		this.SynchronizationContext = synchronizationContext;
		this.action = asyncAction;
		this.isReentrantAllowed = allowReentrant;
	}


	/// <summary>
	/// Initialize new <see cref="ScheduledAction"/> instance.
	/// </summary>
	/// <param name="synchronizable"><see cref="ISynchronizable"/> to provide <see cref="SynchronizationContext"/> to perform action.</param>
	/// <param name="action">Action.</param>
	/// <param name="allowReentrant">True to allow action being reentrant.</param>
	public ScheduledAction(ISynchronizable synchronizable, Action action, bool allowReentrant = false)
	{
		this.SynchronizationContext = synchronizable.SynchronizationContext;
		this.action = action;
		this.isReentrantAllowed = allowReentrant;
	}
	
	
	/// <summary>
	/// Initialize new <see cref="ScheduledAction"/> instance.
	/// </summary>
	/// <param name="synchronizable"><see cref="ISynchronizable"/> to provide <see cref="SynchronizationContext"/> to perform action.</param>
	/// <param name="asyncAction">Asynchronous action.</param>
	/// <param name="allowReentrant">True to allow action being reentrant.</param>
	public ScheduledAction(ISynchronizable synchronizable, Func<Task> asyncAction, bool allowReentrant = false)
	{
		this.SynchronizationContext = synchronizable.SynchronizationContext;
		this.action = asyncAction;
		this.isReentrantAllowed = allowReentrant;
	}
	
	
	/// <summary>
	/// Initialize new <see cref="ScheduledAction"/> instance.
	/// </summary>
	/// <param name="synchronizable"><see cref="ISynchronizable"/> to provide <see cref="SynchronizationContext"/> to perform action.</param>
	/// <param name="asyncAction">Asynchronous action.</param>
	/// <param name="allowReentrant">True to allow action being reentrant.</param>
	public ScheduledAction(ISynchronizable synchronizable, Func<CancellationToken, Task> asyncAction, bool allowReentrant = false)
	{
		this.SynchronizationContext = synchronizable.SynchronizationContext;
		this.action = asyncAction;
		this.isReentrantAllowed = allowReentrant;
	}


	/// <summary>
	/// Initialize new <see cref="ScheduledAction"/> instance with current <see cref="SynchronizationContext"/>.
	/// </summary>
	/// <param name="action">Action.</param>
	/// <param name="allowReentrant">True to allow action being reentrant.</param>
	public ScheduledAction(Action action, bool allowReentrant = false)
	{
		this.SynchronizationContext = SynchronizationContext.Current ?? throw new InvalidOperationException("No SynchronizationContext on current thread.");
		this.action = action;
		this.isReentrantAllowed = allowReentrant;
	}
	
	
	/// <summary>
	/// Initialize new <see cref="ScheduledAction"/> instance with current <see cref="SynchronizationContext"/>.
	/// </summary>
	/// <param name="asyncAction">Asynchronous action.</param>
	/// <param name="allowReentrant">True to allow action being reentrant.</param>
	public ScheduledAction(Func<Task> asyncAction, bool allowReentrant = false)
	{
		this.SynchronizationContext = SynchronizationContext.Current ?? throw new InvalidOperationException("No SynchronizationContext on current thread.");
		this.action = asyncAction;
		this.isReentrantAllowed = allowReentrant;
	}
	
	
	/// <summary>
	/// Initialize new <see cref="ScheduledAction"/> instance with current <see cref="SynchronizationContext"/>.
	/// </summary>
	/// <param name="asyncAction">Asynchronous action.</param>
	/// <param name="allowReentrant">True to allow action being reentrant.</param>
	public ScheduledAction(Func<CancellationToken, Task> asyncAction, bool allowReentrant = false)
	{
		this.SynchronizationContext = SynchronizationContext.Current ?? throw new InvalidOperationException("No SynchronizationContext on current thread.");
		this.action = asyncAction;
		this.isReentrantAllowed = allowReentrant;
	}


	/// <summary>
	/// Cancel scheduled execution.
	/// </summary>
	/// <returns>True if action has been cancelled.</returns>
	[ThreadSafe]
	public bool Cancel()
	{
		// ReSharper disable once InconsistentlySynchronizedField
		if (this.token is null)
			return false;
		lock (syncLock)
		{
			if (this.token is not null)
			{
				this.CancelAction(this.token);
				this.token = null;
				return true;
			}
			return false;
		}
	}


	/// <summary>
	/// Cancel posted action.
	/// </summary>
	/// <param name="token">Token returned from <see cref="PostAction"/> to identify the posted action.</param>
	/// <returns>True if action has been cancelled successfully.</returns>
	[ThreadSafe]
	protected virtual bool CancelAction(object token) =>
		this.SynchronizationContext.CancelDelayed(token);
	
	
	// Do action.
	void DoAction() =>
		_ = this.DoActionAsync(CancellationToken.None);
	
	
	// Do action asynchronously.
	Task DoActionAsync(CancellationToken cancellationToken)
	{
		if (!this.isReentrantAllowed && this.executionCounter > 0)
			throw new InvalidOperationException("Reentrant is not allowed.");
		++this.executionCounter;
		if (this.action is Func<Task> taskFunc)
		{
			try
			{
				this.asyncExecutionCompletedCallback ??= this.OnAsyncExecutionCompleted;
				var task = taskFunc();
				task.GetAwaiter().OnCompleted(this.asyncExecutionCompletedCallback);
				return task;
			}
			catch
			{
				--this.executionCounter;
				throw;
			}
			finally
			{
				Thread.MemoryBarrier();
			}
		}
		if (this.action is Func<CancellationToken, Task> cancellableTaskFunc)
		{
			try
			{
				this.asyncExecutionCompletedCallback ??= this.OnAsyncExecutionCompleted;
				var task = cancellableTaskFunc(cancellationToken);
				task.GetAwaiter().OnCompleted(this.asyncExecutionCompletedCallback);
				return task;
			}
			catch
			{
				--this.executionCounter;
				throw;
			}
			finally
			{
				Thread.MemoryBarrier();
			}
		}
		if (this.action is Action action)
		{
			try
			{
				action();
			}
			finally
			{
				--this.executionCounter;
				Thread.MemoryBarrier();
			}
			return Task.CompletedTask;
		}
		else
		{
			--this.executionCounter;
			throw new NotImplementedException();
		}
	}


	/// <summary>
	/// Execute action on current thread immediately. The scheduled execution will be cancelled.
	/// </summary>
	/// <returns>True if the action has been executed.</returns>
	[ThreadSafe]
	public bool Execute() =>
		this.ExecuteInternal(false);


	// Execute action.
	void ExecuteAction(object? token)
	{
		lock (syncLock)
		{
			if (token != this.token)
				return;
			this.token = null;
		}
		this.DoAction();
	}


	/// <summary>
	/// Execute action on current thread asynchronously. The scheduled execution will be cancelled.
	/// </summary>
	/// <param name="cancellationToken"><see cref="CancellationToken"/> to cancel the action.</param>
	/// <returns>Task of execution. The result will be True if action has been executed.</returns>
	[ThreadSafe]
	public Task<bool> ExecuteAsync(CancellationToken cancellationToken = default) =>
		this.ExecuteAsyncInternal(false, cancellationToken);
	
	
	/// <summary>
	/// Execute action on current thread asynchronously if execution has been scheduled. The scheduled execution will be cancelled.
	/// </summary>
	/// <returns>Task of execution. The result will be True if action has been executed.</returns>
	[ThreadSafe]
	public Task<bool> ExecuteAsyncIfScheduled(CancellationToken cancellationToken = default) =>
		this.ExecuteAsyncInternal(true, cancellationToken);
	
	
	// Execute action on current thread asynchronously
	[ThreadSafe]
	Task<bool> ExecuteAsyncInternal(bool execIfScheduled, CancellationToken cancellationToken = default)
	{
		// check state
		cancellationToken.ThrowIfCancellationRequested();
		
		// cancel scheduled execution
		if (!this.Cancel() && execIfScheduled)
			return Task.FromResult(false);
		
		// execute in-place
		if (SynchronizationContext.Current == this.SynchronizationContext)
		{
			if (this.isReentrantAllowed || this.executionCounter == 0)
				return this.DoActionAsync(cancellationToken).ContinueWith(_ => true, cancellationToken);
			return Task.FromResult(false);
		}

		// execute asynchronously
		CancellationTokenRegistration? ctr = null;
		object? postToken = null;
		var taskCompletionSource = new TaskCompletionSource<bool>().Also(taskCompletionSource =>
		{
			taskCompletionSource.Task.GetAwaiter().UnsafeOnCompleted(() =>
			{
				Thread.MemoryBarrier();
				ctr?.Dispose();
			});
		});
		ctr = cancellationToken.Register(_ =>
		{
			Thread.MemoryBarrier();
			if (postToken is not null)
				this.CancelAction(postToken);
			taskCompletionSource.TrySetCanceled();
		}, null);
		if (taskCompletionSource.Task.IsCanceled)
		{
			ctr.Value.Dispose();
			return Task.FromCanceled<bool>(cancellationToken);
		}
		Thread.MemoryBarrier();
		postToken = this.PostAction(_ =>
		{
			if (cancellationToken.IsCancellationRequested)
				return;
			if (!this.isReentrantAllowed && this.executionCounter > 0)
			{
				taskCompletionSource.TrySetResult(false);
				return;
			}
			var task = this.DoActionAsync(cancellationToken);
			task.GetAwaiter().OnCompleted(() =>
			{
				if (task.IsCanceled)
					taskCompletionSource.TrySetCanceled();
				else if (task.IsFaulted)
					taskCompletionSource.TrySetException(task.Exception?.InnerException ?? new Exception("Error occurred while executing the action."));
				else
					taskCompletionSource.TrySetResult(true);
			});
		}, null, 0);
		Thread.MemoryBarrier();
		return taskCompletionSource.Task;
	}


	/// <summary>
	/// Execute action on current thread immediately if execution has been scheduled. The scheduled execution will be cancelled.
	/// </summary>
	/// <returns>True if action has been executed.</returns>
	[ThreadSafe]
	public bool ExecuteIfScheduled() =>
		this.ExecuteInternal(true);
	
	
	// Execute action and wait for completion.
	[ThreadSafe]
	bool ExecuteInternal(bool execIfScheduled)
	{
		if (!this.Cancel() && execIfScheduled)
			return false;
		if (SynchronizationContext.Current == this.SynchronizationContext)
		{
			if (this.isReentrantAllowed || this.executionCounter == 0)
			{
				this.DoAction();
				return true;
			}
			return false;
		}
		else
		{
			var result = true;
			this.SendAction(_ =>
			{
				if (this.isReentrantAllowed || this.executionCounter == 0)
					this.DoAction();
				else
				{
					result = false;
					Thread.MemoryBarrier();
				}
			}, null);
			Thread.MemoryBarrier();
			return result;
		}
	}


	/// <summary>
	/// Check whether the action to be executed is asynchronous or not.
	/// </summary>
	public bool IsAsynchronous => this.action is not Action;


	/// <summary>
	/// Check whether the action is currently being executed or not.
	/// </summary>
	public bool IsExecuting
	{
		get
		{
			Thread.MemoryBarrier();
			return this.executionCounter > 0;
		}
	}


	/// <summary>
	/// Check whether execution has been scheduled or not.
	/// </summary>
	// ReSharper disable once InconsistentlySynchronizedField
	[ThreadSafe]
	public bool IsScheduled => this.token is not null;


	// Called when asynchronous execution has been completed.
	void OnAsyncExecutionCompleted()
	{
		--this.executionCounter;
		if (this.executionCounter < 0)
			throw new InternalStateCorruptedException();
		Thread.MemoryBarrier();
	}


	/// <summary>
	/// Post action to underlying synchronization context.
	/// </summary>
	/// <param name="action">Action.</param>
	/// <param name="state">State.</param>
	/// <param name="delayMillis">Delay time in milliseconds.</param>
	/// <returns>Token to identify the posted action.</returns>
	[ThreadSafe]
	protected virtual object PostAction(SendOrPostCallback action, object? state, int delayMillis) =>
		this.SynchronizationContext.PostDelayed(action, state, delayMillis);


	/// <summary>
	/// Reschedule execution. It will replace the previous scheduling.
	/// </summary>
	[ThreadSafe]
	public void Reschedule() => this.Reschedule(0);


	/// <summary>
	/// Reschedule execution. It will replace the previous scheduling.
	/// </summary>
	/// <param name="delayMillis">Delay time in milliseconds.</param>
	[ThreadSafe]
	public void Reschedule(int delayMillis)
	{
		using var _ = this.syncLock.EnterScope();
		if (this.token is not null)
			this.CancelAction(this.token);
		object? token = null;
		token = this.PostAction(_ =>
		{
			lock (syncLock) // barrier to make sure that variable 'token' has been assigned
			{ }
			this.ExecuteAction(token);
		}, null, delayMillis);
		this.token = token;
	}


	/// <summary>
	/// Reschedule execution. It will replace the previous scheduling.
	/// </summary>
	/// <param name="delay">Delay time.</param>
	[ThreadSafe]
	public void Reschedule(TimeSpan delay)
	{
		var delayMillis = (long)delay.TotalMilliseconds;
		if (delayMillis > int.MaxValue)
			this.Reschedule(int.MaxValue);
		else if (delayMillis <= 0)
			this.Reschedule(0);
		else
			this.Reschedule((int)delayMillis);
	}


	/// <summary>
	/// Schedule execution. It won't be scheduled again if execution is already scheduled.
	/// </summary>
	[ThreadSafe]
	public void Schedule() => this.Schedule(0);


	/// <summary>
	/// Schedule execution. It won't be scheduled again if execution is already scheduled.
	/// </summary>
	/// <param name="delayMillis">Delay time in milliseconds.</param>
	[ThreadSafe]
	public void Schedule(int delayMillis)
	{
		using var _ = this.syncLock.EnterScope();
		if (this.token is not null)
			return;
		object? token = null;
		token = this.PostAction(_ =>
		{
			using (this.syncLock.EnterScope()) // barrier to make sure that variable 'token' has been assigned
			{ }
			this.ExecuteAction(token);
		}, null, delayMillis);
		this.token = token;
	}


	/// <summary>
	/// Schedule execution. It won't be scheduled again if execution is already scheduled.
	/// </summary>
	/// <param name="delay">Delay time.</param>
	[ThreadSafe]
	public void Schedule(TimeSpan delay)
    {
		var delayMillis = (long)delay.TotalMilliseconds;
		if (delayMillis > int.MaxValue)
			this.Schedule(int.MaxValue);
		else if (delayMillis <= 0)
			this.Schedule(0);
		else
			this.Schedule((int)delayMillis);
	}
	
	
	/// <summary>
	/// Send action to underlying synchronization context and wait for completion.
	/// </summary>
	/// <param name="action">Action.</param>
	/// <param name="state">State.</param>
	[ThreadSafe]
	protected virtual void SendAction(SendOrPostCallback action, object? state) =>
		this.SynchronizationContext.Send(action, state);


	/// <summary>
	/// <see cref="SynchronizationContext"/> to perform action.
	/// </summary>
	[ThreadSafe]
	public SynchronizationContext SynchronizationContext { get; }
}