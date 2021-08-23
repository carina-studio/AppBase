using System;
using System.IO;

namespace CarinaStudio.IO
{
	/// <summary>
	/// Access to <see cref="Stream"/>.
	/// </summary>
	[Flags]
	public enum StreamAccess
	{
		/// <summary>
		/// Read.
		/// </summary>
		Read = 0x1,
		/// <summary>
		/// Write.
		/// </summary>
		Write = 0x2,
		/// <summary>
		/// Read and write.
		/// </summary>
		ReadWrite = Read | Write,
	}
}
