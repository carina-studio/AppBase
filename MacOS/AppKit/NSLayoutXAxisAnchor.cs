using CarinaStudio.MacOS.ObjectiveC;
using System;
using System.Diagnostics.CodeAnalysis;

namespace CarinaStudio.MacOS.AppKit;

/// <summary>
/// NSLayoutXAxisAnchor.
/// </summary>
public class NSLayoutXAxisAnchor : NSLayoutAnchor<NSLayoutXAxisAnchor>
{
    // Static fields.
    static Selector? AnchorWithOffsetSelector;
    static Selector? ConstEqToSysSpacingAfterMultipierSelector;
    static Selector? ConstGtOrEqToSysSpacingAfterMultipierSelector;
    static Selector? ConstLtOrEqToSysSpacingAfterMultipierSelector;
    static readonly Class? NSLayoutXAxisAnchorClass;


    // Static initializer.
    static NSLayoutXAxisAnchor()
    {
        if (Platform.IsNotMacOS)
            return;
        NSLayoutXAxisAnchorClass = Class.GetClass(nameof(NSLayoutXAxisAnchor)).AsNonNull();
    }


    // Constructor.
    NSLayoutXAxisAnchor(IntPtr handle, bool ownsInstance) : base(handle, false, ownsInstance) =>
        this.VerifyClass(NSLayoutXAxisAnchorClass!);
    NSLayoutXAxisAnchor(Class cls, IntPtr handle, bool ownsInstance) : base(cls, handle, ownsInstance)
    { }


    /// <summary>
    /// Creates a layout dimension object from two anchors.
    /// </summary>
    /// <param name="otherAnchor">Other anchor.</param>
    /// <returns>Layout dimension.</returns>
#if NET7_0_OR_GREATER
    [RequiresDynamicCode(CallMethodRdcMessage)]
#endif
    public NSLayoutDimension AnchorWithOffset(NSLayoutXAxisAnchor otherAnchor)
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
    public NSLayoutConstraint ConstraintEqualToSystemSpacingAfter(NSLayoutXAxisAnchor anchor, float multiplier)
    {
        ConstEqToSysSpacingAfterMultipierSelector ??= Selector.FromName("constraintEqualToAnchorSystemSpacingAfter:multiplier:");
        return this.SendMessage<NSLayoutConstraint>(ConstEqToSysSpacingAfterMultipierSelector, anchor, multiplier);
    }
    

    /// <summary>
    /// Define constraint.
    /// </summary>
    /// <returns>Constraint.</returns>
#if NET7_0_OR_GREATER
    [RequiresDynamicCode(CallMethodRdcMessage)]
#endif
    public NSLayoutConstraint ConstraintGreaterThanOrEqualToSystemSpacingAfter(NSLayoutXAxisAnchor anchor, float multiplier)
    {
        ConstGtOrEqToSysSpacingAfterMultipierSelector ??= Selector.FromName("constraintGreaterThanOrEqualToAnchorSystemSpacingAfter:multiplier:");
        return this.SendMessage<NSLayoutConstraint>(ConstGtOrEqToSysSpacingAfterMultipierSelector, anchor, multiplier);
    }
    

    /// <summary>
    /// Define constraint.
    /// </summary>
    /// <returns>Constraint.</returns>
#if NET7_0_OR_GREATER
    [RequiresDynamicCode(CallMethodRdcMessage)]
#endif
    public NSLayoutConstraint ConstraintLessThanOrEqualToSystemSpacingAfter(NSLayoutXAxisAnchor anchor, float multiplier)
    {
        ConstLtOrEqToSysSpacingAfterMultipierSelector ??= Selector.FromName("constraintLessThanOrEqualToAnchorSystemSpacingAfter:multiplier:");
        return this.SendMessage<NSLayoutConstraint>(ConstLtOrEqToSysSpacingAfterMultipierSelector, anchor, multiplier);
    }
}