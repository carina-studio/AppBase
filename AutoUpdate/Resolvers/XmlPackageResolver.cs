using CarinaStudio.IO;
using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace CarinaStudio.AutoUpdate.Resolvers
{
	/// <summary>
	/// <see cref="IPackageResolver"/> to resolve package manifest in XML format.
	/// </summary>
	public class XmlPackageResolver : BasePackageResolver
	{
		/// <summary>
		/// Initialize new <see cref="XmlPackageResolver"/> instance.
		/// </summary>
		public XmlPackageResolver()
		{ }


		/// <summary>
		/// Perform operation asynchronously.
		/// </summary>
		/// <param name="cancellationToken">Cancellation token.</param>
		/// <returns>Task of performing operation.</returns>
		protected override async Task PerformOperationAsync(CancellationToken cancellationToken)
		{
			// get XML data
			using var streamReader = await this.Source.AsNonNull().OpenStreamReaderAsync(StreamAccess.Read, Encoding.UTF8, cancellationToken);

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
				// parse as XML document
				var xmlDocument = new XmlDocument().Also(it => it.Load(streamReader));

				// cancellation check
				if (cancellationToken.IsCancellationRequested)
					throw new TaskCanceledException();

				// find root node
				var rootNode = xmlDocument.Let(it =>
				{
					if (!it.HasChildNodes)
						return null;
					var node = it.FirstChild;
					while (node != null)
					{
						if (node.NodeType == XmlNodeType.Element && node.Name == "PackageManifest")
							return node;
						node = node.NextSibling;
					}
					return null;
				}) ?? throw new XmlException("Node of package manifest not fount.");

				// get name
				rootNode.Attributes?["Name"]?.Let(it => appName = it.Value);

				// get version
				rootNode.Attributes?["Version"]?.Let(it => Version.TryParse(it.Value, out packageVersion));

				// get page URI
				rootNode.Attributes?["PageUri"]?.Let(it => Uri.TryCreate(it.Value, UriKind.Absolute, out pageUri));

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

				// check installed .NET version
				var installedFrameworkVersion = Platform.GetInstalledFrameworkVersion();

				// find package URI
				if (!rootNode.HasChildNodes)
					throw new XmlException("Package not fount.");
				var packageNode = rootNode.FirstChild;
				while (packageNode != null)
				{
					try
					{
						if (packageNode.NodeType == XmlNodeType.Element && packageNode.Name == "Package")
						{
							// get URI
							var packageAttrs = packageNode.Attributes;
							if (packageAttrs == null)
								continue;
							var uri = packageAttrs["Uri"]?.Let(it =>
							{
								Uri.TryCreate(it.Value, UriKind.Absolute, out var uri);
								return uri;
							});
							if (uri == null)
								continue;

							// check operating system
							var osAttribute = packageAttrs["OperatingSystem"];
							var isOsMatched = osAttribute != null && osAttribute.Value == osName;

							// check architecture
							var archArrtibute = packageAttrs["Architecture"];
							var isArchMatched = archArrtibute != null && archArrtibute.Value == archName;

							// check framework version
							var frameworkVersionAttr = packageAttrs["FrameworkVersion"];
							var isFrameworkMatched = frameworkVersionAttr == null
								|| (Version.TryParse(frameworkVersionAttr.Value, out var version) 
									&& installedFrameworkVersion != null 
									&& version <= installedFrameworkVersion);

							// select package
							if (osAttribute == null && archArrtibute == null && frameworkVersionAttr == null)
							{
								genericPackageUri = uri;
								packageAttrs["MD5"]?.Let(attr => genericMd5 = attr.Value);
								packageAttrs["SHA256"]?.Let(attr => genericSha256 = attr.Value);
								packageAttrs["SHA512"]?.Let(attr => genericSha512 = attr.Value);
							}
							else if (isOsMatched && isArchMatched && isFrameworkMatched)
							{
								if (this.SelfContainedPackageOnly && frameworkVersionAttr != null)
									continue;
								if (frameworkVersionAttr == null)
								{
									selfContainedPackageUri = uri;
									packageAttrs["MD5"]?.Let(attr => selfContainedMd5 = attr.Value);
									packageAttrs["SHA256"]?.Let(attr => selfContainedSha256 = attr.Value);
									packageAttrs["SHA512"]?.Let(attr => selfContainedSha512 = attr.Value);
								}
								else
								{
									packageUri = uri;
									packageAttrs["MD5"]?.Let(attr => md5 = attr.Value);
									packageAttrs["SHA256"]?.Let(attr => sha256 = attr.Value);
									packageAttrs["SHA512"]?.Let(attr => sha512 = attr.Value);
								}
							}
						}
					}
					finally
					{
						packageNode = packageNode.NextSibling;
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
}
