using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.InteropServices;

namespace CarinaStudio.MacOS.CoreFoundation
{
    /// <summary>
    /// Object of Core Foundation.
    /// </summary>
    public class CFObject : IShareableDisposable<CFObject>
    {
        // Native symbols.
        [DllImport(NativeLibraryNames.CoreFoundation)]
        static extern IntPtr CFCopyTypeIDDescription(uint type_id);
        [DllImport(NativeLibraryNames.CoreFoundation)]
        static extern uint CFGetTypeID(IntPtr cf);


        // Static fields.
        static readonly IDictionary<uint, string> CachedTypeDescriptions = new ConcurrentDictionary<uint, string>();
        static readonly IDictionary<Type, ConstructorInfo> WrappingConstructors = new ConcurrentDictionary<Type, ConstructorInfo>();


        // Fields.
        volatile IntPtr handle;
        readonly bool ownsInstance;


        /// <summary>
        /// Initialize new <see cref="CFObject"/> instance.
        /// </summary>
        /// <param name="handle">Handle of instance.</param>
        /// <param name="ownsInstance">True to get ownership of instance.</param>
        protected CFObject(IntPtr handle, bool ownsInstance) : this(handle, handle != IntPtr.Zero ? CFGetTypeID(handle) : 0, ownsInstance)
        { }


        /// <summary>
        /// Initialize new <see cref="CFObject"/> instance.
        /// </summary>
        /// <param name="handle">Handle of instance.</param>
        /// <param name="typeId">Type ID.</param>
        /// <param name="ownsInstance">True to get ownership of instance.</param>
        protected CFObject(IntPtr handle, uint typeId, bool ownsInstance)
        {
            if (handle == IntPtr.Zero)
            {
                GC.SuppressFinalize(this);
                throw new ArgumentException("Handle of instance cannot be null.");
            }
            this.handle = handle;
            this.TypeId = typeId;
            this.ownsInstance = ownsInstance;
            if (!ownsInstance)
                GC.SuppressFinalize(this);
        }


        /// <summary>
        /// Finalizer.
        /// </summary>
        ~CFObject() => this.Release();


        /// <inheritdoc/>
        public override bool Equals(object? obj) =>
            obj is CFObject cfo && this.handle == cfo.handle;


        // ReSharper disable NonReadonlyMemberInGetHashCode
        /// <inheritdoc/>
        public override int GetHashCode() =>
            (int)(this.handle.ToInt64() & 0x7fffffff);
        // ReSharper restore NonReadonlyMemberInGetHashCode
        

        // Get static method to wrap native instance.
        static ConstructorInfo GetWrappingConstructor<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)] T>() where T : CFObject =>
            GetWrappingConstructor(typeof(T));
        static ConstructorInfo GetWrappingConstructor([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)] Type type) =>
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
                                if (parameters[0].ParameterType == typeof(IntPtr))
                                    CtorWith1Arg = ctor;
                                break;
                            case 2:
                                if (parameters[0].ParameterType == typeof(IntPtr)
                                    && parameters[1].ParameterType == typeof(bool))
                                {
                                    WrappingConstructors.TryAdd(type, ctor);
                                    return ctor;
                                }
                                break;
                        }
                    }
                    if (CtorWith1Arg is not null)
                    {
                        WrappingConstructors.TryAdd(type, CtorWith1Arg);
                        return CtorWith1Arg;
                    }
                    throw new InvalidCastException($"Cannot find way to construct {type.Name}.");
                });


        /// <summary>
        /// Get native handle of instance.
        /// </summary>
        public IntPtr Handle => this.handle;


        /// <inheritdoc/>
        void IDisposable.Dispose() => this.Release();


        /// <summary>
        /// Wrap a native object.
        /// </summary>
        /// <param name="cf">Handle of instance.</param>
        /// <param name="ownsInstance">True to owns the native object.</param>
        /// <returns>Wrapped object.</returns>
        public static CFObject FromHandle(IntPtr cf, bool ownsInstance = false) =>
            new(cf, ownsInstance);
        

        /// <summary>
        /// Wrap a native object.
        /// </summary>
        /// <param name="cf">Handle of instance.</param>
        /// <param name="ownsInstance">True to owns the native object.</param>
        /// <typeparam name="T">Target type.</typeparam>
        /// <returns>Wrapped object.</returns>
        public static T FromHandle<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)] T>(IntPtr cf, bool ownsInstance = false) where T : CFObject =>
            (T)FromHandle(typeof(T), cf, ownsInstance);


        /// <summary>
        /// Wrap a native object.
        /// </summary>
        /// <param name="type">Target type.</param>
        /// <param name="cf">Handle of instance.</param>
        /// <param name="ownsInstance">True to owns the native object.</param>
        /// <returns>Wrapped object.</returns>
        public static CFObject FromHandle([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)] Type type, IntPtr cf, bool ownsInstance = false)
        {
            if (type == typeof(CFObject))
                return new CFObject(cf, ownsInstance);
            if (!typeof(CFObject).IsAssignableFrom(type))
                throw new ArgumentException($"Invalid type: {type.Name}.");
            var ctor = GetWrappingConstructor(type);
            if (ctor.GetParameters().Length == 2)
                return (CFObject)Activator.CreateInstance(type, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null, new object?[]{ cf, ownsInstance }, null).AsNonNull();
            return (CFObject)Activator.CreateInstance(type, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null, new object?[]{ cf }, null).AsNonNull();
        }


        /// <summary>
        /// Check whether the instance is default instance which cannot be released or not.
        /// </summary>
        public bool IsDefaultInstance { get; protected set; }


        /// <inheritdoc/>
        CFObject IShareableDisposable<CFObject>.Share() => this.Retain();


        /// <summary>
        /// Check whether instance has been released or not.
        /// </summary>
        public bool IsReleased => this.handle == IntPtr.Zero;


        /// <summary>
        /// Called when releasing instance.
        /// </summary>
        public virtual void OnRelease()
        { 
            if (this.ownsInstance && this.handle != IntPtr.Zero)
                Release(this.handle);
        }


        /// <summary>
        /// Equality operator.
        /// </summary>
        public static bool operator ==(CFObject? l, CFObject? r)
        {
            if (!ReferenceEquals(l, null))
            {
                if (!ReferenceEquals(r, null))
                    return l.handle == r.handle;
                return l.handle == IntPtr.Zero;
            }
            if (!ReferenceEquals(r, null))
                return r.handle == IntPtr.Zero;
            return true;
        }


        /// <summary>
        /// Inequality operator.
        /// </summary>
        public static bool operator !=(CFObject? l, CFObject? r)
        {
            if (!ReferenceEquals(l, null))
            {
                if (!ReferenceEquals(r, null))
                    return l.handle != r.handle;
                return l.handle != IntPtr.Zero;
            }
            if (!ReferenceEquals(r, null))
                return r.handle != IntPtr.Zero;
            return false;
        }


        /// <summary>
        /// Release the instance.
        /// </summary>
        public void Release()
        {
            if (this.handle == IntPtr.Zero)
                return;
            if (this.IsDefaultInstance)
                throw new InvalidOperationException("Cannot release default instance.");
            GC.SuppressFinalize(this);
            this.OnRelease();
            this.handle = IntPtr.Zero;
        }


        /// <summary>
        /// Release given instance.
        /// </summary>
        /// <param name="cf">Handle of instance.</param>
        [DllImport(NativeLibraryNames.CoreFoundation, EntryPoint="CFRelease")]
        public static extern void Release(IntPtr cf);


        /// <summary>
        /// Retain the object.
        /// </summary>
        /// <returns>New instance of retained object.</returns>
        public virtual CFObject Retain()
        {
            this.VerifyReleased();
            return new CFObject(Retain(this.handle), true);
        }


        /// <summary>
        /// Retain the object with specific type.
        /// </summary>
        /// <typeparam name="T">Specific type.</typeparam>
        /// <returns>New instance of retained object.</returns>
        public T Retain<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)] T>() where T : CFObject
        {
            this.VerifyReleased();
            return FromHandle<T>(Retain(this.handle), true);
        }


        /// <summary>
        /// Retain given instance.
        /// </summary>
        /// <param name="cf">Handle of instance.</param>
        /// <returns>Handle of retained instance.</returns>
        [DllImport(NativeLibraryNames.CoreFoundation, EntryPoint="CFRetain")]
        public static extern IntPtr Retain(IntPtr cf);
        

        /// <inheritdoc/>
        public override string? ToString() =>
            $"0x{this.handle.ToInt64():x16}";
        

        /// <summary>
        /// Get description of type.
        /// </summary>
        public string TypeDescription
        {
            get
            {
                if (CachedTypeDescriptions.TryGetValue(this.TypeId, out var description))
                    return description;
                description = CFCopyTypeIDDescription(this.TypeId).Let(it =>
                {
                    return new CFString(it, false, true).Use(it =>
                        it.ToString().AsNonNull());
                });
                CachedTypeDescriptions.TryAdd(this.TypeId, description);
                return description;
            }
        }
        

        /// <summary>
        /// Get type ID.
        /// </summary>
        public uint TypeId { get; }
        

        /// <summary>
        /// Throw <see cref="ObjectDisposedException"/> if instance has been released.
        /// </summary>
        protected void VerifyReleased()
        {
            if (this.handle == IntPtr.Zero)
                throw new ObjectDisposedException(this.GetType().Name);
        }
    }
}