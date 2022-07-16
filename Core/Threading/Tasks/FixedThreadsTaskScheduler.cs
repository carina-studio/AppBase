using CarinaStudio.Collections;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CarinaStudio.Threading.Tasks
{
	/// <summary>
	/// <see cref="TaskScheduler"/> which uses dedicated and fixed execution threads to run tasks. This is a thread-safe class.
	/// </summary>
	public class FixedThreadsTaskScheduler : TaskScheduler, IDisposable
	{
		// Synchronization context of scheduler.
		class SyncContext : SynchronizationContext
		{
			// Fields.
			readonly FixedThreadsTaskScheduler scheduler;

			// Constructor.
			public SyncContext(FixedThreadsTaskScheduler scheduler) =>
				this.scheduler = scheduler;
			
			/// <inheritdoc/>
			public override SynchronizationContext CreateCopy() =>
				new SyncContext(this.scheduler);
			
			/// <inheritdoc/>
			public override void Post(SendOrPostCallback d, object? state) =>
				this.scheduler.QueueTask(new Task(() => d(state)));
		}


		// Fields.
		readonly List<Thread> executionThreads;
		volatile bool isDisposed;
		volatile int numberOfBusyThreads;
		readonly LinkedList<Task> scheduledTasks = new LinkedList<Task>();
		readonly object syncLock = new object();
		readonly bool useBackgroundThreads;


		/// <summary>
		/// Initialize new <see cref="FixedThreadsTaskScheduler"/> instance.
		/// </summary>
		/// <param name="maxConcurrencyLevel">Maximum concurrency level.</param>
		/// <param name="useBackgroundThreads">True to set execution threads as background thread.</param>
		public FixedThreadsTaskScheduler(int maxConcurrencyLevel, bool useBackgroundThreads = true)
		{
			if (maxConcurrencyLevel <= 0)
				throw new ArgumentOutOfRangeException(nameof(maxConcurrencyLevel));
			this.MaximumConcurrencyLevel = maxConcurrencyLevel;
			this.useBackgroundThreads = useBackgroundThreads;
			this.executionThreads = new List<Thread>(Math.Min(32, maxConcurrencyLevel));
		}


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
		public int ExecutionThreadCount { get => this.executionThreads.Count; }


		// Entry of execution thread.
		void ExecutionThreadProc()
		{
			var syncContext = new SyncContext(this);
			SynchronizationContext.SetSynchronizationContext(syncContext);
			while (true)
			{
				// get next task
				var task = this.syncLock.Lock(() =>
				{
					if (this.isDisposed)
						return null;
					if (this.scheduledTasks.IsNotEmpty())
					{
						return this.scheduledTasks.First.AsNonNull().Value.Also((_) =>
						{
							this.scheduledTasks.RemoveFirst();
							++this.numberOfBusyThreads;
						});
					}
					else
					{
						Monitor.Wait(this.syncLock);
						return null;
					}
				});
				if (task == null)
				{
					if (this.isDisposed)
						break;
					continue;
				}

				// execute task
				try
				{
					syncContext.OperationStarted();
					this.TryExecuteTask(task);
				}
				finally
				{
					lock (this.syncLock)
						--this.numberOfBusyThreads;
					syncContext.OperationCompleted();
				}
			}
			lock (this.syncLock)
				this.executionThreads.Remove(Thread.CurrentThread);
		}


#pragma warning disable CS1591
		// Get all scheduled tasks.
		protected override IEnumerable<Task>? GetScheduledTasks() => this.scheduledTasks;
#pragma warning restore CS1591


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


#pragma warning disable CS1591
		// Schedule a task.
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
					this.executionThreads.Add(new Thread(this.ExecutionThreadProc).Also((thread) =>
					{
						thread.IsBackground = this.useBackgroundThreads;
						thread.Start();
					}));
				}
			}
		}
#pragma warning restore CS1591


#pragma warning disable CS1591
		// Maximum concurrency level.
		public override int MaximumConcurrencyLevel { get; }
#pragma warning restore CS1591


#pragma warning disable CS1591
		// Try dequeue given task.
		protected override bool TryDequeue(Task task) => this.syncLock.Lock(() =>
		{
			var node = this.scheduledTasks.First;
			while (node != null)
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
#pragma warning restore CS1591


#pragma warning disable CS1591
		// Execute task inline.
		protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
		{
			if (!this.IsExecutionThread)
				return false;
			if (taskWasPreviouslyQueued && !this.TryDequeue(task))
				return false;
			return this.TryExecuteTask(task);
		}
#pragma warning restore CS1591
	}
}
