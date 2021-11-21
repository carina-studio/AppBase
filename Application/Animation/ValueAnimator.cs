using System;

namespace CarinaStudio.Animation
{
    /// <summary>
    /// Generic animator which animates specific type of value.
    /// </summary>
    /// <typeparam name="T">Type of value to animate.</typeparam>
    public abstract class ValueAnimator<T> : Animator
    {
        /// <summary>
        /// Initialize new <see cref="ValueAnimator{T}"/> instance.
        /// </summary>
        protected ValueAnimator() => this.Value = this.GenerateValue(0);


        /// <summary>
        /// Generate value by progress of animation.
        /// </summary>
        /// <param name="progress">Progress.</param>
        /// <returns>Value.</returns>
        protected abstract T GenerateValue(double progress);


        /// <inheritdoc/>
        protected override void OnCancelled(EventArgs e)
        {
            this.Value = this.GenerateValue(this.Progress);
            base.OnCancelled(e);
        }


        /// <inheritdoc/>
        protected override void OnCompleted(EventArgs e)
        {
            this.Value = this.GenerateValue(this.Progress);
            base.OnCompleted(e);
        }


        /// <inheritdoc/>
        protected override void OnProgressChanged(EventArgs e)
        {
            this.Value = this.GenerateValue(this.Progress);
            base.OnProgressChanged(e);
        }


        /// <summary>
        /// Get current value when animating.
        /// </summary>
        public T Value { get; private set; }
    }
}
