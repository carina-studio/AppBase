﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CarinaStudio.Threading.Tasks;

/// <summary>
/// <see cref="TaskScheduler"/> which uses dedicated and fixed execution threads to run tasks. This is a thread-safe class.
/// </summary>
public class FixedThreadsTaskScheduler : TaskScheduler, IDisposable
{
	// Fields.
	readonly List<Thread> executionThreads;
	volatile bool isDisposed;
	volatile int latestExecThreadId;
	int numberOfBusyThreads;
	readonly LinkedList<Task> scheduledTasks = new();
	readonly object syncLock = new();
	readonly bool useBackgroundThreads;
	
	
	/// <summary>
	/// Initialize new <see cref="FixedThreadsTaskScheduler"/> instance.
	/// </summary>
	/// <param name="maxConcurrencyLevel">Maximum concurrency level.</param>
	/// <param name="useBackgroundThreads">True to set execution threads as background thread.</param>
	public FixedThreadsTaskScheduler(int maxConcurrencyLevel, bool useBackgroundThreads = true) : this(null, maxConcurrencyLevel, useBackgroundThreads)
	{ }


	/// <summary>
	/// Initialize new <see cref="FixedThreadsTaskScheduler"/> instance.
	/// </summary>
	/// <param name="name">Name of scheduler.</param>
	/// <param name="maxConcurrencyLevel">Maximum concurrency level.</param>
	/// <param name="useBackgroundThreads">True to set execution threads as background thread.</param>
	public FixedThreadsTaskScheduler(string? name, int maxConcurrencyLevel, bool useBackgroundThreads = true)
	{
		if (maxConcurrencyLevel <= 0)
			throw new ArgumentOutOfRangeException(nameof(maxConcurrencyLevel));
		this.MaximumConcurrencyLevel = maxConcurrencyLevel;
		this.Name = name;
		this.useBackgroundThreads = useBackgroundThreads;
		this.executionThreads = new List<Thread>(Math.Min(32, maxConcurrencyLevel));
	}


	/// <summary>
	/// Get number of threads which are executing tasks.
	/// </summary>
	public int BusyThreadCount => this.numberOfBusyThreads;


	/// <summary>
	/// Dispose the instance.
	/// </summary>
	public void Dispose()
	{
		lock (this.syncLock)
		{
			// update state
			if (this.isDisposed)
				return;
			this.isDisposed = true;

			// notify execution threads to stop
			Monitor.PulseAll(this.syncLock);

			// drop all scheduled tasks
			this.scheduledTasks.Clear();
		}
	}


	/// <summary>
	/// Get number of active execution threads.
	/// </summary>
	public int ExecutionThreadCount => this.executionThreads.Count;


	// Entry of execution thread.
	void ExecutionThreadProc()
	{
		while (true)
		{
			// get next task
			var task = this.syncLock.Lock(() =>
			{
				if (this.isDisposed)
					return null;
				if (this.scheduledTasks.Count > 0)
				{
					return this.scheduledTasks.First.AsNonNull().Value.Also(_ =>
					{
						this.scheduledTasks.RemoveFirst();
						++this.numberOfBusyThreads;
					});
				}
				Monitor.Wait(this.syncLock);
				return null;
			});
			if (task is null)
			{
				if (this.isDisposed)
					break;
				continue;
			}

			// execute task
			try
			{
				this.TryExecuteTask(task);
			}
			finally
			{
				lock (this.syncLock)
					--this.numberOfBusyThreads;
			}
		}
		lock (this.syncLock)
			this.executionThreads.Remove(Thread.CurrentThread);
	}


	/// <inheritdoc/>
	protected override IEnumerable<Task> GetScheduledTasks() => this.scheduledTasks;


	/// <summary>
	/// Check whether current thread is one of execution thread of this scheduler or not.
	/// </summary>
	public bool IsExecutionThread
	{
		get
		{
			lock (this.syncLock)
				return this.executionThreads.Contains(Thread.CurrentThread);
		}
	}
	
	
	/// <summary>
	/// Get name of scheduler.
	/// </summary>
	public string? Name { get; }


	/// <inheritdoc/>
	protected override void QueueTask(Task task)
	{
		lock (this.syncLock)
		{
			// check state
			if (this.isDisposed)
				throw new ObjectDisposedException(this.GetType().Name);

			// enqueue task
			this.scheduledTasks.AddLast(task);

			// trigger execution
			if (this.numberOfBusyThreads < this.executionThreads.Count)
				Monitor.Pulse(this.syncLock);
			else if (this.executionThreads.Count < this.MaximumConcurrencyLevel)
			{
				this.executionThreads.Add(new Thread(this.ExecutionThreadProc).Also(thread =>
				{
					thread.IsBackground = this.useBackgroundThreads;
					thread.Name = $"{this.Name ?? "FTTaskScheduler"} [{this.Id}]-{Interlocked.Increment(ref this.latestExecThreadId)}";
					thread.Start();
				}));
			}
		}
	}


	/// <inheritdoc/>
	public override int MaximumConcurrencyLevel { get; }


	/// <inheritdoc/>
	protected override bool TryDequeue(Task task) => this.syncLock.Lock(() =>
	{
		var node = this.scheduledTasks.First;
		while (node is not null)
		{
			if (node.Value == task)
			{
				this.scheduledTasks.Remove(node);
				return true;
			}
			node = node.Next;
		}
		return false;
	});


	/// <inheritdoc/>
	protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
	{
		if (!this.IsExecutionThread)
			return false;
		if (taskWasPreviouslyQueued && !this.TryDequeue(task))
			return false;
		return this.TryExecuteTask(task);
	}
}