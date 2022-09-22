using System;

namespace CarinaStudio.MacOS.CoreFoundation
{
    /// <summary>
    /// Object of Core Foundation.
    /// </summary>
    public class CFObject : IShareableDisposable<CFObject>
    {
        // Fields.
        volatile IntPtr handle;
        readonly bool ownsInstance;


        /// <summary>
        /// Initialize new <see cref="CFObject"/> instance.
        /// </summary>
        /// <param name="handle">Handle of instance.</param>
        /// <param name="ownsInstance">True to get ownership of instance.</param>
        protected CFObject(IntPtr handle, bool ownsInstance)
        {
            if (handle == IntPtr.Zero)
            {
                GC.SuppressFinalize(this);
                throw new ArgumentException("Handle of instance cannot be null.");
            }
            this.handle = handle;
            this.ownsInstance = ownsInstance;
        }


        /// <summary>
        /// Finalizer.
        /// </summary>
        ~CFObject() => this.Release();


        /// <inheritdoc/>
        public override bool Equals(object? obj) =>
            obj is CFObject cfo && this.handle == cfo.handle;


        /// <inheritdoc/>
        public override int GetHashCode() =>
            (int)(this.handle.ToInt64() & 0x7fffffff);


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
        /// <returns>Wrapped object.</returns>
        public static CFObject Wrap(IntPtr cf) =>
            new CFObject(cf, false);
    }
}