using System;
using System.Collections.Generic;

namespace CarinaStudio.AutoUpdate.Installers;

/// <summary>
/// Object to install package on device.
/// </summary>
public interface IPackageInstaller : IUpdaterComponent
{
	/// <summary>
	/// Get set of the path of the installed files.
	/// </summary>
	ISet<string> InstalledFilePaths { get; }


	/// <summary>
	/// Raised before installing a file.
	/// </summary>
	event Func<IPackageInstaller, string, bool>? InstallingFile;
	
	
	/// <summary>
	/// Check whether the application icon has been updated after installation or not.
	/// </summary>
	bool IsApplicationIconUpdated { get; }
	
	
	/// <summary>
	/// Get or set whether the unrelated files in the target directory should be kept after the installation or not.
	/// </summary>
	bool KeepUnrelatedFiles { get; set; }


	/// <summary>
	/// Get or set name of package file to install.
	/// </summary>
	string? PackageFileName { get; set; }


	/// <summary>
	/// Get or set the path of the target directory to install package.
	/// </summary>
	string? TargetDirectoryPath { get; set; }
}