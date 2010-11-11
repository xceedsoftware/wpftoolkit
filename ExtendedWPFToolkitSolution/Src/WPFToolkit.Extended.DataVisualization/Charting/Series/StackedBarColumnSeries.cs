// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace System.Windows.Controls.DataVisualization.Charting
{
    /// <summary>
    /// Control base class for displaying values as a stacked bar/column chart visualization.
    /// </summary>
    /// <QualityBand>Preview</QualityBand>
    public abstract class StackedBarColumnSeries : DefinitionSeries, IAnchoredToOrigin
    {
        /// <summary>
        /// Gets or sets the orientation of the dependent axis.
        /// </summary>
        protected AxisOrientation DependentAxisOrientation { get; set; }

        /// <summary>
        /// Gets or sets the orientation of the independent axis.
        /// </summary>
        protected AxisOrientation IndependentAxisOrientation { get; set; }

        /// <summary>
        /// Initializes a new instance of the StackedBarColumnSeries class.
        /// </summary>
        protected StackedBarColumnSeries()
        {
        }

        /// <summary>
        /// Acquires a dependent axis suitable for use with the data values of the series.
        /// </summary>
        /// <returns>Axis instance.</returns>
        protected override IAxis AcquireDependentAxis()
        {
            IAxis dependentAxis = SeriesHost.Axes
                .Where(a => (a.Orientation == DependentAxisOrientation) && (a is IRangeAxis) && DataItems.Any() && (a.CanPlot(DataItems.First().ActualDependentValue)))
                .FirstOrDefault();
            if (null == dependentAxis)
            {
                LinearAxis linearAxis = new LinearAxis { Orientation = DependentAxisOrientation, ShowGridLines = true };
                if (IsStacked100)
                {
                    Style style = new Style(typeof(AxisLabel));
                    style.Setters.Add(new Setter(AxisLabel.StringFormatProperty, "{0}%"));
                    linearAxis.AxisLabelStyle = style;
                }
                dependentAxis = linearAxis;
            }
            return dependentAxis;
        }

        /// <summary>
        /// Acquires an independent axis suitable for use with the data values of the series.
        /// </summary>
        /// <returns>Axis instance.</returns>
        protected override IAxis AcquireIndependentAxis()
        {
            IAxis independentAxis = SeriesHost.Axes
                .Where(a => (a.Orientation == IndependentAxisOrientation) && ((a is ICategoryAxis) || (a is IRangeAxis)) && DataItems.Any() && (a.CanPlot(DataItems.First().ActualIndependentValue)))
                .FirstOrDefault();
            if (null == independentAxis)
            {
                independentAxis = new CategoryAxis { Orientation = IndependentAxisOrientation };
            }
            return independentAxis;
        }

        /// <summary>
        /// Returns the range for the data points of the series.
        /// </summary>
        /// <param name="rangeConsumer">Consumer of the range.</param>
        /// <returns>Range of values.</returns>
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "Linq is artificially increasing the rating.")]
        protected override Range<IComparable> IRangeProviderGetRange(IRangeConsumer rangeConsumer)
        {
            if (rangeConsumer == ActualDependentAxis)
            {
                var dependentValuesByIndependentValue = IndependentValueDependentValues.Select(e => e.ToArray()).ToArray();

                var mostNegative = dependentValuesByIndependentValue
                    .Select(g => g.Where(v => v < 0)
                        .Sum())
                    .Where(v => v < 0)
                    .ToArray();
                var leastNegative = dependentValuesByIndependentValue
                    .Select(g => g.Where(v => v <= 0)
                        .DefaultIfEmpty(1.0)
                        .First())
                    .Where(v => v <= 0)
                    .ToArray();
                var mostPositive = dependentValuesByIndependentValue
                    .Select(g => g.Where(v => 0 < v)
                        .Sum())
                    .Where(v => 0 < v)
                    .ToArray();
                var leastPositive = dependentValuesByIndependentValue
                    .Select(g => g.Where(v => 0 <= v)
                        .DefaultIfEmpty(-1.0)
                        .First())
                    .Where(v => 0 <= v)
                    .ToArray();

                // Compute minimum
                double minimum = 0;
                if (mostNegative.Any())
                {
                    minimum = mostNegative.Min();
                }
                else if (leastPositive.Any())
                {
                    minimum = leastPositive.Min();
                }

                // Compute maximum
                double maximum = 0;
                if (mostPositive.Any())
                {
                    maximum = mostPositive.Max();
                }
                else if (leastNegative.Any())
                {
                    maximum = leastNegative.Max();
                }

                if (IsStacked100)
                {
                    minimum = Math.Min(minimum, 0);
                    maximum = Math.Max(maximum, 0);
                }

                return new Range<IComparable>(minimum, maximum);
            }
            else if (rangeConsumer == ActualIndependentAxis)
            {
                // Using a non-ICategoryAxis for the independent axis
                // Need to specifically adjust for slot size of bars/columns so they don't overlap
                // Note: Calculation for slotSize is not perfect, but it's quick, close, and errs on the safe side
                Range<IComparable> range = base.IRangeProviderGetRange(rangeConsumer);
                int count = Math.Max(IndependentValueGroups.Count(), 1);
                if (ActualIndependentAxis.CanPlot(0.0))
                {
                    double minimum = ValueHelper.ToDouble(range.Minimum);
                    double maximum = ValueHelper.ToDouble(range.Maximum);
                    double slotSize = (maximum - minimum) / count;
                    return new Range<IComparable>(minimum - slotSize, maximum + slotSize);
                }
                else
                {
                    DateTime minimum = ValueHelper.ToDateTime(range.Minimum);
                    DateTime maximum = ValueHelper.ToDateTime(range.Maximum);
                    TimeSpan slotSize = TimeSpan.FromTicks((maximum - minimum).Ticks / count);
                    return new Range<IComparable>(minimum - slotSize, maximum + slotSize);
                }
            }
            else
            {
                return base.IRangeProviderGetRange(rangeConsumer);
            }
        }

        /// <summary>
        /// Returns the value margins for the data points of the series.
        /// </summary>
        /// <param name="valueMarginConsumer">Consumer of the value margins.</param>
        /// <returns>Sequence of value margins.</returns>
        protected override IEnumerable<ValueMargin> IValueMarginProviderGetValueMargins(IValueMarginConsumer valueMarginConsumer)
        {
            if (valueMarginConsumer == ActualDependentAxis)
            {
                if (IsStacked100)
                {
                    return Enumerable.Empty<ValueMargin>();
                }
                else
                {
                    Range<IComparable> range = IRangeProviderGetRange((IRangeConsumer)ActualDependentAxis);
                    double margin = ((AxisOrientation.Y == ActualDependentAxis.Orientation) ? ActualHeight : ActualWidth) / 10;
                    return new ValueMargin[]
                    {
                        new ValueMargin(range.Minimum, margin, margin),
                        new ValueMargin(range.Maximum, margin, margin),
                    };
                }
            }
            else if (valueMarginConsumer == ActualIndependentAxis)
            {
                // Using a non-ICategoryAxis for the independent axis
                // Relevant space already accounted for by IRangeProviderGetRange
                return Enumerable.Empty<ValueMargin>();
            }
            else
            {
                return base.IValueMarginProviderGetValueMargins(valueMarginConsumer);
            }
        }

        /// <summary>
        /// Updates the placement of the DataItems (data points) of the series.
        /// </summary>
        /// <param name="dataItems">DataItems in need of an update.</param>
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "Linq is artificially increasing the rating.")]
        protected override void UpdateDataItemPlacement(IEnumerable<DefinitionSeries.DataItem> dataItems)
        {
            IAxis actualIndependentAxis = ActualIndependentAxis;
            if ((null != ActualDependentAxis) && (null != actualIndependentAxis))
            {
                double plotAreaMaximumDependentCoordinate = ActualDependentAxis.GetPlotAreaCoordinate(ActualDependentRangeAxis.Range.Maximum).Value;
                double zeroCoordinate = ActualDependentAxis.GetPlotAreaCoordinate(ActualDependentRangeAxis.Origin ?? 0.0).Value;
                ICategoryAxis actualIndependentCategoryAxis = actualIndependentAxis as ICategoryAxis;
                double nonCategoryAxisRangeMargin = (null != actualIndependentCategoryAxis) ? 0 : GetMarginForNonCategoryAxis(actualIndependentAxis);
                foreach (IndependentValueGroup group in IndependentValueGroups)
                {
                    Range<UnitValue> categoryRange = new Range<UnitValue>();
                    if (null != actualIndependentCategoryAxis)
                    {
                        categoryRange = actualIndependentCategoryAxis.GetPlotAreaCoordinateRange(group.IndependentValue);
                    }
                    else
                    {
                        UnitValue independentValueCoordinate = actualIndependentAxis.GetPlotAreaCoordinate(group.IndependentValue);
                        if (ValueHelper.CanGraph(independentValueCoordinate.Value))
                        {
                            categoryRange = new Range<UnitValue>(new UnitValue(independentValueCoordinate.Value - nonCategoryAxisRangeMargin, independentValueCoordinate.Unit), new UnitValue(independentValueCoordinate.Value + nonCategoryAxisRangeMargin, independentValueCoordinate.Unit));
                        }
                    }
                    if (categoryRange.HasData)
                    {
                        double categoryMinimumCoordinate = categoryRange.Minimum.Value;
                        double categoryMaximumCoordinate = categoryRange.Maximum.Value;
                        double padding = 0.1 * (categoryMaximumCoordinate - categoryMinimumCoordinate);
                        categoryMinimumCoordinate += padding;
                        categoryMaximumCoordinate -= padding;

                        double sum = IsStacked100 ?
                            group.DataItems.Sum(di => Math.Abs(ValueHelper.ToDouble(di.DataPoint.ActualDependentValue))) :
                            1;
                        if (0 == sum)
                        {
                            sum = 1;
                        }
                        double ceiling = 0;
                        double floor = 0;
                        foreach (DataItem dataItem in group.DataItems)
                        {
                            DataPoint dataPoint = dataItem.DataPoint;
                            double value = IsStacked100 ? (ValueHelper.ToDouble(dataPoint.ActualDependentValue) * (100 / sum)) : ValueHelper.ToDouble(dataPoint.ActualDependentValue);
                            if (ValueHelper.CanGraph(value))
                            {
                                double valueCoordinate = ActualDependentAxis.GetPlotAreaCoordinate(value).Value;
                                double fillerCoordinate = (0 <= value) ? ceiling : floor;

                                double topCoordinate = 0, leftCoordinate = 0, height = 0, width = 0, deltaCoordinate = 0;
                                if (AxisOrientation.Y == ActualDependentAxis.Orientation)
                                {
                                    topCoordinate = plotAreaMaximumDependentCoordinate - Math.Max(valueCoordinate + fillerCoordinate, zeroCoordinate + fillerCoordinate);
                                    double bottomCoordinate = plotAreaMaximumDependentCoordinate - Math.Min(valueCoordinate + fillerCoordinate, zeroCoordinate + fillerCoordinate);
                                    deltaCoordinate = bottomCoordinate - topCoordinate;
                                    height = (0 < deltaCoordinate) ? deltaCoordinate + 1 : 0;
                                    leftCoordinate = categoryMinimumCoordinate;
                                    width = categoryMaximumCoordinate - categoryMinimumCoordinate + 1;
                                }
                                else
                                {
                                    leftCoordinate = Math.Min(valueCoordinate + fillerCoordinate, zeroCoordinate + fillerCoordinate);
                                    double rightCoordinate = Math.Max(valueCoordinate + fillerCoordinate, zeroCoordinate + fillerCoordinate);
                                    deltaCoordinate = rightCoordinate - leftCoordinate;
                                    width = (0 < deltaCoordinate) ? deltaCoordinate + 1 : 0;
                                    topCoordinate = categoryMinimumCoordinate;
                                    height = categoryMaximumCoordinate - categoryMinimumCoordinate + 1;
                                }

                                double roundedTopCoordinate = Math.Round(topCoordinate);
                                Canvas.SetTop(dataItem.Container, roundedTopCoordinate);
                                dataPoint.Height = Math.Round(topCoordinate + height - roundedTopCoordinate);
                                double roundedLeftCoordinate = Math.Round(leftCoordinate);
                                Canvas.SetLeft(dataItem.Container, roundedLeftCoordinate);
                                dataPoint.Width = Math.Round(leftCoordinate + width - roundedLeftCoordinate);
                                dataPoint.Visibility = Visibility.Visible;

                                if (0 <= value)
                                {
                                    ceiling += deltaCoordinate;
                                }
                                else
                                {
                                    floor -= deltaCoordinate;
                                }
                            }
                            else
                            {
                                dataPoint.Visibility = Visibility.Collapsed;
                            }
                        }
                    }
                    else
                    {
                        foreach (DataPoint dataPoint in group.DataItems.Select(di => di.DataPoint))
                        {
                            dataPoint.Visibility = Visibility.Collapsed;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets the margin to use for an independent axis that does not implement ICategoryAxis.
        /// </summary>
        /// <param name="axis">Axis to get the margin for.</param>
        /// <returns>Margin for axis.</returns>
        private double GetMarginForNonCategoryAxis(IAxis axis)
        {
            Debug.Assert(!(axis is ICategoryAxis), "This method is unnecessary for ICategoryAxis.");

            // Find the smallest distance between two independent value plot area coordinates
            double smallestDistance = double.MaxValue;
            double lastCoordinate = double.NaN;
            foreach (double coordinate in
                IndependentValueGroupsOrderedByIndependentValue
                    .Select(g => axis.GetPlotAreaCoordinate(g.IndependentValue).Value)
                    .Where(v => ValueHelper.CanGraph(v)))
            {
                if (!double.IsNaN(lastCoordinate))
                {
                    double distance = coordinate - lastCoordinate;
                    if (distance < smallestDistance)
                    {
                        smallestDistance = distance;
                    }
                }
                lastCoordinate = coordinate;
            }
            // Return the margin
            if (double.MaxValue == smallestDistance)
            {
                // No smallest distance because <= 1 independent values to plot
                FrameworkElement element = axis as FrameworkElement;
                if (null != element)
                {
                    // Use width of provided axis so single column scenario looks good
                    return element.GetMargin(axis);
                }
                else
                {
                    // No information to work with; no idea what margin to return
                    throw new NotSupportedException();
                }
            }
            else
            {
                // Found the smallest distance; margin is half of that
                return smallestDistance / 2;
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
