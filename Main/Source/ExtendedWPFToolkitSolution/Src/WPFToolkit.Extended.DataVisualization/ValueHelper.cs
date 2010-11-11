// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Windows;

namespace System.Windows.Controls.DataVisualization
{
    /// <summary>
    /// A set of functions for data conversion operations.
    /// </summary>
    internal static class ValueHelper
    {
        /// <summary>
        /// The value of a single radian.
        /// </summary>
        public const double Radian = Math.PI / 180.0;

        /// <summary>
        /// Returns a value indicating whether this value can be graphed on a 
        /// linear axis.
        /// </summary>
        /// <param name="value">The value to evaluate.</param>
        /// <returns>A value indicating whether this value can be graphed on a 
        /// linear axis.</returns>
        public static bool CanGraph(double value)
        {
            return !double.IsNaN(value) && !double.IsNegativeInfinity(value) && !double.IsPositiveInfinity(value) && !double.IsInfinity(value);
        }

        /// <summary>
        /// Attempts to convert an object into a double.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <param name="doubleValue">The double value.</param>
        /// <returns>A value indicating whether the value can be converted to a 
        /// double.</returns>
        public static bool TryConvert(object value, out double doubleValue)
        {
            doubleValue = default(double);
            try
            {
                if (value != null && 
                    (value is double
                    || value is int
                    || value is byte
                    || value is short
                    || value is decimal
                    || value is float
                    || value is long
                    || value is uint
                    || value is sbyte
                    || value is ushort
                    || value is ulong))
                {
                    doubleValue = ValueHelper.ToDouble(value);
                    return true;
                }
            }
            catch (FormatException)
            {
            }
            catch (InvalidCastException)
            {
            }
            return false;
        }

        /// <summary>
        /// Attempts to convert an object into a date time.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <param name="dateTimeValue">The double value.</param>
        /// <returns>A value indicating whether the value can be converted to a 
        /// date time.</returns>
        public static bool TryConvert(object value, out DateTime dateTimeValue)
        {
            dateTimeValue = default(DateTime);
            if (value != null && value is DateTime)
            {
                dateTimeValue = (DateTime)value;
                return true;
            }

            return false;
        }

        /////// <summary>
        /////// Converts a value in an IComparable.
        /////// </summary>
        /////// <param name="value">The value to convert.</param>
        /////// <returns>The converted value.</returns>
        ////public static IComparable ToComparable(object value)
        ////{
        ////    double doubleValue;
        ////    DateTime dateTimeValue;
        ////    if (TryConvert(value, out doubleValue))
        ////    {
        ////        return doubleValue;
        ////    }
        ////    else if (TryConvert(value, out dateTimeValue))
        ////    {
        ////        return dateTimeValue;
        ////    }
        ////    IComparable comparable = value as IComparable;
        ////    return (comparable != null);
        ////}

        /// <summary>
        /// Converts an object into a double.
        /// </summary>
        /// <param name="value">The value to convert to a double.</param>
        /// <returns>The converted double value.</returns>
        public static double ToDouble(object value)
        {
            return Convert.ToDouble(value, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Converts a value to a date.
        /// </summary>
        /// <param name="value">The value to convert to a date.</param>
        /// <returns>The converted date value.</returns>
        public static DateTime ToDateTime(object value)
        {
            return Convert.ToDateTime(value, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Returns a sequence of date time values from a start and end date 
        /// time inclusive.
        /// </summary>
        /// <param name="start">The start date time.</param>
        /// <param name="end">The end date time.</param>
        /// <param name="count">The number of values to return.</param>
        /// <returns>A sequence of date time values.</returns>
        public static IEnumerable<DateTime> GetDateTimesBetweenInclusive(DateTime start, DateTime end, long count)
        {
            Debug.Assert(count >= 2L, "Count must be at least 2.");

            return GetIntervalsInclusive(start.Ticks, end.Ticks, count).Select(value => new DateTime(value));
        }

        /// <summary>
        /// Returns a sequence of time span values within a time span inclusive.
        /// </summary>
        /// <param name="timeSpan">The time span to split.</param>
        /// <param name="count">The number of time spans to return.</param>
        /// <returns>A sequence of time spans.</returns>
        public static IEnumerable<TimeSpan> GetTimeSpanIntervalsInclusive(TimeSpan timeSpan, long count)
        {
            Debug.Assert(count >= 2L, "Count must be at least 2.");

            long distance = timeSpan.Ticks;

            return GetIntervalsInclusive(0, distance, count).Select(value => new TimeSpan(value));
        }

        /// <summary>
        /// Returns that intervals between a start and end value, including those
        /// start and end values.
        /// </summary>
        /// <param name="start">The start value.</param>
        /// <param name="end">The end value.</param>
        /// <param name="count">The total number of intervals.</param>
        /// <returns>A sequence of intervals.</returns>
        public static IEnumerable<long> GetIntervalsInclusive(long start, long end, long count)
        {
            Debug.Assert(count >= 2L, "Count must be at least 2.");

            long interval = end - start;
            for (long index = 0; index < count; index++)
            {
                double ratio = (double)index / (double)(count - 1);
                long value = (long)((ratio * interval) + start);
                yield return value;
            }
        }

        /// <summary>
        /// Removes the noise from double math.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>A double without a noise.</returns>
        internal static double RemoveNoiseFromDoubleMath(double value)
        {
            if (value == 0.0 || Math.Abs((Math.Log10(Math.Abs(value)))) < 27)
            {
                return (double)((decimal)value);
            }
            return Double.Parse(value.ToString(CultureInfo.InvariantCulture), CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Converts a range into a double range.
        /// </summary>
        /// <param name="range">The range to convert.</param>
        /// <returns>A range with its members converted to doubles.</returns>
        public static Range<double> ToDoubleRange(this Range<IComparable> range)
        {
            if (!range.HasData)
            {
                return new Range<double>();
            }
            else
            {
                return new Range<double>((double)range.Minimum, (double)range.Maximum);
            }
        }

        /// <summary>
        /// Converts a range into a date time range.
        /// </summary>
        /// <param name="range">The range to convert.</param>
        /// <returns>A range with its members converted to date times.
        /// </returns>
        public static Range<DateTime> ToDateTimeRange(this Range<IComparable> range)
        {
            if (!range.HasData)
            {
                return new Range<DateTime>();
            }
            else
            {
                return new Range<DateTime>((DateTime)range.Minimum, (DateTime)range.Maximum);
            }
        }

        /////// <summary>
        /////// Returns the point given an angle and a distanceFromOrigin.
        /////// </summary>
        /////// <param name="angle">The angle of orientation.</param>
        /////// <param name="distanceFromOrigin">The radius.</param>
        /////// <returns>The point calculated from the angle and radius.</returns>
        ////public static Point GetPoint(double angle, double distanceFromOrigin)
        ////{
        ////    return new Point(Math.Cos(angle * Radian) * distanceFromOrigin, Math.Sin(angle * Radian) * distanceFromOrigin);
        ////}

        /// <summary>
        /// Compares two IComparables returning -1 if the left is null and 1 if
        /// the right is null.
        /// </summary>
        /// <param name="left">The left comparable.</param>
        /// <param name="right">The right comparable.</param>
        /// <returns>A value indicating which is larger.</returns>
        public static int Compare(IComparable left, IComparable right)
        {
            if (left == null && right == null)
            {
                return 0;
            }
            else if (left == null && right != null)
            {
                return -1;
            }
            else if (left != null && right == null)
            {
                return 1;
            }
            else
            {
                return left.CompareTo(right);
            }    
        }

        /// <summary>
        /// Applies the translate transform to a point.
        /// </summary>
        /// <param name="origin">The origin point.</param>
        /// <param name="offset">The offset point.</param>
        /// <returns>The translated point.</returns>
        public static Point Translate(this Point origin, Point offset)
        {
            return new Point(origin.X + offset.X, origin.Y + offset.Y);
        }

        /// <summary>
        /// Converts any range to a range of IComparable.
        /// </summary>
        /// <param name="range">The range to be converted.</param>
        /// <returns>The new range type.</returns>
        public static Range<IComparable> ToComparableRange(this Range<double> range)
        {
            if (range.HasData)
            {
                return new Range<IComparable>(range.Minimum, range.Maximum);
            }
            else
            {
                return new Range<IComparable>();
            }
        }

        /// <summary>
        /// Returns the left value of the rectangle.
        /// </summary>
        /// <param name="rectangle">The rectangle.</param>
        /// <param name="value">The default value.</param>
        /// <returns>The left value of the rectangle.</returns>
        public static double LeftOrDefault(this Rect rectangle, double value)
        {
            return rectangle.IsEmpty ? value : rectangle.Left;
        }

        /// <summary>
        /// Returns the right value of the rectangle.
        /// </summary>
        /// <param name="rectangle">The rectangle.</param>
        /// <param name="value">The default value.</param>
        /// <returns>The right value of the rectangle.</returns>
        public static double RightOrDefault(this Rect rectangle, double value)
        {
            return rectangle.IsEmpty ? value : rectangle.Right;
        }

        /// <summary>
        /// Returns the width value of the rectangle.
        /// </summary>
        /// <param name="rectangle">The rectangle.</param>
        /// <param name="value">The default value.</param>
        /// <returns>The width value of the rectangle.</returns>
        public static double WidthOrDefault(this Rect rectangle, double value)
        {
            return rectangle.IsEmpty ? value : rectangle.Width;
        }

        /// <summary>
        /// Returns the height value of the rectangle.
        /// </summary>
        /// <param name="rectangle">The rectangle.</param>
        /// <param name="value">The default value.</param>
        /// <returns>The height value of the rectangle.</returns>
        public static double HeightOrDefault(this Rect rectangle, double value)
        {
            return rectangle.IsEmpty ? value : rectangle.Height;
        }

        /// <summary>
        /// Returns the bottom value of the rectangle.
        /// </summary>
        /// <param name="rectangle">The rectangle.</param>
        /// <param name="value">The default value.</param>
        /// <returns>The bottom value of the rectangle.</returns>
        public static double BottomOrDefault(this Rect rectangle, double value)
        {
            return rectangle.IsEmpty ? value : rectangle.Bottom;
        }

        /// <summary>
        /// Returns the top value of the rectangle.
        /// </summary>
        /// <param name="rectangle">The rectangle.</param>
        /// <param name="value">The default value.</param>
        /// <returns>The top value of the rectangle.</returns>
        public static double TopOrDefault(this Rect rectangle, double value)
        {
            return rectangle.IsEmpty ? value : rectangle.Top;
        }

        /// <summary>
        /// Converts any range to a range of IComparable.
        /// </summary>
        /// <param name="range">The range to be converted.</param>
        /// <returns>The new range type.</returns>
        public static Range<IComparable> ToComparableRange(this Range<DateTime> range)
        {
            if (range.HasData)
            {
                return new Range<IComparable>(range.Minimum, range.Maximum);
            }
            else
            {
                return new Range<IComparable>();
            }
        }

        /// <summary>
        /// Returns the time span of a date range.
        /// </summary>
        /// <param name="range">The range of values.</param>
        /// <returns>The length of the range.</returns>
        public static TimeSpan? GetLength(this Range<DateTime> range)
        {
            return range.HasData ? range.Maximum - range.Minimum : new TimeSpan?();
        }

        /// <summary>
        /// Returns the time span of a date range.
        /// </summary>
        /// <param name="range">The range of values.</param>
        /// <returns>The length of the range.</returns>
        public static double? GetLength(this Range<double> range)
        {
            return range.HasData ? range.Maximum - range.Minimum : new double?();
        }

        /// <summary>
        /// Returns a value indicating whether a rectangle is empty or has
        /// no width or height.
        /// </summary>
        /// <param name="rect">The rectangle.</param>
        /// <returns>A value indicating whether a rectangle is empty or has
        /// no width or height.</returns>
        public static bool IsEmptyOrHasNoSize(this Rect rect)
        {
            return rect.IsEmpty || (rect.Width == 0 && rect.Height == 0);
        }

        /// <summary>
        /// Sets the style property of an element.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="style">The style.</param>
        public static void SetStyle(this FrameworkElement element, Style style)
        {
            element.Style = style;
        }
    }
}