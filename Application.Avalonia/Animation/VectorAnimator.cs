using Avalonia;

namespace CarinaStudio.Animation
{
    /// <summary>
    /// <see cref="Animator"/> to animate <see cref="Vector"/>.
    /// </summary>
    public class VectorAnimator : ValueAnimator<Vector>
    {
        /// <summary>
        /// Initialize new <see cref="VectorAnimator"/> instance.
        /// </summary>
        /// <param name="start">Start <see cref="Vector"/>.</param>
        /// <param name="end">End <see cref="Vector"/>.</param>
        public VectorAnimator(Vector start, Vector end) : base(start, end)
        { }


        /// <inheritdoc/>
        protected override Vector GenerateValue(double progress)
        {
            var offsetX = (this.EndValue.X - this.StartValue.X) * progress;
            var offsetY = (this.EndValue.Y - this.StartValue.Y) * progress;
            return new Vector(this.StartValue.X + offsetX, this.StartValue.Y + offsetY);
        }
    }
}
