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


        // Static fields.
        [ThreadStatic]
        static volatile IntPtr defaultAllocator;


        [DllImport(LibName)]
		public static extern IntPtr CFAllocatorGetDefault();


        [DllImport(LibName)]
        public static extern IntPtr CFCopyTypeIDDescription(uint type_id);


        [DllImport(LibName)]
        public static extern uint CFGetTypeID(IntPtr cf);


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


        /// <summary>
        /// Get default allocator of current thread.
        /// </summary>
        public static IntPtr DefaultAllocator
        {
            get
            {
                if (defaultAllocator != IntPtr.Zero)
                    return defaultAllocator;
                defaultAllocator = CFAllocatorGetDefault();
                return defaultAllocator;
            }
        }
    }
}