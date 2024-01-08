using CarinaStudio.MacOS.ObjectiveC;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace CarinaStudio.MacOS.AppKit;

/// <summary>
/// NSApplication.
/// </summary>
public unsafe class NSApplication : NSResponder
{
#pragma warning disable CS1591
    /// <summary>
    /// Activation policies.
    /// </summary>
    public enum ActivationPolicy
    {
        Regular = 0,
        Accessory = 1,
        Prohibited = 2,
    }
    
    
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
    static Property? EffectiveAppearanceProperty;
    static Selector? GetActivationPolicySelector;
    static Selector? HideOtherApplicationsSelector;
    static Property? IconImageProperty;
    static Selector? IsActiveSelector;
    static Selector? IsRunningSelector;
    static Selector? KeyWindowSelector;
    static Selector? MainWindowSelector;
    static readonly Class? NSApplicationClass;
    static Selector? RunSelector;
    static Selector? SetActivationPolicySelector;
    static Selector? WindowsSelector;


    // Fields.
    WeakReference<NSImage>? appIconImageRef;
    NSDockTile? dockTile;
    WeakReference<NSWindow>? keyWindowRef;
    WeakReference<NSWindow>? mainWindowRef;


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
    [RequiresDynamicCode(CallMethodRdcMessage)]
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
            return this.GetNSObjectProperty<NSAppearance>(AppearanceProperty);
        }
        set 
        {
            AppearanceProperty ??= NSApplicationClass!.GetProperty("appearance").AsNonNull();
            this.SetProperty(AppearanceProperty, (NSObject?)value);
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
            var handle = this.GetIntPtrProperty(IconImageProperty);
            var image = default(NSImage);
            var prevImage = default(NSImage);
            if (handle != default)
            {
                if (this.appIconImageRef?.TryGetTarget(out prevImage) == true && prevImage.Handle == handle)
                    image = prevImage;
                else
                {
                    prevImage?.Release();
                    image = Retain<NSImage>(handle).AsNonNull();
                    this.appIconImageRef = new(image);
                }
            }
            else
                this.appIconImageRef = null;
            return image;
        }
        set
        {
            this.VerifyReleased();
            var prevImage = default(NSImage);
            if (this.appIconImageRef?.TryGetTarget(out prevImage) == true && prevImage == value)
                return;
            prevImage?.Release();
            IconImageProperty ??= NSApplicationClass!.GetProperty("applicationIconImage").AsNonNull();
            this.appIconImageRef = value is not null ? new(value) : null;
            this.SetProperty(IconImageProperty, (NSObject?)value);
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
            return this.GetNSObjectProperty<NSObject>(DelegateProperty);
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
#pragma warning disable IL3050
            return this.dockTile ?? this.SendMessage<IntPtr>(DockTileSelector).Let(it =>
            {
                this.dockTile = new(it);
                return this.dockTile;
            });
#pragma warning restore IL3050
        }
    }
    
    
    /// <summary>
    /// Get the appearance that AppKit uses to draw the app’s interface.
    /// </summary>
    public NSAppearance EffectiveAppearance
    {
        get 
        {
            EffectiveAppearanceProperty ??= NSApplicationClass!.GetProperty("effectiveAppearance").AsNonNull();
            return this.GetNSObjectProperty<NSAppearance>(EffectiveAppearanceProperty).AsNonNull();
        }
    }


    /// <summary>
    /// Get activation policy of application.
    /// </summary>
    /// <returns>Activation policy.</returns>
    public ActivationPolicy GetActivationPolicy()
    {
        GetActivationPolicySelector ??= Selector.FromName("activationPolicy");
#pragma warning disable IL3050
        return this.SendMessage<ActivationPolicy>(GetActivationPolicySelector);
#pragma warning restore IL3050
    }


    /// <summary>
    /// Hides all apps except the current application.
    /// </summary>
    /// <param name="sender">The object that sent this message.</param>
    [RequiresDynamicCode(CallMethodRdcMessage)]
    public void HideOtherApplications(NSObject? sender)
    {
        HideOtherApplicationsSelector ??= Selector.FromName("hideOtherApplications:");
        this.SendMessage(HideOtherApplicationsSelector, sender);
    }
    
    
    /// <summary>
    /// Check whether the main event loop is running or not.
    /// </summary>
    public bool IsActive 
    { 
        get
        {
            IsActiveSelector ??= Selector.FromName("isActive");
#pragma warning disable IL3050
            return this.SendMessage<bool>(IsActiveSelector);
#pragma warning restore IL3050
        }
    }


    /// <summary>
    /// Check whether the main event loop is running or not.
    /// </summary>
    public bool IsRunning 
    { 
        get 
        {
            IsRunningSelector ??= Selector.FromName("isRunning");
#pragma warning disable IL3050
            return this.SendMessage<bool>(IsRunningSelector);
#pragma warning restore IL3050
        }
    }
    
    
    /// <summary>
    /// Get the window that currently receives keyboard events.
    /// </summary>
    public NSWindow? KeyWindow
    {
        get
        {
            this.VerifyReleased();
            KeyWindowSelector ??= Selector.FromName("keyWindow");
#pragma warning disable IL3050
            var handle = this.SendMessage<IntPtr>(KeyWindowSelector);
#pragma warning restore IL3050
            var window = default(NSWindow);
            var prevWindow = default(NSWindow);
            if (handle != IntPtr.Zero)
            {
                if (this.keyWindowRef?.TryGetTarget(out prevWindow) == true && prevWindow.Handle == handle)
                    window = prevWindow;
                else
                {
                    prevWindow?.Release();
                    window = Retain<NSWindow>(handle).AsNonNull();
                    this.keyWindowRef = new(window);
                }
            }
            else
                this.keyWindowRef = null;
            return window;
        }
    }


    /// <summary>
    /// Get main window of application.
    /// </summary>
    public NSWindow? MainWindow
    {
        get
        {
            this.VerifyReleased();
            MainWindowSelector ??= Selector.FromName("mainWindow");
#pragma warning disable IL3050
            var handle = this.SendMessage<IntPtr>(MainWindowSelector);
#pragma warning restore IL3050
            var window = default(NSWindow);
            var prevWindow = default(NSWindow);
            if (handle != IntPtr.Zero)
            {
                if (this.mainWindowRef?.TryGetTarget(out prevWindow) == true && prevWindow.Handle == handle)
                    window = prevWindow;
                else
                {
                    prevWindow?.Release();
                    window = Retain<NSWindow>(handle).AsNonNull();
                    this.mainWindowRef = new(window);
                }
            }
            else
                this.mainWindowRef = null;
            return window;
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
    /// Set activation policy of application.
    /// </summary>
    /// <param name="policy">Activation policy.</param>
    [RequiresDynamicCode(CallMethodRdcMessage)]
    public void SetActivationPolicy(ActivationPolicy policy)
    {
        SetActivationPolicySelector ??= Selector.FromName("setActivationPolicy:");
#pragma warning disable IL3050
        this.SendMessage(SetActivationPolicySelector, policy);
#pragma warning restore IL3050
    }


    /// <summary>
    /// Get the <see cref="NSApplication"/> instance or create one if it does not exist yet.
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
#pragma warning disable IL3050
                var handle = SendMessage<IntPtr>(NSApplicationClass!.Handle, selector);
#pragma warning restore IL3050
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
        [RequiresDynamicCode(GetPropertyRdcMessage)]
        get 
        {
            WindowsSelector ??= Selector.FromName("windows");
            return this.SendMessage<NSArray<NSWindow>>(WindowsSelector); 
        }
    }
}