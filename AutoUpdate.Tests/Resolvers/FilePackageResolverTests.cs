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
                Assert.That(UpdaterComponentState.Succeeded == resolver.State);
                Assert.That(appName == resolver.ApplicationName);
                Assert.That("file" == resolver.PackageUri?.Scheme);
                Assert.That(packageFile == resolver.PackageUri?.Let(uri =>
                {
                    var localPath = uri.LocalPath;
                    return Path.DirectorySeparatorChar switch
                    {
                        '\\' => localPath.Replace('/', '\\'),
                        _ => localPath,
                    };
                }));
                Assert.That(packageVersion == resolver.PackageVersion);
            });
        }
    }
}
