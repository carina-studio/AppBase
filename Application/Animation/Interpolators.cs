using System;

namespace CarinaStudio.Animation
{
    /// <summary>
    /// Predefined animation interpolators.
    /// </summary>
    public static class Interpolators
    {
        /// <summary>
        /// Acceleration with factor 1.
        /// </summary>
        public static readonly Func<double, double> Acceleration = CreateAccelerationInterpolator(1);
        /// <summary>
        /// Default intepolator which outputs the input value directly.
        /// </summary>
        public static readonly Func<double, double> Default = p => p;
        /// <summary>
        /// Deceleration with factor 1.
        /// </summary>
        public static readonly Func<double, double> Deleceleration = CreateDecelerationInterpolator(1);
        /// <summary>
        /// Intepolator which inverts the input value.
        /// </summary>
        public static readonly Func<double, double> Inverted = p => (1 - p);


        /// <summary>
        /// Create acceleration interpolator.
        /// </summary>
        /// <param name="factor">Factor.</param>
        /// <returns>Interpolator.</returns>
        /// <remarks>The acceleration formula is p^(2*f).</remarks>
        public static Func<double, double> CreateAccelerationInterpolator(double factor) => p => Math.Pow(p, 2 * factor);


        /// <summary>
        /// Create deceleration interpolator.
        /// </summary>
        /// <param name="factor">Factor.</param>
        /// <returns>Interpolator.</returns>
        /// <remarks>The deceleration formula is 1-(1-p)^(2*f).</remarks>
        public static Func<double, double> CreateDecelerationInterpolator(double factor) => p => 1 - Math.Pow(1 - p, 2 * factor);
    }
}
