using CarinaStudio.MacOS.CoreFoundation;
using System;
using System.Runtime.InteropServices;

namespace CarinaStudio.MacOS.CoreGraphics;

/// <summary>
/// CGDataProvider.
/// </summary>
public unsafe class CGDataProvider : CFObject
{
    // Native symbols.
    static readonly delegate*<IntPtr, nint, CGDataProviderDirectCallbacks*, IntPtr> CGDataProviderCreateDirect;
    static readonly delegate*<IntPtr, IntPtr> CGDataProviderCreateWithCFData;
    static readonly delegate*<IntPtr, IntPtr> CGDataProviderCopyData;


    // CGDataProviderSequentialCallbacks.
    [StructLayout(LayoutKind.Sequential)]
    struct CGDataProviderDirectCallbacks
    {
        public uint Version;
        public delegate*<IntPtr, IntPtr> GetBytePointer;
        public delegate*<IntPtr, IntPtr, void> ReleaseBytePointer;
        public delegate*<IntPtr, IntPtr, nint, nuint, nuint> GetBytesAtPosition;
        public delegate*<IntPtr, void> ReleaseInfo;
    }


    // Info of direct access.
    class DirectAccessInfo(byte[] data, int offset, int size)
    {
        // Fields.
        public readonly byte[] Data = data;
        public readonly GCHandle DataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
        public readonly int Offset = offset;
        public readonly int Size = size;

        public void Release()
        {
            // ReSharper disable once PossiblyImpureMethodCallOnReadonlyVariable
            this.DataHandle.Free();
        }
    }


    // Static fields.
    static readonly CGDataProviderDirectCallbacks* DataProviderDirectCallbacks;


    // Fields.
    volatile CFData? data;
    
    
    // Static constructor.
    static CGDataProvider()
    {
        if (Platform.IsNotMacOS)
            return;
        var libHandle = NativeLibraryHandles.CoreGraphics;
        CGDataProviderCreateDirect = (delegate*<IntPtr, nint, CGDataProviderDirectCallbacks*, IntPtr>)NativeLibrary.GetExport(libHandle, nameof(CGDataProviderCreateDirect));
        CGDataProviderCreateWithCFData = (delegate*<IntPtr, IntPtr>)NativeLibrary.GetExport(libHandle, nameof(CGDataProviderCreateWithCFData));
        CGDataProviderCopyData = (delegate*<IntPtr, IntPtr>)NativeLibrary.GetExport(libHandle, nameof(CGDataProviderCopyData));
        DataProviderDirectCallbacks = (CGDataProviderDirectCallbacks*)NativeMemory.Alloc((nuint)sizeof(CGDataProviderDirectCallbacks));
        *DataProviderDirectCallbacks = new CGDataProviderDirectCallbacks
        {
            GetBytePointer = &GetDirectBytePointer,
            GetBytesAtPosition = &GetDirectBytesAtPosition,
            ReleaseBytePointer = &ReleaseDirectBytePointer,
            ReleaseInfo = &ReleaseDirectInfo
        };
    }


    /// <summary>
    /// Initialize new <see cref="CGDataProvider"/> which accesses given data directly.
    /// </summary>
    /// <param name="data">Source data.</param>
    public CGDataProvider(byte[] data) : this(data, 0, data.Length)
    { }


    /// <summary>
    /// Initialize new <see cref="CGDataProvider"/> which accesses given data directly.
    /// </summary>
    /// <param name="data">Source data.</param>
    /// <param name="offset">Offset to first byte to access in source data.</param>
    /// <param name="size">Size of data.</param>
    public CGDataProvider(byte[] data, int offset, int size) : this(Global.Run(() =>
    {
        if (offset < 0 || offset > data.Length)
            throw new ArgumentOutOfRangeException(nameof(offset));
        if (size < 0 || offset + size > data.Length)
            throw new ArgumentOutOfRangeException(nameof(size));
        var info = new DirectAccessInfo(data, offset, size);
        var gcHandle = GCHandle.Alloc(info);
        return CGDataProviderCreateDirect(GCHandle.ToIntPtr(gcHandle), size, DataProviderDirectCallbacks);
    }), false, true)
    { }
    

    // Constructor.
    CGDataProvider(IntPtr handle, bool ownsInstance) : this(handle, true, ownsInstance)
    { }
    CGDataProvider(IntPtr handle, bool checkType, bool ownsInstance) : base(handle, ownsInstance)
    { 
        if (checkType && handle != IntPtr.Zero && this.TypeDescription != "CGDataProvider")
            throw new ArgumentException("Type of instance is not CGDataProvider.");
    }


    /// <summary>
    /// Create <see cref="CGDataProvider"/> which accesses given <see cref="CFData"/> directly.
    /// </summary>
    /// <param name="data"><see cref="CFData"/>.</param>
    /// <returns><see cref="CGDataProvider"/>.</returns>
    public static CGDataProvider FromData(CFData data)
    {
        data = data.Retain<CFData>();
        try
        {
            return new(CGDataProviderCreateWithCFData(data.Handle), false, true)
            {
                data = data,
            };
        }
        catch
        {
            data.Release();
            throw;
        }
    }


    // Get pointer of data for direct access.
    static IntPtr GetDirectBytePointer(IntPtr info)
    {
        if (info == default)
            return default;
        var directAccessInfo = GCHandle.FromIntPtr(info).Target as DirectAccessInfo;
        if (directAccessInfo == null)
            return default;
        // ReSharper disable once PossiblyImpureMethodCallOnReadonlyVariable
        return directAccessInfo.DataHandle.AddrOfPinnedObject() + directAccessInfo.Offset;
    }

    
    // Copy direct access data to buffer.
    static nuint GetDirectBytesAtPosition(IntPtr info, IntPtr buffer, nint position, nuint size)
    {
        if (info == default)
            return 0;
        var directAccessInfo = GCHandle.FromIntPtr(info).Target as DirectAccessInfo;
        if (directAccessInfo == null)
            return default;
        if (directAccessInfo.Size <= 0 || position < 0 || position > int.MaxValue || size > int.MaxValue || position >= directAccessInfo.Size)
            return 0;
        var copyCount = Math.Min(directAccessInfo.Size - (int)position, (int)size);
        Marshal.Copy(directAccessInfo.Data, (int)(directAccessInfo.Offset + position), buffer, copyCount);
        return (nuint)copyCount;
    }


    /// <inheritdoc/>
    public override void OnRelease()
    {
        this.data?.Release();
        base.OnRelease();
    }


    // Release data for direct access.
    static void ReleaseDirectBytePointer(IntPtr info, IntPtr pointer)
    { }


    // Release info of direct access.
    static void ReleaseDirectInfo(IntPtr info)
    {
        if (info != default)
            (GCHandle.FromIntPtr(info).Target as DirectAccessInfo)?.Release();
    }


    /// <summary>
    /// Copy data from provider as <see cref="CFData"/>.
    /// </summary>
    /// <returns><see cref="CFData"/>.</returns>
    public CFData ToData()
    {
        this.VerifyReleased();
        var handle = CGDataProviderCopyData(this.Handle);
        if (handle == IntPtr.Zero)
            throw new InvalidOperationException("Unable to copy data from data provider.");
        return FromHandle<CFData>(handle, true).AsNonNull();
    }
}