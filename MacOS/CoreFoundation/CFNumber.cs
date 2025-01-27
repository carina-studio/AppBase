using System;
using System.Runtime.InteropServices;

namespace CarinaStudio.MacOS.CoreFoundation;

/// <summary>
/// CFNumber.
/// </summary>
public unsafe class CFNumber : CFObject, IConvertible
{
    // Native symbols.
    static readonly delegate*<IntPtr, CFNumberType, void*, IntPtr> CFNumberCreate;
    static readonly delegate*<IntPtr, CFNumberType> CFNumberGetType;
    static readonly delegate*<IntPtr, CFNumberType, void*, void> CFNumberGetValue;
    static readonly IntPtr kCFNumberNaN;
    static readonly IntPtr kCFNumberNegativeInfinity;
    static readonly IntPtr kCFNumberPositiveInfinity;


    // Static fields.
    static volatile CFNumber? nanInstance;
    static volatile CFNumber? negativeInfinityInstance;
    static volatile CFNumber? positiveInfinityInstance;


    // Static initializer.
    static CFNumber()
    {
        // check platform
        if (Platform.IsNotMacOS)
            return;
        
        // load symbols
        var libHandle = NativeLibraryHandles.CoreFoundation;
        CFNumberCreate = (delegate*<IntPtr, CFNumberType, void*, IntPtr>)NativeLibrary.GetExport(libHandle, nameof(CFNumberCreate));
        CFNumberGetType = (delegate*<IntPtr, CFNumberType>)NativeLibrary.GetExport(libHandle, nameof(CFNumberGetType));
        CFNumberGetValue = (delegate*<IntPtr, CFNumberType, void*, void>)NativeLibrary.GetExport(libHandle, nameof(CFNumberGetValue));
        kCFNumberNaN = *(IntPtr*)NativeLibrary.GetExport(libHandle, nameof(kCFNumberNaN));
        kCFNumberNegativeInfinity = *(IntPtr*)NativeLibrary.GetExport(libHandle, nameof(kCFNumberNegativeInfinity));
        kCFNumberPositiveInfinity = *(IntPtr*)NativeLibrary.GetExport(libHandle, nameof(kCFNumberPositiveInfinity));
    }


    /// <summary>
    /// Initialize new <see cref="CFNumber"/> instance.
    /// </summary>
    /// <param name="value">Value.</param>
    public CFNumber(sbyte value) : this(value.Let(it =>
        CFNumberCreate(CFAllocator.Default.Handle, CFNumberType.SInt8Type, &it)), 
        CFNumberType.SInt8Type, true)
    { }


    /// <summary>
    /// Initialize new <see cref="CFNumber"/> instance.
    /// </summary>
    /// <param name="value">Value.</param>
    public CFNumber(byte value) : this(value.Let(it =>
        CFNumberCreate(CFAllocator.Default.Handle, CFNumberType.CharType, &it)), 
        CFNumberType.CharType, true)
    { }


    /// <summary>
    /// Initialize new <see cref="CFNumber"/> instance.
    /// </summary>
    /// <param name="value">Value.</param>
    public CFNumber(short value) : this(value.Let(it =>
        CFNumberCreate(CFAllocator.Default.Handle, CFNumberType.SInt16Type, &it)), 
        CFNumberType.SInt16Type, true)
    { }


    /// <summary>
    /// Initialize new <see cref="CFNumber"/> instance.
    /// </summary>
    /// <param name="value">Value.</param>
    public CFNumber(ushort value) : this(value.Let(it =>
        CFNumberCreate(CFAllocator.Default.Handle, CFNumberType.SInt16Type, &it)), 
        CFNumberType.SInt16Type, true)
    { }


    /// <summary>
    /// Initialize new <see cref="CFNumber"/> instance.
    /// </summary>
    /// <param name="value">Value.</param>
    public CFNumber(int value) : this(value.Let(it =>
        CFNumberCreate(CFAllocator.Default.Handle, CFNumberType.SInt32Type, &it)), 
        CFNumberType.SInt32Type, true)
    { }


    /// <summary>
    /// Initialize new <see cref="CFNumber"/> instance.
    /// </summary>
    /// <param name="value">Value.</param>
    public CFNumber(uint value) : this(value.Let(it =>
        CFNumberCreate(CFAllocator.Default.Handle, CFNumberType.SInt32Type, &it)), 
        CFNumberType.SInt32Type, true)
    { }


    /// <summary>
    /// Initialize new <see cref="CFNumber"/> instance.
    /// </summary>
    /// <param name="value">Value.</param>
    public CFNumber(long value) : this(value.Let(it =>
        CFNumberCreate(CFAllocator.Default.Handle, CFNumberType.SInt64Type, &it)), 
        CFNumberType.SInt64Type, true)
    { }


    /// <summary>
    /// Initialize new <see cref="CFNumber"/> instance.
    /// </summary>
    /// <param name="value">Value.</param>
    public CFNumber(ulong value) : this(value.Let(it =>
        CFNumberCreate(CFAllocator.Default.Handle, CFNumberType.SInt64Type, &it)), 
        CFNumberType.SInt64Type, true)
    { }


    /// <summary>
    /// Initialize new <see cref="CFNumber"/> instance.
    /// </summary>
    /// <param name="value">Value.</param>
    public CFNumber(float value) : this(value.Let(it =>
        CFNumberCreate(CFAllocator.Default.Handle, CFNumberType.Float32Type, &it)), 
        CFNumberType.Float32Type, true)
    { }


    /// <summary>
    /// Initialize new <see cref="CFNumber"/> instance.
    /// </summary>
    /// <param name="value">Value.</param>
    public CFNumber(double value) : this(value.Let(it =>
        CFNumberCreate(CFAllocator.Default.Handle, CFNumberType.Float64Type, &it)), 
        CFNumberType.Float64Type, true)
    { }


    // Constructor.
    CFNumber(IntPtr number, bool ownsInstance) : this(number, true, ownsInstance)
    { }
    CFNumber(IntPtr number, bool checkType, bool ownsInstance) : base(number, ownsInstance)
    {
        if (number != IntPtr.Zero)
        {
            if (checkType && this.TypeDescription != "CFNumber")
                throw new ArgumentException("Type of instance is CFNumber.");
            this.Type = CFNumberGetType(number);
        }
    }
    CFNumber(IntPtr number, CFNumberType type, bool ownsInstance) : base(number, ownsInstance)
    {
        this.Type = type;
    }


    /// <inheritdoc/>
    TypeCode IConvertible.GetTypeCode() => this.Type switch
    {
        CFNumberType.CharType => TypeCode.Byte,
        CFNumberType.DoubleType => TypeCode.Double,
        CFNumberType.Float32Type => TypeCode.Single,
        CFNumberType.Float64Type => TypeCode.Double,
        CFNumberType.FloatType => TypeCode.Single,
        CFNumberType.IntType => TypeCode.Int32,
        CFNumberType.LongLongType => TypeCode.Int64,
        CFNumberType.LongType => TypeCode.Int32,
        CFNumberType.ShortType => TypeCode.Int16,
        CFNumberType.SInt16Type => TypeCode.Int16,
        CFNumberType.SInt32Type => TypeCode.Int32,
        CFNumberType.SInt64Type => TypeCode.Int64,
        CFNumberType.SInt8Type => TypeCode.SByte,
        _ => TypeCode.Object,
    };


    /// <summary>
    /// Get default instance represents Not-a-Number (NaN).
    /// </summary>
    public static CFNumber NaN
    {
        get
        {
            return nanInstance ?? kCFNumberNaN.Let(handle =>
            {
                nanInstance = new CFNumber(handle, CFNumberGetType(handle), false);
                nanInstance.IsDefaultInstance = true;
                return nanInstance;
            });
        }
    }


    /// <summary>
    /// Get default instance represents negative infinity.
    /// </summary>
    public static CFNumber NegativeInfinity
    {
        get
        {
            return negativeInfinityInstance ?? kCFNumberNegativeInfinity.Let(handle =>
            {
                negativeInfinityInstance = new CFNumber(handle, CFNumberGetType(handle), false);
                negativeInfinityInstance.IsDefaultInstance = true;
                return negativeInfinityInstance;
            });
        }
    }


    /// <summary>
    /// Get default instance represents positive infinity.
    /// </summary>
    public static CFNumber PositiveInfinity
    {
        get
        {
            return positiveInfinityInstance ?? kCFNumberPositiveInfinity.Let(handle =>
            {
                positiveInfinityInstance = new CFNumber(handle, CFNumberGetType(handle), false);
                positiveInfinityInstance.IsDefaultInstance = true;
                return positiveInfinityInstance;
            });
        }
    }


    /// <inheritdoc/>
    bool IConvertible.ToBoolean(IFormatProvider? provider) =>
        this.ToInt64() != 0;
    

    /// <inheritdoc/>
    DateTime IConvertible.ToDateTime(IFormatProvider? provider) =>
        throw new NotSupportedException();
    

    /// <inheritdoc/>
    decimal IConvertible.ToDecimal(IFormatProvider? provider) =>
        this.ToInt64();


    /// <inheritdoc/>
    object IConvertible.ToType(Type conversionType, IFormatProvider? provider) =>
        throw new NotSupportedException();
    

    /// <inheritdoc/>
    public override CFObject Retain()
    {
        this.VerifyReleased();
        return new CFNumber(CFObject.Retain(this.Handle), this.Type, true);
    }


    /// <summary>
    /// Get number as <see cref="Byte"/>.
    /// </summary>
    /// <param name="formatProvider">Format provider.</param>
    /// <returns>Number.</returns>
    public byte ToByte(IFormatProvider? formatProvider = null)
    {
        this.VerifyReleased();
        var value = (byte)0;
        CFNumberGetValue(this.Handle, CFNumberType.CharType, &value);
        return value;
    }


    /// <summary>
    /// Get number as <see cref="Byte"/>.
    /// </summary>
    /// <param name="formatProvider">Format provider.</param>
    /// <returns>Number.</returns>
    public char ToChar(IFormatProvider? formatProvider = null)
    {
        this.VerifyReleased();
        var value = (ushort)0;
        CFNumberGetValue(this.Handle, CFNumberType.SInt16Type, &value);
        return (char)value;
    }


    /// <summary>
    /// Get number as <see cref="Single"/>.
    /// </summary>
    /// <param name="formatProvider">Format provider.</param>
    /// <returns>Number.</returns>
    public double ToDouble(IFormatProvider? formatProvider = null)
    {
        this.VerifyReleased();
        var value = 0.0;
        CFNumberGetValue(this.Handle, CFNumberType.Float64Type, &value);
        return value;
    }


    /// <summary>
    /// Get number as <see cref="Int16"/>.
    /// </summary>
    /// <param name="formatProvider">Format provider.</param>
    /// <returns>Number.</returns>
    public short ToInt16(IFormatProvider? formatProvider = null)
    {
        this.VerifyReleased();
        var value = (short)0;
        CFNumberGetValue(this.Handle, CFNumberType.SInt32Type, &value);
        return value;
    }


    /// <summary>
    /// Get number as <see cref="Int32"/>.
    /// </summary>
    /// <param name="formatProvider">Format provider.</param>
    /// <returns>Number.</returns>
    public int ToInt32(IFormatProvider? formatProvider = null)
    {
        this.VerifyReleased();
        var value = 0;
        CFNumberGetValue(this.Handle, CFNumberType.SInt32Type, &value);
        return value;
    }


    /// <summary>
    /// Get number as <see cref="Int64"/>.
    /// </summary>
    /// <param name="formatProvider">Format provider.</param>
    /// <returns>Number.</returns>
    public long ToInt64(IFormatProvider? formatProvider = null)
    {
        this.VerifyReleased();
        var value = 0L;
        CFNumberGetValue(this.Handle, CFNumberType.SInt64Type, &value);
        return value;
    }


    /// <summary>
    /// Get number as <see cref="SByte"/>.
    /// </summary>
    /// <param name="formatProvider">Format provider.</param>
    /// <returns>Number.</returns>
    public sbyte ToSByte(IFormatProvider? formatProvider = null)
    {
        this.VerifyReleased();
        var value = (sbyte)0;
        CFNumberGetValue(this.Handle, CFNumberType.SInt8Type, &value);
        return value;
    }


    /// <summary>
    /// Get number as <see cref="Single"/>.
    /// </summary>
    /// <param name="formatProvider">Format provider.</param>
    /// <returns>Number.</returns>
    public float ToSingle(IFormatProvider? formatProvider = null)
    {
        this.VerifyReleased();
        var value = 0f;
        CFNumberGetValue(this.Handle, CFNumberType.Float32Type, &value);
        return value;
    }


    /// <inheritdoc/>
    public override string ToString() =>
        this.ToString(null);


    /// <summary>
    /// Convert to string.
    /// </summary>
    /// <param name="provider">Format provider.</param>
    /// <returns>String.</returns>
    public string ToString(IFormatProvider? provider)
    {
        var value = this.Type switch
        {
            CFNumberType.CGFloatType => this.ToDouble(),
            CFNumberType.CharType => this.ToByte(),
            CFNumberType.DoubleType => this.ToDouble(),
            CFNumberType.Float32Type => this.ToSingle(),
            CFNumberType.Float64Type => this.ToDouble(),
            CFNumberType.FloatType => this.ToSingle(),
            CFNumberType.IntType => this.ToInt32(),
            CFNumberType.LongType => this.ToInt32(),
            CFNumberType.ShortType => this.ToInt16(),
            CFNumberType.SInt16Type => this.ToInt16(),
            CFNumberType.SInt32Type => this.ToInt32(),
            CFNumberType.SInt8Type => this.ToSByte(),
            _ => (IConvertible)this.ToInt64(),
        };
        return value.ToString(provider);
    }


    /// <summary>
    /// Get number as <see cref="UInt16"/>.
    /// </summary>
    /// <param name="formatProvider">Format provider.</param>
    /// <returns>Number.</returns>
    public ushort ToUInt16(IFormatProvider? formatProvider = null)
    {
        this.VerifyReleased();
        var value = (ushort)0;
        CFNumberGetValue(this.Handle, CFNumberType.SInt32Type, &value);
        return value;
    }


    /// <summary>
    /// Get number as <see cref="UInt32"/>.
    /// </summary>
    /// <param name="formatProvider">Format provider.</param>
    /// <returns>Number.</returns>
    public uint ToUInt32(IFormatProvider? formatProvider = null)
    {
        this.VerifyReleased();
        var value = 0u;
        CFNumberGetValue(this.Handle, CFNumberType.SInt32Type, &value);
        return value;
    }


    /// <summary>
    /// Get number as <see cref="UInt64"/>.
    /// </summary>
    /// <param name="formatProvider">Format provider.</param>
    /// <returns>Number.</returns>
    public ulong ToUInt64(IFormatProvider? formatProvider = null)
    {
        this.VerifyReleased();
        var value = 0uL;
        CFNumberGetValue(this.Handle, CFNumberType.SInt64Type, &value);
        return value;
    }


    /// <summary>
    /// Get type of number.
    /// </summary>
    public CFNumberType Type { get; }
}


/// <summary>
/// CFNumberType.
/// </summary>
public enum CFNumberType : long
{
#pragma warning disable CS1591
    SInt8Type = 1,
    SInt16Type = 2,
    SInt32Type = 3,
    SInt64Type = 4,
    Float32Type = 5,
    Float64Type = 6,
    CharType = 7,
    ShortType = 8,
    IntType = 9,
    LongType = 10,
    LongLongType = 11,
    FloatType = 12,
    DoubleType = 13,
    CFIndexType = 14,
    NSIntegerType = 15,
    CGFloatType = 16,
#pragma warning restore CS1591
}