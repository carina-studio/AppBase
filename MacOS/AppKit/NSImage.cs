using CarinaStudio.MacOS.CoreGraphics;
using CarinaStudio.MacOS.ObjectiveC;
using System;
using System.IO;

namespace CarinaStudio.MacOS.AppKit;

/// <summary>
/// NSImage.
/// </summary>
public class NSImage : NSObject
{
    // Static fields.
    static readonly Selector? InitByRefFileSelector;
    static readonly Selector? InitWithCGImageSelector;
    static readonly Selector? InitWithDataSelector;
    static readonly Property? IsValidProperty;
    static readonly Class? NSImageClass;
    static readonly Property? SizeProperty;


    // Static initializer.
    static NSImage()
    {
        if (Platform.IsNotMacOS)
            return;
        NSImageClass = Class.GetClass("NSImage").AsNonNull();
        InitByRefFileSelector = Selector.FromName("initByReferencingFile:");
        InitWithCGImageSelector = Selector.FromName("initWithCGImage:size:");
        InitWithDataSelector = Selector.FromName("initWithData:");
        IsValidProperty = NSImageClass.GetProperty("valid");
        SizeProperty = NSImageClass.GetProperty("size");
    }


    // Constructor.
    NSImage(IntPtr handle, bool ownsInstance) : base(handle, ownsInstance) =>
        this.VerifyClass(NSImageClass!);
    NSImage(Class cls, IntPtr handle, bool ownsInstance) : base(cls, handle, ownsInstance)
    { }


    /// <summary>
    /// Create <see cref="NSImage"/> from file.
    /// </summary>
    /// <param name="fileName">File name.</param>
    /// <returns><see cref="NSImage"/>.</returns>
    public static NSImage FromFile(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException($"Invalid file name: {fileName}.");
        using var nsFileName = new NSString(fileName);
        var handle = NSObject.SendMessage<IntPtr>(NSImageClass!.Allocate(), InitByRefFileSelector!, nsFileName);
        return new(handle, true);
    }


    /// <summary>
    /// Create <see cref="NSImage"/> from <see cref="CGImage"/>.
    /// </summary>
    /// <param name="image"><see cref="CGImage"/>.</param>
    /// <returns><see cref="NSImage"/>.</returns>
    public static NSImage FromCGImage(CGImage image)
    {
        if (image.IsReleased)
            throw new ObjectDisposedException(nameof(CGImage));
        var handle = NSObject.SendMessage<IntPtr>(NSImageClass!.Allocate(), InitWithCGImageSelector!, image, new NSSize());
        return new(handle, true);
    }


    /// <summary>
    /// Create <see cref="NSImage"/> from <see cref="CGImage"/>.
    /// </summary>
    /// <param name="image"><see cref="CGImage"/>.</param>
    /// <param name="size">Size.</param>
    /// <returns><see cref="NSImage"/>.</returns>
    public static NSImage FromCGImage(CGImage image, NSSize size)
    {
        if (image.IsReleased)
            throw new ObjectDisposedException(nameof(CGImage));
        if (size.Width <= 0 || size.Height <= 0 || !double.IsFinite(size.Width) || !double.IsFinite(size.Height))
            throw new ArgumentException($"Invalid size: {size}.");
        var handle = NSObject.SendMessage<IntPtr>(NSImageClass!.Allocate(), InitWithCGImageSelector!, image, size);
        return new(handle, true);
    }


    /// <summary>
    /// Create <see cref="NSImage"/> from data.
    /// </summary>
    /// <param name="data">Data.</param>
    /// <returns><see cref="NSImage"/>.</returns>
    public static NSImage FromData(CoreFoundation.CFData data)
    {
        if (data.IsReleased)
            throw new ObjectDisposedException(nameof(CoreFoundation.CFData));
        var handle = NSObject.SendMessage<IntPtr>(NSImageClass!.Allocate(), InitWithDataSelector!, data);
        if (handle != default)
            return new(handle, true);
        throw new ArgumentException($"Cannot create image from data.");
    }


    /// <summary>
    /// Create <see cref="NSImage"/> from data.
    /// </summary>
    /// <param name="stream">Stream.</param>
    /// <returns><see cref="NSImage"/>.</returns>
    public static NSImage FromStream(Stream stream)
    {
        using var data = CoreFoundation.CFData.FromStream(stream);
        var handle = NSObject.SendMessage<IntPtr>(NSImageClass!.Allocate(), InitWithDataSelector!, data);
        if (handle != default)
            return new(handle, true);
        throw new ArgumentException($"Cannot create image from data of stream.");
    }


    /// <summary>
    /// Check whether it is possible to draw an image representation or not.
    /// </summary>
    public bool IsValid { get => !this.IsReleased && this.GetProperty<bool>(IsValidProperty!); }


    /// <summary>
    /// Get or set size of image.
    /// </summary>
    public NSSize Size
    {
        get => this.GetProperty<NSSize>(SizeProperty!);
        set => this.SetProperty<NSSize>(SizeProperty!, value);
    }
}