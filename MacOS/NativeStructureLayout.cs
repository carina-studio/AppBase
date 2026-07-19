using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace CarinaStudio.MacOS;

// Native memory layout of structure type which supports reading/writing values without runtime marshalling support.
unsafe class NativeStructureLayout
{
    // Layout of single field of structure.
    public class FieldLayout(int arrayLength, Type elementType, FieldInfo field, int offset)
    {
        public readonly int ArrayLength = arrayLength; // 0 for non-array field
        public readonly Type ElementType = elementType; // type of element for array field, or type of field
        public readonly FieldInfo Field = field;
        public readonly int Offset = offset;
    }


    // Static fields.
    static readonly ConcurrentDictionary<Type, NativeStructureLayout> CachedLayouts = new();


    // Fields.
    public readonly int Alignment;
    public readonly FieldLayout[] Fields;
    public readonly int Size;
    public readonly Type Type;


    // Constructor.
    NativeStructureLayout([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.NonPublicFields)] Type type)
    {
        var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic); // fields are returned in declaration order
        if (fields.Length == 0)
            throw new NotSupportedException($"Cannot compute native layout of structure without instance field: {type.Name}.");
        var fieldLayouts = new FieldLayout[fields.Length];
        var offset = 0;
        var maxAlignment = 1;
        for (var i = 0; i < fields.Length; ++i)
        {
            var field = fields[i];
            var fieldType = field.FieldType;
            var arrayLength = 0;
            if (fieldType.IsArray)
            {
                var marshalAsAttr = field.GetCustomAttribute<MarshalAsAttribute>();
                if (marshalAsAttr is null || marshalAsAttr.Value != UnmanagedType.ByValArray || marshalAsAttr.SizeConst <= 0)
                    throw new NotSupportedException($"Array field without fixed size is unsupported: {type.Name}.{field.Name}.");
                arrayLength = marshalAsAttr.SizeConst;
                fieldType = fieldType.GetElementType().AsNonNull();
            }
            var alignment = GetValueAlignment(fieldType);
            var size = GetValueSize(fieldType);
            offset = (offset + alignment - 1) / alignment * alignment;
            fieldLayouts[i] = new(arrayLength, fieldType, field, offset);
            offset += arrayLength > 0 ? size * arrayLength : size;
            if (alignment > maxAlignment)
                maxAlignment = alignment;
        }
        this.Alignment = maxAlignment;
        this.Fields = fieldLayouts;
        this.Size = (offset + maxAlignment - 1) / maxAlignment * maxAlignment;
        this.Type = type;
    }


    // Get or create layout of given structure type. Fields of structure types which are passed to or
    // returned from native methods are expected to be preserved by caller when trimming is enabled.
    [UnconditionalSuppressMessage("Trimming", "IL2067", Justification = "Structure types used with native methods are expected to have fields preserved by caller.")]
    public static NativeStructureLayout Get(Type type)
    {
        if (!type.IsValueType || type.IsEnum || type.IsPrimitive)
            throw new NotSupportedException($"Cannot compute native layout of {type.Name}.");
        if (CachedLayouts.TryGetValue(type, out var layout))
            return layout;
        return CachedLayouts.GetOrAdd(type, new NativeStructureLayout(type));
    }


    // Get alignment of native value of given type in bytes.
    static int GetValueAlignment(Type type)
    {
        if (type.IsEnum || type.IsPrimitive || type == typeof(nint) || type == typeof(nuint))
            return GetValueSize(type);
        if (type.IsValueType)
            return Get(type).Alignment;
        throw new NotSupportedException($"Cannot get alignment of native value of {type.Name}.");
    }


    // Get size of native value of given type in bytes.
    static int GetValueSize(Type type)
    {
        if (type.IsEnum)
            type = type.GetEnumUnderlyingType();
        if (type == typeof(bool) || type == typeof(byte) || type == typeof(sbyte)) // Objective-C BOOL is signed char
            return 1;
        if (type == typeof(short) || type == typeof(ushort) || type == typeof(char))
            return 2;
        if (type == typeof(int) || type == typeof(uint) || type == typeof(float))
            return 4;
        if (type == typeof(long) || type == typeof(ulong) || type == typeof(double))
            return 8;
        if (type == typeof(nint) || type == typeof(nuint))
            return IntPtr.Size;
        if (type.IsValueType)
            return Get(type).Size;
        throw new NotSupportedException($"Cannot get size of native value of {type.Name}.");
    }


    // Read structure from native memory and box as CLR value.
    [UnconditionalSuppressMessage("Trimming", "IL2077", Justification = "Value types are created without using constructor.")]
    public object Read(byte* valuePtr)
    {
        var structure = RuntimeHelpers.GetUninitializedObject(this.Type);
        foreach (var fieldLayout in this.Fields)
        {
            var elementType = fieldLayout.ElementType;
            if (fieldLayout.ArrayLength > 0)
            {
                var elementSize = GetValueSize(elementType);
#pragma warning disable IL3050
                var array = Array.CreateInstance(elementType, fieldLayout.ArrayLength); // element types of fixed-size array fields are expected to be referenced by caller
#pragma warning restore IL3050
                for (var i = 0; i < fieldLayout.ArrayLength; ++i)
                    array.SetValue(ReadValue(valuePtr + fieldLayout.Offset + elementSize * i, elementType), i);
                fieldLayout.Field.SetValue(structure, array);
            }
            else
                fieldLayout.Field.SetValue(structure, ReadValue(valuePtr + fieldLayout.Offset, elementType));
        }
        return structure;
    }


    // Read single value from native memory and box as CLR value.
    static object ReadValue(byte* valuePtr, Type type)
    {
        if (type.IsEnum)
            return Enum.ToObject(type, ReadValue(valuePtr, type.GetEnumUnderlyingType()));
        if (type == typeof(bool)) // Objective-C BOOL is signed char
            return *(sbyte*)valuePtr != 0;
        if (type == typeof(byte))
            return *valuePtr;
        if (type == typeof(sbyte))
            return *(sbyte*)valuePtr;
        if (type == typeof(short))
            return *(short*)valuePtr;
        if (type == typeof(ushort))
            return *(ushort*)valuePtr;
        if (type == typeof(char))
            return (char)*(ushort*)valuePtr;
        if (type == typeof(int))
            return *(int*)valuePtr;
        if (type == typeof(uint))
            return *(uint*)valuePtr;
        if (type == typeof(long))
            return *(long*)valuePtr;
        if (type == typeof(ulong))
            return *(ulong*)valuePtr;
        if (type == typeof(float))
            return *(float*)valuePtr;
        if (type == typeof(double))
            return *(double*)valuePtr;
        if (type == typeof(nint))
            return *(nint*)valuePtr;
        if (type == typeof(nuint))
            return *(nuint*)valuePtr;
        if (type.IsValueType)
            return Get(type).Read(valuePtr);
        throw new NotSupportedException($"Cannot read native value of {type.Name} from native memory.");
    }


    // Write structure into native memory.
    public void Write(object structure, byte* valuePtr)
    {
        foreach (var fieldLayout in this.Fields)
        {
            var elementType = fieldLayout.ElementType;
            var value = fieldLayout.Field.GetValue(structure);
            if (fieldLayout.ArrayLength > 0)
            {
                var elementSize = GetValueSize(elementType);
                var array = (Array?)value;
                if (array is not null && array.GetLength(0) != fieldLayout.ArrayLength)
                    throw new ArgumentException($"Size of array field should be {fieldLayout.ArrayLength}: {this.Type.Name}.{fieldLayout.Field.Name}.");
                for (var i = 0; i < fieldLayout.ArrayLength; ++i)
                    WriteValue(array?.GetValue(i), valuePtr + fieldLayout.Offset + elementSize * i, elementType);
            }
            else
                WriteValue(value, valuePtr + fieldLayout.Offset, elementType);
        }
    }


    // Write single value into native memory.
    static void WriteValue(object? value, byte* valuePtr, Type type)
    {
        if (type.IsEnum)
        {
            WriteValue(value is not null ? Convert.ChangeType(value, type.GetEnumUnderlyingType()) : null, valuePtr, type.GetEnumUnderlyingType());
            return;
        }
        if (type == typeof(bool)) // Objective-C BOOL is signed char
            *(sbyte*)valuePtr = (value is true) ? (sbyte)1 : (sbyte)0;
        else if (type == typeof(byte))
            *valuePtr = value is not null ? (byte)value : (byte)0;
        else if (type == typeof(sbyte))
            *(sbyte*)valuePtr = value is not null ? (sbyte)value : (sbyte)0;
        else if (type == typeof(short))
            *(short*)valuePtr = value is not null ? (short)value : (short)0;
        else if (type == typeof(ushort))
            *(ushort*)valuePtr = value is not null ? (ushort)value : (ushort)0;
        else if (type == typeof(char))
            *(ushort*)valuePtr = value is not null ? (char)value : (ushort)0;
        else if (type == typeof(int))
            *(int*)valuePtr = value is not null ? (int)value : 0;
        else if (type == typeof(uint))
            *(uint*)valuePtr = value is not null ? (uint)value : 0u;
        else if (type == typeof(long))
            *(long*)valuePtr = value is not null ? (long)value : 0L;
        else if (type == typeof(ulong))
            *(ulong*)valuePtr = value is not null ? (ulong)value : 0uL;
        else if (type == typeof(float))
            *(float*)valuePtr = value is not null ? (float)value : 0f;
        else if (type == typeof(double))
            *(double*)valuePtr = value is not null ? (double)value : 0.0;
        else if (type == typeof(nint))
            *(nint*)valuePtr = value is not null ? (nint)value : 0;
        else if (type == typeof(nuint))
            *(nuint*)valuePtr = value is not null ? (nuint)value : 0;
        else if (type.IsValueType && value is not null)
            Get(type).Write(value, valuePtr);
        else
            throw new NotSupportedException($"Cannot write native value of {type.Name} into native memory.");
    }
}
