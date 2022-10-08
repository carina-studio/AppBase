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
    /// BackingStoreType.
    /// </summary>
    public enum BackingStoreType : uint
    {
        [Obsolete]
        Retained = 0,
        [Obsolete]
        NonRetained = 1,
        Buffered = 2,
    }


    /// <summary>
    /// OrderingMode.
    /// </summary>
    public enum OrderingMode : int
    {
        Above = 1,
        Below = -1,
        Out = 0,
    }


    /// <summary>
    /// StyleMask.
    /// </summary>
    [Flags]
    public enum StyleMask : uint
    {
        Borderless = 0,
        Titled = 0x1,
        Closable = 0x1 << 2,
        Resizable = 0x1 << 3,
        UtilityWindow = 0x1 << 4,
        DocModalWindow = 0x1 << 6,
        NonactivatingPanel = 0x1 << 7,
        TexturedBackgroundWindow = 0x1 << 8,
        UnifiedTitleAndToolbar = 0x1 << 12,
        HUDWindow = 0x1 << 13,
        FullScreen = 0x1 << 14,
        FullSizeContentView = 0x1 << 15,
    }
#pragma warning restore CS1591


    // Static fields.
    static readonly Selector? CloseSelector;
    static readonly Property? ContentViewProperty;
    static readonly Property? DelegateProperty;
    static readonly Selector? InitWithRectAndScreenSelector;
    static readonly Selector? InitWithRectSelector;
    static readonly Selector? MakeKeyAndOrderFrontSelector;
    static readonly Class? NSWindowClass;
    static readonly Property? SubtitleProperty;
    static readonly Property? TitleProperty;


    // Static initializer.
    static NSWindow()
    {
        if (Platform.IsNotMacOS)
            return;
        NSWindowClass = Class.GetClass("NSWindow").AsNonNull();
        CloseSelector = Selector.FromName("close");
        ContentViewProperty = NSWindowClass.GetProperty("contentView");
        DelegateProperty = NSWindowClass.GetProperty("delegate");
        InitWithRectAndScreenSelector = Selector.FromName("initWithContentRect:styleMask:backing:defer:screen:");
        InitWithRectSelector = Selector.FromName("initWithContentRect:styleMask:backing:defer:");
        MakeKeyAndOrderFrontSelector = Selector.FromName("makeKeyAndOrderFront:");
        SubtitleProperty = NSWindowClass.GetProperty("subtitle");
        TitleProperty = NSWindowClass.GetProperty("title");
    }


    /// <summary>
    /// Initialize new <see cref="NSWindow"/> instance.
    /// </summary>
    /// <param name="contentRect">Origin and size of the window’s content area in screen coordinates.</param>
    /// <param name="style">Style.</param>
    /// <param name="backingStoreType">How the drawing done in the window is buffered by the window device</param>
    /// <param name="defer">True to create the window device until the window is moved onscreen.</param>
    public NSWindow(NSRect contentRect, StyleMask style, BackingStoreType backingStoreType, bool defer) : this(Initialize(NSWindowClass!.Allocate(), contentRect, style, backingStoreType, defer), true)
    { }


    /// <summary>
    /// Initialize new <see cref="NSWindow"/> instance.
    /// </summary>
    /// <param name="contentRect">Origin and size of the window’s content area in screen coordinates.</param>
    /// <param name="style">Style.</param>
    /// <param name="backingStoreType">How the drawing done in the window is buffered by the window device</param>
    /// <param name="defer">True to create the window device until the window is moved onscreen.</param>
    /// <param name="screen">The screen on which the window is positioned.</param>
    public NSWindow(NSRect contentRect, StyleMask style, BackingStoreType backingStoreType, bool defer, NSObject? screen) : this(Initialize(NSWindowClass!.Allocate(), contentRect, style, backingStoreType, defer, screen), true)
    { }


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
    /// Close the window.
    /// </summary>
    public void Close() =>
        this.SendMessage(CloseSelector!);


    /// <summary>
    /// Get or set content view of window.
    /// </summary>
    public NSView? ContentView
    {
        get => this.GetProperty<NSView>(ContentViewProperty!);
        set => this.SetProperty(ContentViewProperty!, value);
    }


    /// <summary>
    /// Get or set object which conforms to NSWindowDelegate protocol to receive call-back from window.
    /// </summary>
    public NSObject? Delegate
    {
        get => this.GetProperty<NSObject>(DelegateProperty!);
        set => this.SetProperty(DelegateProperty!, value);
    }


    /// <summary>
    /// Initialize allocated instance.
    /// </summary>
    /// <returns>Handle of initialized instance</returns>
    protected static IntPtr Initialize(IntPtr obj, NSRect contentRect, StyleMask style, BackingStoreType backingStoreType, bool defer) =>
        NSObject.SendMessage<IntPtr>(obj, InitWithRectSelector!, contentRect, style, backingStoreType, defer);
    

    /// <summary>
    /// Initialize allocated instance.
    /// </summary>
    /// <returns>Handle of initialized instance</returns>
    protected static IntPtr Initialize(IntPtr obj, NSRect contentRect, StyleMask style, BackingStoreType backingStoreType, bool defer, NSObject? screen) =>
        NSObject.SendMessage<IntPtr>(obj, InitWithRectAndScreenSelector!, contentRect, style, backingStoreType, defer, screen);
    

    /// <summary>
    /// Show the window.
    /// </summary>
    public void MakeKeyAndOrderFront() =>
        this.SendMessage(MakeKeyAndOrderFrontSelector!, this);
    

    /// <summary>
    /// Get or set subtitle of window.
    /// </summary>
    /// <value></value>
    public string Subtitle
    {
        get
        {
            var handle = this.GetProperty<IntPtr>(SubtitleProperty!);
            if (handle == default)
                return "";
            return NSObject.FromHandle<NSString>(handle, false)!.ToString();
        }
        set
        {
            this.VerifyReleased();
            using var s = new NSString(value ?? "");
            this.SetProperty(SubtitleProperty!, s);
        }
    }


    /// <summary>
    /// Get or set title of window.
    /// </summary>
    /// <value></value>
    public string Title
    {
        get
        {
            var handle = this.GetProperty<IntPtr>(TitleProperty!);
            if (handle == default)
                return "";
            return NSObject.FromHandle<NSString>(handle, false)!.ToString();
        }
        set
        {
            this.VerifyReleased();
            using var s = new NSString(value ?? "");
            this.SetProperty(TitleProperty!, s);
        }
    }
}