using CarinaStudio.AutoUpdate.Resolvers;
using CarinaStudio.IO;
using CarinaStudio.Tests;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CarinaStudio.AutoUpdate.ViewModels
{
	/// <summary>
	/// Tests of <see cref="UpdatingSession"/>.
	/// </summary>
	[TestFixture]
	class UpdatingSessionTests : BaseTests
	{
		// Dummy package resolver.
		class DummyPackageResolver : BasePackageResolver
		{
			public DummyPackageResolver(IApplication app) : base(app)
			{ }

			protected override async Task PerformOperationAsync(CancellationToken cancellationToken)
			{
				await Task.Delay(1000, cancellationToken);
				this.PackageUri = new Uri("http://localhost:9521");
			}
		}


		// Updating session.
		class UpdatingSessionImpl : UpdatingSession
		{
			public UpdatingSessionImpl(IApplication app) : base(app)
			{ }
			protected override IPackageResolver CreatePackageResolver(IStreamProvider source) =>
				new DummyPackageResolver(this.Application) { Source = source };
		}


		// Fields.
		readonly HttpListener httpListener = new HttpListener();
		volatile string? remotePackageFilePath;


		/// <summary>
		/// Test for cancellation when updating application.
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
				var packageFilePath = this.GeneratePackageFile(packageDirectory);
				this.remotePackageFilePath = packageFilePath;

				// update application and cancel immediately
				await new UpdatingSessionImpl(this.Application).Setup(session =>
				{
					session.ApplicationDirectoryPath = baseAppDirectory;
					session.PackageManifestSource = new MemoryStreamProvider();
				}).UseAsync(async session =>
				{
					Assert.That(session.StartUpdatingCommand.CanExecute(null));
					session.StartUpdatingCommand.Execute(null);
					await Task.Delay(50);
					Assert.That(session.CancelUpdatingCommand.CanExecute(null));
					session.CancelUpdatingCommand.Execute(null);
					Assert.That(!session.CancelUpdatingCommand.CanExecute(null));
					Assert.That(session.IsUpdatingCancelling);
					Assert.That(await session.WaitForPropertyAsync(nameof(UpdatingSession.IsUpdatingCompleted), true, 60000));
					await Task.Delay(500);
					Assert.That(session.IsUpdatingCancelled);
					Assert.That(!session.IsUpdatingFailed);
					Assert.That(!session.IsUpdatingSucceeded);
				});

				// verify installed files
				Assert.That(baseAppFilePaths.SetEquals(this.CollectFilePaths(baseAppDirectory)));

				// update application and cancel when downloading package
				await new UpdatingSessionImpl(this.Application).Setup(session =>
				{
					session.ApplicationDirectoryPath = baseAppDirectory;
					session.PackageManifestSource = new MemoryStreamProvider();
				}).UseAsync(async session =>
				{
					Assert.That(session.StartUpdatingCommand.CanExecute(null));
					session.StartUpdatingCommand.Execute(null);
					Assert.That(await session.WaitForPropertyAsync(nameof(UpdatingSession.IsDownloadingPackage), true, 60000));
					await Task.Delay(50);
					Assert.That(session.CancelUpdatingCommand.CanExecute(null));
					session.CancelUpdatingCommand.Execute(null);
					Assert.That(!session.CancelUpdatingCommand.CanExecute(null));
					Assert.That(session.IsUpdatingCancelling);
					Assert.That(await session.WaitForPropertyAsync(nameof(UpdatingSession.IsUpdatingCompleted), true, 60000));
					await Task.Delay(500);
					Assert.That(session.IsUpdatingCancelled);
					Assert.That(!session.IsUpdatingFailed);
					Assert.That(!session.IsUpdatingSucceeded);
				});

				// verify installed files
				Assert.That(baseAppFilePaths.SetEquals(this.CollectFilePaths(baseAppDirectory)));

				// update application and cancel when installing package
				await new UpdatingSessionImpl(this.Application).Setup(session =>
				{
					session.ApplicationDirectoryPath = baseAppDirectory;
					session.PackageManifestSource = new MemoryStreamProvider();
				}).UseAsync(async session =>
				{
					Assert.That(session.StartUpdatingCommand.CanExecute(null));
					session.StartUpdatingCommand.Execute(null);
					Assert.That(await session.WaitForPropertyAsync(nameof(UpdatingSession.IsInstallingPackage), true, 60000));
					Assert.That(session.CancelUpdatingCommand.CanExecute(null));
					session.CancelUpdatingCommand.Execute(null);
					Assert.That(!session.CancelUpdatingCommand.CanExecute(null));
					Assert.That(session.IsUpdatingCancelling);
					Assert.That(await session.WaitForPropertyAsync(nameof(UpdatingSession.IsUpdatingCompleted), true, 60000));
					await Task.Delay(500);
					Assert.That(session.IsUpdatingCancelled);
					Assert.That(!session.IsUpdatingFailed);
					Assert.That(!session.IsUpdatingSucceeded);
				});

				// verify installed files
				Assert.That(baseAppFilePaths.SetEquals(this.CollectFilePaths(baseAppDirectory)));
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
		/// Test for failing to update application.
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
				this.remotePackageFilePath = null;

				// update application
				await new UpdatingSessionImpl(this.Application).Setup(session =>
				{
					session.ApplicationDirectoryPath = baseAppDirectory;
					session.PackageManifestSource = new MemoryStreamProvider();
				}).UseAsync(async session =>
				{
					Assert.That(session.StartUpdatingCommand.CanExecute(null));
					session.StartUpdatingCommand.Execute(null);
					Assert.That(await session.WaitForPropertyAsync(nameof(UpdatingSession.IsUpdatingCompleted), true, 60000));
					await Task.Delay(500);
					Assert.That(!session.IsUpdatingCancelled);
					Assert.That(session.IsUpdatingFailed);
					Assert.That(!session.IsUpdatingSucceeded);
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
					var updatePackage = this.remotePackageFilePath?.Let(it =>
					{
						return new FileStream(it, FileMode.Open, FileAccess.Read).Use(stream => stream.ReadAllBytes());
					}) ?? Array.Empty<byte>();

					// response
					context.Response.Let(response =>
					{
						response.ContentLength64 = updatePackage.Length;
						response.ContentEncoding = Encoding.UTF8;
						using var stream = response.OutputStream;
						Thread.Sleep(3000);
						stream.Write(updatePackage, 0, updatePackage.Length);
						stream.Flush();
					});
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

				// setup session
				using var session = new UpdatingSessionImpl(this.Application).Setup(session =>
				{
					session.ApplicationDirectoryPath = baseAppDirectory;
					session.PackageManifestSource = new MemoryStreamProvider();
				});
				Assert.That(!session.CancelUpdatingCommand.CanExecute(null));
				Assert.That(!session.IsProgressAvailable);
				Assert.That(!session.IsUpdating);
				Assert.That(!session.IsUpdatingCancelled);
				Assert.That(!session.IsUpdatingCancelling);
				Assert.That(!session.IsUpdatingCompleted);
				Assert.That(!session.IsUpdatingFailed);
				Assert.That(!session.IsUpdatingSucceeded);

				// update application
				Assert.That(session.StartUpdatingCommand.CanExecute(null));
				session.StartUpdatingCommand.Execute(null);
				Assert.That(!session.StartUpdatingCommand.CanExecute(null));
				Assert.That(session.IsUpdating);
				Assert.That(!session.IsUpdatingCancelled);
				Assert.That(!session.IsUpdatingCancelling);
				Assert.That(!session.IsUpdatingCompleted);
				Assert.That(!session.IsUpdatingFailed);
				Assert.That(!session.IsUpdatingSucceeded);
				Assert.That(await session.WaitForPropertyAsync(nameof(UpdatingSession.IsUpdatingCompleted), true, 60000));
				await Task.Delay(500);
				Assert.That(!session.CancelUpdatingCommand.CanExecute(null));
				Assert.That(!session.StartUpdatingCommand.CanExecute(null));
				Assert.That(!session.IsProgressAvailable);
				Assert.That(!session.IsUpdating);
				Assert.That(!session.IsUpdatingCancelled);
				Assert.That(!session.IsUpdatingCancelling);
				Assert.That(!session.IsUpdatingFailed);
				Assert.That(session.IsUpdatingSucceeded);

				// verify installed files
				Assert.That(expectedUpdatedFilePaths.SetEquals(this.CollectFilePaths(baseAppDirectory)));
			});
		}
	}
}
