using Avalonia.Input;
using System;

namespace CarinaStudio.Input
{
	/// <summary>
	/// Extensions for <see cref="IDataObject"/>.
	/// </summary>
	public static class DataObjectExtensions
	{
		/// <summary>
		/// Check whether at least one file name is contained in <see cref="IDataObject"/> or not.
		/// </summary>
		/// <param name="data"><see cref="IDataObject"/>.</param>
		/// <returns>True if at least one file name is contained in <see cref="IDataObject"/>.</returns>
		public static bool HasFileNames(this IDataObject data) => data.GetFileNames()?.Let(it =>
		{
			foreach (var _ in it)
				return true;
			return false;
		}) ?? false;


		/// <summary>
		/// Get the only file name contained in <see cref="IDataObject"/>.
		/// </summary>
		/// <param name="data"><see cref="IDataObject"/>.</param>
		/// <param name="fileName">File name contained in <see cref="IDataObject"/>.</param>
		/// <returns>True if only one file name contained in <see cref="IDataObject"/>, or false if no file name or more than one file names are contained.</returns>
		public static bool TryGetSingleFileName(this IDataObject data, out string? fileName)
		{
			fileName = data.GetFileNames()?.Let(it =>
			{
				var fileName = (string?)null;
				foreach (var candidate in it)
				{
					if (candidate != null)
					{
						if (fileName == null)
							fileName = candidate;
						else
							return null;
					}
				}
				return fileName;
			});
			return (fileName != null);
		}
	}
}
