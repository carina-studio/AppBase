using System;
using System.Diagnostics;
using System.IO;
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
		static bool? isOpeningFileManagerSupported;
		static bool? isOpeningLinkSupported;
		static LinuxDesktop? linuxDesktop;
		static LinuxDistribution? linuxDistribution;
		static Regex? runtimeVersionRegex;
		static WindowsVersion? windowsVersion;
		static readonly Lock syncLock = new();


		/// <summary>
		/// Get the latest version of .NET Runtime installed on device.
		/// </summary>
		/// <param name="stableVersionOnly">True to include stable version only.</param>
		/// <returns>Installed .NET Runtime version, or null if .NET Runtime is not installed on device.</returns>
		public static Version? GetInstalledRuntimeVersion(bool stableVersionOnly = true)
        {
			try
			{
				using var process = Process.Start(new ProcessStartInfo
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
					var targetRuntime = IsWindows ? "Microsoft.WindowsDesktop.App" : "Microsoft.NETCore.App";
					var line = process.StandardOutput.ReadLine();
					runtimeVersionRegex ??= new Regex("^(?<Runtime>[^\\s]+)[\\s]+(?<Version>[\\d]+(\\.[\\d]+)*)(?<VersionPostfix>\\-.+)?");
					while (line != null)
					{
						try
						{
							var match = runtimeVersionRegex.Match(line);
							if (match.Success)
							{
								if (match.Groups["Runtime"].Value != targetRuntime)
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
			// ReSharper disable EmptyGeneralCatchClause
			catch
			{ }
			// ReSharper restore EmptyGeneralCatchClause
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
		/// Check whether current operating system is Android of not.
		/// </summary>
		public static bool IsAndroid { get; } = OperatingSystem.IsAndroid();


		/// <summary>
		/// Check whether the desktop environment is GNOME or not if current operating system is Linux.
		/// </summary>
		[Obsolete("Use LinuxDesktop instead.")]
		public static bool IsGnome => LinuxDesktop == LinuxDesktop.Gnome;


		/// <summary>
		/// Check whether current operating system is iOS of not.
		/// </summary>
		public static bool IsIOS { get; } = OperatingSystem.IsIOS();


		/// <summary>
		/// Check whether current operating system is Linux of not.
		/// </summary>
		/// <remarks>Please noted that the value is also True on Android platform.</remarks>
		public static bool IsLinux { get; } = OperatingSystem.IsLinux() || IsAndroid;


		/// <summary>
		/// Check whether current operating system is macOS of not.
		/// </summary>
		public static bool IsMacOS { get; } = OperatingSystem.IsMacOS();
		
		
		/// <summary>
		/// Check whether current operating system is not Android of not.
		/// </summary>
		public static bool IsNotAndroid => !IsAndroid;
		
		
		/// <summary>
		/// Check whether current operating system is not iOS of not.
		/// </summary>
		public static bool IsNotIOS => !IsIOS;


		/// <summary>
		/// Check whether current operating system is not Linux of not.
		/// </summary>
		public static bool IsNotLinux => !IsLinux;


		/// <summary>
		/// Check whether current operating system is not macOS of not.
		/// </summary>
		public static bool IsNotMacOS => !IsMacOS;


		/// <summary>
		/// Check whether current operating system is not Windows of not.
		/// </summary>
		public static bool IsNotWindows => !IsWindows;


		/// <summary>
		/// Check whether opening system file manager is supported or not.
		/// </summary>
		public static bool IsOpeningFileManagerSupported
        {
			get
            {
				if (isOpeningFileManagerSupported.HasValue)
					return isOpeningFileManagerSupported.Value;
				lock (syncLock)
				{
					if (isOpeningFileManagerSupported.HasValue)
						return isOpeningFileManagerSupported.Value;
					if (IsWindows || IsMacOS || (IsLinux && IsNotAndroid))
						isOpeningFileManagerSupported = true;
					else
						isOpeningFileManagerSupported = false;
				}
				return isOpeningFileManagerSupported.Value;
			}
        }


		/// <summary>
		/// Check whether opening URI by default browser is supported on current platform or not.
		/// </summary>
		public static bool IsOpeningLinkSupported
		{
			get
			{
				if (isOpeningLinkSupported.HasValue)
					return isOpeningLinkSupported.Value;
				lock (syncLock)
					isOpeningLinkSupported = (IsWindows || IsMacOS || (IsLinux && IsNotAndroid));
				return isOpeningLinkSupported.Value;
			}
		}


		/// <summary>
		/// Check whether current operating system is Windows of not.
		/// </summary>
		public static bool IsWindows { get; } = OperatingSystem.IsWindows();


		/// <summary>
		/// Check whether the version of Windows is Windows 10+ or not.
		/// </summary>
		public static bool IsWindows10OrAbove => WindowsVersion >= WindowsVersion.Windows10;


		/// <summary>
		/// Check whether the version of Windows is Windows 11+ or not.
		/// </summary>
		public static bool IsWindows11OrAbove => WindowsVersion >= WindowsVersion.Windows11;


		/// <summary>
		/// Check whether the version of Windows is Windows 8+ or not.
		/// </summary>
		public static bool IsWindows8OrAbove => WindowsVersion >= WindowsVersion.Windows8;


		/// <summary>
		/// Get current desktop environment running on Linux.
		/// </summary>
		public static LinuxDesktop LinuxDesktop
		{
			get
			{
				if (linuxDesktop.HasValue)
					return linuxDesktop.Value;
				lock (syncLock)
				{
					if (linuxDesktop.HasValue)
						return linuxDesktop.Value;
					if (IsLinux)
					{
						var env = Environment.GetEnvironmentVariable("XDG_CURRENT_DESKTOP")?.ToLower() ?? "";
						if (env.Contains("gnome"))
							linuxDesktop = LinuxDesktop.Gnome;
						else if (env.Contains("kde"))
							linuxDesktop = LinuxDesktop.KDE;
						else if (env.Contains("unity"))
							linuxDesktop = LinuxDesktop.Unity;
						else
							linuxDesktop = LinuxDesktop.Unknown;
					}
					else
						linuxDesktop = LinuxDesktop.Unknown;
				}
				return linuxDesktop.Value;
			}
		}


		/// <summary>
		/// Get Linux distribution if current operating system is Linux.
		/// </summary>
		public static LinuxDistribution LinuxDistribution
        {
			get
            {
				if (linuxDistribution.HasValue)
					return linuxDistribution.Value;
				lock (syncLock)
				{
					if (linuxDistribution.HasValue)
						return linuxDistribution.Value;
					if (IsLinux && IsNotAndroid)
					{
						try
						{
							using var reader = new StreamReader("/proc/version", Encoding.UTF8);
							linuxDistribution = reader.ReadLine()?.Let(data =>
							{
								if (data.Contains("(Alpine"))
									return LinuxDistribution.Alpine;
								if (data.Contains("(Debian"))
									return LinuxDistribution.Debian;
								// ReSharper disable once StringLiteralTypo
								if (data.Contains("(Fedora") || data.Contains(".fedoraproject.org") || Regex.IsMatch(data, "\\.fc\\d+\\."))
									return LinuxDistribution.Fedora;
								if (data.Contains("(Red Hat") || data.Contains(".redhat.com"))
								{
									try
									{
										using var redHatReleaseReader = new StreamReader("/etc/redhat-release", Encoding.UTF8);
										var release = redHatReleaseReader.ReadLine();
										if (release is not null)
										{
											if (release.StartsWith("CentOS", StringComparison.InvariantCultureIgnoreCase))
												return LinuxDistribution.CentOS;
											if (release.StartsWith("Fedora", StringComparison.InvariantCultureIgnoreCase))
												return LinuxDistribution.Fedora;
										}
									}
									// ReSharper disable EmptyGeneralCatchClause
									catch
									{ }
									// ReSharper restore EmptyGeneralCatchClause
									return LinuxDistribution.RedHat;
								}
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
			// ReSharper disable EmptyGeneralCatchClause
			catch
			{ }
			// ReSharper restore EmptyGeneralCatchClause

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
			else if (LinuxDesktop == LinuxDesktop.Gnome || LinuxDesktop == LinuxDesktop.Unity)
			{
				process.StartInfo.Let(it =>
				{
					it.FileName = "nautilus";
					it.Arguments = $"--browser \"{path}\"";
				});
			}
			else if (IsLinux && IsNotAndroid)
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
				if (LinuxDesktop == LinuxDesktop.Gnome || LinuxDesktop == LinuxDesktop.Unity)
				{
					try
					{
						Process.Start(new ProcessStartInfo()
						{
							FileName = "xdg-open",
							Arguments = isDirectory ? path : (Path.GetDirectoryName(path) ?? ""),
						});
					}
					// ReSharper disable EmptyGeneralCatchClause
					catch
					{ }
					// ReSharper restore EmptyGeneralCatchClause
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
					Process.Start(new ProcessStartInfo("cmd", $"/c start {uri.AbsoluteUri}")
					{
						CreateNoWindow = true
					});
				}
				else if (IsLinux && IsNotAndroid)
					Process.Start("xdg-open", uri.AbsoluteUri);
				else if (IsMacOS)
					Process.Start("open", uri.AbsoluteUri);
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
				lock (syncLock)
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
