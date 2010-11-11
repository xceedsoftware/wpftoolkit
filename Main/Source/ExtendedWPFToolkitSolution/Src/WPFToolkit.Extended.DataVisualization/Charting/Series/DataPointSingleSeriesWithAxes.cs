// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Windows.Data;

namespace System.Windows.Controls.DataVisualization.Charting
{
    /// <summary>
    /// A dynamic series with axes and only one legend item and style for all 
    /// data points.
    /// </summary>
    /// <QualityBand>Preview</QualityBand>
    public abstract class DataPointSingleSeriesWithAxes : DataPointSeriesWithAxes, IRequireGlobalSeriesIndex
    {
        /// <summary>
        /// Name of the ActualDataPointStyle property.
        /// </summary>
        protected const string ActualDataPointStyleName = "ActualDataPointStyle";

        /// <summary>
        /// Gets the single legend item associated with the series.
        /// </summary>
        protected LegendItem LegendItem
        {
            get
            {
                if (null == _legendItem)
                {
                    _legendItem = CreateLegendItem(this);
                    LegendItems.Add(_legendItem);
                }
                return _legendItem;
            }
        }

        /// <summary>
        /// Stores the LegendItem for the series.
        /// </summary>
        private LegendItem _legendItem;

        /// <summary>
        /// Gets the Palette-dispensed ResourceDictionary for the Series.
        /// </summary>
        protected ResourceDictionary PaletteResources { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether a custom title is in use.
        /// </summary>
        private bool CustomTitleInUse { get; set; }

        /// <summary>
        /// DataPointStyleProperty property changed handler.
        /// </summary>
        /// <param name="oldValue">Old value.</param>
        /// <param name="newValue">New value.</param>
        protected override void OnDataPointStylePropertyChanged(Style oldValue, Style newValue)
        {
            // Propagate change
            ActualDataPointStyle = newValue;
            base.OnDataPointStylePropertyChanged(oldValue, newValue);
        }

        #region protected Style ActualDataPointStyle
        /// <summary>
        /// Gets or sets the actual style used for the data points.
        /// </summary>
        protected Style ActualDataPointStyle
        {
            get { return GetValue(ActualDataPointStyleProperty) as Style; }
            set { SetValue(ActualDataPointStyleProperty, value); }
        }

        /// <summary>
        /// Identifies the ActualDataPointStyle dependency property.
        /// </summary>
        protected static readonly DependencyProperty ActualDataPointStyleProperty =
            DependencyProperty.Register(
                ActualDataPointStyleName,
                typeof(Style),
                typeof(DataPointSingleSeriesWithAxes),
                null);
        #endregion protected Style ActualDataPointStyle

        #region protected Style ActualLegendItemStyle
        /// <summary>
        /// Gets or sets the actual style used for the legend item.
        /// </summary>
        protected Style ActualLegendItemStyle
        {
            get { return GetValue(ActualLegendItemStyleProperty) as Style; }
            set { SetValue(ActualLegendItemStyleProperty, value); }
        }

        /// <summary>
        /// Identifies the ActualLegendItemStyle dependency property.
        /// </summary>
        protected static readonly DependencyProperty ActualLegendItemStyleProperty =
            DependencyProperty.Register(
                ActualLegendItemStyleName,
                typeof(Style),
                typeof(DataPointSeries),
                null);
        #endregion protected Style ActualLegendItemStyle

        /// <summary>
        /// Called when the value of the LegendItemStyle property changes.
        /// </summary>
        /// <param name="oldValue">Old value.</param>
        /// <param name="newValue">New value.</param>
        protected override void OnLegendItemStylePropertyChanged(Style oldValue, Style newValue)
        {
            // Propagate change
            ActualLegendItemStyle = newValue;
            base.OnLegendItemStylePropertyChanged(oldValue, newValue);
        }

        #region public int? GlobalSeriesIndex
        /// <summary>
        /// Gets the index of the series in the Parent's series collection.
        /// </summary>
        public int? GlobalSeriesIndex
        {
            get { return (int?)GetValue(GlobalSeriesIndexProperty); }
            private set { SetValue(GlobalSeriesIndexProperty, value); }
        }

        /// <summary>
        /// Identifies the GlobalSeriesIndex dependency property.
        /// </summary>
        public static readonly DependencyProperty GlobalSeriesIndexProperty =
            DependencyProperty.Register(
                "GlobalSeriesIndex",
                typeof(int?),
                typeof(Series),
                new PropertyMetadata(new int?(), OnGlobalSeriesIndexPropertyChanged));

        /// <summary>
        /// GlobalSeriesIndexProperty property changed handler.
        /// </summary>
        /// <param name="d">Series that changed its Index.</param>
        /// <param name="e">Event arguments.</param>
        private static void OnGlobalSeriesIndexPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DataPointSingleSeriesWithAxes source = (DataPointSingleSeriesWithAxes)d;
            int? oldValue = (int?)e.OldValue;
            int? newValue = (int?)e.NewValue;
            source.OnGlobalSeriesIndexPropertyChanged(oldValue, newValue);
        }

        /// <summary>
        /// GlobalSeriesIndexProperty property changed handler.
        /// </summary>
        /// <param name="oldValue">Old value.</param>
        /// <param name="newValue">New value.</param>
        [SuppressMessage("Microsoft.Usage", "CA2233:OperationsShouldNotOverflow", MessageId = "newValue+1", Justification = "Impractical to add as many Series as it would take to overflow.")]
        protected virtual void OnGlobalSeriesIndexPropertyChanged(int? oldValue, int? newValue)
        {
            if (!CustomTitleInUse && (null == GetBindingExpression(TitleProperty)))
            {
                Title = newValue.HasValue ? string.Format(CultureInfo.CurrentCulture, Properties.Resources.Series_OnGlobalSeriesIndexPropertyChanged_UntitledSeriesFormatString, newValue.Value + 1) : null;
                // Setting Title will set CustomTitleInUse; reset it now
                CustomTitleInUse = false;
            }
        }
        #endregion public int? GlobalSeriesIndex

        /// <summary>
        /// Called when the Title property changes.
        /// </summary>
        /// <param name="oldValue">Old value of the Title property.</param>
        /// <param name="newValue">New value of the Title property.</param>
        protected override void OnTitleChanged(object oldValue, object newValue)
        {
            // Title property is being set, so a custom Title is in use
            CustomTitleInUse = true;
        }

        /// <summary>
        /// Initializes a new instance of the DataPointSingleSeriesWithAxes class.
        /// </summary>
        protected DataPointSingleSeriesWithAxes()
        {
        }

        /// <summary>
        /// Returns the custom ResourceDictionary to use for necessary resources.
        /// </summary>
        /// <returns>
        /// ResourceDictionary to use for necessary resources.
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "This property does more work than get functions typically do.")]
        protected abstract IEnumerator<ResourceDictionary> GetResourceDictionaryEnumeratorFromHost();

        /// <summary>
        /// Insert grid containing data point used for legend item into the 
        /// plot area.
        /// </summary>
        /// <param name="oldValue">The old plot area.</param>
        /// <param name="newValue">The new plot area.</param>
        protected override void OnPlotAreaChanged(Panel oldValue, Panel newValue)
        {
            if (newValue != null)
            {
                CreateLegendItemDataPoint();
            }
            base.OnPlotAreaChanged(oldValue, newValue);
        }

        /// <summary>
        /// When the series host property is set retrieves a style to use for all the
        /// data points.
        /// </summary>
        /// <param name="oldValue">The old series host value.</param>
        /// <param name="newValue">The new series host value.</param>
        protected override void OnSeriesHostPropertyChanged(ISeriesHost oldValue, ISeriesHost newValue)
        {
            base.OnSeriesHostPropertyChanged(oldValue, newValue);

            if (oldValue != null)
            {
                oldValue.ResourceDictionariesChanged -= new EventHandler(SeriesHostResourceDictionariesChanged);
            }

            if (newValue != null)
            {
                newValue.ResourceDictionariesChanged += new EventHandler(SeriesHostResourceDictionariesChanged);

                DispensedResourcesChanging();
            }
        }

        /// <summary>
        /// Creates the LegendItem Control if conditions are right.
        /// </summary>
        private void CreateLegendItemDataPoint()
        {
            DataPoint dataPoint = CreateDataPoint();
            if (null != PlotArea)
            {
                // Bounce into the visual tree to get default Style applied
                PlotArea.Children.Add(dataPoint);
                PlotArea.Children.Remove(dataPoint);
            }
            dataPoint.SetBinding(DataPoint.StyleProperty, new Binding(ActualDataPointStyleName) { Source = this });
            // Start DataContext null to avoid Binding warnings in the output window
            LegendItem.DataContext = null;
#if !SILVERLIGHT
            if (null == LegendItem.Parent)
            {
#endif
                LegendItem.Loaded += delegate
                {
                    // Wait for Loaded to set the DataPoint
                    LegendItem.DataContext = dataPoint;
                };
#if !SILVERLIGHT
            }
            else
            {
                LegendItem.DataContext = dataPoint;
            }
#endif
        }

        /// <summary>
        /// Called after data points have been loaded from the items source.
        /// </summary>
        /// <param name="newDataPoints">New active data points.</param>
        /// <param name="oldDataPoints">Old inactive data points.</param>
        protected override void OnDataPointsChanged(IList<DataPoint> newDataPoints, IList<DataPoint> oldDataPoints)
        {
            base.OnDataPointsChanged(newDataPoints, oldDataPoints);

            if (null != PlotArea)
            {
                // Create the Control for use by LegendItem
                // Add it to the visual tree so that its style will be applied
                if (null != LegendItem.DataContext)
                {
                    PlotArea.Children.Remove(LegendItem.DataContext as UIElement);
                }
            }
        }

        /// <summary>
        /// Sets the style of the data point to the single style used for all
        /// data points.
        /// </summary>
        /// <param name="dataPoint">The data point to apply the style to.
        /// </param>
        /// <param name="dataContext">The object associated with the data point.
        /// </param>
        protected override void PrepareDataPoint(DataPoint dataPoint, object dataContext)
        {
            dataPoint.SetBinding(DataPoint.StyleProperty, new Binding(ActualDataPointStyleName) { Source = this });
            base.PrepareDataPoint(dataPoint, dataContext);
        }

        /// <summary>
        /// This method updates the global series index property.
        /// </summary>
        /// <param name="globalIndex">The global index of the series.</param>
        public void GlobalSeriesIndexChanged(int? globalIndex)
        {
            this.GlobalSeriesIndex = globalIndex;
        }

        /// <summary>
        /// Handles the SeriesHost's ResourceDictionariesChanged event.
        /// </summary>
        /// <param name="sender">ISeriesHost instance.</param>
        /// <param name="e">Event args.</param>
        private void SeriesHostResourceDictionariesChanged(object sender, EventArgs e)
        {
            DispensedResourcesChanging();
        }

        /// <summary>
        /// Processes the change of the DispensedResources property.
        /// </summary>
        private void DispensedResourcesChanging()
        {
            if (null != PaletteResources)
            {
                Resources.MergedDictionaries.Remove(PaletteResources);
                PaletteResources = null;
            }
            using (IEnumerator<ResourceDictionary> enumerator = GetResourceDictionaryEnumeratorFromHost())
            {
                if (enumerator.MoveNext())
                {
                    PaletteResources =
#if SILVERLIGHT
                        enumerator.Current.ShallowCopy();
#else
                        enumerator.Current;
#endif
                    Resources.MergedDictionaries.Add(PaletteResources);
                }
            }
            CreateLegendItemDataPoint();
            ActualDataPointStyle = DataPointStyle ?? (Resources[DataPointStyleName] as Style);
            ActualLegendItemStyle = LegendItemStyle ?? (Resources[LegendItemStyleName] as Style);
            Refresh();
        }
    }
}