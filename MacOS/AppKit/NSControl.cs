using CarinaStudio.MacOS.ObjectiveC;
using System;

namespace CarinaStudio.MacOS.AppKit;

/// <summary>
/// NSControl.
/// </summary>
public class NSControl : NSView
{
    // Static fields.
    static readonly Property? IsEnabledProperty;
    static readonly Class? NSControlClass;


    // Static initializer.
    static NSControl()
    {
        if (Platform.IsNotMacOS)
            return;
        NSControlClass = Class.GetClass(nameof(NSControl)).AsNonNull();
        IsEnabledProperty = NSControlClass.GetProperty("enabled");
    }


    /// <summary>
    /// Initialize new <see cref="NSControl"/> instance.
    /// </summary>
    /// <param name="handle">Handle of allocated instance.</param>
    /// <param name="frame">Frame.</param>
    protected NSControl(IntPtr handle, NSRect frame) : base(handle, frame)
    { }


    /// <summary>
    /// Initialize new <see cref="NSControl"/> instance.
    /// </summary>
    /// <param name="handle">Handle of instance.</param>
    /// <param name="verifyClass">True to verify whether instance is NSControl or not.</param>
    /// <param name="ownsInstance">True to owns the instance.</param>
    protected NSControl(IntPtr handle, bool verifyClass, bool ownsInstance) : base(handle, false, ownsInstance)
    {
        if (verifyClass)
            this.VerifyClass(NSControlClass!);
    }


    /// <summary>
    /// Initialize new <see cref="NSControl"/> instance.
    /// </summary>
    /// <param name="cls">Class of instance.</param>
    /// <param name="handle">Handle of instance.</param>
    /// <param name="ownsInstance">True to owns the instance.</param>
    protected NSControl(Class cls, IntPtr handle, bool ownsInstance) : base(cls, handle, ownsInstance)
    { }
    

    /// <summary>
    /// Get or set whether control is enabled or not.
    /// </summary>
    public bool IsEnabled
    {
        get => this.GetProperty<bool>(IsEnabledProperty!);
        set => this.SetProperty(IsEnabledProperty!, value);
    }
}