﻿using CarinaStudio.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace CarinaStudio
{
	/// <summary>
	/// Test implementation of <see cref="IApplication"/>.
	/// </summary>
	class TestApplication : IApplication
	{
		// String key.
		public const string FormatString = "Test string {0}";
		public const string FormatStringKey = "FormatString";
		public const string InvalidStringKey = "Invalid";


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
		public string? GetString(string key, string? defaultValue = null) => key switch
		{
			FormatStringKey => FormatString,
			_ => defaultValue,
		};
		public bool IsShutdownStarted => false;
		public ILoggerFactory LoggerFactory { get; }
		public event PropertyChangedEventHandler? PropertyChanged;
		public string RootPrivateDirectoryPath => Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName).AsNonNull();
		public BaseSettings Settings { get; }
		public SynchronizationContext SynchronizationContext { get; }
	}
}