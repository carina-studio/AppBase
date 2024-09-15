using NUnit.Framework;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CarinaStudio.IO
{
	/// <summary>
	/// Base implementation of test of <see cref="IStreamProvider"/>.
	/// </summary>
	public abstract class BaseStreamProviderTests
	{
		// Fields.
		readonly Random random = new();


		/// <summary>
		/// Create <see cref="IStreamProvider"/> instance for testing.
		/// </summary>
		/// <param name="data">Expected data to read from stream.</param>
		/// <returns><see cref="IStreamProvider"/>.</returns>
		protected abstract IStreamProvider CreateInstance(byte[] data);


		/// <summary>
		/// Generate data for testing.
		/// </summary>
		/// <returns>Data.</returns>
		protected byte[] GenerateRandomData() => new byte[1024].Also(it =>
		{
			for (var i = it.Length - 1; i >= 0; --i)
				it[i] = (byte)this.random.Next('0', '9' + 1);
		});


		/// <summary>
		/// Get data written to stream opened by given provider.
		/// </summary>
		/// <param name="provider">Provider.</param>
		/// <returns>Data written to stream</returns>
		protected abstract byte[] GetWrittenData(IStreamProvider provider);


		/// <summary>
		/// Test for accessing stream.
		/// </summary>
		[Test]
		public async Task StreamAccessingTest()
		{
			// prepare
			var data = this.GenerateRandomData();
			var provider = this.CreateInstance(data);

			// check reading
			var isReadable = provider.CanOpenRead();
			if (isReadable)
			{
				await using var stream = await provider.OpenStreamAsync(StreamAccess.Read);
				var dataFromStream = stream.ReadAllBytes();
				Assert.That(data.SequenceEqual(dataFromStream));
			}

			// check writing
			var isWritable = provider.CanOpenWrite();
			if (isWritable)
			{
				await using (var stream = await provider.OpenStreamAsync(StreamAccess.Write))
					stream.Write(data, 0, data.Length);
				try
				{
					var dataFromStream = this.GetWrittenData(provider);
					Assert.That(data.SequenceEqual(dataFromStream));
				}
				catch (NotSupportedException)
				{ }
			}
			else if (!isReadable)
				throw new AssertionException("Neither read nor write is supported by provider.");
		}


		/// <summary>
		/// Test for <see cref="System.IO.Stream.Length"/>.
		/// </summary>
		[Test]
		public async Task StreamLengthTest()
		{
			// prepare
			var data = this.GenerateRandomData();
			var provider = this.CreateInstance(data);

			// check length reported by stream
			await using var stream = await provider.OpenStreamAsync(StreamAccess.Read);
			var dataFromStream = stream.ReadAllBytes();
			Assert.That(data.SequenceEqual(dataFromStream));
			try
			{
				Assert.That(data.Length == stream.Length);
			}
			catch (NotSupportedException)
			{ }
		}
	}
}
