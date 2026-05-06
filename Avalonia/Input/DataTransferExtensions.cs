using Avalonia.Input;
using Avalonia.Platform.Storage;
using CarinaStudio.Collections;
using System;

namespace CarinaStudio.Input;

/// <summary>
/// Extension methods for <see cref="IDataTransfer"/> and <see cref="DataTransfer"/>.
/// </summary>
public static class DataTransferExtensions
{
    /// <summary>
    /// Extensions for <see cref="IDataTransfer"/>.
    /// </summary>
    /// <param name="dataTransfer"><see cref="IDataTransfer"/>.</param>
    extension(IDataTransfer dataTransfer)
    {
        /// <summary>
        /// Check whether the <see cref="IDataTransfer"/> contains one or more file items or not.
        /// </summary>
        public bool HasFiles => dataTransfer.Contains(DataFormat.File);
        
        /// <summary>
        /// Try getting local path of files from <see cref="IDataTransfer"/>.
        /// </summary>
        /// <returns>Array of local path of files.</returns>
        public string[]? TryGetLocalFilePaths()
        {
            var files = dataTransfer.TryGetFiles();
            if (files.IsNullOrEmpty())
                return null;
            var fileCount = files.Length;
            var filePaths = GC.AllocateUninitializedArray<string>(files.Length);
            var filePathCount = 0;
            for (var i = 0; i < fileCount; ++i)
            {
                if (files[i].TryGetLocalPath() is { } filePath && filePath.Length > 0)
                    filePaths[filePathCount++] = filePath;
            }
            if (filePathCount == fileCount)
                return filePaths;
            if (filePathCount == 0)
                return null;
            var subFilePaths = GC.AllocateUninitializedArray<string>(filePathCount);
            Array.Copy(filePaths, 0, subFilePaths, 0, filePathCount);
            return subFilePaths;
        }
    }


    /// <summary>
    /// Extensions for <see cref="DataTransfer"/>.
    /// </summary>
    /// <param name="dataTransfer"><see cref="DataTransfer"/>.</param>
    extension(DataTransfer dataTransfer)
    {
        /// <summary>
        /// Add value to the <see cref="DataTransfer"/>.
        /// </summary>
        /// <param name="format">Format.</param>
        /// <param name="value">Value.</param>
        /// <typeparam name="T">Type of value.</typeparam>
        public void Add<T>(DataFormat<T> format, T? value) where T : class =>
            dataTransfer.Add(DataTransferItem.Create(format, value));
    }
}