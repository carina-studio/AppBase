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
        static extern bool class_addIvar(IntPtr cls, string name, nuint size, byte alignment, string types);
        [DllImport(NativeLibraryNames.ObjectiveC)]
        static extern bool class_addMethod(IntPtr cls, IntPtr name, [MarshalAs(UnmanagedType.FunctionPtr)] Delegate imp, string types);
        [DllImport(NativeLibraryNames.ObjectiveC)]
        static extern bool class_addProperty(IntPtr cls, string name, [MarshalAs(UnmanagedType.LPArray)] PropertyAttribute[] attributes, uint attributeCount);
        [DllImport(NativeLibraryNames.ObjectiveC)]
        static extern IntPtr* class_copyIvarList(IntPtr cls, out int outCount);
        [DllImport(NativeLibraryNames.ObjectiveC)]
        static extern IntPtr* class_copyPropertyList(IntPtr cls, out uint outCount);
        [DllImport(NativeLibraryNames.ObjectiveC)]
        static extern bool class_conformsToProtocol(IntPtr cls, IntPtr protocol);
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
        static extern IntPtr objc_allocateClassPair(IntPtr superclass, string name, nuint extraBytes);
        [DllImport(NativeLibraryNames.ObjectiveC)]
        static extern void objc_disposeClassPair(IntPtr cls);
        [DllImport(NativeLibraryNames.ObjectiveC)]
        static extern IntPtr objc_getClass(string name);
        [DllImport(NativeLibraryNames.ObjectiveC)]
        static extern IntPtr objc_getProtocol(string name);
        [DllImport(NativeLibraryNames.ObjectiveC)]
        static extern void objc_registerClassPair(IntPtr cls);
        [DllImport(NativeLibraryNames.ObjectiveC)]
        static extern IntPtr object_getClass(IntPtr obj);
        [DllImport(NativeLibraryNames.ObjectiveC)]
        static extern sbyte* property_getName(IntPtr property);
        [DllImport(NativeLibraryNames.ObjectiveC)]
        static extern bool protocol_conformsToProtocol(IntPtr proto, IntPtr other);
        [DllImport(NativeLibraryNames.ObjectiveC)]
        static extern IntPtr* protocol_copyPropertyList(IntPtr proto, out uint outCount);
        [DllImport(NativeLibraryNames.ObjectiveC, EntryPoint = NSObject.SendMessageEntryPointName)]
        static extern IntPtr SendMessageIntPtr(IntPtr target, IntPtr selector);


        // Attribute of property.
        [StructLayout(LayoutKind.Sequential)]
        struct PropertyAttribute
        {
            [MarshalAs(UnmanagedType.LPStr)]
            public string? Name;
            [MarshalAs(UnmanagedType.LPStr)]
            public string? Value;
        }


        // Static fields.
        static readonly Selector? AllocSelector;
        static readonly IDictionary<IntPtr, Class> CachedClassesByHandle = new ConcurrentDictionary<IntPtr, Class>();
        static readonly IDictionary<string, Class> CachedClassesByName = new ConcurrentDictionary<string, Class>();
        static readonly IDictionary<IntPtr, Class> CachedProtocolsByHandle = new ConcurrentDictionary<IntPtr, Class>();
        static readonly IDictionary<string, Class> CachedProtocolsByName = new ConcurrentDictionary<string, Class>();


        // Fields.
        volatile bool areAllIVarsCached;
        volatile bool areAllPropertiesCached;
        readonly IDictionary<string, MemberDescriptor> cachedCVars = new ConcurrentDictionary<string, MemberDescriptor>();
        readonly IDictionary<string, MemberDescriptor> cachedIVars = new ConcurrentDictionary<string, MemberDescriptor>();
        readonly IDictionary<string, PropertyDescriptor> cachedProperties = new ConcurrentDictionary<string, PropertyDescriptor>();
        internal MemberDescriptor? clrObjectHandleVar;
        bool isRegistered;
        volatile bool isRootClass;
        readonly IList<Delegate> methodImplementations;
        volatile Class? superClass;


        // Static initializer.
        static Class()
        {
            if (Platform.IsNotMacOS)
                return;
            AllocSelector = Selector.FromName("alloc");
        }


        // Constructor.
        Class(IntPtr handle, string name, bool isProtocol, bool isCustomDefined)
        {
            this.Handle = handle;
            this.IsProtocol = isProtocol;
            this.methodImplementations = isCustomDefined ? new List<Delegate>() : new Delegate[0];
            this.Name = name;
        }


        /// <summary>
        /// Allocate memory for new instance with this class.
        /// </summary>
        /// <returns>Handle of allocated instance.</returns>
        public IntPtr Allocate() =>
            SendMessageIntPtr(this.Handle, AllocSelector!.Handle);
        

        /// <summary>
        /// Define a new class extends from <see cref="NSObject"/>.
        /// </summary>
        /// <param name="name">Name of class.</param>
        /// <param name="defining">Action to define members of class.</param>
        /// <returns>New defined class.</returns>
        public static Class DefineClass(string name, Action<Class> defining) =>
            DefineClass(GetClass("NSObject"), name, defining);
        

        /// <summary>
        /// Define a new class.
        /// </summary>
        /// <param name="superClass">Super class.</param>
        /// <param name="name">Name of class.</param>
        /// <param name="defining">Action to define members of class.</param>
        /// <returns>New defined class.</returns>
        public static Class DefineClass(Class? superClass, string name, Action<Class> defining)
        {
            // allocate class
            var handle = objc_allocateClassPair(superClass?.Handle ?? IntPtr.Zero, name, 0);
            if (handle == IntPtr.Zero)
                throw new ArgumentException($"Class '{name}' is already defined.");
            
            // define custom class
            Class cls;
            try
            {
                cls = new Class(handle, name, false, true);
                defining(cls);
            }
            catch
            {
                objc_disposeClassPair(handle);
                throw;
            }

            // define variable to link to CLR instance
            var clrObjHandleVarName = "_clrObjectHandle";
            if (!cls.cachedIVars.ContainsKey(clrObjHandleVarName))
                cls.clrObjectHandleVar = cls.DefineInstanceVariable(clrObjHandleVarName, IntPtr.Size);
            else
            {
                for (var i = 1; i <= 10; ++i)
                {
                    var candName = $"{clrObjHandleVarName}_{i}";
                    if (!cls.cachedIVars.ContainsKey(candName))
                    {
                        cls.clrObjectHandleVar = cls.DefineInstanceVariable(candName, IntPtr.Size);
                        break;
                    }
                }
                if (cls.clrObjectHandleVar == null)
                {
                    objc_disposeClassPair(handle);
                    throw new Exception($"Unable to define instance variable for handle of CLR object to class '{name}'.");
                }
            }

            // register class
            objc_registerClassPair(handle);
            cls.isRegistered = true;

            // complete
            CachedClassesByHandle.TryAdd(handle, cls);
            CachedClassesByName.TryAdd(name, cls);
            return cls;
        }


        /// <summary>
        /// Define instance variable.
        /// </summary>
        /// <param name="name">Name of variable.</param>
        /// <param name="size">Size of value in bytes.</param>
        /// <returns>Descriptor of instance variable.</returns>
        public MemberDescriptor DefineInstanceVariable(string name, int size)
        {
            this.VerifyRegistered();
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException($"Invalid name: {name}.");
            if (size <= 0)
                throw new ArgumentOutOfRangeException(nameof(size));
            byte alignment = size switch
            {
                <= 32 => 32,
                _ => (byte)IntPtr.Size,
            };
            if (!class_addIvar(this.Handle, name, (nuint)size, alignment, ""))
                throw new Exception($"Failed to add instance variable '{name}' to '{this.Name}'.");
            var handle = class_getInstanceVariable(this.Handle, name);
            if (handle == IntPtr.Zero)
                throw new Exception($"Failed to add instance variable '{name}' to '{this.Name}'.");
            return new MemberDescriptor(this, handle, name).Also(it =>
            {
                cachedIVars.TryAdd(name, it);
            });
        }


        /// <summary>
        /// Add method to this class.
        /// </summary>
        /// <param name="name">Selector of method.</param>
        /// <param name="implementation">Implementation of method.</param>
        public void DefineMethod(Selector name, Delegate implementation)
        {
            this.VerifyRegistered();
            this.VerifyMethodImplementation(implementation);
            if (!class_addMethod(this.Handle, name.Handle, implementation, ""))
                throw new Exception($"Failed to add method '{name}' to '{this.Name}'.");
            this.methodImplementations.Add(implementation); // hold reference to prevent destroying delegate by GC
        }


        /// <summary>
        /// Add new property to this class.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="getter">Implementation of getter.</param>
        /// <param name="setter">Implementation of setter.</param>
        /// <typeparam name="T">Type of property value.</typeparam>
        /// <returns>Descriptor of added property.</returns>
        public PropertyDescriptor DefineProperty<T>(string name, Delegate getter, Delegate? setter = null) where T : struct
        {
            // check state and parameters
            this.VerifyRegistered();
            this.VerifyPropertyGetter<T>(getter);
            if (setter != null)
                this.VerifyPropertySetter<T>(setter);
            if (this.cachedProperties.ContainsKey(name))
                throw new ArgumentException($"Property '{name}' is already defined in '{this.Name}'.");
            
            // prepare property attributes
            var attrs = new PropertyAttribute[3];
            var getterName = $"_auto_{name}_getter_";
            var setterName = $"_auto_{name}_setter_";
            attrs[0] = new() { Name = "N", Value = name };
            attrs[1] = new() { Name = "G", Value = getterName };
            if (setter != null)
                attrs[2] = new() { Name = "S", Value = setterName };
            else
                attrs[2] = new() { Name = "R", Value = "" };
            
            // add property
            if (!class_addProperty(this.Handle, name, attrs, (uint)attrs.Length)
                || !this.TryGetProperty(name, out var property))
            {
                throw new ArgumentException($"Unagle to add property '{name}' to '{this.Name}'.");
            }

            // register getter and setter methods
            try
            {
                this.DefineMethod(Selector.FromName(getterName), getter);
                if (setter != null)
                    this.DefineMethod(Selector.FromName(setterName), setter);
            }
            catch
            {
                this.cachedProperties.Remove(name);
                throw;
            }

            // complete
            return property.AsNonNull();
        }
        

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
                return new Class(handle, name, false, false).Also(it => 
                {
                    it.isRegistered = true;
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
            if (this.areAllIVarsCached || this.IsProtocol)
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
                var count = 0u;
                var propertiesPtr = cls.IsProtocol
                    ? protocol_copyPropertyList(cls.Handle, out count)
                    : class_copyPropertyList(cls.Handle, out count);
                try
                {
                    for (var i = 0; i < count; --i)
                        propertyNames.TryAdd(new string(property_getName(propertiesPtr[i])), propertiesPtr[i]);
                }
                finally
                {
                    NativeMemory.Free(propertiesPtr);
                    cls = cls.IsProtocol ? null : cls?.SuperClass;
                }
            }
            foreach (var (name, handle) in propertyNames)
                this.cachedProperties.TryAdd(name, new PropertyDescriptor(this, handle, name));
            this.areAllPropertiesCached = true;
            return this.cachedProperties.Values.ToArray();
        }


        /// <summary>
        /// Find the protocol with given name.
        /// </summary>
        /// <param name="name">Name of protocol.</param>
        /// <returns>Protocol with given name, or Null if protocol cannot be found.</returns>
        public static Class? GetProtocol(string name)
        {
            if (CachedProtocolsByName.TryGetValue(name, out var nsClass))
                return nsClass;
            if (Platform.IsNotMacOS)
                return null;
            var handle = objc_getProtocol(name);
            if (handle != IntPtr.Zero)
            {
                return new Class(handle, name, true, false).Also(it => 
                {
                    it.isRegistered = true;
                    CachedProtocolsByName.TryAdd(name, it);
                    CachedProtocolsByHandle.TryAdd(handle, it);
                });
            }
            return null;
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
            if (this.IsProtocol)
            {
                if (cls.IsProtocol)
                    return protocol_conformsToProtocol(cls.Handle, this.Handle);
                return class_conformsToProtocol(cls.Handle, this.Handle);
            }
            else
            {
                var superCls = cls.SuperClass;
                while (superCls != null)
                {
                    if (this.Equals(superCls))
                        return true;
                    superCls = superCls.SuperClass;
                }
            }
            return false;
        }


        /// <summary>
        /// Check whether the class represents a protocol or not.
        /// </summary>
        public bool IsProtocol { get; }


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
                    ?? (this.IsProtocol
                        ? GetClass("NSObject").Also(it =>
                        {
                            this.superClass = it;
                        })
                        : (this.isRootClass 
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
                            })));
            }
        }


        /// <inheritdoc/>
        public override string ToString() =>
            this.Name;
        

        /// <summary>
        /// Try getting CLR object bound to Object-C object by calling <see cref="TrySetClrObject"/>.
        /// </summary>
        /// <param name="obj">Handle of instance.</param>
        /// <param name="clrObj">Bound CLR object.</param>
        /// <returns>True if CLR object found.</returns>
        public bool TryGetClrObject(IntPtr obj, out object? clrObj)
        {
            if (this.clrObjectHandleVar == null
                || !this.IsAssignableFrom(GetClass(obj)))
            {
                clrObj = null;
                return false;
            }
            var gcHandle = NSObject.GetGCHandleVariable(obj, this.clrObjectHandleVar);
            clrObj = gcHandle != default ? gcHandle.Target : null;
            return (clrObj != null);
        }
        

        /// <summary>
        /// Try finding class variable of class.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="ivar">Descriptor of class variable.</param>
        /// <returns>True if variable found.</returns>
        public unsafe bool TryGetClassVriable(string name, out MemberDescriptor? ivar)
        {
            if (this.IsProtocol)
            {
                ivar = null;
                return false;
            }
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
            if (this.IsProtocol)
            {
                ivar = null;
                return false;
            }
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
            if (this.IsProtocol)
                this.GetProperties();
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
        /// Try binding CLR object to given Objective-C object.
        /// </summary>
        /// <param name="obj">Handle of instance.</param>
        /// <param name="clrObj">CLR object to bind.</param>
        /// <returns>True if CLR object has been bound successfully.</returns>
        public bool TrySetClrObject(IntPtr obj, object? clrObj)
        {
            if (this.clrObjectHandleVar == null
                || !this.IsAssignableFrom(GetClass(obj)))
            {
                return false;
            }
            NSObject.GetGCHandleVariable(obj, this.clrObjectHandleVar).Let(it =>
            {
                if (it != default)
                    it.Free();
            });
            NSObject.SetVariable(obj, this.clrObjectHandleVar, clrObj != null ? GCHandle.Alloc(clrObj, GCHandleType.Weak) : default);
            return true;
        }


        // Throw exception if implementation is invalid.
        void VerifyMethodImplementation(Delegate implementation)
        {
            var invokeMethod = implementation.GetType().GetMethod("Invoke");
            if (invokeMethod == null)
                throw new ArgumentException("Invalid implementation.");
            var parameters = invokeMethod.GetParameters();
            if (parameters.Length < 2
                || parameters[0].ParameterType != typeof(IntPtr)
                || parameters[1].ParameterType != typeof(IntPtr))
            {
                throw new ArgumentException("The first 2 parameters must be System.IntPtr.");
            }
        }


        // Throw exception if implementation is invalid.
        void VerifyPropertyGetter<T>(Delegate implementation) where T : struct
        {
            this.VerifyMethodImplementation(implementation);
            var invokeMethod = implementation.GetType().GetMethod("Invoke").AsNonNull();
            var parameters = invokeMethod.GetParameters();
            if (parameters.Length != 2)
                throw new ArgumentException("Only first 2 parameters are allowed for property getter.");
            if (invokeMethod.ReturnType != typeof(T))
                throw new ArgumentException($"The return type must be '{typeof(T).Name}' for property getter.");
        }


        // Throw exception if implementation is invalid.
        void VerifyPropertySetter<T>(Delegate implementation) where T : struct
        {
            this.VerifyMethodImplementation(implementation);
            var invokeMethod = implementation.GetType().GetMethod("Invoke").AsNonNull();
            var parameters = invokeMethod.GetParameters();
            if (parameters.Length != 3)
                throw new ArgumentException("Only first 3 parameters are allowed for property setter.");
            if (parameters[2].ParameterType != typeof(T))
                throw new ArgumentException($"The 3rd parameter must be '{typeof(T).Name}' for property setter.");
        }


        // Throw exception if class has been regisgered.
        void VerifyRegistered()
        {
            if (this.isRegistered)
                throw new InvalidOperationException($"Cannot define member of class '{this.Name}' after registering to Objective-C runtime.");
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
            return new Class(handle, name, false, false).Also(it => 
            {
                it.isRegistered = true;
                CachedClassesByName.TryAdd(name, it);
                CachedClassesByHandle.TryAdd(handle, it);
            });
        }
    }


    /// <summary>
    /// Delegate for method implementation of <see cref="Class"/>.
    /// </summary>
    public delegate void MethodImpl(IntPtr self, IntPtr cmd);


    /// <summary>
    /// Delegate for method implementation of <see cref="Class"/>.
    /// </summary>
    public delegate void MethodImpl_Boolean(IntPtr self, IntPtr cmd, bool value);


    /// <summary>
    /// Delegate for method implementation of <see cref="Class"/>.
    /// </summary>
    public delegate void MethodImpl_Int32(IntPtr self, IntPtr cmd, int arg1);


    /// <summary>
    /// Delegate for method implementation of <see cref="Class"/>.
    /// </summary>
    public delegate void MethodImpl_Int64(IntPtr self, IntPtr cmd, long arg1);


    /// <summary>
    /// Delegate for method implementation of <see cref="Class"/>.
    /// </summary>
    public delegate void MethodImpl_IntPtr(IntPtr self, IntPtr cmd, IntPtr arg1);


    /// <summary>
    /// Delegate for method implementation of <see cref="Class"/>.
    /// </summary>
    public delegate bool MethodImplForBoolean(IntPtr self, IntPtr cmd);


    /// <summary>
    /// Delegate for method implementation of <see cref="Class"/>.
    /// </summary>
    public delegate int MethodImplForInt32(IntPtr self, IntPtr cmd);


    /// <summary>
    /// Delegate for method implementation of <see cref="Class"/>.
    /// </summary>
    public delegate long MethodImplForInt64(IntPtr self, IntPtr cmd);


    /// <summary>
    /// Delegate for method implementation of <see cref="Class"/>.
    /// </summary>
    public delegate IntPtr MethodImplForIntPtr(IntPtr self, IntPtr cmd);


    /// <summary>
    /// Delegate for method implementation of <see cref="Class"/>.
    /// </summary>
    public delegate bool MethodImplForBoolean_Boolean(IntPtr self, IntPtr cmd, bool arg1);


    /// <summary>
    /// Delegate for method implementation of <see cref="Class"/>.
    /// </summary>
    public delegate int MethodImplForInt32_Int32(IntPtr self, IntPtr cmd, int arg1);


    /// <summary>
    /// Delegate for method implementation of <see cref="Class"/>.
    /// </summary>
    public delegate long MethodImplForInt64_Int64(IntPtr self, IntPtr cmd, long arg1);


    /// <summary>
    /// Delegate for method implementation of <see cref="Class"/>.
    /// </summary>
    public delegate IntPtr MethodImplForIntPtr_IntPtr(IntPtr self, IntPtr cmd, IntPtr arg1);


    /// <summary>
    /// Delegate for getter of <see cref="Boolean"/> property of <see cref="Class"/>.
    /// </summary>
    public delegate bool BooleanPropertyGetterImpl(IntPtr self, IntPtr cmd);


    /// <summary>
    /// Delegate for setter of <see cref="Boolean"/> property of <see cref="Class"/>.
    /// </summary>
    public delegate void BooleanPropertySetterImpl(IntPtr self, IntPtr cmd, bool value);


    /// <summary>
    /// Delegate for getter of <see cref="Int32"/> property of <see cref="Class"/>.
    /// </summary>
    public delegate int Int32PropertyGetterImpl(IntPtr self, IntPtr cmd);


    /// <summary>
    /// Delegate for setter of <see cref="Int32"/> property of <see cref="Class"/>.
    /// </summary>
    public delegate void Int32PropertySetterImpl(IntPtr self, IntPtr cmd, int value);


    /// <summary>
    /// Delegate for getter of <see cref="Int64"/> property of <see cref="Class"/>.
    /// </summary>
    public delegate long Int64PropertyGetterImpl(IntPtr self, IntPtr cmd);


    /// <summary>
    /// Delegate for setter of <see cref="Boolean"/> property of <see cref="Class"/>.
    /// </summary>
    public delegate void Int64PropertySetterImpl(IntPtr self, IntPtr cmd, long value);


    /// <summary>
    /// Delegate for getter of <see cref="IntPtr"/> property of <see cref="Class"/>.
    /// </summary>
    public delegate IntPtr IntPtrPropertyGetterImpl(IntPtr self, IntPtr cmd);


    /// <summary>
    /// Delegate for setter of <see cref="IntPtr"/> property of <see cref="Class"/>.
    /// </summary>
    public delegate void IntPtrPropertySetterImpl(IntPtr self, IntPtr cmd, IntPtr value);
}