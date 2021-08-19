using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace CarinaStudio.IO
{
	/// <summary>
	/// Provide utility methods for files.
	/// </summary>
	public static class File
	{
		/// <summary>
		/// Try opening file with given timeout.
		/// </summary>
		/// <param name="fileName">File name.</param>
		/// <param name="mode">Mode to open file.</param>
		/// <param name="access">Desired access.</param>
		/// <param name="timeoutMillis">Timeout in milliseconds.</param>
		/// <param name="stream">Opened stream.</param>
		/// <returns>True if file opened successfully in given timeout.</returns>
		public static bool TryOpen(string fileName, FileMode mode, FileAccess access, int timeoutMillis, out FileStream? stream)
		{
			// open without retrying
			if (timeoutMillis == 0)
			{
				try
				{
					stream = new FileStream(fileName, mode, access);
					return true;
				}
				catch
				{
					stream = null;
					return false;
				}
			}

			// try opening
			var stopWatch = new Stopwatch().Also(it => it.Start());
			while (true)
			{
				try
				{
					stream = new FileStream(fileName, mode, access);
					return true;
				}
				catch
				{
					if (timeoutMillis < 0)
						Thread.Sleep(500);
					else
					{
						var elapsedTime = (int)stopWatch.ElapsedMilliseconds;
						var delayTime = Math.Min(200, timeoutMillis - elapsedTime);
						if (delayTime <= 0)
						{
							stream = null;
							return false;
						}
						Thread.Sleep(delayTime);
					}
					continue;
				}
			}
		}


		/// <summary>
		/// Try opening file for reading with given timeout.
		/// </summary>
		/// <param name="fileName">File name.</param>
		/// <param name="timeoutMillis">Timeout in milliseconds.</param>
		/// <param name="stream">Opened stream.</param>
		/// <returns>True if file opened successfully in given timeout.</returns>
		public static bool TryOpenRead(string fileName, int timeoutMillis, out FileStream? stream) => 
			TryOpen(fileName, FileMode.Open, FileAccess.Read, timeoutMillis, out stream);


		/// <summary>
		/// Try opening file for reading and writing with given timeout.
		/// </summary>
		/// <param name="fileName">File name.</param>
		/// <param name="timeoutMillis">Timeout in milliseconds.</param>
		/// <param name="stream">Opened stream.</param>
		/// <returns>True if file opened successfully in given timeout.</returns>
		public static bool TryOpenReadWrite(string fileName, int timeoutMillis, out FileStream? stream) => 
			TryOpen(fileName, FileMode.Create, FileAccess.ReadWrite, timeoutMillis, out stream);


		/// <summary>
		/// Try opening file for writing with given timeout.
		/// </summary>
		/// <param name="fileName">File name.</param>
		/// <param name="timeoutMillis">Timeout in milliseconds.</param>
		/// <param name="stream">Opened stream.</param>
		/// <returns>True if file opened successfully in given timeout.</returns>
		public static bool TryOpenWrite(string fileName, int timeoutMillis, out FileStream? stream) => 
			TryOpen(fileName, FileMode.Create, FileAccess.Write, timeoutMillis, out stream);
	}
}
