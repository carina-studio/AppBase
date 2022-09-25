using CarinaStudio.MacOS.ObjectiveC;
using System;
using System.Runtime.InteropServices;

namespace CarinaStudio.MacOS.AppKit
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
            Class!.TryGetProperty("dockTile", out DockTileProperty);
            Class!.TryGetProperty("mainWindow", out MainWindowProperty);
            Class!.TryGetProperty("running", out IsRunningProperty);
            Class!.TryGetProperty("windows", out WindowsProperty);
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
                return this.dockTile ?? this.GetObjectProperty<NSObject>(DockTileProperty!).Let(it =>
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


        /// <summary>
        /// Wrap given handle as <see cref="NSApplication"/>.
        /// </summary>
        /// <param name="handle">Handle of instance.</param>
        /// <returns>Wrapped instance.</returns>
        public static NSApplication Wrap(IntPtr handle) =>
            Wrap(handle, false);


        /// <summary>
        /// Wrap given handle as <see cref="NSApplication"/>.
        /// </summary>
        /// <param name="handle">Handle of instance.</param>
        /// <param name="ownsInstance">True to owns instance.</param>
        /// <returns>Wrapped instance.</returns>
        internal static new NSApplication Wrap(IntPtr handle, bool ownsInstance = false)
        {
            if (Class?.IsAssignableFrom(Class.GetClass(handle)) != true)
                throw new InvalidOperationException("Cannot wrap instance as NSApplication.");
            return new NSApplication(handle);
        }
    }
}