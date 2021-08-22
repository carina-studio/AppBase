using CarinaStudio.Threading;
using System;
using System.Net;
using System.Runtime.InteropServices;
#if !NETSTANDARD
using System.Text.Json;
#endif
using System.Threading;
using System.Threading.Tasks;

namespace CarinaStudio.AutoUpdate.Resolvers
{
#if !NETSTANDARD
	/// <summary>
	/// <see cref="IPackageResolver"/> to resolve package manifest in JSON format.
	/// </summary>
	public class JsonPackageResolver : BasePackageResolver
	{
		/// <summary>
		/// Initialize new <see cref="JsonPackageResolver"/> instance.
		/// </summary>
		/// <param name="app">Application.</param>
		public JsonPackageResolver(IApplication app) : base(app)
		{ }


		/// <summary>
		/// Perform operation asynchronously.
		/// </summary>
		/// <param name="cancellationToken">Cancellation token.</param>
		/// <returns>Task of performing operation.</returns>
		protected override Task PerformOperationAsync(CancellationToken cancellationToken) => Task.Run(() =>
		{
			// download JSON data
			using var response = WebRequest.Create(this.PackageManifestUri.AsNonNull()).GetResponse();
			using var jsonDocument = response.GetResponseStream().Use(stream => JsonDocument.Parse(stream));
			var rootObject = jsonDocument.RootElement;
			if (rootObject.ValueKind != JsonValueKind.Object)
				throw new ArgumentException("Root element is not an object.");

			// get application name
			if (rootObject.TryGetProperty("Name", out var jsonValue)
				&& jsonValue.ValueKind == JsonValueKind.String)
			{
				var appName = jsonValue.GetString();
				this.SynchronizationContext.Post(() => this.ApplicationName = appName);
			}

			// get version
			if (rootObject.TryGetProperty("Version", out jsonValue)
				&& jsonValue.ValueKind == JsonValueKind.String
				&& Version.TryParse(jsonValue.GetString().AsNonNull(), out var version))
			{
				this.SynchronizationContext.Post(() => this.PackageVersion = version);
			}

			// get page URI
			if (rootObject.TryGetProperty("ReleasePageUrl", out jsonValue)
				&& jsonValue.ValueKind == JsonValueKind.String
				&& Uri.TryCreate(jsonValue.GetString().AsNonNull(), UriKind.Absolute, out var pageUri))
			{
				this.SynchronizationContext.Post(() => this.PageUri = pageUri);
			}

			// check platform
			var osName = Global.Run(() =>
			{
				if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
					return nameof(OSPlatform.Windows);
				if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
					return nameof(OSPlatform.Linux);
				if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
					return nameof(OSPlatform.OSX);
				return "";
			});
			if (string.IsNullOrEmpty(osName))
				throw new ArgumentException("Unknown operating system.");
			var platformName = RuntimeInformation.ProcessArchitecture.ToString();

			// find package URI
			if (!rootObject.TryGetProperty("Packages", out var jsonPackageListElement))
				throw new ArgumentException("No packages list found.");
			if (jsonPackageListElement.ValueKind != JsonValueKind.Array)
				throw new ArgumentException("Package list is not an array.");
			var genericPackageUri = (Uri?)null;
			foreach (var jsonPackageElement in jsonPackageListElement.EnumerateArray())
			{
				// check JSON element type
				if (jsonPackageElement.ValueKind != JsonValueKind.Object)
					continue;

				// get package URI
				if (!jsonPackageElement.TryGetProperty("Url", out jsonValue)
					|| jsonValue.ValueKind != JsonValueKind.String
					|| !Uri.TryCreate(jsonValue.GetString().AsNonNull(), UriKind.Absolute, out var packageUri))
				{
					continue;
				}

				// check OS
				if (!jsonPackageElement.TryGetProperty("OS", out jsonValue))
					genericPackageUri = packageUri;
				else if (jsonValue.GetString() != osName)
					continue;

				// check platform
				if (!jsonPackageElement.TryGetProperty("Platform", out jsonValue))
					genericPackageUri = packageUri;
				else if (jsonValue.GetString() != platformName)
					continue;

				// package found
				this.SynchronizationContext.Post(() => this.PackageUri = packageUri);
				return;
			}
			if (genericPackageUri != null)
				this.SynchronizationContext.Post(() => this.PackageUri = genericPackageUri);
			else
				throw new ArgumentException("Package URI not found.");
		});
	}
#endif
}
