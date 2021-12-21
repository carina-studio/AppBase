using CarinaStudio.IO;
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
		protected override IPackageResolver CreateInstance(string packageManifest)
		{
			var streamProvider = new MemoryStreamProvider(Encoding.UTF8.GetBytes(packageManifest), false);
			return new JsonPackageResolver() { Source = streamProvider };
		}


		// Generate package manifest.
		protected override string GeneratePackageManifest(string? appName, Version? version, Uri? pageUri, IList<PackageInfo> packageInfos)
		{
			using var stream = new MemoryStream();
			using (var jsonWriter = new Utf8JsonWriter(stream))
			{
				jsonWriter.WriteStartObject();
				appName?.Let(it => jsonWriter.WriteString("Name", it));
				version?.Let(it => jsonWriter.WriteString("Version", it.ToString()));
				pageUri?.Let(it => jsonWriter.WriteString("PageUri", it.ToString()));
				jsonWriter.WritePropertyName("Packages");
				jsonWriter.WriteStartArray();
				foreach (var packageInfo in packageInfos)
				{
					jsonWriter.WriteStartObject();
					packageInfo.OperatingSystem?.Let(it => jsonWriter.WriteString("OperatingSystem", it));
					packageInfo.Architecture?.Let(it => jsonWriter.WriteString("Architecture", it.ToString()));
					packageInfo.RuntimeVersion?.Let(it => jsonWriter.WriteString("RuntimeVersion", it.ToString()));
					packageInfo.MD5?.Let(it => jsonWriter.WriteString("MD5", it));
					packageInfo.SHA256?.Let(it => jsonWriter.WriteString("SHA256", it));
					packageInfo.SHA512?.Let(it => jsonWriter.WriteString("SHA512", it));
					packageInfo.Uri?.Let(it => jsonWriter.WriteString("Uri", it.ToString()));
					jsonWriter.WriteEndObject();
				}
				jsonWriter.WriteEndArray();
				jsonWriter.WriteEndObject();
			}
			return Encoding.UTF8.GetString(stream.ToArray());
		}
	}
}
