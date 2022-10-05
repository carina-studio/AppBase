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
        static extern bool class_respondsToSelector(IntPtr cls, IntPtr sel);
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
        static readonly nint[] EmptyNativeValues = new nint[0];


        // Fields.
        volatile bool areAllIVarsCached;
        volatile bool areAllMethodsCached;
        volatile bool areAllPropertiesCached;
        readonly IDictionary<string, Variable> cachedCVars = new ConcurrentDictionary<string, Variable>();
        readonly IDictionary<string, Variable> cachedIVars = new ConcurrentDictionary<string, Variable>();
        readonly IDictionary<Selector, Method> cachedMethods = new ConcurrentDictionary<Selector, Method>();
        readonly IDictionary<string, Property> cachedProperties = new ConcurrentDictionary<string, Property>();
        internal Variable? clrObjectHandleVar;
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
                cls.clrObjectHandleVar = cls.DefineInstanceVariable<IntPtr>(clrObjHandleVarName);
            else
            {
                for (var i = 1; i <= 10; ++i)
                {
                    var candName = $"{clrObjHandleVarName}_{i}";
                    if (!cls.cachedIVars.ContainsKey(candName))
                    {
                        cls.clrObjectHandleVar = cls.DefineInstanceVariable<IntPtr>(candName);
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
        /// <param name="elementCount">Number of element is value is an array.</param>
        /// <returns>Descriptor of instance variable.</returns>
        public Variable DefineInstanceVariable<T>(string name, int elementCount = 1) =>
            this.DefineInstanceVariable(name, typeof(T), elementCount);


        /// <summary>
        /// Define instance variable.
        /// </summary>
        /// <param name="name">Name of variable.</param>
        /// <param name="type">Type of variable.</param>
        /// <param name="elementCount">Number of element is value is an array.</param>
        /// <returns>Descriptor of instance variable.</returns>
        public Variable DefineInstanceVariable(string name, Type type, int elementCount = 1)
        {
            this.VerifyRegistered();
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException($"Invalid name: {name}.");
            if (elementCount <= 0)
                throw new ArgumentOutOfRangeException(nameof(elementCount));
            var typeEncoding = NativeTypeConversion.ToTypeEncoding(type, elementCount);
            var dataSize = Global.Run(() =>
            {
                int SizeOf(Type type)
                {
                    if (type.IsValueType)
                        return Marshal.SizeOf(type);
                    if (typeof(NSObject).IsAssignableFrom(type)
                        || type == typeof(Class)
                        || type == typeof(Selector))
                    {
                        return IntPtr.Size;
                    }
                    return Marshal.SizeOf(type);
                }
                if (type.IsArray)
                {
                    if (type.GetArrayRank() > 1)
                        throw new ArgumentException("Only 1-dimensional array is supported.");
                    return elementCount * SizeOf(type.GetElementType()!);
                }
                return SizeOf(type);
            });
            byte alignment = IntPtr.Size switch
            {
                4 => 2,
                8 => 3,
                16 => 4,
                _ => throw new NotSupportedException($"Unsupported size of poiter: {IntPtr.Size}."),
            };
            if (!class_addIvar(this.Handle, name, (nuint)dataSize, alignment, typeEncoding))
                throw new Exception($"Failed to add instance variable '{name}' to '{this.Name}'.");
            var handle = class_getInstanceVariable(this.Handle, name);
            if (handle == IntPtr.Zero)
                throw new Exception($"Failed to add instance variable '{name}' to '{this.Name}'.");
            return new Variable(this, handle, name).Also(it =>
            {
                cachedIVars.TryAdd(name, it);
            });
        }


        /// <summary>
        /// Define method without argument.
        /// </summary>
        /// <param name="name">Selector.</param>
        /// <param name="implementation">Implementation.</param>
        public void DefineMethod(Selector name, Action<IntPtr, Selector> implementation) =>
            this.DefineMethod(name, implementation, null);


        /// <summary>
        /// Define method without argument.
        /// </summary>
        /// <param name="name">Selector.</param>
        /// <param name="implementation">Implementation.</param>
        public void DefineMethod<R>(Selector name, Func<IntPtr, Selector, R> implementation) =>
            this.DefineMethod(name, implementation, typeof(R));


        /// <summary>
        /// Define method with 1 argument.
        /// </summary>
        /// <param name="name">Selector.</param>
        /// <param name="implementation">Implementation.</param>
        public void DefineMethod<TArg1>(Selector name, Action<IntPtr, Selector, TArg1> implementation)
        {
            if (NativeTypeConversion.IsFloatingPointStructure(typeof(TArg1)))
                this.DefineMethodWithNFpArgs(name, NativeTypeConversion.GetNativeFpValueCount<TArg1>(), implementation, null);
            else
                this.DefineMethodWithNArgs(name, NativeTypeConversion.GetNativeValueCount<TArg1>(), implementation, null);
        }


        /// <summary>
        /// Define method with 1 argument.
        /// </summary>
        /// <param name="name">Selector.</param>
        /// <param name="implementation">Implementation.</param>
        public void DefineMethod<TArg1, R>(Selector name, Func<IntPtr, Selector, TArg1, R> implementation)
        {
            if (NativeTypeConversion.IsFloatingPointStructure(typeof(TArg1)))
                this.DefineMethodWithNFpArgs(name, NativeTypeConversion.GetNativeFpValueCount<TArg1>(), implementation, typeof(R));
            else
                this.DefineMethodWithNArgs(name, NativeTypeConversion.GetNativeValueCount<TArg1>(), implementation, typeof(R));
        }


        /// <summary>
        /// Define method with 2 arguments.
        /// </summary>
        /// <param name="name">Selector.</param>
        /// <param name="implementation">Implementation.</param>
        public void DefineMethod<TArg1, TArg2>(Selector name, Action<IntPtr, Selector, TArg1, TArg2> implementation)
        {
            var allFpArgs = NativeTypeConversion.AreAllFloatingPointStructures(typeof(TArg1), typeof(TArg2));
            var nArgCount = allFpArgs
                ? NativeTypeConversion.GetNativeFpValueCount(typeof(TArg1), typeof(TArg2))
                : NativeTypeConversion.GetNativeValueCount(typeof(TArg1), typeof(TArg2));
            if (allFpArgs)
                this.DefineMethodWithNFpArgs(name, nArgCount, implementation, null);
            else
                this.DefineMethodWithNArgs(name, nArgCount, implementation, null);
        }


        /// <summary>
        /// Define method with 2 arguments.
        /// </summary>
        /// <param name="name">Selector.</param>
        /// <param name="implementation">Implementation.</param>
        public void DefineMethod<TArg1, TArg2, R>(Selector name, Func<IntPtr, Selector, TArg1, TArg2, R> implementation)
        {
            var allFpArgs = NativeTypeConversion.AreAllFloatingPointStructures(typeof(TArg1), typeof(TArg2));
            var nArgCount = allFpArgs
                ? NativeTypeConversion.GetNativeFpValueCount(typeof(TArg1), typeof(TArg2))
                : NativeTypeConversion.GetNativeValueCount(typeof(TArg1), typeof(TArg2));
            if (allFpArgs)
                this.DefineMethodWithNFpArgs(name, nArgCount, implementation, typeof(R));
            else
                this.DefineMethodWithNArgs(name, nArgCount, implementation, typeof(R));
        }


        /// <summary>
        /// Define method with 3 arguments.
        /// </summary>
        /// <param name="name">Selector.</param>
        /// <param name="implementation">Implementation.</param>
        public void DefineMethod<TArg1, TArg2, TArg3>(Selector name, Action<IntPtr, Selector, TArg1, TArg2, TArg3> implementation)
        {
            var allFpArgs = NativeTypeConversion.AreAllFloatingPointStructures(typeof(TArg1), typeof(TArg2), typeof(TArg3));
            var nArgCount = allFpArgs
                ? NativeTypeConversion.GetNativeFpValueCount(typeof(TArg1), typeof(TArg2), typeof(TArg3))
                : NativeTypeConversion.GetNativeValueCount(typeof(TArg1), typeof(TArg2), typeof(TArg3));
            if (allFpArgs)
                this.DefineMethodWithNFpArgs(name, nArgCount, implementation, null);
            else
                this.DefineMethodWithNArgs(name, nArgCount, implementation, null);
        }


        /// <summary>
        /// Define method with 3 arguments.
        /// </summary>
        /// <param name="name">Selector.</param>
        /// <param name="implementation">Implementation.</param>
        public void DefineMethod<TArg1, TArg2, TArg3, R>(Selector name, Func<IntPtr, Selector, TArg1, TArg2, TArg3, R> implementation)
        {
            var allFpArgs = NativeTypeConversion.AreAllFloatingPointStructures(typeof(TArg1), typeof(TArg2), typeof(TArg3));
            var nArgCount = allFpArgs
                ? NativeTypeConversion.GetNativeFpValueCount(typeof(TArg1), typeof(TArg2), typeof(TArg3))
                : NativeTypeConversion.GetNativeValueCount(typeof(TArg1), typeof(TArg2), typeof(TArg3));
            if (allFpArgs)
                this.DefineMethodWithNFpArgs(name, nArgCount, implementation, typeof(R));
            else
                this.DefineMethodWithNArgs(name, nArgCount, implementation, typeof(R));
        }


        /// <summary>
        /// Define method with 4 arguments.
        /// </summary>
        /// <param name="name">Selector.</param>
        /// <param name="implementation">Implementation.</param>
        public void DefineMethod<TArg1, TArg2, TArg3, TArg4>(Selector name, Action<IntPtr, Selector, TArg1, TArg2, TArg3, TArg4> implementation)
        {
            var allFpArgs = NativeTypeConversion.AreAllFloatingPointStructures(typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4));
            var nArgCount = allFpArgs
                ? NativeTypeConversion.GetNativeFpValueCount(typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4))
                : NativeTypeConversion.GetNativeValueCount(typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4));
            if (allFpArgs)
                this.DefineMethodWithNFpArgs(name, nArgCount, implementation, null);
            else
                this.DefineMethodWithNArgs(name, nArgCount, implementation, null);
        }


        /// <summary>
        /// Define method with 4 arguments.
        /// </summary>
        /// <param name="name">Selector.</param>
        /// <param name="implementation">Implementation.</param>
        public void DefineMethod<TArg1, TArg2, TArg3, TArg4, R>(Selector name, Func<IntPtr, Selector, TArg1, TArg2, TArg3, TArg4, R> implementation)
        {
            var allFpArgs = NativeTypeConversion.AreAllFloatingPointStructures(typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4));
            var nArgCount = allFpArgs
                ? NativeTypeConversion.GetNativeFpValueCount(typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4))
                : NativeTypeConversion.GetNativeValueCount(typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4));
            if (allFpArgs)
                this.DefineMethodWithNFpArgs(name, nArgCount, implementation, typeof(R));
            else
                this.DefineMethodWithNArgs(name, nArgCount, implementation, typeof(R));
        }


        /// <summary>
        /// Define method with 5 arguments.
        /// </summary>
        /// <param name="name">Selector.</param>
        /// <param name="implementation">Implementation.</param>
        public void DefineMethod<TArg1, TArg2, TArg3, TArg4, TArg5>(Selector name, Action<IntPtr, Selector, TArg1, TArg2, TArg3, TArg4, TArg5> implementation)
        {
            var allFpArgs = NativeTypeConversion.AreAllFloatingPointStructures(typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4), typeof(TArg5));
            var nArgCount = allFpArgs
                ? NativeTypeConversion.GetNativeFpValueCount(typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4), typeof(TArg5))
                : NativeTypeConversion.GetNativeValueCount(typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4), typeof(TArg5));
            if (allFpArgs)
                this.DefineMethodWithNFpArgs(name, nArgCount, implementation, null);
            else
                this.DefineMethodWithNArgs(name, nArgCount, implementation, null);
        }


        /// <summary>
        /// Define method with 5 arguments.
        /// </summary>
        /// <param name="name">Selector.</param>
        /// <param name="implementation">Implementation.</param>
        public void DefineMethod<TArg1, TArg2, TArg3, TArg4, TArg5, R>(Selector name, Func<IntPtr, Selector, TArg1, TArg2, TArg3, TArg4, TArg5, R> implementation)
        {
            var allFpArgs = NativeTypeConversion.AreAllFloatingPointStructures(typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4), typeof(TArg5));
            var nArgCount = allFpArgs
                ? NativeTypeConversion.GetNativeFpValueCount(typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4), typeof(TArg5))
                : NativeTypeConversion.GetNativeValueCount(typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4), typeof(TArg5));
            if (allFpArgs)
                this.DefineMethodWithNFpArgs(name, nArgCount, implementation, typeof(R));
            else
                this.DefineMethodWithNArgs(name, nArgCount, implementation, typeof(R));
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
                        this.InvokeMethodImplementation(implementation, self, cmd, EmptyNativeValues))
                    : new MethodImpl((self, cmd) =>
                        this.InvokeMethodImplementation(implementation, self, cmd, EmptyNativeValues)),
                1 => isFpResult
                    ? (Delegate)new MethodImplFpRet1((self, cmd) => 
                    {
                        var nr = (nint)0;
                        NativeTypeConversion.ToNativeValue(this.InvokeMethodImplementation(implementation, self, cmd, EmptyNativeValues), &nr);
                        return *(double*)&nr;
                    }) 
                    : new MethodImplRet1((self, cmd) => 
                    {
                        var nr = (nint)0;
                        NativeTypeConversion.ToNativeValue(this.InvokeMethodImplementation(implementation, self, cmd, EmptyNativeValues), &nr);
                        return nr;
                    }),
                2 => isFpResult
                    ? (Delegate)new MethodImplFpRet2((self, cmd) => 
                    {
                        var nr = stackalloc nint[2];
                        NativeTypeConversion.ToNativeValue(this.InvokeMethodImplementation(implementation, self, cmd, EmptyNativeValues), nr);
                        return new NativeFpResult2((double*)nr);
                    }) 
                    : new MethodImplRet2((self, cmd) => 
                    {
                        var nr = stackalloc nint[2];
                        NativeTypeConversion.ToNativeValue(this.InvokeMethodImplementation(implementation, self, cmd, EmptyNativeValues), nr);
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
        void DefineMethodWith1NFpArg(Selector name, Delegate implementation, Type? returnType)
        {
            var isFpResult = returnType != null && NativeTypeConversion.IsFloatingPointStructure(returnType);
            var nrSize = returnType != null ? NativeTypeConversion.GetNativeValueCount(returnType) : 0;
            var wrapper = nrSize switch
            {
                0 => isFpResult
                    ? (Delegate)new MethodImplFpArg1((self, cmd, arg1) => 
                        this.InvokeMethodImplementation(implementation, self, cmd, arg1))
                    : new MethodImplFpArg1((self, cmd, arg1) =>
                        this.InvokeMethodImplementation(implementation, self, cmd, arg1)),
                1 => isFpResult
                    ? (Delegate)new MethodImplFpRet1FpArg1((self, cmd, arg1) => 
                    {
                        var nr = (nint)0;
                        NativeTypeConversion.ToNativeValue(this.InvokeMethodImplementation(implementation, self, cmd, arg1), &nr);
                        return *(double*)&nr;
                    }) 
                    : new MethodImplRet1FpArg1((self, cmd, arg1) => 
                    {
                        var nr = (nint)0;
                        NativeTypeConversion.ToNativeValue(this.InvokeMethodImplementation(implementation, self, cmd, arg1), &nr);
                        return nr;
                    }),
                2 => isFpResult
                    ? (Delegate)new MethodImplFpRet2FpArg1((self, cmd, arg1) => 
                    {
                        var nr = stackalloc nint[2];
                        NativeTypeConversion.ToNativeValue(this.InvokeMethodImplementation(implementation, self, cmd, arg1), nr);
                        return new NativeFpResult2((double*)nr);
                    }) 
                    : new MethodImplRet2FpArg1((self, cmd, arg1) => 
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
        void DefineMethodWith2NFpArgs(Selector name, Delegate implementation, Type? returnType)
        {
            var isFpResult = returnType != null && NativeTypeConversion.IsFloatingPointStructure(returnType);
            var nrSize = returnType != null ? NativeTypeConversion.GetNativeValueCount(returnType) : 0;
            var wrapper = nrSize switch
            {
                0 => isFpResult
                    ? (Delegate)new MethodImplFpArg2((self, cmd, arg1, arg2) => 
                        this.InvokeMethodImplementation(implementation, self, cmd, arg1, arg2))
                    : new MethodImplFpArg2((self, cmd, arg1, arg2) => 
                        this.InvokeMethodImplementation(implementation, self, cmd, arg1, arg2)),
                1 => isFpResult
                    ? (Delegate)new MethodImplFpRet1FpArg2((self, cmd, arg1, arg2) => 
                    {
                        var nr = (nint)0;
                        NativeTypeConversion.ToNativeValue(this.InvokeMethodImplementation(implementation, self, cmd, arg1, arg2), &nr);
                        return *(double*)&nr;
                    }) 
                    : new MethodImplRet1FpArg2((self, cmd, arg1, arg2) => 
                    {
                        var nr = (nint)0;
                        NativeTypeConversion.ToNativeValue(this.InvokeMethodImplementation(implementation, self, cmd, arg1, arg2), &nr);
                        return nr;
                    }),
                2 => isFpResult
                    ? (Delegate)new MethodImplFpRet2FpArg2((self, cmd, arg1, arg2) => 
                    {
                        var nr = stackalloc nint[2];
                        NativeTypeConversion.ToNativeValue(this.InvokeMethodImplementation(implementation, self, cmd, arg1, arg2), nr);
                        return new NativeFpResult2((double*)nr);
                    }) 
                    : new MethodImplRet2FpArg2((self, cmd, arg1, arg2) => 
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
        void DefineMethodWith3NFpArgs(Selector name, Delegate implementation, Type? returnType)
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
        void DefineMethodWith4NFpArgs(Selector name, Delegate implementation, Type? returnType)
        {
            var isFpResult = returnType != null && NativeTypeConversion.IsFloatingPointStructure(returnType);
            var nrSize = returnType != null ? NativeTypeConversion.GetNativeValueCount(returnType) : 0;
            var wrapper = nrSize switch
            {
                0 => isFpResult
                    ? (Delegate)new MethodImplFpArg4((self, cmd, arg1, arg2, arg3, arg4) => 
                        this.InvokeMethodImplementation(implementation, self, cmd, arg1, arg2, arg3, arg4))
                    : new MethodImplFpArg4((self, cmd, arg1, arg2, arg3, arg4) => 
                        this.InvokeMethodImplementation(implementation, self, cmd, arg1, arg2, arg3, arg4)),
                1 => isFpResult
                    ? (Delegate)new MethodImplFpRet1FpArg4((self, cmd, arg1, arg2, arg3, arg4) => 
                    {
                        var nr = (nint)0;
                        NativeTypeConversion.ToNativeValue(this.InvokeMethodImplementation(implementation, self, cmd, arg1, arg2, arg3, arg4), &nr);
                        return *(double*)&nr;
                    }) 
                    : new MethodImplRet1FpArg4((self, cmd, arg1, arg2, arg3, arg4) => 
                    {
                        var nr = (nint)0;
                        NativeTypeConversion.ToNativeValue(this.InvokeMethodImplementation(implementation, self, cmd, arg1, arg2, arg3, arg4), &nr);
                        return nr;
                    }),
                2 => isFpResult
                    ? (Delegate)new MethodImplFpRet2FpArg4((self, cmd, arg1, arg2, arg3, arg4) => 
                    {
                        var nr = stackalloc nint[2];
                        NativeTypeConversion.ToNativeValue(this.InvokeMethodImplementation(implementation, self, cmd, arg1, arg2, arg3, arg4), nr);
                        return new NativeFpResult2((double*)nr);
                    }) 
                    : new MethodImplRet2FpArg4((self, cmd, arg1, arg2, arg3, arg4) => 
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
        void DefineMethodWith5NFpArgs(Selector name, Delegate implementation, Type? returnType)
        {
            var isFpResult = returnType != null && NativeTypeConversion.IsFloatingPointStructure(returnType);
            var nrSize = returnType != null ? NativeTypeConversion.GetNativeValueCount(returnType) : 0;
            var wrapper = nrSize switch
            {
                0 => isFpResult
                    ? (Delegate)new MethodImplFpArg5((self, cmd, arg1, arg2, arg3, arg4, arg5) => 
                        this.InvokeMethodImplementation(implementation, self, cmd, arg1, arg2, arg3, arg4, arg5))
                    : new MethodImplFpArg5((self, cmd, arg1, arg2, arg3, arg4, arg5) => 
                        this.InvokeMethodImplementation(implementation, self, cmd, arg1, arg2, arg3, arg4, arg5)),
                1 => isFpResult
                    ? (Delegate)new MethodImplFpRet1FpArg5((self, cmd, arg1, arg2, arg3, arg4, arg5) => 
                    {
                        var nr = (nint)0;
                        NativeTypeConversion.ToNativeValue(this.InvokeMethodImplementation(implementation, self, cmd, arg1, arg2, arg3, arg4, arg5), &nr);
                        return *(double*)&nr;
                    }) 
                    : new MethodImplRet1FpArg5((self, cmd, arg1, arg2, arg3, arg4, arg5) => 
                    {
                        var nr = (nint)0;
                        NativeTypeConversion.ToNativeValue(this.InvokeMethodImplementation(implementation, self, cmd, arg1, arg2, arg3, arg4, arg5), &nr);
                        return nr;
                    }),
                2 => isFpResult
                    ? (Delegate)new MethodImplFpRet2FpArg5((self, cmd, arg1, arg2, arg3, arg4, arg5) => 
                    {
                        var nr = stackalloc nint[2];
                        NativeTypeConversion.ToNativeValue(this.InvokeMethodImplementation(implementation, self, cmd, arg1, arg2, arg3, arg4, arg5), nr);
                        return new NativeFpResult2((double*)nr);
                    }) 
                    : new MethodImplRet2FpArg5((self, cmd, arg1, arg2, arg3, arg4, arg5) => 
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


        // Define method with native arguments.
        void DefineMethodWithNArgs(Selector name, int nativeArgCount, Delegate implementation, Type? returnType)
        {
            switch (nativeArgCount)
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
        void DefineMethodWithNFpArgs(Selector name, int nativeArgCount, Delegate implementation, Type? returnType)
        {
            switch (nativeArgCount)
            {
                case 1:
                    this.DefineMethodWith1NFpArg(name, implementation, null);
                    break;
                case 2:
                    this.DefineMethodWith2NFpArgs(name, implementation, null);
                    break;
                case 3:
                    this.DefineMethodWith3NFpArgs(name, implementation, null);
                    break;
                case 4:
                    this.DefineMethodWith4NFpArgs(name, implementation, null);
                    break;
                case 5:
                    this.DefineMethodWith5NFpArgs(name, implementation, null);
                    break;
                default:
                    throw new NotSupportedException("Too many native arguments.");
            }
        }


        /// <summary>
        /// Add new property to this class.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="getter">Implementation of getter.</param>
        /// <param name="setter">Implementation of setter.</param>
        /// <typeparam name="T">Type of property value.</typeparam>
        /// <returns>Descriptor of added property.</returns>
        public Property DefineProperty<T>(string name, Func<IntPtr, Selector, T> getter, Action<IntPtr, Selector, T>? setter = null) where T : struct
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
            if (!class_addProperty(this.Handle, name, attrs, (uint)attrs.Length))
                throw new ArgumentException($"Unagle to add property '{name}' to '{this.Name}'.");
            var property = this.GetProperty(name) 
                ?? throw new ArgumentException($"Unagle to add property '{name}' to '{this.Name}'.");

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
            return property;
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
        /// Wrap native instance as <see cref="Class"/>.
        /// </summary>
        /// <param name="handle">Handle of instance.</param>
        /// <returns><see cref="Class"/>.</returns>
        public static unsafe Class FromHandle(IntPtr handle)
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
            return FromHandle(object_getClass(obj));
        }


        /// <summary>
        /// Get class variable defined by this class.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <returns>Descriptor of class variable.</returns>
        public unsafe Variable? GetClassVriable(string name)
        {
            if (this.IsProtocol)
                return null;
            if (this.cachedCVars.TryGetValue(name, out var ivar))
                return ivar;
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException($"Invalid variable name: {name}.");
            var handle = class_getClassVariable(this.Handle, name);
            if (handle == IntPtr.Zero)
                return null;
            return new Variable(this, handle, name).Also(it => this.cachedCVars.TryAdd(name, it));
        }


        /// <inheritdoc/>
        public override int GetHashCode() =>
            this.Name.GetHashCode();
        

        /// <summary>
        /// Get instance variable defined by this class.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <returns>Descriptor of instance variable.</returns>
        public unsafe Variable? GetInstanceVriable(string name)
        {
            if (this.IsProtocol)
                return null;
            if (this.cachedIVars.TryGetValue(name, out var ivar))
                return ivar;
            if (this.areAllIVarsCached)
                return null;
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException($"Invalid variable name: {name}.");
            var handle = class_getInstanceVariable(this.Handle, name);
            if (handle == IntPtr.Zero)
                return null;
            return new Variable(this, handle, name).Also(it => this.cachedIVars.TryAdd(name, it));
        }


        /// <summary>
        /// Get all instance variables of the class. Instance variables defined by super classes are excluded.
        /// </summary>
        /// <returns>Descriptors of instance variables.</returns>
        public Variable[] GetInstanceVariables()
        {
            if (this.areAllIVarsCached || this.IsProtocol)
                return this.cachedIVars.Values.ToArray();
            var varsPtr = class_copyIvarList(this.Handle, out var count);
            try
            {
                for (var i = 0; i < count; ++i)
                {
                    var name = new string(ivar_getName(varsPtr[i]));
                    this.cachedIVars.TryAdd(name, new Variable(this, varsPtr[i], name));
                }
            }
            finally
            {
                NativeMemory.Free(varsPtr);
            }
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
                            var selector = Selector.FromHandle(method_getName(methodsPtr[i]));
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
        /// Get all properties of the class. Properties defined by super classes are excluded.
        /// </summary>
        /// <returns>Descriptors of properties.</returns>
        public unsafe Property[] GetProperties()
        {
            if (areAllPropertiesCached)
                return this.cachedProperties.Values.ToArray();
            var count = 0u;
            var propertiesPtr = this.IsProtocol
                ? protocol_copyPropertyList(this.Handle, out count)
                : class_copyPropertyList(this.Handle, out count);
            try
            {
                for (var i = 0; i < count; ++i)
                {
                    var name = new string(property_getName(propertiesPtr[i]));
                    this.cachedProperties.TryAdd(name, new Property(this, propertiesPtr[i], name));
                }
            }
            finally
            {
                NativeMemory.Free(propertiesPtr);
            }
            this.areAllPropertiesCached = true;
            return this.cachedProperties.Values.ToArray();
        }


        /// <summary>
        /// Get property defined by this class.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <returns>Descriptor of found property.</returns>
        public unsafe Property? GetProperty(string name)
        {
            if (this.IsProtocol)
                this.GetProperties();
            if (this.cachedProperties.TryGetValue(name, out var property))
                return property;
            if (this.areAllPropertiesCached)
                return null;
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException($"Invalid property name: {name}.");
            var handle = class_getProperty(this.Handle, name);
            if (handle == IntPtr.Zero)
                return null;
            return new Property(this, handle, name).Also(it => this.cachedProperties.TryAdd(name, it));
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
        /// Check whether given method is implemented by this class or not.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <returns>True if method is implemented by this class.</returns>
        public bool HasMethod(Selector name) =>
            class_respondsToSelector(this.Handle, name.Handle);


        // Invoke actual method implementation.
        unsafe object? InvokeMethodImplementation(Delegate implementation, IntPtr self, IntPtr cmd, params double[] nativeArgs)
        {
            var invokeMethod = implementation.GetType().GetMethod("Invoke").AsNonNull();
            var parameters = invokeMethod.GetParameters();
            var argCount = parameters.Length;
            var args = new object?[argCount];
            args[0] = self;
            args[1] = Selector.FromHandle(cmd);
            if (argCount >= 3)
            {
                fixed (double* p = nativeArgs)
                {
                    var remainingNArgs = nativeArgs.Length;
                    var nArgsPtr = p;
                    for (var i = 2; i < argCount; ++i)
                    {
                        args[i] = NativeTypeConversion.FromNativeFpValue(nArgsPtr, remainingNArgs, parameters[i].ParameterType, out var consumedNArgs);
                        nArgsPtr += consumedNArgs;
                        remainingNArgs -= consumedNArgs;
                    }
                }
            }
            return implementation.DynamicInvoke(args);
        }
        unsafe object? InvokeMethodImplementation(Delegate implementation, IntPtr self, IntPtr cmd, params nint[] nativeArgs)
        {
            var invokeMethod = implementation.GetType().GetMethod("Invoke").AsNonNull();
            var parameters = invokeMethod.GetParameters();
            var argCount = parameters.Length;
            var args = new object?[argCount];
            args[0] = self;
            args[1] = Selector.FromHandle(cmd);
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
                                this.superClass = FromHandle(it);
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


    delegate void MethodImplFpArg1(IntPtr self, IntPtr cmd, double arg1);
    delegate void MethodImplFpArg2(IntPtr self, IntPtr cmd, double arg1, double arg2);
    delegate void MethodImplFpArg3(IntPtr self, IntPtr cmd, double arg1, double arg2, double arg3);
    delegate void MethodImplFpArg4(IntPtr self, IntPtr cmd, double arg1, double arg2, double arg3, double arg4);
    delegate void MethodImplFpArg5(IntPtr self, IntPtr cmd, double arg1, double arg2, double arg3, double arg4, double arg5);
    delegate nint MethodImplRet1FpArg1(IntPtr self, IntPtr cmd, double arg1);
    delegate nint MethodImplRet1FpArg2(IntPtr self, IntPtr cmd, double arg1, double arg2);
    delegate nint MethodImplRet1FpArg3(IntPtr self, IntPtr cmd, double arg1, double arg2, double arg3);
    delegate nint MethodImplRet1FpArg4(IntPtr self, IntPtr cmd, double arg1, double arg2, double arg3, double arg4);
    delegate nint MethodImplRet1FpArg5(IntPtr self, IntPtr cmd, double arg1, double arg2, double arg3, double arg4, double arg5);
    delegate double MethodImplFpRet1FpArg1(IntPtr self, IntPtr cmd, double arg1);
    delegate double MethodImplFpRet1FpArg2(IntPtr self, IntPtr cmd, double arg1, double arg2);
    delegate double MethodImplFpRet1FpArg3(IntPtr self, IntPtr cmd, double arg1, double arg2, double arg3);
    delegate double MethodImplFpRet1FpArg4(IntPtr self, IntPtr cmd, double arg1, double arg2, double arg3, double arg4);
    delegate double MethodImplFpRet1FpArg5(IntPtr self, IntPtr cmd, double arg1, double arg2, double arg3, double arg4, double arg5);
    delegate NativeResult2 MethodImplRet2FpArg1(IntPtr self, IntPtr cmd, double arg1);
    delegate NativeResult2 MethodImplRet2FpArg2(IntPtr self, IntPtr cmd, double arg1, double arg2);
    delegate NativeResult2 MethodImplRet2FpArg3(IntPtr self, IntPtr cmd, double arg1, double arg2, double arg3);
    delegate NativeResult2 MethodImplRet2FpArg4(IntPtr self, IntPtr cmd, double arg1, double arg2, double arg3, double arg4);
    delegate NativeResult2 MethodImplRet2FpArg5(IntPtr self, IntPtr cmd, double arg1, double arg2, double arg3, double arg4, double arg5);
    delegate NativeFpResult2 MethodImplFpRet2FpArg1(IntPtr self, IntPtr cmd, double arg1);
    delegate NativeFpResult2 MethodImplFpRet2FpArg2(IntPtr self, IntPtr cmd, double arg1, double arg2);
    delegate NativeFpResult2 MethodImplFpRet2FpArg3(IntPtr self, IntPtr cmd, double arg1, double arg2, double arg3);
    delegate NativeFpResult2 MethodImplFpRet2FpArg4(IntPtr self, IntPtr cmd, double arg1, double arg2, double arg3, double arg4);
    delegate NativeFpResult2 MethodImplFpRet2FpArg5(IntPtr self, IntPtr cmd, double arg1, double arg2, double arg3, double arg4, double arg5);
}