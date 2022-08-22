using CarinaStudio.Threading.Tasks;
using System;
using System.Threading;
using System.Threading.Tasks;

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
			var tf1 = new TaskFactory(new FixedThreadsTaskScheduler(1));
			var tf2 = new TaskFactory(new FixedThreadsTaskScheduler(2));

			async Task<bool> CompileAsync()
			{
				return await tf1.StartNew(() =>
				{
					Thread.Sleep(1000);
					return true;
				}).ConfigureAwait(false);
			}

			async Task<bool> RunAsync()
			{
				if (!(await CompileAsync()))
					return false;
				return await tf1.StartNew(() =>
				{
					return true;
				});
			}

			var t1 = tf1.StartNew(() =>
			{
				var t2 = RunAsync();

				while (!t2.IsCompleted)
				{
					if (t2.Wait(1000))
						break;
					Task.Yield();
				}

				return false;
			});
			t1.Wait();

			//var version = Platform.GetInstalledRuntimeVersion();
			//var windowsVersion = Platform.WindowsVersion;

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
