using CarinaStudio.MacOS.ObjectiveC;
using System;

namespace CarinaStudio.MacOS.AppKit;

/// <summary>
/// NSTrackingArea.
/// </summary>
public class NSTrackingArea : NSObject
{
#pragma warning disable CS1591
    /// <summary>
    /// Options.
    /// </summary>
    [Flags]
    public enum Options : uint
    {
        MouseEnteredAndExited = 0x1,
        MouseMoved = 0x2,
        CursorUpdate = 0x4,
        ActiveWhenFirstResponder = 0x10,
        ActiveInKeyWindow = 0x20,
        ActiveInActiveApp = 0x40,
        ActiveAlways = 0x80,
        AssumeInside = 0x100,
        InVisibleRect = 0x200,
        EnabledDuringMouseDrag = 0x400,
    }
#pragma warning restore CS1591


    // Static fields.
    static Selector? InitWithRectSelector;
    static readonly Class? NSTrackingAreaClass;
    static Property? OwnerProperty;
    static Property? RectProperty;


    // Static initializer.
    static NSTrackingArea()
    {
        if (Platform.IsNotMacOS)
            return;
        NSTrackingAreaClass = Class.GetClass("NSTrackingArea").AsNonNull();
    }


    /// <summary>
    /// Initialize new <see cref="NSTrackingArea"/> instance.
    /// </summary>
    /// <param name="rect">Rectangle of tracking area in coordinate space of owner view.</param>
    /// <param name="options">Options of behavior of tracking area.</param>
    /// <param name="owner">Object to receive mouse-tracking events.</param>
    public NSTrackingArea(NSRect rect, Options options, NSObject? owner) : this(Initialize(NSTrackingAreaClass!.Allocate(), rect, options, owner), true, true)
    { }


    /// <summary>
    /// Initialize new <see cref="NSTrackingArea"/> instance.
    /// </summary>
    /// <param name="handle">Handle of instance.</param>
    /// <param name="verifyClass">True to verify whether instance is NSTrackingArea or not.</param>
    /// <param name="ownsInstance">True to owns the instance.</param>
    protected NSTrackingArea(IntPtr handle, bool verifyClass, bool ownsInstance) : base(handle, ownsInstance)
    {
        if (verifyClass)
            this.VerifyClass(NSTrackingAreaClass!);
    }


    /// <summary>
    /// Initialize new <see cref="NSTrackingArea"/> instance.
    /// </summary>
    /// <param name="cls">Class of instance.</param>
    /// <param name="handle">Handle of instance.</param>
    /// <param name="ownsInstance">True to owns the instance.</param>
    protected NSTrackingArea(Class cls, IntPtr handle, bool ownsInstance) : base(cls, handle, ownsInstance)
    { }


    // Constructor.
    NSTrackingArea(IntPtr handle, bool ownsInstance) : this(handle, true, ownsInstance)
    { }


    // Initialize allocated instance.
    static IntPtr Initialize(IntPtr obj, NSRect rect, Options options, NSObject? owner)
    {
        InitWithRectSelector ??= Selector.FromName("initWithRect:options:owner:userInfo:");
        return SendMessage<IntPtr>(obj, InitWithRectSelector, rect, options, owner, null);
    }


    /// <summary>
    /// Get object to receive mouse-tracking events.
    /// </summary>
    public NSObject? Owner
    {
        get
        {
            OwnerProperty ??= NSTrackingAreaClass!.GetProperty("owner").AsNonNull();
            return this.GetNSObjectProperty<NSObject>(OwnerProperty);
        }
    }


    /// <summary>
    /// Get rectangle of tracking area in coordinate space of owner view.
    /// </summary>
    public NSRect Rect
    {
        get
        {
            RectProperty ??= NSTrackingAreaClass!.GetProperty("rect").AsNonNull();
            return this.GetProperty<NSRect>(RectProperty);
        }
    }
}
