using System.Runtime.InteropServices;

namespace CarinaStudio.MacOS.CoreGraphics;

/// <summary>
/// Provide functions for displays.
/// </summary>
public static class Display
{
    // Native symbols.
    [DllImport(NativeLibraryNames.CoreGraphics)]
    static extern CGError CGGetActiveDisplayList(uint maxDisplays, [MarshalAs(UnmanagedType.LPArray)] uint[]? activeDisplays, out uint displayCount);
    [DllImport(NativeLibraryNames.CoreGraphics)]
    static extern nuint CGDisplayPixelsHigh(uint display);
    [DllImport(NativeLibraryNames.CoreGraphics)]
    static extern nuint CGDisplayPixelsWide(uint display);
    [DllImport(NativeLibraryNames.CoreGraphics)]
	static extern CGError CGGetDisplaysWithPoint(CGPoint point, uint maxDisplays, [MarshalAs(UnmanagedType.LPArray)] uint[]? displays, out uint matchingDisplayCount);
    [DllImport(NativeLibraryNames.CoreGraphics)]
	static extern CGError CGGetDisplaysWithRect(CGRect rect, uint maxDisplays, [MarshalAs(UnmanagedType.LPArray)] uint[]? displays, out uint matchingDisplayCount);
    [DllImport(NativeLibraryNames.CoreGraphics)]
    static extern CGError CGGetOnlineDisplayList(uint maxDisplays, [MarshalAs(UnmanagedType.LPArray)] uint[]? onlineDisplays, out uint displayCount);


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
        var result = CGGetActiveDisplayList(0, null, out var displyCount);
        if (result != CGError.Success)
            throw result.ToException();
        if (displyCount == 0)
            return new uint[0];
        var displays = new uint[displyCount];
        result = CGGetActiveDisplayList(displyCount, displays, out displyCount);
        if (result == CGError.Success)
            return displays;
        throw result.ToException();
    }


    /// <summary>
    /// Get bounds of given display.
    /// </summary>
    /// <param name="display">ID of display.</param>
    /// <returns>Bounds of diaplsy.</returns>
    [DllImport(NativeLibraryNames.CoreGraphics, EntryPoint = "CGDisplayBounds")]
    public static extern CGRect GetDisplayBounds(uint display);


    /// <summary>
    /// Get ID of display which contains the given point.
    /// </summary>
    /// <param name="point">Point.</param>
    /// <returns>ID of display.</returns>
    public static uint GetDisplayFromPoint(CGPoint point)
    {
        var displays = new uint[1];
        var result = CGGetDisplaysWithPoint(point, 1, displays, out var displyCount);
        if (result != CGError.Success)
            throw result.ToException();
        if (displyCount == 1)
            return displays[0];
        return Invalid;
    }


    /// <summary>
    /// Get ID of display which contains the given rectangle.
    /// </summary>
    /// <param name="rect">Rectangle.</param>
    /// <returns>ID of display.</returns>
    public static uint GetDisplayFromRect(CGRect rect)
    {
        var displays = new uint[1];
        var result = CGGetDisplaysWithRect(rect, 1, displays, out var displyCount);
        if (result != CGError.Success)
            throw result.ToException();
        if (displyCount == 1)
            return displays[0];
        return Invalid;
    }


    /// <summary>
    /// Get list of ID of displays which contain the given point.
    /// </summary>
    /// <param name="point">Point.</param>
    /// <returns>List of ID of displays.</returns>
    public static uint[] GetDisplaysFromPoint(CGPoint point)
    {
        var result = CGGetDisplaysWithPoint(point, 0, null, out var displyCount);
        if (result != CGError.Success)
            throw result.ToException();
        if (displyCount == 0)
            return new uint[0];
        var displays = new uint[displyCount];
        result = CGGetDisplaysWithPoint(point, displyCount, displays, out displyCount);
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
        var result = CGGetDisplaysWithRect(rect, 0, null, out var displyCount);
        if (result != CGError.Success)
            throw result.ToException();
        if (displyCount == 0)
            return new uint[0];
        var displays = new uint[displyCount];
        result = CGGetDisplaysWithRect(rect, displyCount, displays, out displyCount);
        if (result == CGError.Success)
            return displays;
        throw result.ToException();
    }


    /// <summary>
    /// Get ID of main display.
    /// </summary>
    /// <returns>ID of main display.</returns>
    [DllImport(NativeLibraryNames.CoreGraphics, EntryPoint = "CGMainDisplayID")]
	public static extern uint GetMainDisplay();


    /// <summary>
    /// Get all ID of online displays.
    /// </summary>
    /// <returns>ID of displays.</returns>
    public static uint[] GetOnlineDisplays()
    {
        var result = CGGetOnlineDisplayList(0, null, out var displyCount);
        if (result != CGError.Success)
            throw result.ToException();
        if (displyCount == 0)
            return new uint[0];
        var displays = new uint[displyCount];
        result = CGGetOnlineDisplayList(displyCount, displays, out displyCount);
        if (result == CGError.Success)
            return displays;
        throw result.ToException();
    }
}