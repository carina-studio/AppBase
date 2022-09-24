using System;
using System.Runtime.InteropServices;

namespace CarinaStudio.MacOS.ObjectiveC
{
    /// <summary>
    /// NSApplication.
    /// </summary>
    public unsafe class NSApplication : NSObject
    {
        // Native symbols.
        static readonly IntPtr* NSAppPtr;


        // Static fields.
        static readonly Class? Class;
        static volatile NSApplication? _Current;
        static readonly PropertyDescriptor? DockTileProperty;
        static readonly PropertyDescriptor? IsRunningProperty;
        static readonly PropertyDescriptor? MainWindowProperty;
        static readonly PropertyDescriptor? WindowsProperty;


        // Fields.
        NSObject? dockTile;
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
            Class = Class.GetClass("NSApplication");
            Class!.TryFindProperty("dockTile", out DockTileProperty);
            Class!.TryFindProperty("mainWindow", out MainWindowProperty);
            Class!.TryFindProperty("running", out IsRunningProperty);
            Class!.TryFindProperty("windows", out WindowsProperty);
        }


        // Constructor.
        NSApplication(IntPtr handle) : base(handle, false)
        { }


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
                    _Current = new NSApplication(app);
                    return _Current;
                });
            }
        }


        /// <summary>
        /// Get Dock tile.
        /// </summary>
        public NSObject DockTile
        {
            get
            {
                this.VerifyDisposed();
                return this.dockTile ?? SendMessageForIntPtr(this.Handle, DockTileProperty!.Getter!.Handle).Let(it =>
                {
                    this.dockTile = NSObject.Wrap(it, false);
                    return this.dockTile;
                });
            }
        }


        /// <summary>
        /// Check whether the main event loop is runnig or not.
        /// </summary>
        public bool IsRunning { get => SendMessageForBoolean(this.Handle, IsRunningProperty!.Getter!.Handle); }


        /// <summary>
        /// Get main window of application.
        /// </summary>
        public NSObject? MainWindow
        {
            get
            {
                this.VerifyDisposed();
                var handle = SendMessageForIntPtr(this.Handle, MainWindowProperty!.Getter!.Handle);
                if (handle != IntPtr.Zero)
                {
                    if (this.mainWindow == null || this.mainWindow.Handle != handle)
                        this.mainWindow = NSObject.Wrap(handle, false);
                }
                else if (this.mainWindow != null)
                    this.mainWindow = null;
                return this.mainWindow;
            }
        }
    }
}