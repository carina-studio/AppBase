using CarinaStudio.MacOS.ObjectiveC;
using System;
using System.Diagnostics.CodeAnalysis;

namespace CarinaStudio.MacOS.AppKit;

/// <summary>
/// NSLayoutYAxisAnchor.
/// </summary>
public class NSLayoutYAxisAnchor : NSLayoutAnchor<NSLayoutYAxisAnchor>
{
    // Static fields.
    static Selector? AnchorWithOffsetSelector;
    static Selector? ConstEqToSysSpacingBelowMultipierSelector;
    static Selector? ConstGtOrEqToSysSpacingBelowMultipierSelector;
    static Selector? ConstLtOrEqToSysSpacingBelowMultipierSelector;
    static readonly Class? NSLayoutYAxisAnchorClass;


    // Static initializer.
    static NSLayoutYAxisAnchor()
    {
        if (Platform.IsNotMacOS)
            return;
        NSLayoutYAxisAnchorClass = Class.GetClass(nameof(NSLayoutYAxisAnchor)).AsNonNull();
    }


    // Constructor.
    NSLayoutYAxisAnchor(IntPtr handle, bool ownsInstance) : base(handle, false, ownsInstance) =>
        this.VerifyClass(NSLayoutYAxisAnchorClass!);
    NSLayoutYAxisAnchor(Class cls, IntPtr handle, bool ownsInstance) : base(cls, handle, ownsInstance)
    { }


    /// <summary>
    /// Creates a layout dimension object from two anchors.
    /// </summary>
    /// <param name="otherAnchor">Other anchor.</param>
    /// <returns>Layout dimension.</returns>
#if NET7_0_OR_GREATER
    [RequiresDynamicCode(CallMethodRdcMessage)]
#endif
    public NSLayoutDimension AnchorWithOffset(NSLayoutYAxisAnchor otherAnchor)
    {
        AnchorWithOffsetSelector ??= Selector.FromName("anchorWithOffsetTo:");
        return this.SendMessage<NSLayoutDimension>(AnchorWithOffsetSelector, otherAnchor);
    }


    /// <summary>
    /// Define constraint.
    /// </summary>
    /// <returns>Constraint.</returns>
#if NET7_0_OR_GREATER
    [RequiresDynamicCode(CallMethodRdcMessage)]
#endif
    public NSLayoutConstraint ConstraintEqualToSystemSpacingBelow(NSLayoutXAxisAnchor anchor, float multiplier)
    {
        ConstEqToSysSpacingBelowMultipierSelector ??= Selector.FromName("constraintEqualToAnchorSystemSpacingBelow:multiplier:");
        return this.SendMessage<NSLayoutConstraint>(ConstEqToSysSpacingBelowMultipierSelector, anchor, multiplier);
    }
    

    /// <summary>
    /// Define constraint.
    /// </summary>
    /// <returns>Constraint.</returns>
#if NET7_0_OR_GREATER
    [RequiresDynamicCode(CallMethodRdcMessage)]
#endif
    public NSLayoutConstraint ConstraintGreaterThanOrEqualToSystemSpacingBelow(NSLayoutXAxisAnchor anchor, float multiplier)
    {
        ConstGtOrEqToSysSpacingBelowMultipierSelector ??= Selector.FromName("constraintGreaterThanOrEqualToAnchorSystemSpacingBelow:multiplier:");
        return this.SendMessage<NSLayoutConstraint>(ConstGtOrEqToSysSpacingBelowMultipierSelector, anchor, multiplier);
    }
    

    /// <summary>
    /// Define constraint.
    /// </summary>
    /// <returns>Constraint.</returns>
#if NET7_0_OR_GREATER
    [RequiresDynamicCode(CallMethodRdcMessage)]
#endif
    public NSLayoutConstraint ConstraintLessThanOrEqualToSystemSpacingBelow(NSLayoutXAxisAnchor anchor, float multiplier)
    {
        ConstLtOrEqToSysSpacingBelowMultipierSelector ??= Selector.FromName("constraintLessThanOrEqualToAnchorSystemSpacingBelow:multiplier:");
        return this.SendMessage<NSLayoutConstraint>(ConstLtOrEqToSysSpacingBelowMultipierSelector, anchor, multiplier);
    }
}