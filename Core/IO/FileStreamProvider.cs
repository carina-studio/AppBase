using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace CarinaStudio.IO
{
	/// <summary>
	/// Implementation of <see cref="IStreamProvider"/> based-on file.
	/// </summary>
	public class FileStreamProvider : IStreamProvider
	{
		// Fields.
		readonly bool openAsAppend;


		/// <summary>
		/// Initialize new <see cref="FileStreamProvider"/> instance.
		/// </summary>
		/// <param name="fileName">File name.</param>
		/// <param name="append">True to open file in <see cref="FileMode.Append"/> mode.</param>
		public FileStreamProvider(string fileName, bool append = false)
		{
			this.FileName = fileName;
			this.openAsAppend = append;
		}


		/// <summary>
		/// Check whether given access to <see cref="Stream"/> is supported by this provider or not.
		/// </summary>
		/// <param name="access">Access to stream.</param>
		/// <returns>True if given combination of access is supported.</returns>
		public bool CheckStreamAccess(StreamAccess access) => true;


		/// <summary>
		/// Get file name.
		/// </summary>
		public string FileName { get; }


		/// <summary>
		/// Open stream asynchronously.
		/// </summary>
		/// <param name="access">Desired access to stream.</param>
		/// <param name="token">Cancellation token.</param>
		/// <returns>Task of opening stream.</returns>
		public Task<Stream> OpenStreamAsync(StreamAccess access, CancellationToken token) => Task.Run(() =>
		{
			var isReadNeeded = (access & StreamAccess.Read) != 0;
			var isWriteNeeded = (access & StreamAccess.Write) != 0;
			if (!isReadNeeded && !isWriteNeeded)
				throw new ArgumentException("Invalid access to stream.");
			var fileMode = this.openAsAppend
				? FileMode.Append
				: isWriteNeeded
					? FileMode.Create
					: FileMode.Open;
			var fileAccess = ((FileAccess)0).Let(it =>
			{
				if (isReadNeeded)
					it |= FileAccess.Read;
				if (isWriteNeeded)
					it |= FileAccess.Write;
				return it;
			});
			return (Stream)new FileStream(this.FileName, fileMode, fileAccess);
		});
	}
}
