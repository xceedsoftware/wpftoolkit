// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Windows.Controls.DataVisualization.Charting.Primitives;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;

namespace System.Windows.Controls.DataVisualization.Charting
{
    /// <summary>
    /// Implements a series that is defined by one or more instances of the DefinitionSeries class.
    /// </summary>
    /// <QualityBand>Preview</QualityBand>
    [ContentProperty("SeriesDefinitions")]
    [TemplatePart(Name = SeriesAreaName, Type = typeof(Grid))]
    [TemplatePart(Name = ItemContainerName, Type = typeof(DelegatingListBox))]
    [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Class is maintainable.")]
    public abstract class DefinitionSeries : Control, ISeries, IAxisListener, IRangeProvider, IValueMarginProvider, IDataProvider, ISeriesHost
    {
        /// <summary>
        /// Name of the SeriesArea property.
        /// </summary>
        private const string SeriesAreaName = "SeriesArea";

        /// <summary>
        /// Name of the ItemContainer property.
        /// </summary>
        private const string ItemContainerName = "ItemContainer";

        /// <summary>
        /// Gets or sets a value indicating whether the series is 100% stacked (versus normally stacked).
        /// </summary>
        protected bool IsStacked100 { get; set; }

        /// <summary>
        /// Gets the collection of DataItems representing the data of the series.
        /// </summary>
        protected ObservableCollection<DataItem> DataItems { get; private set; }

        /// <summary>
        /// Gets the SeriesArea template part instance.
        /// </summary>
        protected Panel SeriesArea { get; private set; }

        /// <summary>
        /// Stores an aggregated collection of legend items from the series definitions.
        /// </summary>
        private readonly AggregatedObservableCollection<object> _legendItems = new AggregatedObservableCollection<object>();

        /// <summary>
        /// Stores the collection of SeriesDefinitions that define the series.
        /// </summary>
        private readonly ObservableCollection<SeriesDefinition> _seriesDefinitions = new UniqueObservableCollection<SeriesDefinition>();

        /// <summary>
        /// Stores a mirror collection of ISeries corresponding directly to the collection of SeriesDefinitions.
        /// </summary>
        /// <remarks>
        /// Not using ObservableCollectionListAdapter because of race condition on ItemsChanged event
        /// </remarks>
        private readonly ObservableCollection<ISeries> _seriesDefinitionsAsISeries = new ObservableCollection<ISeries>();

        /// <summary>
        /// Keeps the SeriesDefinitions collection synchronized with the Children collection of the SeriesArea.
        /// </summary>
        private readonly ObservableCollectionListAdapter<UIElement> _seriesAreaChildrenListAdapter = new ObservableCollectionListAdapter<UIElement>();

        /// <summary>
        /// Stores the clip geometry for the ItemContainer.
        /// </summary>
        private readonly RectangleGeometry _clipGeometry = new RectangleGeometry();

        /// <summary>
        /// Stores a reference to the ItemContainer template part.
        /// </summary>
        private DelegatingListBox _itemContainer;

        /// <summary>
        /// Tracks the collection of DataItem that are queued for update.
        /// </summary>
        private readonly List<DataItem> _queueUpdateDataItemPlacement_DataItems = new List<DataItem>();

        /// <summary>
        /// Tracks whether the dependent axis values changed for the next update.
        /// </summary>
        private bool _queueUpdateDataItemPlacement_DependentAxisValuesChanged;

        /// <summary>
        /// Tracks whether the independent axis values changed for the next update.
        /// </summary>
        private bool _queueUpdateDataItemPlacement_IndependentAxisValuesChanged;

        /// <summary>
        /// Stores a reference to the backing collection for the SelectedItems property.
        /// </summary>
        private ObservableCollection<object> _selectedItems = new ObservableCollection<object>();

        /// <summary>
        /// Tracks whether the SelectedItems collection is being synchronized (to prevent reentrancy).
        /// </summary>
        private bool _synchronizingSelectedItems;

#if !SILVERLIGHT
        /// <summary>
        /// Performs one-time initialization of DefinitionSeries data.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline", Justification = "Dependency properties are initialized in-line.")]
        static DefinitionSeries()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DefinitionSeries), new FrameworkPropertyMetadata(typeof(DefinitionSeries)));
        }
#endif

        /// <summary>
        /// Initializes a new instance of the DefinitionSeries class.
        /// </summary>
        protected DefinitionSeries()
        {
#if SILVERLIGHT
            this.DefaultStyleKey = typeof(DefinitionSeries);
#endif
            _seriesDefinitions.CollectionChanged += new NotifyCollectionChangedEventHandler(SeriesDefinitionsCollectionChanged);
            _seriesAreaChildrenListAdapter.Collection = _seriesDefinitions;
            _selectedItems.CollectionChanged += new NotifyCollectionChangedEventHandler(SelectedItemsCollectionChanged);
            DataItems = new ObservableCollection<DataItem>();
        }

        /// <summary>
        /// Gets or sets the dependent axis of the series.
        /// </summary>
        public IAxis DependentAxis
        {
            get { return (IAxis)GetValue(DependentAxisProperty); }
            set { SetValue(DependentAxisProperty, value); }
        }

        /// <summary>
        /// Identifies the DependentAxis dependency property.
        /// </summary>
        public static readonly DependencyProperty DependentAxisProperty =
            DependencyProperty.Register("DependentAxis", typeof(IAxis), typeof(DefinitionSeries), new PropertyMetadata(OnDependentAxisChanged));

        /// <summary>
        /// Handles changes to the DependentAxis dependency property.
        /// </summary>
        /// <param name="o">DependencyObject that changed.</param>
        /// <param name="e">Event data for the DependencyPropertyChangedEvent.</param>
        private static void OnDependentAxisChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            ((DefinitionSeries)o).OnDependentAxisChanged((IAxis)e.OldValue, (IAxis)e.NewValue);
        }

        /// <summary>
        /// Handles changes to the DependentAxis property.
        /// </summary>
        /// <param name="oldValue">Old value.</param>
        /// <param name="newValue">New value.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "oldValue", Justification = "Parameter is part of the pattern for DependencyProperty change handlers.")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "newValue", Justification = "Parameter is part of the pattern for DependencyProperty change handlers.")]
        private void OnDependentAxisChanged(IAxis oldValue, IAxis newValue)
        {
            if (null != ActualDependentAxis)
            {
                EnsureAxes(true, false, false);
            }
        }

        /// <summary>
        /// Gets or sets the independent axis of the series.
        /// </summary>
        public IAxis IndependentAxis
        {
            get { return (IAxis)GetValue(IndependentAxisProperty); }
            set { SetValue(IndependentAxisProperty, value); }
        }

        /// <summary>
        /// Identifies the IndependentAxis dependency property.
        /// </summary>
        public static readonly DependencyProperty IndependentAxisProperty =
            DependencyProperty.Register("IndependentAxis", typeof(IAxis), typeof(DefinitionSeries), new PropertyMetadata(OnIndependentAxisChanged));

        /// <summary>
        /// Handles changes to the IndependentAxis dependency property.
        /// </summary>
        /// <param name="o">DependencyObject that changed.</param>
        /// <param name="e">Event data for the DependencyPropertyChangedEvent.</param>
        private static void OnIndependentAxisChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            ((DefinitionSeries)o).OnIndependentAxisChanged((IAxis)e.OldValue, (IAxis)e.NewValue);
        }

        /// <summary>
        /// Handles changes to the IndependentAxis property.
        /// </summary>
        /// <param name="oldValue">Old value.</param>
        /// <param name="newValue">New value.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "oldValue", Justification = "Parameter is part of the pattern for DependencyProperty change handlers.")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "newValue", Justification = "Parameter is part of the pattern for DependencyProperty change handlers.")]
        private void OnIndependentAxisChanged(IAxis oldValue, IAxis newValue)
        {
            if (null != ActualIndependentAxis)
            {
                EnsureAxes(false, true, false);
            }
        }

        /// <summary>
        /// Gets the rendered dependent axis of the series.
        /// </summary>
        public IAxis ActualDependentAxis
        {
            get { return (IAxis)GetValue(ActualDependentAxisProperty); }
            protected set { SetValue(ActualDependentAxisProperty, value); }
        }

        /// <summary>
        /// Identifies the ActualDependentAxis dependency property.
        /// </summary>
        public static readonly DependencyProperty ActualDependentAxisProperty =
            DependencyProperty.Register("ActualDependentAxis", typeof(IAxis), typeof(DefinitionSeries), null);

        /// <summary>
        /// Gets the rendered independent axis of the series.
        /// </summary>
        public IAxis ActualIndependentAxis
        {
            get { return (IAxis)GetValue(ActualIndependentAxisProperty); }
            protected set { SetValue(ActualIndependentAxisProperty, value); }
        }

        /// <summary>
        /// Identifies the ActualIndependentAxis dependency property.
        /// </summary>
        public static readonly DependencyProperty ActualIndependentAxisProperty =
            DependencyProperty.Register("ActualIndependentAxis", typeof(IAxis), typeof(DefinitionSeries), null);

        /// <summary>
        /// Gets the ActualDependentAxis as an IRangeAxis instance.
        /// </summary>
        protected IRangeAxis ActualDependentRangeAxis
        {
            get { return (IRangeAxis)ActualDependentAxis; }
        }

        /// <summary>
        /// Gets the collection of legend items for the series.
        /// </summary>
        public ObservableCollection<object> LegendItems
        {
            get { return _legendItems; }
        }

        /// <summary>
        /// Gets or sets the SeriesHost for the series.
        /// </summary>
        public ISeriesHost SeriesHost
        {
            get { return _seriesHost; }
            set
            {
                if (null != _seriesHost)
                {
                    _seriesHost.ResourceDictionariesChanged -= new EventHandler(SeriesHostResourceDictionariesChanged);

                    if (null != ActualDependentAxis)
                    {
                        ActualDependentAxis.RegisteredListeners.Remove(this);
                        ActualDependentAxis = null;
                    }
                    if (null != ActualIndependentAxis)
                    {
                        ActualIndependentAxis.RegisteredListeners.Remove(this);
                        ActualIndependentAxis = null;
                    }

                    foreach (SeriesDefinition definition in SeriesDefinitions)
                    {
                        SeriesDefinitionItemsSourceChanged(definition, definition.ItemsSource, null);
                    }
                }
                _seriesHost = value;
                SeriesHostResourceDictionariesChanged(null, null);
                if (null != _seriesHost)
                {
                    _seriesHost.ResourceDictionariesChanged += new EventHandler(SeriesHostResourceDictionariesChanged);
                    foreach (SeriesDefinition definition in SeriesDefinitions)
                    {
                        SeriesDefinitionItemsSourceChanged(definition, null, definition.ItemsSource);
                    }
                }
            }
        }

        /// <summary>
        /// Stores the SeriesHost for the series.
        /// </summary>
        private ISeriesHost _seriesHost;

        /// <summary>
        /// Gets or sets the collection of SeriesDefinitions that define the series.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Setter is public to work around a limitation with the XAML editing tools.")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "value", Justification = "Setter is public to work around a limitation with the XAML editing tools.")]
        public Collection<SeriesDefinition> SeriesDefinitions
        {
            get { return _seriesDefinitions; }
            set { throw new NotSupportedException(Properties.Resources.DefinitionSeries_SeriesDefinitions_SetterNotSupported); }
        }

        /// <summary>
        /// Gets or sets the SelectionMode property.
        /// </summary>
        public SeriesSelectionMode SelectionMode
        {
            get { return (SeriesSelectionMode)GetValue(SelectionModeProperty); }
            set { SetValue(SelectionModeProperty, value); }
        }

        /// <summary>
        /// Identifies the SelectionMode dependency property.
        /// </summary>
        public static readonly DependencyProperty SelectionModeProperty =
            DependencyProperty.Register("SelectionMode", typeof(SeriesSelectionMode), typeof(DefinitionSeries), new PropertyMetadata(SeriesSelectionMode.None, OnSelectionModeChanged));

        /// <summary>
        /// Handles changes to the SelectionMode dependency property.
        /// </summary>
        /// <param name="o">DependencyObject that changed.</param>
        /// <param name="e">Event data for the DependencyPropertyChangedEvent.</param>
        private static void OnSelectionModeChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            ((DefinitionSeries)o).OnSelectionModeChanged((SeriesSelectionMode)e.OldValue, (SeriesSelectionMode)e.NewValue);
        }

        /// <summary>
        /// Handles changes to the SelectionMode property.
        /// </summary>
        /// <param name="oldValue">Old value.</param>
        /// <param name="newValue">New value.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "oldValue", Justification = "Parameter is part of the pattern for DependencyProperty change handlers.")]
        private void OnSelectionModeChanged(SeriesSelectionMode oldValue, SeriesSelectionMode newValue)
        {
            if (null != _itemContainer)
            {
                switch (newValue)
                {
                    case SeriesSelectionMode.None:
                        _itemContainer.SelectedItem = null;
                        _itemContainer.SelectionMode = Controls.SelectionMode.Single;
                        break;
                    case SeriesSelectionMode.Single:
                        _itemContainer.SelectionMode = Controls.SelectionMode.Single;
                        break;
                    case SeriesSelectionMode.Multiple:
                        _itemContainer.SelectionMode = Controls.SelectionMode.Multiple;
                        break;
                }
            }
        }

        /// <summary>
        /// Gets or sets the SelectedIndex property.
        /// </summary>
        public int SelectedIndex
        {
            get { return (int)GetValue(SelectedIndexProperty); }
            set { SetValue(SelectedIndexProperty, value); }
        }

        /// <summary>
        /// Identifies the SelectedIndex dependency property.
        /// </summary>
        public static readonly DependencyProperty SelectedIndexProperty =
            DependencyProperty.Register("SelectedIndex", typeof(int), typeof(DefinitionSeries), new PropertyMetadata(-1));

        /// <summary>
        /// Gets or sets the SelectedItem property.
        /// </summary>
        public object SelectedItem
        {
            get { return (object)GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        /// <summary>
        /// Identifies the SelectedItem dependency property.
        /// </summary>
        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register("SelectedItem", typeof(object), typeof(DefinitionSeries), null);

        /// <summary>
        /// Gets the currently selected items.
        /// </summary>
        /// <remarks>
        /// This property is meant to be used when SelectionMode is Multiple. If the selection mode is Single the correct property to use is SelectedItem.
        /// </remarks>
        public IList SelectedItems
        {
            get { return _selectedItems; }
        }

        /// <summary>
        /// Handles the CollectionChanged event of the SelectedItems collection.
        /// </summary>
        /// <param name="sender">Event source.</param>
        /// <param name="e">Event arguments.</param>
        private void SelectedItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (!_synchronizingSelectedItems)
            {
                try
                {
                    _synchronizingSelectedItems = true;

                    // Synchronize the SelectedItems collection
                    if (null != _itemContainer)
                    {
                        if (NotifyCollectionChangedAction.Reset == e.Action)
                        {
                            if (0 < _itemContainer.SelectedItems.Count)
                            {
                                _itemContainer.SelectedItems.Clear();
                            }
                            foreach (DataItem dataItem in _selectedItems.SelectMany(v => DataItems.Where(di => object.Equals(di.Value, v))))
                            {
                                _itemContainer.SelectedItems.Add(dataItem);
                            }
                        }
                        else
                        {
                            if (null != e.OldItems)
                            {
                                foreach (DataItem dataItem in e.OldItems.CastWrapper<object>().SelectMany(v => DataItems.Where(di => object.Equals(di.Value, v))))
                                {
                                    _itemContainer.SelectedItems.Remove(dataItem);
                                }
                            }
                            if (null != e.NewItems)
                            {
                                foreach (DataItem dataItem in e.NewItems.CastWrapper<object>().SelectMany(v => DataItems.Where(di => object.Equals(di.Value, v))))
                                {
                                    _itemContainer.SelectedItems.Add(dataItem);
                                }
                            }
                        }
                    }
                }
                finally
                {
                    _synchronizingSelectedItems = false;
                }
            }
        }

        /// <summary>
        /// Handles the SelectionChanged event of the ItemContainer class.
        /// </summary>
        /// <param name="sender">Event source.</param>
        /// <param name="e">Event arguments.</param>
        private void ItemContainerSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataItem[] removedDataItems = e.RemovedItems.CastWrapper<DataItem>().ToArray();
            DataItem[] addedDataItems = e.AddedItems.CastWrapper<DataItem>().ToArray();

            if (!_synchronizingSelectedItems)
            {
                try
                {
                    _synchronizingSelectedItems = true;

                    // Synchronize the SelectedItems collection
                    foreach (object obj in removedDataItems.Select(di => di.Value))
                    {
                        _selectedItems.Remove(obj);
                    }
                    foreach (object obj in addedDataItems.Select(di => di.Value))
                    {
                        _selectedItems.Add(obj);
                    }
                }
                finally
                {
                    _synchronizingSelectedItems = false;
                }
            }

            // Pass the SelectionChanged event on to any listeners
            IList removedItems = removedDataItems.Select(di => di.Value).ToArray();
            IList addedItems = addedDataItems.Select(di => di.Value).ToArray();
#if SILVERLIGHT
            SelectionChangedEventHandler handler = SelectionChanged;
            if (null != handler)
            {
                handler(this, new SelectionChangedEventArgs(removedItems, addedItems));
            }
#else
            RaiseEvent(new SelectionChangedEventArgs(SelectionChangedEvent, removedItems, addedItems));
#endif
        }

        /// <summary>
        /// Occurs when the selection of a DefinitionSeries changes.
        /// </summary>
#if SILVERLIGHT
        public event SelectionChangedEventHandler SelectionChanged;
#else
        public event SelectionChangedEventHandler SelectionChanged
        {
            add { AddHandler(SelectionChangedEvent, value); }
            remove { RemoveHandler(SelectionChangedEvent, value); }
        }

        /// <summary>
        /// Identifies the SelectionChanged routed event.
        /// </summary>
        public static readonly RoutedEvent SelectionChangedEvent =
            EventManager.RegisterRoutedEvent("SelectionChanged", RoutingStrategy.Bubble, typeof(SelectionChangedEventHandler), typeof(DefinitionSeries));
#endif

        /// <summary>
        /// Builds the visual tree for the control when a new template is applied.
        /// </summary>
        public override void OnApplyTemplate()
        {
            if (null != _itemContainer)
            {
                _itemContainer.PrepareContainerForItem = null;
                _itemContainer.ClearContainerForItem = null;
                _itemContainer.ItemsSource = null;
                _itemContainer.Clip = null;
                _itemContainer.SizeChanged -= new SizeChangedEventHandler(ItemContainerSizeChanged);
                _itemContainer.SelectionChanged -= new SelectionChangedEventHandler(ItemContainerSelectionChanged);
                _itemContainer.ClearValue(Selector.SelectedIndexProperty);
                _itemContainer.ClearValue(Selector.SelectedItemProperty);
            }

            base.OnApplyTemplate();

            SeriesArea = GetTemplateChild(SeriesAreaName) as Panel;
            if (null != SeriesArea)
            {
                _seriesAreaChildrenListAdapter.TargetList = SeriesArea.Children;
                _seriesAreaChildrenListAdapter.Populate();
            }

            _itemContainer = GetTemplateChild(ItemContainerName) as DelegatingListBox;
            if (null != _itemContainer)
            {
                _itemContainer.PrepareContainerForItem = PrepareContainerForItem;
                _itemContainer.ClearContainerForItem = ClearContainerForItem;
                _itemContainer.ItemsSource = DataItems;
                _itemContainer.Clip = _clipGeometry;
                _itemContainer.SizeChanged += new SizeChangedEventHandler(ItemContainerSizeChanged);
                _itemContainer.SelectionChanged += new SelectionChangedEventHandler(ItemContainerSelectionChanged);
                _itemContainer.SetBinding(Selector.SelectedIndexProperty, new Binding("SelectedIndex") { Source = this, Mode = BindingMode.TwoWay });
                _itemContainer.SetBinding(Selector.SelectedItemProperty, new Binding("SelectedItem") { Source = this, Mode = BindingMode.TwoWay, Converter = new SelectedItemToDataItemConverter(DataItems) });
            }

            // Synchronize selection state with new ItemContainer
            OnSelectionModeChanged(SeriesSelectionMode.None, SelectionMode);
            SelectedItemsCollectionChanged(null, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        /// <summary>
        /// Prepares the specified element to display the specified item.
        /// </summary>
        /// <param name="element">The element used to display the specified item.</param>
        /// <param name="item">The item to display.</param>
        private void PrepareContainerForItem(DependencyObject element, object item)
        {
            DataItem dataItem = (DataItem)item;
            DataPoint dataPoint = CreateDataPoint();
            dataItem.DataPoint = dataPoint;
            dataPoint.DataContext = dataItem.Value;
            dataPoint.SetBinding(DataPoint.DependentValueProperty, dataItem.SeriesDefinition.DependentValueBinding);
            dataPoint.SetBinding(DataPoint.IndependentValueProperty, dataItem.SeriesDefinition.IndependentValueBinding);
            dataPoint.SetBinding(DataPoint.StyleProperty, new Binding("ActualDataPointStyle") { Source = dataItem.SeriesDefinition });
            dataPoint.DependentValueChanged += new RoutedPropertyChangedEventHandler<IComparable>(DataPointDependentValueChanged);
            dataPoint.ActualDependentValueChanged += new RoutedPropertyChangedEventHandler<IComparable>(DataPointActualDependentValueChanged);
            dataPoint.IndependentValueChanged += new RoutedPropertyChangedEventHandler<object>(DataPointIndependentValueChanged);
            dataPoint.ActualIndependentValueChanged += new RoutedPropertyChangedEventHandler<object>(DataPointActualIndependentValueChanged);
            dataPoint.StateChanged += new RoutedPropertyChangedEventHandler<DataPointState>(DataPointStateChanged);
            dataPoint.DefinitionSeriesIsSelectionEnabledHandling = true;
            ContentControl container = (ContentControl)element;
            dataItem.Container = container;
            Binding selectionEnabledBinding = new Binding("SelectionMode") { Source = this, Converter = new SelectionModeToSelectionEnabledConverter() };
            container.SetBinding(ContentControl.IsTabStopProperty, selectionEnabledBinding);
            dataPoint.SetBinding(DataPoint.IsSelectionEnabledProperty, selectionEnabledBinding);
            dataPoint.SetBinding(DataPoint.IsSelectedProperty, new Binding("IsSelected") { Source = container, Mode = BindingMode.TwoWay });
            dataPoint.Visibility = Visibility.Collapsed;
            dataPoint.State = DataPointState.Showing;
            PrepareDataPoint(dataPoint);
            container.Content = dataPoint;
        }

        /// <summary>
        /// Undoes the effects of the PrepareContainerForItemOverride method.
        /// </summary>
        /// <param name="element">The container element.</param>
        /// <param name="item">The item to display.</param>
        private void ClearContainerForItem(DependencyObject element, object item)
        {
            DataItem dataItem = (DataItem)item;
            DataPoint dataPoint = dataItem.DataPoint;
            dataPoint.DependentValueChanged -= new RoutedPropertyChangedEventHandler<IComparable>(DataPointDependentValueChanged);
            dataPoint.ActualDependentValueChanged -= new RoutedPropertyChangedEventHandler<IComparable>(DataPointActualDependentValueChanged);
            dataPoint.IndependentValueChanged -= new RoutedPropertyChangedEventHandler<object>(DataPointIndependentValueChanged);
            dataPoint.ActualIndependentValueChanged -= new RoutedPropertyChangedEventHandler<object>(DataPointActualIndependentValueChanged);
            dataPoint.StateChanged -= new RoutedPropertyChangedEventHandler<DataPointState>(DataPointStateChanged);
            dataPoint.ClearValue(DataPoint.DependentValueProperty);
            dataPoint.ClearValue(DataPoint.IndependentValueProperty);
            dataPoint.ClearValue(DataPoint.StyleProperty);
            dataPoint.ClearValue(DataPoint.IsSelectionEnabledProperty);
            dataPoint.ClearValue(DataPoint.IsSelectedProperty);
            ContentControl container = (ContentControl)dataItem.Container;
            container.ClearValue(ContentControl.IsTabStopProperty);
            dataPoint.DataContext = null;
        }

        /// <summary>
        /// Prepares a DataPoint for use.
        /// </summary>
        /// <param name="dataPoint">DataPoint instance.</param>
        protected virtual void PrepareDataPoint(DataPoint dataPoint) { }

        /// <summary>
        /// Creates a DataPoint for the series.
        /// </summary>
        /// <returns>Series-appropriate DataPoint instance.</returns>
        protected abstract DataPoint CreateDataPoint();

        /// <summary>
        /// Provides an internally-accessible wrapper for calling CreateDataPoint.
        /// </summary>
        /// <returns>Series-appropriate DataPoint instance.</returns>
        internal DataPoint InternalCreateDataPoint()
        {
            return CreateDataPoint();
        }

        /// <summary>
        /// Handles the SizeChanged event of the ItemContainer.
        /// </summary>
        /// <param name="sender">Event source.</param>
        /// <param name="e">Event arguments.</param>
        private void ItemContainerSizeChanged(object sender, SizeChangedEventArgs e)
        {
            _clipGeometry.Rect = new Rect(0, 0, e.NewSize.Width, e.NewSize.Height);
            QueueUpdateDataItemPlacement(false, false, DataItems);
        }

        /// <summary>
        /// Returns the DataItem corresponding to the specified DataPoint.
        /// </summary>
        /// <param name="dataPoint">Specified DataPoint.</param>
        /// <returns>Corresponding DataItem.</returns>
        protected DataItem DataItemFromDataPoint(DataPoint dataPoint)
        {
            return DataItems.Where(di => di.DataPoint == dataPoint).Single();
        }

        /// <summary>
        /// Handles the DependentValueChanged event of a DataPoint.
        /// </summary>
        /// <param name="sender">Event source.</param>
        /// <param name="e">Event arguments.</param>
        private void DataPointDependentValueChanged(object sender, RoutedPropertyChangedEventArgs<IComparable> e)
        {
            DataPoint dataPoint = (DataPoint)sender;
            SeriesDefinition definition = DataItemFromDataPoint(dataPoint).SeriesDefinition;
            TimeSpan transitionDuration = definition.TransitionDuration;
            if (0 < transitionDuration.TotalMilliseconds)
            {
                dataPoint.BeginAnimation(DataPoint.ActualDependentValueProperty, "ActualDependentValue", e.NewValue, definition.TransitionDuration, definition.TransitionEasingFunction);
            }
            else
            {
                dataPoint.ActualDependentValue = e.NewValue;
            }
        }

        /// <summary>
        /// Handles the ActualDependentValueChanged event of a DataPoint.
        /// </summary>
        /// <param name="sender">Event source.</param>
        /// <param name="e">Event arguments.</param>
        private void DataPointActualDependentValueChanged(object sender, RoutedPropertyChangedEventArgs<IComparable> e)
        {
            DataPoint dataPoint = (DataPoint)sender;
            QueueUpdateDataItemPlacement(true, false, DataItems.Where(di => di.DataPoint == dataPoint));
        }

        /// <summary>
        /// Handles the IndependentValueChanged event of a DataPoint.
        /// </summary>
        /// <param name="sender">Event source.</param>
        /// <param name="e">Event arguments.</param>
        private void DataPointIndependentValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            DataPoint dataPoint = (DataPoint)sender;
            SeriesDefinition definition = DataItemFromDataPoint(dataPoint).SeriesDefinition;
            TimeSpan transitionDuration = definition.TransitionDuration;
            if (0 < transitionDuration.TotalMilliseconds)
            {
                dataPoint.BeginAnimation(DataPoint.ActualIndependentValueProperty, "ActualIndependentValue", e.NewValue, definition.TransitionDuration, definition.TransitionEasingFunction);
            }
            else
            {
                dataPoint.ActualIndependentValue = e.NewValue;
            }
        }

        /// <summary>
        /// Handles the ActualIndependentValueChanged event of a DataPoint.
        /// </summary>
        /// <param name="sender">Event source.</param>
        /// <param name="e">Event arguments.</param>
        private void DataPointActualIndependentValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            DataPoint dataPoint = (DataPoint)sender;
            QueueUpdateDataItemPlacement(false, true, DataItems.Where(di => di.DataPoint == dataPoint));
        }

        /// <summary>
        /// Handles the StateChanged event of a DataPoint.
        /// </summary>
        /// <param name="sender">Event source.</param>
        /// <param name="e">Event arguments.</param>
        private void DataPointStateChanged(object sender, RoutedPropertyChangedEventArgs<DataPointState> e)
        {
            DataPoint dataPoint = (DataPoint)sender;
            if (DataPointState.Hidden == dataPoint.State)
            {
                DataItems.Remove(DataItems.Where(di => di.DataPoint == dataPoint).Single());
                RemovedDataItems();
            }
        }

        /// <summary>
        /// Notifies the specified axis of changes to values plotting against it.
        /// </summary>
        /// <param name="axis">Specified axis.</param>
        protected void NotifyAxisValuesChanged(IAxis axis)
        {
            if (null != axis)
            {
                IRangeConsumer rangeConsumer = axis as IRangeConsumer;
                if (null != rangeConsumer)
                {
                    IRangeProvider rangeProvider = (IRangeProvider)this;
                    rangeConsumer.RangeChanged(rangeProvider, new Range<IComparable>() /*rangeProvider.GetRange(rangeConsumer)*/);
                }
                IDataConsumer dataConsumer = axis as IDataConsumer;
                if (null != dataConsumer)
                {
                    IDataProvider dataProvider = (IDataProvider)this;
                    dataConsumer.DataChanged(dataProvider, null /*dataProvider.GetData(dataConsumer)*/);
                }
            }
        }

        /// <summary>
        /// Notifies the specified axis of changes to value margins plotting against it.
        /// </summary>
        /// <param name="axis">Specified axis.</param>
        /// <param name="valueMargins">Sequence of value margins that have changed.</param>
        protected void NotifyValueMarginsChanged(IAxis axis, IEnumerable<ValueMargin> valueMargins)
        {
            if (null != axis)
            {
                IValueMarginConsumer valueMarginConsumer = axis as IValueMarginConsumer;
                if (null != valueMarginConsumer)
                {
                    IValueMarginProvider valueMarginProvider = (IValueMarginProvider)this;
                    valueMarginConsumer.ValueMarginsChanged(valueMarginProvider, valueMargins);
                }
            }
        }

        /// <summary>
        /// Handles the CollectionChanged event of the SeriesDefinitions collection.
        /// </summary>
        /// <param name="sender">Event source.</param>
        /// <param name="e">Event arguments.</param>
        private void SeriesDefinitionsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            SeriesDefinitionsCollectionChanged(e.Action, e.OldItems, e.OldStartingIndex, e.NewItems, e.NewStartingIndex);
        }

        /// <summary>
        /// Handles the CollectionChanged event of the SeriesDefinitions collection.
        /// </summary>
        /// <param name="action">Type of change.</param>
        /// <param name="oldItems">Sequence of old items.</param>
        /// <param name="oldStartingIndex">Starting index of old items.</param>
        /// <param name="newItems">Sequence of new items.</param>
        /// <param name="newStartingIndex">Starting index of new items.</param>
        protected virtual void SeriesDefinitionsCollectionChanged(NotifyCollectionChangedAction action, IList oldItems, int oldStartingIndex, IList newItems, int newStartingIndex)
        {
            if (null != oldItems)
            {
                foreach (SeriesDefinition oldDefinition in oldItems.CastWrapper<SeriesDefinition>())
                {
                    ISeries oldSeries = (ISeries)oldDefinition;
                    SeriesDefinitionItemsSourceChanged(oldDefinition, oldDefinition.ItemsSource, null);
                    _seriesDefinitionsAsISeries.Remove(oldDefinition);
                    _legendItems.ChildCollections.Remove(oldSeries.LegendItems);
                    UpdatePaletteProperties(oldDefinition);
                    oldSeries.SeriesHost = null;
                    oldDefinition.Index = -1;
                }
            }
            if (null != newItems)
            {
                int index = newStartingIndex;
                foreach (SeriesDefinition newDefinition in newItems.CastWrapper<SeriesDefinition>())
                {
                    ISeries newSeries = (ISeries)newDefinition;
                    newSeries.SeriesHost = this;
                    UpdatePaletteProperties(newDefinition);
                    _legendItems.ChildCollections.Add(newSeries.LegendItems);
                    _seriesDefinitionsAsISeries.Add(newDefinition);
                    newDefinition.Index = index;
                    SeriesDefinitionItemsSourceChanged(newDefinition, null, newDefinition.ItemsSource);
                    index++;
                }
            }
        }

        /// <summary>
        /// Updates the palette properties of the specified SeriesDefinition.
        /// </summary>
        /// <param name="definition">Specified SeriesDefinition.</param>
        private void UpdatePaletteProperties(SeriesDefinition definition)
        {
            ResourceDictionary resources = null;
            if (null != SeriesHost)
            {
                Type dataPointType = CreateDataPoint().GetType();
                using (IEnumerator<ResourceDictionary> enumerator = SeriesHost.GetResourceDictionariesWhere(dictionary =>
                {
                    Style style = dictionary["DataPointStyle"] as Style;
                    if (null != style)
                    {
                        return (null != style.TargetType) && (style.TargetType.IsAssignableFrom(dataPointType));
                    }
                    return false;
                }))
                {
                    if (enumerator.MoveNext())
                    {
                        resources = enumerator.Current;
                    }
                }
            }
            definition.PaletteDataPointStyle = (null != resources) ? resources["DataPointStyle"] as Style : null;
            definition.PaletteDataShapeStyle = (null != resources) ? resources["DataShapeStyle"] as Style : null;
            definition.PaletteLegendItemStyle = (null != resources) ? resources["LegendItemStyle"] as Style : null;
        }

        /// <summary>
        /// Handles changes to the ItemsSource of a SeriesDefinition.
        /// </summary>
        /// <param name="definition">SeriesDefinition owner.</param>
        /// <param name="oldValue">Old value.</param>
        /// <param name="newValue">New value.</param>
        internal void SeriesDefinitionItemsSourceChanged(SeriesDefinition definition, IEnumerable oldValue, IEnumerable newValue)
        {
            if (null != oldValue)
            {
                foreach (DataItem dataItem in DataItems.Where(di => di.SeriesDefinition == definition).ToArray())
                {
                    DataItems.Remove(dataItem);
                }
                RemovedDataItems();
            }
            if (null != newValue)
            {
                // No need to add items if SeriesHost null; setting SeriesHost will take care of that
                if (null != SeriesHost)
                {
                    AddDataItems(definition, newValue.CastWrapper<object>(), 0);
                }
            }
        }

        /// <summary>
        /// Handles changes to the ItemsSource collection  of a SeriesDefinition.
        /// </summary>
        /// <param name="definition">SeriesDefinition owner.</param>
        /// <param name="action">Type of change.</param>
        /// <param name="oldItems">Sequence of old items.</param>
        /// <param name="oldStartingIndex">Starting index of old items.</param>
        /// <param name="newItems">Sequence of new items.</param>
        /// <param name="newStartingIndex">Starting index of new items.</param>
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "Linq is artificially increasing the rating.")]
        internal void SeriesDefinitionItemsSourceCollectionChanged(SeriesDefinition definition, NotifyCollectionChangedAction action, IList oldItems, int oldStartingIndex, IList newItems, int newStartingIndex)
        {
            if (NotifyCollectionChangedAction.Replace == action)
            {
                // Perform in-place replacements
                foreach (DataItem dataItem in DataItems.Where(di => (di.SeriesDefinition == definition) && (newStartingIndex <= di.Index) && (di.Index < newStartingIndex + newItems.Count)))
                {
                    dataItem.Value = newItems[dataItem.Index - newStartingIndex];
                }
            }
            else
            {
                if (NotifyCollectionChangedAction.Reset == action)
                {
                    // Set up parameters to allow normal old/new item handling to be used
                    Debug.Assert(null == oldItems, "Reset action with non-null oldItems.");
                    oldItems = DataItems.Where(di => (di.SeriesDefinition == definition)).ToArray();
                    oldStartingIndex = 0;
                    newItems = definition.ItemsSource.CastWrapper<object>().ToArray();
                    newStartingIndex = 0;
                }
                if (null != oldItems)
                {
                    // Get rid of old items
                    foreach (DataItem oldDataItem in DataItems.Where(di => (di.SeriesDefinition == definition) && (oldStartingIndex <= di.Index) && (di.Index < oldStartingIndex + oldItems.Count)))
                    {
                        oldDataItem.Index = -1;
                        if (null != oldDataItem.DataPoint)
                        {
                            oldDataItem.DataPoint.State = DataPointState.Hiding;
                        }
                    }
                    // Adjust index of shifted items
                    foreach (DataItem dataItem in DataItems.Where(di => (di.SeriesDefinition == definition) && (oldStartingIndex + oldItems.Count <= di.Index)))
                    {
                        dataItem.Index -= oldItems.Count;
                    }
                }
                if (null != newItems)
                {
                    // Adjust index of shifted items
                    foreach (DataItem dataItem in DataItems.Where(di => (di.SeriesDefinition == definition) && (newStartingIndex <= di.Index)))
                    {
                        dataItem.Index += newItems.Count;
                    }
                    // Add new items
                    AddDataItems(definition, newItems.CastWrapper<object>(), newStartingIndex);
                }
            }
#if DEBUG
            // Validate all DataItem index and value properties
            foreach (var group in DataItems.Where(di => 0 <= di.Index).OrderBy(di => di.Index).GroupBy(di => di.SeriesDefinition))
            {
                object[] items = group.Key.ItemsSource.CastWrapper<object>().ToArray();
                int i = 0;
                foreach (DataItem dataItem in group)
                {
                    Debug.Assert(i == dataItem.Index, "DataItem index mis-match.");
                    Debug.Assert(dataItem.Value.Equals(items[i]), "DataItem value mis-match.");
                    i++;
                }
            }
#endif
        }

        /// <summary>
        /// Handles the ResourceDictionariesChanged event of the SeriesHost owner.
        /// </summary>
        /// <param name="sender">Event source.</param>
        /// <param name="e">Event arguments.</param>
        private void SeriesHostResourceDictionariesChanged(object sender, EventArgs e)
        {
            foreach (SeriesDefinition definition in SeriesDefinitions)
            {
                UpdatePaletteProperties(definition);
            }
        }

        /// <summary>
        /// Creates and adds DataItems for the specified SeriesDefinition's items.
        /// </summary>
        /// <param name="definition">Specified SeriesDefinition.</param>
        /// <param name="items">Sequence of items.</param>
        /// <param name="startingIndex">Starting index.</param>
        private void AddDataItems(SeriesDefinition definition, IEnumerable<object> items, int startingIndex)
        {
            int index = startingIndex;
            foreach (object item in items)
            {
                DataItems.Add(new DataItem(definition) { Value = item, Index = index });
                index++;
            }
            // Because properties (like DependentValueBinding) may still be getting set
            Dispatcher.BeginInvoke((Action)AddedDataItems);
        }

        /// <summary>
        /// Updates the axes after DataItems have been added.
        /// </summary>
        private void AddedDataItems()
        {
            EnsureAxes(false, false, true);
        }

        /// <summary>
        /// Notifies the axes after DataItems have been removed.
        /// </summary>
        private void RemovedDataItems()
        {
            NotifyAxisValuesChanged(ActualIndependentAxis);
            NotifyAxisValuesChanged(ActualDependentAxis);
        }

        /// <summary>
        /// Ensures that suitable axes are present and registered.
        /// </summary>
        /// <param name="updateDependentAxis">True if the dependent axis needs to be updated.</param>
        /// <param name="updateIndependentAxis">True if the independent axis needs to be updated.</param>
        /// <param name="unconditionallyNotifyAxes">True if both axis are to be notified unconditionally.</param>
        private void EnsureAxes(bool updateDependentAxis, bool updateIndependentAxis, bool unconditionallyNotifyAxes)
        {
            foreach (SeriesDefinition definition in SeriesDefinitions)
            {
                if (null == definition.DependentValueBinding)
                {
                    throw new InvalidOperationException(Properties.Resources.DefinitionSeries_EnsureAxes_MissingDependentValueBinding);
                }
                if (null == definition.IndependentValueBinding)
                {
                    throw new InvalidOperationException(Properties.Resources.DefinitionSeries_EnsureAxes_MissingIndependentValueBinding);
                }
            }
            if ((null != SeriesHost) && DataItems.Any())
            {
                // Ensure a dependent axis is present or updated
                bool changedActualDependentAxis = false;
                if (updateDependentAxis && (null != ActualDependentAxis))
                {
                    ActualDependentAxis.RegisteredListeners.Remove(this);
                    ActualDependentAxis = null;
                }
                if (null == ActualDependentAxis)
                {
                    ActualDependentAxis = DependentAxis ?? AcquireDependentAxis();
                    ActualDependentAxis.RegisteredListeners.Add(this);
                    if (!SeriesHost.Axes.Contains(ActualDependentAxis))
                    {
                        SeriesHost.Axes.Add(ActualDependentAxis);
                    }
                    changedActualDependentAxis = true;
                }
                // Ensure an independent axis is present or updated
                bool changedActualIndependentAxis = false;
                if (updateIndependentAxis && (null != ActualIndependentAxis))
                {
                    ActualIndependentAxis.RegisteredListeners.Remove(this);
                    ActualIndependentAxis = null;
                }
                if (null == ActualIndependentAxis)
                {
                    ActualIndependentAxis = IndependentAxis ?? AcquireIndependentAxis();
                    ActualIndependentAxis.RegisteredListeners.Add(this);
                    if (!SeriesHost.Axes.Contains(ActualIndependentAxis))
                    {
                        SeriesHost.Axes.Add(ActualIndependentAxis);
                    }
                    changedActualIndependentAxis = true;
                }
                // Queue an update if necessary or requested
                if (changedActualDependentAxis || changedActualIndependentAxis || unconditionallyNotifyAxes)
                {
                    QueueUpdateDataItemPlacement(changedActualDependentAxis || unconditionallyNotifyAxes, changedActualIndependentAxis || unconditionallyNotifyAxes, DataItems);
                }
            }
        }

        /// <summary>
        /// Acquires a dependent axis suitable for use with the data values of the series.
        /// </summary>
        /// <returns>Axis instance.</returns>
        protected abstract IAxis AcquireDependentAxis();

        /// <summary>
        /// Acquires an independent axis suitable for use with the data values of the series.
        /// </summary>
        /// <returns>Axis instance.</returns>
        protected abstract IAxis AcquireIndependentAxis();

        /// <summary>
        /// Handles notification of the invalidation of an axis.
        /// </summary>
        /// <param name="axis">Invalidated axis.</param>
        void IAxisListener.AxisInvalidated(IAxis axis)
        {
            QueueUpdateDataItemPlacement(false, false, DataItems);
        }

        /// <summary>
        /// Queues an update of DataItem placement for the next update opportunity.
        /// </summary>
        /// <param name="dependentAxisValuesChanged">True if the dependent axis values have changed.</param>
        /// <param name="independentAxisValuesChanged">True if the independent axis values have changed.</param>
        /// <param name="dataItems">Sequence of DataItems to update.</param>
        private void QueueUpdateDataItemPlacement(bool dependentAxisValuesChanged, bool independentAxisValuesChanged, IEnumerable<DataItem> dataItems)
        {
            _queueUpdateDataItemPlacement_DependentAxisValuesChanged |= dependentAxisValuesChanged;
            _queueUpdateDataItemPlacement_IndependentAxisValuesChanged |= independentAxisValuesChanged;
            _queueUpdateDataItemPlacement_DataItems.AddRange(dataItems);
            InvalidateArrange();
        }

        /// <summary>
        /// Called when the control needs to arrange its children.
        /// </summary>
        /// <param name="arrangeBounds">Bounds to arrange within.</param>
        /// <returns>Arranged size.</returns>
        /// <remarks>
        /// Used as a good place to dequeue queued work.
        /// </remarks>
        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            Size arrangedSize = base.ArrangeOverride(arrangeBounds);
            if (_queueUpdateDataItemPlacement_DependentAxisValuesChanged)
            {
                NotifyAxisValuesChanged(ActualDependentAxis);
                _queueUpdateDataItemPlacement_DependentAxisValuesChanged = false;
            }
            if (_queueUpdateDataItemPlacement_IndependentAxisValuesChanged)
            {
                NotifyAxisValuesChanged(ActualIndependentAxis);
                _queueUpdateDataItemPlacement_IndependentAxisValuesChanged = false;
            }
            UpdateDataItemPlacement(_queueUpdateDataItemPlacement_DataItems.Distinct());
            _queueUpdateDataItemPlacement_DataItems.Clear();
            return arrangedSize;
        }

        /// <summary>
        /// Updates the placement of the DataItems (data points) of the series.
        /// </summary>
        /// <param name="dataItems">DataItems in need of an update.</param>
        protected abstract void UpdateDataItemPlacement(IEnumerable<DataItem> dataItems);

        /// <summary>
        /// Returns the range for the data points of the series.
        /// </summary>
        /// <param name="rangeConsumer">Consumer of the range.</param>
        /// <returns>Range of values.</returns>
        Range<IComparable> IRangeProvider.GetRange(IRangeConsumer rangeConsumer)
        {
            return IRangeProviderGetRange(rangeConsumer);
        }

        /// <summary>
        /// Returns the range for the data points of the series.
        /// </summary>
        /// <param name="rangeConsumer">Consumer of the range.</param>
        /// <returns>Range of values.</returns>
        protected virtual Range<IComparable> IRangeProviderGetRange(IRangeConsumer rangeConsumer)
        {
            if (rangeConsumer == ActualIndependentAxis)
            {
                if (ActualIndependentAxis.CanPlot(0.0))
                {
                    return IndependentValueGroups
                        .Select(g => ValueHelper.ToDouble(g.IndependentValue))
                        .Where(d => ValueHelper.CanGraph(d))
                        .DefaultIfEmpty()
                        .CastWrapper<IComparable>()
                        .GetRange();
                }
                else
                {
                    return IndependentValueGroups
                        .Select(g => ValueHelper.ToDateTime(g.IndependentValue))
                        .DefaultIfEmpty()
                        .CastWrapper<IComparable>()
                        .GetRange();
                }
            }
            throw new NotSupportedException();
        }

        /// <summary>
        /// Returns the value margins for the data points of the series.
        /// </summary>
        /// <param name="valueMarginConsumer">Consumer of the value margins.</param>
        /// <returns>Sequence of value margins.</returns>
        IEnumerable<ValueMargin> IValueMarginProvider.GetValueMargins(IValueMarginConsumer valueMarginConsumer)
        {
            return IValueMarginProviderGetValueMargins(valueMarginConsumer);
        }

        /// <summary>
        /// Returns the value margins for the data points of the series.
        /// </summary>
        /// <param name="valueMarginConsumer">Consumer of the value margins.</param>
        /// <returns>Sequence of value margins.</returns>
        protected virtual IEnumerable<ValueMargin> IValueMarginProviderGetValueMargins(IValueMarginConsumer valueMarginConsumer)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns the data for the data points of the series.
        /// </summary>
        /// <param name="dataConsumer">Consumer of the data.</param>
        /// <returns>Sequence of data.</returns>
        IEnumerable<object> IDataProvider.GetData(IDataConsumer dataConsumer)
        {
            return IDataProviderGetData(dataConsumer);
        }

        /// <summary>
        /// Returns the data for the data points of the series.
        /// </summary>
        /// <param name="dataConsumer">Consumer of the data.</param>
        /// <returns>Sequence of data.</returns>
        protected virtual IEnumerable<object> IDataProviderGetData(IDataConsumer dataConsumer)
        {
            if (dataConsumer == ActualIndependentAxis)
            {
                return IndependentValueGroups.Select(cg => cg.IndependentValue).Distinct();
            }
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets a sequence of IndependentValueGroups.
        /// </summary>
        protected virtual IEnumerable<IndependentValueGroup> IndependentValueGroups
        {
            get
            {
                return DataItems
                    .GroupBy(di => di.ActualIndependentValue)
                    .Select(g => new IndependentValueGroup(g.Key, g.OrderBy(di => di.SeriesDefinition.Index)));
            }
        }

        /// <summary>
        /// Gets a sequence of IndependentValueGroups ordered by independent value.
        /// </summary>
        protected IEnumerable<IndependentValueGroup> IndependentValueGroupsOrderedByIndependentValue
        {
            get
            {
                return IndependentValueGroups
                    .OrderBy(g => g.IndependentValue);
            }
        }

        /// <summary>
        /// Gets a sequence of sequences of the dependent values associated with each independent value.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Nesting reflects the actual hierarchy of the data.")]
        protected IEnumerable<IEnumerable<double>> IndependentValueDependentValues
        {
            get
            {
                return IndependentValueGroups
                    .Select(g =>
                    {
                        g.Denominator = IsStacked100 ?
                            g.DataItems.Sum(di => Math.Abs(ValueHelper.ToDouble(di.ActualDependentValue))) :
                            1;
                        if (0 == g.Denominator)
                        {
                            g.Denominator = 1;
                        }
                        return g;
                    })
                    .Select(g => g.DataItems
                        .Select(di => ValueHelper.ToDouble(di.ActualDependentValue) * (IsStacked100 ? (100 / g.Denominator) : 1)));
            }
        }

        /// <summary>
        /// Represents an independent value and the dependent values that are associated with it.
        /// </summary>
        protected class IndependentValueGroup
        {
            /// <summary>
            /// Initializes a new instance of the IndependentValueGroup class.
            /// </summary>
            /// <param name="independentValue">Independent value.</param>
            /// <param name="dataItems">Associated DataItems.</param>
            public IndependentValueGroup(object independentValue, IEnumerable<DataItem> dataItems)
            {
                IndependentValue = independentValue;
                DataItems = dataItems;
            }

            /// <summary>
            /// Gets the independent value.
            /// </summary>
            public object IndependentValue { get; private set; }

            /// <summary>
            /// Gets a sequence of DataItems associated with the independent value.
            /// </summary>
            public IEnumerable<DataItem> DataItems { get; private set; }

            /// <summary>
            /// Gets or sets the denominator to use when computing with this instance.
            /// </summary>
            /// <remarks>
            /// Exists here purely to simplify the the corresponding algorithm.
            /// </remarks>
            public double Denominator { get; set; }
        }

        /// <summary>
        /// Represents a single data value from a SeriesDefinition's ItemsSource.
        /// </summary>
        protected class DataItem
        {
            /// <summary>
            /// Stores a reference to a shared BindingHelper instance.
            /// </summary>
            private static readonly BindingHelper _bindingHelper = new BindingHelper();

            /// <summary>
            /// Initializes a new instance of the DataItem class.
            /// </summary>
            /// <param name="seriesDefinition">SeriesDefinition owner.</param>
            public DataItem(SeriesDefinition seriesDefinition)
            {
                SeriesDefinition = seriesDefinition;
                CenterPoint = new Point(double.NaN, double.NaN);
            }

            /// <summary>
            /// Gets the SeriesDefinition owner of the DataItem.
            /// </summary>
            public SeriesDefinition SeriesDefinition { get; private set; }

            /// <summary>
            /// Gets or sets the value of the DataItem.
            /// </summary>
            public object Value
            {
                get { return _value; }
                set
                {
                    _value = value;
                    if (null != DataPoint)
                    {
                        DataPoint.DataContext = value;
                    }
                }
            }

            /// <summary>
            /// Stores the value of the DataItem.
            /// </summary>
            private object _value;

            /// <summary>
            /// Gets or sets the index of the DataItem.
            /// </summary>
            public int Index { get; set; }

            /// <summary>
            /// Gets or sets the DataPoint associated with the DataItem.
            /// </summary>
            public DataPoint DataPoint { get; set; }

            /// <summary>
            /// Gets or sets the container for the DataPoint within its parent ItemsControl.
            /// </summary>
            public UIElement Container { get; set; }

            /// <summary>
            /// Gets the ActualDependentValue of the DataPoint (or its equivalent).
            /// </summary>
            public IComparable ActualDependentValue
            {
                get
                {
                    if (null != DataPoint)
                    {
                        return DataPoint.ActualDependentValue;
                    }
                    else
                    {
                        return (IComparable)_bindingHelper.EvaluateBinding(SeriesDefinition.DependentValueBinding, Value);
                    }
                }
            }

            /// <summary>
            /// Gets the ActualIndependentValue of the DataPoint (or its equivalent).
            /// </summary>
            public object ActualIndependentValue
            {
                get
                {
                    if (null != DataPoint)
                    {
                        return DataPoint.ActualIndependentValue;
                    }
                    else
                    {
                        return _bindingHelper.EvaluateBinding(SeriesDefinition.IndependentValueBinding, Value);
                    }
                }
            }

            /// <summary>
            /// Gets or sets the ActualDependentValue of the DataPoint after adjusting for applicable stacking.
            /// </summary>
            public double ActualStackedDependentValue { get; set; }

            /// <summary>
            /// Gets or sets the center-point of the DataPoint in plot area coordinates (if relevant).
            /// </summary>
            public Point CenterPoint { get; set; }
        }

        /// <summary>
        /// Provides an easy way to evaluate a Binding against a source instance.
        /// </summary>
        private class BindingHelper : FrameworkElement
        {
            /// <summary>
            /// Initializes a new instance of the BindingHelper class.
            /// </summary>
            public BindingHelper()
            {
            }

            /// <summary>
            /// Identifies the Result dependency property.
            /// </summary>
            private static readonly DependencyProperty ResultProperty =
                DependencyProperty.Register("Result", typeof(object), typeof(BindingHelper), null);

            /// <summary>
            /// Evaluates a Binding against a source instance.
            /// </summary>
            /// <param name="binding">Binding to evaluate.</param>
            /// <param name="instance">Source instance.</param>
            /// <returns>Result of Binding on source instance.</returns>
            public object EvaluateBinding(Binding binding, object instance)
            {
                DataContext = instance;
                SetBinding(ResultProperty, binding);
                object result = GetValue(ResultProperty);
                ClearValue(ResultProperty);
                DataContext = null;
                return result;
            }
        }

        /// <summary>
        /// Converts from a selected item to the corresponding DataItem.
        /// </summary>
        private class SelectedItemToDataItemConverter : IValueConverter
        {
            /// <summary>
            /// Stores a reference to the DataItem collection.
            /// </summary>
            private ObservableCollection<DataItem> _dataItems;

            /// <summary>
            /// Initializes a new instance of the SelectedItemToDataItemConverter class.
            /// </summary>
            /// <param name="dataItems">Collection of DataItems.</param>
            public SelectedItemToDataItemConverter(ObservableCollection<DataItem> dataItems)
            {
                _dataItems = dataItems;
            }

            /// <summary>
            /// Converts a value.
            /// </summary>
            /// <param name="value">The value produced by the binding source.</param>
            /// <param name="targetType">The type of the binding target property.</param>
            /// <param name="parameter">The converter parameter to use.</param>
            /// <param name="culture">The culture to use in the converter.</param>
            /// <returns>Converted value.</returns>
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                return _dataItems.Where(di => di.Value == value).FirstOrDefault();
            }

            /// <summary>
            /// Converts a value back.
            /// </summary>
            /// <param name="value">The value produced by the binding source.</param>
            /// <param name="targetType">The type of the binding target property.</param>
            /// <param name="parameter">The converter parameter to use.</param>
            /// <param name="culture">The culture to use in the converter.</param>
            /// <returns>Converted value.</returns>
            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                DataItem dataItem = value as DataItem;
                return (null != dataItem) ? dataItem.Value : null;
            }
        }

        /// <summary>
        /// Converts from a SeriesSelectionMode to a true/false value indicating whether selection is enabled.
        /// </summary>
        private class SelectionModeToSelectionEnabledConverter : IValueConverter
        {
            /// <summary>
            /// Initializes a new instance of the SelectionModeToSelectionEnabledConverter class.
            /// </summary>
            public SelectionModeToSelectionEnabledConverter()
            {
            }

            /// <summary>
            /// Converts a value.
            /// </summary>
            /// <param name="value">The value produced by the binding source.</param>
            /// <param name="targetType">The type of the binding target property.</param>
            /// <param name="parameter">The converter parameter to use.</param>
            /// <param name="culture">The culture to use in the converter.</param>
            /// <returns>Converted value.</returns>
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                bool isSelectionEnabled = false;
                if (value is SeriesSelectionMode)
                {
                    isSelectionEnabled = !(SeriesSelectionMode.None == (SeriesSelectionMode)value);
                }
                return isSelectionEnabled;
            }

            /// <summary>
            /// Converts a value back.
            /// </summary>
            /// <param name="value">The value produced by the binding source.</param>
            /// <param name="targetType">The type of the binding target property.</param>
            /// <param name="parameter">The converter parameter to use.</param>
            /// <param name="culture">The culture to use in the converter.</param>
            /// <returns>Converted value.</returns>
            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Gets the axes for the series as a series host.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations", Justification = "Property exists as an interface requirement; implementation is unnecessary.")]
        ObservableCollection<IAxis> ISeriesHost.Axes
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Gets the series for the series as a series host.
        /// </summary>
        ObservableCollection<ISeries> ISeriesHost.Series
        {
            get { return _seriesDefinitionsAsISeries; }
        }

        /// <summary>
        /// Gets the foreground elements for the series as a series host.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations", Justification = "Property exists as an interface requirement; implementation is unnecessary.")]
        ObservableCollection<UIElement> ISeriesHost.ForegroundElements
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Gets the background elements for the series as a series host.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations", Justification = "Property exists as an interface requirement; implementation is unnecessary.")]
        ObservableCollection<UIElement> ISeriesHost.BackgroundElements
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Gets a IResourceDictionaryDispenser for the series as a series host.
        /// </summary>
        /// <param name="predicate">Predicate function.</param>
        /// <returns>Sequence of ResourceDictionaries.</returns>
        IEnumerator<ResourceDictionary> IResourceDictionaryDispenser.GetResourceDictionariesWhere(Func<ResourceDictionary, bool> predicate)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Event that is triggered when the available ResourceDictionaries change.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations", Justification = "Property exists as an interface requirement; implementation is unnecessary.")]
        event EventHandler IResourceDictionaryDispenser.ResourceDictionariesChanged
        {
            add { throw new NotImplementedException(); }
            remove { throw new NotImplementedException(); }
        }
    }
}
