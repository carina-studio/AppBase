using System;
using System.Runtime.InteropServices;

namespace CarinaStudio.MacOS.CoreFoundation
{
    /// <summary>
    /// String.
    /// </summary>
    public unsafe class CFString : CFObject
    {
        // Native symbols.
        [DllImport(NativeLibraryNames.CoreFoundation, CharSet = CharSet.Unicode)]
		static extern IntPtr CFStringCreateWithCharacters(IntPtr alloc, string chars, long numChars);
        [DllImport(NativeLibraryNames.CoreFoundation)]
		static extern void CFStringGetCharacters(IntPtr theString, CFRange range, void* buffer);
        [DllImport(NativeLibraryNames.CoreFoundation)]
		static extern long CFStringGetLength(IntPtr theString);


        // Fields.
        int length;


        /// <summary>
        /// Initialize new <see cref="CFString"/> instance.
        /// </summary>
        /// <param name="s">String.</param>
        public CFString(string s) : base(Global.Run(() =>
        {
            if (s == null)
                throw new ArgumentNullException(nameof(s));
            return CFStringCreateWithCharacters(CFAllocator.Default.Handle, s, s.Length);
        }), true)
        { 
            this.length = s.Length;
        }


        // Constructor.
        CFString(IntPtr s, bool ownsInstance) : base(s, ownsInstance)
        { 
            this.length = -1;
        }


        /// <summary>
        /// Copy string to given buffer.
        /// </summary>
        /// <param name="buffer">Buffer.</param>
        /// <param name="index">Index of position to place copied string.</param>
        public unsafe void CopyTo(char[] buffer, int index)
        {
            this.VerifyReleased();
            var length = this.Length;
            if (index < 0 || index + length > buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(index));
            fixed (char* p = buffer)
            {
                CFStringGetCharacters(this.Handle, new CFRange(0, length), p + index);
            }
        }


        /// <summary>
        /// Get number of characters of string.
        /// </summary>
        public int Length
        {
            get
            {
                if (this.length >= 0)
                    return this.length;
                this.VerifyReleased();
                var length = CFStringGetLength(this.Handle);
                if (length > int.MaxValue)
                    throw new NotSupportedException($"Length of string is too long: {length}");
                this.length = (int)length;
                return (int)length;
            }
        }


        /// <inheritdoc/>
        public override CFObject Retain()
        {
            this.VerifyReleased();
            return new CFString(CFObject.Retain(this.Handle), true)
            {
                length = this.length
            };
        }


        /// <inheritdoc/>
        public override unsafe string? ToString()
        {
            if (this.Handle == IntPtr.Zero)
                return null;
            var buffer = new char[this.Length];
            fixed (char* p = buffer)
            {
                CFStringGetCharacters(this.Handle, new CFRange(0, buffer.Length), p);
                return new string(p);
            }
        }


        /// <summary>
        /// Wrap a native object.
        /// </summary>
        /// <param name="s">Handle of instance.</param>
        /// <param name="ownsInstance">True to .</param>
        /// <returns>Wrapped object.</returns>
        public static new CFString Wrap(IntPtr s, bool ownsInstance = false) =>
            new CFString(s, ownsInstance);
    }
}