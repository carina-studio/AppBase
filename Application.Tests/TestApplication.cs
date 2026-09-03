using CarinaStudio.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel;
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
		readonly Thread thread = Thread.CurrentThread;


		// Implementations.
		public Assembly Assembly => Assembly.GetExecutingAssembly();
		public bool CheckAccess() => Thread.CurrentThread == this.thread;
		public CultureInfo CultureInfo => CultureInfo.CurrentCulture;
		public IObservable<string?> GetObservableString(string key) =>
			new FixedObservableValue<string?>(this.GetString(key));
		public string? GetString(string key, string? defaultValue = null) => key switch
		{
			FormatStringKey => FormatString,
			_ => defaultValue,
		};
		public bool IsShutdownStarted => false;
		public ILoggerFactory LoggerFactory { get; } = new LoggerFactory([ TestLoggerProvider.Default ]);
		public event PropertyChangedEventHandler? PropertyChanged // Properties of TestApplication are all fixed, so the event is never raised.
		{
			add { }
			remove { }
		}
		public ISettings PersistentState { get; } = new MemorySettings();
		public string RootPrivateDirectoryPath => Path.GetTempPath();
		public ISettings Settings { get; } = new TestSettings();
		public event EventHandler? StringsUpdated // Strings of TestApplication are all fixed, so the event is never raised.
		{
			add { }
			remove { }
		}
		public SynchronizationContext SynchronizationContext { get; } = SynchronizationContext.Current ?? throw new InvalidOperationException("No SynchronizationContext on current thread.");
	}
}
