using CarinaStudio.MacOS.ObjectiveC;
using System;

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
    static readonly Selector? AppearanceNamedSelector;
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
        AppearanceNamedSelector = Selector.FromName("appearanceNamed:");
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
    public static NSAppearance? GetNamed(string name)
    {
        using var nsName = new NSString(name);
        var handle = NSObject.SendMessage<IntPtr>(NSAppearanceClass!.Handle, AppearanceNamedSelector!, nsName);
        return handle != default ? new(NSAppearanceClass!, handle, false) : null;
    }


    /// <summary>
    /// Predefined appearance: VibrantDark.
    /// </summary>
    public static NSAppearance VibrantDark
    {
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
        get
        {
            NSAppearanceVibrantLight ??= GetNamed(NSAppearanceNameVibrantLight);
            return NSAppearanceVibrantLight.AsNonNull();
        }
    }
}