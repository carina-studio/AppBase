using Microsoft.Extensions.Logging;
using System;

namespace CarinaStudio
{
	/// <summary>
	/// Test implementation of <see cref="ILogger"/>.
	/// </summary>
	class TestLogger : ILogger
	{
		/// <summary>
		/// Single logged item.
		/// </summary>
		public class LogItem
		{
			// Fields.
			public string CategoryName;
			public EventId EventId;
			public LogLevel Level;
			public string Message;

			// Constructor.
			public LogItem(string categoryName, LogLevel level, EventId eventId, string message)
			{
				this.CategoryName = categoryName;
				this.Message = message;
				this.Level = level;
				this.EventId = eventId;
			}
		}


		// Implementation of Scope.
		class Scope : IDisposable
		{
			public void Dispose()
			{ }
		}


		// Fields.
		readonly string categoryName;
		readonly TestLoggerProvider provider;


		// Constructor.
		public TestLogger(TestLoggerProvider provider, string categoryName)
		{
			this.provider = provider;
			this.categoryName = categoryName;
		}


		// Implementations.
		public IDisposable BeginScope<TState>(TState state) => new Scope();
		public bool IsEnabled(LogLevel logLevel) => true;
		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
		{
			var message = formatter(state, exception);
			this.provider.Log(new LogItem(this.categoryName, logLevel, eventId, message));
		}
	}
}
