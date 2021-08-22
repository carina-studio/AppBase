using CarinaStudio.Threading;
using NUnit.Framework;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CarinaStudio.AutoUpdate
{
	/// <summary>
	/// Base implementations of tests of <see cref="Updater"/> and <see cref="IUpdaterComponent"/>.
	/// </summary>
	abstract class BaseTests
	{
		// Fields.
		IApplication? application;
		SingleThreadSynchronizationContext? applicationSyncContext;


		/// <summary>
		/// Get <see cref="IApplication"/> instance for testing.
		/// </summary>
		protected IApplication Application { get => this.application.AsNonNull(); }


		/// <summary>
		/// Release <see cref="IApplication"/> for testing.
		/// </summary>
		[OneTimeTearDown]
		public void ReleaseApplication()
		{
			this.applicationSyncContext?.Dispose();
		}


		/// <summary>
		/// Setup <see cref="IApplication"/> instance for testing.
		/// </summary>
		[OneTimeSetUp]
		public void SetupApplication()
		{
			this.applicationSyncContext = new SingleThreadSynchronizationContext().Also(it =>
			{
				it.Send(() =>
				{
					this.application = new TestApplication();
				});
			});
		}


		/// <summary>
		/// Run testing on thread of application.
		/// </summary>
		/// <param name="action">Asynchronous test action.</param>
		protected void TestOnApplicationThread(Action action)
		{
			this.applicationSyncContext.AsNonNull().Send(action);
		}


		/// <summary>
		/// Run testing on thread of application.
		/// </summary>
		/// <param name="action">Asynchronous test action.</param>
		protected void TestOnApplicationThread(Func<Task> action)
		{
			var syncLock = new object();
			lock (syncLock)
			{
				this.applicationSyncContext.AsNonNull().Post(async () =>
				{
					await action();
					lock (syncLock)
					{
						Monitor.Pulse(syncLock);
					}
				});
				Monitor.Wait(syncLock);
			}
		}
	}
}
