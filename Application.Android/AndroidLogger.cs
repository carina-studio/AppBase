using Android.Util;
using Microsoft.Extensions.Logging;
using System;

namespace CarinaStudio.Android;

/// <summary>
/// Implementation of <see cref="ILogger"/> based-on android logger.
/// </summary>
class AndroidLogger : ILogger
{
    // Fields.
    readonly string tag;


    // Constructor.
    public AndroidLogger(string tag) =>
        this.tag = tag;
    

    /// <inheritdoc/>
    public IDisposable BeginScope<TState>(TState state) =>
        new EmptyDisposable();
    

    // Convert log level to priority.
    static LogPriority ConvertToLogPriority(LogLevel logLevel) => logLevel switch
    {
        LogLevel.Critical
        or LogLevel.Error => LogPriority.Error,
        LogLevel.Warning => LogPriority.Warn,
        LogLevel.Debug => LogPriority.Debug,
        LogLevel.Information => LogPriority.Info,
        _ => LogPriority.Verbose,
    };
    

    /// <inheritdoc/>
    public bool IsEnabled(LogLevel logLevel)
    {
        if (logLevel == LogLevel.None)
            return false;
        return global::Android.Util.Log.IsLoggable(this.tag, ConvertToLogPriority(logLevel));
    }


    /// <inheritdoc/>
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (logLevel == LogLevel.None)
            return;
        global::Android.Util.Log.WriteLine(ConvertToLogPriority(logLevel), this.tag, formatter(state, exception));
    }
}


/// <summary>
/// Implementation of <see cref="ILoggerProvider"/>.
/// </summary>
class AndroidLoggerProvider : ILoggerProvider
{
    /// <inheritdoc/>
    public ILogger CreateLogger(string categoryName) =>
        new AndroidLogger(categoryName);
    

    /// <inheritdoc/>
    void IDisposable.Dispose()
    { }
}