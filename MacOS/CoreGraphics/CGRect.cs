using System.Runtime.InteropServices;

namespace CarinaStudio.MacOS.CoreGraphics;

/// <summary>
/// CGRect.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public class CGRect
{
    /// <summary>
    /// Initialize <see cref="CGRect"/> structure.
    /// </summary>
    /// <param name="origin">Origin.</param>
    /// <param name="size">Size.</param>
    public CGRect(CGPoint origin, CGSize size)
    {
        this.Origin = origin;
        this.Size = size;
    }


    /// <summary>
    /// Initialize <see cref="CGRect"/> structure.
    /// </summary>
    /// <param name="x">X.</param>
    /// <param name="y">Y.</param>
    /// <param name="width">Width.</param>
    /// <param name="height">Height.</param>
    public CGRect(double x, double y, double width, double height) : this(new(x, y), new(width, height))
    { }


    /// <inheritdoc/>
    public override string ToString() =>
        $"[{Origin}, {Size}]";


    /// <summary>
    /// Origin.
    /// </summary>
    public CGPoint Origin;


    /// <summary>
    /// Size.
    /// </summary>
    public CGSize Size;
}