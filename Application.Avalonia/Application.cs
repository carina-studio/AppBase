﻿using CarinaStudio.Configuration;
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
	/// Base implementation of <see cref="IApplication"/> based-on <see cref="Avalonia.Application"/>.
	/// </summary>
	public abstract class Application : Avalonia.Application, IAvaloniaApplication
	{
		// Fields.
		PropertyChangedEventHandler? propertyChangedHandlers;
		readonly string? rootPrivateDirPath;
		volatile SynchronizationContext? synchronizationContext;


		/// <summary>
		/// Initialize new <see cref="Application"/> instance.
		/// </summary>
		protected Application()
		{
			this.rootPrivateDirPath = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule?.FileName);
		}


		/// <summary>
		/// Get <see cref="Assembly"/> of application.
		/// </summary>
		public virtual Assembly Assembly { get; } = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();


		/// <summary>
		/// Get <see cref="CultureInfo"/> currently used by application.
		/// </summary>
		public abstract CultureInfo CultureInfo { get; }


		/// <summary>
		/// Get <see cref="Application"/> instance for current process.
		/// </summary>
		public static new Application Current { get => (Application)Avalonia.Application.Current.AsNonNull(); }


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
		public abstract bool IsShutdownStarted { get; }


		/// <summary>
		/// <see cref="ILoggerFactory"/> to create logger.
		/// </summary>
		public abstract ILoggerFactory LoggerFactory { get; }


		/// <summary>
		/// Get default <see cref="ISettings"/> to keep persistent state of application.
		/// </summary>
		public abstract ISettings PersistentState { get; }


		/// <summary>
		/// Called when Avalonia framework initialized.
		/// </summary>
		public override void OnFrameworkInitializationCompleted()
		{
			this.synchronizationContext = SynchronizationContext.Current.AsNonNull();
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
		public virtual string RootPrivateDirectoryPath { get => this.rootPrivateDirPath ?? throw new NotSupportedException(); }


		/// <summary>
		/// Get default application level user settings.
		/// </summary>
		public abstract ISettings Settings { get; }


		/// <summary>
		/// Get <see cref="SynchronizationContext"/>.
		/// </summary>
		public SynchronizationContext SynchronizationContext { get => this.synchronizationContext ?? throw new InvalidOperationException("Application is not ready yet."); }


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
	}
}
