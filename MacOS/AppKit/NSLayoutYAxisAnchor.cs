using CarinaStudio.MacOS.ObjectiveC;

namespace CarinaStudio.MacOS.AppKit;

/// <summary>
/// NSLayoutYAxisAnchor.
/// </summary>
public class NSLayoutYAxisAnchor : NSLayoutAnchor<NSLayoutYAxisAnchor>
{
    // Static fields.
    static readonly Selector? AnchorWithOffsetSelector;
    static readonly Selector? ConstEqToSysSpacingBelowMultipierSelector;
    static readonly Selector? ConstGtOrEqToSysSpacingBelowMultipierSelector;
    static readonly Selector? ConstLtOrEqToSysSpacingBelowMultipierSelector;
    static readonly Class? NSLayoutYAxisAnchorClass;


    // Static initializer.
    static NSLayoutYAxisAnchor()
    {
        if (Platform.IsNotMacOS)
            return;
        NSLayoutYAxisAnchorClass = Class.GetClass(nameof(NSLayoutYAxisAnchor)).AsNonNull();
        AnchorWithOffsetSelector = Selector.FromName("anchorWithOffsetTo:");
        ConstEqToSysSpacingBelowMultipierSelector = Selector.FromName("constraintEqualToAnchorSystemSpacingBelow:multiplier:");
        ConstGtOrEqToSysSpacingBelowMultipierSelector = Selector.FromName("constraintGreaterThanOrEqualToAnchorSystemSpacingBelow:multiplier:");
        ConstLtOrEqToSysSpacingBelowMultipierSelector = Selector.FromName("constraintLessThanOrEqualToAnchorSystemSpacingBelow:multiplier:");
    }


    // Constructor.
    NSLayoutYAxisAnchor(InstanceHolder instance, bool ownsInstance) : base(instance, ownsInstance)
    { }


    /// <summary>
    /// Creates a layout dimension object from two anchors.
    /// </summary>
    /// <param name="otherAnchor">Other anchor.</param>
    /// <returns>Layout dimension.</returns>
    public NSLayoutDimension AnchorWithOffset(NSLayoutYAxisAnchor otherAnchor) =>
        this.SendMessage<NSLayoutDimension>(AnchorWithOffsetSelector!, otherAnchor);


    /// <summary>
    /// Define constraint.
    /// </summary>
    /// <returns>Constraint.</returns>
    public NSLayoutConstraint ConstraintEqualToSystemSpacingBelow(NSLayoutXAxisAnchor anchor, float multiplier) =>
        this.SendMessage<NSLayoutConstraint>(ConstEqToSysSpacingBelowMultipierSelector!, anchor, multiplier);
    

    /// <summary>
    /// Define constraint.
    /// </summary>
    /// <returns>Constraint.</returns>
    public NSLayoutConstraint ConstraintGreaterThanOrEqualToSystemSpacingBelow(NSLayoutXAxisAnchor anchor, float multiplier) =>
        this.SendMessage<NSLayoutConstraint>(ConstGtOrEqToSysSpacingBelowMultipierSelector!, anchor, multiplier);
    

    /// <summary>
    /// Define constraint.
    /// </summary>
    /// <returns>Constraint.</returns>
    public NSLayoutConstraint ConstraintLessThanOrEqualToSystemSpacingBelow(NSLayoutXAxisAnchor anchor, float multiplier) =>
        this.SendMessage<NSLayoutConstraint>(ConstLtOrEqToSysSpacingBelowMultipierSelector!, anchor, multiplier);
}