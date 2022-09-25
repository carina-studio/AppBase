using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
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
        internal protected static extern void SendMessage_Boolean(IntPtr target, IntPtr selector, bool arg1);
        /// <summary>
        /// Send message to instance.
        /// </summary>
        [DllImport(NativeLibraryNames.ObjectiveC, EntryPoint = SendMessageEntryPointName)]
        internal protected static extern void SendMessage_Int32(IntPtr target, IntPtr selector, int arg1);
        /// <summary>
        /// Send message to instance.
        /// </summary>
        [DllImport(NativeLibraryNames.ObjectiveC, EntryPoint = SendMessageEntryPointName)]
        internal protected static extern void SendMessage_Int64(IntPtr target, IntPtr selector, long arg1);
        /// <summary>
        /// Send message to instance.
        /// </summary>
        [DllImport(NativeLibraryNames.ObjectiveC, EntryPoint = SendMessageEntryPointName)]
        internal protected static extern void SendMessage_IntPtr(IntPtr target, IntPtr selector, IntPtr arg1);
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
        static readonly IDictionary<Type, MethodInfo> WrappingMethods = new ConcurrentDictionary<Type, MethodInfo>();


        // Fields.
        volatile Class? cls;
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
            DeallocSelector = Selector.FromName("dealloc");
            InitSelector = Selector.FromName("init");
        }


        /// <summary>
        /// Initialize new <see cref="NSObject"/> instance.
        /// </summary>
        /// <param name="handle">Handle of instance.</param>
        /// <param name="ownsInstance">True to own the instance.</param>
        internal protected NSObject(IntPtr handle, bool ownsInstance)
        {
            if (handle == IntPtr.Zero)
                throw new ArgumentException("Handle of instance cannot be null.");
            this.handle = handle;
            this.ownsInstance = ownsInstance;
        }


        /// <summary>
        /// Cast instance as given type.
        /// </summary>
        /// <typeparam name="T">Type.</typeparam>
        /// <returns>Casted instance.</returns>
        public T Cast<T>() where T : NSObject
        {
            this.VerifyDisposed();
            return (T)GetWrappingMethod<T>().Invoke(null, new object?[] { this.handle, this.ownsInstance }).AsNonNull();
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
        /// Get value of property as <see cref="Boolean"/>.
        /// </summary>
        /// <param name="property">Property.</param>
        /// <returns>Value.</returns>
        public bool GetBooleanProperty(PropertyDescriptor property)
        {
            if (!property.Class.IsAssignableFrom(this.GetClass()))
                throw new ArgumentException($"Property '{property}' is not owned by class '{this.GetClass()}'.");
            return SendMessageForBoolean(this.handle, property.Getter!.Handle);
        }


        /// <summary>
        /// Get value of property as <see cref="Int32"/>.
        /// </summary>
        /// <param name="property">Property.</param>
        /// <returns>Value.</returns>
        public int GetInt32Property(PropertyDescriptor property)
        {
            if (!property.Class.IsAssignableFrom(this.GetClass()))
                throw new ArgumentException($"Property '{property}' is not owned by class '{this.GetClass()}'.");
            return SendMessageForInt32(this.handle, property.Getter!.Handle);
        }


        /// <summary>
        /// Get value of property as <see cref="Int64"/>.
        /// </summary>
        /// <param name="property">Property.</param>
        /// <returns>Value.</returns>
        public long GetInt64Property(PropertyDescriptor property)
        {
            if (!property.Class.IsAssignableFrom(this.GetClass()))
                throw new ArgumentException($"Property '{property}' is not owned by class '{this.GetClass()}'.");
            return SendMessageForInt64(this.handle, property.Getter!.Handle);
        }


        /// <summary>
        /// Get value of property as <see cref="IntPtr"/>.
        /// </summary>
        /// <param name="property">Property.</param>
        /// <returns>Value.</returns>
        public IntPtr GetIntPtrProperty(PropertyDescriptor property)
        {
            if (!property.Class.IsAssignableFrom(this.GetClass()))
                throw new ArgumentException($"Property '{property}' is not owned by class '{this.GetClass()}'.");
            return SendMessageForIntPtr(this.handle, property.Getter!.Handle);
        }


        /// <summary>
        /// Get value of property as <see cref="NSObject"/>.
        /// </summary>
        /// <param name="property">Property.</param>
        /// <returns>Value.</returns>
        public T? GetObjectProperty<T>(PropertyDescriptor property) where T : NSObject
        {
            if (!property.Class.IsAssignableFrom(this.GetClass()))
                throw new ArgumentException($"Property '{property}' is not owned by class '{this.GetClass()}'.");
            var handle = SendMessageForIntPtr(this.handle, property.Getter!.Handle);
            return handle == IntPtr.Zero ? null : NSObject.Wrap<T>(handle, false);
        }
        

        /// <summary>
        /// Get <see cref="Class"/> of the instance.
        /// </summary>
        /// <returns><see cref="Class"/>.</returns>
        public virtual Class GetClass()
        {
            this.VerifyDisposed();
            return this.cls ?? Class.GetClass(this.handle).Also(it => this.cls = it);
        }


        /// <inheritdoc/>
        public override int GetHashCode()
        {
            var property = this.hashProperty 
                ?? (this.GetClass().TryGetProperty("hash", out var prop)
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


        // Get static method to wrap native instance.
        static MethodInfo GetWrappingMethod<T>() where T : NSObject =>
            WrappingMethods.TryGetValue(typeof(T), out var method)
                ? method
                : typeof(T).GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static).Let(it =>
                {
                    foreach (var m in it)
                    {
                        if (m.Name == "Wrap" && typeof(T).IsAssignableFrom(m.ReturnType))
                        {
                            var parameters = m.GetParameters();
                            if (parameters.Length == 2 
                                && parameters[0].ParameterType == typeof(IntPtr)
                                && parameters[1].ParameterType == typeof(bool))
                            {
                                WrappingMethods.TryAdd(typeof(T), m);
                                return m;
                            }
                        }
                    }
                    throw new InvalidCastException($"Cannot find method to wrap NSObject as '{typeof(T)}'.");
                });


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


        /// <summary>
        /// Set value of property.
        /// </summary>
        /// <param name="property">Property.</param>
        /// <param name="value">Value.</param>
        public void SetProperty(PropertyDescriptor property, bool value)
        {
            if (!property.Class.IsAssignableFrom(this.GetClass()))
                throw new ArgumentException($"Property '{property}' is not owned by class '{this.GetClass()}'.");
            SendMessage_Boolean(this.handle, property.Setter!.Handle, value);
        }


        /// <summary>
        /// Set value of property.
        /// </summary>
        /// <param name="property">Property.</param>
        /// <param name="value">Value.</param>
        public void SetProperty(PropertyDescriptor property, int value)
        {
            if (!property.Class.IsAssignableFrom(this.GetClass()))
                throw new ArgumentException($"Property '{property}' is not owned by class '{this.GetClass()}'.");
            SendMessage_Int32(this.handle, property.Setter!.Handle, value);
        }


        /// <summary>
        /// Set value of property.
        /// </summary>
        /// <param name="property">Property.</param>
        /// <param name="value">Value.</param>
        public void SetProperty(PropertyDescriptor property, long value)
        {
            if (!property.Class.IsAssignableFrom(this.GetClass()))
                throw new ArgumentException($"Property '{property}' is not owned by class '{this.GetClass()}'.");
            SendMessage_Int64(this.handle, property.Setter!.Handle, value);
        }


        /// <summary>
        /// Set value of property.
        /// </summary>
        /// <param name="property">Property.</param>
        /// <param name="value">Value.</param>
        public void SetProperty(PropertyDescriptor property, IntPtr value)
        {
            if (!property.Class.IsAssignableFrom(this.GetClass()))
                throw new ArgumentException($"Property '{property}' is not owned by class '{this.GetClass()}'.");
            SendMessage_IntPtr(this.handle, property.Setter!.Handle, value);
        }


        /// <summary>
        /// Set value of property.
        /// </summary>
        /// <param name="property">Property.</param>
        /// <param name="value">Value.</param>
        public void SetProperty(PropertyDescriptor property, NSObject? value)
        {
            if (!property.Class.IsAssignableFrom(this.GetClass()))
                throw new ArgumentException($"Property '{property}' is not owned by class '{this.GetClass()}'.");
            SendMessage_IntPtr(this.handle, property.Setter!.Handle, value?.handle ?? IntPtr.Zero);
        }


        /// <inheritdoc/>
        public override string ToString() =>
            string.Format("0x{0:x16}", this.handle);
        

        /// <summary>
        /// Wrap given handle as <see cref="NSObject"/>.
        /// </summary>
        /// <param name="handle">Handle of instance.</param>
        /// <param name="ownsInstance">True to owns instance.</param>
        /// <returns>Wrapped instance.</returns>
        public static NSObject Wrap(IntPtr handle, bool ownsInstance = false) =>
            new NSObject(handle, ownsInstance);
        

        /// <summary>
        /// Wrap given handle as given type.
        /// </summary>
        /// <param name="handle">Handle of instance.</param>
        /// <param name="ownsInstance">True to owns instance.</param>
        /// <typeparam name="T">Type to wrap the instance.</typeparam>
        /// <returns>Wrapped instance.</returns>
        public static T Wrap<T>(IntPtr handle, bool ownsInstance = false) where T : NSObject
        {
            if (typeof(T) == typeof(NSObject))
                return (T)Wrap(handle, ownsInstance);
            return (T)GetWrappingMethod<T>().Invoke(null, new object?[] { handle, ownsInstance }).AsNonNull();
        }
    }
}