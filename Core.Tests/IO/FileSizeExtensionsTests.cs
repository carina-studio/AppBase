using NUnit.Framework;
using System;

namespace CarinaStudio.IO
{
	/// <summary>
	/// Tests of <see cref="FileSizeExtensions"/>.
	/// </summary>
	[TestFixture]
	class FileSizeExtensionsTests
	{
		/// <summary>
		/// Test for using <see cref="FileSizeExtensions.ToFileSizeString(long, int)"/>.
		/// </summary>
		[Test]
		public void FileSizeStringTest()
		{
			Assert.AreEqual("0 B", 0L.ToFileSizeString(0));
			Assert.AreEqual("0 B", 0L.ToFileSizeString(1));
			Assert.AreEqual("1023 B", 1023L.ToFileSizeString());
			Assert.AreEqual("1 KB", (1L << 10).ToFileSizeString(0));
			Assert.AreEqual("1.0 KB", (1L << 10).ToFileSizeString(1));
			Assert.AreEqual("1.000000 KB", (1L << 10).ToFileSizeString(6));
			Assert.AreEqual("1.23 KB", ((long)(1.23 * (1L << 10))).ToFileSizeString(2));
			Assert.AreEqual("1023 KB", (1023 * (1L << 10)).ToFileSizeString(0));
			Assert.AreEqual("1.0000 MB", (1L << 20).ToFileSizeString(4));
			Assert.AreEqual("1.000 GB", (1L << 30).ToFileSizeString(3));
			Assert.AreEqual("1.00000 TB", (1L << 40).ToFileSizeString(5));
			Assert.AreEqual("1.0000 PB", (1L << 50).ToFileSizeString(4));
			Assert.AreEqual("3.14159 PB", ((long)(3.14159 * (1L << 50))).ToFileSizeString(5));
			Assert.AreEqual("-12.345 MB", ((long)(-12.345 * (1L << 20))).ToFileSizeString(3));
			Assert.AreEqual("-567.89 GB", ((long)(-567.89 * (1L << 30))).ToFileSizeString(2));
		}
	}
}
