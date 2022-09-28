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
        static readonly Class? NSApplicationDelegateClass;
        static volatile NSApplication? _Current;
        static readonly Property? DelegateProperty;
        static readonly Property? DockTileProperty;
        static readonly Property? IsRunningProperty;
        static readonly Property? MainWindowProperty;
        static readonly Property? WindowsProperty;


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
            NSApplicationClass = Class.GetClass("NSApplication").AsNonNull();
            NSApplicationDelegateClass = Class.GetProtocol("NSApplicationDelegate").AsNonNull();
            NSApplicationClass.TryGetProperty("delegate", out DelegateProperty);
            NSApplicationClass.TryGetProperty("dockTile", out DockTileProperty);
            NSApplicationClass.TryGetProperty("mainWindow", out MainWindowProperty);
            NSApplicationClass.TryGetProperty("running", out IsRunningProperty);
            NSApplicationClass.TryGetProperty("windows", out WindowsProperty);
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
                this.VerifyDisposed();
                return this.dockTile ?? this.GetProperty<NSDockTile>(DockTileProperty!).Let(it =>
                {
                    this.dockTile = it.AsNonNull();
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
                this.VerifyDisposed();
                var handle = this.GetProperty<IntPtr>(MainWindowProperty!);
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