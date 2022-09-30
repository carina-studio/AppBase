namespace CarinaStudio.MacOS.CoreGraphics;

#pragma warning disable CS1591

/// <summary>
/// CGImageSourceStatus.
/// </summary>
public enum CGImageSourceStatus : int
{
    UnexpectedEOF = -5,
    InvalidData = -4,
    UnknownType = -3,
    ReadingHeader = -2,
    Incomplete = -1,
    Complete = 0
}
