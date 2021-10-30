using CarinaStudio.AutoUpdate.Installers;
using CarinaStudio.AutoUpdate.Resolvers;
using CarinaStudio.IO;
using CarinaStudio.Threading;
using CarinaStudio.ViewModels;
using CarinaStudio.Windows.Input;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
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
		public static readonly ObservableProperty<double> ProgressPercentageProperty = ObservableProperty.Register<UpdatingSession, double>(nameof(ProgressPercentage), double.NaN, coerce: value =>
		{
			if (double.IsNaN(value))
				return value;
			if (value < 0)
				return 0;
			if (value > 100)
				return 100;
			return value;
		});


		// Fields.
		string? applicationDirectoryPath;
		readonly MutableObservableBoolean canCancelUpdating = new MutableObservableBoolean();
		readonly MutableObservableBoolean canStartUpdating = new MutableObservableBoolean(true);
		IStreamProvider? packageManifestSource;
		bool selfContainedPackageOnly;
		readonly Updater updater = new Updater();


		/// <summary>
		/// Initialize new <see cref="UpdatingSession"/> instance.
		/// </summary>
		/// <param name="app">Application.</param>
		protected UpdatingSession(IApplication app) : base(app)
		{
			// attach to updater
			this.updater.PropertyChanged += (_, e) => this.OnUpdaterPropertyChanged(e.PropertyName ?? "");

			// create commands
			this.CancelUpdatingCommand = new Command(() => this.updater.Cancel(), this.canCancelUpdating);
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
		void CheckProgressAvailability()
		{
			if (!this.IsUpdating)
				this.SetValue(IsProgressAvailableProperty, false);
			else
				this.SetValue(IsProgressAvailableProperty, double.IsFinite(this.ProgressPercentage));
		}


		/// <summary>
		/// Create <see cref="IPackageResolver"/> before updating process start.
		/// </summary>
		/// <param name="source">Source of package manifest.</param>
		/// <returns><see cref="IPackageResolver"/>.</returns>
		/// <remarks>Will create <see cref="XmlPackageResolver"/> by default implementation.</remarks>
		protected virtual IPackageResolver CreatePackageResolver(IStreamProvider source) =>
			new XmlPackageResolver() { Source = source };


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
		public long DownloadedPackageSize { get => this.GetValue(DownloadedPackageSizeProperty); }


		/// <summary>
		/// Check whether application is being backed-up or not.
		/// </summary>
		public bool IsBackingUpApplication { get => this.GetValue(IsBackingUpApplicationProperty); }


		/// <summary>
		/// Check whether update package is being downloaded or not.
		/// </summary>
		public bool IsDownloadingPackage { get => this.GetValue(IsDownloadingPackageProperty); }


		/// <summary>
		/// Check whether update package is being installed or not.
		/// </summary>
		public bool IsInstallingPackage { get => this.GetValue(IsInstallingPackageProperty); }


		/// <summary>
		/// Check whether progress of updating is available or not.
		/// </summary>
		public bool IsProgressAvailable { get => this.GetValue(IsProgressAvailableProperty); }


		/// <summary>
		/// Check whether update package is being resolved or not.
		/// </summary>
		public bool IsResolvingPackage { get => this.GetValue(IsResolvingPackageProperty); }


		/// <summary>
		/// Check whether application is being restored or not.
		/// </summary>
		public bool IsRestoringApplication { get => this.GetValue(IsRestoringApplicationProperty); }


		/// <summary>
		/// Check whether updating processing is on-going or not.
		/// </summary>
		public bool IsUpdating { get => this.GetValue(IsUpdatingProperty); }


		/// <summary>
		/// Check whether updating processing is cancelled or not.
		/// </summary>
		public bool IsUpdatingCancelled { get => this.GetValue(IsUpdatingCancelledProperty); }


		/// <summary>
		/// Check whether updating processing is being cancelled or not.
		/// </summary>
		public bool IsUpdatingCancelling { get => this.GetValue(IsUpdatingCancellingProperty); }


		/// <summary>
		/// Check whether updating processing is completed either successfully or unsuccessfully.
		/// </summary>
		public bool IsUpdatingCompleted { get => this.GetValue(IsUpdatingCompletedProperty); }


		/// <summary>
		/// Check whether updating processing is completed unsuccessfully or not.
		/// </summary>
		public bool IsUpdatingFailed { get => this.GetValue(IsUpdatingFailedProperty); }


		/// <summary>
		/// Check whether updating processing is completed successfully or not.
		/// </summary>
		public bool IsUpdatingSucceeded { get => this.GetValue(IsUpdatingSucceededProperty); }


		/// <summary>
		/// Check whether downloaded update package is being verified or not.
		/// </summary>
		public bool IsVerifyingPackage { get => this.GetValue(IsVerifyingPackageProperty); }


		/// <summary>
		/// Get message which describes status of updating.
		/// </summary>
		public string? Message { get => this.GetValue(MessageProperty); }


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
					this.canCancelUpdating.Update(this.updater.IsCancellable && !this.updater.IsCancelling);
					break;
				case nameof(Updater.IsCancelling):
					this.SetValue(IsUpdatingCancellingProperty, this.updater.IsCancelling);
					goto case nameof(Updater.IsCancellable);
				case nameof(Updater.PackageSize):
					this.SetValue(PackageSizeProperty, this.updater.PackageSize);
					break;
				case nameof(Updater.Progress):
					this.OnUpdaterProgressChanged();
					break;
				case nameof(Updater.State):
					this.Logger.LogDebug($"Updater state changed: {this.updater.State}");
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
					this.SetValue(IsUpdatingProperty, false);
					this.SetValue(IsUpdatingCancelledProperty, true);
					this.SetValue(IsUpdatingCompletedProperty, true);
					break;
				case UpdaterState.Failed:
					this.SetValue(IsUpdatingProperty, false);
					this.SetValue(IsUpdatingFailedProperty, true);
					this.SetValue(IsUpdatingCompletedProperty, true);
					break;
				case UpdaterState.Starting:
					this.SetValue(IsUpdatingProperty, true);
					break;
				case UpdaterState.Succeeded:
					this.SetValue(IsUpdatingProperty, false);
					this.SetValue(IsUpdatingSucceededProperty, true);
					this.SetValue(IsUpdatingCompletedProperty, true);
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
		/// Get size of package in bytes.
		/// </summary>
		public long? PackageSize { get => this.GetValue(PackageSizeProperty); }


		/// <summary>
		/// Get current proress of updating in percentage. <see cref="double.NaN"/> if progress is unavailable.
		/// </summary>
		public double ProgressPercentage { get => this.GetValue(ProgressPercentageProperty); }


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
			this.updater.PackageInstaller = new ZipPackageInstaller();
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


		/// <summary>
		/// Get progress reported by internal <see cref="Updater"/>.
		/// </summary>
		protected double UpdaterProgress { get => this.updater.Progress; }


		/// <summary>
		/// Get state of internal <see cref="Updater"/>.
		/// </summary>
		protected UpdaterState UpdaterState { get => this.updater.State; }


		/// <summary>
		/// Get version of application which is updating to.
		/// </summary>
		protected Version? UpdatingVersion { get => this.updater.PackageResolver?.PackageVersion; }
	}
}
