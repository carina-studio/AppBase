using CarinaStudio.Collections;
using CarinaStudio.IO;
using CarinaStudio.Threading;
using System;
using System.Collections.Generic;
using System.IO;

namespace CarinaStudio.AutoUpdate.Installers
{
	/// <summary>
	/// Base implementation of <see cref="IPackageInstaller"/>.
	/// </summary>
	public abstract class BasePackageInstaller : BaseUpdaterComponent, IPackageInstaller
	{
		// Fields.
		readonly HashSet<string> installedFilePaths = new(PathEqualityComparer.Default);
		string? packageFileName;
		string? targetDirectoryPath;


		/// <summary>
		/// Initialize new <see cref="BasePackageInstaller"/> instance.
		/// </summary>
		/// <param name="app">Application.</param>
		protected BasePackageInstaller(IApplication app) : base(app)
		{
			this.InstalledFilePaths = installedFilePaths.AsReadOnly();
		}


		/// <summary>
		/// Get set of path of installed files.
		/// </summary>
		public ISet<string> InstalledFilePaths { get; }


		/// <inheritdoc/>
		public event Func<IPackageInstaller, string, bool>? InstallingFile;


		/// <inheritdoc/>
		public virtual bool IsApplicationIconUpdated => false;


		/// <summary>
		/// Raise <see cref="InstallingFile"/> event.
		/// </summary>
		/// <param name="filePath">Path of file to be installed.</param>
		/// <returns>True to continue installation.</returns>
		protected bool OnInstallingFile(string filePath) =>
			this.InstallingFile?.Invoke(this, filePath) ?? true;


		/// <summary>
		/// Get or set name of package file to install.
		/// </summary>
		public string? PackageFileName 
		{
			get => this.packageFileName;
			set
			{
				this.VerifyAccess();
				this.VerifyDisposed();
				this.VerifyInitializing();
				if (this.packageFileName == value)
					return;
				this.packageFileName = value;
				this.OnPropertyChanged(nameof(PackageFileName));
			}
		}


		/// <summary>
		/// Report path of installed file.
		/// </summary>
		/// <param name="filePath">File path.</param>
		/// <remarks>The method can be called in any thread.</remarks>
		protected void ReportInstalledFilePath(string filePath)
		{
			if (!this.CheckAccess())
			{
				this.SynchronizationContext.Post(() => this.ReportInstalledFilePath(filePath));
				return;
			}
			if (!this.IsStartedOrCancelling() || this.IsCompletedOrCancelled() || this.IsDisposed)
				return;
			if (!Path.IsPathRooted(filePath))
				filePath = Path.Combine(this.targetDirectoryPath.AsNonNull(), filePath);
			this.installedFilePaths.Add(filePath);
		}


		/// <summary>
		/// Get or set path of target directory to install package.
		/// </summary>
		public string? TargetDirectoryPath 
		{
			get => this.targetDirectoryPath;
			set
			{
				this.VerifyAccess();
				this.VerifyDisposed();
				this.VerifyInitializing();
				if (this.targetDirectoryPath == value)
					return;
				this.targetDirectoryPath = value;
				this.OnPropertyChanged(nameof(TargetDirectoryPath));
			}
		}


		/// <summary>
		/// Validate parameters to start performing operation.
		/// </summary>
		/// <returns>True if all parameters are valid.</returns>
		protected override bool ValidateParametersToStart()
		{
			return base.ValidateParametersToStart()
				&& !string.IsNullOrWhiteSpace(this.packageFileName)
				&& !string.IsNullOrWhiteSpace(this.targetDirectoryPath);
		}
	}
}
