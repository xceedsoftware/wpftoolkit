// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows.Media;
using System.Windows.Shapes;

namespace System.Windows.Controls.DataVisualization.Charting
{
    /// <summary>
    /// Control that displays values as a stacked line chart visualization.
    /// </summary>
    /// <QualityBand>Preview</QualityBand>
    public class StackedLineSeries : StackedAreaLineSeries
    {
        /// <summary>
        /// Initializes a new instance of the StackedLineSeries class.
        /// </summary>
        public StackedLineSeries()
        {
        }

        /// <summary>
        /// Creates a DataPoint for the series.
        /// </summary>
        /// <returns>Series-appropriate DataPoint instance.</returns>
        protected override DataPoint CreateDataPoint()
        {
            return new LineDataPoint();
        }

        /// <summary>
        /// Creates a series-appropriate Shape for connecting the points of the series.
        /// </summary>
        /// <returns>Shape instance.</returns>
        protected override Shape CreateDataShape()
        {
            return new Polyline { Fill = null };
        }

        /// <summary>
        /// Updates the shape for the series.
        /// </summary>
        /// <param name="definitionPoints">Locations of the points of each SeriesDefinition in the series.</param>
        protected override void UpdateShape(IList<IEnumerable<Point>> definitionPoints)
        {
            for (int i = 0; i < SeriesDefinitions.Count; i++)
            {
                PointCollection pointCollection = new PointCollection();
                foreach (Point p in ((ActualIndependentAxis is ICategoryAxis) ? definitionPoints[i].OrderBy(p => p.X) : definitionPoints[i]))
                {
                    pointCollection.Add(p);
                }
                SetPolylinePointsProperty((Polyline)SeriesDefinitionShapes[SeriesDefinitions[i]], pointCollection);
            }
        }

        /// <summary>
        /// Sets the Points property of a Polyline to the specified PointCollection.
        /// </summary>
        /// <param name="polyline">Polyline to set the Points property of.</param>
        /// <param name="pointCollection">Specified PointCollection.</param>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Polyline", Justification = "Matches spelling of same-named framework class.")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "polyline", Justification = "Matches spelling of same-named framework class.")]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Silverlight implementation is not static.")]
        protected void SetPolylinePointsProperty(Polyline polyline, PointCollection pointCollection)
        {
#if SILVERLIGHT
            // Changing .Points during an Arrange pass can create a layout cycle on Silverlight
            if (!polyline.Points.SequenceEqual(pointCollection))
            {
#endif
                polyline.Points = pointCollection;
#if SILVERLIGHT
                // In rare cases, Silverlight doesn't update the line visual to match the new points;
                // calling InvalidateArrange works around that problem.
                polyline.InvalidateArrange();
            }
#endif
        }
    }
}
