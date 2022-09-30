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
    static extern CGColorSpaceModel CGColorSpaceGetModel(IntPtr space);
    [DllImport(NativeLibraryNames.CoreGraphics)]
    static extern IntPtr CGColorSpaceGetName(IntPtr space);


    // Constructor.
    CGColorSpace(IntPtr colorSpace, bool ownsInstance) : this(colorSpace, true, ownsInstance)
    { }
    CGColorSpace(IntPtr colorSpace, bool checkType, bool ownsInstance) : base(colorSpace, ownsInstance)
    { 
        if (checkType && colorSpace != IntPtr.Zero && this.TypeDescription != "CGColorSpace")
            throw new ArgumentException("Type of instance is not CGColorSpace.");
    }
}