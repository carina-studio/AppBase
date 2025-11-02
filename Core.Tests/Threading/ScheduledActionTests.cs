using NUnit.Framework;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace CarinaStudio.Threading
{
	/// <summary>
	/// Tests of <see cref="ScheduledAction"/>.
	/// </summary>
	[TestFixture]
	class ScheduledActionTests
	{
		/// <summary>
		/// Test for cancellation.
		/// </summary>
		[Test]
		public async Task CancellationTest()
		{
			// prepare
			var syncContext = new SynchronizationContext();
			var executed = false;
			var scheduledAction = new ScheduledAction(syncContext, () =>
			{
				executed = true;
			});
			var asyncScheduledAction = new ScheduledAction(syncContext, async cancellationToken =>
			{
				cancellationToken.ThrowIfCancellationRequested();
				await Task.Delay(1000, CancellationToken.None);
				cancellationToken.ThrowIfCancellationRequested();
				executed = true;
			});

			// cancel before execution
			Assert.That(!scheduledAction.IsScheduled, "Action should not be scheduled.");
			scheduledAction.Schedule(200);
			Assert.That(scheduledAction.IsScheduled, "Action should be scheduled.");
			await Task.Delay(100);
			Assert.That(scheduledAction.Cancel(), "Cancellation should be successful.");
			Assert.That(!scheduledAction.Cancel(), "Duplicate cancellation should be failed.");
			Assert.That(!scheduledAction.IsScheduled, "Action should not be scheduled.");
			await Task.Delay(200);
			Assert.That(!executed, "Action should not be executed.");

			// cancel after execution
			scheduledAction.Schedule(200);
			Assert.That(scheduledAction.IsScheduled, "Action should be scheduled.");
			await Task.Delay(400);
			Assert.That(executed, "Action should be executed.");
			Assert.That(!scheduledAction.IsScheduled, "Action should not be scheduled.");
			Assert.That(!scheduledAction.Cancel(), "Cancellation should be failed after execution.");
			
			// cancel during execution (async)
			executed = false;
			using var cancellationTokenSource = new CancellationTokenSource();
			var execTask = asyncScheduledAction.ExecuteAsync(cancellationTokenSource.Token);
			await Task.Delay(300, CancellationToken.None);
			cancellationTokenSource.Cancel();
			try
			{
				await execTask;
				Assert.Fail("The execution should be cancelled.");
			}
			catch (TaskCanceledException)
			{ }
			Assert.That(!executed, "Action should not be executed.");
		}


		/// <summary>
		/// Test for execution on current thread.
		/// </summary>
		[Test]
		public async Task ExecutionTest()
		{
			// prepare
			using var syncContext = new SingleThreadSynchronizationContext();
			var executed = false;
			var execSyncContext = default(SynchronizationContext);
			var scheduledAction = new ScheduledAction(syncContext, () =>
			{
				Thread.Sleep(500);
				execSyncContext = SynchronizationContext.Current;
				executed = true;
			});
			var asyncScheduledAction = new ScheduledAction(syncContext, async () =>
			{
				await Task.Delay(1000);
				execSyncContext = SynchronizationContext.Current;
				executed = true;
			});

			// execute
			scheduledAction.Execute();
			Assert.That(executed, "Action should be executed.");
			Assert.That(ReferenceEquals(syncContext, execSyncContext), "Action should be executed by specific SynchronizationContext.");
			
			// execute (async)
			executed = false;
			execSyncContext = default;
			await scheduledAction.ExecuteAsync();
			Assert.That(executed, "Action should be executed.");
			Assert.That(ReferenceEquals(syncContext, execSyncContext), "Action should be executed by specific SynchronizationContext.");
			executed = false;
			execSyncContext = default;
			await asyncScheduledAction.ExecuteAsync();
			Assert.That(executed, "Action should be executed.");
			Assert.That(ReferenceEquals(syncContext, execSyncContext), "Action should be executed by specific SynchronizationContext.");

			// execute after scheduling
			executed = false;
			execSyncContext = default;
			scheduledAction.Schedule(200);
			Assert.That(scheduledAction.IsScheduled, "Action should be scheduled.");
			scheduledAction.Execute();
			Assert.That(executed, "Action should be executed.");
			Assert.That(ReferenceEquals(syncContext, execSyncContext), "Action should be executed by specific SynchronizationContext.");
			Assert.That(!scheduledAction.IsScheduled, "Action should not be scheduled.");
			
			// execute after scheduling (async)
			executed = false;
			execSyncContext = default;
			asyncScheduledAction.Schedule(200);
			Assert.That(asyncScheduledAction.IsScheduled, "Action should be scheduled.");
			await asyncScheduledAction.ExecuteAsync();
			Assert.That(executed, "Action should be executed.");
			Assert.That(ReferenceEquals(syncContext, execSyncContext), "Action should be executed by specific SynchronizationContext.");
			Assert.That(!scheduledAction.IsScheduled, "Action should not be scheduled.");

			// execute before scheduling
			executed = false;
			execSyncContext = default;
			Assert.That(!scheduledAction.ExecuteIfScheduled(), "Should not execute before scheduling.");
			Assert.That(!executed, "Action should not be executed.");

			// execute after scheduling
			executed = false;
			execSyncContext = default;
			scheduledAction.Schedule(200);
			Assert.That(scheduledAction.IsScheduled, "Action should be scheduled.");
			Assert.That(scheduledAction.ExecuteIfScheduled(), "Should execute after scheduling.");
			Assert.That(executed, "Action should be executed.");
			Assert.That(ReferenceEquals(syncContext, execSyncContext), "Action should be executed by specific SynchronizationContext.");
			Assert.That(!scheduledAction.IsScheduled, "Action should not be scheduled.");
			
			// execute after scheduling (async)
			executed = false;
			execSyncContext = default;
			asyncScheduledAction.Schedule(200);
			Assert.That(asyncScheduledAction.IsScheduled, "Action should be scheduled.");
			Assert.That(await asyncScheduledAction.ExecuteAsyncIfScheduled(), "Should execute after scheduling.");
			Assert.That(executed, "Action should be executed.");
			Assert.That(ReferenceEquals(syncContext, execSyncContext), "Action should be executed by specific SynchronizationContext.");
			Assert.That(!asyncScheduledAction.IsScheduled, "Action should not be scheduled.");
		}


		/// <summary>
		/// Test for reentrant of execution.
		/// </summary>
		[Test]
		public async Task ReentrantTest()
		{
			// prepare sync actions
			var syncContext = new SingleThreadSynchronizationContext();
			var executionCounter = 0;
			var maxExecutionCounter = 0;
			ScheduledAction? syncAction = null;
			ScheduledAction? reentrantSyncAction = null;
			syncAction = new(syncContext, () =>
			{
				Thread.Sleep(100);
				Assert.That(Interlocked.Increment(ref executionCounter) == 1);
				try
				{
					Assert.That(syncAction!.IsExecuting);
					var result = syncAction!.Execute();
					Assert.That(!result);
				}
				finally
				{
					Interlocked.Decrement(ref executionCounter);
				}
			});
			reentrantSyncAction = new(syncContext, () =>
			{
				Thread.Sleep(100);
				var counter = Interlocked.Increment(ref executionCounter);
				try
				{
					Assert.That(reentrantSyncAction!.IsExecuting);
					if (counter == 1)
					{
						var result = reentrantSyncAction!.Execute();
						Assert.That(result);
					}
					else
						Assert.That(counter == 2);
					if (executionCounter > maxExecutionCounter)
						Interlocked.Exchange(ref maxExecutionCounter, executionCounter);
				}
				finally
				{
					Interlocked.Decrement(ref executionCounter);
				}
			}, true);
			
			// execute sync action
			var execResult = syncAction.Execute();
			Assert.That(execResult);
			Assert.That(!syncAction.IsExecuting);
			Assert.That(executionCounter == 0);
			execResult = reentrantSyncAction.Execute();
			Assert.That(execResult);
			Assert.That(!reentrantSyncAction.IsExecuting);
			Assert.That(executionCounter == 0);
			Assert.That(maxExecutionCounter == 2);
			maxExecutionCounter = 0;
			
			// prepare async actions
			using var @event = new ManualResetEventSlim();
			ScheduledAction? asyncAction = null;
			ScheduledAction? reentrantAsyncAction = null;
			asyncAction = new(syncContext, async () =>
			{
				@event.Set();
				await Task.Delay(300);
				Assert.That(Interlocked.Increment(ref executionCounter) == 1);
				try
				{
					Assert.That(asyncAction!.IsExecuting);
					var result = await asyncAction!.ExecuteAsync();
					Assert.That(!result);
				}
				finally
				{
					Interlocked.Decrement(ref executionCounter);
				}
			});
			reentrantAsyncAction = new(syncContext, async () =>
			{
				await Task.Delay(300);
				var counter = Interlocked.Increment(ref executionCounter);
				try
				{
					Assert.That(reentrantAsyncAction!.IsExecuting);
					if (counter == 1)
					{
						@event.Set();
						var result = await reentrantAsyncAction!.ExecuteAsync();
						Assert.That(result);
					}
					else
						Assert.That(counter == 2);
					if (executionCounter > maxExecutionCounter)
						Interlocked.Exchange(ref maxExecutionCounter, executionCounter);
				}
				finally
				{
					Interlocked.Decrement(ref executionCounter);
				}
			}, true);
			
			// execute async action
			var execTask = asyncAction.ExecuteAsync();
			Assert.That(@event.Wait(1000));
			Assert.That(asyncAction.IsExecuting);
			execResult = await asyncAction.ExecuteAsync();
			Assert.That(!execResult);
			Assert.That(asyncAction.IsExecuting);
			await execTask;
			Assert.That(!asyncAction.IsExecuting);
			Assert.That(executionCounter == 0);
			@event.Reset();
			execTask = reentrantAsyncAction.ExecuteAsync();
			Assert.That(@event.Wait(1000));
			Assert.That(reentrantAsyncAction.IsExecuting);
			await execTask;
			Assert.That(!reentrantAsyncAction.IsExecuting);
			Assert.That(executionCounter == 0);
			Assert.That(maxExecutionCounter == 2);
			maxExecutionCounter = 0;
			@event.Reset();
			_ = reentrantAsyncAction.ExecuteAsync();
			await reentrantAsyncAction.ExecuteAsync();
			Assert.That(!reentrantAsyncAction.IsExecuting);
			Assert.That(executionCounter == 0);
			Assert.That(maxExecutionCounter == 2);
			maxExecutionCounter = 0;
			@event.Reset();
		}


		/// <summary>
		/// Test for scheduling execution.
		/// </summary>
		[Test]
		public async Task SchedulingTest()
		{
			// prepare
			var stopWatch = new Stopwatch().Also((it) => it.Start());
			var syncContext = new SynchronizationContext();
			var executionTime = 0L;
			using var @event = new ManualResetEventSlim();
			var scheduledAction = new ScheduledAction(syncContext, () =>
			{
				executionTime = stopWatch.ElapsedMilliseconds;
				@event.Set();
			});
			var asyncScheduledAction = new ScheduledAction(syncContext, async () =>
			{
				executionTime = stopWatch.ElapsedMilliseconds;
				await Task.Delay(1000);
				@event.Set();
			});

			// schedule
			Assert.That(!scheduledAction.IsScheduled, "Action should not be scheduled.");
			var schedulingTime = stopWatch.ElapsedMilliseconds;
			@event.Reset();
			scheduledAction.Schedule(200);
			Assert.That(scheduledAction.IsScheduled, "Action should be scheduled.");
			Assert.That(@event.Wait(1000));
			Assert.That(!scheduledAction.IsScheduled, "Action should not be scheduled.");
			Assert.That(executionTime - schedulingTime >= 200, "Action executed too early.");
			
			// schedule (async)
			Assert.That(!asyncScheduledAction.IsScheduled, "Action should not be scheduled.");
			schedulingTime = stopWatch.ElapsedMilliseconds;
			@event.Reset();
			asyncScheduledAction.Schedule(200);
			Assert.That(asyncScheduledAction.IsScheduled, "Action should be scheduled.");
			Assert.That(@event.Wait(2000));
			Assert.That(!asyncScheduledAction.IsScheduled, "Action should not be scheduled.");
			Assert.That(executionTime - schedulingTime >= 200, "Action executed too early.");

			// schedule twice
			schedulingTime = stopWatch.ElapsedMilliseconds;
			@event.Reset();
			scheduledAction.Schedule(400);
			Assert.That(scheduledAction.IsScheduled, "Action should be scheduled.");
			scheduledAction.Schedule(200);
			Assert.That(@event.Wait(1000));
			Assert.That(!scheduledAction.IsScheduled, "Action should not be scheduled.");
			Assert.That(executionTime - schedulingTime >= 400, "Action executed too early.");

			// reschedule
			schedulingTime = stopWatch.ElapsedMilliseconds;
			@event.Reset();
			scheduledAction.Schedule(200);
			Assert.That(scheduledAction.IsScheduled, "Action should be scheduled.");
			await Task.Delay(100);
			scheduledAction.Reschedule(400);
			Assert.That(scheduledAction.IsScheduled, "Action should be scheduled.");
			Assert.That(@event.Wait(1000));
			Assert.That(!scheduledAction.IsScheduled, "Action should not be scheduled.");
			Assert.That(executionTime - schedulingTime >= 500, "Action executed too early.");
		}
	}
}
