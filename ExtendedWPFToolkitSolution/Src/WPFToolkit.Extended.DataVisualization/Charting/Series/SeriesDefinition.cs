// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace System.Windows.Controls.DataVisualization.Charting
{
    /// <summary>
    /// Defines the attributes of a series that is to be rendered by the DefinitionSeries class.
    /// </summary>
    /// <QualityBand>Preview</QualityBand>
    [StyleTypedProperty(Property = DataPointStyleName, StyleTargetType = typeof(DataPoint))]
    [StyleTypedProperty(Property = LegendItemStyleName, StyleTargetType = typeof(LegendItem))]
    [StyleTypedProperty(Property = DataShapeStyleName, StyleTargetType = typeof(Shape))]
    public class SeriesDefinition : FrameworkElement, ISeries, IRequireGlobalSeriesIndex
    {
        /// <summary>
        /// Name of the DataPointStyle property.
        /// </summary>
        private const string DataPointStyleName = "DataPointStyle";

        /// <summary>
        /// Name of the LegendItemStyle property.
        /// </summary>
        private const string LegendItemStyleName = "LegendItemStyle";

        /// <summary>
        /// Name of the DataShapeStyle property.
        /// </summary>
        private const string DataShapeStyleName = "DataShapeStyle";

        /// <summary>
        /// Provides the store for the ISeries.LegendItems property.
        /// </summary>
        private readonly ObservableCollection<object> _legendItems = new ObservableCollection<object>();

        /// <summary>
        /// Represents the single LegendItem corresponding to the SeriesDefinition.
        /// </summary>
        private readonly LegendItem _legendItem;

        /// <summary>
        /// Keeps a reference to the WeakEventListener used to prevent leaks of collections assigned to the ItemsSource property.
        /// </summary>
        private WeakEventListener<SeriesDefinition, object, NotifyCollectionChangedEventArgs> _weakEventListener;

        /// <summary>
        /// Gets or sets the index of the series definition.
        /// </summary>
        internal int Index { get; set; }

        /// <summary>
        /// Initializes a new instance of the SeriesDefinition class.
        /// </summary>
        public SeriesDefinition()
        {
            _legendItem = new LegendItem { Owner = this };
            _legendItem.SetBinding(LegendItem.ContentProperty, new Binding("ActualTitle") { Source = this });
            _legendItem.SetBinding(LegendItem.StyleProperty, new Binding("ActualLegendItemStyle") { Source = this });
            _legendItems.Add(_legendItem);
        }

        /// <summary>
        /// Gets or sets a sequence that provides the content of the series.
        /// </summary>
        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        /// <summary>
        /// Identifies the ItemsSource dependency property.
        /// </summary>
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(SeriesDefinition), new PropertyMetadata(OnItemsSourceChanged));

        /// <summary>
        /// Handles changes to the ItemsSource dependency property.
        /// </summary>
        /// <param name="o">DependencyObject that changed.</param>
        /// <param name="e">Event data for the DependencyPropertyChangedEvent.</param>
        private static void OnItemsSourceChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            ((SeriesDefinition)o).OnItemsSourceChanged((IEnumerable)e.OldValue, (IEnumerable)e.NewValue);
        }

        /// <summary>
        /// Handles changes to the ItemsSource property.
        /// </summary>
        /// <param name="oldValue">Old value.</param>
        /// <param name="newValue">New value.</param>
        private void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
        {
            // Remove handler for oldValue.CollectionChanged (if present)
            INotifyCollectionChanged oldValueINotifyCollectionChanged = oldValue as INotifyCollectionChanged;
            if (null != oldValueINotifyCollectionChanged)
            {
                // Detach the WeakEventListener
                if (null != _weakEventListener)
                {
                    _weakEventListener.Detach();
                    _weakEventListener = null;
                }
            }

            // Add handler for newValue.CollectionChanged (if possible)
            INotifyCollectionChanged newValueINotifyCollectionChanged = newValue as INotifyCollectionChanged;
            if (null != newValueINotifyCollectionChanged)
            {
                // Use a WeakEventListener so that the backwards reference doesn't keep this object alive
                _weakEventListener = new WeakEventListener<SeriesDefinition, object, NotifyCollectionChangedEventArgs>(this);
                _weakEventListener.OnEventAction = (instance, source, eventArgs) => instance.ItemsSourceCollectionChanged(source, eventArgs);
                _weakEventListener.OnDetachAction = (weakEventListener) => newValueINotifyCollectionChanged.CollectionChanged -= weakEventListener.OnEvent;
                newValueINotifyCollectionChanged.CollectionChanged += _weakEventListener.OnEvent;
            }

            if (null != ParentDefinitionSeries)
            {
                ParentDefinitionSeries.SeriesDefinitionItemsSourceChanged(this, oldValue, newValue);
            }
        }

        /// <summary>
        /// Handles the CollectionChanged event for the ItemsSource property.
        /// </summary>
        /// <param name="sender">Event source.</param>
        /// <param name="e">Event arguments..</param>
        private void ItemsSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (null != ParentDefinitionSeries)
            {
                ParentDefinitionSeries.SeriesDefinitionItemsSourceCollectionChanged(this, e.Action, e.OldItems, e.OldStartingIndex, e.NewItems, e.NewStartingIndex);
            }
        }

        /// <summary>
        /// Gets or sets the automatic title of the series definition.
        /// </summary>
        private object AutomaticTitle
        {
            get { return _automaticTitle; }
            set
            {
                _automaticTitle = value;
                ActualTitle = Title ?? _automaticTitle;
            }
        }

        /// <summary>
        /// Stores the automatic title of the series definition.
        /// </summary>
        private object _automaticTitle;

        /// <summary>
        ///  Gets or sets the Title of the series definition.
        /// </summary>
        public object Title
        {
            get { return (object)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        /// <summary>
        /// Identifies the Title dependency property.
        /// </summary>
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(object), typeof(SeriesDefinition), new PropertyMetadata(OnTitleChanged));

        /// <summary>
        /// Handles changes to the Title dependency property.
        /// </summary>
        /// <param name="o">DependencyObject that changed.</param>
        /// <param name="e">Event data for the DependencyPropertyChangedEvent.</param>
        private static void OnTitleChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            ((SeriesDefinition)o).OnTitleChanged((object)e.OldValue, (object)e.NewValue);
        }

        /// <summary>
        /// Handles changes to the Title property.
        /// </summary>
        /// <param name="oldValue">Old value.</param>
        /// <param name="newValue">New value.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "oldValue", Justification = "Parameter is part of the pattern for DependencyProperty change handlers.")]
        private void OnTitleChanged(object oldValue, object newValue)
        {
            ActualTitle = newValue ?? _automaticTitle;
        }

        /// <summary>
        /// Gets the rendered Title of the series definition.
        /// </summary>
        public object ActualTitle
        {
            get { return (object)GetValue(ActualTitleProperty); }
            protected set { SetValue(ActualTitleProperty, value); }
        }

        /// <summary>
        /// Identifies the ActualTitle dependency property.
        /// </summary>
        public static readonly DependencyProperty ActualTitleProperty =
            DependencyProperty.Register("ActualTitle", typeof(object), typeof(SeriesDefinition), null);

        /// <summary>
        /// Gets or sets the DataPoint Style from the SeriesHost's Palette.
        /// </summary>
        internal Style PaletteDataPointStyle
        {
            get { return _paletteDataPointStyle; }
            set
            {
                _paletteDataPointStyle = value;
                ActualDataPointStyle = DataPointStyle ?? _paletteDataPointStyle;
            }
        }

        /// <summary>
        /// Stores the DataPoint Style from the SeriesHost's Palette.
        /// </summary>
        private Style _paletteDataPointStyle;

        /// <summary>
        /// Gets or sets the DataPoint Style for the series definition.
        /// </summary>
        public Style DataPointStyle
        {
            get { return (Style)GetValue(DataPointStyleProperty); }
            set { SetValue(DataPointStyleProperty, value); }
        }

        /// <summary>
        /// Identifies the DataPointStyle dependency property.
        /// </summary>
        public static readonly DependencyProperty DataPointStyleProperty =
            DependencyProperty.Register(DataPointStyleName, typeof(Style), typeof(SeriesDefinition), new PropertyMetadata(OnDataPointStyleChanged));

        /// <summary>
        /// Handles changes to the DataPointStyle dependency property.
        /// </summary>
        /// <param name="o">DependencyObject that changed.</param>
        /// <param name="e">Event data for the DependencyPropertyChangedEvent.</param>
        private static void OnDataPointStyleChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            ((SeriesDefinition)o).OnDataPointStyleChanged((Style)e.OldValue, (Style)e.NewValue);
        }

        /// <summary>
        /// Handles changes to the DataPointStyle property.
        /// </summary>
        /// <param name="oldValue">Old value.</param>
        /// <param name="newValue">New value.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "oldValue", Justification = "Parameter is part of the pattern for DependencyProperty change handlers.")]
        private void OnDataPointStyleChanged(Style oldValue, Style newValue)
        {
            ActualDataPointStyle = newValue ?? _paletteDataPointStyle;
        }

        /// <summary>
        /// Gets the rendered DataPoint Style for the series definition.
        /// </summary>
        public Style ActualDataPointStyle
        {
            get { return (Style)GetValue(ActualDataPointStyleProperty); }
            protected set { SetValue(ActualDataPointStyleProperty, value); }
        }

        /// <summary>
        /// Identifies the ActualDataPointStyle dependency property.
        /// </summary>
        public static readonly DependencyProperty ActualDataPointStyleProperty =
            DependencyProperty.Register("ActualDataPointStyle", typeof(Style), typeof(SeriesDefinition), null);

        /// <summary>
        /// Gets or sets the LegendItem Style from the SeriesHost's Palette.
        /// </summary>
        internal Style PaletteLegendItemStyle
        {
            get { return _paletteLegendItemStyle; }
            set
            {
                _paletteLegendItemStyle = value;
                ActualLegendItemStyle = LegendItemStyle ?? _paletteLegendItemStyle;
            }
        }

        /// <summary>
        /// Stores the LegendItem Style from the SeriesHost's Palette.
        /// </summary>
        private Style _paletteLegendItemStyle;

        /// <summary>
        /// Gets or sets the LegendItem Style for the series definition.
        /// </summary>
        public Style LegendItemStyle
        {
            get { return (Style)GetValue(LegendItemStyleProperty); }
            set { SetValue(LegendItemStyleProperty, value); }
        }

        /// <summary>
        /// Identifies the LegendItemStyle dependency property.
        /// </summary>
        public static readonly DependencyProperty LegendItemStyleProperty =
            DependencyProperty.Register(LegendItemStyleName, typeof(Style), typeof(SeriesDefinition), new PropertyMetadata(OnLegendItemStyleChanged));

        /// <summary>
        /// Handles changes to the LegendItemStyle dependency property.
        /// </summary>
        /// <param name="o">DependencyObject that changed.</param>
        /// <param name="e">Event data for the DependencyPropertyChangedEvent.</param>
        private static void OnLegendItemStyleChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            ((SeriesDefinition)o).OnLegendItemStyleChanged((Style)e.OldValue, (Style)e.NewValue);
        }

        /// <summary>
        /// Handles changes to the LegendItemStyle property.
        /// </summary>
        /// <param name="oldValue">Old value.</param>
        /// <param name="newValue">New value.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "oldValue", Justification = "Parameter is part of the pattern for DependencyProperty change handlers.")]
        private void OnLegendItemStyleChanged(Style oldValue, Style newValue)
        {
            ActualLegendItemStyle = newValue ?? _paletteLegendItemStyle;
        }

        /// <summary>
        /// Gets the rendered LegendItem Style for the series definition.
        /// </summary>
        public Style ActualLegendItemStyle
        {
            get { return (Style)GetValue(ActualLegendItemStyleProperty); }
            protected set { SetValue(ActualLegendItemStyleProperty, value); }
        }

        /// <summary>
        /// Identifies the ActualDataPointStyle dependency property.
        /// </summary>
        public static readonly DependencyProperty ActualLegendItemStyleProperty =
            DependencyProperty.Register("ActualLegendItemStyle", typeof(Style), typeof(SeriesDefinition), null);

        /// <summary>
        /// Gets or sets the DataShape Style from the SeriesHost's Palette.
        /// </summary>
        internal Style PaletteDataShapeStyle
        {
            get { return _paletteDataShapeStyle; }
            set
            {
                _paletteDataShapeStyle = value;
                ActualDataShapeStyle = DataShapeStyle ?? _paletteDataShapeStyle;
            }
        }

        /// <summary>
        /// Stores the DataShape Style from the SeriesHost's Palette.
        /// </summary>
        private Style _paletteDataShapeStyle;

        /// <summary>
        /// Gets or sets the DataShape Style for the series definition.
        /// </summary>
        public Style DataShapeStyle
        {
            get { return (Style)GetValue(DataShapeStyleProperty); }
            set { SetValue(DataShapeStyleProperty, value); }
        }

        /// <summary>
        /// Identifies the DataShapeStyle dependency property.
        /// </summary>
        public static readonly DependencyProperty DataShapeStyleProperty =
            DependencyProperty.Register(DataShapeStyleName, typeof(Style), typeof(SeriesDefinition), new PropertyMetadata(OnDataShapeStyleChanged));

        /// <summary>
        /// Handles changes to the DataShapeStyle dependency property.
        /// </summary>
        /// <param name="o">DependencyObject that changed.</param>
        /// <param name="e">Event data for the DependencyPropertyChangedEvent.</param>
        private static void OnDataShapeStyleChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            ((SeriesDefinition)o).OnDataShapeStyleChanged((Style)e.OldValue, (Style)e.NewValue);
        }

        /// <summary>
        /// Handles changes to the DataShapeStyle property.
        /// </summary>
        /// <param name="oldValue">Old value.</param>
        /// <param name="newValue">New value.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "oldValue", Justification = "Parameter is part of the pattern for DependencyProperty change handlers.")]
        private void OnDataShapeStyleChanged(Style oldValue, Style newValue)
        {
            ActualDataShapeStyle = newValue ?? _paletteDataShapeStyle;
        }

        /// <summary>
        /// Gets the rendered DataShape Style for the series definition.
        /// </summary>
        public Style ActualDataShapeStyle
        {
            get { return (Style)GetValue(ActualDataShapeStyleProperty); }
            protected set { SetValue(ActualDataShapeStyleProperty, value); }
        }

        /// <summary>
        /// Identifies the ActualDataShapeStyle dependency property.
        /// </summary>
        public static readonly DependencyProperty ActualDataShapeStyleProperty =
            DependencyProperty.Register("ActualDataShapeStyle", typeof(Style), typeof(SeriesDefinition), null);

        /// <summary>
        /// Gets or sets the Binding to use for identifying the dependent value.
        /// </summary>
        public Binding DependentValueBinding
        {
            get
            {
                return _dependentValueBinding;
            }
            set
            {
                if (value != _dependentValueBinding)
                {
                    _dependentValueBinding = value;
                    Reset();
                }
            }
        }

        /// <summary>
        /// The binding used to identify the dependent value binding.
        /// </summary>
        private Binding _dependentValueBinding;

        /// <summary>
        /// Gets or sets the Binding Path to use for identifying the dependent value.
        /// </summary>
        public string DependentValuePath
        {
            get
            {
                return (null != DependentValueBinding) ? DependentValueBinding.Path.Path : null;
            }
            set
            {
                if (null == value)
                {
                    DependentValueBinding = null;
                }
                else
                {
                    DependentValueBinding = new Binding(value);
                }
            }
        }

        /// <summary>
        /// Gets or sets the Binding to use for identifying the independent value.
        /// </summary>
        public Binding IndependentValueBinding
        {
            get
            {
                return _independentValueBinding;
            }
            set
            {
                if (_independentValueBinding != value)
                {
                    _independentValueBinding = value;
                    Reset();
                }
            }
        }

        /// <summary>
        /// The binding used to identify the independent value binding.
        /// </summary>
        private Binding _independentValueBinding;

        /// <summary>
        /// Gets or sets the Binding Path to use for identifying the independent value.
        /// </summary>
        public string IndependentValuePath
        {
            get
            {
                return (null != IndependentValueBinding) ? IndependentValueBinding.Path.Path : null;
            }
            set
            {
                if (null == value)
                {
                    IndependentValueBinding = null;
                }
                else
                {
                    IndependentValueBinding = new Binding(value);
                }
            }
        }

        /// <summary>
        /// Resets the display of the series definition.
        /// </summary>
        private void Reset()
        {
            if (null != ParentDefinitionSeries)
            {
                ParentDefinitionSeries.SeriesDefinitionItemsSourceChanged(this, ItemsSource, ItemsSource);
            }
        }

        /// <summary>
        /// Gets the SeriesHost as a DefinitionSeries instance.
        /// </summary>
        private DefinitionSeries ParentDefinitionSeries
        {
            get { return (DefinitionSeries)((ISeries)this).SeriesHost; }
        }

        /// <summary>
        /// Gets the collection of legend items for the series definition.
        /// </summary>
        ObservableCollection<object> ISeries.LegendItems
        {
            get { return _legendItems; }
        }

        /// <summary>
        /// Gets or sets the SeriesHost for the series definition.
        /// </summary>
        ISeriesHost IRequireSeriesHost.SeriesHost
        {
            get { return _seriesHost; }
            set
            {
                _seriesHost = value;
                if (!(_seriesHost is DefinitionSeries) && (null != value))
                {
                    throw new NotSupportedException(Properties.Resources.SeriesDefinition_SeriesHost_InvalidParent);
                }

                if (null != _seriesHost)
                {
                    DataPoint legendItemDataPoint = ((DefinitionSeries)_seriesHost).InternalCreateDataPoint();
#if SILVERLIGHT
                    // Apply default style (hard)
                    ContentPresenter container = new ContentPresenter { Content = legendItemDataPoint, Width = 1, Height = 1 };
                    Popup popup = new Popup { Child = container };
                    container.SizeChanged += delegate
                    {
                        popup.Child = null;
                        popup.IsOpen = false;
                    };
                    popup.IsOpen = true;
#else
                    // Apply default style (easy)
                    ContentControl contentControl = new ContentControl();
                    contentControl.Content = legendItemDataPoint;
                    contentControl.Content = null;
#endif
                    legendItemDataPoint.SetBinding(DataPoint.StyleProperty, new Binding("ActualDataPointStyle") { Source = this });
                    _legendItem.DataContext = legendItemDataPoint;
                }
            }
        }

        /// <summary>
        /// Stores the SeriesHost for the series definition.
        /// </summary>
        private ISeriesHost _seriesHost;

        /// <summary>
        /// Handles changes to the global series index of the series definition.
        /// </summary>
        /// <param name="globalIndex">New index.</param>
        void IRequireGlobalSeriesIndex.GlobalSeriesIndexChanged(int? globalIndex)
        {
            if (globalIndex.HasValue)
            {
                AutomaticTitle = string.Format(CultureInfo.CurrentCulture, Properties.Resources.Series_OnGlobalSeriesIndexPropertyChanged_UntitledSeriesFormatString, globalIndex + 1);
            }
        }

        /// <summary>
        /// Gets or sets the TimeSpan to use for the duration of data transitions.
        /// </summary>
        public TimeSpan TransitionDuration
        {
            get { return (TimeSpan)GetValue(TransitionDurationProperty); }
            set { SetValue(TransitionDurationProperty, value); }
        }

        /// <summary>
        /// Identifies the TransitionDuration dependency property.
        /// </summary>
        public static readonly DependencyProperty TransitionDurationProperty =
            DependencyProperty.Register("TransitionDuration", typeof(TimeSpan), typeof(SeriesDefinition), new PropertyMetadata(TimeSpan.FromSeconds(0.5)));

#if !NO_EASING_FUNCTIONS
        /// <summary>
        /// Gets or sets the IEasingFunction to use for data transitions.
        /// </summary>
        public IEasingFunction TransitionEasingFunction
        {
            get { return (IEasingFunction)GetValue(TransitionEasingFunctionProperty); }
            set { SetValue(TransitionEasingFunctionProperty, value); }
        }

        /// <summary>
        /// Identifies the TransitionEasingFunction dependency property.
        /// </summary>
        public static readonly DependencyProperty TransitionEasingFunctionProperty =
            DependencyProperty.Register("TransitionEasingFunction", typeof(IEasingFunction), typeof(SeriesDefinition), new PropertyMetadata(new QuadraticEase { EasingMode = EasingMode.EaseInOut }));
#else
        /// <summary>
        /// Gets or sets a placeholder for the TransitionEasingFunction dependency property.
        /// </summary>
        internal IEasingFunction TransitionEasingFunction { get; set; }
#endif
    }
}
