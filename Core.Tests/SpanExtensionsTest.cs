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
        Assert.That(empty.AsSpan().IsEmptyOrWhiteSpace());
        Assert.That(!empty.AsSpan().IsNotWhiteSpace());
        Assert.That(whiteSpaces.AsSpan().IsEmptyOrWhiteSpace());
        Assert.That(!whiteSpaces.AsSpan().IsNotWhiteSpace());
        Assert.That(!nonWhiteSpace1.AsSpan().IsEmptyOrWhiteSpace());
        Assert.That(nonWhiteSpace1.AsSpan().IsNotWhiteSpace());
        Assert.That(!nonWhiteSpace2.AsSpan().IsEmptyOrWhiteSpace());
        Assert.That(nonWhiteSpace2.AsSpan().IsNotWhiteSpace());
        Assert.That(!nonWhiteSpace3.AsSpan().IsEmptyOrWhiteSpace());
        Assert.That(nonWhiteSpace3.AsSpan().IsNotWhiteSpace());
        Assert.That(!nonWhiteSpace4.AsSpan().IsEmptyOrWhiteSpace());
        Assert.That(nonWhiteSpace4.AsSpan().IsNotWhiteSpace());
    }
}