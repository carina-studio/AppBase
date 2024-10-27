using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.VisualTree;
using CarinaStudio.Animation;
using System;
using System.Collections.Generic;

namespace CarinaStudio.Controls
{
	/// <summary>
	/// Extensions for <see cref="ScrollViewer"/>.
	/// </summary>
	public static class ScrollViewerExtensions
	{
		// Static fields.
		static readonly Dictionary<ScrollViewer, DoubleAnimator> SmoothScrollingAnimators = new();
		static readonly Dictionary<ScrollViewer, Vector> SmoothScrollingTargetOffsets = new();


		/// <summary>
		/// Check whether given <see cref="ScrollViewer"/> is performing smooth scrolling or not.
		/// </summary>
		/// <param name="scrollViewer"><see cref="ScrollViewer"/>.</param>
		/// <returns>True if <see cref="ScrollViewer"/> is performing smooth scrolling.</returns>
		public static bool IsSmoothScrolling(this ScrollViewer scrollViewer) =>
			SmoothScrollingAnimators.ContainsKey(scrollViewer);


		/// <summary>
		/// Scroll given <see cref="Visual"/> in <see cref="ScrollViewer"/> into view.
		/// </summary>
		/// <param name="scrollViewer"><see cref="ScrollViewer"/>.</param>
		/// <param name="visual"><see cref="Visual"/> inside <see cref="ScrollViewer"/>.</param>
		/// <returns>True if visual has been scrolled into view.</returns>
		public static bool ScrollIntoView(this ScrollViewer scrollViewer, Visual visual) =>
			ScrollIntoView(scrollViewer, visual, TimeSpan.Zero, null, false);
		
		
		/// <summary>
		/// Scroll given <see cref="Visual"/> in <see cref="ScrollViewer"/> into view.
		/// </summary>
		/// <param name="scrollViewer"><see cref="ScrollViewer"/>.</param>
		/// <param name="visual"><see cref="Visual"/> inside <see cref="ScrollViewer"/>.</param>
		/// <param name="scrollToCenter">True to try scrolling <see cref="Visual"/> to center of viewport.</param>
		/// <returns>True if visual has been scrolled into view.</returns>
		public static bool ScrollIntoView(this ScrollViewer scrollViewer, Visual visual, bool scrollToCenter) =>
			ScrollIntoView(scrollViewer, visual, TimeSpan.Zero, null, scrollToCenter);
		
		
		// Scroll given visual into viewport of ScrollViewer.
	    static bool ScrollIntoView(ScrollViewer scrollViewer, Visual visual, TimeSpan duration, Func<double, double>? interpolator, bool scrollToCenter)
	    {
	        // check parameter
	        if (scrollViewer == visual)
	            return false;

	        // find position in scroll viewer
	        var offset = scrollViewer.Offset;
	        var contentBounds = visual.Bounds;
	        var leftInScrollViewer = contentBounds.Left;
	        var topInScrollViewer = contentBounds.Top;
	        var parent = visual.GetVisualParent();
	        while (parent != scrollViewer && parent is not null)
	        {
	            var parentBounds = parent.Bounds;
	            leftInScrollViewer += parentBounds.Left;
	            topInScrollViewer += parentBounds.Top;
	            parent = parent.GetVisualParent();
	        }
	        if (parent is null)
	            return false;
	        leftInScrollViewer += offset.X;
	        topInScrollViewer += offset.Y;
	        var rightInScrollViewer = leftInScrollViewer + contentBounds.Width;
	        var bottomInScrollViewer = topInScrollViewer + contentBounds.Height;

	        // check whether scrolling is needed or not
	        var viewportSize = scrollViewer.Viewport;
	        var viewportCenter = new Point(offset.X + viewportSize.Width / 2, offset.Y + viewportSize.Height / 2);
	        var scrollHorizontally = scrollViewer.HorizontalScrollBarVisibility != ScrollBarVisibility.Disabled && Global.Run(() =>
	        {
	            if (contentBounds.Width > viewportSize.Width)
	            {
	                return scrollViewer.FlowDirection switch
	                {
	                    FlowDirection.RightToLeft => Math.Abs(rightInScrollViewer - (offset.X + viewportSize.Width)) > double.Epsilon * 2,
	                    _ => Math.Abs(leftInScrollViewer - offset.X) > double.Epsilon * 2,
	                };
	            }
	            if (!scrollToCenter)
	                return leftInScrollViewer < offset.X || rightInScrollViewer > offset.X + viewportSize.Width;
	            return leftInScrollViewer > viewportCenter.X || rightInScrollViewer < viewportCenter.X;
	        });
	        var scrollVertically = scrollViewer.VerticalScrollBarVisibility != ScrollBarVisibility.Disabled && Global.Run(() =>
	        {
	            if (contentBounds.Height > viewportSize.Height)
	                return Math.Abs(topInScrollViewer - offset.Y) > double.Epsilon * 2;
	            if (!scrollToCenter)
	                return topInScrollViewer < offset.Y || bottomInScrollViewer > offset.Y + viewportSize.Height;
	            return topInScrollViewer > viewportCenter.Y || bottomInScrollViewer < viewportCenter.Y;
	        });
	        if (!scrollHorizontally && !scrollVertically)
	            return false;

	        // calculate position to scroll
	        var newOffsetX = Global.Run(() =>
	        {
	            if (!scrollHorizontally)
	                return offset.X;
	            if (contentBounds.Width > viewportSize.Width)
	            {
	                return scrollViewer.FlowDirection switch
	                {
	                    FlowDirection.RightToLeft => rightInScrollViewer - viewportSize.Width,
	                    _ => leftInScrollViewer,
	                };
	            }
	            if (scrollToCenter)
	                return (leftInScrollViewer + rightInScrollViewer) / 2 - viewportSize.Width / 2;
	            if (leftInScrollViewer < offset.X)
	                return leftInScrollViewer;
	            return rightInScrollViewer - viewportSize.Width;
	        });
	        var newOffsetY = Global.Run(() =>
	        {
	            if (!scrollVertically)
	                return offset.Y;
	            if (contentBounds.Height > viewportSize.Height)
	                return topInScrollViewer;
	            if (scrollToCenter)
	                return (topInScrollViewer + bottomInScrollViewer) / 2 - viewportSize.Height / 2;
	            if (topInScrollViewer < offset.Y)
	                return topInScrollViewer;
	            return bottomInScrollViewer - viewportSize.Height;
	        });
	        
	        // scroll to content
	        return ScrollTo(scrollViewer, new(newOffsetX, newOffsetY), duration, interpolator);
	    }
	    
	    
	    // Scroll to given offset.
	    static bool ScrollTo(ScrollViewer scrollViewer, Vector offset, TimeSpan duration, Func<double, double>? interpolator)
	    {
		    // check offset
		    var extent = scrollViewer.Extent;
		    var viewportSize = scrollViewer.Viewport;
		    var currentOffset = scrollViewer.Offset;
		    var offsetX = Math.Max(0, Math.Min(offset.X, extent.Width - viewportSize.Width));
		    var offsetY = Math.Max(0, Math.Min(offset.Y, extent.Height - viewportSize.Height));
		    var diffX = (offsetX - currentOffset.X);
		    var diffY = (offsetY - currentOffset.Y);
		    if (Math.Abs(diffX) < Double.Epsilon * 2 && Math.Abs(diffY) < double.Epsilon * 2)
			    return false;
		    
		    // cancel previous scrolling
	        if (SmoothScrollingAnimators.TryGetValue(scrollViewer, out var prevAnimator))
	            prevAnimator.Cancel();
	        SmoothScrollingTargetOffsets.Remove(scrollViewer);

	        // scroll to given offset
	        if (duration.TotalMilliseconds > 0)
	        {
	            var animator = default(DoubleAnimator);
	            void OnPointerPressed(object? sender, RoutedEventArgs e) =>
	                animator?.Cancel();
	            void OnPointerWheelChanged(object? sender, RoutedEventArgs e) =>
	                animator?.Cancel();
	            var viewportChangedObserverToken = scrollViewer.GetObservable(ScrollViewer.ViewportProperty).Subscribe(_ => animator?.Cancel());
	            animator = new DoubleAnimator(0, 1).Also(it =>
	            {
	                it.Cancelled += (_, _) =>
	                {
	                    if (SmoothScrollingAnimators.TryGetValue(scrollViewer, out var currentAnimator) && currentAnimator == it)
	                    {
	                        SmoothScrollingAnimators.Remove(scrollViewer);
	                        SmoothScrollingTargetOffsets.Remove(scrollViewer);
	                    }
	                    scrollViewer.RemoveHandler(ScrollViewer.PointerPressedEvent, OnPointerPressed);
	                    scrollViewer.RemoveHandler(ScrollViewer.PointerWheelChangedEvent, OnPointerWheelChanged);
	                    viewportChangedObserverToken.Dispose();
	                };
	                it.Completed += (_, _) =>
	                {
	                    if (SmoothScrollingAnimators.TryGetValue(scrollViewer, out var currentAnimator) && currentAnimator == it)
	                    {
	                        SmoothScrollingAnimators.Remove(scrollViewer);
	                        SmoothScrollingTargetOffsets.Remove(scrollViewer);
	                        scrollViewer.Offset = new(offsetX, offsetY);
	                    }
	                    scrollViewer.RemoveHandler(ScrollViewer.PointerPressedEvent, OnPointerPressed);
	                    scrollViewer.RemoveHandler(ScrollViewer.PointerWheelChangedEvent, OnPointerWheelChanged);
	                    viewportChangedObserverToken.Dispose();
	                };
	                it.Duration = duration;
	                it.Interpolator = interpolator ?? Interpolators.Default;
	                it.ProgressChanged += (_, _) => { scrollViewer.Offset = new(currentOffset.X + diffX * it.Progress, currentOffset.Y + diffY * it.Progress); };
	            });
	            scrollViewer.AddHandler(ScrollViewer.PointerPressedEvent, OnPointerPressed, RoutingStrategies.Tunnel);
	            scrollViewer.AddHandler(ScrollViewer.PointerWheelChangedEvent, OnPointerWheelChanged, RoutingStrategies.Tunnel);
	            SmoothScrollingAnimators[scrollViewer] = animator;
	            SmoothScrollingTargetOffsets[scrollViewer] = new(offsetX, offsetY);
	            animator.Start();
	        }
	        else
	            scrollViewer.Offset = new(offsetX, offsetY);
	        return true;
	    }
	    
	    
	    /// <summary>
	    /// Scroll given <see cref="Visual"/> smoothly into viewport of <see cref="ScrollViewer"/>.
	    /// </summary>
	    /// <param name="scrollViewer"><see cref="ScrollViewer"/>.</param>
	    /// <param name="visual"><see cref="Visual"/>.</param>
	    /// <param name="duration">Duration of smooth scrolling.</param>
	    /// <param name="interpolator">Interpolator of smooth scrolling.</param>
	    /// <param name="scrollToCenter">True to scroll content to center of viewport.</param>
	    /// <returns>True if smooth scrolling starts successfully.</returns>
	    public static bool SmoothScrollIntoView(this ScrollViewer scrollViewer, Visual visual, TimeSpan duration, Func<double, double>? interpolator = null, bool scrollToCenter = true) =>
		    ScrollIntoView(scrollViewer, visual, duration, interpolator, scrollToCenter);


	    /// <summary>
	    /// Scroll <see cref="ScrollViewer"/> to given offset smoothly.
	    /// </summary>
	    /// <param name="scrollViewer"><see cref="ScrollViewer"/>.</param>
	    /// <param name="offset">Target offset.</param>
	    /// <param name="duration">Duration of smooth scrolling.</param>
	    /// <param name="interpolator">Interpolator of smooth scrolling.</param>
	    /// <returns>True if smooth scrolling starts successfully.</returns>
	    public static bool SmoothScrollTo(this ScrollViewer scrollViewer, Vector offset, TimeSpan duration, Func<double, double>? interpolator = null) =>
		    ScrollTo(scrollViewer, offset, duration, interpolator);
	    
	    
	    /// <summary>
	    /// Try getting target offset of smooth scrolling.
	    /// </summary>
	    /// <param name="scrollViewer"><see cref="ScrollViewer"/>.</param>
	    /// <param name="offset">Target offset.</param>
	    /// <returns>True if tar offset got successfully.</returns>
	    public static bool TryGetSmoothScrollingTargetOffset(this ScrollViewer scrollViewer, out Vector offset) =>
		    SmoothScrollingTargetOffsets.TryGetValue(scrollViewer, out offset);
	}
}
