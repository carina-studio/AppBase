using NUnit.Framework;
using System;
using System.Threading;

namespace CarinaStudio
{
	/// <summary>
	/// Tests of <see cref="BaseShareableDisposable{TSelf}"/>.
	/// </summary>
	[TestFixture]
	class BaseShareableDisposableTests
	{
		// Test implementation of BaseShareableDisposable.
		class TestShareableDisposable : BaseShareableDisposable<TestShareableDisposable>
		{
			// Test implementation of resource holder.
			public class ResourceHolder : BaseResourceHolder
			{
				// Fields.
				public volatile bool IsResourceReleased;

				// Release.
				protected override void Release()
				{
					this.IsResourceReleased = true;
				}
			}

			// Constructor.
			public TestShareableDisposable() : base(new ResourceHolder())
			{ }
			TestShareableDisposable(ResourceHolder resourceHolder) : base(resourceHolder)
			{ }

			// Get resource holder.
			public ResourceHolder GetResourceHolder() => this.GetResourceHolder<ResourceHolder>();

			// Share.
			protected override TestShareableDisposable Share(BaseResourceHolder resourceHolder) => new TestShareableDisposable((ResourceHolder)resourceHolder);
		}


		// Fields.
		readonly Random random = new Random();


		/// <summary>
		/// Test for accessing instance after disposing.
		/// </summary>
		[Test]
		public void AccessingAfterDisposingTest()
		{
			var instance = new TestShareableDisposable();
			var resourceHolder = instance.GetResourceHolder();
			Assert.IsFalse(resourceHolder.IsResourceReleased, "Resource should not be released.");
			instance.Dispose();
			Assert.IsTrue(resourceHolder.IsResourceReleased, "Resource should be released.");
			try
			{
				instance.GetResourceHolder();
				throw new AssertionException("Instance should not be accessible after disposing.");
			}
			catch (AssertionException)
			{
				throw;
			}
			catch
			{ }
			try
			{
				instance.Share();
				throw new AssertionException("Instance should not be shared after disposing.");
			}
			catch (AssertionException)
			{
				throw;
			}
			catch
			{ }
		}


		/// <summary>
		/// Test for sharing instances on multiple threads.
		/// </summary>
		[Test]
		public void SharingOnMultiThreadsTest()
		{
			// build base instance
			var baseInstance = new TestShareableDisposable();
			var resourceHolder = baseInstance.GetResourceHolder();
			Assert.IsFalse(resourceHolder.IsResourceReleased, "Resource should not be released.");

			// share instance on multiple threads
			var syncLock = new object();
			var completedCount = 0;
			var exception = (Exception?)null;
			lock (syncLock)
			{
				for (int i = 0; i < 100; ++i)
				{
					ThreadPool.QueueUserWorkItem((_) =>
					{
						try
						{
							using (var sharedInstance = baseInstance.Share())
							{
								Assert.IsFalse(resourceHolder.IsResourceReleased, "Resource should not be released after sharing instance.");
								Thread.Sleep(this.random.Next(10, 100));
							}
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
					});
				}
				Assert.IsTrue(Monitor.Wait(syncLock, 10000), "Cannot complete waiting for sharing instance on multi-threads.");
			}
			if (exception != null)
				throw exception;

			// dispose base instance
			Assert.IsFalse(resourceHolder.IsResourceReleased, "Resource should not be released before disposing all instances.");
			baseInstance.Dispose();
			Assert.IsTrue(resourceHolder.IsResourceReleased, "Resource should be released after disposing all instances.");
		}


		/// <summary>
		/// Test for sharing instances on single thread.
		/// </summary>
		[Test]
		public void SharingOnSingleThreadTest()
		{
			// build base instance
			var baseInstance = new TestShareableDisposable();
			var resourceHolder = baseInstance.GetResourceHolder();
			Assert.IsFalse(resourceHolder.IsResourceReleased, "Resource should not be released.");

			// share
			var sharedInstance = baseInstance.Share();
			Assert.AreNotSame(baseInstance, sharedInstance, "Shared instance should not be same as base instance.");
			Assert.AreSame(resourceHolder, sharedInstance.GetResourceHolder(), "Resource holder of shared instance should be same as base instance.");

			// dispose shared instance
			sharedInstance.Dispose();
			Assert.IsFalse(resourceHolder.IsResourceReleased, "Resource should not be released after disposing shared instance.");

			// share and dispose base instance
			sharedInstance = baseInstance.Share();
			baseInstance.Dispose();
			Assert.IsFalse(resourceHolder.IsResourceReleased, "Resource should not be released before disposing all instances.");

			// dispose shared instance
			sharedInstance.Dispose();
			Assert.IsTrue(resourceHolder.IsResourceReleased, "Resource should be released after disposing all instances.");
		}
	}
}
