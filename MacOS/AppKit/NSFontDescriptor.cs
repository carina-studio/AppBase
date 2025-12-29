using CarinaStudio.MacOS.CoreFoundation;
using CarinaStudio.MacOS.ObjectiveC;
using System;
#if NET7_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif

namespace CarinaStudio.MacOS.AppKit;

/// <summary>
/// NSFontDescriptor.
/// </summary>
public class NSFontDescriptor : NSObject
{
    /// <summary>
    /// AttributeName.
    /// </summary>
    public class AttributeName : NSString
    {
        // Static fields.
        static AttributeName? _CascadeList;
        static AttributeName? _CharacterSet;
        static AttributeName? _Face;
        static AttributeName? _Family;
        static AttributeName? _FeatureSettings;
        static AttributeName? _FixedAdvance;
        static AttributeName? _Matrix;
        static AttributeName? _Name;
        static AttributeName? _Size;
        static AttributeName? _Traits;
        static AttributeName? _Variation;
        static AttributeName? _VisibleName;
        
        // Constructor.
        AttributeName(Class cls, IntPtr handle, bool ownsInstance) : base(cls, handle, ownsInstance)
        { }
        
        /// <summary>
        /// An array, each member of which is a sub-descriptor. The type of object is <see cref="NSArray{T}"/>.
        /// </summary>
        public static AttributeName CascadeList => FromExport(ref _CascadeList, NativeLibraryHandles.AppKit, "NSFontCascadeListAttribute").AsNonNull();
        
        /// <summary>
        /// The set of Unicode characters covered by the font. The type of object is NSCharacterSet.
        /// </summary>
        public static AttributeName CharacterSet => FromExport(ref _CharacterSet, NativeLibraryHandles.AppKit, "NSFontCharacterSetAttribute").AsNonNull();
        
        /// <summary>
        /// An optional string object that specifies the font face. The type of object is <see cref="NSString"/>.
        /// </summary>
        public static AttributeName Face => FromExport(ref _Face, NativeLibraryHandles.AppKit, "NSFontFaceAttribute").AsNonNull();
        
        /// <summary>
        /// An optional string object that specifies the font family. The type of object is <see cref="NSString"/>.
        /// </summary>
        public static AttributeName Family => FromExport(ref _Family, NativeLibraryHandles.AppKit, "NSFontFamilyAttribute").AsNonNull();
        
        /// <summary>
        /// An array of dictionaries representing non-default font feature settings. The type of value is <see cref="NSArray{T}"/> of NSDictionary.
        /// </summary>
        public static AttributeName FeatureSettings => FromExport(ref _FeatureSettings, NativeLibraryHandles.AppKit, "NSFontFeatureSettingsAttribute").AsNonNull();
        
        /// <summary>
        /// A floating-point value that overrides the glyph advancement specified by the font. The type of value is <see cref="CFNumber"/> or NSNumber.
        /// </summary>
        public static AttributeName FixedAdvance => FromExport(ref _FixedAdvance, NativeLibraryHandles.AppKit, "NSFontFixedAdvanceAttribute").AsNonNull();
        
        /// <summary>
        /// An affine transform that specifies the font’s transformation matrix. The type of object is NSAffineTransform.
        /// </summary>
        public static AttributeName Matrix => FromExport(ref _Matrix, NativeLibraryHandles.AppKit, "NSFontMatrixAttribute").AsNonNull();
        
        /// <summary>
        /// An optional string object that specifies the font name. The type of object is <see cref="NSString"/>.
        /// </summary>
        public static AttributeName Name => FromExport(ref _Name, NativeLibraryHandles.AppKit, "NSFontNameAttribute").AsNonNull();

        /// <summary>
        /// An optional floating-point number that specifies the font size. The type of value is <see cref="CFNumber"/> or NSNumber.
        /// </summary>
        public static AttributeName Size => FromExport(ref _Size, NativeLibraryHandles.AppKit, "NSFontSizeAttribute").AsNonNull();
        
        /// <summary>
        /// A dictionary that fully describes the font traits. The type of object is NSDictionary.
        /// </summary>
        public static AttributeName Traits => FromExport(ref _Traits, NativeLibraryHandles.AppKit, "NSFontTraitsAttribute").AsNonNull();
        
        /// <summary>
        /// A dictionary that describes the font’s variation axis. The type of object is NSDictionary.
        /// </summary>
        public static AttributeName Variation => FromExport(ref _Variation, NativeLibraryHandles.AppKit, "NSFontVariationAttribute").AsNonNull();
        
        /// <summary>
        /// An optional string object that specifies the font’s visible name. The type of object is <see cref="NSString"/>.
        /// </summary>
        public static AttributeName VisibleName => FromExport(ref _VisibleName, NativeLibraryHandles.AppKit, "NSFontVisibleNameAttribute").AsNonNull();
    }
    
    
    // Static fields.
    static readonly Class? NSFontDescriptorClass;
    static Selector? ObjectForKeySelector;
    
    
    // Static initializer.
    static NSFontDescriptor()
    {
        if (Platform.IsNotMacOS)
            return;
        NSFontDescriptorClass = Class.GetClass("NSFontDescriptor").AsNonNull();
    }
    
    
    // Constructor.
#pragma warning disable IDE0051
    NSFontDescriptor(IntPtr handle, bool ownsInstance) : base(handle, ownsInstance) =>
        this.VerifyClass(NSFontDescriptorClass!);
#pragma warning restore IDE0051
    NSFontDescriptor(Class cls, IntPtr handle, bool ownsInstance) : base(cls, handle, ownsInstance)
    { }
    
    
    /// <summary>
    /// Returns the font attribute specified by the given key.
    /// </summary>
#if NET7_0_OR_GREATER
    [RequiresDynamicCode(CallMethodRdcMessage)]
#endif
    public T? GetCFObject<T>(AttributeName attribute) where T : CFObject
    {
        ObjectForKeySelector ??= Selector.FromName("objectForKey:");
        return CFObject.FromHandle<T>(this.SendMessage<IntPtr>(ObjectForKeySelector, attribute));
    }


    /// <summary>
    /// Returns the font attribute specified by the given key.
    /// </summary>
#if NET7_0_OR_GREATER
    [RequiresDynamicCode(CallMethodRdcMessage)]
#endif
    public T? GetNSObject<T>(AttributeName attribute) where T : NSObject
    {
        ObjectForKeySelector ??= Selector.FromName("objectForKey:");
        return FromHandle<T>(this.SendMessage<IntPtr>(ObjectForKeySelector, attribute));
    }
}