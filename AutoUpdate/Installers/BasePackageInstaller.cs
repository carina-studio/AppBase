#if !NET10_0_OR_GREATER
using CarinaStudio.Collections;
#endif
using CarinaStudio.IO;
using CarinaStudio.Threading;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Directory = System.IO.Directory;

namespace CarinaStudio.AutoUpdate.Installers;

/// <summary>
/// Base implementation of <see cref="IPackageInstaller"/>.
/// </summary>
public abstract class BasePackageInstaller : BaseUpdaterComponent, IPackageInstaller
{
	// Fields.
	readonly HashSet<string> installedFilePaths = new(PathEqualityComparer.Default);
	bool keepUnrelatedFiles = true;
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
	/// Copy the directory recursively and asynchronously.
	/// </summary>
	/// <param name="sourceDirectory">Path to source directory.</param>
	/// <param name="targetDirectory">Path to target directory.</param>
	/// <param name="cancellationToken"><see cref="CancellationToken"/> to cancel the operation.</param>
	/// <returns>Task of copying the directory.</returns>
	protected async Task CopyDirectoryAsync(string sourceDirectory, string targetDirectory, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		if (!Directory.Exists(targetDirectory))
			Directory.CreateDirectory(targetDirectory);
		foreach (var srcPath in Directory.EnumerateFileSystemEntries(sourceDirectory))
		{
			if (Directory.Exists(srcPath))
				await CopyDirectoryAsync(srcPath, Path.Combine(targetDirectory, Path.GetFileName(srcPath)), cancellationToken);
			else
			{
				cancellationToken.ThrowIfCancellationRequested();
				System.IO.File.Copy(srcPath, Path.Combine(targetDirectory, Path.GetFileName(srcPath)), true);
			}
		}
	}


	/// <inheritdoc/>
	public ISet<string> InstalledFilePaths { get; }


	/// <inheritdoc/>
	public event Func<IPackageInstaller, string, bool>? InstallingFile;


	/// <inheritdoc/>
	public virtual bool IsApplicationIconUpdated => false;
	
	
	/// <inheritdoc/>
	public bool KeepUnrelatedFiles
	{
		get => this.keepUnrelatedFiles;
		set
		{
			this.VerifyAccess();
			this.VerifyDisposed();
			this.VerifyInitializing();
			if (this.keepUnrelatedFiles == value)
				return;
			this.keepUnrelatedFiles = value;
			this.OnPropertyChanged(nameof(KeepUnrelatedFiles));
		}
	}


	/// <summary>
	/// Raise <see cref="InstallingFile"/> event.
	/// </summary>
	/// <param name="filePath">Path of file to be installed.</param>
	/// <returns>True to continue installation.</returns>
	protected bool OnInstallingFile(string filePath) =>
		this.InstallingFile?.Invoke(this, filePath) ?? true;


	/// <inheritdoc/>
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

	
	/// <inheritdoc/>
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