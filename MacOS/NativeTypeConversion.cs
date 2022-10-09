using CarinaStudio.MacOS.CoreFoundation;
using CarinaStudio.MacOS.ObjectiveC;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace CarinaStudio.MacOS;

/// <summary>
/// Conversion between native and CLR types.
/// </summary>
static class NativeTypeConversion
{
    // Fields.
    static readonly IDictionary<Type, bool> CachedFloatingPointStructures = new ConcurrentDictionary<Type, bool>();


    // Convert from native parameter to CLR parameter.
    public static object? FromNativeParameter(object nativeValue, Type targetType)
    {
        if (IsNativeType(targetType))
            return nativeValue;
        if (targetType == typeof(char))
        {
            if (nativeValue is ushort ushortValue)
                return (char)ushortValue;
        }
        else if (targetType == typeof(nuint))
        {
            if (nativeValue is nint nintValue)
                return (nuint)nintValue;
        }
        else if (targetType == typeof(GCHandle))
        {
            var nintValue = (nint)nativeValue;
            return nintValue == default ? new GCHandle() : GCHandle.FromIntPtr(nintValue);
        }
        else if (targetType.IsClass)
        {
            var nintValue = (nint)nativeValue;
            if (nintValue == default)
                return null;
            if (typeof(CFObject).IsAssignableFrom(targetType))
                return CFObject.FromHandle(targetType, nintValue, false);
            if (typeof(NSObject).IsAssignableFrom(targetType))
                return NSObject.Retain(targetType, nintValue);
            if (targetType == typeof(Class))
                return Class.FromHandle(nintValue);
            if (targetType == typeof(Selector))
                return Selector.FromHandle(nintValue);
            var gcHandle = GCHandle.FromIntPtr(nintValue);
            var clrObj = gcHandle.Target;
            gcHandle.Free();
            if (clrObj == null || targetType.IsAssignableFrom(clrObj.GetType()))
                return clrObj;
        }
        throw new NotSupportedException($"Cannot convert native value to {targetType.Name}.");
    }


    // Convert from native value to CLR value.
    public static unsafe object? FromNativeValue(byte* valuePtr, int valueCount, Type targetType, out int consumedBytes)
    {
        if (valueCount < 1)
            throw new ArgumentException("Insufficient native values for conversion.");
        if (targetType.IsValueType)
        {
            if (targetType.IsEnum)
                targetType = targetType.GetEnumUnderlyingType();
            if (targetType == typeof(bool))
            {
                consumedBytes = sizeof(bool);
                return (*valuePtr != 0);
            }
            if (targetType == typeof(IntPtr))
            {
                consumedBytes = IntPtr.Size;
                return *(IntPtr*)valuePtr;
            }
            if (targetType == typeof(UIntPtr))
            {
                consumedBytes = UIntPtr.Size;
                return *(UIntPtr*)valuePtr;
            }
            if (targetType == typeof(int))
            {
                consumedBytes = sizeof(int);
                return *(int*)valuePtr;
            }
            if (targetType == typeof(byte))
            {
                consumedBytes = sizeof(byte);
                return *(byte*)valuePtr;
            }
            if (targetType == typeof(sbyte))
            {
                consumedBytes = sizeof(sbyte);
                return *(sbyte*)valuePtr;
            }
            if (targetType == typeof(short))
            {
                consumedBytes = sizeof(short);
                return *(short*)valuePtr;
            }
            if (targetType == typeof(ushort))
            {
                consumedBytes = sizeof(ushort);
                return *(ushort*)valuePtr;
            }
            if (targetType == typeof(char))
            {
                consumedBytes = sizeof(ushort);
                return (char)*(ushort*)valuePtr;
            }
            if (targetType == typeof(uint))
            {
                consumedBytes = sizeof(uint);
                return *(uint*)valuePtr;
            }
            if (targetType == typeof(long))
            {
                consumedBytes = sizeof(long);
                return *(long*)valuePtr;
            }
            if (targetType == typeof(ulong))
            {
                consumedBytes = sizeof(ulong);
                return *(ulong*)valuePtr;
            }
            if (targetType == typeof(float))
            {
                consumedBytes = sizeof(float);
                return *(float*)valuePtr;
            }
            if (targetType == typeof(double))
            {
                consumedBytes = sizeof(double);
                return *(double*)valuePtr;
            }
            try
            {
                consumedBytes = Marshal.SizeOf(targetType);
                if (valueCount < consumedBytes)
                    throw new ArgumentException("Insufficient native values for conversion.");
                return Marshal.PtrToStructure((IntPtr)valuePtr, targetType);
            }
            catch (Exception ex)
            {
                throw new NotSupportedException($"Cannot convert native value to {targetType.Name}.", ex);
            }
        }
        else if (targetType.IsClass)
        {
            consumedBytes = IntPtr.Size;
            var handle = *(IntPtr*)valuePtr;
            if (handle == default)
                return null;
            if (typeof(NSObject).IsAssignableFrom(targetType))
                return NSObject.Retain(targetType, handle);
            if (targetType == typeof(Class))
                return Class.FromHandle(handle);
            if (targetType == typeof(Selector))
                return Selector.FromHandle(handle);
            GCHandle gcHandle = GCHandle.FromIntPtr(handle);
            return gcHandle.IsAllocated ? gcHandle.Target : null;
        }
        else if (targetType == typeof(string))
        {
            var s = new string((sbyte*)valuePtr);
            consumedBytes = s.Length + 1;
            return s;
        }
        throw new NotSupportedException($"Cannot convert native value to {targetType.Name}.");
    }


    // Convert from Objective-C type encoding.
    public static Type FromTypeEncoding(string typeEncoding, out int elementCount) =>
        FromTypeEncoding(typeEncoding.AsSpan(), out elementCount, out var _);
    static Type FromTypeEncoding(ReadOnlySpan<char> typeEncoding, out int elementCount, out int consumedChars)
    {
        elementCount = 1;
        consumedChars = 1;
        var typeEncodingLength = typeEncoding.Length;
        if (typeEncodingLength == 0)
            throw new ArgumentException("Empty type encoding.");
        var subIndex = 1;
        switch (typeEncoding[0])
        {
            case 'B': 
                return typeof(bool);
            case 'c':
                return typeof(sbyte);
            case 'C':
                return typeof(byte);
            case 's':
                return typeof(short);
            case 'S':
                return typeof(ushort);
            case 'i':
                return typeof(int);
            case 'I':
                return typeof(uint);
            case 'q':
                return typeof(long);
            case 'Q':
                return typeof(ulong);
            case 'f':
                return typeof(float);
            case 'd':
                return typeof(double);
            case '*':
                return typeof(string);
            case 'v':
                return typeof(void);
            case '@':
                return typeof(NSObject);
            case '#':
                return typeof(Class);
            case ':':
                return typeof(Selector);
            case '?':
                return typeof(IntPtr);
            case '^':
                FromTypeEncoding(typeEncoding.Slice(1), out elementCount, out consumedChars);
                elementCount = 1;
                consumedChars += 1;
                return typeof(IntPtr);
            case '[': // array
                {
                    elementCount = -1;
                    while (subIndex < typeEncodingLength)
                    {
                        if (!char.IsDigit(typeEncoding[subIndex]))
                        {
                            if (subIndex > 1)
                                elementCount = int.Parse(typeEncoding.Slice(1, subIndex - 1));
                            else
                                elementCount = 0;
                            break;
                        }
                        ++subIndex;
                    }
                    if (elementCount < 0)
                        goto default;
                    var elementType = FromTypeEncoding(typeEncoding.Slice(subIndex, typeEncodingLength - subIndex), out var subElementCount, out consumedChars);
                    if (subIndex + consumedChars > typeEncodingLength || typeEncoding[subIndex + consumedChars] != ']')
                        goto default;
                    consumedChars += subIndex + 1;
                    return elementType.MakeArrayType();
                }
            case '{': // structure
                {
                    while (subIndex < typeEncodingLength)
                    {
                        var c = typeEncoding[subIndex];
                        if (c == '=')
                        {
                            ++subIndex;
                            break;
                        }
                        if (c == '}')
                            goto default;
                        ++subIndex;
                    }
                    if (subIndex == typeEncodingLength || typeEncoding[subIndex] == '}')
                        goto default;
                    var totalFieldSize = 0;
                    var alignment = IntPtr.Size;
                    var remainingInSlot = alignment;
                    while (typeEncoding[subIndex] != '}')
                    {
                        var fieldType = FromTypeEncoding(typeEncoding.Slice(subIndex), out elementCount, out consumedChars);
                        var fieldSize = fieldType.IsArray
                            ? elementCount > 0 ? (elementCount * Marshal.SizeOf(fieldType.GetElementType()!)) : IntPtr.Size
                            : Marshal.SizeOf(fieldType);
                        if (remainingInSlot >= fieldSize)
                        {
                            remainingInSlot -= fieldSize;
                            if (remainingInSlot == 0)
                            {
                                totalFieldSize += alignment;
                                remainingInSlot = alignment;
                            }
                        }
                        else
                        {
                            totalFieldSize += alignment;
                            remainingInSlot = alignment;
                            totalFieldSize += (fieldSize / alignment) * alignment;
                            if ((fieldSize % alignment) != 0)
                                totalFieldSize += alignment;
                        }
                        subIndex += consumedChars;
                        if (subIndex >= typeEncodingLength)
                            goto default;
                        if (typeEncoding[subIndex] == '}')
                            break;
                    }
                    if (remainingInSlot < alignment)
                        totalFieldSize += alignment;
                    elementCount = totalFieldSize;
                    consumedChars = (subIndex + 1);
                    return typeof(byte[]);
                }
            case '(': // union
                {
                    while (subIndex < typeEncodingLength)
                    {
                        var c = typeEncoding[subIndex];
                        if (c == '=')
                        {
                            ++subIndex;
                            break;
                        }
                        if (c == '}')
                            goto default;
                        ++subIndex;
                    }
                    if (subIndex == typeEncodingLength || typeEncoding[subIndex] == ')')
                        goto default;
                    var maxFieldSize = 0;
                    while (typeEncoding[subIndex] != '}')
                    {
                        var fieldType = FromTypeEncoding(typeEncoding.Slice(subIndex), out elementCount, out consumedChars);
                        if (fieldType.IsArray)
                            maxFieldSize = Math.Max(maxFieldSize, elementCount > 0 ? (Marshal.SizeOf(fieldType.GetElementType()!)) : IntPtr.Size);
                        else
                            maxFieldSize = Math.Max(maxFieldSize, Marshal.SizeOf(fieldType));
                        subIndex += consumedChars;
                        if (subIndex >= typeEncodingLength)
                            goto default;
                        if (typeEncoding[subIndex] == ')')
                            break;
                    }
                    if ((maxFieldSize % IntPtr.Size) > 0)
                        maxFieldSize = (maxFieldSize / IntPtr.Size) + 1;
                    elementCount = maxFieldSize;
                    consumedChars = (subIndex + 1);
                    return typeof(byte[]);
                }
            case 'b': // bit fields
                {
                    int bitCount = -1;
                    while (subIndex < typeEncodingLength)
                    {
                        if (!char.IsDigit(typeEncoding[subIndex]))
                        {
                            bitCount = int.Parse(typeEncoding.Slice(1, subIndex - 1));
                            break;
                        }
                    }
                    if (bitCount < 0)
                        goto default;
                    consumedChars = subIndex;
                    if (bitCount <= 32)
                        return typeof(int);
                    if (bitCount <= 64)
                        return typeof(long);
                    elementCount = (bitCount >> 5);
                    if ((bitCount % 0x1f) != 0)
                        ++elementCount;
                    return typeof(int[]);
                }
            default:
                throw new ArgumentException($"Invalid type encoding: {typeEncoding}.");
        }
    }


    // Get size of native value in bytes.
    public static int GetNativeValueSize(Type type)
    {
        type = ToNativeType(type);
        if (type == typeof(bool))
            return sizeof(bool);
        if (type == typeof(byte) || type == typeof(sbyte))
            return sizeof(byte);
        if (type == typeof(short) || type == typeof(ushort))
            return sizeof(short);
        if (type == typeof(int) || type == typeof(uint))
            return sizeof(int);
        if (type == typeof(long) || type == typeof(ulong))
            return sizeof(long);
        if (type == typeof(float))
            return sizeof(float);
        if (type == typeof(double))
            return sizeof(double);
        if (type == typeof(nint))
            return IntPtr.Size;
        if (type.IsValueType)
            return Marshal.SizeOf(type);
        throw new NotSupportedException($"Cannot get size of native type: {type.Name}.");
    }


    // Check whether given type is the native type or not.
    public static bool IsNativeType(Type type)
    {
        if (!type.IsValueType)
            return false;
        if (type.IsEnum)
            type = type.GetEnumUnderlyingType();
        if (type == typeof(char)) // To prevent marshalling as sbyte
            return false;
        if (type == typeof(nuint)) // No difference from nint in native layer
            return false;
        return type == typeof(int)
            || type == typeof(uint)
            || type == typeof(float)
            || type == typeof(double)
            || type == typeof(nint)
            || type == typeof(bool)
            || type == typeof(byte)
            || type == typeof(sbyte)
            || type == typeof(short)
            || type == typeof(ushort)
            || type == typeof(long)
            || type == typeof(ulong)
            || Global.RunOrDefault(() =>
            {
                Marshal.SizeOf(type);
                return true;
            }, false);
    }


    // Convert from CLR parameter to native parameter.
    public static object ToNativeParameter(object? value)
    {
        if (value == null)
            return default(nint);
        if (IsNativeType(value.GetType()))
            return value;
        if (value is char charValue)
            return (ushort)charValue;
        if (value is nuint nuintValue)
            return (nint)nuintValue;
        if (value is GCHandle gcHandle)
            return GCHandle.ToIntPtr(gcHandle);
        else if (value.GetType().IsClass)
        {
            if (value is CFObject cfObject)
                return cfObject.Handle;
            else if (value is NSObject nsObject)
                return nsObject.Handle;
            else if (value is Class cls)
                return cls.Handle;
            else if (value is Selector sel)
                return sel.Handle;
            return GCHandle.ToIntPtr(GCHandle.Alloc(value));
        }
        throw new NotSupportedException($"Cannot convert from {value.GetType().Name} to native value.");
    }


    // Convert from CLR object to native value.
    public static unsafe int ToNativeValue(object? obj, byte* valuePtr)
    {
        obj?.GetType()?.Let(t =>
        {
            if (t.IsEnum)
            {
                t = t.GetEnumUnderlyingType();
                if (t == typeof(int))
                    obj = (int)obj;
                else if (t == typeof(uint))
                    obj = (uint)obj;
                else if (t == typeof(long))
                    obj = (uint)obj;
                else if (t == typeof(ulong))
                    obj = (uint)obj;
                else
                    throw new NotSupportedException($"Cannot convert {obj.GetType().Name} to native type.");
            }
        });
        if (obj == null)
        {
            *(nint*)valuePtr = 0;
            return IntPtr.Size;
        }
        else if (obj is bool boolValue)
        {
            *(bool*)valuePtr = boolValue;
            return sizeof(bool);
        }
        else if (obj is IntPtr intPtrValue)
        {
            *(IntPtr*)valuePtr = intPtrValue;
            return IntPtr.Size;
        }
        else if (obj is UIntPtr uintPtrValue)
        {
            *(UIntPtr*)valuePtr = uintPtrValue;
            return UIntPtr.Size;
        }
        else if (obj is byte byteValue)
        {
            *(byte*)valuePtr = byteValue;
            return sizeof(byte);
        }
        else if (obj is sbyte sbyteValue)
        {
            *(sbyte*)valuePtr = sbyteValue;
            return sizeof(sbyte);
        }
        else if (obj is short shortValue)
        {
            *(short*)valuePtr = shortValue;
            return sizeof(short);
        }
        else if (obj is ushort ushortValue)
        {
            *(ushort*)valuePtr = ushortValue;
            return sizeof(ushort);
        }
        else if (obj is char charValue)
        {
            *(ushort*)valuePtr = (ushort)charValue;
            return sizeof(ushort);
        }
        else if (obj is int intValue)
        {
            *(int*)valuePtr = intValue;
            return sizeof(int);
        }
        else if (obj is uint uintValue)
        {
            *(uint*)valuePtr = uintValue;
            return sizeof(uint);
        }
        else if (obj is long longValue)
        {
            *(long*)valuePtr = longValue;
            return sizeof(long);
        }
        else if (obj is ulong ulongValue)
        {
            *(ulong*)valuePtr = ulongValue;
            return sizeof(ulong);
        }
        else if (obj is float floatValue)
        {
            *(float*)valuePtr = floatValue;
            return sizeof(float);
        }
        else if (obj is double doubleValue)
        {
            *(double*)valuePtr = doubleValue;
            return sizeof(double);
        }
        else if (obj is ValueType)
        {
            try
            {
                var size = Marshal.SizeOf(obj);
                Marshal.StructureToPtr(obj, (IntPtr)valuePtr, false);
                return size;
            }
            catch (Exception ex)
            {
                throw new NotSupportedException($"Cannot convert {obj.GetType().Name} to native type.", ex);
            }
        }
        else if (obj is NSObject nsObj)
        {
            *(IntPtr*)valuePtr = nsObj.Handle;
            return IntPtr.Size;
        }
        else if (obj is Class cls)
        {
            *(IntPtr*)valuePtr = cls.Handle;
            return IntPtr.Size;
        }
        else if (obj is Selector selector)
        {
            *(IntPtr*)valuePtr = selector.Handle;
            return IntPtr.Size;
        }
        else
            throw new NotSupportedException($"Cannot convert {obj.GetType().Name} to native type.");
    }


    // Convert to corresponding native type.
    public static Type ToNativeType(Type type)
    {
        if (IsNativeType(type))
            return type;
        else if (type == typeof(char)) // To prevent marshalling as sbyte
            return typeof(ushort);
        else if (type == typeof(nuint)) // No difference in native layer
            return typeof(nint);
        else if (type == typeof(GCHandle) || type.IsClass)
            return typeof(nint); // CLR object through GCHandle
        throw new NotSupportedException($"Cannot convert from {type.Name} to native type.");
    }


    // Convert to Objective-C type encoding.
    public static string ToTypeEncoding<T>(int elementCount = 1) =>
        ToTypeEncoding(typeof(T), elementCount);
    public static string ToTypeEncoding(object obj)
    {
        if (obj is Type type)
            return ToTypeEncoding(type);
        if (obj is Array array)
            return ToTypeEncoding(obj.GetType(), array.GetLength(0));
        return ToTypeEncoding(obj.GetType());
    }
    public static string ToTypeEncoding(Type type, int elementCount = 1)
    {
        var isArray = type.IsArray;
        var arrayLengthPrefix = "";
        if (isArray)
        {
            if (type.GetArrayRank() != 1)
                throw new NotSupportedException($"Only 1-dimensional array is supported for Objective-C.");
            if (elementCount < 0)
                throw new ArgumentOutOfRangeException(nameof(elementCount));
            type = type.GetElementType().AsNonNull();
            arrayLengthPrefix = elementCount > 0 ? elementCount.ToString() : "";
        }
        if (type.IsValueType)
        {
            if (type == typeof(bool))
                return isArray ? $"[{arrayLengthPrefix}B]" : "B";
            if (type == typeof(byte))
                return isArray ? $"[{arrayLengthPrefix}C]" : "C";
            if (type == typeof(sbyte))
                return isArray ? $"[{arrayLengthPrefix}c]" : "c";
            if (type == typeof(short))
                return isArray ? $"[{arrayLengthPrefix}s]" : "s";
            if (type == typeof(ushort) || type == typeof(char))
                return isArray ? $"[{arrayLengthPrefix}S]" : "S";
            if (type == typeof(int))
                return isArray ? $"[{arrayLengthPrefix}i]" : "i";
            if (type == typeof(uint))
                return isArray ? $"[{arrayLengthPrefix}I]" : "I";
            if (type == typeof(long))
                return isArray ? $"[{arrayLengthPrefix}q]" : "q";
            if (type == typeof(ulong))
                return isArray ? $"[{arrayLengthPrefix}Q]" : "Q";
            if (type == typeof(IntPtr) || type == typeof(UIntPtr) || type == typeof(GCHandle))
                return isArray ? $"[{arrayLengthPrefix}^v]" : "^v";
            if (type == typeof(float))
                return isArray ? $"[{arrayLengthPrefix}f]" : "f";
            if (type == typeof(double))
                return isArray ? $"[{arrayLengthPrefix}d]" : "d";
            var teBuffer = new StringBuilder();
            teBuffer.Append('{');
            teBuffer.Append(type.Name);
            teBuffer.Append('=');
            foreach (var fieldInfo in type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
            {
                var fieldType = fieldInfo.FieldType;
                var subElementCount = 1;
                if (fieldType.IsArray)
                {
                    var marshalAsAttr = fieldInfo.GetCustomAttribute<MarshalAsAttribute>();
                    if (marshalAsAttr == null)
                        throw new ArgumentException($"Array field without MarshalAs attribute: {type.Name}.{fieldInfo.Name}.");
                    if (marshalAsAttr.Value == UnmanagedType.ByValArray)
                    {
                        subElementCount = marshalAsAttr.SizeConst;
                        if (subElementCount <= 0)
                            throw new ArgumentException($"Array field without fixed size: {type.Name}.{fieldInfo.Name}.");
                    }
                    else
                        throw new ArgumentException($"Array field with unsupported MarshalAs attribute: {type.Name}.{fieldInfo.Name}.");
                }
                teBuffer.Append(ToTypeEncoding(fieldType, subElementCount));
            }
            teBuffer.Append('}');
            return isArray ? $"[{arrayLengthPrefix}{teBuffer}]" : teBuffer.ToString();
        }
        else if (type == typeof(Class))
            return isArray ? $"[{arrayLengthPrefix}#]" : "#";
        else if (type == typeof(Selector))
            return isArray ? $"[{arrayLengthPrefix}:]" : ":";
        else if (typeof(NSObject).IsAssignableFrom(type))
            return isArray ? $"[{arrayLengthPrefix}@]" : "@";
        else if (type == typeof(string))
            return isArray ? $"[{arrayLengthPrefix}*]" : "*";
        throw new NotSupportedException($"Unsupported type for Objective-C: {type.Name}.");
    }
}