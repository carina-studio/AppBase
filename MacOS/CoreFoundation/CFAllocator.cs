using System;

namespace CarinaStudio.MacOS.CoreFoundation
{
    /// <summary>
    /// CFAllocator.
    /// </summary>
    public class CFAllocator : CFObject
    {
        // Static fields.
        [ThreadStatic]
        static IntPtr defaultAllocatorHandle;


        // Constructor.
        CFAllocator(IntPtr allocator, bool ownsInstance) : base(allocator, Global.Run(() =>
        {
            var defaultHandle = defaultAllocatorHandle != IntPtr.Zero
                ? defaultAllocatorHandle
                : Native.CFAllocatorGetDefault().Also((ref IntPtr it) => defaultAllocatorHandle = it);
            return ownsInstance && allocator != defaultHandle;
        }))
        { }


        /// <summary>
        /// Get default allocator of current thread.
        /// </summary>
        public static CFAllocator Default
        {
            get
            {
                var defaultHandle = defaultAllocatorHandle != IntPtr.Zero
                    ? defaultAllocatorHandle
                    : Native.CFAllocatorGetDefault().Also((ref IntPtr it) => defaultAllocatorHandle = it);
                return new CFAllocator(defaultHandle, false);
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