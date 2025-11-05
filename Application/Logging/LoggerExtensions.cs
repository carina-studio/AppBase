using Microsoft.Extensions.Logging;
using System;
using System.Runtime.CompilerServices;

namespace CarinaStudio.Logging;

#if NET9_0_OR_GREATER
/// <summary>
/// Extension methods for <see cref="ILogger"/>.
/// </summary>
public static class LoggerExtensions
{
    /// <summary>
    /// Format and write a log message at the specified log level.
    /// </summary>
    /// <param name="logger"><see cref="ILogger"/>.</param>
    /// <param name="level">Log level.</param>
    /// <param name="eventId">Event ID.</param>
    /// <param name="exception">Exception.</param>
    /// <param name="message">Format of log message.</param>
    /// <param name="args">Arguments.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Log(this ILogger logger, LogLevel level, EventId eventId, Exception? exception, string message, params ReadOnlySpan<object?> args)
    {
        if (logger.IsEnabled(level))
            Microsoft.Extensions.Logging.LoggerExtensions.Log(logger, level, eventId, exception, message, args.ToArray());
    }
    
    
    /// <summary>
    /// Format and write a log message at <see cref="LogLevel.Critical"/> level.
    /// </summary>
    /// <param name="logger"><see cref="ILogger"/>.</param>
    /// <param name="message">Format of log message.</param>
    /// <param name="args">Arguments.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void LogCritical(this ILogger logger, string message, params ReadOnlySpan<object?> args) =>
        Log(logger, LogLevel.Critical, 0, null, message, args);
    
    
    /// <summary>
    /// Format and write a log message at <see cref="LogLevel.Critical"/> level.
    /// </summary>
    /// <param name="logger"><see cref="ILogger"/>.</param>
    /// <param name="eventId">Event ID.</param>
    /// <param name="message">Format of log message.</param>
    /// <param name="args">Arguments.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void LogCritical(this ILogger logger, EventId eventId, string message, params ReadOnlySpan<object?> args) =>
        Log(logger, LogLevel.Critical, eventId, null, message, args);
    
    
    /// <summary>
    /// Format and write a log message at <see cref="LogLevel.Critical"/> level.
    /// </summary>
    /// <param name="logger"><see cref="ILogger"/>.</param>
    /// <param name="exception">Exception.</param>
    /// <param name="message">Format of log message.</param>
    /// <param name="args">Arguments.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void LogCritical(this ILogger logger, Exception? exception, string message, params ReadOnlySpan<object?> args) =>
        Log(logger, LogLevel.Critical, 0, exception, message, args);
    
    
    /// <summary>
    /// Format and write a log message at <see cref="LogLevel.Error"/> level.
    /// </summary>
    /// <param name="logger"><see cref="ILogger"/>.</param>
    /// <param name="message">Format of log message.</param>
    /// <param name="args">Arguments.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void LogError(this ILogger logger, string message, params ReadOnlySpan<object?> args) =>
        Log(logger, LogLevel.Error, 0, null, message, args);
    
    
    /// <summary>
    /// Format and write a log message at <see cref="LogLevel.Error"/> level.
    /// </summary>
    /// <param name="logger"><see cref="ILogger"/>.</param>
    /// <param name="eventId">Event ID.</param>
    /// <param name="message">Format of log message.</param>
    /// <param name="args">Arguments.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void LogError(this ILogger logger, EventId eventId, string message, params ReadOnlySpan<object?> args) =>
        Log(logger, LogLevel.Error, eventId, null, message, args);
    
    
    /// <summary>
    /// Format and write a log message at <see cref="LogLevel.Error"/> level.
    /// </summary>
    /// <param name="logger"><see cref="ILogger"/>.</param>
    /// <param name="exception">Exception.</param>
    /// <param name="message">Format of log message.</param>
    /// <param name="args">Arguments.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void LogError(this ILogger logger, Exception? exception, string message, params ReadOnlySpan<object?> args) =>
        Log(logger, LogLevel.Error, 0, exception, message, args);


    /// <summary>
    /// Format and write a log message at <see cref="LogLevel.Debug"/> level.
    /// </summary>
    /// <param name="logger"><see cref="ILogger"/>.</param>
    /// <param name="message">Format of log message.</param>
    /// <param name="args">Arguments.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void LogDebug(this ILogger logger, string message, params ReadOnlySpan<object?> args) =>
        Log(logger, LogLevel.Debug, 0, null, message, args);
    
    
    /// <summary>
    /// Format and write a log message at <see cref="LogLevel.Debug"/> level.
    /// </summary>
    /// <param name="logger"><see cref="ILogger"/>.</param>
    /// <param name="eventId">Event ID.</param>
    /// <param name="message">Format of log message.</param>
    /// <param name="args">Arguments.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void LogDebug(this ILogger logger, EventId eventId, string message, params ReadOnlySpan<object?> args) =>
        Log(logger, LogLevel.Debug, eventId, null, message, args);
    
    
    /// <summary>
    /// Format and write a log message at <see cref="LogLevel.Debug"/> level.
    /// </summary>
    /// <param name="logger"><see cref="ILogger"/>.</param>
    /// <param name="exception">Exception.</param>
    /// <param name="message">Format of log message.</param>
    /// <param name="args">Arguments.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void LogDebug(this ILogger logger, Exception? exception, string message, params ReadOnlySpan<object?> args) =>
        Log(logger, LogLevel.Debug, 0, exception, message, args);
    
    
    /// <summary>
    /// Format and write a log message at <see cref="LogLevel.Information"/> level.
    /// </summary>
    /// <param name="logger"><see cref="ILogger"/>.</param>
    /// <param name="message">Format of log message.</param>
    /// <param name="args">Arguments.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void LogInformation(this ILogger logger, string message, params ReadOnlySpan<object?> args) =>
        Log(logger, LogLevel.Information, 0, null, message, args);
    
    
    /// <summary>
    /// Format and write a log message at <see cref="LogLevel.Information"/> level.
    /// </summary>
    /// <param name="logger"><see cref="ILogger"/>.</param>
    /// <param name="eventId">Event ID.</param>
    /// <param name="message">Format of log message.</param>
    /// <param name="args">Arguments.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void LogInformation(this ILogger logger, EventId eventId, string message, params ReadOnlySpan<object?> args) =>
        Log(logger, LogLevel.Information, eventId, null, message, args);
    
    
    /// <summary>
    /// Format and write a log message at <see cref="LogLevel.Information"/> level.
    /// </summary>
    /// <param name="logger"><see cref="ILogger"/>.</param>
    /// <param name="exception">Exception.</param>
    /// <param name="message">Format of log message.</param>
    /// <param name="args">Arguments.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void LogInformation(this ILogger logger, Exception? exception, string message, params ReadOnlySpan<object?> args) =>
        Log(logger, LogLevel.Information, 0, exception, message, args);
    
    
    /// <summary>
    /// Format and write a log message at <see cref="LogLevel.Trace"/> level.
    /// </summary>
    /// <param name="logger"><see cref="ILogger"/>.</param>
    /// <param name="message">Format of log message.</param>
    /// <param name="args">Arguments.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void LogTrace(this ILogger logger, string message, params ReadOnlySpan<object?> args) =>
        Log(logger, LogLevel.Trace, 0, null, message, args);
    
    
    /// <summary>
    /// Format and write a log message at <see cref="LogLevel.Trace"/> level.
    /// </summary>
    /// <param name="logger"><see cref="ILogger"/>.</param>
    /// <param name="eventId">Event ID.</param>
    /// <param name="message">Format of log message.</param>
    /// <param name="args">Arguments.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void LogTrace(this ILogger logger, EventId eventId, string message, params ReadOnlySpan<object?> args) =>
        Log(logger, LogLevel.Trace, eventId, null, message, args);
    
    
    /// <summary>
    /// Format and write a log message at <see cref="LogLevel.Trace"/> level.
    /// </summary>
    /// <param name="logger"><see cref="ILogger"/>.</param>
    /// <param name="exception">Exception.</param>
    /// <param name="message">Format of log message.</param>
    /// <param name="args">Arguments.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void LogTrace(this ILogger logger, Exception? exception, string message, params ReadOnlySpan<object?> args) =>
        Log(logger, LogLevel.Trace, 0, exception, message, args);
    
    
    /// <summary>
    /// Format and write a log message at <see cref="LogLevel.Warning"/> level.
    /// </summary>
    /// <param name="logger"><see cref="ILogger"/>.</param>
    /// <param name="message">Format of log message.</param>
    /// <param name="args">Arguments.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void LogWarning(this ILogger logger, string message, params ReadOnlySpan<object?> args) =>
        Log(logger, LogLevel.Warning, 0, null, message, args);
    
    
    /// <summary>
    /// Format and write a log message at <see cref="LogLevel.Warning"/> level.
    /// </summary>
    /// <param name="logger"><see cref="ILogger"/>.</param>
    /// <param name="eventId">Event ID.</param>
    /// <param name="message">Format of log message.</param>
    /// <param name="args">Arguments.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void LogWarning(this ILogger logger, EventId eventId, string message, params ReadOnlySpan<object?> args) =>
        Log(logger, LogLevel.Warning, eventId, null, message, args);
    
    
    /// <summary>
    /// Format and write a log message at <see cref="LogLevel.Warning"/> level.
    /// </summary>
    /// <param name="logger"><see cref="ILogger"/>.</param>
    /// <param name="exception">Exception.</param>
    /// <param name="message">Format of log message.</param>
    /// <param name="args">Arguments.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void LogWarning(this ILogger logger, Exception? exception, string message, params ReadOnlySpan<object?> args) =>
        Log(logger, LogLevel.Warning, 0, exception, message, args);
}
#endif