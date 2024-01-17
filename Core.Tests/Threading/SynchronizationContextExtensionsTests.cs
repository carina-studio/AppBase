using NUnit.Framework;
using System;
using System.Diagnostics;
using System.Threading;

namespace CarinaStudio.Threading
{
	/// <summary>
	/// Tests of <see cref="SynchronizationContextExtensions"/>.
	/// </summary>
	[TestFixture]
	class SynchronizationContextExtensionsTests
	{
		// Fields.
		readonly Random random = new Random();


		/// <summary>
		/// Test for using CancelDelayed().
		/// </summary>
		[Test]
		public void CancelDelayedTest()
		{
			// prepare
			var syncContext = new SynchronizationContext();
			var anotherSyncContext = new SynchronizationContext();
			var stopWatch = new Stopwatch().Also((it) => it.Start());

			// cancel before execution
			var executed = false;
			var token = syncContext.PostDelayed(() =>
			{
				executed = true;
			}, 500);
			Thread.Sleep(200);
			Assert.IsTrue(syncContext.CancelDelayed(token), "Call-back cancellation should be successful.");
			Thread.Sleep(500);
			Assert.IsFalse(executed, "Call-back should not be executed after cancellation.");
			Assert.IsFalse(syncContext.CancelDelayed(token), "Call-back cancellation should be failed.");

			// cancel after execution
			executed = false;
			token = syncContext.PostDelayed(() =>
			{
				executed = true;
			}, 200);
			Thread.Sleep(500);
			Assert.IsTrue(executed, "Call-back should be executed.");
			Assert.IsFalse(syncContext.CancelDelayed(token), "Call-back cancellation should be failed after execution.");

			// cancel by another synchronization context
			executed = false;
			token = syncContext.PostDelayed(() =>
			{
				executed = true;
			}, 200);
			Assert.IsFalse(anotherSyncContext.CancelDelayed(token), "Call-back cancellation should be failed by another synchronization context.");
			Thread.Sleep(500);
			Assert.IsTrue(executed, "Call-back should be executed.");
		}


		/// <summary>
		/// Test for using PostDelayed().
		/// </summary>
		[Test]
		public void PostDelayedTest()
		{
			// prepare
			var syncContext = new SynchronizationContext();
			var stopWatch = new Stopwatch().Also((it) => it.Start());

			// post without delay time
			var syncLock = new object();
			var postTime = stopWatch.ElapsedMilliseconds;
			var executionTime = 0L;
			lock (syncLock)
			{
				syncContext.PostDelayed(() =>
				{
					executionTime = stopWatch.ElapsedMilliseconds;
					lock (syncLock)
					{
						Monitor.Pulse(syncLock);
					}
				}, 0);
				Assert.IsTrue(Monitor.Wait(syncLock, 1000), "Posted call-back not executed.");
				Assert.GreaterOrEqual(executionTime - postTime, 0, "Call-back executed too early.");
				Assert.LessOrEqual(executionTime - postTime, 200, "Call-back executed too late.");
			}

			// post with delayed time on multi-threads
			var completedCount = 0;
			var exception = (Exception?)null;
			lock (syncLock)
			{
				for (var i = 0; i < 100; ++i)
				{
					ThreadPool.QueueUserWorkItem((_) =>
					{
						var delayedTime = this.random.Next(10, 500);
						var localPostTime = stopWatch.ElapsedMilliseconds;
						syncContext.PostDelayed(() =>
						{
							try
							{
								var actualDelayedTime = (stopWatch.ElapsedMilliseconds - localPostTime);
								Assert.GreaterOrEqual(actualDelayedTime, delayedTime - 1, "Call-back executed too early.");
								Assert.LessOrEqual(actualDelayedTime, delayedTime + 100, "Call-back executed too late.");
							}
							catch (Exception ex)
							{
								exception = ex;
							}
							finally
							{
								lock (syncLock)
								{
									++completedCount;
									if (completedCount == 100 || exception != null)
										Monitor.Pulse(syncLock);
								}
							}
						}, delayedTime);
					});
				}
				Assert.IsTrue(Monitor.Wait(syncLock, 60000), "Unable to complete waiting for posting delayed call-back on multi-threads.");
			}

			// test on SingleThreadSynchronizationContext
			using var singleThreadSyncContext = new SingleThreadSynchronizationContext();

			// post delayed call-back which will be executed after disposing sync context
			var executed = false;
			var postToken = singleThreadSyncContext.PostDelayed(() => executed = true, 1000);
			singleThreadSyncContext.Dispose();
			Thread.Sleep(2000);
			Assert.IsFalse(executed);

			// cancel delayed call-back after disposing sync context
			Assert.IsFalse(singleThreadSyncContext.CancelDelayed(postToken));
		}
	}
}
