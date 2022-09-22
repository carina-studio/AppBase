using System;

namespace CarinaStudio.MacOS.CoreFoundation
{
    /// <summary>
    /// String.
    /// </summary>
    public class CFString : CFObject
    {
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
            return Native.CFStringCreateWithCharacters(Native.DefaultAllocator, s, s.Length);
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
                Native.CFStringGetCharacters(this.Handle, new CFRange(0, length), p + index);
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
                var length = Native.CFStringGetLength(this.Handle);
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
            return new CFString(Native.CFRetain(this.Handle), true)
            {
                length = this.length
            };
        }


        /// <inheritdoc/>
        public override unsafe string? ToString()
        {
            if (this.Handle == IntPtr.Zero)
                return null;
            var buffer = new char[this.length];
            fixed (char* p = buffer)
            {
                Native.CFStringGetCharacters(this.Handle, new CFRange(0, buffer.Length), p);
                return new string(p);
            }
        }


        /// <summary>
        /// Wrap a native object.
        /// </summary>
        /// <param name="s">Handle of instance.</param>
        /// <returns>Wrapped object.</returns>
        public static new CFString Wrap(IntPtr s) =>
            new CFString(s, false);
    }
}