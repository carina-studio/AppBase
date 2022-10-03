using CarinaStudio.MacOS.ObjectiveC;

namespace CarinaStudio.MacOS.AppKit;

/// <summary>
/// NSProgressIndicator.
/// </summary>
public class NSProgressIndicator : NSView
{
    // Static fields.
    static readonly Property? DoubleValueProperty;
    static readonly Selector? IncrementBySelector;
    static readonly Property? IsIndeterminateProperty;
    static readonly Property? MaxValueProperty;
    static readonly Property? MinValueProperty;
    static readonly Class? NSProgressIndicatorClass;
    static readonly Selector? StartAnimationSelector;
    static readonly Selector? StopAnimationSelector;


    // Static initializer.
    static NSProgressIndicator()
    {
        if (Platform.IsNotMacOS)
            return;
        NSProgressIndicatorClass = Class.GetClass("NSProgressIndicator").AsNonNull();
        DoubleValueProperty = NSProgressIndicatorClass.GetProperty("doubleValue");
        IncrementBySelector = Selector.FromName("incrementBy:");
        IsIndeterminateProperty = NSProgressIndicatorClass.GetProperty("indeterminate");
        MaxValueProperty = NSProgressIndicatorClass.GetProperty("maxValue");
        MinValueProperty = NSProgressIndicatorClass.GetProperty("minValue");
        StartAnimationSelector = Selector.FromName("startAnimation:");
        StopAnimationSelector = Selector.FromName("stopAnimation:");
    }


    /// <summary>
    /// Initialize new <see cref="NSProgressIndicator"/> instance.
    /// </summary>
    /// <param name="frame">Frame.</param>
    public NSProgressIndicator(NSRect frame) : base(NSProgressIndicatorClass!.Allocate(), frame)
    { }


    // Constructor.
    NSProgressIndicator(InstanceHolder instance, bool ownsInstance) : base(instance, ownsInstance) =>
        this.VerifyClass(NSProgressIndicatorClass!);
    

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
}