using CarinaStudio.MacOS.ObjectiveC;
using System;

namespace CarinaStudio.MacOS.AppKit;

/// <summary>
/// NSLayoutConstraint.
/// </summary>
public class NSLayoutConstraint : NSObject
{
#pragma warning disable CS1591
    /// <summary>
    /// Attribute.
    /// </summary>
    public enum Attribute : int
    {
        NotAnAttribute = 0,
        Left = 1,
        Right = 2,
        Top = 3,
        Bottom = 4,
        Leading = 5,
        Trailing = 6,
        Width = 7,
        Height = 8,
        CenterX = 9,
        CenterY = 10,
        LastBaseline = 11,
        FirstBaseline = 12,
        LeftMargin = 13,
        RightMargin = 14,
        TopMargin = 15,
        BottomMargin = 16,
        LeadingMargin = 17,
        TrailingMargin = 18,
        CenterXWithinMargins = 19,
        CenterYWithinMargins = 20,
    }


    /// <summary>
    /// Relation.
    /// </summary>
    public enum Relation : int
    {
        LessThanOrEqual = -1,
        Equal = 0,
        GreaterThanOrEqual = 1,
    }
#pragma warning restore CS1591


    // Static fields.
    static readonly Property? IsActiveProperty;
    static readonly Class? NSLayoutConstraintClass;


    // Static initializer.
    static NSLayoutConstraint()
    {
        if (Platform.IsNotMacOS)
            return;
        NSLayoutConstraintClass = Class.GetClass(nameof(NSLayoutConstraint)).AsNonNull();
        IsActiveProperty = NSLayoutConstraintClass.GetProperty("active");
    }


    // Constructor.
    NSLayoutConstraint(IntPtr handle, bool ownsInstance) : base(handle, ownsInstance) =>
        this.VerifyClass(NSLayoutConstraintClass!);
    NSLayoutConstraint(Class cls, IntPtr handle, bool ownsInstance) : base(cls, handle, ownsInstance)
    { }


    /// <summary>
    /// Get or set whether constraint is active or not.
    /// </summary>
    public bool IsActive
    {
        get => this.GetProperty<bool>(IsActiveProperty!);
        set => this.SetProperty(IsActiveProperty!, value);
    }
}