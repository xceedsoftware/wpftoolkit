// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

namespace System.Windows.Controls.DataVisualization.Charting
{
    /// <summary>
    /// A set of extension methods for the DataPoint class.
    /// </summary>
    internal static class FrameworkElementExtensions
    {
        /// <summary>
        /// Returns the actual margin for a given framework element and axis.
        /// </summary>
        /// <param name="element">The framework element.</param>
        /// <param name="axis">The axis along which to return the margin.
        /// </param>
        /// <returns>The margin for a given framework element and axis.
        /// </returns>
        public static double GetActualMargin(this FrameworkElement element, IAxis axis)
        {
            double length = 0.0;
            if (axis.Orientation == AxisOrientation.X)
            {
                length = element.ActualWidth;
            }
            else if (axis.Orientation == AxisOrientation.Y)
            {
                length = element.ActualHeight;
            }
            return length / 2.0;
        }

        /// <summary>
        /// Returns the margin for a given framework element and axis.
        /// </summary>
        /// <param name="element">The framework element.</param>
        /// <param name="axis">The axis along which to return the margin.
        /// </param>
        /// <returns>The margin for a given framework element and axis.
        /// </returns>
        public static double GetMargin(this FrameworkElement element, IAxis axis)
        {
            double length = 0.0;
            if (axis.Orientation == AxisOrientation.X)
            {
                length = !double.IsNaN(element.Width) ? element.Width : element.ActualWidth;
            }
            else if (axis.Orientation == AxisOrientation.Y)
            {
                length = !double.IsNaN(element.Height) ? element.Height : element.ActualHeight;
            }
            return length / 2.0;
        }
    }
}
