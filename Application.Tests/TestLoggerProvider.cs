using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace CarinaStudio
{
	/// <summary>
	/// Test implementation of <see cref="ILoggerProvider"/>.
	/// </summary>
	class TestLoggerProvider : ILoggerProvider
	{
		/// <summary>
		/// Default instance.
		/// </summary>
		public static readonly TestLoggerProvider Default = new TestLoggerProvider();


		// Fields.
		readonly List<TestLogger.LogItem> logItems = new List<TestLogger.LogItem>();


		// Constructor.
		TestLoggerProvider()
		{
			this.LogItems = this.logItems.AsReadOnly();
		}


		// Create logger.
		public ILogger CreateLogger(string categoryName) => new TestLogger(this, categoryName);


		// Dispose.
		public void Dispose()
		{
			lock (this.logItems)
				this.logItems.Clear();
		}


		// Add new log.
		public void Log(TestLogger.LogItem logItem)
		{
			lock (this.logItems)
				this.logItems.Add(logItem);
		}


		/// <summary>
		/// Get logged items.
		/// </summary>
		public IList<TestLogger.LogItem> LogItems { get; }
	}
}
