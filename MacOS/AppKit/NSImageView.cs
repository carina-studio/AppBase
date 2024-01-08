using CarinaStudio.MacOS.ObjectiveC;
using System;
using System.Diagnostics.CodeAnalysis;

namespace CarinaStudio.MacOS.AppKit;

/// <summary>
/// NSImageView.
/// </summary>
public class NSImageView : NSControl
{
    // Static fields.
    static Property? ImageAlignmentProperty;
    static Property? ImageProperty;
    static Property? ImageScalingProperty;
    static readonly Class? NSImageViewClass;


    // Static initializer.
    static NSImageView()
    {
        if (Platform.IsNotMacOS)
            return;
        NSImageViewClass = Class.GetClass(nameof(NSImageView)).AsNonNull();
    }


    /// <summary>
    /// Initialize new <see cref="NSImageView"/> instance.
    /// </summary>
    /// <param name="frame">Frame.</param>
    [RequiresDynamicCode(CallConstructorRdcMessage)]
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
        get 
        {
            ImageProperty ??= NSImageViewClass!.GetProperty("image").AsNonNull();
            return this.GetNSObjectProperty<NSImage>(ImageProperty);
        }
        set 
        {
            ImageProperty ??= NSImageViewClass!.GetProperty("image").AsNonNull();
            this.SetProperty(ImageProperty, (NSObject?)value);
        }
    }


    /// <summary>
    /// Get or set alignment or image.
    /// </summary>
    public NSImageAlignment ImageAlignment
    {
        get 
        {
            ImageAlignmentProperty ??= NSImageViewClass!.GetProperty("imageAlignment").AsNonNull();
            return (NSImageAlignment)this.GetUInt32Property(ImageAlignmentProperty);
        }
        set 
        {
            ImageAlignmentProperty ??= NSImageViewClass!.GetProperty("imageAlignment").AsNonNull();
            this.SetProperty(ImageAlignmentProperty, (uint)value);
        }
    }


    /// <summary>
    /// Get or set scaling or image.
    /// </summary>
    public NSImageScaling ImageScaling
    {
        get 
        {
            ImageScalingProperty ??= NSImageViewClass!.GetProperty("imageScaling").AsNonNull();
            return (NSImageScaling)this.GetUInt32Property(ImageScalingProperty);
        }
        set 
        {
            ImageScalingProperty ??= NSImageViewClass!.GetProperty("imageScaling").AsNonNull();
            this.SetProperty(ImageScalingProperty, (uint)value);
        }
    }
}