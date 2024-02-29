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
			/// Get or set base version to upgrade.
			/// </summary>
			public Version? BaseVersion { get; set; }
			/// <summary>
			/// MD5 of update package.
			/// </summary>
			public string? MD5 { get; set; }
			/// <summary>
			/// Get or set operating system.
			/// </summary>
			public string? OperatingSystem { get; set; }
			/// <summary>
			/// Get or set version of target runtime.
			/// </summary>
			public Version? RuntimeVersion { get; set; }
			/// <summary>
			/// SHA256 of update package.
			/// </summary>
			public string? SHA256 { get; set; }
			/// <summary>
			/// SHA512 of update package.
			/// </summary>
			public string? SHA512 { get; set; }
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
				var appVersion = this.Application.Assembly.GetName().Version.AsNonNull();
				var version = new Version(1, 2, 3, 4);
				var pageUri = new Uri("https://localhost/Package.htm");
				var runtimeVersion = Platform.GetInstalledRuntimeVersion();
				var packageInfos = new List<PackageInfo>()
				{
					new PackageInfo()
					{
						Architecture = Architecture.X86,
						MD5 = "MD5-Windows-X86-SelfContained",
						OperatingSystem = "Windows",
						SHA256 = "SHA256-Windows-X86-SelfContained",
						SHA512 = "SHA512-Windows-X86-SelfContained",
						Uri = new Uri("https://localhost/packages/Windows-X86-SelfContained.zip"),
					},
					new PackageInfo()
					{
						Architecture = Architecture.X86,
						RuntimeVersion = runtimeVersion,
						MD5 = "MD5-Windows-X86",
						OperatingSystem = "Windows",
						SHA256 = "SHA256-Windows-X86",
						SHA512 = "SHA512-Windows-X86",
						Uri = new Uri("https://localhost/packages/Windows-X86.zip"),
					},
					new PackageInfo()
					{
						Architecture = Architecture.X86,
						RuntimeVersion = new Version(999, 0),
						MD5 = "MD5-Windows-X86-Invalid",
						OperatingSystem = "Windows",
						SHA256 = "SHA256-Windows-X86-Invalid",
						SHA512 = "SHA512-Windows-X86-Invalid",
						Uri = new Uri("https://localhost/packages/Windows-X86-Invalid.zip"),
					},
					new PackageInfo()
					{
						Architecture = Architecture.X64,
						MD5 = "MD5-Windows-X64-SelfContained",
						OperatingSystem = "Windows",
						SHA256 = "SHA256-Windows-X64-SelfContained",
						SHA512 = "SHA512-Windows-X64-SelfContained",
						Uri = new Uri("https://localhost/packages/Windows-X64-SelfContained.zip"),
					},
					new PackageInfo()
					{
						Architecture = Architecture.X64,
						RuntimeVersion = runtimeVersion,
						MD5 = "MD5-Windows-X64",
						OperatingSystem = "Windows",
						SHA256 = "SHA256-Windows-X64",
						SHA512 = "SHA512-Windows-X64",
						Uri = new Uri("https://localhost/packages/Windows-X64.zip"),
					},
					new PackageInfo()
					{
						Architecture = Architecture.X64,
						RuntimeVersion = new Version(999, 0),
						MD5 = "MD5-Windows-X64-Invalid",
						OperatingSystem = "Windows",
						SHA256 = "SHA256-Windows-X64-Invalid",
						SHA512 = "SHA512-Windows-X64-Invalid",
						Uri = new Uri("https://localhost/packages/Windows-X64-Invalid.zip"),
					},
					new PackageInfo()
					{
						Architecture = Architecture.X64,
						MD5 = "MD5-Linux-X64-SelfContained",
						OperatingSystem = "Linux",
						SHA256 = "SHA256-Linux-X64-SelfContained",
						SHA512 = "SHA512-Linux-X64-SelfContained",
						Uri = new Uri("https://localhost/packages/Linux-X64-SelfContained.zip"),
					},
					new PackageInfo()
					{
						Architecture = Architecture.X64,
						RuntimeVersion = runtimeVersion,
						MD5 = "MD5-Linux-X64",
						OperatingSystem = "Linux",
						SHA256 = "SHA256-Linux-X64",
						SHA512 = "SHA512-Linux-X64",
						Uri = new Uri("https://localhost/packages/Linux-X64.zip"),
					},
					new PackageInfo()
					{
						Architecture = Architecture.X64,
						RuntimeVersion = new Version(999, 0),
						MD5 = "MD5-Linux-X64-Invalid",
						OperatingSystem = "Linux",
						SHA256 = "SHA256-Linux-X64-Invalid",
						SHA512 = "SHA512-Linux-X64-Invalid",
						Uri = new Uri("https://localhost/packages/Linux-X64-Invalid.zip"),
					},
					new PackageInfo()
					{
						Architecture = Architecture.Arm64,
						MD5 = "MD5-Linux-Arm64-SelfContained",
						OperatingSystem = "Linux",
						SHA256 = "SHA256-Linux-Arm64-SelfContained",
						SHA512 = "SHA512-Linux-Arm64-SelfContained",
						Uri = new Uri("https://localhost/packages/Linux-Arm64-SelfContained.zip"),
					},
					new PackageInfo()
					{
						Architecture = Architecture.Arm64,
						RuntimeVersion = runtimeVersion,
						MD5 = "MD5-Linux-Arm64",
						OperatingSystem = "Linux",
						SHA256 = "SHA256-Linux-Arm64",
						SHA512 = "SHA512-Linux-Arm64",
						Uri = new Uri("https://localhost/packages/Linux-Arm64.zip"),
					},
					new PackageInfo()
					{
						Architecture = Architecture.Arm64,
						RuntimeVersion = new Version(999, 0),
						MD5 = "MD5-Linux-Arm64-Invalid",
						OperatingSystem = "Linux",
						SHA256 = "SHA256-Linux-Arm64-Invalid",
						SHA512 = "SHA512-Linux-Arm64-Invalid",
						Uri = new Uri("https://localhost/packages/Linux-Arm64-Invalid.zip"),
					},
					new PackageInfo()
					{
						Architecture = Architecture.X64,
						MD5 = "MD5-OSX-X64-SelfContained",
						OperatingSystem = "OSX",
						SHA256 = "SHA256-OSX-X64-SelfContained",
						SHA512 = "SHA512-OSX-X64-SelfContained",
						Uri = new Uri("https://localhost/packages/OSX-X64-SelfContained.zip"),
					},
					new PackageInfo()
					{
						Architecture = Architecture.X64,
						RuntimeVersion = runtimeVersion,
						MD5 = "MD5-OSX-X64",
						OperatingSystem = "OSX",
						SHA256 = "SHA256-OSX-X64",
						SHA512 = "SHA512-OSX-X64",
						Uri = new Uri("https://localhost/packages/OSX-X64.zip"),
					},
					new PackageInfo()
					{
						Architecture = Architecture.X64,
						RuntimeVersion = new Version(999, 0),
						MD5 = "MD5-OSX-X64-Invalid",
						OperatingSystem = "OSX",
						SHA256 = "SHA256-OSX-X64-Invalid",
						SHA512 = "SHA512-OSX-X64-Invalid",
						Uri = new Uri("https://localhost/packages/OSX-X64-Invalid.zip"),
					},
					new PackageInfo()
					{
						Architecture = Architecture.Arm64,
						MD5 = "MD5-OSX-Arm64-SelfContained",
						OperatingSystem = "OSX",
						SHA256 = "SHA256-OSX-Arm64-SelfContained",
						SHA512 = "SHA512-OSX-Arm64-SelfContained",
						Uri = new Uri("https://localhost/packages/OSX-Arm64-SelfContained.zip"),
					},
					new PackageInfo()
					{
						Architecture = Architecture.Arm64,
						RuntimeVersion = runtimeVersion,
						MD5 = "MD5-OSX-Arm64",
						OperatingSystem = "OSX",
						SHA256 = "SHA256-OSX-Arm64",
						SHA512 = "SHA512-OSX-Arm64",
						Uri = new Uri("https://localhost/packages/OSX-Arm64.zip"),
					},
					new PackageInfo()
					{
						Architecture = Architecture.Arm64,
						RuntimeVersion = new Version(999, 0),
						MD5 = "MD5-OSX-Arm64-Invalid",
						OperatingSystem = "OSX",
						SHA256 = "SHA256-OSX-Arm64-Invalid",
						SHA512 = "SHA512-OSX-Arm64-Invalid",
						Uri = new Uri("https://localhost/packages/OSX-Arm64-Invalid.zip"),
					},
				};

				// check current operating system and platform
				this.GetEnvironment(out var osName, out var architecture);
				var expectedPackageUri = new Uri($"https://localhost/packages/{osName}-{architecture}-SelfContained.zip");
				var expectedMD5 = $"MD5-{osName}-{architecture}-SelfContained";
				var expectedSHA256 = $"SHA256-{osName}-{architecture}-SelfContained";
				var expectedSHA512 = $"SHA512-{osName}-{architecture}-SelfContained";

				// resolve package info with self-contained package only
				var packageManifest = this.GeneratePackageManifest(appName, version, pageUri, packageInfos);
				using (var packageResolver = this.CreateInstance(packageManifest))
				{
					packageResolver.SelfContainedPackageOnly = true;
					Assert.IsTrue(packageResolver.Start());
					Assert.IsTrue(await packageResolver.WaitForPropertyAsync(nameof(IUpdaterComponent.State), UpdaterComponentState.Succeeded, 10000));
					Assert.IsNull(packageResolver.Exception);
					this.VerifyResolvedPackage(packageResolver, appName, version, pageUri, expectedPackageUri, expectedMD5, expectedSHA256, expectedSHA512);
				}

				// resolve package info
				expectedPackageUri = new Uri($"https://localhost/packages/{osName}-{architecture}.zip");
				expectedMD5 = $"MD5-{osName}-{architecture}";
				expectedSHA256 = $"SHA256-{osName}-{architecture}";
				expectedSHA512 = $"SHA512-{osName}-{architecture}";
				using (var packageResolver = this.CreateInstance(packageManifest))
				{
					Assert.IsTrue(packageResolver.Start());
					Assert.IsTrue(await packageResolver.WaitForPropertyAsync(nameof(IUpdaterComponent.State), UpdaterComponentState.Succeeded, 10000));
					Assert.IsNull(packageResolver.Exception);
					this.VerifyResolvedPackage(packageResolver, appName, version, pageUri, expectedPackageUri, expectedMD5, expectedSHA256, expectedSHA512);
				}

				// resolve package info without application name
				packageManifest = this.GeneratePackageManifest(null, version, pageUri, packageInfos);
				using (var packageResolver = this.CreateInstance(packageManifest))
				{
					Assert.IsTrue(packageResolver.Start());
					Assert.IsTrue(await packageResolver.WaitForPropertyAsync(nameof(IUpdaterComponent.State), UpdaterComponentState.Succeeded, 10000));
					Assert.IsNull(packageResolver.Exception);
					this.VerifyResolvedPackage(packageResolver, null, version, pageUri, expectedPackageUri, expectedMD5, expectedSHA256, expectedSHA512);
				}

				// resolve package info without version
				packageManifest = this.GeneratePackageManifest(appName, null, pageUri, packageInfos);
				using (var packageResolver = this.CreateInstance(packageManifest))
				{
					Assert.IsTrue(packageResolver.Start());
					Assert.IsTrue(await packageResolver.WaitForPropertyAsync(nameof(IUpdaterComponent.State), UpdaterComponentState.Succeeded, 10000));
					Assert.IsNull(packageResolver.Exception);
					this.VerifyResolvedPackage(packageResolver, appName, null, pageUri, expectedPackageUri, expectedMD5, expectedSHA256, expectedSHA512);
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

				// resolve package info with self-contained package with specified base version only
				PackageInfo tempPackageInfo;
				if (Platform.IsNotMacOS)
				{
					tempPackageInfo = new PackageInfo()
					{
						Architecture = architecture,
						BaseVersion = appVersion,
						MD5 = $"MD5-{osName}-{architecture}-SelfContained-partial",
						OperatingSystem = $"{osName}",
						SHA256 = $"SHA256-{osName}-{architecture}-SelfContained-partial",
						SHA512 = $"SHA512-{osName}-{architecture}-SelfContained-partial",
						Uri = new Uri($"https://localhost/packages/{osName}-{architecture}-SelfContained-partial.zip"),
					};
					packageInfos.Insert(0, tempPackageInfo);
					packageManifest = this.GeneratePackageManifest(appName, version, pageUri, packageInfos);
					expectedPackageUri = new Uri($"https://localhost/packages/{osName}-{architecture}-SelfContained-partial.zip");
					expectedMD5 = $"MD5-{osName}-{architecture}-SelfContained-partial";
					expectedSHA256 = $"SHA256-{osName}-{architecture}-SelfContained-partial";
					expectedSHA512 = $"SHA512-{osName}-{architecture}-SelfContained-partial";
					using (var packageResolver = this.CreateInstance(packageManifest))
					{
						packageResolver.SelfContainedPackageOnly = true;
						Assert.IsTrue(packageResolver.Start());
						Assert.IsTrue(await packageResolver.WaitForPropertyAsync(nameof(IUpdaterComponent.State), UpdaterComponentState.Succeeded, 10000));
						Assert.IsNull(packageResolver.Exception);
						this.VerifyResolvedPackage(packageResolver, appName, version, pageUri, expectedPackageUri, expectedMD5, expectedSHA256, expectedSHA512);
					}
					packageInfos.Remove(tempPackageInfo);
				}

				// resolve package info with specified base version only
				if (Platform.IsNotMacOS)
				{
					tempPackageInfo = new PackageInfo()
					{
						Architecture = architecture,
						BaseVersion = appVersion,
						RuntimeVersion = runtimeVersion,
						MD5 = $"MD5-{osName}-{architecture}-partial",
						OperatingSystem = $"{osName}",
						SHA256 = $"SHA256-{osName}-{architecture}-partial",
						SHA512 = $"SHA512-{osName}-{architecture}-partial",
						Uri = new Uri($"https://localhost/packages/{osName}-{architecture}-partial.zip"),
					};
					packageInfos.Insert(0, tempPackageInfo);
					packageManifest = this.GeneratePackageManifest(appName, version, pageUri, packageInfos);
					expectedPackageUri = new Uri($"https://localhost/packages/{osName}-{architecture}-partial.zip");
					expectedMD5 = $"MD5-{osName}-{architecture}-partial";
					expectedSHA256 = $"SHA256-{osName}-{architecture}-partial";
					expectedSHA512 = $"SHA512-{osName}-{architecture}-partial";
					using (var packageResolver = this.CreateInstance(packageManifest))
					{
						Assert.IsTrue(packageResolver.Start());
						Assert.IsTrue(await packageResolver.WaitForPropertyAsync(nameof(IUpdaterComponent.State), UpdaterComponentState.Succeeded, 10000));
						Assert.IsNull(packageResolver.Exception);
						this.VerifyResolvedPackage(packageResolver, appName, version, pageUri, expectedPackageUri, expectedMD5, expectedSHA256, expectedSHA512);
					}
					packageInfos.Remove(tempPackageInfo);
				}
			});
		}


		// Verify fields in resolved package info.
		void VerifyResolvedPackage(IPackageResolver packageResolver, string? appName, Version? version, Uri? pageUri, Uri? packageUri, string? md5 = null, string? sha256 = null, string? sha512 = null)
		{
			Assert.AreEqual(appName, packageResolver.ApplicationName);
			Assert.AreEqual(version, packageResolver.PackageVersion);
			Assert.AreEqual(pageUri, packageResolver.PageUri);
			Assert.AreEqual(packageUri, packageResolver.PackageUri);
			if (md5 != null)
				Assert.AreEqual(md5, packageResolver.MD5.AsNonNull());
			if (sha256 != null)
				Assert.AreEqual(sha256, packageResolver.SHA256.AsNonNull());
			if (sha512 != null)
				Assert.AreEqual(sha512, packageResolver.SHA512.AsNonNull());
		}
	}
}
