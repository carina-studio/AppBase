using CarinaStudio.MacOS.ObjectiveC;
using System;
using System.Runtime.InteropServices;

namespace CarinaStudio.MacOS.AppKit
{
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
        static readonly Class? NSApplicationClass;
        static readonly Class? NSApplicationDelegateClass;
        static volatile NSApplication? _Current;
        static readonly Property? DelegateProperty;
        static readonly Property? DockTileProperty;
        static readonly Property? IconImageProperty;
        static readonly Property? IsRunningProperty;
        static readonly Property? MainWindowProperty;
        static readonly Property? WindowsProperty;


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
            NSApplicationDelegateClass = Class.GetProtocol("NSApplicationDelegate").AsNonNull();
            DelegateProperty = NSApplicationClass.GetProperty("delegate");
            DockTileProperty = NSApplicationClass.GetProperty("dockTile");
            MainWindowProperty = NSApplicationClass.GetProperty("mainWindow");
            IconImageProperty = NSApplicationClass.GetProperty("applicationIconImage");
            IsRunningProperty = NSApplicationClass.GetProperty("running");
            WindowsProperty = NSApplicationClass.GetProperty("windows");
        }


        // Constructor.
        NSApplication(IntPtr handle, bool ownsInstance) : base(handle, false, ownsInstance) =>
            this.VerifyClass(NSApplicationClass!);
        NSApplication(Class cls, IntPtr handle, bool ownsInstance) : base(cls, handle, ownsInstance)
        { }
        

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
        /// Get shared <see cref="NSApplication"/> instance of current application.
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
                    _Current = new NSApplication(app, true);
                    _Current.IsDefaultInstance = true;
                    return _Current;
                });
            }
        }


        /// <summary>
        /// Get or set object which conforms to NSApplicationDelegate protocol to receive call-back from application.
        /// </summary>
        public NSObject? Delegate
        {
            get => this.GetProperty<NSObject>(DelegateProperty!);
            set
            {
                if (value != null && NSApplicationDelegateClass?.IsAssignableFrom(value.Class) != true)
                    throw new ArgumentException($"The value must conforms to protocol '{NSApplicationDelegateClass?.Name}'.");
                this.SetProperty(DelegateProperty!, value);
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
                return this.dockTile ?? this.GetProperty<IntPtr>(DockTileProperty!).Let(it =>
                {
                    this.dockTile = new(it);
                    return this.dockTile;
                });
            }
        }


        /// <summary>
        /// Check whether the main event loop is runnig or not.
        /// </summary>
        public bool IsRunning { get => this.GetProperty<bool>(IsRunningProperty!); }


        /// <summary>
        /// Get main window of application.
        /// </summary>
        public NSObject? MainWindow
        {
            get
            {
                this.VerifyReleased();
                var handle = this.GetProperty<IntPtr>(MainWindowProperty!);
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
        /// Get array of windows.
        /// </summary>
        public NSArray<NSWindow> Windows { get => this.GetProperty<NSArray<NSWindow>>(WindowsProperty!); }
    }
}