// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.DataVisualization;
using System.Windows.Controls.DataVisualization.Collections;

namespace System.Windows.Controls.DataVisualization.Charting
{
    /// <summary>
    /// Represents a dynamic series that uses axes to display data points.
    /// </summary>
    /// <QualityBand>Preview</QualityBand>
    public abstract class DataPointSeriesWithAxes : DataPointSeries, IDataProvider, IRangeProvider, IAxisListener, IValueMarginProvider
    {
        /// <summary>
        /// Gets or sets the data points by dependent value.
        /// </summary>
        private OrderedMultipleDictionary<IComparable, DataPoint> DataPointsByActualDependentValue { get; set; }

        /// <summary>
        /// Creates the correct range axis based on the data.
        /// </summary>
        /// <param name="value">The value to evaluate to determine which type of
        /// axis to create.</param>
        /// <returns>The range axis appropriate that can plot the provided
        /// value.</returns>
        protected static IRangeAxis CreateRangeAxisFromData(object value)
        {
            double doubleValue;
            DateTime dateTime;
            if (ValueHelper.TryConvert(value, out doubleValue))
            {
                return new LinearAxis();
            }
            else if (ValueHelper.TryConvert(value, out dateTime))
            {
                return new DateTimeAxis();
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Retrieves the value for a given access from a data point.
        /// </summary>
        /// <param name="dataPoint">The data point to retrieve the value from.</param>
        /// <param name="axis">The axis to retrieve the value for.</param>
        /// <returns>A function that returns a value appropriate for the axis
        /// when provided a DataPoint.</returns>
        protected virtual object GetActualDataPointAxisValue(DataPoint dataPoint, IAxis axis)
        {
            if (axis == InternalActualIndependentAxis)
            {
                return dataPoint.ActualIndependentValue;
            }
            else if (axis == InternalActualDependentAxis)
            {
                return dataPoint.ActualDependentValue;
            }
            return null;
        }

        /// <summary>
        /// Gets or sets the actual dependent axis.
        /// </summary>
        protected IAxis InternalActualDependentAxis { get; set; }

        #region public Axis InternalDependentAxis

        /// <summary>
        /// Stores the internal dependent axis.
        /// </summary>
        private IAxis _internalDependentAxis;

        /// <summary>
        /// Gets or sets the value of the internal dependent axis.
        /// </summary>
        protected IAxis InternalDependentAxis
        {
            get { return _internalDependentAxis; }
            set 
            {
                if (_internalDependentAxis != value)
                {
                    IAxis oldValue = _internalDependentAxis;
                    _internalDependentAxis = value;
                    OnInternalDependentAxisPropertyChanged(oldValue, value);
                }
            }
        }

        /// <summary>
        /// DependentAxisProperty property changed handler.
        /// </summary>
        /// <param name="oldValue">Old value.</param>
        /// <param name="newValue">New value.</param>
        protected virtual void OnInternalDependentAxisPropertyChanged(IAxis oldValue, IAxis newValue)
        {
            if (newValue != null 
                && InternalActualDependentAxis != null 
                && InternalActualDependentAxis != newValue 
                && InternalActualDependentAxis.RegisteredListeners.Contains(this))
            {
                InternalActualDependentAxis.RegisteredListeners.Remove(this);
                InternalActualDependentAxis = null;
                GetAxes();
            }
        }
        #endregion public Axis InternalDependentAxis

        /// <summary>
        /// Gets or sets the actual independent axis value.
        /// </summary>
        protected IAxis InternalActualIndependentAxis { get; set; }

        #region protected Axis InternalIndependentAxis

        /// <summary>
        /// The internal independent axis.
        /// </summary>
        private IAxis _internalIndependentAxis;

        /// <summary>
        /// Gets or sets the value of the internal independent axis.
        /// </summary>
        protected IAxis InternalIndependentAxis
        {
            get { return _internalIndependentAxis; }
            set 
            {
                if (value != _internalIndependentAxis)
                {
                    IAxis oldValue = _internalIndependentAxis;
                    _internalIndependentAxis = value;
                    OnInternalIndependentAxisPropertyChanged(oldValue, value);
                }
            }
        }

        /// <summary>
        /// IndependentAxisProperty property changed handler.
        /// </summary>
        /// <param name="oldValue">Old value.</param>
        /// <param name="newValue">New value.</param>
        protected virtual void OnInternalIndependentAxisPropertyChanged(IAxis oldValue, IAxis newValue)
        {
            if (newValue != null
                && InternalActualIndependentAxis != null
                && InternalActualIndependentAxis != newValue
                && InternalActualIndependentAxis.RegisteredListeners.Contains(this))
            {
                InternalActualIndependentAxis.RegisteredListeners.Remove(this);
                InternalActualIndependentAxis = null;
                GetAxes();
            }
        }
        #endregion protected Axis IndependentAxis

        /// <summary>
        /// Initializes a new instance of the DataPointSeriesWithAxes class.
        /// </summary>
        protected DataPointSeriesWithAxes()
        {
            this.DataPointsByActualDependentValue =
                new OrderedMultipleDictionary<IComparable, DataPoint>(
                    false,
                    (left, right) => 
                        left.CompareTo(right),
                    (leftDataPoint, rightDataPoint) => 
                        RuntimeHelpers.GetHashCode(leftDataPoint).CompareTo(RuntimeHelpers.GetHashCode(rightDataPoint)));
        }

        /// <summary>
        /// Update the axes when the specified data point's ActualDependentValue property changes.
        /// </summary>
        /// <param name="dataPoint">The data point.</param>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        protected override void OnDataPointActualDependentValueChanged(DataPoint dataPoint, IComparable oldValue, IComparable newValue)
        {
            if (oldValue != null)
            {
                bool removed = DataPointsByActualDependentValue.Remove(oldValue, dataPoint);
                if (removed)
                {
                    DataPointsByActualDependentValue.Add(newValue, dataPoint);
                }
            }

            UpdateActualDependentAxis();
            UpdateDataPoint(dataPoint);
            base.OnDataPointActualDependentValueChanged(dataPoint, oldValue, newValue);
        }

        /// <summary>
        /// Update the axes when the specified data point's DependentValue property changes.
        /// </summary>
        /// <param name="dataPoint">The data point.</param>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        protected override void OnDataPointDependentValueChanged(DataPoint dataPoint, IComparable oldValue, IComparable newValue)
        {
            if ((null != InternalActualDependentAxis))
            {
                dataPoint.BeginAnimation(DataPoint.ActualDependentValueProperty, "ActualDependentValue", newValue, this.TransitionDuration, this.TransitionEasingFunction);
            }
            else
            {
                dataPoint.ActualDependentValue = newValue;
            }
            base.OnDataPointDependentValueChanged(dataPoint, oldValue, newValue);
        }

        /// <summary>
        /// Update axes when the specified data point's effective dependent value changes.
        /// </summary>
        private void UpdateActualDependentAxis()
        {
            if (InternalActualDependentAxis != null)
            {
                IDataConsumer dataConsumer = InternalActualDependentAxis as IDataConsumer;
                if (dataConsumer != null)
                {
                    IDataProvider categoryInformationProvider = (IDataProvider)this;
                    dataConsumer.DataChanged(categoryInformationProvider, categoryInformationProvider.GetData(dataConsumer));
                }

                IRangeConsumer rangeAxis = InternalActualDependentAxis as IRangeConsumer;
                if (rangeAxis != null)
                {
                    IRangeProvider rangeInformationProvider = (IRangeProvider)this;
                    rangeAxis.RangeChanged(rangeInformationProvider, rangeInformationProvider.GetRange(rangeAxis));
                }
            }
        }

        /// <summary>
        /// Update axes when the specified data point's actual independent value changes.
        /// </summary>
        /// <param name="dataPoint">The data point.</param>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        protected override void OnDataPointActualIndependentValueChanged(DataPoint dataPoint, object oldValue, object newValue)
        {
            UpdateActualIndependentAxis();
            UpdateDataPoint(dataPoint);
            base.OnDataPointActualIndependentValueChanged(dataPoint, oldValue, newValue);
        }

        /// <summary>
        /// Update axes when the specified data point's independent value changes.
        /// </summary>
        /// <param name="dataPoint">The data point.</param>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        protected override void OnDataPointIndependentValueChanged(DataPoint dataPoint, object oldValue, object newValue)
        {
            if ((null != InternalActualIndependentAxis) && (InternalActualIndependentAxis is IRangeAxis))
            {
                dataPoint.BeginAnimation(DataPoint.ActualIndependentValueProperty, "ActualIndependentValue", newValue, this.TransitionDuration, this.TransitionEasingFunction);
            }
            else
            {
                dataPoint.ActualIndependentValue = newValue;
            }
            base.OnDataPointIndependentValueChanged(dataPoint, oldValue, newValue);
        }

        /// <summary>
        /// Update axes when a data point's effective independent value changes.
        /// </summary>
        private void UpdateActualIndependentAxis()
        {
            if (InternalActualIndependentAxis != null)
            {
                ICategoryAxis categoryAxis = InternalActualIndependentAxis as ICategoryAxis;
                if (categoryAxis != null)
                {
                    IDataProvider categoryInformationProvider = (IDataProvider)this;
                    categoryAxis.DataChanged(categoryInformationProvider, categoryInformationProvider.GetData(categoryAxis));
                }
                IRangeConsumer rangeAxis = InternalActualIndependentAxis as IRangeConsumer;
                if (rangeAxis != null)
                {
                    IRangeProvider rangeInformationProvider = (IRangeProvider)this;
                    rangeAxis.RangeChanged(rangeInformationProvider, rangeInformationProvider.GetRange(rangeAxis));
                }
            }
        }

        /// <summary>
        /// Called after data points have been loaded from the items source.
        /// </summary>
        /// <param name="newDataPoints">New active data points.</param>
        /// <param name="oldDataPoints">Old inactive data points.</param>
        protected override void OnDataPointsChanged(IList<DataPoint> newDataPoints, IList<DataPoint> oldDataPoints)
        {
            foreach (DataPoint dataPoint in newDataPoints)
            {
                DataPointsByActualDependentValue.Add(dataPoint.ActualDependentValue, dataPoint);
            }

            foreach (DataPoint dataPoint in oldDataPoints)
            {
                DataPointsByActualDependentValue.Remove(dataPoint.ActualDependentValue, dataPoint);
            }

            GetAxes();

            if (InternalActualDependentAxis != null && InternalActualIndependentAxis != null)
            {
                Action action = () =>
                    {
                        AxesInvalidated = false;
                        UpdatingAllAxes = true;
                        try
                        {
                            UpdateActualIndependentAxis();
                            UpdateActualDependentAxis();
                        }
                        finally
                        {
                            UpdatingAllAxes = false;
                        }

                        if (AxesInvalidated)
                        {
                            UpdateDataPoints(ActiveDataPoints);
                        }
                        else
                        {
                            UpdateDataPoints(newDataPoints);
                        }

                        AxesInvalidated = false;
                    };

                InvokeOnLayoutUpdated(action);
            }

            base.OnDataPointsChanged(newDataPoints, oldDataPoints);
        }

        /// <summary>
        /// Gets or sets a value indicating whether to the axes are being 
        /// updated.
        /// </summary>
        private bool UpdatingAllAxes { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the axes have been 
        /// invalidated.
        /// </summary>
        private bool AxesInvalidated { get; set; }

        /// <summary>
        /// Only updates all data points if series has axes.
        /// </summary>
        /// <param name="dataPoints">A sequence of data points to update.
        /// </param>
        protected override void UpdateDataPoints(IEnumerable<DataPoint> dataPoints)
        {
            if (InternalActualIndependentAxis != null && InternalActualDependentAxis != null)
            {
                base.UpdateDataPoints(dataPoints);
            }
        }

        /// <summary>
        /// Method called to get series to acquire the axes it needs.  Acquires
        /// no axes by default.
        /// </summary>
        private void GetAxes()
        {
            if (SeriesHost != null)
            {
                DataPoint firstDataPoint = ActiveDataPoints.FirstOrDefault();
                if (firstDataPoint == null)
                {
                    return;
                }

                GetAxes(firstDataPoint);
            }
        }

        /// <summary>
        /// Method called to get series to acquire the axes it needs.  Acquires
        /// no axes by default.
        /// </summary>
        /// <param name="firstDataPoint">The first data point.</param>
        protected abstract void GetAxes(DataPoint firstDataPoint);

        /// <summary>
        /// Method called to get the axes that the series needs.
        /// </summary>
        /// <param name="firstDataPoint">The first data point.</param>
        /// <param name="independentAxisPredicate">A predicate that returns
        /// a value indicating whether an axis is an acceptable candidate for
        /// the series independent axis.</param>
        /// <param name="independentAxisFactory">A function that creates an
        /// acceptable independent axis.</param>
        /// <param name="dependentAxisPredicate">A predicate that returns
        /// a value indicating whether an axis is an acceptable candidate for
        /// the series dependent axis.</param>
        /// <param name="dependentAxisFactory">A function that creates an
        /// acceptable dependent axis.</param>
        protected virtual void GetAxes(DataPoint firstDataPoint, Func<IAxis, bool> independentAxisPredicate, Func<IAxis> independentAxisFactory, Func<IAxis, bool> dependentAxisPredicate, Func<IAxis> dependentAxisFactory)
        {
            Func<IAxis, bool> actualIndependentAxisPredicate = (axis) => independentAxisPredicate(axis) && axis.CanPlot(firstDataPoint.IndependentValue);
            IAxis workingIndependentAxis = null;
            if (this.InternalActualIndependentAxis == null)
            {
                if (this.InternalIndependentAxis != null)
                {
                    if (actualIndependentAxisPredicate(this.InternalIndependentAxis))
                    {
                        workingIndependentAxis = this.InternalIndependentAxis;
                    }
                    else
                    {
                        throw new InvalidOperationException(Properties.Resources.DataPointSeriesWithAxes_GetAxes_AssignedIndependentAxisCannotBeUsed);
                    }
                }

                if (workingIndependentAxis == null)
                {
                    workingIndependentAxis = this.SeriesHost.Axes.FirstOrDefault(actualIndependentAxisPredicate);
                }

                if (workingIndependentAxis == null)
                {
                    workingIndependentAxis = independentAxisFactory();
                }

                this.InternalActualIndependentAxis = workingIndependentAxis;

                if (!workingIndependentAxis.RegisteredListeners.Contains(this))
                {
                    workingIndependentAxis.RegisteredListeners.Add(this);
                }
                if (!this.SeriesHost.Axes.Contains(workingIndependentAxis))
                {
                    this.SeriesHost.Axes.Add(workingIndependentAxis);
                }
            }

            Func<IAxis, bool> actualDependentAxisPredicate = (axis) => dependentAxisPredicate(axis) && axis.CanPlot(firstDataPoint.DependentValue);
            IAxis workingDependentAxis = null;
            if (this.InternalActualDependentAxis == null)
            {
                if (this.InternalDependentAxis != null)
                {
                    if (actualDependentAxisPredicate(this.InternalDependentAxis))
                    {
                        workingDependentAxis = this.InternalDependentAxis;
                    }
                    else
                    {
                        throw new InvalidOperationException(Properties.Resources.DataPointSeriesWithAxes_GetAxes_AssignedDependentAxisCannotBeUsed);
                    }
                }

                if (workingDependentAxis == null)
                {
                    workingDependentAxis = InternalActualIndependentAxis.DependentAxes.Concat(this.SeriesHost.Axes).FirstOrDefault(actualDependentAxisPredicate);
                }

                if (workingDependentAxis == null)
                {
                    workingDependentAxis = dependentAxisFactory();
                }

                this.InternalActualDependentAxis = workingDependentAxis;

                if (!workingDependentAxis.RegisteredListeners.Contains(this))
                {
                    workingDependentAxis.RegisteredListeners.Add(this);
                }

                // Only add axis to the axes collection of the series host if 
                // it is not a dependent axis belonging to the acquired 
                // independent axis.
                if (!this.SeriesHost.Axes.Contains(workingDependentAxis) && !InternalActualIndependentAxis.DependentAxes.Contains(workingDependentAxis))
                {
                    this.SeriesHost.Axes.Add(workingDependentAxis);
                }
            }
        }

        /// <summary>
        /// Updates data points when the axis is invalidated.
        /// </summary>
        /// <param name="axis">The axis that was invalidated.</param>
        void IAxisListener.AxisInvalidated(IAxis axis)
        {
            if (InternalActualDependentAxis != null && InternalActualIndependentAxis != null && PlotArea != null)
            {
                if (!UpdatingAllAxes)
                {
                    UpdateDataPoints(ActiveDataPoints);
                }
                else
                {
                    AxesInvalidated = true;
                }
            }
        }

        /// <summary>
        /// Returns the actual range of data for a given axis.
        /// </summary>
        /// <param name="consumer">The axis to retrieve the range for.</param>
        /// <returns>The actual range of data.</returns>
        protected virtual Range<IComparable> GetRange(IRangeConsumer consumer)
        {
            if (consumer == null)
            {
                throw new ArgumentNullException("consumer");
            }

            if (consumer == InternalActualDependentAxis)
            {
                if (this.DataPointsByActualDependentValue.Count > 0)
                {
                    return this.DataPointsByActualDependentValue.GetKeyRange();
                }
            }

            IAxis axis = consumer as IAxis;
            return (axis != null)
                ? ActiveDataPoints.Select(dataPoint => (IComparable)GetActualDataPointAxisValue(dataPoint, axis)).GetRange() 
                : new Range<IComparable>();
        }

        /// <summary>
        /// Returns the value margins for a given axis.
        /// </summary>
        /// <param name="consumer">The axis to retrieve the value margins for.
        /// </param>
        /// <returns>A sequence of value margins.</returns>
        protected virtual IEnumerable<ValueMargin> GetValueMargins(IValueMarginConsumer consumer)
        {
            IAxis axis = consumer as IAxis;
            if (axis != null && ActiveDataPoints.Any())
            {
                Func<DataPoint, IComparable> selector = null;
                DataPoint minimumPoint = null;
                DataPoint maximumPoint = null;
                double margin = 0.0;
                if (axis == InternalActualIndependentAxis)
                {
                    selector = (dataPoint) => (IComparable)dataPoint.ActualIndependentValue;

                    minimumPoint = ActiveDataPoints.MinOrNull(selector);
                    maximumPoint = ActiveDataPoints.MaxOrNull(selector);
                    margin = minimumPoint.GetActualMargin(this.InternalActualIndependentAxis);
                }
                else if (axis == InternalActualDependentAxis)
                {
                    selector = (dataPoint) => (IComparable)dataPoint.ActualDependentValue;

                    Tuple<DataPoint, DataPoint> largestAndSmallestValues = this.DataPointsByActualDependentValue.GetLargestAndSmallestValues();
                    minimumPoint = largestAndSmallestValues.Item1;
                    maximumPoint = largestAndSmallestValues.Item2;
                    margin = minimumPoint.GetActualMargin(this.InternalActualDependentAxis);
                }
                
                yield return new ValueMargin(selector(minimumPoint), margin, margin);
                yield return new ValueMargin(selector(maximumPoint), margin, margin);
            }
            else
            {
                yield break;
            }
        }

        /// <summary>
        /// Returns data to a data consumer.
        /// </summary>
        /// <param name="dataConsumer">The data consumer requesting the data.
        /// </param>
        /// <returns>The data for a given data consumer.</returns>
        IEnumerable<object> IDataProvider.GetData(IDataConsumer dataConsumer)
        {
            IAxis axis = (IAxis)dataConsumer;
            if (axis == null)
            {
                throw new ArgumentNullException("dataConsumer");
            }

            Func<DataPoint, object> selector = null;
            if (axis == InternalActualIndependentAxis)
            {
                if (IndependentValueBinding == null)
                {
                    return Enumerable.Range(1, ActiveDataPointCount).CastWrapper<object>();
                }
                selector = (dataPoint) => dataPoint.ActualIndependentValue ?? dataPoint.ActualDependentValue;
            }
            else if (axis == InternalActualDependentAxis)
            {
                selector = (dataPoint) => dataPoint.ActualDependentValue;
            }

            return ActiveDataPoints.Select(selector).Distinct();
        }

        /// <summary>
        /// Called when the value of the SeriesHost property changes.
        /// </summary>
        /// <param name="oldValue">The value to be replaced.</param>
        /// <param name="newValue">The new series host value.</param>
        protected override void OnSeriesHostPropertyChanged(ISeriesHost oldValue, ISeriesHost newValue)
        {
            if (oldValue != null)
            {
                if (InternalActualIndependentAxis != null)
                {
                    InternalActualIndependentAxis.RegisteredListeners.Remove(this);
                    InternalActualIndependentAxis = null;
                }
                if (InternalActualDependentAxis != null)
                {
                    InternalActualDependentAxis.RegisteredListeners.Remove(this);
                    InternalActualDependentAxis = null;
                }
            }

            base.OnSeriesHostPropertyChanged(oldValue, newValue);
        }

        /// <summary>
        /// Returns the data range.
        /// </summary>
        /// <param name="rangeConsumer">The consumer requesting the range.</param>
        /// <returns>The data range.</returns>
        Range<IComparable> IRangeProvider.GetRange(IRangeConsumer rangeConsumer)
        {
            return GetRange(rangeConsumer);
        }

        /// <summary>
        /// Returns the value margins for a given axis.
        /// </summary>
        /// <param name="axis">The axis to retrieve the value margins for.
        /// </param>
        /// <returns>A sequence of value margins.</returns>
        IEnumerable<ValueMargin> IValueMarginProvider.GetValueMargins(IValueMarginConsumer axis)
        {
            return GetValueMargins(axis);
        }
    }
}