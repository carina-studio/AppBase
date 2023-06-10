using System;
using System.Runtime.InteropServices;

namespace CarinaStudio.MacOS.CoreFoundation;

/// <summary>
/// CFDictionary.
/// </summary>
public class CFDictionary : CFObject
{
    // Native symbols.
    [DllImport(NativeLibraryNames.CoreFoundation)]
    static extern bool CFDictionaryContainsKey(IntPtr theDict, IntPtr key);
    [DllImport(NativeLibraryNames.CoreFoundation)]
    static extern bool CFDictionaryContainsValue(IntPtr theDict, IntPtr value);
    [DllImport(NativeLibraryNames.CoreFoundation)]
    static extern nint CFDictionaryGetCount(IntPtr theDict);
    [DllImport(NativeLibraryNames.CoreFoundation)]
    static extern bool CFDictionaryGetValueIfPresent(IntPtr theDict, IntPtr key, out IntPtr value);
    
    
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
        if (CFDictionaryGetValueIfPresent(this.Handle, key.Handle, out var valueHandle))
        {
            value = FromHandle(valueHandle);
            return true;
        }
        value = default;
        return false;
    }
}