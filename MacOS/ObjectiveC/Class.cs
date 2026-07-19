using CarinaStudio.MacOS.Ffi;
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
        static extern bool class_addMethod(IntPtr cls, IntPtr name, IntPtr imp, string types);
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


        // Context of method implementation which is dispatched through libffi closure.
        class MethodImplContext(Delegate impl, string label, Type[] nativeParamTypes, Type? nativeReturnType, Type[] paramTypes)
        {
            public readonly Delegate Implementation = impl;
            public readonly string Label = label;
            public readonly Type[] NativeParameterTypes = nativeParamTypes;
            public readonly Type? NativeReturnType = nativeReturnType;
            public readonly Type[] ParameterTypes = paramTypes;
        }


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
        readonly IDictionary<string, Variable> cachedCVars = new ConcurrentDictionary<string, Variable>();
        readonly IDictionary<string, Variable> cachedIVars = new ConcurrentDictionary<string, Variable>();
        readonly IDictionary<Selector, Method> cachedMethods = new ConcurrentDictionary<Selector, Method>();
        readonly IDictionary<string, Property> cachedProperties = new ConcurrentDictionary<string, Property>();
        internal Variable? clrObjectHandleVar;
        bool isRegistered;
        volatile bool isRootClass;
        readonly IList<object> nativeBridgeMethods;
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
            this.Name = name;
            this.nativeBridgeMethods = isCustomDefined ? new List<object>() : Array.Empty<object>();
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
        public IntPtr Allocate()
        {
#pragma warning disable IL3050
            return NSObject.SendMessage<IntPtr>(this.Handle, AllocSelector!);
#pragma warning restore IL3050
        }


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
                    var candidateName = $"{clrObjHandleVarName}_{i}";
                    if (!cls.cachedIVars.ContainsKey(candidateName))
                    {
                        cls.clrObjectHandleVar = cls.DefineInstanceVariable<IntPtr>(candidateName);
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
        public Variable DefineInstanceVariable<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.NonPublicFields)] T>(string name, int elementCount = 1) =>
            this.DefineInstanceVariable(name, typeof(T), elementCount);


        /// <summary>
        /// Define instance variable.
        /// </summary>
        /// <param name="name">Name of variable.</param>
        /// <param name="type">Type of variable.</param>
        /// <param name="elementCount">Number of element is value is an array.</param>
        /// <returns>Descriptor of instance variable.</returns>
        public Variable DefineInstanceVariable(string name, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.NonPublicFields)] Type type, int elementCount = 1)
        {
            this.VerifyRegistered();
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException($"Invalid name: {name}.");
            if (elementCount <= 0)
                throw new ArgumentOutOfRangeException(nameof(elementCount));
            if (type.IsClass)
            {
                var typeToCheck = type.IsArray ? type.GetElementType()! : type;
                if (typeToCheck.IsClass
                    && !typeof(NSObject).IsAssignableFrom(typeToCheck)
                    && typeToCheck != typeof(Class)
                    && typeToCheck != typeof(Selector))
                {
                    throw new NotSupportedException("Variable with CLR object or CLR object array is unsupported. Only NSObject, Class and Selector are supported.");
                }
            }
            var typeEncoding = NativeTypeConversion.ToTypeEncoding(type, elementCount);
            var dataSize = Global.Run(() =>
            {
                if (type.IsArray)
                {
                    if (type.GetArrayRank() > 1)
                        throw new ArgumentException("Only 1-dimensional array is supported.");
                    return elementCount * NativeTypeConversion.GetNativeValueSize(type.GetElementType()!);
                }
                return NativeTypeConversion.GetNativeValueSize(type);
            });
            byte alignment = IntPtr.Size switch
            {
                4 => 2,
                8 => 3,
                16 => 4,
                _ => throw new NotSupportedException($"Unsupported size of pointer: {IntPtr.Size}."),
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
        /// <param name="name">Name of method.</param>
        /// <param name="implementation">Implementation.</param>
        public void DefineMethod(string name, Action<IntPtr, Selector> implementation) =>
            this.DefineMethodCore(Selector.FromName(name), implementation, false);


        /// <summary>
        /// Define method without argument.
        /// </summary>
        /// <param name="name">Name of method.</param>
        /// <param name="implementation">Implementation.</param>
        public void DefineMethod(Selector name, Action<IntPtr, Selector> implementation) =>
            this.DefineMethodCore(name, implementation, false);
        

        /// <summary>
        /// Define method without argument.
        /// </summary>
        /// <param name="name">Name of method.</param>
        /// <param name="implementation">Implementation.</param>
        /// <remarks>If trimming or Native AOT compilation is enabled, fields of custom structure type which is passed to or returned from the implementation must be kept explicitly, e.g. by <see cref="System.Diagnostics.CodeAnalysis.DynamicDependencyAttribute"/>.</remarks>
        public void DefineMethod<R>(string name, Func<IntPtr, Selector, R> implementation) =>
            this.DefineMethodCore(Selector.FromName(name), implementation, false);


        /// <summary>
        /// Define method without argument.
        /// </summary>
        /// <param name="name">Name of method.</param>
        /// <param name="implementation">Implementation.</param>
        /// <remarks>If trimming or Native AOT compilation is enabled, fields of custom structure type which is passed to or returned from the implementation must be kept explicitly, e.g. by <see cref="System.Diagnostics.CodeAnalysis.DynamicDependencyAttribute"/>.</remarks>
        public void DefineMethod<R>(Selector name, Func<IntPtr, Selector, R> implementation) =>
            this.DefineMethodCore(name, implementation, false);


        /// <summary>
        /// Define method with 1 argument.
        /// </summary>
        /// <param name="name">Name of method.</param>
        /// <param name="implementation">Implementation.</param>
        /// <remarks>If trimming or Native AOT compilation is enabled, fields of custom structure type which is passed to or returned from the implementation must be kept explicitly, e.g. by <see cref="System.Diagnostics.CodeAnalysis.DynamicDependencyAttribute"/>.</remarks>
        public void DefineMethod<TArg1>(string name, Action<IntPtr, Selector, TArg1> implementation) =>
            this.DefineMethodCore(Selector.FromName(name), implementation, false);


        /// <summary>
        /// Define method with 1 argument.
        /// </summary>
        /// <param name="name">Name of method.</param>
        /// <param name="implementation">Implementation.</param>
        /// <remarks>If trimming or Native AOT compilation is enabled, fields of custom structure type which is passed to or returned from the implementation must be kept explicitly, e.g. by <see cref="System.Diagnostics.CodeAnalysis.DynamicDependencyAttribute"/>.</remarks>
        public void DefineMethod<TArg1>(Selector name, Action<IntPtr, Selector, TArg1> implementation) =>
            this.DefineMethodCore(name, implementation, false);


        /// <summary>
        /// Define method with 1 argument.
        /// </summary>
        /// <param name="name">Name of method.</param>
        /// <param name="implementation">Implementation.</param>
        /// <remarks>If trimming or Native AOT compilation is enabled, fields of custom structure type which is passed to or returned from the implementation must be kept explicitly, e.g. by <see cref="System.Diagnostics.CodeAnalysis.DynamicDependencyAttribute"/>.</remarks>
        public void DefineMethod<TArg1, R>(string name, Func<IntPtr, Selector, TArg1, R> implementation) =>
            this.DefineMethodCore(Selector.FromName(name), implementation, false);


        /// <summary>
        /// Define method with 1 argument.
        /// </summary>
        /// <param name="name">Name of method.</param>
        /// <param name="implementation">Implementation.</param>
        /// <remarks>If trimming or Native AOT compilation is enabled, fields of custom structure type which is passed to or returned from the implementation must be kept explicitly, e.g. by <see cref="System.Diagnostics.CodeAnalysis.DynamicDependencyAttribute"/>.</remarks>
        public void DefineMethod<TArg1, R>(Selector name, Func<IntPtr, Selector, TArg1, R> implementation) =>
            this.DefineMethodCore(name, implementation, false);


        /// <summary>
        /// Define method with 2 arguments.
        /// </summary>
        /// <param name="name">Name of method.</param>
        /// <param name="implementation">Implementation.</param>
        /// <remarks>If trimming or Native AOT compilation is enabled, fields of custom structure type which is passed to or returned from the implementation must be kept explicitly, e.g. by <see cref="System.Diagnostics.CodeAnalysis.DynamicDependencyAttribute"/>.</remarks>
        public void DefineMethod<TArg1, TArg2>(string name, Action<IntPtr, Selector, TArg1, TArg2> implementation) =>
            this.DefineMethodCore(Selector.FromName(name), implementation, false);


        /// <summary>
        /// Define method with 2 arguments.
        /// </summary>
        /// <param name="name">Name of method.</param>
        /// <param name="implementation">Implementation.</param>
        /// <remarks>If trimming or Native AOT compilation is enabled, fields of custom structure type which is passed to or returned from the implementation must be kept explicitly, e.g. by <see cref="System.Diagnostics.CodeAnalysis.DynamicDependencyAttribute"/>.</remarks>
        public void DefineMethod<TArg1, TArg2>(Selector name, Action<IntPtr, Selector, TArg1, TArg2> implementation) =>
            this.DefineMethodCore(name, implementation, false);


        /// <summary>
        /// Define method with 2 arguments.
        /// </summary>
        /// <param name="name">Name of method.</param>
        /// <param name="implementation">Implementation.</param>
        /// <remarks>If trimming or Native AOT compilation is enabled, fields of custom structure type which is passed to or returned from the implementation must be kept explicitly, e.g. by <see cref="System.Diagnostics.CodeAnalysis.DynamicDependencyAttribute"/>.</remarks>
        public void DefineMethod<TArg1, TArg2, R>(string name, Func<IntPtr, Selector, TArg1, TArg2, R> implementation) =>
            this.DefineMethodCore(Selector.FromName(name), implementation, false);


        /// <summary>
        /// Define method with 2 arguments.
        /// </summary>
        /// <param name="name">Name of method.</param>
        /// <param name="implementation">Implementation.</param>
        /// <remarks>If trimming or Native AOT compilation is enabled, fields of custom structure type which is passed to or returned from the implementation must be kept explicitly, e.g. by <see cref="System.Diagnostics.CodeAnalysis.DynamicDependencyAttribute"/>.</remarks>
        public void DefineMethod<TArg1, TArg2, R>(Selector name, Func<IntPtr, Selector, TArg1, TArg2, R> implementation) =>
            this.DefineMethodCore(name, implementation, false);


        /// <summary>
        /// Define method with 3 arguments.
        /// </summary>
        /// <param name="name">Name of method.</param>
        /// <param name="implementation">Implementation.</param>
        /// <remarks>If trimming or Native AOT compilation is enabled, fields of custom structure type which is passed to or returned from the implementation must be kept explicitly, e.g. by <see cref="System.Diagnostics.CodeAnalysis.DynamicDependencyAttribute"/>.</remarks>
        public void DefineMethod<TArg1, TArg2, TArg3>(string name, Action<IntPtr, Selector, TArg1, TArg2, TArg3> implementation) =>
            this.DefineMethodCore(Selector.FromName(name), implementation, false);


        /// <summary>
        /// Define method with 3 arguments.
        /// </summary>
        /// <param name="name">Name of method.</param>
        /// <param name="implementation">Implementation.</param>
        /// <remarks>If trimming or Native AOT compilation is enabled, fields of custom structure type which is passed to or returned from the implementation must be kept explicitly, e.g. by <see cref="System.Diagnostics.CodeAnalysis.DynamicDependencyAttribute"/>.</remarks>
        public void DefineMethod<TArg1, TArg2, TArg3>(Selector name, Action<IntPtr, Selector, TArg1, TArg2, TArg3> implementation) =>
            this.DefineMethodCore(name, implementation, false);


        /// <summary>
        /// Define method with 3 arguments.
        /// </summary>
        /// <param name="name">Name of method.</param>
        /// <param name="implementation">Implementation.</param>
        /// <remarks>If trimming or Native AOT compilation is enabled, fields of custom structure type which is passed to or returned from the implementation must be kept explicitly, e.g. by <see cref="System.Diagnostics.CodeAnalysis.DynamicDependencyAttribute"/>.</remarks>
        public void DefineMethod<TArg1, TArg2, TArg3, R>(string name, Func<IntPtr, Selector, TArg1, TArg2, TArg3, R> implementation) =>
            this.DefineMethodCore(Selector.FromName(name), implementation, false);


        /// <summary>
        /// Define method with 3 arguments.
        /// </summary>
        /// <param name="name">Name of method.</param>
        /// <param name="implementation">Implementation.</param>
        /// <remarks>If trimming or Native AOT compilation is enabled, fields of custom structure type which is passed to or returned from the implementation must be kept explicitly, e.g. by <see cref="System.Diagnostics.CodeAnalysis.DynamicDependencyAttribute"/>.</remarks>
        public void DefineMethod<TArg1, TArg2, TArg3, R>(Selector name, Func<IntPtr, Selector, TArg1, TArg2, TArg3, R> implementation) =>
            this.DefineMethodCore(name, implementation, false);


        /// <summary>
        /// Define method with 4 arguments.
        /// </summary>
        /// <param name="name">Name of method.</param>
        /// <param name="implementation">Implementation.</param>
        /// <remarks>If trimming or Native AOT compilation is enabled, fields of custom structure type which is passed to or returned from the implementation must be kept explicitly, e.g. by <see cref="System.Diagnostics.CodeAnalysis.DynamicDependencyAttribute"/>.</remarks>
        public void DefineMethod<TArg1, TArg2, TArg3, TArg4>(string name, Action<IntPtr, Selector, TArg1, TArg2, TArg3, TArg4> implementation) =>
            this.DefineMethodCore(Selector.FromName(name), implementation, false);


        /// <summary>
        /// Define method with 4 arguments.
        /// </summary>
        /// <param name="name">Name of method.</param>
        /// <param name="implementation">Implementation.</param>
        /// <remarks>If trimming or Native AOT compilation is enabled, fields of custom structure type which is passed to or returned from the implementation must be kept explicitly, e.g. by <see cref="System.Diagnostics.CodeAnalysis.DynamicDependencyAttribute"/>.</remarks>
        public void DefineMethod<TArg1, TArg2, TArg3, TArg4>(Selector name, Action<IntPtr, Selector, TArg1, TArg2, TArg3, TArg4> implementation) =>
            this.DefineMethodCore(name, implementation, false);


        /// <summary>
        /// Define method with 4 arguments.
        /// </summary>
        /// <param name="name">Name of method.</param>
        /// <param name="implementation">Implementation.</param>
        /// <remarks>If trimming or Native AOT compilation is enabled, fields of custom structure type which is passed to or returned from the implementation must be kept explicitly, e.g. by <see cref="System.Diagnostics.CodeAnalysis.DynamicDependencyAttribute"/>.</remarks>
        public void DefineMethod<TArg1, TArg2, TArg3, TArg4, R>(string name, Func<IntPtr, Selector, TArg1, TArg2, TArg3, TArg4, R> implementation) =>
            this.DefineMethodCore(Selector.FromName(name), implementation, false);


        /// <summary>
        /// Define method with 4 arguments.
        /// </summary>
        /// <param name="name">Name of method.</param>
        /// <param name="implementation">Implementation.</param>
        /// <remarks>If trimming or Native AOT compilation is enabled, fields of custom structure type which is passed to or returned from the implementation must be kept explicitly, e.g. by <see cref="System.Diagnostics.CodeAnalysis.DynamicDependencyAttribute"/>.</remarks>
        public void DefineMethod<TArg1, TArg2, TArg3, TArg4, R>(Selector name, Func<IntPtr, Selector, TArg1, TArg2, TArg3, TArg4, R> implementation) =>
            this.DefineMethodCore(name, implementation, false);


        /// <summary>
        /// Define method with 5 arguments.
        /// </summary>
        /// <param name="name">Name of method.</param>
        /// <param name="implementation">Implementation.</param>
        /// <remarks>If trimming or Native AOT compilation is enabled, fields of custom structure type which is passed to or returned from the implementation must be kept explicitly, e.g. by <see cref="System.Diagnostics.CodeAnalysis.DynamicDependencyAttribute"/>.</remarks>
        public void DefineMethod<TArg1, TArg2, TArg3, TArg4, TArg5>(string name, Action<IntPtr, Selector, TArg1, TArg2, TArg3, TArg4, TArg5> implementation) =>
            this.DefineMethodCore(Selector.FromName(name), implementation, false);


        /// <summary>
        /// Define method with 5 arguments.
        /// </summary>
        /// <param name="name">Name of method.</param>
        /// <param name="implementation">Implementation.</param>
        /// <remarks>If trimming or Native AOT compilation is enabled, fields of custom structure type which is passed to or returned from the implementation must be kept explicitly, e.g. by <see cref="System.Diagnostics.CodeAnalysis.DynamicDependencyAttribute"/>.</remarks>
        public void DefineMethod<TArg1, TArg2, TArg3, TArg4, TArg5>(Selector name, Action<IntPtr, Selector, TArg1, TArg2, TArg3, TArg4, TArg5> implementation) =>
            this.DefineMethodCore(name, implementation, false);


        /// <summary>
        /// Define method with 5 arguments.
        /// </summary>
        /// <param name="name">Name of method.</param>
        /// <param name="implementation">Implementation.</param>
        /// <remarks>If trimming or Native AOT compilation is enabled, fields of custom structure type which is passed to or returned from the implementation must be kept explicitly, e.g. by <see cref="System.Diagnostics.CodeAnalysis.DynamicDependencyAttribute"/>.</remarks>
        public void DefineMethod<TArg1, TArg2, TArg3, TArg4, TArg5, R>(string name, Func<IntPtr, Selector, TArg1, TArg2, TArg3, TArg4, TArg5, R> implementation) =>
            this.DefineMethodCore(Selector.FromName(name), implementation, false);


        /// <summary>
        /// Define method with 5 arguments.
        /// </summary>
        /// <param name="name">Name of method.</param>
        /// <param name="implementation">Implementation.</param>
        /// <remarks>If trimming or Native AOT compilation is enabled, fields of custom structure type which is passed to or returned from the implementation must be kept explicitly, e.g. by <see cref="System.Diagnostics.CodeAnalysis.DynamicDependencyAttribute"/>.</remarks>
        public void DefineMethod<TArg1, TArg2, TArg3, TArg4, TArg5, R>(Selector name, Func<IntPtr, Selector, TArg1, TArg2, TArg3, TArg4, TArg5, R> implementation) =>
            this.DefineMethodCore(name, implementation, false);
        

        /// <summary>
        /// Define method.
        /// </summary>
        /// <param name="name">Name of method.</param>
        /// <param name="implementation">Implementation.</param>
        /// <remarks>If trimming or Native AOT compilation is enabled, fields of custom structure type which is passed to or returned from the implementation must be kept explicitly, e.g. by <see cref="System.Diagnostics.CodeAnalysis.DynamicDependencyAttribute"/>.</remarks>
        public void DefineMethod(Selector name, Delegate implementation) =>
            this.DefineMethodCore(name, implementation, true);


        // Define method.
        void DefineMethodCore(Selector name, Delegate impl, bool verifyImpl)
        {
            // verify implementation
            if (verifyImpl)
                this.VerifyMethodImplementation(impl);
            
            // convert to native types
            var invokeMethod = impl.GetType().GetMethod("Invoke").AsNonNull();
            var parameters = invokeMethod.GetParameters();
            var paramCount = parameters.Length;
            var paramTypes = new Type[paramCount].Also(it =>
            {
                for (var i = paramCount - 1; i >= 0; --i)
                    it[i] = parameters[i].ParameterType;
            });
            var nativeParamTypes = new Type[paramCount].Also(it =>
            {
                for (var i = paramCount - 1; i >= 0; --i)
                    it[i] = NativeTypeConversion.ToNativeType(paramTypes[i]);
            });
            var returnType = invokeMethod.ReturnType.Let(it =>
            {
                if (it == typeof(void))
                    return null;
                return it;
            });
            var nativeReturnType = returnType != null ? NativeTypeConversion.ToNativeType(returnType) : null;

            // create libffi closure to dispatch calls to implementation
            var cif = LibFfi.CreateCif(nativeReturnType, nativeParamTypes, out _, out _);
            var context = new MethodImplContext(impl, $"{this.Name}.{name.Name}", nativeParamTypes, nativeReturnType, paramTypes);
            var contextHandle = GCHandle.Alloc(context); // keep context alive for process lifetime
            void* closureCode;
            try
            {
                LibFfi.CreateClosure(cif, (delegate* unmanaged<void*, void*, void**, void*, void>)&InvokeMethodImpl, GCHandle.ToIntPtr(contextHandle), out closureCode);
            }
            catch
            {
                contextHandle.Free();
                throw;
            }

            // add method
            if (!class_addMethod(this.Handle, name.Handle, (IntPtr)closureCode, ""))
                throw new Exception($"Failed to add method '{name}' to '{this.Name}'.");
            this.nativeBridgeMethods.Add(context);
        }


        /// <summary>
        /// Add new property to this class.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="getter">Implementation of getter.</param>
        /// <param name="setter">Implementation of setter.</param>
        /// <typeparam name="T">Type of property value.</typeparam>
        /// <returns>Descriptor of added property.</returns>
        /// <remarks>If trimming or Native AOT compilation is enabled, fields of custom structure type which is passed to or returned from the implementation must be kept explicitly, e.g. by <see cref="System.Diagnostics.CodeAnalysis.DynamicDependencyAttribute"/>.</remarks>
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
                throw new ArgumentException($"Unable to add property '{name}' to '{this.Name}'.");
            var property = this.GetProperty(name) 
                ?? throw new ArgumentException($"Unable to add property '{name}' to '{this.Name}'.");

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
            return property;
        }
        

        /// <inheritdoc/>
        public bool Equals(Class? cls)
        {
            if (cls == null)
                return false;
            if (ReferenceEquals(cls, this))
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
        public static Class FromHandle(IntPtr handle)
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
        public Variable? GetClassVariable(string name)
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
        public Variable? GetInstanceVariable(string name)
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
        public Method? GetMethod(Selector selector)
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
        public Method[] GetMethods()
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
        public Property[] GetProperties()
        {
            if (areAllPropertiesCached)
                return this.cachedProperties.Values.ToArray();
            var propertiesPtr = this.IsProtocol
                ? protocol_copyPropertyList(this.Handle, out var count)
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
        public Property? GetProperty(string name)
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


        // Entry of methods defined through DefineMethod, called by libffi closure.
        [UnconditionalSuppressMessage("Trimming", "IL2062", Justification = "Structure types used with native methods are expected to have fields preserved by caller.")]
        [UnmanagedCallersOnly]
        static void InvokeMethodImpl(void* cif, void* returnBuffer, void** args, void* userData)
        {
            MethodImplContext? context = null;
            try
            {
                // get context
                context = (MethodImplContext)GCHandle.FromIntPtr((IntPtr)userData).Target.AsNonNull();
                var paramTypes = context.ParameterTypes;
                var nativeParamTypes = context.NativeParameterTypes;
                var paramCount = paramTypes.Length;

                // convert arguments
                var implArgs = new object?[paramCount];
                implArgs[0] = *(IntPtr*)args[0];
                implArgs[1] = Selector.FromHandle(*(IntPtr*)args[1]);
                for (var i = 2; i < paramCount; ++i)
                {
                    var nativeValue = NativeTypeConversion.FromNativeValue((byte*)args[i], NativeTypeConversion.GetNativeValueSize(nativeParamTypes[i]), nativeParamTypes[i], out _);
                    implArgs[i] = nativeValue is not null ? NativeTypeConversion.FromNativeParameter(nativeValue, paramTypes[i]) : null;
                }

                // invoke implementation
                var result = context.Implementation.DynamicInvoke(implArgs);

                // convert return value
                if (context.NativeReturnType is not null)
                    WriteNativeReturnValue(NativeTypeConversion.ToNativeParameter(result), returnBuffer);
            }
            catch (Exception ex)
            {
                Environment.FailFast($"Unhandled exception in implementation of '{context?.Label}'.", ex);
            }
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
        public bool TryGetClrObject<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)] T>(IntPtr obj, [NotNullWhen(true)] out T? clrObj)
        {
            clrObj = default;
            if (this.clrObjectHandleVar == null
                || !this.IsAssignableFrom(GetClass(obj)))
            {
                return false;
            }
            var handle = NSObject.GetVariable<IntPtr>(obj, this.clrObjectHandleVar);
            var gcHandle = handle != default ? GCHandle.FromIntPtr(handle) : default;
            var rawClrObj = gcHandle.IsAllocated ? gcHandle.Target : null;
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
                var gcHandle = it != default ? GCHandle.FromIntPtr(it) : default;
                if (gcHandle.IsAllocated)
                    gcHandle.Free();
            });
            NSObject.SetVariable(obj, this.clrObjectHandleVar, clrObj != null ? GCHandle.ToIntPtr(GCHandle.Alloc(clrObj)) : default);
            return true;
        }


        // Throw exception if implementation is invalid.
        void VerifyMethodImplementation(Delegate implementation)
        {
            var invokeMethod = implementation.GetType().GetMethod("Invoke");
            if (invokeMethod == null)
                throw new ArgumentException("Invalid implementation.");
            var parameters = invokeMethod.GetParameters();
            if (parameters.Length < 2)
                throw new ArgumentException("At least 2 parameters are needed.");
            if (parameters[0].ParameterType != typeof(IntPtr))
                throw new ArgumentException("The first parameter must be IntPtr.");
            if (parameters[1].ParameterType != typeof(Selector))
                throw new ArgumentException("The second parameter must be Selector.");
        }


        // Throw exception if class has been registered.
        void VerifyRegistered()
        {
            if (this.isRegistered)
                throw new InvalidOperationException($"Cannot define member of class '{this.Name}' after registering to Objective-C runtime.");
        }


        // Write native value returned from method implementation into return buffer of libffi closure.
        static void WriteNativeReturnValue(object nativeValue, void* returnBuffer)
        {
            if (nativeValue is Enum)
                nativeValue = Convert.ChangeType(nativeValue, Enum.GetUnderlyingType(nativeValue.GetType()));
            switch (nativeValue)
            {
                // integral values narrower than ffi_arg must be widened to full ffi_arg
                case bool boolValue: // Objective-C BOOL is signed char
                    *(long*)returnBuffer = boolValue ? 1 : 0;
                    break;
                case sbyte sbyteValue:
                    *(long*)returnBuffer = sbyteValue;
                    break;
                case short shortValue:
                    *(long*)returnBuffer = shortValue;
                    break;
                case int intValue:
                    *(long*)returnBuffer = intValue;
                    break;
                case byte byteValue:
                    *(ulong*)returnBuffer = byteValue;
                    break;
                case ushort ushortValue:
                    *(ulong*)returnBuffer = ushortValue;
                    break;
                case char charValue:
                    *(ulong*)returnBuffer = charValue;
                    break;
                case uint uintValue:
                    *(ulong*)returnBuffer = uintValue;
                    break;
                case long longValue:
                    *(long*)returnBuffer = longValue;
                    break;
                case ulong ulongValue:
                    *(ulong*)returnBuffer = ulongValue;
                    break;
                case nint nintValue:
                    *(nint*)returnBuffer = nintValue;
                    break;
                case float floatValue:
                    *(float*)returnBuffer = floatValue;
                    break;
                case double doubleValue:
                    *(double*)returnBuffer = doubleValue;
                    break;
                case ValueType:
                    NativeStructureLayout.Get(nativeValue.GetType()).Write(nativeValue, (byte*)returnBuffer);
                    break;
                default:
                    throw new NotSupportedException($"Cannot convert {nativeValue.GetType().Name} to native return value.");
            }
        }
    }
}