using System;
using System.Runtime.InteropServices;

namespace CarinaStudio.MacOS.CoreFoundation
{
    /// <summary>
    /// CFAllocator.
    /// </summary>
    public class CFAllocator : CFObject
    {
        // Native symbols.
        [DllImport(NativeLibraryNames.CoreFoundation)]
		static extern IntPtr CFAllocatorGetDefault();


        // Static fields.
        [ThreadStatic]
        static CFAllocator? defaultAllocator;
        [ThreadStatic]
        static IntPtr defaultAllocatorHandle;


        // Constructor.
        CFAllocator(IntPtr allocator, bool ownsInstance) : base(allocator, Global.Run(() =>
        {
            if (defaultAllocatorHandle == IntPtr.Zero)
                defaultAllocatorHandle = CFAllocatorGetDefault();
            return ownsInstance && allocator != defaultAllocatorHandle;
        }))
        { 
            this.IsDefaultInstance = (allocator == defaultAllocatorHandle);
        }


        /// <summary>
        /// Get default allocator of current thread.
        /// </summary>
        public static CFAllocator Default
        {
            get
            {
                return defaultAllocator ?? CFAllocatorGetDefault().Let(handle =>
                {
                    defaultAllocatorHandle = handle;
                    defaultAllocator = new CFAllocator(handle, false);
                    return defaultAllocator;
                });
            }
        }


        /// <summary>
        /// Wrap a native object.
        /// </summary>
        /// <param name="s">Handle of instance.</param>
        /// <param name="ownsInstance">True to .</param>
        /// <returns>Wrapped object.</returns>
        public static new CFAllocator Wrap(IntPtr s, bool ownsInstance = false) =>
            new CFAllocator(s, ownsInstance);
    }
}