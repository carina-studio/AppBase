using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;

namespace CarinaStudio.Diagnostics
{
    /// <summary>
    /// Tests of <see cref="Memory"/>
    /// </summary>
    [TestFixture]
    public class MemoryTests
    {
        // Static fields.
        static readonly int ObjectHeaderSize = IntPtr.Size << 1; // Object header + Method table pointer


        /// <summary>
        /// Test for estimating size of array instance.
        /// </summary>
        [Test]
        public void ArrayTest()
        {
            // value array
            var random = new Random();
            int[] intArray;
            var size1 = 0L;
            var size2 = 0L;
            var size3 = 0L;
            for (var i = 0; i < 10; ++i)
            {
                intArray = new int[random.Next(1, 1024)];
                size1 = Memory.EstimateArrayInstanceSize<int>(intArray.Length);
                size2 = Memory.EstimateArrayInstanceSize(sizeof(int), intArray.Length);
                size3 = Memory.EstimateInstanceSize(intArray);
                Assert.AreEqual(ObjectHeaderSize + IntPtr.Size + sizeof(int) * intArray.Length, size1);
                Assert.AreEqual(size1, size2);
                Assert.AreEqual(size1, size3);
            }

            // empty value array
            intArray = new int[0];
            size1 = Memory.EstimateArrayInstanceSize<int>(intArray.Length);
            size2 = Memory.EstimateArrayInstanceSize(sizeof(int), intArray.Length);
            size3 = Memory.EstimateInstanceSize(intArray);
            Assert.AreEqual(ObjectHeaderSize + IntPtr.Size, size1);
            Assert.AreEqual(size1, size2);
            Assert.AreEqual(size1, size3);

            // object array
            object[] objArray;
            for (var i = 0; i < 10; ++i)
            {
                objArray = new object[random.Next(1, 1024)];
                size1 = Memory.EstimateArrayInstanceSize<object>(objArray.Length);
                size2 = Memory.EstimateArrayInstanceSize(IntPtr.Size, objArray.Length);
                size3 = Memory.EstimateInstanceSize(objArray);
                Assert.AreEqual(ObjectHeaderSize + IntPtr.Size + IntPtr.Size * objArray.Length, size1);
                Assert.AreEqual(size1, size2);
                Assert.AreEqual(size1, size3);
            }

            // empty object array
            objArray = new object[0];
            size1 = Memory.EstimateArrayInstanceSize<object>(objArray.Length);
            size2 = Memory.EstimateArrayInstanceSize(IntPtr.Size, objArray.Length);
            size3 = Memory.EstimateInstanceSize(objArray);
            Assert.AreEqual(ObjectHeaderSize + IntPtr.Size, size1);
            Assert.AreEqual(size1, size2);
            Assert.AreEqual(size1, size3);
        }


        /// <summary>
        /// Test for size of collection instance.
        /// </summary>
        [Test]
        public void CollectionTest()
        {
            // value collection
            var random = new Random();
            ICollection collection;
            ICollection<int> intCollection;
            var count = 0;
            var size1 = 0L;
            var size2 = 0L;
            var size3 = 0L;
            for (var i = 0; i < 10; ++i)
            {
                count = random.Next(1, 1024);
                collection = new ArrayList().Also(it =>
                {
                    for (var i = count; i > 0; --i)
                        it.Add(i);
                });
                intCollection = new List<int>(count).Also(it =>
                {
                    for (var i = count; i > 0; --i)
                        it.Add(i);
                });
                size1 = Memory.EstimateCollectionInstanceSize<object>(count);
                size2 = Memory.EstimateCollectionInstanceSize(IntPtr.Size, count);
                size3 = Memory.EstimateInstanceSize(collection);
                Assert.AreEqual(ObjectHeaderSize + sizeof(int) + Memory.EstimateArrayInstanceSize<object>(count), size1);
                Assert.AreEqual(size1, size2);
                Assert.AreEqual(size1, size3);
                size1 = Memory.EstimateCollectionInstanceSize<int>(count);
                size2 = Memory.EstimateCollectionInstanceSize(sizeof(int), count);
                size3 = Memory.EstimateInstanceSize(intCollection);
                Assert.AreEqual(ObjectHeaderSize + sizeof(int) + Memory.EstimateArrayInstanceSize<int>(count), size1);
                Assert.AreEqual(size1, size2);
                Assert.AreEqual(size1, size3);
            }

            // object collection
            ICollection<object> objCollection;
            for (var i = 0; i < 10; ++i)
            {
                count = random.Next(1, 1024);
                objCollection = new List<object>(count).Also(it =>
                {
                    for (var i = count; i > 0; --i)
                        it.Add(i);
                });
                size1 = Memory.EstimateCollectionInstanceSize<object>(count);
                size2 = Memory.EstimateCollectionInstanceSize(IntPtr.Size, count);
                size3 = Memory.EstimateInstanceSize(objCollection);
                Assert.AreEqual(ObjectHeaderSize + sizeof(int) + Memory.EstimateArrayInstanceSize<object>(count), size1);
                Assert.AreEqual(size1, size2);
                Assert.AreEqual(size1, size3);
            }

            // empty collection
            collection = new ArrayList();
            objCollection = new List<object>();
            size1 = Memory.EstimateCollectionInstanceSize<object>(0);
            size2 = Memory.EstimateCollectionInstanceSize(IntPtr.Size, 0);
            size3 = Memory.EstimateInstanceSize(collection);
            Assert.AreEqual(ObjectHeaderSize + sizeof(int) + Memory.EstimateArrayInstanceSize<object>(0), size1);
            Assert.AreEqual(size1, size2);
            Assert.AreEqual(size1, size3);
            size3 = Memory.EstimateInstanceSize(objCollection);
            Assert.AreEqual(size1, size3);
        }


        /// <summary>
        /// Test for size of object.
        /// </summary>
        [Test]
        public void InstanceTest()
        {
            // native type
            Assert.AreEqual(sizeof(int), Memory.EstimateValueSize(0));
            Assert.AreEqual(sizeof(int), Memory.EstimateValueSize<int>());

            // native type (boxed)
            Assert.AreEqual(ObjectHeaderSize + IntPtr.Size, Memory.EstimateInstanceSize(0));
            Assert.AreEqual(ObjectHeaderSize + IntPtr.Size, Memory.EstimateInstanceSize<int>());

            // structure
            Assert.AreEqual(sizeof(int) * 3, Memory.EstimateValueSize(new ValueTuple<int, int, int>()));
            Assert.AreEqual(sizeof(int) * 3, Memory.EstimateValueSize<ValueTuple<int, int, int>>());

            // structure (boxed)
            Assert.AreEqual(ObjectHeaderSize + IntPtr.Size * 3, Memory.EstimateInstanceSize(new ValueTuple<IntPtr, IntPtr, IntPtr>()));
            Assert.AreEqual(ObjectHeaderSize + IntPtr.Size * 3, Memory.EstimateInstanceSize<ValueTuple<IntPtr, IntPtr, IntPtr>>());

            // object
            Assert.AreEqual(ObjectHeaderSize + IntPtr.Size * 2, Memory.EstimateInstanceSize(new Tuple<IntPtr, IntPtr>(default, default)));
            Assert.AreEqual(ObjectHeaderSize + IntPtr.Size * 2, Memory.EstimateInstanceSize<Tuple<IntPtr, IntPtr>>());
        }
    }
}