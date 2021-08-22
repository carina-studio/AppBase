using CarinaStudio.Tests;
using CarinaStudio.Threading;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace CarinaStudio.AutoUpdate.Resolvers
{
	/// <summary>
	/// Tests of <see cref="IPackageResolver"/>.
	/// </summary>
	abstract class BasePackageResolverTests: BaseTests
	{
		/// <summary>
		/// Information of single package.
		/// </summary>
		public struct PackageInfo
		{
			/// <summary>
			/// Get or set operating system.
			/// </summary>
			public string? OperatingSystem { get; set; }
			/// <summary>
			/// Get or set platform.
			/// </summary>
			public Architecture? Platform { get; set; }
			/// <summary>
			/// Get or set URI of package.
			/// </summary>
			public Uri? Uri { get; set; }
		}


		// Fields.
		HttpListener? httpListener;
		volatile string? httpResponseContent;


		/// <summary>
		/// Create <see cref="IPackageResolver"/> instance.
		/// </summary>
		/// <param name="app">Application.</param>
		/// <returns><see cref="IPackageResolver"/>.</returns>
		protected abstract IPackageResolver CreateInstance(IApplication app);


		/// <summary>
		/// Release HTTP server for testing.
		/// </summary>
		[OneTimeTearDown]
		public void DisposeHttpListener()
		{
			this.httpListener?.Stop();
		}


		/// <summary>
		/// Generate content of package manifest.
		/// </summary>
		/// <param name="appName">Application name.</param>
		/// <param name="version">Application version.</param>
		/// <param name="pageUri">URI of package web page.</param>
		/// <param name="packageInfos">Package info list.</param>
		/// <returns></returns>
		protected abstract string GeneratePackageManifest(string? appName, Version? version, Uri? pageUri, IList<PackageInfo> packageInfos);


		/// <summary>
		/// Test for resolving update package.
		/// </summary>
		[Test]
		public void ResolvingTest()
		{
			this.TestOnApplicationThread(async () =>
			{
				// prepare
				var appName = Tests.Random.GenerateRandomString(8);
				var version = new Version(1, 2, 3, 4);
				var pageUri = new Uri("https://localhost/Package.htm");
				var packageInfos = new List<PackageInfo>()
				{
					new PackageInfo()
					{
						OperatingSystem = "Windows",
						Platform = Architecture.X86,
						Uri = new Uri("https://localhost/packages/Windows-X86.zip"),
					},
					new PackageInfo()
					{
						OperatingSystem = "Windows",
						Platform = Architecture.X64,
						Uri = new Uri("https://localhost/packages/Windows-X64.zip"),
					},
					new PackageInfo()
					{
						OperatingSystem = "Linux",
						Platform = Architecture.X64,
						Uri = new Uri("https://localhost/packages/Linux-X64.zip"),
					},
					new PackageInfo()
					{
						OperatingSystem = "Linux",
						Platform = Architecture.Arm64,
						Uri = new Uri("https://localhost/packages/Linux-Arm64.zip"),
					},
					new PackageInfo()
					{
						OperatingSystem = "OSX",
						Platform = Architecture.X64,
						Uri = new Uri("https://localhost/packages/OSX-X64.zip"),
					},
				};

				// check current operating system and platform
				var osName = Global.Run(() =>
				{
					if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
						return nameof(OSPlatform.Windows);
					if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
						return nameof(OSPlatform.Linux);
					if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
						return nameof(OSPlatform.OSX);
					throw new AssertionException("Unknown operating system.");
				});
				var platformName = RuntimeInformation.ProcessArchitecture.ToString();
				var expectedPackageUri = new Uri($"https://localhost/packages/{osName}-{platformName}.zip");

				// resolve package info
				this.httpResponseContent = this.GeneratePackageManifest(appName, version, pageUri, packageInfos);
				using (var packageResolver = this.CreateInstance(this.Application))
				{
					packageResolver.PackageManifestUri = new Uri("http://localhost:9521/PackageInfo");
					Assert.IsTrue(packageResolver.Start());
					Assert.IsTrue(await packageResolver.WaitForPropertyAsync(nameof(IUpdaterComponent.State), UpdaterComponentState.Succeeded, 10000));
					Assert.AreEqual(appName, packageResolver.ApplicationName);
					Assert.AreEqual(version, packageResolver.PackageVersion);
					Assert.AreEqual(pageUri, packageResolver.PageUri);
					Assert.IsNull(packageResolver.Exception);
					Assert.AreEqual(expectedPackageUri, packageResolver.PackageUri);
				}

				// resolve package info without application name
				this.httpResponseContent = this.GeneratePackageManifest(null, version, pageUri, packageInfos);
				using (var packageResolver = this.CreateInstance(this.Application))
				{
					packageResolver.PackageManifestUri = new Uri("http://localhost:9521/PackageInfo");
					Assert.IsTrue(packageResolver.Start());
					Assert.IsTrue(await packageResolver.WaitForPropertyAsync(nameof(IUpdaterComponent.State), UpdaterComponentState.Succeeded, 10000));
					Assert.IsNull(packageResolver.ApplicationName);
					Assert.AreEqual(version, packageResolver.PackageVersion);
					Assert.AreEqual(pageUri, packageResolver.PageUri);
					Assert.IsNull(packageResolver.Exception);
					Assert.AreEqual(expectedPackageUri, packageResolver.PackageUri);
				}

				// resolve package info without version
				this.httpResponseContent = this.GeneratePackageManifest(appName, null, pageUri, packageInfos);
				using (var packageResolver = this.CreateInstance(this.Application))
				{
					packageResolver.PackageManifestUri = new Uri("http://localhost:9521/PackageInfo");
					Assert.IsTrue(packageResolver.Start());
					Assert.IsTrue(await packageResolver.WaitForPropertyAsync(nameof(IUpdaterComponent.State), UpdaterComponentState.Succeeded, 10000));
					Assert.AreEqual(appName, packageResolver.ApplicationName);
					Assert.IsNull(packageResolver.PackageVersion);
					Assert.AreEqual(pageUri, packageResolver.PageUri);
					Assert.IsNull(packageResolver.Exception);
					Assert.AreEqual(expectedPackageUri, packageResolver.PackageUri);
				}

				// resolve package info without page URI
				this.httpResponseContent = this.GeneratePackageManifest(appName, version, null, packageInfos);
				using (var packageResolver = this.CreateInstance(this.Application))
				{
					packageResolver.PackageManifestUri = new Uri("http://localhost:9521/PackageInfo");
					Assert.IsTrue(packageResolver.Start());
					Assert.IsTrue(await packageResolver.WaitForPropertyAsync(nameof(IUpdaterComponent.State), UpdaterComponentState.Succeeded, 10000));
					Assert.AreEqual(appName, packageResolver.ApplicationName);
					Assert.AreEqual(version, packageResolver.PackageVersion);
					Assert.IsNull(packageResolver.PageUri);
					Assert.IsNull(packageResolver.Exception);
					Assert.AreEqual(expectedPackageUri, packageResolver.PackageUri);
				}

				// resolve package info without package list
				this.httpResponseContent = this.GeneratePackageManifest(appName, version, pageUri, new PackageInfo[0]);
				using (var packageResolver = this.CreateInstance(this.Application))
				{
					packageResolver.PackageManifestUri = new Uri("http://localhost:9521/PackageInfo");
					Assert.IsTrue(packageResolver.Start());
					Assert.IsTrue(await packageResolver.WaitForPropertyAsync(nameof(IUpdaterComponent.State), UpdaterComponentState.Failed, 10000));
					Assert.IsNotNull(packageResolver.Exception);
				}
			});
		}


		/// <summary>
		/// Setup HTTP server for testing.
		/// </summary>
		[OneTimeSetUp]
		public void SetupHttpListener()
		{
			this.httpListener = new HttpListener().Also(it =>
			{
				it.Prefixes.Add("http://localhost:9521/");
			});
			this.httpListener.Start();
			ThreadPool.QueueUserWorkItem(_ =>
			{
				while (true)
				{
					// wait for connection
					var context = (HttpListenerContext?)null;
					try
					{
						context = this.httpListener.GetContext();
					}
					catch
					{
						if (this.httpListener?.IsListening != true)
							break;
						throw;
					}

					// prepare response data
					var responseBuffer = this.httpResponseContent?.Let(it =>
					{
						return Encoding.UTF8.GetBytes(it);
					}) ?? new byte[0];

					// response
					context.Response.Let(response =>
					{
						response.ContentLength64 = responseBuffer.Length;
						response.ContentEncoding = Encoding.UTF8;
						using var stream = response.OutputStream;
						stream.Write(responseBuffer, 0, responseBuffer.Length);
						stream.Flush();
					});
				}
			});
		}
	}
}
