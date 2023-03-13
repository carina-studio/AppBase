using System;
using CarinaStudio.MacOS.ObjectiveC;

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
    public NSView(NSRect frame) : this(Initialize(Initialize(NSViewClass!.Allocate(), frame), frame), false, true)
    { }


    /// <summary>
    /// Initialize new <see cref="NSView"/> instance.
    /// </summary>
    /// <param name="handle">Handle of allocated instance.</param>
    /// <param name="frame">Frame.</param>
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
    public void AddConstraint(NSLayoutConstraint constraint)
    {
        AddConstraintSelector ??= Selector.FromName("addConstraint:");
        this.SendMessage(AddConstraintSelector, constraint);
    }
        

    /// <summary>
    /// Add multiple constraints on the layout of view.
    /// </summary>
    /// <param name="constraints">Constraint.</param>
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
    public void AddConstraints(NSArray<NSLayoutConstraint> constraints)
    {
        AddConstraintsSelector ??= Selector.FromName("addConstraints:");
        this.SendMessage(AddConstraintsSelector, constraints);
    }


    /// <summary>
    /// Add given view as sub-view.
    /// </summary>
    /// <param name="view">View.</param>
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
            return this.GetProperty<NSAppearance>(AppearanceProperty!);
        }
        set 
        {
            AppearanceProperty ??= NSViewClass!.GetProperty("appearance").AsNonNull();
            this.SetProperty(AppearanceProperty, value);
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
            return this.bottomAnchor ?? this.SendMessage<NSLayoutYAxisAnchor>(BottomAnchorSelector).AsNonNull().Also(it =>
                this.bottomAnchor = it);
        }
    }
    

    /// <summary>
    /// Get or set bounds rectangle of view.
    /// </summary>
    public NSRect Bounds
    {
        get 
        {
            BoundsProperty ??= NSViewClass!.GetProperty("bounds").AsNonNull();
            return this.GetProperty<NSRect>(BoundsProperty);
        }
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
            return this.GetProperty<double>(BoundsRotationProperty);
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
            return this.centerXAnchor ?? this.SendMessage<NSLayoutXAxisAnchor>(CenterXAnchorSelector).AsNonNull().Also(it =>
                this.centerXAnchor = it);
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
            return this.centerYAnchor ?? this.SendMessage<NSLayoutYAxisAnchor>(CenterYAnchorSelector).AsNonNull().Also(it =>
                this.centerYAnchor = it);
        }
    }


    /// <summary>
    /// Get constraints held by the view.
    /// </summary>
    public NSArray<NSLayoutConstraint> Constraints 
    { 
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
            return this.firstBaselineAnchor ?? this.SendMessage<NSLayoutYAxisAnchor>(FirstBaselineAnchorSelector).AsNonNull().Also(it =>
                this.firstBaselineAnchor = it);
        }
    }


    /// <summary>
    /// Get the minimum size of the view that satisfies the constraints it holds.
    /// </summary>
    public NSSize FittingSize 
    { 
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
        get 
        {
            FrameProperty ??= NSViewClass!.GetProperty("frame").AsNonNull();
            return this.GetProperty<NSRect>(FrameProperty);
        }
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
            return this.GetProperty<double>(FrameRotationProperty);
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
            return this.heightAnchor ?? this.SendMessage<NSLayoutDimension>(HeightAnchorSelector).AsNonNull().Also(it =>
                this.heightAnchor = it);
        }
    }


    /// <summary>
    /// Initialize <see cref="NSView"/> with frame.
    /// </summary>
    /// <param name="view">Handle of allocated <see cref="NSView"/>.</param>
    /// <param name="frame">Frame.</param>
    /// <returns>Handle of initialized <see cref="NSView"/>.</returns>
    protected static IntPtr Initialize(IntPtr view, NSRect frame)
    {
        InitWithFrameSelector ??= Selector.FromName("initWithFrame:");
        return NSObject.SendMessage<IntPtr>(view, InitWithFrameSelector, frame);
    }
    

    /// <summary>
    /// Get natural size of view.
    /// </summary>
    public NSSize IntrinsicContentSize 
    { 
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
            return this.SendMessage<bool>(IsFlippedSelector); 
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
            return this.GetProperty<bool>(IsHiddenOrHasHiddenAncestorProperty); 
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
            return this.GetProperty<bool>(IsHiddenProperty);
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
            return this.lastBaselineAnchor ?? this.SendMessage<NSLayoutYAxisAnchor>(LastBaselineAnchorSelector).AsNonNull().Also(it =>
                this.lastBaselineAnchor = it);
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
            return this.leadingAnchor ?? this.SendMessage<NSLayoutXAxisAnchor>(LeadingAnchorSelector).AsNonNull().Also(it =>
                this.leadingAnchor = it);
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
            return this.leftAnchor ?? this.SendMessage<NSLayoutXAxisAnchor>(LeftAnchorSelector).AsNonNull().Also(it =>
                this.leftAnchor = it);
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
            return this.GetProperty<bool>(NeedsLayoutProperty);
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
    public void RemoveConstraint(NSLayoutConstraint constraint)
    {
        RemoveConstraintSelector ??= Selector.FromName("removeConstraint:");
        this.SendMessage(RemoveConstraintSelector, constraint);
    }
    

    /// <summary>
    /// Add multiple constraints from view.
    /// </summary>
    /// <param name="constraints">Constraint.</param>
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
            return this.rightAnchor ?? this.SendMessage<NSLayoutXAxisAnchor>(RightAnchorSelector).AsNonNull().Also(it =>
                this.rightAnchor = it);
        }
    }
    

    /// <summary>
    /// Get distances from the edges of your view that define the safe area.
    /// </summary>
    public NSEdgeInsets SafeAreaInsets 
    { 
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
            return this.SendMessage<NSView>(SuperViewSelector); 
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
            return this.SendMessage<int>(TagSelector); 
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
            return this.topAnchor ?? this.SendMessage<NSLayoutYAxisAnchor>(TopAnchorSelector).AsNonNull().Also(it =>
                this.topAnchor = it);
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
            return this.trailingAnchor ?? this.SendMessage<NSLayoutXAxisAnchor>(TrailingAnchorSelector).AsNonNull().Also(it =>
                this.trailingAnchor = it);
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
            return this.SendMessage<bool>(GetTranslatesAutoresizingMaskIntoConstraintsSelector);
        }
        set
        {
            SetTranslatesAutoresizingMaskIntoConstraintsSelector ??= Selector.FromName("setTranslatesAutoresizingMaskIntoConstraints:");
            this.SendMessage(SetTranslatesAutoresizingMaskIntoConstraintsSelector, value);
        }
    }


    /// <summary>
    /// Get bounds of view which is not clipped by its super view.
    /// </summary>
    public NSRect VisibleRect 
    {
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
        get
        {
            WidthAnchorSelector ??= Selector.FromName("widthAnchor");
            return this.widthAnchor ?? this.SendMessage<NSLayoutDimension>(WidthAnchorSelector).AsNonNull().Also(it =>
                this.widthAnchor = it);
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
            return this.SendMessage<NSWindow>(WindowSelector); 
        }
    }
}