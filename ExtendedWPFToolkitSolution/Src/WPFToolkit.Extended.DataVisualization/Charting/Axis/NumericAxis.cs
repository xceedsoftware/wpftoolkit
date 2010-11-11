// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using EF = System.Windows.Controls.DataVisualization.EnumerableFunctions;

namespace System.Windows.Controls.DataVisualization.Charting
{
    /// <summary>
    /// An axis that displays numeric values.
    /// </summary>
    public abstract class NumericAxis : RangeAxis
    {
        #region public double? ActualMaximum
        /// <summary>
        /// Gets the actual maximum value plotted on the chart.
        /// </summary>
        public double? ActualMaximum
        {
            get { return (double?)GetValue(ActualMaximumProperty); }
            private set { SetValue(ActualMaximumProperty, value); }
        }

        /// <summary>
        /// Identifies the ActualMaximum dependency property.
        /// </summary>
        public static readonly DependencyProperty ActualMaximumProperty =
            DependencyProperty.Register(
                "ActualMaximum",
                typeof(double?),
                typeof(NumericAxis),
                null);

        #endregion public double? ActualMaximum

        #region public double? ActualMinimum
        /// <summary>
        /// Gets the actual maximum value plotted on the chart.
        /// </summary>
        public double? ActualMinimum
        {
            get { return (double?)GetValue(ActualMinimumProperty); }
            private set { SetValue(ActualMinimumProperty, value); }
        }

        /// <summary>
        /// Identifies the ActualMinimum dependency property.
        /// </summary>
        public static readonly DependencyProperty ActualMinimumProperty =
            DependencyProperty.Register(
                "ActualMinimum",
                typeof(double?),
                typeof(NumericAxis),
                null);

        #endregion public double? ActualMinimum

        #region public double? Maximum
        /// <summary>
        /// Gets or sets the maximum value plotted on the axis.
        /// </summary>
        [TypeConverter(typeof(NullableConverter<double>))]
        public double? Maximum
        {
            get { return (double?)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }

        /// <summary>
        /// Identifies the Maximum dependency property.
        /// </summary>
        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register(
                "Maximum",
                typeof(double?),
                typeof(NumericAxis),
                new PropertyMetadata(null, OnMaximumPropertyChanged));

        /// <summary>
        /// MaximumProperty property changed handler.
        /// </summary>
        /// <param name="d">NumericAxis that changed its Maximum.</param>
        /// <param name="e">Event arguments.</param>
        private static void OnMaximumPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            NumericAxis source = (NumericAxis)d;
            double? newValue = (double?)e.NewValue;
            source.OnMaximumPropertyChanged(newValue);
        }

        /// <summary>
        /// MaximumProperty property changed handler.
        /// </summary>
        /// <param name="newValue">New value.</param>
        protected virtual void OnMaximumPropertyChanged(double? newValue)
        {
            this.ProtectedMaximum = newValue;
        }
        #endregion public double? Maximum

        #region public double? Minimum
        /// <summary>
        /// Gets or sets the minimum value to plot on the axis.
        /// </summary>
        [TypeConverter(typeof(NullableConverter<double>))]
        public double? Minimum
        {
            get { return (double?)GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }

        /// <summary>
        /// Identifies the Minimum dependency property.
        /// </summary>
        public static readonly DependencyProperty MinimumProperty =
            DependencyProperty.Register(
                "Minimum",
                typeof(double?),
                typeof(NumericAxis),
                new PropertyMetadata(null, OnMinimumPropertyChanged));

        /// <summary>
        /// MinimumProperty property changed handler.
        /// </summary>
        /// <param name="d">NumericAxis that changed its Minimum.</param>
        /// <param name="e">Event arguments.</param>
        private static void OnMinimumPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            NumericAxis source = (NumericAxis)d;
            double? newValue = (double?)e.NewValue;
            source.OnMinimumPropertyChanged(newValue);
        }

        /// <summary>
        /// MinimumProperty property changed handler.
        /// </summary>
        /// <param name="newValue">New value.</param>
        protected virtual void OnMinimumPropertyChanged(double? newValue)
        {
            this.ProtectedMinimum = newValue;
        }
        #endregion public double? Minimum

        #region public bool ExtendRangeToOrigin
        /// <summary>
        /// Gets or sets a value indicating whether to always show the origin.
        /// </summary>
        public bool ExtendRangeToOrigin
        {
            get { return (bool)GetValue(ExtendRangeToOriginProperty); }
            set { SetValue(ExtendRangeToOriginProperty, value); }
        }

        /// <summary>
        /// Identifies the ExtendRangeToOrigin dependency property.
        /// </summary>
        public static readonly DependencyProperty ExtendRangeToOriginProperty =
            DependencyProperty.Register(
                "ExtendRangeToOrigin",
                typeof(bool),
                typeof(NumericAxis),
                new PropertyMetadata(false, OnExtendRangeToOriginPropertyChanged));

        /// <summary>
        /// ExtendRangeToOriginProperty property changed handler.
        /// </summary>
        /// <param name="d">NumericAxis that changed its ExtendRangeToOrigin.</param>
        /// <param name="e">Event arguments.</param>
        private static void OnExtendRangeToOriginPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            NumericAxis source = (NumericAxis)d;
            bool oldValue = (bool)e.OldValue;
            bool newValue = (bool)e.NewValue;
            source.OnExtendRangeToOriginPropertyChanged(oldValue, newValue);
        }

        /// <summary>
        /// ExtendRangeToOriginProperty property changed handler.
        /// </summary>
        /// <param name="oldValue">Old value.</param>
        /// <param name="newValue">New value.</param>
        protected virtual void OnExtendRangeToOriginPropertyChanged(bool oldValue, bool newValue)
        {
            this.ActualRange = this.OverrideDataRange(this.ActualRange);
        }
        #endregion public bool ExtendRangeToOrigin

        /// <summary>
        /// Gets the origin value on the axis.
        /// </summary>
        protected override IComparable Origin
        {
            get { return 0.0; }
        }

        /// <summary>
        /// Instantiates a new instance of the NumericAxis class.
        /// </summary>
        protected NumericAxis()
        {
        }

        /// <summary>
        /// Updates the typed actual maximum and minimum properties when the
        /// actual range changes.
        /// </summary>
        /// <param name="range">The actual range.</param>
        protected override void OnActualRangeChanged(Range<IComparable> range)
        {
            if (range.HasData)
            {
                this.ActualMaximum = (double)range.Maximum;
                this.ActualMinimum = (double)range.Minimum;
            }
            else
            {
                this.ActualMaximum = null;
                this.ActualMinimum = null;
            }

            base.OnActualRangeChanged(range);
        }

        /// <summary>
        /// Returns a value indicating whether a value can plot.
        /// </summary>
        /// <param name="value">The value to plot.</param>
        /// <returns>A value indicating whether a value can plot.</returns>
        public override bool CanPlot(object value)
        {
            double val;
            return ValueHelper.TryConvert(value, out val);
        }

        /// <summary>
        /// Returns a numeric axis label.
        /// </summary>
        /// <returns>A numeric axis label.</returns>
        protected override Control CreateAxisLabel()
        {
            return new NumericAxisLabel();
        }

        /// <summary>
        /// Overrides the data value range and returns a range that takes the
        /// margins of the values into account.
        /// </summary>
        /// <param name="range">The range of data values.</param>
        /// <returns>A range that can store both the data values and their 
        /// margins.</returns>
        protected override Range<IComparable> OverrideDataRange(Range<IComparable> range)
        {
            range = base.OverrideDataRange(range);

            if (ExtendRangeToOrigin)
            {
                Range<double> adjustedRange = range.ToDoubleRange();

                if (!adjustedRange.HasData)
                {
                    return new Range<IComparable>(0.0, 0.0);
                }
                else
                {
                    double minimum = adjustedRange.Minimum;
                    double maximum = adjustedRange.Maximum;
                    if (minimum > 0.0)
                    {
                        minimum = 0.0;
                    }
                    else if (maximum < 0.0)
                    {
                        maximum = 0.0;
                    }
                    return new Range<IComparable>(minimum, maximum);
                }
            }
            return range;
        }
    }
}