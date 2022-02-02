using CarinaStudio.IO;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace CarinaStudio.AutoUpdate.Installers
{
	/// <summary>
	/// Tests of <see cref="ZipPackageInstaller"/>.
	/// </summary>
	[TestFixture]
	class ZipPackageInstallerTests : BasePackageInstallerTests
	{
		// Create instance.
		protected override IPackageInstaller CreateInstance() => new ZipPackageInstaller(this.Application);


		// Create package file.
		protected override void CreatePackageFile(string sourceDirectory, string packagFileName)
		{
			using var zipArchive = ZipFile.Open(packagFileName, ZipArchiveMode.Update);
			this.CreatePackageFile(sourceDirectory, sourceDirectory, zipArchive);
		}
		void CreatePackageFile(string rootDirectory, string directory, ZipArchive zipArchive)
		{
			foreach (var fileName in Directory.EnumerateFiles(directory))
				zipArchive.CreateEntryFromFile(fileName, Path.GetRelativePath(rootDirectory, fileName).Replace('\\', '/'));
			foreach (var subDirectory in Directory.EnumerateDirectories(directory))
				this.CreatePackageFile(rootDirectory, subDirectory, zipArchive);
		}
	}
}
