using NUnit.Framework;
using System;
using System.IO;

namespace CarinaStudio.AutoUpdate.Resolvers
{
    /// <summary>
    /// Tests of <see cref="FilePackageResolver"/>.
    /// </summary>
    [TestFixture]
    class FilePackageResolverTests : BaseTests
    {
        /// <summary>
        /// Test for resolving package.
        /// </summary>
        [Test]
        public void ResolvingTest()
        {
            this.TestOnApplicationThread(async () =>
            {
                // prepare
                var appName = "Test";
                var packageFile = Path.GetTempFileName();
                var packageVersion = new Version(1, 2, 3, 4);

                // resolve
                var resolver = new FilePackageResolver(this.Application, packageFile, appName, packageVersion);
                await resolver.StartAndWaitAsync();

                // check result
                Assert.AreEqual(UpdaterComponentState.Succeeded, resolver.State);
                Assert.AreEqual(appName, resolver.ApplicationName);
                Assert.AreEqual("file", resolver.PackageUri?.Scheme);
                Assert.AreEqual(packageFile, resolver.PackageUri?.Let(uri =>
                {
                    var localPath = uri.LocalPath;
                    return Path.DirectorySeparatorChar switch
                    {
                        '\\' => localPath.Replace('/', '\\'),
                        _ => localPath,
                    };
                }));
                Assert.AreEqual(packageVersion, resolver.PackageVersion);
            });
        }
    }
}
