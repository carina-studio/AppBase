using Avalonia;
using System;

namespace CarinaStudio.Animation
{
    /// <summary>
    /// <see cref="Animator"/> to animate <see cref="Size"/>.
    /// </summary>
    [Obsolete($"Use {nameof(SizeRenderingAnimator)} instead.")]
    public class SizeAnimator : ValueAnimator<Size>
    {
        /// <summary>
        /// Initialize new <see cref="SizeAnimator"/> instance.
        /// </summary>
        /// <param name="start">Start <see cref="Size"/>.</param>
        /// <param name="end">End <see cref="Size"/>.</param>
        public SizeAnimator(Size start, Size end) : base(start, end)
        { }


        /// <inheritdoc/>
        protected override Size GenerateValue(double progress)
        {
            var offsetX = (this.EndValue.Width - this.StartValue.Width) * progress;
            var offsetY = (this.EndValue.Height - this.StartValue.Height) * progress;
            return new Size(this.StartValue.Width + offsetX, this.StartValue.Height + offsetY);
        }
    }
}
