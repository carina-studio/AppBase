using System;

namespace CarinaStudio.MacOS.CoreGraphics;

#pragma warning disable CS1591

/// <summary>
/// CGBitmapInfo.
/// </summary>
[Flags]
public enum CGBitmapInfo : uint
{
    AlphaInfoMask = 0x1F,
    FloatComponents = (1 << 8),

    ByteOrderMask = 0x7000,
    ByteOrderDefault = (0 << 12),
    ByteOrder16Little = (1 << 12),
    ByteOrder32Little = (2 << 12),
    ByteOrder16Big = (3 << 12),
    ByteOrder32Big = (4 << 12),
}
