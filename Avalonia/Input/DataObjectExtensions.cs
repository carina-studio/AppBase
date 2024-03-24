using Avalonia.Input;
using Avalonia.Platform.Storage;
using System;
using System.Diagnostics.CodeAnalysis;

namespace CarinaStudio.Input
{
	/// <summary>
	/// Extensions for <see cref="IDataObject"/>.
	/// </summary>
	public static class DataObjectExtensions
	{
		/// <summary>
		/// Clone <see cref="IDataObject"/> as <see cref="DataObject"/>.
		/// </summary>
		/// <param name="data"><see cref="IDataObject"/>.</param>
		/// <returns><see cref="DataObject"/>.</returns>
		[Obsolete("Use TryClone() instead.")]
		public static DataObject Clone(this IDataObject data) => new DataObject().Also(it =>
		{
			foreach (var format in data.GetDataFormats())
			{
				var value = data.Get(format);
				if (value != null)
					it.Set(format, value);
			}
		});


		/// <summary>
		/// Check whether at least one file name is contained in <see cref="IDataObject"/> or not.
		/// </summary>
		/// <param name="data"><see cref="IDataObject"/>.</param>
		/// <returns>True if at least one file name is contained in <see cref="IDataObject"/>.</returns>
		public static bool HasFileNames(this IDataObject data) => Global.RunOrDefault(() =>
		{
			try
			{
				return data.GetFiles()?.Let(it =>
				{
					foreach (var item in it)
					{
						if (!string.IsNullOrEmpty(item.TryGetLocalPath()))
							return true;
					}
					return false;
				}) ?? false;
			}
			catch
			{
				return false;
			}
		});


		/// <summary>
		/// Try cloning <see cref="IDataObject"/> as <see cref="DataObject"/>.
		/// </summary>
		/// <param name="data"><see cref="IDataObject"/> to clone.</param>
		/// <param name="clone">Cloned <see cref="IDataObject"/> as <see cref="DataObject"/>.</param>
		/// <returns>True if cloning successfully.</returns>
		public static bool TryClone(this IDataObject data, [NotNullWhen(true)] out DataObject? clone) =>
			TryClone(data, out clone, out _);


		/// <summary>
		/// Try cloning <see cref="IDataObject"/> as <see cref="DataObject"/>.
		/// </summary>
		/// <param name="data"><see cref="IDataObject"/> to clone.</param>
		/// <param name="clone">Cloned <see cref="IDataObject"/> as <see cref="DataObject"/>.</param>
		/// <param name="exception">Exception occurred while cloning.</param>
		/// <returns>True if cloning successfully.</returns>
		public static bool TryClone(this IDataObject data, [NotNullWhen(true)] out DataObject? clone, out Exception? exception)
		{
			try
			{
				exception = null;
				clone = new();
				foreach (var format in data.GetDataFormats())
				{
					var value = data.Get(format);
					if (value != null)
						clone.Set(format, value);
				}
				return true;
			}
			catch (Exception ex)
			{
				exception = ex;
				clone = null;
				return false;
			}
		}


		/// <summary>
		/// Try getting the data with given format and type.
		/// </summary>
		/// <typeparam name="T">Type of data.</typeparam>
		/// <param name="dataObject"><see cref="IDataObject"/>.</param>
		/// <param name="format">Format.</param>
		/// <param name="data">Data.</param>
		/// <returns>True if data got successfully.</returns>
		public static bool TryGetData<T>(this IDataObject dataObject, string format, [NotNullWhen(true)] out T? data) where T : class =>
			TryGetData(dataObject, format, out data, out _);
		
		
		/// <summary>
		/// Try getting the data with given format and type.
		/// </summary>
		/// <typeparam name="T">Type of data.</typeparam>
		/// <param name="dataObject"><see cref="IDataObject"/>.</param>
		/// <param name="format">Format.</param>
		/// <param name="data">Data.</param>
		/// <param name="exception">Exception occurred while getting data.</param>
		/// <returns>True if data got successfully.</returns>
		public static bool TryGetData<T>(this IDataObject dataObject, string format, [NotNullWhen(true)] out T? data, out Exception? exception) where T : class
		{
			exception = null;
			try
			{
				var rawData = dataObject.Get(format);
				if (rawData is T dataT)
				{
					data = dataT;
					return true;
				}
			}
			catch (Exception ex)
			{
				exception = ex;
			}
			data = default;
			return false;
		}


		/// <summary>
		/// Get the only file name contained in <see cref="IDataObject"/>.
		/// </summary>
		/// <param name="data"><see cref="IDataObject"/>.</param>
		/// <param name="fileName">File name contained in <see cref="IDataObject"/>.</param>
		/// <returns>True if only one file name contained in <see cref="IDataObject"/>, or false if no file name or more than one file names are contained.</returns>
		public static bool TryGetSingleFileName(this IDataObject data, [NotNullWhen(true)] out string? fileName) =>
			TryGetSingleFileName(data, out fileName, out _);


		/// <summary>
		/// Get the only file name contained in <see cref="IDataObject"/>.
		/// </summary>
		/// <param name="data"><see cref="IDataObject"/>.</param>
		/// <param name="fileName">File name contained in <see cref="IDataObject"/>.</param>
		/// <param name="exception">Exception occurred while getting data.</param>
		/// <returns>True if only one file name contained in <see cref="IDataObject"/>, or false if no file name or more than one file names are contained.</returns>
		public static bool TryGetSingleFileName(this IDataObject data, [NotNullWhen(true)] out string? fileName, out Exception? exception)
		{
			exception = null;
			try
			{
				fileName = data.GetFiles()?.Let(it =>
				{
					var fileName = default(string);
					foreach (var candidate in it)
					{
						var filePath = candidate.TryGetLocalPath();
						if (!string.IsNullOrEmpty(filePath))
						{
							if (fileName is null)
								fileName = filePath;
							else
								return null;
						}
					}
					return fileName;
				});
			}
			catch (Exception ex)
			{
				exception = ex;
				fileName = null;
			}
			return fileName is not null;
		}


		/// <summary>
		/// Try getting the value type data with given format and type.
		/// </summary>
		/// <typeparam name="T">Type of data.</typeparam>
		/// <param name="dataObject"><see cref="IDataObject"/>.</param>
		/// <param name="format">Format.</param>
		/// <param name="value">Value.</param>
		/// <returns>True if value got successfully.</returns>
		public static bool TryGetValue<T>(this IDataObject dataObject, string format, out T value) where T : struct =>
			TryGetValue(dataObject, format, out value, out _);


		/// <summary>
		/// Try getting the value type data with given format and type.
		/// </summary>
		/// <typeparam name="T">Type of data.</typeparam>
		/// <param name="dataObject"><see cref="IDataObject"/>.</param>
		/// <param name="format">Format.</param>
		/// <param name="value">Value.</param>
		/// <param name="exception">Exception occurred while getting data.</param>
		/// <returns>True if value got successfully.</returns>
		public static bool TryGetValue<T>(this IDataObject dataObject, string format, out T value, out Exception? exception) where T : struct
		{
			exception = null;
			try
			{
				var rawData = dataObject.Get(format);
				if (rawData is T targetValue)
				{
					value = targetValue;
					return true;
				}
			}
			catch (Exception ex)
			{
				exception = ex;
			}
			value = default;
			return false;
		}
	}
}
