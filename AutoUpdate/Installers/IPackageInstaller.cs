using System;
using System.Collections.Generic;

namespace CarinaStudio.AutoUpdate.Installers
{
	/// <summary>
	/// Object to install package on device.
	/// </summary>
	public interface IPackageInstaller : IUpdaterComponent
	{
		/// <summary>
		/// Get set of path of installed files.
		/// </summary>
		ISet<string> InstalledFilePaths { get; }


		/// <summary>
		/// Raised before installing a file.
		/// </summary>
		event Func<IPackageInstaller, string, bool>? InstallingFile;
		
		
		/// <summary>
		/// Check whether application icon has been updated after installation or not.
		/// </summary>
		bool IsApplicationIconUpdated { get; }


		/// <summary>
		/// Get or set name of package file to install.
		/// </summary>
		string? PackageFileName { get; set; }


		/// <summary>
		/// Get or set path of target directory to install package.
		/// </summary>
		string? TargetDirectoryPath { get; set; }
	}
}
