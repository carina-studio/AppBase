using Avalonia;

namespace CarinaStudio.Animation
{
    /// <summary>
    /// <see cref="Animator"/> to animate <see cref="Thickness"/>.
    /// </summary>
    public class ThicknessAnimator : ValueAnimator<Thickness>
    {
        /// <summary>
        /// Initialize new <see cref="ThicknessAnimator"/> instance.
        /// </summary>
        /// <param name="start">Start <see cref="Thickness"/>.</param>
        /// <param name="end">End <see cref="Thickness"/>.</param>
        public ThicknessAnimator(Thickness start, Thickness end) : base(start, end)
        { }


        /// <inheritdoc/>
        protected override Thickness GenerateValue(double progress)
        {
            var start = this.StartValue;
            var end = this.EndValue;
            var offsetLeft = (end.Left - start.Left) * progress;
            var offsetTop = (end.Top - start.Top) * progress;
            var offsetRight = (end.Right - start.Right) * progress;
            var offsetBottom = (end.Bottom - start.Bottom) * progress;
            return new Thickness(start.Left + offsetLeft, start.Top + offsetTop, start.Right + offsetRight, start.Bottom + offsetBottom);
        }
    }
}
