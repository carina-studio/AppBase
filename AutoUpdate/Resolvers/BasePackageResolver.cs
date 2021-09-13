using CarinaStudio.IO;
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
		Uri? packageUri;
		Version? packageVersion;
		Uri? pageUri;
		IStreamProvider? source;


		/// <summary>
		/// Initialize new <see cref="BasePackageResolver"/> instance.
		/// </summary>
		public BasePackageResolver()
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
		/// Get or set source <see cref="IStreamProvider"/> to provide data of package manifest to be resolved.
		/// </summary>
		public virtual IStreamProvider? Source
		{
			get => this.source;
			set
			{
				this.VerifyAccess();
				this.VerifyDisposed();
				this.VerifyInitializing();
				if (this.source == value)
					return;
				this.source = value;
				this.OnPropertyChanged(nameof(Source));
			}
		}


		/// <summary>
		/// Validate parameters to start performing operation.
		/// </summary>
		/// <returns>True if all parameters are valid.</returns>
		protected override bool ValidateParametersToStart()
		{
			return base.ValidateParametersToStart() && this.source != null && this.source.CanOpenRead();
		}
	}
}
