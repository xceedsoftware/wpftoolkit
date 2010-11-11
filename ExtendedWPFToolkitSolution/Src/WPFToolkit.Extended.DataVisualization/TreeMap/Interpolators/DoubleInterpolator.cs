// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System.Globalization;

namespace System.Windows.Controls.DataVisualization
{
    /// <summary>
    /// Interpolator which converts a numeric value from its [RangeMinimum, RangeMaximum]
    /// range to another value in the range [From, To].
    /// </summary>
    /// <QualityBand>Preview</QualityBand>
    public class DoubleInterpolator : RangeInterpolator<double>
    {
        /// <summary>
        /// Interpolates the given value between its [RangeMinimum, RangeMaximum] range
        /// and returns an interpolated value in the range [From, To].
        /// </summary>
        /// <param name="value">Value to interpolate.</param>
        /// <returns>An interpolated value in the range [From, To].</returns>
        public override object Interpolate(double value)
        {
            double result = From;
            if (ActualDataMaximum - ActualDataMinimum != 0)
            {
                double ratio = (value - ActualDataMinimum) / (ActualDataMaximum - ActualDataMinimum);
                result = From + (ratio * (To - From));
            }

            return result;
        }
    }
}
