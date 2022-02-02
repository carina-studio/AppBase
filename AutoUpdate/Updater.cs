﻿using CarinaStudio.AutoUpdate.Installers;
using CarinaStudio.AutoUpdate.Resolvers;
using CarinaStudio.Threading;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace CarinaStudio.AutoUpdate
{
	/// <summary>
	/// Core object to perform auto/self update.
	/// </summary>
	public class Updater : BaseDisposableApplicationObject, INotifyPropertyChanged, IThreadDependent
	{
		// Fields.
		string? applicationDirectoryPath;
		readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
		readonly ILogger logger;
		IPackageInstaller? packageInstaller;
		IPackageResolver? packageResolver;
		double progress = double.NaN;
		UpdaterState state = UpdaterState.Initializing;
		readonly object waitingSyncLock = new object();


		/// <summary>
		/// Initialize new <see cref="Updater"/> instance.
		/// </summary>
		/// <param name="app">Application.</param>
		public Updater(IApplication app) : base(app)
		{
			this.logger = app.LoggerFactory.CreateLogger(nameof(Updater));
		}


		/// <summary>
		/// Get or set path of application directory to update.
		/// </summary>
		public string? ApplicationDirectoryPath
		{
			get => this.applicationDirectoryPath;
			set
			{
				this.VerifyAccess();
				this.VerifyDisposed();
				this.VerifyInitializing();
				if (this.applicationDirectoryPath == value)
					return;
				this.applicationDirectoryPath = value;
				this.OnPropertyChanged(nameof(ApplicationDirectoryPath));
			}
		}


		// Back up application.
		async Task<string?> BackUpApplicationAsync()
		{
			// check state
			switch (this.state)
			{
				case UpdaterState.BackingUpApplication:
				case UpdaterState.DownloadingPackage:
				case UpdaterState.ResolvingPackage:
					break;
				default:
					return null;
			}
			if (this.cancellationTokenSource.IsCancellationRequested)
				return null;

			this.logger.LogDebug("Start backing up application");

			// check directory
			var appDirectory = this.applicationDirectoryPath.AsNonNull();

			// create temp directory for backing up
			var exception = (Exception?)null;
			var backupDirectory = await Task.Run(() =>
			{
				try
				{
					var directory = Path.Combine(Path.GetTempPath(), $"CarinaStudio-AutoUpdate-{DateTime.Now.ToBinary()}");
					Directory.CreateDirectory(directory);
					this.logger.LogTrace($"Back up application to '{directory}'");
					return directory;
				}
				catch (Exception ex)
				{
					this.logger.LogError(ex, $"Unable to create directory to back up application");
					exception = ex;
					return null;
				}
			});
			if (backupDirectory == null)
			{
				this.CompleteUpdating(exception);
				return null;
			}
			if (this.cancellationTokenSource.IsCancellationRequested)
			{
				this.logger.LogWarning("Cancelled while backing up application");
				this.CompleteUpdating(null);
				return null;
			}

			// backup
			try
			{
				await Task.Run(() => this.CopyFiles(appDirectory, backupDirectory, this.cancellationTokenSource.Token, true));
			}
			catch(Exception ex)
			{
				Global.RunWithoutErrorAsync(() => Directory.Delete(backupDirectory, true));
				this.CompleteUpdating(ex);
				return null;
			}

			// cancellation check
			if (this.cancellationTokenSource.IsCancellationRequested)
			{
				this.logger.LogWarning("Cancelled after backing up application");
				Global.RunWithoutErrorAsync(() => 
				{
					this.logger.LogTrace($"Delete back up directory '{backupDirectory}'");
					try
					{
						Directory.Delete(backupDirectory, true);
					}
					catch (Exception ex)
					{
						this.logger.LogError(ex, $"Failed to delete back up directory '{backupDirectory}'");
					}
				});
				this.CompleteUpdating(null);
				return null;
			}

			// complete
			this.logger.LogDebug("Complete backing up application");
			return backupDirectory;
		}


		/// <summary>
		/// Cancel updating process.
		/// </summary>
		/// <returns>True if cancellation has been accepted.</returns>
		public bool Cancel()
		{
			// check state
			this.VerifyAccess();
			this.VerifyDisposed();
			if (this.IsCancelling)
				return true;
			if (!this.IsUpdating || !this.IsCancellable)
				return false;

			// cancel
			this.IsCancelling = true;
			this.OnPropertyChanged(nameof(IsCancelling));
			this.cancellationTokenSource.Cancel();
			this.packageResolver?.Cancel();
			this.packageInstaller?.Cancel();
			return true;
		}


		// Change state.
		bool ChangeState(UpdaterState state)
		{
			if (this.state == state)
				return true;
			this.logger.LogDebug($"Change state from {this.state} to {state}");
			this.state = state;
			this.OnPropertyChanged(nameof(State));
			return (this.state == state);
		}


		// Complete updating process.
		void CompleteUpdating(Exception? ex)
		{
			// check state
			if (this.IsDisposed || !this.IsUpdating)
				return;

			// update state
			this.IsUpdating = false;
			this.OnPropertyChanged(nameof(IsUpdating));
			if (this.IsCancellable)
			{
				this.IsCancellable = false;
				this.OnPropertyChanged(nameof(IsCancellable));
			}

			// complete
			if (this.IsCancelling)
			{
				this.logger.LogWarning("Updating cancelled");
				this.ChangeState(UpdaterState.Cancelled);
				this.IsCancelling = false;
				this.OnPropertyChanged(nameof(IsCancelling));
			}
			else if (ex == null)
			{
				this.logger.LogDebug("Updating succeeded");
				this.ChangeState(UpdaterState.Succeeded);
			}
			else
			{
				this.logger.LogError("Updating failed");
				this.Exception = ex;
				this.OnPropertyChanged(nameof(Exception));
				this.ChangeState(UpdaterState.Failed);
			}

			// release waiting lock
			lock (this.waitingSyncLock)
				Monitor.PulseAll(this.waitingSyncLock);
		}


		// Copy files in directory.
		void CopyFiles(string srcDirectory, string destDirectory, CancellationToken cancellationToken, bool throwException)
		{
			try
			{
				this.logger.LogTrace($"Start copying items in '{srcDirectory}' to '{destDirectory}'");
				foreach (var srcFilePath in Directory.EnumerateFiles(srcDirectory))
				{
					var destFilePath = Path.Combine(destDirectory, Path.GetFileName(srcFilePath));
					try
					{
						this.logger.LogTrace($"Copy '{srcFilePath}' to '{destFilePath}'");
						File.Copy(srcFilePath, destFilePath, true);
					}
					catch (Exception ex)
					{
						this.logger.LogError(ex, $"Failed to copy '{srcFilePath}' to '{destFilePath}'");
						if (throwException)
							throw;
					}
					if (cancellationToken.IsCancellationRequested)
					{
						this.logger.LogWarning("Items copying has been cancelled");
						return;
					}
				}
				foreach (var srcSubDirectory in Directory.EnumerateDirectories(srcDirectory))
				{
					var destSubDirectory = Path.Combine(destDirectory, Path.GetFileName(srcSubDirectory));
					try
					{
						Directory.CreateDirectory(destSubDirectory);
						this.CopyFiles(srcSubDirectory, destSubDirectory, cancellationToken, throwException);
					}
					catch (Exception ex)
					{
						this.logger.LogError(ex, $"Failed to copy items from '{srcSubDirectory}' to '{destSubDirectory}'");
						if (throwException)
							throw;
					}
					if (cancellationToken.IsCancellationRequested)
					{
						this.logger.LogWarning("Items copying has been cancelled");
						return;
					}
				}
				this.logger.LogTrace($"Complete copying items in '{srcDirectory}' to '{destDirectory}'");
			}
			catch (Exception ex)
			{
				this.logger.LogError(ex, $"Error occurred while copying items in '{srcDirectory}' to '{destDirectory}'");
				if (throwException)
					throw;
			}
		}


		/// <summary>
		/// Dispose instance.
		/// </summary>
		/// <param name="disposing">True to dispose managed resources.</param>
		protected override void Dispose(bool disposing)
		{
			// check thread
			if (disposing)
				this.VerifyAccess();

			// change state
			if (disposing)
				this.ChangeState(UpdaterState.Disposed);

			// cancel updating
			this.cancellationTokenSource.Cancel();

			// release waiting lock
			lock (this.waitingSyncLock)
				Monitor.PulseAll(this.waitingSyncLock);
		}


		/// <summary>
		/// Get size of downloaded package in bytes.
		/// </summary>
		public long DownloadedPackageSize { get; private set; }


		// Download package.
		async Task<string?> DownloadPackageAsync(Uri packageUri)
		{
			// check state
			if (this.state != UpdaterState.DownloadingPackage)
				return null;
			if (this.cancellationTokenSource.IsCancellationRequested)
				return null;

			// create temp file
			var packageFilePath = (string?)null;
			try
			{
				packageFilePath = await Task.Run(Path.GetTempFileName);
				this.logger.LogTrace($"Temp file for downloaded package: {packageFilePath}");
			}
			catch (Exception ex)
			{
				this.logger.LogError(ex, "Unable to get temp file for downloaded package");
				this.CompleteUpdating(ex);
				return null;
			}

			// download package
			var downloadedSize = 0L;
			var reportProgressAction = new ScheduledAction(() =>
			{
				if (this.cancellationTokenSource.IsCancellationRequested)
					return;
				this.logger.LogTrace($"Size of downloaded package: {downloadedSize}, total: {this.PackageSize.GetValueOrDefault()}");
				this.DownloadedPackageSize = downloadedSize;
				this.OnPropertyChanged(nameof(DownloadedPackageSize));
				var packageSize = this.PackageSize.GetValueOrDefault();
				if (packageSize > 0)
					this.ReportProgress((double)downloadedSize / packageSize);
			});
			try
			{
				await Task.Run(() =>
				{
					// get response
					this.logger.LogDebug($"Start downloading package from '{packageUri}'");
					using var response = WebRequest.Create(packageUri).GetResponse();
					using var downloadStream = response.GetResponseStream();

					// cancellation check
					if (this.cancellationTokenSource.IsCancellationRequested)
					{
						this.logger.LogWarning("Downloading has been cancelled");
						throw new TaskCanceledException();
					}

					// get package size
					var packageSize = 0L;
					try
					{
						packageSize = response.ContentLength;
					}
					catch
					{ }
					if (packageSize > 0)
					{
						this.logger.LogDebug($"Size of package to download: {packageSize}");
						this.SynchronizationContext.Post(() =>
						{
							this.PackageSize = packageSize;
							this.OnPropertyChanged(nameof(PackageSize));
							this.ReportProgress(0);
						});
					}

					// download
					using var packageFileStream = new FileStream(packageFilePath, FileMode.Create, FileAccess.Write);
					var buffer = new byte[65536];
					var readCount = downloadStream.Read(buffer, 0, buffer.Length);
					while (readCount > 0)
					{
						downloadedSize += readCount;
						reportProgressAction.Schedule(100);
						packageFileStream.Write(buffer, 0, readCount);
						if (this.cancellationTokenSource.IsCancellationRequested)
						{
							this.logger.LogWarning("Downloading has been cancelled");
							throw new TaskCanceledException();
						}
						readCount = downloadStream.Read(buffer, 0, buffer.Length);
					}
					reportProgressAction.Reschedule();
					this.logger.LogDebug($"Complete downloading package from '{packageUri}'");
				});
			}
			catch (Exception ex)
			{
				Global.RunWithoutErrorAsync(() => File.Delete(packageFilePath));
				this.CompleteUpdating(ex);
				return null;
			}
			if (this.cancellationTokenSource.IsCancellationRequested)
			{
				Global.RunWithoutErrorAsync(() => File.Delete(packageFilePath));
				return null;
			}

			// complete
			return packageFilePath;
		}


		/// <summary>
		/// Get latest exception occurred while updating.
		/// </summary>
		public Exception? Exception { get; private set; }


		// Install downloaded package.
		async Task<bool> InstallPackageAsync(string packageFilePath, string backupDirectory)
		{
			// check state
			if (this.state != UpdaterState.InstallingPackage)
				return false;
			if (this.cancellationTokenSource.IsCancellationRequested)
				return false;

			// prepare installer
			var installer = this.packageInstaller.AsNonNull();
			try
			{
				installer.PackageFileName = packageFilePath;
				installer.PropertyChanged += (_, e) =>
				{
					if (e.PropertyName == nameof(IPackageInstaller.Progress))
						this.ReportProgress(installer.Progress);
				};
				installer.TargetDirectoryPath = this.applicationDirectoryPath;
			}
			catch (Exception ex)
			{
				this.CompleteUpdating(ex);
				return false;
			}

			// install
			var exception = (Exception?)null;
			try
			{
				this.ReportProgress(0);
				await installer.StartAndWaitAsync(this.cancellationTokenSource.Token);
			}
			catch (Exception ex)
			{
				exception = ex;
			}
			if (exception != null
				|| installer.State == UpdaterComponentState.Failed
				|| this.cancellationTokenSource.IsCancellationRequested)
			{
				if (this.state == UpdaterState.InstallingPackage)
					this.ChangeState(UpdaterState.RestoringApplication);
				await this.RestoreApplicationAsync(backupDirectory);
				if (installer.State == UpdaterComponentState.Failed && exception == null)
					exception = installer.Exception ?? new Exception("Failed to install package.");
				this.CompleteUpdating(exception);
				return false;
			}

			// complete
			return true;
		}


		/// <summary>
		/// Check whether updating process is cancellable or not.
		/// </summary>
		public bool IsCancellable { get; private set; }


		/// <summary>
		/// Check whether updating process is cancelling or not.
		/// </summary>
		public bool IsCancelling { get; private set; }


		/// <summary>
		/// Check whether updating process is on-going or not.
		/// </summary>
		public bool IsUpdating { get; private set; }


		/// <summary>
		/// Raise <see cref="PropertyChanged"/> event.
		/// </summary>
		/// <param name="propertyName">Name of changed property.</param>
		protected virtual void OnPropertyChanged(string propertyName) => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));


		/// <summary>
		/// Get or set <see cref="IPackageInstaller"/> to install downloaded package.
		/// </summary>
		public IPackageInstaller? PackageInstaller
		{
			get => this.packageInstaller;
			set
			{
				this.VerifyAccess();
				this.VerifyDisposed();
				this.VerifyInitializing();
				if (this.packageInstaller == value)
					return;
				value?.VerifyAccess();
				this.packageInstaller = value;
				this.OnPropertyChanged(nameof(PackageInstaller));
			}
		}


		/// <summary>
		/// Get or set <see cref="IPackageResolver"/> to resolve package to download.
		/// </summary>
		/// <remarks>You need to setup <see cref="IPackageResolver.Source"/> before calling <see cref="Start"/>.</remarks>
		public IPackageResolver? PackageResolver
		{
			get => this.packageResolver;
			set
			{
				this.VerifyAccess();
				this.VerifyDisposed();
				this.VerifyInitializing();
				if (this.packageResolver == value)
					return;
				value?.VerifyAccess();
				this.packageResolver = value;
				this.OnPropertyChanged(nameof(PackageResolver));
			}
		}


		/// <summary>
		/// Get size of package in bytes.
		/// </summary>
		public long? PackageSize { get; private set; }


		/// <summary>
		/// Get current progress of updating. Range is [0.0, 1.0] or <see cref="double.NaN"/> if progress is unavailable.
		/// </summary>
		public double Progress { get => this.progress; }


		/// <summary>
		/// Raise when property changed.
		/// </summary>
		public event PropertyChangedEventHandler? PropertyChanged;


		// Report progress
		void ReportProgress(double progress)
		{
			if (!this.CheckAccess())
			{
				this.SynchronizationContext.Post(() => this.ReportProgress(progress));
				return;
			}
			if (!this.IsUpdating)
				return;
			if (double.IsNaN(progress))
			{
				if (double.IsNaN(this.progress))
					return;
			}
			else
			{
				progress = Math.Min(Math.Max(0, progress), 1);
				if (!double.IsNaN(this.progress) && Math.Abs(this.progress - progress) <= 0.001)
					return;
			}
			this.progress = progress;
			this.OnPropertyChanged(nameof(Progress));
		}


		// Resolve package.
		async Task<Uri?> ResolvePackageAsync()
		{
			// check state
			if (this.state != UpdaterState.ResolvingPackage)
				return null;
			if (this.cancellationTokenSource.IsCancellationRequested)
				return null;
			var packageResolver = this.packageResolver.AsNonNull();
			try
			{
				await packageResolver.StartAndWaitAsync(this.cancellationTokenSource.Token);
			}
			catch (Exception ex)
			{
				this.CompleteUpdating(ex);
				return null;
			}
			if (packageResolver.State == UpdaterComponentState.Failed)
			{
				this.CompleteUpdating(packageResolver.Exception ?? new Exception("Failed to resolve update package."));
				return null;
			}
			if (this.cancellationTokenSource.IsCancellationRequested)
			{
				this.CompleteUpdating(null);
				return null;
			}
			return packageResolver.PackageUri.Also(it =>
			{
				if (it == null)
					this.CompleteUpdating(new Exception("No package URI resolved."));
			});
		}


		// Restore backuped application.
		async Task RestoreApplicationAsync(string backupDirectory)
		{
			// check state
			if (this.state != UpdaterState.RestoringApplication)
				return;

			// report progress
			this.ReportProgress(double.NaN);

			// delete installed files
			var installedFilePaths = this.packageInstaller.AsNonNull().InstalledFilePaths;
			await Task.Run(() =>
			{
				foreach (var installedFilePath in installedFilePaths)
					Global.RunWithoutError(() => File.Delete(installedFilePath));
			});

			// restore
			try
			{
				await Task.Run(() => this.CopyFiles(backupDirectory, this.applicationDirectoryPath.AsNonNull(), new CancellationToken(), false));
			}
			catch
			{ }
		}


		/// <summary>
		/// Start updating application.
		/// </summary>
		/// <returns>True if updating process has been started successfully.</returns>
		public bool Start()
		{
			// check state
			this.VerifyAccess();
			this.VerifyDisposed();
			if (this.state != UpdaterState.Initializing)
			{
				if (this.IsCancelling)
					return false;
				return this.IsUpdating;
			}
			if (this.packageResolver == null)
				return false;
			if (this.packageInstaller == null)
				return false;
			if (string.IsNullOrWhiteSpace(this.applicationDirectoryPath))
				return false;

			// change state
			if (!this.ChangeState(UpdaterState.Starting))
				return false;
			this.IsUpdating = true;
			this.OnPropertyChanged(nameof(IsUpdating));
			if (this.state != UpdaterState.Starting)
				return false;
			this.IsCancellable = true;
			this.OnPropertyChanged(nameof(IsCancellable));
			if (this.state != UpdaterState.Starting)
				return false;

			// start updating
			this.Update();
			return true;
		}


		/// <summary>
		/// Start updating application and wait for completion asynchronously.
		/// </summary>
		/// <param name="cancellationToken">Cancellation token.</param>
		/// <returns>True if updating process has been started successfully.</returns>
		public async Task<bool> StartAndWaitAsync(CancellationToken cancellationToken)
		{
			// start
			if (!this.Start())
				return false;

			// wait for complete
			await Task.Run(() =>
			{
				while (!cancellationToken.IsCancellationRequested)
				{
					if (!this.IsUpdating)
						return;
					lock (this.waitingSyncLock)
					{
						if (this.IsUpdating)
							Monitor.Wait(this.waitingSyncLock, 1000);
					}
				}
			});
			if (this.IsUpdating)
				throw new TaskCanceledException();
			return true;
		}


		/// <summary>
		/// Get current state.
		/// </summary>
		public UpdaterState State { get => this.state; }


		// Updating process.
		async void Update()
		{
			// check state
			if (this.state != UpdaterState.Starting)
				return;

			// resolve package
			if (!this.ChangeState(UpdaterState.ResolvingPackage))
				return;
			var packageUri = await this.ResolvePackageAsync();
			if (packageUri == null)
				return;

			// start backing up application
			var backupTask = this.BackUpApplicationAsync();

			// download package
			if (!this.ChangeState(UpdaterState.DownloadingPackage))
				return;
			var packageFilePath = await this.DownloadPackageAsync(packageUri);
			if (packageFilePath == null)
				return;

			// wait for backing up application
			var backupDirectory = (string?)null;
			if (backupTask.IsCompleted)
				backupDirectory = backupTask.Result;
			else
			{
				if (!this.ChangeState(UpdaterState.BackingUpApplication))
				{
					Global.RunWithoutErrorAsync(() => File.Delete(packageFilePath));
					return;
				}
				backupDirectory = await backupTask;
			}
			if (backupDirectory == null)
			{
				Global.RunWithoutErrorAsync(() => File.Delete(packageFilePath));
				return;
			}

			// verify and install package
			var packageVerified = false;
			var packageInstalled = false;
			if (this.ChangeState(UpdaterState.VerifyingPackage))
			{
				packageVerified = await this.VerifyPackage(packageFilePath, this.packageResolver?.MD5, this.packageResolver?.SHA256, this.packageResolver?.SHA512);
				if (packageVerified && this.ChangeState(UpdaterState.InstallingPackage))
					packageInstalled = await this.InstallPackageAsync(packageFilePath, backupDirectory);
			}

			// delete temp files
			_ = Task.Run(() =>
			{
				Global.RunWithoutError(() => File.Delete(packageFilePath));
				Global.RunWithoutError(() => Directory.Delete(backupDirectory, true));
			});

			// complete
			if (!packageVerified)
				this.CompleteUpdating(new Exception("Package verification failed."));
			else if (!packageInstalled)
				this.CompleteUpdating(new Exception("Package installation failed."));
			else
				this.CompleteUpdating(null);
		}


		// Throw exception if current state is not initializing.
		void VerifyInitializing()
		{
			if (this.state != UpdaterState.Initializing)
				throw new InvalidOperationException($"Cannot perform oprtation when state is {this.state}.");
		}


		// Verify package.
		async Task<bool> VerifyPackage(string packageFileName, string? md5, string? sha256, string? sha512)
		{
			// check state
			if (this.state != UpdaterState.VerifyingPackage)
				return false;
			if (this.cancellationTokenSource.IsCancellationRequested)
				return false;

			// prepare comparison
			bool compareHash(string hash, byte[] hashBytes)
			{
				if (hash.Length != hashBytes.Length * 2)
					return false;
				var hashCheckingIndex = hash.Length - 2;
				for (var i = hashBytes.Length - 1; i >= 0; --i, hashCheckingIndex -= 2)
				{
					var byteString = hashBytes[i].ToString("X2");
					if (char.ToUpper(hash[hashCheckingIndex]) != byteString[0] || char.ToUpper(hash[hashCheckingIndex + 1]) != byteString[1])
						return false;
				}
				return true;
			}

			// verify with MD5
			if (md5 != null)
			{
				var verified = await Task.Run(() =>
				{
					try
                    {
						using var hashAlgorithm = MD5.Create();
						using var stream = new FileStream(packageFileName, FileMode.Open, FileAccess.Read);
						var hashBytes = hashAlgorithm.ComputeHash(stream);
						return compareHash(md5, hashBytes);
                    }
					catch
                    {
						return false;
                    }
				});
				if (!verified)
					return false;
			}

			// verify with SHA256
			if (sha256 != null)
			{
				var verified = await Task.Run(() =>
				{
					try
					{
						using var hashAlgorithm = SHA256.Create();
						using var stream = new FileStream(packageFileName, FileMode.Open, FileAccess.Read);
						var hashBytes = hashAlgorithm.ComputeHash(stream);
						return compareHash(sha256, hashBytes);
					}
					catch
					{
						return false;
					}
				});
				if (!verified)
					return false;
			}

			// verify with SHA512
			if (sha512 != null)
			{
				var verified = await Task.Run(() =>
				{
					try
					{
						using var hashAlgorithm = SHA512.Create();
						using var stream = new FileStream(packageFileName, FileMode.Open, FileAccess.Read);
						var hashBytes = hashAlgorithm.ComputeHash(stream);
						return compareHash(sha512, hashBytes);
					}
					catch
					{
						return false;
					}
				});
				if (!verified)
					return false;
			}

			// complete
			return true;
		}
	}


	/// <summary>
	/// State of <see cref="Updater"/>.
	/// </summary>
	public enum UpdaterState
	{
		/// <summary>
		/// Initializing.
		/// </summary>
		Initializing,
		/// <summary>
		/// Disposed.
		/// </summary>
		Disposed,
		/// <summary>
		/// Starting.
		/// </summary>
		Starting,
		/// <summary>
		/// Resolving package to download.
		/// </summary>
		ResolvingPackage,
		/// <summary>
		/// Downloading package.
		/// </summary>
		DownloadingPackage,
		/// <summary>
		/// Verifying downloaded package.
		/// </summary>
		VerifyingPackage,
		/// <summary>
		/// Installing package.
		/// </summary>
		InstallingPackage,
		/// <summary>
		/// Backing up current application.
		/// </summary>
		BackingUpApplication,
		/// <summary>
		/// Restoring to current application.
		/// </summary>
		RestoringApplication,
		/// <summary>
		/// Cancelled.
		/// </summary>
		Cancelled,
		/// <summary>
		/// Failed.
		/// </summary>
		Failed,
		/// <summary>
		/// Succeeded.
		/// </summary>
		Succeeded,
	}
}
