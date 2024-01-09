using CarinaStudio.MacOS.ObjectiveC;
using System;
using System.Diagnostics.CodeAnalysis;

namespace CarinaStudio.MacOS.AppKit;

/// <summary>
/// NSAppearance.
/// </summary>
public class NSAppearance : NSObject
{
    // Constants.
    const string NSAppearanceNameAqua = "NSAppearanceNameAqua";
    const string NSAppearanceNameDarkAqua = "NSAppearanceNameDarkAqua";
    const string NSAppearanceNameVibrantDark = "NSAppearanceNameVibrantDark";
    const string NSAppearanceNameVibrantLight = "NSAppearanceNameVibrantLight";
    const string NSAppearanceNameAccessibilityHighContrastAqua = "NSAppearanceNameAccessibilityHighContrastAqua";
    const string NSAppearanceNameAccessibilityHighContrastDarkAqua = "NSAppearanceNameAccessibilityHighContrastDarkAqua";
    const string NSAppearanceNameAccessibilityHighContrastVibrantLight = "NSAppearanceNameAccessibilityHighContrastVibrantLight";
    const string NSAppearanceNameAccessibilityHighContrastVibrantDark = "NSAppearanceNameAccessibilityHighContrastVibrantDark";


    // Static fields.
    static Selector? AppearanceNamedSelector;
    static NSAppearance? NSAppearanceAqua;
    static readonly Class? NSAppearanceClass;
    static NSAppearance? NSAppearanceDarkAqua;
    static NSAppearance? NSAppearanceVibrantDark;
    static NSAppearance? NSAppearanceVibrantLight;
    static NSAppearance? NSAppearanceAccessibilityHighContrastAqua;
    static NSAppearance? NSAppearanceAccessibilityHighContrastDarkAqua;
    static NSAppearance? NSAppearanceAccessibilityHighContrastVibrantLight;
    static NSAppearance? NSAppearanceAccessibilityHighContrastVibrantDark;


    // Static initializer.
    static NSAppearance()
    {
        if (Platform.IsNotMacOS)
            return;
        NSAppearanceClass = Class.GetClass("NSAppearance").AsNonNull();
    }


    // Constructor.
#pragma warning disable IDE0051
    NSAppearance(IntPtr handle, bool ownsInstance) : base(handle, ownsInstance) =>
        this.VerifyClass(NSAppearanceClass!);
#pragma warning restore IDE0051
    NSAppearance(Class cls, IntPtr handle, bool ownsInstance) : base(cls, handle, ownsInstance)
    { }


    /// <summary>
    /// Predefined appearance: AccessibilityHighContrastAqua.
    /// </summary>
    public static NSAppearance AccessibilityHighContrastAqua
    {
#if NET7_0_OR_GREATER
        [RequiresDynamicCode(GetPropertyRdcMessage)]
#endif
        get
        {
            NSAppearanceAccessibilityHighContrastAqua ??= GetNamed(NSAppearanceNameAccessibilityHighContrastAqua);
            return NSAppearanceAccessibilityHighContrastAqua.AsNonNull();
        }
    }


    /// <summary>
    /// Predefined appearance: AccessibilityHighContrastDarkAqua.
    /// </summary>
    public static NSAppearance AccessibilityHighContrastDarkAqua
    {
#if NET7_0_OR_GREATER
        [RequiresDynamicCode(GetPropertyRdcMessage)]
#endif
        get
        {
            NSAppearanceAccessibilityHighContrastDarkAqua ??= GetNamed(NSAppearanceNameAccessibilityHighContrastDarkAqua);
            return NSAppearanceAccessibilityHighContrastDarkAqua.AsNonNull();
        }
    }


    /// <summary>
    /// Predefined appearance: AccessibilityHighContrastVibrantDark.
    /// </summary>
    public static NSAppearance AccessibilityHighContrastVibrantDark
    {
#if NET7_0_OR_GREATER
        [RequiresDynamicCode(GetPropertyRdcMessage)]
#endif
        get
        {
            NSAppearanceAccessibilityHighContrastVibrantDark ??= GetNamed(NSAppearanceNameAccessibilityHighContrastVibrantDark);
            return NSAppearanceAccessibilityHighContrastVibrantDark.AsNonNull();
        }
    }


    /// <summary>
    /// Predefined appearance: AccessibilityHighContrastVibrantLight.
    /// </summary>
    public static NSAppearance AccessibilityHighContrastVibrantLight
    {
#if NET7_0_OR_GREATER
        [RequiresDynamicCode(GetPropertyRdcMessage)]
#endif
        get
        {
            NSAppearanceAccessibilityHighContrastVibrantLight ??= GetNamed(NSAppearanceNameAccessibilityHighContrastVibrantLight);
            return NSAppearanceAccessibilityHighContrastVibrantLight.AsNonNull();
        }
    }


    /// <summary>
    /// Predefined appearance: Aqua.
    /// </summary>
    public static NSAppearance Aqua
    {
#if NET7_0_OR_GREATER
        [RequiresDynamicCode(GetPropertyRdcMessage)]
#endif
        get
        {
            NSAppearanceAqua ??= GetNamed(NSAppearanceNameAqua);
            return NSAppearanceAqua.AsNonNull();
        }
    }


    /// <summary>
    /// Predefined appearance: DarkAqua.
    /// </summary>
    public static NSAppearance DarkAqua
    {
#if NET7_0_OR_GREATER
        [RequiresDynamicCode(GetPropertyRdcMessage)]
#endif
        get
        {
            NSAppearanceDarkAqua ??= GetNamed(NSAppearanceNameDarkAqua);
            return NSAppearanceDarkAqua.AsNonNull();
        }
    }


    /// <summary>
    /// Get named appearance.
    /// </summary>
    /// <param name="name">Name.</param>
    /// <returns><see cref="NSAppearance"/> with specific name, or Null if appearance cannot be found.</returns>
#if NET7_0_OR_GREATER
    [RequiresDynamicCode(CallMethodRdcMessage)]
#endif
    public static NSAppearance? GetNamed(string name)
    {
        AppearanceNamedSelector ??= Selector.FromName("appearanceNamed:");
        using var nsName = new NSString(name);
        var handle = SendMessage<IntPtr>(NSAppearanceClass!.Handle, AppearanceNamedSelector, nsName);
        return handle != default ? new(NSAppearanceClass!, handle, false) : null;
    }


    /// <summary>
    /// Predefined appearance: VibrantDark.
    /// </summary>
    public static NSAppearance VibrantDark
    {
#if NET7_0_OR_GREATER
        [RequiresDynamicCode(GetPropertyRdcMessage)]
#endif
        get
        {
            NSAppearanceVibrantDark ??= GetNamed(NSAppearanceNameVibrantDark);
            return NSAppearanceVibrantDark.AsNonNull();
        }
    }


    /// <summary>
    /// Predefined appearance: VibrantLight.
    /// </summary>
    public static NSAppearance VibrantLight
    {
#if NET7_0_OR_GREATER
        [RequiresDynamicCode(GetPropertyRdcMessage)]
#endif
        get
        {
            NSAppearanceVibrantLight ??= GetNamed(NSAppearanceNameVibrantLight);
            return NSAppearanceVibrantLight.AsNonNull();
        }
    }
}