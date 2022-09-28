using CarinaStudio.Collections;
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
#pragma warning disable CS8600
#pragma warning disable CS8603
    public static unsafe T FromNativeValue<T>(nint* valuePtr, int valueCount) =>
        (T)FromNativeValue(valuePtr, valueCount, typeof(T), out var _);
    public static unsafe T FromNativeValue<T>(nint* valuePtr, int valueCount, out int consumedValues) =>
        (T)FromNativeValue(valuePtr, valueCount, typeof(T), out consumedValues);
#pragma warning restore CS8600
#pragma warning restore CS8603
    public static unsafe object? FromNativeValue(nint* valuePtr, int valueCount, Type targetType, out int consumedValues)
    {
        if (valueCount < 1)
            throw new ArgumentException("Insufficient native values for conversion.");
        consumedValues = 1;
        if (targetType.IsValueType)
        {
            if (targetType == typeof(bool))
                return (*valuePtr != 0);
            if (targetType == typeof(IntPtr))
                return *(IntPtr*)valuePtr;
            if (targetType == typeof(UIntPtr))
                return *(UIntPtr*)valuePtr;
            if (targetType == typeof(int))
                return *(int*)valuePtr;
            if (targetType == typeof(uint))
                return *(uint*)valuePtr;
            if (targetType == typeof(long))
                return *(long*)valuePtr;
            if (targetType == typeof(ulong))
                return *(ulong*)valuePtr;
            if (targetType == typeof(float))
                return *(float*)valuePtr;
            if (targetType == typeof(double))
                return *(double*)valuePtr;
            try
            {
                var size = Marshal.SizeOf(targetType);
                consumedValues = (size / IntPtr.Size);
                if ((size % IntPtr.Size) > 0)
                    ++consumedValues;
                if (valueCount < consumedValues)
                    throw new ArgumentException("Insufficient native values for conversion.");
                return Marshal.PtrToStructure((IntPtr)valuePtr, targetType);
            }
            catch (Exception ex)
            {
                throw new NotSupportedException($"Cannot convert native value to {targetType.Name}.", ex);
            }
        }
        else if (targetType.IsClass
            && typeof(NSObject).IsAssignableFrom(targetType))
        {
            if (*valuePtr == 0)
                return null;
            return NSObject.Wrap(targetType, (IntPtr)(*valuePtr), false);
        }
        throw new NotSupportedException($"Cannot convert native value to {targetType.Name}.");
    }


    // Calculate number of native values needed for CLR object.
    public static int GetNativeValueCount(object? obj) =>
        obj != null ? GetNativeValueCount(obj.GetType()) : 1;
    public static int GetNativeValueCount<T>() =>
        GetNativeValueCount(typeof(T));
    public static int GetNativeValueCount(Type type)
    {
        if (typeof(NSObject).IsAssignableFrom(type))
            return 1;
        else if (type == typeof(bool)
            || type == typeof(IntPtr)
            || type == typeof(UIntPtr)
            || type == typeof(int)
            || type == typeof(uint)
            || type == typeof(long)
            || type == typeof(ulong)
            || type == typeof(float)
            || type == typeof(double))
        {
            return 1;
        }
        else if (type.IsValueType)
        {
            try
            {
                var size = Marshal.SizeOf(type);
                if ((size % IntPtr.Size) == 0)
                    return size / IntPtr.Size;
                return size / IntPtr.Size + 1;
            }
            catch (Exception ex)
            {
                throw new NotSupportedException($"Cannot convert {type.Name} to native type.", ex);
            }
        }
        throw new NotSupportedException($"Cannot convert {type.Name} to native type.");
    }


    // Check whether given field is a float-point value or not.
    static bool IsFloatingPointField(FieldInfo fieldInfo) => fieldInfo.FieldType.Let(it =>
        it == typeof(float) || it == typeof(double));


    // Check whether given type is structure contains float-point fields only or not.
    public static bool IsFloatingPointStructure(Type type)
    {
        if (!type.IsValueType)
            return false;
        if (CachedFloatingPointStructures.TryGetValue(type, out var isFpStructure))
            return isFpStructure;
        var fields = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        if (fields.IsEmpty())
            return false;
        isFpStructure = IsFloatingPointField(fields[0]);
        for (var i = fields.Length - 1; i > 1; --i)
        {
            if (isFpStructure != IsFloatingPointField(fields[i]))
                throw new NotSupportedException($"Structure '{type.Name}' can only be consist of floating-point fields nor integer fields.");
        }
        CachedFloatingPointStructures.TryAdd(type, isFpStructure);
        return isFpStructure;
    }


    // Convert from CLR object to native value.
    public static unsafe int ToNativeValue(object? obj, nint* valuePtr)
    {
        if (obj == null)
            *valuePtr = 0;
        else if (obj is NSObject nsObj)
            *valuePtr = (nint)nsObj.Handle;
        else if (obj is bool boolValue)
            *valuePtr = (boolValue ? 1: 0);
        else if (obj is IntPtr intPtrValue)
            *valuePtr = (nint)intPtrValue.ToPointer();
        else if (obj is UIntPtr uintPtrValue)
            *valuePtr = (nint)uintPtrValue.ToPointer();
        else if (obj is int intValue)
            *valuePtr = (nint)intValue;
        else if (obj is uint uintValue)
            *valuePtr = (nint)uintValue;
        else if (obj is long longValue)
            *valuePtr = (nint)longValue;
        else if (obj is ulong ulongValue)
            *valuePtr = (nint)ulongValue;
        else if (obj is float floatValue)
            *valuePtr = (nint)(*(nint*)&floatValue);
        else if (obj is double doubleValue)
            *valuePtr = (nint)(*(nint*)&doubleValue);
        else if (obj is ValueType)
        {
            try
            {
                var size = Marshal.SizeOf(obj);
                Marshal.StructureToPtr(obj, (IntPtr)valuePtr, false);
                if ((size % IntPtr.Size) == 0)
                    return size / IntPtr.Size;
                return size / IntPtr.Size + 1;
            }
            catch (Exception ex)
            {
                throw new NotSupportedException($"Cannot convert {obj.GetType().Name} to native type.", ex);
            }
        }
        else
            throw new NotSupportedException($"Cannot convert {obj.GetType().Name} to native type.");
        return 1;
    }


    // Convert CLR objects to native values.
    public static unsafe nint[] ToNativeValues(object?[] objs)
    {
        // calculate number of native values needed
        if (objs.Length == 0)
            return new nint[0];
        var nvCount = 0;
        for (var i = objs.Length - 1; i >= 0; --i)
            nvCount += GetNativeValueCount(objs[i]);
        
        // convert to native values
        var nvs = new nint[nvCount];
        fixed (nint* p = nvs)
        {
            var nvp = p;
            for (var i = 0; i< objs.Length; ++i)
                nvp += ToNativeValue(objs[i], nvp);
        }
        return nvs;
    }
}