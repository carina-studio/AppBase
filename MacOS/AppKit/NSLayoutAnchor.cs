using CarinaStudio.MacOS.ObjectiveC;
using System;

namespace CarinaStudio.MacOS.AppKit;

// NSLayoutAnchor.
static class NSLayoutAnchor
{
    // Static fields.
    public static Selector? ConstEqToConstantSelector;
    public static Selector? ConstEqToSelector;
    public static Selector? ConstGtOrEqToConstantSelector;
    public static Selector? ConstGtOrEqToSelector;
    public static Selector? ConstLtOrEqToConstantSelector;
    public static Selector? ConstLtOrEqToSelector;
    public static Selector? ItemSelector;
    public static Selector? NameSelector;
    public static readonly Class? NSLayoutAnchorClass;


    // Static initializer.
    static NSLayoutAnchor()
    {
        if (Platform.IsNotMacOS)
            return;
        NSLayoutAnchorClass = Class.GetClass("NSLayoutAnchor").AsNonNull(); 
    }
}


/// <summary>
/// NSLayoutAnchor.
/// </summary>
public class NSLayoutAnchor<TAnchor> : NSObject where TAnchor : NSLayoutAnchor<TAnchor>
{
    /// <summary>
    /// Initialize new <see cref="NSLayoutAnchor{T}"/> instance.
    /// </summary>
    /// <param name="handle">Handle of instance.</param>
    /// <param name="verifyClass">True to verify whether instance is NSLayoutAnchor or not.</param>
    /// <param name="ownsInstance">True to own the instance.</param>
    protected NSLayoutAnchor(IntPtr handle, bool verifyClass, bool ownsInstance) : base(handle, ownsInstance)
    { 
        if (verifyClass)
            this.VerifyClass(NSLayoutAnchor.NSLayoutAnchorClass!);
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
    public NSLayoutConstraint ConstraintEqualTo(NSLayoutAnchor<TAnchor> another)
    {
        NSLayoutAnchor.ConstEqToSelector ??= Selector.FromName("constraintEqualToAnchor:");
        return this.SendMessage<NSLayoutConstraint>(NSLayoutAnchor.ConstEqToSelector, another);
    }
    

    /// <summary>
    /// Define constraint.
    /// </summary>
    /// <returns>Constraint.</returns>
    public NSLayoutConstraint ConstraintEqualToWithConstant(NSLayoutAnchor<TAnchor> another, float offset)
    {
        NSLayoutAnchor.ConstEqToConstantSelector ??= Selector.FromName("constraintEqualToAnchor:constant:");
        return this.SendMessage<NSLayoutConstraint>(NSLayoutAnchor.ConstEqToConstantSelector, another, offset);
    }
    

    /// <summary>
    /// Define constraint.
    /// </summary>
    /// <returns>Constraint.</returns>
    public NSLayoutConstraint ConstraintGreaterThanOrEqualTo(NSLayoutAnchor<TAnchor> another)
    {
        NSLayoutAnchor.ConstGtOrEqToSelector ??= Selector.FromName("constraintGreaterThanOrEqualToAnchor:");
        return this.SendMessage<NSLayoutConstraint>(NSLayoutAnchor.ConstGtOrEqToSelector, another);
    }
    

    /// <summary>
    /// Define constraint.
    /// </summary>
    /// <returns>Constraint.</returns>
    public NSLayoutConstraint ConstraintGreaterThanOrEqualToWithConstant(NSLayoutAnchor<TAnchor> another, float offset)
    {
        NSLayoutAnchor.ConstGtOrEqToConstantSelector ??= Selector.FromName("constraintGreaterThanOrEqualToAnchor:constant:");
        return this.SendMessage<NSLayoutConstraint>(NSLayoutAnchor.ConstGtOrEqToConstantSelector, another, offset);
    }
    

    /// <summary>
    /// Define constraint.
    /// </summary>
    /// <returns>Constraint.</returns>
    public NSLayoutConstraint ConstraintLessThanOrEqualTo(NSLayoutAnchor<TAnchor> another)
    {
        NSLayoutAnchor.ConstLtOrEqToSelector ??= Selector.FromName("constraintLessThanOrEqualToAnchor:");
        return this.SendMessage<NSLayoutConstraint>(NSLayoutAnchor.ConstLtOrEqToSelector, another);
    }
    

    /// <summary>
    /// Define constraint.
    /// </summary>
    /// <returns>Constraint.</returns>
    public NSLayoutConstraint ConstraintLessThanOrEqualToWithConstant(NSLayoutAnchor<TAnchor> another, float offset)
    {
        NSLayoutAnchor.ConstLtOrEqToConstantSelector ??= Selector.FromName("constraintLessThanOrEqualToAnchor:constant:");
        return this.SendMessage<NSLayoutConstraint>(NSLayoutAnchor.ConstLtOrEqToConstantSelector, another, offset);
    }


    /// <summary>
    /// Get object which is used to calculate the anchor’s position.
    /// </summary>
    public NSObject? Item 
    { 
        get 
        {
            NSLayoutAnchor.ItemSelector ??= Selector.FromName("item");
            return this.SendMessage<NSObject?>(NSLayoutAnchor.ItemSelector); 
        }
    }


    /// <summary>
    /// Get name of anchor.
    /// </summary>
    public string Name
    {
        get
        {
            NSLayoutAnchor.NameSelector ??= Selector.FromName("name");
            using var name = this.SendMessage<NSString?>(NSLayoutAnchor.NameSelector);
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