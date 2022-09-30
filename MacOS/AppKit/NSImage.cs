using CarinaStudio.MacOS.ObjectiveC;
using System;

namespace CarinaStudio.MacOS.AppKit;

/// <summary>
/// NSImage.
/// </summary>
public class NSImage : NSObject
{
    // Static fields.
    static readonly Selector? InitByRefFileSelector;
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
        IsValidProperty = NSImageClass.GetProperty("valid");
        SizeProperty = NSImageClass.GetProperty("size");
    }


    // Constructor.
    NSImage(InstanceHolder instance, bool ownsInstance) : base(instance, ownsInstance)
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
        return new NSImage(new InstanceHolder(handle), true);
    }


    /// <summary>
    /// Check whether it is possible to draw an image representation or not.
    /// </summary>
    public bool IsValid { get => !this.IsDisposed && this.GetProperty<bool>(IsValidProperty!); }


    /// <summary>
    /// Get or set size of image.
    /// </summary>
    public NSSize Size
    {
        get => this.GetProperty<NSSize>(SizeProperty!);
        set => this.SetProperty<NSSize>(SizeProperty!, value);
    }
}