// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System.Collections.ObjectModel;

namespace System.Windows.Controls.DataVisualization.Charting
{
    /// <summary>
    /// Represents a series in a chart.
    /// </summary>
    public interface ISeries : IRequireSeriesHost
    {
        /// <summary>
        /// Gets a list of the legend items associated with the object.
        /// </summary>
        ObservableCollection<object> LegendItems { get; }
    }
}