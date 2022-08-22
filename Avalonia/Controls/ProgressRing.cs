using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Styling;
using System;

namespace CarinaStudio.Controls
{
    /// <summary>
    /// Progress ring.
    /// </summary>
    public class ProgressRing : RangeBase, IStyleable
    {
        /// <summary>
        /// Property of <see cref="IsIndeterminate"/>.
        /// </summary>
        public static readonly StyledProperty<bool> IsIndeterminateProperty = AvaloniaProperty.Register<ProgressRing, bool>(nameof(IsIndeterminate), false);
        /// <summary>
        /// Property of <see cref="RingBorderThickness"/>.
        /// </summary>
        public static readonly StyledProperty<double> RingBorderThicknessProperty = AvaloniaProperty.Register<ProgressRing, double>(nameof(RingBorderThickness), 0,
            coerce: (o, it) => Math.Max(0, it),
            validate: double.IsFinite);
        /// <summary>
        /// Property of <see cref="RingThickness"/>.
        /// </summary>
        public static readonly StyledProperty<double> RingThicknessProperty = AvaloniaProperty.Register<ProgressRing, double>(nameof(RingThickness), 5,
            coerce: (o, it) => Math.Max(1, it),
            validate: double.IsFinite);
        

        /// <summary>
        /// Get or set whether the progress is in indeterminate state or not.
        /// </summary>
        public bool IsIndeterminate
        {
            get => this.GetValue<bool>(IsIndeterminateProperty);
            set => this.SetValue<bool>(IsIndeterminateProperty, value);
        }


        /// <summary>
        /// Get or set thickness of border of ring in pixels.
        /// </summary>
        public double RingBorderThickness
        {
            get => this.GetValue<double>(RingBorderThicknessProperty);
            set => this.SetValue<double>(RingBorderThicknessProperty, value);
        }


        /// <summary>
        /// Get or set thickness of ring in pixels.
        /// </summary>
        public double RingThickness
        {
            get => this.GetValue<double>(RingThicknessProperty);
            set => this.SetValue<double>(RingThicknessProperty, value);
        }


        // Interface implementations.
		Type IStyleable.StyleKey => typeof(ProgressRing);
    }
}