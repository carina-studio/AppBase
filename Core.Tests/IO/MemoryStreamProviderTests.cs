using NUnit.Framework;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CarinaStudio.IO
{
	/// <summary>
	/// Tests of <see cref="MemoryStreamProvider"/>.
	/// </summary>
	[TestFixture]
	class MemoryStreamProviderTests : BaseStreamProviderTests
	{
		// Create instance.
		protected override IStreamProvider CreateInstance(byte[] data) => new MemoryStreamProvider(data);


		// Get written data.
		protected override byte[] GetWrittenData(IStreamProvider provider) => ((MemoryStreamProvider)provider).ToByteArray();


		/// <summary>
		/// Test for reading from partial buffer.
		/// </summary>
		/// <returns></returns>
		[Test]
		public async Task PartialBufferReadingTest()
		{
			// prepare
			var buffer = this.GenerateRandomData();
			var firstBuffer = new byte[buffer.Length / 2].Also(it => Array.Copy(buffer, 0, it, 0, it.Length));
			var secondBuffer = new byte[buffer.Length - firstBuffer.Length].Also(it => Array.Copy(buffer, firstBuffer.Length, it, 0, it.Length));
			var provider = new MemoryStreamProvider(buffer, firstBuffer.Length, secondBuffer.Length, true);

			// check read data
			using (var stream = await provider.OpenStreamAsync(StreamAccess.Read))
			{
				var data = stream.ReadAllBytes();
				Assert.IsTrue(data.SequenceEqual(secondBuffer));
			}
		}


		/// <summary>
		/// Test for writing data to the same buffer as reading.
		/// </summary>
		[Test]
		public async Task WritingInPlaceTest()
		{
			// prepare
			var data1 = this.GenerateRandomData();
			var data2 = this.GenerateRandomData();
			var dataBuffer = (byte[])data1.Clone();
			var provider = new MemoryStreamProvider(dataBuffer, true);
			Assert.IsFalse(data1.SequenceEqual(data2));

			// write data
			using (var stream = await provider.OpenStreamAsync(StreamAccess.Write))
				stream.Write(data2);
			Assert.IsTrue(dataBuffer.SequenceEqual(data2));
			using (var stream = await provider.OpenStreamAsync(StreamAccess.Write))
				stream.Write(data1);
			Assert.IsTrue(dataBuffer.SequenceEqual(data1));
		}
	}
}
