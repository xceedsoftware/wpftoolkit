// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;

namespace System.Windows.Controls.DataVisualization.Charting
{
    /// <summary>
    /// Control base class for displaying values as a stacked area/line chart visualization.
    /// </summary>
    /// <QualityBand>Preview</QualityBand>
    public abstract class StackedAreaLineSeries : DefinitionSeries
    {
        /// <summary>
        /// Gets the Shapes corresponding to each SeriesDefinition.
        /// </summary>
        protected Dictionary<SeriesDefinition, Shape> SeriesDefinitionShapes { get; private set; }

        /// <summary>
        /// Initializes a new instance of the StackedAreaLineSeries class.
        /// </summary>
        protected StackedAreaLineSeries()
        {
            SeriesDefinitionShapes = new Dictionary<SeriesDefinition, Shape>();
        }

        /// <summary>
        /// Builds the visual tree for the control when a new template is applied.
        /// </summary>
        public override void OnApplyTemplate()
        {
            SynchronizeSeriesDefinitionShapes(SeriesDefinitions, null);
            base.OnApplyTemplate();
            SynchronizeSeriesDefinitionShapes(null, SeriesDefinitions);
        }

        /// <summary>
        /// Called when the SeriesDefinitions collection changes.
        /// </summary>
        /// <param name="action">Type of change.</param>
        /// <param name="oldItems">Sequence of old items.</param>
        /// <param name="oldStartingIndex">Starting index of old items.</param>
        /// <param name="newItems">Sequence of new items.</param>
        /// <param name="newStartingIndex">Starting index of new items.</param>
        protected override void SeriesDefinitionsCollectionChanged(NotifyCollectionChangedAction action, IList oldItems, int oldStartingIndex, IList newItems, int newStartingIndex)
        {
            base.SeriesDefinitionsCollectionChanged(action, oldItems, oldStartingIndex, newItems, newStartingIndex);
            if (null != oldItems)
            {
                SynchronizeSeriesDefinitionShapes(oldItems.CastWrapper<SeriesDefinition>(), null);
                foreach (SeriesDefinition oldDefinition in oldItems.CastWrapper<SeriesDefinition>())
                {
                    SeriesDefinitionShapes.Remove(oldDefinition);
                }
            }
            if (null != newItems)
            {
                foreach (SeriesDefinition newDefinition in newItems.CastWrapper<SeriesDefinition>())
                {
                    Shape dataShape = CreateDataShape();
                    dataShape.SetBinding(Shape.StyleProperty, new Binding("ActualDataShapeStyle") { Source = newDefinition });
                    SeriesDefinitionShapes[newDefinition] = dataShape;
                }
                SynchronizeSeriesDefinitionShapes(null, newItems.CastWrapper<SeriesDefinition>());
            }
        }

        /// <summary>
        /// Acquires a dependent axis suitable for use with the data values of the series.
        /// </summary>
        /// <returns>Axis instance.</returns>
        protected override IAxis AcquireDependentAxis()
        {
            IAxis dependentAxis = SeriesHost.Axes
                .Where(a => (a.Orientation == AxisOrientation.Y) && (a is IRangeAxis) && DataItems.Any() && (a.CanPlot(DataItems.First().ActualDependentValue)))
                .FirstOrDefault();
            if (null == dependentAxis)
            {
                LinearAxis linearAxis = new LinearAxis { Orientation = AxisOrientation.Y, ShowGridLines = true };
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
                .Where(a => (a.Orientation == AxisOrientation.X) && ((a is IRangeAxis) || (a is ICategoryAxis)) && DataItems.Any() && (a.CanPlot(DataItems.First().ActualIndependentValue)))
                .FirstOrDefault();
            if (null == independentAxis)
            {
                object probeValue = DataItems.Any() ? DataItems.First().ActualIndependentValue : null;
                double convertedDouble;
                DateTime convertedDateTime;
                if ((null != probeValue) && ValueHelper.TryConvert(probeValue, out convertedDouble))
                {
                    independentAxis = new LinearAxis();
                }
                else if ((null != probeValue) && ValueHelper.TryConvert(probeValue, out convertedDateTime))
                {
                    independentAxis = new DateTimeAxis();
                }
                else
                {
                    independentAxis = new CategoryAxis();
                }
                independentAxis.Orientation = AxisOrientation.X;
            }
            return independentAxis;
        }

        /// <summary>
        /// Prepares a DataPoint for use.
        /// </summary>
        /// <param name="dataPoint">DataPoint instance.</param>
        protected override void PrepareDataPoint(DataPoint dataPoint)
        {
            base.PrepareDataPoint(dataPoint);
            dataPoint.SizeChanged += new SizeChangedEventHandler(DataPointSizeChanged);
        }

        /// <summary>
        /// Handles the SizeChanged event of a DataPoint to update the value margins for the series.
        /// </summary>
        /// <param name="sender">Event source.</param>
        /// <param name="e">Event arguments.</param>
        private void DataPointSizeChanged(object sender, SizeChangedEventArgs e)
        {
            DataPoint dataPoint = (DataPoint)sender;
            DataItem dataItem = DataItemFromDataPoint(dataPoint);

            // Update placement
            double newWidth = e.NewSize.Width;
            double newHeight = e.NewSize.Height;
            Canvas.SetLeft(dataItem.Container, Math.Round(dataItem.CenterPoint.X - (newWidth / 2)));
            Canvas.SetTop(dataItem.Container, Math.Round(dataItem.CenterPoint.Y - (newHeight / 2)));

            // Update value margins
            double heightMargin = newHeight * (3.0 / 4.0);
            NotifyValueMarginsChanged(ActualDependentAxis, new ValueMargin[] { new ValueMargin(dataItem.ActualStackedDependentValue, heightMargin, heightMargin) });
            double widthMargin = newWidth * (3.0 / 4.0);
            NotifyValueMarginsChanged(ActualIndependentAxis, new ValueMargin[] { new ValueMargin(dataPoint.ActualIndependentValue, widthMargin, widthMargin) });
        }

        /// <summary>
        /// Creates a series-appropriate Shape for connecting the points of the series.
        /// </summary>
        /// <returns>Shape instance.</returns>
        protected abstract Shape CreateDataShape();

        /// <summary>
        /// Synchronizes the SeriesDefinitionShapes dictionary with the contents of the SeriesArea Panel.
        /// </summary>
        /// <param name="oldDefinitions">SeriesDefinition being removed.</param>
        /// <param name="newDefinitions">SeriesDefinition being added.</param>
        private void SynchronizeSeriesDefinitionShapes(IEnumerable<SeriesDefinition> oldDefinitions, IEnumerable<SeriesDefinition> newDefinitions)
        {
            if (null != SeriesArea)
            {
                if (null != oldDefinitions)
                {
                    foreach (SeriesDefinition oldDefinition in oldDefinitions)
                    {
                        SeriesArea.Children.Remove(SeriesDefinitionShapes[oldDefinition]);
                    }
                }
                if (null != newDefinitions)
                {
                    foreach (SeriesDefinition newDefinition in newDefinitions.OrderBy(sd => sd.Index))
                    {
                        SeriesArea.Children.Insert(newDefinition.Index, SeriesDefinitionShapes[newDefinition]);
                    }
                }
            }
        }

        /// <summary>
        /// Returns the range for the data points of the series.
        /// </summary>
        /// <param name="rangeConsumer">Consumer of the range.</param>
        /// <returns>Range of values.</returns>
        protected override Range<IComparable> IRangeProviderGetRange(IRangeConsumer rangeConsumer)
        {
            if (rangeConsumer == ActualDependentAxis)
            {
                IEnumerable<Range<double>> dependentValueRangesByIndependentValue = IndependentValueDependentValues
                    .Select(g => g.Where(d => ValueHelper.CanGraph(d)))
                    .Select(g => g.Scan(0.0, (s, t) => s + t).Skip(1).GetRange())
                    .DefaultIfEmpty(new Range<double>(0, 0))
                    .ToArray();
                double minimum = dependentValueRangesByIndependentValue.Min(r => r.Minimum);
                double maximum = dependentValueRangesByIndependentValue.Max(r => r.Maximum);

                if (IsStacked100)
                {
                    minimum = Math.Min(minimum, 0);
                    maximum = Math.Max(maximum, 0);
                }

                return new Range<IComparable>(minimum, maximum);
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
            if (IsStacked100 && (valueMarginConsumer == ActualDependentAxis))
            {
                return Enumerable.Empty<ValueMargin>();
            }
            else if ((valueMarginConsumer == ActualDependentAxis) || (valueMarginConsumer == ActualIndependentAxis))
            {
                Range<IComparable> range = IRangeProviderGetRange((IRangeConsumer)valueMarginConsumer);
                double margin = DataItems
                    .Select(di =>
                    {
                        return (null != di.DataPoint) ?
                            (valueMarginConsumer == ActualDependentAxis) ? di.DataPoint.ActualHeight : di.DataPoint.ActualWidth :
                            0;
                    })
                    .Average() * (3.0 / 4.0);
                return new ValueMargin[]
                {
                    new ValueMargin(range.Minimum, margin, margin),
                    new ValueMargin(range.Maximum, margin, margin),
                };
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
        protected override void UpdateDataItemPlacement(IEnumerable<DefinitionSeries.DataItem> dataItems)
        {
            if ((null != ActualDependentAxis) && (null != ActualIndependentAxis))
            {
                double plotAreaMaximumDependentCoordinate = ActualDependentAxis.GetPlotAreaCoordinate(ActualDependentRangeAxis.Range.Maximum).Value;
                double lineTopBuffer = 1;
                List<Point>[] points = new List<Point>[SeriesDefinitions.Count];
                for (int i = 0; i < points.Length; i++)
                {
                    points[i] = new List<Point>();
                }
                foreach (IndependentValueGroup group in IndependentValueGroupsOrderedByIndependentValue)
                {
                    double sum = IsStacked100 ?
                        group.DataItems.Sum(di => Math.Abs(ValueHelper.ToDouble(di.DataPoint.ActualDependentValue))) :
                        1;
                    if (0 == sum)
                    {
                        sum = 1;
                    }
                    double x = ActualIndependentAxis.GetPlotAreaCoordinate(group.IndependentValue).Value;
                    if (ValueHelper.CanGraph(x))
                    {
                        double lastValue = 0;
                        Point lastPoint = new Point(x, Math.Max(plotAreaMaximumDependentCoordinate - ActualDependentRangeAxis.GetPlotAreaCoordinate(lastValue).Value, lineTopBuffer));
                        int i = -1;
                        SeriesDefinition lastDefinition = null;
                        foreach (DataItem dataItem in group.DataItems)
                        {
                            if (lastDefinition != dataItem.SeriesDefinition)
                            {
                                i++;
                            }

                            while (dataItem.SeriesDefinition != SeriesDefinitions[i])
                            {
                                points[i].Add(lastPoint);
                                i++;
                            }

                            DataPoint dataPoint = dataItem.DataPoint;
                            double value = IsStacked100 ?
                                (ValueHelper.ToDouble(dataItem.DataPoint.ActualDependentValue) * (100 / sum)) :
                                ValueHelper.ToDouble(dataItem.DataPoint.ActualDependentValue);
                            if (ValueHelper.CanGraph(value))
                            {
                                value += lastValue;
                                dataItem.ActualStackedDependentValue = value;
                                double y = ActualDependentRangeAxis.GetPlotAreaCoordinate(value).Value;
                                lastValue = value;
                                lastPoint.Y = Math.Max(plotAreaMaximumDependentCoordinate - y, lineTopBuffer);
                                points[i].Add(lastPoint);

                                dataItem.CenterPoint = new Point(x, plotAreaMaximumDependentCoordinate - y);
                                double left = dataItem.CenterPoint.X - (dataPoint.ActualWidth / 2);
                                double top = dataItem.CenterPoint.Y - (dataPoint.ActualHeight / 2);

                                Canvas.SetLeft(dataItem.Container, Math.Round(left));
                                Canvas.SetTop(dataItem.Container, Math.Round(top));
                                dataPoint.Visibility = Visibility.Visible;
                            }
                            else
                            {
                                points[i].Add(lastPoint);
                                dataPoint.Visibility = Visibility.Collapsed;
                            }

                            lastDefinition = dataItem.SeriesDefinition;
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
                UpdateShape(points);
            }
        }

        /// <summary>
        /// Updates the Shape for the series.
        /// </summary>
        /// <param name="definitionPoints">Locations of the points of each SeriesDefinition in the series.</param>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Nesting is convenient way to represent data.")]
        protected abstract void UpdateShape(IList<IEnumerable<Point>> definitionPoints);
    }
}
