using CarinaStudio.AutoUpdate.Resolvers;
using CarinaStudio.AutoUpdate.ViewModels;
using CarinaStudio.IO;
using System;
using System.Diagnostics;

namespace CarinaStudio
{
    class TestUpdatingSession : UpdatingSession
    {
        public TestUpdatingSession(IApplication app) : base(app)
        { }


        protected override IPackageResolver CreatePackageResolver(IStreamProvider source) =>
            new JsonPackageResolver() { Source = source };


        protected override void OnUpdaterProgressChanged()
        {
            base.OnUpdaterProgressChanged();
            Debug.WriteLine($"Updater progress: {this.UpdaterProgress * 100:F2}%");
        }
    }
}