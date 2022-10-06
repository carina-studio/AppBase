using CarinaStudio.Collections;
using CarinaStudio.MacOS.CoreFoundation;
using CarinaStudio.MacOS.ObjectiveC;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;

namespace CarinaStudio.MacOS;

/// <summary>
/// Conversion between native and CLR types.
/// </summary>
static class NativeTypeConversion
{
    // Fields.
    static readonly IDictionary<Type, bool> CachedFloatingPointStructures = new ConcurrentDictionary<Type, bool>();


    // Convert from native value to CLR value.
    public static object? FromNativeValue(object nativeValue, Type targetType)
    {
        if (nativeValue is nint nintValue)
        {
            if (targetType == typeof(bool))
                return nintValue != 0;
            if (targetType == typeof(byte))
                return (byte)nintValue;
            if (targetType == typeof(sbyte))
                return (sbyte)nintValue;
            if (targetType == typeof(short))
                return (short)nintValue;
            if (targetType == typeof(ushort))
                return (ushort)nintValue;
            if (targetType == typeof(int))
                return (int)nintValue;
            if (targetType == typeof(uint))
                return (uint)nintValue;
            if (targetType == typeof(long))
                return (long)nintValue;
            if (targetType == typeof(ulong))
                return (ulong)nintValue;
            if (targetType == typeof(nint))
                return nintValue;
            if (targetType == typeof(nuint))
                return (nuint)nintValue;
            if (targetType == typeof(GCHandle))
                return nintValue == default ? new GCHandle() : GCHandle.FromIntPtr(nintValue);
            if (targetType.IsClass)
            {
                if (nintValue == default)
                    return null;
                if (typeof(CFObject).IsAssignableFrom(targetType))
                    return CFObject.FromHandle(targetType, nintValue);
                if (typeof(NSObject).IsAssignableFrom(targetType))
                    return NSObject.FromHandle(targetType, nintValue);
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
        }
        else if (nativeValue is double doubleValue)
        {
            if (targetType == typeof(float))
                return (float)doubleValue;
            if (targetType == typeof(double))
                return doubleValue;
        }
        else if (targetType.IsAssignableFrom(nativeValue.GetType()))
            return nativeValue;
        throw new NotSupportedException($"Cannot convert native value to {targetType.Name}.");
    }
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
            if (targetType == typeof(GCHandle))
            {
                var handle = *(IntPtr*)valuePtr;
                consumedBytes = IntPtr.Size;
                return handle == default ? new GCHandle() : GCHandle.FromIntPtr(handle);
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
                return NSObject.FromHandle(targetType,handle, false);
            if (targetType == typeof(Class))
                return Class.FromHandle(handle);
            if (targetType == typeof(Selector))
                return Selector.FromHandle(handle);
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
                    var fieldSize = 0;
                    while (typeEncoding[subIndex] != '}')
                    {
                        var fieldType = FromTypeEncoding(typeEncoding.Slice(subIndex), out elementCount, out consumedChars);
                        if (fieldType.IsArray)
                            fieldSize += elementCount > 0 ? (Marshal.SizeOf(fieldType.GetElementType()!)) : IntPtr.Size;
                        else
                            fieldSize += Marshal.SizeOf(fieldType);
                        subIndex += consumedChars;
                        if (subIndex >= typeEncodingLength)
                            goto default;
                        if (typeEncoding[subIndex] == '}')
                            break;
                    }
                    if ((fieldSize % IntPtr.Size) > 0)
                        fieldSize = (fieldSize / IntPtr.Size) + 1;
                    elementCount = fieldSize;
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


    // Calculate number of native values needed for CLR object.
    public static int GetNativeFpValueCount(object? obj) =>
        obj != null ? GetNativeFpValueCount(obj.GetType()) : 1;
    public static int GetNativeFpValueCount<T>() =>
        GetNativeFpValueCount(typeof(T));
    public static int GetNativeFpValueCount(Type type)
    {
        if (type.IsEnum)
            type = type.GetEnumUnderlyingType();
        else if (type == typeof(float)
            || type == typeof(double))
        {
            return 1;
        }
        else if (type.IsValueType)
        {
            try
            {
                var size = Marshal.SizeOf(type);
                if ((size & 0x3) == 0)
                    return size >> 3;
                return (size >> 3) + 1;
            }
            catch (Exception ex)
            {
                throw new NotSupportedException($"Cannot convert {type.Name} to native float-point type.", ex);
            }
        }
        throw new NotSupportedException($"Cannot convert {type.Name} to native float-point type.");
    }
    public static int GetNativeFpValueCount(params Type[] types)
    {
        var count = 0;
        for (var i = types.Length - 1; i >= 0; --i)
            count += GetNativeFpValueCount(types[i]);
        return count;
    }


    // Convert from CLR object to native value.
    public static object ToNativeValue(object? value)
    {
        if (value == null)
            return default(nint);
        if (value is ValueType)
        {
            if (value is bool boolValue)
                return boolValue ? (nint)1 : 0;
            if (value is byte byteValue)
                return (nint)byteValue;
            if (value is sbyte sbyteValue)
                return (nint)sbyteValue;
            if (value is short shortValue)
                return (nint)shortValue;
            if (value is ushort ushortValue)
                return (nint)ushortValue;
            if (value is int intValue)
                return (nint)intValue;
            if (value is uint uintValue)
                return (nint)uintValue;
            if (value is long longValue)
                return (nint)longValue;
            if (value is ulong ulongValue)
                return (nint)ulongValue;
            if (value is nint nintValue)
                return nintValue;
            if (value is nuint nuintValue)
                return (nint)nuintValue;
            if (value is GCHandle gcHandle)
                return GCHandle.ToIntPtr(gcHandle);
            if (value is float floatValue)
                return (double)floatValue;
            if (value is double doubleValue)
                return doubleValue;
            return value;
        }
        else if (value is CFObject cfObject)
            return cfObject.Handle;
        else if (value is NSObject nsObject)
            return nsObject.Handle;
        else if (value is Class cls)
            return cls.Handle;
        else if (value is Selector sel)
            return sel.Handle;
        else
            return GCHandle.ToIntPtr(GCHandle.Alloc(value));
        throw new NotSupportedException($"Cannot convert {value.GetType().Name} to native value.");
    }
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
        else if (obj is GCHandle gcHandle)
        {
            *(IntPtr*)valuePtr = gcHandle == default ? default : GCHandle.ToIntPtr(gcHandle);
            return IntPtr.Size;
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
    public static Type ToNativeType(Type? type)
    {
        if (type == null
            || type == typeof(bool)
            || type == typeof(byte)
            || type == typeof(sbyte)
            || type == typeof(short)
            || type == typeof(ushort)
            || type == typeof(int)
            || type == typeof(uint)
            || type == typeof(long)
            || type == typeof(ulong)
            || type == typeof(nint)
            || type == typeof(nuint)
            || type == typeof(GCHandle))
        {
            return typeof(nint);
        }
        else if (type == typeof(float)
            || type == typeof(double))
        {
            return typeof(double);
        }
        else if (type.IsValueType)
            return type;
        else
            return typeof(nint); // CLR object through GCHandle
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
            if (type == typeof(ushort))
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