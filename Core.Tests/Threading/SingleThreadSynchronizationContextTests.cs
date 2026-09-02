using NUnit.Framework;
using System;
using System.Threading;

namespace CarinaStudio.Threading
{
	/// <summary>
	/// Tests of <see cref="SingleThreadSynchronizationContext"/>.
	/// </summary>
	[TestFixture]
	class SingleThreadSynchronizationContextTests
	{
		/// <summary>
		/// Test for disposing.
		/// </summary>
		[Test]
		public void DisposingTest()
		{
			// create instance
			var syncContext = new SingleThreadSynchronizationContext();
			Assert.That(syncContext.ExecutionThread.IsAlive, "Execution thread should be alive.");

			// dispose
			syncContext.Dispose();
			Thread.Sleep(500);
			Assert.That(syncContext.IsDisposed, "Instance should be disposed.");
			Assert.That(!syncContext.ExecutionThread.IsAlive, "Execution thread should not be alive after disposing.");

			// post after disposing, the call-back should be dropped instead of throwing, because the runtime also posts
			// await continuations through this method and cannot handle an exception from it
			var isCallbackExecuted = false;
			Assert.DoesNotThrow(() => syncContext.Post(_ => isCallbackExecuted = true, null), "Posting after disposing should not throw.");
			Thread.Sleep(500);
			Assert.That(!isCallbackExecuted, "Call-back posted after disposing should not be executed.");

			// send after disposing, the caller waits for the call-back so it must be told instead of blocking forever
			Assert.Throws<ObjectDisposedException>(() => syncContext.Send(_ => { }, null), "Sending after disposing should throw.");
		}


		/// <summary>
		/// Test for posting call-backs.
		/// </summary>
		[Test]
		public void PostingTest()
		{
			// prepare
			using var syncContext = new SingleThreadSynchronizationContext();

			// check call-back execution thread
			var executionThread = (Thread?)null;
			syncContext.Post(() =>
			{
				executionThread = Thread.CurrentThread;
			});
			Thread.Sleep(100);
			Assert.That(ReferenceEquals(syncContext.ExecutionThread, executionThread), "Call-back should be executed on execution thread.");

			// post call-backs continuously from multiple threads.
			var syncLock = new object();
			var nextPostingId = 1;
			var expectedPostingId = 1;
			var exception = (Exception?)null;
			lock (syncLock)
			{
				for (var i = 0; i < 100; ++i)
				{
					ThreadPool.QueueUserWorkItem(_ =>
					{
						lock (syncLock)
						{
							var postingId = nextPostingId++;
							syncContext.Post(() =>
							{
								try
								{
									Assert.That(expectedPostingId == postingId, "Incorrect call-back execution order.");
									Thread.Sleep(10);
									++expectedPostingId;
								}
								catch (Exception ex)
								{
									exception = ex;
								}
								finally
								{
									if (postingId == 100 || exception != null)
									{
										lock (syncLock)
											Monitor.Pulse(syncLock);
									}
								}
							});
						}
					});
				}
				Assert.That(Monitor.Wait(syncLock, 10000), "Cannot complete waiting for all call-backs.");
				if (exception != null)
					throw exception;
			}
		}


		/// <summary>
		/// Test for sending call-backs.
		/// </summary>
		[Test]
		public void SendingTest()
		{
			// prepare
			using var syncContext = new SingleThreadSynchronizationContext();

			// send call-back
			var executionThread = (Thread?)null;
			syncContext.Send(() =>
			{
				Thread.Sleep(100);
				executionThread = Thread.CurrentThread;
			});
			Assert.That(ReferenceEquals(syncContext.ExecutionThread, executionThread), "Call-back should be executed on execution thread.");
		}
	}
}
