using System;
using System.Runtime.InteropServices;

namespace CarinaStudio.MacOS.ObjectiveCRuntime
{
    /// <summary>
    /// Selector of NSObject.
    /// </summary>
    public unsafe class Selector
    {
        // Native symbols.
        [DllImport(NativeLibraryNames.ObjectiveCRuntime)]
		static extern IntPtr sel_getName(IntPtr sel);
        [DllImport(NativeLibraryNames.ObjectiveCRuntime)]
		[return: MarshalAs(UnmanagedType.U1)]
		static extern bool sel_isMapped(IntPtr sel);
        [DllImport(NativeLibraryNames.ObjectiveCRuntime)]
		static extern IntPtr sel_registerName(string name);
    }
}