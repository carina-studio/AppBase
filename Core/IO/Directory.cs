using System.Threading;
using System.Threading.Tasks;

namespace CarinaStudio.IO
{
    /// <summary>
    /// Provide utility methods for directories.
    /// </summary>
    public static class Directory
    {
        /// <summary>
        /// Check whether the given directory exists or not asynchronously.
        /// </summary>
        /// <param name="path">Path to the directory.</param>
        /// <param name="cancellationToken">Cancellation.</param>
        /// <returns>Task of checking existence of directory.</returns>
        public static Task<bool> ExistsAsync(string? path, CancellationToken cancellationToken = default) => 
            Task.Run(() => System.IO.Directory.Exists(path), cancellationToken);
    }
}