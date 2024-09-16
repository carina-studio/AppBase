using CarinaStudio.Threading;
using NUnit.Framework;
using System;
using System.IO;

namespace CarinaStudio
{
	/// <summary>
	/// Tests of <see cref="ApplicationExtensions"/>.
	/// </summary>
	[TestFixture]
	class ApplicationExtensionsTests
	{
		// Fields.
		TestApplication? application;
		SingleThreadSynchronizationContext? testSyncContext;


		// Complete tests.
		[OneTimeTearDown]
		public void Complete()
		{
			this.testSyncContext?.Dispose();
		}


		/// <summary>
		/// Test for getting strings.
		/// </summary>
		[Test]
		public void GettingStringsTest()
		{
			this.testSyncContext?.Send(() =>
			{
				// prepare
				var app = this.application.AsNonNull();

				// get non-null string
				var str = app.GetStringNonNull(TestApplication.FormatStringKey, "default");
				Assert.That(TestApplication.FormatString == str, "Invalid string get from application.");
				str = app.GetStringNonNull(TestApplication.InvalidStringKey, "default");
				Assert.That("default" == str, "Invalid default string get from application.");

				// get formatted string
				str = app.GetFormattedString(TestApplication.FormatStringKey, "Carina Studio");
				Assert.That(string.Format(TestApplication.FormatString, "Carina Studio") == str, "Invalid formatted string get from application.");
				try
				{
					app.GetFormattedString(TestApplication.InvalidStringKey, "Carina Studio");
					throw new AssertionException("Should not get formatted string with invalid key.");
				}
				catch(Exception ex)
				{
					if (ex is AssertionException)
						throw;
				}
			});
		}


		// Prepare for all tests.
		[OneTimeSetUp]
		public void Prepare()
		{
			this.testSyncContext = new SingleThreadSynchronizationContext();
			this.testSyncContext.Post(() =>
			{
				this.application = new TestApplication();
			});
		}


		/// <summary>
		/// Test for creating private directories.
		/// </summary>
		[Test]
		public void PrivateDirectoryCreationTest()
		{
			this.testSyncContext?.Send(() =>
			{
				// prepare
				var app = this.application.AsNonNull();
				var rootDirectoryPath = app.RootPrivateDirectoryPath;

				// create non-existing directory
				var dirPath1 = $"test1{Path.DirectorySeparatorChar}test1";
				var fullDirPath1 = Path.Combine(rootDirectoryPath, dirPath1);
				if (Directory.Exists(fullDirPath1))
					Directory.Delete(fullDirPath1, true);
				var dirInfo1 = app.CreatePrivateDirectory(dirPath1);
				Assert.That(dirInfo1.Exists, "Private directory didn't created.");
				Assert.That(fullDirPath1 == dirInfo1.FullName, "Private directory path is incorrect.");

				// create existing directory
				var dirInfo2 = app.CreatePrivateDirectory(dirPath1);
				Assert.That(dirInfo2.Exists, "Private directory didn't created.");
				Assert.That(fullDirPath1 == dirInfo2.FullName, "Private directory path is incorrect.");

				// create directory by absolute path
				var dirPath2 = $"test1{Path.DirectorySeparatorChar}test2";
				var fullDirPath2 = Path.Combine(rootDirectoryPath, dirPath2);
				try
				{
					app.CreatePrivateDirectory(fullDirPath2);
					throw new AssertionException("Should not support creating private directory by absolute path.");
				}
				catch (Exception ex)
				{
					if (ex is AssertionException)
						throw;
				}
			});
		}
	}
}
