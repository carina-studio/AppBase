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
        // Native symbols.
        static readonly IntPtr* NSAppPtr;


        // Static fields.
        static readonly Class? NSApplicationClass;
        static volatile NSApplication? _Current;
        static readonly PropertyDescriptor? DockTileProperty;
        static readonly PropertyDescriptor? IsRunningProperty;
        static readonly PropertyDescriptor? MainWindowProperty;
        static readonly PropertyDescriptor? WindowsProperty;


        // Fields.
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
            NSApplicationClass = Class.GetClass("NSApplication");
            if (NSApplicationClass != null)
            {
                NSApplicationClass.TryGetProperty("dockTile", out DockTileProperty);
                NSApplicationClass.TryGetProperty("mainWindow", out MainWindowProperty);
                NSApplicationClass.TryGetProperty("running", out IsRunningProperty);
                NSApplicationClass.TryGetProperty("windows", out WindowsProperty);
            }
        }


        // Constructor.
        NSApplication(InstanceHolder instance) : base(instance) =>
            this.VerifyClass(NSApplicationClass!);


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
                    _Current = new NSApplication(new(app));
                    return _Current;
                });
            }
        }


        /// <summary>
        /// Get Dock tile.
        /// </summary>
        public NSDockTile DockTile
        {
            get
            {
                this.VerifyDisposed();
                return this.dockTile ?? this.GetObjectProperty<NSDockTile>(DockTileProperty!).Let(it =>
                {
                    this.dockTile = it.AsNonNull();
                    return this.dockTile;
                });
            }
        }


        /// <summary>
        /// Check whether the main event loop is runnig or not.
        /// </summary>
        public bool IsRunning { get => this.GetBooleanProperty(IsRunningProperty!); }


        /// <summary>
        /// Get main window of application.
        /// </summary>
        public NSObject? MainWindow
        {
            get
            {
                this.VerifyDisposed();
                var handle = this.GetIntPtrProperty(MainWindowProperty!);
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