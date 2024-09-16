using CarinaStudio.IO;
using CarinaStudio.Tests;
using NUnit.Framework;
using System.Collections.Generic;

namespace CarinaStudio.AutoUpdate.Installers
{
	/// <summary>
	/// Base implementation of tests of <see cref="IPackageInstaller"/>.
	/// </summary>
	abstract class BasePackageInstallerTests : BaseTests
	{
		/// <summary>
		/// Create <see cref="IPackageInstaller"/> instance for testing.
		/// </summary>
		/// <returns><see cref="IPackageInstaller"/>.</returns>
		protected abstract IPackageInstaller CreateInstance();


		/// <summary>
		/// Create package file for testing.
		/// </summary>
		/// <param name="sourceDirectory">Path of directory which contains files of package.</param>
		/// <param name="packagFileName">Name of created package file.</param>
		protected abstract void CreatePackageFile(string sourceDirectory, string packagFileName);


		/// <summary>
		/// Test for installation.
		/// </summary>
		[Test]
		public void InatallationTest()
		{
			this.TestOnApplicationThread(async () =>
			{
				// prepare
				var installer = this.CreateInstance();
				var packageSourceDir = this.GenerateRandomApplication();
				var packageFileName = Random.CreateFileWithRandomName(this.RootTempDirectoryPath).Use(it => it.Name);
				this.CreatePackageFile(packageSourceDir, packageFileName);

				// install package
				installer.PackageFileName = packageFileName;
				installer.TargetDirectoryPath = this.CreateTempDirectory();
				Assert.That(installer.Start());
				Assert.That(await installer.WaitForPropertyAsync(nameof(IPackageInstaller.State), UpdaterComponentState.Succeeded, 60000));

				// verify installed files
				this.VerifyFilesAndDirectories(packageSourceDir, installer.TargetDirectoryPath);

				// verify reported installed files
				var actualInstalledFilePaths = new HashSet<string>(PathEqualityComparer.Default);
				this.CollectFilePaths(installer.TargetDirectoryPath, actualInstalledFilePaths);
				Assert.That(actualInstalledFilePaths.SetEquals(installer.InstalledFilePaths));
			});
		}
	}
}
