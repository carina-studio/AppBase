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

			// canecl after execution
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
			var syncContext = new SynchronizationContext();
			var executed = false;
			var scheduledAction = new ScheduledAction(syncContext, () =>
			{
				executed = true;
			});
			var prevSyncContext = SynchronizationContext.Current;
			SynchronizationContext.SetSynchronizationContext(syncContext);

			// test
			try
			{
				// execute
				scheduledAction.Execute();
				Assert.IsTrue(executed, "Action should be executed.");

				// execute after scheduling
				executed = false;
				scheduledAction.Schedule(200);
				Assert.IsTrue(scheduledAction.IsScheduled, "Action should be scheduled.");
				scheduledAction.Execute();
				Assert.IsTrue(executed, "Action should be executed.");
				Assert.IsFalse(scheduledAction.IsScheduled, "Action should not be scheduled.");

				// execute on other thread
				executed = false;
				var exception = (Exception?)null;
				ThreadPool.QueueUserWorkItem((_) =>
				{
					try
					{
						scheduledAction.Execute();
						throw new AssertionException("Should not execute on another thread.");
					}
					catch (InvalidOperationException)
					{ }
					catch (Exception ex)
					{
						exception = ex;
					}
				});
				Thread.Sleep(200);
				if (exception != null)
					throw exception;
				Assert.IsFalse(executed, "Action should not be executed.");

				// execute before scheduling
				Assert.IsFalse(scheduledAction.ExecuteIfScheduled(), "Should not execute before scheduling.");
				Assert.IsFalse(executed, "Action should not be executed.");

				// execute after scheduling
				scheduledAction.Schedule(200);
				Assert.IsTrue(scheduledAction.IsScheduled, "Action should be scheduled.");
				Assert.IsTrue(scheduledAction.ExecuteIfScheduled(), "Should execute after scheduling.");
				Assert.IsTrue(executed, "Action should be executed.");
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
						throw new AssertionException("Should not execute on another thread.");
					}
					catch (InvalidOperationException)
					{ }
					catch (Exception ex)
					{
						exception = ex;
					}
				});
				Thread.Sleep(400);
				if (exception != null)
					throw exception;
				Assert.IsTrue(executed, "Action should be executed.");
			}
			finally
			{
				SynchronizationContext.SetSynchronizationContext(prevSyncContext);
			}
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
