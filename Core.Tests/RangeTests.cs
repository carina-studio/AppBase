using NUnit.Framework;
using System;

namespace CarinaStudio
{
    /// <summary>
    /// Tests of <see cref="Range{T}"/>.
    /// </summary>
    [TestFixture]
    class RangeTests
    {
        /// <summary>
        /// Test for value and range containing.
        /// </summary>
        [Test]
        public void ContainingTest()
        {
            // empty range contains value
            var range = Range<int>.Empty;
            Assert.IsFalse(range.Contains(0));
            Assert.IsFalse(range.Contains(int.MaxValue));
            Assert.IsFalse(range.Contains(int.MinValue));
            range = range.Offsets(1234);
            Assert.IsFalse(range.Contains(0));
            Assert.IsFalse(range.Contains(int.MaxValue));
            Assert.IsFalse(range.Contains(int.MinValue));

            // empty range contains range
            range = Range<int>.Empty;
            Assert.IsFalse(range.Contains(Range<int>.Empty));
            Assert.IsFalse(range.Contains(Range<int>.Universal));

            // open range contains value
            range = new Range<int>(0, null);
            Assert.IsTrue(range.Contains(0));
            Assert.IsTrue(range.Contains(int.MaxValue));
            Assert.IsFalse(range.Contains(-1));
            range = new Range<int>(null, 0);
            Assert.IsFalse(range.Contains(0));
            Assert.IsFalse(range.Contains(int.MaxValue));
            Assert.IsTrue(range.Contains(-1));

            // open range contains range
            range = new Range<int>(0, null);
            Assert.IsTrue(range.Contains(new Range<int>(0, 1)));
            Assert.IsFalse(range.Contains(new Range<int>(0, null)));
            Assert.IsTrue(range.ContainsOrEquals(new Range<int>(0, null)));
            Assert.IsFalse(range.Contains(new Range<int>(int.MinValue, 1)));
            Assert.IsFalse(range.Contains(new Range<int>(int.MinValue, 0)));
            range = new Range<int>(null, 0);
            Assert.IsTrue(range.Contains(new Range<int>(-2, -1)));
            Assert.IsFalse(range.Contains(new Range<int>(null, 0)));
            Assert.IsTrue(range.ContainsOrEquals(new Range<int>(null, 0)));
            Assert.IsFalse(range.Contains(new Range<int>(-1, int.MaxValue)));
            Assert.IsFalse(range.Contains(new Range<int>(0, int.MaxValue)));

            // range contains value
            range = new Range<int>(-100, 10);
            Assert.IsTrue(range.Contains(-100));
            Assert.IsTrue(range.Contains(9));
            Assert.IsFalse(range.Contains(-101));
            Assert.IsFalse(range.Contains(10));
            range = range.Offsets(90);
            Assert.IsTrue(range.Contains(-10));
            Assert.IsTrue(range.Contains(99));
            Assert.IsFalse(range.Contains(-11));
            Assert.IsFalse(range.Contains(100));

            // range contains range
            range = new Range<int>(-100, 10);
            Assert.IsTrue(range.Contains(new Range<int>(-100, 0)));
            Assert.IsTrue(range.Contains(new Range<int>(0, 9)));
            Assert.IsFalse(range.Contains(new Range<int>(-200, -100)));
            Assert.IsFalse(range.Contains(new Range<int>(-200, -101)));
            Assert.IsFalse(range.Contains(new Range<int>(-200, -99)));
            Assert.IsFalse(range.Contains(new Range<int>(-200, 10)));
            Assert.IsFalse(range.Contains(new Range<int>(10, 200)));
            Assert.IsFalse(range.Contains(new Range<int>(11, 200)));
            Assert.IsFalse(range.Contains(new Range<int>(9, 200)));
            Assert.IsFalse(range.Contains(new Range<int>(-100, 200)));
            range = range.Offsets(90);
            Assert.IsTrue(range.Contains(new Range<int>(-10, 0)));
            Assert.IsTrue(range.Contains(new Range<int>(0, 99)));
            Assert.IsFalse(range.Contains(new Range<int>(100, 200)));
            Assert.IsFalse(range.Contains(new Range<int>(101, 200)));
            Assert.IsFalse(range.Contains(new Range<int>(99, 200)));
            Assert.IsFalse(range.Contains(new Range<int>(-100, 200)));
            Assert.IsFalse(range.Contains(new Range<int>(-11, 10)));
            Assert.IsFalse(range.Contains(new Range<int>(-100, 200)));
        }


        /// <summary>
        /// Test for intersection check and operation.
        /// </summary>
        [Test]
        public void IntersectionTest()
        {
            // intersect ranges
            var range1 = new Range<int>(0, 100);
            var range2 = new Range<int>(-10, 90);
            Assert.IsTrue(range1.IsIntersectedWith(range2));
            Assert.IsTrue(range2.IsIntersectedWith(range1));
            Assert.IsTrue(range1.IntersectsWith(range2) == (0, 90));
            Assert.IsTrue(range2.IntersectsWith(range1) == (0, 90));
            range2 = (-100, 0);
            Assert.IsFalse(range1.IsIntersectedWith(range2));
            Assert.IsFalse(range2.IsIntersectedWith(range1));
            Assert.IsTrue(range1.IntersectsWith(range2).IsEmpty);
            Assert.IsTrue(range2.IntersectsWith(range1).IsEmpty);
            range2 = (10, 90);
            Assert.IsTrue(range1.IsIntersectedWith(range2));
            Assert.IsTrue(range2.IsIntersectedWith(range1));
            Assert.IsTrue(range1.IntersectsWith(range2) == (10, 90));
            Assert.IsTrue(range2.IntersectsWith(range1) == (10, 90));

            // intersect open ranges
            range1 = (0, null);
            range2 = (null, 100);
            Assert.IsTrue(range1.IsIntersectedWith(range2));
            Assert.IsTrue(range2.IsIntersectedWith(range1));
            Assert.IsTrue(range1.IntersectsWith(range2) == (0, 100));
            Assert.IsTrue(range2.IntersectsWith(range1) == (0, 100));
            range2 = (null, 0);
            Assert.IsFalse(range1.IsIntersectedWith(range2));
            Assert.IsFalse(range2.IsIntersectedWith(range1));
            Assert.IsTrue(range1.IntersectsWith(range2).IsEmpty);
            Assert.IsTrue(range2.IntersectsWith(range1).IsEmpty);
            range2 = (10, 100);
            Assert.IsTrue(range1.IsIntersectedWith(range2));
            Assert.IsTrue(range2.IsIntersectedWith(range1));
            Assert.IsTrue(range1.IntersectsWith(range2) == (10, 100));
            Assert.IsTrue(range2.IntersectsWith(range1) == (10, 100));
            range2 = (-100, 100);
            Assert.IsTrue(range1.IsIntersectedWith(range2));
            Assert.IsTrue(range2.IsIntersectedWith(range1));
            Assert.IsTrue(range1.IntersectsWith(range2) == (0, 100));
            Assert.IsTrue(range2.IntersectsWith(range1) == (0, 100));
            range2 = (-100, 0);
            Assert.IsFalse(range1.IsIntersectedWith(range2));
            Assert.IsFalse(range2.IsIntersectedWith(range1));
            Assert.IsTrue(range1.IntersectsWith(range2).IsEmpty);
            Assert.IsTrue(range2.IntersectsWith(range1).IsEmpty);
            range1 = (null, 0);
            range2 = (-100, -10);
            Assert.IsTrue(range1.IsIntersectedWith(range2));
            Assert.IsTrue(range2.IsIntersectedWith(range1));
            Assert.IsTrue(range1.IntersectsWith(range2) == (-100, -10));
            Assert.IsTrue(range2.IntersectsWith(range1) == (-100, -10));
            range2 = (-100, 100);
            Assert.IsTrue(range1.IsIntersectedWith(range2));
            Assert.IsTrue(range2.IsIntersectedWith(range1));
            Assert.IsTrue(range1.IntersectsWith(range2) == (-100, 0));
            Assert.IsTrue(range2.IntersectsWith(range1) == (-100, 0));
            range2 = (0, 100);
            Assert.IsFalse(range1.IsIntersectedWith(range2));
            Assert.IsFalse(range2.IsIntersectedWith(range1));
            Assert.IsTrue(range1.IntersectsWith(range2).IsEmpty);
            Assert.IsTrue(range2.IntersectsWith(range1).IsEmpty);

            // intersect universal range
            range1 = Range<int>.Universal;
            range2 = (-100, 100);
            Assert.IsFalse(range1.IsIntersectedWith(Range<int>.Empty));
            Assert.IsFalse(Range<int>.Empty.IsIntersectedWith(range1));
            Assert.IsTrue(range1.IsIntersectedWith(range2));
            Assert.IsTrue(range2.IsIntersectedWith(range1));
            Assert.IsTrue(range1.IntersectsWith(range2) == range2);
            Assert.IsTrue(range2.IntersectsWith(range1) == range2);
            Assert.IsTrue(range1.IsIntersectedWith(Range<int>.Universal));
            Assert.IsTrue(range1.IntersectsWith(Range<int>.Universal).IsUniversal);

            // intersecs empty ranges
            Assert.IsFalse(Range<int>.Empty.IsIntersectedWith(Range<int>.Empty));
            Assert.IsTrue(Range<int>.Empty.IntersectsWith(Range<int>.Empty).IsEmpty);
        }


        /// <summary>
        /// Test for offsetting ranges.
        /// </summary>
        [Test]
        public void OffsettingTest()
        {
            // byte
            var length = 10;
            var byteRange = new Range<byte>(10, (byte)(10 + length));
            byteRange = byteRange.Offsets(byte.MinValue - 10);
            Assert.IsTrue(byteRange.Equals(new Range<byte>(byte.MinValue, (byte)(byte.MinValue + length))));
            byteRange = byteRange.Offsets(byte.MaxValue - length);
            Assert.IsTrue(byteRange.Equals(new Range<byte>((byte)(byte.MaxValue - length), byte.MaxValue)));
            try
            {
                byteRange.Offsets(1);
                throw new AssertionException("Overflow expected.");
            }
            catch (OverflowException)
            { }
            try
            {
                byteRange.Offsets(-(byte.MaxValue - byte.MinValue + 1));
                throw new AssertionException("Overflow expected.");
            }
            catch (OverflowException)
            { }

            // short
            var shortRange = new Range<short>(10, (short)(10 + length));
            shortRange = shortRange.Offsets(short.MinValue - 10);
            Assert.IsTrue(shortRange.Equals(new Range<short>(short.MinValue, (short)(short.MinValue + length))));
            shortRange = shortRange.Offsets((short.MaxValue - short.MinValue) - length);
            Assert.IsTrue(shortRange.Equals(new Range<short>((short)(short.MaxValue - length), short.MaxValue)));
            try
            {
                shortRange.Offsets(1);
                throw new AssertionException("Overflow expected.");
            }
            catch (OverflowException)
            { }
            try
            {
                shortRange.Offsets(-(short.MaxValue - short.MinValue + 1));
                throw new AssertionException("Overflow expected.");
            }
            catch (OverflowException)
            { }

            // int
            var intRange = new Range<int>(10, (10 + length));
            intRange = intRange.Offsets((long)int.MinValue - 10);
            Assert.IsTrue(intRange.Equals(new Range<int>(int.MinValue, (int)(int.MinValue + length))));
            intRange = intRange.Offsets(((long)int.MaxValue - int.MinValue) - length);
            Assert.IsTrue(intRange.Equals(new Range<int>((int)(int.MaxValue - length), int.MaxValue)));
            try
            {
                intRange.Offsets(1);
                throw new AssertionException("Overflow expected.");
            }
            catch (OverflowException)
            { }
            try
            {
                intRange.Offsets(-((long)int.MaxValue - int.MinValue + 1));
                throw new AssertionException("Overflow expected.");
            }
            catch (OverflowException)
            { }

            // long
            var longRange = new Range<long>(long.MaxValue - length - 10, long.MaxValue - 10);
            longRange = longRange.Offsets(10);
            Assert.IsTrue(longRange.Equals(new Range<long>(long.MaxValue - length, long.MaxValue)));
            longRange = new Range<long>(long.MinValue + 10, long.MinValue + length + 10);
            longRange = longRange.Offsets(-10);
            Assert.IsTrue(longRange.Equals(new Range<long>(long.MinValue, long.MinValue + length)));
            try
            {
                longRange = new Range<long>(long.MaxValue - length, long.MaxValue);
                longRange.Offsets(1);
                throw new AssertionException("Overflow expected.");
            }
            catch (OverflowException)
            { }
            try
            {
                longRange = new Range<long>(long.MinValue, long.MinValue + length);
                longRange.Offsets(-1);
                throw new AssertionException("Overflow expected.");
            }
            catch (OverflowException)
            { }

            // ulong
            var ulongRange = new Range<ulong>(ulong.MaxValue - (ulong)length - 10, ulong.MaxValue - 10);
            ulongRange = ulongRange.Offsets(10);
            Assert.IsTrue(ulongRange.Equals(new Range<ulong>(ulong.MaxValue - (ulong)length, ulong.MaxValue)));
            ulongRange = new Range<ulong>(ulong.MinValue + 10, ulong.MinValue + (ulong)length + 10);
            ulongRange = ulongRange.Offsets(-10);
            Assert.IsTrue(ulongRange.Equals(new Range<ulong>(ulong.MinValue, ulong.MinValue + (ulong)length)));
            try
            {
                ulongRange = new Range<ulong>(ulong.MaxValue - (ulong)length, ulong.MaxValue);
                ulongRange.Offsets(1);
                throw new AssertionException("Overflow expected.");
            }
            catch (OverflowException)
            { }
            try
            {
                ulongRange = new Range<ulong>(ulong.MinValue, ulong.MinValue + (ulong)length);
                ulongRange.Offsets(-1);
                throw new AssertionException("Overflow expected.");
            }
            catch (OverflowException)
            { }
        }
    }
}