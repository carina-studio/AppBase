using System;

namespace CarinaStudio.MacOS;

/// <summary>
/// Names of native libraries.
/// </summary>
public static class NativeLibraryNames
{
    /// <summary>
    /// AppKit.
    /// </summary>
    [Obsolete("The path to AppKit may be different between macOS, use NativeLibraryHandles.AppKit instead.")]
    public const string AppKit = "/System/Library/Frameworks/AppKit.framework/AppKit";
    /// <summary>
    /// Core Foundation.
    /// </summary>
    [Obsolete("The path to Core Foundation may be different between macOS, use NativeLibraryHandles.CoreFoundation instead.")]
    public const string CoreFoundation = "/System/Library/Frameworks/ApplicationServices.framework/Frameworks/CoreFoundation.framework/CoreFoundation";
    /// <summary>
    /// Core Graphics.
    /// </summary>
    [Obsolete("The path to Core Graphics may be different between macOS, use NativeLibraryHandles.CoreGraphics instead.")]
    public const string CoreGraphics = "/System/Library/Frameworks/ApplicationServices.framework/Frameworks/CoreGraphics.framework/CoreGraphics";
    /// <summary>
    /// Image I/O.
    /// </summary>
    [Obsolete("The path to ImageIO may be different between macOS, use NativeLibraryHandles.ImageIO instead.")]
    public const string ImageIO = "/System/Library/Frameworks/ApplicationServices.framework/Frameworks/ImageIO.framework/ImageIO";
    /// <summary>
    /// Objective-C Runtime.
    /// </summary>
    public const string ObjectiveC = "/usr/lib/libobjc.dylib";
    /// <summary>
    /// System.
    /// </summary>
    public const string System = "/usr/lib/libSystem.dylib";
}