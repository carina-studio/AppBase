using System;
using CarinaStudio.MacOS.ObjectiveC;

namespace CarinaStudio.MacOS.AppKit;

/// <summary>
/// NSView.
/// </summary>
public class NSView : NSResponder
{
    // Static fields.
    static readonly Selector? AddConstraintSelector;
    static readonly Selector? AddConstraintsSelector;
    static readonly Selector? AddSubViewPositionedSelector;
    static readonly Selector? AddSubViewSelector;
    static readonly Selector? BottomAnchorSelector;
    static readonly Property? BoundsProperty;
    static readonly Property? BoundsRotationProperty;
    static readonly Selector? CenterXAnchorSelector;
    static readonly Selector? CenterYAnchorSelector;
    static readonly Selector? ConstraintsSelector;
    static readonly Selector? FirstBaselineAnchorSelector;
    static readonly Selector? FittingSizeSelector;
    static readonly Property? FrameProperty;
    static readonly Property? FrameRotationProperty;
    static readonly Selector? GetTranslatesAutoresizingMaskIntoConstraintsSelector;
    static readonly Selector? HeightAnchorSelector;
    static readonly Selector? InitWithFrameSelector;
    static readonly Selector? IntrinsicContentSizeSelector;
    static readonly Selector? InvalidateIntrinsicContentSizeSelector;
    static readonly Selector? IsFlippedSelector;
    static readonly Property? IsHiddenOrHasHiddenAncestorProperty;
    static readonly Property? IsHiddenProperty;
    static readonly Selector? LastBaselineAnchorSelector;
    static readonly Selector? LayoutSelector;
    static readonly Selector? LeadingAnchorSelector;
    static readonly Selector? LeftAnchorSelector;
    static readonly Property? NeedsLayoutProperty;
    static readonly Class? NSViewClass;
    static readonly Selector? RemoveConstraintSelector;
    static readonly Selector? RemoveConstraintsSelector;
    static readonly Selector? RemoveFromSuperViewSelector;
    static readonly Selector? RightAnchorSelector;
    static readonly Selector? SafeAreaInsetsSelector;
    static readonly Selector? SafeAreaRectSelector;
    static readonly Selector? SetTranslatesAutoresizingMaskIntoConstraintsSelector;
    static readonly Selector? SubViewsSelector;
    static readonly Selector? SuperViewSelector;
    static readonly Selector? TagSelector;
    static readonly Selector? TopAnchorSelector;
    static readonly Selector? TrailingAnchorSelector;
    static readonly Selector? VisibleRectSelector;
    static readonly Selector? WidthAnchorSelector;
    static readonly Selector? WindowSelector;


    // Static initializer.
    static NSView()
    {
        if (Platform.IsNotMacOS)
            return;
        NSViewClass = Class.GetClass("NSView").AsNonNull();
        AddConstraintSelector = Selector.FromName("addConstraint:");
        AddConstraintsSelector = Selector.FromName("addConstraints:");
        AddSubViewPositionedSelector = Selector.FromName("addSubview:positioned:relativeTo:");
        AddSubViewSelector = Selector.FromName("addSubview:");
        BottomAnchorSelector = Selector.FromName("bottomAnchor");
        BoundsProperty = NSViewClass.GetProperty("bounds");
        BoundsRotationProperty = NSViewClass.GetProperty("boundsRotation");
        CenterXAnchorSelector = Selector.FromName("centerXAnchor");
        CenterYAnchorSelector = Selector.FromName("centerYAnchor");
        ConstraintsSelector = Selector.FromName("constraints");
        FirstBaselineAnchorSelector = Selector.FromName("firstBaselineAnchor");
        FittingSizeSelector = Selector.FromName("fittingSize");
        FrameProperty = NSViewClass.GetProperty("frame");
        FrameRotationProperty = NSViewClass.GetProperty("frameRotation");
        GetTranslatesAutoresizingMaskIntoConstraintsSelector = Selector.FromName("translatesAutoresizingMaskIntoConstraints");
        HeightAnchorSelector = Selector.FromName("heightAnchor");
        InitWithFrameSelector = Selector.FromName("initWithFrame:");
        IntrinsicContentSizeSelector = Selector.FromName("intrinsicContentSize");
        InvalidateIntrinsicContentSizeSelector = Selector.FromName("invalidateIntrinsicContentSize");
        IsFlippedSelector = Selector.FromName("isFlipped");
        IsHiddenOrHasHiddenAncestorProperty = NSViewClass.GetProperty("hiddenOrHasHiddenAncestor");
        IsHiddenProperty = NSViewClass.GetProperty("hidden");
        LastBaselineAnchorSelector = Selector.FromName("lastBaselineAnchor");
        LayoutSelector = Selector.FromName("layout");
        LeadingAnchorSelector = Selector.FromName("leadingAnchor");
        LeftAnchorSelector = Selector.FromName("leftAnchor");
        NeedsLayoutProperty = NSViewClass.GetProperty("needsLayout");
        RemoveConstraintSelector = Selector.FromName("removeConstraint:");
        RemoveConstraintsSelector = Selector.FromName("removeConstraints:");
        RemoveFromSuperViewSelector = Selector.FromName("removeFromSuperview:");
        RightAnchorSelector = Selector.FromName("rightAnchor");
        SafeAreaInsetsSelector = Selector.FromName("safeAreaInsets");
        SafeAreaRectSelector = Selector.FromName("safeAreaRect");
        SetTranslatesAutoresizingMaskIntoConstraintsSelector = Selector.FromName("setTranslatesAutoresizingMaskIntoConstraints:");
        SubViewsSelector = Selector.FromName("subviews");
        SuperViewSelector = Selector.FromName("superview");
        TagSelector = Selector.FromName("tag");
        TopAnchorSelector = Selector.FromName("topAnchor");
        TrailingAnchorSelector = Selector.FromName("trailingAnchor");
        VisibleRectSelector = Selector.FromName("visibleRect");
        WidthAnchorSelector = Selector.FromName("widthAnchor");
        WindowSelector = Selector.FromName("window");
#if DEBUG
        var properties = NSViewClass.GetProperties();
        var methods = NSViewClass.GetMethods();
        Array.Sort(properties, (l, r) => l.Name.CompareTo(r.Name));
        Array.Sort(methods, (l, r) => l.Name.CompareTo(r.Name));
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
    public NSView(NSRect frame) : this(new InstanceHolder(Initialize(Initialize(NSViewClass!.Allocate(), frame), frame)), true)
    { }


    /// <summary>
    /// Initialize new <see cref="NSView"/> instance.
    /// </summary>
    /// <param name="handle">Handle of allocated instance.</param>
    /// <param name="frame">Frame.</param>
    protected NSView(IntPtr handle, NSRect frame) : this(new InstanceHolder(Initialize(handle, frame)), true)
    { }


    /// <summary>
    /// Initialize new <see cref="NSView"/> instance.
    /// </summary>
    /// <param name="instance">Instance.</param>
    /// <param name="ownsInstance">True to own the instance.</param>
    protected NSView(InstanceHolder instance, bool ownsInstance) : base(instance, ownsInstance)
    { }


    /// <summary>
    /// Add constraint on the layout of view.
    /// </summary>
    /// <param name="constraint">Constraint.</param>
    public void AddConstraint(NSLayoutConstraint constraint) =>
        this.SendMessage(AddConstraintSelector!, constraint);
    

    /// <summary>
    /// Add multiple constraints on the layout of view.
    /// </summary>
    /// <param name="constraints">Constraint.</param>
    public void AddConstraints(params NSLayoutConstraint[] constraints)
    {
        using var array = new NSArray<NSLayoutConstraint>(constraints);
        this.SendMessage(AddConstraintsSelector!, array);
    }
    

    /// <summary>
    /// Add multiple constraints on the layout of view.
    /// </summary>
    /// <param name="constraints">Constraint.</param>
    public void AddConstraints(NSArray<NSLayoutConstraint> constraints) =>
        this.SendMessage(AddConstraintsSelector!, constraints);


    /// <summary>
    /// Add given view as sub-view.
    /// </summary>
    /// <param name="view">View.</param>
    public void AddSubView(NSView view) =>
        this.SendMessage(AddSubViewSelector!, view);
    

    /// <summary>
    /// Add given view as sub-view.
    /// </summary>
    /// <param name="view">View.</param>
    /// <param name="place">Relation to other view.</param>
    /// <param name="otherView">Other view which the sub-view relative to.</param>
    public void AddSubView(NSView view, NSWindow.OrderingMode place, NSView? otherView) =>
        this.SendMessage(AddSubViewSelector!, view, place, otherView);
    

    /// <summary>
    /// Get layout anchor representing the bottom edge of the view’s frame.
    /// </summary>
    public NSLayoutYAxisAnchor BottomAnchor
    {
        get => this.bottomAnchor ?? this.SendMessage<NSLayoutYAxisAnchor>(BottomAnchorSelector!).AsNonNull().Also(it =>
                this.bottomAnchor = it);
    }
    

    /// <summary>
    /// Get or set bounds rectangle of view.
    /// </summary>
    public NSRect Bounds
    {
        get => this.GetProperty<NSRect>(BoundsProperty!);
        set => this.SetProperty(BoundsProperty!, value);
    }


    /// <summary>
    /// Get or set rotation of bounds in degrees.
    /// </summary>
    public float BoundsRotation
    {
        get => this.GetProperty<float>(BoundsRotationProperty!);
        set => this.SetProperty(BoundsRotationProperty!, value);
    }


    /// <summary>
    /// Get layout anchor representing the horizontal center of the view’s frame.
    /// </summary>
    public NSLayoutXAxisAnchor CenterXAnchor
    {
        get => this.centerXAnchor ?? this.SendMessage<NSLayoutXAxisAnchor>(CenterXAnchorSelector!).AsNonNull().Also(it =>
                this.centerXAnchor = it);
    }


    /// <summary>
    /// Get layout anchor representing the vertical center of the view’s frame.
    /// </summary>
    public NSLayoutYAxisAnchor CenterYAnchor
    {
        get => this.centerYAnchor ?? this.SendMessage<NSLayoutYAxisAnchor>(CenterYAnchorSelector!).AsNonNull().Also(it =>
                this.centerYAnchor = it);
    }


    /// <summary>
    /// Get constraints held by the view.
    /// </summary>
    public NSArray<NSLayoutConstraint> Constraints { get => this.SendMessage<NSArray<NSLayoutConstraint>>(ConstraintsSelector!); }


    /// <summary>
    /// Get layout anchor representing the baseline for the topmost line of text in the view.
    /// </summary>
    public NSLayoutYAxisAnchor FirstBaselineAnchor
    {
        get => this.firstBaselineAnchor ?? this.SendMessage<NSLayoutYAxisAnchor>(FirstBaselineAnchorSelector!).AsNonNull().Also(it =>
                this.firstBaselineAnchor = it);
    }


    /// <summary>
    /// Get the minimum size of the view that satisfies the constraints it holds.
    /// </summary>
    public NSSize FittingSize { get => this.SendMessage<NSSize>(FittingSizeSelector!); }
    

    /// <summary>
    /// Get or set frame of view in its superview’s coordinate system.
    /// </summary>
    public NSRect Frame
    {
        get => this.GetProperty<NSRect>(FrameProperty!);
        set => this.SetProperty(FrameProperty!, value);
    }


    /// <summary>
    /// Get or set rotation of frame in degrees.
    /// </summary>
    public float FrameRotation
    {
        get => this.GetProperty<float>(FrameRotationProperty!);
        set => this.SetProperty(FrameRotationProperty!, value);
    }


    /// <summary>
    /// Get layout anchor representing the height of the view’s frame.
    /// </summary>
    public NSLayoutDimension HeightAnchor
    {
        get => this.heightAnchor ?? this.SendMessage<NSLayoutDimension>(HeightAnchorSelector!).AsNonNull().Also(it =>
            this.heightAnchor = it);
    }


    /// <summary>
    /// Initialize <see cref="NSView"/> with frame.
    /// </summary>
    /// <param name="view">Handle of allocated <see cref="NSView"/>.</param>
    /// <param name="frame">Frame.</param>
    /// <returns>Handle of initialized <see cref="NSView"/>.</returns>
    protected static IntPtr Initialize(IntPtr view, NSRect frame) =>
        NSObject.SendMessage<IntPtr>(view, InitWithFrameSelector!, frame);
    

    /// <summary>
    /// Get natural size of view.
    /// </summary>
    public NSSize IntrinsicContentSize { get => this.SendMessage<NSSize>(IntrinsicContentSizeSelector!); }


    /// <summary>
    /// Invalidate the view’s intrinsic content size.
    /// </summary>
    public void InvalidateIntrinsicContentSize() =>
        this.SendMessage(InvalidateIntrinsicContentSizeSelector!);
    

    /// <summary>
    /// Check whether the view uses a flipped coordinate system.
    /// </summary>
    public bool IsFlipped { get => this.SendMessage<bool>(IsFlippedSelector!); }
    

    /// <summary>
    /// Check whether view or its ancestor is hidden or not.
    /// </summary>
    public bool IsHiddenOrHasHiddenAncestor { get => this.GetProperty<bool>(IsHiddenOrHasHiddenAncestorProperty!); }
    

    /// <summary>
    /// Get or set whether view is hidden or not.
    /// </summary>
    public bool IsHidden
    {
        get => this.GetProperty<bool>(IsHiddenProperty!);
        set => this.SetProperty(IsHiddenProperty!, value);
    }


    /// <summary>
    /// Get layout anchor representing the baseline for the bottommost line of text in the view.
    /// </summary>
    public NSLayoutYAxisAnchor LastBaselineAnchor
    {
        get => this.lastBaselineAnchor ?? this.SendMessage<NSLayoutYAxisAnchor>(LastBaselineAnchorSelector!).AsNonNull().Also(it =>
                this.lastBaselineAnchor = it);
    }
    

    /// <summary>
    /// Perform layout.
    /// </summary>
    public void Layout() =>
        this.SendMessage(LayoutSelector!);
    

    /// <summary>
    /// Get layout anchor representing the leading edge of the view’s frame.
    /// </summary>
    public NSLayoutXAxisAnchor LeadingAnchor
    {
        get => this.leadingAnchor ?? this.SendMessage<NSLayoutXAxisAnchor>(LeadingAnchorSelector!).AsNonNull().Also(it =>
                this.leadingAnchor = it);
    }
    

    /// <summary>
    /// Get layout anchor representing the left edge of the view’s frame.
    /// </summary>
    public NSLayoutXAxisAnchor LeftAnchor
    {
        get => this.leftAnchor ?? this.SendMessage<NSLayoutXAxisAnchor>(LeftAnchorSelector!).AsNonNull().Also(it =>
                this.leftAnchor = it);
    }


    /// <summary>
    /// Get or set whether the view needs a layout pass before it can be drawn.
    /// </summary>
    public bool NeedsLayout
    {
        get => this.GetProperty<bool>(NeedsLayoutProperty!);
        set => this.SetProperty(NeedsLayoutProperty!, value);
    }


    /// <summary>
    /// Add constraint from view.
    /// </summary>
    /// <param name="constraint">Constraint.</param>
    public void RemoveConstraint(NSLayoutConstraint constraint) =>
        this.SendMessage(RemoveConstraintSelector!, constraint);
    

    /// <summary>
    /// Add multiple constraints from view.
    /// </summary>
    /// <param name="constraints">Constraint.</param>
    public void RemoveConstraints(params NSLayoutConstraint[] constraints)
    {
        using var array = new NSArray<NSLayoutConstraint>(constraints);
        this.SendMessage(RemoveConstraintsSelector!, array);
    }
    

    /// <summary>
    /// Add multiple constraints from view.
    /// </summary>
    /// <param name="constraints">Constraint.</param>
    public void RemoveConstraints(NSArray<NSLayoutConstraint> constraints) =>
        this.SendMessage(RemoveConstraintsSelector!, constraints);
    

    /// <summary>
    /// Remove from its super view.
    /// </summary>
    public void RemoveFromSuperView() =>
        this.SendMessage(RemoveFromSuperViewSelector!);
    

    /// <summary>
    /// Get layout anchor representing the right edge of the view’s frame.
    /// </summary>
    public NSLayoutXAxisAnchor RightAnchor
    {
        get => this.rightAnchor ?? this.SendMessage<NSLayoutXAxisAnchor>(RightAnchorSelector!).AsNonNull().Also(it =>
                this.rightAnchor = it);
    }
    

    /// <summary>
    /// Get distances from the edges of your view that define the safe area.
    /// </summary>
    public NSEdgeInsets SafeAreaInsets { get => this.SendMessage<NSEdgeInsets>(SafeAreaInsetsSelector!); }
    

    /// <summary>
    /// A rectangle in the view’s coordinate system that contains the unobscured portion of the view.
    /// </summary>
    public NSRect SafeAreaRect { get => this.SendMessage<NSRect>(SafeAreaRectSelector!); }


    /// <summary>
    /// Get all child views.
    /// </summary>
    public NSArray<NSView> SubViews { get => this.SendMessage<NSArray<NSView>>(SubViewsSelector!); }


    /// <summary>
    /// Get parent view.
    /// </summary>
    public NSView? SuperView { get => this.SendMessage<NSView>(SuperViewSelector!); }


    /// <summary>
    /// Get tag of view.
    /// </summary>
    public int Tag { get => this.SendMessage<int>(TagSelector!); }


    /// <summary>
    /// Get layout anchor representing the top edge of the view’s frame.
    /// </summary>
    public NSLayoutYAxisAnchor TopAnchor
    {
        get => this.topAnchor ?? this.SendMessage<NSLayoutYAxisAnchor>(TopAnchorSelector!).AsNonNull().Also(it =>
                this.topAnchor = it);
    }


    /// <inheritdoc/>
    public override string ToString() =>
        $"{{{this.Class.Name}}}";
    

    /// <summary>
    /// Get layout anchor representing the trailing edge of the view’s frame.
    /// </summary>
    public NSLayoutXAxisAnchor TrailingAnchor
    {
        get => this.trailingAnchor ?? this.SendMessage<NSLayoutXAxisAnchor>(TrailingAnchorSelector!).AsNonNull().Also(it =>
                this.trailingAnchor = it);
    }
    

    /// <summary>
    /// Get or set whether the view’s autoresizing mask is translated into constraints for the constraint-based layout system.
    /// </summary>
    public bool TranslatesAutoresizingMaskIntoConstraints
    {
        get => this.SendMessage<bool>(GetTranslatesAutoresizingMaskIntoConstraintsSelector!);
        set => this.SendMessage(SetTranslatesAutoresizingMaskIntoConstraintsSelector!, value);
    }


    /// <summary>
    /// Get bounds of view which is not clipped by its super view.
    /// </summary>
    public NSRect VisibleRect { get => this.SendMessage<NSRect>(VisibleRectSelector!); }


    /// <summary>
    /// Get layout anchor representing the width of the view’s frame.
    /// </summary>
    public NSLayoutDimension WidthAnchor
    {
        get => this.widthAnchor ?? this.SendMessage<NSLayoutDimension>(WidthAnchorSelector!).AsNonNull().Also(it =>
            this.widthAnchor = it);
    }


    /// <summary>
    /// Get window which contains the view.
    /// </summary>
    public NSWindow? Window { get => this.SendMessage<NSWindow>(WindowSelector!); }
}