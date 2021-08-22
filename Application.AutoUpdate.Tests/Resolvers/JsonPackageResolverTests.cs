using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

namespace CarinaStudio.AutoUpdate.Resolvers
{
	/// <summary>
	/// Tests of <see cref="JsonPackageResolver"/>.
	/// </summary>
	[TestFixture]
	class JsonPackageResolverTests : BasePackageResolverTests
	{
		// Create instance.
		protected override IPackageResolver CreateInstance(IApplication app) => new JsonPackageResolver(app);


		// Generate package manifest.
		protected override string GeneratePackageManifest(string? appName, Version? version, Uri? pageUri, IList<PackageInfo> packageInfos)
		{
			using var stream = new MemoryStream();
			using (var jsonWriter = new Utf8JsonWriter(stream))
			{
				jsonWriter.WriteStartObject();
				appName?.Let(it => jsonWriter.WriteString("Name", it));
				version?.Let(it => jsonWriter.WriteString("Version", it.ToString()));
				pageUri?.Let(it => jsonWriter.WriteString("ReleasePageUrl", it.ToString()));
				jsonWriter.WritePropertyName("Packages");
				jsonWriter.WriteStartArray();
				foreach (var packageInfo in packageInfos)
				{
					jsonWriter.WriteStartObject();
					packageInfo.OperatingSystem?.Let(it => jsonWriter.WriteString("OS", it));
					packageInfo.Platform?.Let(it => jsonWriter.WriteString("Platform", it.ToString()));
					packageInfo.Uri?.Let(it => jsonWriter.WriteString("Url", it.ToString()));
					jsonWriter.WriteEndObject();
				}
				jsonWriter.WriteEndArray();
				jsonWriter.WriteEndObject();
			}
			return Encoding.UTF8.GetString(stream.ToArray());
		}
	}
}
