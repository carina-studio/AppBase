using CarinaStudio.MacOS.ObjectiveC;
using System;

namespace CarinaStudio.MacOS.AppKit;

/// <summary>
/// NSLayoutDimension.
/// </summary>
public class NSLayoutDimension : NSLayoutAnchor<NSLayoutDimension>
{
    // Static fields.
    static Selector? ConstEqToAnchorMultiplierConstSelector;
    static Selector? ConstEqToAnchorMultiplierSelector;
    static Selector? ConstEqToConstantSelector;
    static Selector? ConstGtOrEqToAnchorMultiplierConstSelector;
    static Selector? ConstGtOrEqToAnchorMultiplierSelector;
    static Selector? ConstGtOrEqToConstantSelector;
    static Selector? ConstLtOrEqToAnchorMultiplierConstSelector;
    static Selector? ConstLtOrEqToAnchorMultiplierSelector;
    static Selector? ConstLtOrEqToConstantSelector;
    static readonly Class? NSLayoutDimensionClass;


    // Static fields.
    static NSLayoutDimension()
    {
        if (Platform.IsNotMacOS)
            return;
        NSLayoutDimensionClass = Class.GetClass(nameof(NSLayoutDimension)).AsNonNull();
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
    public NSLayoutConstraint ConstraintEqualTo(float value)
    {
        ConstEqToConstantSelector ??= Selector.FromName("constraintEqualToConstant:");
        return this.SendMessage<NSLayoutConstraint>(ConstEqToConstantSelector, value);
    }

    
    /// <summary>
    /// Define constraint.
    /// </summary>
    /// <returns>Constraint.</returns>
    public NSLayoutConstraint ConstraintEqualToWithMultiplier(NSLayoutDimension anchor, float multiplier)
    {
        ConstEqToAnchorMultiplierSelector ??= Selector.FromName("constraintEqualToAnchor:multiplier:");
        return this.SendMessage<NSLayoutConstraint>(ConstEqToAnchorMultiplierSelector, anchor, multiplier);
    }
    

    /// <summary>
    /// Define constraint.
    /// </summary>
    /// <returns>Constraint.</returns>
    public NSLayoutConstraint ConstraintEqualToWithMultiplier(NSLayoutDimension anchor, float multiplier, float constant)
    {
        ConstEqToAnchorMultiplierConstSelector ??= Selector.FromName("constraintEqualToAnchor:multiplier:constant:");
        return this.SendMessage<NSLayoutConstraint>(ConstEqToAnchorMultiplierConstSelector, anchor, multiplier, constant);
    }
    

    /// <summary>
    /// Define constraint.
    /// </summary>
    /// <returns>Constraint.</returns>
    public NSLayoutConstraint ConstraintGreaterThanOrEqualTo(float value)
    {
        ConstGtOrEqToConstantSelector ??= Selector.FromName("constraintGreaterThanOrEqualToConstant:");
        return this.SendMessage<NSLayoutConstraint>(ConstGtOrEqToConstantSelector, value);
    }

    
    /// <summary>
    /// Define constraint.
    /// </summary>
    /// <returns>Constraint.</returns>
    public NSLayoutConstraint ConstraintGreaterThanOrEqualToWithMultiplier(NSLayoutDimension anchor, float multiplier)
    {
        ConstGtOrEqToAnchorMultiplierSelector ??= Selector.FromName("constraintGreaterThanOrEqualToAnchor:multiplier:");
        return this.SendMessage<NSLayoutConstraint>(ConstGtOrEqToAnchorMultiplierSelector, anchor, multiplier);
    }
    

    /// <summary>
    /// Define constraint.
    /// </summary>
    /// <returns>Constraint.</returns>
    public NSLayoutConstraint ConstraintGreaterThanOrEqualToWithMultiplier(NSLayoutDimension anchor, float multiplier, float constant)
    {
        ConstGtOrEqToAnchorMultiplierConstSelector ??= Selector.FromName("constraintGreaterThanOrEqualToAnchor:multiplier:constant:");
        return this.SendMessage<NSLayoutConstraint>(ConstGtOrEqToAnchorMultiplierConstSelector, anchor, multiplier, constant);
    }
    

    /// <summary>
    /// Define constraint.
    /// </summary>
    /// <returns>Constraint.</returns>
    public NSLayoutConstraint ConstraintLessThanOrEqualTo(float value)
    {
        ConstLtOrEqToConstantSelector ??= Selector.FromName("constraintLessThanOrEqualToConstant:");
        return this.SendMessage<NSLayoutConstraint>(ConstLtOrEqToConstantSelector, value);
    }

    
    /// <summary>
    /// Define constraint.
    /// </summary>
    /// <returns>Constraint.</returns>
    public NSLayoutConstraint ConstraintLessThanOrEqualToWithMultiplier(NSLayoutDimension anchor, float multiplier)
    {
        ConstLtOrEqToAnchorMultiplierSelector ??= Selector.FromName("constraintLessThanOrEqualToAnchor:multiplier:");
        return this.SendMessage<NSLayoutConstraint>(ConstLtOrEqToAnchorMultiplierSelector, anchor, multiplier);
    }
    

    /// <summary>
    /// Define constraint.
    /// </summary>
    /// <returns>Constraint.</returns>
    public NSLayoutConstraint ConstraintLessThanOrEqualToWithMultiplier(NSLayoutDimension anchor, float multiplier, float constant)
    {
        ConstLtOrEqToAnchorMultiplierConstSelector ??= Selector.FromName("constraintLessThanOrEqualToAnchor:multiplier:constant:");
        return this.SendMessage<NSLayoutConstraint>(ConstLtOrEqToAnchorMultiplierConstSelector, anchor, multiplier, constant);
    }
}