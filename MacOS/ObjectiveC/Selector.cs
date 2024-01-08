using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace CarinaStudio.MacOS.ObjectiveC
{
    /// <summary>
    /// Selector of NSObject.
    /// </summary>
    public unsafe sealed class Selector : IEquatable<Selector>
    {
        // Native symbols.
        [DllImport(NativeLibraryNames.ObjectiveC)]
		static extern sbyte* sel_getName(IntPtr sel);
        [DllImport(NativeLibraryNames.ObjectiveC)]
		static extern IntPtr sel_getUid(string str);
        [DllImport(NativeLibraryNames.ObjectiveC)]
		static extern IntPtr sel_registerName(string name);


        // Static fields.
        static readonly IDictionary<IntPtr, Selector> CachedSelectorsByHandle = new ConcurrentDictionary<IntPtr, Selector>();
        static readonly IDictionary<string, Selector> CachedSelectorsByName = new ConcurrentDictionary<string, Selector>();
        static readonly IDictionary<string, Selector> CachedSelectorsByUid = new ConcurrentDictionary<string, Selector>();


        // Constructor.
        Selector(IntPtr handle)
        {
            this.Handle = handle;
            this.Name = new string(sel_getName(handle));
        }
        Selector(IntPtr handle, string name)
        {
            this.Handle = handle;
            this.Name = name;
        }


        /// <inheritdoc/>
        public bool Equals(Selector? selector) =>
            selector is not null && selector.Name == this.Name;


        /// <inheritdoc/>
        public override bool Equals(object? obj) =>
            obj is Selector s && this.Equals(s);
        

        /// <summary>
        /// Get registered selector by its handle.
        /// </summary>
        /// <param name="handle">Handle of selector.</param>
        /// <returns>Selector.</returns>
        public static Selector FromHandle(IntPtr handle)
        {
            if (handle == IntPtr.Zero)
                throw new ArgumentException("Handle of selector cannot be null.");
            if (CachedSelectorsByHandle.TryGetValue(handle, out var selector))
                return selector;
            return new Selector(handle).Also(it =>
            {
                CachedSelectorsByHandle.TryAdd(handle, it);
            });
        }
        

        /// <summary>
        /// Get registered selector or create new one.
        /// </summary>
        /// <param name="name">Name of selector.</param>
        /// <returns>Selector.</returns>
        public static Selector FromName(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException($"Invalid selector name: {name}");
            if (CachedSelectorsByName.TryGetValue(name, out var selector))
                return selector;
            var handle = sel_registerName(name);
            if (handle == IntPtr.Zero)
                throw new Exception($"Unable to register selector '{name}'.");
            if (CachedSelectorsByHandle.TryGetValue(handle, out selector))
                return selector;
            return new Selector(handle, name).Also(it => 
            {
                CachedSelectorsByHandle.TryAdd(handle, it);
                CachedSelectorsByName.TryAdd(name, it);
            });
        }


        /// <summary>
        /// Get registered selector or create new one.
        /// </summary>
        /// <param name="uid">Name of selector.</param>
        /// <returns>Selector.</returns>
        public static Selector FromUid(string uid)
        {
            if (string.IsNullOrEmpty(uid))
                throw new ArgumentException($"Invalid selector UID: {uid}");
            if (CachedSelectorsByUid.TryGetValue(uid, out var selector))
                return selector;
            var handle = sel_getUid(uid);
            if (handle == IntPtr.Zero)
                throw new Exception($"Unable to register method '{uid}' as selector.");
            if (CachedSelectorsByHandle.TryGetValue(handle, out selector))
                return selector;
            return new Selector(handle).Also(it => 
            {
                CachedSelectorsByHandle.TryAdd(handle, it);
                CachedSelectorsByUid.TryAdd(uid, it);
            });
        }


        /// <inheritdoc/>
        public override int GetHashCode() =>
            this.Name.GetHashCode();


        /// <summary>
        /// Get handle of selector.
        /// </summary>
        public IntPtr Handle { get; }


        /// <summary>
        /// Get name of selector.
        /// </summary>
        public string Name { get; }


        /// <summary>
        /// Equality operator.
        /// </summary>
        public static bool operator ==(Selector? l, Selector? r) =>
            l?.Equals(r) ?? false;
        

        /// <summary>
        /// Inequality operator.
        /// </summary>
        public static bool operator !=(Selector? l, Selector? r) =>
            l?.Equals(r) == false || (l is null) != (r is null);


        /// <inheritdoc/>
        public override string ToString() =>
            this.Name;
    }
}