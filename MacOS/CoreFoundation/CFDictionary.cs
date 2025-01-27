using System;
using System.Runtime.InteropServices;

namespace CarinaStudio.MacOS.CoreFoundation;

/// <summary>
/// CFDictionary.
/// </summary>
public unsafe class CFDictionary : CFObject
{
    // Native symbols.
    static readonly delegate*<IntPtr, IntPtr, bool> CFDictionaryContainsKey;
    static readonly delegate*<IntPtr, IntPtr, bool> CFDictionaryContainsValue;
    static readonly delegate*<IntPtr, nint> CFDictionaryGetCount;
    static readonly delegate*<IntPtr, IntPtr, IntPtr*, bool> CFDictionaryGetValueIfPresent;
    
    
    // Static constructor.
    static CFDictionary()
    {
        if (Platform.IsNotMacOS)
            return;
        var libHandle = NativeLibraryHandles.CoreFoundation;
        CFDictionaryContainsKey = (delegate*<IntPtr, IntPtr, bool>)NativeLibrary.GetExport(libHandle, nameof(CFDictionaryContainsKey));
        CFDictionaryContainsValue = (delegate*<IntPtr, IntPtr, bool>)NativeLibrary.GetExport(libHandle, nameof(CFDictionaryContainsValue));
        CFDictionaryGetCount = (delegate*<IntPtr, nint>)NativeLibrary.GetExport(libHandle, nameof(CFDictionaryGetCount));
        CFDictionaryGetValueIfPresent = (delegate*<IntPtr, IntPtr, IntPtr*, bool>)NativeLibrary.GetExport(libHandle, nameof(CFDictionaryGetValueIfPresent));
    }
    
    
    // Constructor.
    CFDictionary(IntPtr s, bool ownsInstance) : this(s, true, ownsInstance)
    { }
    internal CFDictionary(IntPtr s, bool checkType, bool ownsInstance) : base(s, ownsInstance)
    { 
        if (checkType && s != IntPtr.Zero && this.TypeDescription != "CFDictionary")
            throw new ArgumentException("Type of instance is not CFDictionary.");
    }


    /// <summary>
    /// Check whether given key is contained in dictionary or not.
    /// </summary>
    /// <param name="key">Key.</param>
    /// <returns>True if key is contained in dictionary.</returns>
    public bool ContainsKey(CFObject key) =>
        CFDictionaryContainsKey(this.Handle, key.Handle);
    
    
    /// <summary>
    /// Check whether given value is contained in dictionary or not.
    /// </summary>
    /// <param name="value">Value which can be null.</param>
    /// <returns>True if value is contained in dictionary.</returns>
    public bool ContainsValue(CFObject? value) =>
        CFDictionaryContainsValue(this.Handle, value?.Handle ?? default);


    /// <summary>
    /// Get number of key-value pairs in dictionary.
    /// </summary>
    public int Count => (int)CFDictionaryGetCount(this.Handle);


    /// <summary>
    /// Try getting value from dictionary.
    /// </summary>
    /// <param name="key">Key.</param>
    /// <param name="value">Value got from dictionary.</param>
    /// <returns>True if value got from dictionary successfully.</returns>
    public bool TryGetValue(CFString key, out CFObject? value)
    {
        var valueHandle = IntPtr.Zero;
        if (CFDictionaryGetValueIfPresent(this.Handle, key.Handle, &valueHandle))
        {
            value = FromHandle(valueHandle);
            return true;
        }
        value = default;
        return false;
    }
}