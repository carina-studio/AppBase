using System;
using System.IO;

namespace CarinaStudio.IO
{
	/// <summary>
	/// Extensions for <see cref="FileInfo"/>.
	/// </summary>
	public static class FileInfoExtensions
	{
		/// <summary>
		/// Try opening file with given timeout.
		/// </summary>
		/// <param name="fileInfo"><see cref="FileInfo"/>.</param>
		/// <param name="mode">Mode to open file.</param>
		/// <param name="access">Desired access.</param>
		/// <param name="timeoutMillis">Timeout in milliseconds.</param>
		/// <param name="stream">Opened stream.</param>
		/// <returns>True if file opened successfully in given timeout.</returns>
		public static bool TryOpen(this FileInfo fileInfo, FileMode mode, FileAccess access, int timeoutMillis, out FileStream? stream) =>
			File.TryOpen(fileInfo.FullName, mode, access, timeoutMillis, out stream);


		/// <summary>
		/// Try opening file for reading with given timeout.
		/// </summary>
		/// <param name="fileInfo"><see cref="FileInfo"/>.</param>
		/// <param name="timeoutMillis">Timeout in milliseconds.</param>
		/// <param name="stream">Opened stream.</param>
		/// <returns>True if file opened successfully in given timeout.</returns>
		public static bool TryOpenRead(this FileInfo fileInfo, int timeoutMillis, out FileStream? stream) =>
			File.TryOpen(fileInfo.FullName, FileMode.Open, FileAccess.Read, timeoutMillis, out stream);


		/// <summary>
		/// Try opening file for reading and writing with given timeout.
		/// </summary>
		/// <param name="fileInfo"><see cref="FileInfo"/>.</param>
		/// <param name="timeoutMillis">Timeout in milliseconds.</param>
		/// <param name="stream">Opened stream.</param>
		/// <returns>True if file opened successfully in given timeout.</returns>
		public static bool TryOpenReadWrite(this FileInfo fileInfo, int timeoutMillis, out FileStream? stream) =>
			File.TryOpen(fileInfo.FullName, FileMode.Create, FileAccess.ReadWrite, timeoutMillis, out stream);


		/// <summary>
		/// Try opening file for writing with given timeout.
		/// </summary>
		/// <param name="fileInfo"><see cref="FileInfo"/>.</param>
		/// <param name="timeoutMillis">Timeout in milliseconds.</param>
		/// <param name="stream">Opened stream.</param>
		/// <returns>True if file opened successfully in given timeout.</returns>
		public static bool TryOpenWrite(this FileInfo fileInfo, int timeoutMillis, out FileStream? stream) =>
			File.TryOpen(fileInfo.FullName, FileMode.Create, FileAccess.Write, timeoutMillis, out stream);
	}
}
