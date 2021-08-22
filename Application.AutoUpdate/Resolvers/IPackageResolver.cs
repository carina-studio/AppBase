using System;

namespace CarinaStudio.AutoUpdate.Resolvers
{
	/// <summary>
	/// Update package resolver.
	/// </summary>
	public interface IPackageResolver : IUpdaterComponent
	{
		/// <summary>
		/// Get resolved application name.
		/// </summary>
		string? ApplicationName { get; }


		/// <summary>
		/// Get or set URI to get package manifest to resolve.
		/// </summary>
		Uri? PackageManifestUri { get; set; }


		/// <summary>
		/// Get resolved URI to download update package.
		/// </summary>
		Uri? PackageUri { get; }


		/// <summary>
		/// Get resolved version of update package.
		/// </summary>
		Version? PackageVersion { get; }


		/// <summary>
		/// Get resolved URI of web page.
		/// </summary>
		Uri? PageUri { get; }
	}
}
