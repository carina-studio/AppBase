using CarinaStudio.Configuration;
using CarinaStudio.Threading;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace CarinaStudio
{
	/// <summary>
	/// Application interface. This is the top-level object for application infrastructure.
	/// </summary>
	public interface IApplication : INotifyPropertyChanged, IThreadDependent
	{
		/// <summary>
		/// Get <see cref="Assembly"/> of application.
		/// </summary>
		Assembly Assembly { get; }


		/// <summary>
		/// Get <see cref="CultureInfo"/> currently used by application.
		/// </summary>
		CultureInfo CultureInfo { get; }


		/// <summary>
		/// Get string from resources according to given key and current settings or system language.
		/// </summary>
		/// <param name="key">Key of string to get.</param>
		/// <param name="defaultValue">Default string.</param>
		/// <returns>String.</returns>
		string? GetString(string key, string? defaultValue = null);


		/// <summary>
		/// Whether application shutdown has been started or not.
		/// </summary>
		bool IsShutdownStarted { get; }


		/// <summary>
		/// <see cref="ILoggerFactory"/> to create logger.
		/// </summary>
		ILoggerFactory LoggerFactory { get; }


		/// <summary>
		/// Get default <see cref="ISettings"/> to keep persistent state of application.
		/// </summary>
		ISettings PersistentState { get; }


		/// <summary>
		/// Path to root of private directory which is suitable to be accessed by this application.
		/// </summary>
		string RootPrivateDirectoryPath { get; }


		/// <summary>
		/// Get default application level user settings.
		/// </summary>
		ISettings Settings { get; }


		/// <summary>
		/// Raised when string resources updated.
		/// </summary>
		event EventHandler StringsUpdated;
	}


	/// <summary>
	/// Extensions of <see cref="IApplication"/>.
	/// </summary>
	public static class ApplicationExtensions
	{
		/// <summary>
		/// Create directory in private directory of application.
		/// </summary>
		/// <param name="app"><see cref="IApplication"/>.</param>
		/// <param name="relativePath">Relative path of directory to create.</param>
		/// <returns><see cref="DirectoryInfo"/> represents the directory.</returns>
		/// <exception cref="ArgumentException">Given path is not a relative path.</exception>
		/// <exception cref="IOException">Unable to create directory.</exception>
		public static DirectoryInfo CreatePrivateDirectory(this IApplication app, string relativePath)
		{
			// check parameter
			if (Path.IsPathRooted(relativePath))
				throw new ArgumentException($"'{relativePath}' is not a relative path.");

			// create directory
			var fullPath = Path.Combine(app.RootPrivateDirectoryPath, relativePath);
			if (Directory.Exists(fullPath))
				return new DirectoryInfo(fullPath);
			return Directory.CreateDirectory(fullPath);
		}


		/// <summary>
		/// Get formatted string from resources according to given key and current settings or system language.
		/// </summary>
		/// <param name="app"><see cref="IApplication"/>.</param>
		/// <param name="key">Key of format string to get.</param>
		/// <param name="args">Arguments to be formatted.</param>
		/// <returns>Formatted string.</returns>
		/// <exception cref="ArgumentException">Format string is null.</exception>
		public static string GetFormattedString(this IApplication app, string key, params object?[] args) => app.GetString(key)?.Let((format) =>
		{
			return string.Format(format, args);
		}) ?? throw new ArgumentException("Format string is null.");


		/// <summary>
		/// Get non-null string from resources according to given key and current settings or system language.
		/// </summary>
		/// <param name="app"><see cref="IApplication"/>.</param>
		/// <param name="key">Key of string to get.</param>
		/// <param name="defaultValue">Default string.</param>
		/// <returns>String.</returns>
		public static string GetStringNonNull(this IApplication app, string key, string defaultValue = "") => app.GetString(key, defaultValue) ?? defaultValue;


		/// <summary>
		/// Open stream to read manifest resource.
		/// </summary>
		/// <param name="app"><see cref="IApplication"/>.</param>
		/// <param name="name">Name of resource.</param>
		/// <returns><see cref="Stream"/> of manifest resource.</returns>
		/// <exception cref="ArgumentException">Resource cannot be found.</exception>
		public static Stream OpenManifestResourceStream(this IApplication app, string name) => app.Assembly.GetManifestResourceStream(name) ?? throw new ArgumentException($"Cannot find manifest resource '{name}'.");


		/// <summary>
		/// Open stream to read manifest resource.
		/// </summary>
		/// <param name="app"><see cref="IApplication"/>.</param>
		/// <param name="type">Type to provide namespace to scope resource name.</param>
		/// <param name="name">Name of resource.</param>
		/// <returns><see cref="Stream"/> of manifest resource.</returns>
		/// <exception cref="ArgumentException">Resource cannot be found.</exception>
		public static Stream OpenManifestResourceStream(this IApplication app, Type type, string name) => app.Assembly.GetManifestResourceStream(type, name) ?? throw new ArgumentException($"Cannot find manifest resource '{name}'.");


		/// <summary>
		/// Try creating directory in private directory of application.
		/// </summary>
		/// <param name="app"><see cref="IApplication"/>.</param>
		/// <param name="relativePath">Relative path of directory to create.</param>
		/// <param name="directoryInfo"><see cref="DirectoryInfo"/> represents the directory.</param>
		/// <returns>True if directory has been created successfully.</returns>
		public static bool TryCreatePrivateDirectory(this IApplication app, string relativePath, out DirectoryInfo? directoryInfo)
		{
			try
			{
				directoryInfo = CreatePrivateDirectory(app, relativePath);
				return true;
			}
			catch
			{
				directoryInfo = null;
				return false;
			}
		}
	}
}
