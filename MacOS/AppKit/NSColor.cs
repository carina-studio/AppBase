using CarinaStudio.MacOS.ObjectiveC;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace CarinaStudio.MacOS.AppKit;

/// <summary>
/// NSColor.
/// </summary>
public class NSColor : NSObject
{
#pragma warning disable CS1591
    /// <summary>
    /// ColorType.
    /// </summary>
    public enum ColorType
    {
        ComponentBased = 0,
        Pattern = 1,
        Catalog = 2,
    }
#pragma warning restore CS1591


    // Static fields.
    static Selector? AlphaComponentSelector;
    static Selector? BlendedSelector;
    static Selector? BlueComponentSelector;
    static Selector? ColorSpaceSelector;
    static Selector? ColorWithColorSpaceHsiSelector;
    static Selector? ColorWithColorSpaceSelector;
    static Selector? ColorWithDisplayP3Selector;
    static Selector? ColorWithSrgbSelector;
    static Selector? GetCmykaSelector;
    static Selector? GetHsiaSelector;
    static Selector? GetRgbaSelector;
    static Selector? GetWhiteSelector;
    static Selector? GreenComponentSelector;
    static Selector? HighlightSelector;
    static readonly IDictionary<string, NSColor> NamedColors = new ConcurrentDictionary<string, NSColor>();
    static readonly Class? NSColorClass;
    static Selector? RedComponentSelector;
    static Selector? ShadowSelector;
    static Selector? TypeSelector;
    static Selector? UsingColorSpaceSelector;
    static Selector? UsingTypeSelector;
    static Selector? WithAlphaSelector;


    // Static fields.
    static NSColor()
    {
        if (Platform.IsNotMacOS)
            return;
        NSColorClass = Class.GetClass(nameof(NSColor)).AsNonNull(); 
    }


    // Fields.
    readonly bool isDynamic;
    readonly bool isPatterned;


    // Constructor.
    NSColor(IntPtr handle, bool ownsInstance) : base(handle, ownsInstance)
    {
        this.VerifyClass(NSColorClass!);
        this.isDynamic = this.Class.Name.StartsWith("NSDynamic");
        this.isPatterned = !this.isDynamic && this.Class.Name.StartsWith("NSPattern");
    }
    NSColor(Class cls, IntPtr handle, bool ownsInstance) : base(cls, handle, ownsInstance)
    { 
        this.isDynamic = this.Class.Name.StartsWith("NSDynamic");
        this.isPatterned = !this.isDynamic && this.Class.Name.StartsWith("NSPattern");
    }


    /// <summary>
    /// Get alpha component.
    /// </summary>
    public double AlphaComponent 
    { 
        get 
        {
            if (this.isDynamic)
                throw new InvalidOperationException("Cannot get RGBA from dynamic color before color space conversion.");
            AlphaComponentSelector ??= Selector.FromName("alphaComponent");
#pragma warning disable IL3050
            return this.SendMessage<double>(AlphaComponentSelector);
#pragma warning restore IL3050
        }
    }


    /// <summary>
    /// Similar to SelectedControlColor.
    /// </summary>
    public static NSColor AlternateSelectedControl
    {
        [RequiresDynamicCode(GetPropertyRdcMessage)]
        get => GetNamedColor("alternateSelectedControlColor");
    }


    /// <summary>
    /// Similar to SelectedControlTextColor.
    /// </summary>
    public static NSColor AlternateSelectedControlText
    {
        [RequiresDynamicCode(GetPropertyRdcMessage)]
        get => GetNamedColor("alternateSelectedControlTextColor");
    }


    /// <summary>
    /// Black.
    /// </summary>
    public static NSColor Black
    {
        [RequiresDynamicCode(GetPropertyRdcMessage)]
        get => GetNamedColor("blackColor");
    }


    /// <summary>
    /// Creates a new color object whose component values are a weighted sum of the current color object and the specified color object's.
    /// </summary>
    /// <param name="fraction">The amount of the color to blend with this color.</param>
    /// <param name="color">The color to blend with this color.</param>
    /// <returns>Blended color, or Null if the colors can’t be converted.</returns>
    [RequiresDynamicCode(CallMethodRdcMessage)]
    public NSColor? BlendWith(double fraction, NSColor color)
    {
        BlendedSelector ??= Selector.FromName("blendedColorWithFraction:ofColor:");
        return this.SendMessage<NSColor?>(BlendedSelector, fraction, color);
    }


    /// <summary>
    /// Blue.
    /// </summary>
    public static NSColor Blue
    {
        [RequiresDynamicCode(GetPropertyRdcMessage)]
        get => GetNamedColor("blueColor");
    }


    /// <summary>
    /// Get blue component.
    /// </summary>
    public double BlueComponent 
    { 
        get 
        {
            if (this.isDynamic)
                throw new InvalidOperationException("Cannot get RGBA from dynamic color before color space conversion.");
            if (!this.IsRGB)
                throw new InvalidOperationException("Cannot get RGBA from color with non-RGB model.");
            BlueComponentSelector ??= Selector.FromName("blueComponent");
#pragma warning disable IL3050
            return this.SendMessage<double>(BlueComponentSelector);
#pragma warning restore IL3050
        }
    }


    /// <summary>
    /// Brown.
    /// </summary>
    public static NSColor Brown
    {
        [RequiresDynamicCode(GetPropertyRdcMessage)]
        get => GetNamedColor("brownColor");
    }


    /// <summary>
    /// Transparent.
    /// </summary>
    public static NSColor Clear
    {
        [RequiresDynamicCode(GetPropertyRdcMessage)]
        get => GetNamedColor("clearColor");
    }


    /// <summary>
    /// Get cyan, magenta, yellow, black and alpha of color.
    /// </summary>
    public unsafe (double, double, double, double, double) CMYK
    {
        get
        {
            if (this.isDynamic)
                throw new InvalidOperationException("Cannot get CMYK from dynamic color before color space conversion.");
            if (!this.IsCMYK)
                throw new InvalidOperationException("Cannot get CMYK from color with non-CMYK model.");
            var cmyka = stackalloc double[5];
            GetCmykaSelector ??= Selector.FromName("getCyan:magenta:yellow:black:alpha:");
            ((delegate*unmanaged<nint, nint, double*, double*, double*, double*, double*, void>)SendMessageNative)(this.Handle, GetCmykaSelector.Handle, cmyka, (cmyka + 1), (cmyka + 2), (cmyka + 3), (cmyka + 4));
            return (cmyka[0], cmyka[1], cmyka[2], cmyka[3], cmyka[4]);
        }
    }


    /// <summary>
    /// Get color space.
    /// </summary>
    public NSColorSpace? ColorSpace 
    { 
        get 
        {
            if (this.IsDynamic || this.isPatterned)
                return null;
            ColorSpaceSelector ??= Selector.FromName("colorSpace");
#pragma warning disable IL3050
            return this.SendMessage<NSColorSpace>(ColorSpaceSelector);
#pragma warning restore IL3050
        }
    }


    /// <summary>
    /// Control face and old window background color.
    /// </summary>
    public static NSColor Control
    {
        [RequiresDynamicCode(GetPropertyRdcMessage)]
        get => GetNamedColor("controlColor");
    }


    /// <summary>
    /// Background of large controls.
    /// </summary>
    public static NSColor ControlBackground
    {
        [RequiresDynamicCode(GetPropertyRdcMessage)]
        get => GetNamedColor("controlBackgroundColor");
    }


    /// <summary>
    /// Darker border for controls.
    /// </summary>
    public static NSColor ControlDarkShadow
    {
        [RequiresDynamicCode(GetPropertyRdcMessage)]
        get => GetNamedColor("controlDarkShadowColor");
    }


    /// <summary>
    /// Light border for controls.
    /// </summary>
    public static NSColor ControlHighlight
    {
        [RequiresDynamicCode(GetPropertyRdcMessage)]
        get => GetNamedColor("controlHighlightColor");
    }


    /// <summary>
    /// Lighter border for controls.
    /// </summary>
    public static NSColor ControlLightHighlight
    {
        [RequiresDynamicCode(GetPropertyRdcMessage)]
        get => GetNamedColor("controlLightHighlightColor");
    }


    /// <summary>
    /// Dark border for controls.
    /// </summary>
    public static NSColor ControlShadow
    {
        [RequiresDynamicCode(GetPropertyRdcMessage)]
        get => GetNamedColor("controlShadowColor");
    }


    /// <summary>
    /// Text on controls.
    /// </summary>
    public static NSColor ControlText
    {
        [RequiresDynamicCode(GetPropertyRdcMessage)]
        get => GetNamedColor("controlTextColor");
    }


    /// <summary>
    /// Create color object.
    /// </summary>
    /// <param name="colorSpace">Color space</param>
    /// <param name="components">Color components.</param>
    /// <returns>Color.</returns>
    [RequiresDynamicCode(CallMethodRdcMessage)]
    public unsafe static NSColor Create(NSColorSpace colorSpace, params double[] components)
    {
        var componentCount = components.Length;
        if (componentCount == 0)
            throw new ArgumentException("At least one component of color need to be provided.");
        switch (colorSpace.ColorSpaceModel)
        {
            case NSColorSpace.Model.CMYK:
                if (componentCount != 5)
                    throw new ArgumentException("Number of color components should be 5.");
                break;
            case NSColorSpace.Model.Gray:
                if (componentCount != 2)
                    throw new ArgumentException("Number of color components should be 2.");
                break;
            case NSColorSpace.Model.RGB:
                if (componentCount != 4)
                    throw new ArgumentException("Number of color components should be 4.");
                break;
        }
        ColorWithColorSpaceSelector ??= Selector.FromName("colorWithColorSpace:components:count:");
        fixed (double* p = components)
        {
            var handle = SendMessage<IntPtr>(NSColorClass!.Handle, ColorWithColorSpaceSelector, colorSpace, (IntPtr)p, componentCount);
            return new NSColor(handle, true);
        }
    }


    /// <summary>
    /// Create RGBA color object from HSI components.
    /// </summary>
    /// <param name="colorSpace">Color space.</param>
    /// <param name="h">Hue.</param>
    /// <param name="s">Saturation.</param>
    /// <param name="i">Intensity (Brightness).</param>
    /// <param name="a">Alpha.</param>
    /// <returns>Color.</returns>
    [RequiresDynamicCode(CallMethodRdcMessage)]
    public static NSColor CreateHSI(NSColorSpace colorSpace, double h, double s, double i, double a = 1.0)
    {
        if (colorSpace.ColorSpaceModel != NSColorSpace.Model.RGB)
            throw new ArgumentException($"Color model of '{colorSpace.LocalizedName}' is not RGB.");
        ColorWithColorSpaceHsiSelector ??= Selector.FromName("colorWithColorSpace:hue:saturation:brightness:alpha:");
        var handle = SendMessage<IntPtr>(NSColorClass!.Handle, ColorWithColorSpaceHsiSelector, colorSpace, h, s, i, a);
        return new NSColor(handle, true);
    }


    /// <summary>
    /// Create RGBA color object.
    /// </summary>
    /// <param name="colorSpace">Color space.</param>
    /// <param name="r">Red.</param>
    /// <param name="g">Green.</param>
    /// <param name="b">Blue.</param>
    /// <param name="a">Alpha.</param>
    /// <returns>Color.</returns>
    [RequiresDynamicCode(CallMethodRdcMessage)]
    public unsafe static NSColor CreateRGBA(NSColorSpace colorSpace, double r, double g, double b, double a = 1.0)
    {
        if (colorSpace.ColorSpaceModel != NSColorSpace.Model.RGB)
            throw new ArgumentException($"Color model of '{colorSpace.LocalizedName}' is not RGB.");
        ColorWithColorSpaceSelector ??= Selector.FromName("colorWithColorSpace:components:count:");
        var components = stackalloc double[] { r, g, b, a };
        var handle = SendMessage<IntPtr>(NSColorClass!.Handle, ColorWithColorSpaceSelector, colorSpace, (IntPtr)components, 4);
        return new NSColor(handle, true);
    }


    /// <summary>
    /// Create sRGB color object.
    /// </summary>
    /// <param name="r">Red.</param>
    /// <param name="g">Green.</param>
    /// <param name="b">Blue.</param>
    /// <param name="a">Alpha.</param>
    /// <returns>Color.</returns>
    [RequiresDynamicCode(CallMethodRdcMessage)]
    public static NSColor CreateWithSRGB(double r, double g, double b, double a = 1.0)
    {
        ColorWithSrgbSelector ??= Selector.FromName("colorWithSRGBRed:green:blue:alpha:");
        return new NSColor(SendMessage<IntPtr>(NSColorClass!.Handle, ColorWithSrgbSelector, r, g, b, a), true);
    }
    

    /// <summary>
    /// Create Display-P3 color object.
    /// </summary>
    /// <param name="r">Red.</param>
    /// <param name="g">Green.</param>
    /// <param name="b">Blue.</param>
    /// <param name="a">Alpha.</param>
    /// <returns>Color.</returns>
    [RequiresDynamicCode(CallMethodRdcMessage)]
    public static NSColor CreateWithDisplayP3(double r, double g, double b, double a = 1.0)
    {
        ColorWithDisplayP3Selector ??= Selector.FromName("colorWithDisplayP3Red:green:blue:alpha:");
        return new NSColor(SendMessage<IntPtr>(NSColorClass!.Handle, ColorWithDisplayP3Selector, r, g, b, a), true);
    }


    /// <summary>
    /// Cyan.
    /// </summary>
    public static NSColor Cyan
    {
        [RequiresDynamicCode(GetPropertyRdcMessage)]
        get => GetNamedColor("cyanColor");
    }


    /// <summary>
    /// Dark gray.
    /// </summary>
    public static NSColor DarkGray
    {
        [RequiresDynamicCode(GetPropertyRdcMessage)]
        get => GetNamedColor("darkGrayColor");
    }


    /// <summary>
    /// Text on disabled controls.
    /// </summary>
    public static NSColor DisabledControlText
    {
        [RequiresDynamicCode(GetPropertyRdcMessage)]
        get => GetNamedColor("disabledControlTextColor");
    }


    // Get color with given name.
    [RequiresDynamicCode(CallMethodRdcMessage)]
    static NSColor GetNamedColor(string name)
    {
        if (NamedColors.TryGetValue(name, out var color))
            return color;
        var handle = SendMessage<IntPtr>(NSColorClass!.Handle, Selector.FromName(name));
        return new NSColor(handle, false).Also(it => 
        {
            it.IsDefaultInstance = true;
            NamedColors.TryAdd(name, it);
        });
    }


    /// <summary>
    /// Gray.
    /// </summary>
    public static NSColor Gray
    {
        [RequiresDynamicCode(GetPropertyRdcMessage)]
        get => GetNamedColor("grayColor");
    }


    /// <summary>
    /// Get grayscale and alpha of color.
    /// </summary>
    public unsafe (double, double) Grayscale
    {
        [RequiresDynamicCode(GetPropertyRdcMessage)]
        get
        {
            if (this.isDynamic)
                throw new InvalidOperationException("Cannot get grayscale from dynamic color before color space conversion.");
            if (!this.IsGrayscale)
                throw new InvalidOperationException("Cannot get grayscale from color with non-Gray model.");
            var ga = stackalloc double[2];
            GetWhiteSelector ??= Selector.FromName("getWhite:alpha:");
            this.SendMessage(GetWhiteSelector, (IntPtr)ga, (IntPtr)(ga + 1));
            return (ga[0], ga[1]);
        }
    }


    /// <summary>
    /// Green.
    /// </summary>
    public static NSColor Green
    {
        [RequiresDynamicCode(GetPropertyRdcMessage)]
        get => GetNamedColor("greenColor");
    }


    /// <summary>
    /// Get green component.
    /// </summary>
    public double GreenComponent
    { 
        get 
        {
            if (this.isDynamic)
                throw new InvalidOperationException("Cannot get RGBA from dynamic color before color space conversion.");
            if (!this.IsRGB)
                throw new InvalidOperationException("Cannot get RGBA from color with non-RGB model.");
            GreenComponentSelector ??= Selector.FromName("greenComponent");
#pragma warning disable IL3050
            return this.SendMessage<double>(GreenComponentSelector);
#pragma warning restore IL3050
        }
    }


    /// <summary>
    /// Grids in controls.
    /// </summary>
    public static NSColor Grid
    {
        [RequiresDynamicCode(GetPropertyRdcMessage)]
        get => GetNamedColor("gridColor");
    }


    /// <summary>
    /// Background color for header cells in Table/OutlineView.
    /// </summary>
    public static NSColor Header
    {
        [RequiresDynamicCode(GetPropertyRdcMessage)]
        get => GetNamedColor("headerColor");
    }


    /// <summary>
    /// Text color for header cells in Table/OutlineView.
    /// </summary>
    public static NSColor HeaderText
    {
        [RequiresDynamicCode(GetPropertyRdcMessage)]
        get => GetNamedColor("headerTextColor");
    }


    /// <summary>
    /// Highlight color for UI elements.
    /// </summary>
    public static NSColor Highlight
    {
        [RequiresDynamicCode(GetPropertyRdcMessage)]
        get => GetNamedColor("highlightColor");
    }


    /// <summary>
    /// Creates a new color object that represents a blend between the current color and the highlight color.
    /// </summary>
    /// <param name="level">The level, range is [0.0, 1.0].</param>
    /// <returns>Color object, or Null if the colors can’t be converted.</returns>
    [RequiresDynamicCode(CallMethodRdcMessage)]
    public NSColor? HighlightWith(double level)
    {
        HighlightSelector ??= Selector.FromName("highlightWithLevel:");
        return this.SendMessage<NSColor?>(HighlightSelector, level);
    }


    /// <summary>
    /// Get hue, saturation, intensity (brightness) and alpha of color.
    /// </summary>
    public unsafe (double, double, double, double) HSI
    {
        [RequiresDynamicCode(GetPropertyRdcMessage)]
        get
        {
            if (this.isDynamic)
                throw new InvalidOperationException("Cannot get HSI from dynamic color before color space conversion.");
            if (!this.IsRGB)
                throw new InvalidOperationException("Cannot get HSI from color with non-RGB model.");
            var hsia = stackalloc double[4];
            GetHsiaSelector ??= Selector.FromName("getHue:saturation:brightness:alpha:");
            this.SendMessage(GetHsiaSelector, (IntPtr)hsia, (IntPtr)(hsia + 1), (IntPtr)(hsia + 2), (IntPtr)(hsia + 3));
            return (hsia[0], hsia[1], hsia[2], hsia[3]);
        }
    }


    /// <summary>
    /// Check whether the color model is <see cref="NSColorSpace.Model.CMYK"/> or not.
    /// </summary>
    public bool IsCMYK => this.ColorSpace?.ColorSpaceModel == NSColorSpace.Model.CMYK;


    /// <summary>
    /// Check whether the color object represents a dynamic color or not.
    /// </summary>
    public bool IsDynamic => this.isDynamic;


    /// <summary>
    /// Check whether the color model is <see cref="NSColorSpace.Model.Gray"/> or not.
    /// </summary>
    public bool IsGrayscale => this.ColorSpace?.ColorSpaceModel == NSColorSpace.Model.Gray;


    /// <summary>
    /// Check whether the color model is <see cref="NSColorSpace.Model.Indexed"/> or not.
    /// </summary>
    public bool IsIndexed => this.ColorSpace?.ColorSpaceModel == NSColorSpace.Model.Indexed;


    /// <summary>
    /// Check whether the color patterned color or not.
    /// </summary>
    public bool IsPatterned => this.isPatterned;


    /// <summary>
    /// Check whether the color model is <see cref="NSColorSpace.Model.RGB"/> or not.
    /// </summary>
    public bool IsRGB => this.ColorSpace?.ColorSpaceModel == NSColorSpace.Model.RGB;


    /// <summary>
    /// Keyboard focus ring around controls.
    /// </summary>
    public static NSColor KeyboardFocusIndicator
    {
        [RequiresDynamicCode(GetPropertyRdcMessage)]
        get => GetNamedColor("keyboardFocusIndicatorColor");
    }


    /// <summary>
    /// Knob face color for controls.
    /// </summary>
    public static NSColor Knob
    {
        [RequiresDynamicCode(GetPropertyRdcMessage)]
        get => GetNamedColor("knobColor");
    }


    /// <summary>
    /// Text color for static text and related elements.
    /// </summary>
    public static NSColor Label
    {
        [RequiresDynamicCode(GetPropertyRdcMessage)]
        get => GetNamedColor("labelColor");
    }


    /// <summary>
    /// Light gray.
    /// </summary>
    public static NSColor LightGray
    {
        [RequiresDynamicCode(GetPropertyRdcMessage)]
        get => GetNamedColor("lightGrayColor");
    }


    /// <summary>
    /// Magenta.
    /// </summary>
    public static NSColor Magenta
    {
        [RequiresDynamicCode(GetPropertyRdcMessage)]
        get => GetNamedColor("magentaColor");
    }


    /// <summary>
    /// Orange.
    /// </summary>
    public static NSColor Orange
    {
        [RequiresDynamicCode(GetPropertyRdcMessage)]
        get => GetNamedColor("orangeColor");
    }


    /// <summary>
    /// Purple.
    /// </summary>
    public static NSColor Purple
    {
        [RequiresDynamicCode(GetPropertyRdcMessage)]
        get => GetNamedColor("purpleColor");
    }


    /// <summary>
    /// Text color for large secondary or disabled static text, separators, large glyphs/icons, etc
    /// </summary>
    public static NSColor QuaternaryLabel
    {
        [RequiresDynamicCode(GetPropertyRdcMessage)]
        get => GetNamedColor("quaternaryLabelColor");
    }


    /// <summary>
    /// Red.
    /// </summary>
    public static NSColor Red
    {
        [RequiresDynamicCode(GetPropertyRdcMessage)]
        get => GetNamedColor("redColor");
    }


    /// <summary>
    /// Get red component.
    /// </summary>
    public double RedComponent 
    { 
        get 
        {
            if (this.isDynamic)
                throw new InvalidOperationException("Cannot get RGBA from dynamic color before color space conversion.");
            if (!this.IsRGB)
                throw new InvalidOperationException("Cannot get RGBA from color with non-RGB model.");
            RedComponentSelector ??= Selector.FromName("redComponent");
#pragma warning disable IL3050
            return this.SendMessage<double>(RedComponentSelector);
#pragma warning restore IL3050
        }
    }


    /// <summary>
    /// Get RGBA components.
    /// </summary>
    public unsafe (double, double, double, double) RGBA
    {
        get
        {
            if (this.isDynamic)
                throw new InvalidOperationException("Cannot get RGBA from dynamic color before color space conversion.");
            if (!this.IsRGB)
                throw new InvalidOperationException("Cannot get RGBA from color with non-RGB model.");
            var rgba = stackalloc double[4];
            GetRgbaSelector ??= Selector.FromName("getRed:green:blue:alpha:");
            ((delegate*unmanaged<nint, nint, double*, double*, double*, double*, void>)SendMessageNative)(this.Handle, GetRgbaSelector.Handle, rgba, (rgba + 1), (rgba + 2), (rgba + 3));
            return (rgba[0], rgba[1], rgba[2], rgba[3]);
        }
    }


    /// <summary>
    /// Scroll bar slot color.
    /// </summary>
    public static NSColor ScrollBar
    {
        [RequiresDynamicCode(GetPropertyRdcMessage)]
        get => GetNamedColor("scrollBarColor");
    }


    /// <summary>
    /// Similar to SelectedControlColor.
    /// </summary>
    public static NSColor ScrubberTexturedBackground
    {
        [RequiresDynamicCode(GetPropertyRdcMessage)]
        get => GetNamedColor("scrubberTexturedBackgroundColor");
    }


    /// <summary>
    /// Patterned background color for use in NSScrubber.
    /// </summary>
    public static NSColor SecondaryLabel
    {
        [RequiresDynamicCode(GetPropertyRdcMessage)]
        get => GetNamedColor("secondaryLabelColor");
    }


    /// <summary>
    /// Color for selected controls when control is not active.
    /// </summary>
    public static NSColor SecondarySelectedControl
    {
        [RequiresDynamicCode(GetPropertyRdcMessage)]
        get => GetNamedColor("secondarySelectedControlColor");
    }


    /// <summary>
    /// Control face for selected controls.
    /// </summary>
    public static NSColor SelectedControl
    {
        [RequiresDynamicCode(GetPropertyRdcMessage)]
        get => GetNamedColor("selectedControlColor");
    }


    /// <summary>
    /// Text on selected controls.
    /// </summary>
    public static NSColor SelectedControlText
    {
        [RequiresDynamicCode(GetPropertyRdcMessage)]
        get => GetNamedColor("selectedControlTextColor");
    }


    /// <summary>
    /// Knob face color for selected controls.
    /// </summary>
    public static NSColor SelectedKnob
    {
        [RequiresDynamicCode(GetPropertyRdcMessage)]
        get => GetNamedColor("selectedKnobColor");
    }


    /// <summary>
    /// Highlight color for menus.
    /// </summary>
    public static NSColor SelectedMenuItem
    {
        [RequiresDynamicCode(GetPropertyRdcMessage)]
        get => GetNamedColor("selectedMenuItemColor");
    }


    /// <summary>
    /// Highlight color for menu text.
    /// </summary>
    public static NSColor SelectedMenuItemText
    {
        [RequiresDynamicCode(GetPropertyRdcMessage)]
        get => GetNamedColor("selectedMenuItemTextColor");
    }


    /// <summary>
    /// Selected document text.
    /// </summary>
    public static NSColor SelectedText
    {
        [RequiresDynamicCode(GetPropertyRdcMessage)]
        get => GetNamedColor("selectedTextColor");
    }


    /// <summary>
    /// Selected document text background.
    /// </summary>
    public static NSColor SelectedTextBackground
    {
        [RequiresDynamicCode(GetPropertyRdcMessage)]
        get => GetNamedColor("selectedTextBackgroundColor");
    }


    /// <summary>
    /// Shadow color for UI elements.
    /// </summary>
    public static NSColor Shadow
    {
        [RequiresDynamicCode(GetPropertyRdcMessage)]
        get => GetNamedColor("shadowColor");
    }


    /// <summary>
    /// Creates a new color object that represents a blend between the current color and the shadow color.
    /// </summary>
    /// <param name="level">The level, range is [0.0, 1.0].</param>
    /// <returns>Color object, or Null if the colors can’t be converted.</returns>
    [RequiresDynamicCode(CallMethodRdcMessage)]
    public NSColor? ShadowWith(double level)
    {
        ShadowSelector ??= Selector.FromName("shadowWithLevel:");
        return this.SendMessage<NSColor?>(ShadowSelector, level);
    }


    /// <summary>
    /// Text color for disabled static text and related elements.
    /// </summary>
    public static NSColor TertiaryLabel
    {
        [RequiresDynamicCode(GetPropertyRdcMessage)]
        get => GetNamedColor("tertiaryLabelColor");
    }


    /// <summary>
    /// Document text background.
    /// </summary>
    public static NSColor TextBackground
    {
        [RequiresDynamicCode(GetPropertyRdcMessage)]
        get => GetNamedColor("textBackgroundColor");
    }


    /// <summary>
    /// Document text.
    /// </summary>
    public static NSColor Text
    {
        [RequiresDynamicCode(GetPropertyRdcMessage)]
        get => GetNamedColor("textColor");
    }


    /// <inheritdoc/>
    public override string ToString()
    {
        var colorSpace = this.ColorSpace;
        if (colorSpace?.ColorSpaceModel == NSColorSpace.Model.RGB)
        {
            var (r, g, b, a) = this.RGBA;
            return $"(R:{r:F3}, G:{g:F3}, B:{b:F3}, A:{a:F3})";
        }
        if (colorSpace?.ColorSpaceModel == NSColorSpace.Model.CMYK)
        {
            var (c, m, y, k, a) = this.CMYK;
            return $"(C:{c:F3}, M:{m:F3}, Y:{y:F3}, K:{k:F3}, A:{a:F3})";
        }
        if (this.isDynamic)
            return "(Dynamic Color)";
        if (this.isPatterned)
            return "(Dynamic Color)";
        return "(Color)";
    }
    

    /// <summary>
    /// Get type of color.
    /// </summary>
    public ColorType Type 
    { 
        get 
        {
            TypeSelector ??= Selector.FromName("type");
#pragma warning disable IL3050
            return (ColorType)this.SendMessage<int>(TypeSelector);
#pragma warning restore IL3050
        }
    }


    /// <summary>
    /// Background areas revealed behind views.
    /// </summary>
    public static NSColor UnderPageBackground
    {
        [RequiresDynamicCode(GetPropertyRdcMessage)]
        get => GetNamedColor("underPageBackgroundColor");
    }


    /// <summary>
    /// Create a new color object representing the color of the current color object in the specified color space.
    /// </summary>
    /// <param name="colorSpace">Color space.</param>
    /// <returns>Color object with color space, or Null if conversion is not possible.</returns>
    [RequiresDynamicCode(CallMethodRdcMessage)]
    public NSColor? UseColorSpace(NSColorSpace colorSpace)
    {
        UsingColorSpaceSelector ??= Selector.FromName("colorUsingColorSpace:");
        return this.SendMessage<NSColor?>(UsingColorSpaceSelector, colorSpace);
    }


    /// <summary>
    /// Return a version of the color object that is compatible with the specified color type.
    /// </summary>
    /// <param name="type">Color type.</param>
    /// <returns>A compatible color object, or Null if a compatible color object is not available.</returns>
    [RequiresDynamicCode(CallMethodRdcMessage)]
    public NSColor? UseType(ColorType type)
    {
        UsingTypeSelector ??= Selector.FromName("colorUsingType:");
        return this.SendMessage<NSColor?>(UsingTypeSelector, type);
    }


    /// <summary>
    /// White.
    /// </summary>
    public static NSColor White
    {
        [RequiresDynamicCode(GetPropertyRdcMessage)]
        get => GetNamedColor("whiteColor");
    }


    /// <summary>
    /// Background fill for window contents.
    /// </summary>
    public static NSColor WindowBackground
    {
        [RequiresDynamicCode(GetPropertyRdcMessage)]
        get => GetNamedColor("windowBackgroundColor");
    }


    /// <summary>
    /// Window frames.
    /// </summary>
    public static NSColor WindowFrame
    {
        [RequiresDynamicCode(GetPropertyRdcMessage)]
        get => GetNamedColor("windowFrameColor");
    }


    /// <summary>
    /// Text on window frames.
    /// </summary>
    public static NSColor WindowFrameText
    {
        [RequiresDynamicCode(GetPropertyRdcMessage)]
        get => GetNamedColor("windowFrameTextColor");
    }


    /// <summary>
    /// Creates a new color object that has the same color space and component values as the current color object, but the specified alpha component.
    /// </summary>
    /// <param name="alpha">Alpha component.</param>
    /// <returns>Color.</returns>
    [RequiresDynamicCode(CallMethodRdcMessage)]
    public NSColor WithAlphaComponent(double alpha)
    {
        WithAlphaSelector ??= Selector.FromName("colorWithAlphaComponent:");
        return this.SendMessage<NSColor>(WithAlphaSelector, alpha);
    }


    /// <summary>
    /// Yellow.
    /// </summary>
    public static NSColor Yellow
    {
        [RequiresDynamicCode(GetPropertyRdcMessage)]
        get => GetNamedColor("yellowColor");
    }
}