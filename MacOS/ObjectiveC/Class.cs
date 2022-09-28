using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
        static extern bool class_addProtocol(IntPtr cls, IntPtr protocol);
        [DllImport(NativeLibraryNames.ObjectiveC)]
        static extern IntPtr* class_copyIvarList(IntPtr cls, out uint outCount);
        [DllImport(NativeLibraryNames.ObjectiveC)]
        static extern IntPtr* class_copyMethodList(IntPtr cls, out uint outCount);
        [DllImport(NativeLibraryNames.ObjectiveC)]
        static extern IntPtr* class_copyPropertyList(IntPtr cls, out uint outCount);
        [DllImport(NativeLibraryNames.ObjectiveC)]
        static extern bool class_conformsToProtocol(IntPtr cls, IntPtr protocol);
        [DllImport(NativeLibraryNames.ObjectiveC)]
        static extern IntPtr class_getClassVariable(IntPtr cls, string name);
        [DllImport(NativeLibraryNames.ObjectiveC)]
        static extern IntPtr class_getInstanceMethod(IntPtr cls, IntPtr name);
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
        static extern IntPtr method_getName(IntPtr m);
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
        static extern void protocol_addProtocol(IntPtr proto, IntPtr addition);
        [DllImport(NativeLibraryNames.ObjectiveC)]
        static extern bool protocol_conformsToProtocol(IntPtr proto, IntPtr other);
        [DllImport(NativeLibraryNames.ObjectiveC)]
        static extern IntPtr* protocol_copyPropertyList(IntPtr proto, out uint outCount);
        [DllImport(NativeLibraryNames.ObjectiveC)]
        static extern bool protocol_isEqual(IntPtr proto, IntPtr other);
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
        volatile bool areAllMethodsCached;
        volatile bool areAllPropertiesCached;
        readonly IDictionary<string, Member> cachedCVars = new ConcurrentDictionary<string, Member>();
        readonly IDictionary<string, Member> cachedIVars = new ConcurrentDictionary<string, Member>();
        readonly IDictionary<Selector, Method> cachedMethods = new ConcurrentDictionary<Selector, Method>();
        readonly IDictionary<string, Property> cachedProperties = new ConcurrentDictionary<string, Property>();
        internal Member? clrObjectHandleVar;
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
        /// Add a protocol to this class.
        /// </summary>
        /// <param name="protocol">Protocol.</param>
        public void AddProtocol(Class protocol)
        {
            if (!protocol.IsProtocol)
                throw new ArgumentException($"'{protocol.Name}' is not a protocol.");
            if (this.Equals(protocol))
                throw new ArgumentException("Cannot add protocol to itself.");
            if (this.IsProtocol)
                protocol_addProtocol(this.Handle, protocol.Handle);
            else
            {
                if (class_addProtocol(this.Handle, protocol.Handle))
                    return;
                if (!class_conformsToProtocol(this.Handle, protocol.Handle))
                    throw new InvalidOperationException($"Unable to add protocol '{protocol.Name}' to '{this.Name}'.");
            }
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
        public Member DefineInstanceVariable(string name, int size)
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
            return new Member(this, handle, name).Also(it =>
            {
                cachedIVars.TryAdd(name, it);
            });
        }


        /// <summary>
        /// Define method without argument.
        /// </summary>
        /// <param name="name">Selector.</param>
        /// <param name="implementation">Implementation.</param>
        public void DefineMethod(Selector name, Action<IntPtr, IntPtr> implementation) =>
            this.DefineMethod(name, implementation, null);


        /// <summary>
        /// Define method without argument.
        /// </summary>
        /// <param name="name">Selector.</param>
        /// <param name="implementation">Implementation.</param>
        public void DefineMethod<R>(Selector name, Func<IntPtr, IntPtr, R> implementation) =>
            this.DefineMethod(name, implementation, typeof(R));


        /// <summary>
        /// Define method with 1 argument.
        /// </summary>
        /// <param name="name">Selector.</param>
        /// <param name="implementation">Implementation.</param>
        public void DefineMethod<TArg1>(Selector name, Action<IntPtr, IntPtr, TArg1> implementation)
        {
            switch (NativeTypeConversion.GetNativeValueCount<TArg1>())
            {
                case 1:
                    this.DefineMethodWith1NArg(name, implementation, null);
                    break;
                case 2:
                    this.DefineMethodWith2NArgs(name, implementation, null);
                    break;
                case 3:
                    this.DefineMethodWith3NArgs(name, implementation, null);
                    break;
                case 4:
                    this.DefineMethodWith4NArgs(name, implementation, null);
                    break;
                case 5:
                    this.DefineMethodWith5NArgs(name, implementation, null);
                    break;
                default:
                    throw new NotSupportedException("Too many native arguments.");
            }
        }


        /// <summary>
        /// Define method with 1 argument.
        /// </summary>
        /// <param name="name">Selector.</param>
        /// <param name="implementation">Implementation.</param>
        public void DefineMethod<TArg1, R>(Selector name, Func<IntPtr, IntPtr, TArg1, R> implementation)
        {
            switch (NativeTypeConversion.GetNativeValueCount<TArg1>())
            {
                case 1:
                    this.DefineMethodWith1NArg(name, implementation, typeof(R));
                    break;
                case 2:
                    this.DefineMethodWith2NArgs(name, implementation, typeof(R));
                    break;
                case 3:
                    this.DefineMethodWith3NArgs(name, implementation, typeof(R));
                    break;
                case 4:
                    this.DefineMethodWith4NArgs(name, implementation, typeof(R));
                    break;
                case 5:
                    this.DefineMethodWith5NArgs(name, implementation, typeof(R));
                    break;
                default:
                    throw new NotSupportedException("Too many native arguments.");
            }
        }


        /// <summary>
        /// Define method with 2 arguments.
        /// </summary>
        /// <param name="name">Selector.</param>
        /// <param name="implementation">Implementation.</param>
        public void DefineMethod<TArg1, TArg2>(Selector name, Action<IntPtr, IntPtr, TArg1, TArg2> implementation)
        {
            var nArgCount = NativeTypeConversion.GetNativeValueCount<TArg1>()
                + NativeTypeConversion.GetNativeValueCount<TArg2>();
            switch (nArgCount)
            {
                case 2: 
                    this.DefineMethodWith2NArgs(name, implementation, null);
                    break;
                case 3: 
                    this.DefineMethodWith3NArgs(name, implementation, null);
                    break;
                case 4: 
                    this.DefineMethodWith4NArgs(name, implementation, null);
                    break;
                case 5: 
                    this.DefineMethodWith5NArgs(name, implementation, null);
                    break;
                default:
                    throw new NotSupportedException("Too many native arguments.");
            }
        }


        /// <summary>
        /// Define method with 2 arguments.
        /// </summary>
        /// <param name="name">Selector.</param>
        /// <param name="implementation">Implementation.</param>
        public void DefineMethod<TArg1, TArg2, R>(Selector name, Func<IntPtr, IntPtr, TArg1, TArg2, R> implementation)
        {
            var nArgCount = NativeTypeConversion.GetNativeValueCount<TArg1>()
                + NativeTypeConversion.GetNativeValueCount<TArg2>();
            switch (nArgCount)
            {
                case 2: 
                    this.DefineMethodWith2NArgs(name, implementation, typeof(R));
                    break;
                case 3: 
                    this.DefineMethodWith3NArgs(name, implementation, typeof(R));
                    break;
                case 4: 
                    this.DefineMethodWith4NArgs(name, implementation, typeof(R));
                    break;
                case 5: 
                    this.DefineMethodWith5NArgs(name, implementation, typeof(R));
                    break;
                default:
                    throw new NotSupportedException("Too many native arguments.");
            }
        }


        /// <summary>
        /// Define method with 3 arguments.
        /// </summary>
        /// <param name="name">Selector.</param>
        /// <param name="implementation">Implementation.</param>
        public void DefineMethod<TArg1, TArg2, TArg3>(Selector name, Action<IntPtr, IntPtr, TArg1, TArg2, TArg3> implementation)
        {
            var nArgCount = NativeTypeConversion.GetNativeValueCount<TArg1>()
                + NativeTypeConversion.GetNativeValueCount<TArg2>()
                + NativeTypeConversion.GetNativeValueCount<TArg3>();
            switch (nArgCount)
            {
                case 3: 
                    this.DefineMethodWith3NArgs(name, implementation, null);
                    break;
                case 4: 
                    this.DefineMethodWith4NArgs(name, implementation, null);
                    break;
                case 5: 
                    this.DefineMethodWith5NArgs(name, implementation, null);
                    break;
                default:
                    throw new NotSupportedException("Too many native arguments.");
            }
        }


        /// <summary>
        /// Define method with 3 arguments.
        /// </summary>
        /// <param name="name">Selector.</param>
        /// <param name="implementation">Implementation.</param>
        public void DefineMethod<TArg1, TArg2, TArg3, R>(Selector name, Func<IntPtr, IntPtr, TArg1, TArg2, TArg3, R> implementation)
        {
            var nArgCount = NativeTypeConversion.GetNativeValueCount<TArg1>()
                + NativeTypeConversion.GetNativeValueCount<TArg2>()
                + NativeTypeConversion.GetNativeValueCount<TArg3>();
            switch (nArgCount)
            {
                case 3: 
                    this.DefineMethodWith3NArgs(name, implementation, typeof(R));
                    break;
                case 4: 
                    this.DefineMethodWith4NArgs(name, implementation, typeof(R));
                    break;
                case 5: 
                    this.DefineMethodWith5NArgs(name, implementation, typeof(R));
                    break;
                default:
                    throw new NotSupportedException("Too many native arguments.");
            }
        }


        /// <summary>
        /// Define method with 4 arguments.
        /// </summary>
        /// <param name="name">Selector.</param>
        /// <param name="implementation">Implementation.</param>
        public void DefineMethod<TArg1, TArg2, TArg3, TArg4>(Selector name, Action<IntPtr, IntPtr, TArg1, TArg2, TArg3, TArg4> implementation)
        {
            var nArgCount = NativeTypeConversion.GetNativeValueCount<TArg1>()
                + NativeTypeConversion.GetNativeValueCount<TArg2>()
                + NativeTypeConversion.GetNativeValueCount<TArg3>()
                + NativeTypeConversion.GetNativeValueCount<TArg4>();
            switch (nArgCount)
            {
                case 4: 
                    this.DefineMethodWith4NArgs(name, implementation, null);
                    break;
                case 5: 
                    this.DefineMethodWith5NArgs(name, implementation, null);
                    break;
                default:
                    throw new NotSupportedException("Too many native arguments.");
            }
        }


        /// <summary>
        /// Define method with 4 arguments.
        /// </summary>
        /// <param name="name">Selector.</param>
        /// <param name="implementation">Implementation.</param>
        public void DefineMethod<TArg1, TArg2, TArg3, TArg4, R>(Selector name, Func<IntPtr, IntPtr, TArg1, TArg2, TArg3, TArg4, R> implementation)
        {
            var nArgCount = NativeTypeConversion.GetNativeValueCount<TArg1>()
                + NativeTypeConversion.GetNativeValueCount<TArg2>()
                + NativeTypeConversion.GetNativeValueCount<TArg3>()
                + NativeTypeConversion.GetNativeValueCount<TArg4>();
            switch (nArgCount)
            {
                case 4: 
                    this.DefineMethodWith4NArgs(name, implementation, typeof(R));
                    break;
                case 5: 
                    this.DefineMethodWith5NArgs(name, implementation, typeof(R));
                    break;
                default:
                    throw new NotSupportedException("Too many native arguments.");
            }
        }


        /// <summary>
        /// Define method with 5 arguments.
        /// </summary>
        /// <param name="name">Selector.</param>
        /// <param name="implementation">Implementation.</param>
        public void DefineMethod<TArg1, TArg2, TArg3, TArg4, TArg5>(Selector name, Action<IntPtr, IntPtr, TArg1, TArg2, TArg3, TArg4, TArg5> implementation)
        {
            var nArgCount = NativeTypeConversion.GetNativeValueCount<TArg1>()
                + NativeTypeConversion.GetNativeValueCount<TArg2>()
                + NativeTypeConversion.GetNativeValueCount<TArg3>()
                + NativeTypeConversion.GetNativeValueCount<TArg4>()
                + NativeTypeConversion.GetNativeValueCount<TArg5>();
            switch (nArgCount)
            {
                case 5: 
                    this.DefineMethodWith5NArgs(name, implementation, null);
                    break;
                default:
                    throw new NotSupportedException("Too many native arguments.");
            }
        }


        /// <summary>
        /// Define method with 5 arguments.
        /// </summary>
        /// <param name="name">Selector.</param>
        /// <param name="implementation">Implementation.</param>
        public void DefineMethod<TArg1, TArg2, TArg3, TArg4, TArg5, R>(Selector name, Func<IntPtr, IntPtr, TArg1, TArg2, TArg3, TArg4, TArg5, R> implementation)
        {
            var nArgCount = NativeTypeConversion.GetNativeValueCount<TArg1>()
                + NativeTypeConversion.GetNativeValueCount<TArg2>()
                + NativeTypeConversion.GetNativeValueCount<TArg3>()
                + NativeTypeConversion.GetNativeValueCount<TArg4>()
                + NativeTypeConversion.GetNativeValueCount<TArg5>();
            switch (nArgCount)
            {
                case 5: 
                    this.DefineMethodWith5NArgs(name, implementation, typeof(R));
                    break;
                default:
                    throw new NotSupportedException("Too many native arguments.");
            }
        }


        // Define method without native argument.
        void DefineMethod(Selector name, Delegate implementation, Type? returnType)
        {
            var isFpResult = returnType != null && NativeTypeConversion.IsFloatingPointStructure(returnType);
            var nrSize = returnType != null ? NativeTypeConversion.GetNativeValueCount(returnType) : 0;
            var wrapper = nrSize switch
            {
                0 => isFpResult
                    ? (Delegate)new MethodImpl((self, cmd) =>
                        this.InvokeMethodImplementation(implementation, self, cmd))
                    : new MethodImpl((self, cmd) =>
                        this.InvokeMethodImplementation(implementation, self, cmd)),
                1 => isFpResult
                    ? (Delegate)new MethodImplFpRet1((self, cmd) => 
                    {
                        var nr = (nint)0;
                        NativeTypeConversion.ToNativeValue(implementation.DynamicInvoke(self, cmd), &nr);
                        return *(double*)&nr;
                    }) 
                    : new MethodImplRet1((self, cmd) => 
                    {
                        var nr = (nint)0;
                        NativeTypeConversion.ToNativeValue(implementation.DynamicInvoke(self, cmd), &nr);
                        return nr;
                    }),
                2 => isFpResult
                    ? (Delegate)new MethodImplFpRet2((self, cmd) => 
                    {
                        var nr = stackalloc nint[2];
                        NativeTypeConversion.ToNativeValue(implementation.DynamicInvoke(self, cmd), nr);
                        return new NativeFpResult2((double*)nr);
                    }) 
                    : new MethodImplRet2((self, cmd) => 
                    {
                        var nr = stackalloc nint[2];
                        NativeTypeConversion.ToNativeValue(implementation.DynamicInvoke(self, cmd), nr);
                        return new NativeResult2(nr);
                    }),
                _ => throw new NotSupportedException($"Unsupported return type '{returnType?.Name}'."),
            };
            if (!class_addMethod(this.Handle, name.Handle, wrapper, ""))
                throw new Exception($"Failed to add method '{name}' to '{this.Name}'.");
            this.methodImplementations.Add(wrapper); // hold reference to prevent destroying delegate by GC
        }


        // Define method with 1 native argument1.
        void DefineMethodWith1NArg(Selector name, Delegate implementation, Type? returnType)
        {
            var isFpResult = returnType != null && NativeTypeConversion.IsFloatingPointStructure(returnType);
            var nrSize = returnType != null ? NativeTypeConversion.GetNativeValueCount(returnType) : 0;
            var wrapper = nrSize switch
            {
                0 => isFpResult
                    ? (Delegate)new MethodImplArg1((self, cmd, arg1) => 
                        this.InvokeMethodImplementation(implementation, self, cmd, arg1))
                    : new MethodImplArg1((self, cmd, arg1) =>
                        this.InvokeMethodImplementation(implementation, self, cmd, arg1)),
                1 => isFpResult
                    ? (Delegate)new MethodImplFpRet1Arg1((self, cmd, arg1) => 
                    {
                        var nr = (nint)0;
                        NativeTypeConversion.ToNativeValue(this.InvokeMethodImplementation(implementation, self, cmd, arg1), &nr);
                        return *(double*)&nr;
                    }) 
                    : new MethodImplRet1Arg1((self, cmd, arg1) => 
                    {
                        var nr = (nint)0;
                        NativeTypeConversion.ToNativeValue(this.InvokeMethodImplementation(implementation, self, cmd, arg1), &nr);
                        return nr;
                    }),
                2 => isFpResult
                    ? (Delegate)new MethodImplFpRet2Arg1((self, cmd, arg1) => 
                    {
                        var nr = stackalloc nint[2];
                        NativeTypeConversion.ToNativeValue(this.InvokeMethodImplementation(implementation, self, cmd, arg1), nr);
                        return new NativeFpResult2((double*)nr);
                    }) 
                    : new MethodImplRet2Arg1((self, cmd, arg1) => 
                    {
                        var nr = stackalloc nint[2];
                        NativeTypeConversion.ToNativeValue(this.InvokeMethodImplementation(implementation, self, cmd, arg1), nr);
                        return new NativeResult2(nr);
                    }),
                _ => throw new NotSupportedException($"Unsupported return type '{returnType?.Name}'."),
            };
            if (!class_addMethod(this.Handle, name.Handle, wrapper, ""))
                throw new Exception($"Failed to add method '{name}' to '{this.Name}'.");
            this.methodImplementations.Add(wrapper); // hold reference to prevent destroying delegate by GC
        }


        // Define method with 2 native arguments.
        void DefineMethodWith2NArgs(Selector name, Delegate implementation, Type? returnType)
        {
            var isFpResult = returnType != null && NativeTypeConversion.IsFloatingPointStructure(returnType);
            var nrSize = returnType != null ? NativeTypeConversion.GetNativeValueCount(returnType) : 0;
            var wrapper = nrSize switch
            {
                0 => isFpResult
                    ? (Delegate)new MethodImplArg2((self, cmd, arg1, arg2) => 
                        this.InvokeMethodImplementation(implementation, self, cmd, arg1, arg2))
                    : new MethodImplArg2((self, cmd, arg1, arg2) => 
                        this.InvokeMethodImplementation(implementation, self, cmd, arg1, arg2)),
                1 => isFpResult
                    ? (Delegate)new MethodImplFpRet1Arg2((self, cmd, arg1, arg2) => 
                    {
                        var nr = (nint)0;
                        NativeTypeConversion.ToNativeValue(this.InvokeMethodImplementation(implementation, self, cmd, arg1, arg2), &nr);
                        return *(double*)&nr;
                    }) 
                    : new MethodImplRet1Arg2((self, cmd, arg1, arg2) => 
                    {
                        var nr = (nint)0;
                        NativeTypeConversion.ToNativeValue(this.InvokeMethodImplementation(implementation, self, cmd, arg1, arg2), &nr);
                        return nr;
                    }),
                2 => isFpResult
                    ? (Delegate)new MethodImplFpRet2Arg2((self, cmd, arg1, arg2) => 
                    {
                        var nr = stackalloc nint[2];
                        NativeTypeConversion.ToNativeValue(this.InvokeMethodImplementation(implementation, self, cmd, arg1, arg2), nr);
                        return new NativeFpResult2((double*)nr);
                    }) 
                    : new MethodImplRet2Arg2((self, cmd, arg1, arg2) => 
                    {
                        var nr = stackalloc nint[2];
                        NativeTypeConversion.ToNativeValue(this.InvokeMethodImplementation(implementation, self, cmd, arg1, arg2), nr);
                        return new NativeResult2(nr);
                    }),
                _ => throw new NotSupportedException($"Unsupported return type '{returnType?.Name}'."),
            };
            if (!class_addMethod(this.Handle, name.Handle, wrapper, ""))
                throw new Exception($"Failed to add method '{name}' to '{this.Name}'.");
            this.methodImplementations.Add(wrapper); // hold reference to prevent destroying delegate by GC
        }


        // Define method with 3 native arguments.
        void DefineMethodWith3NArgs(Selector name, Delegate implementation, Type? returnType)
        {
            var isFpResult = returnType != null && NativeTypeConversion.IsFloatingPointStructure(returnType);
            var nrSize = returnType != null ? NativeTypeConversion.GetNativeValueCount(returnType) : 0;
            var wrapper = nrSize switch
            {
                0 => isFpResult
                    ? (Delegate)new MethodImplArg3((self, cmd, arg1, arg2, arg3) => 
                        this.InvokeMethodImplementation(implementation, self, cmd, arg1, arg2, arg3))
                    : new MethodImplArg3((self, cmd, arg1, arg2, arg3) => 
                        this.InvokeMethodImplementation(implementation, self, cmd, arg1, arg2, arg3)),
                1 => isFpResult
                    ? (Delegate)new MethodImplFpRet1Arg3((self, cmd, arg1, arg2, arg3) => 
                    {
                        var nr = (nint)0;
                        NativeTypeConversion.ToNativeValue(this.InvokeMethodImplementation(implementation, self, cmd, arg1, arg2, arg3), &nr);
                        return *(double*)&nr;
                    }) 
                    : new MethodImplRet1Arg3((self, cmd, arg1, arg2, arg3) => 
                    {
                        var nr = (nint)0;
                        NativeTypeConversion.ToNativeValue(this.InvokeMethodImplementation(implementation, self, cmd, arg1, arg2, arg3), &nr);
                        return nr;
                    }),
                2 => isFpResult
                    ? (Delegate)new MethodImplFpRet2Arg3((self, cmd, arg1, arg2, arg3) => 
                    {
                        var nr = stackalloc nint[2];
                        NativeTypeConversion.ToNativeValue(this.InvokeMethodImplementation(implementation, self, cmd, arg1, arg2, arg3), nr);
                        return new NativeFpResult2((double*)nr);
                    }) 
                    : new MethodImplRet2Arg3((self, cmd, arg1, arg2, arg3) => 
                    {
                        var nr = stackalloc nint[2];
                        NativeTypeConversion.ToNativeValue(this.InvokeMethodImplementation(implementation, self, cmd, arg1, arg2, arg3), nr);
                        return new NativeResult2(nr);
                    }),
                _ => throw new NotSupportedException($"Unsupported return type '{returnType?.Name}'."),
            };
            if (!class_addMethod(this.Handle, name.Handle, wrapper, ""))
                throw new Exception($"Failed to add method '{name}' to '{this.Name}'.");
            this.methodImplementations.Add(wrapper); // hold reference to prevent destroying delegate by GC
        }


        // Define method with 4 native arguments.
        void DefineMethodWith4NArgs(Selector name, Delegate implementation, Type? returnType)
        {
            var isFpResult = returnType != null && NativeTypeConversion.IsFloatingPointStructure(returnType);
            var nrSize = returnType != null ? NativeTypeConversion.GetNativeValueCount(returnType) : 0;
            var wrapper = nrSize switch
            {
                0 => isFpResult
                    ? (Delegate)new MethodImplArg4((self, cmd, arg1, arg2, arg3, arg4) => 
                        this.InvokeMethodImplementation(implementation, self, cmd, arg1, arg2, arg3, arg4))
                    : new MethodImplArg4((self, cmd, arg1, arg2, arg3, arg4) => 
                        this.InvokeMethodImplementation(implementation, self, cmd, arg1, arg2, arg3, arg4)),
                1 => isFpResult
                    ? (Delegate)new MethodImplFpRet1Arg4((self, cmd, arg1, arg2, arg3, arg4) => 
                    {
                        var nr = (nint)0;
                        NativeTypeConversion.ToNativeValue(this.InvokeMethodImplementation(implementation, self, cmd, arg1, arg2, arg3, arg4), &nr);
                        return *(double*)&nr;
                    }) 
                    : new MethodImplRet1Arg4((self, cmd, arg1, arg2, arg3, arg4) => 
                    {
                        var nr = (nint)0;
                        NativeTypeConversion.ToNativeValue(this.InvokeMethodImplementation(implementation, self, cmd, arg1, arg2, arg3, arg4), &nr);
                        return nr;
                    }),
                2 => isFpResult
                    ? (Delegate)new MethodImplFpRet2Arg4((self, cmd, arg1, arg2, arg3, arg4) => 
                    {
                        var nr = stackalloc nint[2];
                        NativeTypeConversion.ToNativeValue(this.InvokeMethodImplementation(implementation, self, cmd, arg1, arg2, arg3, arg4), nr);
                        return new NativeFpResult2((double*)nr);
                    }) 
                    : new MethodImplRet2Arg4((self, cmd, arg1, arg2, arg3, arg4) => 
                    {
                        var nr = stackalloc nint[2];
                        NativeTypeConversion.ToNativeValue(this.InvokeMethodImplementation(implementation, self, cmd, arg1, arg2, arg3, arg4), nr);
                        return new NativeResult2(nr);
                    }),
                _ => throw new NotSupportedException($"Unsupported return type '{returnType?.Name}'."),
            };
            if (!class_addMethod(this.Handle, name.Handle, wrapper, ""))
                throw new Exception($"Failed to add method '{name}' to '{this.Name}'.");
            this.methodImplementations.Add(wrapper); // hold reference to prevent destroying delegate by GC
        }


        // Define method with 5 native arguments.
        void DefineMethodWith5NArgs(Selector name, Delegate implementation, Type? returnType)
        {
            var isFpResult = returnType != null && NativeTypeConversion.IsFloatingPointStructure(returnType);
            var nrSize = returnType != null ? NativeTypeConversion.GetNativeValueCount(returnType) : 0;
            var wrapper = nrSize switch
            {
                0 => isFpResult
                    ? (Delegate)new MethodImplArg5((self, cmd, arg1, arg2, arg3, arg4, arg5) => 
                        this.InvokeMethodImplementation(implementation, self, cmd, arg1, arg2, arg3, arg4, arg5))
                    : new MethodImplArg5((self, cmd, arg1, arg2, arg3, arg4, arg5) => 
                        this.InvokeMethodImplementation(implementation, self, cmd, arg1, arg2, arg3, arg4, arg5)),
                1 => isFpResult
                    ? (Delegate)new MethodImplFpRet1Arg5((self, cmd, arg1, arg2, arg3, arg4, arg5) => 
                    {
                        var nr = (nint)0;
                        NativeTypeConversion.ToNativeValue(this.InvokeMethodImplementation(implementation, self, cmd, arg1, arg2, arg3, arg4, arg5), &nr);
                        return *(double*)&nr;
                    }) 
                    : new MethodImplRet1Arg5((self, cmd, arg1, arg2, arg3, arg4, arg5) => 
                    {
                        var nr = (nint)0;
                        NativeTypeConversion.ToNativeValue(this.InvokeMethodImplementation(implementation, self, cmd, arg1, arg2, arg3, arg4, arg5), &nr);
                        return nr;
                    }),
                2 => isFpResult
                    ? (Delegate)new MethodImplFpRet2Arg5((self, cmd, arg1, arg2, arg3, arg4, arg5) => 
                    {
                        var nr = stackalloc nint[2];
                        NativeTypeConversion.ToNativeValue(this.InvokeMethodImplementation(implementation, self, cmd, arg1, arg2, arg3, arg4, arg5), nr);
                        return new NativeFpResult2((double*)nr);
                    }) 
                    : new MethodImplRet2Arg5((self, cmd, arg1, arg2, arg3, arg4, arg5) => 
                    {
                        var nr = stackalloc nint[2];
                        NativeTypeConversion.ToNativeValue(this.InvokeMethodImplementation(implementation, self, cmd, arg1, arg2, arg3, arg4, arg5), nr);
                        return new NativeResult2(nr);
                    }),
                _ => throw new NotSupportedException($"Unsupported return type '{returnType?.Name}'."),
            };
            if (!class_addMethod(this.Handle, name.Handle, wrapper, ""))
                throw new Exception($"Failed to add method '{name}' to '{this.Name}'.");
            this.methodImplementations.Add(wrapper); // hold reference to prevent destroying delegate by GC
        }


        /// <summary>
        /// Add new property to this class.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="getter">Implementation of getter.</param>
        /// <param name="setter">Implementation of setter.</param>
        /// <typeparam name="T">Type of property value.</typeparam>
        /// <returns>Descriptor of added property.</returns>
        public Property DefineProperty<T>(string name, Func<IntPtr, IntPtr, T> getter, Action<IntPtr, IntPtr, T>? setter = null) where T : struct
        {
            // check state
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
                this.DefineMethod<T>(Selector.FromName(getterName), getter);
                if (setter != null)
                    this.DefineMethod<T>(Selector.FromName(setterName), setter);
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
        public bool Equals(Class? cls)
        {
            if (cls == null)
                return false;
            if (object.ReferenceEquals(cls, this))
                return true;
            if (this.IsProtocol)
                return cls.IsProtocol && protocol_isEqual(this.Handle, cls.Handle);
            return !cls.IsProtocol && this.Handle == cls.Handle;
        }


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
        public Member[] GetInstanceVariables()
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
                this.cachedIVars.TryAdd(name, new Member(this, handle, name));
            this.areAllIVarsCached = true;
            return this.cachedIVars.Values.ToArray();
        }


        /// <summary>
        /// Get method with specific selector.
        /// </summary>
        /// <param name="selector">Selector.</param>
        /// <returns>Method with specific selector, or Null if method cannot be found.</returns>
        public unsafe Method? GetMethod(Selector selector)
        {
            if (this.cachedMethods.TryGetValue(selector, out var method))
                return method;
            if (this.IsProtocol)
                return null;
            var handle = class_getInstanceMethod(this.Handle, selector.Handle);
            if (handle == IntPtr.Zero)
                return null;
            return new Method(this, handle, selector).Also(it =>
            {
                this.cachedMethods.TryAdd(selector, it);
            });
        }


        /// <summary>
        /// Get all methods of this class.
        /// </summary>
        /// <returns>Descriptors of methods.</returns>
        public unsafe Method[] GetMethods()
        {
            if (this.areAllMethodsCached)
                return this.cachedMethods.Values.ToArray();
            if (!this.IsProtocol)
            {
                var methodsPtr = class_copyMethodList(this.Handle, out var count);
                if (methodsPtr != null)
                {
                    try
                    {
                        for (var i = 0; i < count; ++i)
                        {
                            var selector = new Selector(method_getName(methodsPtr[i]));
                            var method = new Method(this, methodsPtr[i], selector);
                            this.cachedMethods.TryAdd(selector, method);
                        }
                    }
                    finally
                    {
                        NativeMemory.Free(methodsPtr);
                    }
                }
            }
            this.areAllMethodsCached = true;
            return this.cachedMethods.Values.ToArray();
        }


        /// <summary>
        /// Get all properties of the class.
        /// </summary>
        /// <returns>Descriptors of properties.</returns>
        public unsafe Property[] GetProperties()
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
                this.cachedProperties.TryAdd(name, new Property(this, handle, name));
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


        // Invoke actual method implementation.
        unsafe object? InvokeMethodImplementation(Delegate implementation, IntPtr self, IntPtr cmd, params nint[] nativeArgs)
        {
            var invokeMethod = implementation.GetType().GetMethod("Invoke").AsNonNull();
            var parameters = invokeMethod.GetParameters();
            var argCount = parameters.Length;
            var args = new object?[argCount];
            args[0] = self;
            args[1] = cmd;
            if (argCount >= 3)
            {
                fixed (nint* p = nativeArgs)
                {
                    var remainingNArgs = nativeArgs.Length;
                    var nArgsPtr = p;
                    for (var i = 2; i < argCount; ++i)
                    {
                        args[i] = NativeTypeConversion.FromNativeValue(nArgsPtr, remainingNArgs, parameters[i].ParameterType, out var consumedNArgs);
                        nArgsPtr += consumedNArgs;
                        remainingNArgs -= consumedNArgs;
                    }
                }
            }
            return implementation.DynamicInvoke(args);
        }


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
        public bool TryGetClrObject<T>(IntPtr obj, [NotNullWhen(true)] out T? clrObj)
        {
            clrObj = default;
            if (this.clrObjectHandleVar == null
                || !this.IsAssignableFrom(GetClass(obj)))
            {
                return false;
            }
            var handle = NSObject.GetVariable<IntPtr>(obj, this.clrObjectHandleVar);
            var rawClrObj = handle != IntPtr.Zero ? GCHandle.FromIntPtr(handle).Target : null;
            if (rawClrObj is T targetObj)
            {
                clrObj = targetObj;
                return true;
            }
            return false;
        }
        

        /// <summary>
        /// Try finding class variable of class.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="ivar">Descriptor of class variable.</param>
        /// <returns>True if variable found.</returns>
        public unsafe bool TryGetClassVriable(string name, out Member? ivar)
        {
            if (this.IsProtocol)
            {
                ivar = null;
                return false;
            }
            if (this.cachedCVars.TryGetValue(name, out ivar))
                return true;
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException($"Invalid variable name: {name}.");
            var handle = class_getClassVariable(this.Handle, name);
            if (handle == IntPtr.Zero)
            {
                ivar = null;
                return false;
            }
            ivar = new Member(this, handle, name).Also(it => this.cachedCVars.TryAdd(name, it));
            return true;
        }
        

        /// <summary>
        /// Try finding instance variable of class.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="ivar">Descriptor of instance variable.</param>
        /// <returns>True if variable found.</returns>
        public unsafe bool TryGetInstanceVriable(string name, out Member? ivar)
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
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException($"Invalid variable name: {name}.");
            var handle = class_getInstanceVariable(this.Handle, name);
            if (handle == IntPtr.Zero)
            {
                ivar = null;
                return false;
            }
            ivar = new Member(this, handle, name).Also(it => this.cachedIVars.TryAdd(name, it));
            return true;
        }
        

        /// <summary>
        /// Try finding property of class.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="property">Descriptor of found property.</param>
        /// <returns>True if property found.</returns>
        public unsafe bool TryGetProperty(string name, out Property? property)
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
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException($"Invalid property name: {name}.");
            var handle = class_getProperty(this.Handle, name);
            if (handle == IntPtr.Zero)
            {
                property = null;
                return false;
            }
            property = new Property(this, handle, name).Also(it => this.cachedProperties.TryAdd(name, it));
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
            NSObject.GetVariable<IntPtr>(obj, this.clrObjectHandleVar).Let(it =>
            {
                if (it != IntPtr.Zero)
                    GCHandle.FromIntPtr(it).Free();
            });
            NSObject.SetVariable(obj, this.clrObjectHandleVar, clrObj != null ? GCHandle.Alloc(clrObj, GCHandleType.Weak) : default);
            return true;
        }


        // Throw exception if implementation is invalid.
        /*
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
        */


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
    

    delegate void MethodImpl(IntPtr self, IntPtr cmd);
    delegate void MethodImplArg1(IntPtr self, IntPtr cmd, nint arg1);
    delegate void MethodImplArg2(IntPtr self, IntPtr cmd, nint arg1, nint arg2);
    delegate void MethodImplArg3(IntPtr self, IntPtr cmd, nint arg1, nint arg2, nint arg3);
    delegate void MethodImplArg4(IntPtr self, IntPtr cmd, nint arg1, nint arg2, nint arg3, nint arg4);
    delegate void MethodImplArg5(IntPtr self, IntPtr cmd, nint arg1, nint arg2, nint arg3, nint arg4, nint arg5);
    delegate nint MethodImplRet1(IntPtr self, IntPtr cmd);
    delegate nint MethodImplRet1Arg1(IntPtr self, IntPtr cmd, nint arg1);
    delegate nint MethodImplRet1Arg2(IntPtr self, IntPtr cmd, nint arg1, nint arg2);
    delegate nint MethodImplRet1Arg3(IntPtr self, IntPtr cmd, nint arg1, nint arg2, nint arg3);
    delegate nint MethodImplRet1Arg4(IntPtr self, IntPtr cmd, nint arg1, nint arg2, nint arg3, nint arg4);
    delegate nint MethodImplRet1Arg5(IntPtr self, IntPtr cmd, nint arg1, nint arg2, nint arg3, nint arg4, nint arg5);
    delegate double MethodImplFpRet1(IntPtr self, IntPtr cmd);
    delegate double MethodImplFpRet1Arg1(IntPtr self, IntPtr cmd, nint arg1);
    delegate double MethodImplFpRet1Arg2(IntPtr self, IntPtr cmd, nint arg1, nint arg2);
    delegate double MethodImplFpRet1Arg3(IntPtr self, IntPtr cmd, nint arg1, nint arg2, nint arg3);
    delegate double MethodImplFpRet1Arg4(IntPtr self, IntPtr cmd, nint arg1, nint arg2, nint arg3, nint arg4);
    delegate double MethodImplFpRet1Arg5(IntPtr self, IntPtr cmd, nint arg1, nint arg2, nint arg3, nint arg4, nint arg5);
    delegate NativeResult2 MethodImplRet2(IntPtr self, IntPtr cmd);
    delegate NativeResult2 MethodImplRet2Arg1(IntPtr self, IntPtr cmd, nint arg1);
    delegate NativeResult2 MethodImplRet2Arg2(IntPtr self, IntPtr cmd, nint arg1, nint arg2);
    delegate NativeResult2 MethodImplRet2Arg3(IntPtr self, IntPtr cmd, nint arg1, nint arg2, nint arg3);
    delegate NativeResult2 MethodImplRet2Arg4(IntPtr self, IntPtr cmd, nint arg1, nint arg2, nint arg3, nint arg4);
    delegate NativeResult2 MethodImplRet2Arg5(IntPtr self, IntPtr cmd, nint arg1, nint arg2, nint arg3, nint arg4, nint arg5);
    delegate NativeFpResult2 MethodImplFpRet2(IntPtr self, IntPtr cmd);
    delegate NativeFpResult2 MethodImplFpRet2Arg1(IntPtr self, IntPtr cmd, nint arg1);
    delegate NativeFpResult2 MethodImplFpRet2Arg2(IntPtr self, IntPtr cmd, nint arg1, nint arg2);
    delegate NativeFpResult2 MethodImplFpRet2Arg3(IntPtr self, IntPtr cmd, nint arg1, nint arg2, nint arg3);
    delegate NativeFpResult2 MethodImplFpRet2Arg4(IntPtr self, IntPtr cmd, nint arg1, nint arg2, nint arg3, nint arg4);
    delegate NativeFpResult2 MethodImplFpRet2Arg5(IntPtr self, IntPtr cmd, nint arg1, nint arg2, nint arg3, nint arg4, nint arg5);
}