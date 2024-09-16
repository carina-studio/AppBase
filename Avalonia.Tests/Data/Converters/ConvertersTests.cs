using NUnit.Framework;
using System;
using System.Globalization;

namespace CarinaStudio.Data.Converters;

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
                Assert.That((x > y) == ComparableConverters.IsGreaterThan.Convert<bool>(x, y));
                Assert.That((x >= y) == ComparableConverters.IsNotSmallerThan.Convert<bool>(x, y));
                Assert.That((x <= y) == ComparableConverters.IsNotGreaterThan.Convert<bool>(x, y));
                Assert.That((x < y) == ComparableConverters.IsSmallerThan.Convert<bool>(x, y));

                // to object
                Assert.That((x > y) == (bool)ComparableConverters.IsGreaterThan.Convert<object?>(x, y)!);
                Assert.That((x >= y) == (bool)ComparableConverters.IsNotSmallerThan.Convert<object?>(x, y)!);
                Assert.That((x <= y) == (bool)ComparableConverters.IsNotGreaterThan.Convert<object?>(x, y)!);
                Assert.That((x < y) == (bool)ComparableConverters.IsSmallerThan.Convert<object?>(x, y)!);
            }
        }
        Assert.That(ComparableConverters.IsGreaterThan.Convert<int?>(1, 2) is null);
        Assert.That(ComparableConverters.IsNotSmallerThan.Convert<int?>(1, 2) is null);
        Assert.That(ComparableConverters.IsNotGreaterThan.Convert<int?>(1, 2) is null);
        Assert.That(ComparableConverters.IsSmallerThan.Convert<int?>(1, 2) is null);
        Assert.That(ComparableConverters.IsGreaterThan.Convert<bool?>(1, new object()) is null);
        Assert.That(ComparableConverters.IsNotSmallerThan.Convert<bool?>(1, new object()) is null);
        Assert.That(ComparableConverters.IsNotGreaterThan.Convert<bool?>(new object(), 2) is null);
        Assert.That(ComparableConverters.IsSmallerThan.Convert<bool?>(new object(), 2) is null);

        // min and max
        var random = new Random();
        var values = new int[10].Also(it =>
        {
            for (var i = it.Length - 1; i >= 0; --i)
                it[i] = random.Next(int.MaxValue);
        });
        var sortedValues = ((int[])values.Clone()).Also(Array.Sort);
        var objects = new object?[values.Length].Also(it =>
        {
            for (var i = it.Length - 1; i >= 0; --i)
                it[i] = values[i];
        });
        Assert.That(sortedValues[0] == (int)ComparableConverters.Min.Convert(new object?[] { sortedValues[0] }, typeof(int), null, CultureInfo.InvariantCulture)!);
        Assert.That(sortedValues[0] == (int)ComparableConverters.Min.Convert(objects, typeof(int), null, CultureInfo.InvariantCulture)!);
        Assert.That(sortedValues[0] == (int)ComparableConverters.Min.Convert(objects, typeof(IComparable<int>), null, CultureInfo.InvariantCulture)!);
        Assert.That(sortedValues[0] == (int)ComparableConverters.Min.Convert(objects, typeof(object), null, CultureInfo.InvariantCulture)!);
        Assert.That(sortedValues[^1] == (int)ComparableConverters.Max.Convert(new object?[] { sortedValues[^1] }, typeof(int), null, CultureInfo.InvariantCulture)!);
        Assert.That(sortedValues[^1] == (int)ComparableConverters.Max.Convert(objects, typeof(int), null, CultureInfo.InvariantCulture)!);
        Assert.That(sortedValues[^1] == (int)ComparableConverters.Max.Convert(objects, typeof(IComparable<int>), null, CultureInfo.InvariantCulture)!);
        Assert.That(sortedValues[^1] == (int)ComparableConverters.Max.Convert(objects, typeof(object), null, CultureInfo.InvariantCulture)!);
        Assert.That(ComparableConverters.Min.Convert(objects, typeof(double), null, CultureInfo.InvariantCulture) is null);
        Assert.That(ComparableConverters.Max.Convert(new[] { new object(), new object() }, typeof(object), null, CultureInfo.InvariantCulture) is null);
    }


    /// <summary>
    /// Test for <see cref="ObjectConverters"/>.
    /// </summary>
    [Test]
    public void ObjectConvertersTest()
    {
        // IsEquivalentTo
        Assert.That(ObjectConverters.IsEquivalentTo.Convert<bool>(1, 1));
        Assert.That((bool)ObjectConverters.IsEquivalentTo.Convert<object?>(1, 1)!);
        Assert.That(!ObjectConverters.IsEquivalentTo.Convert<bool>(1, 2));
        Assert.That(!(bool)ObjectConverters.IsEquivalentTo.Convert<object?>(1, 2)!);
        Assert.That(ObjectConverters.IsEquivalentTo.Convert<int?>(1, 1) is null);

        // IsNotEquivalentTo
        Assert.That(ObjectConverters.IsNotEquivalentTo.Convert<bool>(1, 2));
        Assert.That((bool)ObjectConverters.IsNotEquivalentTo.Convert<object?>(1, 2)!);
        Assert.That(!ObjectConverters.IsNotEquivalentTo.Convert<bool>(2, 2));
        Assert.That(!(bool)ObjectConverters.IsNotEquivalentTo.Convert<object?>(2, 2)!);
        Assert.That(ObjectConverters.IsNotEquivalentTo.Convert<int?>(1, 1) is null);
    }
}