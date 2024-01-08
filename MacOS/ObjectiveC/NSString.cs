using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace CarinaStudio.MacOS.ObjectiveC;

/// <summary>
/// NSString.
/// </summary>
#pragma warning disable CS0659
#pragma warning disable CS0661
public class NSString : NSObject, IComparable<NSString>, IEquatable<NSString>
#pragma warning restore CS0659
#pragma warning restore CS0661
{
    // Static fields.
    static readonly Selector? CompareSelector;
    static readonly Selector? GetCharsSelector;
    static readonly Selector? InitWithCharSelector;
    static readonly Selector? IsEqualToSelector;
    static readonly Property? LengthProperty;
    static readonly Class? NSStringClass = Class.GetClass("NSString");


    // Fields.
    volatile WeakReference<string>? stringRef;


    // Static initializer.
    static NSString()
    {
        if (Platform.IsNotMacOS)
            return;
        NSStringClass = Class.GetClass("NSString").AsNonNull();
        CompareSelector = Selector.FromName("compare:");
        GetCharsSelector = Selector.FromName("getCharacters:range:");
        InitWithCharSelector = Selector.FromName("initWithCharacters:length:");
        IsEqualToSelector = Selector.FromName("isEqualTo:");
        LengthProperty = NSStringClass.GetProperty("length");
    }


    /// <summary>
    /// Initialize new <see cref="NSString"/> instance.
    /// </summary>
    [RequiresDynamicCode(CallConstructorRdcMessage)]
    public NSString() : base(Initialize(NSStringClass!.Allocate()), true)
    { }


    /// <summary>
    /// Initialize new <see cref="NSString"/> instance.
    /// </summary>
    /// <param name="s">Characters.</param>
    [RequiresDynamicCode(CallConstructorRdcMessage)]
    public NSString(string s) : base(Initialize(NSStringClass!.Allocate(), s), true)
    { 
        this.stringRef = new WeakReference<string>(s);
    }


    // Constructor.
    NSString(IntPtr handle, bool ownsInstance) : base(handle, ownsInstance) =>
        this.VerifyClass(NSStringClass!);
    NSString(Class cls, IntPtr handle, bool ownsInstance) : base(cls, handle, ownsInstance)
    { }


    /// <inheritdoc/>
    public unsafe int CompareTo(NSString? s)
    {
        if (s == null)
            return 1;
        this.VerifyReleased();
        s.VerifyReleased();
        return ((delegate*unmanaged<nint, nint, nint, int>)SendMessageNative)(this.Handle, CompareSelector!.Handle, s.Handle);
    }


    /// <inheritdoc/>
    public unsafe bool Equals(NSString? s)
    {
        if (s == null || s.IsReleased || this.IsReleased)
            return false;
        return ((delegate*unmanaged<nint, nint, nint, bool>)SendMessageNative)(this.Handle, IsEqualToSelector!.Handle, s.Handle);
    }


    /// <inheritdoc/>
#pragma warning disable CS0659
    public override bool Equals(object? obj) =>
        obj is NSString s && this.Equals(s);
#pragma warning restore CS0659


    // Initialize.
    [RequiresDynamicCode(CallMethodRdcMessage)]
    static IntPtr Initialize(IntPtr obj, string s)
    {
        var pStr = Marshal.StringToHGlobalUni(s);
        var newObj = SendMessage<IntPtr>(obj, InitWithCharSelector!, pStr, s.Length);
        Marshal.FreeHGlobal(pStr);
        return newObj;
    }
    

    /// <summary>
    /// Get number of characters.
    /// </summary>
    public int Length => this.GetInt32Property(LengthProperty!);


    /// <summary>
    /// Equality operator.
    /// </summary>
    public static bool operator ==(NSString? l, NSString? r) =>
        l?.Equals(r) ?? r is null;
    

    /// <summary>
    /// Inequality operator.
    /// </summary>
    public static bool operator !=(NSString? l, NSString? r) =>
        l?.Equals(r) != true && ((l is null) != (r is null));


    /// <inheritdoc/>
    public override unsafe string ToString()
    {
        if (this.IsReleased)
            return "";
        if (this.stringRef?.TryGetTarget(out var s) == true)
            return s;
        var length = this.Length;
        var buffer = new char[length];
        fixed (char* p = buffer)
            ((delegate*unmanaged<nint, nint, char*, NSRange, void>)SendMessageNative)(this.Handle, GetCharsSelector!.Handle, p, new NSRange(0, length));
        s = new string(buffer);
        this.stringRef = new WeakReference<string>(s);
        return s;
    }
}