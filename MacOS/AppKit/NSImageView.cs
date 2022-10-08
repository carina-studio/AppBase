using CarinaStudio.MacOS.ObjectiveC;
using System;

namespace CarinaStudio.MacOS.AppKit;

/// <summary>
/// NSImageView.
/// </summary>
public class NSImageView : NSControl
{
    // Static fields.
    static readonly Property? ImageAlignmentProperty;
    static readonly Property? ImageProperty;
    static readonly Property? ImageScalingProperty;
    static readonly Class? NSImageViewClass;


    // Static initializer.
    static NSImageView()
    {
        if (Platform.IsNotMacOS)
            return;
        NSImageViewClass = Class.GetClass(nameof(NSImageView)).AsNonNull();
        ImageAlignmentProperty = NSImageViewClass.GetProperty("imageAlignment");
        ImageProperty = NSImageViewClass.GetProperty("image");
        ImageScalingProperty = NSImageViewClass.GetProperty("imageScaling");
    }


    /// <summary>
    /// Initialize new <see cref="NSImageView"/> instance.
    /// </summary>
    /// <param name="frame">Frame.</param>
    public NSImageView(NSRect frame) : base(NSImageViewClass!.Allocate(), frame)
    { }


    /// <summary>
    /// Initialize new <see cref="NSImageView"/> instance.
    /// </summary>
    /// <param name="handle">Handle of instance.</param>
    /// <param name="verifyClass">True to verify whether instance is NSImageView or not.</param>
    /// <param name="ownsInstance">True to owns the instance.</param>
    protected NSImageView(IntPtr handle, bool verifyClass, bool ownsInstance) : base(handle, false, ownsInstance)
    {
        if (verifyClass)
            this.VerifyClass(NSImageViewClass!);
    }
    

    /// <summary>
    /// Initialize new <see cref="NSImageView"/> instance.
    /// </summary>
    /// <param name="cls">Class of instance.</param>
    /// <param name="handle">Handle of instance.</param>
    /// <param name="ownsInstance">True to owns the instance.</param>
    protected NSImageView(Class cls, IntPtr handle, bool ownsInstance) : base(cls, handle, ownsInstance)
    { }


    // Constructor.
    NSImageView(IntPtr handle, bool ownsInstance) : this(handle, true, ownsInstance)
    { }
    

    /// <summary>
    /// Get or set image.
    /// </summary>
    public NSImage? Image
    {
        get => this.GetProperty<NSImage>(ImageProperty!);
        set => this.SetProperty(ImageProperty!, value);
    }


    /// <summary>
    /// Get or set alignment or image.
    /// </summary>
    public NSImageAlignment ImageAlignment
    {
        get => this.GetProperty<NSImageAlignment>(ImageAlignmentProperty!);
        set => this.SetProperty(ImageAlignmentProperty!, value);
    }


    /// <summary>
    /// Get or set scaling or image.
    /// </summary>
    public NSImageScaling ImageScaling
    {
        get => this.GetProperty<NSImageScaling>(ImageScalingProperty!);
        set => this.SetProperty(ImageScalingProperty!, value);
    }
}