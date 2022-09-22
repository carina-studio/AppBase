using System;
using System.Runtime.InteropServices;
using System.Text;

namespace CarinaStudio.MacOS.CoreFoundation
{
    /// <summary>
    /// Native layer of Core Foundation.
    /// </summary>
    internal static unsafe class Native
    {
        // Constants.
        const string LibName = "/System/Library/Frameworks/ApplicationServices.framework/Frameworks/CoreFoundation.framework/CoreFoundation";


        // Static initializer.
        static unsafe Native()
        {
            // check platform
            if (Platform.IsNotMacOS)
                return;
            
            // load symbols
            var libHandle = NativeLibrary.Load(LibName);
            if (libHandle != IntPtr.Zero)
            {
                kCFNumberNaN = *(IntPtr*)NativeLibrary.GetExport(libHandle, "kCFNumberNaN");
                kCFNumberNegativeInfinity = *(IntPtr*)NativeLibrary.GetExport(libHandle, "kCFNumberNegativeInfinity");
                kCFNumberPositiveInfinity = *(IntPtr*)NativeLibrary.GetExport(libHandle, "kCFNumberPositiveInfinity");
            }
        }


        [DllImport(LibName)]
		public static extern IntPtr CFAllocatorGetDefault();


        [DllImport(LibName)]
        public static extern IntPtr CFCopyTypeIDDescription(uint type_id);


        [DllImport(LibName)]
        public static extern uint CFGetTypeID(IntPtr cf);


        [DllImport(LibName)]
        public static extern IntPtr CFNumberCreate(IntPtr allocator, CFNumberType theType, void* valuePtr);


        [DllImport(LibName)]
        public static extern CFNumberType CFNumberGetType(IntPtr number);


        [DllImport(LibName)]
		public static extern void CFNumberGetValue(IntPtr number, CFNumberType theType, void* value);


        [DllImport(LibName)]
		public static extern void CFRelease(IntPtr cf);


        [DllImport(LibName)]
		public static extern IntPtr CFRetain(IntPtr cf);


        [DllImport(LibName, CharSet = CharSet.Unicode)]
		public static extern IntPtr CFStringCreateWithCharacters(IntPtr alloc, string chars, long numChars);


        [DllImport(LibName)]
		public static extern void CFStringGetCharacters(IntPtr theString, CFRange range, void* buffer);


        [DllImport(LibName, CharSet = CharSet.Unicode)]
		public static extern void CFStringGetCharacters(IntPtr theString, CFRange range, StringBuilder buffer);


        [DllImport(LibName)]
		public static extern long CFStringGetLength(IntPtr theString);


        public static readonly IntPtr kCFNumberNaN;


        public static readonly IntPtr kCFNumberNegativeInfinity;


        public static readonly IntPtr kCFNumberPositiveInfinity;
    }
}