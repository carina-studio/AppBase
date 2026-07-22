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
    /// Standard title bar buttons.
    /// </summary>
    public enum ButtonType : uint
    {
        CloseButton = 0,
        MiniaturizeButton = 1,
        ZoomButton = 2,
        ToolbarButton = 3,
        DocumentIconButton = 4,
        DocumentVersionsButton = 6,
    }


    /// <summary>
    /// OrderingMode.
    /// </summary>
    public enum OrderingMode
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


    /// <summary>
    /// Appearance of the window’s title bar area.
    /// </summary>
    public enum TitleAppearance
    {
        Visible = 0,
        Hidden = 1,
    }
#pragma warning restore CS1591


    // Static fields.
    static Property? AppearanceProperty;
    static Selector? CloseSelector;
    static Property? ContentViewProperty;
    static Property? DelegateProperty;
    static NSString? DidEnterFullScreenNotificationName;
    static NSString? DidExitFullScreenNotificationName;
    static Selector? InitWithRectAndScreenSelector;
    static Selector? InitWithRectSelector;
    static Selector? MakeKeyAndOrderFrontSelector;
    static readonly Class? NSWindowClass;
    static Selector? OrderBackSelector;
    static Selector? OrderFrontRegardlessSelector;
    static Selector? OrderFrontSelector;
    static Selector? OrderOutSelector;
    static Selector? OrderRelativeToSelector;
    static Selector? SetStyleMaskSelector;
    static Property? SubtitleProperty;
    static Selector? StandardWindowButtonForStyleSelector;
    static Selector? StandardWindowButtonSelector;
    static Selector? StyleMaskSelector;
    static Property? TitleProperty;
    static Property? TitleVisibilityProperty;
    static Selector? ToggleToolbarShownSelector;
    static Property? ToolbarProperty;
    static Property? ToolbarStyleProperty;
    static NSString? WillEnterFullScreenNotificationName;
    static NSString? WillExitFullScreenNotificationName;


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
    /// Get or set appearance of window.
    /// </summary>
    public NSAppearance? Appearance
    {
        get 
        {
            AppearanceProperty ??= NSWindowClass!.GetProperty("appearance").AsNonNull();
            return this.GetNSObjectProperty<NSAppearance>(AppearanceProperty);
        }
        set 
        {
            AppearanceProperty ??= NSWindowClass!.GetProperty("appearance").AsNonNull();
            this.SetProperty(AppearanceProperty, (NSObject?)value);
        }
    }


    /// <summary>
    /// Close the window.
    /// </summary>
    public void Close()
    {
        CloseSelector ??= Selector.FromName("close");
        this.SendMessage(CloseSelector);
    }


    /// <summary>
    /// Get or set content view of window.
    /// </summary>
    public NSView? ContentView
    {
        get 
        {
            ContentViewProperty ??= NSWindowClass!.GetProperty("contentView").AsNonNull();
            return this.GetNSObjectProperty<NSView>(ContentViewProperty);
        }
        set 
        {
            ContentViewProperty ??= NSWindowClass!.GetProperty("contentView").AsNonNull();
            this.SetProperty(ContentViewProperty, (NSObject?)value);
        }
    }


    /// <summary>
    /// Get or set object which conforms to NSWindowDelegate protocol to receive call-back from window.
    /// </summary>
    public NSObject? Delegate
    {
        get
        {
            DelegateProperty ??= NSWindowClass!.GetProperty("delegate").AsNonNull();
            return this.GetNSObjectProperty<NSObject>(DelegateProperty);
        }
        set
        {
            DelegateProperty ??= NSWindowClass!.GetProperty("delegate").AsNonNull();
            this.SetProperty(DelegateProperty, value);
        }
    }


    /// <summary>
    /// Get name of notification posted when window entered full-screen mode.
    /// </summary>
    public static NSString DidEnterFullScreenNotification
    {
        get
        {
            DidEnterFullScreenNotificationName ??= new NSString("NSWindowDidEnterFullScreenNotification");
            return DidEnterFullScreenNotificationName;
        }
    }


    /// <summary>
    /// Get name of notification posted when window exited full-screen mode.
    /// </summary>
    public static NSString DidExitFullScreenNotification
    {
        get
        {
            DidExitFullScreenNotificationName ??= new NSString("NSWindowDidExitFullScreenNotification");
            return DidExitFullScreenNotificationName;
        }
    }


    /// <summary>
    /// Initialize allocated instance.
    /// </summary>
    /// <returns>Handle of initialized instance</returns>
    protected static IntPtr Initialize(IntPtr obj, NSRect contentRect, StyleMask style, BackingStoreType backingStoreType, bool defer)
    {
        InitWithRectSelector ??= Selector.FromName("initWithContentRect:styleMask:backing:defer:");
        return SendMessage<IntPtr>(obj, InitWithRectSelector, contentRect, style, backingStoreType, defer);
    }
    

    /// <summary>
    /// Initialize allocated instance.
    /// </summary>
    /// <returns>Handle of initialized instance</returns>
    protected static IntPtr Initialize(IntPtr obj, NSRect contentRect, StyleMask style, BackingStoreType backingStoreType, bool defer, NSObject? screen)
    {
        InitWithRectAndScreenSelector ??= Selector.FromName("initWithContentRect:styleMask:backing:defer:screen:");
        return SendMessage<IntPtr>(obj, InitWithRectAndScreenSelector, contentRect, style, backingStoreType, defer, screen);
    }
    

    /// <summary>
    /// Show the window.
    /// </summary>
    public void MakeKeyAndOrderFront()
    {
        MakeKeyAndOrderFrontSelector ??= Selector.FromName("makeKeyAndOrderFront:");
        this.SendMessage(MakeKeyAndOrderFrontSelector, this);
    }


    /// <summary>
    /// Move the window to the back of its level in the screen list.
    /// </summary>
    public void Order(OrderingMode place, int otherWin)
    {
        OrderRelativeToSelector ??= Selector.FromName("order:relativeTo:");
        this.SendMessage(OrderRelativeToSelector, place, otherWin);
    }


    /// <summary>
    /// Move the window to the back of its level in the screen list.
    /// </summary>
    public void OrderBack(NSObject? sender = null)
    {
        OrderBackSelector ??= Selector.FromName("orderBack:");
        this.SendMessage(OrderBackSelector, sender);
    }


    /// <summary>
    /// Move the window to the front of its level in the screen list.
    /// </summary>
    public void OrderFront(NSObject? sender = null)
    {
        OrderFrontSelector ??= Selector.FromName("orderFront:");
        this.SendMessage(OrderFrontSelector, sender);
    }


    /// <summary>
    /// Move the window to the front of its level, even if its application isn’t active, without changing either the key window or the main window.
    /// </summary>
    public void OrderFrontRegardless()
    {
        OrderFrontRegardlessSelector ??= Selector.FromName("orderFrontRegardless");
        this.SendMessage(OrderFrontRegardlessSelector);
    }


    /// <summary>
    /// Remove the window from the screen list.
    /// </summary>
    public void OrderOut(NSObject? sender = null)
    {
        OrderOutSelector ??= Selector.FromName("orderOut:");
        this.SendMessage(OrderOutSelector, sender);
    }


    /// <summary>
    /// Get standard button of window.
    /// </summary>
    /// <param name="button">Type of button.</param>
    /// <returns><see cref="NSControl"/> of button.</returns>
    public NSControl? StandardWindowButton(ButtonType button)
    {
        StandardWindowButtonSelector ??= Selector.FromName("standardWindowButton:");
        return this.SendMessage<NSControl?>(StandardWindowButtonSelector, button);
    }


    /// <summary>
    /// Create a new standard button for window with given style.
    /// </summary>
    /// <param name="button">Type of button.</param>
    /// <param name="style">Style of window.</param>
    /// <returns><see cref="NSControl"/> of button.</returns>
    public static NSControl? StandardWindowButton(ButtonType button, StyleMask style)
    {
        StandardWindowButtonForStyleSelector ??= Selector.FromName("standardWindowButton:forStyleMask:");
        return SendMessage<NSControl?>(NSWindowClass!.Handle, StandardWindowButtonForStyleSelector, button, style);
    }


    /// <summary>
    /// Get or set style of window.
    /// </summary>
    public StyleMask Style
    {
        get
        {
            StyleMaskSelector ??= Selector.FromName("styleMask");
            return this.SendMessage<StyleMask>(StyleMaskSelector);
        }
        set
        {
            SetStyleMaskSelector ??= Selector.FromName("setStyleMask:");
            this.SendMessage(SetStyleMaskSelector, value);
        }
    }


    /// <summary>
    /// Get or set subtitle of window.
    /// </summary>
    /// <value></value>
    public string Subtitle
    {
        get
        {
            SubtitleProperty ??= NSWindowClass!.GetProperty("subtitle").AsNonNull();
            var handle = this.GetIntPtrProperty(SubtitleProperty);
            if (handle == default)
                return "";
            return FromHandle<NSString>(handle, false)!.ToString();
        }
        set
        {
            this.VerifyReleased();
            SubtitleProperty ??= NSWindowClass!.GetProperty("subtitle").AsNonNull();
            using var s = new NSString(value);
            this.SetProperty(SubtitleProperty, s);
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
            TitleProperty ??= NSWindowClass!.GetProperty("title").AsNonNull();
            var handle = this.GetIntPtrProperty(TitleProperty);
            if (handle == default)
                return "";
            return FromHandle<NSString>(handle, false)!.ToString();
        }
        set
        {
            this.VerifyReleased();
            TitleProperty ??= NSWindowClass!.GetProperty("title").AsNonNull();
            using var s = new NSString(value);
            this.SetProperty(TitleProperty, s);
        }
    }


    /// <summary>
    /// Get or set the visibility of the window’s title and title bar buttons.
    /// </summary>
    public TitleAppearance TitleVisibility
    {
        get
        {
            TitleVisibilityProperty ??= NSWindowClass!.GetProperty("titleVisibility").AsNonNull();
            return this.GetProperty<TitleAppearance>(TitleVisibilityProperty);
        }
        set
        {
            TitleVisibilityProperty ??= NSWindowClass!.GetProperty("titleVisibility").AsNonNull();
            this.SetProperty(TitleVisibilityProperty, value);
        }
    }


    /// <summary>
    /// Toggle visibility of toolbar of window.
    /// </summary>
    /// <param name="sender">Sender.</param>
    public void ToggleToolbarShown(NSObject? sender = null)
    {
        ToggleToolbarShownSelector ??= Selector.FromName("toggleToolbarShown:");
        this.SendMessage(ToggleToolbarShownSelector, sender);
    }


    /// <summary>
    /// Get or set toolbar of window.
    /// </summary>
    public NSToolbar? Toolbar
    {
        get
        {
            ToolbarProperty ??= NSWindowClass!.GetProperty("toolbar").AsNonNull();
            return this.GetNSObjectProperty<NSToolbar>(ToolbarProperty);
        }
        set
        {
            ToolbarProperty ??= NSWindowClass!.GetProperty("toolbar").AsNonNull();
            this.SetProperty(ToolbarProperty, (NSObject?)value);
        }
    }


    /// <summary>
    /// Get or set the way of window to display its toolbar. Only works on macOS 11.0 and later.
    /// </summary>
    public NSWindowToolbarStyle ToolbarStyle
    {
        get
        {
            ToolbarStyleProperty ??= NSWindowClass!.GetProperty("toolbarStyle").AsNonNull();
            return (NSWindowToolbarStyle)this.GetInt32Property(ToolbarStyleProperty);
        }
        set
        {
            ToolbarStyleProperty ??= NSWindowClass!.GetProperty("toolbarStyle").AsNonNull();
            this.SetProperty(ToolbarStyleProperty, (int)value);
        }
    }


    /// <summary>
    /// Get name of notification posted when window is about to enter full-screen mode.
    /// </summary>
    public static NSString WillEnterFullScreenNotification
    {
        get
        {
            WillEnterFullScreenNotificationName ??= new NSString("NSWindowWillEnterFullScreenNotification");
            return WillEnterFullScreenNotificationName;
        }
    }


    /// <summary>
    /// Get name of notification posted when window is about to exit full-screen mode.
    /// </summary>
    public static NSString WillExitFullScreenNotification
    {
        get
        {
            WillExitFullScreenNotificationName ??= new NSString("NSWindowWillExitFullScreenNotification");
            return WillExitFullScreenNotificationName;
        }
    }
}