using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
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
						isGnome = LinuxDistribution switch
						{
							LinuxDistribution.Fedora => true,
							LinuxDistribution.Ubuntu => true,
							_ => false,
						};
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
					if (IsWindows || IsMacOS)
						isOpeningFileManagerSupported = true;
					else if (IsLinux)
						isOpeningFileManagerSupported = IsGnome;
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
								if (data.Contains("(Fedora"))
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
			else
				return;
			try
			{
				process.Start();
			}
			catch
			{ }
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
	}
}
