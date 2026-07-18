using CarinaStudio.MacOS.ObjectiveC;
using System;
using System.Diagnostics.CodeAnalysis;

namespace CarinaStudio.MacOS.AppKit;

/// <summary>
/// NSToolbar.
/// </summary>
public class NSToolbar : NSObject
{
    // Static fields.
    static Property? AllowsUserCustomizationProperty;
    static Property? AutosavesConfigurationProperty;
    static Property? DisplayModeProperty;
    static Property? IdentifierProperty;
    static Selector? InitWithIdentifierSelector;
    static Property? IsVisibleProperty;
    static readonly Class? NSToolbarClass;
    static Selector? SetShowsBaselineSeparatorSelector;
    static Selector? ShowsBaselineSeparatorSelector;


    // Static initializer.
    static NSToolbar()
    {
        if (Platform.IsNotMacOS)
            return;
        NSToolbarClass = Class.GetClass("NSToolbar").AsNonNull();
    }


    /// <summary>
    /// Initialize new <see cref="NSToolbar"/> instance.
    /// </summary>
#if NET7_0_OR_GREATER
    [RequiresDynamicCode(CallConstructorRdcMessage)]
#endif
    public NSToolbar() : this(Initialize(NSToolbarClass!.Allocate()), true)
    { }


    /// <summary>
    /// Initialize new <see cref="NSToolbar"/> instance.
    /// </summary>
    /// <param name="identifier">Identifier of toolbar.</param>
#if NET7_0_OR_GREATER
    [RequiresDynamicCode(CallConstructorRdcMessage)]
#endif
    public NSToolbar(string identifier) : this(Initialize(NSToolbarClass!.Allocate(), identifier), true)
    { }


    /// <summary>
    /// Initialize new <see cref="NSToolbar"/> instance.
    /// </summary>
    /// <param name="handle">Handle of instance.</param>
    /// <param name="verifyClass">True to verify whether instance is NSToolbar or not.</param>
    /// <param name="ownsInstance">True to owns the instance.</param>
    protected NSToolbar(IntPtr handle, bool verifyClass, bool ownsInstance) : base(handle, ownsInstance)
    {
        if (verifyClass)
            this.VerifyClass(NSToolbarClass!);
    }


    /// <summary>
    /// Initialize new <see cref="NSToolbar"/> instance.
    /// </summary>
    /// <param name="cls">Class of instance.</param>
    /// <param name="handle">Handle of instance.</param>
    /// <param name="ownsInstance">True to owns the instance.</param>
    protected NSToolbar(Class cls, IntPtr handle, bool ownsInstance) : base(cls, handle, ownsInstance)
    { }


    // Constructor.
    NSToolbar(IntPtr handle, bool ownsInstance) : this(handle, true, ownsInstance)
    { }


    /// <summary>
    /// Get or set whether users are allowed to modify the toolbar or not.
    /// </summary>
    public bool AllowsUserCustomization
    {
        get
        {
            AllowsUserCustomizationProperty ??= NSToolbarClass!.GetProperty("allowsUserCustomization").AsNonNull();
            return this.GetBooleanProperty(AllowsUserCustomizationProperty);
        }
        set
        {
            AllowsUserCustomizationProperty ??= NSToolbarClass!.GetProperty("allowsUserCustomization").AsNonNull();
            this.SetProperty(AllowsUserCustomizationProperty, value);
        }
    }


    /// <summary>
    /// Get or set whether the toolbar saves its configuration automatically or not.
    /// </summary>
    public bool AutosavesConfiguration
    {
        get
        {
            AutosavesConfigurationProperty ??= NSToolbarClass!.GetProperty("autosavesConfiguration").AsNonNull();
            return this.GetBooleanProperty(AutosavesConfigurationProperty);
        }
        set
        {
            AutosavesConfigurationProperty ??= NSToolbarClass!.GetProperty("autosavesConfiguration").AsNonNull();
            this.SetProperty(AutosavesConfigurationProperty, value);
        }
    }


    /// <summary>
    /// Get or set the way of the toolbar to display its items.
    /// </summary>
    public NSToolbarDisplayMode DisplayMode
    {
        get
        {
            DisplayModeProperty ??= NSToolbarClass!.GetProperty("displayMode").AsNonNull();
            return (NSToolbarDisplayMode)this.GetUInt32Property(DisplayModeProperty);
        }
        set
        {
            DisplayModeProperty ??= NSToolbarClass!.GetProperty("displayMode").AsNonNull();
            this.SetProperty(DisplayModeProperty, (uint)value);
        }
    }


    /// <summary>
    /// Get identifier of toolbar.
    /// </summary>
    public string? Identifier
    {
        get
        {
            IdentifierProperty ??= NSToolbarClass!.GetProperty("identifier").AsNonNull();
            using var identifier = this.GetNSObjectProperty<NSString>(IdentifierProperty);
            return identifier?.ToString();
        }
    }


    // Initialize allocated instance with identifier.
#if NET7_0_OR_GREATER
    [RequiresDynamicCode(CallMethodRdcMessage)]
#endif
    static IntPtr Initialize(IntPtr obj, string identifier)
    {
        InitWithIdentifierSelector ??= Selector.FromName("initWithIdentifier:");
        using var nsIdentifier = new NSString(identifier);
        return SendMessage<IntPtr>(obj, InitWithIdentifierSelector, nsIdentifier);
    }


    /// <summary>
    /// Get or set whether the toolbar is visible or not.
    /// </summary>
    public bool IsVisible
    {
        get
        {
            IsVisibleProperty ??= NSToolbarClass!.GetProperty("visible").AsNonNull();
            return this.GetBooleanProperty(IsVisibleProperty);
        }
        set
        {
            IsVisibleProperty ??= NSToolbarClass!.GetProperty("visible").AsNonNull();
            this.SetProperty(IsVisibleProperty, value);
        }
    }


    /// <summary>
    /// Get or set whether the toolbar shows the separator between the toolbar and the main window contents or not. The property has no effect on macOS 15.0 and later.
    /// </summary>
    /// <remarks>The property is accessed through selectors directly because its property meta data has been removed from Objective-C runtime on later macOS.</remarks>
    public bool ShowsBaselineSeparator
    {
        get
        {
            ShowsBaselineSeparatorSelector ??= Selector.FromName("showsBaselineSeparator");
#pragma warning disable IL3050
            return this.SendMessage<bool>(ShowsBaselineSeparatorSelector);
#pragma warning restore IL3050
        }
        set
        {
            SetShowsBaselineSeparatorSelector ??= Selector.FromName("setShowsBaselineSeparator:");
#pragma warning disable IL3050
            this.SendMessage(SetShowsBaselineSeparatorSelector, value);
#pragma warning restore IL3050
        }
    }
}
