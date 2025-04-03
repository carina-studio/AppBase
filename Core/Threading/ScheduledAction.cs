﻿using System;
using System.Threading;

namespace CarinaStudio.Threading;

/// <summary>
/// Scheduled action which will be performed by specific <see cref="SynchronizationContext"/>. This is a thread-safe class.
/// </summary>
public class ScheduledAction : ISynchronizable
{
	// Fields.
	readonly Action action;
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
	/// <param name="synchronizable"><see cref="ISynchronizable"/> to provide <see cref="SynchronizationContext"/> to perform action.</param>
	/// <param name="action">Action.</param>
	public ScheduledAction(ISynchronizable synchronizable, Action action)
	{
		this.SynchronizationContext = synchronizable.SynchronizationContext;
		this.action = action;
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
	/// Cancel scheduled execution.
	/// </summary>
	/// <returns>True if action has been cancelled.</returns>
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
	protected virtual bool CancelAction(object token) =>
		this.SynchronizationContext.CancelDelayed(token);


	/// <summary>
	/// Execute action on current thread immediately. The scheduled execution will be cancelled.
	/// </summary>
	public void Execute()
	{
		this.Cancel();
		if (SynchronizationContext.Current == this.SynchronizationContext)
			this.action();
		else
			this.SynchronizationContext.Send(_ => this.action(), null);
	}


	// Execute action.
	void ExecuteAction(object? token)
	{
		lock (syncLock)
		{
			if (token != this.token)
				return;
			this.token = null;
		}
		this.action();
	}


	/// <summary>
	/// Execute action on current thread immediately if execution has been scheduled. The scheduled execution will be cancelled.
	/// </summary>
	/// <returns>True if action has been executed.</returns>
	public bool ExecuteIfScheduled()
	{
		if (this.Cancel())
		{
			if (SynchronizationContext.Current == this.SynchronizationContext)
				this.action();
			else
				this.SynchronizationContext.Send(_ => this.action(), null);
			return true;
		}
		return false;
	}


	/// <summary>
	/// Check whether execution has been scheduled or not.
	/// </summary>
	// ReSharper disable once InconsistentlySynchronizedField
	public bool IsScheduled => this.token is not null;


	/// <summary>
	/// Post action to underlying synchronization context.
	/// </summary>
	/// <param name="action">Action.</param>
	/// <param name="state">State.</param>
	/// <param name="delayMillis">Delay time in milliseconds.</param>
	/// <returns>Token to identify the posted action.</returns>
	protected virtual object PostAction(SendOrPostCallback action, object? state, int delayMillis) =>
		this.SynchronizationContext.PostDelayed(action, state, delayMillis);


	/// <summary>
	/// Reschedule execution. It will replace the previous scheduling.
	/// </summary>
	public void Reschedule() => this.Reschedule(0);


	/// <summary>
	/// Reschedule execution. It will replace the previous scheduling.
	/// </summary>
	/// <param name="delayMillis">Delay time in milliseconds.</param>
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
	public void Schedule() => this.Schedule(0);


	/// <summary>
	/// Schedule execution. It won't be scheduled again if execution is already scheduled.
	/// </summary>
	/// <param name="delayMillis">Delay time in milliseconds.</param>
	public void Schedule(int delayMillis)
	{
		using var _ = this.syncLock.EnterScope();
		if (this.token is not null)
			return;
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
	/// Schedule execution. It won't be scheduled again if execution is already scheduled.
	/// </summary>
	/// <param name="delay">Delay time.</param>
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
	/// <see cref="SynchronizationContext"/> to perform action.
	/// </summary>
	public SynchronizationContext SynchronizationContext { get; }
}