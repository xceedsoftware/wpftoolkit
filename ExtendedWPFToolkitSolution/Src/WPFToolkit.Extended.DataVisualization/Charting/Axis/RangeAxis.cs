// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace System.Windows.Controls.DataVisualization.Charting
{
    /// <summary>
    /// An axis that has a range.
    /// </summary>
    public abstract class RangeAxis : DisplayAxis, IRangeAxis, IValueMarginConsumer
    {
        /// <summary>
        /// A pool of major tick marks.
        /// </summary>
        private ObjectPool<Line> _majorTickMarkPool;

        /// <summary>
        /// A pool of major tick marks.
        /// </summary>
        private ObjectPool<Line> _minorTickMarkPool;

        /// <summary>
        /// A pool of labels.
        /// </summary>
        private ObjectPool<Control> _labelPool;
             
        #region public Style MinorTickMarkStyle
        /// <summary>
        /// Gets or sets the minor tick mark style.
        /// </summary>
        public Style MinorTickMarkStyle
        {
            get { return GetValue(MinorTickMarkStyleProperty) as Style; }
            set { SetValue(MinorTickMarkStyleProperty, value); }
        }

        /// <summary>
        /// Identifies the MinorTickMarkStyle dependency property.
        /// </summary>
        public static readonly DependencyProperty MinorTickMarkStyleProperty =
            DependencyProperty.Register(
                "MinorTickMarkStyle",
                typeof(Style),
                typeof(RangeAxis),
                new PropertyMetadata(null));

        #endregion public Style MinorTickMarkStyle

        /// <summary>
        /// The actual range of values.
        /// </summary>
        private Range<IComparable> _actualRange;

        /// <summary>
        /// Gets or sets the actual range of values.
        /// </summary>
        protected Range<IComparable> ActualRange
        {
            get
            {
                return _actualRange;
            }
            set
            {
                Range<IComparable> oldValue = _actualRange;
                Range<IComparable> minMaxEnforcedValue = EnforceMaximumAndMinimum(value);

                if (!oldValue.Equals(minMaxEnforcedValue))
                {
                    _actualRange = minMaxEnforcedValue;
                    OnActualRangeChanged(minMaxEnforcedValue);
                }
            }
        }

        /// <summary>
        /// The maximum value displayed in the range axis.
        /// </summary>
        private IComparable _protectedMaximum;

        /// <summary>
        /// Gets or sets the maximum value displayed in the range axis.
        /// </summary>
        protected IComparable ProtectedMaximum
        {
            get
            {
                return _protectedMaximum;
            }
            set
            {
                if (value != null && ProtectedMinimum != null && ValueHelper.Compare(ProtectedMinimum, value) > 0)
                {
                    throw new InvalidOperationException(Properties.Resources.RangeAxis_MaximumValueMustBeLargerThanOrEqualToMinimumValue);
                }
                if (!object.ReferenceEquals(_protectedMaximum, value) && !object.Equals(_protectedMaximum, value))
                {
                    _protectedMaximum = value;
                    UpdateActualRange();
                }
            }
        }

        /// <summary>
        /// The minimum value displayed in the range axis.
        /// </summary>
        private IComparable _protectedMinimum;

        /// <summary>
        /// Gets or sets the minimum value displayed in the range axis.
        /// </summary>
        protected IComparable ProtectedMinimum
        {
            get
            {
                return _protectedMinimum;
            }
            set
            {
                if (value != null && ProtectedMaximum != null && ValueHelper.Compare(value, ProtectedMaximum) > 0)
                {
                    throw new InvalidOperationException(Properties.Resources.RangeAxis_MinimumValueMustBeLargerThanOrEqualToMaximumValue);
                }
                if (!object.ReferenceEquals(_protectedMinimum, value) && !object.Equals(_protectedMinimum, value))
                {
                    _protectedMinimum = value;
                    UpdateActualRange();
                }
            }
        }

#if !SILVERLIGHT
        /// <summary>
        /// Initializes the static members of the RangeAxis class.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline", Justification = "Dependency properties are initialized in-line.")]
        static RangeAxis()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RangeAxis), new FrameworkPropertyMetadata(typeof(RangeAxis)));
        }

#endif
        /// <summary>
        /// Instantiates a new instance of the RangeAxis class.
        /// </summary>
        protected RangeAxis()
        {
#if SILVERLIGHT
            this.DefaultStyleKey = typeof(RangeAxis);
#endif
            this._labelPool = new ObjectPool<Control>(() => CreateAxisLabel());
            this._majorTickMarkPool = new ObjectPool<Line>(() => CreateMajorTickMark());
            this._minorTickMarkPool = new ObjectPool<Line>(() => CreateMinorTickMark());

            // Update actual range when size changes for the first time.  This
            // is necessary because the value margins may have changed after
            // the first layout pass.
            SizeChangedEventHandler handler = null;
            handler = delegate
            {
                SizeChanged -= handler;
                UpdateActualRange();
            };
            SizeChanged += handler;
        }

        /// <summary>
        /// Creates a minor axis tick mark.
        /// </summary>
        /// <returns>A line to used to render a tick mark.</returns>
        protected Line CreateMinorTickMark()
        {
            return CreateTickMark(MinorTickMarkStyle);
        }

        /// <summary>
        /// Invalidates axis when the actual range changes.
        /// </summary>
        /// <param name="range">The new actual range.</param>
        protected virtual void OnActualRangeChanged(Range<IComparable> range)
        {
            Invalidate();
        }

        /// <summary>
        /// Returns the plot area coordinate of a given value.
        /// </summary>
        /// <param name="value">The value to return the plot area coordinate for.</param>
        /// <returns>The plot area coordinate of the given value.</returns>
        public override UnitValue GetPlotAreaCoordinate(object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            return GetPlotAreaCoordinate(value, ActualLength);
        }

        /// <summary>
        /// Returns the plot area coordinate of a given value.
        /// </summary>
        /// <param name="value">The value to return the plot area coordinate for.</param>
        /// <param name="length">The length of the axis.</param>
        /// <returns>The plot area coordinate of the given value.</returns>
        protected abstract UnitValue GetPlotAreaCoordinate(object value, double length);

        /// <summary>
        /// Returns the plot area coordinate of a given value.
        /// </summary>
        /// <param name="value">The value to return the plot area coordinate for.</param>
        /// <param name="currentRange">The value range to use when calculating the plot area coordinate.</param>
        /// <param name="length">The length of the axis.</param>
        /// <returns>The plot area coordinate of the given value.</returns>
        protected abstract UnitValue GetPlotAreaCoordinate(object value, Range<IComparable> currentRange, double length);

        /// <summary>
        /// Overrides the data range.
        /// </summary>
        /// <param name="range">The range to potentially override.</param>
        /// <returns>The overridden range.</returns>
        protected virtual Range<IComparable> OverrideDataRange(Range<IComparable> range)
        {
            return range;
        }

        /// <summary>
        /// Modifies a range to respect the minimum and maximum axis values.
        /// </summary>
        /// <param name="range">The range of data.</param>
        /// <returns>A range modified to  respect the minimum and maximum axis 
        /// values.</returns>
        private Range<IComparable> EnforceMaximumAndMinimum(Range<IComparable> range)
        {
            if (range.HasData)
            {
                IComparable minimum = ProtectedMinimum ?? range.Minimum;
                IComparable maximum = ProtectedMaximum ?? range.Maximum;

                if (ValueHelper.Compare(minimum, maximum) > 0)
                {
                    IComparable temp = maximum;
                    maximum = minimum;
                    minimum = temp;
                }

                return new Range<IComparable>(minimum, maximum);
            }
            else
            {
                IComparable minimum = ProtectedMinimum;
                IComparable maximum = ProtectedMaximum;
                if (ProtectedMinimum != null && ProtectedMaximum == null)
                {
                    maximum = minimum;
                }
                else if (ProtectedMaximum != null && ProtectedMinimum == null)
                {
                    minimum = maximum;
                }
                else
                {
                    return range;
                }
                return new Range<IComparable>(minimum, maximum);
            }
        }

        /// <summary>
        /// Updates the actual range displayed on the axis.
        /// </summary>
        private void UpdateActualRange()
        {
            Action action = () =>
            {
                Range<IComparable> dataRange;
                if (ProtectedMaximum == null || ProtectedMinimum == null)
                {
                    if (Orientation == AxisOrientation.None)
                    {
                        if (ProtectedMinimum != null)
                        {
                            this.ActualRange = OverrideDataRange(new Range<IComparable>(ProtectedMinimum, ProtectedMinimum));
                        }
                        else
                        {
                            this.ActualRange = OverrideDataRange(new Range<IComparable>(ProtectedMaximum, ProtectedMaximum));
                        }
                    }
                    else
                    {
                        dataRange =
                            this.RegisteredListeners
                                .OfType<IRangeProvider>()
                                .Select(rangeProvider => rangeProvider.GetRange(this))
                                .Sum();

                        this.ActualRange = OverrideDataRange(dataRange);
                    }
                }
                else
                {
                    this.ActualRange = new Range<IComparable>(ProtectedMinimum, ProtectedMaximum);
                }
            };

            // Repeat this after layout pass.
            if (this.ActualLength == 0.0)
            {
                this.Dispatcher.BeginInvoke(action);
            }

            action();
        }

        /// <summary>
        /// Renders the axis as an oriented axis.
        /// </summary>
        /// <param name="availableSize">The available size.</param>
        private void RenderOriented(Size availableSize)
        {
            _minorTickMarkPool.Reset();
            _majorTickMarkPool.Reset();
            _labelPool.Reset();

            double length = GetLength(availableSize);
            try
            {
                OrientedPanel.Children.Clear();
                if (ActualRange.HasData && !Object.Equals(ActualRange.Minimum, ActualRange.Maximum))
                {
                    foreach (IComparable axisValue in GetMajorTickMarkValues(availableSize))
                    {
                        UnitValue coordinate = GetPlotAreaCoordinate(axisValue, length);
                        if (ValueHelper.CanGraph(coordinate.Value))
                        {
                            Line line = _majorTickMarkPool.Next();
                            OrientedPanel.SetCenterCoordinate(line, coordinate.Value);
                            OrientedPanel.SetPriority(line, 0);
                            OrientedPanel.Children.Add(line);
                        }
                    }

                    foreach (IComparable axisValue in GetMinorTickMarkValues(availableSize))
                    {
                        UnitValue coordinate = GetPlotAreaCoordinate(axisValue, length);
                        if (ValueHelper.CanGraph(coordinate.Value))
                        {
                            Line line = _minorTickMarkPool.Next();
                            OrientedPanel.SetCenterCoordinate(line, coordinate.Value);
                            OrientedPanel.SetPriority(line, 0);
                            OrientedPanel.Children.Add(line);
                        }
                    }

                    int count = 0;
                    foreach (IComparable axisValue in GetLabelValues(availableSize))
                    {
                        UnitValue coordinate = GetPlotAreaCoordinate(axisValue, length);
                        if (ValueHelper.CanGraph(coordinate.Value))
                        {
                            Control axisLabel = _labelPool.Next();
                            PrepareAxisLabel(axisLabel, axisValue);
                            OrientedPanel.SetCenterCoordinate(axisLabel, coordinate.Value);
                            OrientedPanel.SetPriority(axisLabel, count + 1);
                            OrientedPanel.Children.Add(axisLabel);
                            count = (count + 1) % 2;
                        }
                    }
                }
            }
            finally
            {
                _minorTickMarkPool.Done();
                _majorTickMarkPool.Done();
                _labelPool.Done();
            }
        }

        /// <summary>
        /// Renders the axis labels, tick marks, and other visual elements.
        /// </summary>
        /// <param name="availableSize">The available size.</param>
        protected override void Render(Size availableSize)
        {
            RenderOriented(availableSize);
        }

        /// <summary>
        /// Returns a sequence of the major grid line coordinates.
        /// </summary>
        /// <param name="availableSize">The available size.</param>
        /// <returns>A sequence of the major grid line coordinates.</returns>
        protected override IEnumerable<UnitValue> GetMajorGridLineCoordinates(Size availableSize)
        {
            return GetMajorTickMarkValues(availableSize).Select(value => GetPlotAreaCoordinate(value)).Where(value => ValueHelper.CanGraph(value.Value));
        }

        /// <summary>
        /// Returns a sequence of the values at which to plot major grid lines.
        /// </summary>
        /// <param name="availableSize">The available size.</param>
        /// <returns>A sequence of the values at which to plot major grid lines.
        /// </returns>
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "GridLine", Justification = "This is the expected capitalization.")]
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "This method may do a lot of work and is therefore not a suitable candidate for a property.")]
        protected virtual IEnumerable<IComparable> GetMajorGridLineValues(Size availableSize)
        {
            return GetMajorTickMarkValues(availableSize);
        }

        /// <summary>
        /// Returns a sequence of values to plot on the axis.
        /// </summary>
        /// <param name="availableSize">The available size.</param>
        /// <returns>A sequence of values to plot on the axis.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "This method may do a lot of work and is therefore not a suitable candidate for a property.")]
        protected abstract IEnumerable<IComparable> GetMajorTickMarkValues(Size availableSize);

        /// <summary>
        /// Returns a sequence of values to plot on the axis.
        /// </summary>
        /// <param name="availableSize">The available size.</param>
        /// <returns>A sequence of values to plot on the axis.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "This method may do a lot of work and is therefore not a suitable candidate for a property.")]
        protected virtual IEnumerable<IComparable> GetMinorTickMarkValues(Size availableSize)
        {
            yield break;
        }

        /// <summary>
        /// Returns a sequence of values to plot on the axis.
        /// </summary>
        /// <param name="availableSize">The available size.</param>
        /// <returns>A sequence of values to plot on the axis.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "This method may do a lot of work and is therefore not a suitable candidate for a property.")]
        protected abstract IEnumerable<IComparable> GetLabelValues(Size availableSize);

        /// <summary>
        /// Returns the value range given a plot area coordinate.
        /// </summary>
        /// <param name="value">The plot area coordinate.</param>
        /// <returns>A range of values at that plot area coordinate.</returns>
        protected abstract IComparable GetValueAtPosition(UnitValue value);

        /// <summary>
        /// Gets the actual maximum value.
        /// </summary>
        Range<IComparable> IRangeAxis.Range
        {
            get { return ActualRange; }
        }

        /// <summary>
        /// Returns the value range given a plot area coordinate.
        /// </summary>
        /// <param name="value">The plot area coordinate.</param>
        /// <returns>A range of values at that plot area coordinate.</returns>
        IComparable IRangeAxis.GetValueAtPosition(UnitValue value)
        {
            return GetValueAtPosition(value);
        }

        /// <summary>
        /// Updates the axis with information about a provider's data range.
        /// </summary>
        /// <param name="usesRangeAxis">The information provider.</param>
        /// <param name="range">The range of data in the information provider.
        /// </param>
        void IRangeConsumer.RangeChanged(IRangeProvider usesRangeAxis, Range<IComparable> range)
        {
            UpdateActualRange();
        }

        /// <summary>
        /// Updates the layout of the axis to accommodate a sequence of value
        /// margins.
        /// </summary>
        /// <param name="provider">A value margin provider.</param>
        /// <param name="valueMargins">A sequence of value margins.</param>
        void IValueMarginConsumer.ValueMarginsChanged(IValueMarginProvider provider, IEnumerable<ValueMargin> valueMargins)
        {
            Action action = () =>
                {
                    if (this.Orientation != AxisOrientation.None)
                    {
                        // Determine if any of the value margins are outside the axis
                        // area.  If so update range.
                        bool updateRange =
                            valueMargins
                                .Select(
                                    valueMargin =>
                                    {
                                        double coordinate = GetPlotAreaCoordinate(valueMargin.Value).Value;
                                        return new Range<double>(coordinate - valueMargin.LowMargin, coordinate + valueMargin.HighMargin);
                                    })
                                .Where(range => range.Minimum < 0 || range.Maximum > this.ActualLength)
                                .Any();

                        if (updateRange)
                        {
                            UpdateActualRange();
                        }
                    }
                };
            
            // Repeat this after layout pass.
            if (this.ActualLength == 0)
            {
                this.Dispatcher.BeginInvoke(action);
            }
            else
            {
                action();
            }
        }

        /// <summary>
        /// If a new range provider is registered, update actual range.
        /// </summary>
        /// <param name="series">The axis listener being registered.</param>
        protected override void OnObjectRegistered(IAxisListener series)
        {
            base.OnObjectRegistered(series);
            if (series is IRangeProvider || series is IValueMarginProvider)
            {
                UpdateActualRange();
            }
        }

        /// <summary>
        /// If a range provider is unregistered, update actual range.
        /// </summary>
        /// <param name="series">The axis listener being unregistered.</param>
        protected override void OnObjectUnregistered(IAxisListener series)
        {
            base.OnObjectUnregistered(series);
            if (series is IRangeProvider || series is IValueMarginProvider)
            {
                UpdateActualRange();
            }
        }

        /// <summary>
        /// Create function that when given a range will return the 
        /// amount in pixels by which the value margin range 
        /// overlaps.  Positive numbers represent values outside the
        /// range.
        /// </summary>
        /// <param name="valueMargins">The list of value margins, coordinates, and overlaps.</param>
        /// <param name="comparableRange">The new range to use to calculate coordinates.</param>
        internal void UpdateValueMargins(IList<ValueMarginCoordinateAndOverlap> valueMargins, Range<IComparable> comparableRange)
        {
            double actualLength = this.ActualLength;
            int valueMarginsCount = valueMargins.Count;
            for (int count = 0; count < valueMarginsCount; count++)
            {
                ValueMarginCoordinateAndOverlap item = valueMargins[count];
                item.Coordinate = GetPlotAreaCoordinate(item.ValueMargin.Value, comparableRange, actualLength).Value;
                item.LeftOverlap = -(item.Coordinate - item.ValueMargin.LowMargin);
                item.RightOverlap = (item.Coordinate + item.ValueMargin.HighMargin) - actualLength;
            }
        }

        /// <summary>
        /// Returns the value margin, coordinate, and overlap triples that have the largest left and right overlap.
        /// </summary>
        /// <param name="valueMargins">The list of value margin, coordinate, and 
        /// overlap triples.</param>
        /// <param name="maxLeftOverlapValueMargin">The value margin, 
        /// coordinate, and overlap triple that has the largest left overlap.
        /// </param>
        /// <param name="maxRightOverlapValueMargin">The value margin, 
        /// coordinate, and overlap triple that has the largest right overlap.
        /// </param>
        internal static void GetMaxLeftAndRightOverlap(IList<ValueMarginCoordinateAndOverlap> valueMargins, out ValueMarginCoordinateAndOverlap maxLeftOverlapValueMargin, out ValueMarginCoordinateAndOverlap maxRightOverlapValueMargin)
        {
            maxLeftOverlapValueMargin = new ValueMarginCoordinateAndOverlap();
            maxRightOverlapValueMargin = new ValueMarginCoordinateAndOverlap();
            double maxLeftOverlap = double.MinValue;
            double maxRightOverlap = double.MinValue;
            int valueMarginsCount = valueMargins.Count;
            for (int cnt = 0; cnt < valueMarginsCount; cnt++)
            {
                ValueMarginCoordinateAndOverlap valueMargin = valueMargins[cnt];
                double leftOverlap = valueMargin.LeftOverlap;
                if (leftOverlap > maxLeftOverlap)
                {
                    maxLeftOverlap = leftOverlap;
                    maxLeftOverlapValueMargin = valueMargin;
                }
                double rightOverlap = valueMargin.RightOverlap;
                if (rightOverlap > maxRightOverlap)
                {
                    maxRightOverlap = rightOverlap;
                    maxRightOverlapValueMargin = valueMargin;
                }
            }
        }

        /// <summary>
        /// Gets the origin value on the axis.
        /// </summary>
        IComparable IRangeAxis.Origin { get { return this.Origin; } }

        /// <summary>
        /// Gets the origin value on the axis.
        /// </summary>
        protected abstract IComparable Origin { get; }
    }
}