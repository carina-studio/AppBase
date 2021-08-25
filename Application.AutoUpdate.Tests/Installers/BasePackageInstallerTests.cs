using CarinaStudio.Collections;
using CarinaStudio.IO;
using CarinaStudio.Tests;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CarinaStudio.AutoUpdate.Installers
{
	/// <summary>
	/// Base implementation of tests of <see cref="IPackageInstaller"/>.
	/// </summary>
	abstract class BasePackageInstallerTests : BaseTests
	{
		// Fields.
		string tempDirectory = "";


		/// <summary>
		/// Clear temporary directory for testing.
		/// </summary>
		[OneTimeTearDown]
		public void ClearTempDirectory()
		{
			if (!string.IsNullOrEmpty(this.tempDirectory))
				Directory.Delete(this.tempDirectory, true);
		}


		// Collect file paths.
		void CollectFilePaths(string directory, ISet<string> filePaths)
		{
			filePaths.AddAll(Directory.EnumerateFiles(directory));
			foreach (var subDirectory in Directory.EnumerateDirectories(directory))
				this.CollectFilePaths(subDirectory, filePaths);
		}


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


		// Create empty temporary directory.
		string CreateTempDirectory()
		{
			var name = Tests.Random.GenerateRandomString(8);
			var path = Path.Combine(this.tempDirectory, name);
			while (true)
			{
				if (!Directory.Exists(path))
				{
					Directory.CreateDirectory(path);
					return path;
				}
				name = Tests.Random.GenerateRandomString(8);
				path = Path.Combine(this.tempDirectory, name);
			}
		}


		// Generate random files of package.
		string GeneratePackageFiles()
		{
			// create root directory
			var rootDirectory = this.CreateTempDirectory();

			// create first level files
			for (var i = Tests.Random.Next(1, 17); i > 0; --i)
			{
				using var stream = Tests.Random.CreateFileWithRandomName(rootDirectory);
				var data = new byte[Tests.Random.Next(100, 1025)];
				Tests.Random.NextBytes(data);
				stream.Write(data);
			}

			// create second level files
			var subDirectoryInfo = Directory.CreateDirectory(Path.Combine(rootDirectory, "SubDirectory"));
			for (var i = Tests.Random.Next(1, 17); i > 0; --i)
			{
				using var stream = Tests.Random.CreateFileWithRandomName(subDirectoryInfo);
				var data = new byte[Tests.Random.Next(100, 1025)];
				Tests.Random.NextBytes(data);
				stream.Write(data);
			}

			// complete
			return rootDirectory;
		}


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
				var packageSourceDir = this.GeneratePackageFiles();
				var packageFileName = Tests.Random.CreateFileWithRandomName(this.tempDirectory).Use(it => it.Name);
				this.CreatePackageFile(packageSourceDir, packageFileName);

				// install package
				installer.PackageFileName = packageFileName;
				installer.TargetDirectoryPath = this.CreateTempDirectory();
				Assert.IsTrue(installer.Start());
				Assert.IsTrue(await installer.WaitForPropertyAsync(nameof(IPackageInstaller.State), UpdaterComponentState.Succeeded, 60000));

				// verify installed files
				this.VerifyInstalledFiles(packageSourceDir, installer.TargetDirectoryPath);

				// verify reported installed files
				var actualInstalledFilePaths = new HashSet<string>(PathEqualityComparer.Default);
				this.CollectFilePaths(installer.TargetDirectoryPath, actualInstalledFilePaths);
				Assert.IsTrue(actualInstalledFilePaths.SetEquals(installer.InstalledFilePaths));
			});
		}


		/// <summary>
		/// Setup temporary directory for testing.
		/// </summary>
		[OneTimeSetUp]
		public void SetupTempDirectory()
		{
			var path = Path.Combine(Path.GetTempPath(), $"{this.GetType().Name}-{DateTime.Now.ToBinary()}");
			Directory.CreateDirectory(path);
			this.tempDirectory = path;
		}


		// Verify installed files.
		void VerifyInstalledFiles(string sourceDirectory, string targetDirectory)
		{
			// verify files
			var srcFileNames = Directory.GetFiles(sourceDirectory).Also(it => Array.Sort(it, string.Compare));
			var targetFileNames = Directory.GetFiles(targetDirectory).Also(it => Array.Sort(it, string.Compare));
			Assert.AreEqual(srcFileNames.Length, targetFileNames.Length);
			for (var i = srcFileNames.Length - 1; i >= 0; --i)
			{
				var srcFileName = srcFileNames[i];
				var targetFileName = targetFileNames[i];
				Assert.AreEqual(Path.GetFileName(srcFileName), Path.GetFileName(targetFileName));
				var srcData = new FileStream(srcFileName, FileMode.Open, FileAccess.Read).Use(it => it.ReadAllBytes());
				var targetData = new FileStream(targetFileName, FileMode.Open, FileAccess.Read).Use(it => it.ReadAllBytes());
				Assert.IsTrue(srcData.SequenceEqual(targetData));
			}

			// verify sub directories
			var srcSubDirectories = Directory.GetDirectories(sourceDirectory).Also(it => Array.Sort(it, string.Compare));
			var targetSubDirectories = Directory.GetDirectories(targetDirectory).Also(it => Array.Sort(it, string.Compare));
			Assert.AreEqual(srcSubDirectories.Length, targetSubDirectories.Length);
			for (var i = srcSubDirectories.Length - 1; i >= 0; --i)
			{
				var srcSubDirectory = srcSubDirectories[i];
				var targetSubDirectory = targetSubDirectories[i];
				Assert.AreEqual(Path.GetFileName(srcSubDirectory), Path.GetFileName(targetSubDirectory));
				this.VerifyInstalledFiles(srcSubDirectory, targetSubDirectory);
			}
		}
	}
}
