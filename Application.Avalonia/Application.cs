using Avalonia.Controls;
using Avalonia.Styling;
using CarinaStudio.Configuration;
using CarinaStudio.Threading;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading;

namespace CarinaStudio;

/// <summary>
/// Base implementation of <see cref="IApplication"/> based-on <see cref="Avalonia.Application"/>.
/// </summary>
public abstract class Application : Avalonia.Application, IAvaloniaApplication
{
	// Fields.
	PropertyChangedEventHandler? propertyChangedHandlers;
#if !NET10_0_OR_GREATER
	readonly string? rootPrivateDirPath;
#endif
	volatile DispatcherSynchronizationContext? synchronizationContext;


	/// <summary>
	/// Initialize new <see cref="Application"/> instance.
	/// </summary>
	protected Application()
	{
#if !NET10_0_OR_GREATER
		this.rootPrivateDirPath = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule?.FileName);
#endif
	}


	/// <summary>
	/// Get <see cref="Assembly"/> of application.
	/// </summary>
	[ThreadSafe]
	public virtual Assembly Assembly { get; } = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();


	/// <summary>
	/// Get <see cref="CultureInfo"/> currently used by application.
	/// </summary>
	[ThreadSafe]
	public abstract CultureInfo CultureInfo { get; }


	/// <summary>
	/// Get <see cref="Application"/> instance for current process.
	/// </summary>
	public static new Application Current => (Application)Avalonia.Application.Current.AsNonNull();


	/// <summary>
	/// Get <see cref="Application"/> instance for current process, or null if <see cref="Application"/> is not ready yet.
	/// </summary>
	public static Application? CurrentOrNull 
	{
		get
        {
			try
			{
				return Current;
            }
			catch
            {
				return null;
            }
        }
	}


	/// <inheritdoc/>
	public abstract IObservable<string?> GetObservableString(string key);


	/// <inheritdoc/>
	public IObservable<object?> GetResourceObservable(object key, Func<object?, object?>? converter = null) =>
		ResourceNodeExtensions.GetResourceObservable(this, key, converter);


	/// <summary>
	/// Get string from resources according to given key and current settings or system language.
	/// </summary>
	/// <param name="key">Key of string to get.</param>
	/// <param name="defaultValue">Default string.</param>
	/// <returns>String.</returns>
	public abstract string? GetString(string key, string? defaultValue = null);


	/// <summary>
	/// Whether application shutdown has been started or not.
	/// </summary>
	[ThreadSafe]
	public abstract bool IsShutdownStarted { get; }


	/// <summary>
	/// <see cref="ILoggerFactory"/> to create logger.
	/// </summary>
	[ThreadSafe]
	public abstract ILoggerFactory LoggerFactory { get; }


	/// <summary>
	/// Get default <see cref="ISettings"/> to keep persistent state of application.
	/// </summary>
	[ThreadSafe]
	public abstract ISettings PersistentState { get; }


	/// <summary>
	/// Called when Avalonia framework initialized.
	/// </summary>
	public override void OnFrameworkInitializationCompleted()
	{
		this.synchronizationContext = DispatcherSynchronizationContext.UIThread;
		base.OnFrameworkInitializationCompleted();
	}


	/// <summary>
	/// Raise <see cref="INotifyPropertyChanged.PropertyChanged"/> event.
	/// </summary>
	/// <param name="propertyName">Name of changed property.</param>
	protected virtual void OnPropertyChanged(string propertyName) => this.propertyChangedHandlers?.Invoke(this, new PropertyChangedEventArgs(propertyName));


	/// <summary>
	/// Raise <see cref="StringsUpdated"/> event.
	/// </summary>
	/// <param name="e">Event data.</param>
	protected virtual void OnStringUpdated(EventArgs e) => this.StringsUpdated?.Invoke(this, e);


	/// <summary>
	/// Path to root of private directory which is suitable to be accessed by this application.
	/// </summary>
	[ThreadSafe]
	public virtual string RootPrivateDirectoryPath
	{
		get
		{
#if NET10_0_OR_GREATER
			field ??= Path.GetDirectoryName(Process.GetCurrentProcess().MainModule?.FileName) ?? throw new NotSupportedException();
			return field;
#else
			return this.rootPrivateDirPath ?? throw new NotSupportedException();
#endif
		}
	}


	/// <summary>
	/// Get default application level user settings.
	/// </summary>
	[ThreadSafe]
	public abstract ISettings Settings { get; }


	/// <summary>
	/// Get <see cref="DispatcherSynchronizationContext"/> of UI thread.
	/// </summary>
	[ThreadSafe]
	public DispatcherSynchronizationContext SynchronizationContext => this.synchronizationContext ?? throw new InvalidOperationException("Application is not ready yet.");


	/// <inheritdoc/>
	[ThreadSafe]
	SynchronizationContext ISynchronizable.SynchronizationContext => this.SynchronizationContext;


	/// <summary>
	/// Raised when string resources updated.
	/// </summary>
	public event EventHandler? StringsUpdated;


	// Implementations.
	event PropertyChangedEventHandler? INotifyPropertyChanged.PropertyChanged
	{
		add => this.propertyChangedHandlers += value;
		remove => this.propertyChangedHandlers -= value;
	}


	/// <inheritdoc/>
	public bool TryFindResource(object key, ThemeVariant? theme, [NotNullWhen(true)] out object? value) =>
		ResourceNodeExtensions.TryFindResource(this, key, theme, out value);
}