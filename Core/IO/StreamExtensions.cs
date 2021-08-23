using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace CarinaStudio.IO
{
	/// <summary>
	/// Extensions for <see cref="Stream"/>.
	/// </summary>
	public static class StreamExtensions
	{
		/// <summary>
		/// Read all remaining bytes from stream.
		/// </summary>
		/// <param name="stream"><see cref="Stream"/>.</param>
		/// <returns>Remaining bytes read from stream.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static byte[] ReadAllBytes(this Stream stream) => new MemoryStream().Use(it =>
		{
			stream.CopyTo(it);
			return it.ToArray();
		});


		/// <summary>
		/// Read all remaining bytes from stream asynchronously.
		/// </summary>
		/// <param name="stream"><see cref="Stream"/>.</param>
		/// <returns>Remaining bytes read from stream.</returns>
		public static Task<byte[]> ReadAllBytesAsync(this Stream stream) =>
			ReadAllBytesAsync(stream, new CancellationToken());


		/// <summary>
		/// Read all remaining bytes from stream asynchronously.
		/// </summary>
		/// <param name="stream"><see cref="Stream"/>.</param>
		/// <param name="cancellationToken">Cancellation token.</param>
		/// <returns>Remaining bytes read from stream.</returns>
		public static async Task<byte[]> ReadAllBytesAsync(this Stream stream, CancellationToken cancellationToken)
		{
			using var memoryStream = new MemoryStream();
			await stream.CopyToAsync(memoryStream, cancellationToken);
			return memoryStream.ToArray();
		}
	}
}
