using CarinaStudio.MacOS.ObjectiveC;
using System;
using System.Runtime.InteropServices;

namespace CarinaStudio.MacOS.AppKit;

/// <summary>
/// NSApplication.
/// </summary>
public unsafe class NSApplication : NSResponder
{
#pragma warning disable CS1591
    /// <summary>
    /// TerminateReply.
    /// </summary>
    public enum TerminateReply : uint
    {
        TerminateNow = 1,
        TerminateCancel = 0,
        TerminateLater = 2,
    }
#pragma warning restore CS1591


    // Native symbols.
    static readonly IntPtr* NSAppPtr;


    // Static fields.
    static Selector? ActivateSelector;
    static Property? AppearanceProperty;
    static volatile NSApplication? _Current;
    static Selector? DeactivateSelector;
    static Property? DelegateProperty;
    static Selector? DockTileSelector;
    static Property? IconImageProperty;
    static Selector? IsRunningSelector;
    static Selector? MainWindowSelector;
    static readonly Class? NSApplicationClass;
    static Selector? RunSelector;
    static Selector? WindowsSelector;


    // Fields.
    NSImage? appIconImage;
    NSDockTile? dockTile;
    NSObject? mainWindow;


    // Static initializer.
    static NSApplication()
    {
        if (Platform.IsNotMacOS)
            return;
        var libHandle = NativeLibrary.Load(NativeLibraryNames.AppKit);
        if (libHandle != IntPtr.Zero)
        {
            NSAppPtr = (IntPtr*)NativeLibrary.GetExport(libHandle, "NSApp");
        }
        NSApplicationClass = Class.GetClass("NSApplication").AsNonNull();
    }


    // Constructor.
    NSApplication(IntPtr handle, bool ownsInstance) : base(handle, false, ownsInstance) =>
        this.VerifyClass(NSApplicationClass!);
    NSApplication(Class cls, IntPtr handle, bool ownsInstance) : base(cls, handle, ownsInstance)
    { }


    /// <summary>
    /// Activate the application.
    /// </summary>
    /// <param name="ignoreOtherApps">True to active application regardless.</param>
    public void Activate(bool ignoreOtherApps)
    {
        ActivateSelector ??= Selector.FromName("activateIgnoringOtherApps:");
        this.SendMessage(ActivateSelector, ignoreOtherApps);
    }
    

    /// <summary>
    /// Get or set appearance of application.
    /// </summary>
    public NSAppearance? Appearance
    {
        get 
        {
            AppearanceProperty ??= NSApplicationClass!.GetProperty("appearance").AsNonNull();
            return this.GetProperty<NSAppearance>(AppearanceProperty);
        }
        set 
        {
            AppearanceProperty ??= NSApplicationClass!.GetProperty("appearance").AsNonNull();
            this.SetProperty(AppearanceProperty, value);
        }
    }
    

    /// <summary>
    /// Get or set icon of application.
    /// </summary>
    /// <value></value>
    public NSImage? ApplicationIconImage
    {
        get
        {
            this.VerifyReleased();
            IconImageProperty ??= NSApplicationClass!.GetProperty("applicationIconImage").AsNonNull();
            var handle = this.GetProperty<IntPtr>(IconImageProperty);
            if (this.appIconImage is null)
            {
                if (handle == IntPtr.Zero)
                    return null;
                this.appIconImage = NSObject.Retain<NSImage>(handle);
            }
            else if (handle != this.appIconImage.Handle)
                this.appIconImage = handle != IntPtr.Zero ? NSObject.Retain<NSImage>(handle) : null;
            return this.appIconImage;
        }
        set
        {
            this.VerifyReleased();
            if (this.appIconImage == value)
                return;
            IconImageProperty ??= NSApplicationClass!.GetProperty("applicationIconImage").AsNonNull();
            this.appIconImage = value;
            this.SetProperty<NSObject>(IconImageProperty, value);
        }
    }


    /// <summary>
    /// Get existing <see cref="NSApplication"/> instance.
    /// </summary>
    public static NSApplication? Current
    {
        get
        {
            return _Current ?? typeof(NSApplication).Lock(() =>
            {
                if (_Current != null)
                    return _Current;
                var app = NSAppPtr != null ? *NSAppPtr : IntPtr.Zero;
                if (app == IntPtr.Zero)
                    return null;
                _Current = new(app, true)
                {
                    IsDefaultInstance = true
                };
                return _Current;
            });
        }
    }


    /// <summary>
    /// Deactivate the application.
    /// </summary>
    public void Deactivate()
    {
        DeactivateSelector ??= Selector.FromName("deactivate");
        this.SendMessage(DeactivateSelector);
    }


    /// <summary>
    /// Get or set object which conforms to NSApplicationDelegate protocol to receive call-back from application.
    /// </summary>
    public NSObject? Delegate
    {
        get 
        {
            DelegateProperty ??= NSApplicationClass!.GetProperty("delegate").AsNonNull();
            return this.GetProperty<NSObject>(DelegateProperty);
        }
        set 
        {
            DelegateProperty ??= NSApplicationClass!.GetProperty("delegate").AsNonNull();
            this.SetProperty(DelegateProperty, value);
        }
    }


    /// <summary>
    /// Get Dock tile.
    /// </summary>
    public NSDockTile DockTile
    {
        get
        {
            this.VerifyReleased();
            DockTileSelector ??= Selector.FromName("dockTile");
            return this.dockTile ?? this.SendMessage<IntPtr>(DockTileSelector).Let(it =>
            {
                this.dockTile = new(it);
                return this.dockTile;
            });
        }
    }


    /// <summary>
    /// Check whether the main event loop is runnig or not.
    /// </summary>
    public bool IsRunning 
    { 
        get 
        {
            IsRunningSelector ??= Selector.FromName("isRunning");
            return this.SendMessage<bool>(IsRunningSelector); 
        }
    }


    /// <summary>
    /// Get main window of application.
    /// </summary>
    public NSObject? MainWindow
    {
        get
        {
            this.VerifyReleased();
            MainWindowSelector ??= Selector.FromName("mainWindow");
            var handle = this.SendMessage<IntPtr>(MainWindowSelector);
            if (handle != IntPtr.Zero)
            {
                if (this.mainWindow == null || this.mainWindow.Handle != handle)
                    this.mainWindow = NSObject.Retain<NSWindow>(handle);
            }
            else if (this.mainWindow != null)
                this.mainWindow = null;
            return this.mainWindow;
        }
    }


    /// <summary>
    /// Start the main event loop.
    /// </summary>
    public void Run()
    {
        RunSelector ??= Selector.FromName("run");
        this.SendMessage(RunSelector);
    }


    /// <summary>
    /// Get the <see cref="NSApplication"/> instance or create one if it doesn’t exist yet.
    /// </summary>
    public static NSApplication Shared
    {
        get
        {
            return _Current ?? typeof(NSApplication).Lock(() =>
            {
                if (_Current != null)
                    return _Current;
                var selector = Selector.FromName("sharedApplication");
                var handle = NSObject.SendMessage<IntPtr>(NSApplicationClass!.Handle, selector);
                if (handle == default)
                    throw new Exception("Unable to create NSApplication instance.");
                _Current = new(handle, true)
                {
                    IsDefaultInstance = true
                };
                return _Current;
            });
        }
    }


    /// <summary>
    /// Get array of windows.
    /// </summary>
    public NSArray<NSWindow> Windows 
    { 
        get 
        {
            WindowsSelector ??= Selector.FromName("windows");
            return this.SendMessage<NSArray<NSWindow>>(WindowsSelector); 
        }
    }
}