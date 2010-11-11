// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace System.Windows.Controls.DataVisualization.Charting
{
    /// <summary>
    /// This series serves as the base class for the column and bar series.
    /// </summary>
    /// <typeparam name="T">The type of the data point.</typeparam>
    public abstract class ColumnBarBaseSeries<T> : DataPointSingleSeriesWithAxes, IAnchoredToOrigin
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
                typeof(ColumnBarBaseSeries<T>),
                new PropertyMetadata(null, OnDependentRangeAxisPropertyChanged));

        /// <summary>
        /// DependentRangeAxisProperty property changed handler.
        /// </summary>
        /// <param name="d">ColumnBarBaseSeries that changed its DependentRangeAxis.</param>
        /// <param name="e">Event arguments.</param>
        private static void OnDependentRangeAxisPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ColumnBarBaseSeries<T> source = (ColumnBarBaseSeries<T>)d;
            IRangeAxis newValue = (IRangeAxis)e.NewValue;
            source.OnDependentRangeAxisPropertyChanged(newValue);
        }

        /// <summary>
        /// DependentRangeAxisProperty property changed handler.
        /// </summary>
        /// <param name="newValue">New value.</param>
        private void OnDependentRangeAxisPropertyChanged(IRangeAxis newValue)
        {
            InternalDependentAxis = (IAxis)newValue;
        }
        #endregion public IRangeAxis DependentRangeAxis

        #region public IAxis IndependentAxis
        /// <summary>
        /// Gets or sets the independent category axis.
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
                typeof(ColumnBarBaseSeries<T>),
                new PropertyMetadata(null, OnIndependentAxisPropertyChanged));

        /// <summary>
        /// IndependentAxisProperty property changed handler.
        /// </summary>
        /// <param name="d">ColumnBarBaseSeries that changed its IndependentAxis.</param>
        /// <param name="e">Event arguments.</param>
        private static void OnIndependentAxisPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ColumnBarBaseSeries<T> source = (ColumnBarBaseSeries<T>)d;
            IAxis newValue = (IAxis)e.NewValue;
            source.OnIndependentAxisPropertyChanged(newValue);
        }

        /// <summary>
        /// IndependentAxisProperty property changed handler.
        /// </summary>
        /// <param name="newValue">New value.</param>
        private void OnIndependentAxisPropertyChanged(IAxis newValue)
        {
            InternalIndependentAxis = (IAxis)newValue;
        }
        #endregion public IAxis IndependentAxis

        /// <summary>
        /// Keeps a list of DataPoints that share the same category.
        /// </summary>
        private IDictionary<object, IGrouping<object, DataPoint>> _categoriesWithMultipleDataPoints;

        /// <summary>
        /// Returns the group of data points in a given category.
        /// </summary>
        /// <param name="category">The category for which to return the data
        /// point group.</param>
        /// <returns>The group of data points in a given category.</returns>
        protected IGrouping<object, DataPoint> GetDataPointGroup(object category)
        {
            return _categoriesWithMultipleDataPoints[category];
        }

        /// <summary>
        /// Returns a value indicating whether a data point corresponding to
        /// a category is grouped.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <returns>A value indicating whether a data point corresponding to
        /// a category is grouped.</returns>
        protected bool GetIsDataPointGrouped(object category)
        {
            return _categoriesWithMultipleDataPoints.ContainsKey(category);
        }

        /// <summary>
        /// The length of each data point.
        /// </summary>
        private double? _dataPointlength;

        /// <summary>
        /// Gets the dependent axis as a range axis.
        /// </summary>
        public IRangeAxis ActualDependentRangeAxis { get { return this.InternalActualDependentAxis as IRangeAxis; } }

        /// <summary>
        /// Gets the independent axis as a category axis.
        /// </summary>
        public IAxis ActualIndependentAxis { get { return this.InternalActualIndependentAxis; } }

        /// <summary>
        /// Initializes a new instance of the ColumnBarBaseSeries class.
        /// </summary>
        protected ColumnBarBaseSeries()
        {
        }

        /// <summary>
        /// Method run before DataPoints are updated.
        /// </summary>
        protected override void OnBeforeUpdateDataPoints()
        {
            base.OnBeforeUpdateDataPoints();

            CalculateDataPointLength();

            // Update the list of DataPoints with the same category
            _categoriesWithMultipleDataPoints = ActiveDataPoints
                .Where(point => null != point.IndependentValue)
                .OrderBy(point => point.DependentValue)
                .GroupBy(point => point.IndependentValue)
                .Where(grouping => 1 < grouping.Count())
                .ToDictionary(grouping => grouping.Key);
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
        /// Updates a data point when its actual dependent value has changed.
        /// </summary>
        /// <param name="dataPoint">The data point.</param>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        protected override void OnDataPointActualDependentValueChanged(DataPoint dataPoint, IComparable oldValue, IComparable newValue)
        {
            UpdateDataPoint(dataPoint);
            base.OnDataPointActualDependentValueChanged(dataPoint, oldValue, newValue);
        }

        /// <summary>
        /// Redraws other column series to assure they allocate the right amount
        /// of space for their columns.
        /// </summary>
        /// <param name="seriesHost">The series host to update.</param>
        protected void RedrawOtherSeries(ISeriesHost seriesHost)
        {
            Type thisType = typeof(ColumnBarBaseSeries<T>);

            // redraw all other column series to ensure they make space for new one
            foreach (ColumnBarBaseSeries<T> series in seriesHost.Series.Where(series => thisType.IsAssignableFrom(series.GetType())).OfType<ColumnBarBaseSeries<T>>().Where(series => series != this))
            {
                series.UpdateDataPoints(series.ActiveDataPoints);
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

            CalculateDataPointLength();

            if (this.SeriesHost != null)
            {
                RedrawOtherSeries(this.SeriesHost);
            }
        }

        /// <summary>
        /// Redraw other column series when removed from a series host.
        /// </summary>
        /// <param name="oldValue">The old value of the series host property.</param>
        /// <param name="newValue">The new value of the series host property.</param>
        protected override void OnSeriesHostPropertyChanged(ISeriesHost oldValue, ISeriesHost newValue)
        {
            base.OnSeriesHostPropertyChanged(oldValue, newValue);

            // If being removed from series host, redraw all column series.
            if (newValue == null || oldValue != null)
            {
                RedrawOtherSeries(oldValue);
            }
        }

        /// <summary>
        /// Creates the bar data point.
        /// </summary>
        /// <returns>A bar data point.</returns>
        protected override DataPoint CreateDataPoint()
        {
            return new T();
        }

        /// <summary>
        /// Calculates the length of the data points.
        /// </summary>
        protected void CalculateDataPointLength()
        {
            if (!(ActualIndependentAxis is ICategoryAxis))
            {
                IEnumerable<UnitValue> values =
                    ActiveDataPoints
                        .Select(dataPoint => ActualIndependentAxis.GetPlotAreaCoordinate(dataPoint.ActualIndependentValue))
                        .Where(value => ValueHelper.CanGraph(value.Value))
                        .OrderBy(value => value.Value)
                        .ToList();

                _dataPointlength =
                    EnumerableFunctions.Zip(
                        values,
                        values.Skip(1),
                        (left, right) => new Range<double>(left.Value, right.Value))
                        .Select(range => range.Maximum - range.Minimum)
                        .MinOrNullable();
            }
        }

        /// <summary>
        /// Returns the value margins for a given axis.
        /// </summary>
        /// <param name="consumer">The axis to retrieve the value margins for.
        /// </param>
        /// <returns>A sequence of value margins.</returns>
        protected override IEnumerable<ValueMargin> GetValueMargins(IValueMarginConsumer consumer)
        {
            double dependentValueMargin = this.ActualHeight / 10;
            IAxis axis = consumer as IAxis;
            if (axis != null && ActiveDataPoints.Any())
            {
                Func<DataPoint, IComparable> selector = null;
                if (axis == InternalActualIndependentAxis)
                {
                    selector = (dataPoint) => (IComparable)dataPoint.ActualIndependentValue;

                    DataPoint minimumPoint = ActiveDataPoints.MinOrNull(selector);
                    DataPoint maximumPoint = ActiveDataPoints.MaxOrNull(selector);

                    double minimumMargin = minimumPoint.GetMargin(axis);
                    yield return new ValueMargin(selector(minimumPoint), minimumMargin, minimumMargin);

                    double maximumMargin = maximumPoint.GetMargin(axis);
                    yield return new ValueMargin(selector(maximumPoint), maximumMargin, maximumMargin);
                }
                else if (axis == InternalActualDependentAxis)
                {
                    selector = (dataPoint) => (IComparable)dataPoint.ActualDependentValue;

                    DataPoint minimumPoint = ActiveDataPoints.MinOrNull(selector);
                    DataPoint maximumPoint = ActiveDataPoints.MaxOrNull(selector);

                    yield return new ValueMargin(selector(minimumPoint), dependentValueMargin, dependentValueMargin);
                    yield return new ValueMargin(selector(maximumPoint), dependentValueMargin, dependentValueMargin);
                }
            }
            else
            {
                yield break;
            }
        }

        /// <summary>
        /// Gets a range in which to render a data point.
        /// </summary>
        /// <param name="category">The category to retrieve the range for.
        /// </param>
        /// <returns>The range in which to render a data point.</returns>
        protected Range<UnitValue> GetCategoryRange(object category)
        {
            ICategoryAxis categoryAxis = ActualIndependentAxis as CategoryAxis;
            if (categoryAxis != null)
            {
                return categoryAxis.GetPlotAreaCoordinateRange(category);
            }
            else
            {
                UnitValue unitValue = ActualIndependentAxis.GetPlotAreaCoordinate(category);
                if (ValueHelper.CanGraph(unitValue.Value) && _dataPointlength.HasValue)
                {
                    double halfLength = _dataPointlength.Value / 2.0;

                    return new Range<UnitValue>(
                        new UnitValue(unitValue.Value - halfLength, unitValue.Unit),
                        new UnitValue(unitValue.Value + halfLength, unitValue.Unit));
                }

                return new Range<UnitValue>();
            }
        }

        /// <summary>
        /// Gets the axis to which the data is anchored.
        /// </summary>
        IRangeAxis IAnchoredToOrigin.AnchoredAxis
        {
            get { return this.ActualDependentRangeAxis; }
        }
    }
}