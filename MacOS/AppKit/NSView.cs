using System;
using CarinaStudio.MacOS.ObjectiveC;

namespace CarinaStudio.MacOS.AppKit;

/// <summary>
/// NSView.
/// </summary>
public class NSView : NSResponder
{
    // Static fields.
    static readonly Selector? AddSubViewPositionedSelector;
    static readonly Selector? AddSubViewSelector;
    static readonly Selector? InitWithFrameSelector;
    static readonly Property? IsHiddenOrHasHiddenAncestorProperty;
    static readonly Property? IsHiddenProperty;
    static readonly Selector? LayoutSelector;
    static readonly Class? NSViewClass;
    static readonly Selector? RemoveFromSuperViewSelector;
    static readonly Property? SafeAreaRectProperty;
    static readonly Property? SubViewsProperty;
    static readonly Property? SuperViewProperty;
    static readonly Property? VisibleRectProperty;
    static readonly Property? WindowProperty;


    // Static initializer.
    static NSView()
    {
        if (Platform.IsNotMacOS)
            return;
        NSViewClass = Class.GetClass("NSView").AsNonNull();
        AddSubViewPositionedSelector = Selector.FromName("addSubview:positioned:relativeTo:");
        AddSubViewSelector = Selector.FromName("addSubview:");
        InitWithFrameSelector = Selector.FromName("initWithFrame:");
        IsHiddenOrHasHiddenAncestorProperty = NSViewClass.GetProperty("hiddenOrHasHiddenAncestor");
        IsHiddenProperty = NSViewClass.GetProperty("hidden");
        LayoutSelector = Selector.FromName("layout");
        RemoveFromSuperViewSelector = Selector.FromName("removeFromSuperview:");
        SafeAreaRectProperty = NSViewClass.GetProperty("safeAreaRect");
        SubViewsProperty = NSViewClass.GetProperty("subviews");
        SuperViewProperty = NSViewClass.GetProperty("superview");
        VisibleRectProperty = NSViewClass.GetProperty("visibleRect");
        WindowProperty = NSViewClass.GetProperty("window");
    }


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
    /// Initialize <see cref="NSView"/> with frame.
    /// </summary>
    /// <param name="view">Handle of allocated <see cref="NSView"/>.</param>
    /// <param name="frame">Frame.</param>
    /// <returns>Handle of initialized <see cref="NSView"/>.</returns>
    protected static IntPtr Initialize(IntPtr view, NSRect frame) =>
        NSObject.SendMessage<IntPtr>(view, InitWithFrameSelector!, frame);
    

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
    /// Perform layout.
    /// </summary>
    public void Layout() =>
        this.SendMessage(LayoutSelector!);
    

    /// <summary>
    /// Remove from its super view.
    /// </summary>
    public void RemoveFromSuperView() =>
        this.SendMessage(RemoveFromSuperViewSelector!);
    

    /// <summary>
    /// A rectangle in the view’s coordinate system that contains the unobscured portion of the view.
    /// </summary>
    public NSRect SafeAreaRect { get => this.GetProperty<NSRect>(SafeAreaRectProperty!); }


    /// <summary>
    /// Get all child views.
    /// </summary>
    public NSArray<NSView> SubViews { get => this.GetProperty<NSArray<NSView>>(SubViewsProperty!); }


    /// <summary>
    /// Get parent view.
    /// </summary>
    public NSView? SuperView { get => this.GetProperty<NSView>(SuperViewProperty!); }


    /// <summary>
    /// Get bounds of view which is not clipped by its super view.
    /// </summary>
    public NSRect VisibleRect { get => this.GetProperty<NSRect>(VisibleRectProperty!); }


    /// <summary>
    /// Get window which contains the view.
    /// </summary>
    public NSWindow? Window { get => this.GetProperty<NSWindow>(WindowProperty!); }
}