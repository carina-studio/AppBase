using System.Reflection.Metadata;
using CarinaStudio.MacOS.ObjectiveC;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

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
    public enum ColorType : int
    {
        ComponentBased = 0,
        Pattern = 1,
        Catalog = 2,
    }
#pragma warning restore CS1591


    // Static fields.
    static readonly Selector? AlphaComponentSelector;
    static readonly Selector? BlendedSelector;
    static readonly Selector? BlueComponentSelector;
    static readonly Selector? ColorSpaceSelector;
    static readonly Selector? ColorWithColorSpaceHsiSelector;
    static readonly Selector? ColorWithColorSpaceSelector;
    static readonly Selector? ColorWithDisplayP3Selector;
    static readonly Selector? ColorWithSrgbSelector;
    static readonly Selector? GetCmykaSelector;
    static readonly Selector? GetHsiaSelector;
    static readonly Selector? GetRgbaSelector;
    static readonly Selector? GetWhiteSelector;
    static readonly Selector? GreenComponentSelector;
    static readonly Selector? HighlightSelector;
    static readonly IDictionary<string, NSColor> NamedColors = new ConcurrentDictionary<string, NSColor>();
    static readonly Class? NSColorClass;
    static readonly Selector? RedComponentSelector;
    static readonly Selector? ShadowSelector;
    static readonly Selector? TypeSelector;
    static readonly Selector? UsingColorSpaceSelector;
    static readonly Selector? UsingTypeSelector;
    static readonly Selector? WithAlphaSelector;


    // Static fields.
    static NSColor()
    {
        if (Platform.IsNotMacOS)
            return;
        NSColorClass = Class.GetClass(nameof(NSColor)).AsNonNull();
        AlphaComponentSelector = Selector.FromName("alphaComponent");
        BlendedSelector = Selector.FromName("blendedColorWithFraction:ofColor:");
        BlueComponentSelector = Selector.FromName("blueComponent");
        ColorSpaceSelector = Selector.FromName("colorSpace");
        ColorWithColorSpaceHsiSelector = Selector.FromName("colorWithColorSpace:hue:saturation:brightness:alpha:");
        ColorWithColorSpaceSelector = Selector.FromName("colorWithColorSpace:components:count:");
        ColorWithDisplayP3Selector = Selector.FromName("colorWithDisplayP3Red:green:blue:alpha:");
        ColorWithSrgbSelector = Selector.FromName("colorWithSRGBRed:green:blue:alpha:");
        GetCmykaSelector = Selector.FromName("getCyan:magenta:yellow:black:alpha:");
        GetHsiaSelector = Selector.FromName("getHue:saturation:brightness:alpha:");
        GetRgbaSelector = Selector.FromName("getRed:green:blue:alpha:");
        GetWhiteSelector = Selector.FromName("getWhite:alpha:");
        GreenComponentSelector = Selector.FromName("greenComponent");
        HighlightSelector = Selector.FromName("highlightWithLevel:");
        RedComponentSelector = Selector.FromName("redComponent");
        ShadowSelector = Selector.FromName("shadowWithLevel:");
        TypeSelector = Selector.FromName("type");
        UsingColorSpaceSelector = Selector.FromName("colorUsingColorSpace:");
        UsingTypeSelector = Selector.FromName("colorUsingType:");
        WithAlphaSelector = Selector.FromName("colorWithAlphaComponent:");
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
            return this.SendMessage<double>(AlphaComponentSelector!);
        }
    }


    /// <summary>
    /// Similar to SelectedControlColor.
    /// </summary>
    public static NSColor AlternateSelectedControl { get => GetNamedColor("alternateSelectedControlColor"); }


    /// <summary>
    /// Similar to SelectedControlTextColor.
    /// </summary>
    public static NSColor AlternateSelectedControlText { get => GetNamedColor("alternateSelectedControlTextColor"); }


    /// <summary>
    /// Black.
    /// </summary>
    public static NSColor Black { get => GetNamedColor("blackColor"); }


    /// <summary>
    /// Creates a new color object whose component values are a weighted sum of the current color object and the specified color object's.
    /// </summary>
    /// <param name="fraction">The amount of the color to blend with this color.</param>
    /// <param name="color">The color to blend with this color.</param>
    /// <returns>Blended color, or Null if the colors can’t be converted.</returns>
    public NSColor? BlendWith(double fraction, NSColor color) =>
        this.SendMessage<NSColor>(BlendedSelector!, fraction, color);


    /// <summary>
    /// Blue.
    /// </summary>
    public static NSColor Blue { get => GetNamedColor("blueColor"); }


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
            return this.SendMessage<double>(BlueComponentSelector!);
        }
    }


    /// <summary>
    /// Brown.
    /// </summary>
    public static NSColor Brown { get => GetNamedColor("brownColor"); }


    /// <summary>
    /// Transparent.
    /// </summary>
    public static NSColor Clear { get => GetNamedColor("clearColor"); }


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
            this.SendMessage(GetCmykaSelector!, (IntPtr)cmyka, (IntPtr)(cmyka + 1), (IntPtr)(cmyka + 2), (IntPtr)(cmyka + 3), (IntPtr)(cmyka + 4));
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
            return this.SendMessage<NSColorSpace>(ColorSpaceSelector!);
        }
    }


    /// <summary>
    /// Control face and old window background color.
    /// </summary>
    public static NSColor Control { get => GetNamedColor("controlColor"); }


    /// <summary>
    /// Background of large controls.
    /// </summary>
    public static NSColor ControlBackground { get => GetNamedColor("controlBackgroundColor"); }


    /// <summary>
    /// Darker border for controls.
    /// </summary>
    public static NSColor ControlDarkShadow { get => GetNamedColor("controlDarkShadowColor"); }


    /// <summary>
    /// Light border for controls.
    /// </summary>
    public static NSColor ControlHighlight { get => GetNamedColor("controlHighlightColor"); }


    /// <summary>
    /// Lighter border for controls.
    /// </summary>
    public static NSColor ControlLightHighlight { get => GetNamedColor("controlLightHighlightColor"); }


    /// <summary>
    /// Dark border for controls.
    /// </summary>
    public static NSColor ControlShadow { get => GetNamedColor("controlShadowColor"); }


    /// <summary>
    /// Text on controls.
    /// </summary>
    public static NSColor ControlText { get => GetNamedColor("controlTextColor"); }


    /// <summary>
    /// Create color object.
    /// </summary>
    /// <param name="colorSpace">Color space</param>
    /// <param name="components">Color components.</param>
    /// <returns>Color.</returns>
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
        fixed (double* p = components)
        {
            var handle = NSObject.SendMessage<IntPtr>(NSColorClass!.Handle, ColorWithColorSpaceSelector!, colorSpace, (IntPtr)p, componentCount);
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
    public unsafe static NSColor CreateHSI(NSColorSpace colorSpace, double h, double s, double i, double a = 1.0)
    {
        if (colorSpace.ColorSpaceModel != NSColorSpace.Model.RGB)
            throw new ArgumentException($"Color model of '{colorSpace.LocalizedName}' is not RGB.");
        var handle = NSObject.SendMessage<IntPtr>(NSColorClass!.Handle, ColorWithColorSpaceHsiSelector!, colorSpace, h, s, i, a);
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
    public unsafe static NSColor CreateRGBA(NSColorSpace colorSpace, double r, double g, double b, double a = 1.0)
    {
        if (colorSpace.ColorSpaceModel != NSColorSpace.Model.RGB)
            throw new ArgumentException($"Color model of '{colorSpace.LocalizedName}' is not RGB.");
        var components = stackalloc double[] { r, g, b, a };
        var handle = NSObject.SendMessage<IntPtr>(NSColorClass!.Handle, ColorWithColorSpaceSelector!, colorSpace, (IntPtr)components, 4);
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
    public static NSColor CreateWithSRGB(double r, double g, double b, double a = 1.0) =>
        new NSColor(NSObject.SendMessage<IntPtr>(NSColorClass!.Handle, ColorWithSrgbSelector!, r, g, b, a), true);
    

    /// <summary>
    /// Create Display-P3 color object.
    /// </summary>
    /// <param name="r">Red.</param>
    /// <param name="g">Green.</param>
    /// <param name="b">Blue.</param>
    /// <param name="a">Alpha.</param>
    /// <returns>Color.</returns>
    public static NSColor CreateWithDisplayP3(double r, double g, double b, double a = 1.0) =>
        new NSColor(NSObject.SendMessage<IntPtr>(NSColorClass!.Handle, ColorWithDisplayP3Selector!, r, g, b, a), true);
    

    /// <summary>
    /// Cyan.
    /// </summary>
    public static NSColor Cyan { get => GetNamedColor("cyanColor"); }
    

    /// <summary>
    /// Dark gray.
    /// </summary>
    public static NSColor DarkGray { get => GetNamedColor("darkGrayColor"); }


    /// <summary>
    /// Text on disabled controls.
    /// </summary>
    public static NSColor DisabledControlText { get => GetNamedColor("disabledControlTextColor"); }


    // Get color with given name.
    static NSColor GetNamedColor(string name)
    {
        if (NamedColors.TryGetValue(name, out var color))
            return color;
        var handle = NSObject.SendMessage<IntPtr>(NSColorClass!.Handle, Selector.FromName(name));
        return new NSColor(handle, false).Also(it => 
        {
            it.IsDefaultInstance = true;
            NamedColors.TryAdd(name, it);
        });
    }


    /// <summary>
    /// Gray.
    /// </summary>
    public static NSColor Gray { get => GetNamedColor("grayColor"); }


    /// <summary>
    /// Get grayscale and alpha of color.
    /// </summary>
    public unsafe (double, double) Grayscale
    {
        get
        {
            if (this.isDynamic)
                throw new InvalidOperationException("Cannot get grayscale from dynamic color before color space conversion.");
            if (!this.IsGrayscale)
                throw new InvalidOperationException("Cannot get grayscale from color with non-Gray model.");
            var ga = stackalloc double[2];
            this.SendMessage(GetWhiteSelector!, (IntPtr)ga, (IntPtr)(ga + 1));
            return (ga[0], ga[1]);
        }
    }


    /// <summary>
    /// Green.
    /// </summary>
    public static NSColor Green { get => GetNamedColor("greenColor"); }


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
            return this.SendMessage<double>(GreenComponentSelector!);
        }
    }


    /// <summary>
    /// Grids in controls.
    /// </summary>
    public static NSColor Grid { get => GetNamedColor("gridColor"); }


    /// <summary>
    /// Background color for header cells in Table/OutlineView.
    /// </summary>
    public static NSColor Header { get => GetNamedColor("headerColor"); }


    /// <summary>
    /// Text color for header cells in Table/OutlineView.
    /// </summary>
    public static NSColor HeaderText { get => GetNamedColor("headerTextColor"); }


    /// <summary>
    /// Highlight color for UI elements.
    /// </summary>
    public static NSColor Highlight { get => GetNamedColor("highlightColor"); }


    /// <summary>
    /// Creates a new color object that represents a blend between the current color and the highlight color.
    /// </summary>
    /// <param name="level">The level, range is [0.0, 1.0].</param>
    /// <returns>Color object, or Null if the colors can’t be converted.</returns>
    public NSColor? HighlightWith(double level) =>
        this.SendMessage<NSColor>(HighlightSelector!, level);


    /// <summary>
    /// Get hue, saturation, intensity (brightness) and alpha of color.
    /// </summary>
    public unsafe (double, double, double, double) HSI
    {
        get
        {
            if (this.isDynamic)
                throw new InvalidOperationException("Cannot get HSI from dynamic color before color space conversion.");
            if (!this.IsRGB)
                throw new InvalidOperationException("Cannot get HSI from color with non-RGB model.");
            var hsia = stackalloc double[4];
            this.SendMessage(GetHsiaSelector!, (IntPtr)hsia, (IntPtr)(hsia + 1), (IntPtr)(hsia + 2), (IntPtr)(hsia + 3));
            return (hsia[0], hsia[1], hsia[2], hsia[3]);
        }
    }


    /// <summary>
    /// Check whether the color model is <see cref="NSColorSpace.Model.CMYK"/> or not.
    /// </summary>
    public bool IsCMYK { get => this.ColorSpace?.ColorSpaceModel == NSColorSpace.Model.CMYK; }


    /// <summary>
    /// Check whether the color object represents a dynamic color or not.
    /// </summary>
    public bool IsDynamic { get => this.isDynamic; }
    

    /// <summary>
    /// Check whether the color model is <see cref="NSColorSpace.Model.Gray"/> or not.
    /// </summary>
    public bool IsGrayscale { get => this.ColorSpace?.ColorSpaceModel == NSColorSpace.Model.Gray; }


    /// <summary>
    /// Check whether the color model is <see cref="NSColorSpace.Model.Indexed"/> or not.
    /// </summary>
    public bool IsIndexed { get => this.ColorSpace?.ColorSpaceModel == NSColorSpace.Model.Indexed; }


    /// <summary>
    /// Check whether the color patterned color or not.
    /// </summary>
    public bool IsPatterned { get => this.isPatterned; }


    /// <summary>
    /// Check whether the color model is <see cref="NSColorSpace.Model.RGB"/> or not.
    /// </summary>
    public bool IsRGB { get => this.ColorSpace?.ColorSpaceModel == NSColorSpace.Model.RGB; }


    /// <summary>
    /// Keyboard focus ring around controls.
    /// </summary>
    public static NSColor KeyboardFocusIndicator { get => GetNamedColor("keyboardFocusIndicatorColor"); }


    /// <summary>
    /// Knob face color for controls.
    /// </summary>
    public static NSColor Knob { get => GetNamedColor("knobColor"); }


    /// <summary>
    /// Text color for static text and related elements.
    /// </summary>
    public static NSColor Label { get => GetNamedColor("labelColor"); }


    /// <summary>
    /// Light gray.
    /// </summary>
    public static NSColor LightGray { get => GetNamedColor("lightGrayColor"); }


    /// <summary>
    /// Magenta.
    /// </summary>
    public static NSColor Magenta { get => GetNamedColor("magentaColor"); }


    /// <summary>
    /// Orange.
    /// </summary>
    public static NSColor Orange { get => GetNamedColor("orangeColor"); }


    /// <summary>
    /// Purple.
    /// </summary>
    public static NSColor Purple { get => GetNamedColor("purpleColor"); }


    /// <summary>
    /// Text color for large secondary or disabled static text, separators, large glyphs/icons, etc
    /// </summary>
    public static NSColor QuaternaryLabel { get => GetNamedColor("quaternaryLabelColor"); }


    /// <summary>
    /// Red.
    /// </summary>
    public static NSColor Red { get => GetNamedColor("redColor"); }


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
            return this.SendMessage<double>(RedComponentSelector!);
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
            this.SendMessage(GetRgbaSelector!, (IntPtr)rgba, (IntPtr)(rgba + 1), (IntPtr)(rgba + 2), (IntPtr)(rgba + 3));
            return (rgba[0], rgba[1], rgba[2], rgba[3]);
        }
    }


    /// <summary>
    /// Scroll bar slot color.
    /// </summary>
    public static NSColor ScrollBar { get => GetNamedColor("scrollBarColor"); }


    /// <summary>
    /// Similar to SelectedControlColor.
    /// </summary>
    public static NSColor ScrubberTexturedBackground { get => GetNamedColor("scrubberTexturedBackgroundColor"); }


    /// <summary>
    /// Patterned background color for use in NSScrubber.
    /// </summary>
    public static NSColor SecondaryLabel { get => GetNamedColor("secondaryLabelColor"); }


    /// <summary>
    /// Color for selected controls when control is not active.
    /// </summary>
    public static NSColor SecondarySelectedControl { get => GetNamedColor("secondarySelectedControlColor"); }


    /// <summary>
    /// Control face for selected controls.
    /// </summary>
    public static NSColor SelectedControl { get => GetNamedColor("selectedControlColor"); }


    /// <summary>
    /// Text on selected controls.
    /// </summary>
    public static NSColor SelectedControlText { get => GetNamedColor("selectedControlTextColor"); }


    /// <summary>
    /// Knob face color for selected controls.
    /// </summary>
    public static NSColor SelectedKnob { get => GetNamedColor("selectedKnobColor"); }


    /// <summary>
    /// Highlight color for menus.
    /// </summary>
    public static NSColor SelectedMenuItem { get => GetNamedColor("selectedMenuItemColor"); }


    /// <summary>
    /// Highlight color for menu text.
    /// </summary>
    public static NSColor SelectedMenuItemText { get => GetNamedColor("selectedMenuItemTextColor"); }


    /// <summary>
    /// Selected document text.
    /// </summary>
    public static NSColor SelectedText { get => GetNamedColor("selectedTextColor"); }


    /// <summary>
    /// Selected document text background.
    /// </summary>
    public static NSColor SelectedTextBackground { get => GetNamedColor("selectedTextBackgroundColor"); }


    /// <summary>
    /// Shadow color for UI elements.
    /// </summary>
    public static NSColor Shadow { get => GetNamedColor("shadowColor"); }


    /// <summary>
    /// Creates a new color object that represents a blend between the current color and the shadow color.
    /// </summary>
    /// <param name="level">The level, range is [0.0, 1.0].</param>
    /// <returns>Color object, or Null if the colors can’t be converted.</returns>
    public NSColor? ShadowWith(double level) =>
        this.SendMessage<NSColor>(ShadowSelector!, level);


    /// <summary>
    /// Text color for disabled static text and related elements.
    /// </summary>
    public static NSColor TertiaryLabel { get => GetNamedColor("tertiaryLabelColor"); }


    /// <summary>
    /// Document text background.
    /// </summary>
    public static NSColor TextBackground { get => GetNamedColor("textBackgroundColor"); }


    /// <summary>
    /// Document text.
    /// </summary>
    public static NSColor Text { get => GetNamedColor("textColor"); }


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
    public ColorType Type { get => this.SendMessage<ColorType>(TypeSelector!); }


    /// <summary>
    /// Background areas revealed behind views.
    /// </summary>
    public static NSColor UnderPageBackground { get => GetNamedColor("underPageBackgroundColor"); }


    /// <summary>
    /// Create a new color object representing the color of the current color object in the specified color space.
    /// </summary>
    /// <param name="colorSpace">Color space.</param>
    /// <returns>Color object with color space, or Null if conversion is not possible.</returns>
    public NSColor? UseColorSpace(NSColorSpace colorSpace) =>
        this.SendMessage<NSColor>(UsingColorSpaceSelector!, colorSpace);


    /// <summary>
    /// Return a version of the color object that is compatible with the specified color type.
    /// </summary>
    /// <param name="type">Color type.</param>
    /// <returns>A compatible color object, or Null if a compatible color object is not available.</returns>
    public NSColor? UseType(ColorType type) =>
        this.SendMessage<NSColor>(UsingTypeSelector!, type);
    

    /// <summary>
    /// White.
    /// </summary>
    public static NSColor White { get => GetNamedColor("whiteColor"); }


    /// <summary>
    /// Background fill for window contents.
    /// </summary>
    public static NSColor WindowBackground { get => GetNamedColor("windowBackgroundColor"); }


    /// <summary>
    /// Window frames.
    /// </summary>
    public static NSColor WindowFrame { get => GetNamedColor("windowFrameColor"); }


    /// <summary>
    /// Text on window frames.
    /// </summary>
    public static NSColor WindowFrameText { get => GetNamedColor("windowFrameTextColor"); }


    /// <summary>
    /// Creates a new color object that has the same color space and component values as the current color object, but the specified alpha component.
    /// </summary>
    /// <param name="alpha">Alpha component.</param>
    /// <returns>Color.</returns>
    public NSColor WithAlphaComponent(double alpha) =>
        this.SendMessage<NSColor>(WithAlphaSelector!, alpha);


    /// <summary>
    /// Yellow.
    /// </summary>
    public static NSColor Yellow { get => GetNamedColor("yellowColor"); }
}