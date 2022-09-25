using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace CarinaStudio.MacOS.ObjectiveC
{
    /// <summary>
    /// Class of Objective-C.
    /// </summary>
    public sealed unsafe class Class : IEquatable<Class>
    {
        // Native symbols.
        [DllImport(NativeLibraryNames.ObjectiveC)]
        static extern IntPtr* class_copyIvarList(IntPtr cls, out int outCount);
        [DllImport(NativeLibraryNames.ObjectiveC)]
        static extern IntPtr* class_copyPropertyList(IntPtr cls, out int outCount);
        [DllImport(NativeLibraryNames.ObjectiveC)]
        static extern IntPtr class_getClassVariable(IntPtr cls, string name);
        [DllImport(NativeLibraryNames.ObjectiveC)]
        static extern IntPtr class_getInstanceVariable(IntPtr cls, string name);
        [DllImport(NativeLibraryNames.ObjectiveC)]
        static extern sbyte* class_getName(IntPtr cls);
        [DllImport(NativeLibraryNames.ObjectiveC)]
        static extern IntPtr class_getProperty(IntPtr cls, string name);
        [DllImport(NativeLibraryNames.ObjectiveC)]
        static extern IntPtr class_getSuperclass(IntPtr cls);
        [DllImport(NativeLibraryNames.ObjectiveC)]
        static extern sbyte* ivar_getName(IntPtr ivar);
        [DllImport(NativeLibraryNames.ObjectiveC)]
        static extern IntPtr objc_getClass(string name);
        [DllImport(NativeLibraryNames.ObjectiveC)]
        static extern IntPtr object_getClass(IntPtr obj);
        [DllImport(NativeLibraryNames.ObjectiveC)]
        static extern sbyte* property_getName(IntPtr property);
        [DllImport(NativeLibraryNames.ObjectiveC, EntryPoint = NSObject.SendMessageEntryPointName)]
        static extern IntPtr SendMessageIntPtr(IntPtr target, IntPtr selector);


        // Static fields.
        static readonly Selector? AllocSelector;
        static readonly IDictionary<IntPtr, Class> CachedClassesByHandle = new ConcurrentDictionary<IntPtr, Class>();
        static readonly IDictionary<string, Class> CachedClassesByName = new ConcurrentDictionary<string, Class>();


        // Fields.
        volatile bool areAllIVarsCached;
        volatile bool areAllPropertiesCached;
        readonly IDictionary<string, MemberDescriptor> cachedCVars = new ConcurrentDictionary<string, MemberDescriptor>();
        readonly IDictionary<string, MemberDescriptor> cachedIVars = new ConcurrentDictionary<string, MemberDescriptor>();
        readonly IDictionary<string, PropertyDescriptor> cachedProperties = new ConcurrentDictionary<string, PropertyDescriptor>();
        volatile bool isRootClass;
        volatile Class? superClass;


        // Static initializer.
        static Class()
        {
            if (Platform.IsNotMacOS)
                return;
            AllocSelector = Selector.FromName("alloc");
        }


        // Constructor.
        Class(IntPtr handle, string name)
        {
            this.Handle = handle;
            this.Name = name;
        }


        /// <summary>
        /// Allocate memory for new instance with this class.
        /// </summary>
        /// <returns>Handle of allocated instance.</returns>
        public IntPtr Allocate() =>
            SendMessageIntPtr(this.Handle, AllocSelector!.Handle);
        

        /// <inheritdoc/>
        public bool Equals(Class? cls) =>
            cls != null && this.Handle == cls.Handle;


        /// <inheritdoc/>
        public override bool Equals(object? obj) =>
            obj is Class cls && this.Equals(cls);


        /// <summary>
        /// Find the class with given name.
        /// </summary>
        /// <param name="name">Name of class.</param>
        /// <returns>Class with given name, or Null if class cannot be found.</returns>
        public static Class? GetClass(string name)
        {
            if (CachedClassesByName.TryGetValue(name, out var nsClass))
                return nsClass;
            if (Platform.IsNotMacOS)
                return null;
            var handle = objc_getClass(name);
            if (handle != IntPtr.Zero)
            {
                return new Class(handle, name).Also(it => 
                {
                    CachedClassesByName.TryAdd(name, it);
                    CachedClassesByHandle.TryAdd(handle, it);
                });
            }
            return null;
        }


        /// <summary>
        /// Get class of given instance.
        /// </summary>
        /// <param name="obj">Handle of instance.</param>
        /// <returns>Class.</returns>
        public static Class GetClass(IntPtr obj)
        {
            if (obj == IntPtr.Zero)
                throw new ArgumentException("Handle of instance cannot be null.");
            return Wrap(object_getClass(obj));
        }


        /// <inheritdoc/>
        public override int GetHashCode() =>
            this.Name.GetHashCode();


        /// <summary>
        /// Get all instance variables of the class.
        /// </summary>
        /// <returns>Descriptors of instance variables.</returns>
        public MemberDescriptor[] GetInstanceVariables()
        {
            if (this.areAllIVarsCached)
                return this.cachedIVars.Values.ToArray();
            var cls = this;
            var varNames = new Dictionary<string, IntPtr>();
            while (cls != null)
            {
                var varsPtr = class_copyIvarList(cls.Handle, out var count);
                try
                {
                    for (var i = count - 1; i >= 0; --i)
                        varNames.TryAdd(new string(ivar_getName(varsPtr[i])), varsPtr[i]);
                }
                finally
                {
                    NativeMemory.Free(varsPtr);
                    cls = cls?.SuperClass;
                }
            }
            foreach (var (name, handle) in varNames)
                this.cachedIVars.TryAdd(name, new MemberDescriptor(this, handle, name));
            this.areAllIVarsCached = true;
            return this.cachedIVars.Values.ToArray();
        }


        /// <summary>
        /// Get all properties of the class.
        /// </summary>
        /// <returns>Descriptors of properties.</returns>
        public unsafe PropertyDescriptor[] GetProperties()
        {
            if (areAllPropertiesCached)
                return this.cachedProperties.Values.ToArray();
            var cls = this;
            var propertyNames = new Dictionary<string, IntPtr>();
            while (cls != null)
            {
                var propertiesPtr = class_copyPropertyList(cls.Handle, out var count);
                try
                {
                    for (var i = count - 1; i >= 0; --i)
                        propertyNames.TryAdd(new string(property_getName(propertiesPtr[i])), propertiesPtr[i]);
                }
                finally
                {
                    NativeMemory.Free(propertiesPtr);
                    cls = cls?.SuperClass;
                }
            }
            foreach (var (name, handle) in propertyNames)
                this.cachedProperties.TryAdd(name, new PropertyDescriptor(this, handle, name));
            this.areAllPropertiesCached = true;
            return this.cachedProperties.Values.ToArray();
        }


        /// <summary>
        /// Get handle of class.
        /// </summary>
        public IntPtr Handle { get; }


        /// <summary>
        /// Check whether instances of given class can be casted to this class or not.
        /// </summary>
        /// <param name="cls">Given class.</param>
        /// <returns>True if instances of given class can be casted to this class.</returns>
        public bool IsAssignableFrom(Class cls)
        {
            if (this.Equals(cls))
                return true;
            var superCls = cls.SuperClass;
            while (superCls != null)
            {
                if (this.Equals(superCls))
                    return true;
                superCls = superCls.SuperClass;
            }
            return false;
        }


        /// <summary>
        /// Get name of class.
        /// </summary>
        public string Name { get; }


        /// <summary>
        /// Get super class.
        /// </summary>
        public Class? SuperClass
        {
            get
            {
                return this.superClass 
                    ?? (this.isRootClass 
                        ? null
                        : class_getSuperclass(this.Handle).Let(it =>
                        {
                            if (it == IntPtr.Zero)
                            {
                                this.isRootClass = true;
                                return null;
                            }
                            this.superClass = Wrap(it);
                            return this.superClass;
                        }));
            }
        }


        /// <inheritdoc/>
        public override string ToString() =>
            this.Name;
        

        /// <summary>
        /// Try finding class variable of class.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="ivar">Descriptor of class variable.</param>
        /// <returns>True if variable found.</returns>
        public unsafe bool TryGetClassVriable(string name, out MemberDescriptor? ivar)
        {
            if (this.cachedCVars.TryGetValue(name, out ivar))
                return true;
            var handle = class_getClassVariable(this.Handle, name);
            if (handle == IntPtr.Zero)
            {
                ivar = null;
                return false;
            }
            ivar = new MemberDescriptor(this, handle, name).Also(it => this.cachedCVars.TryAdd(name, it));
            return true;
        }
        

        /// <summary>
        /// Try finding instance variable of class.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="ivar">Descriptor of instance variable.</param>
        /// <returns>True if variable found.</returns>
        public unsafe bool TryGetInstanceVriable(string name, out MemberDescriptor? ivar)
        {
            if (this.cachedIVars.TryGetValue(name, out ivar))
                return true;
            if (this.areAllIVarsCached)
            {
                ivar = null;
                return false;
            }
            var handle = class_getInstanceVariable(this.Handle, name);
            if (handle == IntPtr.Zero)
            {
                ivar = null;
                return false;
            }
            ivar = new MemberDescriptor(this, handle, name).Also(it => this.cachedIVars.TryAdd(name, it));
            return true;
        }
        

        /// <summary>
        /// Try finding property of class.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="property">Descriptor of found property.</param>
        /// <returns>True if property found.</returns>
        public unsafe bool TryGetProperty(string name, out PropertyDescriptor? property)
        {
            if (this.cachedProperties.TryGetValue(name, out property))
                return true;
            if (this.areAllPropertiesCached)
            {
                property = null;
                return false;
            }
            var handle = class_getProperty(this.Handle, name);
            if (handle == IntPtr.Zero)
            {
                property = null;
                return false;
            }
            property = new PropertyDescriptor(this, handle, name).Also(it => this.cachedProperties.TryAdd(name, it));
            return true;
        }
        

        /// <summary>
        /// Wrap native instance as <see cref="Class"/>.
        /// </summary>
        /// <param name="handle">Handle of instance.</param>
        /// <returns><see cref="Class"/>.</returns>
        public static unsafe Class Wrap(IntPtr handle)
        {
            if (handle == IntPtr.Zero)
                throw new ArgumentException("Handle of instance cannot be null.");
            if (CachedClassesByHandle.TryGetValue(handle, out var nsClass))
                return nsClass;
            var namePtr = class_getName(handle);
            if (namePtr == null)
                throw new ArgumentException("Unable to get name of class.");
            var name = new string(namePtr);
            if (CachedClassesByName.TryGetValue(name, out nsClass))
                return nsClass;
            return new Class(handle, name).Also(it => 
            {
                CachedClassesByName.TryAdd(name, it);
                CachedClassesByHandle.TryAdd(handle, it);
            });
        }
    }
}