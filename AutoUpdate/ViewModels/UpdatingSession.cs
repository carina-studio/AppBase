using CarinaStudio.AutoUpdate.Installers;
using CarinaStudio.AutoUpdate.Resolvers;
using CarinaStudio.IO;
using CarinaStudio.Threading;
using CarinaStudio.ViewModels;
using CarinaStudio.Windows.Input;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CarinaStudio.AutoUpdate.ViewModels
{
	/// <summary>
	/// Base class of view-model of auto updating session.
	/// </summary>
	public abstract class UpdatingSession : ViewModel
	{
		/// <summary>
		/// Property of <see cref="ApplicationName"/>
		/// </summary>
		public static readonly ObservableProperty<string?> ApplicationNameProperty = ObservableProperty.Register<UpdatingSession, string?>(nameof(ApplicationName));
		/// <summary>
		/// Property of <see cref="DownloadedPackageSize"/>
		/// </summary>
		public static readonly ObservableProperty<long> DownloadedPackageSizeProperty = ObservableProperty.Register<UpdatingSession, long>(nameof(DownloadedPackageSize));
		/// <summary>
		/// Property of <see cref="IsBackingUpApplication"/>
		/// </summary>
		public static readonly ObservableProperty<bool> IsBackingUpApplicationProperty = ObservableProperty.Register<UpdatingSession, bool>(nameof(IsBackingUpApplication));
		/// <summary>
		/// Property of <see cref="IsDownloadingPackage"/>
		/// </summary>
		public static readonly ObservableProperty<bool> IsDownloadingPackageProperty = ObservableProperty.Register<UpdatingSession, bool>(nameof(IsDownloadingPackage));
		/// <summary>
		/// Property of <see cref="IsInstallingPackage"/>
		/// </summary>
		public static readonly ObservableProperty<bool> IsInstallingPackageProperty = ObservableProperty.Register<UpdatingSession, bool>(nameof(IsInstallingPackage));
		/// <summary>
		/// Property of <see cref="IsProgressAvailable"/>
		/// </summary>
		public static readonly ObservableProperty<bool> IsProgressAvailableProperty = ObservableProperty.Register<UpdatingSession, bool>(nameof(IsProgressAvailable));
		/// <summary>
		/// Property of <see cref="IsRefreshingApplicationIcon"/>
		/// </summary>
		public static readonly ObservableProperty<bool> IsRefreshingApplicationIconProperty = ObservableProperty.Register<UpdatingSession, bool>(nameof(IsRefreshingApplicationIcon));
		/// <summary>
		/// Property of <see cref="IsResolvingPackage"/>
		/// </summary>
		public static readonly ObservableProperty<bool> IsResolvingPackageProperty = ObservableProperty.Register<UpdatingSession, bool>(nameof(IsResolvingPackage));
		/// <summary>
		/// Property of <see cref="IsRestoringApplication"/>
		/// </summary>
		public static readonly ObservableProperty<bool> IsRestoringApplicationProperty = ObservableProperty.Register<UpdatingSession, bool>(nameof(IsRestoringApplication));
		/// <summary>
		/// Property of <see cref="IsUpdating"/>
		/// </summary>
		public static readonly ObservableProperty<bool> IsUpdatingProperty = ObservableProperty.Register<UpdatingSession, bool>(nameof(IsUpdating));
		/// <summary>
		/// Property of <see cref="IsUpdatingCancelled"/>
		/// </summary>
		public static readonly ObservableProperty<bool> IsUpdatingCancelledProperty = ObservableProperty.Register<UpdatingSession, bool>(nameof(IsUpdatingCancelled));
		/// <summary>
		/// Property of <see cref="IsUpdatingCancelling"/>
		/// </summary>
		public static readonly ObservableProperty<bool> IsUpdatingCancellingProperty = ObservableProperty.Register<UpdatingSession, bool>(nameof(IsUpdatingCancelling));
		/// <summary>
		/// Property of <see cref="IsUpdatingCompleted"/>
		/// </summary>
		public static readonly ObservableProperty<bool> IsUpdatingCompletedProperty = ObservableProperty.Register<UpdatingSession, bool>(nameof(IsUpdatingCompleted));
		/// <summary>
		/// Property of <see cref="IsUpdatingFailed"/>
		/// </summary>
		public static readonly ObservableProperty<bool> IsUpdatingFailedProperty = ObservableProperty.Register<UpdatingSession, bool>(nameof(IsUpdatingFailed));
		/// <summary>
		/// Property of <see cref="IsUpdatingSucceeded"/>
		/// </summary>
		public static readonly ObservableProperty<bool> IsUpdatingSucceededProperty = ObservableProperty.Register<UpdatingSession, bool>(nameof(IsUpdatingSucceeded));
		/// <summary>
		/// Property of <see cref="IsVerifyingPackage"/>
		/// </summary>
		public static readonly ObservableProperty<bool> IsVerifyingPackageProperty = ObservableProperty.Register<UpdatingSession, bool>(nameof(IsVerifyingPackage));
		/// <summary>
		/// Property of <see cref="Message"/>
		/// </summary>
		public static readonly ObservableProperty<string?> MessageProperty = ObservableProperty.Register<UpdatingSession, string?>(nameof(Message));
		/// <summary>
		/// Property of <see cref="PackageSize"/>
		/// </summary>
		public static readonly ObservableProperty<long?> PackageSizeProperty = ObservableProperty.Register<UpdatingSession, long?>(nameof(PackageSize));
		/// <summary>
		/// Property of <see cref="ProgressPercentage"/>
		/// </summary>
		public static readonly ObservableProperty<double> ProgressPercentageProperty = ObservableProperty.Register<UpdatingSession, double>(nameof(ProgressPercentage), double.NaN, 
			coerce: (_, value) =>
			{
				if (double.IsNaN(value))
					return value;
				if (value < 0)
					return 0;
				if (value > 100)
					return 100;
				return value;
			});
		/// <summary>
		/// Property of <see cref="RefreshApplicationIconAutomatically"/>
		/// </summary>
		public static readonly ObservableProperty<bool> RefreshApplicationIconAutomaticallyProperty = ObservableProperty.Register<UpdatingSession, bool>(nameof(RefreshApplicationIconAutomatically),
			coerce: (session, update) =>
			{
				if (session.GetValue(IsUpdatingProperty))
					throw new InvalidOperationException();
				return update;
			});
		/// <summary>
		/// Property of <see cref="RefreshApplicationIconMessage"/>
		/// </summary>
		public static readonly ObservableProperty<string?> RefreshApplicationIconMessageProperty = ObservableProperty.Register<UpdatingSession, string?>(nameof(RefreshApplicationIconMessage),
			coerce: (session, message) =>
			{
				if (session.GetValue(IsUpdatingProperty))
					throw new InvalidOperationException();
				return message;
			});


		// Fields.
		Version? applicationBaseVersion;
		string? applicationDirectoryPath;
		readonly MutableObservableBoolean canCancelUpdating = new();
		readonly MutableObservableBoolean canStartUpdating = new(true);
		IStreamProvider? packageManifestSource;
		CancellationTokenSource? refreshAppIconCancellationTokenSource;
		bool selfContainedPackageOnly;
		readonly Updater updater;


		/// <summary>
		/// Initialize new <see cref="UpdatingSession"/> instance.
		/// </summary>
		/// <param name="app">Application.</param>
		protected UpdatingSession(IApplication app) : base(app)
		{
			// attach to updater
			this.updater = new Updater(app);
			this.updater.PropertyChanged += (_, e) => this.OnUpdaterPropertyChanged(e.PropertyName ?? "");

			// create commands
			this.CancelUpdatingCommand = new Command(() =>
			{
				if (this.refreshAppIconCancellationTokenSource is not null)
					this.refreshAppIconCancellationTokenSource.Cancel();
				else
					this.updater.Cancel();
			}, this.canCancelUpdating);
			this.StartUpdatingCommand = new Command(this.StartUpdating, this.canStartUpdating);
		}


		/// <summary>
		/// Get or set path to directory of application to update.
		/// </summary>
		public string? ApplicationDirectoryPath
		{
			get => this.applicationDirectoryPath;
			set
			{
				this.VerifyAccess();
				this.VerifyDisposed();
				if (this.updater.State != UpdaterState.Initializing)
					throw new InvalidOperationException();
				if (this.applicationDirectoryPath == value)
					return;
				this.applicationDirectoryPath = value;
				this.OnPropertyChanged(nameof(ApplicationDirectoryPath));
			}
		}


		/// <summary>
		/// Get or set base version of application to update.
		/// </summary>
		public Version? ApplicationBaseVersion
		{
			get => this.applicationBaseVersion;
			set
			{
				this.VerifyAccess();
				this.VerifyDisposed();
				if (this.updater.State != UpdaterState.Initializing)
					throw new InvalidOperationException();
				if (this.applicationBaseVersion == value)
					return;
				this.applicationBaseVersion = value;
				this.OnPropertyChanged(nameof(ApplicationBaseVersion));
			}
		}


		/// <summary>
		/// Get or set name of application to update.
		/// </summary>
		public string? ApplicationName
		{
			get => this.GetValue(ApplicationNameProperty);
			set => this.SetValue(ApplicationNameProperty, value);
		}


		/// <summary>
		/// Command to cancel updating process.
		/// </summary>
		public ICommand CancelUpdatingCommand { get; }


		// Check availability of progress.
		void CheckProgressAvailability() =>
			this.SetValue(IsProgressAvailableProperty, this.IsUpdating && double.IsFinite(this.ProgressPercentage));
		
		
		// Complete updating progress.
		void CompleteUpdating(bool isSucceeded, bool isCancelled)
		{
			if (this.IsDisposed || !this.GetValue(IsUpdatingProperty))
				return;
			this.Logger.LogTrace("Complete updating, succeeded: {s}, cancelled: {c}", isSucceeded, isCancelled);
			if (isCancelled)
			{
				this.SetValue(IsUpdatingProperty, false);
				this.SetValue(IsUpdatingCancelledProperty, true);
				this.SetValue(IsUpdatingCompletedProperty, true);
			}
			else if (isSucceeded)
			{
				this.SetValue(IsUpdatingProperty, false);
				this.SetValue(IsUpdatingSucceededProperty, true);
				this.SetValue(IsUpdatingCompletedProperty, true);
			}
			else
			{
				this.SetValue(IsUpdatingProperty, false);
				this.SetValue(IsUpdatingFailedProperty, true);
				this.SetValue(IsUpdatingCompletedProperty, true);
			}
		}


		/// <summary>
		/// Create <see cref="IPackageResolver"/> before updating process start.
		/// </summary>
		/// <param name="source">Source of package manifest.</param>
		/// <returns><see cref="IPackageResolver"/>.</returns>
		/// <remarks>Will create <see cref="XmlPackageResolver"/> by default implementation.</remarks>
		protected virtual IPackageResolver CreatePackageResolver(IStreamProvider source) =>
			new XmlPackageResolver(this.Application, this.applicationBaseVersion) { Source = source };


		/// <summary>
		/// Dispose the instance.
		/// </summary>
		/// <param name="disposing">True to dispose managed resources.</param>
		protected override void Dispose(bool disposing)
		{
			// do nothing if called by finalizer
			if (!disposing)
			{
				base.Dispose(disposing);
				return;
			}

			// check thread
			this.VerifyAccess();

			// dispose updater
			this.updater.Dispose();

			// call base
			base.Dispose(disposing);
		}


		/// <summary>
		/// Get size of downloaded package in bytes.
		/// </summary>
		public long DownloadedPackageSize => this.GetValue(DownloadedPackageSizeProperty);


		/// <summary>
		/// Check whether application is being backed-up or not.
		/// </summary>
		public bool IsBackingUpApplication => this.GetValue(IsBackingUpApplicationProperty);


		/// <summary>
		/// Check whether update package is being downloaded or not.
		/// </summary>
		public bool IsDownloadingPackage => this.GetValue(IsDownloadingPackageProperty);


		/// <summary>
		/// Check whether update package is being installed or not.
		/// </summary>
		public bool IsInstallingPackage => this.GetValue(IsInstallingPackageProperty);


		/// <summary>
		/// Check whether progress of updating is available or not.
		/// </summary>
		public bool IsProgressAvailable => this.GetValue(IsProgressAvailableProperty);
		
		
		/// <summary>
		/// Check whether application icon refreshing is being performed or not.
		/// </summary>
		public bool IsRefreshingApplicationIcon => this.GetValue(IsRefreshingApplicationIconProperty);


		/// <summary>
		/// Check whether update package is being resolved or not.
		/// </summary>
		public bool IsResolvingPackage => this.GetValue(IsResolvingPackageProperty);


		/// <summary>
		/// Check whether application is being restored or not.
		/// </summary>
		public bool IsRestoringApplication => this.GetValue(IsRestoringApplicationProperty);


		/// <summary>
		/// Check whether updating processing is on-going or not.
		/// </summary>
		public bool IsUpdating => this.GetValue(IsUpdatingProperty);


		/// <summary>
		/// Check whether updating processing is cancelled or not.
		/// </summary>
		public bool IsUpdatingCancelled => this.GetValue(IsUpdatingCancelledProperty);


		/// <summary>
		/// Check whether updating processing is being cancelled or not.
		/// </summary>
		public bool IsUpdatingCancelling => this.GetValue(IsUpdatingCancellingProperty);


		/// <summary>
		/// Check whether updating processing is completed either successfully or unsuccessfully.
		/// </summary>
		public bool IsUpdatingCompleted => this.GetValue(IsUpdatingCompletedProperty);


		/// <summary>
		/// Check whether updating processing is completed unsuccessfully or not.
		/// </summary>
		public bool IsUpdatingFailed => this.GetValue(IsUpdatingFailedProperty);


		/// <summary>
		/// Check whether updating processing is completed successfully or not.
		/// </summary>
		public bool IsUpdatingSucceeded => this.GetValue(IsUpdatingSucceededProperty);


		/// <summary>
		/// Check whether downloaded update package is being verified or not.
		/// </summary>
		public bool IsVerifyingPackage => this.GetValue(IsVerifyingPackageProperty);


		/// <summary>
		/// Get message which describes status of updating.
		/// </summary>
		public string? Message => this.GetValue(MessageProperty);


		/// <summary>
		/// Called when property changed.
		/// </summary>
		/// <param name="property">Changed property.</param>
		/// <param name="oldValue">Old value.</param>
		/// <param name="newValue">New value.</param>
		protected override void OnPropertyChanged(ObservableProperty property, object? oldValue, object? newValue)
		{
			base.OnPropertyChanged(property, oldValue, newValue);
			if (property == IsUpdatingProperty || property == ProgressPercentageProperty)
				this.CheckProgressAvailability();
		}


		/// <summary>
		/// Called when progress reported by internal <see cref="Updater"/> changed.
		/// </summary>
		protected virtual void OnUpdaterProgressChanged()
		{
			this.SetValue(ProgressPercentageProperty, this.updater.Progress * 100);
		}


		// Called when property of updater changed.
		void OnUpdaterPropertyChanged(string propertyName)
		{
			switch (propertyName)
			{
				case nameof(Updater.DownloadedPackageSize):
					this.SetValue(DownloadedPackageSizeProperty, this.updater.DownloadedPackageSize);
					break;
				case nameof(Updater.IsCancellable):
					this.UpdateCanCancelUpdating();
					break;
				case nameof(Updater.IsCancelling):
					this.SetValue(IsUpdatingCancellingProperty, this.updater.IsCancelling);
					this.UpdateCanCancelUpdating();
					break;
				case nameof(Updater.PackageSize):
					this.SetValue(PackageSizeProperty, this.updater.PackageSize);
					break;
				case nameof(Updater.Progress):
					this.OnUpdaterProgressChanged();
					break;
				case nameof(Updater.State):
					this.Logger.LogDebug("Updater state changed: {state}", this.updater.State);
					this.OnUpdaterStateChanged();
					break;
			}
		}


		/// <summary>
		/// Called when state of internal <see cref="Updater"/> changed.
		/// </summary>
		protected virtual void OnUpdaterStateChanged()
		{
			if (this.IsDisposed)
				return;
			this.SetValue(IsBackingUpApplicationProperty, this.updater.State == UpdaterState.BackingUpApplication);
			this.SetValue(IsDownloadingPackageProperty, this.updater.State == UpdaterState.DownloadingPackage);
			this.SetValue(IsInstallingPackageProperty, this.updater.State == UpdaterState.InstallingPackage);
			this.SetValue(IsResolvingPackageProperty, this.updater.State == UpdaterState.ResolvingPackage);
			this.SetValue(IsRestoringApplicationProperty, this.updater.State == UpdaterState.RestoringApplication);
			this.SetValue(IsVerifyingPackageProperty, this.updater.State == UpdaterState.VerifyingPackage);
			switch (this.updater.State)
			{
				case UpdaterState.Cancelled:
					this.CompleteUpdating(false, true);
					break;
				case UpdaterState.Failed:
					this.CompleteUpdating(false, false);
					break;
				case UpdaterState.Starting:
					this.SetValue(IsUpdatingProperty, true);
					break;
				case UpdaterState.Succeeded:
					if (this.GetValue(RefreshApplicationIconAutomaticallyProperty))
						this.RefreshApplicationIcon();
					else
						this.CompleteUpdating(true, false);
					break;
			}
		}


		/// <summary>
		/// Get or set source of package manifest.
		/// </summary>
		public IStreamProvider? PackageManifestSource
		{
			get => this.packageManifestSource;
			set
			{
				this.VerifyAccess();
				this.VerifyDisposed();
				if (this.updater.State != UpdaterState.Initializing)
					throw new InvalidOperationException();
				if (this.packageManifestSource == value)
					return;
				this.packageManifestSource = value;
				this.OnPropertyChanged(nameof(PackageManifestSource));
			}
		}
		
		
		/// <summary>
		/// Get collection of custom headers for requesting package download.
		/// </summary>
		public IDictionary<string, string> PackageRequestHeaders => this.updater.PackageRequestHeaders;


		/// <summary>
		/// Get size of package in bytes.
		/// </summary>
		public long? PackageSize => this.GetValue(PackageSizeProperty);


		/// <summary>
		/// Get current progress of updating in percentage. <see cref="double.NaN"/> if progress is unavailable.
		/// </summary>
		public double ProgressPercentage => this.GetValue(ProgressPercentageProperty);


		// Refresh application icon.
		async void RefreshApplicationIcon()
		{
			// check state
			if (this.IsDisposed || this.UpdaterState != UpdaterState.Succeeded || this.GetValue(IsRefreshingApplicationIconProperty))
				return;
			if (this.updater.PackageInstaller?.IsApplicationIconUpdated != true)
			{
				this.Logger.LogTrace("No need to refresh application icon");
				this.CompleteUpdating(true, false);
				return;
			}
			
			// refresh
			var cancellationTokenSource = new CancellationTokenSource();
			this.refreshAppIconCancellationTokenSource = cancellationTokenSource;
			this.UpdateCanCancelUpdating();
			this.SetValue(IsRefreshingApplicationIconProperty, true);
			try
			{
				if (Platform.IsMacOS)
				{
					var appBundlePath = this.applicationDirectoryPath;
					if (string.IsNullOrEmpty(appBundlePath) || (!appBundlePath.EndsWith(".app") && !appBundlePath.EndsWith(".app/")))
						this.Logger.LogWarning("Unable to refresh application icon because application directory is not an application bundle: '{path}'", appBundlePath);
					else
					{
						this.Logger.LogDebug("Refresh application icon of application bundle: '{path}'", appBundlePath);
						if (appBundlePath.EndsWith("/"))
							appBundlePath = appBundlePath[..^1];
						var title = this.GetValue(RefreshApplicationIconMessageProperty);
						if (string.IsNullOrEmpty(title))
							title = "Refresh application icon";
						await Task.Run(() =>
						{
							var tempScriptFile = default(string);
							try
							{
								// generate apple script file
								tempScriptFile = Path.GetTempFileName();
								using (var stream = new FileStream(tempScriptFile, FileMode.Create, FileAccess.ReadWrite))
								{
									using var writer = new StreamWriter(stream, Encoding.UTF8);
									writer.Write($"do shell script \"touch '{appBundlePath}'; sleep 1s; killall Finder; killall Dock\"");
									writer.Write($" with prompt \"{title}\"");
									writer.Write(" with administrator privileges");
								}
								
								// cancellation check
								if (cancellationTokenSource.IsCancellationRequested)
									throw new TaskCanceledException();

								// run apple script
								using var process = Process.Start(new ProcessStartInfo
								{
									Arguments = tempScriptFile,
									CreateNoWindow = true,
									FileName = "osascript",
									RedirectStandardError = true,
									RedirectStandardOutput = true,
									UseShellExecute = false,
								});
								if (process is not null)
								{
									process.WaitForExit();
									if (process.ExitCode != 0)
										this.Logger.LogWarning("Result of osascript to refresh application icon is {code}", process.ExitCode);
								}
								else
									this.Logger.LogError("Unable to start osascript to refresh application icon");
							}
							finally
							{
								if (tempScriptFile is not null)
									Global.RunWithoutError(() => System.IO.File.Delete(tempScriptFile));
							}
						}, this.refreshAppIconCancellationTokenSource.Token);
					}
				}
				else
					this.Logger.LogWarning("Application icon refreshing is unsupported on current platform");
			}
			catch (Exception ex)
			{
				if (ex is TaskCanceledException)
					this.Logger.LogWarning("Application icon refreshing has been cancelled");
				else
					this.Logger.LogError(ex, "Error occurred while refreshing application icon");
			}
			finally
			{
				cancellationTokenSource.Dispose();
				this.refreshAppIconCancellationTokenSource = null;
				this.UpdateCanCancelUpdating();
				this.ResetValue(IsRefreshingApplicationIconProperty);
			}
			this.CompleteUpdating(true, false);
		}


		/// <summary>
		/// Get or set whether refreshing application should be performed when needed or not.
		/// </summary>
		public bool RefreshApplicationIconAutomatically
		{
			get => this.GetValue(RefreshApplicationIconAutomaticallyProperty);
			set => this.SetValue(RefreshApplicationIconAutomaticallyProperty, value);
		}
		
		
		/// <summary>
		/// Get or set message for refreshing application icon.
		/// </summary>
		public string? RefreshApplicationIconMessage
		{
			get => this.GetValue(RefreshApplicationIconMessageProperty);
			set => this.SetValue(RefreshApplicationIconMessageProperty, value);
		}


		/// <summary>
		/// Get or set whether only self-contained update package can be selected or not.
		/// </summary>
		public bool SelfContainedPackageOnly
		{
			get => this.selfContainedPackageOnly;
			set
			{
				this.VerifyAccess();
				this.VerifyDisposed();
				if (this.updater.State != UpdaterState.Initializing)
					throw new InvalidOperationException();
				if (this.selfContainedPackageOnly == value)
					return;
				this.selfContainedPackageOnly = value;
				this.OnPropertyChanged(nameof(SelfContainedPackageOnly));
			}
		}


		// Start updating.
		void StartUpdating()
		{
			// check state
			this.VerifyAccess();
			this.VerifyDisposed();
			if (!this.canStartUpdating.Value)
				return;
			if (string.IsNullOrWhiteSpace(this.applicationDirectoryPath))
			{
				this.Logger.LogError("No application directory specified");
				return;
			}
			if (this.packageManifestSource == null)
			{
				this.Logger.LogError("No package manifest source specified");
				return;
			}

			// update state
			this.canStartUpdating.Update(false);

			// prepare updater
			this.updater.ApplicationDirectoryPath = applicationDirectoryPath;
			this.updater.PackageInstaller = new ZipPackageInstaller(this.Application);
			this.updater.PackageResolver = this.CreatePackageResolver(this.packageManifestSource).Also(it =>
			{
				it.SelfContainedPackageOnly = this.selfContainedPackageOnly;
			});

			// start
			this.Logger.LogDebug("Start updating");
			if (!this.updater.Start())
			{
				this.Logger.LogError("Failed to start updating");
				this.canStartUpdating.Update(true);
			}
		}


		/// <summary>
		/// Command to start updating process.
		/// </summary>
		public ICommand StartUpdatingCommand { get; }


		// Report whether cancellation of updating can be performed or not.
		void UpdateCanCancelUpdating()
		{
			if (this.IsDisposed)
				return;
			if (this.refreshAppIconCancellationTokenSource is not null)
				this.canCancelUpdating.Update(!this.refreshAppIconCancellationTokenSource.IsCancellationRequested);
			else
				this.canCancelUpdating.Update(this.updater.IsCancellable && !this.updater.IsCancelling);
		}


		/// <summary>
		/// Get progress reported by internal <see cref="Updater"/>.
		/// </summary>
		protected double UpdaterProgress => this.updater.Progress;


		/// <summary>
		/// Get state of internal <see cref="Updater"/>.
		/// </summary>
		protected UpdaterState UpdaterState => this.updater.State;


		/// <summary>
		/// Get version of application which is updating to.
		/// </summary>
		protected Version? UpdatingVersion => this.updater.PackageResolver?.PackageVersion;
	}
}
