using System;
using System.Runtime.InteropServices;

namespace CarinaStudio.MacOS.ObjectiveCRuntime
{
    /// <summary>
    /// Native layer of Objective-C runtime.
    /// </summary>
    internal static unsafe class Native
    {
        // Constants.
        const string LibName = "/usr/lib/libobjc.dylib";


        // Static initializer.
        static unsafe Native()
        {
            // check platform
            if (Platform.IsNotMacOS)
                return;
        }


        [DllImport(LibName)]
		public static extern IntPtr sel_getName(IntPtr sel);


        [DllImport(LibName)]
		[return: MarshalAs(UnmanagedType.U1)]
		public static extern bool sel_isMapped(IntPtr sel);


        [DllImport(LibName)]
		public static extern IntPtr sel_registerName(string name);
    }
}