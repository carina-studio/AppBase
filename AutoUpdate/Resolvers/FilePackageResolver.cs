using CarinaStudio.IO;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace CarinaStudio.AutoUpdate.Resolvers;

/// <summary>
/// <see cref="IPackageResolver"/> which takes given file as update package directly.
/// </summary>
public class FilePackageResolver : BasePackageResolver
{
    // Fields.
    readonly string? appName;
    readonly string packageFilePath;
    readonly Version? packageVersion;


    /// <summary>
    /// Initialize new <see cref="FilePackageResolver"/> instance.
    /// </summary>
    /// <param name="app">Application.</param>
    /// <param name="packageFilePath">Path of package file.</param>
    /// <param name="appName">Application name.</param>
    /// <param name="packageVersion">Version of package.</param>
    public FilePackageResolver(IApplication app, string packageFilePath, string? appName = null, Version? packageVersion = null) : base(app)
    {
        this.appName = appName;
        this.packageFilePath = packageFilePath;
        this.packageVersion = packageVersion;
        base.Source = new MemoryStreamProvider([], false);
    }


    /// <summary>
    /// Perform operation asynchronously.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task of performing operation.</returns>
    protected override Task PerformOperationAsync(CancellationToken cancellationToken)
    {
        this.Logger.LogTrace($"Use '{this.packageFilePath}'");
        this.ApplicationName = appName;
        this.PackageUri = Path.DirectorySeparatorChar switch
        {
            '\\' => new Uri($"file:///{this.packageFilePath.Replace('\\', '/')}"),
            _ => new Uri($"file://{this.packageFilePath}"),
        };
        this.PackageVersion = this.packageVersion;
        return Task.CompletedTask;
    }


    /// <summary>
	/// Get or set source <see cref="IStreamProvider"/> to provide data of package manifest to be resolved.
	/// </summary>
    /// <remarks>It is unsupported to set this property on <see cref="FilePackageResolver"/>.</remarks>
    public override IStreamProvider? Source
    { 
        get => base.Source;
        set => throw new InvalidOperationException();
    }
}