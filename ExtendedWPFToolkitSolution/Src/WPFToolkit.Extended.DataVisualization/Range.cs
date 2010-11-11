// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.Globalization;

namespace System.Windows.Controls.DataVisualization
{
    /// <summary>
    /// A range of values.
    /// </summary>
    /// <typeparam name="T">The type of the values.</typeparam>
    /// <QualityBand>Preview</QualityBand>
    public struct Range<T>
        where T : IComparable
    {
        /// <summary>
        /// A flag that determines whether the range is empty or not.
        /// </summary>
        private bool _hasData;

        /// <summary>
        /// Gets a value indicating whether the range is empty or not.
        /// </summary>
        public bool HasData
        {
            get
            {
                return _hasData;
            }
        }

        /// <summary>
        /// The maximum value in the range.
        /// </summary>
        private T _maximum;

        /// <summary>
        /// Gets the maximum value in the range.
        /// </summary>
        public T Maximum
        {
            get
            {
                if (!HasData)
                {
                    throw new InvalidOperationException(Properties.Resources.Range_get_Maximum_CannotReadTheMaximumOfAnEmptyRange);
                }
                return _maximum;
            }
        }

        /// <summary>
        /// The minimum value in the range.
        /// </summary>
        private T _minimum;

        /// <summary>
        /// Gets the minimum value in the range.
        /// </summary>
        public T Minimum
        {
            get
            {
                if (!HasData)
                {
                    throw new InvalidOperationException(Properties.Resources.Range_get_Minimum_CannotReadTheMinimumOfAnEmptyRange);
                }
                return _minimum;
            }
        }

        /// <summary>
        /// Initializes a new instance of the Range class.
        /// </summary>
        /// <param name="minimum">The minimum value.</param>
        /// <param name="maximum">The maximum value.</param>
        public Range(T minimum, T maximum)
        {
            if (minimum == null)
            {
                throw new ArgumentNullException("minimum");
            }
            if (maximum == null)
            {
                throw new ArgumentNullException("maximum");
            }

            _hasData = true;
            _minimum = minimum;
            _maximum = maximum;

            int compareValue = ValueHelper.Compare(minimum, maximum);
            if (compareValue == 1)
            {
                throw new InvalidOperationException(Properties.Resources.Range_ctor_MaximumValueMustBeLargerThanOrEqualToMinimumValue);
            }
        }

        /// <summary>
        /// Compare two ranges and return a value indicating whether they are
        /// equal.
        /// </summary>
        /// <param name="leftRange">Left-hand side range.</param>
        /// <param name="rightRange">Right-hand side range.</param>
        /// <returns>A value indicating whether the ranges are equal.</returns>
        public static bool operator ==(Range<T> leftRange, Range<T> rightRange)
        {
            if (!leftRange.HasData)
            {
                return !rightRange.HasData;
            }
            if (!rightRange.HasData)
            {
                return !leftRange.HasData;
            }

            return leftRange.Minimum.Equals(rightRange.Minimum) && leftRange.Maximum.Equals(rightRange.Maximum);
        }

        /// <summary>
        /// Compare two ranges and return a value indicating whether they are
        /// not equal.
        /// </summary>
        /// <param name="leftRange">Left-hand side range.</param>
        /// <param name="rightRange">Right-hand side range.</param>
        /// <returns>A value indicating whether the ranges are not equal.
        /// </returns>
        public static bool operator !=(Range<T> leftRange, Range<T> rightRange)
        {
            return !(leftRange == rightRange);
        }

        /// <summary>
        /// Adds a range to the current range.
        /// </summary>
        /// <param name="range">A range to add to the current range.</param>
        /// <returns>A new range that encompasses the instance range and the
        /// range parameter.</returns>
        public Range<T> Add(Range<T> range)
        {
            if (!this.HasData)
            {
                return range;
            }
            else if (!range.HasData)
            {
                return this;
            }
            T minimum = ValueHelper.Compare(this.Minimum, range.Minimum) == -1 ? this.Minimum : range.Minimum;
            T maximum = ValueHelper.Compare(this.Maximum, range.Maximum) == 1 ? this.Maximum : range.Maximum;
            return new Range<T>(minimum, maximum);
        }

        /// <summary>
        /// Compares the range to another range.
        /// </summary>
        /// <param name="range">A different range.</param>
        /// <returns>A value indicating whether the ranges are equal.</returns>
        public bool Equals(Range<T> range)
        {
            return this == range;
        }

        /// <summary>
        /// Compares the range to an object.
        /// </summary>
        /// <param name="obj">Another object.</param>
        /// <returns>A value indicating whether the other object is a range,
        /// and if so, whether that range is equal to the instance range.
        /// </returns>
        public override bool Equals(object obj)
        {
            Range<T> range = (Range<T>)obj;
            if (range == null)
            {
                return false;
            }
            return this == range;
        }

        /// <summary>
        /// Returns a value indicating whether a value is within a range.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>Whether the value is within the range.</returns>
        public bool Contains(T value)
        {
            return ValueHelper.Compare(Minimum, value) <= 0 && ValueHelper.Compare(value, Maximum) <= 0;
        }

        /////// <summary>
        /////// Returns a new range that contains the value.
        /////// </summary>
        /////// <param name="value">The value to extend the range to.</param>
        /////// <returns>The range which contains the value.</returns>
        ////public Range<T> ExtendTo(T value)
        ////{
        ////    if (!HasData)
        ////    {
        ////        return new Range<T>(value, value);
        ////    }

        ////    if (ValueHelper.Compare(Minimum, value) > 0)
        ////    {
        ////        return new Range<T>(value, Maximum);
        ////    }
        ////    else if (ValueHelper.Compare(Maximum, value) < 0)
        ////    {
        ////        return new Range<T>(Minimum, value);
        ////    }

        ////    return this;
        ////}

        /// <summary>
        /// Returns a value indicating whether two ranges intersect.
        /// </summary>
        /// <param name="range">The range to compare against this range.</param>
        /// <returns>A value indicating whether the ranges intersect.</returns>
        public bool IntersectsWith(Range<T> range)
        {
            if (!this.HasData || !range.HasData)
            {
                return false;
            }

            Func<Range<T>, Range<T>, bool> rightCollidesWithLeft =
                (leftRange, rightRange) =>
                    (ValueHelper.Compare(rightRange.Minimum, leftRange.Maximum) <= 0 && ValueHelper.Compare(rightRange.Minimum, leftRange.Minimum) >= 0)
                    || (ValueHelper.Compare(leftRange.Minimum, rightRange.Maximum) <= 0 && ValueHelper.Compare(leftRange.Minimum, rightRange.Minimum) >= 0);

            return rightCollidesWithLeft(this, range) || rightCollidesWithLeft(range, this);
        }

        /// <summary>
        /// Computes a hash code value.
        /// </summary>
        /// <returns>A hash code value.</returns>
        public override int GetHashCode()
        {
            if (!HasData)
            {
                return 0;
            }

            int num = 0x5374e861;
            num = (-1521134295 * num) + EqualityComparer<T>.Default.GetHashCode(Minimum);
            return ((-1521134295 * num) + EqualityComparer<T>.Default.GetHashCode(Maximum));
        }

        /// <summary>
        /// Returns the string representation of the range.
        /// </summary>
        /// <returns>The string representation of the range.</returns>
        public override string ToString()
        {
            if (!this.HasData)
            {
                return Properties.Resources.Range_ToString_NoData;
            }
            else
            {
                return string.Format(CultureInfo.CurrentCulture, Properties.Resources.Range_ToString_Data, this.Minimum, this.Maximum);
            }
        }
    }
}
