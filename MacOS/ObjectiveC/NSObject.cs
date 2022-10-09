using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Threading;

namespace CarinaStudio.MacOS.ObjectiveC
{
    /// <summary>
    /// Object of Objective-C.
    /// </summary>
    public unsafe class NSObject : IDisposable, IEquatable<NSObject>
    {
        [StructLayout(LayoutKind.Sequential)]
        ref struct objc_super
        {
            public IntPtr receiver;
            public IntPtr super_class;
        }


        // Descriptor of stub of objc_msgSend function.
        class SendMessageStubInfo : NativeMethodInfo
        {
            // Constructor.
            public SendMessageStubInfo(MethodInfo stub, Type[] paramTypes, Type? returnType) : base(paramTypes, returnType) =>
                this.Stub = stub;

            // Stub.
            public MethodInfo Stub { get; }
        }


        // Native symbols.
        static readonly void* objc_msgSend;
        static readonly void* objc_msgSendSuper;
        [DllImport(NativeLibraryNames.ObjectiveC)]
        static extern IntPtr object_getInstanceVariable(IntPtr obj, string name, out void* outValue);
        //static readonly void* object_getIvar;
        [DllImport(NativeLibraryNames.ObjectiveC)]
        static extern void object_setInstanceVariable(IntPtr obj, string name, void* value);
        //static readonly void* object_setIvar;


        // Static fields.
        static readonly nint[] EmptyNativeArgs = new nint[0];
        static readonly double[] EmptyNativeFpArgs = new double[0];
        static readonly Selector? InitSelector;
        static readonly Selector? ReleaseSelector;
        static readonly Selector? RetainSelector;
        static readonly IDictionary<int, List<SendMessageStubInfo>> SendMessageStubs = new Dictionary<int, List<SendMessageStubInfo>>();
        static readonly IDictionary<Type, ConstructorInfo> WrappingConstructors = new ConcurrentDictionary<Type, ConstructorInfo>();
        

        // Fields.
        readonly Class @class;
        volatile IntPtr handle;
        volatile Property? hashProperty;
        volatile int isReleased;
        readonly bool ownsInstance;


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
                //object_getIvar = (void*)NativeLibrary.GetExport(libHandle, nameof(object_getIvar));
                //object_setIvar = (void*)NativeLibrary.GetExport(libHandle, nameof(object_setIvar));
                if (objc_msgSend == null)
                    throw new NotSupportedException("Cannot find 'objc_msgSend' in Objective-C runtime.");
                if (objc_msgSendSuper == null)
                    throw new NotSupportedException("Cannot find 'objc_msgSendSuper' in Objective-C runtime.");
            }
            else
                throw new NotSupportedException("Cannot load Objective-C runtime.");
            InitSelector = Selector.FromName("init");
            ReleaseSelector = Selector.FromName("release");
            RetainSelector = Selector.FromName("retain");
        }


        /// <summary>
        /// Initialize new <see cref="NSObject"/> instance.
        /// </summary>
        /// <param name="handle">Handle of instance.</param>
        /// <param name="ownsInstance">True to own the instance.</param>
        internal protected NSObject(IntPtr handle, bool ownsInstance)
        {
            this.@class = Class.GetClass(handle);
            this.handle = handle;
            this.ownsInstance = ownsInstance;
            if (!ownsInstance)
                GC.SuppressFinalize(this);
        }


        /// <summary>
        /// Initialize new <see cref="NSObject"/> instance.
        /// </summary>
        /// <param name="cls">Class of instance.</param>
        /// <param name="handle">Handle of instance.</param>
        /// <param name="ownsInstance">True to own the instance.</param>
        internal protected NSObject(Class cls, IntPtr handle, bool ownsInstance)
        {
            VerifyHandle(handle);
            this.@class = cls;
            this.handle = handle;
            this.ownsInstance = ownsInstance;
            if (!ownsInstance)
                GC.SuppressFinalize(this);
        }


        /// <summary>
        /// Finalizer.
        /// </summary>
        ~NSObject() => this.OnRelease();


        /// <summary>
        /// Get class of instance.
        /// </summary>
        public Class Class { get => this.@class; }


        /// <inheritdoc/>
        public bool Equals(NSObject? obj) =>
            obj != null && obj.handle == this.handle;


        /// <inheritdoc/>
        public override bool Equals(object? obj) =>
            obj is NSObject nsObj && this.Equals(nsObj);
        

        /// <summary>
        /// Wrap given handle as <see cref="NSObject"/>.
        /// </summary>
        /// <param name="handle">Handle of instance.</param>
        /// <param name="ownsInstance">True to owns instance.</param>
        /// <returns>Wrapped instance, or Null if <paramref name="handle"/> is <see cref="IntPtr.Zero"/>.</returns>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static NSObject? FromHandle(IntPtr handle, bool ownsInstance = false)
        {
            if (handle == default)
                return null;
            return new NSObject(handle, ownsInstance);
        }
        

        /// <summary>
        /// Wrap given handle as given type.
        /// </summary>
        /// <param name="handle">Handle of instance.</param>
        /// <param name="ownsInstance">True to owns instance.</param>
        /// <typeparam name="T">Type to wrap the instance.</typeparam>
        /// <returns>Wrapped instance, or Null if <paramref name="handle"/> is <see cref="IntPtr.Zero"/>.</returns>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static T? FromHandle<T>(IntPtr handle, bool ownsInstance = false) where T : NSObject =>
            (T?)FromHandle(typeof(T), handle, ownsInstance);


        /// <summary>
        /// Wrap given handle as given type.
        /// </summary>
        /// <param name="type">Type to wrap the instance.</param>
        /// <param name="handle">Handle of instance.</param>
        /// <param name="ownsInstance">True to owns instance.</param>
        /// <returns>Wrapped instance, or Null if <paramref name="handle"/> is <see cref="IntPtr.Zero"/>.</returns>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static NSObject? FromHandle(Type type, IntPtr handle, bool ownsInstance = false)
        {
            if (handle == default)
                return null;
            if (type == typeof(NSObject))
                return new NSObject(handle, ownsInstance);
            var ctor = GetWrappingConstructor(type);
            if (ctor.GetParameters().Length == 2)
                return (NSObject)Activator.CreateInstance(type, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null, new object?[]{ handle, ownsInstance }, null).AsNonNull();
            return (NSObject)Activator.CreateInstance(type, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null, new object?[]{ Class.GetClass(handle), handle, ownsInstance }, null).AsNonNull();
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
                ?? this.Class.GetProperty("hash").Also(it => this.hashProperty = it);
            return property != null
                ? this.GetProperty<int>(property)
                : this.handle.GetHashCode();
        }


        /// <summary>
        /// Get instance variable as given type.
        /// </summary>
        /// <param name="ivar">Descriptor of instance variable.</param>
        /// <typeparam name="T">Type of variable.</typeparam>
        /// <returns>Value of variable.</returns>
        public T GetVariable<T>(Variable ivar)
        {
            this.VerifyReleased();
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
        public static T GetVariable<T>(IntPtr obj, Variable ivar)
        {
            VerifyHandle(obj);
            var type = ivar.Type;
            var size = ivar.Size;
            var ivarHandle = object_getInstanceVariable(obj, ivar.Name, out var outValue);
            if (outValue == null)
                return default;
            if (type.IsArray)
            {
                if (type.GetArrayRank() > 1)
                    throw new NotSupportedException($"Only 1-dimensional array is supported.");
                var count = ivar.ElementCount;
                var elementType = type.GetElementType()!;
                var array = Array.CreateInstance(elementType, count);
                var elementPtr = (byte*)outValue;
                for (var i = 0; i < count; ++i)
                {
                    array.SetValue(NativeTypeConversion.FromNativeValue(elementPtr, size, elementType, out var elementSize), i);
                    elementPtr += elementSize;
                    size -= elementSize;
                }
                return (T)(object)array;
            }
            return (T)NativeTypeConversion.FromNativeValue((byte*)outValue, size, typeof(T), out var _);
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
                    var CtorWith2Args = (ConstructorInfo?)null;
                    foreach (var ctor in it)
                    {
                        var parameters = ctor.GetParameters();
                        switch (parameters.Length)
                        {
                            case 2:
                                if (parameters[0].ParameterType == typeof(IntPtr)
                                    && parameters[1].ParameterType == typeof(bool))
                                {
                                    CtorWith2Args = ctor;
                                }
                                break;
                            case 3:
                                if (parameters[0].ParameterType == typeof(Class)
                                    && parameters[1].ParameterType == typeof(IntPtr)
                                    && parameters[2].ParameterType == typeof(bool))
                                {
                                    WrappingConstructors.TryAdd(type, ctor);
                                    return ctor;
                                }
                                break;
                        }
                    }
                    if (CtorWith2Args != null)
                    {
                        WrappingConstructors.TryAdd(type, CtorWith2Args);
                        return CtorWith2Args;
                    }
                    throw new InvalidCastException($"Cannot find way to construct {type.Name}.");
                });


        /// <summary>
        /// Get handle of instance.
        /// </summary>
        public IntPtr Handle { get => this.handle; }


        /// <inheritdoc/>
        void IDisposable.Dispose() => this.Release();


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
        /// Check whether instance has been released or not.
        /// </summary>
        public bool IsReleased { get => this.isReleased != 0; }


        /// <summary>
        /// Called to release instance.
        /// </summary>
        protected virtual void OnRelease()
        {
            if (this.IsDefaultInstance)
                throw new InvalidOperationException("Cannot release default instance.");
            if (this.handle != IntPtr.Zero && this.ownsInstance)
                ((delegate*<IntPtr, IntPtr, void>)objc_msgSend)(this.handle, ReleaseSelector!.Handle);
            this.handle = default;
        }


        /// <summary>
        /// Equality operator.
        /// </summary>
        public static bool operator ==(NSObject? l, NSObject? r)
        {
            var lHandle = l?.Handle ?? default;
            var rHandle = r?.Handle ?? default;
            return lHandle == rHandle;
        }


        /// <summary>
        /// Inequality operator.
        /// </summary>
        public static bool operator !=(NSObject? l, NSObject? r)
        {
            var lHandle = l?.Handle ?? default;
            var rHandle = r?.Handle ?? default;
            return lHandle != rHandle;
        }


        /// <summary>
        /// Release the instance.
        /// </summary>
        public void Release()
        {
            if (Interlocked.Exchange(ref this.isReleased, 1) != 0)
                return;
            GC.SuppressFinalize(this);
            this.OnRelease();
        }


        /// <summary>
        /// Retain the instance.
        /// </summary>
        /// <typeparam name="T">Type of instance.</typeparam>
        /// <returns>Retained instance.</returns>
        public T Retain<T>() where T : NSObject
        {
            this.VerifyReleased();
            var newHandle = ((delegate*unmanaged<IntPtr, IntPtr, IntPtr>)objc_msgSend)(this.handle, RetainSelector!.Handle);
            try
            {
                if (typeof(T) == typeof(NSObject))
                    return (T)new NSObject(this.@class, newHandle, true);
                var ctor = GetWrappingConstructor(typeof(T));
                if (ctor.GetParameters().Length == 2)
                    return (T)Activator.CreateInstance(typeof(T), BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null, new object?[]{ newHandle, true }, null).AsNonNull();
                return (T)Activator.CreateInstance(typeof(T), BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null, new object?[]{ this.@class, newHandle, true }, null).AsNonNull();
            }
            catch
            {
                ((delegate*unmanaged<IntPtr, IntPtr, void>)objc_msgSend)(newHandle, ReleaseSelector!.Handle);
                throw;
            }
        }


        /// <summary>
        /// Retain the instance.
        /// </summary>
        /// <param name="handle">Handle of instance to retain.</param>
        /// <typeparam name="T">Type of instance.</typeparam>
        /// <returns>Retained instance, or Null if <paramref name="handle"/> is <see cref="IntPtr.Zero"/>.</returns>
        public static T? Retain<T>(IntPtr handle) where T : NSObject =>
            (T?)Retain(typeof(T), handle);


        /// <summary>
        /// Retain the instance.
        /// </summary>
        /// <param name="type">Type of instance.</param>
        /// <param name="handle">Handle of instance to retain.</param>
        /// <returns>Retained instance, or Null if <paramref name="handle"/> is <see cref="IntPtr.Zero"/>.</returns>
        public static NSObject? Retain(Type type, IntPtr handle)
        {
            if (handle == default)
                return null;
            if (!typeof(NSObject).IsAssignableFrom(type))
                throw new ArgumentException("The type must be NSObject or extend from NSObject.");
            var newHandle = ((delegate*unmanaged<IntPtr, IntPtr, IntPtr>)objc_msgSend)(handle, RetainSelector!.Handle);
            try
            {
                if (type == typeof(NSObject))
                    return new NSObject(newHandle, true);
                var ctor = GetWrappingConstructor(type);
                if (ctor.GetParameters().Length == 2)
                    return (NSObject)Activator.CreateInstance(type, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null, new object?[]{ newHandle, true }, null).AsNonNull();
                return (NSObject)Activator.CreateInstance(type, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null, new object?[]{ Class.GetClass(newHandle), newHandle, true }, null).AsNonNull();
            }
            catch
            {
                ((delegate*unmanaged<IntPtr, IntPtr, void>)objc_msgSend)(newHandle, ReleaseSelector!.Handle);
                throw;
            }
        }


        /// <summary>
        /// Send message to instance.
        /// </summary>
        /// <param name="selector">Selector.</param>
        public void SendMessage(Selector selector) =>
            SendMessageCore(objc_msgSend, this.Handle, selector);
        

        /// <summary>
        /// Send message to instance.
        /// </summary>
        /// <param name="selector">Selector.</param>
        /// <param name="arg">Argument.</param>
        public void SendMessage(Selector selector, object? arg) =>
            SendMessageCore(objc_msgSend, this.Handle, selector, arg);


        /// <summary>
        /// Send message to instance.
        /// </summary>
        /// <param name="selector">Selector.</param>
        /// <param name="args">Arguments.</param>
        public void SendMessage(Selector selector, params object?[] args)
        {
            switch (args.Length)
            {
                case 0:
                    SendMessageCore(objc_msgSend, this.Handle, selector);
                    break;
                case 1:
                    SendMessageCore(objc_msgSend, this.Handle, selector, args[0]);
                    break;
                default:
                    SendMessageCore(objc_msgSend, this.Handle, selector, null, args);
                    break;
            }
        }


        /// <summary>
        /// Send message to instance.
        /// </summary>
        /// <param name="obj">Handle of instance.</param>
        /// <param name="selector">Selector.</param>
        public static void SendMessage(IntPtr obj, Selector selector) =>
            SendMessageCore(objc_msgSend, obj, selector);
        

        /// <summary>
        /// Send message to instance.
        /// </summary>
        /// <param name="obj">Handle of instance.</param>
        /// <param name="selector">Selector.</param>
        /// <param name="arg">Argument.</param>
        public static void SendMessage(IntPtr obj, Selector selector, object? arg) =>
            SendMessageCore(objc_msgSend, obj, selector, arg);


        /// <summary>
        /// Send message to instance.
        /// </summary>
        /// <param name="obj">Handle of instance.</param>
        /// <param name="selector">Selector.</param>
        /// <param name="args">Arguments.</param>
        public static void SendMessage(IntPtr obj, Selector selector, params object?[] args)
        {
            switch (args.Length)
            {
                case 0:
                    SendMessageCore(objc_msgSend, obj, selector);
                    break;
                case 1:
                    SendMessageCore(objc_msgSend, obj, selector, args[0]);
                    break;
                default:
                    SendMessageCore(objc_msgSend, obj, selector, null, args);
                    break;
            }
        }


#pragma warning disable CS8600
#pragma warning disable CS8603
        /// <summary>
        /// Send message to instance.
        /// </summary>
        /// <param name="selector">Selector.</param>
        /// <typeparam name="T">Type of returned value.</typeparam>
        /// <returns>Result.</returns>
        public T SendMessage<T>(Selector selector) =>
            (T)SendMessageCore(objc_msgSend, this.Handle, selector, typeof(T));
#pragma warning restore CS8600
#pragma warning restore CS8603


#pragma warning disable CS8600
#pragma warning disable CS8603
        /// <summary>
        /// Send message to instance.
        /// </summary>
        /// <param name="selector">Selector.</param>
        /// <param name="args">Arguments.</param>
        /// <typeparam name="T">Type of returned value.</typeparam>
        /// <returns>Result.</returns>
        public T SendMessage<T>(Selector selector, params object?[] args)
        {
            if (args.Length == 0)
                return (T)SendMessageCore(objc_msgSend, this.Handle, selector, typeof(T));
            return (T)SendMessageCore(objc_msgSend, this.Handle, selector, typeof(T), args);
        }
#pragma warning restore CS8600
#pragma warning restore CS8603


#pragma warning disable CS8600
#pragma warning disable CS8603
        /// <summary>
        /// Send message to instance.
        /// </summary>
        /// <param name="obj">Handle of instance.</param>
        /// <param name="selector">Selector.</param>
        /// <typeparam name="T">Type of returned value.</typeparam>
        /// <returns>Result.</returns>
        public static T SendMessage<T>(IntPtr obj, Selector selector) =>
            (T)SendMessageCore(objc_msgSend, obj, selector, typeof(T));
#pragma warning restore CS8600
#pragma warning restore CS8603
        

#pragma warning disable CS8600
#pragma warning disable CS8603
        /// <summary>
        /// Send message to instance.
        /// </summary>
        /// <param name="obj">Handle of instance.</param>
        /// <param name="selector">Selector.</param>
        /// <param name="args">Arguments.</param>
        /// <typeparam name="T">Type of returned value.</typeparam>
        /// <returns>Result.</returns>
        public static T SendMessage<T>(IntPtr obj, Selector selector, params object?[] args)
        {
            if (args.Length == 0)
                return (T)SendMessageCore(objc_msgSend, obj, selector, typeof(T));
            return (T)SendMessageCore(objc_msgSend, obj, selector, typeof(T), args);
        }
#pragma warning restore CS8600
#pragma warning restore CS8603


#pragma warning disable CS8600
#pragma warning disable CS8603
        // Core implementation of send message to object.
        static void SendMessageCore(void* msgSendFunc, IntPtr obj, Selector sel) // optimize for calling method without parameter and return value
        {
            VerifyHandle(obj);
            ((delegate*unmanaged<IntPtr, IntPtr, void>)msgSendFunc)(obj, sel.Handle);
        }
        static void SendMessageCore(void* msgSendFunc, IntPtr obj, Selector sel, object? arg) // optimize for property setter
        {
            VerifyHandle(obj);
            var nativeArg = NativeTypeConversion.ToNativeValue(arg);
            if (nativeArg == null)
                ((delegate*unmanaged<IntPtr, IntPtr, IntPtr, void>)msgSendFunc)(obj, sel.Handle, IntPtr.Zero);
            else if (nativeArg is IntPtr intPtrValue)
                ((delegate*unmanaged<IntPtr, IntPtr, IntPtr, void>)msgSendFunc)(obj, sel.Handle, intPtrValue);
            else if (nativeArg is bool boolValue)
                ((delegate*unmanaged<IntPtr, IntPtr, bool, void>)msgSendFunc)(obj, sel.Handle, boolValue);
            else if (nativeArg is int intValue)
                ((delegate*unmanaged<IntPtr, IntPtr, int, void>)msgSendFunc)(obj, sel.Handle, intValue);
            else if (nativeArg is float floatValue)
                ((delegate*unmanaged<IntPtr, IntPtr, float, void>)msgSendFunc)(obj, sel.Handle, floatValue);
            else if (nativeArg is double doubleValue)
                ((delegate*unmanaged<IntPtr, IntPtr, double, void>)msgSendFunc)(obj, sel.Handle, doubleValue);
            else
                SendMessageCore(msgSendFunc, obj, sel, null, new object?[] { arg });
        }
        static object? SendMessageCore(void* msgSendFunc, IntPtr obj, Selector sel, Type returnType) // optimize for property getter
        {
            var nativeReturnType = NativeTypeConversion.ToNativeType(returnType);
            if (nativeReturnType == typeof(IntPtr))
                return NativeTypeConversion.FromNativeValue(((delegate*unmanaged<IntPtr, IntPtr, IntPtr>)msgSendFunc)(obj, sel.Handle), returnType);
            if (nativeReturnType == typeof(bool))
                return NativeTypeConversion.FromNativeValue(((delegate*unmanaged<IntPtr, IntPtr, bool>)msgSendFunc)(obj, sel.Handle), returnType);
            if (nativeReturnType == typeof(int))
                return NativeTypeConversion.FromNativeValue(((delegate*unmanaged<IntPtr, IntPtr, int>)msgSendFunc)(obj, sel.Handle), returnType);
            if (nativeReturnType == typeof(float))
                return NativeTypeConversion.FromNativeValue(((delegate*unmanaged<IntPtr, IntPtr, float>)msgSendFunc)(obj, sel.Handle), returnType);
            if (nativeReturnType == typeof(double))
                return NativeTypeConversion.FromNativeValue(((delegate*unmanaged<IntPtr, IntPtr, double>)msgSendFunc)(obj, sel.Handle), returnType);
            return SendMessageCore(msgSendFunc, obj, sel, returnType, new object?[0]);
        }
        static object? SendMessageCore(void* msgSendFunc, IntPtr obj, Selector sel, Type? returnType, params object?[] args)
        {
            // check parameter
            VerifyHandle(obj);

            // convert to native types
            var nativeReturnType = returnType != null ? NativeTypeConversion.ToNativeType(returnType) : null;
            var argCount = args.Length;
            var nativeArgs = new object[argCount];
            var nativeArgTypes = new Type[argCount].Also(it =>
            {
                for (var i = it.Length - 1; i >= 0; --i)
                {
                    nativeArgs[i] = NativeTypeConversion.ToNativeValue(args[i]);
                    it[i] = nativeArgs[i].GetType();
                }
            });

            // find or create stub
            var stubInfo = SendMessageStubs.Lock(stubs =>
            {
                // find existing stub
                var argCount = args.Length;
                var stubInfoList = (List<SendMessageStubInfo>?)null;
                if (stubs.TryGetValue(argCount, out stubInfoList))
                {
                    for (var i = stubInfoList.Count - 1; i >= 0; --i)
                    {
                        var candStubInfo = (SendMessageStubInfo?)stubInfoList[i];
                        var candArgTypes = candStubInfo!.ParameterTypes;
                        for (var j = argCount - 1; j >= 0; --j)
                        {
                            if (candArgTypes[j] != nativeArgTypes[j])
                            {
                                candStubInfo = null;
                                break;
                            }
                        }
                        if (candStubInfo != null && candStubInfo.RetuenType == nativeReturnType)
                            return candStubInfo;
                    }
                }

                // create new stub
                var stubMethod = new DynamicMethod($"SendMessageStub@{argCount}", nativeReturnType, new Type[]{ typeof(nint), typeof(nint), typeof(nint), typeof(object[]) }, typeof(NSObject).Module, false);
                stubMethod.GetILGenerator(256).Let(ilGen =>
                {
                    // load obj and sel
                    ilGen.Emit(OpCodes.Ldarg_1);
                    ilGen.Emit(OpCodes.Ldarg_2);

                    // expand args
                    for (var i = 0; i < argCount; ++i)
                    {
                        // load args
                        ilGen.Emit(OpCodes.Ldarg_3);

                        // get args[i]
                        ilGen.Emit(OpCodes.Ldc_I4, i);
                        ilGen.Emit(OpCodes.Ldelem, typeof(object));

                        // unbox for value type
                        if (typeof(ValueType).IsAssignableFrom(nativeArgTypes[i]))
                            ilGen.Emit(OpCodes.Unbox_Any, nativeArgTypes[i]);
                    }

                    // load function pointer
                    ilGen.Emit(OpCodes.Ldarg_0);

                    // call obj_msgSend
                    var fullNativeArgTypes = new Type[argCount + 2];
                    fullNativeArgTypes[0] = typeof(nint);
                    fullNativeArgTypes[1] = typeof(nint);
                    Array.Copy(nativeArgTypes, 0, fullNativeArgTypes, 2, argCount);
                    ilGen.EmitCalli(OpCodes.Calli, CallingConvention.StdCall, nativeReturnType, fullNativeArgTypes);

                    // complete
                    ilGen.Emit(OpCodes.Ret);
                });
                var stubInfo = new SendMessageStubInfo(stubMethod, nativeArgTypes, nativeReturnType);
                if (stubInfoList == null)
                {
                    stubInfoList = new();
                    SendMessageStubs.Add(argCount, stubInfoList);
                }
                stubInfoList.Add(stubInfo);
                return stubInfo;
            });

            // send message
            try
            {
                var nativeReturnValue = stubInfo.Stub.Invoke(null, new object[]{ (IntPtr)msgSendFunc, obj, sel.Handle, nativeArgs });
                if (returnType == null)
                    return null;
                if (nativeReturnValue == null)
                {
                    if (returnType.IsValueType)
                        throw new InvalidOperationException("No value returned from sending message.");
                    return null;
                }
                return NativeTypeConversion.FromNativeValue(nativeReturnValue, returnType);
            }
            catch
            {
                for (var i = argCount - 1; i >= 0; --i)
                {
                    if (nativeArgs[i] is GCHandle gcHandle 
                        && args[i] is not GCHandle
                        && gcHandle != default)
                    {
                        gcHandle.Free();
                    }
                }
                throw;
            }
        }
#pragma warning restore CS8600
#pragma warning restore CS8603


        /// <summary>
        /// Send message to super class of instance.
        /// </summary>
        /// <param name="selector">Selector.</param>
        public void SendMessageToSuper(Selector selector)
        {
            this.VerifyReleased();
            var superClass = this.Class.SuperClass;
            if (superClass == null)
                return;
            var superInfo = new objc_super() { receiver = this.Handle, super_class = superClass.Handle };
            SendMessageCore(objc_msgSendSuper, new IntPtr(&superInfo), selector);
        }


        /// <summary>
        /// Send message to super class of instance.
        /// </summary>
        /// <param name="selector">Selector.</param>
        /// <param name="arg">Argument.</param>
        public void SendMessageToSuper(Selector selector, object? arg)
        {
            this.VerifyReleased();
            var superClass = this.Class.SuperClass;
            if (superClass == null)
                return;
            var superInfo = new objc_super() { receiver = this.Handle, super_class = superClass.Handle };
            SendMessageCore(objc_msgSendSuper, new IntPtr(&superInfo), selector, arg);
        }


        /// <summary>
        /// Send message to super class of instance.
        /// </summary>
        /// <param name="selector">Selector.</param>
        /// <param name="args">Arguments.</param>
        public void SendMessageToSuper(Selector selector, params object?[] args)
        {
            this.VerifyReleased();
            var superClass = this.Class.SuperClass;
            if (superClass == null)
                return;
            var superInfo = new objc_super() { receiver = this.Handle, super_class = superClass.Handle };
            switch (args.Length)
            {
                case 0:
                    SendMessageCore(objc_msgSendSuper, new IntPtr(&superInfo), selector);
                    break;
                case 1:
                    SendMessageCore(objc_msgSendSuper, new IntPtr(&superInfo), selector, args[0]);
                    break;
                default:
                    SendMessageCore(objc_msgSendSuper, new IntPtr(&superInfo), selector, null, args);
                    break;
            }
        }
    

#pragma warning disable CS8600
#pragma warning disable CS8603
        /// <summary>
        /// Send message to super class of instance.
        /// </summary>
        /// <param name="selector">Selector.</param>
        /// <typeparam name="T">Type of returned value.</typeparam>
        /// <returns>Result.</returns>
        public T SendMessageToSuper<T>(Selector selector)
        {
            this.VerifyReleased();
            var superClass = this.Class.SuperClass;
            if (superClass == null)
                throw new InvalidOperationException("No super class found.");
            var superInfo = new objc_super() { receiver = this.Handle, super_class = superClass.Handle };
            return (T)SendMessageCore(objc_msgSendSuper, new IntPtr(&superInfo), selector, typeof(T));
        }
#pragma warning restore CS8600
#pragma warning restore CS8603


#pragma warning disable CS8600
#pragma warning disable CS8603
        /// <summary>
        /// Send message to super class of instance.
        /// </summary>
        /// <param name="selector">Selector.</param>
        /// <param name="args">Arguments.</param>
        /// <typeparam name="T">Type of returned value.</typeparam>
        /// <returns>Result.</returns>
        public T SendMessageToSuper<T>(Selector selector, params object?[] args)
        {
            this.VerifyReleased();
            var superClass = this.Class.SuperClass;
            if (superClass == null)
                throw new InvalidOperationException("No super class found.");
            var superInfo = new objc_super() { receiver = this.Handle, super_class = superClass.Handle };
            if (args.Length == 0)
                return (T)SendMessageCore(objc_msgSendSuper, new IntPtr(&superInfo), selector, typeof(T));
            return (T)SendMessageCore(objc_msgSendSuper, new IntPtr(&superInfo), selector, typeof(T), args);
        }
#pragma warning restore CS8600
#pragma warning restore CS8603


        /// <summary>
        /// Send message to super class of instance.
        /// </summary>
        /// <param name="obj">Handle of instance.</param>
        /// <param name="selector">Selector.</param>
        public static void SendMessageToSuper(IntPtr obj, Selector selector)
        {
            var superClass = Class.GetClass(obj).SuperClass;
            if (superClass == null)
                return;
            var superInfo = new objc_super() { receiver = obj, super_class = superClass.Handle };
            SendMessageCore(objc_msgSendSuper, new IntPtr(&superInfo), selector);
        }


        /// <summary>
        /// Send message to super class of instance.
        /// </summary>
        /// <param name="obj">Handle of instance.</param>
        /// <param name="selector">Selector.</param>
        /// <param name="arg">Argument.</param>
        public static void SendMessageToSuper(IntPtr obj, Selector selector, object? arg)
        {
            var superClass = Class.GetClass(obj).SuperClass;
            if (superClass == null)
                return;
            var superInfo = new objc_super() { receiver = obj, super_class = superClass.Handle };
            SendMessageCore(objc_msgSendSuper, new IntPtr(&superInfo), selector, arg);
        }


        /// <summary>
        /// Send message to super class of instance.
        /// </summary>
        /// <param name="obj">Handle of instance.</param>
        /// <param name="selector">Selector.</param>
        /// <param name="args">Arguments.</param>
        public static void SendMessageToSuper(IntPtr obj, Selector selector, params object?[] args)
        {
            var superClass = Class.GetClass(obj).SuperClass;
            if (superClass == null)
                return;
            var superInfo = new objc_super() { receiver = obj, super_class = superClass.Handle };
            SendMessageCore(objc_msgSendSuper, new IntPtr(&superInfo), selector, null, args);
        }


#pragma warning disable CS8600
#pragma warning disable CS8603
        /// <summary>
        /// Send message to super class of instance.
        /// </summary>
        /// <param name="obj">Handle of instance.</param>
        /// <param name="selector">Selector.</param>
        /// <typeparam name="T">Type of returned value.</typeparam>
        /// <returns>Result.</returns>
        public static T SendMessageToSuper<T>(IntPtr obj, Selector selector)
        {
            var superClass = Class.GetClass(obj).SuperClass;
            if (superClass == null)
                throw new InvalidOperationException("No super class found.");
            var superInfo = new objc_super() { receiver = obj, super_class = superClass.Handle };
            return (T)SendMessageCore(objc_msgSendSuper, new IntPtr(&superInfo), selector, typeof(T));
        }
#pragma warning restore CS8600
#pragma warning restore CS8603


#pragma warning disable CS8600
#pragma warning disable CS8603
        /// <summary>
        /// Send message to super class of instance.
        /// </summary>
        /// <param name="obj">Handle of instance.</param>
        /// <param name="selector">Selector.</param>
        /// <param name="args">Arguments.</param>
        /// <typeparam name="T">Type of returned value.</typeparam>
        /// <returns>Result.</returns>
        public static T SendMessageToSuper<T>(IntPtr obj, Selector selector, params object?[] args)
        {
            var superClass = Class.GetClass(obj).SuperClass;
            if (superClass == null)
                throw new InvalidOperationException("No super class found.");
            var superInfo = new objc_super() { receiver = obj, super_class = superClass.Handle };
            return (T)SendMessageCore(objc_msgSendSuper, new IntPtr(&superInfo), selector, typeof(T), args);
        }
#pragma warning restore CS8600
#pragma warning restore CS8603


        /// <summary>
        /// Set value of property.
        /// </summary>
        /// <param name="property">Property.</param>
        /// <param name="value">Value.</param>
        /// <typeparam name="T">Type of property.</typeparam>
        public void SetProperty<T>(Property property, T? value)
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
        public void SetVariable<T>(Variable ivar, T? value)
        {
            this.VerifyReleased();
            SetVariable<T>(this.Handle, ivar, value);
        }


        /// <summary>
        /// Set instance variable.
        /// </summary>
        /// <param name="obj">Handle of instance.</param>
        /// <param name="ivar">Instance variable.</param>
        /// <param name="value">Value.</param>
        /// <typeparam name="T">Type of variable.</typeparam>
        public static void SetVariable<T>(IntPtr obj, Variable ivar, T? value)
        {
            VerifyHandle(obj);
            var type = ivar.Type;
            var size = ivar.Size;
            fixed (byte* valuePtr = new byte[size])
            {
                if (type.IsArray)
                {
                    var maxLength = ivar.ElementCount;
                    var elementType = type.GetElementType()!;
                    if (type.GetArrayRank() > 1)
                        throw new NotSupportedException("Only 1-dimensional array is supported.");
                    if (value is not Array array)
                        throw new ArgumentException("Value is not an array.");
                    if (array.GetType().GetArrayRank() > 1)
                        throw new NotSupportedException("Only 1-dimensional array is supported.");
                    if (!elementType.IsAssignableFrom(array.GetType().GetElementType()))
                        throw new ArgumentException($"Invalid type of array element: {array.GetType().GetElementType()?.Name}, {elementType.Name} expected.");
                    var arrayLength = array.GetLength(0);
                    if (arrayLength > maxLength)
                        throw new ArgumentException($"Size of array is too large: {arrayLength}, maximum size is {maxLength}.");
                    var count = Math.Min(arrayLength, maxLength);
                    if (count <= 0)
                        return;
                    var elementPtr = valuePtr;
                    for (var i = 0; i < count; ++i)
                    {
                        var elementSize = NativeTypeConversion.ToNativeValue(array.GetValue(i), elementPtr);
                        elementPtr += elementSize;
                    }
                }
                else if (value == null && type.IsValueType)
                    throw new ArgumentException($"Cannot set null value to variable {ivar.Name}.");
                else
                    NativeTypeConversion.ToNativeValue(value, valuePtr);
                object_setInstanceVariable(obj, ivar.Name, valuePtr);
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
            if (!cls.IsAssignableFrom(this.@class))
                throw new ArgumentException($"Instance is not {cls}.");
        }


        // Make sure that handle is not 0.
        static void VerifyHandle(IntPtr handle)
        {
            if (handle == IntPtr.Zero)
                throw new ArgumentException("Handle of instance cannot be null.");
        }


        /// <summary>
        /// Throw <see cref="ObjectDisposedException"/> if instance has been released.
        /// </summary>
        protected void VerifyReleased()
        {
            if (this.IsReleased)
                throw new ObjectDisposedException(this.GetType().Name);
        }
    }
}