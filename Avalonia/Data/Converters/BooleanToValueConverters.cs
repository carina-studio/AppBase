using System;

namespace CarinaStudio.Data.Converters
{
    /// <summary>
    /// Predefined <see cref="BooleanToValueConverter{TValue}"/>s.
    /// </summary>
    public static class BooleanToValueConverters
    {
        /// <summary>
        /// Convert from <see cref="bool"/> to 0.0 (False) and 1.0 (True).
        /// </summary>
        public static readonly BooleanToValueConverter<double> BooleanToOpacity = new BooleanToValueConverter<double>(1.0, 0.0, (x, y) => Math.Abs(x - y) < 0.01);
        /// <summary>
        /// Convert from <see cref="bool"/> to 1.0 (False) and 0.0 (True).
        /// </summary>
        public static readonly BooleanToValueConverter<double> BooleanToOpacityInverted = new BooleanToValueConverter<double>(0.0, 1.0, (x, y) => Math.Abs(x - y) < 0.01);
    }
}
