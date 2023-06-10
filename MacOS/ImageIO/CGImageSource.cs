using CarinaStudio.MacOS.CoreFoundation;
using CarinaStudio.MacOS.CoreGraphics;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace CarinaStudio.MacOS.ImageIO;

/// <summary>
/// CGImageSource.
/// </summary>
public class CGImageSource : CFObject
{
    // Native symbols.
    [DllImport(NativeLibraryNames.ImageIO)]
    static extern IntPtr CGImageSourceCopyPropertiesAtIndex(IntPtr isrc, nuint index, IntPtr options);
    [DllImport(NativeLibraryNames.ImageIO)]
    static extern IntPtr CGImageSourceCreateImageAtIndex(IntPtr isrc, nuint index, IntPtr options);
    [DllImport(NativeLibraryNames.ImageIO)]
    static extern IntPtr CGImageSourceCreateWithData(IntPtr data, IntPtr options);
    [DllImport(NativeLibraryNames.ImageIO)]
    static extern nuint CGImageSourceGetCount(IntPtr isrc);
    [DllImport(NativeLibraryNames.ImageIO)]
    static extern nuint CGImageSourceGetPrimaryImageIndex(IntPtr isrc);
    [DllImport(NativeLibraryNames.ImageIO)]
    static extern CGImageSourceStatus CGImageSourceGetStatus(IntPtr isrc);


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
    /// <returns>Image.</returns>
    public CGImage CreateImage()
    {
        this.VerifyReleased();
        return CFObject.FromHandle<CGImage>(CGImageSourceCreateImageAtIndex(this.Handle, CGImageSourceGetPrimaryImageIndex(this.Handle), IntPtr.Zero), true);
    }


    /// <summary>
    /// Create image.
    /// </summary>
    /// <param name="index">Index of image.</param>
    /// <returns>Image.</returns>
    public CGImage CreateImage(int index)
    {
        this.VerifyReleased();
        if (index < 0 || index >= this.ImageCount)
            throw new ArgumentOutOfRangeException(nameof(index));
        return CFObject.FromHandle<CGImage>(CGImageSourceCreateImageAtIndex(this.Handle, (nuint)index, IntPtr.Zero), true);
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
            CFObject.Release(handle);
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
            CFObject.Release(handle);
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
            CFObject.Release(handle);
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