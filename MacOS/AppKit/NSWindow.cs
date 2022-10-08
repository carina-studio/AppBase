using CarinaStudio.MacOS.ObjectiveC;
using System;

namespace CarinaStudio.MacOS.AppKit;

/// <summary>
/// NSWindow.
/// </summary>
public class NSWindow : NSResponder
{
#pragma warning disable CS1591
    /// <summary>
    /// OrderingMode.
    /// </summary>
    public enum OrderingMode : int
    {
        Above = 1,
        Below = -1,
        Out = 0,
    }
#pragma warning restore CS1591


    // Static fields.
    static readonly Property? ContentViewProperty;
    static readonly Class? NSWindowClass;


    // Static initializer.
    static NSWindow()
    {
        if (Platform.IsNotMacOS)
            return;
        NSWindowClass = Class.GetClass("NSWindow").AsNonNull();
        ContentViewProperty = NSWindowClass.GetProperty("contentView");
    }


    /// <summary>
    /// Initialize new <see cref="NSWindow"/> instance.
    /// </summary>
    /// <param name="handle">Handle of instance.</param>
    /// <param name="verifyClass">True to verify whether instance is NSWindow or not.</param>
    /// <param name="ownsInstance">True to owns the instance.</param>
    protected NSWindow(IntPtr handle, bool verifyClass, bool ownsInstance) : base(handle, false, ownsInstance)
    {
        if (verifyClass)
            this.VerifyClass(NSWindowClass!);
    }
    

    /// <summary>
    /// Initialize new <see cref="NSWindow"/> instance.
    /// </summary>
    /// <param name="cls">Class of instance.</param>
    /// <param name="handle">Handle of instance.</param>
    /// <param name="ownsInstance">True to owns the instance.</param>
    protected NSWindow(Class cls, IntPtr handle, bool ownsInstance) : base(cls, handle, ownsInstance)
    { }
    

    // Constructor.
    NSWindow(IntPtr handle, bool ownsInstance) : this(handle, true, ownsInstance)
    { }


    /// <summary>
    /// Get or set content view of window.
    /// </summary>
    public NSView? ContentView
    {
        get => this.GetProperty<NSView>(ContentViewProperty!);
        set => this.SetProperty(ContentViewProperty!, value);
    }
}