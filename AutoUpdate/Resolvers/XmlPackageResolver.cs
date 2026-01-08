using CarinaStudio.IO;
using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace CarinaStudio.AutoUpdate.Resolvers;

/// <summary>
/// <see cref="IPackageResolver"/> to resolve package manifest in XML format.
/// </summary>
public class XmlPackageResolver : BasePackageResolver
{
	// Fields.
	readonly Version? appVersion;


	/// <summary>
	/// Initialize new <see cref="XmlPackageResolver"/> instance.
	/// </summary>
	/// <param name="app">Application.</param>
	/// <param name="baseAppVersion">Base version of application to update.</param>
	public XmlPackageResolver(IApplication app, Version? baseAppVersion) : base(app)
	{
		this.appVersion = baseAppVersion;
	}


	/// <summary>
	/// Perform operation asynchronously.
	/// </summary>
	/// <param name="cancellationToken">Cancellation token.</param>
	/// <returns>Task of performing operation.</returns>
	protected override async Task PerformOperationAsync(CancellationToken cancellationToken)
	{
		// check state
		var isWindows7 = (Platform.WindowsVersion == WindowsVersion.Windows7);
		if (this.SelfContainedPackageOnly && isWindows7)
			throw new NotSupportedException("Self-contained package is not supported on Windows 7.");

		// get XML data
		using var streamReader = await this.Source.AsNonNull().OpenStreamReaderAsync(StreamAccess.Read, Encoding.UTF8, cancellationToken);

		// parse package manifest
		var appName = (string?)null;
		var packageInformationalVersion = (string?)null;
		var packageVersion = (Version?)null;
		var pageUri = (Uri?)null;
		var packageUri = (Uri?)null;
		var baseVersion = (Version?)null;
		var md5 = (string?)null;
		var sha256 = (string?)null;
		var sha512 = (string?)null;
		var selfContainedPackageUri = (Uri?)null;
		var selfContainedBaseVersion = (Version?)null;
		var selfContainedMd5 = (string?)null;
		var selfContainedSha256 = (string?)null;
		var selfContainedSha512 = (string?)null;
		var genericPackageUri = (Uri?)null;
		var genericBaseVersion = (Version?)null;
		var genericMd5 = (string?)null;
		var genericSha256 = (string?)null;
		var genericSha512 = (string?)null;
		await Task.Run(() =>
		{
			// parse as XML document
			var xmlDocument = new XmlDocument().Also(it => it.Load(streamReader));

			// cancellation check
			cancellationToken.ThrowIfCancellationRequested();

			// find root node
			var rootNode = xmlDocument.Let(it =>
			{
				if (!it.HasChildNodes)
					return null;
				var node = it.FirstChild;
				while (node is not null)
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
			
			// get informational version
			packageInformationalVersion = rootNode.Attributes?["InformationalVersion"]?.Value;

			// get page URI
			rootNode.Attributes?["PageUri"]?.Let(it => Uri.TryCreate(it.Value, UriKind.Absolute, out pageUri));

			// check platform
			var osName = Global.Run(() =>
			{
				if (Platform.IsWindows)
					return nameof(OSPlatform.Windows);
				if (Platform.IsLinux)
					return nameof(OSPlatform.Linux);
				if (Platform.IsMacOS)
					return nameof(OSPlatform.OSX);
				return "";
			});
			if (string.IsNullOrEmpty(osName))
				throw new ArgumentException("Unknown operating system.");
			var archName = RuntimeInformation.OSArchitecture.ToString();

			// check installed runtime version
			var installedRuntimeVersion = Platform.GetInstalledRuntimeVersion();

			// find package URI
			if (!rootNode.HasChildNodes)
				throw new XmlException("Package not fount.");
			var packageNode = rootNode.FirstChild;
			while (packageNode is not null)
			{
				try
				{
					if (packageNode.NodeType == XmlNodeType.Element && packageNode.Name == "Package")
					{
						// get URI
						var packageAttrs = packageNode.Attributes;
						if (packageAttrs is null)
							continue;
						var uri = packageAttrs["Uri"]?.Let(it =>
						{
							Uri.TryCreate(it.Value, UriKind.Absolute, out var uri);
							return uri;
						});
						if (uri is null)
							continue;

						// check operating system
						var osAttribute = packageAttrs["OperatingSystem"];
						var isOsMatched = osAttribute is not null && osAttribute.Value == osName;

						// check architecture
						var archArrtibute = packageAttrs["Architecture"];
						var isArchMatched = archArrtibute is not null && archArrtibute.Value == archName;

						// check runtime version
						var runtimeVersionAttr = packageAttrs["RuntimeVersion"];
						var isruntimeMatched = runtimeVersionAttr is null
							|| (Version.TryParse(runtimeVersionAttr.Value, out var version) 
								&& installedRuntimeVersion is not null 
								&& version <= installedRuntimeVersion);
						
						// check base version
						var trgetBaseVersionAttr = packageAttrs["BaseVersion"];
						var targetBaseVersion = trgetBaseVersionAttr is not null && Version.TryParse(trgetBaseVersionAttr.Value, out version)
							? version 
							: null;
						if (targetBaseVersion is not null && targetBaseVersion != this.appVersion)
							continue;

						// select package
						if (osAttribute is null && archArrtibute is null && runtimeVersionAttr is null)
						{
							if (genericBaseVersion is null || targetBaseVersion is not null)
							{
								genericPackageUri = uri;
								genericBaseVersion = targetBaseVersion;
								packageAttrs["MD5"]?.Let(attr => genericMd5 = attr.Value);
								packageAttrs["SHA256"]?.Let(attr => genericSha256 = attr.Value);
								packageAttrs["SHA512"]?.Let(attr => genericSha512 = attr.Value);
							}
						}
						else if (isOsMatched && isArchMatched && isruntimeMatched)
						{
							if (this.SelfContainedPackageOnly && runtimeVersionAttr is not null)
								continue;
							if (runtimeVersionAttr is null)
							{
								if (!isWindows7 && (selfContainedBaseVersion is null || targetBaseVersion is not null))
								{
									selfContainedPackageUri = uri;
									selfContainedBaseVersion = targetBaseVersion;
									packageAttrs["MD5"]?.Let(attr => selfContainedMd5 = attr.Value);
									packageAttrs["SHA256"]?.Let(attr => selfContainedSha256 = attr.Value);
									packageAttrs["SHA512"]?.Let(attr => selfContainedSha512 = attr.Value);
								}
							}
							else if (baseVersion is null || targetBaseVersion is not null)
							{
								packageUri = uri;
								baseVersion = targetBaseVersion;
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
		}, cancellationToken);
		cancellationToken.ThrowIfCancellationRequested();

		// save result
		this.ApplicationName = appName;
		this.PageUri = pageUri;
		this.PackageInformationalVersion = packageInformationalVersion;
		this.PackageVersion = packageVersion;
		if (packageUri is not null)
		{
			this.PackageUri = packageUri;
			this.MD5 = md5;
			this.SHA256 = sha256;
			this.SHA512 = sha512;
		}
		else if (selfContainedPackageUri is not null)
		{
			this.PackageUri = selfContainedPackageUri;
			this.MD5 = selfContainedMd5;
			this.SHA256 = selfContainedSha256;
			this.SHA512 = selfContainedSha512;
		}
		else if (genericPackageUri is not null)
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