
namespace CarinaStudio.Animation
{
    /// <summary>
    /// <see cref="ValueAnimator{T}"/> to animate <see cref="double"/>.
    /// </summary>
    public class DoubleAnimator : ValueAnimator<double>
    {
        /// <summary>
        /// Initialize new <see cref="DoubleAnimator"/> instance.
        /// </summary>
        /// <param name="start"><see cref="double"/> for start of animation.</param>
        /// <param name="end"><see cref="double"/> for end of animation.</param>
        public DoubleAnimator(double start, double end) : base(start, end)
        { }


        /// <inheritdoc/>
        protected override double GenerateValue(double progress) =>
            this.StartValue + (this.EndValue - this.StartValue) * progress;
    }
}
