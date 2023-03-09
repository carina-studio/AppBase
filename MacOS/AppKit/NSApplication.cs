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
    static readonly Selector? ActivateSelector;
    static readonly Property? AppearanceProperty;
    static volatile NSApplication? _Current;
    static readonly Selector? DeactivateSelector;
    static readonly Property? DelegateProperty;
    static readonly Selector? DockTileSelector;
    static readonly Property? IconImageProperty;
    static readonly Selector? IsRunningSelector;
    static readonly Selector? MainWindowSelector;
    static readonly Class? NSApplicationClass;
    static readonly Selector? RunSelector;
    static readonly Selector? WindowsSelector;


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
        ActivateSelector = Selector.FromName("activateIgnoringOtherApps:");
        AppearanceProperty = NSApplicationClass.GetProperty("appearance");
        DeactivateSelector = Selector.FromName("deactivate");
        DelegateProperty = NSApplicationClass.GetProperty("delegate");
        DockTileSelector = Selector.FromName("dockTile");
        IconImageProperty = NSApplicationClass.GetProperty("applicationIconImage");
        IsRunningSelector = Selector.FromName("isRunning");
        MainWindowSelector = Selector.FromName("mainWindow");
        RunSelector = Selector.FromName("run");
        WindowsSelector = Selector.FromName("windows");
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
    public void Activate(bool ignoreOtherApps) =>
        this.SendMessage(ActivateSelector!, ignoreOtherApps);
    

    /// <summary>
    /// Get or set appearance of application.
    /// </summary>
    public NSAppearance? Appearance
    {
        get => this.GetProperty<NSAppearance>(AppearanceProperty!);
        set => this.SetProperty(AppearanceProperty!, value);
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
            var handle = this.GetProperty<IntPtr>(IconImageProperty!);
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
            this.appIconImage = value;
            this.SetProperty<NSObject>(IconImageProperty!, value);
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
                _Current = new(app, true);
                _Current.IsDefaultInstance = true;
                return _Current;
            });
        }
    }


    /// <summary>
    /// Deactivate the application.
    /// </summary>
    public void Deactivate() =>
        this.SendMessage(DeactivateSelector!);


    /// <summary>
    /// Get or set object which conforms to NSApplicationDelegate protocol to receive call-back from application.
    /// </summary>
    public NSObject? Delegate
    {
        get => this.GetProperty<NSObject>(DelegateProperty!);
        set => this.SetProperty(DelegateProperty!, value);
    }


    /// <summary>
    /// Get Dock tile.
    /// </summary>
    public NSDockTile DockTile
    {
        get
        {
            this.VerifyReleased();
            return this.dockTile ?? this.SendMessage<IntPtr>(DockTileSelector!).Let(it =>
            {
                this.dockTile = new(it);
                return this.dockTile;
            });
        }
    }


    /// <summary>
    /// Check whether the main event loop is runnig or not.
    /// </summary>
    public bool IsRunning { get => this.SendMessage<bool>(IsRunningSelector!); }


    /// <summary>
    /// Get main window of application.
    /// </summary>
    public NSObject? MainWindow
    {
        get
        {
            this.VerifyReleased();
            var handle = this.SendMessage<IntPtr>(MainWindowSelector!);
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
    public void Run() =>
        this.SendMessage(RunSelector!);


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
                _Current = new(handle, true);
                _Current.IsDefaultInstance = true;
                return _Current;
            });
        }
    }


    /// <summary>
    /// Get array of windows.
    /// </summary>
    public NSArray<NSWindow> Windows { get => this.SendMessage<NSArray<NSWindow>>(WindowsSelector!); }
}