// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System.Collections.Generic;

namespace System.Windows.Controls.DataVisualization.Charting
{
    /// <summary>
    /// Provides information to a category axis.
    /// </summary>
    public interface IDataProvider
    {
        /// <summary>
        /// Retrieves the data to be plotted on the axis.
        /// </summary>
        /// <param name="axis">The axis to retrieve the data for.</param>
        /// <returns>The data to plot on the axis.</returns>
        IEnumerable<object> GetData(IDataConsumer axis);
    }
}