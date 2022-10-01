using System;
using System.Diagnostics.CodeAnalysis;

namespace CarinaStudio.MacOS.CoreGraphics;

/// <summary>
/// Represent size in pixels.
/// </summary>
public struct PixelSize : IEquatable<PixelSize>
{
    /// <summary>
    /// Initialize <see cref="PixelSize"/> structure.
    /// </summary>
    /// <param name="width">Width.</param>
    /// <param name="height">Height.</param>
    public PixelSize(int width, int height)
    {
        this.Width = width;
        this.Height = height;
    }


    /// <inheritdoc/>
    public bool Equals(PixelSize pixelSize) =>
        this.Width == pixelSize.Width
        && this.Height == pixelSize.Height;


    /// <inheritdoc/>
    public override bool Equals([NotNullWhen(true)] object? obj) =>
        obj is PixelSize pixelSize && this.Equals(pixelSize);


    /// <inheritdoc/>
    public override int GetHashCode() =>
        (this.Width << 16) | (this.Height & 0xffff);


    /// <summary>
    /// Height.
    /// </summary>
    public int Height { get; }


    /// <summary>
    /// Equality operator.
    /// </summary>
    public static bool operator ==(PixelSize l, PixelSize r) =>
        l.Equals(r);
    

    /// <summary>
    /// Inequality operator.
    /// </summary>
    public static bool operator !=(PixelSize l, PixelSize r) =>
        !l.Equals(r);


    /// <inheritdoc/>
    public override string ToString() =>
        $"{Width}x{Height}";


    /// <summary>
    /// Width.
    /// </summary>
    public int Width { get; }
}