using CarinaStudio.AutoUpdate.Installers;
using CarinaStudio.AutoUpdate.Resolvers;
using CarinaStudio.IO;
using CarinaStudio.Tests;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CarinaStudio.AutoUpdate
{
	/// <summary>
	/// Tests of <see cref="Updater"/>.
	/// </summary>
	[TestFixture]
	class UpdaterTests : BaseTests
	{
		// Dummy package resolver.
		class DummyPackageResolver : BasePackageResolver
		{
			public DummyPackageResolver(IApplication app, string packageFilePath) : base(app)
            {
				using var stream = new FileStream(packageFilePath, FileMode.Open, FileAccess.Read);
				using (var hashAlgorithm = System.Security.Cryptography.MD5.Create())
					this.MD5 = ComputeHash(stream, hashAlgorithm);
				using (var hashAlgorithm = System.Security.Cryptography.SHA256.Create())
					this.SHA256 = ComputeHash(stream, hashAlgorithm);
				using (var hashAlgorithm = System.Security.Cryptography.SHA512.Create())
					this.SHA512 = ComputeHash(stream, hashAlgorithm);
			}

			static string ComputeHash(FileStream stream, HashAlgorithm hashAlgorithm)
			{
				var bytes = hashAlgorithm.ComputeHash(stream);
				var hashBuilder = new StringBuilder();
				foreach (var b in bytes)
					hashBuilder.AppendFormat("{0:X2}", b);
				stream.Position = 0;
				return hashBuilder.ToString();
			}

			protected override async Task PerformOperationAsync(CancellationToken cancellationToken)
			{
				await Task.Delay(1000, cancellationToken);
				this.PackageUri = new Uri("http://localhost:9521");
			}
		}


		// Fields.
		readonly HttpListener httpListener = new HttpListener();
		volatile string? remotePackageFilePath;


		/// <summary>
		/// Test for cancellation of updating.
		/// </summary>
		[Test]
		public void CancellationTest()
		{
			this.TestOnApplicationThread(async () =>
			{
				// prepare base application
				var baseAppDirectory = this.GenerateRandomApplication();
				var baseAppFilePaths = this.CollectFilePaths(baseAppDirectory);

				// prepare update package
				var packageDirectory = this.GenerateRandomApplication();
				var packageFilePaths = this.CollectFilePaths(packageDirectory);
				var packageFilePath = this.GeneratePackageFile(packageDirectory);
				var expectedUpdatedFilePaths = new HashSet<string>(baseAppFilePaths, PathEqualityComparer.Default).Also(it =>
				{
					foreach (var filePath in packageFilePaths)
					{
						var relativePath = Path.GetRelativePath(packageDirectory, filePath);
						it.Add(Path.Combine(baseAppDirectory, relativePath));
					}
				});
				this.remotePackageFilePath = packageFilePath;

				// update and cancel immediately
				await new Updater(this.Application).Setup(updater =>
				{
					updater.ApplicationDirectoryPath = baseAppDirectory;
					updater.PackageInstaller = new ZipPackageInstaller(this.Application);
					updater.PackageResolver = new DummyPackageResolver(this.Application, packageFilePath) { Source = new MemoryStreamProvider() };
				}).UseAsync(async updater =>
				{
					Assert.That(!updater.Cancel());
					Assert.That(updater.Start());
					Assert.That(updater.Cancel());
					Assert.That(await updater.WaitForPropertyAsync(nameof(Updater.State), UpdaterState.Cancelled, 10000));
				});

				// verify installed files
				Assert.That(baseAppFilePaths.SetEquals(this.CollectFilePaths(baseAppDirectory)));

				// update and cancel when resolving package
				await new Updater(this.Application).Setup(updater =>
				{
					updater.ApplicationDirectoryPath = baseAppDirectory;
					updater.PackageInstaller = new ZipPackageInstaller(this.Application);
					updater.PackageResolver = new DummyPackageResolver(this.Application, packageFilePath) { Source = new MemoryStreamProvider() };
				}).UseAsync(async updater =>
				{
					Assert.That(updater.Start());
					await Task.Delay(500);
					Assert.That(UpdaterState.ResolvingPackage == updater.State);
					Assert.That(updater.Cancel());
					Assert.That(await updater.WaitForPropertyAsync(nameof(Updater.State), UpdaterState.Cancelled, 10000));
				});

				// verify installed files
				Assert.That(baseAppFilePaths.SetEquals(this.CollectFilePaths(baseAppDirectory)));

				// update and cancel when downloading package
				await new Updater(this.Application).Setup(updater =>
				{
					updater.ApplicationDirectoryPath = baseAppDirectory;
					updater.PackageInstaller = new ZipPackageInstaller(this.Application);
					updater.PackageResolver = new DummyPackageResolver(this.Application, packageFilePath) { Source = new MemoryStreamProvider() };
				}).UseAsync(async updater =>
				{
					Assert.That(updater.Start());
					Assert.That(await updater.WaitForPropertyAsync(nameof(Updater.State), UpdaterState.DownloadingPackage, 10000));
					await Task.Delay(500);
					Assert.That(updater.Cancel());
					Assert.That(await updater.WaitForPropertyAsync(nameof(Updater.State), UpdaterState.Cancelled, 10000));
				});

				// verify installed files
				Assert.That(baseAppFilePaths.SetEquals(this.CollectFilePaths(baseAppDirectory)));

				// update and cancel when installing package
				await new Updater(this.Application).Setup(updater =>
				{
					updater.ApplicationDirectoryPath = baseAppDirectory;
					updater.PackageInstaller = new ZipPackageInstaller(this.Application);
					updater.PackageResolver = new DummyPackageResolver(this.Application, packageFilePath) { Source = new MemoryStreamProvider() };
				}).UseAsync(async updater =>
				{
					Assert.That(updater.Start());
					Assert.That(await updater.WaitForPropertyAsync(nameof(Updater.State), UpdaterState.InstallingPackage, 60000));
					Assert.That(updater.Cancel());
					Assert.That(await updater.WaitForPropertyAsync(nameof(Updater.State), UpdaterState.Cancelled, 10000));
				});

				// verify installed files
				Assert.That(baseAppFilePaths.SetEquals(this.CollectFilePaths(baseAppDirectory)));

				// update and cancel after completed
				await new Updater(this.Application).Setup(updater =>
				{
					updater.ApplicationDirectoryPath = baseAppDirectory;
					updater.PackageInstaller = new ZipPackageInstaller(this.Application);
					updater.PackageResolver = new DummyPackageResolver(this.Application, packageFilePath) { Source = new MemoryStreamProvider() };
				}).UseAsync(async updater =>
				{
					Assert.That(updater.Start());
					Assert.That(await updater.WaitForPropertyAsync(nameof(Updater.State), UpdaterState.Succeeded, 60000));
					Assert.That(!updater.Cancel());
				});

				// verify installed files
				Assert.That(expectedUpdatedFilePaths.SetEquals(this.CollectFilePaths(baseAppDirectory)));
			});
		}


		/// <summary>
		/// Dispose HTTP listener.
		/// </summary>
		[OneTimeTearDown]
		public void DisposeHttpListener()
		{
			this.httpListener.Stop();
		}


		/// <summary>
		/// Test for disposing when updating.
		/// </summary>
		[Test]
		public void DisposingTest()
		{
			this.TestOnApplicationThread(async () =>
			{
				// prepare base application
				var baseAppDirectory = this.GenerateRandomApplication();
				var baseAppFilePaths = this.CollectFilePaths(baseAppDirectory);

				// prepare update package
				var packageDirectory = this.GenerateRandomApplication();
				var packageFilePath = this.GeneratePackageFile(packageDirectory);
				this.remotePackageFilePath = packageFilePath;

				// update and dispose immediately
				new Updater(this.Application).Setup(updater =>
				{
					updater.ApplicationDirectoryPath = baseAppDirectory;
					updater.PackageInstaller = new ZipPackageInstaller(this.Application);
					updater.PackageResolver = new DummyPackageResolver(this.Application, packageFilePath) { Source = new MemoryStreamProvider() };
				}).Use(updater =>
				{
					Assert.That(!updater.Cancel());
					Assert.That(updater.Start());
					// ReSharper disable once DisposeOnUsingVariable
					updater.Dispose();
					Assert.That(UpdaterState.Disposed == updater.State);
				});

				// verify installed files
				Assert.That(baseAppFilePaths.SetEquals(this.CollectFilePaths(baseAppDirectory)));

				// update and dispose when resolving package
				await new Updater(this.Application).Setup(updater =>
				{
					updater.ApplicationDirectoryPath = baseAppDirectory;
					updater.PackageInstaller = new ZipPackageInstaller(this.Application);
					updater.PackageResolver = new DummyPackageResolver(this.Application, packageFilePath) { Source = new MemoryStreamProvider() };
				}).UseAsync(async updater =>
				{
					Assert.That(updater.Start());
					await Task.Delay(500);
					Assert.That(UpdaterState.ResolvingPackage == updater.State);
					// ReSharper disable once DisposeOnUsingVariable
					updater.Dispose();
					Assert.That(UpdaterState.Disposed == updater.State);
				});

				// verify installed files
				Assert.That(baseAppFilePaths.SetEquals(this.CollectFilePaths(baseAppDirectory)));

				// update and dispose when downloading package
				await new Updater(this.Application).Setup(updater =>
				{
					updater.ApplicationDirectoryPath = baseAppDirectory;
					updater.PackageInstaller = new ZipPackageInstaller(this.Application);
					updater.PackageResolver = new DummyPackageResolver(this.Application, packageFilePath) { Source = new MemoryStreamProvider() };
				}).UseAsync(async updater =>
				{
					Assert.That(updater.Start());
					Assert.That(await updater.WaitForPropertyAsync(nameof(Updater.State), UpdaterState.DownloadingPackage, 10000));
					await Task.Delay(500);
					// ReSharper disable once DisposeOnUsingVariable
					updater.Dispose();
					Assert.That(UpdaterState.Disposed == updater.State);
				});

				// verify installed files
				Assert.That(baseAppFilePaths.SetEquals(this.CollectFilePaths(baseAppDirectory)));

				// update and dispose when installing package
				await new Updater(this.Application).Setup(updater =>
				{
					updater.ApplicationDirectoryPath = baseAppDirectory;
					updater.PackageInstaller = new ZipPackageInstaller(this.Application);
					updater.PackageResolver = new DummyPackageResolver(this.Application, packageFilePath) { Source = new MemoryStreamProvider() };
				}).UseAsync(async updater =>
				{
					Assert.That(updater.Start());
					Assert.That(await updater.WaitForPropertyAsync(nameof(Updater.State), UpdaterState.InstallingPackage, 60000));
					// ReSharper disable once DisposeOnUsingVariable
					updater.Dispose();
					Assert.That(UpdaterState.Disposed == updater.State);
				});

				// verify installed files
				Assert.That(baseAppFilePaths.SetEquals(this.CollectFilePaths(baseAppDirectory)));
			});
		}


		/// <summary>
		/// Test for updating failure.
		/// </summary>
		[Test]
		public void FailingTest()
		{
			this.TestOnApplicationThread(async () =>
			{
				// prepare base application
				var baseAppDirectory = this.GenerateRandomApplication();
				var baseAppFilePaths = this.CollectFilePaths(baseAppDirectory);

				// prepare update package
				var packageDirectory = this.GenerateRandomApplication();
				var packageFilePath = this.GeneratePackageFile(packageDirectory);
				this.remotePackageFilePath = null;

				// update application
				await new Updater(this.Application).Setup(updater =>
				{
					updater.ApplicationDirectoryPath = baseAppDirectory;
					updater.PackageInstaller = new ZipPackageInstaller(this.Application);
					updater.PackageResolver = new DummyPackageResolver(this.Application, packageFilePath) { Source = new MemoryStreamProvider() };
				}).UseAsync(async updater =>
				{
					Assert.That(updater.Start());
					Assert.That(await updater.WaitForPropertyAsync(nameof(Updater.State), UpdaterState.ResolvingPackage, 1000));
					Assert.That(await updater.WaitForPropertyAsync(nameof(Updater.State), UpdaterState.DownloadingPackage, 5000));
					Assert.That(await updater.WaitForPropertyAsync(nameof(Updater.State), UpdaterState.VerifyingPackage, 60000));
					Assert.That(await updater.WaitForPropertyAsync(nameof(Updater.State), UpdaterState.Failed, 10000));
				});

				// verify installed files
				Assert.That(baseAppFilePaths.SetEquals(this.CollectFilePaths(baseAppDirectory)));
			});
		}


		// Generate package in ZIP format.
		string GeneratePackageFile(string directory)
		{
			var fileName = Tests.Random.CreateFileWithRandomName(this.RootTempDirectoryPath).Use(it => it.Name);
			using var zipArchive = ZipFile.Open(fileName, ZipArchiveMode.Update);
			this.GeneratePackageFile(directory, directory, zipArchive);
			return fileName;
		}
		void GeneratePackageFile(string rootDirectory, string directory, ZipArchive zipArchive)
		{
			foreach (var fileName in System.IO.Directory.EnumerateFiles(directory))
				zipArchive.CreateEntryFromFile(fileName, Path.GetRelativePath(rootDirectory, fileName).Replace('\\', '/'));
			foreach (var subDirectory in System.IO.Directory.EnumerateDirectories(directory))
				this.GeneratePackageFile(rootDirectory, subDirectory, zipArchive);
		}


		/// <summary>
		/// Test for <see cref="Updater.Progress"/>.
		/// </summary>
		[Test]
		public void ProgressReportingTest()
		{
			this.TestOnApplicationThread(async () =>
			{
				// prepare base application
				var baseAppDirectory = this.GenerateRandomApplication();

				// prepare update package
				var packageDirectory = this.GenerateRandomApplication();
				var packageFilePath = this.GeneratePackageFile(packageDirectory);
				this.remotePackageFilePath = packageFilePath;

				// check progress reported when downloading package
				await new Updater(this.Application).Setup(updater =>
				{
					updater.ApplicationDirectoryPath = baseAppDirectory;
					updater.PackageInstaller = new ZipPackageInstaller(this.Application);
					updater.PackageResolver = new DummyPackageResolver(this.Application, packageFilePath) { Source = new MemoryStreamProvider() };
				}).UseAsync(async updater =>
				{
					var prevProgress = updater.Progress;
					var hasIncrementalProgressChange = false;
					var propertyChangedHandler = new PropertyChangedEventHandler((_, e) =>
					{
						if (e.PropertyName == nameof(Updater.Progress))
						{
							if (updater.State != UpdaterState.DownloadingPackage)
								return;
							var progress = updater.Progress;
							if (double.IsFinite(progress))
							{
								Assert.That(double.IsNaN(prevProgress) || prevProgress < progress);
								hasIncrementalProgressChange = true;
							}
							prevProgress = progress;
						}
					});
					Assert.That(updater.Start());
					Assert.That(await updater.WaitForPropertyAsync(nameof(Updater.State), UpdaterState.DownloadingPackage, 5000));
					Assert.That(await updater.WaitForPropertyAsync(nameof(Updater.Progress), 0.0, 5000));
					updater.PropertyChanged += propertyChangedHandler;
					Assert.That(await updater.WaitForPropertyAsync(nameof(Updater.State), UpdaterState.InstallingPackage, 60000));
					updater.PropertyChanged -= propertyChangedHandler;
					Assert.That(hasIncrementalProgressChange);
					Assert.That(await updater.WaitForPropertyAsync(nameof(Updater.State), UpdaterState.Succeeded, 10000));
				});
			});
		}


		/// <summary>
		/// Setup HTTP listener.
		/// </summary>
		[OneTimeSetUp]
		public void SetupHttpListener()
		{
			this.httpListener.Prefixes.Add("http://localhost:9521/");
			this.httpListener.Start();
			ThreadPool.QueueUserWorkItem(_ =>
			{
				while (true)
				{
					// wait for connection
					HttpListenerContext? context;
					try
					{
						context = this.httpListener.GetContext();
					}
					catch
					{
						if (this.httpListener.IsListening != true)
							break;
						throw;
					}

					// load update package
					var updatePackage = this.remotePackageFilePath?.Let(it => { return new FileStream(it, FileMode.Open, FileAccess.Read).Use(stream => stream.ReadAllBytes()); }) ?? Array.Empty<byte>();

					// response
					using var response = context.Response;
					var offset = 0;
					var chunkSize = updatePackage.Length / 30;
					var stopWatch = new Stopwatch().Also(it => it.Start());
					using var stream = response.OutputStream;
					response.ContentLength64 = updatePackage.Length;
					while (offset < updatePackage.Length)
					{
						try
						{
							if (offset + chunkSize >= updatePackage.Length)
							{
								var delay = (3000 - stopWatch.ElapsedMilliseconds);
								if (delay > 0)
									Thread.Sleep((int)delay);
								stream.Write(updatePackage, offset, updatePackage.Length - offset);
								offset = updatePackage.Length;
							}
							else
							{
								stream.Write(updatePackage, offset, chunkSize);
								offset += chunkSize;
							}
							stream.Flush();
						}
						catch (Exception ex)
						{
							Console.Error.WriteLine("Error occurred while sending update package");
							Console.Error.WriteLine(ex.Message);
							Console.Error.WriteLine(ex.StackTrace);
							break;
						}
						Thread.Sleep(100);
					}
				}
			});
		}


		/// <summary>
		/// Test for updating application.
		/// </summary>
		[Test]
		public void UpdatingTest()
		{
			this.TestOnApplicationThread(async () =>
			{
				// prepare base application
				var baseAppDirectory = this.GenerateRandomApplication();
				var baseAppFilePaths = this.CollectFilePaths(baseAppDirectory);

				// prepare update package
				var packageDirectory = this.GenerateRandomApplication();
				var packageFilePaths = this.CollectFilePaths(packageDirectory);
				var packageFilePath = this.GeneratePackageFile(packageDirectory);
				var expectedUpdatedFilePaths = new HashSet<string>(baseAppFilePaths, PathEqualityComparer.Default).Also(it =>
				{
					foreach (var filePath in packageFilePaths)
					{
						var relativePath = Path.GetRelativePath(packageDirectory, filePath);
						it.Add(Path.Combine(baseAppDirectory, relativePath));
					}
				});
				this.remotePackageFilePath = packageFilePath;

				// update application
				await new Updater(this.Application).Setup(updater =>
				{
					updater.ApplicationDirectoryPath = baseAppDirectory;
					updater.PackageInstaller = new ZipPackageInstaller(this.Application);
					updater.PackageResolver = new DummyPackageResolver(this.Application, packageFilePath) { Source = new MemoryStreamProvider() };
				}).UseAsync(async updater =>
				{
					Assert.That(updater.Start());
					Assert.That(await updater.WaitForPropertyAsync(nameof(Updater.State), UpdaterState.ResolvingPackage, 1000));
					Assert.That(await updater.WaitForPropertyAsync(nameof(Updater.State), UpdaterState.DownloadingPackage, 5000));
					Assert.That(await updater.WaitForPropertyAsync(nameof(Updater.State), UpdaterState.VerifyingPackage, 60000));
					Assert.That(await updater.WaitForPropertyAsync(nameof(Updater.State), UpdaterState.InstallingPackage, 5000));
					Assert.That(await updater.WaitForPropertyAsync(nameof(Updater.State), UpdaterState.Succeeded, 10000));
				});

				// verify installed files
				Assert.That(expectedUpdatedFilePaths.SetEquals(this.CollectFilePaths(baseAppDirectory)));
			});
		}
	}
}
