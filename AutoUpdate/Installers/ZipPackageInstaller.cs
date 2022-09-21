using Microsoft.Extensions.Logging;
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
		/// Initialize new <see cref="ZipPackageInstaller"/> instance.
		/// </summary>
		/// <param name="app">Application.</param>
		public ZipPackageInstaller(IApplication app) : base(app)
		{ }


		/// <summary>
		/// Perform operation asynchronously.
		/// </summary>
		/// <param name="cancellationToken">Cancellation token.</param>
		/// <returns>Task of performing operation.</returns>
		protected override Task PerformOperationAsync(CancellationToken cancellationToken) => Task.Run(() =>
		{
			// load zip archive
			this.Logger.LogTrace($"Open ZIP file '{this.PackageFileName}'");
			using var zipArchive = ZipFile.OpenRead(this.PackageFileName.AsNonNull());

			// cancellation check
			if (cancellationToken.IsCancellationRequested)
			{
				this.Logger.LogWarning("Installation has been cancelled");
				throw new TaskCanceledException();
			}

			// create target directory
			var targetRootDirectory = this.TargetDirectoryPath.AsNonNull();
			this.Logger.LogDebug($"Install to {targetRootDirectory}");
			if (File.Exists(targetRootDirectory))
				throw new ArgumentException($"'{targetRootDirectory}' is a file.");
			if (!Directory.Exists(targetRootDirectory))
			{
				this.Logger.LogDebug($"Create {targetRootDirectory}");
				Directory.CreateDirectory(targetRootDirectory);
			}

			// cancellation check
			if (cancellationToken.IsCancellationRequested)
			{
				this.Logger.LogWarning("Installation has been cancelled");
				throw new TaskCanceledException();
			}

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
				{
					this.Logger.LogTrace($"Create directory '{targetDirectory}'");
					Directory.CreateDirectory(targetDirectory);
				}
				if (zipEntryPath.EndsWith(Path.DirectorySeparatorChar))
					continue;
				var retryCount = 10;
				while (true)
				{
					try
					{
						if (!this.OnInstallingFile(targetFileName))
						{
							retryCount = 0;
							throw new Exception($"Installation of '{targetFileName}' was interrupted");
						}
						if (File.Exists(targetFileName))
						{
							this.Logger.LogTrace($"Delete file '{targetFileName}'");
							File.Delete(targetFileName);
						}
						this.Logger.LogTrace($"Install file '{zipEntryPath}' to '{targetFileName}'");
						zipEntry.ExtractToFile(targetFileName, false);
						break;
					}
					catch (Exception ex)
					{
						if (retryCount > 0)
						{
							--retryCount;
							this.Logger.LogError(ex, $"Unable to install file '{zipEntryPath}' to '{targetFileName}', try again");
							Thread.Sleep(500);
						}
						else
						{
							this.Logger.LogError(ex, $"Unable to install file '{zipEntryPath}' to '{targetFileName}'");
							throw;
						}
					}
					if (cancellationToken.IsCancellationRequested)
					{
						this.Logger.LogWarning("Installation has been cancelled");
						throw new TaskCanceledException();
					}
				}
				this.ReportInstalledFilePath(targetFileName);
				this.ReportProgress((double)(++extractedEntryCount) / entryCount);
				if (cancellationToken.IsCancellationRequested)
				{
					this.Logger.LogWarning("Installation has been cancelled");
					throw new TaskCanceledException();
				}
			}
		});
	}
}
