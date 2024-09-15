using System;

namespace CarinaStudio
{
    /// <summary>
    /// Represent a range of values.
    /// </summary>
    public readonly struct Range<T> : IEquatable<Range<T>> where T : struct, IComparable<T>
    {
        /// <summary>
        /// Empty range.
        /// </summary>
        public static readonly Range<T> Empty = new(default(T), default(T));
        /// <summary>
        /// Universal range.
        /// </summary>
        public static readonly Range<T> Universal = new(null, null);


        /// <summary>
        /// Initialize fields of <see cref="Range"/> structure,
        /// </summary>
        /// <param name="start">Start of range.</param>
        /// <param name="end">End of range.</param>
        /// <remarks><paramref name="start"/> and <paramref name="end"/> may be swapped if <paramref name="start"/> is greater than <paramref name="end"/>.</remarks>
        public Range(T? start, T? end)
        {
            if (start.HasValue)
            {
                if (start.Value is double doubleStartValue)
                {
                    if (!double.IsFinite(doubleStartValue))
                        throw new ArgumentException("Only finite value can be used for start value.");
                }
                else if (start.Value is float floatStartValue)
                {
                    if (!float.IsFinite(floatStartValue))
                        throw new ArgumentException("Only finite value can be used for start value.");
                }
                if (end.HasValue)
                {
                    if (end.Value is double doubleEndValue)
                    {
                        if (!double.IsFinite(doubleEndValue))
                            throw new ArgumentException("Only finite value can be used for start value.");
                    }
                    else if (start.Value is float floatEndValue)
                    {
                        if (!float.IsFinite(floatEndValue))
                            throw new ArgumentException("Only finite value can be used for start value.");
                    }
                    if (start.Value.CompareTo(end.Value) <= 0)
                    {
                        this.Start = start;
                        this.End = end;
                    }
                    else
                    {
                        this.Start = end;
                        this.End = start;
                    }
                }
                else
                {
                    this.Start = start;
                    this.End = null;
                }
            }
            else
            {
                this.Start = null;
                this.End = end;
            }
        }


        /// <summary>
        /// Check whether given value is contained in this range or not.
        /// </summary>
        /// <param name="value">Value.</param>
        /// <returns>True if value is contained in this range.</returns>
        public bool Contains(T value)
        {
            if (this.Start.HasValue && this.Start.Value.CompareTo(value) > 0)
                return false;
            if (this.End.HasValue && this.End.Value.CompareTo(value) <= 0)
                return false;
            return true;
        }


        /// <summary>
        /// Check whether given range is contained in this range or not.
        /// </summary>
        /// <param name="range">Range.</param>
        /// <returns>True if value is contained in this range.</returns>
        public bool Contains(Range<T> range) =>
            this.Contains(range, false);


        // Check whether given range is contained in this range or not.
        bool Contains(Range<T> range, bool includeEnd)
        {
            if (this.Start.HasValue)
            {
                if (range.Start.HasValue)
                {
                    if (this.Start.Value.CompareTo(range.Start.Value) > 0)
                        return false;
                }
                else
                    return false;  
            }
            if (this.End.HasValue)
            {
                if (range.End.HasValue)
                {
                    var result = this.End.Value.CompareTo(range.End.Value);
                    if (result < 0 || (result == 0 && !includeEnd))
                        return false;
                }
                else
                    return false;  
            }
            else if (!range.End.HasValue && !includeEnd)
                return false;
            return true;
        }


        /// <summary>
        /// Check whether given range is contained in or equivalent to this range or not.
        /// </summary>
        /// <param name="range">Range.</param>
        /// <returns>True if value is contained in or equivalent to this range.</returns>
        public bool ContainsOrEquals(Range<T> range) =>
            this.Contains(range, true);


        /// <summary>
        /// Get the exclusive end of range. Returns Null if it is an open range without end.
        /// </summary>
        public T? End { get; }


        /// <inheritdoc/>
        public bool Equals(Range<T> range) =>
            this.Start.GetValueOrDefault().Equals(range.Start.GetValueOrDefault()) 
            && this.End.GetValueOrDefault().Equals(range.End.GetValueOrDefault());


        /// <inheritdoc/>
        public override bool Equals(object? obj) =>
            obj is Range<T> range && this.Equals(range);


        /// <inheritdoc/>
        public override int GetHashCode() =>
            (this.Start.GetValueOrDefault().GetHashCode() << 16) | (this.End.GetValueOrDefault().GetHashCode() & 0xffff);
        

        /// <summary>
        /// Get the intersected range of this range and given range.
        /// </summary>
        /// <param name="range">Range.</param>
        /// <returns>Intersected range.</returns>
        public Range<T> IntersectsWith(Range<T> range)
        {
            if (this.IsClosed)
            {
                if (range.IsClosed)
                {
                    if (range.Start.GetValueOrDefault().CompareTo(this.End.GetValueOrDefault()) >= 0 || range.End.GetValueOrDefault().CompareTo(this.Start.GetValueOrDefault()) <= 0)
                        return Empty;
                    var start = Max(this.Start.GetValueOrDefault(), range.Start.GetValueOrDefault());
                    var end = Min(this.End.GetValueOrDefault(), range.End.GetValueOrDefault());
                    return new Range<T>(start, end);
                }
                else if (range.Start.HasValue)
                {
                    if (range.Start.GetValueOrDefault().CompareTo(this.End.GetValueOrDefault()) >= 0)
                        return Empty;
                    var start = Max(this.Start.GetValueOrDefault(), range.Start.GetValueOrDefault());
                    return new Range<T>(start, this.End);
                }
                else if (range.End.HasValue)
                {
                    if (range.End.GetValueOrDefault().CompareTo(this.Start.GetValueOrDefault()) <= 0)
                        return Empty;
                    var end = Min(this.End.GetValueOrDefault(), range.End.GetValueOrDefault());
                    return new Range<T>(this.Start, end);
                }
                return this;
            }
            else if (this.Start.HasValue)
            {
                if (range.IsClosed)
                {
                    if (range.End.GetValueOrDefault().CompareTo(this.Start.Value) <= 0)
                        return Empty;
                    var start = Max(this.Start.Value, range.Start.GetValueOrDefault());
                    return new Range<T>(start, range.End);
                }
                else if (range.Start.HasValue)
                {
                    var start = Max(this.Start.Value, range.Start.GetValueOrDefault());
                    return new Range<T>(start, null);
                }
                if (range.End.HasValue)
                {
                    if (range.End.Value.CompareTo(this.Start.Value) <= 0)
                        return Empty;
                    return new Range<T>(this.Start, range.End);
                }
                return this;
            }
            else if (this.End.HasValue)
            {
                if (range.IsClosed)
                {
                    if (range.Start.GetValueOrDefault().CompareTo(this.End.GetValueOrDefault()) >= 0)
                        return Empty;
                    var end = Min(this.End.GetValueOrDefault(), range.End.GetValueOrDefault());
                    return new Range<T>(range.Start, end);
                }
                if (range.Start.HasValue)
                {
                    if (range.Start.Value.CompareTo(this.End.Value) >= 0)
                        return Empty;
                    return new Range<T>(range.Start, this.End);
                }
                else if (range.End.HasValue)
                {
                    var end = Min(this.End.GetValueOrDefault(), range.End.GetValueOrDefault());
                    return new Range<T>(null, end);
                }
            }
            return range;
        }
        

        /// <summary>
        /// Check whether it is an closed range or not.
        /// </summary>
        public bool IsClosed => this.Start.HasValue && this.End.HasValue;


        /// <summary>
        /// Check whether it is an empty range or not.
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                if (!this.Start.HasValue || !this.End.HasValue)
                    return false;
                return this.Start.Value.CompareTo(this.End.Value) == 0;
            }
        }


        /// <summary>
        /// Check whether the range is intersected with given range or not.
        /// </summary>
        /// <param name="range">Range.</param>
        /// <returns>True if the range is intersected with given range.</returns>
        public bool IsIntersectedWith(Range<T> range)
        {
            if (this.IsClosed)
            {
                if (range.IsClosed)
                    return range.Start.GetValueOrDefault().CompareTo(this.End.GetValueOrDefault()) < 0 && range.End.GetValueOrDefault().CompareTo(this.Start.GetValueOrDefault()) > 0;
                else if (range.Start.HasValue)
                    return range.Start.GetValueOrDefault().CompareTo(this.End.GetValueOrDefault()) < 0;
                else if (range.End.HasValue)
                    return range.End.GetValueOrDefault().CompareTo(this.Start.GetValueOrDefault()) > 0;
                return !this.IsEmpty;
            }
            else if (this.Start.HasValue)
            {
                if (range.End.HasValue)
                    return range.End.GetValueOrDefault().CompareTo(this.Start.GetValueOrDefault()) > 0;
                return true;
            }
            else if (this.End.HasValue)
            {
                if (range.Start.HasValue)
                    return range.Start.GetValueOrDefault().CompareTo(this.End.GetValueOrDefault()) < 0;
                return true;
            }
            return !range.IsEmpty;
        }


        /// <summary>
        /// Check whether it is an open range without either start or end or both.
        /// </summary>
        public bool IsOpen => !this.Start.HasValue || !this.End.HasValue;


        /// <summary>
        /// Check whether it is a universal range which covers all values or not.
        /// </summary>
        public bool IsUniversal => !this.Start.HasValue && !this.End.HasValue;


        // Get max value.
        static T Max(T a, T b) =>
            a.CompareTo(b) >= 0 ? a : b;
        

        // Get min value.
        static T Min(T a, T b) =>
            a.CompareTo(b) <= 0 ? a : b;


        /// <summary>
        /// Get the inclusive start of range. Returns Null if it is an open range without start.
        /// </summary>
        public T? Start { get; }


        /// <summary>
        /// Equality operator.
        /// </summary>
        public static bool operator ==(Range<T> x, Range<T> y) =>
            x.Equals(y);
        

        /// <summary>
        /// Implicit type conversion operator.
        /// </summary>
        public static implicit operator Range<T>((T?, T?) range) =>
            new Range<T>(range.Item1, range.Item2);
        

        /// <summary>
        /// Implicit type conversion operator.
        /// </summary>
        public static implicit operator Range<T>((T, T) range) =>
            new Range<T>(range.Item1, range.Item2);
        

        /// <summary>
        /// Implicit type conversion operator.
        /// </summary>
        public static implicit operator (T?, T?)(Range<T> range) =>
            (range.Start, range.End);


        /// <summary>
        /// Inequality operator.
        /// </summary>
        public static bool operator !=(Range<T> x, Range<T> y) =>
            !x.Equals(y);


        /// <inheritdoc/>
        public override string ToString() =>
            $"[{this.Start}, {this.End})";
    }


    /// <summary>
    /// Extension methods for <see cref="Range{T}"/>.
    /// </summary>
    public static class RangeExtensions
    {
        /// <summary>
        /// Get length of the range.
        /// </summary>
        /// <param name="range">Range.</param>
        /// <returns>Length of  the range.</returns>
        public static int Length(this Range<byte> range)
        {
            if (range.Start.HasValue && range.End.HasValue)
                return range.End.Value - range.Start.Value;
            throw new ArgumentException("Cannot get length of open range.");
        }


        /// <summary>
        /// Get length of the range.
        /// </summary>
        /// <param name="range">Range.</param>
        /// <returns>Length of  the range.</returns>
        public static TimeSpan Length(this Range<DateTime> range)
        {
            if (range.Start.HasValue && range.End.HasValue)
                return range.End.Value - range.Start.Value;
            throw new ArgumentException("Cannot get length of open range.");
        }


        /// <summary>
        /// Get length of the range.
        /// </summary>
        /// <param name="range">Range.</param>
        /// <returns>Length of  the range.</returns>
        public static double Length(this Range<double> range)
        {
            if (range.Start.HasValue && range.End.HasValue)
                return range.End.Value - range.Start.Value;
            throw new ArgumentException("Cannot get length of open range.");
        }


        /// <summary>
        /// Get length of the range.
        /// </summary>
        /// <param name="range">Range.</param>
        /// <returns>Length of  the range.</returns>
        public static float Length(this Range<float> range)
        {
            if (range.Start.HasValue && range.End.HasValue)
                return range.End.Value - range.Start.Value;
            throw new ArgumentException("Cannot get length of open range.");
        }


        /// <summary>
        /// Get length of the range.
        /// </summary>
        /// <param name="range">Range.</param>
        /// <returns>Length of  the range.</returns>
        public static uint Length(this Range<int> range)
        {
            if (range.Start.HasValue && range.End.HasValue)
                return (uint)(range.End.Value - (long)range.Start.Value);
            throw new ArgumentException("Cannot get length of open range.");
        }


        /// <summary>
        /// Get length of the range.
        /// </summary>
        /// <param name="range">Range.</param>
        /// <returns>Length of  the range.</returns>
        public static ulong Length(this Range<long> range)
        {
            if (range.Start.HasValue && range.End.HasValue)
            {
                var start = range.Start.Value;
                var end = range.End.Value;
                if (start >= 0L)
                    return (ulong)end - (ulong)start;
                if (end <= 0L)
                    return (ulong)-start - (ulong)-end;
                return (ulong)-start + (ulong)end;
            }
            throw new ArgumentException("Cannot get length of open range.");
        }


        /// <summary>
        /// Get length of the range.
        /// </summary>
        /// <param name="range">Range.</param>
        /// <returns>Length of  the range.</returns>
        public static int Length(this Range<sbyte> range)
        {
            if (range.Start.HasValue && range.End.HasValue)
                return range.End.Value - range.Start.Value;
            throw new ArgumentException("Cannot get length of open range.");
        }


        /// <summary>
        /// Get length of the range.
        /// </summary>
        /// <param name="range">Range.</param>
        /// <returns>Length of  the range.</returns>
        public static int Length(this Range<short> range)
        {
            if (range.Start.HasValue && range.End.HasValue)
                return range.End.Value - range.Start.Value;
            throw new ArgumentException("Cannot get length of open range.");
        }


        /// <summary>
        /// Get length of the range.
        /// </summary>
        /// <param name="range">Range.</param>
        /// <returns>Length of  the range.</returns>
        public static uint Length(this Range<uint> range)
        {
            if (range.Start.HasValue && range.End.HasValue)
                return range.End.Value - range.Start.Value;
            throw new ArgumentException("Cannot get length of open range.");
        }


        /// <summary>
        /// Get length of the range.
        /// </summary>
        /// <param name="range">Range.</param>
        /// <returns>Length of  the range.</returns>
        public static ulong Length(this Range<ulong> range)
        {
            if (range.Start.HasValue && range.End.HasValue)
                return range.End.Value - range.Start.Value;
            throw new ArgumentException("Cannot get length of open range.");
        }


        /// <summary>
        /// Get length of the range.
        /// </summary>
        /// <param name="range">Range.</param>
        /// <returns>Length of  the range.</returns>
        public static int Length(this Range<ushort> range)
        {
            if (range.Start.HasValue && range.End.HasValue)
                return range.End.Value - range.Start.Value;
            throw new ArgumentException("Cannot get length of open range.");
        }


        /// <summary>
        /// Offset the range.
        /// </summary>
        /// <param name="range">Range.</param>
        /// <param name="offset">Offset.</param>
        /// <returns>New range.</returns>
        public static Range<byte> Offsets(this Range<byte> range, int offset)
        {
            if (offset == 0)
                return range;
            var start = range.Start?.Let(it =>
            {
                if (offset > 0)
                {
                    if (offset > byte.MaxValue - it)
                        throw new OverflowException();
                }
                else
                {
                    if (offset < byte.MinValue - it)
                        throw new OverflowException();
                }
                return it + offset;
            });
            var end = range.End?.Let(it =>
            {
                if (offset > 0)
                {
                    if (offset > byte.MaxValue - it)
                        throw new OverflowException();
                }
                else
                {
                    if (offset < byte.MinValue - it)
                        throw new OverflowException();
                }
                return it + offset;
            });
            return new Range<byte>((byte?)start, (byte?)end);
        }


        /// <summary>
        /// Offset the range.
        /// </summary>
        /// <param name="range">Range.</param>
        /// <param name="offset">Offset.</param>
        /// <returns>New range.</returns>
        public static Range<DateTime> Offsets(this Range<DateTime> range, TimeSpan offset)
        {
            var start = range.Start + offset;
            var end = range.End + offset;
            return new Range<DateTime>(start, end);
        }


        /// <summary>
        /// Offset the range.
        /// </summary>
        /// <param name="range">Range.</param>
        /// <param name="offset">Offset.</param>
        /// <returns>New range.</returns>
        public static Range<double> Offsets(this Range<double> range, double offset)
        {
            var start = range.Start + offset;
            var end = range.End + offset;
            return new Range<double>(start, end);
        }


        /// <summary>
        /// Offset the range.
        /// </summary>
        /// <param name="range">Range.</param>
        /// <param name="offset">Offset.</param>
        /// <returns>New range.</returns>
        public static Range<float> Offsets(this Range<float> range, float offset)
        {
            var start = range.Start + offset;
            var end = range.End + offset;
            return new Range<float>(start, end);
        }


        /// <summary>
        /// Offset the range.
        /// </summary>
        /// <param name="range">Range.</param>
        /// <param name="offset">Offset.</param>
        /// <returns>New range.</returns>
        public static Range<int> Offsets(this Range<int> range, long offset)
        {
            if (offset == 0)
                return range;
            var start = range.Start?.Let(it =>
            {
                if (offset > 0)
                {
                    if (offset > (long)int.MaxValue - it)
                        throw new OverflowException();
                }
                else
                {
                    if (offset < (long)int.MinValue - it)
                        throw new OverflowException();
                }
                return it + offset;
            });
            var end = range.End?.Let(it =>
            {
                if (offset > 0)
                {
                    if (offset > (long)int.MaxValue - it)
                        throw new OverflowException();
                }
                else
                {
                    if (offset < (long)int.MinValue - it)
                        throw new OverflowException();
                }
                return it + offset;
            });
            return new Range<int>((int?)start, (int?)end);
        }


        /// <summary>
        /// Offset the range.
        /// </summary>
        /// <param name="range">Range.</param>
        /// <param name="offset">Offset.</param>
        /// <returns>New range.</returns>
        public static Range<long> Offsets(this Range<long> range, long offset)
        {
            if (offset == 0)
                return range;
            var start = range.Start?.Let(it =>
            {
                if (offset > 0)
                {
                    if (it >= 0 && offset > long.MaxValue - it)
                        throw new OverflowException();
                }
                else
                {
                    if (it < 0 && offset < long.MinValue - it)
                        throw new OverflowException();
                }
                return it + offset;
            });
            var end = range.End?.Let(it =>
            {
                if (offset > 0)
                {
                    if (it >= 0 && offset > long.MaxValue - it)
                        throw new OverflowException();
                }
                else
                {
                    if (it < 0 && offset < long.MinValue - it)
                        throw new OverflowException();
                }
                return it + offset;
            });
            return new Range<long>(start, end);
        }


        /// <summary>
        /// Offset the range.
        /// </summary>
        /// <param name="range">Range.</param>
        /// <param name="offset">Offset.</param>
        /// <returns>New range.</returns>
        public static Range<sbyte> Offsets(this Range<sbyte> range, int offset)
        {
            if (offset == 0)
                return range;
            var start = range.Start?.Let(it =>
            {
                if (offset > 0)
                {
                    if (offset > sbyte.MaxValue - it)
                        throw new OverflowException();
                }
                else
                {
                    if (offset < sbyte.MinValue - it)
                        throw new OverflowException();
                }
                return it + offset;
            });
            var end = range.End?.Let(it =>
            {
                if (offset > 0)
                {
                    if (offset > sbyte.MaxValue - it)
                        throw new OverflowException();
                }
                else
                {
                    if (offset < sbyte.MinValue - it)
                        throw new OverflowException();
                }
                return it + offset;
            });
            return new Range<sbyte>((sbyte?)start, (sbyte?)end);
        }


        /// <summary>
        /// Offset the range.
        /// </summary>
        /// <param name="range">Range.</param>
        /// <param name="offset">Offset.</param>
        /// <returns>New range.</returns>
        public static Range<short> Offsets(this Range<short> range, int offset)
        {
            if (offset == 0)
                return range;
            var start = range.Start?.Let(it =>
            {
                if (offset > 0)
                {
                    if (offset > short.MaxValue - it)
                        throw new OverflowException();
                }
                else
                {
                    if (offset < short.MinValue - it)
                        throw new OverflowException();
                }
                return it + offset;
            });
            var end = range.End?.Let(it =>
            {
                if (offset > 0)
                {
                    if (offset > short.MaxValue - it)
                        throw new OverflowException();
                }
                else
                {
                    if (offset < short.MinValue - it)
                        throw new OverflowException();
                }
                return it + offset;
            });
            return new Range<short>((short?)start, (short?)end);
        }


        /// <summary>
        /// Offset the range.
        /// </summary>
        /// <param name="range">Range.</param>
        /// <param name="offset">Offset.</param>
        /// <returns>New range.</returns>
        public static Range<uint> Offsets(this Range<uint> range, long offset)
        {
            if (offset == 0)
                return range;
            var start = range.Start?.Let(it =>
            {
                if (offset > 0)
                {
                    if (offset > uint.MaxValue - it)
                        throw new OverflowException();
                }
                else
                {
                    if (offset < uint.MinValue - it)
                        throw new OverflowException();
                }
                return it + offset;
            });
            var end = range.End?.Let(it =>
            {
                if (offset > 0)
                {
                    if (offset > uint.MaxValue - it)
                        throw new OverflowException();
                }
                else
                {
                    if (offset < uint.MinValue - it)
                        throw new OverflowException();
                }
                return it + offset;
            });
            return new Range<uint>((uint?)start, (uint?)end);
        }


        /// <summary>
        /// Offset the range.
        /// </summary>
        /// <param name="range">Range.</param>
        /// <param name="offset">Offset.</param>
        /// <returns>New range.</returns>
        public static Range<ulong> Offsets(this Range<ulong> range, long offset)
        {
            if (offset == 0)
                return range;
            var start = range.Start?.Let(it =>
            {
                if (offset > 0)
                {
                    if ((ulong)offset > ulong.MaxValue - it)
                        throw new OverflowException();
                    return it + (ulong)offset;
                }
                else
                {
                    if (it < (long.MaxValue + 1ul) && (long)it < -offset)
                        throw new OverflowException();
                    return it - (ulong)-offset;
                }
            });
            var end = range.End?.Let(it =>
            {
                if (offset > 0)
                {
                    if ((ulong)offset > ulong.MaxValue - it)
                        throw new OverflowException();
                    return it + (ulong)offset;
                }
                else
                {
                    if (it < (long.MaxValue + 1ul) && (long)it < -offset)
                        throw new OverflowException();
                    return it - (ulong)-offset;
                }
            });
            return new Range<ulong>(start, end);
        }


        /// <summary>
        /// Offset the range.
        /// </summary>
        /// <param name="range">Range.</param>
        /// <param name="offset">Offset.</param>
        /// <returns>New range.</returns>
        public static Range<ushort> Offsets(this Range<ushort> range, int offset)
        {
            if (offset == 0)
                return range;
            var start = range.Start?.Let(it =>
            {
                if (offset > 0)
                {
                    if (offset > ushort.MaxValue - it)
                        throw new OverflowException();
                }
                else
                {
                    if (offset < ushort.MinValue - it)
                        throw new OverflowException();
                }
                return it + offset;
            });
            var end = range.End?.Let(it =>
            {
                if (offset > 0)
                {
                    if (offset > ushort.MaxValue - it)
                        throw new OverflowException();
                }
                else
                {
                    if (offset < ushort.MinValue - it)
                        throw new OverflowException();
                }
                return it + offset;
            });
            return new Range<ushort>((ushort?)start, (ushort?)end);
        }


        /// <summary>
        /// Create range from this value.
        /// </summary>
        /// <param name="value">Value.</param>
        /// <param name="another">Another value.</param>
        public static Range<T> ToRange<T>(this T value, T another) where T : struct, IComparable<T> =>
            new Range<T>(value, another);
    }
}