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
