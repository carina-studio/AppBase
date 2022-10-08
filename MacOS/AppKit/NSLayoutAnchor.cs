using CarinaStudio.MacOS.ObjectiveC;
using System;

namespace CarinaStudio.MacOS.AppKit;

/// <summary>
/// NSLayoutAnchor.
/// </summary>
public class NSLayoutAnchor<TAnchor> : NSObject where TAnchor : NSLayoutAnchor<TAnchor>
{
    // Static fields.
    static readonly Selector? ConstEqToConstantSelector;
    static readonly Selector? ConstEqToSelector;
    static readonly Selector? ConstGtOrEqToConstantSelector;
    static readonly Selector? ConstGtOrEqToSelector;
    static readonly Selector? ConstLtOrEqToConstantSelector;
    static readonly Selector? ConstLtOrEqToSelector;
    static readonly Selector? ItemSelector;
    static readonly Selector? NameSelector;
    static readonly Class? NSLayoutAnchorClass;


    // Static initializer.
    static NSLayoutAnchor()
    {
        if (Platform.IsNotMacOS)
            return;
        NSLayoutAnchorClass = Class.GetClass("NSLayoutAnchor").AsNonNull();
        ConstEqToConstantSelector = Selector.FromName("constraintEqualToAnchor:constant:");
        ConstEqToSelector = Selector.FromName("constraintEqualToAnchor:");
        ConstGtOrEqToConstantSelector = Selector.FromName("constraintGreaterThanOrEqualToAnchor:constant:");
        ConstGtOrEqToSelector = Selector.FromName("constraintGreaterThanOrEqualToAnchor:");
        ConstLtOrEqToConstantSelector = Selector.FromName("constraintLessThanOrEqualToAnchor:constant:");
        ConstLtOrEqToSelector = Selector.FromName("constraintLessThanOrEqualToAnchor:");
        ItemSelector = Selector.FromName("item");
        NameSelector = Selector.FromName("name");
    }


    /// <summary>
    /// Initialize new <see cref="NSLayoutAnchor{T}"/> instance.
    /// </summary>
    /// <param name="handle">Handle of instance.</param>
    /// <param name="verifyClass">True to verify whether instance is NSLayoutAnchor or not.</param>
    /// <param name="ownsInstance">True to own the instance.</param>
    protected NSLayoutAnchor(IntPtr handle, bool verifyClass, bool ownsInstance) : base(handle, ownsInstance)
    { 
        if (verifyClass)
            this.VerifyClass(NSLayoutAnchorClass!);
    }


    /// <summary>
    /// Initialize new <see cref="NSLayoutAnchor{T}"/> instance.
    /// </summary>
    /// <param name="cls">Class of instance.</param>
    /// <param name="handle">Handle of instance.</param>
    /// <param name="ownsInstance">True to own the instance.</param>
    protected NSLayoutAnchor(Class cls, IntPtr handle, bool ownsInstance) : base(cls, handle, ownsInstance)
    { }


    /// <summary>
    /// Define constraint.
    /// </summary>
    /// <returns>Constraint.</returns>
    public NSLayoutConstraint ConstraintEqualTo(NSLayoutAnchor<TAnchor> another) =>
        this.SendMessage<NSLayoutConstraint>(ConstEqToSelector!, another);
    

    /// <summary>
    /// Define constraint.
    /// </summary>
    /// <returns>Constraint.</returns>
    public NSLayoutConstraint ConstraintEqualToWithConstant(NSLayoutAnchor<TAnchor> another, float offset) =>
        this.SendMessage<NSLayoutConstraint>(ConstEqToConstantSelector!, another, offset);
    

    /// <summary>
    /// Define constraint.
    /// </summary>
    /// <returns>Constraint.</returns>
    public NSLayoutConstraint ConstraintGreaterThanOrEqualTo(NSLayoutAnchor<TAnchor> another) =>
        this.SendMessage<NSLayoutConstraint>(ConstGtOrEqToSelector!, another);
    

    /// <summary>
    /// Define constraint.
    /// </summary>
    /// <returns>Constraint.</returns>
    public NSLayoutConstraint ConstraintGreaterThanOrEqualToWithConstant(NSLayoutAnchor<TAnchor> another, float offset) =>
        this.SendMessage<NSLayoutConstraint>(ConstGtOrEqToConstantSelector!, another, offset);
    

    /// <summary>
    /// Define constraint.
    /// </summary>
    /// <returns>Constraint.</returns>
    public NSLayoutConstraint ConstraintLessThanOrEqualTo(NSLayoutAnchor<TAnchor> another) =>
        this.SendMessage<NSLayoutConstraint>(ConstLtOrEqToSelector!, another);
    

    /// <summary>
    /// Define constraint.
    /// </summary>
    /// <returns>Constraint.</returns>
    public NSLayoutConstraint ConstraintLessThanOrEqualToWithConstant(NSLayoutAnchor<TAnchor> another, float offset) =>
        this.SendMessage<NSLayoutConstraint>(ConstLtOrEqToConstantSelector!, another, offset);


    /// <summary>
    /// Get object which is used to calculate the anchor’s position.
    /// </summary>
    public NSObject? Item { get => this.SendMessage<NSObject>(ItemSelector!); }


    /// <summary>
    /// Get name of anchor.
    /// </summary>
    public string Name
    {
        get
        {
            using var name = this.SendMessage<NSString>(NameSelector!);
            return name?.ToString() ?? "";
        }
    }


    /// <inheritdoc/>
    public override string ToString()
    {
        var name = this.Name;
        if (name != "")
            return $"{{{this.GetType().Name}: {name}}}";
        return $"{{{this.GetType().Name}}}";
    }
}