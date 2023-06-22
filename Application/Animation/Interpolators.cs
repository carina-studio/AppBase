using System;

namespace CarinaStudio.Animation
{
    /// <summary>
    /// Predefined animation interpolators.
    /// </summary>
    public static class Interpolators
    {
        /// <summary>
        /// Acceleration with factor 2.
        /// </summary>
        public static readonly Func<double, double> Acceleration = CreateAccelerationInterpolator(2);
        /// <summary>
        /// Default interpolator which outputs the input value directly.
        /// </summary>
        public static readonly Func<double, double> Default = p => p;
        /// <summary>
        /// Deceleration with factor 2.
        /// </summary>
        public static readonly Func<double, double> Deceleration = CreateDecelerationInterpolator(2);
        /// <summary>
        /// Acceleration with factor 3.
        /// </summary>
        public static readonly Func<double, double> FastAcceleration = CreateAccelerationInterpolator(3);
        /// <summary>
        /// Deceleration with factor 3.
        /// </summary>
        public static readonly Func<double, double> FastDeceleration = CreateDecelerationInterpolator(3);
        /// <summary>
        /// Interpolator which inverts the input value.
        /// </summary>
        public static readonly Func<double, double> Inverted = p => (1 - p);
        /// <summary>
        /// Acceleration with factor 1.
        /// </summary>
        public static readonly Func<double, double> SlowAcceleration = CreateAccelerationInterpolator(1);
        /// <summary>
        /// Deceleration with factor 1.
        /// </summary>
        public static readonly Func<double, double> SlowDeceleration = CreateDecelerationInterpolator(1);


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
