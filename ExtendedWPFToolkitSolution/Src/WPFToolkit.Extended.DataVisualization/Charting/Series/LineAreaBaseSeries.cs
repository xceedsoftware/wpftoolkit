// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Controls.DataVisualization.Collections;

namespace System.Windows.Controls.DataVisualization.Charting
{
    /// <summary>
    /// A base class that contains methods used by both the line and area series.
    /// </summary>
    /// <typeparam name="T">The type of data point used by the series.</typeparam>
    public abstract class LineAreaBaseSeries<T> : DataPointSingleSeriesWithAxes
        where T : DataPoint, new()
    {
        #region public IRangeAxis DependentRangeAxis
        /// <summary>
        /// Gets or sets the dependent range axis.
        /// </summary>
        public IRangeAxis DependentRangeAxis
        {
            get { return GetValue(DependentRangeAxisProperty) as IRangeAxis; }
            set { SetValue(DependentRangeAxisProperty, value); }
        }

        /// <summary>
        /// Identifies the DependentRangeAxis dependency property.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes", Justification = "This member is necessary because child classes need to share this dependency property.")]
        public static readonly DependencyProperty DependentRangeAxisProperty =
            DependencyProperty.Register(
                "DependentRangeAxis",
                typeof(IRangeAxis),
                typeof(LineAreaBaseSeries<T>),
                new PropertyMetadata(null, OnDependentRangeAxisPropertyChanged));

        /// <summary>
        /// DependentRangeAxisProperty property changed handler.
        /// </summary>
        /// <param name="d">LineAreaBaseSeries that changed its DependentRangeAxis.</param>
        /// <param name="e">Event arguments.</param>
        private static void OnDependentRangeAxisPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            LineAreaBaseSeries<T> source = (LineAreaBaseSeries<T>)d;
            IRangeAxis newValue = (IRangeAxis)e.NewValue;
            source.OnDependentRangeAxisPropertyChanged(newValue);
        }

        /// <summary>
        /// DependentRangeAxisProperty property changed handler.
        /// </summary>
        /// <param name="newValue">New value.</param>
        private void OnDependentRangeAxisPropertyChanged(IRangeAxis newValue)
        {
            this.InternalDependentAxis = (IAxis)newValue;
        }
        #endregion public IRangeAxis DependentRangeAxis

        #region public IAxis IndependentAxis
        /// <summary>
        /// Gets or sets the independent range axis.
        /// </summary>
        public IAxis IndependentAxis
        {
            get { return GetValue(IndependentAxisProperty) as IAxis; }
            set { SetValue(IndependentAxisProperty, value); }
        }

        /// <summary>
        /// Identifies the IndependentAxis dependency property.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes", Justification = "This member is necessary because child classes need to share this dependency property.")]
        public static readonly DependencyProperty IndependentAxisProperty =
            DependencyProperty.Register(
                "IndependentAxis",
                typeof(IAxis),
                typeof(LineAreaBaseSeries<T>),
                new PropertyMetadata(null, OnIndependentAxisPropertyChanged));

        /// <summary>
        /// IndependentAxisProperty property changed handler.
        /// </summary>
        /// <param name="d">LineAreaBaseSeries that changed its IndependentAxis.</param>
        /// <param name="e">Event arguments.</param>
        private static void OnIndependentAxisPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            LineAreaBaseSeries<T> source = (LineAreaBaseSeries<T>)d;
            IAxis newValue = (IAxis)e.NewValue;
            source.OnIndependentAxisPropertyChanged(newValue);
        }

        /// <summary>
        /// IndependentAxisProperty property changed handler.
        /// </summary>
        /// <param name="newValue">New value.</param>
        private void OnIndependentAxisPropertyChanged(IAxis newValue)
        {
            this.InternalIndependentAxis = (IAxis)newValue;
        }
        #endregion public IAxis IndependentAxis

        /// <summary>
        /// Gets data points collection sorted by independent value.
        /// </summary>
        internal OrderedMultipleDictionary<IComparable, DataPoint> DataPointsByIndependentValue { get; private set; }

        /// <summary>
        /// Gets the independent axis as a range axis.
        /// </summary>
        public IAxis ActualIndependentAxis { get { return this.InternalActualIndependentAxis as IAxis; } }

        /// <summary>
        /// Gets the dependent axis as a range axis.
        /// </summary>
        public IRangeAxis ActualDependentRangeAxis { get { return this.InternalActualDependentAxis as IRangeAxis; } }

        /// <summary>
        /// Initializes a new instance of the LineAreaBaseSeries class.
        /// </summary>
        protected LineAreaBaseSeries()
        {
            DataPointsByIndependentValue =
                new OrderedMultipleDictionary<IComparable, DataPoint>(
                    false,
                    (left, right) =>
                        left.CompareTo(right),
                    (leftDataPoint, rightDataPoint) =>
                            RuntimeHelpers.GetHashCode(leftDataPoint).CompareTo(RuntimeHelpers.GetHashCode(rightDataPoint)));
        }

        /// <summary>
        /// Creates a DataPoint for determining the line color.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if (null != PlotArea)
            {
                Grid grid = new Grid();
                DataPoint dataPoint = CreateDataPoint();
                dataPoint.Visibility = Visibility.Collapsed;
                dataPoint.Loaded += delegate
                {
                    dataPoint.SetStyle(ActualDataPointStyle);
                    Background = dataPoint.Background;
                    if (null != PlotArea)
                    {
                        PlotArea.Children.Remove(grid);
                    }
                };
                grid.Children.Add(dataPoint);
                PlotArea.Children.Add(grid);
            }
        }

        /// <summary>
        /// Called after data points have been loaded from the items source.
        /// </summary>
        /// <param name="newDataPoints">New active data points.</param>
        /// <param name="oldDataPoints">Old inactive data points.</param>
        protected override void OnDataPointsChanged(IList<DataPoint> newDataPoints, IList<DataPoint> oldDataPoints)
        {
            base.OnDataPointsChanged(newDataPoints, oldDataPoints);

            if (ActualIndependentAxis is IRangeAxis)
            {
                foreach (DataPoint dataPoint in oldDataPoints)
                {
                    DataPointsByIndependentValue.Remove((IComparable)dataPoint.IndependentValue, dataPoint);
                }

                foreach (DataPoint dataPoint in newDataPoints)
                {
                    DataPointsByIndependentValue.Add((IComparable)dataPoint.IndependentValue, dataPoint);
                }
            }
        }

        /// <summary>
        /// This method executes after all data points have been updated.
        /// </summary>
        protected override void OnAfterUpdateDataPoints()
        {
            if (InternalActualDependentAxis != null && InternalActualIndependentAxis != null)
            {
                UpdateShape();
            }
        }

        /// <summary>
        /// Repositions line data point in the sorted collection if the actual 
        /// independent axis is a range axis.
        /// </summary>
        /// <param name="dataPoint">The data point that has changed.</param>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        protected override void OnDataPointIndependentValueChanged(DataPoint dataPoint, object oldValue, object newValue)
        {
            if (ActualIndependentAxis is IRangeAxis && !oldValue.Equals(newValue))
            {
                bool removed = DataPointsByIndependentValue.Remove((IComparable)oldValue, dataPoint);
                if (removed)
                {
                    DataPointsByIndependentValue.Add((IComparable)newValue, dataPoint);
                }
            }

            base.OnDataPointIndependentValueChanged(dataPoint, oldValue, newValue);
        }

        /// <summary>
        /// Creates a new line data point.
        /// </summary>
        /// <returns>A line data point.</returns>
        protected override DataPoint CreateDataPoint()
        {
            return new T();
        }

        /// <summary>
        /// Returns the custom ResourceDictionary to use for necessary resources.
        /// </summary>
        /// <returns>
        /// ResourceDictionary to use for necessary resources.
        /// </returns>
        protected override IEnumerator<ResourceDictionary> GetResourceDictionaryEnumeratorFromHost()
        {
            return GetResourceDictionaryWithTargetType(SeriesHost, typeof(T), true);
        }

        /// <summary>
        /// Updates the visual representation of the data point.
        /// </summary>
        /// <param name="dataPoint">The data point to update.</param>
        protected override void UpdateDataPoint(DataPoint dataPoint)
        {
            double maximum = ActualDependentRangeAxis.GetPlotAreaCoordinate(ActualDependentRangeAxis.Range.Maximum).Value;
            if (ValueHelper.CanGraph(maximum))
            {
                double x = ActualIndependentAxis.GetPlotAreaCoordinate(dataPoint.ActualIndependentValue).Value;
                double y = ActualDependentRangeAxis.GetPlotAreaCoordinate(dataPoint.ActualDependentValue).Value;

                if (ValueHelper.CanGraph(x) && ValueHelper.CanGraph(y))
                {
                    dataPoint.Visibility = Visibility.Visible;

                    double coordinateY = Math.Round(maximum - (y + (dataPoint.ActualHeight / 2)));
                    Canvas.SetTop(dataPoint, coordinateY);
                    double coordinateX = Math.Round(x - (dataPoint.ActualWidth / 2));
                    Canvas.SetLeft(dataPoint, coordinateX);
                }
                else
                {
                    dataPoint.Visibility = Visibility.Collapsed;
                }
            }

            if (!UpdatingDataPoints)
            {
                UpdateShape();
            }
        }

        /// <summary>
        /// Updates the Series shape object.
        /// </summary>
        protected virtual void UpdateShape()
        {
            double maximum = ActualDependentRangeAxis.GetPlotAreaCoordinate(ActualDependentRangeAxis.Range.Maximum).Value;

            Func<DataPoint, Point> createPoint =
                dataPoint =>
                    new Point(
                        ActualIndependentAxis.GetPlotAreaCoordinate(dataPoint.ActualIndependentValue).Value,
                        maximum - ActualDependentRangeAxis.GetPlotAreaCoordinate(dataPoint.ActualDependentValue).Value);

            IEnumerable<Point> points = Enumerable.Empty<Point>();
            if (ValueHelper.CanGraph(maximum))
            {
                if (ActualIndependentAxis is IRangeAxis)
                {
                    points = DataPointsByIndependentValue.Select(createPoint);
                }
                else
                {
                    points =
                        ActiveDataPoints
                            .Select(createPoint)
                            .OrderBy(point => point.X);
                }
            }
            UpdateShapeFromPoints(points);
        }

        /// <summary>
        /// Updates the Series shape object from a collection of Points.
        /// </summary>
        /// <param name="points">Collection of Points.</param>
        protected abstract void UpdateShapeFromPoints(IEnumerable<Point> points);
    }
}
