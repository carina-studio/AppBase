using CarinaStudio.MacOS.CoreFoundation;
using System;
using System.Runtime.InteropServices;

namespace CarinaStudio.MacOS.CoreGraphics;

/// <summary>
/// CGColorSpace.
/// </summary>
public class CGColorSpace : CFObject
{
    // Native symbols.
    [DllImport(NativeLibraryNames.CoreGraphics)]
    static extern IntPtr CGColorSpaceCopyICCData(IntPtr space);
    [DllImport(NativeLibraryNames.CoreGraphics)]
    static extern IntPtr CGColorSpaceCreateWithICCProfile(IntPtr data);
    [DllImport(NativeLibraryNames.CoreGraphics)]
    static extern CGColorSpaceModel CGColorSpaceGetModel(IntPtr space);
    [DllImport(NativeLibraryNames.CoreGraphics)]
    static extern IntPtr CGColorSpaceCopyName(IntPtr space);
    [DllImport(NativeLibraryNames.CoreGraphics)]
	static extern IntPtr CGDisplayCopyColorSpace(uint display);


    // Constructor.
    CGColorSpace(IntPtr colorSpace, bool ownsInstance) : this(colorSpace, true, ownsInstance)
    { }
    CGColorSpace(IntPtr colorSpace, bool checkType, bool ownsInstance) : base(colorSpace, ownsInstance)
    { 
        if (checkType && colorSpace != IntPtr.Zero && this.TypeDescription != "CGColorSpace")
            throw new ArgumentException("Type of instance is not CGColorSpace.");
    }


    /// <summary>
    /// Get color space of given display.
    /// </summary>
    /// <param name="display">ID of display.</param>
    /// <returns>Color space of display.</returns>
    public static CGColorSpace FromDisplay(uint display) =>
        new CGColorSpace(CGDisplayCopyColorSpace(display), false, true);
    

    /// <summary>
    /// Create color space from ICC profile.
    /// </summary>
    /// <param name="iccProfile"><see cref="CFData"/> contains ICC profile.</param>
    /// <returns>Color space.</returns>
    public static CGColorSpace FromIccProfile(CFData iccProfile) =>
        new CGColorSpace(CGColorSpaceCreateWithICCProfile(iccProfile.Handle), false, true);
    

    /// <summary>
    /// Get model of color space.
    /// </summary>
    public CGColorSpaceModel Model 
    {
        get
        {
            this.VerifyReleased();
            return CGColorSpaceGetModel(this.Handle);
        }
    }


    /// <summary>
    /// Get name of color space.
    /// </summary>
    public string? Name
    {
        get
        {
            this.VerifyReleased();
            var sHandle = CGColorSpaceCopyName(this.Handle);
            if (sHandle == IntPtr.Zero)
                return null;
            using var s = CFObject.FromHandle<CFString>(sHandle, true);
            return s.ToString();
        }
    }


    /// <summary>
    /// Copy color space as ICC profile.
    /// </summary>
    /// <returns><see cref="CFData"/> contains ICC profile.</returns>
    public CFData ToIccProfile()
    {
        this.VerifyReleased();
        return CFObject.FromHandle<CFData>(CGColorSpaceCopyICCData(this.Handle), true);
    }


    /// <inheritdoc/>
    public override string? ToString() =>
        this.IsReleased ? null : this.Name;
}