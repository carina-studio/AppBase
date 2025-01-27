using System;
using System.Runtime.InteropServices;

namespace CarinaStudio.MacOS;

/// <summary>
/// Handles of native libraries.
/// </summary>
public static class NativeLibraryHandles
{
    // Constants.
    static readonly string[] CoreFoundationLibPaths =
    [
        "/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation",
        "/System/Library/Frameworks/ApplicationServices.framework/Frameworks/CoreFoundation.framework/CoreFoundation"
    ];
    
    
    // Fields.
    static IntPtr coreFoundationLibHandle;
    static bool isCoreFoundationLibResolved;


    /// <summary>
    /// Handle of Core Foundation library.
    /// </summary>
    public static IntPtr CoreFoundation => GetHandle(CoreFoundationLibPaths, ref coreFoundationLibHandle, ref isCoreFoundationLibResolved);
    
    
    // Get handle of library.
    static IntPtr GetHandle(string[] libPaths, ref IntPtr handle, ref bool isResolved)
    {
        if (Platform.IsNotMacOS)
            throw new NotSupportedException();
        if (isResolved)
        {
            if (handle != IntPtr.Zero)
                return handle;
            throw new DllNotFoundException();
        }
        for (int i = 0, count = libPaths.Length; i < count; ++i)
        {
            if (NativeLibrary.TryLoad(libPaths[i], out handle))
            {
                isResolved = true;
                return handle;
            }
        }
        isResolved = true;
        throw new DllNotFoundException();
    }
}