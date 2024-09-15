using NUnit.Framework;
using System;
using System.Diagnostics;
using System.Threading;

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
		public void CancellationTest()
		{
			// prepare
			var syncContext = new SynchronizationContext();
			var executed = false;
			var scheduledAction = new ScheduledAction(syncContext, () =>
			{
				executed = true;
			});

			// cancel before execution
			Assert.That(!scheduledAction.IsScheduled, "Action should not be scheduled.");
			scheduledAction.Schedule(200);
			Assert.That(scheduledAction.IsScheduled, "Action should be scheduled.");
			Thread.Sleep(100);
			Assert.That(scheduledAction.Cancel(), "Cancellation should be successful.");
			Assert.That(!scheduledAction.Cancel(), "Duplicate cancellation should be failed.");
			Assert.That(!scheduledAction.IsScheduled, "Action should not be scheduled.");
			Thread.Sleep(200);
			Assert.That(!executed, "Action should not be executed.");

			// cancel after execution
			scheduledAction.Schedule(200);
			Assert.That(scheduledAction.IsScheduled, "Action should be scheduled.");
			Thread.Sleep(400);
			Assert.That(executed, "Action should be executed.");
			Assert.That(!scheduledAction.IsScheduled, "Action should not be scheduled.");
			Assert.That(!scheduledAction.Cancel(), "Cancellation should be failed after execution.");
		}


		/// <summary>
		/// Test for execution on current thread.
		/// </summary>
		[Test]
		public void ExecutionTest()
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

			// execute
			scheduledAction.Execute();
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

			// execute on other thread
			executed = false;
			execSyncContext = default;
			var exception = (Exception?)null;
			ThreadPool.QueueUserWorkItem(_ =>
			{
				try
				{
					scheduledAction.Execute();
				}
				catch (Exception ex)
				{
					exception = ex;
				}
			});
			Thread.Sleep(1000);
			if (exception != null)
				throw exception;
			Assert.That(executed, "Action should be executed.");
			Assert.That(ReferenceEquals(syncContext, execSyncContext), "Action should be executed by specific SynchronizationContext.");

			// execute before scheduling
			executed = false;
			execSyncContext = default;
			Assert.That(!scheduledAction.ExecuteIfScheduled(), "Should not execute before scheduling.");
			Assert.That(!executed, "Action should not be executed.");

			// execute after scheduling
			scheduledAction.Schedule(200);
			Assert.That(scheduledAction.IsScheduled, "Action should be scheduled.");
			Assert.That(scheduledAction.ExecuteIfScheduled(), "Should execute after scheduling.");
			Assert.That(executed, "Action should be executed.");
			Assert.That(ReferenceEquals(syncContext, execSyncContext), "Action should be executed by specific SynchronizationContext.");
			Assert.That(!scheduledAction.IsScheduled, "Action should not be scheduled.");

			// execute on other thread
			executed = false;
			exception = null;
			scheduledAction.Schedule(200);
			Assert.That(scheduledAction.IsScheduled, "Action should be scheduled.");
			ThreadPool.QueueUserWorkItem(_ =>
			{
				try
				{
					scheduledAction.ExecuteIfScheduled();
				}
				catch (Exception ex)
				{
					exception = ex;
				}
			});
			Thread.Sleep(1000);
			if (exception != null)
				throw exception;
			Assert.That(executed, "Action should be executed.");
			Assert.That(ReferenceEquals(syncContext, execSyncContext), "Action should be executed by specific SynchronizationContext.");
		}


		/// <summary>
		/// Test for scheduling execution.
		/// </summary>
		[Test]
		public void SchedulingTest()
		{
			// prepare
			var stopWatch = new Stopwatch().Also((it) => it.Start());
			var syncContext = new SynchronizationContext();
			var executed = false;
			var executionTime = 0L;
			var scheduledAction = new ScheduledAction(syncContext, () =>
			{
				executionTime = stopWatch.ElapsedMilliseconds;
				executed = true;
			});

			// schedule
			Assert.That(!scheduledAction.IsScheduled, "Action should not be scheduled.");
			var schedulingTime = stopWatch.ElapsedMilliseconds;
			scheduledAction.Schedule(200);
			Assert.That(scheduledAction.IsScheduled, "Action should be scheduled.");
			Thread.Sleep(400);
			Assert.That(executed, "Action should be executed.");
			Assert.That(!scheduledAction.IsScheduled, "Action should not be scheduled.");
			Assert.That(executionTime - schedulingTime >= 200, "Action executed too early.");

			// schedule twice
			executed = false;
			schedulingTime = stopWatch.ElapsedMilliseconds;
			scheduledAction.Schedule(400);
			Assert.That(scheduledAction.IsScheduled, "Action should be scheduled.");
			scheduledAction.Schedule(200);
			Thread.Sleep(600);
			Assert.That(executed, "Action should be executed.");
			Assert.That(!scheduledAction.IsScheduled, "Action should not be scheduled.");
			Assert.That(executionTime - schedulingTime >= 400, "Action executed too early.");

			// reschedule
			executed = false;
			schedulingTime = stopWatch.ElapsedMilliseconds;
			scheduledAction.Schedule(200);
			Assert.That(scheduledAction.IsScheduled, "Action should be scheduled.");
			Thread.Sleep(100);
			scheduledAction.Reschedule(400);
			Assert.That(scheduledAction.IsScheduled, "Action should be scheduled.");
			Thread.Sleep(300);
			Assert.That(!executed, "Action should not be executed.");
			Thread.Sleep(300);
			Assert.That(executed, "Action should be executed.");
			Assert.That(!scheduledAction.IsScheduled, "Action should not be scheduled.");
			Assert.That(executionTime - schedulingTime >= 500, "Action executed too early.");
		}
	}
}
