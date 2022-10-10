using CarinaStudio.MacOS.ObjectiveC;
using System;

namespace CarinaStudio.MacOS.AppKit;

/// <summary>
/// NSProgressIndicator.
/// </summary>
public class NSProgressIndicator : NSView
{
    // Static fields.
    static readonly Property? ControlSizeProperty;
    static readonly Property? ControlTintProperty;
    static readonly Property? DoubleValueProperty;
    static readonly Selector? IncrementBySelector;
    static readonly Property? IsBezeledProperty;
    static readonly Property? IsIndeterminateProperty;
    static readonly Property? MaxValueProperty;
    static readonly Property? MinValueProperty;
    static readonly Class? NSProgressIndicatorClass;
    static readonly Selector? StartAnimationSelector;
    static readonly Selector? StopAnimationSelector;
    static readonly Property? StyleProperty;


    // Static initializer.
    static NSProgressIndicator()
    {
        if (Platform.IsNotMacOS)
            return;
        NSProgressIndicatorClass = Class.GetClass("NSProgressIndicator").AsNonNull();
        ControlSizeProperty = NSProgressIndicatorClass.GetProperty("controlSize");
        ControlTintProperty = NSProgressIndicatorClass.GetProperty("controlTint");
        DoubleValueProperty = NSProgressIndicatorClass.GetProperty("doubleValue");
        IncrementBySelector = Selector.FromName("incrementBy:");
        IsBezeledProperty = NSProgressIndicatorClass.GetProperty("bezeled");
        IsIndeterminateProperty = NSProgressIndicatorClass.GetProperty("indeterminate");
        MaxValueProperty = NSProgressIndicatorClass.GetProperty("maxValue");
        MinValueProperty = NSProgressIndicatorClass.GetProperty("minValue");
        StartAnimationSelector = Selector.FromName("startAnimation:");
        StopAnimationSelector = Selector.FromName("stopAnimation:");
        StyleProperty = NSProgressIndicatorClass.GetProperty("style");
    }


    /// <summary>
    /// Initialize new <see cref="NSProgressIndicator"/> instance.
    /// </summary>
    /// <param name="frame">Frame.</param>
    public NSProgressIndicator(NSRect frame) : base(NSProgressIndicatorClass!.Allocate(), frame)
    { }


    // Constructor.
    NSProgressIndicator(IntPtr handle, bool ownsInstance) : base(handle, false, ownsInstance) =>
        this.VerifyClass(NSProgressIndicatorClass!);
    NSProgressIndicator(Class cls, IntPtr handle, bool ownsInstance) : base(cls, handle, ownsInstance)
    { }


    /// <summary>
    /// Get or set size of progress indicator.
    /// </summary>
    public NSControlSize ControlSize
    {
        get => this.GetProperty<NSControlSize>(ControlSizeProperty!);
        set => this.SetProperty(ControlSizeProperty!, value);
    }
    

    /// <summary>
    /// Get or set tint of progress indicator.
    /// </summary>
    public NSControlTint ControlTint
    {
        get => this.GetProperty<NSControlTint>(ControlTintProperty!);
        set => this.SetProperty(ControlTintProperty!, value);
    }
    

    /// <summary>
    /// Get or set value of progress indicator.
    /// </summary>
    public double DoubleValue
    {
        get => this.GetProperty<double>(DoubleValueProperty!);
        set => this.SetProperty<double>(DoubleValueProperty!, value);
    }
    

    /// <summary>
    /// Increment the value of progress indicator.
    /// </summary>
    /// <param name="delta">Value to incremenet.</param>
    public void Increment(double delta) =>
        this.SendMessage(IncrementBySelector!, delta);
    

    /// <summary>
    /// Get or set whether the progress indicator’s frame has a three-dimensional bezel.
    /// </summary>
    public bool IsBezeled
    {
        get => this.GetProperty<bool>(IsBezeledProperty!);
        set => this.SetProperty(IsBezeledProperty!, value);
    }
    

    /// <summary>
    /// Get or set whether progress indicator is indeterminate or not.
    /// </summary>
    public bool IsIndeterminate
    {
        get => this.GetProperty<bool>(IsIndeterminateProperty!);
        set => this.SetProperty<bool>(IsIndeterminateProperty!, value);
    }


    /// <summary>
    /// Get or set maximum value of progress indicator.
    /// </summary>
    public double MaxValue
    {
        get => this.GetProperty<double>(MaxValueProperty!);
        set => this.SetProperty<double>(MaxValueProperty!, value);
    }


    /// <summary>
    /// Get or set minimum value of progress indicator.
    /// </summary>
    public double MinValue
    {
        get => this.GetProperty<double>(MinValueProperty!);
        set => this.SetProperty<double>(MinValueProperty!, value);
    }


    /// <summary>
    /// Start animation of indeterminate progress indicator.
    /// </summary>
    /// <param name="sender">Sender.</param>
    public void StartAnimation(NSObject? sender = null) =>
        this.SendMessage(StartAnimationSelector!, sender);
    

    /// <summary>
    /// Stop animation of indeterminate progress indicator.
    /// </summary>
    /// <param name="sender">Sender.</param>
    public void StopAnimation(NSObject? sender = null) =>
        this.SendMessage(StopAnimationSelector!, sender);
    

    /// <summary>
    /// Get or set style of progress indicator.
    /// </summary>
    public NSProgressIndicatorStyle Style
    {
        get => this.GetProperty<NSProgressIndicatorStyle>(StyleProperty!);
        set => this.SetProperty(StyleProperty!, value);
    }
}