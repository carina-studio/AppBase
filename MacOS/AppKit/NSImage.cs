using CarinaStudio.MacOS.CoreGraphics;
using CarinaStudio.MacOS.ObjectiveC;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace CarinaStudio.MacOS.AppKit;

/// <summary>
/// NSImage.
/// </summary>
public class NSImage : NSObject
{
    // Static fields.
    static Selector? InitByRefFileSelector;
    static Selector? InitWithCGImageSelector;
    static Selector? InitWithDataSelector;
    static Property? IsValidProperty;
    static readonly Class? NSImageClass;
    static Property? SizeProperty;


    // Static initializer.
    static NSImage()
    {
        if (Platform.IsNotMacOS)
            return;
        NSImageClass = Class.GetClass("NSImage").AsNonNull();
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
#if NET7_0_OR_GREATER
    [RequiresDynamicCode(CallMethodRdcMessage)]
#endif
    public static NSImage FromFile(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException($"Invalid file name: {fileName}.");
        InitByRefFileSelector ??= Selector.FromName("initByReferencingFile:");
        using var nsFileName = new NSString(fileName);
        var handle = SendMessage<IntPtr>(NSImageClass!.Allocate(), InitByRefFileSelector, nsFileName);
        return new(handle, true);
    }


    /// <summary>
    /// Create <see cref="NSImage"/> from <see cref="CGImage"/>.
    /// </summary>
    /// <param name="image"><see cref="CGImage"/>.</param>
    /// <returns><see cref="NSImage"/>.</returns>
#if NET7_0_OR_GREATER
    [RequiresDynamicCode(CallMethodRdcMessage)]
#endif
    public static NSImage FromCGImage(CGImage image)
    {
        if (image.IsReleased)
            throw new ObjectDisposedException(nameof(CGImage));
        InitWithCGImageSelector ??= Selector.FromName("initWithCGImage:size:");
        var handle = SendMessage<IntPtr>(NSImageClass!.Allocate(), InitWithCGImageSelector, image, new NSSize());
        return new(handle, true);
    }


    /// <summary>
    /// Create <see cref="NSImage"/> from <see cref="CGImage"/>.
    /// </summary>
    /// <param name="image"><see cref="CGImage"/>.</param>
    /// <param name="size">Size.</param>
    /// <returns><see cref="NSImage"/>.</returns>
#if NET7_0_OR_GREATER
    [RequiresDynamicCode(CallMethodRdcMessage)]
#endif
    public static NSImage FromCGImage(CGImage image, NSSize size)
    {
        if (image.IsReleased)
            throw new ObjectDisposedException(nameof(CGImage));
        if (size.Width <= 0 || size.Height <= 0 || !double.IsFinite(size.Width) || !double.IsFinite(size.Height))
            throw new ArgumentException($"Invalid size: {size}.");
        InitWithCGImageSelector ??= Selector.FromName("initWithCGImage:size:");
        var handle = SendMessage<IntPtr>(NSImageClass!.Allocate(), InitWithCGImageSelector, image, size);
        return new(handle, true);
    }


    /// <summary>
    /// Create <see cref="NSImage"/> from data.
    /// </summary>
    /// <param name="data">Data.</param>
    /// <returns><see cref="NSImage"/>.</returns>
#if NET7_0_OR_GREATER
    [RequiresDynamicCode(CallMethodRdcMessage)]
#endif
    public static NSImage FromData(CoreFoundation.CFData data)
    {
        if (data.IsReleased)
            throw new ObjectDisposedException(nameof(CoreFoundation.CFData));
        InitWithDataSelector ??= Selector.FromName("initWithData:");
        var handle = SendMessage<IntPtr>(NSImageClass!.Allocate(), InitWithDataSelector, data);
        if (handle != default)
            return new(handle, true);
        throw new ArgumentException($"Cannot create image from data.");
    }


    /// <summary>
    /// Create <see cref="NSImage"/> from data.
    /// </summary>
    /// <param name="stream">Stream.</param>
    /// <returns><see cref="NSImage"/>.</returns>
#if NET7_0_OR_GREATER
    [RequiresDynamicCode(CallMethodRdcMessage)]
#endif
    public static NSImage FromStream(Stream stream)
    {
        InitWithDataSelector ??= Selector.FromName("initWithData:");
        using var data = CoreFoundation.CFData.FromStream(stream);
        var handle = SendMessage<IntPtr>(NSImageClass!.Allocate(), InitWithDataSelector, data);
        if (handle != default)
            return new(handle, true);
        throw new ArgumentException($"Cannot create image from data of stream.");
    }


    /// <summary>
    /// Check whether it is possible to draw an image representation or not.
    /// </summary>
    public bool IsValid 
    { 
        get 
        {
            IsValidProperty ??= NSImageClass!.GetProperty("valid").AsNonNull();
            return !this.IsReleased && this.GetBooleanProperty(IsValidProperty); 
        }
    }


    /// <summary>
    /// Get or set size of image.
    /// </summary>
    public NSSize Size
    {
#if NET7_0_OR_GREATER
        [RequiresDynamicCode(GetPropertyRdcMessage)]
#endif
        get 
        {
            SizeProperty ??= NSImageClass!.GetProperty("size").AsNonNull();
            return this.GetProperty<NSSize>(SizeProperty);
        }
#if NET7_0_OR_GREATER
        [RequiresDynamicCode(SetPropertyRdcMessage)]
#endif
        set 
        {
            SizeProperty ??= NSImageClass!.GetProperty("size").AsNonNull();
            this.SetProperty(SizeProperty, value);
        }
    }
}