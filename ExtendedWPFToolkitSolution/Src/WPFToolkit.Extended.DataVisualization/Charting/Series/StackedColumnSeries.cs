// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

namespace System.Windows.Controls.DataVisualization.Charting
{
    /// <summary>
    /// Control that displays values as a stacked column chart visualization.
    /// </summary>
    /// <QualityBand>Preview</QualityBand>
    public class StackedColumnSeries : StackedBarColumnSeries
    {
        /// <summary>
        /// Initializes a new instance of the StackedColumnSeries class.
        /// </summary>
        public StackedColumnSeries()
        {
            DependentAxisOrientation = AxisOrientation.Y;
            IndependentAxisOrientation = AxisOrientation.X;
        }

        /// <summary>
        /// Creates a DataPoint for the series.
        /// </summary>
        /// <returns>Series-appropriate DataPoint instance.</returns>
        protected override DataPoint CreateDataPoint()
        {
            return new ColumnDataPoint();
        }
    }
}
