using CarinaStudio.Threading;
using System;

namespace CarinaStudio.AutoUpdate.Resolvers
{
	/// <summary>
	/// Base implementation of <see cref="IPackageResolver"/>.
	/// </summary>
	public abstract class BasePackageResolver : BaseUpdaterComponent, IPackageResolver
	{
		// Fields.
		string? applicationName;
		Uri? packageManifestUri;
		Uri? packageUri;
		Version? packageVersion;
		Uri? pageUri;


		/// <summary>
		/// Initialize new <see cref="BasePackageResolver"/> instance.
		/// </summary>
		/// <param name="app">Application.</param>
		public BasePackageResolver(IApplication app) : base(app)
		{ }


		/// <summary>
		/// Get or set resolved application name.
		/// </summary>
		public string? ApplicationName
		{
			get => this.applicationName;
			protected set
			{
				this.VerifyAccess();
				if (this.applicationName == value)
					return;
				this.applicationName = value;
				this.OnPropertyChanged(nameof(ApplicationName));
			}
		}


		/// <summary>
		/// Get or set URI to get package manifest to resolve.
		/// </summary>
		public Uri? PackageManifestUri
		{
			get => this.packageManifestUri;
			set
			{
				this.VerifyAccess();
				this.VerifyDisposed();
				this.VerifyInitializing();
				if (this.packageManifestUri == value)
					return;
				this.packageManifestUri = value;
				this.OnPropertyChanged(nameof(PackageManifestUri));
			}
		}


		/// <summary>
		/// Get or set resolved URI to download update package.
		/// </summary>
		public Uri? PackageUri
		{
			get => this.packageUri;
			protected set
			{
				this.VerifyAccess();
				if (this.packageUri == value)
					return;
				this.packageUri = value;
				this.OnPropertyChanged(nameof(PackageUri));
			}
		}


		/// <summary>
		/// Get resolved version of update package.
		/// </summary>
		public Version? PackageVersion
		{
			get => this.packageVersion;
			protected set
			{
				this.VerifyAccess();
				if (this.packageVersion == value)
					return;
				this.packageVersion = value;
				this.OnPropertyChanged(nameof(PackageVersion));
			}
		}


		/// <summary>
		/// Get resolved URI of web page.
		/// </summary>
		public Uri? PageUri
		{
			get => this.pageUri;
			protected set
			{
				this.VerifyAccess();
				if (this.pageUri == value)
					return;
				this.pageUri = value;
				this.OnPropertyChanged(nameof(PageUri));
			}
		}


		/// <summary>
		/// Validate parameters to start performing operation.
		/// </summary>
		/// <returns>True if all parameters are valid.</returns>
		protected override bool ValidateParametersToStart()
		{
			return base.ValidateParametersToStart() && this.packageManifestUri != null;
		}
	}
}
