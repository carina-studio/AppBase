using CarinaStudio.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;

namespace CarinaStudio
{
	/// <summary>
	/// Test implementation of <see cref="IApplication"/>.
	/// </summary>
	class TestApplication : IApplication
	{
		// Static fields.
		static volatile TestApplication? current;


		// Fields.
		readonly Thread thread;


		// Constructor.
		TestApplication()
		{
			this.LoggerFactory = new LoggerFactory(new ILoggerProvider[] { TestLoggerProvider.Default });
			this.thread = Thread.CurrentThread;
			this.Settings = new TestSettings();
			this.SynchronizationContext = SynchronizationContext.Current ?? throw new InvalidOperationException("No SynchronizationContext on current thread.");
		}


		/// <summary>
		/// Get current instance.
		/// </summary>
		public static TestApplication Current { get => current ?? throw new InvalidOperationException("Application is not ready."); }


		/// <summary>
		/// Setup application instance if needed.
		/// </summary>
		public static void Setup()
		{
			if (current != null)
				return;
			lock (typeof(TestApplication))
			{
				if (current != null)
					return;
				current = new TestApplication();
			}
		}


		// Implementations.
		public bool CheckAccess() => Thread.CurrentThread == this.thread;
		public ILoggerFactory LoggerFactory { get; }
		public BaseSettings Settings { get; }
		public SynchronizationContext SynchronizationContext { get; }
	}
}
