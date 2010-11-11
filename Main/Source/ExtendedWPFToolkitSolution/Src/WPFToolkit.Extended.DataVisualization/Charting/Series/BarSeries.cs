// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

#if !DEFINITION_SERIES_COMPATIBILITY_MODE

namespace System.Windows.Controls.DataVisualization.Charting
{
    /// <summary>
    /// Represents a control that contains a data series to be rendered in bar format.
    /// </summary>
    /// <QualityBand>Preview</QualityBand>
    [SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance", Justification = "Depth of hierarchy is necessary to avoid code duplication.")]
    [StyleTypedProperty(Property = DataPointStyleName, StyleTargetType = typeof(BarDataPoint))]
    [StyleTypedProperty(Property = "LegendItemStyle", StyleTargetType = typeof(LegendItem))]
    [TemplatePart(Name = DataPointSeries.PlotAreaName, Type = typeof(Canvas))]
    public partial class BarSeries : ColumnBarBaseSeries<BarDataPoint>
    {
        /// <summary>
        /// Initializes a new instance of the BarSeries class.
        /// </summary>
        public BarSeries()
        {
        }

        /// <summary>
        /// Acquire a horizontal category axis and a vertical linear axis.
        /// </summary>
        /// <param name="firstDataPoint">The first data point.</param>
        protected override void GetAxes(DataPoint firstDataPoint)
        {
            GetAxes(
                firstDataPoint,
                (axis) => axis.Orientation == AxisOrientation.Y,
                () => new CategoryAxis { Orientation = AxisOrientation.Y },
                (axis) =>
                {
                    IRangeAxis rangeAxis = axis as IRangeAxis;
                    return rangeAxis != null && rangeAxis.Origin != null && axis.Orientation == AxisOrientation.X;
                },
                () =>
                {
                    IRangeAxis rangeAxis = CreateRangeAxisFromData(firstDataPoint.DependentValue);
                    rangeAxis.Orientation = AxisOrientation.X;
                    if (rangeAxis == null || rangeAxis.Origin == null)
                    {
                        throw new InvalidOperationException(Properties.Resources.DataPointSeriesWithAxes_NoSuitableAxisAvailableForPlottingDependentValue);
                    }
                    DisplayAxis axis = rangeAxis as DisplayAxis;
                    if (axis != null)
                    {
                        axis.ShowGridLines = true;
                    }
                    return rangeAxis;
                });
        }

        /// <summary>
        /// Updates each point.
        /// </summary>
        /// <param name="dataPoint">The data point to update.</param>
        protected override void UpdateDataPoint(DataPoint dataPoint)
        {
            if (SeriesHost == null || PlotArea == null)
            {
                return;
            }

            object category = dataPoint.ActualIndependentValue ?? (this.ActiveDataPoints.IndexOf(dataPoint) + 1);
            Range<UnitValue> coordinateRange = GetCategoryRange(category);

            if (!coordinateRange.HasData)
            {
                return;
            }
            else if (coordinateRange.Maximum.Unit != Unit.Pixels || coordinateRange.Minimum.Unit != Unit.Pixels)
            {
                throw new InvalidOperationException(Properties.Resources.DataPointSeriesWithAxes_ThisSeriesDoesNotSupportRadialAxes);
            }

            double minimum = (double)coordinateRange.Minimum.Value;
            double maximum = (double)coordinateRange.Maximum.Value;

            IEnumerable<BarSeries> barSeries = SeriesHost.Series.OfType<BarSeries>().Where(series => series.ActualIndependentAxis == ActualIndependentAxis);
            int numberOfSeries = barSeries.Count();
            double coordinateRangeHeight = (maximum - minimum);
            double segmentHeight = coordinateRangeHeight * 0.8;
            double barHeight = segmentHeight / numberOfSeries;
            int seriesIndex = barSeries.IndexOf(this);

            double dataPointX = ActualDependentRangeAxis.GetPlotAreaCoordinate(ValueHelper.ToDouble(dataPoint.ActualDependentValue)).Value;
            double zeroPointX = ActualDependentRangeAxis.GetPlotAreaCoordinate(ActualDependentRangeAxis.Origin).Value;

            double offset = seriesIndex * Math.Round(barHeight) + coordinateRangeHeight * 0.1;
            double dataPointY = minimum + offset;

            if (GetIsDataPointGrouped(category))
            {
                // Multiple DataPoints share this category; offset and overlap them appropriately
                IGrouping<object, DataPoint> categoryGrouping = GetDataPointGroup(category);
                int index = categoryGrouping.IndexOf(dataPoint);
                dataPointY += (index * (barHeight * 0.2)) / (categoryGrouping.Count() - 1);
                barHeight *= 0.8;
                Canvas.SetZIndex(dataPoint, -index);
            }

            if (ValueHelper.CanGraph(dataPointX) && ValueHelper.CanGraph(dataPointY) && ValueHelper.CanGraph(zeroPointX))
            {
                dataPoint.Visibility = Visibility.Visible;

                double top = Math.Round(dataPointY);
                double height = Math.Round(barHeight);

                double left = Math.Round(Math.Min(dataPointX, zeroPointX) - 0.5);
                double right = Math.Round(Math.Max(dataPointX, zeroPointX) - 0.5);
                double width = right - left + 1;

                Canvas.SetLeft(dataPoint, left);
                Canvas.SetTop(dataPoint, top);
                dataPoint.Width = width;
                dataPoint.Height = height;
            }
            else
            {
                dataPoint.Visibility = Visibility.Collapsed;
            }
        }
    }
}

#endif
