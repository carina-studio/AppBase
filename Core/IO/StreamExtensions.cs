using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace CarinaStudio.IO;

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
	public static byte[] ReadAllBytes(this Stream stream) =>
		ReadAllBytes(stream, CancellationToken.None);
	
	
	// Read all remaining bytes from stream.
	static byte[] ReadAllBytes(Stream stream, CancellationToken cancellationToken)
	{
		// check size to read
		long size = -1;
		try
		{
			size = stream.Length - stream.Position;
		}
		catch
		{
			// ignored
		}
		if (size == 0)
			return [];
		
		// read with known size
		var checkCancellation = cancellationToken != CancellationToken.None;
		int bufferSize;
		byte[] buffer;
		int readCount;
		if (size > 0 && size <= int.MaxValue)
		{
			bufferSize = (int)size;
			buffer = GC.AllocateUninitializedArray<byte>(bufferSize);
			readCount = stream.Read(buffer, 0, bufferSize);
			if (readCount == 0)
				throw new EndOfStreamException();
			if (checkCancellation)
				cancellationToken.ThrowIfCancellationRequested();
			var offset = 0;
			while (true)
			{
				offset += readCount;
				if (offset >= bufferSize)
					break;
				readCount = stream.Read(buffer, offset, bufferSize - offset);
				if (readCount == 0)
					throw new EndOfStreamException();
				if (checkCancellation)
					cancellationToken.ThrowIfCancellationRequested();
			}
			return buffer;
		}
		
		// read with unknown size
		using var memoryStream = new MemoryStream();
		bufferSize = 4096;
		buffer = GC.AllocateUninitializedArray<byte>(bufferSize);
		readCount = stream.Read(buffer, 0, bufferSize);
		while (readCount > 0)
		{
			if (checkCancellation)
				cancellationToken.ThrowIfCancellationRequested();
			memoryStream.Write(buffer, 0, readCount);
			readCount = stream.Read(buffer, 0, bufferSize);
		}
		return memoryStream.ToArray();
	}


	/// <summary>
	/// Read all remaining bytes from stream asynchronously.
	/// </summary>
	/// <param name="stream"><see cref="Stream"/>.</param>
	/// <returns>Remaining bytes read from stream.</returns>
	public static Task<byte[]> ReadAllBytesAsync(this Stream stream) => 
		Task.Run(() => ReadAllBytes(stream, CancellationToken.None), CancellationToken.None);


	/// <summary>
	/// Read all remaining bytes from stream asynchronously.
	/// </summary>
	/// <param name="stream"><see cref="Stream"/>.</param>
	/// <param name="cancellationToken">Cancellation token.</param>
	/// <returns>Remaining bytes read from stream.</returns>
	public static Task<byte[]> ReadAllBytesAsync(this Stream stream, CancellationToken cancellationToken) => 
		Task.Run(() => ReadAllBytes(stream, cancellationToken), cancellationToken);
}