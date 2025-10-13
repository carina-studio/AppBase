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
	volatile object? token;
	readonly Lock syncLock = new();


	/// <summary>
	/// Initialize new <see cref="ScheduledAction"/> instance.
	/// </summary>
	/// <param name="synchronizationContext"><see cref="SynchronizationContext"/> to perform action.</param>
	/// <param name="action">Action.</param>
	public ScheduledAction(SynchronizationContext synchronizationContext, Action action)
	{
		this.SynchronizationContext = synchronizationContext;
		this.action = action;
	}
	
	
	/// <summary>
	/// Initialize new <see cref="ScheduledAction"/> instance.
	/// </summary>
	/// <param name="synchronizationContext"><see cref="SynchronizationContext"/> to perform action.</param>
	/// <param name="asyncAction">Asynchronous action.</param>
	public ScheduledAction(SynchronizationContext synchronizationContext, Func<Task> asyncAction)
	{
		this.SynchronizationContext = synchronizationContext;
		this.action = asyncAction;
	}
	
	
	/// <summary>
	/// Initialize new <see cref="ScheduledAction"/> instance.
	/// </summary>
	/// <param name="synchronizationContext"><see cref="SynchronizationContext"/> to perform action.</param>
	/// <param name="asyncAction">Asynchronous action.</param>
	public ScheduledAction(SynchronizationContext synchronizationContext, Func<CancellationToken, Task> asyncAction)
	{
		this.SynchronizationContext = synchronizationContext;
		this.action = asyncAction;
	}


	/// <summary>
	/// Initialize new <see cref="ScheduledAction"/> instance.
	/// </summary>
	/// <param name="synchronizable"><see cref="ISynchronizable"/> to provide <see cref="SynchronizationContext"/> to perform action.</param>
	/// <param name="action">Action.</param>
	public ScheduledAction(ISynchronizable synchronizable, Action action)
	{
		this.SynchronizationContext = synchronizable.SynchronizationContext;
		this.action = action;
	}
	
	
	/// <summary>
	/// Initialize new <see cref="ScheduledAction"/> instance.
	/// </summary>
	/// <param name="synchronizable"><see cref="ISynchronizable"/> to provide <see cref="SynchronizationContext"/> to perform action.</param>
	/// <param name="asyncAction">Asynchronous action.</param>
	public ScheduledAction(ISynchronizable synchronizable, Func<Task> asyncAction)
	{
		this.SynchronizationContext = synchronizable.SynchronizationContext;
		this.action = asyncAction;
	}
	
	
	/// <summary>
	/// Initialize new <see cref="ScheduledAction"/> instance.
	/// </summary>
	/// <param name="synchronizable"><see cref="ISynchronizable"/> to provide <see cref="SynchronizationContext"/> to perform action.</param>
	/// <param name="asyncAction">Asynchronous action.</param>
	public ScheduledAction(ISynchronizable synchronizable, Func<CancellationToken, Task> asyncAction)
	{
		this.SynchronizationContext = synchronizable.SynchronizationContext;
		this.action = asyncAction;
	}


	/// <summary>
	/// Initialize new <see cref="ScheduledAction"/> instance with current <see cref="SynchronizationContext"/>.
	/// </summary>
	/// <param name="action">Action.</param>
	public ScheduledAction(Action action)
	{
		this.SynchronizationContext = SynchronizationContext.Current ?? throw new InvalidOperationException("No SynchronizationContext on current thread.");
		this.action = action;
	}
	
	
	/// <summary>
	/// Initialize new <see cref="ScheduledAction"/> instance with current <see cref="SynchronizationContext"/>.
	/// </summary>
	/// <param name="asyncAction">Asynchronous action.</param>
	public ScheduledAction(Func<Task> asyncAction)
	{
		this.SynchronizationContext = SynchronizationContext.Current ?? throw new InvalidOperationException("No SynchronizationContext on current thread.");
		this.action = asyncAction;
	}
	
	
	/// <summary>
	/// Initialize new <see cref="ScheduledAction"/> instance with current <see cref="SynchronizationContext"/>.
	/// </summary>
	/// <param name="asyncAction">Asynchronous action.</param>
	public ScheduledAction(Func<CancellationToken, Task> asyncAction)
	{
		this.SynchronizationContext = SynchronizationContext.Current ?? throw new InvalidOperationException("No SynchronizationContext on current thread.");
		this.action = asyncAction;
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
	void DoAction()
	{
		if (this.action is Action action)
			action();
		else if (this.action is Func<Task> taskFunc)
			taskFunc();
		else if (this.action is Func<CancellationToken, Task> cancellableTaskFunc)
			cancellableTaskFunc(CancellationToken.None);
		else
			throw new NotImplementedException();
	}
	
	
	// Do action asynchronously.
	Task DoActionAsync(CancellationToken cancellationToken)
	{
		if (this.action is Func<Task> taskFunc)
			return taskFunc();
		if (this.action is Func<CancellationToken, Task> cancellableTaskFunc)
			return cancellableTaskFunc(cancellationToken);
		if (this.action is Action action)
			action();
		else 
			throw new NotImplementedException();
		return Task.CompletedTask;
	}


	/// <summary>
	/// Execute action on current thread immediately. The scheduled execution will be cancelled.
	/// </summary>
	[ThreadSafe]
	public void Execute()
	{
		this.Cancel();
		if (SynchronizationContext.Current == this.SynchronizationContext)
			this.DoAction();
		else
			this.SendAction(_ => this.DoAction(), null);
	}


	// Execute action.
	[ThreadSafe]
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
	/// <returns>Task of execution.</returns>
	[ThreadSafe]
	public Task ExecuteAsync(CancellationToken cancellationToken = default) =>
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
			return this.DoActionAsync(cancellationToken).ContinueWith(_ => true, cancellationToken);

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
	public bool ExecuteIfScheduled()
	{
		if (this.Cancel())
		{
			if (SynchronizationContext.Current == this.SynchronizationContext)
				this.DoAction();
			else
				this.SendAction(_ => this.DoAction(), null);
			return true;
		}
		return false;
	}


	/// <summary>
	/// Check whether the action to be executed is asynchronous or not.
	/// </summary>
	public bool IsAsynchronous => this.action is not Action;


	/// <summary>
	/// Check whether execution has been scheduled or not.
	/// </summary>
	// ReSharper disable once InconsistentlySynchronizedField
	[ThreadSafe]
	public bool IsScheduled => this.token is not null;


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