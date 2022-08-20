using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Rendering;
using CarinaStudio.Collections;
using System;
using System.Diagnostics;

namespace CarinaStudio.Controls.Presenters
{
    /// <summary>
    /// Presenter of progress ring.
    /// </summary>
    public class ProgressRingPresenter : Control
    {
        // Timer for indeterminate progress animation.
        class IndeterminateProgressAnimationTimerImpl : UiThreadRenderTimer
        {
            // Fields.
            readonly LinkedList<ProgressRingPresenter> presenters = new LinkedList<ProgressRingPresenter>();

            // Constructor.
            public IndeterminateProgressAnimationTimerImpl() : base(60)
            { 
                this.Tick += timeSpan =>
                {
                    var node = this.presenters.First;
                    while (node != null)
                    {
                        node.Value.AnimateIntermediateProgress();
                        node = node.Next;
                    }
                };
            }

            // Start animation
            public void Start(LinkedListNode<ProgressRingPresenter> node)
            {
                if (node.List != null)
                    return;
                this.presenters.AddLast(node);
                if (this.presenters.Count == 1)
                    this.Start();
            }

            // Stop animation.
            public void Stop(LinkedListNode<ProgressRingPresenter> node)
            {
                if (node.List == null)
                    return;
                this.presenters.Remove(node);
                if (this.presenters.IsEmpty())
                    this.Stop();
            }
        }


        /// <summary>
        /// Property of <see cref="BorderBrush"/>.
        /// </summary>
        public static readonly StyledProperty<IBrush?> BorderBrushProperty = AvaloniaProperty.Register<ProgressRingPresenter, IBrush?>(nameof(BorderBrush), Brushes.Gray);
        /// <summary>
        /// Property of <see cref="BorderThickness"/>.
        /// </summary>
        public static readonly StyledProperty<double> BorderThicknessProperty = AvaloniaProperty.Register<ProgressRingPresenter, double>(nameof(BorderThickness), 1,
            coerce: (o, it) => Math.Max(0, it),
            validate: double.IsFinite);
        /// <summary>
        /// Property of <see cref="IsIndeterminate"/>.
        /// </summary>
        public static readonly StyledProperty<bool> IsIndeterminateProperty = AvaloniaProperty.Register<ProgressRingPresenter, bool>(nameof(IsIndeterminate), false);
        /// <summary>
        /// Property of <see cref="MaxProgress"/>.
        /// </summary>
        public static readonly StyledProperty<double> MaxProgressProperty = AvaloniaProperty.Register<ProgressRingPresenter, double>(nameof(MaxProgress), 100,
            coerce: (o, it) => Math.Max(o.GetValue<double>(MinProgressProperty.AsNonNull()), it),
            validate: double.IsFinite);
        /// <summary>
        /// Property of <see cref="MinProgress"/>.
        /// </summary>
        public static readonly StyledProperty<double> MinProgressProperty = AvaloniaProperty.Register<ProgressRingPresenter, double>(nameof(MinProgress), 0,
            coerce: (o, it) => Math.Min(o.GetValue<double>(MaxProgressProperty), it),
            validate: double.IsFinite);
        /// <summary>
        /// Property of <see cref="Progress"/>.
        /// </summary>
        public static readonly StyledProperty<double> ProgressProperty = AvaloniaProperty.Register<ProgressRingPresenter, double>(nameof(Progress), 0,
            coerce: (o, it) => 
            {
                if (it < o.GetValue<double>(MinProgressProperty))
                    return o.GetValue<double>(MinProgressProperty);
                if (it > o.GetValue<double>(MaxProgressProperty))
                    return o.GetValue<double>(MaxProgressProperty);
                return it;
            },
            validate: double.IsFinite);
        /// <summary>
        /// Property of <see cref="ProgressBrush"/>.
        /// </summary>
        public static readonly StyledProperty<IBrush?> ProgressBrushProperty = AvaloniaProperty.Register<ProgressRingPresenter, IBrush?>(nameof(ProgressBrush), Brushes.Gray);
        /// <summary>
        /// Property of <see cref="RingBrush"/>.
        /// </summary>
        public static readonly StyledProperty<IBrush?> RingBrushProperty = AvaloniaProperty.Register<ProgressRingPresenter, IBrush?>(nameof(RingBrush), Brushes.LightGray);
        /// <summary>
        /// Property of <see cref="RingThickness"/>.
        /// </summary>
        public static readonly StyledProperty<double> RingThicknessProperty = AvaloniaProperty.Register<ProgressRingPresenter, double>(nameof(RingThickness), 5,
            coerce: (o, it) => Math.Max(1, it),
            validate: double.IsFinite);


        // Constants.
        const int IntermediateProgressAnimationDuration = 1000;
        

        // Static fields.
        static readonly IndeterminateProgressAnimationTimerImpl IndeterminateProgressAnimationTimer = new IndeterminateProgressAnimationTimerImpl();
        static readonly StyledProperty<double> IndeterminateProgressProperty = AvaloniaProperty.Register<ProgressRingPresenter, double>("IndeterminateProgress", 0);
        

        // Fields.
        Pen? borderPen;
        readonly LinkedListNode<ProgressRingPresenter> indeterminateProgressAnimatingNode;
        long indeterminateProgressAnimationStartTime = -1;
        bool isAttachedToVisualTree;
        double progressEndAngle = double.NaN;
        StreamGeometry? progressGeometry;
        Pen? progressPen;
        double progressStartAngle = double.NaN;
        Pen? ringPen;
        readonly Stopwatch stopwatch = new Stopwatch();
        

        // Static initializer.
        static ProgressRingPresenter()
        {
            AffectsRender<ProgressRingPresenter>(
                BorderBrushProperty,
                BorderThicknessProperty,
                IndeterminateProgressProperty,
                IsIndeterminateProperty,
                MaxProgressProperty,
                MinProgressProperty,
                ProgressProperty,
                ProgressBrushProperty,
                RingBrushProperty, 
                RingThicknessProperty);
        }


        /// <summary>
        /// Initialize new <see cref="ProgressRingPresenter"/> instance.
        /// </summary>
        public ProgressRingPresenter()
        { 
            this.indeterminateProgressAnimatingNode = new LinkedListNode<ProgressRingPresenter>(this);
        }


        // Animate intermediate progress.
        void AnimateIntermediateProgress()
        {
            if (this.indeterminateProgressAnimationStartTime >= 0)
            {
                var duration = (this.stopwatch.ElapsedMilliseconds - this.indeterminateProgressAnimationStartTime) % IntermediateProgressAnimationDuration;
                this.SetValue<double>(IndeterminateProgressProperty, (double)duration / IntermediateProgressAnimationDuration);
            }
            else
                this.indeterminateProgressAnimationStartTime = this.stopwatch.ElapsedMilliseconds;
            this.InvalidateProgressAngles();
        }


        /// <summary>
        /// Get or set brush of border.
        /// </summary>
        public IBrush? BorderBrush
        {
            get => this.GetValue<IBrush?>(BorderBrushProperty);
            set => this.SetValue<IBrush?>(BorderBrushProperty, value);
        }


        /// <summary>
        /// Get or set thickness of border in pixels.
        /// </summary>
        public double BorderThickness
        {
            get => this.GetValue<double>(BorderThicknessProperty);
            set => this.SetValue<double>(BorderThicknessProperty, value);
        }


        // Invalidate and update progress angles.
        void InvalidateProgressAngles()
        {
            if (this.GetValue<bool>(IsIndeterminateProperty))
            {
                var progress = this.GetValue<double>(IndeterminateProgressProperty);
                var centerAngle = progress * 360 - 90;
                var sweepAngle = progress >= 0.5
                    ? 15 + (1 - progress) * 75
                    : 15 + progress * 75;
                this.progressStartAngle = centerAngle - sweepAngle;
                this.progressEndAngle = centerAngle + sweepAngle;
            }
            else
            {
                var min = this.GetValue<double>(MinProgressProperty);
                var max = this.GetValue<double>(MaxProgressProperty);
                var progress = this.GetValue<double>(ProgressProperty);
                this.progressStartAngle = -90;
                if (max > min)
                    this.progressEndAngle = (progress / (max - min)) * 360 - 90;
                else
                    this.progressEndAngle = -90;
            }
            this.progressGeometry = null;
        }


        /// <summary>
        /// Get or set whether the progress is in indeterminate state or not.
        /// </summary>
        public bool IsIndeterminate
        {
            get => this.GetValue<bool>(IsIndeterminateProperty);
            set => this.SetValue<bool>(IsIndeterminateProperty, value);
        }


        /// <summary>
        /// Get or set maximum of progress.
        /// </summary>
        public double MaxProgress
        {
            get => this.GetValue<double>(MaxProgressProperty);
            set => this.SetValue<double>(MaxProgressProperty, value);
        }


        /// <summary>
        /// Get or set minimum of progress.
        /// </summary>
        public double MinProgress
        {
            get => this.GetValue<double>(MinProgressProperty);
            set => this.SetValue<double>(MinProgressProperty, value);
        }


        /// <inheritdoc/>
        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);
            this.stopwatch.Start();
            if (this.GetValue<bool>(IsIndeterminateProperty))
            {
                this.SetValue<double>(IndeterminateProgressProperty, 0);
                this.indeterminateProgressAnimationStartTime = this.stopwatch.ElapsedMilliseconds;
                IndeterminateProgressAnimationTimer.Start(this.indeterminateProgressAnimatingNode);
            }
            this.isAttachedToVisualTree = true;
        }


        /// <inheritdoc/>
        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            IndeterminateProgressAnimationTimer.Stop(this.indeterminateProgressAnimatingNode);
            this.indeterminateProgressAnimationStartTime = -1;
            this.isAttachedToVisualTree = false;
            this.stopwatch.Stop();
            base.OnDetachedFromVisualTree(e);
        }


        /// <inheritdoc/>
        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);
            var property = change.Property;
            if (property == BorderBrushProperty)
                this.borderPen = null;
            else if (property == BorderThicknessProperty)
            {
                this.borderPen = null;
                this.progressGeometry = null;
            }
            else if (property == IsIndeterminateProperty)
            {
                if (!this.GetValue<bool>(IsIndeterminateProperty))
                {
                    this.indeterminateProgressAnimationStartTime = -1;
                    IndeterminateProgressAnimationTimer.Stop(this.indeterminateProgressAnimatingNode);
                    this.InvalidateProgressAngles();
                }
                else if (this.isAttachedToVisualTree)
                {
                    this.SetValue<double>(IndeterminateProgressProperty, 0);
                    this.indeterminateProgressAnimationStartTime = this.stopwatch.ElapsedMilliseconds;
                    IndeterminateProgressAnimationTimer.Start(this.indeterminateProgressAnimatingNode);
                }
            }
            else if (property == ProgressProperty)
            {
                if (!this.GetValue<bool>(IsIndeterminateProperty))
                    this.InvalidateProgressAngles();
            }
            else if (property == ProgressBrushProperty)
                this.progressPen = null;
            else if (property == RingBrushProperty)
                this.ringPen = null;
            else if (property == RingThicknessProperty)
            {
                this.ringPen = null;
                this.progressGeometry = null;
            }
        }


        // Calculate point on ring.
        Point PointOnRing(double width, double height, double borderThickness, double ringThickness, double angle)
        {
            var a = (width - borderThickness - ringThickness) / 2;
            var b = (height - borderThickness - ringThickness) / 2;
            var r = angle / 180 * Math.PI;
            var x = a * Math.Cos(r) + width / 2;
            var y = b * Math.Sin(r) + height / 2;
            return new Point(x, y);
        }


        /// <summary>
        /// Get or set progress.
        /// </summary>
        public double Progress
        {
            get => this.GetValue<double>(ProgressProperty);
            set => this.SetValue<double>(ProgressProperty, value);
        }


        /// <summary>
        /// Get or set brush of progress.
        /// </summary>
        public IBrush? ProgressBrush
        {
            get => this.GetValue<IBrush?>(ProgressBrushProperty);
            set => this.SetValue<IBrush?>(ProgressBrushProperty, value);
        }


        /// <inheritdoc/>
        public override void Render(DrawingContext drawingContext)
        {
            // prepare resources
            var bounds = this.Bounds;
            var width = bounds.Width;
            var height = bounds.Height;
            var centerX = width / 2;
            var centerY = height / 2;
            var ringThickness = this.GetValue<double>(RingThicknessProperty);
            var borderThickness = this.GetValue<double>(BorderThicknessProperty);
            var hasBorder = Math.Abs(borderThickness) >= 0.1;
            if (hasBorder && this.borderPen == null)
            {
                var brush = this.GetValue<IBrush?>(BorderBrushProperty);
                if (brush != null)
                    this.borderPen = new Pen(brush, borderThickness);
            }
            if (this.ringPen == null)
            {
                var brush = this.GetValue<IBrush?>(RingBrushProperty);
                if (brush != null)
                    this.ringPen = new Pen(brush, ringThickness);
            }
            if (this.progressGeometry == null 
                && Math.Abs(this.progressStartAngle - this.progressEndAngle) >= 1)
            {
                this.progressGeometry = new StreamGeometry();
                using var geometryContext = this.progressGeometry.Open();
                var startPoint = this.PointOnRing(width, height, borderThickness, ringThickness, this.progressStartAngle);
                var endPoint = this.PointOnRing(width, height, borderThickness, ringThickness, this.progressEndAngle);
                geometryContext.BeginFigure(startPoint, false);
                geometryContext.ArcTo(
                    endPoint,
                    new Size((width - borderThickness - ringThickness) / 2, (height - borderThickness - ringThickness) / 2),
                    0,
                    (this.progressEndAngle - this.progressStartAngle) >= 180,
                    SweepDirection.Clockwise
                );
                geometryContext.EndFigure(false);
            }
            if (this.progressPen == null)
            {
                var brush = this.GetValue<IBrush?>(ProgressBrushProperty);
                if (brush != null)
                    this.progressPen = new Pen(brush, ringThickness, lineCap: PenLineCap.Round);
            }

            // draw ring
            if (this.ringPen != null)
            {
                drawingContext.DrawEllipse(null, 
                    this.ringPen, 
                    new Point(centerX, centerY), 
                    (width - borderThickness - ringThickness) / 2,
                    (height - borderThickness - ringThickness) / 2
                );
            }

            // draw progress
            if (this.progressGeometry != null)
            {
                drawingContext.DrawGeometry(null,
                    this.progressPen,
                    this.progressGeometry
                );
            }

            // draw border
            if (hasBorder && this.borderPen != null)
            {
                drawingContext.DrawEllipse(null, 
                    this.borderPen, 
                    new Point(centerX, centerY), 
                    (width - borderThickness) / 2,
                    (height - borderThickness) / 2
                );
                drawingContext.DrawEllipse(null, 
                    this.borderPen, 
                    new Point(centerX, centerY), 
                    centerX - borderThickness - ringThickness,
                    centerY - borderThickness - ringThickness
                );
            }
        }


        /// <summary>
        /// Get or set brush of ring.
        /// </summary>
        public IBrush? RingBrush
        {
            get => this.GetValue<IBrush?>(RingBrushProperty);
            set => this.SetValue<IBrush?>(RingBrushProperty, value);
        }


        /// <summary>
        /// Get or set thickness of ring in pixels.
        /// </summary>
        public double RingThickness
        {
            get => this.GetValue<double>(RingThicknessProperty);
            set => this.SetValue<double>(RingThicknessProperty, value);
        }
    }
}