using CarinaStudio.MacOS.ObjectiveC;
using System;

namespace CarinaStudio.MacOS.AppKit;

/// <summary>
/// NSLayoutXAxisAnchor.
/// </summary>
public class NSLayoutXAxisAnchor : NSLayoutAnchor<NSLayoutXAxisAnchor>
{
    // Static fields.
    static readonly Selector? AnchorWithOffsetSelector;
    static readonly Selector? ConstEqToSysSpacingAfterMultipierSelector;
    static readonly Selector? ConstGtOrEqToSysSpacingAfterMultipierSelector;
    static readonly Selector? ConstLtOrEqToSysSpacingAfterMultipierSelector;
    static readonly Class? NSLayoutXAxisAnchorClass;


    // Static initializer.
    static NSLayoutXAxisAnchor()
    {
        if (Platform.IsNotMacOS)
            return;
        NSLayoutXAxisAnchorClass = Class.GetClass(nameof(NSLayoutXAxisAnchor)).AsNonNull();
        AnchorWithOffsetSelector = Selector.FromName("anchorWithOffsetTo:");
        ConstEqToSysSpacingAfterMultipierSelector = Selector.FromName("constraintEqualToAnchorSystemSpacingAfter:multiplier:");
        ConstGtOrEqToSysSpacingAfterMultipierSelector = Selector.FromName("constraintGreaterThanOrEqualToAnchorSystemSpacingAfter:multiplier:");
        ConstLtOrEqToSysSpacingAfterMultipierSelector = Selector.FromName("constraintLessThanOrEqualToAnchorSystemSpacingAfter:multiplier:");
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
    public NSLayoutDimension AnchorWithOffset(NSLayoutXAxisAnchor otherAnchor) =>
        this.SendMessage<NSLayoutDimension>(AnchorWithOffsetSelector!, otherAnchor);


    /// <summary>
    /// Define constraint.
    /// </summary>
    /// <returns>Constraint.</returns>
    public NSLayoutConstraint ConstraintEqualToSystemSpacingAfter(NSLayoutXAxisAnchor anchor, float multiplier) =>
        this.SendMessage<NSLayoutConstraint>(ConstEqToSysSpacingAfterMultipierSelector!, anchor, multiplier);
    

    /// <summary>
    /// Define constraint.
    /// </summary>
    /// <returns>Constraint.</returns>
    public NSLayoutConstraint ConstraintGreaterThanOrEqualToSystemSpacingAfter(NSLayoutXAxisAnchor anchor, float multiplier) =>
        this.SendMessage<NSLayoutConstraint>(ConstGtOrEqToSysSpacingAfterMultipierSelector!, anchor, multiplier);
    

    /// <summary>
    /// Define constraint.
    /// </summary>
    /// <returns>Constraint.</returns>
    public NSLayoutConstraint ConstraintLessThanOrEqualToSystemSpacingAfter(NSLayoutXAxisAnchor anchor, float multiplier) =>
        this.SendMessage<NSLayoutConstraint>(ConstLtOrEqToSysSpacingAfterMultipierSelector!, anchor, multiplier);
}