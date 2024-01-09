using System;
using CarinaStudio.MacOS.ObjectiveC;
using System.Diagnostics.CodeAnalysis;

namespace CarinaStudio.MacOS.AppKit;

/// <summary>
/// NSView.
/// </summary>
public class NSView : NSResponder
{
#pragma warning disable CS1591
    /// <summary>
    /// BackgroundStyle.
    /// </summary>
    public enum BackgroundStyle : int
    {
        Normal = 0,
        Emphasized = 1,
        Raised = 2,
        Lowered = 3,
    }
#pragma warning restore CS1591


    // Static fields.
    static Selector? AddConstraintSelector;
    static Selector? AddConstraintsSelector;
    static Selector? AddSubViewSelector;
    static Property? AppearanceProperty;
    static Selector? BottomAnchorSelector;
    static Property? BoundsProperty;
    static Property? BoundsRotationProperty;
    static Selector? CenterXAnchorSelector;
    static Selector? CenterYAnchorSelector;
    static Selector? ConstraintsSelector;
    static Selector? FirstBaselineAnchorSelector;
    static Selector? FittingSizeSelector;
    static Property? FrameProperty;
    static Property? FrameRotationProperty;
    static Selector? GetTranslatesAutoresizingMaskIntoConstraintsSelector;
    static Selector? HeightAnchorSelector;
    static Selector? InitWithFrameSelector;
    static Selector? IntrinsicContentSizeSelector;
    static Selector? InvalidateIntrinsicContentSizeSelector;
    static Selector? IsFlippedSelector;
    static Property? IsHiddenOrHasHiddenAncestorProperty;
    static Property? IsHiddenProperty;
    static Selector? LastBaselineAnchorSelector;
    static Selector? LayoutSelector;
    static Selector? LeadingAnchorSelector;
    static Selector? LeftAnchorSelector;
    static Property? NeedsLayoutProperty;
    static readonly Class? NSViewClass;
    static Selector? RemoveConstraintSelector;
    static Selector? RemoveConstraintsSelector;
    static Selector? RemoveFromSuperViewSelector;
    static Selector? RightAnchorSelector;
    static Selector? SafeAreaInsetsSelector;
    static Selector? SafeAreaRectSelector;
    static Selector? SetTranslatesAutoresizingMaskIntoConstraintsSelector;
    static Selector? SubViewsSelector;
    static Selector? SuperViewSelector;
    static Selector? TagSelector;
    static Selector? TopAnchorSelector;
    static Selector? TrailingAnchorSelector;
    static Selector? VisibleRectSelector;
    static Selector? WidthAnchorSelector;
    static Selector? WindowSelector;


    // Static initializer.
    static NSView()
    {
        if (Platform.IsNotMacOS)
            return;
        NSViewClass = Class.GetClass("NSView").AsNonNull();
#if DEBUG
        var properties = NSViewClass.GetProperties();
        var methods = NSViewClass.GetMethods();
        Array.Sort(properties, (l, r) => string.CompareOrdinal(l.Name, r.Name));
        Array.Sort(methods, (l, r) => string.CompareOrdinal(l.Name, r.Name));
#endif
    }


    // Fields.
    NSLayoutYAxisAnchor? bottomAnchor;
    NSLayoutXAxisAnchor? centerXAnchor;
    NSLayoutYAxisAnchor? centerYAnchor;
    NSLayoutDimension? heightAnchor;
    NSLayoutYAxisAnchor? firstBaselineAnchor;
    NSLayoutYAxisAnchor? lastBaselineAnchor;
    NSLayoutXAxisAnchor? leadingAnchor;
    NSLayoutXAxisAnchor? leftAnchor;
    NSLayoutXAxisAnchor? rightAnchor;
    NSLayoutYAxisAnchor? topAnchor;
    NSLayoutXAxisAnchor? trailingAnchor;
    NSLayoutDimension? widthAnchor;


    /// <summary>
    /// Initialize new <see cref="NSView"/> instance.
    /// </summary>
    /// <param name="frame">Frame.</param>
#if NET7_0_OR_GREATER
    [RequiresDynamicCode(CallConstructorRdcMessage)]
#endif
    public NSView(NSRect frame) : this(Initialize(Initialize(NSViewClass!.Allocate(), frame), frame), false, true)
    { }


    /// <summary>
    /// Initialize new <see cref="NSView"/> instance.
    /// </summary>
    /// <param name="handle">Handle of allocated instance.</param>
    /// <param name="frame">Frame.</param>
#if NET7_0_OR_GREATER
    [RequiresDynamicCode(CallConstructorRdcMessage)]
#endif
    protected NSView(IntPtr handle, NSRect frame) : this(Initialize(handle, frame), false, true)
    { }


    /// <summary>
    /// Initialize new <see cref="NSView"/> instance.
    /// </summary>
    /// <param name="handle">Handle of instance.</param>
    /// <param name="verifyClass">True to verify whether instance is NSView or not.</param>
    /// <param name="ownsInstance">True to own the instance.</param>
    protected NSView(IntPtr handle, bool verifyClass, bool ownsInstance) : base(handle, false, ownsInstance)
    { 
        if (verifyClass)
            this.VerifyClass(NSViewClass!);
    }


    /// <summary>
    /// Initialize new <see cref="NSView"/> instance.
    /// </summary>
    /// <param name="cls">Class of instance.</param>
    /// <param name="handle">Handle of instance.</param>
    /// <param name="ownsInstance">True to own the instance.</param>
    protected NSView(Class cls, IntPtr handle, bool ownsInstance) : base(cls, handle, ownsInstance)
    { }


    // Constructor.
#pragma warning disable IDE0051
    NSView(IntPtr handle, bool ownsInstance) : this(handle, true, ownsInstance)
    { }
#pragma warning restore IDE0051


    /// <summary>
    /// Add constraint on the layout of view.
    /// </summary>
    /// <param name="constraint">Constraint.</param>
#if NET7_0_OR_GREATER
    [RequiresDynamicCode(CallMethodRdcMessage)]
#endif
    public void AddConstraint(NSLayoutConstraint constraint)
    {
        AddConstraintSelector ??= Selector.FromName("addConstraint:");
        this.SendMessage(AddConstraintSelector, constraint);
    }
        

    /// <summary>
    /// Add multiple constraints on the layout of view.
    /// </summary>
    /// <param name="constraints">Constraint.</param>
#if NET7_0_OR_GREATER
    [RequiresDynamicCode(CallMethodRdcMessage)]
#endif
    public void AddConstraints(params NSLayoutConstraint[] constraints)
    {
        AddConstraintsSelector ??= Selector.FromName("addConstraints:");
        using var array = new NSArray<NSLayoutConstraint>(constraints);
        this.SendMessage(AddConstraintsSelector, array);
    }
    

    /// <summary>
    /// Add multiple constraints on the layout of view.
    /// </summary>
    /// <param name="constraints">Constraint.</param>
#if NET7_0_OR_GREATER
    [RequiresDynamicCode(CallMethodRdcMessage)]
#endif
    public void AddConstraints(NSArray<NSLayoutConstraint> constraints)
    {
        AddConstraintsSelector ??= Selector.FromName("addConstraints:");
        this.SendMessage(AddConstraintsSelector, constraints);
    }


    /// <summary>
    /// Add given view as sub-view.
    /// </summary>
    /// <param name="view">View.</param>
#if NET7_0_OR_GREATER
    [RequiresDynamicCode(CallMethodRdcMessage)]
#endif
    public void AddSubView(NSView view)
    {
        AddSubViewSelector ??= Selector.FromName("addSubview:");
        this.SendMessage(AddSubViewSelector, view);
    }
    

    /// <summary>
    /// Add given view as sub-view.
    /// </summary>
    /// <param name="view">View.</param>
    /// <param name="place">Relation to other view.</param>
    /// <param name="otherView">Other view which the sub-view relative to.</param>
#if NET7_0_OR_GREATER
    [RequiresDynamicCode(CallMethodRdcMessage)]
#endif
    public void AddSubView(NSView view, NSWindow.OrderingMode place, NSView? otherView)
    {
        AddSubViewSelector ??= Selector.FromName("addSubview:");
        this.SendMessage(AddSubViewSelector, view, place, otherView);
    }
    

    /// <summary>
    /// Get or set appearance of view.
    /// </summary>
    public NSAppearance? Appearance
    {
        get 
        {
            AppearanceProperty ??= NSViewClass!.GetProperty("appearance").AsNonNull();
            return this.GetNSObjectProperty<NSAppearance>(AppearanceProperty!);
        }
        set 
        {
            AppearanceProperty ??= NSViewClass!.GetProperty("appearance").AsNonNull();
            this.SetProperty(AppearanceProperty, (NSObject?)value);
        }
    }
    

    /// <summary>
    /// Get layout anchor representing the bottom edge of the view’s frame.
    /// </summary>
    public NSLayoutYAxisAnchor BottomAnchor
    {
        get 
        {
            BottomAnchorSelector ??= Selector.FromName("bottomAnchor");
#pragma warning disable IL3050
            return this.bottomAnchor ?? this.SendMessage<NSLayoutYAxisAnchor>(BottomAnchorSelector).AsNonNull().Also(it =>
                this.bottomAnchor = it);
#pragma warning restore IL3050
        }
    }
    

    /// <summary>
    /// Get or set bounds rectangle of view.
    /// </summary>
    public NSRect Bounds
    {
#if NET7_0_OR_GREATER
        [RequiresDynamicCode(GetPropertyRdcMessage)]
#endif
        get 
        {
            BoundsProperty ??= NSViewClass!.GetProperty("bounds").AsNonNull();
            return this.GetProperty<NSRect>(BoundsProperty);
        }
#if NET7_0_OR_GREATER
        [RequiresDynamicCode(SetPropertyRdcMessage)]
#endif
        set 
        {
            BoundsProperty ??= NSViewClass!.GetProperty("bounds").AsNonNull();
            this.SetProperty(BoundsProperty, value);
        }
    }


    /// <summary>
    /// Get or set rotation of bounds in degrees.
    /// </summary>
    public double BoundsRotation
    {
        get 
        {
            BoundsRotationProperty ??= NSViewClass!.GetProperty("boundsRotation").AsNonNull();
            return this.GetDoubleProperty(BoundsRotationProperty);
        }
        set 
        {
            BoundsRotationProperty ??= NSViewClass!.GetProperty("boundsRotation").AsNonNull();
            this.SetProperty(BoundsRotationProperty, value);
        }
    }


    /// <summary>
    /// Get layout anchor representing the horizontal center of the view’s frame.
    /// </summary>
    public NSLayoutXAxisAnchor CenterXAnchor
    {
        get 
        {
            CenterXAnchorSelector ??= Selector.FromName("centerXAnchor");
#pragma warning disable IL3050
            return this.centerXAnchor ?? this.SendMessage<NSLayoutXAxisAnchor>(CenterXAnchorSelector).AsNonNull().Also(it =>
                this.centerXAnchor = it);
#pragma warning restore IL3050
        }
    }


    /// <summary>
    /// Get layout anchor representing the vertical center of the view’s frame.
    /// </summary>
    public NSLayoutYAxisAnchor CenterYAnchor
    {
        get 
        {
            CenterYAnchorSelector ??= Selector.FromName("centerYAnchor");
#pragma warning disable IL3050
            return this.centerYAnchor ?? this.SendMessage<NSLayoutYAxisAnchor>(CenterYAnchorSelector).AsNonNull().Also(it =>
                this.centerYAnchor = it);
#pragma warning restore IL3050
        }
    }


    /// <summary>
    /// Get constraints held by the view.
    /// </summary>
    public NSArray<NSLayoutConstraint> Constraints 
    { 
#if NET7_0_OR_GREATER
        [RequiresDynamicCode(GetPropertyRdcMessage)]
#endif
        get 
        {
            ConstraintsSelector ??= Selector.FromName("constraints");
            return this.SendMessage<NSArray<NSLayoutConstraint>>(ConstraintsSelector); 
        }
    }


    /// <summary>
    /// Get layout anchor representing the baseline for the topmost line of text in the view.
    /// </summary>
    public NSLayoutYAxisAnchor FirstBaselineAnchor
    {
        get 
        {
            FirstBaselineAnchorSelector ??= Selector.FromName("firstBaselineAnchor");
#pragma warning disable IL3050
            return this.firstBaselineAnchor ?? this.SendMessage<NSLayoutYAxisAnchor>(FirstBaselineAnchorSelector).AsNonNull().Also(it =>
                this.firstBaselineAnchor = it);
#pragma warning restore IL3050
        }
    }


    /// <summary>
    /// Get the minimum size of the view that satisfies the constraints it holds.
    /// </summary>
    public NSSize FittingSize 
    { 
#if NET7_0_OR_GREATER
        [RequiresDynamicCode(GetPropertyRdcMessage)]
#endif
        get 
        {
            FittingSizeSelector ??= Selector.FromName("fittingSize");
            return this.SendMessage<NSSize>(FittingSizeSelector); 
        }
    }
    

    /// <summary>
    /// Get or set frame of view in its superview’s coordinate system.
    /// </summary>
    public NSRect Frame
    {
#if NET7_0_OR_GREATER
        [RequiresDynamicCode(GetPropertyRdcMessage)]
#endif
        get 
        {
            FrameProperty ??= NSViewClass!.GetProperty("frame").AsNonNull();
            return this.GetProperty<NSRect>(FrameProperty);
        }
#if NET7_0_OR_GREATER
        [RequiresDynamicCode(SetPropertyRdcMessage)]
#endif
        set 
        {
            FrameProperty ??= NSViewClass!.GetProperty("frame").AsNonNull();
            this.SetProperty(FrameProperty, value);
        }
    }


    /// <summary>
    /// Get or set rotation of frame in degrees.
    /// </summary>
    public double FrameRotation
    {
        get 
        {
            FrameRotationProperty ??= NSViewClass!.GetProperty("frameRotation").AsNonNull();
            return this.GetDoubleProperty(FrameRotationProperty);
        }
        set 
        {
            FrameRotationProperty ??= NSViewClass!.GetProperty("frameRotation").AsNonNull();
            this.SetProperty(FrameRotationProperty, value);
        }
    }


    /// <summary>
    /// Get layout anchor representing the height of the view’s frame.
    /// </summary>
    public NSLayoutDimension HeightAnchor
    {
        get 
        {
            HeightAnchorSelector ??= Selector.FromName("heightAnchor");
#pragma warning disable IL3050
            return this.heightAnchor ?? this.SendMessage<NSLayoutDimension>(HeightAnchorSelector).AsNonNull().Also(it =>
                this.heightAnchor = it);
#pragma warning restore IL3050
        }
    }


    /// <summary>
    /// Initialize <see cref="NSView"/> with frame.
    /// </summary>
    /// <param name="view">Handle of allocated <see cref="NSView"/>.</param>
    /// <param name="frame">Frame.</param>
    /// <returns>Handle of initialized <see cref="NSView"/>.</returns>
#if NET7_0_OR_GREATER
    [RequiresDynamicCode(CallMethodRdcMessage)]
#endif
    protected static IntPtr Initialize(IntPtr view, NSRect frame)
    {
        InitWithFrameSelector ??= Selector.FromName("initWithFrame:");
        return SendMessage<IntPtr>(view, InitWithFrameSelector, frame);
    }
    

    /// <summary>
    /// Get natural size of view.
    /// </summary>
    public NSSize IntrinsicContentSize 
    { 
#if NET7_0_OR_GREATER
        [RequiresDynamicCode(GetPropertyRdcMessage)]
#endif
        get 
        {
            IntrinsicContentSizeSelector ??= Selector.FromName("intrinsicContentSize");
            return this.SendMessage<NSSize>(IntrinsicContentSizeSelector); 
        }
    }


    /// <summary>
    /// Invalidate the view’s intrinsic content size.
    /// </summary>
    public void InvalidateIntrinsicContentSize()
    {
        InvalidateIntrinsicContentSizeSelector ??= Selector.FromName("invalidateIntrinsicContentSize");
        this.SendMessage(InvalidateIntrinsicContentSizeSelector);
    }
    

    /// <summary>
    /// Check whether the view uses a flipped coordinate system.
    /// </summary>
    public bool IsFlipped 
    { 
        get 
        {
            IsFlippedSelector ??= Selector.FromName("isFlipped");
#pragma warning disable IL3050
            return this.SendMessage<bool>(IsFlippedSelector);
#pragma warning restore IL3050
        }
    }
    

    /// <summary>
    /// Check whether view or its ancestor is hidden or not.
    /// </summary>
    public bool IsHiddenOrHasHiddenAncestor 
    { 
        get 
        {
            IsHiddenOrHasHiddenAncestorProperty ??= NSViewClass!.GetProperty("hiddenOrHasHiddenAncestor").AsNonNull();
            return this.GetBooleanProperty(IsHiddenOrHasHiddenAncestorProperty); 
        }
    }
    

    /// <summary>
    /// Get or set whether view is hidden or not.
    /// </summary>
    public bool IsHidden
    {
        get 
        {
            IsHiddenProperty ??= NSViewClass!.GetProperty("hidden").AsNonNull();
            return this.GetBooleanProperty(IsHiddenProperty);
        }
        set 
        {
            IsHiddenProperty ??= NSViewClass!.GetProperty("hidden").AsNonNull();
            this.SetProperty(IsHiddenProperty, value);
        }
    }


    /// <summary>
    /// Get layout anchor representing the baseline for the bottommost line of text in the view.
    /// </summary>
    public NSLayoutYAxisAnchor LastBaselineAnchor
    {
        get 
        {
            LastBaselineAnchorSelector ??= Selector.FromName("lastBaselineAnchor");
#pragma warning disable IL3050
            return this.lastBaselineAnchor ?? this.SendMessage<NSLayoutYAxisAnchor>(LastBaselineAnchorSelector).AsNonNull().Also(it =>
                this.lastBaselineAnchor = it);
#pragma warning restore IL3050
        }
    }
    

    /// <summary>
    /// Perform layout.
    /// </summary>
    public void Layout()
    {
        LayoutSelector ??= Selector.FromName("layout");
        this.SendMessage(LayoutSelector);
    }
    

    /// <summary>
    /// Get layout anchor representing the leading edge of the view’s frame.
    /// </summary>
    public NSLayoutXAxisAnchor LeadingAnchor
    {
        get 
        {
            LeadingAnchorSelector ??= Selector.FromName("leadingAnchor");
#pragma warning disable IL3050
            return this.leadingAnchor ?? this.SendMessage<NSLayoutXAxisAnchor>(LeadingAnchorSelector).AsNonNull().Also(it =>
                this.leadingAnchor = it);
#pragma warning restore IL3050
        }
    }
    

    /// <summary>
    /// Get layout anchor representing the left edge of the view’s frame.
    /// </summary>
    public NSLayoutXAxisAnchor LeftAnchor
    {
        get 
        {
            LeftAnchorSelector ??= Selector.FromName("leftAnchor");
#pragma warning disable IL3050
            return this.leftAnchor ?? this.SendMessage<NSLayoutXAxisAnchor>(LeftAnchorSelector).AsNonNull().Also(it =>
                this.leftAnchor = it);
#pragma warning restore IL3050
        }
    }


    /// <summary>
    /// Get or set whether the view needs a layout pass before it can be drawn.
    /// </summary>
    public bool NeedsLayout
    {
        get 
        {
            NeedsLayoutProperty ??= NSViewClass!.GetProperty("needsLayout").AsNonNull();
            return this.GetBooleanProperty(NeedsLayoutProperty);
        }
        set 
        {
            NeedsLayoutProperty ??= NSViewClass!.GetProperty("needsLayout").AsNonNull();
            this.SetProperty(NeedsLayoutProperty, value);
        }
    }


    /// <summary>
    /// Add constraint from view.
    /// </summary>
    /// <param name="constraint">Constraint.</param>
#if NET7_0_OR_GREATER
    [RequiresDynamicCode(CallMethodRdcMessage)]
#endif
    public void RemoveConstraint(NSLayoutConstraint constraint)
    {
        RemoveConstraintSelector ??= Selector.FromName("removeConstraint:");
        this.SendMessage(RemoveConstraintSelector, constraint);
    }
    

    /// <summary>
    /// Add multiple constraints from view.
    /// </summary>
    /// <param name="constraints">Constraint.</param>
#if NET7_0_OR_GREATER
    [RequiresDynamicCode(CallMethodRdcMessage)]
#endif
    public void RemoveConstraints(params NSLayoutConstraint[] constraints)
    {
        RemoveConstraintsSelector ??= Selector.FromName("removeConstraints:");
        using var array = new NSArray<NSLayoutConstraint>(constraints);
        this.SendMessage(RemoveConstraintsSelector, array);
    }
    

    /// <summary>
    /// Add multiple constraints from view.
    /// </summary>
    /// <param name="constraints">Constraint.</param>
#if NET7_0_OR_GREATER
    [RequiresDynamicCode(CallMethodRdcMessage)]
#endif
    public void RemoveConstraints(NSArray<NSLayoutConstraint> constraints)
    {
        RemoveConstraintsSelector ??= Selector.FromName("removeConstraints:");
        this.SendMessage(RemoveConstraintsSelector, constraints);
    }
    

    /// <summary>
    /// Remove from its super view.
    /// </summary>
    public void RemoveFromSuperView()
    {
        RemoveFromSuperViewSelector ??= Selector.FromName("removeFromSuperview");
        this.SendMessage(RemoveFromSuperViewSelector);
    }


    /// <summary>
    /// Get layout anchor representing the right edge of the view’s frame.
    /// </summary>
    public NSLayoutXAxisAnchor RightAnchor
    {
        get 
        {
            RightAnchorSelector ??= Selector.FromName("rightAnchor");
#pragma warning disable IL3050
            return this.rightAnchor ?? this.SendMessage<NSLayoutXAxisAnchor>(RightAnchorSelector).AsNonNull().Also(it =>
                this.rightAnchor = it);
#pragma warning restore IL3050
        }
    }
    

    /// <summary>
    /// Get distances from the edges of your view that define the safe area.
    /// </summary>
    public NSEdgeInsets SafeAreaInsets 
    { 
#if NET7_0_OR_GREATER
        [RequiresDynamicCode(GetPropertyRdcMessage)]
#endif
        get 
        {
            SafeAreaInsetsSelector ??= Selector.FromName("safeAreaInsets");
            return this.SendMessage<NSEdgeInsets>(SafeAreaInsetsSelector); 
        }
    }
    

    /// <summary>
    /// A rectangle in the view’s coordinate system that contains the unobscured portion of the view.
    /// </summary>
    public NSRect SafeAreaRect 
    { 
#if NET7_0_OR_GREATER
        [RequiresDynamicCode(GetPropertyRdcMessage)]
#endif
        get
        {
            SafeAreaRectSelector ??= Selector.FromName("safeAreaRect");
            return this.SendMessage<NSRect>(SafeAreaRectSelector); 
        }
    }


    /// <summary>
    /// Get all child views.
    /// </summary>
    public NSArray<NSView> SubViews 
    { 
#if NET7_0_OR_GREATER
        [RequiresDynamicCode(GetPropertyRdcMessage)]
#endif
        get 
        {
            SubViewsSelector ??= Selector.FromName("subviews");
            return this.SendMessage<NSArray<NSView>>(SubViewsSelector); 
        }
    }


    /// <summary>
    /// Get parent view.
    /// </summary>
    public NSView? SuperView 
    { 
        get  
        {
            SuperViewSelector ??= Selector.FromName("superview");
#pragma warning disable IL3050
            return this.SendMessage<NSView>(SuperViewSelector);
#pragma warning restore IL3050
        }
    }


    /// <summary>
    /// Get tag of view.
    /// </summary>
    public int Tag 
    {
        get 
        {
            TagSelector ??= Selector.FromName("tag");
#pragma warning disable IL3050
            return this.SendMessage<int>(TagSelector);
#pragma warning restore IL3050
        }
    }


    /// <summary>
    /// Get layout anchor representing the top edge of the view’s frame.
    /// </summary>
    public NSLayoutYAxisAnchor TopAnchor
    {
        get 
        {
            TopAnchorSelector ??= Selector.FromName("topAnchor");
#pragma warning disable IL3050
            return this.topAnchor ?? this.SendMessage<NSLayoutYAxisAnchor>(TopAnchorSelector).AsNonNull().Also(it =>
                this.topAnchor = it);
#pragma warning restore IL3050
        }
    }


    /// <inheritdoc/>
    public override string ToString() =>
        $"{{{this.Class.Name}}}";
    

    /// <summary>
    /// Get layout anchor representing the trailing edge of the view’s frame.
    /// </summary>
    public NSLayoutXAxisAnchor TrailingAnchor
    {
        get 
        {
            TrailingAnchorSelector ??= Selector.FromName("trailingAnchor");
#pragma warning disable IL3050
            return this.trailingAnchor ?? this.SendMessage<NSLayoutXAxisAnchor>(TrailingAnchorSelector).AsNonNull().Also(it =>
                this.trailingAnchor = it);
#pragma warning restore IL3050
        }
    }
    

    /// <summary>
    /// Get or set whether the view’s autoresizing mask is translated into constraints for the constraint-based layout system.
    /// </summary>
    public bool TranslatesAutoresizingMaskIntoConstraints
    {
        get 
        {
            GetTranslatesAutoresizingMaskIntoConstraintsSelector ??= Selector.FromName("translatesAutoresizingMaskIntoConstraints");
#pragma warning disable IL3050
            return this.SendMessage<bool>(GetTranslatesAutoresizingMaskIntoConstraintsSelector);
#pragma warning restore IL3050
        }
        set
        {
            SetTranslatesAutoresizingMaskIntoConstraintsSelector ??= Selector.FromName("setTranslatesAutoresizingMaskIntoConstraints:");
#pragma warning disable IL3050
            this.SendMessage(SetTranslatesAutoresizingMaskIntoConstraintsSelector, value);
#pragma warning restore IL3050
        }
    }


    /// <summary>
    /// Get bounds of view which is not clipped by its super view.
    /// </summary>
    public NSRect VisibleRect 
    {
#if NET7_0_OR_GREATER
        [RequiresDynamicCode(GetPropertyRdcMessage)]
#endif
        get 
        {
            VisibleRectSelector ??= Selector.FromName("visibleRect");
            return this.SendMessage<NSRect>(VisibleRectSelector); 
        }
    }


    /// <summary>
    /// Get layout anchor representing the width of the view’s frame.
    /// </summary>
    public NSLayoutDimension WidthAnchor
    {
#if NET7_0_OR_GREATER
        [RequiresDynamicCode(GetPropertyRdcMessage)]
#endif
        get
        {
            WidthAnchorSelector ??= Selector.FromName("widthAnchor");
#pragma warning disable IL3050
            return this.widthAnchor ?? this.SendMessage<NSLayoutDimension>(WidthAnchorSelector).AsNonNull().Also(it =>
                this.widthAnchor = it);
#pragma warning restore IL3050
        }
    }


    /// <summary>
    /// Get window which contains the view.
    /// </summary>
    public NSWindow? Window 
    { 
        get 
        {
            WindowSelector ??= Selector.FromName("window");
#pragma warning disable IL3050
            return this.SendMessage<NSWindow>(WindowSelector);
#pragma warning restore IL3050
        }
    }
}