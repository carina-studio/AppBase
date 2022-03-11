using CarinaStudio.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading;

namespace CarinaStudio
{
	/// <summary>
	/// Test implementation of <see cref="IApplication"/>.
	/// </summary>
	public class TestApplication : IApplication
	{
		// String key.
		internal const string FormatString = "Test string {0}";
		internal const string FormatStringKey = "FormatString";
		internal const string InvalidStringKey = "Invalid";


		// Fields.
		readonly Thread thread;


		// Constructor.
		public TestApplication()
		{
			this.LoggerFactory = new LoggerFactory(new ILoggerProvider[] { TestLoggerProvider.Default });
			this.thread = Thread.CurrentThread;
			this.PersistentState = new MemorySettings();
			this.Settings = new TestSettings();
			this.SynchronizationContext = SynchronizationContext.Current ?? throw new InvalidOperationException("No SynchronizationContext on current thread.");
		}


		// Implementations.
		public Assembly Assembly => Assembly.GetExecutingAssembly();
		public bool CheckAccess() => Thread.CurrentThread == this.thread;
		public CultureInfo CultureInfo => CultureInfo.CurrentCulture;
		public string? GetString(string key, string? defaultValue = null) => key switch
		{
			FormatStringKey => FormatString,
			_ => defaultValue,
		};
		public bool IsShutdownStarted => false;
		public ILoggerFactory LoggerFactory { get; }
		public event PropertyChangedEventHandler? PropertyChanged;
		public ISettings PersistentState { get; }
		public string RootPrivateDirectoryPath => Path.GetTempPath();
		public ISettings Settings { get; }
		public event EventHandler? StringsUpdated;
		public SynchronizationContext SynchronizationContext { get; }
	}
}
