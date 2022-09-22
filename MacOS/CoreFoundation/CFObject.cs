using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;

namespace CarinaStudio.MacOS.CoreFoundation
{
    /// <summary>
    /// Object of Core Foundation.
    /// </summary>
    public class CFObject : IShareableDisposable<CFObject>
    {
        // Static fields.
        static readonly IDictionary<uint, string> CachedTypeDescriptions = new ConcurrentDictionary<uint, string>();
        static readonly IDictionary<Type, MethodInfo> ObjectWrappingMethods = new ConcurrentDictionary<Type, MethodInfo>();


        // Fields.
        volatile IntPtr handle;
        bool ownsInstance;


        /// <summary>
        /// Initialize new <see cref="CFObject"/> instance.
        /// </summary>
        /// <param name="handle">Handle of instance.</param>
        /// <param name="ownsInstance">True to get ownership of instance.</param>
        protected CFObject(IntPtr handle, bool ownsInstance) : this(handle, handle != IntPtr.Zero ? Native.CFGetTypeID(handle) : 0, ownsInstance)
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
            var wrappingMethod = GetObjectWrappingMethod<T>();
            var handle = this.handle;
            var ownsInstance = this.ownsInstance;
            this.handle = IntPtr.Zero;
            this.ownsInstance = false;
            return (T)wrappingMethod.Invoke(null, new object?[] { handle, ownsInstance }).AsNonNull();
        }


        /// <inheritdoc/>
        public override bool Equals(object? obj) =>
            obj is CFObject cfo && this.handle == cfo.handle;


        /// <inheritdoc/>
        public override int GetHashCode() =>
            (int)(this.handle.ToInt64() & 0x7fffffff);
        

        // Get static method to wrap native instance.
        static MethodInfo GetObjectWrappingMethod<T>() where T : CFObject =>
            ObjectWrappingMethods.TryGetValue(typeof(T), out var method)
                ? method
                : typeof(T).GetMethod("Wrap", BindingFlags.Public | BindingFlags.Static).Let(it =>
                {
                    if (it != null)
                    {
                        var parameters = it.GetParameters();
                        if (parameters.Length == 2 
                            && parameters[0].ParameterType == typeof(IntPtr)
                            && parameters[1].ParameterType == typeof(bool)
                            && typeof(T).IsAssignableFrom(it.ReturnType))
                        {
                            ObjectWrappingMethods.TryAdd(typeof(T), it);
                            return it;
                        }
                    }
                    throw new InvalidCastException($"Cannot find method to wrap CFObject as '{typeof(T)}'.");
                });


        /// <summary>
        /// Get native handle of instance.
        /// </summary>
        public IntPtr Handle { get => this.handle; }


        /// <inheritdoc/>
        void IDisposable.Dispose() => this.Release();


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
                Native.CFRelease(this.handle);
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
            GC.SuppressFinalize(this);
            this.OnRelease();
            this.handle = IntPtr.Zero;
        }


        /// <summary>
        /// Retain the object.
        /// </summary>
        /// <returns>New instanec of retained object.</returns>
        public virtual CFObject Retain()
        {
            this.VerifyReleased();
            return new CFObject(Native.CFRetain(this.handle), true);
        }


        /// <summary>
        /// Retain the object with specific type.
        /// </summary>
        /// <typeparam name="T">Specific type.</typeparam>
        /// <returns>New instanec of retained object.</returns>
        public T Retain<T>() where T : CFObject
        {
            if (this is T)
                return (T)this.Retain();
            this.VerifyReleased();
            return Wrap<T>(Native.CFRetain(this.handle), true);
        }


        /// <summary>
        /// Retain an object.
        /// </summary>
        /// <param name="cf">Handle of instance.</param>
        /// <returns>Retained object.</returns>
        public static CFObject Retain(IntPtr cf) =>
            new CFObject(Native.CFRetain(cf), true);
        

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
                description = Native.CFCopyTypeIDDescription(this.TypeId).Let(it =>
                {
                    var length = Native.CFStringGetLength(it);
                    var buffer = new char[(int)length];
                    unsafe
                    {
                        fixed (char* p = buffer)
                            Native.CFStringGetCharacters(it, new CFRange(0, length), p);
                    }
                    Native.CFRelease(it);
                    return new string(buffer);
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


        /// <summary>
        /// Wrap a native object.
        /// </summary>
        /// <param name="cf">Handle of instance.</param>
        /// <param name="ownsInstance">True to owns the native object.</param>
        /// <returns>Wrapped object.</returns>
        public static CFObject Wrap(IntPtr cf, bool ownsInstance = false) =>
            new CFObject(cf, ownsInstance);
        

        /// <summary>
        /// Wrap a native object.
        /// </summary>
        /// <param name="cf">Handle of instance.</param>
        /// <param name="ownsInstance">True to owns the native object.</param>
        /// <typeparam name="T">Target type.</typeparam>
        /// <returns>Wrapped object.</returns>
        public static T Wrap<T>(IntPtr cf, bool ownsInstance = false) where T : CFObject =>
            (T)GetObjectWrappingMethod<T>().Invoke(null, new object?[] { cf, ownsInstance }).AsNonNull();
    }
}