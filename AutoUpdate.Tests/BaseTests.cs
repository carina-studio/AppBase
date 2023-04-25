using CarinaStudio.Collections;
using CarinaStudio.IO;
using CarinaStudio.Threading;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CarinaStudio.AutoUpdate
{
	/// <summary>
	/// Base implementations of tests of <see cref="Updater"/> and <see cref="IUpdaterComponent"/>.
	/// </summary>
	abstract class BaseTests
	{
		// Fields.
		IApplication? application;
		SingleThreadSynchronizationContext? applicationSyncContext;
		string? tempDirectory;


		/// <summary>
		/// Get <see cref="IApplication"/> instance for testing.
		/// </summary>
		protected IApplication Application => this.application.AsNonNull();


		/// <summary>
		/// Clear root temporary directory for testing.
		/// </summary>
		[OneTimeTearDown]
		public void ClearRootTempDirectory()
		{
			if (!string.IsNullOrEmpty(this.tempDirectory))
				System.IO.Directory.Delete(this.tempDirectory, true);
		}


		/// <summary>
		/// Collect all files in given directory recursively.
		/// </summary>
		/// <param name="directory">Directory.</param>
		/// <returns>Collected file paths.</returns>
		protected ISet<string> CollectFilePaths(string directory)
		{
			var filePaths = new HashSet<string>(PathEqualityComparer.Default);
			this.CollectFilePaths(directory, filePaths);
			return filePaths;
		}


		/// <summary>
		/// Collect all files in given directory recursively.
		/// </summary>
		/// <param name="directory">Directory.</param>
		/// <param name="filePaths"><see cref="ISet{T}"/> to collect file paths.</param>
		protected void CollectFilePaths(string directory, ISet<string> filePaths)
		{
			filePaths.AddAll(System.IO.Directory.EnumerateFiles(directory));
			foreach (var subDirectory in System.IO.Directory.EnumerateDirectories(directory))
				this.CollectFilePaths(subDirectory, filePaths);
		}


		/// <summary>
		/// Create empty temporary directory.
		/// </summary>
		/// <returns>Path of created directory.</returns>
		protected string CreateTempDirectory()
		{
			var name = Tests.Random.GenerateRandomString(8);
			var path = Path.Combine(this.RootTempDirectoryPath, name);
			while (true)
			{
				if (!System.IO.Directory.Exists(path))
				{
					System.IO.Directory.CreateDirectory(path);
					return path;
				}
				name = Tests.Random.GenerateRandomString(8);
				path = Path.Combine(this.RootTempDirectoryPath, name);
			}
		}


		/// <summary>
		/// Generate random application and its files.
		/// </summary>
		/// <returns>Directory of generated application.</returns>
		protected string GenerateRandomApplication()
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
			var subDirectoryInfo = System.IO.Directory.CreateDirectory(Path.Combine(rootDirectory, "SubDirectory_1"));
			for (var i = Tests.Random.Next(1, 17); i > 0; --i)
			{
				using var stream = Tests.Random.CreateFileWithRandomName(subDirectoryInfo);
				var data = new byte[Tests.Random.Next(100, 1025)];
				Tests.Random.NextBytes(data);
				stream.Write(data);
			}
			subDirectoryInfo = System.IO.Directory.CreateDirectory(Path.Combine(rootDirectory, "SubDirectory_2"));
			for (var i = Tests.Random.Next(1, 17); i > 0; --i)
			{
				using var stream = Tests.Random.CreateFileWithRandomName(subDirectoryInfo);
				var data = new byte[Tests.Random.Next(100, 1025)];
				Tests.Random.NextBytes(data);
				stream.Write(data);
			}

			// create third level files
			subDirectoryInfo = System.IO.Directory.CreateDirectory(Path.Combine(subDirectoryInfo.FullName, "SubDirectory_3"));
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
		/// Release <see cref="IApplication"/> for testing.
		/// </summary>
		[OneTimeTearDown]
		public void ReleaseApplication()
		{
			this.applicationSyncContext?.Dispose();
		}


		/// <summary>
		/// Get root temporary directory.
		/// </summary>
		protected string RootTempDirectoryPath => this.tempDirectory ?? throw new InvalidOperationException();


		/// <summary>
		/// Setup <see cref="IApplication"/> instance for testing.
		/// </summary>
		[OneTimeSetUp]
		public void SetupApplication()
		{
			this.applicationSyncContext = new SingleThreadSynchronizationContext().Also(it =>
			{
				it.Send(() =>
				{
					this.application = new TestApplication();
				});
			});
		}


		/// <summary>
		/// Setup root temporary directory for testing.
		/// </summary>
		[OneTimeSetUp]
		public void SetupRootTempDirectory()
		{
			var path = Path.Combine(Path.GetTempPath(), $"{this.GetType().Name}-{DateTime.Now.ToBinary()}");
			System.IO.Directory.CreateDirectory(path);
			this.tempDirectory = path;
		}


		/// <summary>
		/// Run testing on thread of application.
		/// </summary>
		/// <param name="action">Asynchronous test action.</param>
		protected void TestOnApplicationThread(Action action)
		{
			try
			{
				this.applicationSyncContext.AsNonNull().Send(action);
			}
			catch (Exception ex)
			{
				Console.Error.WriteLine($"[{this.GetType().Name}]");
				Console.Error.WriteLine(ex.Message);
				Console.Error.WriteLine(ex.StackTrace);
				throw new AssertionException("Error occured while testing.", ex);
			}
		}


		/// <summary>
		/// Run testing on thread of application.
		/// </summary>
		/// <param name="action">Asynchronous test action.</param>
		protected void TestOnApplicationThread(Func<Task> action)
		{
			var syncLock = new object();
			var exception = (Exception?)null;
			lock (syncLock)
			{
				this.applicationSyncContext.AsNonNull().Post(async () =>
				{
					try
					{
						await action();
					}
					catch (Exception ex)
					{
						exception = ex;
					}
					finally
					{
						lock (syncLock)
						{
							Monitor.Pulse(syncLock);
						}
					}
				});
				Monitor.Wait(syncLock);
				if (exception != null)
				{
					Console.Error.WriteLine($"[{this.GetType().Name}]");
					Console.Error.WriteLine(exception.Message);
					Console.Error.WriteLine(exception.StackTrace);
					throw new AssertionException("Error occured while testing.", exception);
				}
			}
		}


		/// <summary>
		/// Verify all files and directories in given directory.
		/// </summary>
		/// <param name="refDirectory">Reference directory.</param>
		/// <param name="directory">Directory to be verified.</param>
		protected void VerifyFilesAndDirectories(string refDirectory, string directory)
		{
			// verify files
			var refFileNames = System.IO.Directory.GetFiles(refDirectory).Also(it => Array.Sort(it, string.Compare));
			var fileNames = System.IO.Directory.GetFiles(directory).Also(it => Array.Sort(it, string.Compare));
			Assert.AreEqual(refFileNames.Length, fileNames.Length);
			for (var i = refFileNames.Length - 1; i >= 0; --i)
			{
				var refFileName = refFileNames[i];
				var fileName = fileNames[i];
				Assert.AreEqual(Path.GetFileName(refFileName), Path.GetFileName(fileName));
				var srcData = new FileStream(refFileName, FileMode.Open, FileAccess.Read).Use(it => it.ReadAllBytes());
				var targetData = new FileStream(fileName, FileMode.Open, FileAccess.Read).Use(it => it.ReadAllBytes());
				Assert.IsTrue(srcData.SequenceEqual(targetData));
			}

			// verify sub directories
			var refSubDirectories = System.IO.Directory.GetDirectories(refDirectory).Also(it => Array.Sort(it, string.Compare));
			var subDirectories = System.IO.Directory.GetDirectories(directory).Also(it => Array.Sort(it, string.Compare));
			Assert.AreEqual(refSubDirectories.Length, subDirectories.Length);
			for (var i = refSubDirectories.Length - 1; i >= 0; --i)
			{
				var refSubDirectory = refSubDirectories[i];
				var subDirectory = subDirectories[i];
				Assert.AreEqual(Path.GetFileName(refSubDirectory), Path.GetFileName(subDirectory));
				this.VerifyFilesAndDirectories(refSubDirectory, subDirectory);
			}
		}
	}
}
