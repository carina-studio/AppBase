using NUnit.Framework;
using System;
using System.Globalization;

namespace CarinaStudio.Data.Converters
{
    /// <summary>
    /// Tests for converters.
    /// </summary>
    [TestFixture]
    class ConvertersTests
    {
        /// <summary>
        /// Test for <see cref="ComparableConverters"/>.
        /// </summary>
        [Test]
        public void ComparableConvertersTest()
        {
            // comparison
            for (var x = 0; x < 3; ++x)
            {
                for (var y = 0; y < 3; ++y)
                {
                    // to boolean
                    Assert.AreEqual(x > y, ComparableConverters.IsGreaterThan.Convert<bool>(x, y));
                    Assert.AreEqual(x >= y, ComparableConverters.IsNotSmallerThan.Convert<bool>(x, y));
                    Assert.AreEqual(x <= y, ComparableConverters.IsNotGreaterThan.Convert<bool>(x, y));
                    Assert.AreEqual(x < y, ComparableConverters.IsSmallerThan.Convert<bool>(x, y));

                    // to object
                    Assert.AreEqual(x > y, (bool)ComparableConverters.IsGreaterThan.Convert<object?>(x, y)!);
                    Assert.AreEqual(x >= y, (bool)ComparableConverters.IsNotSmallerThan.Convert<object?>(x, y)!);
                    Assert.AreEqual(x <= y, (bool)ComparableConverters.IsNotGreaterThan.Convert<object?>(x, y)!);
                    Assert.AreEqual(x < y, (bool)ComparableConverters.IsSmallerThan.Convert<object?>(x, y)!);
                }
            }
            Assert.IsNull(ComparableConverters.IsGreaterThan.Convert<int?>(1, 2));
            Assert.IsNull(ComparableConverters.IsNotSmallerThan.Convert<int?>(1, 2));
            Assert.IsNull(ComparableConverters.IsNotGreaterThan.Convert<int?>(1, 2));
            Assert.IsNull(ComparableConverters.IsSmallerThan.Convert<int?>(1, 2));
            Assert.IsNull(ComparableConverters.IsGreaterThan.Convert<bool?>(1, new object()));
            Assert.IsNull(ComparableConverters.IsNotSmallerThan.Convert<bool?>(1, new object()));
            Assert.IsNull(ComparableConverters.IsNotGreaterThan.Convert<bool?>(new object(), 2));
            Assert.IsNull(ComparableConverters.IsSmallerThan.Convert<bool?>(new object(), 2));

            // min and max
            var random = new Random();
            var values = new int[10].Also(it =>
            {
                for (var i = it.Length - 1; i >= 0; --i)
                    it[i] = random.Next(int.MaxValue);
            });
            var sortedValues = ((int[])values.Clone()).Also(it =>
                Array.Sort(it));
            var objects = new object?[values.Length].Also(it =>
            {
                for (var i = it.Length - 1; i >= 0; --i)
                    it[i] = values[i];
            });
            Assert.AreEqual(sortedValues[0], ComparableConverters.Min.Convert(new object?[] { sortedValues[0] }, typeof(int), null, CultureInfo.InvariantCulture));
            Assert.AreEqual(sortedValues[0], ComparableConverters.Min.Convert(objects, typeof(int), null, CultureInfo.InvariantCulture));
            Assert.AreEqual(sortedValues[0], ComparableConverters.Min.Convert(objects, typeof(IComparable<int>), null, CultureInfo.InvariantCulture));
            Assert.AreEqual(sortedValues[0], ComparableConverters.Min.Convert(objects, typeof(object), null, CultureInfo.InvariantCulture));
            Assert.AreEqual(sortedValues[sortedValues.Length - 1], ComparableConverters.Max.Convert(new object?[] { sortedValues[sortedValues.Length - 1] }, typeof(int), null, CultureInfo.InvariantCulture));
            Assert.AreEqual(sortedValues[sortedValues.Length - 1], ComparableConverters.Max.Convert(objects, typeof(int), null, CultureInfo.InvariantCulture));
            Assert.AreEqual(sortedValues[sortedValues.Length - 1], ComparableConverters.Max.Convert(objects, typeof(IComparable<int>), null, CultureInfo.InvariantCulture));
            Assert.AreEqual(sortedValues[sortedValues.Length - 1], ComparableConverters.Max.Convert(objects, typeof(object), null, CultureInfo.InvariantCulture));
            Assert.IsNull(ComparableConverters.Min.Convert(objects, typeof(double), null, CultureInfo.InvariantCulture));
            Assert.IsNull(ComparableConverters.Max.Convert(new object?[] { new object(), new object() }, typeof(object), null, CultureInfo.InvariantCulture));
        }


        /// <summary>
        /// Test for <see cref="ObjectConverters"/>.
        /// </summary>
        [Test]
        public void ObjectConvertersTest()
        {
            // IsEquivalentTo
            Assert.IsTrue(ObjectConverters.IsEquivalentTo.Convert<bool>(1, 1));
            Assert.IsTrue((bool)ObjectConverters.IsEquivalentTo.Convert<object?>(1, 1)!);
            Assert.IsFalse(ObjectConverters.IsEquivalentTo.Convert<bool>(1, 2));
            Assert.IsFalse((bool)ObjectConverters.IsEquivalentTo.Convert<object?>(1, 2)!);
            Assert.IsNull(ObjectConverters.IsEquivalentTo.Convert<int?>(1, 1));

            // IsNotEquivalentTo
            Assert.IsTrue(ObjectConverters.IsNotEquivalentTo.Convert<bool>(1, 2));
            Assert.IsTrue((bool)ObjectConverters.IsNotEquivalentTo.Convert<object?>(1, 2)!);
            Assert.IsFalse(ObjectConverters.IsNotEquivalentTo.Convert<bool>(2, 2));
            Assert.IsFalse((bool)ObjectConverters.IsNotEquivalentTo.Convert<object?>(2, 2)!);
            Assert.IsNull(ObjectConverters.IsNotEquivalentTo.Convert<int?>(1, 1));
        }
    }
}