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
    CGImage(IntPtr image, bool ownsInstance) : this(image, true, ownsInstance)
    { }
    CGImage(IntPtr image, bool checkType, bool ownsInstance) : base(image, ownsInstance)
    { 
        if (checkType && image != IntPtr.Zero && this.TypeDescription != "CGImage")
            throw new ArgumentException("Type of instance is not CGImage.");
    }
}