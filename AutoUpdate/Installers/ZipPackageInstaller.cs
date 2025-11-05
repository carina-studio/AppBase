using CarinaStudio.IO;
#if NET9_0_OR_GREATER
using CarinaStudio.Logging;
#else
using Microsoft.Extensions.Logging;
#endif
using System;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Directory = System.IO.Directory;
using File = System.IO.File;

namespace CarinaStudio.AutoUpdate.Installers;

/// <summary>
/// Implementation of <see cref="IPackageInstaller"/> which install package in ZIP format.
/// </summary>
public class ZipPackageInstaller : BasePackageInstaller
{
	// Information of application icon
	record AppIconInfo(string FilePath, string? FileHashCode);
	
	
	// Fields.
	bool isAppIconUpdated;
	
	
	/// <summary>
	/// Initialize new <see cref="ZipPackageInstaller"/> instance.
	/// </summary>
	/// <param name="app">Application.</param>
	public ZipPackageInstaller(IApplication app) : base(app)
	{ }


	// Get path of application icon file.
	async Task<AppIconInfo?> GetMacOSApplicationIconAsync(CancellationToken cancellationToken)
	{
		// check state
		var targetRootDirectory = this.TargetDirectoryPath.AsNonNull();
		if (!targetRootDirectory.EndsWith(".app") && !targetRootDirectory.EndsWith(".app/")) 
			return null;
		
		// check resource directory
		var resDirectory = Path.Combine(targetRootDirectory, "Contents", "Resources");
		var isResDirectoryExists = await Task.Run(() =>
		{
			try
			{
				return Directory.Exists(resDirectory);
			}
			catch
			{
				return false;
			}
		}, cancellationToken);
		if (!isResDirectoryExists)
		{
			this.Logger.LogWarning("No resource folder in application bundle");
			return null;
		}
		
		// get application icon
		// ReSharper disable AccessToDisposedClosure
		var iconName = await Task.Run(() =>
		{
			try
			{
				// load XML
				using var reader = new StreamReader(Path.Combine(targetRootDirectory, "Contents", "Info.plist"), Encoding.UTF8);
				var xmlDocument = new XmlDocument().Also(it => it.Load(reader));
				if (cancellationToken.IsCancellationRequested)
					throw new TaskCanceledException();

				// parse
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
			}
			// ReSharper disable once EmptyGeneralCatchClause
			catch (Exception ex)
			{
				if (ex is TaskCanceledException)
					throw;
			}
			return null;
		}, cancellationToken);
		// ReSharper restore AccessToDisposedClosure
		
		// calculate hash code of icon file
		var iconFilePath = string.IsNullOrEmpty(iconName) ? null : Path.Combine(resDirectory, iconName);
		var iconHashCode = string.IsNullOrEmpty(iconFilePath)
			? null
			: await Task.Run(() =>
			{
				try
				{
					this.Logger.LogTrace("Compute hash code of application icon '{path}'", iconFilePath);
					using var md5 = MD5.Create();
					using var stream = new FileStream(iconFilePath, FileMode.Open, FileAccess.Read);
					return Convert.ToBase64String(md5.ComputeHash(stream));
				}
				catch (Exception ex)
				{
					this.Logger.LogError(ex, "Error occurred while computing hash code of application icon '{path}'", iconFilePath);
					return null;
				}
			}, cancellationToken);
		
		// complete
		return string.IsNullOrEmpty(iconFilePath) ? null : new AppIconInfo(iconFilePath, iconHashCode);
	}


	/// <inheritdoc/>
	public override bool IsApplicationIconUpdated => this.isAppIconUpdated;


	/// <summary>
	/// Perform operation asynchronously.
	/// </summary>
	/// <param name="cancellationToken">Cancellation token.</param>
	/// <returns>Task of performing operation.</returns>
	protected override Task PerformOperationAsync(CancellationToken cancellationToken) => Task.Run(async () =>
	{
		// load zip archive
		this.Logger.LogTrace("Open ZIP file '{packageFileName}'", this.PackageFileName);
#if NET10_0_OR_GREATER
		await using var zipArchive = await ZipFile.OpenReadAsync(this.PackageFileName.AsNonNull(), cancellationToken);
#else
		using var zipArchive = ZipFile.OpenRead(this.PackageFileName.AsNonNull());
#endif

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
		
		// get original application icon on macOS
		var originalAppIconInfo = Platform.IsMacOS
			? await GetMacOSApplicationIconAsync(cancellationToken)
			: null;
		if (originalAppIconInfo is not null)
			this.Logger.LogDebug("Original application icon: '{path}'", originalAppIconInfo.FilePath);

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
#if NET10_0_OR_GREATER
					await zipEntry.ExtractToFileAsync(targetFileName, false, cancellationToken);
#else
					zipEntry.ExtractToFile(targetFileName, false);
#endif
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
		
		// check whether application icon has been updated or not
		if (Platform.IsMacOS)
		{
			var newAppIconInfo = await GetMacOSApplicationIconAsync(cancellationToken);
			this.Logger.LogDebug("New application icon: '{path}'", newAppIconInfo?.FilePath);
			if (newAppIconInfo is not null)
			{
				if (originalAppIconInfo is not null)
				{
					this.isAppIconUpdated = !PathEqualityComparer.Default.Equals(originalAppIconInfo.FilePath, newAppIconInfo.FilePath)
					                        || originalAppIconInfo.FileHashCode != newAppIconInfo.FileHashCode;
				}
				else
					this.isAppIconUpdated = true;
			}
			else if (originalAppIconInfo is not null)
				this.isAppIconUpdated = true;
		}
	}, cancellationToken);
}