using CarinaStudio.IO;
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
		/// Get MD5 hash code of update package.
		/// </summary>
		string? MD5 { get; }


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


		/// <summary>
		/// Get SHA256 hash code of update package.
		/// </summary>
		string? SHA256 { get; }


		/// <summary>
		/// Get SHA512 hash code of update package.
		/// </summary>
		string? SHA512 { get; }


		/// <summary>
		/// Get or set source <see cref="IStreamProvider"/> to provide data of package manifest to be resolved.
		/// </summary>
		IStreamProvider? Source { get; set; }
	}
}
