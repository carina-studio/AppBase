using System;
using System.IO;

namespace CarinaStudio.IO;

/// <summary>
/// <see cref="Stream"/> which wraps another <see cref="Stream"/>.
/// </summary>
/// <param name="stream"><see cref="Stream"/> to be wrapped.</param>
/// <param name="disposeStream">True to dispose <paramref name="stream"/> when disposing <see cref="StreamWrapper"/> instance.</param>
public abstract class StreamWrapper(Stream stream, bool disposeStream = true) : Stream
{
	/// <summary>
	/// Dispose instance.
	/// </summary>
	/// <param name="disposing">True to dispose managed resources.</param>
	protected override void Dispose(bool disposing)
	{
		if (disposeStream)
			stream.Dispose();
		base.Dispose(disposing);
	}


	/// <summary>
	/// Get <see cref="Stream"/> which is wrapped by this instance.
	/// </summary>
	protected Stream WrappedStream => stream;


#pragma warning disable CS1591
	// Implementations.
	public override bool CanRead => stream.CanRead;
	public override bool CanSeek => stream.CanSeek;
	public override bool CanTimeout => stream.CanTimeout;
	public override bool CanWrite => stream.CanWrite;
	public override void Flush() => stream.Flush();
	public override long Length => stream.Length;
	public override long Position
	{
		get => stream.Position;
		set => stream.Position = value;
	}
	public override int Read(byte[] buffer, int offset, int count) => stream.Read(buffer, offset, count);
	public override int Read(Span<byte> buffer) => stream.Read(buffer);
	public override int ReadByte() => stream.ReadByte();
	public override int ReadTimeout 
	{
		get => stream.ReadTimeout; 
		set => stream.ReadTimeout = value; 
	}
	public override long Seek(long offset, SeekOrigin origin) => stream.Seek(offset, origin);
	public override void SetLength(long value) => stream.SetLength(value);
	public override void Write(byte[] buffer, int offset, int count) => stream.Write(buffer, offset, count);
	public override void Write(ReadOnlySpan<byte> buffer) => stream.Write(buffer);
	public override int WriteTimeout
	{
		get => stream.WriteTimeout;
		set => stream.WriteTimeout = value; 
	}
#pragma warning disable CS1591
}