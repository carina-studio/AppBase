using System;
using System.Collections.Generic;
using System.Reflection;
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

            // collect fields in layout order
#pragma warning disable IL2070
            var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
#pragma warning restore IL2070
            if (fields.Length == 0)
                throw new NotSupportedException($"Cannot create libffi type for structure without instance field: {type.Name}.");
            Array.Sort(fields, (l, r) =>
                ((long)Marshal.OffsetOf(type, l.Name)).CompareTo((long)Marshal.OffsetOf(type, r.Name)));

            // count elements including expanded fixed-size arrays
            var elementCount = 0;
            foreach (var field in fields)
            {
                if (field.FieldType.IsArray)
                {
                    var marshalAsAttr = field.GetCustomAttribute<MarshalAsAttribute>();
                    if (marshalAsAttr is null || marshalAsAttr.Value != UnmanagedType.ByValArray || marshalAsAttr.SizeConst <= 0)
                        throw new NotSupportedException($"Array field without fixed size is unsupported: {type.Name}.{field.Name}.");
                    elementCount += marshalAsAttr.SizeConst;
                }
                else
                    ++elementCount;
            }

            // build descriptor
            var elements = (FfiType**)NativeMemory.Alloc((nuint)(elementCount + 1), (nuint)sizeof(FfiType*));
            var elementIndex = 0;
            foreach (var field in fields)
            {
                var fieldType = field.FieldType;
                if (fieldType.IsArray)
                {
                    var elementFfiType = Get(fieldType.GetElementType()!);
                    var arrayLength = field.GetCustomAttribute<MarshalAsAttribute>()!.SizeConst;
                    for (var i = arrayLength; i > 0; --i)
                        elements[elementIndex++] = elementFfiType;
                }
                else
                    elements[elementIndex++] = Get(fieldType);
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
