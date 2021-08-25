using CarinaStudio.Tests;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

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
			/// Get or set CPU architecture.
			/// </summary>
			public Architecture? Architecture { get; set; }
			/// <summary>
			/// Get or set operating system.
			/// </summary>
			public string? OperatingSystem { get; set; }
			/// <summary>
			/// Get or set URI of package.
			/// </summary>
			public Uri? Uri { get; set; }
		}


		/// <summary>
		/// Test for cancellation.
		/// </summary>
		[Test]
		public void CancellationTest()
		{
			this.TestOnApplicationThread(async () =>
			{
				// prepare
				this.GetEnvironment(out var osName, out var architecture);
				var appName = Tests.Random.GenerateRandomString(8);
				var version = new Version(1, 2, 3, 4);
				var pageUri = new Uri("https://localhost/Package.htm");
				var expectedPackageUri = new Uri($"https://localhost/packages/{osName}-{architecture}.zip");
				var packageInfos = new List<PackageInfo>()
				{
					new PackageInfo()
					{
						Architecture = architecture,
						OperatingSystem = osName,
						Uri = expectedPackageUri,
					},
				};
				var packageManifest = this.GeneratePackageManifest(appName, version, pageUri, packageInfos);

				// cancel before resolving completed
				using (var packageResolver = this.CreateInstance(packageManifest))
				{
					Assert.IsTrue(packageResolver.Start());
					Assert.IsTrue(packageResolver.Cancel());
					Assert.AreEqual(UpdaterComponentState.Cancelling, packageResolver.State);
					Assert.IsTrue(await packageResolver.WaitForPropertyAsync(nameof(IUpdaterComponent.State), UpdaterComponentState.Cancelled, 10000));
				}

				// cancel after resolving completed
				using (var packageResolver = this.CreateInstance(packageManifest))
				{
					Assert.IsTrue(packageResolver.Start());
					Assert.IsTrue(await packageResolver.WaitForPropertyAsync(nameof(IUpdaterComponent.State), UpdaterComponentState.Succeeded, 10000));
					Assert.IsFalse(packageResolver.Cancel());
					Assert.AreEqual(UpdaterComponentState.Succeeded, packageResolver.State);
				}

				// cancel randomly when resolving
				for (var t = 0; t < 100; ++t)
				{
					using (var packageResolver = this.CreateInstance(packageManifest))
					{
						Assert.IsTrue(packageResolver.Start());
						await Task.Delay(Tests.Random.Next(100));
						if (packageResolver.Cancel())
						{
							Assert.AreEqual(UpdaterComponentState.Cancelling, packageResolver.State);
							Assert.IsTrue(await packageResolver.WaitForPropertyAsync(nameof(IUpdaterComponent.State), UpdaterComponentState.Cancelled, 10000));
						}
						else
						{
							Assert.AreNotEqual(UpdaterComponentState.Cancelling, packageResolver.State);
							Assert.IsTrue(await packageResolver.WaitForPropertyAsync(nameof(IUpdaterComponent.State), UpdaterComponentState.Succeeded, 10000));
						}
					}
				}
			});
		}


		/// <summary>
		/// Create <see cref="IPackageResolver"/> instance.
		/// </summary>
		/// <param name="packageManifest">Package manifest.</param>
		/// <returns><see cref="IPackageResolver"/>.</returns>
		protected abstract IPackageResolver CreateInstance(string packageManifest);


		/// <summary>
		/// Generate content of package manifest.
		/// </summary>
		/// <param name="appName">Application name.</param>
		/// <param name="version">Application version.</param>
		/// <param name="pageUri">URI of package web page.</param>
		/// <param name="packageInfos">Package info list.</param>
		/// <returns></returns>
		protected abstract string GeneratePackageManifest(string? appName, Version? version, Uri? pageUri, IList<PackageInfo> packageInfos);


		// Get operating system and CPU architecture of current environment.
		void GetEnvironment(out string osName, out Architecture architecure)
		{
			osName = Global.Run(() =>
			{
				if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
					return nameof(OSPlatform.Windows);
				if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
					return nameof(OSPlatform.Linux);
				if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
					return nameof(OSPlatform.OSX);
				throw new AssertionException("Unknown operating system.");
			});
			architecure = RuntimeInformation.ProcessArchitecture;
		}


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
						Architecture = Architecture.X86,
						OperatingSystem = "Windows",
						Uri = new Uri("https://localhost/packages/Windows-X86.zip"),
					},
					new PackageInfo()
					{
						Architecture = Architecture.X64,
						OperatingSystem = "Windows",
						Uri = new Uri("https://localhost/packages/Windows-X64.zip"),
					},
					new PackageInfo()
					{
						Architecture = Architecture.X64,
						OperatingSystem = "Linux",
						Uri = new Uri("https://localhost/packages/Linux-X64.zip"),
					},
					new PackageInfo()
					{
						Architecture = Architecture.Arm64,
						OperatingSystem = "Linux",
						Uri = new Uri("https://localhost/packages/Linux-Arm64.zip"),
					},
					new PackageInfo()
					{
						Architecture = Architecture.X64,
						OperatingSystem = "OSX",
						Uri = new Uri("https://localhost/packages/OSX-X64.zip"),
					},
				};

				// check current operating system and platform
				this.GetEnvironment(out var osName, out var architecture);
				var expectedPackageUri = new Uri($"https://localhost/packages/{osName}-{architecture}.zip");

				// resolve package info
				var packageManifest = this.GeneratePackageManifest(appName, version, pageUri, packageInfos);
				using (var packageResolver = this.CreateInstance(packageManifest))
				{
					Assert.IsTrue(packageResolver.Start());
					Assert.IsTrue(await packageResolver.WaitForPropertyAsync(nameof(IUpdaterComponent.State), UpdaterComponentState.Succeeded, 10000));
					Assert.IsNull(packageResolver.Exception);
					this.VerifyResolvedPackage(packageResolver, appName, version, pageUri, expectedPackageUri);
				}

				// resolve package info without application name
				packageManifest = this.GeneratePackageManifest(null, version, pageUri, packageInfos);
				using (var packageResolver = this.CreateInstance(packageManifest))
				{
					Assert.IsTrue(packageResolver.Start());
					Assert.IsTrue(await packageResolver.WaitForPropertyAsync(nameof(IUpdaterComponent.State), UpdaterComponentState.Succeeded, 10000));
					Assert.IsNull(packageResolver.Exception);
					this.VerifyResolvedPackage(packageResolver, null, version, pageUri, expectedPackageUri);
				}

				// resolve package info without version
				packageManifest = this.GeneratePackageManifest(appName, null, pageUri, packageInfos);
				using (var packageResolver = this.CreateInstance(packageManifest))
				{
					Assert.IsTrue(packageResolver.Start());
					Assert.IsTrue(await packageResolver.WaitForPropertyAsync(nameof(IUpdaterComponent.State), UpdaterComponentState.Succeeded, 10000));
					Assert.IsNull(packageResolver.Exception);
					this.VerifyResolvedPackage(packageResolver, appName, null, pageUri, expectedPackageUri);
				}

				// resolve package info without package list
				packageManifest = this.GeneratePackageManifest(appName, version, pageUri, new PackageInfo[0]);
				using (var packageResolver = this.CreateInstance(packageManifest))
				{
					Assert.IsTrue(packageResolver.Start());
					Assert.IsTrue(await packageResolver.WaitForPropertyAsync(nameof(IUpdaterComponent.State), UpdaterComponentState.Failed, 10000));
				}

				// resolve package info without package manifest content
				using (var packageResolver = this.CreateInstance(" "))
				{
					Assert.IsTrue(packageResolver.Start());
					Assert.IsTrue(await packageResolver.WaitForPropertyAsync(nameof(IUpdaterComponent.State), UpdaterComponentState.Failed, 10000));
				}
			});
		}


		// Verify fields in resolved package info.
		void VerifyResolvedPackage(IPackageResolver packageResolver, string? appName, Version? version, Uri? pageUri, Uri? packageUri)
		{
			Assert.AreEqual(appName, packageResolver.ApplicationName);
			Assert.AreEqual(version, packageResolver.PackageVersion);
			Assert.AreEqual(pageUri, packageResolver.PageUri);
			Assert.AreEqual(packageUri, packageResolver.PackageUri);
		}
	}
}
