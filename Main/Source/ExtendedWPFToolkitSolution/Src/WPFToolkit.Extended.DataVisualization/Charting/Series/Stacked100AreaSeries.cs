// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

namespace System.Windows.Controls.DataVisualization.Charting
{
    /// <summary>
    /// Control that displays values as a 100% stacked area chart visualization.
    /// </summary>
    /// <QualityBand>Preview</QualityBand>
    public class Stacked100AreaSeries : StackedAreaSeries
    {
        /// <summary>
        /// Initializes a new instance of the Stacked100AreaSeries class.
        /// </summary>
        public Stacked100AreaSeries()
        {
            IsStacked100 = true;
        }
    }
}
