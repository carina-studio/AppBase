using NUnit.Framework;
using System;

namespace CarinaStudio
{
	/// <summary>
	/// Test of <see cref="MemoryExtensions"/>.
	/// </summary>
	[TestFixture]
	class MemoryExtensionsTest
	{
		/// <summary>
		/// Test for pinning memory.
		/// </summary>
		[Test]
		public unsafe void PinningTest()
		{
			// pin byte memory
			var byteMemory = new Memory<byte>(new byte[128]);
			var byteSpan = byteMemory.Span;
			var result = byteMemory.Pin((address) =>
			{
				var ptr = (byte*)address;
				for (var i = 0; i < byteMemory.Length; ++i)
					*(ptr++) = (byte)i;
				return true;
			});
			Assert.IsTrue(result, "Result of Pin() is incorrect.");
			for (var i = 0; i < byteSpan.Length; ++i)
				Assert.AreEqual(i, byteSpan[i], $"Value[{i}] updated to array is incorrect.");

			// pin int memory
			var intMemory = new ReadOnlyMemory<int>(new int[128]);
			var intSpan = intMemory.Span;
			result = intMemory.Pin((address) =>
			{
				var ptr = (int*)address;
				for (var i = 0; i < intMemory.Length; ++i)
					*(ptr++) = i * 2;
				return false;
			});
			Assert.IsFalse(result, "Result of Pin() is incorrect.");
			for (var i = 0; i < intSpan.Length; ++i)
				Assert.AreEqual(i * 2, intSpan[i], $"Value[{i}] updated to array is incorrect.");
		}
	}
}
