using CarinaStudio.MacOS.CoreFoundation;
using CarinaStudio.MacOS.CoreGraphics;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace CarinaStudio.MacOS.ImageIO;

/// <summary>
/// CGImageSource.
/// </summary>
public unsafe class CGImageSource : CFObject
{
    // Native symbols.
    static readonly delegate*<IntPtr, nuint, IntPtr, IntPtr> CGImageSourceCopyPropertiesAtIndex;
    static readonly delegate*<IntPtr, nuint, IntPtr, IntPtr> CGImageSourceCreateImageAtIndex;
    static readonly delegate*<IntPtr, IntPtr, IntPtr> CGImageSourceCreateWithData;
    static readonly delegate*<IntPtr, nuint> CGImageSourceGetCount;
    static readonly delegate*<IntPtr, nuint> CGImageSourceGetPrimaryImageIndex;
    static readonly delegate*<IntPtr, CGImageSourceStatus> CGImageSourceGetStatus;
    
    
    // Static constructor.
    static CGImageSource()
    {
        if (Platform.IsNotMacOS)
            return;
        var libHandle = NativeLibraryHandles.ImageIO;
        CGImageSourceCopyPropertiesAtIndex = (delegate*<IntPtr, nuint, IntPtr, IntPtr>)NativeLibrary.GetExport(libHandle, nameof(CGImageSourceCopyPropertiesAtIndex));
        CGImageSourceCreateImageAtIndex = (delegate*<IntPtr, nuint, IntPtr, IntPtr>)NativeLibrary.GetExport(libHandle, nameof(CGImageSourceCreateImageAtIndex));
        CGImageSourceCreateWithData = (delegate*<IntPtr, IntPtr, IntPtr>)NativeLibrary.GetExport(libHandle, nameof(CGImageSourceCreateWithData));
        CGImageSourceGetCount = (delegate*<IntPtr, nuint>)NativeLibrary.GetExport(libHandle, nameof(CGImageSourceGetCount));
        CGImageSourceGetPrimaryImageIndex = (delegate*<IntPtr, nuint>)NativeLibrary.GetExport(libHandle, nameof(CGImageSourceGetPrimaryImageIndex));
        CGImageSourceGetStatus = (delegate*<IntPtr, CGImageSourceStatus>)NativeLibrary.GetExport(libHandle, nameof(CGImageSourceGetStatus));
    }


    // Constructor.
    CGImageSource(IntPtr image, bool ownsInstance) : this(image, true, ownsInstance)
    { }
    CGImageSource(IntPtr image, bool checkType, bool ownsInstance) : base(image, ownsInstance)
    { 
        if (checkType && image != IntPtr.Zero && this.TypeDescription != "CGImageSource")
            throw new ArgumentException("Type of instance is not CGImageSource.");
    }

    
    /// <summary>
    /// Get properties of the image at a specified location.
    /// </summary>
    /// <param name="index">Location.</param>
    /// <param name="options">Options.</param>
    /// <returns>Properties got from image source.</returns>
    public CFDictionary? CopyPropertiesAtIndex(int index, CFDictionary? options = null)
    {
        if (index < 0)
            throw new ArgumentOutOfRangeException(nameof(index));
        var handle = CGImageSourceCopyPropertiesAtIndex(this.Handle, (nuint)index, options?.Handle ?? default);
        return handle != default ? new CFDictionary(handle, false, true) : null;
    }


    /// <summary>
    /// Create primary image.
    /// </summary>
    /// <returns>Created image, or Null if error occurred.</returns>
    public CGImage? CreateImage()
    {
        this.VerifyReleased();
        return FromHandle<CGImage>(CGImageSourceCreateImageAtIndex(this.Handle, CGImageSourceGetPrimaryImageIndex(this.Handle), IntPtr.Zero), true);
    }


    /// <summary>
    /// Create image.
    /// </summary>
    /// <param name="index">Index of image.</param>
    /// <returns>Created image, or Null if error occurred.</returns>
    public CGImage? CreateImage(int index)
    {
        this.VerifyReleased();
        if (index < 0 || index >= this.ImageCount)
            throw new ArgumentOutOfRangeException(nameof(index));
        return FromHandle<CGImage>(CGImageSourceCreateImageAtIndex(this.Handle, (nuint)index, IntPtr.Zero), true);
    }


    /// <summary>
    /// Create <see cref="CGImageSource"/> from <see cref="CFData"/>.
    /// </summary>
    /// <param name="data">Data.</param>
    /// <returns><see cref="CGImageSource"/>.</returns>
    public static CGImageSource FromData(CFData data)
    {
        if (data.Handle == IntPtr.Zero)
            throw new ArgumentException("Data cannot be null.");
        var handle = CGImageSourceCreateWithData(data.Handle, IntPtr.Zero);
        try
        {
            VerifyStatus(CGImageSourceGetStatus(handle));
            return new(handle, false, true);
        }
        catch
        {
            Release(handle);
            throw;
        }
    }


    /// <summary>
    /// Create <see cref="CGImageSource"/> from file.
    /// </summary>
    /// <param name="fileName">File name.</param>
    /// <returns><see cref="CGImageSource"/>.</returns>
    public static CGImageSource FromFile(string fileName)
    {
        using var stream = new FileStream(fileName, FileMode.Open);
        using var data = CFData.FromStream(stream);
        var handle = CGImageSourceCreateWithData(data.Handle, IntPtr.Zero);
        try
        {
            VerifyStatus(CGImageSourceGetStatus(handle));
            return new(handle, false, true);
        }
        catch
        {
            Release(handle);
            throw;
        }
    }


    /// <summary>
    /// Create <see cref="CGImageSource"/> from stream.
    /// </summary>
    /// <param name="stream">Stream.</param>
    /// <returns><see cref="CGImageSource"/>.</returns>
    public static CGImageSource FromStream(Stream stream)
    {
        using var data = CFData.FromStream(stream);
        var handle = CGImageSourceCreateWithData(data.Handle, IntPtr.Zero);
        try
        {
            VerifyStatus(CGImageSourceGetStatus(handle));
            return new(handle, false, true);
        }
        catch
        {
            Release(handle);
            throw;
        }
    }


    /// <summary>
    /// Get number of images.
    /// </summary>
    public int ImageCount
    {
        get
        {
            this.VerifyReleased();
            return (int)CGImageSourceGetCount(this.Handle);
        }
    }


    /// <summary>
    /// Get index of primary image.
    /// </summary>
    public int PrimaryImageIndex
    {
        get
        {
            this.VerifyReleased();
            return (int)CGImageSourceGetPrimaryImageIndex(this.Handle);
        }
    }


    /// <summary>
    /// Get status of image source.
    /// </summary>
    public CGImageSourceStatus Status
    {
        get
        {
            this.VerifyReleased();
            return CGImageSourceGetStatus(this.Handle);
        }
    }


    // Throw exception if status is not Completed.
    static void VerifyStatus(CGImageSourceStatus status)
    {
        if (status != CGImageSourceStatus.Complete)
            throw new ArgumentException($"Failed to create image source, error: {(int)status}");
    }
}