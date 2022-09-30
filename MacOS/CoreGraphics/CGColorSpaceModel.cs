namespace CarinaStudio.MacOS.CoreGraphics;

#pragma warning disable CS1591

/// <summary>
/// CGColorSpaceModel.
/// </summary>
public enum CGColorSpaceModel : int
{
    Unknown = -1,
    Monochrome,
    RGB,
    CMYK,
    Lab,
    DeviceN,
    Indexed,
    Pattern,
    XYZ
}