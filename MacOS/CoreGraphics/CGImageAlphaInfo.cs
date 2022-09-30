namespace CarinaStudio.MacOS.CoreGraphics;

#pragma warning disable CS1591

/// <summary>
/// CGImageAlphaInfo.
/// </summary>
public enum CGImageAlphaInfo : uint
{
    AlphaNone,               /* For example, RGB. */
    AlphaPremultipliedLast,  /* For example, premultiplied RGBA */
    AlphaPremultipliedFirst, /* For example, premultiplied ARGB */
    AlphaLast,               /* For example, non-premultiplied RGBA */
    AlphaFirst,              /* For example, non-premultiplied ARGB */
    AlphaNoneSkipLast,       /* For example, RGBX. */
    AlphaNoneSkipFirst,      /* For example, XRGB. */
    AlphaOnly                /* No color data, alpha data only */
}
