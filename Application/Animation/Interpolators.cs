using System;

namespace CarinaStudio.Animation;

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
    /// Create Cubic Bezier interpolator.
    /// </summary>
    /// <param name="x1">X of 1st control point. The range is [0.0, 1.0].</param>
    /// <param name="y1">Y of 1st control point.</param>
    /// <param name="x2">X of 2nd control point. The range is [0.0, 1.0].</param>
    /// <param name="y2">Y of 2nd control point.</param>
    /// <returns>Cubic Bezier interpolator.</returns>
    public static Func<double, double> CreateCubicBezierInterpolator(double x1, double y1, double x2, double y2)
    {
        if (x1 < 0 || x1 > 1)
            throw new ArgumentOutOfRangeException(nameof(x1), "The valid range of X of control point is [0.0, 1.0].");
        if (x2 < 0 || x2 > 1)
            throw new ArgumentOutOfRangeException(nameof(x2), "The valid range of X of control point is [0.0, 1.0].");
        return p =>
        {
            if (p < 0)
                p = 0;
            else if (p > 1)
                p = 1;
            var minT = 0.0;
            var maxT = 1.0;
            var t = p;
            var x = CubicBezier(t, x1, x2);
            var xpDiff = (p - x);
            var remaining = 20;
            while (Math.Abs(xpDiff) > 0.001)
            {
                if (xpDiff > 0)
                    minT = t;
                else
                    maxT = t;
                t = (minT + maxT) / 2;
                x = CubicBezier(t, x1, x2);
                xpDiff = (p - x);
                --remaining;
                if (remaining <= 0)
                    break;
            }
            return CubicBezier(t, y1, y2);
        };
    }


    // Calculate value on Cubic Bezier curve based-on given parameter (time).
    static double CubicBezier(double t, double p1, double p2)
    {
        var oneMinusT = (1 - t);
        return (3 * p1 * t * oneMinusT * oneMinusT) + (3 * p2 * t * t * oneMinusT) + (t * t * t);
    }


    /// <summary>
    /// Create deceleration interpolator.
    /// </summary>
    /// <param name="factor">Factor.</param>
    /// <returns>Interpolator.</returns>
    /// <remarks>The deceleration formula is 1-(1-p)^(2*f).</remarks>
    public static Func<double, double> CreateDecelerationInterpolator(double factor) => p => 1 - Math.Pow(1 - p, 2 * factor);


    /// <summary>
    /// Create Quadratic Bezier interpolator.
    /// </summary>
    /// <param name="x">X of control point. The range is [0.0, 1.0].</param>
    /// <param name="y">Y of control point.</param>
    /// <returns>Quadratic Bezier interpolator.</returns>
    public static Func<double, double> CreateQuadraticBezierInterpolator(double x, double y)
    {
        if (x < 0 || x > 1)
            throw new ArgumentOutOfRangeException(nameof(x), "The valid range of X of control point is [0.0, 1.0].");
        return p =>
        {
            if (p < 0)
                p = 0;
            else if (p > 1)
                p = 1;
            var minT = 0.0;
            var maxT = 1.0;
            var t = p;
            var candidateX = QuadraticBezier(t, x);
            var xpDiff = (p - candidateX);
            var remaining = 20;
            while (Math.Abs(xpDiff) > 0.001)
            {
                if (xpDiff > 0)
                    minT = t;
                else
                    maxT = t;
                t = (minT + maxT) / 2;
                candidateX = QuadraticBezier(t, x);
                xpDiff = (p - candidateX);
                --remaining;
                if (remaining <= 0)
                    break;
            }
            return QuadraticBezier(t, y);
        };
    }


    // Calculate value on Quadratic Bezier curve based-on given parameter (time).
    static double QuadraticBezier(double t, double p) =>
        (2 * t * (1 - t) * p) + (t * t);
}