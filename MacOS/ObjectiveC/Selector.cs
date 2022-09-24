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
		static extern IntPtr sel_getName(IntPtr sel);
        [DllImport(NativeLibraryNames.ObjectiveC)]
		static extern IntPtr sel_getUid(string str);
        [DllImport(NativeLibraryNames.ObjectiveC)]
		static extern IntPtr sel_registerName(string name);


        // Static fields.
        static readonly IDictionary<string, Selector> CachedSelectors = new ConcurrentDictionary<string, Selector>();


        // Constructor.
        internal Selector(IntPtr handle, string name)
        {
            this.Handle = handle;
            this.Name = name;
        }


        /// <inheritdoc/>
        public bool Equals(Selector? selector) =>
            selector != null && selector.Name == this.Name;


        /// <inheritdoc/>
        public override bool Equals(object? obj) =>
            obj is Selector s && this.Equals(s);


        /// <inheritdoc/>
        public override int GetHashCode() =>
            this.Name.GetHashCode();


        /// <summary>
        /// Get registered selector or create new one.
        /// </summary>
        /// <param name="name">Name of selector.</param>
        /// <returns>Selector.</returns>
        public static Selector GetOrCreate(string name)
        {
            if (CachedSelectors.TryGetValue(name, out var selector))
                return selector;
            var handle = sel_registerName(name);
            if (handle == IntPtr.Zero)
                throw new Exception($"Unable to register selector '{name}'.");
            return new Selector(handle, name).Also(it => CachedSelectors.TryAdd(name, it));
        }


        /// <summary>
        /// Get registered selector or create new one.
        /// </summary>
        /// <param name="name">Name of selector.</param>
        /// <returns>Selector.</returns>
        internal static Selector GetOrCreateUid(string name)
        {
            var handle = sel_getUid(name);
            if (handle == IntPtr.Zero)
                throw new Exception($"Unable to register method '{name}' as selector.");
            return new Selector(handle, name);
        }


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
            l?.Equals(r) == true ? false : true;


        /// <inheritdoc/>
        public override string ToString() =>
            this.Name;
    }
}