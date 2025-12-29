using CarinaStudio.MacOS.ObjectiveC;
using System;
#if NET7_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif

namespace CarinaStudio.MacOS.AppKit;

/// <summary>
/// NSFont.
/// </summary>
public class NSFont : NSObject
{
    /// <summary>
    /// Text styles.
    /// </summary>
    public class TextStyle : NSString
    {
        // Fields.
        static TextStyle? _Body;
        static TextStyle? _Callout;
        static TextStyle? _Caption1;
        static TextStyle? _Caption2;
        static TextStyle? _Footnote;
        static TextStyle? _Headline;
        static TextStyle? _LargeTitle;
        static TextStyle? _Subheadline;
        static TextStyle? _Title1;
        static TextStyle? _Title2;
        static TextStyle? _Title3;
        
        // Constructor.
        TextStyle(Class cls, IntPtr handle, bool ownsInstance) : base(cls, handle, ownsInstance)
        { }
        
        /// <summary>
        /// The font you use for body text.
        /// </summary>
        public static TextStyle Body => FromExport(ref _Body, NativeLibraryHandles.AppKit, "NSFontTextStyleBody").AsNonNull();
        
        /// <summary>
        /// The font you use for callouts.
        /// </summary>
        public static TextStyle Callout => FromExport(ref _Callout, NativeLibraryHandles.AppKit, "NSFontTextStyleCallout").AsNonNull();
        
        /// <summary>
        /// The font you use for standard captions.
        /// </summary>
        public static TextStyle Caption1 => FromExport(ref _Caption1, NativeLibraryHandles.AppKit, "NSFontTextStyleCaption1").AsNonNull();
        
        /// <summary>
        /// The font you use for alternate captions.
        /// </summary>
        public static TextStyle Caption2 => FromExport(ref _Caption2, NativeLibraryHandles.AppKit, "NSFontTextStyleCaption2").AsNonNull();
        
        /// <summary>
        /// The font you use in footnotes.
        /// </summary>
        public static TextStyle Footnote => FromExport(ref _Footnote, NativeLibraryHandles.AppKit, "NSFontTextStyleFootnote").AsNonNull();
        
        /// <summary>
        /// The font you use for headings.
        /// </summary>
        public static TextStyle Headline => FromExport(ref _Headline, NativeLibraryHandles.AppKit, "NSFontTextStyleHeadline").AsNonNull();

        /// <summary>
        /// The font you use for large titles.
        /// </summary>
        public static TextStyle LargeTitle => FromExport(ref _LargeTitle, NativeLibraryHandles.AppKit, "NSFontTextStyleLargeTitle").AsNonNull();
        
        /// <summary>
        /// The font you use for subheadings.
        /// </summary>
        public static TextStyle Subheadline => FromExport(ref _Subheadline, NativeLibraryHandles.AppKit, "NSFontTextStyleSubheadline").AsNonNull();
        
        /// <summary>
        /// The font you use for first-level hierarchical headings.
        /// </summary>
        public static TextStyle Title1 => FromExport(ref _Title1, NativeLibraryHandles.AppKit, "NSFontTextStyleTitle1").AsNonNull();
        
        /// <summary>
        /// The font you use for second-level hierarchical headings.
        /// </summary>
        public static TextStyle Title2 => FromExport(ref _Title2, NativeLibraryHandles.AppKit, "NSFontTextStyleTitle2").AsNonNull();
        
        /// <summary>
        /// The font you use for third-level hierarchical headings.
        /// </summary>
        public static TextStyle Title3 => FromExport(ref _Title3, NativeLibraryHandles.AppKit, "NSFontTextStyleTitle3").AsNonNull();
    }
    
    
    // Static fields.
    static Property? DisplayNameProperty;
    static Property? FamilyNameProperty;
    static Property? FontDescriptorProperty;
    static Property? FontNameProperty;
    static Selector? GetPreferredFontSelector;
    static Selector? LabelFontSizeSelector;
    static readonly Class? NSFontClass;
    static Property? PointSizeProperty;
    static Selector? SmallSystemFontSizeSelector;
    static Selector? SystemFontSizeSelector;
    
    
    // Static initializer.
    static NSFont()
    {
        if (Platform.IsNotMacOS)
            return;
        NSFontClass = Class.GetClass("NSFont").AsNonNull();
    }
    
    
    // Constructor.
#pragma warning disable IDE0051
    NSFont(IntPtr handle, bool ownsInstance) : base(handle, ownsInstance) =>
        this.VerifyClass(NSFontClass!);
#pragma warning restore IDE0051
    NSFont(Class cls, IntPtr handle, bool ownsInstance) : base(cls, handle, ownsInstance)
    { }
    
    
    /// <summary>
    /// The name of the font, including family and face names, to use when displaying the font information to the user.
    /// </summary>
    public NSString? DisplayName
    {
#if NET7_0_OR_GREATER
        [RequiresDynamicCode(GetPropertyRdcMessage)]
#endif
        get
        {
            DisplayNameProperty ??= NSFontClass!.GetProperty("displayName").AsNonNull();
            return this.GetProperty<NSString?>(DisplayNameProperty);
        }
    }
    
    
    /// <summary>
    /// The family name of the font—for example, “Times” or “Helvetica.”
    /// </summary>
    public NSString? FamilyName
    {
#if NET7_0_OR_GREATER
        [RequiresDynamicCode(GetPropertyRdcMessage)]
#endif
        get
        {
            FamilyNameProperty ??= NSFontClass!.GetProperty("familyName").AsNonNull();
            return this.GetProperty<NSString?>(FamilyNameProperty);
        }
    }
    
    
    /// <summary>
    /// The font descriptor object for the font.
    /// </summary>
    public NSFontDescriptor FontDescriptor
    {
#if NET7_0_OR_GREATER
        [RequiresDynamicCode(GetPropertyRdcMessage)]
#endif
        get
        {
            FontDescriptorProperty ??= NSFontClass!.GetProperty("fontDescriptor").AsNonNull();
            return this.GetProperty<NSFontDescriptor>(FontDescriptorProperty);
        }
    }
    
    
    /// <summary>
    /// The full name of the font, as used in PostScript language code—for example, “Times-Roman” or “Helvetica-Oblique.”
    /// </summary>
    public NSString FontName
    {
#if NET7_0_OR_GREATER
        [RequiresDynamicCode(GetPropertyRdcMessage)]
#endif
        get
        {
            FontNameProperty ??= NSFontClass!.GetProperty("fontName").AsNonNull();
            return this.GetProperty<NSString>(FontNameProperty);
        }
    }


    /// <summary>
    /// Returns the font associated with the text style.
    /// </summary>
#if NET7_0_OR_GREATER
    [RequiresDynamicCode(CallMethodRdcMessage)]
#endif
    public static NSFont GetPreferredFont(TextStyle style, NSObject? options = null)
    {
        GetPreferredFontSelector ??= Selector.FromName("preferredFontForTextStyle:options:");
        var handle = SendMessage<IntPtr>(NSFontClass!.Handle, GetPreferredFontSelector, style, options);
        return FromHandle<NSFont>(handle, ownsInstance: false).AsNonNull();
    }
    
    
    /// <summary>
    /// Get size of the standard Label Font.
    /// </summary>
    public static float LabelFontSize
    {
#if NET7_0_OR_GREATER
        [RequiresDynamicCode(GetPropertyRdcMessage)]
#endif
        get
        {
            LabelFontSizeSelector ??= Selector.FromName("labelFontSize");
            return SendMessage<float>(NSFontClass!.Handle, LabelFontSizeSelector);
        }
    }


    /// <summary>
    /// The point size of the font.
    /// </summary>
    public float PointSize
    {
#if NET7_0_OR_GREATER
        [RequiresDynamicCode(GetPropertyRdcMessage)]
#endif
        get
        {
            PointSizeProperty ??= NSFontClass!.GetProperty("pointSize").AsNonNull();
            return this.GetProperty<float>(PointSizeProperty);
        }
    }
    
    
    /// <summary>
    /// Get size of standard small System font.
    /// </summary>
    public static float SmallSystemFontSize
    {
#if NET7_0_OR_GREATER
        [RequiresDynamicCode(GetPropertyRdcMessage)]
#endif
        get
        {
            SmallSystemFontSizeSelector ??= Selector.FromName("smallSystemFontSize");
            return SendMessage<float>(NSFontClass!.Handle, SmallSystemFontSizeSelector);
        }
    }


    /// <summary>
    /// Get size of the standard System font.
    /// </summary>
    public static float SystemFontSize
    {
#if NET7_0_OR_GREATER
        [RequiresDynamicCode(GetPropertyRdcMessage)]
#endif
        get
        {
            SystemFontSizeSelector ??= Selector.FromName("systemFontSize");
            return SendMessage<float>(NSFontClass!.Handle, SystemFontSizeSelector);
        }
    }
}