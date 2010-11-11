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
    /// Control that displays values as a stacked area chart visualization.
    /// </summary>
    /// <QualityBand>Preview</QualityBand>
    public class StackedAreaSeries : StackedAreaLineSeries, IAnchoredToOrigin
    {
        /// <summary>
        /// Initializes a new instance of the StackedAreaSeries class.
        /// </summary>
        public StackedAreaSeries()
        {
        }

        /// <summary>
        /// Creates a DataPoint for the series.
        /// </summary>
        /// <returns>Series-appropriate DataPoint instance.</returns>
        protected override DataPoint CreateDataPoint()
        {
            return new AreaDataPoint();
        }

        /// <summary>
        /// Creates a series-appropriate Shape for connecting the points of the series.
        /// </summary>
        /// <returns>Shape instance.</returns>
        protected override Shape CreateDataShape()
        {
            return new Polygon();
        }

        /// <summary>
        /// Updates the Shape for the series.
        /// </summary>
        /// <param name="definitionPoints">Locations of the points of each SeriesDefinition in the series.</param>
        protected override void UpdateShape(IList<IEnumerable<Point>> definitionPoints)
        {
            for (int i = SeriesDefinitions.Count - 1; 0 < i; i--)
            {
                PointCollection pointCollection = new PointCollection();
                IEnumerable<Point> topPoints = (ActualIndependentAxis is ICategoryAxis) ? definitionPoints[i].OrderBy(p => p.X) : definitionPoints[i];
                foreach (Point p in topPoints)
                {
                    pointCollection.Add(p);
                }
                IEnumerable<Point> bottomPoints = (ActualIndependentAxis is ICategoryAxis) ? definitionPoints[i - 1].OrderByDescending(p => p.X) : definitionPoints[i - 1].Reverse();
                foreach (Point p in bottomPoints)
                {
                    pointCollection.Add(p);
                }
                SetPolygonPointsProperty((Polygon)SeriesDefinitionShapes[SeriesDefinitions[i]], pointCollection);
            }
            if (1 <= SeriesDefinitions.Count)
            {
                double plotAreaMaximumDependentCoordinate = ActualDependentAxis.GetPlotAreaCoordinate(ActualDependentRangeAxis.Range.Maximum).Value;
                IComparable zeroValue = ActualDependentRangeAxis.Origin ?? 0.0;
                if (zeroValue.CompareTo(ActualDependentRangeAxis.Range.Minimum) < 0)
                {
                    zeroValue = ActualDependentRangeAxis.Range.Minimum;
                }
                if (0 < zeroValue.CompareTo(ActualDependentRangeAxis.Range.Maximum))
                {
                    zeroValue = ActualDependentRangeAxis.Range.Maximum;
                }
                double zeroCoordinate = ActualDependentAxis.GetPlotAreaCoordinate(zeroValue).Value;
                PointCollection pointCollection = new PointCollection();
                Point[] topPoints = ((ActualIndependentAxis is ICategoryAxis) ? definitionPoints[0].OrderBy(p => p.X) : definitionPoints[0]).ToArray();
                foreach (Point p in topPoints)
                {
                    pointCollection.Add(p);
                }
                if (0 < topPoints.Length)
                {
                    Point firstPoint = topPoints[0];
                    Point lastPoint = topPoints[topPoints.Length - 1];
                    pointCollection.Add(new Point(lastPoint.X, plotAreaMaximumDependentCoordinate - zeroCoordinate));
                    pointCollection.Add(new Point(firstPoint.X, plotAreaMaximumDependentCoordinate - zeroCoordinate));
                }
                SetPolygonPointsProperty((Polygon)SeriesDefinitionShapes[SeriesDefinitions[0]], pointCollection);
            }
        }

        /// <summary>
        /// Sets the Points property of a Polygon to the specified PointCollection.
        /// </summary>
        /// <param name="polygon">Polygon to set the Points property of.</param>
        /// <param name="pointCollection">Specified PointCollection.</param>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Silverlight implementation is not static.")]
        protected void SetPolygonPointsProperty(Polygon polygon, PointCollection pointCollection)
        {
#if SILVERLIGHT
            // Changing .Points during an Arrange pass can create a layout cycle on Silverlight
            if (!polygon.Points.SequenceEqual(pointCollection))
            {
#endif
                polygon.Points = pointCollection;
#if SILVERLIGHT
                // In rare cases, Silverlight doesn't update the line visual to match the new points;
                // calling InvalidateArrange works around that problem.
                polygon.InvalidateArrange();
            }
#endif
        }

        /// <summary>
        /// Returns the value margins for the data points of the series.
        /// </summary>
        /// <param name="valueMarginConsumer">Consumer of the value margins.</param>
        /// <returns>Sequence of value margins.</returns>
        protected override IEnumerable<ValueMargin> IValueMarginProviderGetValueMargins(IValueMarginConsumer valueMarginConsumer)
        {
            if (valueMarginConsumer == ActualIndependentAxis)
            {
                return Enumerable.Empty<ValueMargin>();
            }
            else
            {
                return base.IValueMarginProviderGetValueMargins(valueMarginConsumer);
            }
        }

        /// <summary>
        /// Gets the anchored axis for the series.
        /// </summary>
        IRangeAxis IAnchoredToOrigin.AnchoredAxis
        {
            get { return ActualDependentRangeAxis; }
        }
    }
}
