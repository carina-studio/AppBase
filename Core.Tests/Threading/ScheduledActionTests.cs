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
			Assert.IsFalse(scheduledAction.IsScheduled, "Action should not be scheduled.");
			scheduledAction.Schedule(200);
			Assert.IsTrue(scheduledAction.IsScheduled, "Action should be scheduled.");
			Thread.Sleep(100);
			Assert.IsTrue(scheduledAction.Cancel(), "Cancellation should be successful.");
			Assert.IsFalse(scheduledAction.Cancel(), "Duplicate cancellation should be failed.");
			Assert.IsFalse(scheduledAction.IsScheduled, "Action should not be scheduled.");
			Thread.Sleep(200);
			Assert.IsFalse(executed, "Action should not be executed.");

			// cancel after execution
			scheduledAction.Schedule(200);
			Assert.IsTrue(scheduledAction.IsScheduled, "Action should be scheduled.");
			Thread.Sleep(400);
			Assert.IsTrue(executed, "Action should be executed.");
			Assert.IsFalse(scheduledAction.IsScheduled, "Action should not be scheduled.");
			Assert.IsFalse(scheduledAction.Cancel(), "Cancellation should be failed after execution.");
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
			Assert.IsTrue(executed, "Action should be executed.");
			Assert.AreEqual(syncContext, execSyncContext, "Action should be executed by specific SynchronizationContext.");

			// execute after scheduling
			executed = false;
			execSyncContext = default;
			scheduledAction.Schedule(200);
			Assert.IsTrue(scheduledAction.IsScheduled, "Action should be scheduled.");
			scheduledAction.Execute();
			Assert.IsTrue(executed, "Action should be executed.");
			Assert.AreEqual(syncContext, execSyncContext, "Action should be executed by specific SynchronizationContext.");
			Assert.IsFalse(scheduledAction.IsScheduled, "Action should not be scheduled.");

			// execute on other thread
			executed = false;
			execSyncContext = default;
			var exception = (Exception?)null;
			ThreadPool.QueueUserWorkItem((_) =>
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
			Assert.IsTrue(executed, "Action should be executed.");
			Assert.AreEqual(syncContext, execSyncContext, "Action should be executed by specific SynchronizationContext.");

			// execute before scheduling
			executed = false;
			execSyncContext = default;
			Assert.IsFalse(scheduledAction.ExecuteIfScheduled(), "Should not execute before scheduling.");
			Assert.IsFalse(executed, "Action should not be executed.");

			// execute after scheduling
			scheduledAction.Schedule(200);
			Assert.IsTrue(scheduledAction.IsScheduled, "Action should be scheduled.");
			Assert.IsTrue(scheduledAction.ExecuteIfScheduled(), "Should execute after scheduling.");
			Assert.IsTrue(executed, "Action should be executed.");
			Assert.AreEqual(syncContext, execSyncContext, "Action should be executed by specific SynchronizationContext.");
			Assert.IsFalse(scheduledAction.IsScheduled, "Action should not be scheduled.");

			// execute on other thread
			executed = false;
			exception = null;
			scheduledAction.Schedule(200);
			Assert.IsTrue(scheduledAction.IsScheduled, "Action should be scheduled.");
			ThreadPool.QueueUserWorkItem((_) =>
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
			Assert.IsTrue(executed, "Action should be executed.");
			Assert.AreEqual(syncContext, execSyncContext, "Action should be executed by specific SynchronizationContext.");
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
			Assert.IsFalse(scheduledAction.IsScheduled, "Action should not be scheduled.");
			var schedulingTime = stopWatch.ElapsedMilliseconds;
			scheduledAction.Schedule(200);
			Assert.IsTrue(scheduledAction.IsScheduled, "Action should be scheduled.");
			Thread.Sleep(400);
			Assert.IsTrue(executed, "Action should be executed.");
			Assert.IsFalse(scheduledAction.IsScheduled, "Action should not be scheduled.");
			Assert.GreaterOrEqual(executionTime - schedulingTime, 200, "Action executed too early.");

			// schedule twice
			executed = false;
			schedulingTime = stopWatch.ElapsedMilliseconds;
			scheduledAction.Schedule(400);
			Assert.IsTrue(scheduledAction.IsScheduled, "Action should be scheduled.");
			scheduledAction.Schedule(200);
			Thread.Sleep(600);
			Assert.IsTrue(executed, "Action should be executed.");
			Assert.IsFalse(scheduledAction.IsScheduled, "Action should not be scheduled.");
			Assert.GreaterOrEqual(executionTime - schedulingTime, 400, "Action executed too early.");

			// reschedule
			executed = false;
			schedulingTime = stopWatch.ElapsedMilliseconds;
			scheduledAction.Schedule(200);
			Assert.IsTrue(scheduledAction.IsScheduled, "Action should be scheduled.");
			Thread.Sleep(100);
			scheduledAction.Reschedule(400);
			Assert.IsTrue(scheduledAction.IsScheduled, "Action should be scheduled.");
			Thread.Sleep(300);
			Assert.IsFalse(executed, "Action should not be executed.");
			Thread.Sleep(300);
			Assert.IsTrue(executed, "Action should be executed.");
			Assert.IsFalse(scheduledAction.IsScheduled, "Action should not be scheduled.");
			Assert.GreaterOrEqual(executionTime - schedulingTime, 500, "Action executed too early.");
		}
	}
}
