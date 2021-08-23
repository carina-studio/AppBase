using System;
using System.IO;

namespace CarinaStudio.IO
{
	/// <summary>
	/// <see cref="Stream"/> which wraps another <see cref="Stream"/>.
	/// </summary>
	public abstract class StreamWrapper : Stream
	{
		// Fields.
		readonly bool disposeStream;
		readonly Stream stream;


		/// <summary>
		/// Initialize new <see cref="StreamWrapper"/> instance.
		/// </summary>
		/// <param name="stream"><see cref="Stream"/> to be wrapped.</param>
		/// <param name="disposeStream">True to dispose <paramref name="stream"/> when disposing <see cref="StreamWrapper"/> instance.</param>
		public StreamWrapper(Stream stream, bool disposeStream = true)
		{
			this.disposeStream = disposeStream;
			this.stream = stream;
		}


		/// <summary>
		/// Dispose instance.
		/// </summary>
		/// <param name="disposing">True to dispose managed resources.</param>
		protected override void Dispose(bool disposing)
		{
			if (this.disposeStream)
				this.stream.Dispose();
			base.Dispose(disposing);
		}


		/// <summary>
		/// Get <see cref="Stream"/> which is wrapped by this instance.
		/// </summary>
		protected Stream WrappedStream { get => this.stream; }


#pragma warning disable CS1591
		// Implementations.
		public override bool CanRead => this.stream.CanRead;
		public override bool CanSeek => this.stream.CanSeek;
		public override bool CanTimeout => this.stream.CanTimeout;
		public override bool CanWrite => this.stream.CanWrite;
		public override void Flush() => this.stream.Flush();
		public override long Length => this.stream.Length;
		public override long Position
		{
			get => this.stream.Position;
			set => this.stream.Position = value;
		}
		public override int Read(byte[] buffer, int offset, int count) => this.stream.Read(buffer, offset, count);
		public override int Read(Span<byte> buffer) => this.stream.Read(buffer);
		public override int ReadByte() => this.stream.ReadByte();
		public override int ReadTimeout 
		{
			get => this.stream.ReadTimeout; 
			set => this.stream.ReadTimeout = value; 
		}
		public override long Seek(long offset, SeekOrigin origin) => this.stream.Seek(offset, origin);
		public override void SetLength(long value) => this.stream.SetLength(value);
		public override void Write(byte[] buffer, int offset, int count) => this.stream.Write(buffer, offset, count);
		public override void Write(ReadOnlySpan<byte> buffer) => this.stream.Write(buffer);
		public override int WriteTimeout
		{
			get => this.stream.WriteTimeout;
			set => this.stream.WriteTimeout = value; 
		}
#pragma warning disable CS1591
	}
}
