// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System.Collections.Generic;
using System.Linq;
using System.Windows.Data;

namespace System.Windows.Controls.DataVisualization.Charting
{
    /// <summary>
    /// Represents a control that contains a data series to be rendered in X/Y 
    /// line format.  A third binding determines the size of the data point.
    /// </summary>
    /// <QualityBand>Preview</QualityBand>
    [TemplatePart(Name = DataPointSeries.PlotAreaName, Type = typeof(Canvas))]
    [StyleTypedProperty(Property = DataPointStyleName, StyleTargetType = typeof(BubbleDataPoint))]
    [StyleTypedProperty(Property = "LegendItemStyle", StyleTargetType = typeof(LegendItem))]
    public class BubbleSeries : DataPointSingleSeriesWithAxes
    {
        /// <summary>
        /// The maximum bubble size as a ratio of the smallest dimension.
        /// </summary>
        private const double MaximumBubbleSizeAsRatioOfSmallestDimension = 0.25;

        /// <summary>
        /// The binding used to identify the size value.
        /// </summary>
        private Binding _sizeValueBinding;

        /// <summary>
        /// Gets or sets the Binding to use for identifying the size of the bubble.
        /// </summary>
        public Binding SizeValueBinding 
        {
            get
            {
                return _sizeValueBinding;
            }
            set
            {
                if (_sizeValueBinding != value)
                {
                    _sizeValueBinding = value;
                    Refresh();
                }
            }
        }

        /// <summary>
        /// Gets or sets the Binding Path to use for identifying the size of the bubble.
        /// </summary>
        public string SizeValuePath
        {
            get
            {
                return (null != SizeValueBinding) ? SizeValueBinding.Path.Path : null;
            }
            set
            {
                if (null == value)
                {
                    SizeValueBinding = null;
                }
                else
                {
                    SizeValueBinding = new Binding(value);
                }
            }
        }

        /// <summary>
        /// Stores the range of ActualSize values for the BubbleDataPoints.
        /// </summary>
        private Range<double> _rangeOfActualSizeValues = new Range<double>();

        /// <summary>
        /// Initializes a new instance of the bubble series.
        /// </summary>
        public BubbleSeries()
        {
        }

        /// <summary>
        /// Creates a new instance of bubble data point.
        /// </summary>
        /// <returns>A new instance of bubble data point.</returns>
        protected override DataPoint CreateDataPoint()
        {
            return new BubbleDataPoint();
        }

        /// <summary>
        /// Returns the custom ResourceDictionary to use for necessary resources.
        /// </summary>
        /// <returns>
        /// ResourceDictionary to use for necessary resources.
        /// </returns>
        protected override IEnumerator<ResourceDictionary> GetResourceDictionaryEnumeratorFromHost()
        {
            return GetResourceDictionaryWithTargetType(SeriesHost, typeof(BubbleDataPoint), true);
        }

        /// <summary>
        /// Acquire a horizontal linear axis and a vertical linear axis.
        /// </summary>
        /// <param name="firstDataPoint">The first data point.</param>
        protected override void GetAxes(DataPoint firstDataPoint)
        {
            GetAxes(
                firstDataPoint,
                (axis) => axis.Orientation == AxisOrientation.X,
                () =>
                {
                    IAxis axis = CreateRangeAxisFromData(firstDataPoint.IndependentValue);
                    if (axis == null)
                    {
                        axis = new CategoryAxis();
                    }
                    axis.Orientation = AxisOrientation.X;
                    return axis;
                },
                (axis) => axis.Orientation == AxisOrientation.Y && axis is IRangeAxis,
                () =>
                {
                    DisplayAxis axis = (DisplayAxis)CreateRangeAxisFromData(firstDataPoint.DependentValue);
                    if (axis == null)
                    {
                        throw new InvalidOperationException(Properties.Resources.DataPointSeriesWithAxes_NoSuitableAxisAvailableForPlottingDependentValue);
                    }
                    axis.ShowGridLines = true;
                    axis.Orientation = AxisOrientation.Y;
                    return axis;
                });
        }

        /// <summary>
        /// Prepares a bubble data point by binding the size value binding to
        /// the size property.
        /// </summary>
        /// <param name="dataPoint">The data point to prepare.</param>
        /// <param name="dataContext">The data context of the data point.
        /// </param>
        protected override void PrepareDataPoint(DataPoint dataPoint, object dataContext)
        {
            base.PrepareDataPoint(dataPoint, dataContext);

            BubbleDataPoint bubbleDataPoint = (BubbleDataPoint)dataPoint;
            bubbleDataPoint.SetBinding(BubbleDataPoint.SizeProperty, SizeValueBinding ?? DependentValueBinding ?? IndependentValueBinding);
        }

        /// <summary>
        /// Attaches size change and actual size change event handlers to the
        /// data point.
        /// </summary>
        /// <param name="dataPoint">The data point.</param>
        protected override void AttachEventHandlersToDataPoint(DataPoint dataPoint)
        {
            BubbleDataPoint bubbleDataPoint = (BubbleDataPoint)dataPoint;
            bubbleDataPoint.SizePropertyChanged += BubbleDataPointSizePropertyChanged;
            bubbleDataPoint.ActualSizePropertyChanged += BubbleDataPointActualSizePropertyChanged;
            base.AttachEventHandlersToDataPoint(dataPoint);
        }

        /// <summary>
        /// Detaches size change and actual size change event handlers from the
        /// data point.
        /// </summary>
        /// <param name="dataPoint">The data point.</param>
        protected override void DetachEventHandlersFromDataPoint(DataPoint dataPoint)
        {
            BubbleDataPoint bubbleDataPoint = (BubbleDataPoint)dataPoint;
            bubbleDataPoint.SizePropertyChanged -= BubbleDataPointSizePropertyChanged;
            bubbleDataPoint.ActualSizePropertyChanged -= BubbleDataPointActualSizePropertyChanged;
            base.DetachEventHandlersFromDataPoint(dataPoint);
        }

        /// <summary>
        /// Updates all data points when the actual size property of a data 
        /// point changes.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Information about the event.</param>
        private void BubbleDataPointActualSizePropertyChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Range<double> newRangeOfActualSizeValues = ActiveDataPoints.OfType<BubbleDataPoint>().Select(d => Math.Abs(d.ActualSize)).GetRange();
            if (newRangeOfActualSizeValues == _rangeOfActualSizeValues)
            {
                // No range change - only need to update the current point
                UpdateDataPoint((BubbleDataPoint)sender);
            }
            else
            {
                // Range has changed - need to update all points
                UpdateDataPoints(ActiveDataPoints);
            }
        }

        /// <summary>
        /// Animates the value of the ActualSize property to the size property
        /// when it changes.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Information about the event.</param>
        private void BubbleDataPointSizePropertyChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            BubbleDataPoint dataPoint = (BubbleDataPoint)sender;

            DependencyPropertyAnimationHelper.BeginAnimation(
                dataPoint, 
                BubbleDataPoint.ActualSizeProperty, 
                "ActualSize", 
                e.NewValue, 
                TransitionDuration, 
                this.TransitionEasingFunction);
        }

        /// <summary>
        /// Calculates the range of ActualSize values of all active BubbleDataPoints.
        /// </summary>
        protected override void OnBeforeUpdateDataPoints()
        {
            _rangeOfActualSizeValues = ActiveDataPoints.OfType<BubbleDataPoint>().Select(d => Math.Abs(d.ActualSize)).GetRange();
        }

        /// <summary>
        /// Ensure that if any data points are updated, all data points are 
        /// updated.
        /// </summary>
        /// <param name="dataPoints">The data points to update.</param>
        protected override void UpdateDataPoints(IEnumerable<DataPoint> dataPoints)
        {
            base.UpdateDataPoints(ActiveDataPoints);
        }

        /// <summary>
        /// Updates the data point's visual representation.
        /// </summary>
        /// <param name="dataPoint">The data point.</param>
        protected override void UpdateDataPoint(DataPoint dataPoint)
        {
            double maximumDiameter = Math.Min(PlotAreaSize.Width, PlotAreaSize.Height) * MaximumBubbleSizeAsRatioOfSmallestDimension;

            BubbleDataPoint bubbleDataPoint = (BubbleDataPoint)dataPoint;

            double ratioOfLargestBubble =
                (_rangeOfActualSizeValues.HasData && _rangeOfActualSizeValues.Maximum != 0.0 && bubbleDataPoint.ActualSize >= 0.0) ? Math.Abs(bubbleDataPoint.ActualSize) / _rangeOfActualSizeValues.Maximum : 0.0;

            bubbleDataPoint.Width = ratioOfLargestBubble * maximumDiameter;
            bubbleDataPoint.Height = ratioOfLargestBubble * maximumDiameter;

            double left =
                (ActualIndependentAxis.GetPlotAreaCoordinate(bubbleDataPoint.ActualIndependentValue)).Value
                    - (bubbleDataPoint.Width / 2.0);

            double top =
                (PlotAreaSize.Height
                    - (bubbleDataPoint.Height / 2.0))
                    - ActualDependentRangeAxis.GetPlotAreaCoordinate(bubbleDataPoint.ActualDependentValue).Value;

            if (ValueHelper.CanGraph(left) && ValueHelper.CanGraph(top))
            {
                dataPoint.Visibility = Visibility.Visible;

                Canvas.SetLeft(bubbleDataPoint, left);
                Canvas.SetTop(bubbleDataPoint, top);
            }
            else
            {
                dataPoint.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Updates the value margins after all data points are updated.
        /// </summary>
        protected override void OnAfterUpdateDataPoints()
        {
            IValueMarginProvider provider = this as IValueMarginProvider;
            {
                IValueMarginConsumer consumer = ActualDependentRangeAxis as IValueMarginConsumer;
                if (consumer != null)
                {
                    consumer.ValueMarginsChanged(provider, GetValueMargins(consumer));
                }
            }
            {
                IValueMarginConsumer consumer = ActualIndependentAxis as IValueMarginConsumer;
                if (consumer != null)
                {
                    consumer.ValueMarginsChanged(provider, GetValueMargins(consumer));
                }
            }
            base.OnAfterUpdateDataPoints();
        }

        /// <summary>
        /// Gets the dependent axis as a range axis.
        /// </summary>
        public IRangeAxis ActualDependentRangeAxis { get { return this.InternalActualDependentAxis as IRangeAxis; } }

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
        public static readonly DependencyProperty DependentRangeAxisProperty =
            DependencyProperty.Register(
                "DependentRangeAxis",
                typeof(IRangeAxis),
                typeof(BubbleSeries),
                new PropertyMetadata(null, OnDependentRangeAxisPropertyChanged));

        /// <summary>
        /// DependentRangeAxisProperty property changed handler.
        /// </summary>
        /// <param name="d">BubbleSeries that changed its DependentRangeAxis.</param>
        /// <param name="e">Event arguments.</param>
        private static void OnDependentRangeAxisPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            BubbleSeries source = (BubbleSeries)d;
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

        /// <summary>
        /// Gets the independent axis as a range axis.
        /// </summary>
        public IAxis ActualIndependentAxis { get { return this.InternalActualIndependentAxis as IAxis; } }

        #region public IAxis IndependentAxis
        /// <summary>
        /// Gets or sets independent range axis.
        /// </summary>
        public IAxis IndependentAxis
        {
            get { return GetValue(IndependentAxisProperty) as IAxis; }
            set { SetValue(IndependentAxisProperty, value); }
        }

        /// <summary>
        /// Identifies the IndependentAxis dependency property.
        /// </summary>
        public static readonly DependencyProperty IndependentAxisProperty =
            DependencyProperty.Register(
                "IndependentAxis",
                typeof(IAxis),
                typeof(BubbleSeries),
                new PropertyMetadata(null, OnIndependentAxisPropertyChanged));

        /// <summary>
        /// IndependentAxisProperty property changed handler.
        /// </summary>
        /// <param name="d">BubbleSeries that changed its IndependentAxis.</param>
        /// <param name="e">Event arguments.</param>
        private static void OnIndependentAxisPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            BubbleSeries source = (BubbleSeries)d;
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
        /// The margins required for each value.
        /// </summary>
        /// <param name="consumer">The consumer to return the value margins for.</param>
        /// <returns>A sequence of margins for each value.</returns>
        protected override IEnumerable<ValueMargin> GetValueMargins(IValueMarginConsumer consumer)
        {
            IAxis axis = consumer as IAxis;
            if (axis != null)
            {
                return ActiveDataPoints.Select(dataPoint =>
                {
                    double margin = dataPoint.GetMargin(axis);
                    return new ValueMargin(
                        GetActualDataPointAxisValue(dataPoint, axis), 
                        margin,
                        margin);
                });
            }

            return Enumerable.Empty<ValueMargin>();
        }
    }
}