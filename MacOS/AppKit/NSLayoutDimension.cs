using CarinaStudio.MacOS.ObjectiveC;
using System;

namespace CarinaStudio.MacOS.AppKit;

/// <summary>
/// NSLayoutDimension.
/// </summary>
public class NSLayoutDimension : NSLayoutAnchor<NSLayoutDimension>
{
    // Static fields.
    static readonly Selector? ConstEqToAnchorMultiplierConstSelector;
    static readonly Selector? ConstEqToAnchorMultiplierSelector;
    static readonly Selector? ConstEqToConstantSelector;
    static readonly Selector? ConstGtOrEqToAnchorMultiplierConstSelector;
    static readonly Selector? ConstGtOrEqToAnchorMultiplierSelector;
    static readonly Selector? ConstGtOrEqToConstantSelector;
    static readonly Selector? ConstLtOrEqToAnchorMultiplierConstSelector;
    static readonly Selector? ConstLtOrEqToAnchorMultiplierSelector;
    static readonly Selector? ConstLtOrEqToConstantSelector;
    static readonly Class? NSLayoutDimensionClass;


    // Static fields.
    static NSLayoutDimension()
    {
        if (Platform.IsNotMacOS)
            return;
        NSLayoutDimensionClass = Class.GetClass(nameof(NSLayoutDimension)).AsNonNull();
        ConstEqToAnchorMultiplierConstSelector = Selector.FromName("constraintEqualToAnchor:multiplier:constant:");
        ConstEqToAnchorMultiplierSelector = Selector.FromName("constraintEqualToAnchor:multiplier:");
        ConstEqToConstantSelector = Selector.FromName("constraintEqualToConstant:");
        ConstGtOrEqToAnchorMultiplierConstSelector = Selector.FromName("constraintGreaterThanOrEqualToAnchor:multiplier:constant:");
        ConstGtOrEqToAnchorMultiplierSelector = Selector.FromName("constraintGreaterThanOrEqualToAnchor:multiplier:");
        ConstGtOrEqToConstantSelector = Selector.FromName("constraintGreaterThanOrEqualToConstant:");
        ConstLtOrEqToAnchorMultiplierConstSelector = Selector.FromName("constraintLessThanOrEqualToAnchor:multiplier:constant:");
        ConstLtOrEqToAnchorMultiplierSelector = Selector.FromName("constraintLessThanOrEqualToAnchor:multiplier:");
        ConstLtOrEqToConstantSelector = Selector.FromName("constraintLessThanOrEqualToConstant:");
    }


    // Constructor.
    NSLayoutDimension(IntPtr handle, bool ownsInstance) : base(handle, false, ownsInstance) =>
        this.VerifyClass(NSLayoutDimensionClass!);
    NSLayoutDimension(Class cls, IntPtr handle, bool ownsInstance) : base(cls, handle, ownsInstance)
    { }


    /// <summary>
    /// Define constraint.
    /// </summary>
    /// <returns>Constraint.</returns>
    public NSLayoutConstraint ConstraintEqualTo(float value) =>
        this.SendMessage<NSLayoutConstraint>(ConstEqToConstantSelector!, value);

    
    /// <summary>
    /// Define constraint.
    /// </summary>
    /// <returns>Constraint.</returns>
    public NSLayoutConstraint ConstraintEqualToWithMultiplier(NSLayoutDimension anchor, float multiplier) =>
        this.SendMessage<NSLayoutConstraint>(ConstEqToAnchorMultiplierSelector!, anchor, multiplier);
    

    /// <summary>
    /// Define constraint.
    /// </summary>
    /// <returns>Constraint.</returns>
    public NSLayoutConstraint ConstraintEqualToWithMultiplier(NSLayoutDimension anchor, float multiplier, float constant) =>
        this.SendMessage<NSLayoutConstraint>(ConstEqToAnchorMultiplierConstSelector!, anchor, multiplier, constant);
    

    /// <summary>
    /// Define constraint.
    /// </summary>
    /// <returns>Constraint.</returns>
    public NSLayoutConstraint ConstraintGreaterThanOrEqualTo(float value) =>
        this.SendMessage<NSLayoutConstraint>(ConstGtOrEqToConstantSelector!, value);

    
    /// <summary>
    /// Define constraint.
    /// </summary>
    /// <returns>Constraint.</returns>
    public NSLayoutConstraint ConstraintGreaterThanOrEqualToWithMultiplier(NSLayoutDimension anchor, float multiplier) =>
        this.SendMessage<NSLayoutConstraint>(ConstGtOrEqToAnchorMultiplierSelector!, anchor, multiplier);
    

    /// <summary>
    /// Define constraint.
    /// </summary>
    /// <returns>Constraint.</returns>
    public NSLayoutConstraint ConstraintGreaterThanOrEqualToWithMultiplier(NSLayoutDimension anchor, float multiplier, float constant) =>
        this.SendMessage<NSLayoutConstraint>(ConstGtOrEqToAnchorMultiplierConstSelector!, anchor, multiplier, constant);
    

    /// <summary>
    /// Define constraint.
    /// </summary>
    /// <returns>Constraint.</returns>
    public NSLayoutConstraint ConstraintLessThanOrEqualTo(float value) =>
        this.SendMessage<NSLayoutConstraint>(ConstLtOrEqToConstantSelector!, value);

    
    /// <summary>
    /// Define constraint.
    /// </summary>
    /// <returns>Constraint.</returns>
    public NSLayoutConstraint ConstraintLessThanOrEqualToWithMultiplier(NSLayoutDimension anchor, float multiplier) =>
        this.SendMessage<NSLayoutConstraint>(ConstLtOrEqToAnchorMultiplierSelector!, anchor, multiplier);
    

    /// <summary>
    /// Define constraint.
    /// </summary>
    /// <returns>Constraint.</returns>
    public NSLayoutConstraint ConstraintLessThanOrEqualToWithMultiplier(NSLayoutDimension anchor, float multiplier, float constant) =>
        this.SendMessage<NSLayoutConstraint>(ConstLtOrEqToAnchorMultiplierConstSelector!, anchor, multiplier, constant);
}