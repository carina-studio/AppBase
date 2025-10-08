﻿using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace CarinaStudio.IO;

/// <summary>
/// Implementation of <see cref="IStreamProvider"/> which keeps data in memory.
/// </summary>
public class MemoryStreamProvider : IStreamProvider
{
	// Stream.
	class StreamImpl(MemoryStreamProvider provider, MemoryStream stream) : StreamWrapper(stream)
	{
		protected override void Dispose(bool disposing)
		{
			if (stream.CanWrite && provider.isWritable && !provider.isFixedSize)
			{
				using var _ = provider.syncLock.EnterScope();
				provider.data = stream.ToArray();
				provider.dataOffset = 0;
				provider.dataCount = provider.data.Length;
			}
			base.Dispose(disposing);
		}
	}


	// Fields.
	volatile byte[] data;
	volatile int dataCount;
	volatile int dataOffset;
	readonly bool isFixedSize;
	readonly bool isWritable;
    readonly Lock syncLock = new();


    /// <summary>
    /// Initialize new <see cref="MemoryStreamProvider"/>.
    /// </summary>
    public MemoryStreamProvider()
    {
        this.data = [];
		this.isWritable = true;
	}


	/// <summary>
	/// Initialize new <see cref="MemoryStreamProvider"/> for reading.
	/// </summary>
	/// <param name="buffer">Data buffer.</param>
	public MemoryStreamProvider(byte[] buffer) : this(buffer, 0, buffer.Length, true)
	{ }


	/// <summary>
	/// Initialize new <see cref="MemoryStreamProvider"/> for reading.
	/// </summary>
	/// <param name="buffer">Data buffer.</param>
	/// <param name="isWritable">True if data can be written back to <paramref name="buffer"/>.</param>
	public MemoryStreamProvider(byte[] buffer, bool isWritable) : this(buffer, 0, buffer.Length, isWritable)
	{ }


	/// <summary>
	/// Initialize new <see cref="MemoryStreamProvider"/> for reading.
	/// </summary>
	/// <param name="buffer">Data buffer.</param>
	/// <param name="index">Index of first byte to be read or written in <paramref name="buffer"/>.</param>
	/// <param name="count">Number of bytes in <paramref name="buffer"/> to be read or written.</param>
	/// <param name="isWritable">True if data can be written back to <paramref name="buffer"/>.</param>
	public MemoryStreamProvider(byte[] buffer, int index, int count, bool isWritable)
	{
		if (buffer == null)
			throw new ArgumentNullException(nameof(buffer));
		if (index < 0 || index > buffer.Length)
			throw new ArgumentOutOfRangeException(nameof(index));
		if (count < 0 || index + count > buffer.Length)
			throw new ArgumentOutOfRangeException(nameof(count));
		this.data = buffer;
		this.dataCount = count;
		this.dataOffset = index;
		this.isFixedSize = true;
		this.isWritable = isWritable;
	}


	/// <summary>
	/// Check whether given access to <see cref="Stream"/> is supported by this provider or not.
	/// </summary>
	/// <param name="access">Access to stream.</param>
	/// <returns>True if given combination of access is supported.</returns>
	public bool CheckStreamAccess(StreamAccess access)
	{
		if ((access & StreamAccess.Write) != 0 && !this.isWritable)
			return false;
		return true;
	}


	/// <summary>
	/// Open stream asynchronously.
	/// </summary>
	/// <param name="access">Desired access to stream.</param>
	/// <param name="token">Cancellation token.</param>
	/// <returns>Task of opening stream.</returns>
	public Task<Stream> OpenStreamAsync(StreamAccess access, CancellationToken token)
	{
		// check state
		if (!this.CheckStreamAccess(access))
			throw new InvalidOperationException();

		// create memory stream
		var memoryStream = this.syncLock.Lock(() =>
			this.isFixedSize || this.data.Length > 0
				? new MemoryStream(this.data, this.dataOffset, this.dataCount, this.isWritable)
				: new MemoryStream());
		return Task.Run(() => (Stream)new StreamImpl(this, memoryStream), token);
	}


	/// <summary>
	/// Copy data as byte array.
	/// </summary>
	/// <returns>Byte array.</returns>
	public byte[] ToByteArray()
	{
		using var _ = this.syncLock.EnterScope();
		if (this.dataOffset != 0 || this.dataCount != this.data.Length)
			return new byte[this.dataCount].Also(it => Array.Copy(this.data, this.dataOffset, it, 0, this.dataCount));
		return (byte[])this.data.Clone();
	}
}