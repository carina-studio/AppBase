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
			public DummyPackageResolver(string packageFilePath)
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
				for (var i = 0; i < bytes.Length; ++i)
					hashBuilder.AppendFormat("{0:X2}", bytes[i]);
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
				using (var updater = new Updater()
				{
					ApplicationDirectoryPath = baseAppDirectory,
					PackageInstaller = new ZipPackageInstaller(),
					PackageResolver = new DummyPackageResolver(packageFilePath) { Source = new MemoryStreamProvider() },
				})
				{
					Assert.IsFalse(updater.Cancel());
					Assert.IsTrue(updater.Start());
					Assert.IsTrue(updater.Cancel());
					Assert.IsTrue(await updater.WaitForPropertyAsync(nameof(Updater.State), UpdaterState.Cancelled, 10000));
				}

				// verify installed files
				Assert.IsTrue(baseAppFilePaths.SetEquals(this.CollectFilePaths(baseAppDirectory)));

				// update and cancel when resolving package
				using (var updater = new Updater()
				{
					ApplicationDirectoryPath = baseAppDirectory,
					PackageInstaller = new ZipPackageInstaller(),
					PackageResolver = new DummyPackageResolver(packageFilePath) { Source = new MemoryStreamProvider() },
				})
				{
					Assert.IsTrue(updater.Start());
					await Task.Delay(500);
					Assert.AreEqual(UpdaterState.ResolvingPackage, updater.State);
					Assert.IsTrue(updater.Cancel());
					Assert.IsTrue(await updater.WaitForPropertyAsync(nameof(Updater.State), UpdaterState.Cancelled, 10000));
				}

				// verify installed files
				Assert.IsTrue(baseAppFilePaths.SetEquals(this.CollectFilePaths(baseAppDirectory)));

				// update and cancel when downloading package
				using (var updater = new Updater()
				{
					ApplicationDirectoryPath = baseAppDirectory,
					PackageInstaller = new ZipPackageInstaller(),
					PackageResolver = new DummyPackageResolver(packageFilePath) { Source = new MemoryStreamProvider() },
				})
				{
					Assert.IsTrue(updater.Start());
					Assert.IsTrue(await updater.WaitForPropertyAsync(nameof(Updater.State), UpdaterState.DownloadingPackage, 10000));
					await Task.Delay(500);
					Assert.IsTrue(updater.Cancel());
					Assert.IsTrue(await updater.WaitForPropertyAsync(nameof(Updater.State), UpdaterState.Cancelled, 10000));
				}

				// verify installed files
				Assert.IsTrue(baseAppFilePaths.SetEquals(this.CollectFilePaths(baseAppDirectory)));

				// update and cancel when installing package
				using (var updater = new Updater()
				{
					ApplicationDirectoryPath = baseAppDirectory,
					PackageInstaller = new ZipPackageInstaller(),
					PackageResolver = new DummyPackageResolver(packageFilePath) { Source = new MemoryStreamProvider() },
				})
				{
					Assert.IsTrue(updater.Start());
					Assert.IsTrue(await updater.WaitForPropertyAsync(nameof(Updater.State), UpdaterState.InstallingPackage, 60000));
					Assert.IsTrue(updater.Cancel());
					Assert.IsTrue(await updater.WaitForPropertyAsync(nameof(Updater.State), UpdaterState.Cancelled, 10000));
				}

				// verify installed files
				Assert.IsTrue(baseAppFilePaths.SetEquals(this.CollectFilePaths(baseAppDirectory)));

				// update and cancel after completed
				using (var updater = new Updater()
				{
					ApplicationDirectoryPath = baseAppDirectory,
					PackageInstaller = new ZipPackageInstaller(),
					PackageResolver = new DummyPackageResolver(packageFilePath) { Source = new MemoryStreamProvider() },
				})
				{
					Assert.IsTrue(updater.Start());
					Assert.IsTrue(await updater.WaitForPropertyAsync(nameof(Updater.State), UpdaterState.Succeeded, 60000));
					Assert.IsFalse(updater.Cancel());
				}

				// verify installed files
				Assert.IsTrue(expectedUpdatedFilePaths.SetEquals(this.CollectFilePaths(baseAppDirectory)));
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
				var packageFilePaths = this.CollectFilePaths(packageDirectory);
				var packageFilePath = this.GeneratePackageFile(packageDirectory);
				this.remotePackageFilePath = packageFilePath;

				// update and dispose immediately
				using (var updater = new Updater()
				{
					ApplicationDirectoryPath = baseAppDirectory,
					PackageInstaller = new ZipPackageInstaller(),
					PackageResolver = new DummyPackageResolver(packageFilePath) { Source = new MemoryStreamProvider() },
				})
				{
					Assert.IsFalse(updater.Cancel());
					Assert.IsTrue(updater.Start());
					updater.Dispose();
					Assert.AreEqual(UpdaterState.Disposed, updater.State);
				}

				// verify installed files
				Assert.IsTrue(baseAppFilePaths.SetEquals(this.CollectFilePaths(baseAppDirectory)));

				// update and dispose when resolving package
				using (var updater = new Updater()
				{
					ApplicationDirectoryPath = baseAppDirectory,
					PackageInstaller = new ZipPackageInstaller(),
					PackageResolver = new DummyPackageResolver(packageFilePath) { Source = new MemoryStreamProvider() },
				})
				{
					Assert.IsTrue(updater.Start());
					await Task.Delay(500);
					Assert.AreEqual(UpdaterState.ResolvingPackage, updater.State);
					updater.Dispose();
					Assert.AreEqual(UpdaterState.Disposed, updater.State);
				}

				// verify installed files
				Assert.IsTrue(baseAppFilePaths.SetEquals(this.CollectFilePaths(baseAppDirectory)));

				// update and dispose when downloading package
				using (var updater = new Updater()
				{
					ApplicationDirectoryPath = baseAppDirectory,
					PackageInstaller = new ZipPackageInstaller(),
					PackageResolver = new DummyPackageResolver(packageFilePath) { Source = new MemoryStreamProvider() },
				})
				{
					Assert.IsTrue(updater.Start());
					Assert.IsTrue(await updater.WaitForPropertyAsync(nameof(Updater.State), UpdaterState.DownloadingPackage, 10000));
					await Task.Delay(500);
					updater.Dispose();
					Assert.AreEqual(UpdaterState.Disposed, updater.State);
				}

				// verify installed files
				Assert.IsTrue(baseAppFilePaths.SetEquals(this.CollectFilePaths(baseAppDirectory)));

				// update and dispose when installing package
				using (var updater = new Updater()
				{
					ApplicationDirectoryPath = baseAppDirectory,
					PackageInstaller = new ZipPackageInstaller(),
					PackageResolver = new DummyPackageResolver(packageFilePath) { Source = new MemoryStreamProvider() },
				})
				{
					Assert.IsTrue(updater.Start());
					Assert.IsTrue(await updater.WaitForPropertyAsync(nameof(Updater.State), UpdaterState.InstallingPackage, 60000));
					updater.Dispose();
					Assert.AreEqual(UpdaterState.Disposed, updater.State);
				}

				// verify installed files
				Assert.IsTrue(baseAppFilePaths.SetEquals(this.CollectFilePaths(baseAppDirectory)));
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
				var packageFilePaths = this.CollectFilePaths(packageDirectory);
				var packageFilePath = this.GeneratePackageFile(packageDirectory);
				this.remotePackageFilePath = null;

				// update application
				using var updater = new Updater()
				{
					ApplicationDirectoryPath = baseAppDirectory,
					PackageInstaller = new ZipPackageInstaller(),
					PackageResolver = new DummyPackageResolver(packageFilePath) { Source = new MemoryStreamProvider() },
				};
				Assert.IsTrue(updater.Start());
				Assert.IsTrue(await updater.WaitForPropertyAsync(nameof(Updater.State), UpdaterState.ResolvingPackage, 1000));
				Assert.IsTrue(await updater.WaitForPropertyAsync(nameof(Updater.State), UpdaterState.DownloadingPackage, 5000));
				Assert.IsTrue(await updater.WaitForPropertyAsync(nameof(Updater.State), UpdaterState.VerifyingPackage, 60000));
				Assert.IsTrue(await updater.WaitForPropertyAsync(nameof(Updater.State), UpdaterState.Failed, 10000));

				// verify installed files
				Assert.IsTrue(baseAppFilePaths.SetEquals(this.CollectFilePaths(baseAppDirectory)));
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
			foreach (var fileName in Directory.EnumerateFiles(directory))
				zipArchive.CreateEntryFromFile(fileName, Path.GetRelativePath(rootDirectory, fileName).Replace('\\', '/'));
			foreach (var subDirectory in Directory.EnumerateDirectories(directory))
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
				var baseAppFilePaths = this.CollectFilePaths(baseAppDirectory);

				// prepare update package
				var packageDirectory = this.GenerateRandomApplication();
				var packageFilePaths = this.CollectFilePaths(packageDirectory);
				var packageFilePath = this.GeneratePackageFile(packageDirectory);
				this.remotePackageFilePath = packageFilePath;

				// check progress reported when downloading package
				using var updater = new Updater()
				{
					ApplicationDirectoryPath = baseAppDirectory,
					PackageInstaller = new ZipPackageInstaller(),
					PackageResolver = new DummyPackageResolver(packageFilePath) { Source = new MemoryStreamProvider() },
				};
				var prevProgress = updater.Progress;
				var hasIncrementalProgressChange = false;
				var propertyChangedHandler = new PropertyChangedEventHandler((_, e) =>
				{
					if (e.PropertyName == nameof(Updater.Progress))
					{
						var progress = updater.Progress;
						if (double.IsFinite(progress))
						{
							Assert.IsTrue(double.IsNaN(prevProgress) || prevProgress < progress);
							hasIncrementalProgressChange = true;
						}
						prevProgress = progress;
					}
				});
				Assert.IsTrue(updater.Start());
				Assert.IsTrue(await updater.WaitForPropertyAsync(nameof(Updater.State), UpdaterState.DownloadingPackage, 5000));
				Assert.IsTrue(await updater.WaitForPropertyAsync(nameof(Updater.Progress), 0.0, 5000));
				updater.PropertyChanged += propertyChangedHandler;
				Assert.IsTrue(await updater.WaitForPropertyAsync(nameof(Updater.State), UpdaterState.InstallingPackage, 60000));
				updater.PropertyChanged -= propertyChangedHandler;
				Assert.IsTrue(hasIncrementalProgressChange);
				Assert.IsTrue(await updater.WaitForPropertyAsync(nameof(Updater.State), UpdaterState.Succeeded, 10000));
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
					var context = (HttpListenerContext?)null;
					try
					{
						context = this.httpListener.GetContext();
					}
					catch
					{
						if (this.httpListener?.IsListening != true)
							break;
						throw;
					}

					// load update package
					var updatePackage = this.remotePackageFilePath?.Let(it =>
					{
						return new FileStream(it, FileMode.Open, FileAccess.Read).Use(stream => stream.ReadAllBytes());
					}) ?? new byte[0];

					// response
					using (var response = context.Response)
					{
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
							catch
							{ }
							Thread.Sleep(100);
						}
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
				using var updater = new Updater()
				{
					ApplicationDirectoryPath = baseAppDirectory,
					PackageInstaller = new ZipPackageInstaller(),
					PackageResolver = new DummyPackageResolver(packageFilePath) { Source = new MemoryStreamProvider() },
				};
				Assert.IsTrue(updater.Start());
				Assert.IsTrue(await updater.WaitForPropertyAsync(nameof(Updater.State), UpdaterState.ResolvingPackage, 1000));
				Assert.IsTrue(await updater.WaitForPropertyAsync(nameof(Updater.State), UpdaterState.DownloadingPackage, 5000));
				Assert.IsTrue(await updater.WaitForPropertyAsync(nameof(Updater.State), UpdaterState.VerifyingPackage, 60000));
				Assert.IsTrue(await updater.WaitForPropertyAsync(nameof(Updater.State), UpdaterState.InstallingPackage, 5000));
				Assert.IsTrue(await updater.WaitForPropertyAsync(nameof(Updater.State), UpdaterState.Succeeded, 10000));

				// verify installed files
				Assert.IsTrue(expectedUpdatedFilePaths.SetEquals(this.CollectFilePaths(baseAppDirectory)));
			});
		}
	}
}
