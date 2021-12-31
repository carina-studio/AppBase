using Avalonia;
using System;

namespace CarinaStudio.Animation
{
    /// <summary>
    /// <see cref="Animator"/> to animate <see cref="CornerRadius"/>.
    /// </summary>
    public class CornerRadiusAnimator : ValueAnimator<CornerRadius>
    {
        /// <summary>
        /// Initialize new <see cref="ThicknessAnimator"/> instance.
        /// </summary>
        /// <param name="start">Start <see cref="CornerRadius"/>.</param>
        /// <param name="end">End <see cref="CornerRadius"/>.</param>
        public CornerRadiusAnimator(CornerRadius start, CornerRadius end) : base(start, end)
        { }


        /// <inheritdoc/>
        protected override CornerRadius GenerateValue(double progress)
        {
            var start = this.StartValue;
            var end = this.EndValue;
            var offsetTL = (end.TopLeft - start.TopLeft) * progress;
            var offsetTR = (end.TopRight - start.TopRight) * progress;
            var offsetBR = (end.BottomRight - start.BottomRight) * progress;
            var offsetBL = (end.BottomLeft - start.BottomLeft) * progress;
            return new CornerRadius(start.TopLeft + offsetTL, start.TopRight + offsetTR, start.BottomRight + offsetBR, start.BottomLeft + offsetBL);
        }
    }
}
