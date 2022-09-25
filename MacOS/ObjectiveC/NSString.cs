using System;
using System.Runtime.InteropServices;

namespace CarinaStudio.MacOS.ObjectiveC
{
    /// <summary>
    /// NSString.
    /// </summary>
#pragma warning disable CS0659
#pragma warning disable CS0661
    public class NSString : NSObject, IComparable<NSString>, IEquatable<NSString>
#pragma warning restore CS0659
#pragma warning restore CS0661
    {
        // Static fields.
        static readonly Selector? CompareSelector;
        static readonly Selector? GetCharsSelector;
        static readonly Selector? InitWithCharSelector;
        static readonly Selector? IsEqualToSelector;
        static readonly PropertyDescriptor? LengthProperty;
        static readonly Class? NSStringClass = Class.GetClass("NSString");


        // Fields.
        volatile WeakReference<string>? stringRef;


        // Static initializer.
        static NSString()
        {
            if (NSStringClass != null)
            {
                CompareSelector = Selector.FromName("compare:");
                GetCharsSelector = Selector.FromName("getCharacters:range:");
                InitWithCharSelector = Selector.FromName("initWithCharacters:length:");
                IsEqualToSelector = Selector.FromName("isEqualTo:");
                NSStringClass.TryGetProperty("length", out LengthProperty);
            }
        }


        /// <summary>
        /// Initialize new <see cref="NSString"/> instance.
        /// </summary>
        public NSString() : base(Initialize(NSStringClass!.Allocate()), true)
        { }


        /// <summary>
        /// Initialize new <see cref="NSString"/> instance.
        /// </summary>
        /// <param name="s">Characters.</param>
        public NSString(string s) : base(Initialize(NSStringClass!.Allocate(), s), true)
        { 
            this.stringRef = new WeakReference<string>(s);
        }


        // Constructor.
        NSString(InstanceHolder instance, bool ownsInstance) : base(instance, ownsInstance) =>
            this.VerifyClass(NSStringClass!);


        /// <inheritdoc/>
        public int CompareTo(NSString? s)
        {
            if (s == null)
                return 1;
            this.VerifyDisposed();
            s.VerifyDisposed();
            return SendMessageForInt32_IntPtr(this.Handle, CompareSelector!.Handle, s.Handle);
        }


        /// <inheritdoc/>
        public bool Equals(NSString? s)
        {
            if (s == null || s.IsDisposed || this.IsDisposed)
                return false;
            return SendMessageForBoolean_IntPtr(this.Handle, IsEqualToSelector!.Handle, s.Handle);
        }


        /// <inheritdoc/>
        public override bool Equals(object? obj) =>
            obj is NSString s && this.Equals(s);


        // Initialize.
        static IntPtr Initialize(IntPtr obj, string s)
        {
            var pStr = Marshal.StringToHGlobalUni(s);
            var newObj = SendMessageForIntPtr_IntPtr_Int32(obj, InitWithCharSelector!.Handle, pStr, s.Length);
            Marshal.FreeHGlobal(pStr);
            return newObj;
        }
        

        /// <summary>
        /// Get number of characters.
        /// </summary>
        public int Length { get => this.SendMessageForInt32(LengthProperty!.Getter!); }


        /// <summary>
        /// Equality operator.
        /// </summary>
        public static bool operator ==(NSString? l, NSString? r) =>
            l?.Equals(r) ?? r is null;
        

        /// <summary>
        /// Inequality operator.
        /// </summary>
        public static bool operator !=(NSString? l, NSString? r) =>
            l?.Equals(r) == true ? false : ((l is null) != (r is null));


        /// <inheritdoc/>
        public override unsafe string ToString()
        {
            if (this.IsDisposed)
                return "";
            if (this.stringRef?.TryGetTarget(out var s) == true)
                return s;
            var length = this.Length;
            var buffer = new char[length];
            fixed (char* p = buffer)
                SendMessage_IntPtr_NSRange(this.Handle, GetCharsSelector!.Handle, (IntPtr)p, new NSRange(0, length));
            s = new string(buffer);
            this.stringRef = new WeakReference<string>(s);
            return s;
        }
    }
}