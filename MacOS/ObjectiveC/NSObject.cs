using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;

namespace CarinaStudio.MacOS.ObjectiveC
{
    /// <summary>
    /// Object of Objective-C.
    /// </summary>
    public unsafe class NSObject : IDisposable, IEquatable<NSObject>
    {
        /// <summary>
        /// Holder of native instance.
        /// </summary>
        public class InstanceHolder : IEquatable<InstanceHolder>
        {
            // Constructor.
            internal InstanceHolder(IntPtr handle)
            { 
                this.Class = Class.GetClass(handle);
                this.Handle = handle;
            }
            internal InstanceHolder(IntPtr handle, Class cls)
            {
                if (handle == IntPtr.Zero)
                    throw new ArgumentException("Handle of instance cannot be null.");
                this.Class = cls;
                this.Handle = handle;
            }

            /// <summary>
            /// Get class of instance.
            /// </summary>
            public Class Class { get; }

            /// <inheritdoc/>
            public bool Equals(InstanceHolder? holder) =>
                holder is not null && this.Handle == holder.Handle;

            /// <inheritdoc/>
            public override bool Equals(object? obj) =>
                obj is InstanceHolder holder && this.Equals(holder);

            /// <inheritdoc/>
            public override int GetHashCode() =>
                (int)(this.Handle.ToInt64() & 0x7fffffff);

            /// <summary>
            /// Get handle of instance.
            /// </summary>
            public IntPtr Handle { get; internal set; }

            /// <summary>
            /// Equality operator.
            /// </summary>
            public static bool operator ==(InstanceHolder l, InstanceHolder r) =>
                l.Handle == r.Handle;
            
            /// <summary>
            /// Inequality operator.
            /// </summary>
            public static bool operator !=(InstanceHolder l, InstanceHolder r) =>
                l.Handle != r.Handle;
        }


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
        /// <summary>
        /// Send message to instance.
        /// </summary>
        [DllImport(NativeLibraryNames.ObjectiveC, EntryPoint = SendMessageEntryPointName)]
        internal protected static extern NSSize SendMessageForNSSize(IntPtr target, IntPtr selector);


        // Static fields.
        static readonly Selector? DeallocSelector;
        static readonly Selector? InitSelector;
        static readonly IDictionary<Type, ConstructorInfo> WrappingConstructors = new ConcurrentDictionary<Type, ConstructorInfo>();
        static readonly IDictionary<Type, Func<InstanceHolder, bool, NSObject>> WrappingMethods = new ConcurrentDictionary<Type, Func<InstanceHolder, bool, NSObject>>();


        // Fields.
        volatile PropertyDescriptor? hashProperty;
        readonly InstanceHolder instance;
        volatile int isDisposed;
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
        /// <param name="instance">Native instance.</param>
        /// <param name="ownsInstance">True to own the instance.</param>
        internal protected NSObject(InstanceHolder instance, bool ownsInstance)
        {
            this.instance = instance;
            this.ownsInstance = ownsInstance;
        }


        /// <summary>
        /// Initialize new <see cref="NSObject"/> instance.
        /// </summary>
        /// <param name="handle">Handle of instance.</param>
        /// <param name="ownsInstance">True to own the instance.</param>
        internal protected NSObject(IntPtr handle, bool ownsInstance)
        {
            this.instance = new(handle);
            this.ownsInstance = ownsInstance;
        }


        /// <summary>
        /// Finalizer.
        /// </summary>
        ~NSObject() =>
            this.Dispose(false);


        /// <summary>
        /// Cast instance as given type.
        /// </summary>
        /// <typeparam name="T">Type.</typeparam>
        /// <returns>Casted instance.</returns>
        public T Cast<T>() where T : NSObject
        {
            if (this is T target)
                return target;
            var ctor = GetWrappingConstructor<T>();
            if (ctor.GetParameters().Length == 2)
                return (T)Activator.CreateInstance(typeof(T), BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null, new object?[]{ this.instance, this.ownsInstance }, null).AsNonNull();
            return (T)Activator.CreateInstance(typeof(T), BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null, new object?[]{ this.instance }, null).AsNonNull();
        }


        /// <summary>
        /// Get class of instance.
        /// </summary>
        public Class Class { get => this.instance.Class; }


        /// <inheritdoc/>
        public void Dispose()
        {
            if (Interlocked.Exchange(ref this.isDisposed, 1) != 0)
                return;
            this.Dispose(true);
        }


        /// <summary>
        /// Dispose the instance.
        /// </summary>
        /// <param name="disposing">True to dispose managed resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (this.IsDefaultInstance && disposing)
                throw new InvalidOperationException("Cannot dispose default instance.");
            if (this.instance.Handle != IntPtr.Zero && this.ownsInstance)
                SendMessage(this.instance.Handle, DeallocSelector!.Handle);
            this.instance.Handle = IntPtr.Zero;
        }


        /// <inheritdoc/>
        public bool Equals(NSObject? obj) =>
            obj != null && obj.instance == this.instance;


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
            if (!property.Class.IsAssignableFrom(this.Class))
                throw new ArgumentException($"Property '{property}' is not owned by class '{this.Class}'.");
            return SendMessageForBoolean(this.Handle, property.Getter!.Handle);
        }


        /// <summary>
        /// Get value of property as <see cref="Int32"/>.
        /// </summary>
        /// <param name="property">Property.</param>
        /// <returns>Value.</returns>
        public int GetInt32Property(PropertyDescriptor property)
        {
            if (!property.Class.IsAssignableFrom(this.Class))
                throw new ArgumentException($"Property '{property}' is not owned by class '{this.Class}'.");
            return SendMessageForInt32(this.Handle, property.Getter!.Handle);
        }


        /// <summary>
        /// Get value of property as <see cref="Int64"/>.
        /// </summary>
        /// <param name="property">Property.</param>
        /// <returns>Value.</returns>
        public long GetInt64Property(PropertyDescriptor property)
        {
            if (!property.Class.IsAssignableFrom(this.Class))
                throw new ArgumentException($"Property '{property}' is not owned by class '{this.Class}'.");
            return SendMessageForInt64(this.Handle, property.Getter!.Handle);
        }


        /// <summary>
        /// Get value of property as <see cref="IntPtr"/>.
        /// </summary>
        /// <param name="property">Property.</param>
        /// <returns>Value.</returns>
        public IntPtr GetIntPtrProperty(PropertyDescriptor property)
        {
            if (!property.Class.IsAssignableFrom(this.Class))
                throw new ArgumentException($"Property '{property}' is not owned by class '{this.Class}'.");
            return SendMessageForIntPtr(this.Handle, property.Getter!.Handle);
        }


        /// <summary>
        /// Get value of property as <see cref="NSObject"/>.
        /// </summary>
        /// <param name="property">Property.</param>
        /// <returns>Value.</returns>
        public T? GetObjectProperty<T>(PropertyDescriptor property) where T : NSObject
        {
            if (!property.Class.IsAssignableFrom(this.Class))
                throw new ArgumentException($"Property '{property}' is not owned by class '{this.Class}'.");
            var handle = SendMessageForIntPtr(this.Handle, property.Getter!.Handle);
            return handle == IntPtr.Zero ? null : NSObject.Wrap<T>(handle, false);
        }


        /// <inheritdoc/>
        public override int GetHashCode()
        {
            var property = this.hashProperty 
                ?? (this.Class.TryGetProperty("hash", out var prop)
                    ? prop.Also(it => this.hashProperty = prop)
                    : null);
            return property != null
                ? this.SendMessageForInt32(property.Getter!)
                : this.instance.GetHashCode();
        }
        

        /// <summary>
        /// Get instance variable as <see cref="Int32"/>.
        /// </summary>
        /// <param name="ivar">Descriptor of instance variable.</param>
        /// <returns>Value of variable.</returns>
        public int GetInt32Variable(MemberDescriptor ivar)
        {
            this.VerifyDisposed();
            return object_getIvar_Int32(this.Handle, ivar.Handle);
        }


        /// <summary>
        /// Get instance variable as <see cref="Int64"/>.
        /// </summary>
        /// <param name="ivar">Descriptor of instance variable.</param>
        /// <returns>Value of variable.</returns>
        public long GetInt64Variable(MemberDescriptor ivar)
        {
            this.VerifyDisposed();
            return object_getIvar_Int64(this.Handle, ivar.Handle);
        }


        // Get static method to wrap native instance.
        static ConstructorInfo GetWrappingConstructor<T>() where T : NSObject =>
            WrappingConstructors.TryGetValue(typeof(T), out var method)
                ? method
                : typeof(T).GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).Let(it =>
                {
                    var CtorWith1Arg = (ConstructorInfo?)null;
                    foreach (var ctor in it)
                    {
                        var parameters = ctor.GetParameters();
                        switch (parameters.Length)
                        {
                            case 1:
                                if (parameters[0].ParameterType == typeof(InstanceHolder))
                                    CtorWith1Arg = ctor;
                                break;
                            case 2:
                                if (parameters[0].ParameterType == typeof(InstanceHolder)
                                    && parameters[1].ParameterType == typeof(bool))
                                {
                                    WrappingConstructors.TryAdd(typeof(T), ctor);
                                    return ctor;
                                }
                                break;
                        }
                    }
                    if (CtorWith1Arg != null)
                    {
                        WrappingConstructors.TryAdd(typeof(T), CtorWith1Arg);
                        return CtorWith1Arg;
                    }
                    throw new InvalidCastException($"Cannot find way to construct {typeof(T).Name}.");
                });


        // Get static method to wrap native instance.
        static Func<InstanceHolder, bool, NSObject> GetWrappingMethod<T>() where T : NSObject =>
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
                                && parameters[0].ParameterType == typeof(InstanceHolder)
                                && parameters[1].ParameterType == typeof(bool))
                            {
                                var func = (Func<InstanceHolder, bool, NSObject>)Delegate.CreateDelegate(typeof(Func<InstanceHolder, bool, NSObject>), null, m);
                                WrappingMethods.TryAdd(typeof(T), func);
                                return func;
                            }
                        }
                    }
                    throw new InvalidCastException($"Cannot find method to wrap NSObject as '{typeof(T)}'.");
                });


        /// <summary>
        /// Get handle of instance.
        /// </summary>
        public IntPtr Handle { get => this.instance.Handle; }


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
        /// Check whether instance has been disposed or not.
        /// </summary>
        public bool IsDisposed { get => this.isDisposed != 0; }


        /// <summary>
        /// Equality operator.
        /// </summary>
        public static bool operator ==(NSObject? l, NSObject? r)
        {
            var lHandle = l?.instance?.Handle ?? IntPtr.Zero;
            var rHandle = r?.instance?.Handle ?? IntPtr.Zero;
            return lHandle == rHandle;
        }


        /// <summary>
        /// Inequality operator.
        /// </summary>
        public static bool operator !=(NSObject? l, NSObject? r)
        {
            var lHandle = l?.instance?.Handle ?? IntPtr.Zero;
            var rHandle = r?.instance?.Handle ?? IntPtr.Zero;
            return lHandle != rHandle;
        }


        /// <summary>
        /// Send message to instance.
        /// </summary>
        /// <param name="selector">Selector.</param>
        public void SendMessage(Selector selector)
        {
            this.VerifyDisposed();
            SendMessage(this.Handle, selector.Handle);
        }


        /// <summary>
        /// Send message to instance and get result as <see cref="Int32"/>.
        /// </summary>
        /// <param name="selector">Selector.</param>
        /// <returns>Result.</returns>
        public int SendMessageForInt32(Selector selector)
        {
            this.VerifyDisposed();
            return SendMessageForInt32(this.Handle, selector.Handle);
        }


        /// <summary>
        /// Send message to instance and get result as <see cref="Int64"/>.
        /// </summary>
        /// <param name="selector">Selector.</param>
        /// <returns>Result.</returns>
        public long SendMessageForInt64(Selector selector)
        {
            this.VerifyDisposed();
            return SendMessageForInt64(this.Handle, selector.Handle);
        }


        /// <summary>
        /// Send message to instance and get result as <see cref="IntPtr"/>.
        /// </summary>
        /// <param name="selector">Selector.</param>
        /// <returns>Result.</returns>
        public IntPtr SendMessageForIntPtr(Selector selector)
        {
            this.VerifyDisposed();
            return SendMessageForIntPtr(this.Handle, selector.Handle);
        }


        /// <summary>
        /// Set value of property.
        /// </summary>
        /// <param name="property">Property.</param>
        /// <param name="value">Value.</param>
        public void SetProperty(PropertyDescriptor property, bool value)
        {
            if (!property.Class.IsAssignableFrom(this.Class))
                throw new ArgumentException($"Property '{property}' is not owned by class '{this.Class}'.");
            SendMessage_Boolean(this.Handle, property.Setter!.Handle, value);
        }


        /// <summary>
        /// Set value of property.
        /// </summary>
        /// <param name="property">Property.</param>
        /// <param name="value">Value.</param>
        public void SetProperty(PropertyDescriptor property, int value)
        {
            if (!property.Class.IsAssignableFrom(this.Class))
                throw new ArgumentException($"Property '{property}' is not owned by class '{this.Class}'.");
            SendMessage_Int32(this.Handle, property.Setter!.Handle, value);
        }


        /// <summary>
        /// Set value of property.
        /// </summary>
        /// <param name="property">Property.</param>
        /// <param name="value">Value.</param>
        public void SetProperty(PropertyDescriptor property, long value)
        {
            if (!property.Class.IsAssignableFrom(this.Class))
                throw new ArgumentException($"Property '{property}' is not owned by class '{this.Class}'.");
            SendMessage_Int64(this.Handle, property.Setter!.Handle, value);
        }


        /// <summary>
        /// Set value of property.
        /// </summary>
        /// <param name="property">Property.</param>
        /// <param name="value">Value.</param>
        public void SetProperty(PropertyDescriptor property, IntPtr value)
        {
            if (!property.Class.IsAssignableFrom(this.Class))
                throw new ArgumentException($"Property '{property}' is not owned by class '{this.Class}'.");
            SendMessage_IntPtr(this.Handle, property.Setter!.Handle, value);
        }


        /// <summary>
        /// Set value of property.
        /// </summary>
        /// <param name="property">Property.</param>
        /// <param name="value">Value.</param>
        public void SetProperty(PropertyDescriptor property, NSObject? value)
        {
            if (!property.Class.IsAssignableFrom(this.Class))
                throw new ArgumentException($"Property '{property}' is not owned by class '{this.Class}'.");
            SendMessage_IntPtr(this.Handle, property.Setter!.Handle, value?.Handle ?? IntPtr.Zero);
        }


        /// <inheritdoc/>
        public override string ToString() =>
            string.Format("0x{0:x16}", this.Handle);
        

        /// <summary>
        /// Throw <see cref="ArgumentException"/> if the class of this instance is not assignable to given class.
        /// </summary>
        /// <param name="cls">Given class.</param>
        protected void VerifyClass(Class cls)
        {
            if (!cls.IsAssignableFrom(this.instance.Class))
                throw new ArgumentException($"Instance is not {cls}.");
        }
        

        /// <summary>
        /// Throw <see cref="ObjectDisposedException"/> if instance has been disposed.
        /// </summary>
        protected void VerifyDisposed()
        {
            if (this.IsDisposed)
                throw new ObjectDisposedException(this.GetType().Name);
        }
        

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
            var ctor = GetWrappingConstructor<T>();
            var instance = new InstanceHolder(handle);
            if (ctor.GetParameters().Length == 2)
                return (T)Activator.CreateInstance(typeof(T), BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null, new object?[]{ instance, ownsInstance }, null).AsNonNull();
            return (T)Activator.CreateInstance(typeof(T), BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null, new object?[]{ instance }, null).AsNonNull();
        }
    }
}