namespace CarinaStudio.MacOS.CoreGraphics;

#pragma warning disable CS1591

/// <summary>
/// CGImagePixelFormatInfo.
/// </summary>
public enum CGImagePixelFormatInfo : uint
{
    Mask = 0xF0000,
    Packed = (0 << 16),
    RGB555 = (1 << 16),
    RGB565 = (2 << 16),
    RGB101010 = (3 << 16),
    RGBCIF10 = (4 << 16),
}