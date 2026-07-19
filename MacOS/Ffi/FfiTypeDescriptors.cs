using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace CarinaStudio.MacOS.Ffi;

// Mapping from CLR native types to libffi type descriptors.
static unsafe class FfiTypeDescriptors
{
    // Cached descriptors of structure types. Descriptors are allocated in native heap and kept for process lifetime.
    static readonly Dictionary<Type, IntPtr> StructureDescriptors = new();


    // Get libffi type descriptor for given CLR native type.
    public static FfiType* Get(Type nativeType)
    {
        if (nativeType.IsEnum)
#pragma warning disable IL2072
            nativeType = nativeType.GetEnumUnderlyingType();
#pragma warning restore IL2072
        if (nativeType == typeof(nint) || nativeType == typeof(nuint))
            return LibFfi.TypePointer;
        if (nativeType == typeof(bool) || nativeType == typeof(sbyte)) // Objective-C BOOL is signed char
            return LibFfi.TypeSInt8;
        if (nativeType == typeof(byte))
            return LibFfi.TypeUInt8;
        if (nativeType == typeof(short))
            return LibFfi.TypeSInt16;
        if (nativeType == typeof(ushort) || nativeType == typeof(char))
            return LibFfi.TypeUInt16;
        if (nativeType == typeof(int))
            return LibFfi.TypeSInt32;
        if (nativeType == typeof(uint))
            return LibFfi.TypeUInt32;
        if (nativeType == typeof(long))
            return LibFfi.TypeSInt64;
        if (nativeType == typeof(ulong))
            return LibFfi.TypeUInt64;
        if (nativeType == typeof(float))
            return LibFfi.TypeFloat;
        if (nativeType == typeof(double))
            return LibFfi.TypeDouble;
        if (nativeType.IsValueType)
            return GetStructure(nativeType);
        throw new NotSupportedException($"Cannot map {nativeType.Name} to libffi type.");
    }


    // Get or create descriptor for structure type. Size and alignment of created descriptor are filled by libffi when preparing call interface.
    static FfiType* GetStructure(Type type)
    {
        lock (StructureDescriptors)
        {
            // use existing descriptor
            if (StructureDescriptors.TryGetValue(type, out var existingDescriptor))
                return (FfiType*)existingDescriptor;

            // count elements including expanded fixed-size arrays
            var layout = NativeStructureLayout.Get(type);
            var elementCount = 0;
            foreach (var fieldLayout in layout.Fields)
                elementCount += fieldLayout.ArrayLength > 0 ? fieldLayout.ArrayLength : 1;

            // build descriptor
            var elements = (FfiType**)NativeMemory.Alloc((nuint)(elementCount + 1), (nuint)sizeof(FfiType*));
            var elementIndex = 0;
            foreach (var fieldLayout in layout.Fields)
            {
                var elementFfiType = Get(fieldLayout.ElementType);
                if (fieldLayout.ArrayLength > 0)
                {
                    for (var i = fieldLayout.ArrayLength; i > 0; --i)
                        elements[elementIndex++] = elementFfiType;
                }
                else
                    elements[elementIndex++] = elementFfiType;
            }
            elements[elementIndex] = null;
            var descriptor = (FfiType*)NativeMemory.AllocZeroed((nuint)sizeof(FfiType));
            descriptor->Type = LibFfi.StructureTypeCode;
            descriptor->Elements = elements;
            StructureDescriptors.Add(type, (IntPtr)descriptor);
            return descriptor;
        }
    }
}
