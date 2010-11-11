// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace System.Windows.Controls.DataVisualization.Charting
{
    /// <summary>
    /// Represents a data point used for a pie series.
    /// </summary>
    /// <QualityBand>Preview</QualityBand>
    [TemplatePart(Name = SliceName, Type = typeof(UIElement))]
    [TemplateVisualState(Name = DataPoint.StateCommonNormal, GroupName = DataPoint.GroupCommonStates)]
    [TemplateVisualState(Name = DataPoint.StateCommonMouseOver, GroupName = DataPoint.GroupCommonStates)]
    [TemplateVisualState(Name = DataPoint.StateSelectionUnselected, GroupName = DataPoint.GroupSelectionStates)]
    [TemplateVisualState(Name = DataPoint.StateSelectionSelected, GroupName = DataPoint.GroupSelectionStates)]
    [TemplateVisualState(Name = DataPoint.StateRevealShown, GroupName = DataPoint.GroupRevealStates)]
    [TemplateVisualState(Name = DataPoint.StateRevealHidden, GroupName = DataPoint.GroupRevealStates)]
    public class PieDataPoint : DataPoint
    {
        /// <summary>
        /// The name of the slice template part.
        /// </summary>
        private const string SliceName = "Slice";

        /// <summary>
        /// Name of the ActualDataPointStyle property.
        /// </summary>
        internal const string ActualDataPointStyleName = "ActualDataPointStyle";

        #region public Geometry Geometry
        /// <summary>
        /// Gets or sets the Geometry property which defines the shape of the
        /// data point.
        /// </summary>
        public Geometry Geometry
        {
            get { return GetValue(GeometryProperty) as Geometry; }
            set { SetValue(GeometryProperty, value); }
        }

        /// <summary>
        /// Identifies the Geometry dependency property.
        /// </summary>
        public static readonly DependencyProperty GeometryProperty =
            DependencyProperty.Register(
                "Geometry",
                typeof(Geometry),
                typeof(PieDataPoint),
                null);
        #endregion public Geometry Geometry

        // GeometrySelection and GeometryHighlight exist on Silverlight because
        // a single Geometry object can not be the target of multiple
        // TemplateBindings - yet the default template has 3 Paths that bind.

        #region public Geometry GeometrySelection
        /// <summary>
        /// Gets or sets the Geometry which defines the shape of a point. The 
        /// GeometrySelection property is a copy of the Geometry property.
        /// </summary>
        public Geometry GeometrySelection
        {
            get { return GetValue(GeometrySelectionProperty) as Geometry; }
            set { SetValue(GeometrySelectionProperty, value); }
        }

        /// <summary>
        /// Identifies the GeometrySelection dependency property.
        /// </summary>
        public static readonly DependencyProperty GeometrySelectionProperty =
            DependencyProperty.Register(
                "GeometrySelection",
                typeof(Geometry),
                typeof(PieDataPoint),
                null);
        #endregion public Geometry GeometrySelection

        #region public Geometry GeometryHighlight
        /// <summary>
        /// Gets or sets the GeometryHighlight property which is a clone of the
        /// Geometry property.
        /// </summary>
        public Geometry GeometryHighlight
        {
            get { return GetValue(GeometryHighlightProperty) as Geometry; }
            set { SetValue(GeometryHighlightProperty, value); }
        }

        /// <summary>
        /// Identifies the GeometryHighlight dependency property.
        /// </summary>
        public static readonly DependencyProperty GeometryHighlightProperty =
            DependencyProperty.Register(
                "GeometryHighlight",
                typeof(Geometry),
                typeof(PieDataPoint),
                null);
        #endregion public Geometry GeometryHighlight

        /// <summary>
        /// Occurs when the actual offset ratio of the pie data point changes.
        /// </summary>
        internal event RoutedPropertyChangedEventHandler<double> ActualOffsetRatioChanged;

        #region public double ActualOffsetRatio
        /// <summary>
        /// Gets or sets the offset ratio that is displayed on the screen.
        /// </summary>
        public double ActualOffsetRatio
        {
            get { return (double)GetValue(ActualOffsetRatioProperty); }
            set { SetValue(ActualOffsetRatioProperty, value); }
        }

        /// <summary>
        /// Identifies the ActualOffsetRatio dependency property.
        /// </summary>
        public static readonly DependencyProperty ActualOffsetRatioProperty =
            DependencyProperty.Register(
                "ActualOffsetRatio",
                typeof(double),
                typeof(PieDataPoint),
                new PropertyMetadata(OnActualOffsetRatioPropertyChanged));

        /// <summary>
        /// Called when the value of the ActualOffsetRatioProperty property changes.
        /// </summary>
        /// <param name="d">PieDataPoint that changed its ActualOffsetRatio.</param>
        /// <param name="e">Event arguments.</param>
        private static void OnActualOffsetRatioPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PieDataPoint source = (PieDataPoint)d;
            double oldValue = (double)e.OldValue;
            double newValue = (double)e.NewValue;
            source.OnActualOffsetRatioPropertyChanged(oldValue, newValue);
        }

        /// <summary>
        /// Called when the value of the ActualOffsetRatioProperty property changes.
        /// </summary>
        /// <param name="oldValue">The value to be replaced.</param>
        /// <param name="newValue">The new value.</param>
        private void OnActualOffsetRatioPropertyChanged(double oldValue, double newValue)
        {
            RoutedPropertyChangedEventHandler<double> handler = this.ActualOffsetRatioChanged;
            if (handler != null)
            {
                handler(this, new RoutedPropertyChangedEventArgs<double>(oldValue, newValue));
            }

            if (DesignerProperties.GetIsInDesignMode(this))
            {
                PieSeries.UpdatePieDataPointGeometry(this, ActualWidth, ActualHeight);
            }
        }
        #endregion public double ActualOffsetRatio

        /// <summary>
        /// An event raised when the actual ratio of the pie data point is
        /// changed.
        /// </summary>
        internal event RoutedPropertyChangedEventHandler<double> ActualRatioChanged;

        #region public double ActualRatio
        /// <summary>
        /// Gets or sets the ratio displayed on the screen.
        /// </summary>
        public double ActualRatio
        {
            get { return (double)GetValue(ActualRatioProperty); }
            set { SetValue(ActualRatioProperty, value); }
        }

        /// <summary>
        /// Identifies the ActualRatio dependency property.
        /// </summary>
        public static readonly DependencyProperty ActualRatioProperty =
            DependencyProperty.Register(
                "ActualRatio",
                typeof(double),
                typeof(PieDataPoint),
                new PropertyMetadata(OnActualRatioPropertyChanged));

        /// <summary>
        /// Called when the value of the ActualRatioProperty property changes.
        /// </summary>
        /// <param name="d">PieDataPoint that changed its ActualRatio.</param>
        /// <param name="e">Event arguments.</param>
        private static void OnActualRatioPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PieDataPoint source = (PieDataPoint)d;
            double oldValue = (double)e.OldValue;
            double newValue = (double)e.NewValue;
            source.OnActualRatioPropertyChanged(oldValue, newValue);
        }

        /// <summary>
        /// Called when the value of the ActualRatioProperty property changes.
        /// </summary>
        /// <param name="oldValue">The value to be replaced.</param>
        /// <param name="newValue">The new value.</param>
        private void OnActualRatioPropertyChanged(double oldValue, double newValue)
        {
            if (ValueHelper.CanGraph(newValue))
            {
                RoutedPropertyChangedEventHandler<double> handler = this.ActualRatioChanged;
                if (handler != null)
                {
                    handler(this, new RoutedPropertyChangedEventArgs<double>(oldValue, newValue));
                }
            }
            else
            {
                this.ActualRatio = 0.0;
            }

            if (DesignerProperties.GetIsInDesignMode(this))
            {
                PieSeries.UpdatePieDataPointGeometry(this, ActualWidth, ActualHeight);
            }
        }
        #endregion public double ActualRatio

        #region public string FormattedRatio
        /// <summary>
        /// Gets the Ratio with the value of the RatioStringFormat property applied.
        /// </summary>
        public string FormattedRatio
        {
            get { return GetValue(FormattedRatioProperty) as string; }
        }

        /// <summary>
        /// Identifies the FormattedRatio dependency property.
        /// </summary>
        public static readonly DependencyProperty FormattedRatioProperty =
            DependencyProperty.Register(
                "FormattedRatio",
                typeof(string),
                typeof(PieDataPoint),
                null);
        #endregion public string FormattedRatio

        /// <summary>
        /// An event raised when the offset ratio of the pie data point is
        /// changed.
        /// </summary>
        internal event RoutedPropertyChangedEventHandler<double> OffsetRatioChanged;

        #region public double OffsetRatio
        /// <summary>
        /// Gets or sets the offset ratio of the pie data point.
        /// </summary>
        public double OffsetRatio
        {
            get { return (double)GetValue(OffsetRatioProperty); }
            set { SetValue(OffsetRatioProperty, value); }
        }

        /// <summary>
        /// Identifies the OffsetRatio dependency property.
        /// </summary>
        public static readonly DependencyProperty OffsetRatioProperty =
            DependencyProperty.Register(
                "OffsetRatio",
                typeof(double),
                typeof(PieDataPoint),
                new PropertyMetadata(OnOffsetRatioPropertyChanged));

        /// <summary>
        /// Called when the value of the OffsetRatioProperty property changes.
        /// </summary>
        /// <param name="d">PieDataPoint that changed its OffsetRatio.</param>
        /// <param name="e">Event arguments.</param>
        private static void OnOffsetRatioPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PieDataPoint source = (PieDataPoint)d;
            double oldValue = (double)e.OldValue;
            double newValue = (double)e.NewValue;
            source.OnOffsetRatioPropertyChanged(oldValue, newValue);
        }

        /// <summary>
        /// Called when the value of the OffsetRatioProperty property changes.
        /// </summary>
        /// <param name="oldValue">The value to be replaced.</param>
        /// <param name="newValue">The new value.</param>
        private void OnOffsetRatioPropertyChanged(double oldValue, double newValue)
        {
            if (ValueHelper.CanGraph(newValue))
            {
                RoutedPropertyChangedEventHandler<double> handler = this.OffsetRatioChanged;
                if (handler != null)
                {
                    handler(this, new RoutedPropertyChangedEventArgs<double>(oldValue, newValue));
                }
                if (this.State == DataPointState.Created)
                {
                    ActualOffsetRatio = newValue;
                }
            }
            else
            {
                this.OffsetRatio = 0.0;
            }
        }
        #endregion public double OffsetRatio

        /// <summary>
        /// An event raised when the ratio of the pie data point is
        /// changed.
        /// </summary>
        internal event RoutedPropertyChangedEventHandler<double> RatioChanged;

        #region public double Ratio
        /// <summary>
        /// Gets or sets the ratio of the total that the data point 
        /// represents.
        /// </summary>
        public double Ratio
        {
            get { return (double)GetValue(RatioProperty); }
            set { SetValue(RatioProperty, value); }
        }

        /// <summary>
        /// Identifies the Ratio dependency property.
        /// </summary>
        public static readonly DependencyProperty RatioProperty =
            DependencyProperty.Register(
                "Ratio",
                typeof(double),
                typeof(PieDataPoint),
                new PropertyMetadata(OnRatioPropertyChanged));

        /// <summary>
        /// Called when the value of the RatioProperty property changes.
        /// </summary>
        /// <param name="d">PieDataPoint that changed its Ratio.</param>
        /// <param name="e">Event arguments.</param>
        private static void OnRatioPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PieDataPoint source = (PieDataPoint)d;
            double oldValue = (double)e.OldValue;
            double newValue = (double)e.NewValue;
            source.OnRatioPropertyChanged(oldValue, newValue);
        }

        /// <summary>
        /// Called when the value of the RatioProperty property changes.
        /// </summary>
        /// <param name="oldValue">The value to be replaced.</param>
        /// <param name="newValue">The new value.</param>
        private void OnRatioPropertyChanged(double oldValue, double newValue)
        {
            if (ValueHelper.CanGraph(newValue))
            {
                SetFormattedProperty(FormattedRatioProperty, RatioStringFormat, newValue);
                RoutedPropertyChangedEventHandler<double> handler = this.RatioChanged;
                if (handler != null)
                {
                    handler(this, new RoutedPropertyChangedEventArgs<double>(oldValue, newValue));
                }

                if (this.State == DataPointState.Created)
                {
                    ActualRatio = newValue;
                }
            }
            else
            {
                this.Ratio = 0.0;
            }
        }
        #endregion public double Ratio

        #region public string RatioStringFormat
        /// <summary>
        /// Gets or sets the format string for the FormattedRatio property.
        /// </summary>
        public string RatioStringFormat
        {
            get { return GetValue(RatioStringFormatProperty) as string; }
            set { SetValue(RatioStringFormatProperty, value); }
        }

        /// <summary>
        /// Identifies the RatioStringFormat dependency property.
        /// </summary>
        public static readonly DependencyProperty RatioStringFormatProperty =
            DependencyProperty.Register(
                "RatioStringFormat",
                typeof(string),
                typeof(PieDataPoint),
                new PropertyMetadata(null, OnRatioStringFormatPropertyChanged));

        /// <summary>
        /// Called when the value of the RatioStringFormatProperty property changes.
        /// </summary>
        /// <param name="d">PieDataPoint that changed its RatioStringFormat.</param>
        /// <param name="e">Event arguments.</param>
        private static void OnRatioStringFormatPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PieDataPoint source = d as PieDataPoint;
            string newValue = e.NewValue as string;
            source.OnRatioStringFormatPropertyChanged(newValue);
        }

        /// <summary>
        /// Called when the value of the RatioStringFormatProperty property changes.
        /// </summary>
        /// <param name="newValue">The new value.</param>
        private void OnRatioStringFormatPropertyChanged(string newValue)
        {
            SetFormattedProperty(FormattedRatioProperty, newValue, Ratio);
        }
        #endregion public string RatioStringFormat

        #region internal Style ActualDataPointStyle
        /// <summary>
        /// Gets or sets the actual style used for the data points.
        /// </summary>
        internal Style ActualDataPointStyle
        {
            get { return GetValue(ActualDataPointStyleProperty) as Style; }
            set { SetValue(ActualDataPointStyleProperty, value); }
        }

        /// <summary>
        /// Identifies the ActualDataPointStyle dependency property.
        /// </summary>
        internal static readonly DependencyProperty ActualDataPointStyleProperty =
            DependencyProperty.Register(
                ActualDataPointStyleName,
                typeof(Style),
                typeof(PieDataPoint),
                null);
        #endregion internal Style ActualDataPointStyle

        #region internal Style ActualLegendItemStyle
        /// <summary>
        /// Gets or sets the actual style used for the legend item.
        /// </summary>
        internal Style ActualLegendItemStyle
        {
            get { return GetValue(ActualLegendItemStyleProperty) as Style; }
            set { SetValue(ActualLegendItemStyleProperty, value); }
        }

        /// <summary>
        /// Identifies the ActualLegendItemStyle dependency property.
        /// </summary>
        internal static readonly DependencyProperty ActualLegendItemStyleProperty =
            DependencyProperty.Register(
                DataPointSeries.ActualLegendItemStyleName,
                typeof(Style),
                typeof(PieDataPoint),
                null);
        #endregion protected Style ActualLegendItemStyle

        /// <summary>
        /// Gets the Palette-dispensed ResourceDictionary for the Series.
        /// </summary>
        protected internal ResourceDictionary PaletteResources { get; internal set; }

        /// <summary>
        /// Gets or sets the element that represents the pie slice.
        /// </summary>
        private UIElement SliceElement { get; set; }

#if !SILVERLIGHT
        /// <summary>
        /// Initializes the static members of the PieDataPoint class.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline", Justification = "Dependency properties are initialized in-line.")]
        static PieDataPoint()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PieDataPoint), new FrameworkPropertyMetadata(typeof(PieDataPoint)));
        }

#endif
        /// <summary>
        /// Initializes a new instance of the PieDataPoint class.
        /// </summary>
        public PieDataPoint()
        {
#if SILVERLIGHT
            DefaultStyleKey = typeof(PieDataPoint);
#endif

            if (DesignerProperties.GetIsInDesignMode(this))
            {
                // Create default design-mode-friendly settings
                ActualRatio = 0.2;
                SizeChanged += delegate(object sender, SizeChangedEventArgs e)
                {
                    // Handle SizeChanged event to update Geometry dynamically
                    PieSeries.UpdatePieDataPointGeometry(this, e.NewSize.Width, e.NewSize.Height);
                };
            }
        }

        /// <summary>
        /// Builds the visual tree for the PieDataPoint when a new template is applied.
        /// </summary>
        public override void OnApplyTemplate()
        {
            if (null != SliceElement)
            {
                SliceElement.MouseEnter -= new MouseEventHandler(SliceElement_MouseEnter);
                SliceElement.MouseLeave -= new MouseEventHandler(SliceElement_MouseLeave);
            }

            base.OnApplyTemplate();

            SliceElement = GetTemplateChild(SliceName) as UIElement;

            if (null != SliceElement)
            {
                SliceElement.MouseEnter += new MouseEventHandler(SliceElement_MouseEnter);
                SliceElement.MouseLeave += new MouseEventHandler(SliceElement_MouseLeave);
            }
        }

        /// <summary>
        /// Provides handling for the MouseEnter event.
        /// </summary>
        /// <param name="e">The event data.</param>
        protected override void OnMouseEnter(MouseEventArgs e)
        {
            // Do nothing because PieDataPoint handles SliceElement.MouseEnter instead
        }

        /// <summary>
        /// Provides handling for the MouseLeave event.
        /// </summary>
        /// <param name="e">The event data.</param>
        protected override void OnMouseLeave(MouseEventArgs e)
        {
            // Do nothing because PieDataPoint handles SliceElement.MouseLeave instead
        }

        /// <summary>
        /// Provides handling for the MouseEnter event.
        /// </summary>
        /// <param name="sender">Event source.</param>
        /// <param name="e">The event data.</param>
        private void SliceElement_MouseEnter(object sender, MouseEventArgs e)
        {
            // Defer to Control's default MouseEnter handling
            base.OnMouseEnter(e);
        }

        /// <summary>
        /// Provides handling for the MouseLeave event.
        /// </summary>
        /// <param name="sender">Event source.</param>
        /// <param name="e">The event data.</param>
        private void SliceElement_MouseLeave(object sender, MouseEventArgs e)
        {
            // Defer to Control's default MouseLeave handling
            base.OnMouseLeave(e);
        }
    }
}