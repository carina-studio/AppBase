using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Threading;

namespace CarinaStudio.MacOS.ObjectiveC;

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
    
    
    // Constants.
    internal const string CallConstructorRdcMessage = "Dynamic code generation is required for calling specific constructor of NSObject.";
    internal const string CallMethodRdcMessage = "Dynamic code generation is required for calling specific method of NSObject.";
    internal const string CreateArrayRdcMessage = "Dynamic code generation is required for creating array instance.";
    internal const string GetPropertyRdcMessage = "Dynamic code generation is required for getting property of NSObject.";
    internal const string GetVariableRdcMessage = "Dynamic code generation is required for getting variable of NSObject.";
    internal const string SendMessageRdcMessage = "Dynamic code generation is required for sending message to NSObject with specific parameters.";
    internal const string SetPropertyRdcMessage = "Dynamic code generation is required for setting specific property of NSObject.";
    internal const string SetVariableRdcMessage = "Dynamic code generation is required for setting variable of NSObject.";


    // Static fields.
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
            if (objc_msgSend is null)
                throw new NotSupportedException("Cannot find 'objc_msgSend' in Objective-C runtime.");
            if (objc_msgSendSuper is null)
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
    public Class Class => this.@class;


    /// <inheritdoc/>
    public bool Equals(NSObject? obj) =>
        obj is not null && obj.handle == this.handle;


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
    public static T? FromHandle<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)] T>(IntPtr handle, bool ownsInstance = false) where T : NSObject =>
        (T?)FromHandle(typeof(T), handle, ownsInstance);


    /// <summary>
    /// Wrap given handle as given type.
    /// </summary>
    /// <param name="type">Type to wrap the instance.</param>
    /// <param name="handle">Handle of instance.</param>
    /// <param name="ownsInstance">True to owns instance.</param>
    /// <returns>Wrapped instance, or Null if <paramref name="handle"/> is <see cref="IntPtr.Zero"/>.</returns>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public static NSObject? FromHandle([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)] Type type, IntPtr handle, bool ownsInstance = false)
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
    /// Get value of property as <see cref="Boolean"/>.
    /// </summary>
    /// <param name="property">Property.</param>
    /// <returns>Value.</returns>
    public bool GetBooleanProperty(Property property)
    {
        if (!property.Class.IsAssignableFrom(this.Class))
            throw new ArgumentException($"Property '{property}' is not owned by class '{this.Class}'.");
#pragma warning disable IL3050
        return this.SendMessage<bool>(property.Getter!);
#pragma warning restore IL3050
    }
    
    
    /// <summary>
    /// Get value of property as <see cref="Double"/>.
    /// </summary>
    /// <param name="property">Property.</param>
    /// <returns>Value.</returns>
    public double GetDoubleProperty(Property property)
    {
        if (!property.Class.IsAssignableFrom(this.Class))
            throw new ArgumentException($"Property '{property}' is not owned by class '{this.Class}'.");
#pragma warning disable IL3050
        return this.SendMessage<double>(property.Getter!);
#pragma warning restore IL3050
    }
    
    
    /// <summary>
    /// Get value of property as <see cref="Single"/>.
    /// </summary>
    /// <param name="property">Property.</param>
    /// <returns>Value.</returns>
    public float GetSingleProperty(Property property)
    {
        if (!property.Class.IsAssignableFrom(this.Class))
            throw new ArgumentException($"Property '{property}' is not owned by class '{this.Class}'.");
#pragma warning disable IL3050
        return this.SendMessage<float>(property.Getter!);
#pragma warning restore IL3050
    }
    
    
    // ReSharper disable NonReadonlyMemberInGetHashCode
    /// <inheritdoc/>
    public override int GetHashCode()
    {
        var property = this.hashProperty 
                       ?? this.Class.GetProperty("hash").Also(it => this.hashProperty = it);
        return property is not null
            ? this.GetInt32Property(property)
            : this.handle.GetHashCode();
    }
    // ReSharper restore NonReadonlyMemberInGetHashCode
    
    
    /// <summary>
    /// Get value of property as <see cref="Int32"/>.
    /// </summary>
    /// <param name="property">Property.</param>
    /// <returns>Value.</returns>
    public int GetInt32Property(Property property)
    {
        if (!property.Class.IsAssignableFrom(this.Class))
            throw new ArgumentException($"Property '{property}' is not owned by class '{this.Class}'.");
#pragma warning disable IL3050
        return this.SendMessage<int>(property.Getter!);
#pragma warning restore IL3050
    }
    
    
    /// <summary>
    /// Get value of property as <see cref="IntPtr"/>.
    /// </summary>
    /// <param name="property">Property.</param>
    /// <returns>Value.</returns>
    public nint GetIntPtrProperty(Property property)
    {
        if (!property.Class.IsAssignableFrom(this.Class))
            throw new ArgumentException($"Property '{property}' is not owned by class '{this.Class}'.");
#pragma warning disable IL3050
        return this.SendMessage<nint>(property.Getter!);
#pragma warning restore IL3050
    }
    
    
    /// <summary>
    /// Get value of property as <see cref="NSObject"/>.
    /// </summary>
    /// <param name="property">Property.</param>
    /// <returns>Value.</returns>
    public T? GetNSObjectProperty<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)] T>(Property property) where T : NSObject
    {
        if (!property.Class.IsAssignableFrom(this.Class))
            throw new ArgumentException($"Property '{property}' is not owned by class '{this.Class}'.");
#pragma warning disable IL3050
        var handle = this.SendMessage<nint>(property.Getter!);
#pragma warning restore IL3050
        if (handle != default)
            return FromHandle<T>(handle, false);
        return null;
    }
    

    /// <summary>
    /// Get value of property as given type.
    /// </summary>
    /// <param name="property">Property.</param>
    /// <returns>Value.</returns>
#if NET7_0_OR_GREATER
    [RequiresDynamicCode(GetPropertyRdcMessage)]
#endif
    public T GetProperty<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)] T>(Property property)
    {
        if (!property.Class.IsAssignableFrom(this.Class))
            throw new ArgumentException($"Property '{property}' is not owned by class '{this.Class}'.");
        return this.SendMessage<T>(property.Getter!);
    }
    
    
    /// <summary>
    /// Get value of property as <see cref="UInt32"/>.
    /// </summary>
    /// <param name="property">Property.</param>
    /// <returns>Value.</returns>
    public uint GetUInt32Property(Property property)
    {
        if (!property.Class.IsAssignableFrom(this.Class))
            throw new ArgumentException($"Property '{property}' is not owned by class '{this.Class}'.");
#pragma warning disable IL3050
        return this.SendMessage<uint>(property.Getter!);
#pragma warning restore IL3050
    }


    /// <summary>
    /// Get instance variable as given type.
    /// </summary>
    /// <param name="ivar">Descriptor of instance variable.</param>
    /// <typeparam name="T">Type of variable.</typeparam>
    /// <returns>Value of variable.</returns>
#if NET7_0_OR_GREATER
    [RequiresDynamicCode(CreateArrayRdcMessage)]
#endif
    public T GetVariable<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.NonPublicConstructors)]T>(Variable ivar)
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
#if NET7_0_OR_GREATER
    [RequiresDynamicCode(CreateArrayRdcMessage)]
#endif
    public static T GetVariable<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.NonPublicConstructors)] T>(IntPtr obj, Variable ivar) =>
        (T)GetVariable(obj, ivar, typeof(T));
#pragma warning restore CS8600
#pragma warning restore CS8603


    /// <summary>
    /// Get instance variable as given type.
    /// </summary>
    /// <param name="obj">Handle of instance.</param>
    /// <param name="ivar">Descriptor of instance variable.</param>
    /// <param name="targetType">Type of value of instance variable.</param>
    /// <returns>Value of variable.</returns>
#if NET7_0_OR_GREATER
    [RequiresDynamicCode(CreateArrayRdcMessage)]
#endif
    public static object? GetVariable(IntPtr obj, Variable ivar, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.NonPublicConstructors)] Type targetType)
    {
        VerifyHandle(obj);
        var size = ivar.Size;
        object_getInstanceVariable(obj, ivar.Name, out var outValue);
        if (outValue is null)
            return targetType.IsValueType ? Activator.CreateInstance(targetType) : default;
        if (targetType.IsArray)
        {
            if (targetType.GetArrayRank() > 1)
                throw new NotSupportedException("Only 1-dimensional array is supported.");
            var elementType = targetType.GetElementType()!;
            var elementSize = NativeTypeConversion.GetNativeValueSize(elementType);
            var count = (ivar.Size / elementSize);
            var array = Array.CreateInstance(elementType, count);
            var elementPtr = (byte*)outValue;
            for (var i = 0; i < count; ++i)
            {
#pragma warning disable IL2072
                array.SetValue(NativeTypeConversion.FromNativeValue(elementPtr, size, elementType, out var consumedSize), i);
#pragma warning restore IL2072
                elementPtr += consumedSize;
                size -= consumedSize;
            }
            return array;
        }
        return NativeTypeConversion.FromNativeValue((byte*)outValue, size, targetType, out var _);
    }


    // Get static method to wrap native instance.
    static ConstructorInfo GetWrappingConstructor<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)] T>() where T : NSObject =>
        GetWrappingConstructor(typeof(T));
    static ConstructorInfo GetWrappingConstructor([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)] Type type) =>
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
                if (CtorWith2Args is not null)
                {
                    WrappingConstructors.TryAdd(type, CtorWith2Args);
                    return CtorWith2Args;
                }
                throw new InvalidCastException($"Cannot find way to construct {type.Name}.");
            });


    /// <summary>
    /// Get handle of instance.
    /// </summary>
    public IntPtr Handle => this.handle;


    /// <inheritdoc/>
    void IDisposable.Dispose() => this.Release();


    /// <summary>
    /// Call default initializer (init) without parameter.
    /// </summary>
    /// <param name="obj">Handle of uninitialized instance.</param>
    /// <returns>Handle of initialized instance.</returns>
    public static IntPtr Initialize(IntPtr obj)
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
    public bool IsReleased => this.isReleased != 0;


    /// <summary>
    /// Called to release instance.
    /// </summary>
    protected virtual void OnRelease()
    {
        if (this.IsDefaultInstance)
            throw new InvalidOperationException("Cannot release default instance.");
        if (this.ownsInstance)
            Release(this.handle);
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
    /// Release the instance.
    /// </summary>
    /// <param name="obj">Handle of instance.</param>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public static void Release(IntPtr obj)
    {
        if (obj != default)
            ((delegate*<IntPtr, IntPtr, void>)objc_msgSend)(obj, ReleaseSelector!.Handle);
    }


    /// <summary>
    /// Retain the instance.
    /// </summary>
    /// <typeparam name="T">Type of instance.</typeparam>
    /// <returns>Retained instance.</returns>
    public T Retain<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)] T>() where T : NSObject
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
    public static T? Retain<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)] T>(IntPtr handle) where T : NSObject =>
        (T?)Retain(typeof(T), handle);


    /// <summary>
    /// Retain the instance.
    /// </summary>
    /// <param name="type">Type of instance.</param>
    /// <param name="handle">Handle of instance to retain.</param>
    /// <returns>Retained instance, or Null if <paramref name="handle"/> is <see cref="IntPtr.Zero"/>.</returns>
    public static NSObject? Retain([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)] Type type, IntPtr handle)
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
#if NET7_0_OR_GREATER
    [RequiresDynamicCode(SendMessageRdcMessage)]
#endif
    public void SendMessage(Selector selector, object? arg) =>
        SendMessageCore(objc_msgSend, this.Handle, selector, arg);


    /// <summary>
    /// Send message to instance.
    /// </summary>
    /// <param name="selector">Selector.</param>
    /// <param name="args">Arguments.</param>
#if NET7_0_OR_GREATER
    [RequiresDynamicCode(SendMessageRdcMessage)]
#endif
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
#if NET7_0_OR_GREATER
    [RequiresDynamicCode(SendMessageRdcMessage)]
#endif
    public static void SendMessage(IntPtr obj, Selector selector, object? arg) =>
        SendMessageCore(objc_msgSend, obj, selector, arg);


    /// <summary>
    /// Send message to instance.
    /// </summary>
    /// <param name="obj">Handle of instance.</param>
    /// <param name="selector">Selector.</param>
    /// <param name="args">Arguments.</param>
#if NET7_0_OR_GREATER
    [RequiresDynamicCode(SendMessageRdcMessage)]
#endif
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
#if NET7_0_OR_GREATER
    [RequiresDynamicCode(SendMessageRdcMessage)]
#endif
    public T SendMessage<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)] T>(Selector selector) =>
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
#if NET7_0_OR_GREATER
    [RequiresDynamicCode(SendMessageRdcMessage)]
#endif
    public T SendMessage<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)] T>(Selector selector, params object?[] args)
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
#if NET7_0_OR_GREATER
    [RequiresDynamicCode(SendMessageRdcMessage)]
#endif
    public static T SendMessage<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)] T>(IntPtr obj, Selector selector) =>
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
#if NET7_0_OR_GREATER
    [RequiresDynamicCode(SendMessageRdcMessage)]
#endif
    public static T SendMessage<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)] T>(IntPtr obj, Selector selector, params object?[] args)
    {
        if (args.Length == 0)
            return (T)SendMessageCore(objc_msgSend, obj, selector, typeof(T));
        return (T)SendMessageCore(objc_msgSend, obj, selector, typeof(T), args);
    }
#pragma warning restore CS8600
#pragma warning restore CS8603


    // Send message for testing purpose
#if NET7_0_OR_GREATER
    [RequiresDynamicCode(SendMessageRdcMessage)]
#endif
    internal static object? SendMessageCore(IntPtr obj, Selector sel, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)] Type? returnType, params object?[] args) =>
        SendMessageCore(objc_msgSend, obj, sel, returnType, args);


#pragma warning disable CS8600
#pragma warning disable CS8603
    // Core implementation of send message to object.
    static void SendMessageCore(void* msgSendFunc, IntPtr obj, Selector sel) // optimize for calling method without parameter and return value
    {
        VerifyHandle(obj);
        ((delegate*unmanaged<IntPtr, IntPtr, void>)msgSendFunc)(obj, sel.Handle);
    }
#if NET7_0_OR_GREATER
    [RequiresDynamicCode(SendMessageRdcMessage)]
#endif
    static void SendMessageCore(void* msgSendFunc, IntPtr obj, Selector sel, object? arg) // optimize for property setter
    {
        VerifyHandle(obj);
        var nativeArg = NativeTypeConversion.ToNativeParameter(arg);
        if (nativeArg is IntPtr intPtrValue)
            ((delegate*unmanaged<IntPtr, IntPtr, IntPtr, void>)msgSendFunc)(obj, sel.Handle, intPtrValue);
        else if (nativeArg is bool boolValue)
            ((delegate*unmanaged<IntPtr, IntPtr, bool, void>)msgSendFunc)(obj, sel.Handle, boolValue);
        else if (nativeArg is int intValue)
            ((delegate*unmanaged<IntPtr, IntPtr, int, void>)msgSendFunc)(obj, sel.Handle, intValue);
        else if (nativeArg is uint uintValue)
            ((delegate*unmanaged<IntPtr, IntPtr, uint, void>)msgSendFunc)(obj, sel.Handle, uintValue);
        else if (nativeArg is float floatValue)
            ((delegate*unmanaged<IntPtr, IntPtr, float, void>)msgSendFunc)(obj, sel.Handle, floatValue);
        else if (nativeArg is double doubleValue)
            ((delegate*unmanaged<IntPtr, IntPtr, double, void>)msgSendFunc)(obj, sel.Handle, doubleValue);
        else
            SendMessageCore(msgSendFunc, obj, sel, null, new object?[] { arg });
    }
#if NET7_0_OR_GREATER
    [RequiresDynamicCode(SendMessageRdcMessage)]
#endif
    static object? SendMessageCore(void* msgSendFunc, IntPtr obj, Selector sel, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)] Type returnType) // optimize for property getter
    {
        var nativeReturnType = NativeTypeConversion.ToNativeType(returnType);
        if (nativeReturnType == typeof(IntPtr))
            return NativeTypeConversion.FromNativeParameter(((delegate*unmanaged<IntPtr, IntPtr, IntPtr>)msgSendFunc)(obj, sel.Handle), returnType);
        if (nativeReturnType == typeof(bool))
            return NativeTypeConversion.FromNativeParameter(((delegate*unmanaged<IntPtr, IntPtr, bool>)msgSendFunc)(obj, sel.Handle), returnType);
        if (nativeReturnType == typeof(short))
            return NativeTypeConversion.FromNativeParameter(((delegate*unmanaged<IntPtr, IntPtr, short>)msgSendFunc)(obj, sel.Handle), returnType);
        if (nativeReturnType == typeof(ushort))
            return NativeTypeConversion.FromNativeParameter(((delegate*unmanaged<IntPtr, IntPtr, ushort>)msgSendFunc)(obj, sel.Handle), returnType);
        if (nativeReturnType == typeof(int))
            return NativeTypeConversion.FromNativeParameter(((delegate*unmanaged<IntPtr, IntPtr, int>)msgSendFunc)(obj, sel.Handle), returnType);
        if (nativeReturnType == typeof(uint))
            return NativeTypeConversion.FromNativeParameter(((delegate*unmanaged<IntPtr, IntPtr, uint>)msgSendFunc)(obj, sel.Handle), returnType);
        if (nativeReturnType == typeof(long))
            return NativeTypeConversion.FromNativeParameter(((delegate*unmanaged<IntPtr, IntPtr, long>)msgSendFunc)(obj, sel.Handle), returnType);
        if (nativeReturnType == typeof(ulong))
            return NativeTypeConversion.FromNativeParameter(((delegate*unmanaged<IntPtr, IntPtr, ulong>)msgSendFunc)(obj, sel.Handle), returnType);
        if (nativeReturnType == typeof(float))
            return NativeTypeConversion.FromNativeParameter(((delegate*unmanaged<IntPtr, IntPtr, float>)msgSendFunc)(obj, sel.Handle), returnType);
        if (nativeReturnType == typeof(double))
            return NativeTypeConversion.FromNativeParameter(((delegate*unmanaged<IntPtr, IntPtr, double>)msgSendFunc)(obj, sel.Handle), returnType);
        return SendMessageCore(msgSendFunc, obj, sel, returnType, Array.Empty<object?>());
    }
#if NET7_0_OR_GREATER
    [RequiresDynamicCode(SendMessageRdcMessage)]
#endif
    static object? SendMessageCore(void* msgSendFunc, IntPtr obj, Selector sel, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)] Type? returnType, params object?[] args)
    {
        // check parameter
        VerifyHandle(obj);

        // convert to native types
        var nativeReturnType = returnType is not null ? NativeTypeConversion.ToNativeType(returnType) : null;
        var argCount = args.Length;
        var nativeArgs = new object[argCount];
        var nativeArgTypes = new Type[argCount].Also(it =>
        {
            for (var i = it.Length - 1; i >= 0; --i)
            {
                nativeArgs[i] = NativeTypeConversion.ToNativeParameter(args[i]);
                it[i] = nativeArgs[i].GetType();
            }
        });

        // find or create stub
        var stubInfo = SendMessageStubs.Lock(stubs =>
        {
            // find existing stub
            var argCount = args.Length;
            if (stubs.TryGetValue(argCount, out List<SendMessageStubInfo>? stubInfoList))
            {
                for (var i = stubInfoList.Count - 1; i >= 0; --i)
                {
                    var candidateStubInfo = (SendMessageStubInfo?)stubInfoList[i];
                    var candidateArgTypes = candidateStubInfo!.ParameterTypes;
                    for (var j = argCount - 1; j >= 0; --j)
                    {
                        if (candidateArgTypes[j] != nativeArgTypes[j])
                        {
                            candidateStubInfo = null;
                            break;
                        }
                    }
                    if (candidateStubInfo is not null && candidateStubInfo.ReturnType == nativeReturnType)
                        return candidateStubInfo;
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
            if (stubInfoList is null)
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
            if (returnType is null)
                return null;
            if (nativeReturnValue is null)
            {
                if (returnType.IsValueType)
                    throw new InvalidOperationException("No value returned from sending message.");
                return null;
            }
            return NativeTypeConversion.FromNativeParameter(nativeReturnValue, returnType);
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
    /// Pointer to native objc_msgSend() function.
    /// </summary>
    internal void* SendMessageNative => objc_msgSend;


    /// <summary>
    /// Send message to super class of instance.
    /// </summary>
    /// <param name="selector">Selector.</param>
    public void SendMessageToSuper(Selector selector)
    {
        this.VerifyReleased();
        var superClass = this.Class.SuperClass;
        if (superClass is null)
            return;
        var superInfo = new objc_super { receiver = this.Handle, super_class = superClass.Handle };
        SendMessageCore(objc_msgSendSuper, new IntPtr(&superInfo), selector);
    }


    /// <summary>
    /// Send message to super class of instance.
    /// </summary>
    /// <param name="selector">Selector.</param>
    /// <param name="arg">Argument.</param>
#if NET7_0_OR_GREATER
    [RequiresDynamicCode(SendMessageRdcMessage)]
#endif
    public void SendMessageToSuper(Selector selector, object? arg)
    {
        this.VerifyReleased();
        var superClass = this.Class.SuperClass;
        if (superClass is null)
            return;
        var superInfo = new objc_super { receiver = this.Handle, super_class = superClass.Handle };
        SendMessageCore(objc_msgSendSuper, new IntPtr(&superInfo), selector, arg);
    }


    /// <summary>
    /// Send message to super class of instance.
    /// </summary>
    /// <param name="selector">Selector.</param>
    /// <param name="args">Arguments.</param>
#if NET7_0_OR_GREATER
    [RequiresDynamicCode(SendMessageRdcMessage)]
#endif
    public void SendMessageToSuper(Selector selector, params object?[] args)
    {
        this.VerifyReleased();
        var superClass = this.Class.SuperClass;
        if (superClass is null)
            return;
        var superInfo = new objc_super { receiver = this.Handle, super_class = superClass.Handle };
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
#if NET7_0_OR_GREATER
    [RequiresDynamicCode(SendMessageRdcMessage)]
#endif
    public T SendMessageToSuper<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)] T>(Selector selector)
    {
        this.VerifyReleased();
        var superClass = this.Class.SuperClass;
        if (superClass is null)
            throw new InvalidOperationException("No super class found.");
        var superInfo = new objc_super { receiver = this.Handle, super_class = superClass.Handle };
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
#if NET7_0_OR_GREATER
    [RequiresDynamicCode(SendMessageRdcMessage)]
#endif
    public T SendMessageToSuper<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)] T>(Selector selector, params object?[] args)
    {
        this.VerifyReleased();
        var superClass = this.Class.SuperClass;
        if (superClass is null)
            throw new InvalidOperationException("No super class found.");
        var superInfo = new objc_super { receiver = this.Handle, super_class = superClass.Handle };
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
        if (superClass is null)
            return;
        var superInfo = new objc_super { receiver = obj, super_class = superClass.Handle };
        SendMessageCore(objc_msgSendSuper, new IntPtr(&superInfo), selector);
    }


    /// <summary>
    /// Send message to super class of instance.
    /// </summary>
    /// <param name="obj">Handle of instance.</param>
    /// <param name="selector">Selector.</param>
    /// <param name="arg">Argument.</param>
#if NET7_0_OR_GREATER
    [RequiresDynamicCode(SendMessageRdcMessage)]
#endif
    public static void SendMessageToSuper(IntPtr obj, Selector selector, object? arg)
    {
        var superClass = Class.GetClass(obj).SuperClass;
        if (superClass is null)
            return;
        var superInfo = new objc_super { receiver = obj, super_class = superClass.Handle };
        SendMessageCore(objc_msgSendSuper, new IntPtr(&superInfo), selector, arg);
    }


    /// <summary>
    /// Send message to super class of instance.
    /// </summary>
    /// <param name="obj">Handle of instance.</param>
    /// <param name="selector">Selector.</param>
    /// <param name="args">Arguments.</param>
#if NET7_0_OR_GREATER
    [RequiresDynamicCode(SendMessageRdcMessage)]
#endif
    public static void SendMessageToSuper(IntPtr obj, Selector selector, params object?[] args)
    {
        var superClass = Class.GetClass(obj).SuperClass;
        if (superClass is null)
            return;
        var superInfo = new objc_super { receiver = obj, super_class = superClass.Handle };
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
#if NET7_0_OR_GREATER
    [RequiresDynamicCode(SendMessageRdcMessage)]
#endif
    public static T SendMessageToSuper<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)] T>(IntPtr obj, Selector selector)
    {
        var superClass = Class.GetClass(obj).SuperClass;
        if (superClass is null)
            throw new InvalidOperationException("No super class found.");
        var superInfo = new objc_super { receiver = obj, super_class = superClass.Handle };
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
#if NET7_0_OR_GREATER
    [RequiresDynamicCode(SendMessageRdcMessage)]
#endif
    public static T SendMessageToSuper<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)] T>(IntPtr obj, Selector selector, params object?[] args)
    {
        var superClass = Class.GetClass(obj).SuperClass;
        if (superClass is null)
            throw new InvalidOperationException("No super class found.");
        var superInfo = new objc_super { receiver = obj, super_class = superClass.Handle };
        return (T)SendMessageCore(objc_msgSendSuper, new IntPtr(&superInfo), selector, typeof(T), args);
    }
#pragma warning restore CS8600
#pragma warning restore CS8603
    
    
    /// <summary>
    /// Set value of property.
    /// </summary>
    /// <param name="property">Property.</param>
    /// <param name="value">Value.</param>
    public void SetProperty(Property property, bool value)
    {
        if (!property.Class.IsAssignableFrom(this.Class))
            throw new ArgumentException($"Property '{property}' is not owned by class '{this.Class}'.");
        if (property.IsReadOnly)
            throw new InvalidOperationException($"Cannot set value to read-only property '{property.Name}'.");
#pragma warning disable IL3050
        this.SendMessage(property.Setter!, value);
#pragma warning restore IL3050
    }
    
    
    /// <summary>
    /// Set value of property.
    /// </summary>
    /// <param name="property">Property.</param>
    /// <param name="value">Value.</param>
    public void SetProperty(Property property, int value)
    {
        if (!property.Class.IsAssignableFrom(this.Class))
            throw new ArgumentException($"Property '{property}' is not owned by class '{this.Class}'.");
        if (property.IsReadOnly)
            throw new InvalidOperationException($"Cannot set value to read-only property '{property.Name}'.");
#pragma warning disable IL3050
        this.SendMessage(property.Setter!, value);
#pragma warning restore IL3050
    }
    
    
    /// <summary>
    /// Set value of property.
    /// </summary>
    /// <param name="property">Property.</param>
    /// <param name="value">Value.</param>
    public void SetProperty(Property property, uint value)
    {
        if (!property.Class.IsAssignableFrom(this.Class))
            throw new ArgumentException($"Property '{property}' is not owned by class '{this.Class}'.");
        if (property.IsReadOnly)
            throw new InvalidOperationException($"Cannot set value to read-only property '{property.Name}'.");
#pragma warning disable IL3050
        this.SendMessage(property.Setter!, value);
#pragma warning restore IL3050
    }
    
    
    /// <summary>
    /// Set value of property.
    /// </summary>
    /// <param name="property">Property.</param>
    /// <param name="value">Value.</param>
    public void SetProperty(Property property, nint value)
    {
        if (!property.Class.IsAssignableFrom(this.Class))
            throw new ArgumentException($"Property '{property}' is not owned by class '{this.Class}'.");
        if (property.IsReadOnly)
            throw new InvalidOperationException($"Cannot set value to read-only property '{property.Name}'.");
#pragma warning disable IL3050
        this.SendMessage(property.Setter!, value);
#pragma warning restore IL3050
    }
    
    
    /// <summary>
    /// Set value of property.
    /// </summary>
    /// <param name="property">Property.</param>
    /// <param name="value">Value.</param>
    public void SetProperty(Property property, float value)
    {
        if (!property.Class.IsAssignableFrom(this.Class))
            throw new ArgumentException($"Property '{property}' is not owned by class '{this.Class}'.");
        if (property.IsReadOnly)
            throw new InvalidOperationException($"Cannot set value to read-only property '{property.Name}'.");
#pragma warning disable IL3050
        this.SendMessage(property.Setter!, value);
#pragma warning restore IL3050
    }
    
    
    /// <summary>
    /// Set value of property.
    /// </summary>
    /// <param name="property">Property.</param>
    /// <param name="value">Value.</param>
    public void SetProperty(Property property, double value)
    {
        if (!property.Class.IsAssignableFrom(this.Class))
            throw new ArgumentException($"Property '{property}' is not owned by class '{this.Class}'.");
        if (property.IsReadOnly)
            throw new InvalidOperationException($"Cannot set value to read-only property '{property.Name}'.");
#pragma warning disable IL3050
        this.SendMessage(property.Setter!, value);
#pragma warning restore IL3050
    }
    
    
    /// <summary>
    /// Set value of property.
    /// </summary>
    /// <param name="property">Property.</param>
    /// <param name="value">Value.</param>
    public void SetProperty(Property property, NSObject? value)
    {
        if (!property.Class.IsAssignableFrom(this.Class))
            throw new ArgumentException($"Property '{property}' is not owned by class '{this.Class}'.");
        if (property.IsReadOnly)
            throw new InvalidOperationException($"Cannot set value to read-only property '{property.Name}'.");
#pragma warning disable IL3050
        this.SendMessage(property.Setter!, value?.Handle ?? default);
#pragma warning restore IL3050
    }


    /// <summary>
    /// Set value of property.
    /// </summary>
    /// <param name="property">Property.</param>
    /// <param name="value">Value.</param>
    /// <typeparam name="T">Type of property.</typeparam>
#if NET7_0_OR_GREATER
    [RequiresDynamicCode(SetPropertyRdcMessage)]
#endif
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
#if NET7_0_OR_GREATER
    [RequiresDynamicCode(SetVariableRdcMessage)]
#endif
    public void SetVariable(Variable ivar, object? value)
    {
        this.VerifyReleased();
        SetVariable(this.Handle, ivar, value);
    }


    /// <summary>
    /// Set instance variable.
    /// </summary>
    /// <param name="obj">Handle of instance.</param>
    /// <param name="ivar">Instance variable.</param>
    /// <param name="value">Value.</param>
#if NET7_0_OR_GREATER
    [RequiresDynamicCode(SetVariableRdcMessage)]
#endif
    public static void SetVariable(IntPtr obj, Variable ivar, object? value)
    {
        VerifyHandle(obj);
        var type = ivar.Type;
        var size = ivar.Size;
        var valueType = value?.GetType();
        if (valueType is null)
        {
            if (type.IsArray)
                throw new ArgumentException("Cannot set Null to variable with array type.");
            if (type != typeof(NSObject)
                && type != typeof(Class)
                && type != typeof(Selector))
            {
                throw new ArgumentException($"Incompatible type: Object, {type.Name} expected.");
            }
        }
        else
        {
            var typeToCheck = valueType.IsArray ? valueType.GetElementType()! : valueType;
            if (typeToCheck.IsClass 
                && !typeof(NSObject).IsAssignableFrom(typeToCheck)
                && typeToCheck != typeof(Class)
                && typeToCheck != typeof(Selector))
            {
                throw new NotSupportedException($"Setting variable with CLR object or CLR object array is unsupported. Only NSObject, Class and Selector are supported.");
            }
        }
        fixed (byte* valuePtr = new byte[size])
        {
            if (value is Array array)
            {
                if (valueType!.GetArrayRank() > 1)
                    throw new NotSupportedException("Only 1-dimensional array is supported.");
                var arrayLength = array.GetLength(0);
                if (arrayLength <= 0)
                    return;
                var elementType = valueType.GetElementType()!;
                var elementSize = NativeTypeConversion.GetNativeValueSize(elementType);
                if (arrayLength * elementSize > size)
                    throw new ArgumentException($"Size of array is too large: {arrayLength * elementSize}, maximum size is {size}.");
                var elementPtr = valuePtr;
                for (var i = 0; i < arrayLength; ++i)
                    elementPtr += NativeTypeConversion.ToNativeValue(array.GetValue(i), elementPtr);
            }
            else
                NativeTypeConversion.ToNativeValue(value, valuePtr);
            object_setInstanceVariable(obj, ivar.Name, valuePtr);
        }
    }


    /// <inheritdoc/>
    public override string ToString() =>
        $"0x{this.Handle:x16}";
    

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