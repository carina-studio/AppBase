using System;

namespace CarinaStudio.MacOS.CoreFoundation;

/// <summary>
/// CFMutableData.
/// </summary>
public unsafe class CFMutableData : CFData
{
    /// <summary>
    /// Initialize new <see cref="CFMutableData"/> instance.
    /// </summary>
    /// <param name="capacity">Capacity.</param>
    public CFMutableData(int capacity) : this(CFAllocator.Default, capacity)
    { }


    /// <summary>
    /// Initialize new <see cref="CFMutableData"/> instance.
    /// </summary>
    /// <param name="allocator">Allocator.</param>
    /// <param name="capacity">Capacity.</param>
    public CFMutableData(CFAllocator allocator, int capacity) : this(CFDataCreateMutable(allocator.Handle, capacity), false, true)
    { }


    // Constructor.
    CFMutableData(IntPtr s, bool ownsInstance) : this(s, true, ownsInstance)
    { }
    internal CFMutableData(IntPtr s, bool checkType, bool ownsInstance) : base(s, checkType, ownsInstance)
    { }


    /// <summary>
    /// Append data to instance.
    /// </summary>
    /// <param name="data">Data to append.</param>
    public void Append(byte[] data) =>
        this.Append(data, 0, data.Length);


    /// <summary>
    /// Append data to instance.
    /// </summary>
    /// <param name="data">Data to append.</param>
    /// <param name="offset">Offset to first byte in <paramref name="data"/> to append.</param>
    /// <param name="size">Number of bytes to append.</param>
    public void Append(byte[] data, int offset, int size)
    {
        if (size == 0)
            return;
        if (offset < 0 || offset + size > data.Length)
            throw new ArgumentOutOfRangeException(nameof(offset));
        if (size < 0)
            throw new ArgumentOutOfRangeException(nameof(size));
        this.VerifyReleased();
        fixed (byte* p = data)
            CFDataAppendBytes(this.Handle, p + offset, size);
    }


    /// <summary>
    /// Append data to instance.
    /// </summary>
    /// <param name="data">Data to append.</param>
    public void Append(Span<byte> data)
    {
        if (data.Length == 0)
            return;
        this.VerifyReleased();
        ref var bytes = ref data.GetPinnableReference();
        fixed (byte* p = &bytes)
            CFDataAppendBytes(this.Handle, p, data.Length);
    }


    /// <summary>
    /// Append data to instance.
    /// </summary>
    /// <param name="data">Data to append.</param>
    public void Append(ReadOnlySpan<byte> data)
    {
        if (data.Length == 0)
            return;
        this.VerifyReleased();
        ref readonly var bytes = ref data.GetPinnableReference();
        fixed (byte* p = &bytes)
            CFDataAppendBytes(this.Handle, p, data.Length);
    }


    /// <summary>
    /// Get <see cref="Span{Byte}"/> to access data.
    /// </summary>
    /// <returns><see cref="Span{Byte}"/>.</returns>
    public new Span<byte> AsSpan()
    {
        var length = this.Length;
        if (length > int.MaxValue)
            throw new NotSupportedException($"Too large to get data as span: {length}.");
        return new(CFDataGetMutableBytePtr(this.Handle), (int)length);
    }


    /// <summary>
    /// Get <see cref="Span{Byte}"/> to access data.
    /// </summary>
    /// <param name="offset">Offset to first byte to access.</param>
    /// <returns><see cref="Span{Byte}"/>.</returns>
    public new Span<byte> AsSpan(long offset)
    {
        var length = this.Length;
        if (offset < 0 || offset >= length)
            throw new ArgumentOutOfRangeException(nameof(Length));
        var size = (length - offset);
        if (size > int.MaxValue)
            throw new NotSupportedException($"Too large to get data as span: {size}.");
        return new(CFDataGetMutableBytePtr(this.Handle) + offset, (int)size);
    }


    /// <summary>
    /// Get <see cref="Span{Byte}"/> to access data.
    /// </summary>
    /// <param name="offset">Offset to first byte to access.</param>
    /// <param name="size">Size in bytes to access.</param>
    /// <returns><see cref="Span{Byte}"/>.</returns>
    public new Span<byte> AsSpan(long offset, int size)
    {
        var length = this.Length;
        if (offset < 0 || offset >= length)
            throw new ArgumentOutOfRangeException(nameof(Length));
        if (size <= 0 || offset + size > length)
            throw new ArgumentOutOfRangeException(nameof(size));
        return new(CFDataGetMutableBytePtr(this.Handle) + offset, size);
    }


    /// <summary>
    /// Get or set length of data in bytes.
    /// </summary>
    public new long Length
    {
        get => base.Length;
        set
        {
            if (value < 0 || value > nint.MaxValue)
                throw new ArgumentOutOfRangeException();
            this.VerifyReleased();
            CFDataSetLength(this.Handle, (nint)value);
        }
    }
}