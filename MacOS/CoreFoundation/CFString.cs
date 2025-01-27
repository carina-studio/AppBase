using System;
using System.Runtime.InteropServices;

namespace CarinaStudio.MacOS.CoreFoundation;

/// <summary>
/// String.
/// </summary>
public unsafe class CFString : CFObject
{
    // Native symbols.
    static readonly delegate*<IntPtr, char*, long, IntPtr> CFStringCreateWithCharacters;
    static readonly delegate*<IntPtr, CFRange, void*, void> CFStringGetCharacters;
    static readonly delegate*<IntPtr, long> CFStringGetLength;


    // Fields.
    int length;
    
    
    // Static constructor.
    static CFString()
    {
        if (Platform.IsNotMacOS)
            return;
        var libHandle = NativeLibraryHandles.CoreFoundation;
        CFStringCreateWithCharacters = (delegate*<IntPtr, char*, long, IntPtr>)NativeLibrary.GetExport(libHandle, nameof(CFStringCreateWithCharacters));
        CFStringGetCharacters = (delegate*<IntPtr, CFRange, void*, void>)NativeLibrary.GetExport(libHandle, nameof(CFStringGetCharacters));
        CFStringGetLength = (delegate*<IntPtr, long>)NativeLibrary.GetExport(libHandle, nameof(CFStringGetLength));
    }


    /// <summary>
    /// Initialize new <see cref="CFString"/> instance.
    /// </summary>
    /// <param name="s">String.</param>
    public CFString(string s) : base(Global.Run(() =>
    {
        if (s == null)
            throw new ArgumentNullException(nameof(s));
        fixed (char* sPtr = s)
            return CFStringCreateWithCharacters(CFAllocator.Default.Handle, sPtr, s.Length);
    }), true)
    { 
        this.length = s.Length;
    }


    // Constructor.
    CFString(IntPtr s, bool ownsInstance) : this(s, true, ownsInstance)
    { }
    internal CFString(IntPtr s, bool checkType, bool ownsInstance) : base(s, ownsInstance)
    { 
        if (checkType && s != IntPtr.Zero && this.TypeDescription != "CFString")
            throw new ArgumentException("Type of instance is not CFString.");
        this.length = -1;
    }


    /// <summary>
    /// Copy string to given buffer.
    /// </summary>
    /// <param name="buffer">Buffer.</param>
    /// <param name="index">Index of position to place copied string.</param>
    public void CopyTo(char[] buffer, int index)
    {
        this.VerifyReleased();
        var length = this.Length;
        if (index < 0 || index + length > buffer.Length)
            throw new ArgumentOutOfRangeException(nameof(index));
        fixed (char* p = buffer)
        {
            CFStringGetCharacters(this.Handle, new CFRange(0, length), p + index);
        }
    }


    /// <summary>
    /// Get number of characters of string.
    /// </summary>
    public int Length
    {
        get
        {
            if (this.length >= 0)
                return this.length;
            this.VerifyReleased();
            var length = CFStringGetLength(this.Handle);
            if (length > int.MaxValue)
                throw new NotSupportedException($"Length of string is too long: {length}");
            this.length = (int)length;
            return (int)length;
        }
    }


    /// <inheritdoc/>
    public override CFObject Retain()
    {
        this.VerifyReleased();
        return new CFString(CFObject.Retain(this.Handle), true)
        {
            length = this.length
        };
    }


    /// <inheritdoc/>
    public override string? ToString()
    {
        if (this.Handle == IntPtr.Zero)
            return null;
        var buffer = new char[this.Length];
        fixed (char* p = buffer)
        {
            CFStringGetCharacters(this.Handle, new CFRange(0, buffer.Length), p);
            return new string(p);
        }
    }
}