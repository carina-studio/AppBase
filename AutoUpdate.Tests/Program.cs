using CarinaStudio.Collections;
using CarinaStudio.Threading;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CarinaStudio.AutoUpdate
{
    public class Program
    {
        static void Main(string[] args)
        {
            if (args.IsEmpty())
                return;

            using var syncContext = new SingleThreadSynchronizationContext();
            using var condEvent = new ManualResetEventSlim(false);

            syncContext.Post(async () =>
            {
                try
                {
                    var app = new TestApplication();
                    using var packageResolver = new Resolvers.JsonPackageResolver(app, null)
                    {
                        Source = new Net.WebRequestStreamProvider(new(args[0])),
                    };

                    Console.WriteLine($"Resolving packaging info from {args[0]}...");
                    Console.WriteLine();

                    await packageResolver.StartAndWaitAsync();

                    Console.WriteLine($"Application Name: {packageResolver.ApplicationName}");
                    Console.WriteLine($"Package version: {packageResolver.PackageVersion}");
                    Console.WriteLine($"Package URI: {packageResolver.PackageUri}");
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"{ex.GetType().Name}: {ex.Message}");
                }
                finally
                {
                    condEvent.Set();
                }
            });

            condEvent.Wait();
        }
    }
}