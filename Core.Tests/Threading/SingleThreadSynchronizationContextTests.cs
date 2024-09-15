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
			Assert.That(!syncContext.ExecutionThread.IsAlive, "Execution thread should not be alive after disposing.");

			// access after disposing
			try
			{
				syncContext.Post(_ => { }, null);
				throw new AssertionException("Instance should not be accessible after disposing.");
			}
			catch (AssertionException)
			{
				throw;
			}
			// ReSharper disable once EmptyGeneralCatchClause
			catch
			{ }
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
