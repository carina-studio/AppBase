using CarinaStudio.MacOS.CoreFoundation;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace CarinaStudio.MacOS.CoreGraphics;

/// <summary>
/// CGImage.
/// </summary>
public unsafe class CGImage : CFObject
{
    // Native symbols.
    static readonly delegate*<nuint, nuint, nuint, nuint, nuint, IntPtr, CGBitmapInfo, IntPtr, IntPtr, bool, CGColorRenderingIntent, IntPtr> CGImageCreate;
    static readonly delegate*<IntPtr, IntPtr> CGImageCreateCopy;
    static readonly delegate*<IntPtr, IntPtr, IntPtr> CGImageCreateCopyWithColorSpace;
    static readonly delegate*<IntPtr, CGImageAlphaInfo> CGImageGetAlphaInfo;
    static readonly delegate*<IntPtr, CGBitmapInfo> CGImageGetBitmapInfo;
    static readonly delegate*<IntPtr, nuint> CGImageGetBitsPerPixel;
    static readonly delegate*<IntPtr, CGImageByteOrderInfo> CGImageGetByteOrderInfo;
    static readonly delegate*<IntPtr, nuint> CGImageGetBytesPerRow;
    static readonly delegate*<IntPtr, IntPtr> CGImageGetColorSpace;
    static readonly delegate*<IntPtr, IntPtr> CGImageGetDataProvider;
    static readonly delegate*<IntPtr, nuint> CGImageGetHeight;
    static readonly delegate*<IntPtr, CGImagePixelFormatInfo> CGImageGetPixelFormatInfo;
    static readonly delegate*<IntPtr, nuint> CGImageGetWidth;


    // Static fields.
    [ThreadStatic]
    static CGDataProvider? tempDataProvider;


    // Fields.
    CGDataProvider? dataProvider;
    readonly bool ownsDataProvider;
    
    
    // Static constructor.
    static CGImage()
    {
        if (Platform.IsNotMacOS)
            return;
        var libHandle = NativeLibraryHandles.CoreGraphics;
        CGImageCreate = (delegate*<nuint, nuint, nuint, nuint, nuint, IntPtr, CGBitmapInfo, IntPtr, IntPtr, bool, CGColorRenderingIntent, IntPtr>)NativeLibrary.GetExport(libHandle, nameof(CGImageCreate));
        CGImageCreateCopy = (delegate*<IntPtr, IntPtr>)NativeLibrary.GetExport(libHandle, nameof(CGImageCreateCopy));
        CGImageCreateCopyWithColorSpace = (delegate*<IntPtr, IntPtr, IntPtr>)NativeLibrary.GetExport(libHandle, nameof(CGImageCreateCopyWithColorSpace));
        CGImageGetAlphaInfo = (delegate*<IntPtr, CGImageAlphaInfo>)NativeLibrary.GetExport(libHandle, nameof(CGImageGetAlphaInfo));
        CGImageGetBitmapInfo = (delegate*<IntPtr, CGBitmapInfo>)NativeLibrary.GetExport(libHandle, nameof(CGImageGetBitmapInfo));
        CGImageGetBitsPerPixel = (delegate*<IntPtr, nuint>)NativeLibrary.GetExport(libHandle, nameof(CGImageGetBitsPerPixel));
        CGImageGetByteOrderInfo = (delegate*<IntPtr, CGImageByteOrderInfo>)NativeLibrary.GetExport(libHandle, nameof(CGImageGetByteOrderInfo));
        CGImageGetBytesPerRow = (delegate*<IntPtr, nuint>)NativeLibrary.GetExport(libHandle, nameof(CGImageGetBytesPerRow));
        CGImageGetColorSpace = (delegate*<IntPtr, IntPtr>)NativeLibrary.GetExport(libHandle, nameof(CGImageGetColorSpace));
        CGImageGetDataProvider = (delegate*<IntPtr, IntPtr>)NativeLibrary.GetExport(libHandle, nameof(CGImageGetDataProvider));
        CGImageGetHeight = (delegate*<IntPtr, nuint>)NativeLibrary.GetExport(libHandle, nameof(CGImageGetHeight));
        CGImageGetPixelFormatInfo = (delegate*<IntPtr, CGImagePixelFormatInfo>)NativeLibrary.GetExport(libHandle, nameof(CGImageGetPixelFormatInfo));
        CGImageGetWidth = (delegate*<IntPtr, nuint>)NativeLibrary.GetExport(libHandle, nameof(CGImageGetWidth));
    }


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
            return handle != IntPtr.Zero ? FromHandle<CGColorSpace>(handle, false) : null;
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
            return this.dataProvider ?? FromHandle<CGDataProvider>(CGImageGetDataProvider(this.Handle), false).Also(it =>
            {
                this.dataProvider = it;
            }).AsNonNull();
        }
    }


    /// <summary>
    /// Create <see cref="CGImage"/> from data.
    /// </summary>
    /// <param name="data">Data.</param>
    /// <returns><see cref="CGImage"/>, or Null if error occurred.</returns>
    public static CGImage? FromData(CFData data)
    {
        using var imageSource = ImageIO.CGImageSource.FromData(data);
        return imageSource.CreateImage();
    }


    /// <summary>
    /// Create <see cref="CGImage"/> from file.
    /// </summary>
    /// <param name="fileName">File name.</param>
    /// <returns><see cref="CGImage"/>, or Null if error occurred.</returns>
    public static CGImage? FromFile(string fileName)
    {
        using var imageSource = ImageIO.CGImageSource.FromFile(fileName);
        return imageSource.CreateImage();
    }


    /// <summary>
    /// Create <see cref="CGImage"/> from data.
    /// </summary>
    /// <param name="stream">Stream.</param>
    /// <returns><see cref="CGImage"/>, or Null if error occurred.</returns>
    public static CGImage? FromStream(Stream stream)
    {
        using var data = CFData.FromStream(stream);
        return FromData(data);
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