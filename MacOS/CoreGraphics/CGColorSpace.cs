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
    static extern IntPtr CGColorSpaceCreateWithName(IntPtr name);
    [DllImport(NativeLibraryNames.CoreGraphics)]
    static extern CGColorSpaceModel CGColorSpaceGetModel(IntPtr space);
    [DllImport(NativeLibraryNames.CoreGraphics)]
    static extern IntPtr CGColorSpaceCopyName(IntPtr space);
    [DllImport(NativeLibraryNames.CoreGraphics)]
	static extern IntPtr CGDisplayCopyColorSpace(uint display);
    static readonly CFString? kCGColorSpaceAdobeRGB1998;
    static readonly CFString? kCGColorSpaceGenericCMYK;
    static readonly CFString? kCGColorSpaceGenericGray;
    static readonly CFString? kCGColorSpaceGenericGrayGamma2_2;
    static readonly CFString? kCGColorSpaceGenericRGB;
    static readonly CFString? kCGColorSpaceGenericRGBLinear;
    static readonly CFString? kCGColorSpaceSRGB;


    // Static fields.
    static volatile CGColorSpace? adobeRGB1998;
    static volatile CGColorSpace? genericCmyk;
    static volatile CGColorSpace? genericGray;
    static volatile CGColorSpace? genericGrayGamma2_2;
    static volatile CGColorSpace? genericRGB;
    static volatile CGColorSpace? genericRGBLinear;
    static volatile CGColorSpace? sRGB;


    // Static initializer.
    static unsafe CGColorSpace()
    {
        if (Platform.IsNotMacOS)
            return;
        var libHandle = NativeLibrary.Load(NativeLibraryNames.CoreGraphics);
        if (libHandle != default)
        {
            kCGColorSpaceAdobeRGB1998 = CFObject.FromHandle<CFString>(*(IntPtr*)NativeLibrary.GetExport(libHandle, nameof(kCGColorSpaceAdobeRGB1998)));
            kCGColorSpaceGenericCMYK = CFObject.FromHandle<CFString>(*(IntPtr*)NativeLibrary.GetExport(libHandle, nameof(kCGColorSpaceGenericCMYK)));
            kCGColorSpaceGenericGray = CFObject.FromHandle<CFString>(*(IntPtr*)NativeLibrary.GetExport(libHandle, nameof(kCGColorSpaceGenericGray)));
            kCGColorSpaceGenericGrayGamma2_2 = CFObject.FromHandle<CFString>(*(IntPtr*)NativeLibrary.GetExport(libHandle, nameof(kCGColorSpaceGenericGrayGamma2_2)));
            kCGColorSpaceGenericRGB = CFObject.FromHandle<CFString>(*(IntPtr*)NativeLibrary.GetExport(libHandle, nameof(kCGColorSpaceGenericRGB)));
            kCGColorSpaceGenericRGBLinear = CFObject.FromHandle<CFString>(*(IntPtr*)NativeLibrary.GetExport(libHandle, nameof(kCGColorSpaceGenericRGBLinear)));
            kCGColorSpaceSRGB = CFObject.FromHandle<CFString>(*(IntPtr*)NativeLibrary.GetExport(libHandle, nameof(kCGColorSpaceSRGB)));
        }
    }


    // Constructor.
    CGColorSpace(IntPtr colorSpace, bool ownsInstance) : this(colorSpace, true, ownsInstance)
    { }
    CGColorSpace(IntPtr colorSpace, bool checkType, bool ownsInstance) : base(colorSpace, ownsInstance)
    { 
        if (checkType && colorSpace != IntPtr.Zero && this.TypeDescription != "CGColorSpace")
            throw new ArgumentException("Type of instance is not CGColorSpace.");
    }


    /// <summary>
    /// Get Adobe RGB (1998) color space.
    /// </summary>
    public static CGColorSpace AdobeRGB1998
    {
        get
        {
            return adobeRGB1998 ?? new CGColorSpace(CGColorSpaceCreateWithName(kCGColorSpaceAdobeRGB1998!.Handle), false).Also(it =>
            {
                it.IsDefaultInstance = true;
                adobeRGB1998 = it;
            });
        }
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
    /// Get generic CMYK color space.
    /// </summary>
    public static CGColorSpace GenericCMYK
    {
        get
        {
            return genericCmyk ?? new CGColorSpace(CGColorSpaceCreateWithName(kCGColorSpaceGenericCMYK!.Handle), false).Also(it =>
            {
                it.IsDefaultInstance = true;
                genericCmyk = it;
            });
        }
    }


    /// <summary>
    /// Get generic gray color space.
    /// </summary>
    public static CGColorSpace GenericGray
    {
        get
        {
            return genericGray ?? new CGColorSpace(CGColorSpaceCreateWithName(kCGColorSpaceGenericGray!.Handle), false).Also(it =>
            {
                it.IsDefaultInstance = true;
                genericGray = it;
            });
        }
    }


    /// <summary>
    /// Get generic gry with gamma 2.2 color space.
    /// </summary>
    public static CGColorSpace GenericGrayGamma2_2
    {
        get
        {
            return genericGrayGamma2_2 ?? new CGColorSpace(CGColorSpaceCreateWithName(kCGColorSpaceGenericGrayGamma2_2!.Handle), false).Also(it =>
            {
                it.IsDefaultInstance = true;
                genericGrayGamma2_2 = it;
            });
        }
    }


    /// <summary>
    /// Get generic RGB color space.
    /// </summary>
    public static CGColorSpace GenericRGB
    {
        get
        {
            return genericRGB ?? new CGColorSpace(CGColorSpaceCreateWithName(kCGColorSpaceGenericRGB!.Handle), false).Also(it =>
            {
                it.IsDefaultInstance = true;
                genericRGB = it;
            });
        }
    }


    /// <summary>
    /// Get generic linear RGB color space.
    /// </summary>
    public static CGColorSpace GenericRGBLinear
    {
        get
        {
            return genericRGBLinear ?? new CGColorSpace(CGColorSpaceCreateWithName(kCGColorSpaceGenericRGBLinear!.Handle), false).Also(it =>
            {
                it.IsDefaultInstance = true;
                genericRGBLinear = it;
            });
        }
    }
    

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
    /// Get sRGB color space.
    /// </summary>
    public static CGColorSpace SRGB
    {
        get
        {
            return sRGB ?? new CGColorSpace(CGColorSpaceCreateWithName(kCGColorSpaceSRGB!.Handle), false).Also(it =>
            {
                it.IsDefaultInstance = true;
                sRGB = it;
            });
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