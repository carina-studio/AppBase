using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace CarinaStudio
{
    /// <summary>
    /// Utility class to provide platform specific functions.
    /// </summary>
    public static class Platform
    {
		// Fields.
		static bool? isGnome;
		static bool? isOpeningFileManagerSupported;
		static LinuxDistribution? linuxDistribution;
		static readonly Regex runtimeVersionRegex = new Regex("^(?<Runtime>[^\\s]+)[\\s]+(?<Version>[\\d]+(\\.[\\d]+)*)(?<VersionPostfix>\\-.+)?");
		static WindowsVersion? windowsVersion;


		/// <summary>
		/// Get the latest version of .NET Runtime installed on device.
		/// </summary>
		/// <param name="stableVersionOnly">True to include stable version only.</param>
		/// <returns>Installed .NET Runtime version, or null if .NET Runtime is not installed on device.</returns>
		public static Version? GetInstalledRuntimeVersion(bool stableVersionOnly = true)
        {
			try
			{
				using var process = Process.Start(new ProcessStartInfo()
				{
					Arguments = "--list-runtimes",
					CreateNoWindow = true,
					FileName = "dotnet",
					RedirectStandardOutput = true,
					UseShellExecute = false,
				});
				if (process != null)
				{
					var latestVersion = (Version?)null;
					var targetRuntine = IsWindows ? "Microsoft.WindowsDesktop.App" : "Microsoft.NETCore.App";
					var line = process.StandardOutput.ReadLine();
					while (line != null)
					{
						try
						{
							var match = runtimeVersionRegex.Match(line);
							if (match.Success)
							{
								if (match.Groups["Runtime"].Value != targetRuntine)
									continue;
								if (match.Groups["VersionPostfix"].Success && stableVersionOnly)
									continue;
								if (Version.TryParse(match.Groups["Version"].Value, out var version))
                                {
									if (latestVersion == null || latestVersion < version)
										latestVersion = version;
                                }
							}
						}
						finally
						{
							line = process.StandardOutput.ReadLine();
						}
					}
					return latestVersion;
				}
			}
			catch
			{ }
			return null;
		}


		/// <summary>
		/// Get the latest version of .NET Runtime installed on device
		/// </summary>
		/// <param name="stableVersionOnly">True to include stable version only.</param>
		/// <returns>Task of getting version. The result will be null if .NET Runtime is not installed on device.</returns>
		public static Task<Version?> GetInstalledRuntimeVersionAsync(bool stableVersionOnly = true) =>
			GetInstalledRuntimeVersionAsync(stableVersionOnly, new CancellationToken());


		/// <summary>
		/// Get the latest version of .NET Runtime installed on device
		/// </summary>
		/// <param name="cancellationToken">Cancellation token.</param>
		/// <param name="stableVersionOnly">True to include stable version only.</param>
		/// <returns>Task of getting version. The result will be null if .NET Runtime is not installed on device.</returns>
		public static async Task<Version?> GetInstalledRuntimeVersionAsync(bool stableVersionOnly, CancellationToken cancellationToken) =>
			await Task.Run(() => GetInstalledRuntimeVersion(stableVersionOnly), cancellationToken);


		/// <summary>
		/// Check whether the desktop environment is GNOME or not if current operating system is Linux.
		/// </summary>
		public static bool IsGnome
        {
			get
            {
				if (isGnome.HasValue)
					return isGnome.Value;
				lock (typeof(Platform))
				{
					if (isGnome.HasValue)
						return isGnome.Value;
					if (IsLinux)
					{
						try
						{
							using var process = Process.Start(new ProcessStartInfo()
							{
								Arguments = "--version",
								CreateNoWindow = true,
								FileName = "gnome-shell",
								UseShellExecute = false,
							});
							isGnome = process != null;
						}
						catch
						{
							isGnome = false;
						}
					}
					else
						isGnome = false;
				}
				return isGnome.Value;
			}
        }


		/// <summary>
		/// Check whether current operating system is Linux of not.
		/// </summary>
		public static bool IsLinux { get; } = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);


		/// <summary>
		/// Check whether current operating system is macOS of not.
		/// </summary>
		public static bool IsMacOS { get; } = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);


		/// <summary>
		/// Check whether opening system file manager is supported or not.
		/// </summary>
		public static bool IsOpeningFileManagerSupported
        {
			get
            {
				if (isOpeningFileManagerSupported.HasValue)
					return isOpeningFileManagerSupported.Value;
				lock (typeof(Platform))
				{
					if (isOpeningFileManagerSupported.HasValue)
						return isOpeningFileManagerSupported.Value;
					if (IsWindows || IsMacOS || IsLinux)
						isOpeningFileManagerSupported = true;
					else
						isOpeningFileManagerSupported = false;
				}
				return isOpeningFileManagerSupported.Value;
			}
        }


		/// <summary>
		/// Check whether current operating system is Windows of not.
		/// </summary>
		public static bool IsWindows { get; } = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);


		/// <summary>
		/// Check whether the version of Windows is Windows 10+ or not.
		/// </summary>
		public static bool IsWindows10OrAbove { get => WindowsVersion >= WindowsVersion.Windows10; }


		/// <summary>
		/// Check whether the version of Windows is Windows 11+ or not.
		/// </summary>
		public static bool IsWindows11OrAbove { get => WindowsVersion >= WindowsVersion.Windows11; }


		/// <summary>
		/// Check whether the version of Windows is Windows 8+ or not.
		/// </summary>
		public static bool IsWindows8OrAbove { get => WindowsVersion >= WindowsVersion.Windows8; }


		/// <summary>
		/// Get Linux distribution if current operating system is Linux.
		/// </summary>
		public static LinuxDistribution LinuxDistribution
        {
			get
            {
				if (linuxDistribution.HasValue)
					return linuxDistribution.Value;
				lock (typeof(Platform))
				{
					if (linuxDistribution.HasValue)
						return linuxDistribution.Value;
					if (IsLinux)
					{
						try
						{
							using var reader = new StreamReader("/proc/version", Encoding.UTF8);
							linuxDistribution = reader.ReadLine()?.Let(data =>
							{
								if (data.Contains("(Debian"))
									return LinuxDistribution.Debian;
								if (data.Contains("(Fedora") || data.Contains("fedoraproject"))
									return LinuxDistribution.Fedora;
								if (data.Contains("(Ubuntu"))
									return LinuxDistribution.Ubuntu;
								return LinuxDistribution.Unknown;
							}) ?? LinuxDistribution.Unknown;
						}
						catch
						{
							linuxDistribution = LinuxDistribution.Unknown;
						}
					}
					else
						linuxDistribution = LinuxDistribution.Unknown;
				}
				return linuxDistribution.Value;
			}
        }


		/// <summary>
		/// Open system file manager and show given file or directory.
		/// </summary>
		/// <param name="path">Path of file or directory to show.</param>
		public static void OpenFileManager(string path) => Task.Run(() =>
		{
			// check whether path is directory or not
			var isDirectory = false;
			try
			{
				isDirectory = Directory.Exists(path);
			}
			catch
			{ }

			// open file manager
			using var process = new Process();
			if (IsWindows)
			{
				process.StartInfo.Let(it =>
				{
					it.FileName = "Explorer";
					it.Arguments = isDirectory
						? $"\"{path}\""
						: $"/select, \"{path}\"";
				});
			}
			else if (IsMacOS)
			{
				process.StartInfo.Let(it =>
				{
					it.FileName = "open";
					it.Arguments = isDirectory
						? $"-a finder \"{path}\""
						: $"-a finder -R \"{path}\"";
				});
			}
			else if (IsGnome)
			{
				process.StartInfo.Let(it =>
				{
					it.FileName = "nautilus";
					it.Arguments = $"--browser \"{path}\"";
				});
			}
			else if (IsLinux)
			{
				process.StartInfo.Let(it =>
				{
					it.FileName = "xdg-open";
					it.Arguments = isDirectory ? path : (Path.GetDirectoryName(path) ?? "");
				});
			}
			else
				return;
			try
			{
				process.Start();
			}
			catch
			{ 
				// Fallback to xdg-open on Gnome
				if (IsGnome)
				{
					try
					{
						Process.Start(new ProcessStartInfo()
						{
							FileName = "xdg-open",
							Arguments = isDirectory ? path : (Path.GetDirectoryName(path) ?? ""),
						});
					}
					catch
					{ }
				}
			}
		});


		/// <summary>
		/// Open given URI by default browser.
		/// </summary>
		/// <param name="uri">URI to open.</param>
		/// <returns>True if URI opened successfully.</returns>
		public static bool OpenLink(string uri) => OpenLink(new Uri(uri));


		/// <summary>
		/// Open given <see cref="Uri"/> by default browser.
		/// </summary>
		/// <param name="uri"><see cref="Uri"/> to open.</param>
		/// <returns>True if URI opened successfully.</returns>
		public static bool OpenLink(Uri uri)
		{
			try
			{
				if (IsWindows)
				{
					Process.Start(new ProcessStartInfo("cmd", $"/c start {uri}")
					{
						CreateNoWindow = true
					});
				}
				else if (IsLinux)
					Process.Start("xdg-open", uri.ToString());
				else if (IsMacOS)
					Process.Start("open", uri.ToString());
				return true;
			}
			catch
			{
				return false;
			}
		}


		/// <summary>
		/// Get version of Windows currently running on.
		/// </summary>
		public static WindowsVersion WindowsVersion
		{
			get
			{
				if (windowsVersion.HasValue)
					return windowsVersion.GetValueOrDefault();
				lock (typeof(Platform))
				{
					if (windowsVersion.HasValue)
						return windowsVersion.GetValueOrDefault();
					if (IsWindows)
					{
						windowsVersion = Environment.OSVersion.Version.Let(version =>
						{
							return version.Major switch
							{
								6 => version.Minor.Let(it =>
								{
									if (it >= 2)
										return WindowsVersion.Windows8;
									if (it == 1)
										return WindowsVersion.Windows7;
									return WindowsVersion.Unknown;
								}),
								10 => version.Build.Let(it =>
								{
									if (version.Minor > 0)
										return WindowsVersion.Above;
									if (it < 22000)
										return WindowsVersion.Windows10;
									return WindowsVersion.Windows11;
								}),
								_ => version.Major > 10 ? WindowsVersion.Above : WindowsVersion.Unknown,
							};
						});
					}
					else
						windowsVersion = WindowsVersion.Unknown;
				}
				return windowsVersion.GetValueOrDefault();
			}
		}
	}
}
