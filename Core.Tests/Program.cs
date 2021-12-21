using System;

namespace CarinaStudio
{
	/// <summary>
	/// Program.
	/// </summary>
	class Program
	{
		// Program entry.
		static void Main(string[] args)
		{
			var version = Platform.GetInstalledRuntimeVersion();
			var windowsVersion = Platform.WindowsVersion;

			/*
			var testFixure = new Collections.SortedObservableListTests();
			for (var b = 0x1000; b <= 1000000; b <<= 1)
			{
				for (var n = b; n <= 1000000; n <<= 1)
					testFixure.RandomNonOverlappedAddingPerformanceTest(n, b);
			}
			*/
		}
	}
}
