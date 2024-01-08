using CarinaStudio.MacOS.CoreFoundation;
using CarinaStudio.MacOS.CoreGraphics;
using CarinaStudio.MacOS.ObjectiveC;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace CarinaStudio.MacOS.AppKit;

/// <summary>
/// NSColorSpace.
/// </summary>
public class NSColorSpace : NSObject
{
#pragma warning disable CS1591
    /// <summary>
    /// Model.
    /// </summary>
    public enum Model
    {
        Unknown = -1,
        Gray = 0,
        RGB = 1,
        CMYK = 2,
        Lab = 3,
        DeviceN = 4,
        Indexed = 5,
        Patterned = 6,
    }
#pragma warning restore CS1591


    // Static fields.
    static Selector? AvailableColorSpacesSelector;
    static Selector? CGColorSpaceSelector;
    static Selector? ColorSpaceModelSelector;
    static Selector? IccProfileDataSelector;
    static Selector? InitWithCGColorSpaceSelector;
    static Selector? InitWithIccDataSelector;
    static Selector? LocalizedNameSelector;
    static readonly IDictionary<string, NSColorSpace> NamedColorSpaces = new ConcurrentDictionary<string, NSColorSpace>();
    static readonly Class? NSColorSpaceClass;
    static Selector? NumOfColorComponentsSelector;


    // Static initializer.
    static NSColorSpace()
    {
        if (Platform.IsNotMacOS)
            return;
        NSColorSpaceClass = Class.GetClass(nameof(NSColorSpace)).AsNonNull();
    }


    // Constructor.
    NSColorSpace(IntPtr handle, bool ownsInstance) : base(handle, ownsInstance) =>
        this.VerifyClass(NSColorSpaceClass!);
    NSColorSpace(Class cls, IntPtr handle, bool ownsInstance) : base(cls, handle, ownsInstance)
    { }


    /// <summary>
    /// Get predefined color space: Adobe RGB 1998.
    /// </summary>
    public static NSColorSpace AdobeRGB1998
    {
        [RequiresDynamicCode(GetPropertyRdcMessage)]
        get => GetNamedColorSpace("adobeRGB1998ColorSpace");
    }


    /// <summary>
    /// Get <see cref="CGColorSpace"/> which is used for creating the instance.
    /// </summary>
    public CGColorSpace? CGColorSpace 
    { 
        get 
        {
            CGColorSpaceSelector ??= Selector.FromName("CGColorSpace");
#pragma warning disable IL3050
            return this.SendMessage<CGColorSpace?>(CGColorSpaceSelector);
#pragma warning restore IL3050
        }
    }


    /// <summary>
    /// Get color model of color space.
    /// </summary>
    public Model ColorSpaceModel 
    { 
        get 
        {
            ColorSpaceModelSelector ??= Selector.FromName("colorSpaceModel");
#pragma warning disable IL3050
            return this.SendMessage<Model>(ColorSpaceModelSelector);
#pragma warning restore IL3050
        }
    }


    /// <summary>
    /// Get predefined color space: Device RGB.
    /// </summary>
    public static NSColorSpace DeviceRGB
    {
        [RequiresDynamicCode(GetPropertyRdcMessage)]
        get => GetNamedColorSpace("deviceRGBColorSpace");
    }


    /// <summary>
    /// Get predefined color space: Device gray.
    /// </summary>
    public static NSColorSpace DeviceGray
    {
        [RequiresDynamicCode(GetPropertyRdcMessage)]
        get => GetNamedColorSpace("deviceGrayColorSpace");
    }


    /// <summary>
    /// Get predefined color space: Device CMYK.
    /// </summary>
    public static NSColorSpace DeviceCMYK
    {
        [RequiresDynamicCode(GetPropertyRdcMessage)]
        get => GetNamedColorSpace("deviceCMYKColorSpace");
    }


    /// <summary>
    /// Get predefined color space: Display-P3.
    /// </summary>
    public static NSColorSpace DisplayP3
    {
        [RequiresDynamicCode(GetPropertyRdcMessage)]
        get => GetNamedColorSpace("displayP3ColorSpace");
    }


    /// <summary>
    /// Get predefined color space: Extended generic gray with gamma 2.2.
    /// </summary>
    public static NSColorSpace ExtendedGenericGamma22Gray
    {
        [RequiresDynamicCode(GetPropertyRdcMessage)]
        get => GetNamedColorSpace("extendedGenericGamma22GrayColorSpace");
    }


    /// <summary>
    /// Get predefined color space: Extended sRGB.
    /// </summary>
    public static NSColorSpace ExtendedSRGB
    {
        [RequiresDynamicCode(GetPropertyRdcMessage)]
        get => GetNamedColorSpace("extendedSRGBColorSpace");
    }


    /// <summary>
    /// Create <see cref="NSColorSpace"/> from <see cref="CGColorSpace"/>.
    /// </summary>
    /// <param name="colorSpace"><see cref="CGColorSpace"/>.</param>
    /// <returns>Color space.</returns>
    [RequiresDynamicCode(CallMethodRdcMessage)]
    public static NSColorSpace FromCGColorSpace(CGColorSpace colorSpace)
    {
        if (colorSpace.IsReleased)
            throw new ObjectDisposedException(nameof(CGColorSpace));
        InitWithCGColorSpaceSelector ??= Selector.FromName("initWithCGColorSpace:");
        var handle = SendMessage<IntPtr>(NSColorSpaceClass!.Allocate(), InitWithCGColorSpaceSelector, colorSpace);
        return new(NSColorSpaceClass, handle, true);
    }


    /// <summary>
    /// Create <see cref="NSColorSpace"/> from ICC profile.
    /// </summary>
    /// <param name="iccProfile">Data contains ICC profile.</param>
    /// <returns>Color space.</returns>
    [RequiresDynamicCode(CallMethodRdcMessage)]
    public static NSColorSpace FromIccProfile(CFData iccProfile)
    {
        if (iccProfile.IsReleased)
            throw new ObjectDisposedException(nameof(CFData));
        InitWithIccDataSelector ??= Selector.FromName("initWithIccProfileData:");
        var handle = SendMessage<IntPtr>(NSColorSpaceClass!.Allocate(), InitWithIccDataSelector, iccProfile);
        return new(NSColorSpaceClass, handle, true);
    }


    /// <summary>
    /// Get predefined color space: Generic CMYK.
    /// </summary>
    public static NSColorSpace GenericCMYK
    {
        [RequiresDynamicCode(GetPropertyRdcMessage)]
        get => GetNamedColorSpace("genericCMYKColorSpace");
    }


    /// <summary>
    /// Get predefined color space: Generic gray with gamma 2.2.
    /// </summary>
    public static NSColorSpace GenericGamma22Gray
    {
        [RequiresDynamicCode(GetPropertyRdcMessage)]
        get => GetNamedColorSpace("genericGamma22GrayColorSpace");
    }


    /// <summary>
    /// Get predefined color space: Generic gray.
    /// </summary>
    public static NSColorSpace GenericGray
    {
        [RequiresDynamicCode(GetPropertyRdcMessage)]
        get => GetNamedColorSpace("genericGrayColorSpace");
    }


    /// <summary>
    /// Get predefined color space: Generic RGB.
    /// </summary>
    public static NSColorSpace GenericRGB
    {
        [RequiresDynamicCode(GetPropertyRdcMessage)]
        get => GetNamedColorSpace("genericRGBColorSpace");
    }


    /// <summary>
    /// Get color spaces which support given color model.
    /// </summary>
    /// <param name="model">Color model.</param>
    /// <returns>Available color spaces.</returns>
    [RequiresDynamicCode(CallMethodRdcMessage)]
    public static NSColorSpace[] GetAvailableColorSpaces(Model model)
    {
        AvailableColorSpacesSelector ??= Selector.FromName("availableColorSpacesWithModel:");
        using var array = SendMessage<NSArray<NSColorSpace>?>(NSColorSpaceClass!.Handle, AvailableColorSpacesSelector, model);
        if (array is null)
            return Array.Empty<NSColorSpace>();
        return new NSColorSpace[array.Count].Also(it =>
        {
            for (var i = it.Length - 1; i >= 0; --i)
                it[i] = array[i];
        });
    }


    // Get color space with given name.
    [RequiresDynamicCode(CallMethodRdcMessage)]
    static NSColorSpace GetNamedColorSpace(string name)
    {
        if (NamedColorSpaces.TryGetValue(name, out var colorSpace))
            return colorSpace;
        var handle = SendMessage<IntPtr>(NSColorSpaceClass!.Handle, Selector.FromName(name));
        return new NSColorSpace(NSColorSpaceClass, handle, false).Also(it =>
        {
            it.IsDefaultInstance = true;
            NamedColorSpaces.TryAdd(name, it);
        });
    }


    /// <summary>
    /// Get <see cref="CFData"/> which contains ICC profile for creating the instance.
    /// </summary>
    public CFData? IccProfileData 
    { 
        get 
        {
            IccProfileDataSelector ??= Selector.FromName("ICCProfileData");
#pragma warning disable IL3050
            return this.SendMessage<CFData?>(IccProfileDataSelector);
#pragma warning restore IL3050
        }
    }


    /// <summary>
    /// Get localized name of color space.
    /// </summary>
    public string? LocalizedName
    {
        get
        {
            LocalizedNameSelector ??= Selector.FromName("localizedName");
#pragma warning disable IL3050
            using var s = this.SendMessage<NSString?>(LocalizedNameSelector);
#pragma warning restore IL3050
            return s?.ToString();
        }
    }


    /// <summary>
    /// Get number of color components excluding alpha component.
    /// </summary>
    public int NumberOfColorComponents 
    { 
        get 
        {
            NumOfColorComponentsSelector ??= Selector.FromName("numberOfColorComponents");
#pragma warning disable IL3050
            return this.SendMessage<int>(NumOfColorComponentsSelector);
#pragma warning restore IL3050
        }
    }


    /// <summary>
    /// Get predefined color space: sRGB.
    /// </summary>
    public static NSColorSpace SRGB
    {
        [RequiresDynamicCode(GetPropertyRdcMessage)]
        get => GetNamedColorSpace("sRGBColorSpace");
    }


    /// <inheritdoc/>
    public override string ToString()
    {
        var name = this.IsReleased ? default : this.LocalizedName;
        if (string.IsNullOrEmpty(name))
            return "{NSColorSpace}";
        return $"{{{name}}}";
    }
}