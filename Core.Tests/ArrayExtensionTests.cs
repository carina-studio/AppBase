using NUnit.Framework;
using System;

namespace CarinaStudio
{
	/// <summary>
	/// Tests of <see cref="ArrayExtensions"/>.
	/// </summary>
	[TestFixture]
	class ArrayExtensionTests
	{
		/// <summary>
		/// Test for pinning array.
		/// </summary>
		[Test]
		public unsafe void PinningTest()
		{
			// pin byte array
			var byteArray = new byte[128];
			var result = byteArray.Pin((address) =>
			{
				var ptr = (byte*)address;
				for (var i = 0; i < byteArray.Length; ++i)
					*(ptr++) = (byte)i;
				return true;
			});
			Assert.IsTrue(result, "Result of Pin() is incorrect.");
			for (var i = 0; i < byteArray.Length; ++i)
				Assert.AreEqual(i, byteArray[i], $"Value[{i}] updated to array is incorrect.");

			// pin int array
			var intArray = new int[128];
			result = intArray.Pin((address) =>
			{
				var ptr = (int*)address;
				for (var i = 0; i < intArray.Length; ++i)
					*(ptr++) = (i * 2);
				return false;
			});
			Assert.IsFalse(result, "Result of Pin() is incorrect.");
			for (var i = 0; i < intArray.Length; ++i)
				Assert.AreEqual(i * 2, intArray[i], $"Value[{i}] updated to array is incorrect.");

			// pin object array
			var objectArray = new object?[1];
			try
			{
				objectArray.Pin((_) => { });
				throw new AssertionException("Should not get address of array with unsupported type of element.");
			}
			catch(Exception ex)
			{
				if (ex is AssertionException)
					throw;
			}
		}
	}
}
