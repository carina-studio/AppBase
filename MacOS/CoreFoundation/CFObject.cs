using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
        bool ownsInstance;


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
        }


        /// <summary>
        /// Finalizer.
        /// </summary>
        ~CFObject() => this.Release();


        /// <summary>
        /// Cast to object with specific type and release this instance if needed.
        /// </summary>
        /// <typeparam name="T">Specific type.</typeparam>
        /// <returns>Object with specific type</returns>
        public T Cast<T>() where T : CFObject
        {
            if (this is T target)
                return target;
            this.VerifyReleased();
            var ctor = GetWrappingConstructor<T>();
            var handle = this.handle;
            var ownsInstance = this.ownsInstance;
            this.handle = IntPtr.Zero;
            this.ownsInstance = false;
            if (ctor.GetParameters().Length == 2)
                return (T)Activator.CreateInstance(typeof(T), BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null, new object?[]{ handle, ownsInstance }, null).AsNonNull();
            return (T)Activator.CreateInstance(typeof(T), BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null, new object?[]{ handle }, null).AsNonNull();
        }


        /// <inheritdoc/>
        public override bool Equals(object? obj) =>
            obj is CFObject cfo && this.handle == cfo.handle;


        /// <inheritdoc/>
        public override int GetHashCode() =>
            (int)(this.handle.ToInt64() & 0x7fffffff);
        

        // Get static method to wrap native instance.
        static ConstructorInfo GetWrappingConstructor<T>() where T : CFObject =>
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
                    if (CtorWith1Arg != null)
                    {
                        WrappingConstructors.TryAdd(type, CtorWith1Arg);
                        return CtorWith1Arg;
                    }
                    throw new InvalidCastException($"Cannot find way to construct {type.Name}.");
                });


        /// <summary>
        /// Get native handle of instance.
        /// </summary>
        public IntPtr Handle { get => this.handle; }


        /// <inheritdoc/>
        void IDisposable.Dispose() => this.Release();


        /// <summary>
        /// Wrap a native object.
        /// </summary>
        /// <param name="cf">Handle of instance.</param>
        /// <param name="ownsInstance">True to owns the native object.</param>
        /// <returns>Wrapped object.</returns>
        public static CFObject FromHandle(IntPtr cf, bool ownsInstance = false) =>
            new CFObject(cf, ownsInstance);
        

        /// <summary>
        /// Wrap a native object.
        /// </summary>
        /// <param name="cf">Handle of instance.</param>
        /// <param name="ownsInstance">True to owns the native object.</param>
        /// <typeparam name="T">Target type.</typeparam>
        /// <returns>Wrapped object.</returns>
        public static T FromHandle<T>(IntPtr cf, bool ownsInstance = false) where T : CFObject
        {
            if (typeof(T) == typeof(CFObject))
                return (T)new CFObject(cf, ownsInstance);
            var ctor = GetWrappingConstructor<T>();
            if (ctor.GetParameters().Length == 2)
                return (T)Activator.CreateInstance(typeof(T), BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null, new object?[]{ cf, ownsInstance }, null).AsNonNull();
            return (T)Activator.CreateInstance(typeof(T), BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null, new object?[]{ cf }, null).AsNonNull();
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
        public bool IsReleased { get => this.handle == IntPtr.Zero; }


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
            if (!object.ReferenceEquals(l, null))
            {
                if (!object.ReferenceEquals(r, null))
                    return l.handle == r.handle;
                return l.handle == IntPtr.Zero;
            }
            if (!object.ReferenceEquals(r, null))
                return r.handle == IntPtr.Zero;
            return true;
        }


        /// <summary>
        /// Inequality operator.
        /// </summary>
        public static bool operator !=(CFObject? l, CFObject? r)
        {
            if (!object.ReferenceEquals(l, null))
            {
                if (!object.ReferenceEquals(r, null))
                    return l.handle != r.handle;
                return l.handle != IntPtr.Zero;
            }
            if (!object.ReferenceEquals(r, null))
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
        /// <returns>New instanec of retained object.</returns>
        public virtual CFObject Retain()
        {
            this.VerifyReleased();
            return new CFObject(Retain(this.handle), true);
        }


        /// <summary>
        /// Retain the object with specific type.
        /// </summary>
        /// <typeparam name="T">Specific type.</typeparam>
        /// <returns>New instanec of retained object.</returns>
        public T Retain<T>() where T : CFObject
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
            string.Format("0x{0:x16}", this.handle.ToInt64());
        

        /// <summary>
        /// Get description of type.
        /// </summary>
        public unsafe string TypeDescription
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