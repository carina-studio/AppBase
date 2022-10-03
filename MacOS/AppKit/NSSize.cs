using System.Runtime.InteropServices;

namespace CarinaStudio.MacOS.AppKit;

/// <summary>
/// CGSize.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct NSSize
{
    /// <summary>
    /// Initialize <see cref="NSSize"/> structure.
    /// </summary>
    /// <param name="width">Width.</param>
    /// <param name="height">Height.</param>
    public NSSize(double width, double height)
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