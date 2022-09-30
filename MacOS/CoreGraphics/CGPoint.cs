using System.Runtime.InteropServices;

namespace CarinaStudio.MacOS.CoreGraphics;

/// <summary>
/// CGPoint.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct CGPoint 
{
    /// <summary>
    /// Initialize <see cref="CGPoint"/> structure.
    /// </summary>
    /// <param name="x">X.</param>
    /// <param name="y">Y.</param>
    public CGPoint(double x, double y)
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