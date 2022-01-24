using System;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;

namespace CarinaStudio.AutoUpdate.Installers
{
	/// <summary>
	/// Implementation of <see cref="IPackageInstaller"/> which install package in ZIP format.
	/// </summary>
	public class ZipPackageInstaller : BasePackageInstaller
	{
		/// <summary>
		/// Perform operation asynchronously.
		/// </summary>
		/// <param name="cancellationToken">Cancellation token.</param>
		/// <returns>Task of performing operation.</returns>
		protected override Task PerformOperationAsync(CancellationToken cancellationToken) => Task.Run(() =>
		{
			// load zip archive
			using var zipArchive = ZipFile.OpenRead(this.PackageFileName.AsNonNull());

			// cancellation check
			if (cancellationToken.IsCancellationRequested)
				throw new TaskCanceledException();

			// create target directory
			var targetRootDirectory = this.TargetDirectoryPath.AsNonNull();
			if (File.Exists(targetRootDirectory))
				throw new ArgumentException($"'{targetRootDirectory}' is a file.");
			if (!Directory.Exists(targetRootDirectory))
				Directory.CreateDirectory(targetRootDirectory);

			// cancellation check
			if (cancellationToken.IsCancellationRequested)
				throw new TaskCanceledException();

			// extract files
			var entryCount = zipArchive.Entries.Count;
			var extractedEntryCount = 0;
			var targetRootDirectoryName = Path.GetFileName(targetRootDirectory);
			this.ReportProgress(0);
			foreach (var zipEntry in zipArchive.Entries)
			{
				var zipEntryPath = zipEntry.FullName.Let(it => Path.DirectorySeparatorChar switch
				{
					'\\' => it.Replace('/', '\\'),
					'/' => it.Replace('\\', '/'),
					_ => it,
				});
				if (Platform.IsMacOS && zipEntryPath.StartsWith($"{targetRootDirectoryName}/"))
				{
					zipEntryPath = zipEntryPath.Substring(targetRootDirectoryName.Length + 1);
					if (string.IsNullOrEmpty(zipEntryPath))
						continue;
				}
				var targetFileName = Path.Combine(targetRootDirectory, zipEntryPath);
				var targetDirectory = Path.GetDirectoryName(targetFileName);
				if (targetDirectory != null)
					Directory.CreateDirectory(targetDirectory);
				if (zipEntryPath.EndsWith(Path.DirectorySeparatorChar))
					continue;
				zipEntry.ExtractToFile(targetFileName, true);
				this.ReportInstalledFilePath(targetFileName);
				this.ReportProgress((double)(++extractedEntryCount) / entryCount);
				if (cancellationToken.IsCancellationRequested)
					throw new TaskCanceledException();
			}
		});
	}
}
