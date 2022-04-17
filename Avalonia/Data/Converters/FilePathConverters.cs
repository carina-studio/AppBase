using Avalonia.Data.Converters;
using System;
using System.IO;

namespace CarinaStudio.Data.Converters
{
    /// <summary>
	/// Predefined <see cref="IValueConverter"/>s to convert from file path to specific string.
	/// </summary>
    public static class FilePathConverters
    {
        /// <summary>
        /// <see cref="IValueConverter"/> to extract path of parent directory from file path.
        /// </summary>
        public static readonly IValueConverter DirectoryName = new FuncValueConverter<string, string?>(Path.GetDirectoryName);


        /// <summary>
        /// <see cref="IValueConverter"/> to extract extension from file path.
        /// </summary>
        public static readonly IValueConverter Extension = new FuncValueConverter<string, string?>(Path.GetExtension);


        /// <summary>
        /// <see cref="IValueConverter"/> to extract file name from file path.
        /// </summary>
        public static readonly IValueConverter FileName = new FuncValueConverter<string, string?>(Path.GetFileName);


        /// <summary>
        /// <see cref="IValueConverter"/> to extract file name without extension from file path.
        /// </summary>
        public static readonly IValueConverter FileNameWithoutExtension = new FuncValueConverter<string, string?>(Path.GetFileNameWithoutExtension);
    }
}