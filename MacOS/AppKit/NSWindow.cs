using CarinaStudio.MacOS.ObjectiveC;

namespace CarinaStudio.MacOS.AppKit;

/// <summary>
/// NSWindow.
/// </summary>
public class NSWindow : NSResponder
{
    // Static fields.
    static readonly Class? NSWindowClass;


    // Static initializer.
    static NSWindow()
    {
        if (Platform.IsNotMacOS)
            return;
        NSWindowClass = Class.GetClass("NSWindow").AsNonNull();
    }


    /// <summary>
    /// Initialize new <see cref="NSWindow"/> instance.
    /// </summary>
    /// <param name="instance">Handle of instance.</param>
    /// <param name="ownsInstance">True to owns the instance.</param>
    protected NSWindow(InstanceHolder instance, bool ownsInstance) : base(instance, ownsInstance)
    { 
        this.VerifyClass(NSWindowClass!);
    }
}