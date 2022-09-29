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
                NSObject.VerifyHandle(handle);
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


        [StructLayout(LayoutKind.Sequential)]
        ref struct objc_super
        {
            public IntPtr receiver;
            public IntPtr super_class;
        }


        // Native symbols.
        static readonly void* objc_msgSend;
        static readonly void* objc_msgSendSuper;
        static readonly void* object_getIvar;
        static readonly void* object_setIvar;


        // Static fields.
        static readonly Selector? DeallocSelector;
        static readonly Selector? InitSelector;
        static readonly IDictionary<Type, ConstructorInfo> WrappingConstructors = new ConcurrentDictionary<Type, ConstructorInfo>();
        

        // Fields.
        volatile Property? hashProperty;
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
                objc_msgSend = (void*)NativeLibrary.GetExport(libHandle, nameof(objc_msgSend));
                objc_msgSendSuper = (void*)NativeLibrary.GetExport(libHandle, nameof(objc_msgSendSuper));
                object_getIvar = (void*)NativeLibrary.GetExport(libHandle, nameof(object_getIvar));
                object_setIvar = (void*)NativeLibrary.GetExport(libHandle, nameof(object_setIvar));
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
                ((delegate*<IntPtr, IntPtr, void>)objc_msgSend)(this.instance.Handle, DeallocSelector!.Handle);
            this.instance.Handle = IntPtr.Zero;
        }


        /// <inheritdoc/>
        public bool Equals(NSObject? obj) =>
            obj != null && obj.instance == this.instance;


        /// <inheritdoc/>
        public override bool Equals(object? obj) =>
            obj is NSObject nsObj && this.Equals(nsObj);
        

        /// <summary>
        /// Wrap given handle as <see cref="NSObject"/>.
        /// </summary>
        /// <param name="handle">Handle of instance.</param>
        /// <param name="ownsInstance">True to owns instance.</param>
        /// <returns>Wrapped instance.</returns>
        public static NSObject FromHandle(IntPtr handle, bool ownsInstance = false) =>
            new NSObject(handle, ownsInstance);
        

        /// <summary>
        /// Wrap given handle as given type.
        /// </summary>
        /// <param name="handle">Handle of instance.</param>
        /// <param name="ownsInstance">True to owns instance.</param>
        /// <typeparam name="T">Type to wrap the instance.</typeparam>
        /// <returns>Wrapped instance.</returns>
        public static T FromHandle<T>(IntPtr handle, bool ownsInstance = false) where T : NSObject =>
            (T)FromHandle(typeof(T), handle, ownsInstance);


        // Wrap given handle as given type.
        internal static NSObject FromHandle(Type type, IntPtr handle, bool ownsInstance = false)
        {
            if (type == typeof(NSObject))
                return FromHandle(handle, ownsInstance);
            var ctor = GetWrappingConstructor(type);
            var instance = new InstanceHolder(handle);
            if (ctor.GetParameters().Length == 2)
                return (NSObject)Activator.CreateInstance(type, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null, new object?[]{ instance, ownsInstance }, null).AsNonNull();
            return (NSObject)Activator.CreateInstance(type, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null, new object?[]{ instance }, null).AsNonNull();
        }
        

        /// <summary>
        /// Get value of property as given type.
        /// </summary>
        /// <param name="property">Property.</param>
        /// <returns>Value.</returns>
        public T GetProperty<T>(Property property)
        {
            if (!property.Class.IsAssignableFrom(this.Class))
                throw new ArgumentException($"Property '{property}' is not owned by class '{this.Class}'.");
            return this.SendMessage<T>(property.Getter!);
        }


        /// <inheritdoc/>
        public override int GetHashCode()
        {
            var property = this.hashProperty 
                ?? (this.Class.TryGetProperty("hash", out var prop)
                    ? prop.Also(it => this.hashProperty = prop)
                    : null);
            return property != null
                ? this.SendMessage<int>(property.Getter!)
                : this.instance.GetHashCode();
        }


        /// <summary>
        /// Get instance variable as given type.
        /// </summary>
        /// <param name="ivar">Descriptor of instance variable.</param>
        /// <typeparam name="T">Type of variable.</typeparam>
        /// <returns>Value of variable.</returns>
        public T GetVariable<T>(Member ivar)
        {
            this.VerifyDisposed();
            return GetVariable<T>(this.Handle, ivar);
        }


#pragma warning disable CS8600
#pragma warning disable CS8603
        /// <summary>
        /// Get instance variable as given type.
        /// </summary>
        /// <param name="obj">Handle of instance.</param>
        /// <param name="ivar">Descriptor of instance variable.</param>
        /// <typeparam name="T">Type of variable.</typeparam>
        /// <returns>Value of variable.</returns>
        public static T GetVariable<T>(IntPtr obj, Member ivar)
        {
            VerifyHandle(obj);
            var isFpStructure = NativeTypeConversion.IsFloatingPointStructure(typeof(T));
            return (T)(NativeTypeConversion.GetNativeValueCount<T>() switch
            {
                1 => Global.Run(() =>
                {
                    if (isFpStructure)
                    {
                        var nr = ((delegate*unmanaged<IntPtr, IntPtr, NativeFpResult1>)object_getIvar)(obj, ivar.Handle);
                        return NativeTypeConversion.FromNativeValue<T>((nint*)&nr, 1);
                    }
                    else
                    {
                        var nr = ((delegate*unmanaged<IntPtr, IntPtr, NativeResult1>)object_getIvar)(obj, ivar.Handle);
                        return NativeTypeConversion.FromNativeValue<T>((nint*)&nr, 1);
                    }
                }),
                2 => Global.Run(() =>
                {
                    if (isFpStructure)
                    {
                        var nr = ((delegate*unmanaged<IntPtr, IntPtr, NativeFpResult2>)object_getIvar)(obj, ivar.Handle);
                        return NativeTypeConversion.FromNativeValue<T>((nint*)&nr, 2);
                    }
                    else
                    {
                        var nr = ((delegate*unmanaged<IntPtr, IntPtr, NativeResult2>)object_getIvar)(obj, ivar.Handle);
                        return NativeTypeConversion.FromNativeValue<T>((nint*)&nr, 2);
                    }
                }),
                3 => Global.Run(() =>
                {
                    if (isFpStructure)
                    {
                        var nr = ((delegate*unmanaged<IntPtr, IntPtr, NativeFpResult3>)object_getIvar)(obj, ivar.Handle);
                        return NativeTypeConversion.FromNativeValue<T>((nint*)&nr, 3);
                    }
                    else
                    {
                        var nr = ((delegate*unmanaged<IntPtr, IntPtr, NativeResult3>)object_getIvar)(obj, ivar.Handle);
                        return NativeTypeConversion.FromNativeValue<T>((nint*)&nr, 3);
                    }
                }),
                4 => Global.Run(() =>
                {
                    if (isFpStructure)
                    {
                        var nr = ((delegate*unmanaged<IntPtr, IntPtr, NativeFpResult4>)object_getIvar)(obj, ivar.Handle);
                        return NativeTypeConversion.FromNativeValue<T>((nint*)&nr, 4);
                    }
                    else
                    {
                        var nr = ((delegate*unmanaged<IntPtr, IntPtr, NativeResult4>)object_getIvar)(obj, ivar.Handle);
                        return NativeTypeConversion.FromNativeValue<T>((nint*)&nr, 4);
                    }
                }),
                _ => throw new NotSupportedException($"Unsupported variable type '{typeof(T).Name}'."),
            });
        }
#pragma warning restore CS8600
#pragma warning restore CS8603


        // Get static method to wrap native instance.
        static ConstructorInfo GetWrappingConstructor<T>() where T : NSObject =>
            GetWrappingConstructor(typeof(T));
        static ConstructorInfo GetWrappingConstructor(Type type) =>
            WrappingConstructors.TryGetValue(type, out var method)
                ? method
                : type.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).Let(it =>
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
                                    WrappingConstructors.TryAdd(type, ctor);
                                    return ctor;
                                }
                                break;
                        }
                    }
                    if (CtorWith1Arg != null)
                    {
                        WrappingConstructors.TryAdd(type, CtorWith1Arg);
                        return CtorWith1Arg;
                    }
                    throw new InvalidCastException($"Cannot find way to construct {type.Name}.");
                });


        /// <summary>
        /// Get handle of instance.
        /// </summary>
        public IntPtr Handle { get => this.isDisposed == 0 ? this.instance.Handle : IntPtr.Zero; }


        /// <summary>
        /// Call default initializer (init) without parameter.
        /// </summary>
        /// <param name="obj">Handle of uninitialized instance.</param>
        /// <returns>Handle of initialized instance.</returns>
        protected static IntPtr Initialize(IntPtr obj)
        {
            VerifyHandle(obj);
            return ((delegate*unmanaged<IntPtr, IntPtr, IntPtr>)objc_msgSend)(obj, InitSelector!.Handle);
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
        /// <param name="args">Arguments</param>
        public void SendMessage(Selector selector, params object?[] args) =>
            SendMessage(this.Handle, selector, args);


        /// <summary>
        /// Send message to instance.
        /// </summary>
        /// <param name="obj">Handle of instance.</param>
        /// <param name="selector">Selector.</param>
        /// <param name="args">Arguments</param>
        public static void SendMessage(IntPtr obj, Selector selector, params object?[] args) =>
            SendMessage<nint>(obj, selector, args);


        /// <summary>
        /// Send message to instance.
        /// </summary>
        /// <param name="selector">Selector.</param>
        /// <param name="args">Arguments</param>
        /// <typeparam name="T">Type of returned value.</typeparam>
        /// <returns>Result.</returns>
        public T SendMessage<T>(Selector selector, params object?[] args) =>
            SendMessage<T>(this.Handle, selector, args);
        

        /// <summary>
        /// Send message to instance.
        /// </summary>
        /// <param name="obj">Handle of instance.</param>
        /// <param name="selector">Selector.</param>
        /// <param name="args">Arguments</param>
        /// <typeparam name="T">Type of returned value.</typeparam>
        /// <returns>Result.</returns>
        public static T SendMessage<T>(IntPtr obj, Selector selector, params object?[] args) =>
            SendMessage<T>(objc_msgSend, obj, selector, args);


#pragma warning disable CS8600
#pragma warning disable CS8603
        // Send message.
        static T SendMessage<T>(void* sendMsgFunction, IntPtr obj, Selector selector, params object?[] args)
        {
            VerifyHandle(obj);
            var nvs = NativeTypeConversion.ToNativeValues(args);
            var isFpResult = NativeTypeConversion.IsFloatingPointStructure(typeof(T));
            return (T)(NativeTypeConversion.GetNativeValueCount<T>() switch
            {
                1 => Global.Run(() =>
                {
                    if (isFpResult)
                    {
                        var nr = nvs.Length switch
                        {
                            0 => ((delegate*unmanaged<IntPtr, IntPtr, NativeFpResult1>)sendMsgFunction)(obj, selector.Handle),
                            1 => ((delegate*unmanaged<IntPtr, IntPtr, nint, NativeFpResult1>)sendMsgFunction)(obj, selector.Handle, nvs[0]),
                            2 => ((delegate*unmanaged<IntPtr, IntPtr, nint, nint, NativeFpResult1>)sendMsgFunction)(obj, selector.Handle, nvs[0], nvs[1]),
                            3 => ((delegate*unmanaged<IntPtr, IntPtr, nint, nint, nint, NativeFpResult1>)sendMsgFunction)(obj, selector.Handle, nvs[0], nvs[1], nvs[2]),
                            4 => ((delegate*unmanaged<IntPtr, IntPtr, nint, nint, nint, nint, NativeFpResult1>)sendMsgFunction)(obj, selector.Handle, nvs[0], nvs[1], nvs[2], nvs[3]),
                            5 => ((delegate*unmanaged<IntPtr, IntPtr, nint, nint, nint, nint, nint, NativeFpResult1>)sendMsgFunction)(obj, selector.Handle, nvs[0], nvs[1], nvs[2], nvs[3], nvs[4]),
                            6 => ((delegate*unmanaged<IntPtr, IntPtr, nint, nint, nint, nint, nint, nint, NativeFpResult1>)sendMsgFunction)(obj, selector.Handle, nvs[0], nvs[1], nvs[2], nvs[3], nvs[4], nvs[5]),
                            _ => throw new NotSupportedException("Too many arguments to send."),
                        };
                        return NativeTypeConversion.FromNativeValue<T>((nint*)&nr, 1);
                    }
                    else
                    {
                        var nr = nvs.Length switch
                        {
                            0 => ((delegate*unmanaged<IntPtr, IntPtr, NativeResult1>)sendMsgFunction)(obj, selector.Handle),
                            1 => ((delegate*unmanaged<IntPtr, IntPtr, nint, NativeResult1>)sendMsgFunction)(obj, selector.Handle, nvs[0]),
                            2 => ((delegate*unmanaged<IntPtr, IntPtr, nint, nint, NativeResult1>)sendMsgFunction)(obj, selector.Handle, nvs[0], nvs[1]),
                            3 => ((delegate*unmanaged<IntPtr, IntPtr, nint, nint, nint, NativeResult1>)sendMsgFunction)(obj, selector.Handle, nvs[0], nvs[1], nvs[2]),
                            4 => ((delegate*unmanaged<IntPtr, IntPtr, nint, nint, nint, nint, NativeResult1>)sendMsgFunction)(obj, selector.Handle, nvs[0], nvs[1], nvs[2], nvs[3]),
                            5 => ((delegate*unmanaged<IntPtr, IntPtr, nint, nint, nint, nint, nint, NativeResult1>)sendMsgFunction)(obj, selector.Handle, nvs[0], nvs[1], nvs[2], nvs[3], nvs[4]),
                            6 => ((delegate*unmanaged<IntPtr, IntPtr, nint, nint, nint, nint, nint, nint, NativeResult1>)sendMsgFunction)(obj, selector.Handle, nvs[0], nvs[1], nvs[2], nvs[3], nvs[4], nvs[5]),
                            _ => throw new NotSupportedException("Too many arguments to send."),
                        };
                        return NativeTypeConversion.FromNativeValue<T>((nint*)&nr, 1);
                    }
                }),
                2 => Global.Run(() =>
                {
                    if (isFpResult)
                    {
                        var nr = nvs.Length switch
                        {
                            0 => ((delegate*unmanaged<IntPtr, IntPtr, NativeFpResult2>)sendMsgFunction)(obj, selector.Handle),
                            1 => ((delegate*unmanaged<IntPtr, IntPtr, nint, NativeFpResult2>)sendMsgFunction)(obj, selector.Handle, nvs[0]),
                            2 => ((delegate*unmanaged<IntPtr, IntPtr, nint, nint, NativeFpResult2>)sendMsgFunction)(obj, selector.Handle, nvs[0], nvs[1]),
                            3 => ((delegate*unmanaged<IntPtr, IntPtr, nint, nint, nint, NativeFpResult2>)sendMsgFunction)(obj, selector.Handle, nvs[0], nvs[1], nvs[2]),
                            4 => ((delegate*unmanaged<IntPtr, IntPtr, nint, nint, nint, nint, NativeFpResult2>)sendMsgFunction)(obj, selector.Handle, nvs[0], nvs[1], nvs[2], nvs[3]),
                            5 => ((delegate*unmanaged<IntPtr, IntPtr, nint, nint, nint, nint, nint, NativeFpResult2>)sendMsgFunction)(obj, selector.Handle, nvs[0], nvs[1], nvs[2], nvs[3], nvs[4]),
                            6 => ((delegate*unmanaged<IntPtr, IntPtr, nint, nint, nint, nint, nint, nint, NativeFpResult2>)sendMsgFunction)(obj, selector.Handle, nvs[0], nvs[1], nvs[2], nvs[3], nvs[4], nvs[5]),
                            _ => throw new NotSupportedException("Too many arguments to send."),
                        };
                        return NativeTypeConversion.FromNativeValue<T>((nint*)&nr, 2);
                    }
                    else
                    {
                        var nr = nvs.Length switch
                        {
                            0 => ((delegate*unmanaged<IntPtr, IntPtr, NativeResult2>)sendMsgFunction)(obj, selector.Handle),
                            1 => ((delegate*unmanaged<IntPtr, IntPtr, nint, NativeResult2>)sendMsgFunction)(obj, selector.Handle, nvs[0]),
                            2 => ((delegate*unmanaged<IntPtr, IntPtr, nint, nint, NativeResult2>)sendMsgFunction)(obj, selector.Handle, nvs[0], nvs[1]),
                            3 => ((delegate*unmanaged<IntPtr, IntPtr, nint, nint, nint, NativeResult2>)sendMsgFunction)(obj, selector.Handle, nvs[0], nvs[1], nvs[2]),
                            4 => ((delegate*unmanaged<IntPtr, IntPtr, nint, nint, nint, nint, NativeResult2>)sendMsgFunction)(obj, selector.Handle, nvs[0], nvs[1], nvs[2], nvs[3]),
                            5 => ((delegate*unmanaged<IntPtr, IntPtr, nint, nint, nint, nint, nint, NativeResult2>)sendMsgFunction)(obj, selector.Handle, nvs[0], nvs[1], nvs[2], nvs[3], nvs[4]),
                            6 => ((delegate*unmanaged<IntPtr, IntPtr, nint, nint, nint, nint, nint, nint, NativeResult2>)sendMsgFunction)(obj, selector.Handle, nvs[0], nvs[1], nvs[2], nvs[3], nvs[4], nvs[5]),
                            _ => throw new NotSupportedException("Too many arguments to send."),
                        };
                        return NativeTypeConversion.FromNativeValue<T>((nint*)&nr, 2);
                    }
                }),
                3 => Global.Run(() =>
                {
                    if (isFpResult)
                    {
                        var nr = nvs.Length switch
                        {
                            0 => ((delegate*unmanaged<IntPtr, IntPtr, NativeFpResult3>)sendMsgFunction)(obj, selector.Handle),
                            1 => ((delegate*unmanaged<IntPtr, IntPtr, nint, NativeFpResult3>)sendMsgFunction)(obj, selector.Handle, nvs[0]),
                            2 => ((delegate*unmanaged<IntPtr, IntPtr, nint, nint, NativeFpResult3>)sendMsgFunction)(obj, selector.Handle, nvs[0], nvs[1]),
                            3 => ((delegate*unmanaged<IntPtr, IntPtr, nint, nint, nint, NativeFpResult3>)sendMsgFunction)(obj, selector.Handle, nvs[0], nvs[1], nvs[2]),
                            4 => ((delegate*unmanaged<IntPtr, IntPtr, nint, nint, nint, nint, NativeFpResult3>)sendMsgFunction)(obj, selector.Handle, nvs[0], nvs[1], nvs[2], nvs[3]),
                            5 => ((delegate*unmanaged<IntPtr, IntPtr, nint, nint, nint, nint, nint, NativeFpResult3>)sendMsgFunction)(obj, selector.Handle, nvs[0], nvs[1], nvs[2], nvs[3], nvs[4]),
                            6 => ((delegate*unmanaged<IntPtr, IntPtr, nint, nint, nint, nint, nint, nint, NativeFpResult3>)sendMsgFunction)(obj, selector.Handle, nvs[0], nvs[1], nvs[2], nvs[3], nvs[4], nvs[5]),
                            _ => throw new NotSupportedException("Too many arguments to send."),
                        };
                        return NativeTypeConversion.FromNativeValue<T>((nint*)&nr, 3);
                    }
                    else
                    {
                        var nr = nvs.Length switch
                        {
                            0 => ((delegate*unmanaged<IntPtr, IntPtr, NativeResult3>)sendMsgFunction)(obj, selector.Handle),
                            1 => ((delegate*unmanaged<IntPtr, IntPtr, nint, NativeResult3>)sendMsgFunction)(obj, selector.Handle, nvs[0]),
                            2 => ((delegate*unmanaged<IntPtr, IntPtr, nint, nint, NativeResult3>)sendMsgFunction)(obj, selector.Handle, nvs[0], nvs[1]),
                            3 => ((delegate*unmanaged<IntPtr, IntPtr, nint, nint, nint, NativeResult3>)sendMsgFunction)(obj, selector.Handle, nvs[0], nvs[1], nvs[2]),
                            4 => ((delegate*unmanaged<IntPtr, IntPtr, nint, nint, nint, nint, NativeResult3>)sendMsgFunction)(obj, selector.Handle, nvs[0], nvs[1], nvs[2], nvs[3]),
                            5 => ((delegate*unmanaged<IntPtr, IntPtr, nint, nint, nint, nint, nint, NativeResult3>)sendMsgFunction)(obj, selector.Handle, nvs[0], nvs[1], nvs[2], nvs[3], nvs[4]),
                            6 => ((delegate*unmanaged<IntPtr, IntPtr, nint, nint, nint, nint, nint, nint, NativeResult3>)sendMsgFunction)(obj, selector.Handle, nvs[0], nvs[1], nvs[2], nvs[3], nvs[4], nvs[5]),
                            _ => throw new NotSupportedException("Too many arguments to send."),
                        };
                        return NativeTypeConversion.FromNativeValue<T>((nint*)&nr, 3);
                    }
                }),
                4 => Global.Run(() =>
                {
                    if (isFpResult)
                    {
                        var nr = nvs.Length switch
                        {
                            0 => ((delegate*unmanaged<IntPtr, IntPtr, NativeFpResult4>)sendMsgFunction)(obj, selector.Handle),
                            1 => ((delegate*unmanaged<IntPtr, IntPtr, nint, NativeFpResult4>)sendMsgFunction)(obj, selector.Handle, nvs[0]),
                            2 => ((delegate*unmanaged<IntPtr, IntPtr, nint, nint, NativeFpResult4>)sendMsgFunction)(obj, selector.Handle, nvs[0], nvs[1]),
                            3 => ((delegate*unmanaged<IntPtr, IntPtr, nint, nint, nint, NativeFpResult4>)sendMsgFunction)(obj, selector.Handle, nvs[0], nvs[1], nvs[2]),
                            4 => ((delegate*unmanaged<IntPtr, IntPtr, nint, nint, nint, nint, NativeFpResult4>)sendMsgFunction)(obj, selector.Handle, nvs[0], nvs[1], nvs[2], nvs[3]),
                            5 => ((delegate*unmanaged<IntPtr, IntPtr, nint, nint, nint, nint, nint, NativeFpResult4>)sendMsgFunction)(obj, selector.Handle, nvs[0], nvs[1], nvs[2], nvs[3], nvs[4]),
                            6 => ((delegate*unmanaged<IntPtr, IntPtr, nint, nint, nint, nint, nint, nint, NativeFpResult4>)sendMsgFunction)(obj, selector.Handle, nvs[0], nvs[1], nvs[2], nvs[3], nvs[4], nvs[5]),
                            _ => throw new NotSupportedException("Too many arguments to send."),
                        };
                        return NativeTypeConversion.FromNativeValue<T>((nint*)&nr, 4);
                    }
                    else
                    {
                        var nr = nvs.Length switch
                        {
                            0 => ((delegate*unmanaged<IntPtr, IntPtr, NativeResult4>)sendMsgFunction)(obj, selector.Handle),
                            1 => ((delegate*unmanaged<IntPtr, IntPtr, nint, NativeResult4>)sendMsgFunction)(obj, selector.Handle, nvs[0]),
                            2 => ((delegate*unmanaged<IntPtr, IntPtr, nint, nint, NativeResult4>)sendMsgFunction)(obj, selector.Handle, nvs[0], nvs[1]),
                            3 => ((delegate*unmanaged<IntPtr, IntPtr, nint, nint, nint, NativeResult4>)sendMsgFunction)(obj, selector.Handle, nvs[0], nvs[1], nvs[2]),
                            4 => ((delegate*unmanaged<IntPtr, IntPtr, nint, nint, nint, nint, NativeResult4>)sendMsgFunction)(obj, selector.Handle, nvs[0], nvs[1], nvs[2], nvs[3]),
                            5 => ((delegate*unmanaged<IntPtr, IntPtr, nint, nint, nint, nint, nint, NativeResult4>)sendMsgFunction)(obj, selector.Handle, nvs[0], nvs[1], nvs[2], nvs[3], nvs[4]),
                            6 => ((delegate*unmanaged<IntPtr, IntPtr, nint, nint, nint, nint, nint, nint, NativeResult4>)sendMsgFunction)(obj, selector.Handle, nvs[0], nvs[1], nvs[2], nvs[3], nvs[4], nvs[5]),
                            _ => throw new NotSupportedException("Too many arguments to send."),
                        };
                        return NativeTypeConversion.FromNativeValue<T>((nint*)&nr, 4);
                    }
                }),
                _ => throw new NotSupportedException($"Unsupported return type '{typeof(T).Name}'."),
            });
        }
#pragma warning restore CS8600
#pragma warning restore CS8603


        /// <summary>
        /// Send message to super class of instance.
        /// </summary>
        /// <param name="selector">Selector.</param>
        /// <param name="args">Arguments</param>
        public void SendMessageToSuper(Selector selector, params object?[] args)
        {
            this.VerifyDisposed();
            var superClass = this.Class.SuperClass;
            if (superClass == null)
                return;
            var superInfo = new objc_super() { receiver = this.Handle, super_class = superClass.Handle };
            SendMessage<nint>(objc_msgSendSuper, new IntPtr(&superInfo), selector, args);
        }


        /// <summary>
        /// Send message to super class of instance.
        /// </summary>
        /// <param name="selector">Selector.</param>
        /// <param name="args">Arguments</param>
        /// <typeparam name="T">Type of returned value.</typeparam>
        /// <returns>Result.</returns>
        public T SendMessageToSuper<T>(Selector selector, params object?[] args)
        {
            this.VerifyDisposed();
            var superClass = this.Class.SuperClass;
            if (superClass == null)
                throw new InvalidOperationException("No super class found.");
            var superInfo = new objc_super() { receiver = this.Handle, super_class = superClass.Handle };
            return SendMessage<T>(objc_msgSendSuper, new IntPtr(&superInfo), selector, args);
        }


        /// <summary>
        /// Send message to super class of instance.
        /// </summary>
        /// <param name="obj">Handle of instance.</param>
        /// <param name="selector">Selector.</param>
        /// <param name="args">Arguments</param>
        public static void SendMessageToSuper(IntPtr obj, Selector selector, params object?[] args)
        {
            var superClass = Class.GetClass(obj).SuperClass;
            if (superClass == null)
                return;
            var superInfo = new objc_super() { receiver = obj, super_class = superClass.Handle };
            SendMessage<nint>(objc_msgSendSuper, new IntPtr(&superInfo), selector, args);
        }


        /// <summary>
        /// Send message to super class of instance.
        /// </summary>
        /// <param name="obj">Handle of instance.</param>
        /// <param name="selector">Selector.</param>
        /// <param name="args">Arguments</param>
        /// <typeparam name="T">Type of returned value.</typeparam>
        /// <returns>Result.</returns>
        public static T SendMessageToSuper<T>(IntPtr obj, Selector selector, params object?[] args)
        {
            var superClass = Class.GetClass(obj).SuperClass;
            if (superClass == null)
                throw new InvalidOperationException("No super class found.");
            var superInfo = new objc_super() { receiver = obj, super_class = superClass.Handle };
            return SendMessage<T>(objc_msgSendSuper, new IntPtr(&superInfo), selector, args);
        }


        /// <summary>
        /// Set value of property.
        /// </summary>
        /// <param name="property">Property.</param>
        /// <param name="value">Value.</param>
        /// <typeparam name="T">Type of property.</typeparam>
        public void SetProperty<T>(Property property, T value)
        {
            if (!property.Class.IsAssignableFrom(this.Class))
                throw new ArgumentException($"Property '{property}' is not owned by class '{this.Class}'.");
            if (property.IsReadOnly)
                throw new InvalidOperationException($"Cannot set value to read-only property '{property.Name}'.");
            this.SendMessage(property.Setter!, value);
        }


        /// <summary>
        /// Set instance variable.
        /// </summary>
        /// <param name="ivar">Instance variable.</param>
        /// <param name="value">Value.</param>
        /// <typeparam name="T">Type of variable.</typeparam>
        public void SetVariable<T>(Member ivar, T value)
        {
            this.VerifyDisposed();
            SetVariable<T>(this.Handle, ivar, value);
        }


        /// <summary>
        /// Set instance variable.
        /// </summary>
        /// <param name="obj">Handle of instance.</param>
        /// <param name="ivar">Instance variable.</param>
        /// <param name="value">Value.</param>
        /// <typeparam name="T">Type of variable.</typeparam>
        public static void SetVariable<T>(IntPtr obj, Member ivar, T value)
        {
            VerifyHandle(obj);
            var nValues = NativeTypeConversion.ToNativeValues(new object?[] { value });
            switch (nValues.Length)
            {
                case 1:
                    ((delegate*unmanaged<IntPtr, IntPtr, nint, void>)object_setIvar)(obj, ivar.Handle, nValues[0]);
                    break;
                case 2:
                    ((delegate*unmanaged<IntPtr, IntPtr, nint, nint, void>)object_setIvar)(obj, ivar.Handle, nValues[0], nValues[1]);
                    break;
                case 3:
                    ((delegate*unmanaged<IntPtr, IntPtr, nint, nint, nint, void>)object_setIvar)(obj, ivar.Handle, nValues[0], nValues[1], nValues[2]);
                    break;
                case 4:
                    ((delegate*unmanaged<IntPtr, IntPtr, nint, nint, nint, nint, void>)object_setIvar)(obj, ivar.Handle, nValues[0], nValues[1], nValues[2], nValues[3]);
                    break;
                default:
                    throw new NotSupportedException($"Unsupported variable type '{typeof(T).Name}'.");
            }
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


        // Make sure that handle is not 0.
        static void VerifyHandle(IntPtr handle)
        {
            if (handle == IntPtr.Zero)
                throw new ArgumentException("Handle of instance cannot be null.");
        }
    }
}