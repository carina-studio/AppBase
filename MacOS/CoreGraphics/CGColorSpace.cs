using CarinaStudio.MacOS.CoreFoundation;
using System;
using System.Runtime.InteropServices;

namespace CarinaStudio.MacOS.CoreGraphics;

/// <summary>
/// CGColorSpace.
/// </summary>
public unsafe class CGColorSpace : CFObject
{
    // Native symbols.
    static readonly delegate*<IntPtr, IntPtr> CGColorSpaceCopyICCData;
    static readonly delegate*<IntPtr, IntPtr> CGColorSpaceCreateWithICCProfile;
    static readonly delegate*<IntPtr, IntPtr> CGColorSpaceCreateWithName;
    static readonly delegate*<IntPtr, CGColorSpaceModel> CGColorSpaceGetModel;
    static readonly delegate*<IntPtr, IntPtr> CGColorSpaceCopyName;
    static readonly delegate*<uint, IntPtr> CGDisplayCopyColorSpace;
    static readonly CFString? kCGColorSpaceAdobeRGB1998;
    static readonly CFString? kCGColorSpaceDisplayP3;
    static readonly CFString? kCGColorSpaceGenericCMYK;
    static readonly CFString? kCGColorSpaceGenericGray;
    static readonly CFString? kCGColorSpaceGenericGrayGamma2_2;
    static readonly CFString? kCGColorSpaceGenericRGB;
    static readonly CFString? kCGColorSpaceGenericRGBLinear;
    static readonly CFString? kCGColorSpaceSRGB;


    // Static fields.
    static volatile CGColorSpace? adobeRGB1998;
    static volatile CGColorSpace? displayP3;
    static volatile CGColorSpace? genericCmyk;
    static volatile CGColorSpace? genericGray;
    static volatile CGColorSpace? genericGrayGamma2_2;
    static volatile CGColorSpace? genericRGB;
    static volatile CGColorSpace? genericRGBLinear;
    static volatile CGColorSpace? sRGB;


    // Static initializer.
    static CGColorSpace()
    {
        if (Platform.IsNotMacOS)
            return;
        var libHandle = NativeLibraryHandles.CoreGraphics;
        CGColorSpaceCopyICCData = (delegate*<IntPtr, IntPtr>)NativeLibrary.GetExport(libHandle, nameof(CGColorSpaceCopyICCData));
        CGColorSpaceCreateWithICCProfile = (delegate*<IntPtr, IntPtr>)NativeLibrary.GetExport(libHandle, nameof(CGColorSpaceCreateWithICCProfile));
        CGColorSpaceCreateWithName = (delegate*<IntPtr, IntPtr>)NativeLibrary.GetExport(libHandle, nameof(CGColorSpaceCreateWithName));
        CGColorSpaceGetModel = (delegate*<IntPtr, CGColorSpaceModel>)NativeLibrary.GetExport(libHandle, nameof(CGColorSpaceGetModel));
        CGColorSpaceCopyName = (delegate*<IntPtr, IntPtr>)NativeLibrary.GetExport(libHandle, nameof(CGColorSpaceCopyName));
        CGDisplayCopyColorSpace = (delegate*<uint, IntPtr>)NativeLibrary.GetExport(libHandle, nameof(CGDisplayCopyColorSpace));
        kCGColorSpaceAdobeRGB1998 = FromHandle<CFString>(*(IntPtr*)NativeLibrary.GetExport(libHandle, nameof(kCGColorSpaceAdobeRGB1998)));
        kCGColorSpaceDisplayP3 = FromHandle<CFString>(*(IntPtr*)NativeLibrary.GetExport(libHandle, nameof(kCGColorSpaceDisplayP3)));
        kCGColorSpaceGenericCMYK = FromHandle<CFString>(*(IntPtr*)NativeLibrary.GetExport(libHandle, nameof(kCGColorSpaceGenericCMYK)));
        kCGColorSpaceGenericGray = FromHandle<CFString>(*(IntPtr*)NativeLibrary.GetExport(libHandle, nameof(kCGColorSpaceGenericGray)));
        kCGColorSpaceGenericGrayGamma2_2 = FromHandle<CFString>(*(IntPtr*)NativeLibrary.GetExport(libHandle, nameof(kCGColorSpaceGenericGrayGamma2_2)));
        kCGColorSpaceGenericRGB = FromHandle<CFString>(*(IntPtr*)NativeLibrary.GetExport(libHandle, nameof(kCGColorSpaceGenericRGB)));
        kCGColorSpaceGenericRGBLinear = FromHandle<CFString>(*(IntPtr*)NativeLibrary.GetExport(libHandle, nameof(kCGColorSpaceGenericRGBLinear)));
        kCGColorSpaceSRGB = FromHandle<CFString>(*(IntPtr*)NativeLibrary.GetExport(libHandle, nameof(kCGColorSpaceSRGB)));
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
    /// Get Display-P3 color space.
    /// </summary>
    public static CGColorSpace DisplayP3
    {
        get
        {
            return displayP3 ?? new CGColorSpace(CGColorSpaceCreateWithName(kCGColorSpaceDisplayP3!.Handle), false).Also(it =>
            {
                it.IsDefaultInstance = true;
                displayP3 = it;
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
            using var s = FromHandle<CFString>(sHandle, true);
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
        return FromHandle<CFData>(CGColorSpaceCopyICCData(this.Handle), true);
    }


    /// <inheritdoc/>
    public override string? ToString() =>
        this.IsReleased ? null : this.Name;
}