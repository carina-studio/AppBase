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
		/// <param name="streamProvider"><see cref="IStreamProvider"/> to provide data in XML format.</param>
		public XmlPackageResolver(IO.IStreamProvider streamProvider) : base(streamProvider)
		{ }


		/// <summary>
		/// Perform operation asynchronously.
		/// </summary>
		/// <param name="cancellationToken">Cancellation token.</param>
		/// <returns>Task of performing operation.</returns>
		protected override async Task PerformOperationAsync(CancellationToken cancellationToken)
		{
			// get XML data
			using var streamReader = await this.StreamProvider.OpenStreamReaderAsync(StreamAccess.Read, Encoding.UTF8, cancellationToken);
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
			rootNode.Attributes["Name"]?.Let(it => this.ApplicationName = it.Value);

			// get version
			rootNode.Attributes["Version"]?.Let(it =>
			{
				if (Version.TryParse(it.Value, out var version))
					this.PackageVersion = version;
			});

			// get page URI
			rootNode.Attributes["PageUri"]?.Let(it =>
			{
				if (Uri.TryCreate(it.Value, UriKind.Absolute, out var uri))
					this.PageUri = uri;
			});

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
			var archName = RuntimeInformation.ProcessArchitecture.ToString();

			// find package URI
			if (!rootNode.HasChildNodes)
				throw new XmlException("Package not fount.");
			var packageNode = rootNode.FirstChild;
			var genericPackageUri = (Uri?)null;
			while (packageNode != null)
			{
				try
				{
					if (packageNode.NodeType == XmlNodeType.Element && packageNode.Name == "Package")
					{
						// get URI
						var packageUri = packageNode.Attributes["Uri"]?.Let(it =>
						{
							Uri.TryCreate(it.Value, UriKind.Absolute, out var uri);
							return uri;
						});
						if (packageUri == null)
							continue;

						// check operating system
						var osAttribute = packageNode.Attributes["OperatingSystem"];
						var isOsMatched = osAttribute != null && osAttribute.Value == osName;

						// check architecture
						var archArrtibute = packageNode.Attributes["Architecture"];
						var isArchMatched = archArrtibute != null && archArrtibute.Value == archName;

						// select package
						if (osAttribute == null && archArrtibute == null)
							genericPackageUri = packageUri;
						else if (isOsMatched && isArchMatched)
						{
							this.PackageUri = packageUri;
							break;
						}
					}
				}
				finally
				{
					packageNode = packageNode.NextSibling;
				}
			}
			if (this.PackageUri == null)
			{
				if (genericPackageUri == null)
					throw new XmlException("Package not fount.");
				this.PackageUri = genericPackageUri;
			}
		}
	}
}
