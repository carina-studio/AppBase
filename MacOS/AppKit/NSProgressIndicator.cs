using CarinaStudio.MacOS.ObjectiveC;
using System;
using System.Diagnostics.CodeAnalysis;

namespace CarinaStudio.MacOS.AppKit;

/// <summary>
/// NSProgressIndicator.
/// </summary>
public class NSProgressIndicator : NSView
{
    // Static fields.
    static Property? ControlSizeProperty;
    static Property? ControlTintProperty;
    static Property? DoubleValueProperty;
    static Selector? IncrementBySelector;
    static Property? IsBezeledProperty;
    static Property? IsIndeterminateProperty;
    static Property? MaxValueProperty;
    static Property? MinValueProperty;
    static readonly Class? NSProgressIndicatorClass;
    static Selector? StartAnimationSelector;
    static Selector? StopAnimationSelector;
    static Property? StyleProperty;


    // Static initializer.
    static NSProgressIndicator()
    {
        if (Platform.IsNotMacOS)
            return;
        NSProgressIndicatorClass = Class.GetClass("NSProgressIndicator").AsNonNull();
    }


    /// <summary>
    /// Initialize new <see cref="NSProgressIndicator"/> instance.
    /// </summary>
    /// <param name="frame">Frame.</param>
#if NET7_0_OR_GREATER
    [RequiresDynamicCode(CallConstructorRdcMessage)]
#endif
    public NSProgressIndicator(NSRect frame) : base(NSProgressIndicatorClass!.Allocate(), frame)
    { }


    // Constructor.
#pragma warning disable IDE0051
    NSProgressIndicator(IntPtr handle, bool ownsInstance) : base(handle, false, ownsInstance) =>
        this.VerifyClass(NSProgressIndicatorClass!);
    NSProgressIndicator(Class cls, IntPtr handle, bool ownsInstance) : base(cls, handle, ownsInstance)
    { }
#pragma warning restore IDE0051


    /// <summary>
    /// Get or set size of progress indicator.
    /// </summary>
    public NSControlSize ControlSize
    {
        get 
        {
            ControlSizeProperty ??= NSProgressIndicatorClass!.GetProperty("controlSize").AsNonNull();
            return (NSControlSize)this.GetUInt32Property(ControlSizeProperty);
        }
        set 
        {
            ControlSizeProperty ??= NSProgressIndicatorClass!.GetProperty("controlSize").AsNonNull();
            this.SetProperty(ControlSizeProperty, (uint)value);
        }
    }
    

    /// <summary>
    /// Get or set tint of progress indicator.
    /// </summary>
    public NSControlTint ControlTint
    {
        get 
        {
            ControlTintProperty ??= NSProgressIndicatorClass!.GetProperty("controlTint").AsNonNull();
            return (NSControlTint)this.GetUInt32Property(ControlTintProperty);
        }
        set 
        {
            ControlTintProperty ??= NSProgressIndicatorClass!.GetProperty("controlTint").AsNonNull();
            this.SetProperty(ControlTintProperty, (uint)value);
        }
    }
    

    /// <summary>
    /// Get or set value of progress indicator.
    /// </summary>
    public double DoubleValue
    {
        get 
        {
            DoubleValueProperty ??= NSProgressIndicatorClass!.GetProperty("doubleValue").AsNonNull();
            return this.GetDoubleProperty(DoubleValueProperty);
        }
        set 
        {
            DoubleValueProperty ??= NSProgressIndicatorClass!.GetProperty("doubleValue").AsNonNull();
            this.SetProperty(DoubleValueProperty, value);
        }
    }
    

    /// <summary>
    /// Increment the value of progress indicator.
    /// </summary>
    /// <param name="delta">Value to increment.</param>
#if NET7_0_OR_GREATER
    [RequiresDynamicCode(CallMethodRdcMessage)]
#endif
    public void Increment(double delta)
    {
        IncrementBySelector ??= Selector.FromName("incrementBy:");
        this.SendMessage(IncrementBySelector, delta);
    }
    

    /// <summary>
    /// Get or set whether the progress indicator’s frame has a three-dimensional bezel.
    /// </summary>
    public bool IsBezeled
    {
        get 
        {
            IsBezeledProperty ??= NSProgressIndicatorClass!.GetProperty("bezeled").AsNonNull();
            return this.GetBooleanProperty(IsBezeledProperty);
        }
        set 
        {
            IsBezeledProperty ??= NSProgressIndicatorClass!.GetProperty("bezeled").AsNonNull();
            this.SetProperty(IsBezeledProperty, value);
        }
    }
    

    /// <summary>
    /// Get or set whether progress indicator is indeterminate or not.
    /// </summary>
    public bool IsIndeterminate
    {
        get 
        {
            IsIndeterminateProperty ??= NSProgressIndicatorClass!.GetProperty("indeterminate").AsNonNull();
            return this.GetBooleanProperty(IsIndeterminateProperty);
        }
        set 
        {
            IsIndeterminateProperty ??= NSProgressIndicatorClass!.GetProperty("indeterminate").AsNonNull();
            this.SetProperty(IsIndeterminateProperty, value);
        }
    }


    /// <summary>
    /// Get or set maximum value of progress indicator.
    /// </summary>
    public double MaxValue
    {
        get 
        {
            MaxValueProperty ??= NSProgressIndicatorClass!.GetProperty("maxValue").AsNonNull();
            return this.GetDoubleProperty(MaxValueProperty);
        }
        set 
        {
            MaxValueProperty ??= NSProgressIndicatorClass!.GetProperty("maxValue").AsNonNull();
            this.SetProperty(MaxValueProperty, value);
        }
    }


    /// <summary>
    /// Get or set minimum value of progress indicator.
    /// </summary>
    public double MinValue
    {
        get 
        {
            MinValueProperty ??= NSProgressIndicatorClass!.GetProperty("minValue").AsNonNull();
            return this.GetDoubleProperty(MinValueProperty);
        }
        set 
        {
            MinValueProperty ??= NSProgressIndicatorClass!.GetProperty("minValue").AsNonNull();
            this.SetProperty(MinValueProperty, value);
        }
    }


    /// <summary>
    /// Start animation of indeterminate progress indicator.
    /// </summary>
    /// <param name="sender">Sender.</param>
#if NET7_0_OR_GREATER
    [RequiresDynamicCode(CallMethodRdcMessage)]
#endif
    public void StartAnimation(NSObject? sender = null) 
    {
        StartAnimationSelector ??= Selector.FromName("startAnimation:");
        this.SendMessage(StartAnimationSelector, sender);
    }
    

    /// <summary>
    /// Stop animation of indeterminate progress indicator.
    /// </summary>
    /// <param name="sender">Sender.</param>
#if NET7_0_OR_GREATER
    [RequiresDynamicCode(CallMethodRdcMessage)]
#endif
    public void StopAnimation(NSObject? sender = null)
    {
        StopAnimationSelector ??= Selector.FromName("stopAnimation:");
        this.SendMessage(StopAnimationSelector, sender);
    }
    

    /// <summary>
    /// Get or set style of progress indicator.
    /// </summary>
    public NSProgressIndicatorStyle Style
    {
        get 
        {
            StyleProperty ??= NSProgressIndicatorClass!.GetProperty("style").AsNonNull();
            return (NSProgressIndicatorStyle)this.GetUInt32Property(StyleProperty);
        }
        set 
        {
            StyleProperty ??= NSProgressIndicatorClass!.GetProperty("style").AsNonNull();
            this.SetProperty(StyleProperty, (uint)value);
        }
    }
}