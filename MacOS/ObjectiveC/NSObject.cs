using System;
using System.Runtime.InteropServices;

namespace CarinaStudio.MacOS.ObjectiveC
{
    /// <summary>
    /// Object of Objective-C.
    /// </summary>
    public unsafe class NSObject : BaseDisposable, IEquatable<NSObject>
    {
        /// <summary>
        /// Entry point of function to send message to instance (objc_msgSend).
        /// </summary>
        public const string SendMessageEntryPointName = "objc_msgSend";


        // Native symbols.
        static readonly IntPtr objc_msgSend;
        [DllImport(NativeLibraryNames.ObjectiveC)]
        static extern IntPtr object_getClass(IntPtr obj);
        [DllImport(NativeLibraryNames.ObjectiveC, EntryPoint = "object_getIvar")]
        static extern int object_getIvar_Int32(IntPtr obj, IntPtr ivar);
        [DllImport(NativeLibraryNames.ObjectiveC, EntryPoint = "object_getIvar")]
        static extern long object_getIvar_Int64(IntPtr obj, IntPtr ivar);
        /// <summary>
        /// Send message to instance.
        /// </summary>
        [DllImport(NativeLibraryNames.ObjectiveC, EntryPoint = SendMessageEntryPointName)]
        internal protected static extern void SendMessage(IntPtr target, IntPtr selector);
        /// <summary>
        /// Send message to instance.
        /// </summary>
        [DllImport(NativeLibraryNames.ObjectiveC, EntryPoint = SendMessageEntryPointName)]
        internal protected static extern void SendMessage_IntPtr_NSRange(IntPtr target, IntPtr selector, IntPtr arg1, NSRange arg2);
        /// <summary>
        /// Send message to instance.
        /// </summary>
        [DllImport(NativeLibraryNames.ObjectiveC, EntryPoint = SendMessageEntryPointName)]
        internal protected static extern bool SendMessageForBoolean(IntPtr target, IntPtr selector);
        /// <summary>
        /// Send message to instance.
        /// </summary>
        [DllImport(NativeLibraryNames.ObjectiveC, EntryPoint = SendMessageEntryPointName)]
        internal protected static extern bool SendMessageForBoolean_IntPtr(IntPtr target, IntPtr selector, IntPtr arg1);
        /// <summary>
        /// Send message to instance.
        /// </summary>
        [DllImport(NativeLibraryNames.ObjectiveC, EntryPoint = SendMessageEntryPointName)]
        internal protected static extern int SendMessageForInt32(IntPtr target, IntPtr selector);
        /// <summary>
        /// Send message to instance.
        /// </summary>
        [DllImport(NativeLibraryNames.ObjectiveC, EntryPoint = SendMessageEntryPointName)]
        internal protected static extern int SendMessageForInt32_IntPtr(IntPtr target, IntPtr selector, IntPtr arg1);
        /// <summary>
        /// Send message to instance.
        /// </summary>
        [DllImport(NativeLibraryNames.ObjectiveC, EntryPoint = SendMessageEntryPointName)]
        internal protected static extern int SendMessageForInt64(IntPtr target, IntPtr selector);
        /// <summary>
        /// Send message to instance.
        /// </summary>
        [DllImport(NativeLibraryNames.ObjectiveC, EntryPoint = SendMessageEntryPointName)]
        internal protected static extern IntPtr SendMessageForIntPtr(IntPtr target, IntPtr selector);
        /// <summary>
        /// Send message to instance.
        /// </summary>
        [DllImport(NativeLibraryNames.ObjectiveC, EntryPoint = SendMessageEntryPointName)]
        internal protected static extern IntPtr SendMessageForIntPtr_IntPtr_Int32(IntPtr target, IntPtr selector, IntPtr arg1, int arg2);


        // Static fields.
        static readonly Selector? DeallocSelector;
        static readonly Selector? InitSelector;


        // Fields.
        volatile IntPtr handle;
        volatile PropertyDescriptor? hashProperty;
        volatile bool ownsInstance;


        // Static initializer.
        static NSObject()
        {
            if (Platform.IsNotMacOS)
                return;
            var libHandle = NativeLibrary.Load(NativeLibraryNames.ObjectiveC);
            if (libHandle != IntPtr.Zero)
            {
                objc_msgSend = *(IntPtr*)NativeLibrary.GetExport(libHandle, "objc_msgSend");
            }
            DeallocSelector = Selector.GetOrCreate("dealloc");
            InitSelector = Selector.GetOrCreate("init");
        }


        /// <summary>
        /// Initialize new <see cref="NSObject"/> instance.
        /// </summary>
        /// <param name="handle">Handle of instance.</param>
        /// <param name="ownsInstance">True to own the instance.</param>
        protected NSObject(IntPtr handle, bool ownsInstance)
        {
            if (handle == IntPtr.Zero)
                throw new ArgumentException("Handle of instance cannot be null.");
            this.handle = handle;
            this.ownsInstance = ownsInstance;
        }


        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (this.IsDefaultInstance && disposing)
                throw new InvalidOperationException("Cannot dispose default instance.");
            if (this.handle != IntPtr.Zero && this.ownsInstance)
                SendMessage(this.handle, DeallocSelector!.Handle);
            this.handle = IntPtr.Zero;
        }


        /// <inheritdoc/>
        public bool Equals(NSObject? obj) =>
            obj != null && obj.handle == this.handle;


        /// <inheritdoc/>
        public override bool Equals(object? obj) =>
            obj is NSObject nsObj && this.Equals(nsObj);
        

        /// <summary>
        /// Get <see cref="Class"/> of the instance.
        /// </summary>
        /// <returns><see cref="Class"/>.</returns>
        public virtual Class GetClass()
        {
            this.VerifyDisposed();
            return Class.Wrap(object_getClass(this.handle));
        }


        /// <inheritdoc/>
        public override int GetHashCode()
        {
            var property = this.hashProperty 
                ?? (this.GetClass().TryFindProperty("hash", out var prop)
                    ? prop.Also(it => this.hashProperty = prop)
                    : null);
            return property != null
                ? this.SendMessageForInt32(property.Getter!)
                : (int)(this.handle.ToInt64() & 0x7fffffff);
        }
        

        /// <summary>
        /// Get instance variable as <see cref="Int32"/>.
        /// </summary>
        /// <param name="ivar">Descriptor of instance variable.</param>
        /// <returns>Value of variable.</returns>
        public int GetInt32Variable(MemberDescriptor ivar)
        {
            this.VerifyDisposed();
            return object_getIvar_Int32(this.handle, ivar.Handle);
        }


        /// <summary>
        /// Get instance variable as <see cref="Int64"/>.
        /// </summary>
        /// <param name="ivar">Descriptor of instance variable.</param>
        /// <returns>Value of variable.</returns>
        public long GetInt64Variable(MemberDescriptor ivar)
        {
            this.VerifyDisposed();
            return object_getIvar_Int64(this.handle, ivar.Handle);
        }


        /// <summary>
        /// Get handle of instance.
        /// </summary>
        public IntPtr Handle { get => this.handle; }


        /// <summary>
        /// Call default initializer (init) without parameter.
        /// </summary>
        /// <param name="obj">Handle of uninitialized instance.</param>
        /// <returns>Handle of initialized instance.</returns>
        protected static IntPtr Initialize(IntPtr obj)
        {
            if (obj == IntPtr.Zero)
                throw new ArgumentException("Handle of instance cannot be null.");
            return SendMessageForIntPtr(obj, InitSelector!.Handle);
        }


        /// <summary>
        /// Check whether the instance is default instance which cannot be disposed.
        /// </summary>
        public bool IsDefaultInstance { get; protected set; }


        /// <summary>
        /// Equality operator.
        /// </summary>
        public static bool operator ==(NSObject? l, NSObject? r)
        {
            var lHandle = l?.handle ?? IntPtr.Zero;
            var rHandle = r?.handle ?? IntPtr.Zero;
            return lHandle == rHandle;
        }


        /// <summary>
        /// Inequality operator.
        /// </summary>
        public static bool operator !=(NSObject? l, NSObject? r)
        {
            var lHandle = l?.handle ?? IntPtr.Zero;
            var rHandle = r?.handle ?? IntPtr.Zero;
            return lHandle != rHandle;
        }


        /// <summary>
        /// Send message to instance.
        /// </summary>
        /// <param name="selector">Selector.</param>
        public void SendMessage(Selector selector)
        {
            this.VerifyDisposed();
            SendMessage(this.handle, selector.Handle);
        }


        /// <summary>
        /// Send message to instance and get result as <see cref="Int32"/>.
        /// </summary>
        /// <param name="selector">Selector.</param>
        /// <returns>Result.</returns>
        public int SendMessageForInt32(Selector selector)
        {
            this.VerifyDisposed();
            return SendMessageForInt32(this.handle, selector.Handle);
        }


        /// <summary>
        /// Send message to instance and get result as <see cref="Int64"/>.
        /// </summary>
        /// <param name="selector">Selector.</param>
        /// <returns>Result.</returns>
        public long SendMessageForInt64(Selector selector)
        {
            this.VerifyDisposed();
            return SendMessageForInt64(this.handle, selector.Handle);
        }


        /// <summary>
        /// Send message to instance and get result as <see cref="IntPtr"/>.
        /// </summary>
        /// <param name="selector">Selector.</param>
        /// <returns>Result.</returns>
        public IntPtr SendMessageForIntPtr(Selector selector)
        {
            this.VerifyDisposed();
            return SendMessageForIntPtr(this.handle, selector.Handle);
        }


        /// <inheritdoc/>
        public override string ToString() =>
            string.Format("0x{0:x16}", this.handle);
    }
}