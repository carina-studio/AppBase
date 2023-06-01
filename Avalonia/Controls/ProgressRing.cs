using Avalonia;
using Avalonia.Controls.Primitives;
#if AVALONIA_11_0_0_P4
using Avalonia.Styling;
#endif
using System;

namespace CarinaStudio.Controls
{
    /// <summary>
    /// Progress ring.
    /// </summary>
    public class ProgressRing : RangeBase
#if AVALONIA_11_0_0_P4
        , IStyleable
#endif
    {
        /// <summary>
        /// Property of <see cref="IsIndeterminate"/>.
        /// </summary>
        public static readonly StyledProperty<bool> IsIndeterminateProperty = AvaloniaProperty.Register<ProgressRing, bool>(nameof(IsIndeterminate), false);
        /// <summary>
        /// Property of <see cref="RingBorderThickness"/>.
        /// </summary>
        public static readonly StyledProperty<double> RingBorderThicknessProperty = AvaloniaProperty.Register<ProgressRing, double>(nameof(RingBorderThickness), 0,
            coerce: (_, it) => Math.Max(0, it),
            validate: double.IsFinite);
        /// <summary>
        /// Property of <see cref="RingThickness"/>.
        /// </summary>
        public static readonly StyledProperty<double> RingThicknessProperty = AvaloniaProperty.Register<ProgressRing, double>(nameof(RingThickness), 5,
            coerce: (_, it) => Math.Max(1, it),
            validate: double.IsFinite);
        

        /// <summary>
        /// Get or set whether the progress is in indeterminate state or not.
        /// </summary>
        public bool IsIndeterminate
        {
            get => this.GetValue(IsIndeterminateProperty);
            set => this.SetValue(IsIndeterminateProperty, value);
        }


        /// <summary>
        /// Get or set thickness of border of ring in pixels.
        /// </summary>
        public double RingBorderThickness
        {
            get => this.GetValue(RingBorderThicknessProperty);
            set => this.SetValue(RingBorderThicknessProperty, value);
        }


        /// <summary>
        /// Get or set thickness of ring in pixels.
        /// </summary>
        public double RingThickness
        {
            get => this.GetValue(RingThicknessProperty);
            set => this.SetValue(RingThicknessProperty, value);
        }


#if AVALONIA_11_0_0_P4
        /// <inheritdoc/>
        Type IStyleable.StyleKey => typeof(ProgressRing);
#else
        /// <inheritdoc/>
        protected override Type StyleKeyOverride => typeof(ProgressRing);
#endif
    }
}