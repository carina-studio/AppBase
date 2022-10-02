using CarinaStudio.MacOS.CoreFoundation;
using System;
using System.Runtime.InteropServices;

namespace CarinaStudio.MacOS.CoreGraphics;

/// <summary>
/// CGImage.
/// </summary>
public class CGImage : CFObject
{
    // Native symbols.
    [DllImport(NativeLibraryNames.CoreGraphics)]
    static extern CGImageAlphaInfo CGImageGetAlphaInfo(IntPtr image);
    [DllImport(NativeLibraryNames.CoreGraphics)]
    static extern nuint CGImageGetBitsPerPixel(IntPtr image);
    [DllImport(NativeLibraryNames.CoreGraphics)]
    static extern CGImageByteOrderInfo CGImageGetByteOrderInfo(IntPtr image);
    [DllImport(NativeLibraryNames.CoreGraphics)]
    static extern nuint CGImageGetBytesPerRow(IntPtr image);
    [DllImport(NativeLibraryNames.CoreGraphics)]
    static extern IntPtr CGImageGetColorSpace(IntPtr image);
    [DllImport(NativeLibraryNames.CoreGraphics)]
    static extern IntPtr CGImageGetDataProvider(IntPtr image);
    [DllImport(NativeLibraryNames.CoreGraphics)]
    static extern nuint CGImageGetHeight(IntPtr image);
    [DllImport(NativeLibraryNames.CoreGraphics)]
    static extern CGImagePixelFormatInfo CGImageGetPixelFormatInfo(IntPtr image);
    [DllImport(NativeLibraryNames.CoreGraphics)]
    static extern nuint CGImageGetWidth(IntPtr image);


    // Fields.
    CGDataProvider? dataProvider;


    // Constructor.
    CGImage(IntPtr image, bool ownsInstance) : this(image, true, ownsInstance)
    { }
    CGImage(IntPtr image, bool checkType, bool ownsInstance) : base(image, ownsInstance)
    { 
        if (checkType && image != IntPtr.Zero && this.TypeDescription != "CGImage")
            throw new ArgumentException("Type of instance is not CGImage.");
    }


    /// <summary>
    /// Get number of bits for each pixel.
    /// </summary>
    public int BitsPerPixel
    {
        get
        {
            this.VerifyReleased();
            return (int)CGImageGetBitsPerPixel(this.Handle);
        }
    }


    /// <summary>
    /// Get byte order.
    /// </summary>
    public CGImageByteOrderInfo ByteOrder
    {
        get
        {
            this.VerifyReleased();
            return CGImageGetByteOrderInfo(this.Handle);
        }
    }


    /// <summary>
    /// Get number of bytes for each row of image.
    /// </summary>
    public int BytesPerRow
    {
        get
        {
            this.VerifyReleased();
            return (int)CGImageGetBytesPerRow(this.Handle);
        }
    }


    /// <summary>
    /// Get color space of image.
    /// </summary>
    public CGColorSpace? ColorSpace
    {
        get
        {
            this.VerifyReleased();
            var handle = CGImageGetColorSpace(this.Handle);
            return handle != IntPtr.Zero ? CFObject.FromHandle<CGColorSpace>(handle, false) : null;
        }
    }


    /// <summary>
    /// Get <see cref="CGDataProvider"/> to access data of image.
    /// </summary>
    public CGDataProvider DataProvider
    {
        get
        {
            this.VerifyReleased();
            return this.dataProvider ?? CFObject.FromHandle<CGDataProvider>(CGImageGetDataProvider(this.Handle), false).Also(it =>
            {
                this.dataProvider = it;
            });
        }
    }


    /// <summary>
    /// Get height of image in pixels.
    /// </summary>
    public int Height
    {
        get
        {
            this.VerifyReleased();
            return (int)CGImageGetHeight(this.Handle);
        }
    }


    /// <summary>
    /// Get pixel format.
    /// </summary>
    public CGImagePixelFormatInfo PixelFormat
    {
        get
        {
            this.VerifyReleased();
            return CGImageGetPixelFormatInfo(this.Handle);
        }
    }


    /// <summary>
    /// Get width of image in pixels.
    /// </summary>
    public int Width
    {
        get
        {
            this.VerifyReleased();
            return (int)CGImageGetWidth(this.Handle);
        }
    }
}