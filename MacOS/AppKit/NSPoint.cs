using System.Runtime.InteropServices;

namespace CarinaStudio.MacOS.AppKit;

/// <summary>
/// NSPoint.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct NSPoint 
{
    /// <summary>
    /// Initialize <see cref="NSPoint"/> structure.
    /// </summary>
    /// <param name="x">X.</param>
    /// <param name="y">Y.</param>
    public NSPoint(double x, double y)
    {
        this.X = x;
        this.Y = y;
    }


    /// <inheritdoc/>
    public override string ToString() =>
        $"({X}, {Y})";


    /// <summary>
    /// X.
    /// </summary>
    public double X;


    /// <summary>
    /// Y.
    /// </summary>
    public double Y;
}