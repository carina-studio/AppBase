using CarinaStudio.IO;
using System;
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
		public JsonPackageResolver() 
		{ }


		/// <summary>
		/// Perform operation asynchronously.
		/// </summary>
		/// <param name="cancellationToken">Cancellation token.</param>
		/// <returns>Task of performing operation.</returns>
		protected override async Task PerformOperationAsync(CancellationToken cancellationToken)
		{
			// get JSON data
			using var stream = await this.Source.AsNonNull().OpenStreamAsync(StreamAccess.Read, cancellationToken);

			// parse package manifest
			var appName = (string?)null;
			var packageVersion = (Version?)null;
			var pageUri = (Uri?)null;
			var packageUri = (Uri?)null;
			var md5 = (string?)null;
			var sha256 = (string?)null;
			var sha512 = (string?)null;
			var selfContainedPackageUri = (Uri?)null;
			var selfContainedMd5 = (string?)null;
			var selfContainedSha256 = (string?)null;
			var selfContainedSha512 = (string?)null;
			var genericPackageUri = (Uri?)null;
			var genericMd5 = (string?)null;
			var genericSha256 = (string?)null;
			var genericSha512 = (string?)null;
			await Task.Run(() =>
			{
				// parse as JSON document
				using var jsonDocument = JsonDocument.Parse(stream);
				var rootObject = jsonDocument.RootElement;
				if (rootObject.ValueKind != JsonValueKind.Object)
					throw new JsonException("Root element is not an object.");

				// get application name
				if (rootObject.TryGetProperty("Name", out var jsonValue)
					&& jsonValue.ValueKind == JsonValueKind.String)
				{
					appName = jsonValue.GetString();
				}

				// get version
				if (rootObject.TryGetProperty("Version", out jsonValue)
					&& jsonValue.ValueKind == JsonValueKind.String)
				{
					Version.TryParse(jsonValue.GetString().AsNonNull(), out packageVersion);
				}

				// get page URI
				if (rootObject.TryGetProperty("PageUri", out jsonValue)
					&& jsonValue.ValueKind == JsonValueKind.String)
				{
					Uri.TryCreate(jsonValue.GetString().AsNonNull(), UriKind.Absolute, out pageUri);
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
				var archName = RuntimeInformation.OSArchitecture.ToString();

				// check installed runtime version
				var installedRuntimeVersion = Platform.GetInstalledRuntimeVersion();

				// find package URI
				if (!rootObject.TryGetProperty("Packages", out var jsonPackageListElement))
					throw new ArgumentException("No packages list found.");
				if (jsonPackageListElement.ValueKind != JsonValueKind.Array)
					throw new ArgumentException("Package list is not an array.");
				foreach (var jsonPackageElement in jsonPackageListElement.EnumerateArray())
				{
					// check JSON element type
					if (jsonPackageElement.ValueKind != JsonValueKind.Object)
						continue;

					// get package URI
					if (!jsonPackageElement.TryGetProperty("Uri", out jsonValue)
						|| jsonValue.ValueKind != JsonValueKind.String
						|| !Uri.TryCreate(jsonValue.GetString().AsNonNull(), UriKind.Absolute, out var uri))
					{
						continue;
					}

					// check OS
					var hasOsProperty = jsonPackageElement.TryGetProperty("OperatingSystem", out jsonValue);
					var isOsMatched = (hasOsProperty && jsonValue.ValueKind == JsonValueKind.String && jsonValue.GetString() == osName);

					// check CPU architecture
					var hasArchProperty = jsonPackageElement.TryGetProperty("Architecture", out jsonValue);
					var isArchMatched = (hasArchProperty && jsonValue.ValueKind == JsonValueKind.String && jsonValue.GetString() == archName);

					// check runtime version
					var hasRuntimeProperty = jsonPackageElement.TryGetProperty("RuntimeVersion", out jsonValue);
					var runtimeVersion = hasRuntimeProperty && jsonValue.ValueKind == JsonValueKind.String
						? (Version.TryParse(jsonValue.GetString(), out var version) ? version : null)
						: null;
					var isRuntimeMatched = runtimeVersion == null || (installedRuntimeVersion != null && runtimeVersion <= installedRuntimeVersion);

					// select package
					if (!hasOsProperty && !hasArchProperty && !hasRuntimeProperty)
					{
						genericPackageUri = uri;
						if (jsonPackageElement.TryGetProperty("MD5", out jsonValue) && jsonValue.ValueKind == JsonValueKind.String)
							genericMd5 = jsonValue.GetString();
						if (jsonPackageElement.TryGetProperty("SHA256", out jsonValue) && jsonValue.ValueKind == JsonValueKind.String)
							genericSha256 = jsonValue.GetString();
						if (jsonPackageElement.TryGetProperty("SHA512", out jsonValue) && jsonValue.ValueKind == JsonValueKind.String)
							genericSha512 = jsonValue.GetString();
					}
					else if (isOsMatched && isArchMatched && isRuntimeMatched)
					{
						if (this.SelfContainedPackageOnly && runtimeVersion != null)
							continue;
						if (runtimeVersion == null)
						{
							selfContainedPackageUri = uri;
							if (jsonPackageElement.TryGetProperty("MD5", out jsonValue) && jsonValue.ValueKind == JsonValueKind.String)
								selfContainedMd5 = jsonValue.GetString();
							if (jsonPackageElement.TryGetProperty("SHA256", out jsonValue) && jsonValue.ValueKind == JsonValueKind.String)
								selfContainedSha256 = jsonValue.GetString();
							if (jsonPackageElement.TryGetProperty("SHA512", out jsonValue) && jsonValue.ValueKind == JsonValueKind.String)
								selfContainedSha512 = jsonValue.GetString();
						}
						else
						{
							packageUri = uri;
							if (jsonPackageElement.TryGetProperty("MD5", out jsonValue) && jsonValue.ValueKind == JsonValueKind.String)
								md5 = jsonValue.GetString();
							if (jsonPackageElement.TryGetProperty("SHA256", out jsonValue) && jsonValue.ValueKind == JsonValueKind.String)
								sha256 = jsonValue.GetString();
							if (jsonPackageElement.TryGetProperty("SHA512", out jsonValue) && jsonValue.ValueKind == JsonValueKind.String)
								sha512 = jsonValue.GetString();
						}
					}
				}
			});
			if (cancellationToken.IsCancellationRequested)
				throw new TaskCanceledException();

			// save result
			this.ApplicationName = appName;
			this.PageUri = pageUri;
			this.PackageVersion = packageVersion;
			if (packageUri != null)
			{
				this.PackageUri = packageUri;
				this.MD5 = md5;
				this.SHA256 = sha256;
				this.SHA512 = sha512;
			}
			else if (selfContainedPackageUri != null)
			{
				this.PackageUri = selfContainedPackageUri;
				this.MD5 = selfContainedMd5;
				this.SHA256 = selfContainedSha256;
				this.SHA512 = selfContainedSha512;
			}
			else if (genericPackageUri != null)
			{
				this.PackageUri = genericPackageUri;
				this.MD5 = genericMd5;
				this.SHA256 = genericSha256;
				this.SHA512 = genericSha512;
			}
			else
				throw new ArgumentException("Package URI not found.");
		}
	}
#endif
}
