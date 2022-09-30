using System.Runtime.InteropServices;

namespace CarinaStudio.MacOS.CoreGraphics;

/// <summary>
/// CGSize.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct CGSize
{
    /// <summary>
    /// Initialize <see cref="CGSize"/> structure.
    /// </summary>
    /// <param name="width">Width.</param>
    /// <param name="height">Height.</param>
    public CGSize(double width, double height)
    {
        this.Width = width;
        this.Height = height;
    }


    /// <inheritdoc/>
    public override string ToString() =>
        $"{Width}x{Height}";


    /// <summary>
    /// Width.
    /// </summary>
    public double Width;


    /// <summary>
    /// Height.
    /// </summary>
    public double Height;
}