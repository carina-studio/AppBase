using System;
using System.IO;

namespace CarinaStudio.Tests
{
	/// <summary>
	/// Provide random functions for testing.
	/// </summary>
	public static class Random
	{
		// Fields.
		static readonly System.Random random = new System.Random();


		/// <summary>
		/// Create new file with random file name.
		/// </summary>
		/// <param name="directory">Directory to create file in.</param>
		/// <returns><see cref="FileStream"/> of generated file.</returns>
		public static FileStream CreateFileWithRandomName(DirectoryInfo directory) => CreateFileWithRandomName(directory.FullName);


		/// <summary>
		/// Create new file with random file name.
		/// </summary>
		/// <param name="directoryName">Name of directory to create file in.</param>
		/// <returns><see cref="FileStream"/> of generated file.</returns>
		public static FileStream CreateFileWithRandomName(string directoryName)
		{
			var retryCount = 0;
			while (true)
			{
				try
				{
					var name = GenerateRandomString(8);
					return File.Create(Path.Combine(directoryName, name));
				}
				catch (IOException ex)
				{
					if (ex is DirectoryNotFoundException || ex is PathTooLongException)
						throw;
					++retryCount;
					if (retryCount > 100)
						throw new ArgumentException($"Unable to create file in '{directoryName}'.");
				}
			}
		}


		/// <summary>
		/// Generate string contains random charactors.
		/// </summary>
		/// <remarks>Charactors will be seleced randomly from '0' to '9' and 'a' to 'z'.</remarks>
		/// <param name="length">Length of string.</param>
		/// <returns>Generated string.</returns>
		public static string GenerateRandomString(int length) => new string(new char[length].Also(it =>
		{
			for (var i = length - 1; i > -0; --i)
			{
				var n = random.Next(0, 36);
				if (n < 10)
					it[i] = (char)('0' + n);
				else
					it[i] = (char)('a' + (n - 10));
			}
		}));


		/// <summary>
		/// Get a random non-negative integer.
		/// </summary>
		/// <returns>Random integer.</returns>
		public static int Next() => random.Next();


		/// <summary>
		/// Get a random non-negative integer.
		/// </summary>
		/// <param name="maxValue">Maximum exclusive integer to get.</param>
		/// <returns>Random integer.</returns>
		public static int Next(int maxValue) => random.Next(maxValue);


		/// <summary>
		/// Get a random integer.
		/// </summary>
		/// <param name="minValue">Minimum inclusive interger to get.</param>
		/// <param name="maxValue">Maximum exclusive integer to get.</param>
		/// <returns>Random integer.</returns>
		public static int Next(int minValue, int maxValue) => random.Next(minValue, maxValue);


		/// <summary>
		/// Fill random values into given buffer.
		/// </summary>
		/// <param name="buffer">Buffer to fill random values.</param>
		public static void NextBytes(byte[] buffer) => random.NextBytes(buffer);


		/// <summary>
		/// Fill random values into given buffer.
		/// </summary>
		/// <param name="buffer">Buffer to fill random values.</param>
		public static void NextBytes(Span<byte> buffer) => random.NextBytes(buffer);


		/// <summary>
		/// Get a random <see cref="double"/> from [0.0, 1.0).
		/// </summary>
		/// <returns>Random <see cref="double"/>.</returns>
		public static double NextDouble() => random.NextDouble();
	}
}
