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
		/// Test for operations on character sequence.
		/// </summary>
		[Test]
		public void CharacterSequenceTest()
		{
			var empty = "";
			var whiteSpaces = "  ";
			var nonWhiteSpace1 = "123 456";
			var nonWhiteSpace2 = " 123456";
			var nonWhiteSpace3 = "123456 ";
			var nonWhiteSpace4 = "123";
			Assert.IsTrue(empty.AsMemory().IsEmptyOrWhiteSpace());
			Assert.IsFalse(empty.AsMemory().IsNotWhiteSpace());
			Assert.IsTrue(whiteSpaces.AsMemory().IsEmptyOrWhiteSpace());
			Assert.IsFalse(whiteSpaces.AsMemory().IsNotWhiteSpace());
			Assert.IsFalse(nonWhiteSpace1.AsMemory().IsEmptyOrWhiteSpace());
			Assert.IsTrue(nonWhiteSpace1.AsMemory().IsNotWhiteSpace());
			Assert.IsFalse(nonWhiteSpace2.AsMemory().IsEmptyOrWhiteSpace());
			Assert.IsTrue(nonWhiteSpace2.AsMemory().IsNotWhiteSpace());
			Assert.IsFalse(nonWhiteSpace3.AsMemory().IsEmptyOrWhiteSpace());
			Assert.IsTrue(nonWhiteSpace3.AsMemory().IsNotWhiteSpace());
			Assert.IsFalse(nonWhiteSpace4.AsMemory().IsEmptyOrWhiteSpace());
			Assert.IsTrue(nonWhiteSpace4.AsMemory().IsNotWhiteSpace());
		}
		
		
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
