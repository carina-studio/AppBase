using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace CarinaStudio.IO
{
	/// <summary>
	/// <see cref="IComparer{T}"/> to compare file paths. Case will be ignored on Windows.
	/// </summary>
	public class PathComparer : IComparer<string>
	{
		/// <summary>
		/// Default instance.
		/// </summary>
		public static readonly PathComparer Default = new PathComparer();


		// Static fields.
		static readonly bool ignoreCases = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);


		// Constructor.
		PathComparer()
		{ }


		/// <summary>
		/// Compare file paths.
		/// </summary>
		/// <param name="x">1st file path.</param>
		/// <param name="y">2nd file path.</param>
		/// <returns>Comparison result.</returns>
		public int Compare(string? x, string? y) => string.Compare(x, y, ignoreCases);
	}
}
