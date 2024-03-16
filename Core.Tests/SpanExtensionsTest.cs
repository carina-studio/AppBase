using NUnit.Framework;
using System;

namespace CarinaStudio;

/// <summary>
/// Tests of <see cref="SpanExtensions"/>.
/// </summary>
[TestFixture]
public class SpanExtensionsTest
{
    /// <summary>
    /// Test for operations on character sequence.
    /// </summary>
    [Test]
    public void CharacterSequenceTest()
    {
        var empty = "";
        var whiteSpaces = "  ";
        var nonWhiteSpace1 = "123 456";
        var nonWhiteSpace2 = " 123456";
        var nonWhiteSpace3 = "123456 ";
        var nonWhiteSpace4 = "123";
        Assert.IsTrue(empty.AsSpan().IsEmptyOrWhiteSpace());
        Assert.IsFalse(empty.AsSpan().IsNotWhiteSpace());
        Assert.IsTrue(whiteSpaces.AsSpan().IsEmptyOrWhiteSpace());
        Assert.IsFalse(whiteSpaces.AsSpan().IsNotWhiteSpace());
        Assert.IsFalse(nonWhiteSpace1.AsSpan().IsEmptyOrWhiteSpace());
        Assert.IsTrue(nonWhiteSpace1.AsSpan().IsNotWhiteSpace());
        Assert.IsFalse(nonWhiteSpace2.AsSpan().IsEmptyOrWhiteSpace());
        Assert.IsTrue(nonWhiteSpace2.AsSpan().IsNotWhiteSpace());
        Assert.IsFalse(nonWhiteSpace3.AsSpan().IsEmptyOrWhiteSpace());
        Assert.IsTrue(nonWhiteSpace3.AsSpan().IsNotWhiteSpace());
        Assert.IsFalse(nonWhiteSpace4.AsSpan().IsEmptyOrWhiteSpace());
        Assert.IsTrue(nonWhiteSpace4.AsSpan().IsNotWhiteSpace());
    }
}