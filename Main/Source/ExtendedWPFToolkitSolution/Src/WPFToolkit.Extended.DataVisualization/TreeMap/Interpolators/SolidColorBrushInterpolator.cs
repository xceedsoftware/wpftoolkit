// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.
using System.Windows.Media;

namespace System.Windows.Controls.DataVisualization
{
    /// <summary>
    /// Interpolator which converts a numeric value from its [RangeMinimum, RangeMaximum]
    /// range to a color in the range [From, To].
    /// </summary>
    /// <QualityBand>Preview</QualityBand>
    public class SolidColorBrushInterpolator : RangeInterpolator<Color>
    {
        /// <summary>
        /// Interpolates the given value between its [RangeMinimum, RangeMaximum] range
        /// and returns a color in the range [From, To].
        /// </summary>
        /// <param name="value">Value to interpolate.</param>
        /// <returns>An interpolated color in the range [From, To].</returns>
        public override object Interpolate(double value)
        {
            Color color = From;
            if (ActualDataMaximum - ActualDataMinimum != 0)
            {
                double ratio = (value - ActualDataMinimum) / (ActualDataMaximum 
                    - ActualDataMinimum);

                color = Color.FromArgb(
                    (byte)(From.A + (ratio * (To.A - From.A))),
                    (byte)(From.R + (ratio * (To.R - From.R))),
                    (byte)(From.G + (ratio * (To.G - From.G))),
                    (byte)(From.B + (ratio * (To.B - From.B))));
            }

            return new SolidColorBrush(color);
        }
    }
}
