using CarinaStudio.IO;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Directory = System.IO.Directory;
using File = System.IO.File;

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


		// Delete old application icons on macOS.
		void DeleteOldApplicationIconsOnMacOS(CancellationToken cancellationToken)
		{
			// check state
			if (cancellationToken.IsCancellationRequested)
			{
				this.Logger.LogWarning("Installation has been cancelled");
				throw new TaskCanceledException();
			}
			var targetRootDirectory = this.TargetDirectoryPath.AsNonNull();
			if (!targetRootDirectory.EndsWith(".app") && !targetRootDirectory.EndsWith(".app/"))
			{
				this.Logger.LogInformation("No need to delete old application icons because root directory is not an application bundle");
				return;
			}
			
			// check resource directory
			var resDirectory = Path.Combine(targetRootDirectory, "Contents", "Resources");
			if (!Global.RunOrDefault(() => Directory.Exists(resDirectory)))
			{
				this.Logger.LogWarning("No resource folder in application bundle");
				return;
			}
			
			// get application icon
			// ReSharper disable AccessToDisposedClosure
			var appIconName = Global.RunOrDefault(() =>
			{
				using var reader = new StreamReader(Path.Combine(targetRootDirectory, "Contents", "Info.plist"), Encoding.UTF8);
				var xmlDocument = new XmlDocument().Also(it => it.Load(reader));
				var plistNode = xmlDocument.FirstChild;
				while (plistNode is not null)
				{
					if (plistNode is XmlElement && plistNode.Name == "plist")
					{
						var dictNode = plistNode.FirstChild;
						while (dictNode is not null)
						{
							if (dictNode is XmlElement && dictNode.Name == "dict")
							{
								var keyNode = dictNode.FirstChild;
								while (keyNode is not null)
								{
									if (keyNode is XmlElement && keyNode.Name == "key" && keyNode.InnerText == "CFBundleIconFile")
									{
										if (keyNode.NextSibling is XmlElement valueNode && valueNode.Name == "string")
											return valueNode.InnerText;
										break;
									}
									keyNode = keyNode.NextSibling;
								}
								break;
							}
							dictNode = dictNode.NextSibling;
						}
						break;
					}
					plistNode = plistNode.NextSibling;
				}
				return null;
			});
			// ReSharper restore AccessToDisposedClosure
			if (string.IsNullOrWhiteSpace(appIconName))
			{
				this.Logger.LogWarning("Application icon is undefined in application bundle");
				return;
			}
			
			// cancellation check
			if (cancellationToken.IsCancellationRequested)
			{
				this.Logger.LogWarning("Installation has been cancelled");
				throw new TaskCanceledException();
			}
			
			// delete application icons
			try
			{
				this.Logger.LogDebug("Application icon is '{name}'", appIconName);
				var appIconFilePath = Path.Combine(resDirectory, appIconName);
				foreach (var iconFilePath in Directory.EnumerateFiles(resDirectory, "*.icns"))
				{
					if (PathEqualityComparer.Default.Equals(iconFilePath, appIconFilePath))
						continue;
					try
					{
						this.Logger.LogTrace("Delete old application icon '{name}'", Path.GetFileName(iconFilePath));
						File.Delete(iconFilePath);
					}
					catch (Exception ex)
					{
						this.Logger.LogError(ex, "Unable to delete application icon '{name}'", Path.GetFileName(iconFilePath));
					}
				}
			}
			catch (Exception ex)
			{
				this.Logger.LogError(ex, "Error occurred while deleting old application icons");
			}
		}


		/// <summary>
		/// Perform operation asynchronously.
		/// </summary>
		/// <param name="cancellationToken">Cancellation token.</param>
		/// <returns>Task of performing operation.</returns>
		protected override Task PerformOperationAsync(CancellationToken cancellationToken) => Task.Run(() =>
		{
			// load zip archive
			this.Logger.LogTrace("Open ZIP file '{packageFileName}'", this.PackageFileName);
			using var zipArchive = ZipFile.OpenRead(this.PackageFileName.AsNonNull());

			// cancellation check
			if (cancellationToken.IsCancellationRequested)
			{
				this.Logger.LogWarning("Installation has been cancelled");
				throw new TaskCanceledException();
			}

			// create target directory
			var targetRootDirectory = this.TargetDirectoryPath.AsNonNull();
			this.Logger.LogDebug("Install to {targetRootDirectory}", targetRootDirectory);
			if (File.Exists(targetRootDirectory))
				throw new ArgumentException($"'{targetRootDirectory}' is a file.");
			if (!Directory.Exists(targetRootDirectory))
			{
				this.Logger.LogDebug("Create {targetRootDirectory}", targetRootDirectory);
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
			var isTargetRootDirAnAppBundle = Platform.IsMacOS && targetRootDirectoryName.EndsWith(".app");
			if (isTargetRootDirAnAppBundle)
				this.Logger.LogDebug("Target root directory is an application bundle");
			this.ReportProgress(0);
			foreach (var zipEntry in zipArchive.Entries)
			{
				var zipEntryPath = zipEntry.FullName.Let(it => Path.DirectorySeparatorChar switch
				{
					'\\' => it.Replace('/', '\\'),
					'/' => it.Replace('\\', '/'),
					_ => it,
				});
				if (isTargetRootDirAnAppBundle)
				{
					var zipEntryPathSegments = zipEntryPath.Split('/');
					if (zipEntryPathSegments[0].EndsWith(".app"))
					{
						if (zipEntryPathSegments.Length == 1)
							continue;
						zipEntryPath = zipEntryPath[(zipEntryPathSegments[0].Length + 1)..];
						if (string.IsNullOrEmpty(zipEntryPath))
							continue;
					}
				}
				var targetFileName = Path.Combine(targetRootDirectory, zipEntryPath);
				var targetDirectory = Path.GetDirectoryName(targetFileName);
				if (targetDirectory != null)
				{
					this.Logger.LogTrace("Create directory '{targetDirectory}'", targetRootDirectory);
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
							this.Logger.LogTrace("Delete file '{targetFileName}'", targetFileName);
							File.Delete(targetFileName);
						}
						this.Logger.LogTrace("Install file '{zipEntryPath}' to '{targetFileName}'", zipEntryPath, targetFileName);
						zipEntry.ExtractToFile(targetFileName, false);
						break;
					}
					catch (Exception ex)
					{
						if (retryCount > 0)
						{
							--retryCount;
							this.Logger.LogError(ex, "Unable to install file '{zipEntryPath}' to '{targetFileName}', try again", zipEntryPath, targetFileName);
							Thread.Sleep(500);
						}
						else
						{
							this.Logger.LogError(ex, "Unable to install file '{zipEntryPath}' to '{targetFileName}'", zipEntryPath, targetFileName);
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
			
			// delete old application icons
			if (this.DeleteOldApplicationIcons && Platform.IsMacOS)
				this.DeleteOldApplicationIconsOnMacOS(cancellationToken);
		}, cancellationToken);
	}
}
