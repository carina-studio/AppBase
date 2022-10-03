using System.Runtime.InteropServices;

namespace CarinaStudio.MacOS.AppKit;

/// <summary>
/// NSRect.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct NSRect
{
    /// <summary>
    /// Initialize <see cref="NSRect"/> structure.
    /// </summary>
    /// <param name="origin">Origin.</param>
    /// <param name="size">Size.</param>
    public NSRect(NSPoint origin, NSSize size)
    {
        this.Origin = origin;
        this.Size = size;
    }


    /// <summary>
    /// Initialize <see cref="NSRect"/> structure.
    /// </summary>
    /// <param name="x">X.</param>
    /// <param name="y">Y.</param>
    /// <param name="width">Width.</param>
    /// <param name="height">Height.</param>
    public NSRect(double x, double y, double width, double height) : this(new(x, y), new(width, height))
    { }


    /// <inheritdoc/>
    public override string ToString() =>
        $"[{Origin}, {Size}]";


    /// <summary>
    /// Origin.
    /// </summary>
    public NSPoint Origin;


    /// <summary>
    /// Size.
    /// </summary>
    public NSSize Size;
}