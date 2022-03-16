using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Rendering;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using CarinaStudio.Collections;
using SkiaSharp;
using System;
using System.Diagnostics;

namespace CarinaStudio.Controls.Presenters
{
    /// <summary>
    /// Presenter of circular progress.
    /// </summary>
    public class CircularProgressPresenter : Control
    {
        // Adapter of custom drawing.
        class DrawingParameters : ICustomDrawOperation
        {
            // Fields
            public SKPaint? BorderPaint;
            public readonly double BorderThickness;
            public readonly CircularProgressPresenter Presenter;
            public readonly double ProgressEndAngle;
            public SKPaint? ProgressPaint;
            public readonly double ProgressStartAngle;
            public SKPaint? RingPaint;
            public readonly double RingThickness;

            // Constructor.
            public DrawingParameters(CircularProgressPresenter presenter)
            {
                this.BorderPaint = presenter.borderPaint;
                this.BorderThickness = presenter.GetValue<double>(BorderThicknessProperty);
                this.Bounds = presenter.Bounds.Let(it => new Rect(0, 0, it.Width, it.Height));
                this.Presenter = presenter;
                this.ProgressEndAngle = presenter.progressEndAngle;
                this.ProgressPaint = presenter.progressPaint;
                this.ProgressStartAngle = presenter.progressStartAngle;
                this.RingPaint = presenter.ringPaint;
                this.RingThickness = presenter.GetValue<double>(RingThicknessProperty);
            }

            // Bounds.
            public Rect Bounds { get; }

            // Dispose.
            public void Dispose()
            { }

            // Check equality.
            public bool Equals(ICustomDrawOperation? obj) =>
                obj == this;

            // Hit test.
            public bool HitTest(Point point) => false;

            // Render.
            public void Render(IDrawingContextImpl drawingContext) =>
                this.Presenter.OnRender(drawingContext, this);
        }


        // Timer for intermediate progress animation.
        class IntermediateProgressAnimationTimerImpl : UiThreadRenderTimer
        {
            // Fields.
            readonly LinkedList<CircularProgressPresenter> presenters = new LinkedList<CircularProgressPresenter>();

            // Constructor.
            public IntermediateProgressAnimationTimerImpl() : base(60)
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
            public void Start(LinkedListNode<CircularProgressPresenter> node)
            {
                if (node.List != null)
                    return;
                this.presenters.AddLast(node);
                if (this.presenters.Count == 1)
                    this.Start();
            }

            // Stop animation.
            public void Stop(LinkedListNode<CircularProgressPresenter> node)
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
        public static readonly AvaloniaProperty<IBrush?> BorderBrushProperty = AvaloniaProperty.Register<CircularProgressPresenter, IBrush?>(nameof(BorderBrush), Brushes.Gray);
        /// <summary>
        /// Property of <see cref="BorderThickness"/>.
        /// </summary>
        public static readonly AvaloniaProperty<double> BorderThicknessProperty = AvaloniaProperty.Register<CircularProgressPresenter, double>(nameof(BorderThickness), 1,
            coerce: (o, it) => Math.Max(0, it),
            validate: double.IsFinite);
        /// <summary>
        /// Property of <see cref="IsIntermediate"/>.
        /// </summary>
        public static readonly AvaloniaProperty<bool> IsIntermediateProperty = AvaloniaProperty.Register<CircularProgressPresenter, bool>(nameof(IsIntermediate), false);
        /// <summary>
        /// Property of <see cref="MaxProgress"/>.
        /// </summary>
        public static readonly AvaloniaProperty<double> MaxProgressProperty = AvaloniaProperty.Register<CircularProgressPresenter, double>(nameof(MaxProgress), 100,
            coerce: (o, it) => Math.Max(o.GetValue<double>(MinProgressProperty.AsNonNull()), it),
            validate: double.IsFinite);
        /// <summary>
        /// Property of <see cref="MinProgress"/>.
        /// </summary>
        public static readonly AvaloniaProperty<double> MinProgressProperty = AvaloniaProperty.Register<CircularProgressPresenter, double>(nameof(MinProgress), 0,
            coerce: (o, it) => Math.Min(o.GetValue<double>(MaxProgressProperty), it),
            validate: double.IsFinite);
        /// <summary>
        /// Property of <see cref="Progress"/>.
        /// </summary>
        public static readonly AvaloniaProperty<double> ProgressProperty = AvaloniaProperty.Register<CircularProgressPresenter, double>(nameof(Progress), 0,
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
        public static readonly AvaloniaProperty<IBrush?> ProgressBrushProperty = AvaloniaProperty.Register<CircularProgressPresenter, IBrush?>(nameof(ProgressBrush), Brushes.Gray);
        /// <summary>
        /// Property of <see cref="RingBrush"/>.
        /// </summary>
        public static readonly AvaloniaProperty<IBrush?> RingBrushProperty = AvaloniaProperty.Register<CircularProgressPresenter, IBrush?>(nameof(RingBrush), Brushes.LightGray);
        /// <summary>
        /// Property of <see cref="RingThickness"/>.
        /// </summary>
        public static readonly AvaloniaProperty<double> RingThicknessProperty = AvaloniaProperty.Register<CircularProgressPresenter, double>(nameof(RingThickness), 5,
            coerce: (o, it) => Math.Max(1, it),
            validate: double.IsFinite);


        // Constants.
        const int IntermediateProgressAnimationDuration = 1000;
        

        // Static fields.
        static readonly LinkedList<CircularProgressPresenter> IntermediateProgressAnimatingPresenters = new LinkedList<CircularProgressPresenter>();
        static readonly IntermediateProgressAnimationTimerImpl IntermediateProgressAnimationTimer = new IntermediateProgressAnimationTimerImpl();
        static readonly AvaloniaProperty<double> IntermediateProgresProperty = AvaloniaProperty.Register<CircularProgressPresenter, double>("IntermediateProgres", 0);
        

        // Fields.
        SKPaint? borderPaint;
        readonly LinkedListNode<CircularProgressPresenter> intermediateProgressAnimatingNode;
        long intermediateProgressAnimationStartTime = -1;
        bool isAttachedToVisualTree;
        double progressEndAngle = double.NaN;
        SKPaint? progressPaint;
        double progressStartAngle = double.NaN;
        SKPaint? ringPaint;
        readonly Stopwatch stopwatch = new Stopwatch();
        

        // Static initializer.
        static CircularProgressPresenter()
        {
            AffectsRender<CircularProgressPresenter>(
                BorderBrushProperty,
                BorderThicknessProperty,
                IntermediateProgresProperty,
                IsIntermediateProperty,
                MaxProgressProperty,
                MinProgressProperty,
                ProgressProperty,
                ProgressBrushProperty,
                RingBrushProperty, 
                RingThicknessProperty);
        }


        /// <summary>
        /// Initialize new <see cref="CircularProgressPresenter"/> instance.
        /// </summary>
        public CircularProgressPresenter()
        { 
            this.intermediateProgressAnimatingNode = new LinkedListNode<CircularProgressPresenter>(this);
        }


        // Animate intermediate progress.
        void AnimateIntermediateProgress()
        {
            if (this.intermediateProgressAnimationStartTime >= 0)
            {
                var duration = (this.stopwatch.ElapsedMilliseconds - this.intermediateProgressAnimationStartTime) % IntermediateProgressAnimationDuration;
                this.SetValue<double>(IntermediateProgresProperty, (double)duration / IntermediateProgressAnimationDuration);
            }
            else
                this.intermediateProgressAnimationStartTime = this.stopwatch.ElapsedMilliseconds;
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


        // Create Skia paint object.
        SKPaint? CreateSkiaPaint(IBrush? brush)
        {
            if (brush is ISolidColorBrush solidColorBrush)
            {
                var color = solidColorBrush.Color;
                return new SKPaint() { Color = new SKColor(color.R, color.G, color.B, color.A) };
            }
            return null;
        }


        /// <summary>
        /// Get or set whether the progress is intermediate state or not.
        /// </summary>
        public bool IsIntermediate
        {
            get => this.GetValue<bool>(IsIntermediateProperty);
            set => this.SetValue<bool>(IsIntermediateProperty, value);
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
            if (this.GetValue<bool>(IsIntermediateProperty))
            {
                this.SetValue<double>(IntermediateProgresProperty, 0);
                this.intermediateProgressAnimationStartTime = this.stopwatch.ElapsedMilliseconds;
                IntermediateProgressAnimationTimer.Start(this.intermediateProgressAnimatingNode);
            }
            this.isAttachedToVisualTree = true;
        }


        /// <inheritdoc/>
        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            IntermediateProgressAnimationTimer.Stop(this.intermediateProgressAnimatingNode);
            this.borderPaint = null;
            this.intermediateProgressAnimationStartTime = -1;
            this.isAttachedToVisualTree = false;
            this.progressPaint = null;
            this.ringPaint = null;
            this.stopwatch.Stop();
            base.OnDetachedFromVisualTree(e);
        }


        /// <inheritdoc/>
        protected override void OnPropertyChanged<TProperty>(AvaloniaPropertyChangedEventArgs<TProperty> change)
        {
            base.OnPropertyChanged(change);
            var property = change.Property;
            if (property == BorderBrushProperty)
                this.borderPaint = null;
            else if (property == IsIntermediateProperty)
            {
                if (!this.GetValue<bool>(IsIntermediateProperty))
                {
                    this.intermediateProgressAnimationStartTime = -1;
                    IntermediateProgressAnimationTimer.Stop(this.intermediateProgressAnimatingNode);
                }
                else if (this.isAttachedToVisualTree)
                {
                    this.SetValue<double>(IntermediateProgresProperty, 0);
                    this.intermediateProgressAnimationStartTime = this.stopwatch.ElapsedMilliseconds;
                    IntermediateProgressAnimationTimer.Start(this.intermediateProgressAnimatingNode);
                }
            }
            else if (property == ProgressBrushProperty)
                this.progressPaint = null;
            else if (property == RingBrushProperty)
                this.ringPaint = null;
        }


        // Render content.
        void OnRender(IDrawingContextImpl drawingContext, DrawingParameters parameters)
        {
            // get canvas
            if (!(drawingContext is ISkiaDrawingContextImpl skiaDrawingContext))
                return;
            var canvas = skiaDrawingContext.SkCanvas;

            // get state
            var halfWidth = (float)parameters.Bounds.Width / 2;
            var halfHeight = (float)parameters.Bounds.Height / 2;
            var borderThickness = (float)parameters.BorderThickness;
            var halfBorderThickness = borderThickness / 2;
            var ringThickness = (float)parameters.RingThickness;
            var halfRingThickness = ringThickness / 2;
            
            // draw ring
            parameters.RingPaint?.Let(paint =>
            {
                canvas.DrawOval(halfWidth, halfHeight, 
                    halfWidth - borderThickness - halfRingThickness,
                    halfHeight - borderThickness - halfRingThickness,
                    paint);
            });

            // draw progress
            parameters.ProgressPaint?.Let(paint =>
            {
                if (Math.Abs(parameters.ProgressStartAngle - parameters.ProgressEndAngle) > 0.1)
                {
                    var sweepAngle = parameters.ProgressEndAngle - parameters.ProgressStartAngle;
                    if (sweepAngle < 0)
                        sweepAngle += 360;
                    canvas.DrawArc(new SKRect(
                            halfBorderThickness + halfRingThickness, 
                            halfBorderThickness + halfRingThickness, 
                            (float)parameters.Bounds.Width - halfBorderThickness - halfRingThickness, 
                            (float)parameters.Bounds.Height - halfBorderThickness - halfRingThickness),
                        (float)parameters.ProgressStartAngle,
                        (float)sweepAngle,
                        false,
                        paint);
                }
            });

            // draw border
            parameters.BorderPaint?.Let(paint =>
            {
                canvas.DrawOval(halfWidth, halfHeight, 
                    halfWidth - halfBorderThickness,
                    halfHeight - halfBorderThickness,
                    paint);
                canvas.DrawOval(halfWidth, halfHeight, 
                    halfWidth - halfBorderThickness - ringThickness,
                    halfHeight - halfBorderThickness - ringThickness,
                    paint);
            });
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
            // update progress angles
            this.UpdateProgressAngles();

            // prepare resources
            if (this.borderPaint == null)
            {
                var thickness = this.GetValue<double>(BorderThicknessProperty);
                if (thickness >= 0.1)
                {
                    this.borderPaint = this.CreateSkiaPaint(this.GetValue<IBrush?>(BorderBrushProperty))?.Also(it =>
                    {
                        it.IsAntialias = true;
                        it.StrokeWidth = (float)thickness;
                        it.Style = SKPaintStyle.Stroke;
                    });
                }
            }
            if (this.progressPaint == null)
            {
                var thickness = this.GetValue<double>(RingThicknessProperty);
                this.progressPaint = this.CreateSkiaPaint(this.GetValue<IBrush?>(ProgressBrushProperty))?.Also(it =>
                {
                    it.IsAntialias = true;
                    it.StrokeCap = SKStrokeCap.Round;
                    it.StrokeWidth = (float)thickness;
                    it.Style = SKPaintStyle.Stroke;
                });
            }
            if (this.ringPaint == null)
            {
                var thickness = this.GetValue<double>(RingThicknessProperty);
                this.ringPaint = this.CreateSkiaPaint(this.GetValue<IBrush?>(RingBrushProperty))?.Also(it =>
                {
                    it.IsAntialias = true;
                    it.StrokeWidth = (float)thickness;
                    it.Style = SKPaintStyle.Stroke;
                });
            }

            // draw
            drawingContext.Custom(new DrawingParameters(this));
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


        // Update progress angles.
        void UpdateProgressAngles()
        {
            if (this.GetValue<bool>(IsIntermediateProperty))
            {
                var progress = this.GetValue<double>(IntermediateProgresProperty);
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
        }
    }
}