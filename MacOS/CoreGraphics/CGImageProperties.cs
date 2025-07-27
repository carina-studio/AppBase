using CarinaStudio.MacOS.CoreFoundation;
using System;
using System.Runtime.InteropServices;

namespace CarinaStudio.MacOS.CoreGraphics;

#pragma warning disable CS8618

/// <summary>
/// Predefined properties of <see cref="CGImage"/>.
/// </summary>
public static unsafe class CGImageProperties
{
    /// <summary>
    /// Please refer to <a href="https://developer.apple.com/documentation/imageio/kCGImagePropertyColorModel">kCGImagePropertyColorModel</a>.
    /// </summary>
    public static readonly CFString ColorModel;
    /// <summary>
    /// Please refer to <a href="https://developer.apple.com/documentation/imageio/kCGImagePropertyColorModelCMYK">kCGImagePropertyColorModelCMYK</a>.
    /// </summary>
    public static readonly CFString ColorModelCMYK;
    /// <summary>
    /// Please refer to <a href="https://developer.apple.com/documentation/imageio/kCGImagePropertyColorModelGray">kCGImagePropertyColorModelGray</a>.
    /// </summary>
    public static readonly CFString ColorModelGray;
    /// <summary>
    /// Please refer to <a href="https://developer.apple.com/documentation/imageio/kCGImagePropertyColorModelLab">kCGImagePropertyColorModelLab</a>.
    /// </summary>
    public static readonly CFString ColorModelLab;
    /// <summary>
    /// Please refer to <a href="https://developer.apple.com/documentation/imageio/kCGImagePropertyColorModelRGB">kCGImagePropertyColorModelRGB</a>.
    /// </summary>
    public static readonly CFString ColorModelRGB;
    /// <summary>
    /// Please refer to <a href="https://developer.apple.com/documentation/imageio/kCGImagePropertyDepth">kCGImagePropertyDepth</a>.
    /// </summary>
    public static readonly CFString Depth;
    /// <summary>
    /// Please refer to <a href="https://developer.apple.com/documentation/imageio/kCGImagePropertyDPIHeight">kCGImagePropertyDPIHeight</a>.
    /// </summary>
    public static readonly CFString DPIHeight;
    /// <summary>
    /// Please refer to <a href="https://developer.apple.com/documentation/imageio/kCGImagePropertyDPIWidth">kCGImagePropertyDPIWidth</a>.
    /// </summary>
    public static readonly CFString DPIWidth;
    /// <summary>
    /// Please refer to <a href="https://developer.apple.com/documentation/imageio/kCGImagePropertyFileContentsDictionary">kCGImagePropertyFileContentsDictionary</a>.
    /// </summary>
    public static readonly CFString FileContentsDictionary;
    /// <summary>
    /// Please refer to <a href="https://developer.apple.com/documentation/imageio/kCGImagePropertyFileSize">kCGImagePropertyFileSize</a>.
    /// </summary>
    public static readonly CFString FileSize;
    /// <summary>
    /// Please refer to <a href="https://developer.apple.com/documentation/imageio/kCGImagePropertyHasAlpha">kCGImagePropertyHasAlpha</a>.
    /// </summary>
    public static readonly CFString HasAlpha;
    /// <summary>
    /// Please refer to <a href="https://developer.apple.com/documentation/imageio/kCGImagePropertyImageCount">kCGImagePropertyImageCount</a>.
    /// </summary>
    public static readonly CFString ImageCount;
    /// <summary>
    /// Please refer to <a href="https://developer.apple.com/documentation/imageio/kCGImagePropertyImages">kCGImagePropertyImages</a>.
    /// </summary>
    public static readonly CFString Images;
    /// <summary>
    /// Please refer to <a href="https://developer.apple.com/documentation/imageio/kCGImagePropertyNamedColorSpace">kCGImagePropertyNamedColorSpace</a>.
    /// </summary>
    public static readonly CFString NamedColorSpace;
    /// <summary>
    /// Please refer to <a href="https://developer.apple.com/documentation/imageio/kCGImagePropertyOrientation">kCGImagePropertyOrientation</a>.
    /// </summary>
    public static readonly CFString Orientation;
    /// <summary>
    /// Please refer to <a href="https://developer.apple.com/documentation/imageio/kCGImagePropertyPixelFormat">kCGImagePropertyPixelFormat</a>.
    /// </summary>
    public static readonly CFString PixelFormat;
    /// <summary>
    /// Please refer to <a href="https://developer.apple.com/documentation/imageio/kCGImagePropertyPixelHeight">kCGImagePropertyPixelHeight</a>.
    /// </summary>
    public static readonly CFString PixelHeight;
    /// <summary>
    /// Please refer to <a href="https://developer.apple.com/documentation/imageio/kCGImagePropertyPixelWidth">kCGImagePropertyPixelWidth</a>.
    /// </summary>
    public static readonly CFString PixelWidth;
    /// <summary>
    /// Please refer to <a href="https://developer.apple.com/documentation/imageio/kCGImagePropertyPrimaryImage">kCGImagePropertyPrimaryImage</a>.
    /// </summary>
    public static readonly CFString PrimaryImage;
    /// <summary>
    /// Please refer to <a href="https://developer.apple.com/documentation/imageio/kCGImagePropertyProfileName">kCGImagePropertyProfileName</a>.
    /// </summary>
    public static readonly CFString ProfileName;
    /// <summary>
    /// Please refer to <a href="https://developer.apple.com/documentation/imageio/kCGImagePropertyThumbnailImages">kCGImagePropertyThumbnailImages</a>.
    /// </summary>
    public static readonly CFString ThumbnailImages;


    // Static initializer.
    static CGImageProperties()
    {
        // check platform
        if (Platform.IsNotMacOS)
            return;
        
        // load symbols in ImageIO.Framework
        var imageIOLibHandle = NativeLibraryHandles.ImageIO;
        ColorModel = new CFString(*(nint*)NativeLibrary.GetExport(imageIOLibHandle, "kCGImagePropertyColorModel"), false, false);
        ColorModelCMYK = new CFString(*(nint*)NativeLibrary.GetExport(imageIOLibHandle, "kCGImagePropertyColorModelCMYK"), false, false);
        ColorModelGray = new CFString(*(nint*)NativeLibrary.GetExport(imageIOLibHandle, "kCGImagePropertyColorModelGray"), false, false);
        ColorModelLab = new CFString(*(nint*)NativeLibrary.GetExport(imageIOLibHandle, "kCGImagePropertyColorModelLab"), false, false);
        ColorModelRGB = new CFString(*(nint*)NativeLibrary.GetExport(imageIOLibHandle, "kCGImagePropertyColorModelRGB"), false, false);
        Depth = new CFString(*(nint*)NativeLibrary.GetExport(imageIOLibHandle, "kCGImagePropertyDepth"), false, false);
        DPIHeight = new CFString(*(nint*)NativeLibrary.GetExport(imageIOLibHandle, "kCGImagePropertyDPIHeight"), false, false);
        DPIWidth = new CFString(*(nint*)NativeLibrary.GetExport(imageIOLibHandle, "kCGImagePropertyDPIWidth"), false, false);
        FileContentsDictionary = new CFString(*(nint*)NativeLibrary.GetExport(imageIOLibHandle, "kCGImagePropertyFileContentsDictionary"), false, false);
        FileSize = new CFString(*(nint*)NativeLibrary.GetExport(imageIOLibHandle, "kCGImagePropertyFileSize"), false, false);
        HasAlpha = new CFString(*(nint*)NativeLibrary.GetExport(imageIOLibHandle, "kCGImagePropertyHasAlpha"), false, false);
        ImageCount = new CFString(*(nint*)NativeLibrary.GetExport(imageIOLibHandle, "kCGImagePropertyImageCount"), false, false);
        Images = new CFString(*(nint*)NativeLibrary.GetExport(imageIOLibHandle, "kCGImagePropertyImages"), false, false);
        NamedColorSpace = new CFString(*(nint*)NativeLibrary.GetExport(imageIOLibHandle, "kCGImagePropertyNamedColorSpace"), false, false);
        Orientation = new CFString(*(nint*)NativeLibrary.GetExport(imageIOLibHandle, "kCGImagePropertyOrientation"), false, false);
        PixelFormat = new CFString(*(nint*)NativeLibrary.GetExport(imageIOLibHandle, "kCGImagePropertyPixelFormat"), false, false);
        PixelHeight = new CFString(*(nint*)NativeLibrary.GetExport(imageIOLibHandle, "kCGImagePropertyPixelHeight"), false, false);
        PixelWidth = new CFString(*(nint*)NativeLibrary.GetExport(imageIOLibHandle, "kCGImagePropertyPixelWidth"), false, false);
        PrimaryImage = new CFString(*(nint*)NativeLibrary.GetExport(imageIOLibHandle, "kCGImagePropertyPrimaryImage"), false, false);
        ProfileName = new CFString(*(nint*)NativeLibrary.GetExport(imageIOLibHandle, "kCGImagePropertyProfileName"), false, false);
        ThumbnailImages = new CFString(*(nint*)NativeLibrary.GetExport(imageIOLibHandle, "kCGImagePropertyThumbnailImages"), false, false);
    }
}