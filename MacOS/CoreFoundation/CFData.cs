using System;
using System.IO;
using System.Runtime.InteropServices;

namespace CarinaStudio.MacOS.CoreFoundation;

/// <summary>
/// CFData.
/// </summary>
public unsafe class CFData : CFObject
{
    // Native symbols.
    [DllImport(NativeLibraryNames.CoreFoundation)]
	internal static extern void CFDataAppendBytes(IntPtr theData, void* bytes, nint length);
    [DllImport(NativeLibraryNames.CoreFoundation)]
	static extern IntPtr CFDataCreate(IntPtr allocator, void* data, nint length);
    [DllImport(NativeLibraryNames.CoreFoundation)]
    internal static extern IntPtr CFDataCreateMutable(IntPtr allocator, nint capacity);
    [DllImport(NativeLibraryNames.CoreFoundation)]
    static extern byte* CFDataGetBytePtr(IntPtr theData);
    [DllImport(NativeLibraryNames.CoreFoundation)]
    static extern nuint CFDataGetLength(IntPtr theData);
    [DllImport(NativeLibraryNames.CoreFoundation)]
    internal static extern byte* CFDataGetMutableBytePtr(IntPtr theData);
    [DllImport(NativeLibraryNames.CoreFoundation)]
    internal static extern void CFDataSetLength(IntPtr theData, nint length);


    /// <summary>
    /// Initialize new <see cref="CFData"/> instance.
    /// </summary>
    /// <param name="data">Source data.</param>
    public CFData(byte[] data) : this(CFAllocator.Default, data, 0, data.Length)
    { }


    /// <summary>
    /// Initialize new <see cref="CFData"/> instance.
    /// </summary>
    /// <param name="data">Source data.</param>
    /// <param name="offset">Offset to first byte to copy from source.</param>
    /// <param name="size">Number of bytes to copy from source.</param>
    public CFData(byte[] data, int offset, int size) : this(CFAllocator.Default, data, offset, size)
    { }


    /// <summary>
    /// Initialize new <see cref="CFData"/> instance.
    /// </summary>
    /// <param name="allocator">Allocator.</param>
    /// <param name="data">Source data.</param>
    /// <param name="offset">Offset to first byte to copy from source.</param>
    /// <param name="size">Number of bytes to copy from source.</param>
    public CFData(CFAllocator allocator, byte[] data, int offset, int size) : this(data.PinAs<byte, byte, IntPtr>(ptr =>
    {
        if (offset < 0 || offset >= data.Length)
            throw new ArgumentOutOfRangeException(nameof(offset));
        if (size <= 0 || offset + size > data.Length)
            throw new ArgumentOutOfRangeException(nameof(size));
        return CFDataCreate(allocator.Handle, ptr + offset, size);
    }), false, true)
    { }


    /// <summary>
    /// Initialize new <see cref="CFData"/> instance.
    /// </summary>
    /// <param name="stream">Stream to copy data from.</param>
    public CFData(Stream stream) : this(CFAllocator.Default, stream)
    { }


    /// <summary>
    /// Initialize new <see cref="CFData"/> instance.
    /// </summary>
    /// <param name="allocator">Allocator.</param>
    /// <param name="stream">Stream to copy data from.</param>
    public CFData(CFAllocator allocator, Stream stream) : this(Global.Run(() =>
    {
        var size = 0L;
        try
        {
            size = stream.Length - stream.Position;
        }
        catch
        {
            var memoryStream = new MemoryStream();
            stream.CopyTo(memoryStream);
            memoryStream.Position = 0;
            stream = memoryStream;
            size = memoryStream.Length;
        }
        if (size > int.MaxValue)
            throw new NotSupportedException($"Size of data in stream is too large: {size}.");
        var handle = IntPtr.Zero;
        try
        {
            handle = CFDataCreateMutable(allocator.Handle, (nint)size);
            var bufferPtr = CFDataGetMutableBytePtr(handle);
            CFDataSetLength(handle, stream.Read(new Span<byte>(bufferPtr, (int)size)));
        }
        catch
        {
            if (handle != IntPtr.Zero)
                CFObject.Release(handle);
            throw;
        }
        return handle;
    }), false, true)
    { }


    /// <summary>
    /// Initialize new <see cref="CFData"/> instance.
    /// </summary>
    /// <param name="data">Address of data to copy from.</param>
    /// <param name="size">Size of data to copy.</param>
    public CFData(IntPtr data, int size) : this(CFAllocator.Default, data, size)
    { }


    /// <summary>
    /// Initialize new <see cref="CFData"/> instance.
    /// </summary>
    /// <param name="allocator">Allocator.</param>
    /// <param name="data">Address of data to copy from.</param>
    /// <param name="size">Size of data to copy.</param>
    public CFData(CFAllocator allocator, IntPtr data, int size) : this(Global.Run(() =>
    {
        if (data == IntPtr.Zero)
            throw new ArgumentException("Address of data cannot be 0.");
        if (size < 0)
            throw new ArgumentOutOfRangeException(nameof(size));
        return CFDataCreate(allocator.Handle, data.ToPointer(), size);
    }), false, true)
    { }


    /// <summary>
    /// Initialize new <see cref="CFData"/> instance.
    /// </summary>
    /// <param name="data">Source data to copy.</param>
    public CFData(ReadOnlySpan<byte> data) : this(CFAllocator.Default, data)
    { }


    /// <summary>
    /// Initialize new <see cref="CFData"/> instance.
    /// </summary>
    /// <param name="allocator">Allocator.</param>
    /// <param name="data">Source data to copy.</param>
    public CFData(CFAllocator allocator, ReadOnlySpan<byte> data) : this(data.Pin((ptr, size) =>
    {
        return CFDataCreate(allocator.Handle, ptr.ToPointer(), size);
    }), false, true)
    { }


    /// <summary>
    /// Initialize new <see cref="CFData"/> instance.
    /// </summary>
    /// <param name="data">Source data to copy.</param>
    public CFData(Span<byte> data) : this(CFAllocator.Default, data)
    { }


    /// <summary>
    /// Initialize new <see cref="CFData"/> instance.
    /// </summary>
    /// <param name="allocator">Allocator.</param>
    /// <param name="data">Source data to copy.</param>
    public CFData(CFAllocator allocator, Span<byte> data) : this(data.Pin((ptr, size) =>
    {
        return CFDataCreate(allocator.Handle, ptr.ToPointer(), size);
    }), false, true)
    { }


    // Constructor.
    CFData(IntPtr s, bool ownsInstance) : this(s, true, ownsInstance)
    { }
    internal CFData(IntPtr s, bool checkType, bool ownsInstance) : base(s, ownsInstance)
    { 
        if (checkType && s != IntPtr.Zero && this.TypeDescription != "CFData" && this.TypeDescription != "CFMutableData")
            throw new ArgumentException("Type of instance is not CFData.");
    }


    /// <summary>
    /// Get <see cref="ReadOnlySpan{Byte}"/> to read data.
    /// </summary>
    /// <returns><see cref="ReadOnlySpan{Byte}"/>.</returns>
    public ReadOnlySpan<byte> AsSpan()
    {
        var length = this.Length;
        if (length > int.MaxValue)
            throw new NotSupportedException($"Too large to get data as span: {length}.");
        return new(CFDataGetBytePtr(this.Handle), (int)length);
    }


    /// <summary>
    /// Get <see cref="ReadOnlySpan{Byte}"/> to read data.
    /// </summary>
    /// <param name="offset">Offset to first byte to read.</param>
    /// <returns><see cref="ReadOnlySpan{Byte}"/>.</returns>
    public ReadOnlySpan<byte> AsSpan(long offset)
    {
        var length = this.Length;
        if (offset < 0 || offset >= length)
            throw new ArgumentOutOfRangeException(nameof(Length));
        var size = (length - offset);
        if (size > int.MaxValue)
            throw new NotSupportedException($"Too large to get data as span: {size}.");
        return new(CFDataGetBytePtr(this.Handle) + offset, (int)size);
    }


    /// <summary>
    /// Get <see cref="ReadOnlySpan{Byte}"/> to read data.
    /// </summary>
    /// <param name="offset">Offset to first byte to read.</param>
    /// <param name="size">Size in bytes to read.</param>
    /// <returns><see cref="ReadOnlySpan{Byte}"/>.</returns>
    public ReadOnlySpan<byte> AsSpan(long offset, int size)
    {
        var length = this.Length;
        if (offset < 0 || offset >= length)
            throw new ArgumentOutOfRangeException(nameof(Length));
        if (size <= 0 || offset + size > length)
            throw new ArgumentOutOfRangeException(nameof(size));
        return new(CFDataGetBytePtr(this.Handle) + offset, size);
    }


    /// <summary>
    /// Get length of data in bytes.
    /// </summary>
    public long Length
    {
        get
        {
            this.VerifyReleased();
            var length = CFDataGetLength(this.Handle);
            if (length > long.MaxValue)
                throw new NotSupportedException($"Length is too large: {length}.");
            return (long)length;
        }
    }
}