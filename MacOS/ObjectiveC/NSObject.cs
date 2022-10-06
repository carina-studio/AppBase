using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
        static readonly Selector? DeallocSelector;
        static readonly nint[] EmptyNativeArgs = new nint[0];
        static readonly double[] EmptyNativeFpArgs = new double[0];
        static readonly Selector? InitSelector;
        static readonly IDictionary<int, List<SendMessageStubInfo>> SendMessageStubs = new Dictionary<int, List<SendMessageStubInfo>>();
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
                //object_getIvar = (void*)NativeLibrary.GetExport(libHandle, nameof(object_getIvar));
                //object_setIvar = (void*)NativeLibrary.GetExport(libHandle, nameof(object_setIvar));
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
                ?? this.Class.GetProperty("hash").Also(it => this.hashProperty = it);
            return property != null
                ? this.GetProperty<int>(property)
                : this.instance.GetHashCode();
        }


        /// <summary>
        /// Get instance variable as given type.
        /// </summary>
        /// <param name="ivar">Descriptor of instance variable.</param>
        /// <typeparam name="T">Type of variable.</typeparam>
        /// <returns>Value of variable.</returns>
        public T GetVariable<T>(Variable ivar)
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
        /// Get native instance held by this <see cref="NSObject"/>.
        /// </summary>
        protected InstanceHolder Instance { get => this.instance; }


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
            SendMessageCore(objc_msgSend, obj, selector, null, args);


        /// <summary>
        /// Send message to instance.
        /// </summary>
        /// <param name="selector">Selector.</param>
        /// <param name="args">Arguments</param>
        /// <typeparam name="T">Type of returned value.</typeparam>
        /// <returns>Result.</returns>
        public T SendMessage<T>(Selector selector, params object?[] args) =>
            SendMessage<T>(this.Handle, selector, args);
        

#pragma warning disable CS8600
#pragma warning disable CS8603
        /// <summary>
        /// Send message to instance.
        /// </summary>
        /// <param name="obj">Handle of instance.</param>
        /// <param name="selector">Selector.</param>
        /// <param name="args">Arguments</param>
        /// <typeparam name="T">Type of returned value.</typeparam>
        /// <returns>Result.</returns>
        public static T SendMessage<T>(IntPtr obj, Selector selector, params object?[] args) =>
            (T)SendMessageCore(objc_msgSend, obj, selector, typeof(T), args);
#pragma warning restore CS8600
#pragma warning restore CS8603


#pragma warning disable CS8600
#pragma warning disable CS8603
        // Core implementation of send message to object.
        static object? SendMessageCore(void* msgSendFunc, IntPtr obj, Selector sel, Type? returnType, params object?[] args)
        {
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
        /// <param name="args">Arguments</param>
        public void SendMessageToSuper(Selector selector, params object?[] args)
        {
            this.VerifyDisposed();
            var superClass = this.Class.SuperClass;
            if (superClass == null)
                return;
            var superInfo = new objc_super() { receiver = this.Handle, super_class = superClass.Handle };
            SendMessageCore(objc_msgSendSuper, new IntPtr(&superInfo), selector, null, args);
        }


#pragma warning disable CS8600
#pragma warning disable CS8603
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
            return (T)SendMessageCore(objc_msgSendSuper, new IntPtr(&superInfo), selector, typeof(T), args);
        }
#pragma warning restore CS8600
#pragma warning restore CS8603


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
            SendMessageCore(objc_msgSendSuper, new IntPtr(&superInfo), selector, null, args);
        }


#pragma warning disable CS8600
#pragma warning disable CS8603
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