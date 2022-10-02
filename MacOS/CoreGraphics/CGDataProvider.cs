using CarinaStudio.Collections;
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
    [DllImport(NativeLibraryNames.CoreGraphics)]
    static extern IntPtr CGDataProviderCreateDirect(IntPtr info, nint size, ref CGDataProviderDirectCallbacks callbacks);
    [DllImport(NativeLibraryNames.CoreGraphics)]
    static extern IntPtr CGDataProviderCreateWithCFData(IntPtr data);
    [DllImport(NativeLibraryNames.CoreGraphics)]
	static extern IntPtr CGDataProviderCopyData(IntPtr provider);
    delegate IntPtr CGDataProviderGetBytePointerCallback(IntPtr info);
    delegate nuint CGDataProviderGetBytesAtPositionCallback(IntPtr info, IntPtr buffer, nint position, nuint size);
    delegate void CGDataProviderReleaseBytePointerCallback(IntPtr info, IntPtr pointer);
    delegate void CGDataProviderReleaseInfoCallback(IntPtr info);


    // CGDataProviderSequentialCallbacks.
    [StructLayout(LayoutKind.Sequential)]
    struct CGDataProviderDirectCallbacks
    {
        public uint Version;
        [MarshalAs(UnmanagedType.FunctionPtr)]
        public CGDataProviderGetBytePointerCallback GetBytePointer;
        [MarshalAs(UnmanagedType.FunctionPtr)]
        public CGDataProviderReleaseBytePointerCallback ReleaseBytePointer;
        [MarshalAs(UnmanagedType.FunctionPtr)]
        public CGDataProviderGetBytesAtPositionCallback GetBytesAtPosition;
        [MarshalAs(UnmanagedType.FunctionPtr)]
        public CGDataProviderReleaseInfoCallback ReleaseInfo;
    }


    // Info of direct access.
    class DirectAccessInfo
    {
        public readonly byte[] Data;
        public readonly GCHandle DataHandle;
        public readonly int Offset;

        public DirectAccessInfo(byte[] data, int offset)
        {
            this.Data = data;
            this.DataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
            this.Offset = offset;
        }

        public void Release()
        {
            this.DataHandle.Free();
        }
    }


    // Static fields.
    static CGDataProviderDirectCallbacks DataProviderDirectCallbacks = new()
    {
        GetBytePointer = GetDirectBytePointer,
        GetBytesAtPosition = GetDirectBytesAtPosition,
        ReleaseBytePointer = ReleaseDirectBytePointer,
        ReleaseInfo = ReleaseDirectInfo,
    };


    // Fields.
    volatile CFData? data;


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
        var info = new DirectAccessInfo(data, offset);
        var gcHandle = GCHandle.Alloc(info);
        return CGDataProviderCreateDirect(GCHandle.ToIntPtr(gcHandle), (nint)size, ref DataProviderDirectCallbacks);
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
        return directAccessInfo.DataHandle.AddrOfPinnedObject();
    }

    
    // Copy direct access data to buffer.
    static nuint GetDirectBytesAtPosition(IntPtr info, IntPtr buffer, nint position, nuint size)
    {
        if (info == default)
            return 0;
        var directAccessInfo = GCHandle.FromIntPtr(info).Target as DirectAccessInfo;
        if (directAccessInfo == null)
            return default;
        if (directAccessInfo.Data.IsEmpty() || position >= directAccessInfo.Data.Length || position >= int.MaxValue || position < 0)
            return 0;
        var maxCopyCount = Math.Min((ulong)int.MaxValue, (ulong)directAccessInfo.Data.Length - (ulong)position);
        var copyCount = (int)Math.Min(maxCopyCount, (ulong)size);
        Marshal.Copy(directAccessInfo.Data, (int)position, (IntPtr)buffer, copyCount);
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
        return CFObject.FromHandle<CFData>(handle, true);
    }
}