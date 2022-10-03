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
    static extern IntPtr CGImageCreate(nuint width, nuint height, nuint bitsPerComponent, nuint bitsPerPixel, nuint bytesPerRow, IntPtr space, CGBitmapInfo bitmapInfo, IntPtr provider, IntPtr decode, bool shouldInterpolate, CGColorRenderingIntent intent);
    [DllImport(NativeLibraryNames.CoreGraphics)]
    static extern IntPtr CGImageCreateCopy(IntPtr image);
    [DllImport(NativeLibraryNames.CoreGraphics)]
    static extern IntPtr CGImageCreateCopyWithColorSpace(IntPtr image, IntPtr space);
    [DllImport(NativeLibraryNames.CoreGraphics)]
    static extern CGImageAlphaInfo CGImageGetAlphaInfo(IntPtr image);
    [DllImport(NativeLibraryNames.CoreGraphics)]
    static extern CGBitmapInfo CGImageGetBitmapInfo(IntPtr image);
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


    // Static fields.
    [ThreadStatic]
    static CGDataProvider? tempDataProvider;


    // Fields.
    CGDataProvider? dataProvider;
    readonly bool ownsDataProvider;


    /// <summary>
    /// Initialize new empty <see cref="CGImage"/> instance with <see cref="CGImagePixelFormatInfo.Packed"/> pixel format and 8-bit component.
    /// </summary>
    /// <param name="width">Width in pixels.</param>
    /// <param name="height">Height in pixels.</param>
    /// <param name="colorSpace">Color space.</param>
    public CGImage(int width, int height, CGColorSpace colorSpace) : this(width, height, CGImagePixelFormatInfo.Packed, 8, CGImageByteOrderInfo.ByteOrderDefault, width << 2, CGImageAlphaInfo.AlphaLast, colorSpace, CGColorRenderingIntent.Default)
    { }


    /// <summary>
    /// Initialize new empty <see cref="CGImage"/> instance.
    /// </summary>
    /// <param name="width">Width in pixels.</param>
    /// <param name="height">Height in pixels.</param>
    /// <param name="pixelFormat">Pixel format.</param>
    /// <param name="bitsPerComponent">Bits per component.</param>
    /// <param name="byteOrder">Byte order.</param>
    /// <param name="bytesPerRow">Bytes per row.</param>
    /// <param name="alphaInfo">Alpha info.</param>
    /// <param name="colorSpace">Color space.</param>
    /// <param name="renderingIntent">Rendering intent.</param>
    public CGImage(int width, int height, CGImagePixelFormatInfo pixelFormat, int bitsPerComponent, CGImageByteOrderInfo byteOrder, int bytesPerRow, CGImageAlphaInfo alphaInfo, CGColorSpace colorSpace, CGColorRenderingIntent renderingIntent = CGColorRenderingIntent.Default) : this(width, height, pixelFormat, bitsPerComponent, byteOrder, bytesPerRow, alphaInfo, Global.Run(() =>
    {
        if (height <= 0)
            throw new ArgumentOutOfRangeException(nameof(height));
        if (bytesPerRow < 0)
            throw new ArgumentOutOfRangeException(nameof(bytesPerRow));
        tempDataProvider = new CGDataProvider(new byte[bytesPerRow * height]);
        return tempDataProvider;
    }), colorSpace, renderingIntent)
    { 
        tempDataProvider?.Release();
        tempDataProvider = null;
    }


    /// <summary>
    /// Initialize new <see cref="CGImage"/> instance.
    /// </summary>
    /// <param name="width">Width in pixels.</param>
    /// <param name="height">Height in pixels.</param>
    /// <param name="pixelFormat">Pixel format.</param>
    /// <param name="bitsPerComponent">Bits per component.</param>
    /// <param name="byteOrder">Byte order.</param>
    /// <param name="bytesPerRow">Bytes per row.</param>
    /// <param name="dataProvider">Provider of data of image.</param>
    /// <param name="alphaInfo">Alpha info.</param>
    /// <param name="colorSpace">Color space.</param>
    /// <param name="renderingIntent">Rendering intent.</param>
    public CGImage(int width, int height, CGImagePixelFormatInfo pixelFormat, int bitsPerComponent, CGImageByteOrderInfo byteOrder, int bytesPerRow, CGImageAlphaInfo alphaInfo, CGDataProvider dataProvider, CGColorSpace colorSpace, CGColorRenderingIntent renderingIntent = CGColorRenderingIntent.Default) : this(Global.Run(() =>
    {
        if (width <= 0)
            throw new ArgumentOutOfRangeException(nameof(width));
        if (height <= 0)
            throw new ArgumentOutOfRangeException(nameof(height));
        if (pixelFormat == CGImagePixelFormatInfo.Packed)
        {
            switch (bitsPerComponent)
            {
                case 8:
                case 16:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(bitsPerComponent));
            }
        } else if (bitsPerComponent <= 0)
            throw new ArgumentOutOfRangeException(nameof(bitsPerComponent));
        var bitsPerPixel = pixelFormat switch
        {
            CGImagePixelFormatInfo.Packed => alphaInfo switch
            {
                CGImageAlphaInfo.AlphaFirst
                or CGImageAlphaInfo.AlphaLast
                or CGImageAlphaInfo.AlphaNoneSkipFirst
                or CGImageAlphaInfo.AlphaNoneSkipLast
                or CGImageAlphaInfo.AlphaPremultipliedFirst
                or CGImageAlphaInfo.AlphaPremultipliedLast => bitsPerComponent * 4,
                CGImageAlphaInfo.AlphaNone => bitsPerComponent * 3,
                CGImageAlphaInfo.AlphaOnly => bitsPerComponent,
                _ => throw new ArgumentException($"Unsupported alpha info: {alphaInfo}."),
            },
            CGImagePixelFormatInfo.RGB101010 => 32,
            CGImagePixelFormatInfo.RGB555
            or CGImagePixelFormatInfo.RGB565 => 16,
            CGImagePixelFormatInfo.RGBCIF10 => 32,
            _ => throw new ArgumentException($"Unsupported pixel format: {pixelFormat}."),
        };
        var minBytesPerRow = (bitsPerPixel * width).Let(it =>
        {
            if ((it & 0xf) != 0)
                return (it >> 3) + 1;
            return (it >> 3);
        });
        if (bytesPerRow < minBytesPerRow)
            throw new ArgumentOutOfRangeException(nameof(bytesPerRow));
        using var data = dataProvider.ToData();
        if (data.Length < bytesPerRow * height)
            throw new ArgumentException($"Insufficient data for image: {data.Length}, {bytesPerRow * height} required.");
        var bitmapInfo = (CGBitmapInfo)(((uint)alphaInfo & (uint)CGBitmapInfo.AlphaInfoMask) | ((uint)byteOrder & (uint)~CGBitmapInfo.AlphaInfoMask));
        return CGImageCreate((nuint)width, (nuint)height, (nuint)bitsPerComponent, (nuint)bitsPerPixel, (nuint)bytesPerRow, colorSpace.Handle, bitmapInfo, dataProvider.Handle, IntPtr.Zero, false, renderingIntent);
    }), false, true)
    { 
        this.dataProvider = dataProvider.Retain<CGDataProvider>();
        this.ownsDataProvider = true;
    }


    /// <summary>
    /// Initialize new <see cref="CGImage"/> instance.
    /// </summary>
    /// <param name="source">Source image to copy.</param>
    public CGImage(CGImage source) : this(Global.Run(() =>
    {
        source.VerifyReleased();
        return CGImageCreateCopy(source.Handle);
    }), false, true)
    { }


    /// <summary>
    /// Initialize new <see cref="CGImage"/> instance.
    /// </summary>
    /// <param name="source">Source image to copy.</param>
    /// <param name="colorSpace">Color space.</param>
    public CGImage(CGImage source, CGColorSpace colorSpace) : this(Global.Run(() =>
    {
        source.VerifyReleased();
        return CGImageCreateCopyWithColorSpace(source.Handle, colorSpace.Handle);
    }), false, true)
    { }


    // Constructor.
    CGImage(IntPtr image, bool ownsInstance) : this(image, true, ownsInstance)
    { }
    CGImage(IntPtr image, bool checkType, bool ownsInstance) : base(image, ownsInstance)
    { 
        if (checkType && image != IntPtr.Zero && this.TypeDescription != "CGImage")
            throw new ArgumentException("Type of instance is not CGImage.");
    }


    /// <summary>
    /// Get alpha information of image.
    /// </summary>
    public CGImageAlphaInfo AlphaInfo
    {
        get
        {
            this.VerifyReleased();
            return (CGImageAlphaInfo)(CGImageGetBitmapInfo(this.Handle) & CGBitmapInfo.AlphaInfoMask);
        }
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


    /// <inheritdoc/>
    public override void OnRelease()
    {
        if (this.ownsDataProvider)
            this.dataProvider?.Release();
        base.OnRelease();
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