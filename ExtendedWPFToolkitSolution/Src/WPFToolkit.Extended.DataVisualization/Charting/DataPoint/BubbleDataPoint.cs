// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System.Diagnostics.CodeAnalysis;

namespace System.Windows.Controls.DataVisualization.Charting
{
    /// <summary>
    /// Represents a data point used for a bubble series.
    /// </summary>
    /// <QualityBand>Preview</QualityBand>
    [TemplateVisualState(Name = DataPoint.StateCommonNormal, GroupName = DataPoint.GroupCommonStates)]
    [TemplateVisualState(Name = DataPoint.StateCommonMouseOver, GroupName = DataPoint.GroupCommonStates)]
    [TemplateVisualState(Name = DataPoint.StateSelectionUnselected, GroupName = DataPoint.GroupSelectionStates)]
    [TemplateVisualState(Name = DataPoint.StateSelectionSelected, GroupName = DataPoint.GroupSelectionStates)]
    [TemplateVisualState(Name = DataPoint.StateRevealShown, GroupName = DataPoint.GroupRevealStates)]
    [TemplateVisualState(Name = DataPoint.StateRevealHidden, GroupName = DataPoint.GroupRevealStates)]
    public class BubbleDataPoint : DataPoint
    {
        #region public double Size
        /// <summary>
        /// Gets or sets the size value of the bubble data point.
        /// </summary>
        public double Size
        {
            get { return (double)GetValue(SizeProperty); }
            set { SetValue(SizeProperty, value); }
        }

        /// <summary>
        /// Identifies the Size dependency property.
        /// </summary>
        public static readonly DependencyProperty SizeProperty =
            DependencyProperty.Register(
                "Size",
                typeof(double),
                typeof(BubbleDataPoint),
                new PropertyMetadata(0.0, OnSizePropertyChanged));

        /// <summary>
        /// SizeProperty property changed handler.
        /// </summary>
        /// <param name="d">BubbleDataPoint that changed its Size.</param>
        /// <param name="e">Event arguments.</param>
        private static void OnSizePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            BubbleDataPoint source = (BubbleDataPoint)d;
            double oldValue = (double)e.OldValue;
            double newValue = (double)e.NewValue;
            source.OnSizePropertyChanged(oldValue, newValue);
        }

        /// <summary>
        /// SizeProperty property changed handler.
        /// </summary>
        /// <param name="oldValue">Old value.</param>
        /// <param name="newValue">New value.</param>
        private void OnSizePropertyChanged(double oldValue, double newValue)
        {
            RoutedPropertyChangedEventHandler<double> handler = SizePropertyChanged;
            if (handler != null)
            {
                handler(this, new RoutedPropertyChangedEventArgs<double>(oldValue, newValue));
            }

            if (this.State == DataPointState.Created)
            {
                this.ActualSize = newValue;
            }
        }

        /// <summary>
        /// This event is raised when the size property is changed.
        /// </summary>
        internal event RoutedPropertyChangedEventHandler<double> SizePropertyChanged;

        #endregion public double Size

        #region public double ActualSize
        /// <summary>
        /// Gets or sets the actual size of the bubble data point.
        /// </summary>
        public double ActualSize
        {
            get { return (double)GetValue(ActualSizeProperty); }
            set { SetValue(ActualSizeProperty, value); }
        }

        /// <summary>
        /// Identifies the ActualSize dependency property.
        /// </summary>
        public static readonly DependencyProperty ActualSizeProperty =
            DependencyProperty.Register(
                "ActualSize",
                typeof(double),
                typeof(BubbleDataPoint),
                new PropertyMetadata(0.0, OnActualSizePropertyChanged));

        /// <summary>
        /// ActualSizeProperty property changed handler.
        /// </summary>
        /// <param name="d">BubbleDataPoint that changed its ActualSize.</param>
        /// <param name="e">Event arguments.</param>
        private static void OnActualSizePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            BubbleDataPoint source = (BubbleDataPoint)d;
            double oldValue = (double)e.OldValue;
            double newValue = (double)e.NewValue;
            source.OnActualSizePropertyChanged(oldValue, newValue);
        }

        /// <summary>
        /// ActualSizeProperty property changed handler.
        /// </summary>
        /// <param name="oldValue">Old value.</param>
        /// <param name="newValue">New value.</param>
        private void OnActualSizePropertyChanged(double oldValue, double newValue)
        {
            RoutedPropertyChangedEventHandler<double> handler = ActualSizePropertyChanged;
            if (handler != null)
            {
                handler(this, new RoutedPropertyChangedEventArgs<double>(oldValue, newValue));
            }
        }

        /// <summary>
        /// This event is raised when the actual size property is changed.
        /// </summary>
        internal event RoutedPropertyChangedEventHandler<double> ActualSizePropertyChanged;

        #endregion public double ActualSize

#if !SILVERLIGHT
        /// <summary>
        /// Initializes the static members of the BubbleDataPoint class.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline", Justification = "Dependency properties are initialized in-line.")]
        static BubbleDataPoint()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(BubbleDataPoint), new FrameworkPropertyMetadata(typeof(BubbleDataPoint)));
        }

#endif
        /// <summary>
        /// Initializes a new instance of the bubble data point.
        /// </summary>
        public BubbleDataPoint()
        {
#if SILVERLIGHT
            this.DefaultStyleKey = typeof(BubbleDataPoint);
#endif
        }
    }
}