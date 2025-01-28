using System.Runtime.InteropServices;

namespace CarinaStudio.MacOS.CoreGraphics;

/// <summary>
/// Provide functions for displays.
/// </summary>
public static unsafe class Display
{
    // Native symbols.
    static readonly delegate*<uint, CGRect> CGDisplayBounds;
    static readonly delegate*<uint, nuint> CGDisplayPixelsHigh;
    static readonly delegate*<uint, nuint> CGDisplayPixelsWide;
    static readonly delegate*<uint, uint*, uint*, CGError> CGGetActiveDisplayList;
    static readonly delegate*<CGPoint, uint, uint*, uint*, CGError> CGGetDisplaysWithPoint;
    static readonly delegate*<CGRect, uint, uint*, uint*, CGError> CGGetDisplaysWithRect;
    static readonly delegate*<uint, uint*, uint*, CGError> CGGetOnlineDisplayList;
    static readonly delegate*<uint> CGMainDisplayID;
    
    
    // Static constructor.
    static Display()
    {
        if (Platform.IsNotMacOS)
            return;
        var libHandle = NativeLibraryHandles.CoreGraphics;
        CGDisplayBounds = (delegate*<uint, CGRect>)NativeLibrary.GetExport(libHandle, nameof(CGDisplayBounds));
        CGDisplayPixelsHigh = (delegate*<uint, nuint>)NativeLibrary.GetExport(libHandle, nameof(CGDisplayPixelsHigh));
        CGDisplayPixelsWide = (delegate*<uint, nuint>)NativeLibrary.GetExport(libHandle, nameof(CGDisplayPixelsWide));
        CGGetActiveDisplayList = (delegate*<uint, uint*, uint*, CGError>)NativeLibrary.GetExport(libHandle, nameof(CGGetActiveDisplayList));
        CGGetDisplaysWithPoint = (delegate*<CGPoint, uint, uint*, uint*, CGError>)NativeLibrary.GetExport(libHandle, nameof(CGGetDisplaysWithPoint));
        CGGetDisplaysWithRect = (delegate*<CGRect, uint, uint*, uint*, CGError>)NativeLibrary.GetExport(libHandle, nameof(CGGetDisplaysWithRect));
        CGGetOnlineDisplayList = (delegate*<uint, uint*, uint*, CGError>)NativeLibrary.GetExport(libHandle, nameof(CGGetOnlineDisplayList));
        CGMainDisplayID = (delegate*<uint>)NativeLibrary.GetExport(libHandle, nameof(CGMainDisplayID));
    }


    /// <summary>
    /// Invalid ID of display.
    /// </summary>
    public const uint Invalid = 0;


    /// <summary>
    /// Get all ID of active displays.
    /// </summary>
    /// <returns>ID of displays.</returns>
    public static uint[] GetActiveDisplays()
    {
        var displayCount = 0u;
        var result = CGGetActiveDisplayList(0, null, &displayCount);
        if (result != CGError.Success)
            throw result.ToException();
        if (displayCount == 0)
            return [];
        var displays = new uint[displayCount];
        fixed (uint* displaysPtr = displays)
            result = CGGetActiveDisplayList(displayCount, displaysPtr, &displayCount);
        if (result == CGError.Success)
            return displays;
        throw result.ToException();
    }


    /// <summary>
    /// Get bounds of given display.
    /// </summary>
    /// <param name="display">ID of display.</param>
    /// <returns>Bounds of display.</returns>
    public static CGRect GetDisplayBounds(uint display) => 
        CGDisplayBounds(display);


    /// <summary>
    /// Get ID of display which contains the given point.
    /// </summary>
    /// <param name="point">Point.</param>
    /// <returns>ID of display.</returns>
    public static uint GetDisplayFromPoint(CGPoint point)
    {
        var displays = 0u;
        var displayCount = 0u;
        var result = CGGetDisplaysWithPoint(point, 1, &displays, &displayCount);
        if (result != CGError.Success)
            throw result.ToException();
        if (displayCount == 1)
            return displays;
        return Invalid;
    }


    /// <summary>
    /// Get ID of display which contains the given rectangle.
    /// </summary>
    /// <param name="rect">Rectangle.</param>
    /// <returns>ID of display.</returns>
    public static uint GetDisplayFromRect(CGRect rect)
    {
        var displays = 0u;
        var displayCount = 0u;
        var result = CGGetDisplaysWithRect(rect, 1, &displays, &displayCount);
        if (result != CGError.Success)
            throw result.ToException();
        if (displayCount == 1)
            return displays;
        return Invalid;
    }


    /// <summary>
    /// Get list of ID of displays which contain the given point.
    /// </summary>
    /// <param name="point">Point.</param>
    /// <returns>List of ID of displays.</returns>
    public static uint[] GetDisplaysFromPoint(CGPoint point)
    {
        var displayCount = 0u;
        var result = CGGetDisplaysWithPoint(point, 0, null, &displayCount);
        if (result != CGError.Success)
            throw result.ToException();
        if (displayCount == 0)
            return [];
        var displays = new uint[displayCount];
        fixed (uint* displaysPtr = displays)
            result = CGGetDisplaysWithPoint(point, displayCount, displaysPtr, &displayCount);
        if (result == CGError.Success)
            return displays;
        throw result.ToException();
    }


    /// <summary>
    /// Get list of ID of displays which contain the given rectangle.
    /// </summary>
    /// <param name="rect">Rectangle.</param>
    /// <returns>List of ID of displays.</returns>
    public static uint[] GetDisplaysFromRect(CGRect rect)
    {
        var displayCount = 0u;
        var result = CGGetDisplaysWithRect(rect, 0, null, &displayCount);
        if (result != CGError.Success)
            throw result.ToException();
        if (displayCount == 0)
            return [];
        var displays = new uint[displayCount];
        fixed (uint* displaysPtr = displays)
            result = CGGetDisplaysWithRect(rect, displayCount, displaysPtr, &displayCount);
        if (result == CGError.Success)
            return displays;
        throw result.ToException();
    }


    /// <summary>
    /// Get ID of main display.
    /// </summary>
    /// <returns>ID of main display.</returns>
	public static uint GetMainDisplay() => 
        CGMainDisplayID();


    /// <summary>
    /// Get all ID of online displays.
    /// </summary>
    /// <returns>ID of displays.</returns>
    public static uint[] GetOnlineDisplays()
    {
        var displayCount = 0u;
        var result = CGGetOnlineDisplayList(0, null, &displayCount);
        if (result != CGError.Success)
            throw result.ToException();
        if (displayCount == 0)
            return [];
        var displays = new uint[displayCount];
        fixed (uint* displaysPtr = displays) 
            result = CGGetOnlineDisplayList(displayCount, displaysPtr, &displayCount);
        if (result == CGError.Success)
            return displays;
        throw result.ToException();
    }
}