// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System.Collections.Generic;

namespace System.Windows.Controls.DataVisualization.Charting
{
    /// <summary>
    /// An axis that is arranged by category.
    /// </summary>
    public interface ICategoryAxis : IAxis, IDataConsumer
    {
        /// <summary>
        /// Accepts a category and returns the coordinate range of that category
        /// on the axis.
        /// </summary>
        /// <param name="category">A category for which to retrieve the 
        /// coordinate location.</param>
        /// <returns>The coordinate range of the category on the axis.</returns>        
        Range<UnitValue> GetPlotAreaCoordinateRange(object category);
        
        /// <summary>
        /// Returns the category at a given coordinate.
        /// </summary>
        /// <param name="position">The plot are coordinate.</param>
        /// <returns>The category at the given plot area coordinate.</returns>
        object GetCategoryAtPosition(UnitValue position);
    }
}
