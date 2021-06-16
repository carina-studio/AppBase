using CarinaStudio.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel;
using System.Threading;

namespace CarinaStudio
{
	/// <summary>
	/// Test implementation of <see cref="IApplication"/>.
	/// </summary>
	class TestApplication : IApplication
	{
		// Fields.
		readonly Thread thread;


		// Constructor.
		public TestApplication()
		{
			this.LoggerFactory = new LoggerFactory(new ILoggerProvider[] { TestLoggerProvider.Default });
			this.thread = Thread.CurrentThread;
			this.Settings = new TestSettings();
			this.SynchronizationContext = SynchronizationContext.Current ?? throw new InvalidOperationException("No SynchronizationContext on current thread.");
		}


		// Implementations.
		public bool CheckAccess() => Thread.CurrentThread == this.thread;
		public ILoggerFactory LoggerFactory { get; }
		public event PropertyChangedEventHandler? PropertyChanged;
		public BaseSettings Settings { get; }
		public SynchronizationContext SynchronizationContext { get; }
	}
}
