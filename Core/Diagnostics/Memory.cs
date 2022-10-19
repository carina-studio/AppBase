using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;

namespace CarinaStudio.Diagnostics
{
    /// <summary>
    /// Diagnostic functions of memory.
    /// </summary>
    public static class Memory
    {
        // Static fields.
        static readonly int ObjectHeaderSize = IntPtr.Size << 1; // Object header + Method table pointer
        static readonly ConcurrentDictionary<Type, long> ObjectSizes = new();
        static readonly ConcurrentDictionary<Type, long> StructureSizes = new();


        /// <summary>
        /// Estimate size of array instance in bytes.
        /// </summary>
        /// <param name="elementCount">Number of element.</param>
        /// <typeparam name="T">Type of element.</typeparam>
        /// <returns>Size of array instance.</returns>
        public static long EstimateArrayInstanceSize<T>(long elementCount) =>
            EstimateArrayInstanceSize(typeof(T), elementCount);


        /// <summary>
        /// Estimate size of array instance in bytes.
        /// </summary>
        /// <param name="elementType">Type of element.</param>
        /// <param name="elementCount">Number of element.</param>
        /// <returns>Size of array instance.</returns>
        public static long EstimateArrayInstanceSize(Type elementType, long elementCount)
        {
            if (elementCount < 0)
                throw new ArgumentOutOfRangeException(nameof(elementCount));
            var elementSize = elementType.IsValueType
                ? EstimateStructureSizeInternal(elementType)
                : IntPtr.Size;
            return EstimateArrayInstanceSizeInternal(elementSize, elementCount);
        }


        /// <summary>
        /// Estimate size of array instance in bytes.
        /// </summary>
        /// <param name="elementSize">Size of element in bytes.</param>
        /// <param name="elementCount">Number of element.</param>
        /// <returns>Size of array instance.</returns>
        public static long EstimateArrayInstanceSize(long elementSize, long elementCount)
        {
            if (elementSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(elementSize));
            if (elementCount < 0)
                throw new ArgumentOutOfRangeException(nameof(elementCount));
            return EstimateArrayInstanceSizeInternal(elementSize, elementCount);
        }


        // Estimate size of array instance.
        static long EstimateArrayInstanceSizeInternal(long elementSize, long elementCount) =>
            ObjectHeaderSize + IntPtr.Size /* Length */ + (elementSize * elementCount);
        

        /// <summary>
        /// Estimate size of collection instance in bytes.
        /// </summary>
        /// <param name="elementCount">Number of element.</param>
        /// <typeparam name="T">Type of element.</typeparam>
        /// <returns>Size of collection instance.</returns>
        public static long EstimateCollectionInstanceSize<T>(long elementCount) =>
            EstimateCollectionInstanceSize(typeof(T), elementCount);
        

        /// <summary>
        /// Estimate size of collection instance in bytes.
        /// </summary>
        /// <param name="elementType">Type of element.</param>
        /// <param name="elementCount">Number of element.</param>
        /// <returns>Size of collection instance.</returns>
        public static long EstimateCollectionInstanceSize(Type elementType, long elementCount)
        {
            if (elementCount < 0)
                throw new ArgumentOutOfRangeException(nameof(elementCount));
            var elementSize = elementType.IsValueType
                ? EstimateStructureSizeInternal(elementType)
                : IntPtr.Size;
            return EstimateCollectionInstanceSizeInternal(elementSize, elementCount);
        }


        /// <summary>
        /// Estimate size of collection instance in bytes.
        /// </summary>
        /// <param name="elementSize">Size of element in bytes.</param>
        /// <param name="elementCount">Number of element.</param>
        /// <returns>Size of collection instance.</returns>
        public static long EstimateCollectionInstanceSize(long elementSize, long elementCount)
        {
            if (elementSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(elementSize));
            if (elementCount < 0)
                throw new ArgumentOutOfRangeException(nameof(elementCount));
            return EstimateCollectionInstanceSizeInternal(elementSize, elementCount);
        }
        

        // Estimate size of collection instance.
        static long EstimateCollectionInstanceSizeInternal(long elementSize, long elementCount) =>
            ObjectHeaderSize + sizeof(int) /* Count */ + EstimateArrayInstanceSizeInternal(elementSize, elementCount);
        

        /// <summary>
        /// Estimate size of instance in bytes.
        /// </summary>
        /// <param name="obj">Reference of instance.</param>
        /// <returns>Size of instance.</returns>
        public static long EstimateInstanceSize(object? obj)
        {
            if (obj == null)
                return 0;
            var type = obj.GetType();
            if (type.IsValueType)
                return EstimateStructureSizeInternal(type, true);
            if (obj is string str)
                return EstimateObjectSizeInternal(typeof(string), str.Length);
            if (obj is Array array)
                return EstimateArrayInstanceSize(type.GetElementType()!, array.LongLength);
            var interfaces = type.GetInterfaces();
            for (var i = interfaces.Length - 1; i >= 0; --i)
            {
                var interfaceType = interfaces[i];
                if (interfaceType.IsGenericType
                    && interfaceType.GetGenericTypeDefinition() == typeof(ICollection<>))
                {
                    var elementType = interfaceType.GetGenericArguments()[0];
                    var elementCount = (int)interfaceType.GetProperty(nameof(ICollection.Count))!.GetValue(obj)!;
                    return EstimateCollectionInstanceSize(elementType, elementCount);
                }
            }
            if (obj is ICollection collection)
                return EstimateCollectionInstanceSizeInternal(IntPtr.Size, collection.Count);
            return EstimateInstanceSize(type);
        }
        

        /// <summary>
        /// Estimate size of instance in bytes.
        /// </summary>
        /// <param name="length">Length of data if type is array or string.</param>
        /// <typeparam name="T">Type of instance.</typeparam>
        /// <returns>Size of instance.</returns>
        public static long EstimateInstanceSize<T>(long length = 0) =>
            EstimateInstanceSize(typeof(T));
        

        /// <summary>
        /// Estimate size of instance in bytes.
        /// </summary>
        /// <param name="type">Type.</param>
        /// <param name="length">Length of data if type is array or string.</param>
        /// <returns>Size of instance.</returns>
        public static long EstimateInstanceSize(Type type, long length = 0)
        {
            if (type.IsValueType)
                return EstimateStructureSizeInternal(type, true);
            if (type.IsPointer)
                return IntPtr.Size;
            if (type.IsArray)
                return EstimateArrayInstanceSize(type.GetElementType()!, length);
            if (type.IsClass)
                return EstimateObjectSizeInternal(type, length);
            throw new NotSupportedException($"Cannot estimate instance size of {type.Name}.");
        }


        // Estimate size of object.
        static long EstimateObjectSizeInternal(Type type, long length = 0)
        {
            // use cached size
            if (ObjectSizes.TryGetValue(type, out var size))
                return size;
            
            // calculate size
            if (type == typeof(string))
                size = sizeof(int) /* Length */ + sizeof(char) * length;
            else
            {
                foreach (var field in type.GetRuntimeFields())
                {
                    if (field.IsStatic)
                        continue;
                    var fieldType = field.FieldType;
                    if (fieldType.IsValueType)
                        size += EstimateStructureSizeInternal(fieldType);
                    else
                        size += IntPtr.Size;
                }
            }
            if ((size % IntPtr.Size) != 0)
                size = (size / IntPtr.Size + 1) * IntPtr.Size;
            size += ObjectHeaderSize;
            ObjectSizes.TryAdd(type, size);
            return size;
        }


        // Estimate size of structure.
        static long EstimateStructureSizeInternal(Type type, bool boxed = false)
        {
            // get cached size
            var size = 0L;
            if (type == typeof(bool))
                size = sizeof(bool);
            else if (type == typeof(byte) || type == typeof(sbyte))
                size = 1;
            else if (type == typeof(short) || type == typeof(ushort))
                size = 2;
            else if (type == typeof(int) || type == typeof(uint) || type == typeof(float))
                size = 4;
            else if (type == typeof(long) || type == typeof(ulong) || type == typeof(double))
                size = 8;
            else if (type == typeof(nint) || type == typeof(nuint))
                size = IntPtr.Size;
            else if (type == typeof(decimal))
                size = sizeof(decimal);
            else if (StructureSizes.TryGetValue(type, out var cachedSize))
                size = cachedSize;

            // calculate size
            if (size == 0)
            {
                foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
                {
                    var fieldType = field.FieldType;
                    if (fieldType.IsValueType)
                        size += EstimateStructureSizeInternal(fieldType);
                    else
                        size += IntPtr.Size;
                }
                StructureSizes.TryAdd(type, size);
            }
            if (boxed)
            {
                if ((size % IntPtr.Size) != 0)
                    return ObjectHeaderSize + ((size / IntPtr.Size) + 1) * IntPtr.Size;
                return ObjectHeaderSize + size;
            }
            return size;
        }


        /// <summary>
        /// Estimate size of value in bytes.
        /// </summary>
        /// <param name="value">Value.</param>
        /// <returns>Size of value.</returns>
        public static long EstimateValueSize(ValueType value) =>
            EstimateStructureSizeInternal(value.GetType());
        

        /// <summary>
        /// Estimate size of value in bytes.
        /// </summary>
        public static long EstimateValueSize<T>() where T : struct =>
            EstimateStructureSizeInternal(typeof(T));
    }
}