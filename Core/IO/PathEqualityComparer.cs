using System;
using System.Collections.Generic;

namespace CarinaStudio.IO;

/// <summary>
/// <see cref="IEqualityComparer{T}"/> to check equality of file path. Case will be ignored on Windows.
/// </summary>
public class PathEqualityComparer : IEqualityComparer<string>
{
	/// <summary>
	/// Default instance.
	/// </summary>
	public static readonly PathEqualityComparer Default = new();


	// Static fields.
	static readonly bool ignoreCases = Platform.IsWindows;


	// Constructor.
	PathEqualityComparer()
	{ }


	/// <summary>
	/// Check equality of file path.
	/// </summary>
	/// <param name="filePathX">First file path.</param>
	/// <param name="filePathY">Second file path.</param>
	/// <returns>True if two file paths are same.</returns>
	public bool Equals(string? filePathX, string? filePathY)
	{
		if (filePathX == null)
			return filePathY == null;
		if (filePathY == null)
			return false;
		if (ignoreCases)
			return filePathX.Equals(filePathY, StringComparison.InvariantCultureIgnoreCase);
		return filePathX.Equals(filePathY);
	}


	/// <summary>
	/// Calculate hash-code of file path.
	/// </summary>
	/// <param name="filePath">File path.</param>
	/// <returns>Hash-code of file path.</returns>
	public int GetHashCode(string filePath)
	{
		if (ignoreCases)
			return filePath.GetHashCode(StringComparison.InvariantCultureIgnoreCase);
		return filePath.GetHashCode();
	}
}