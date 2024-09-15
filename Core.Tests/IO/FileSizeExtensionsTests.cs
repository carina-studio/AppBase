using NUnit.Framework;

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
			Assert.That("0 B" == 0L.ToFileSizeString(0));
			Assert.That("0 B" == 0L.ToFileSizeString(1));
			Assert.That("1023 B" == 1023L.ToFileSizeString());
			Assert.That("1 KB" == (1L << 10).ToFileSizeString(0));
			Assert.That("1.0 KB" == (1L << 10).ToFileSizeString(1));
			Assert.That("1.000000 KB" == (1L << 10).ToFileSizeString(6));
			Assert.That("1.23 KB" == ((long)(1.23 * (1L << 10))).ToFileSizeString(2));
			Assert.That("1023 KB" == (1023 * (1L << 10)).ToFileSizeString(0));
			Assert.That("1.0000 MB" == (1L << 20).ToFileSizeString(4));
			Assert.That("1.000 GB" == (1L << 30).ToFileSizeString(3));
			Assert.That("1.00000 TB" == (1L << 40).ToFileSizeString(5));
			Assert.That("1.0000 PB" == (1L << 50).ToFileSizeString(4));
			Assert.That("3.14159 PB" == ((long)(3.14159 * (1L << 50))).ToFileSizeString(5));
			Assert.That("-12.345 MB" == ((long)(-12.345 * (1L << 20))).ToFileSizeString(3));
			Assert.That("-567.89 GB" == ((long)(-567.89 * (1L << 30))).ToFileSizeString(2));
		}
	}
}
