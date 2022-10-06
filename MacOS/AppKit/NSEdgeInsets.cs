using System.Runtime.InteropServices;

namespace CarinaStudio.MacOS.AppKit;

/// <summary>
/// NSEdgeInsets.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct NSEdgeInsets
{
    /// <summary>
    /// Initialize <see cref="NSEdgeInsets"/> structure.
    /// </summary>
    /// <param name="inset">Inset for left, top, right and bottom.</param>
    public NSEdgeInsets(double inset)
    {
        this.Left = inset;
        this.Top = inset;
        this.Right = inset;
        this.Bottom = inset;
    }


    /// <summary>
    /// Initialize <see cref="NSEdgeInsets"/> structure.
    /// </summary>
    /// <param name="left">Left inset.</param>
    /// <param name="top">Top inset.</param>
    /// <param name="right">Right inset.</param>
    /// <param name="bottom">Bottom inset.</param>
    public NSEdgeInsets(double left, double top, double right, double bottom)
    {
        this.Left = left;
        this.Top = top;
        this.Right = right;
        this.Bottom = bottom;
    }


    /// <summary>
    /// Bottom inset.
    /// </summary>
    public double Bottom;


    /// <summary>
    /// Left inset.
    /// </summary>
    public double Left;


    /// <summary>
    /// Right inset.
    /// </summary>
    public double Right;


    /// <summary>
    /// Top inset.
    /// </summary>
    public double Top;


    /// <inheritdoc/>
    public override string ToString() =>
        $"[L:{Left}, T:{Top}, R:{Right}, B:{Bottom}]";
}
