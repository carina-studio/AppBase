using NUnit.Framework;
using System;
using System.Globalization;

namespace CarinaStudio.Controls;

/// <summary>
/// Tests for <see cref="RealNumberTextBox"/>.
/// </summary>
[TestFixture]
class RealNumberTextBoxTests
{
    /// <summary>
    /// Test for <see cref="RealNumberTextBox.CalculateActualRange"/>.
    /// </summary>
    [Test]
    public void CalculateActualRangeTest()
    {
        // specific cases
        Assert.That(RealNumberTextBox.CalculateActualRange(0.123, 9.999, -1), Is.EqualTo((0.123, 9.999)));
        Assert.That(RealNumberTextBox.CalculateActualRange(0.123, 9.999, 2), Is.EqualTo((0.13, 9.99)));
        Assert.That(RealNumberTextBox.CalculateActualRange(-9.999, -0.123, 2), Is.EqualTo((-9.99, -0.13)));
        Assert.That(RealNumberTextBox.CalculateActualRange(0.1, 0.3, 1), Is.EqualTo((0.1, 0.3)));
        Assert.That(RealNumberTextBox.CalculateActualRange(0.001, 0.009, 2), Is.EqualTo((0.01, 0.01)));
        Assert.That(RealNumberTextBox.CalculateActualRange(double.MinValue, double.MaxValue, 2), Is.EqualTo((double.MinValue, double.MaxValue)));

        // general cases
        var bounds = new[]
        {
            double.MinValue,
            -1e15,
            -123.456,
            -1.15,
            -0.1,
            0.0,
            0.1,
            0.123,
            1.15,
            9.999,
            123.456,
            1e15,
            double.MaxValue,
        };
        foreach (var minimum in bounds)
        {
            foreach (var maximum in bounds)
            {
                if (minimum > maximum)
                    continue;
                for (var decimalPlaces = 0; decimalPlaces <= 15; ++decimalPlaces)
                {
                    var (actualMinimum, actualMaximum) = RealNumberTextBox.CalculateActualRange(minimum, maximum, decimalPlaces);
                    var message = $"Range: [{minimum}, {maximum}], decimal places: {decimalPlaces}, actual range: [{actualMinimum}, {actualMaximum}]";
                    Assert.That(actualMinimum, Is.GreaterThanOrEqualTo(minimum), message);
                    Assert.That(actualMinimum, Is.LessThanOrEqualTo(actualMaximum), message);
                    if (actualMaximum != actualMinimum)
                        Assert.That(actualMaximum, Is.LessThanOrEqualTo(maximum), message);
                    Assert.That(RealNumberTextBox.RoundToDecimalPlaces(actualMinimum, decimalPlaces), Is.EqualTo(actualMinimum), message);
                    Assert.That(RealNumberTextBox.RoundToDecimalPlaces(actualMaximum, decimalPlaces), Is.EqualTo(actualMaximum), message);
                }
            }
        }
    }


    /// <summary>
    /// Test for <see cref="RealNumberTextBox.PadDecimalPlaces"/>.
    /// </summary>
    [Test]
    public void PadDecimalPlacesTest()
    {
        var values = new[]
        {
            0.0,
            -0.0,
            1.0,
            -1.5,
            0.1 + 0.2,
            1e-5,
            1e20,
            double.Epsilon,
            double.MaxValue,
            double.MinValue,
            double.NaN,
        };
        foreach (var cultureName in new[] { "en-US", "de-DE", "fr-FR" })
        {
            var format = CultureInfo.GetCultureInfo(cultureName).NumberFormat;
            foreach (var value in values)
            {
                var originalText = value.ToString(format);
                for (var minDecimalPlaces = 0; minDecimalPlaces <= 5; ++minDecimalPlaces)
                {
                    var text = RealNumberTextBox.PadDecimalPlaces(value, minDecimalPlaces, format);
                    var message = $"Culture: {cultureName}, value: {originalText}, min decimal places: {minDecimalPlaces}, text: {text}";

                    // check conversion back to value
                    Assert.That(double.TryParse(text, NumberStyles.Float | NumberStyles.AllowThousands, format, out var parsedValue), message);
                    Assert.That(BitConverter.DoubleToInt64Bits(parsedValue), Is.EqualTo(BitConverter.DoubleToInt64Bits(value)), message);

                    // check decimal places
                    if (minDecimalPlaces == 0 || !double.IsFinite(value) || originalText.Contains('E', StringComparison.OrdinalIgnoreCase))
                        Assert.That(text, Is.EqualTo(originalText), message);
                    else
                    {
                        var separatorIndex = text.IndexOf(format.NumberDecimalSeparator, StringComparison.Ordinal);
                        Assert.That(separatorIndex, Is.GreaterThanOrEqualTo(0), message);
                        Assert.That(text.Length - separatorIndex - format.NumberDecimalSeparator.Length, Is.GreaterThanOrEqualTo(minDecimalPlaces), message);
                        Assert.That(text, Does.StartWith(originalText), message);
                    }
                }
            }
        }
    }


    /// <summary>
    /// Test for <see cref="RealNumberTextBox.RoundToDecimalPlaces"/>.
    /// </summary>
    [Test]
    public void RoundToDecimalPlacesTest()
    {
        // specific cases
        Assert.That(RealNumberTextBox.RoundToDecimalPlaces(1.234, -1), Is.EqualTo(1.234));
        Assert.That(RealNumberTextBox.RoundToDecimalPlaces(1.234, 2), Is.EqualTo(1.23));
        Assert.That(RealNumberTextBox.RoundToDecimalPlaces(-1.236, 2), Is.EqualTo(-1.24));
        Assert.That(RealNumberTextBox.RoundToDecimalPlaces(2.5, 0), Is.EqualTo(3.0));
        Assert.That(RealNumberTextBox.RoundToDecimalPlaces(-2.5, 0), Is.EqualTo(-3.0));
        Assert.That(RealNumberTextBox.RoundToDecimalPlaces(0.1 + 0.2, 2), Is.EqualTo(0.3));
        Assert.That(RealNumberTextBox.RoundToDecimalPlaces(Math.PI, 4), Is.EqualTo(3.1416));
        Assert.That(double.IsNaN(RealNumberTextBox.RoundToDecimalPlaces(double.NaN, 2)));
        Assert.That(RealNumberTextBox.RoundToDecimalPlaces(double.MaxValue, 2), Is.EqualTo(double.MaxValue));
        Assert.That(RealNumberTextBox.RoundToDecimalPlaces(double.MinValue, 2), Is.EqualTo(double.MinValue));

        // rounding rounded value
        foreach (var value in new[] { -123.456789, -0.1, 0.0, 1e-5, 0.1 + 0.2, 1.15, Math.E, 98765.4321 })
        {
            for (var decimalPlaces = 0; decimalPlaces <= 15; ++decimalPlaces)
            {
                var roundedValue = RealNumberTextBox.RoundToDecimalPlaces(value, decimalPlaces);
                Assert.That(RealNumberTextBox.RoundToDecimalPlaces(roundedValue, decimalPlaces), Is.EqualTo(roundedValue), $"Value: {value}, decimal places: {decimalPlaces}");
            }
        }
    }
}
