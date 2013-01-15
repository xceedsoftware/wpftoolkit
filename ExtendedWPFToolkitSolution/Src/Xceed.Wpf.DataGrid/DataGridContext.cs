/************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2010-2012 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus edition at http://xceed.com/wpf_toolkit

   Visit http://xceed.com and follow @datagrid on Twitter

  **********************************************************************/

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System;
using System.Collections.Generic;
using System.Windows.Data;
using System.Collections.Specialized;
using System.Windows.Input;
using System.Diagnostics;
using System.Collections;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Threading;
using Xceed.Utils.Collections;
using Xceed.Wpf.DataGrid.Views;
using System.Globalization;
using Xceed.Wpf.DataGrid.Automation;
using System.Windows.Automation.Peers;
using Xceed.Wpf.DataGrid.Utils;

namespace Xceed.Wpf.DataGrid
{
  public sealed class DataGridContext : DependencyObject, INotifyPropertyChanged, IWeakEventListener, IDataGridContextVisitable, ICustomTypeDescriptor
  {
    #region Constructors

    internal DataGridContext(
      DataGridContext parentDataGridContext,
      DataGridControl dataGridControl,
      object parentItem,
      CollectionView collectionView,
      DetailConfiguration detailConfiguration )
    {
      if( dataGridControl == null )
        throw new ArgumentNullException( "dataGridControl" );

      if( collectionView == null )
        throw new ArgumentNullException( "collectionView" );

      // ParentItem cannot be null except when dealing with the root DataGridContext.
      if( ( parentDataGridContext != null ) && ( parentItem == null ) )
        throw new ArgumentNullException( "parentItem" );

      m_parentDataGridContext = parentDataGridContext; //voluntarily not validating if not null... this will be null for master level.
      m_dataGridControl = dataGridControl;
      m_dataGridControlItemsSource = m_dataGridControl.ItemsSource;
      m_parentItem = parentItem;
      m_items = collectionView;
      m_detailConfig = detailConfiguration; //voluntarily not validating if not null... this will be null for master level.

      m_currencyManager = new CurrencyManager( this, m_items );

      m_selectedItemsStore = new SelectedItemsStorage( this, 2 );
      m_selectedCellsStore = new SelectedCellsStorage( this, 2 );
      m_selectedItemsRanges = new SelectionItemRangeCollection( m_selectedItemsStore );
      m_selectedCellRanges = new SelectionCellRangeCollection( m_selectedCellsStore );
      m_selectedItems = new SelectionItemCollection( m_selectedItemsStore );

      CollectionChangedEventManager.AddListener( m_items, this );

      if( detailConfiguration == null )
      {
        //If the detailConfiguration is null then we are in the master...

        this.CreateStandaloneCollections();
        this.SetupCollectionsChangeNotification();

        GroupConfigurationSelectorChangedEventManager.AddListener( m_dataGridControl, this );
        AllowDetailToggleChangedEventManager.AddListener( m_dataGridControl, this );
        MaxGroupLevelsChangedEventManager.AddListener( m_dataGridControl, this );
        MaxSortLevelsChangedEventManager.AddListener( m_dataGridControl, this );
        ItemsSourceChangeCompletedEventManager.AddListener( m_dataGridControl, this );
      }
      else
      {
        //Detail DataGridContext


        //only listen to the detail grid config current column changed if the detail grid config is synchronized.
        CurrentColumnChangedEventManager.AddListener( m_detailConfig, this );

        GroupConfigurationSelectorChangedEventManager.AddListener( m_detailConfig, this );
        AllowDetailToggleChangedEventManager.AddListener( m_detailConfig, this );

        // Register to the VisibleColumnsChanged to update, if need be, the columns desired width
        // when column stretching is active and there's a column reordering.
        CollectionChangedEventManager.AddListener( m_detailConfig.VisibleColumns, this );

        MaxGroupLevelsChangedEventManager.AddListener( m_detailConfig, this );
        MaxSortLevelsChangedEventManager.AddListener( m_detailConfig, this );

        m_detailConfig.RequestingDelayBringIntoViewAndFocusCurrent += this.OnDetailConfigRequestingDelayBringIntoViewAndFocusCurrent;
        m_detailConfig.DetailConfigurations.DataGridControl = dataGridControl;
      }

      CollectionChangedEventManager.AddListener( this.DetailConfigurations, this );
      VisibleColumnsUpdatedEventManager.AddListener( this.Columns, this );
      VisibleColumnsUpdatingEventManager.AddListener( this.Columns, this );
      RealizedContainersRequestedEventManager.AddListener( this.Columns, this );
      DistinctValuesRequestedEventManager.AddListener( this.Columns, this );

      DetailVisibilityChangedEventManager.AddListener( this.DetailConfigurations, this );

      this.SetupViewProperties( true );
      this.InitializeViewProperties();

      ViewChangedEventManager.AddListener( m_dataGridControl, this );

      GroupLevelDescriptionCollection groupLevelDescriptions = this.GroupLevelDescriptions;
      ObservableCollection<GroupDescription> groupDescriptions = m_items.GroupDescriptions;
      if( groupLevelDescriptions.Count != groupDescriptions.Count )
      {
        DataGridContext.UpdateGroupLevelDescriptions( groupLevelDescriptions, new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Reset ), groupDescriptions, this.Columns );
      }

      // Set the value of DetailLevel property
      DataGridContext tempDataGridContext = this.ParentDataGridContext;
      while( tempDataGridContext != null )
      {
        this.DetailLevel++;
        tempDataGridContext = tempDataGridContext.ParentDataGridContext;
      }

      this.HookToItemPropertiesChanged();
    }

    public void Items_CollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
    {
      if( m_dataGridControl.SelectedIndexPropertyNeedCoerce )
        m_dataGridControl.CoerceValue( DataGridControl.SelectedIndexProperty );

      if( m_dataGridControl.SelectedItemPropertyNeedCoerce )
        m_dataGridControl.CoerceValue( DataGridControl.SelectedItemProperty );

      if( this.Peer != null )
        m_dataGridControl.QueueDataGridContextPeerChlidrenRefresh( this );

      m_dataGridControl.SelectionChangerManager.UpdateSelectionAfterSourceCollectionChanged( this, e );
    }

    #endregion

    internal void CleanDataGridContext()
    {
      if( m_currencyManager != null )
      {
        m_currencyManager.CleanCurrencyManager();
        m_currencyManager = null;
      }

      if( m_items != null )
      {
        CollectionChangedEventManager.RemoveListener( m_items, this );

        DataGridCollectionViewBase dataGridCollectionViewBase = this.ItemsSourceCollection as DataGridCollectionViewBase;

        if( dataGridCollectionViewBase != null )
          dataGridCollectionViewBase.UnregisterChangedEvent();
      }

      //For non-master DataGridContext
      if( m_detailConfig == null )
      {
        GroupConfigurationSelectorChangedEventManager.RemoveListener( m_dataGridControl, this );
        AllowDetailToggleChangedEventManager.RemoveListener( m_dataGridControl, this );
        MaxGroupLevelsChangedEventManager.RemoveListener( m_dataGridControl, this );
        MaxSortLevelsChangedEventManager.RemoveListener( m_dataGridControl, this );
        ItemsSourceChangeCompletedEventManager.RemoveListener( m_dataGridControl, this );

        if( m_sortDescriptionsCollectionChangedHandler != null )
        {
          // Unregister for change notification of the SortDescriptions
          INotifyCollectionChanged notify = m_items.SortDescriptions;
          notify.CollectionChanged -= m_sortDescriptionsCollectionChangedHandler;
          m_sortDescriptionsCollectionChangedHandler = null;
        }

        if( m_groupDescriptionsCollectionChangedHandler != null )
        {
          // Unregister for change notification of the GroupDescriptions
          m_items.GroupDescriptions.CollectionChanged -= m_groupDescriptionsCollectionChangedHandler;
          m_groupDescriptionsCollectionChangedHandler = null;
        }
      }
      else
      {
        CurrentColumnChangedEventManager.RemoveListener( m_detailConfig, this );
        GroupConfigurationSelectorChangedEventManager.RemoveListener( m_detailConfig, this );
        AllowDetailToggleChangedEventManager.RemoveListener( m_detailConfig, this );
        CollectionChangedEventManager.RemoveListener( m_detailConfig.VisibleColumns, this );
        MaxGroupLevelsChangedEventManager.RemoveListener( m_detailConfig, this );
        MaxSortLevelsChangedEventManager.RemoveListener( m_detailConfig, this );

        m_detailConfig.RequestingDelayBringIntoViewAndFocusCurrent -= OnDetailConfigRequestingDelayBringIntoViewAndFocusCurrent;
      }

      this.ClearSizeStates();

      CollectionChangedEventManager.RemoveListener( this.DetailConfigurations, this );

      DetailVisibilityChangedEventManager.RemoveListener( this.DetailConfigurations, this );

      VisibleColumnsUpdatedEventManager.RemoveListener( this.Columns, this );
      VisibleColumnsUpdatingEventManager.RemoveListener( this.Columns, this );
      RealizedContainersRequestedEventManager.RemoveListener( this.Columns, this );
      DistinctValuesRequestedEventManager.RemoveListener( this.Columns, this );

      ViewChangedEventManager.RemoveListener( m_dataGridControl, this );
      this.UnhookToItemPropertiesChanged( m_dataGridControlItemsSource as DataGridCollectionViewBase );

      // Case 124245 : Be sure to clear the ColumnVirtualizationManager before
      // clearing the ViewProperties since it binds to the FixedColumnCount to
      // avoid Binding Errors
      ColumnVirtualizationManager columnVirtualizationManager = this.ColumnVirtualizationManager;

      if( columnVirtualizationManager != null )
      {
        columnVirtualizationManager.CleanManager();
        ColumnVirtualizationManager.ClearColumnVirtualizationManager( this );
      }

      this.ClearViewPropertyBindings();

      if( m_defaultDetailConfiguration != null )
      {
        GroupConfigurationSelectorChangedEventManager.RemoveListener( m_defaultDetailConfiguration, this );
        m_defaultDetailConfiguration = null;
      }
    }

    //---------- PUBLIC PROPERTIES ----------

    #region ParentDataGridContext Read-Only Property

    public DataGridContext ParentDataGridContext
    {
      get
      {
        return m_parentDataGridContext;
      }
    }

    private DataGridContext m_parentDataGridContext; // = null

    #endregion ParentDataGridContext Read-Only Property

    #region ParentItem Read-Only Property

    private object m_parentItem;

    public object ParentItem
    {
      get
      {
        return m_parentItem;
      }
    }

    #endregion ParentItem Read-Only Property

    #region AllowDetailToggle Property

    public bool AllowDetailToggle
    {
      get
      {
        if( m_detailConfig != null )
        {
          DefaultDetailConfiguration defaultDetailConfig = this.GetDefaultDetailConfigurationForContext();
          if( defaultDetailConfig != null )
          {
            return defaultDetailConfig.AllowDetailToggle;
          }
          else
          {
            return m_detailConfig.AllowDetailToggle;
          }
        }

        return m_dataGridControl.AllowDetailToggle;
      }
    }

    #endregion

    #region DistinctValues Property

    public IDictionary<string, ReadOnlyObservableHashList> DistinctValues
    {
      get
      {
        DataGridCollectionViewBase dataGridCollectionViewBase =
                  this.ItemsSourceCollection as DataGridCollectionViewBase;

        if( dataGridCollectionViewBase == null )
          return null;

        return dataGridCollectionViewBase.DistinctValues;
      }
    }

    #endregion

    #region Columns Read-Only property

    public ColumnCollection Columns
    {
      get
      {
        if( m_detailConfig != null )
          return m_detailConfig.Columns;

        return m_columns;
      }
    }

    private ColumnCollection m_columns; // = null, this variable will store the ColumnCollection only when
    //the DataGridContext is used "desynchronized".

    #endregion

    #region Items Property

    public CollectionView Items
    {
      get
      {
        return m_items;
      }
    }

    private CollectionView m_items;

    #endregion Items Property

    #region OriginalItems Property

    internal IEnumerable ItemsSourceCollection
    {
      get
      {
        if( m_parentDataGridContext != null )
          return m_items;

        return m_dataGridControl.ItemsSource;
      }
    }

    #endregion

    #region CurrentColumn Property

    public ColumnBase CurrentColumn
    {
      get
      {
        if( m_detailConfig != null )
          return m_detailConfig.CurrentColumn;

        return m_currentColumn;
      }
      set
      {
        if( m_currentColumn == value )
          return;

        this.SetCurrentColumnCore( value, true, m_dataGridControl.SynchronizeSelectionWithCurrent );
      }
    }

    internal void SetCurrentColumnCore( ColumnBase column, bool isCancelable, bool synchronizeSelectionWithCurrent )
    {
      this.SetCurrent( this.InternalCurrentItem, null, null, column, false, isCancelable, synchronizeSelectionWithCurrent );
    }

    private ColumnBase m_currentColumn; // = null

    private void SetCurrentColumnHelper( ColumnBase value )
    {
      if( m_detailConfig != null )
      {
        m_detailConfig.CurrentColumn = value;
      }
      else
      {
        m_currentColumn = value;
      }
    }

    #endregion

    #region CurrentItem Properties

    public object CurrentItem
    {
      get
      {
        return m_currentItem;
      }
      set
      {
        if( value == m_currentItem )
          return;

        this.SetCurrentItemCore( value, true, m_dataGridControl.SynchronizeSelectionWithCurrent );
      }
    }

    internal void SetCurrentItemCore( object item, bool isCancelable, bool synchronizeSelectionWithCurrent )
    {
      this.SetCurrent( item, null, null, this.CurrentColumn, false, isCancelable, synchronizeSelectionWithCurrent );
    }

    public int CurrentItemIndex
    {
      get
      {
        return m_currentItemIndex;
      }
      set
      {
        if( value == m_currentItemIndex )
          return;

        this.SetCurrentItemIndexCore( value, true, m_dataGridControl.SynchronizeSelectionWithCurrent );
      }
    }

    internal void SetCurrentItemIndexCore( int index, bool isCancelable, bool synchronizeSelectionWithCurrent )
    {
      this.SetCurrent( this.Items.GetItemAt( index ), null, index, this.CurrentColumn, false, isCancelable, synchronizeSelectionWithCurrent );
    }

    private void SetCurrentItemHelper( object dataItem, int sourceDataItemIndex )
    {
      // This is called only for DataItem
      if( ( m_currentItem == dataItem ) && ( m_currentItemIndex == sourceDataItemIndex ) )
        return;

      m_currentItem = dataItem;
      m_currentItemIndex = sourceDataItemIndex;
      this.OnCurrentItemChanged();
    }

    internal event EventHandler CurrentItemChanged;

    private void OnCurrentItemChanged()
    {
      if( this.CurrentItemChanged != null )
        this.CurrentItemChanged( this, EventArgs.Empty );
    }

    private object m_currentItem; // = null
    private int m_currentItemIndex = -1;

    #endregion CurrentItem

    #region DefaultGroupConfiguration Read-Only Property

    public GroupConfiguration DefaultGroupConfiguration
    {
      get
      {
        if( m_detailConfig != null )
        {
          DefaultDetailConfiguration defaultDetailConfig = this.GetDefaultDetailConfigurationForContext();
          if( defaultDetailConfig != null )
          {
            return defaultDetailConfig.DefaultGroupConfiguration;
          }
          else
          {
            return m_detailConfig.DefaultGroupConfiguration;
          }
        }

        return m_dataGridControl.DefaultGroupConfiguration;
      }
    }

    #endregion

    #region DetailConfigurations Read-Only Property

    internal DetailConfigurationCollection DetailConfigurations
    {
      get
      {
        if( m_detailConfig != null )
        {
          return m_detailConfig.DetailConfigurations;
        }

        return m_dataGridControl.DetailConfigurations;
      }
    }

    #endregion

    #region Footers Read-Only Property

    public ObservableCollection<DataTemplate> Footers
    {
      get
      {
        if( m_detailConfig != null )
        {
          DefaultDetailConfiguration defaultDetailConfig = this.GetDefaultDetailConfigurationForContext();
          if( defaultDetailConfig != null )
          {
            return defaultDetailConfig.Footers;
          }
          else
          {
            return m_detailConfig.Footers;
          }
        }

        Xceed.Wpf.DataGrid.Views.ViewBase view = m_dataGridControl.GetView( false );
        if( view == null )
          return new ObservableCollection<DataTemplate>();

        return view.Footers;
      }
    }

    #endregion

    #region GroupLevelDescriptions Read-Only Property

    public GroupLevelDescriptionCollection GroupLevelDescriptions
    {
      get
      {
        if( m_detailConfig != null )
          return m_detailConfig.GroupLevelDescriptions;

        return m_groupLevelDescriptions;
      }
    }

    private GroupLevelDescriptionCollection m_groupLevelDescriptions; // = null, this variable will store the GroupLevelDescriptions only when
    //the DataGridContext is used "desynchronized".

    #endregion

    #region GroupConfigirationSelector Property

    public GroupConfigurationSelector GroupConfigurationSelector
    {
      get
      {
        if( m_detailConfig != null )
        {
          DefaultDetailConfiguration defaultDetailConfig = this.GetDefaultDetailConfigurationForContext();
          if( defaultDetailConfig != null )
          {
            return defaultDetailConfig.GroupConfigurationSelector;
          }

          return m_detailConfig.GroupConfigurationSelector;
        }
        return m_dataGridControl.GroupConfigurationSelector;
      }
    }

    private void OnGroupConfigurationSelectorChanged()
    {
      if( this.GroupConfigurationSelectorChanged != null )
      {
        this.GroupConfigurationSelectorChanged( this, EventArgs.Empty );
      }
    }

    internal event EventHandler GroupConfigurationSelectorChanged;

    #endregion GroupConfigurationSelector Property

    #region HasDetails Read-Only Property

    public bool HasDetails
    {
      get
      {
        bool isDataGridCollectionView = ( this.ItemsSourceCollection is DataGridCollectionView );

        if( !isDataGridCollectionView )
          return false;

        foreach( DetailConfiguration detailConfig in this.DetailConfigurations )
        {
          if( detailConfig.Visible )
            return true;
        }

        return false;
      }
    }

    #endregion

    #region Headers Read-Only property

    public ObservableCollection<DataTemplate> Headers
    {
      get
      {
        if( m_detailConfig != null )
        {
          DefaultDetailConfiguration defaultDetailConfig = this.GetDefaultDetailConfigurationForContext();
          if( defaultDetailConfig != null )
          {
            return defaultDetailConfig.Headers;
          }
          else
          {
            return m_detailConfig.Headers;
          }
        }

        Xceed.Wpf.DataGrid.Views.ViewBase view = m_dataGridControl.GetView( false );
        if( view == null )
          return new ObservableCollection<DataTemplate>();

        return view.Headers;
      }
    }

    #endregion

    #region IsCurrent Read-Only Property

    public bool IsCurrent
    {
      get
      {
        return IsCurrentCache;
      }
    }

    internal void SetIsCurrentHelper( bool value )
    {
      IsCurrentCache = value;
    }

    private bool IsCurrentCache
    {
      get
      {
        return m_flags[ ( int )DataGridContextFlags.IsCurrent ];
      }
      set
      {
        m_flags[ ( int )DataGridContextFlags.IsCurrent ] = value;
      }
    }

    #endregion

    #region ItemContainerStyle

    public Style ItemContainerStyle
    {
      get
      {
        if( m_detailConfig != null )
        {
          DefaultDetailConfiguration defaultDetailConfig = this.GetDefaultDetailConfigurationForContext();
          if( defaultDetailConfig != null )
          {
            return defaultDetailConfig.ItemContainerStyle;
          }
          else
          {
            return m_detailConfig.ItemContainerStyle;
          }
        }
        return m_dataGridControl.ItemContainerStyle;
      }
    }

    #endregion

    #region ItemContainerStyleSelector

    public StyleSelector ItemContainerStyleSelector
    {
      get
      {
        if( m_detailConfig != null )
        {
          DefaultDetailConfiguration defaultDetailConfig = this.GetDefaultDetailConfigurationForContext();
          if( defaultDetailConfig != null )
          {
            return defaultDetailConfig.ItemContainerStyleSelector;
          }
          else
          {
            return m_detailConfig.ItemContainerStyleSelector;
          }
        }

        return m_dataGridControl.ItemContainerStyleSelector;
      }
    }

    #endregion

    #region DataGridControl Read-Only Property

    private DataGridControl m_dataGridControl;

    public DataGridControl DataGridControl
    {
      get
      {
        return m_dataGridControl;
      }
    }

    #endregion DataGridControl Read-Only Property

    #region SelectedItems Read-Only Property

    public IList<object> SelectedItems
    {
      get
      {
        return m_selectedItems;
      }
    }

    private SelectionItemCollection m_selectedItems;

    #endregion SelectedItems Read-Only Property

    #region SelectedItemRanges Read-Only Property

    public IList<SelectionRange> SelectedItemRanges
    {
      get
      {
        return m_selectedItemsRanges;
      }
    }

    private SelectionItemRangeCollection m_selectedItemsRanges;

    #endregion SelectedItemRanges Read-Only Property

    #region SelectedCellRanges Read-Only Property

    public IList<SelectionCellRange> SelectedCellRanges
    {
      get
      {
        return m_selectedCellRanges;
      }
    }

    private SelectionCellRangeCollection m_selectedCellRanges;

    #endregion SelectedCellRanges Read-Only Property


    #region SourceDetailConfiguration Read-Only Property

    private DetailConfiguration m_detailConfig;

    internal DetailConfiguration SourceDetailConfiguration
    {
      get
      {
        return m_detailConfig;
      }
    }

    #endregion SourceDetailConfiguration Read-Only Property

    #region VisibleColumns Read-Only Property

    public ReadOnlyObservableCollection<ColumnBase> VisibleColumns
    {
      get
      {
        if( m_detailConfig != null )
          return m_detailConfig.VisibleColumns;

        return m_visibleColumns;
      }
    }

    private ReadOnlyColumnCollection m_visibleColumns; // = null, this variable will store the VisibleColumns only when
    //the DataGridContext is used "desynchronized".

    #endregion

    #region AutoCreateForeignKeyConfigurations Internal Property

    internal bool AutoCreateForeignKeyConfigurations
    {
      get
      {
        if( m_detailConfig != null )
        {
          return m_detailConfig.AutoCreateForeignKeyConfigurations;
        }

        return m_dataGridControl.AutoCreateForeignKeyConfigurations;
      }
    }

    #endregion

    #region MaxSortLevels Property

    public int MaxSortLevels
    {
      get
      {
        if( m_detailConfig != null )
        {
          DefaultDetailConfiguration defaultDetailConfig = this.GetDefaultDetailConfigurationForContext();
          if( defaultDetailConfig != null )
          {
            return defaultDetailConfig.MaxSortLevels;
          }
          else
          {
            return m_detailConfig.MaxSortLevels;
          }
        }

        return m_dataGridControl.MaxSortLevels;
      }
    }

    #endregion MaxSortLevels Property

    #region MaxGroupLevels Property

    public int MaxGroupLevels
    {
      get
      {
        if( m_detailConfig != null )
        {
          DefaultDetailConfiguration defaultDetailConfig = this.GetDefaultDetailConfigurationForContext();
          if( defaultDetailConfig != null )
          {
            return defaultDetailConfig.MaxGroupLevels;
          }
          else
          {
            return m_detailConfig.MaxGroupLevels;
          }
        }

        return m_dataGridControl.MaxGroupLevels;
      }
    }

    #endregion MaxGroupLevels Property

    //--------- INTERNAL PROPERTIES ---------

    #region SelectedItemsStore Read-Only Property

    internal SelectedItemsStorage SelectedItemsStore
    {
      get
      {
        return m_selectedItemsStore;
      }
    }

    private SelectedItemsStorage m_selectedItemsStore;

    #endregion SelectedItemsStore Read-Only Property

    #region SelectedItemsStore Read-Only Property

    internal SelectedCellsStorage SelectedCellsStore
    {
      get
      {
        return m_selectedCellsStore;
      }
    }

    private SelectedCellsStorage m_selectedCellsStore;

    #endregion SelectedItemsStore Read-Only Property

    #region ColumnsByVisiblePosition Read-Only Property

    internal HashedLinkedList<ColumnBase> ColumnsByVisiblePosition
    {
      get
      {
        if( m_detailConfig != null )
          return m_detailConfig.ColumnsByVisiblePosition;

        return m_columnsByVisiblePosition;
      }
    }

    private HashedLinkedList<ColumnBase> m_columnsByVisiblePosition;// = null, this variable will store the ColumnsByVisiblePosition only when
    //the DataGridContext is used "desynchronized".


    #endregion

    #region ColumnStretchingManager Property

    internal ColumnStretchingManager ColumnStretchingManager
    {
      get
      {
        if( m_columnStretchingManager == null )
          m_columnStretchingManager = new ColumnStretchingManager( this );

        return m_columnStretchingManager;
      }
    }

    private ColumnStretchingManager m_columnStretchingManager;

    #endregion

    #region CurrentCell Read-Only Property

    internal Cell CurrentCell
    {
      get
      {
        Row currentRow = this.CurrentRow;

        if( currentRow == null )
          return null;

        return currentRow.Cells[ this.CurrentColumn ];
      }
    }

    #endregion CurrentCell Property

    #region CurrentRow Read-Only Property

    internal Row CurrentRow
    {
      get
      {
        if( this.InternalCurrentItem == null )
          return null;

        return Row.FromContainer( this.GetContainerFromItem( this.InternalCurrentItem ) );
      }
    }

    #endregion CurrentRow Property

    #region CustomItemContainerGenerator Read-Only Property

    private CustomItemContainerGenerator m_generator;

    internal CustomItemContainerGenerator CustomItemContainerGenerator
    {
      get
      {
        return m_generator;
      }
    }

    internal void SetGenerator( CustomItemContainerGenerator generator )
    {
      if( m_generator != null )
        throw new InvalidOperationException( "An attempt was made to reset the generator after it has already been set." );

      if( generator == null )
        throw new ArgumentNullException( "generator" );

      m_generator = generator;
    }

    #endregion

    #region InternalCurrentItem Read-Only Property

    internal object InternalCurrentItem
    {
      get
      {
        return m_internalCurrentItem;
      }
    }

    internal void SetInternalCurrentItemHelper( object value )
    {
      m_internalCurrentItem = value;
    }

    private object m_internalCurrentItem; // = null

    #endregion

    #region InternalCurrentItemNamesTree Read-Only Property

    internal object[] InternalCurrentItemNamesTree
    {
      get
      {
        return m_internalCurrentItemNamesTree;
      }
    }

    private void SetInternalCurrentItemNamesTree( object[] namesTree )
    {
      m_internalCurrentItemNamesTree = namesTree;
    }

    private object[] m_internalCurrentItemNamesTree;

    #endregion

    #region ItemsSourceFieldDescriptors Property

    internal Dictionary<string, ItemsSourceHelper.FieldDescriptor> ItemsSourceFieldDescriptors
    {
      get
      {
        if( m_detailConfig != null )
        {
          Dictionary<string, ItemsSourceHelper.FieldDescriptor> itemsSourceFieldDescriptors =
            m_detailConfig.ItemsSourceFieldDescriptors;

          if( itemsSourceFieldDescriptors == null )
          {
            itemsSourceFieldDescriptors = ItemsSourceHelper.GetFields( m_items, null );
            m_detailConfig.ItemsSourceFieldDescriptors = itemsSourceFieldDescriptors;
          }

          return itemsSourceFieldDescriptors;
        }

        //we are nescessarily in the master context in this case ( m_detailConfig == null ).
        if( m_itemsSourceFieldDescriptors == null )
          m_itemsSourceFieldDescriptors = ItemsSourceHelper.GetFields( m_dataGridControl.ItemsSource, null );

        return m_itemsSourceFieldDescriptors;
      }
      set
      {
        if( m_detailConfig != null )
        {
          m_detailConfig.ItemsSourceFieldDescriptors = value;
          return;
        }

        m_itemsSourceFieldDescriptors = value;
      }
    }

    private Dictionary<string, ItemsSourceHelper.FieldDescriptor> m_itemsSourceFieldDescriptors;

    #endregion

    #region SortDescriptionsSyncContext Read-Only Property

    internal SortDescriptionsSyncContext SortDescriptionsSyncContext
    {
      get
      {
        DataGridCollectionViewBase dataGridCollectionViewBase =
          this.ItemsSourceCollection as DataGridCollectionViewBase;

        if( dataGridCollectionViewBase != null )
          return dataGridCollectionViewBase.DataGridSortDescriptions.SyncContext;

        Debug.Assert( m_sortDescriptionsSyncContext != null );
        return m_sortDescriptionsSyncContext;
      }
    }

    private SortDescriptionsSyncContext m_sortDescriptionsSyncContext; // = null

    #endregion

    #region DefaultDetailConfiguration Property

    internal DefaultDetailConfiguration DefaultDetailConfiguration
    {
      get
      {
        if( m_detailConfig != null )
        {
          return m_detailConfig.DefaultDetailConfiguration;
        }

        return m_dataGridControl.DefaultDetailConfiguration;
      }
    }
    #endregion

    #region ColumnVirtualizationManager Property

    internal ColumnVirtualizationManager ColumnVirtualizationManager
    {
      get
      {
        ColumnVirtualizationManager columnVirtualizationManager = ColumnVirtualizationManager.GetColumnVirtualizationManager( this );

        if( columnVirtualizationManager == null )
        {
          columnVirtualizationManager = this.DataGridControl.GetView().CreateColumnVirtualizationManager( this );
        }

        // We must ensure the manager is up to date before using it
        columnVirtualizationManager.Update();

        return columnVirtualizationManager;
      }
    }

    #endregion

    #region FixedHeaderFooterViewPortSize

    internal Size FixedHeaderFooterViewPortSize
    {
      get
      {
        return m_fixedHeaderFooterViewPortSize;
      }
      set
      {
        if( Size.Equals( m_fixedHeaderFooterViewPortSize, value ) == false )
        {
          m_fixedHeaderFooterViewPortSize = value;
          this.NotifyPropertyChanged( "FixedHeaderFooterViewPortSize" );
        }
      }
    }

    private Size m_fixedHeaderFooterViewPortSize = new Size();

    #endregion

    #region RecyclingManager Read-Only property

    internal RecyclingManager RecyclingManager
    {
      get
      {
        RecyclingManager retval = null;

        //If the DataGridContext belongs to a detail, then use the DetailConfiguration's RecyclingManager
        DetailConfiguration sourceDetailConfiguration = this.SourceDetailConfiguration;
        if( sourceDetailConfiguration != null )
        {
          retval = sourceDetailConfiguration.RecyclingManager;
        }
        else
        {
          //If the DataGridContext is from the Master context, then use and/or create the RecyclingManager
          if( m_recyclingManager == null )
          {
            m_recyclingManager = new RecyclingManager();
          }
          retval = m_recyclingManager;
        }

        //RecyclingManager should never be null
        Debug.Assert( retval != null );
        return retval;
      }
    }

    #endregion

    #region IsDeleteCommandEnabled Property

    internal bool IsDeleteCommandEnabled
    {
      get
      {
        return this.FindIsDeletedCommandEnabledAmbient( this );
      }
    }

    private bool FindIsDeletedCommandEnabledAmbient( DataGridContext dataGridContext )
    {
      if( dataGridContext == null )
        return false;

      if( dataGridContext.SourceDetailConfiguration == null )
      {
        if( dataGridContext.DataGridControl == null )
          return false;

        return dataGridContext.DataGridControl.IsDeleteCommandEnabled;
      }

      DefaultDetailConfiguration defaultDetailConfig = dataGridContext.GetDefaultDetailConfigurationForContext();

      if( defaultDetailConfig != null )
      {
        if( defaultDetailConfig.ReadLocalValue( DefaultDetailConfiguration.IsDeleteCommandEnabledProperty ) != DependencyProperty.UnsetValue )
          return defaultDetailConfig.IsDeleteCommandEnabled;
      }
      else
      {
        if( dataGridContext.SourceDetailConfiguration.ReadLocalValue( DetailConfiguration.IsDeleteCommandEnabledProperty ) != DependencyProperty.UnsetValue )
          return dataGridContext.SourceDetailConfiguration.IsDeleteCommandEnabled;
      }

      return this.FindIsDeletedCommandEnabledAmbient( dataGridContext.ParentDataGridContext );
    }

    #endregion IsDeleteCommandEnabled Property

    #region SelectionChanged Event

    internal void InvokeSelectionChanged( SelectionInfo selectionInfo )
    {

      Debug.Assert( selectionInfo.DataGridContext == this );

      if( ( selectionInfo.RemovedItems.Count == 0 ) && ( selectionInfo.AddedItems.Count == 0 ) )
        return;

      if( ( AutomationPeer.ListenerExists( AutomationEvents.SelectionPatternOnInvalidated )
        || AutomationPeer.ListenerExists( AutomationEvents.SelectionItemPatternOnElementSelected ) )
        || ( AutomationPeer.ListenerExists( AutomationEvents.SelectionItemPatternOnElementAddedToSelection )
          || AutomationPeer.ListenerExists( AutomationEvents.SelectionItemPatternOnElementRemovedFromSelection ) ) )
      {
        DataGridContextAutomationPeer peer = this.Peer;

        if( peer != null )
        {
          peer.RaiseSelectionEvents(
            ( IList<object> )selectionInfo.RemovedItems,
            ( IList<object> )selectionInfo.AddedItems );
        }
      }
    }

    #endregion

    #region DetailLevel Property

    // Represents the detail level from the master item
    internal int DetailLevel
    {
      get;
      private set;
    }

    #endregion

    #region IsSavingState Property

    internal bool IsSavingState
    {
      get
      {
        return m_flags[ ( int )DataGridContextFlags.IsSavingState ];
      }
      set
      {
        m_flags[ ( int )DataGridContextFlags.IsSavingState ] = value;
      }
    }

    #endregion

    #region IsRestoringState Property

    internal bool IsRestoringState
    {
      get
      {
        return m_flags[ ( int )DataGridContextFlags.IsRestoringState ];
      }
      set
      {
        m_flags[ ( int )DataGridContextFlags.IsRestoringState ] = value;
      }
    }

    #endregion

    #region IsDeferRestoringState Internal Property

    internal bool IsDeferRestoringState
    {
      get
      {
        return ( m_deferRestoreStateCount > 0 );
      }
    }

    #endregion

    #region AlreadySearchedForDefaultDetailConfig Private Property

    private bool AlreadySearchedForDefaultDetailConfig
    {
      get
      {
        return m_flags[ ( int )DataGridContextFlags.AlreadySearchedForDetailConfig ];
      }
      set
      {
        m_flags[ ( int )DataGridContextFlags.AlreadySearchedForDetailConfig ] = value;
      }
    }

    #endregion

    #region DefaultImageColumnDetermined Private Property

    private bool DefaultImageColumnDetermined
    {
      get
      {
        return m_flags[ ( int )DataGridContextFlags.DefaultImageColumnDetermined ];
      }
      set
      {
        m_flags[ ( int )DataGridContextFlags.DefaultImageColumnDetermined ] = value;
      }
    }

    #endregion

    #region ItemsProperties/Column Auto update

    private void HookToItemPropertiesChanged()
    {
      DataGridCollectionViewBase itemsSourceCollection = this.ItemsSourceCollection as DataGridCollectionViewBase;

      if( itemsSourceCollection != null )
      {
        ItemPropertiesChangedEventManager.AddListener( itemsSourceCollection.ItemProperties, this );
      }
    }

    private void UnhookToItemPropertiesChanged( DataGridCollectionViewBase itemsSourceCollection )
    {
      if( itemsSourceCollection != null )
      {
        ItemPropertiesChangedEventManager.RemoveListener( itemsSourceCollection.ItemProperties, this );
      }
    }

    #endregion ItemsProperties/Column Auto update

    internal void SetCurrentColumnAndChangeSelection( ColumnBase newCurrentColumn )
    {
      // Since SetCurrentColumnCore can be aborted, we do it before changing the selection.
      this.SetCurrentColumnCore( newCurrentColumn, true, false );

      if( m_dataGridControl != null )
      {
        Row row = this.CurrentRow;
        object item = this.InternalCurrentItem;

        int dataRowIndex;
        bool rowIsBeingEdited;

        if( row == null )
        {
          dataRowIndex = -1;
          rowIsBeingEdited = false;
        }
        else
        {
          dataRowIndex = DataGridVirtualizingPanel.GetItemIndex( row );
          rowIsBeingEdited = row.IsBeingEdited;
        }

        m_dataGridControl.SelectionChangerManager.UpdateSelection(
          this, this.CurrentItem, this.CurrentColumn,
          this, true, rowIsBeingEdited,
          dataRowIndex, item, newCurrentColumn.VisiblePosition,
          SelectionManager.UpdateSelectionSource.Navigation );
      }
    }

    //---------- PUBLIC METHODS ----------

    public void BeginEdit()
    {
      this.BeginEdit( this.InternalCurrentItem );
    }

    public void BeginEdit( object item )
    {
      DataGridControl.BeginEditHelper( this, item );
    }

    public void EndEdit()
    {
      DataGridControl.EndEditHelper( this );
    }

    public void CancelEdit()
    {
      DataGridControl.CancelEditHelper( this );
    }

    internal bool IsContainingItem( object item )
    {
      DataTemplate itemTemplate = item as DataTemplate;

      if( ( this.ParentItem == null ) && ( itemTemplate != null ) )
      {
        if( m_dataGridControl.View.FixedHeaders.Contains( itemTemplate )
          || m_dataGridControl.View.FixedFooters.Contains( itemTemplate ) )
        {
          return true;
        }
      }

      return m_generator.Contains( item );
    }

    public bool IsGroupExpanded( CollectionViewGroup group )
    {
      return this.CustomItemContainerGenerator.IsGroupExpanded( group );
    }

    internal bool IsGroupExpandedCore( CollectionViewGroup group, bool recurseDetails )
    {
      return this.CustomItemContainerGenerator.IsGroupExpandedCore( group, recurseDetails );
    }

    public void ToggleGroupExpansion( CollectionViewGroup group )
    {
      this.CustomItemContainerGenerator.ToggleGroupExpansion( group );
    }

    public void ExpandGroup( CollectionViewGroup group )
    {
      this.CustomItemContainerGenerator.ExpandGroup( group );
    }

    internal bool ExpandGroupCore( CollectionViewGroup group, bool recurseDetails )
    {
      return this.CustomItemContainerGenerator.ExpandGroupCore( group, recurseDetails );
    }

    public void CollapseGroup( CollectionViewGroup group )
    {
      this.CustomItemContainerGenerator.CollapseGroup( group );
    }

    internal bool CollapseGroupCore( CollectionViewGroup group, bool recurseDetails )
    {
      return this.CustomItemContainerGenerator.CollapseGroupCore( group, recurseDetails );
    }

    internal bool AreDetailsExpanded( object dataItem )
    {
      return this.CustomItemContainerGenerator.AreDetailsExpanded( dataItem );
    }

    internal void CollapseDetails( object dataItem )
    {
      this.CustomItemContainerGenerator.CollapseDetails( dataItem );
    }

    internal void ExpandDetails( object dataItem )
    {
      this.CustomItemContainerGenerator.ExpandDetails( dataItem );
    }

    internal void ToggleDetailExpansion( object dataItem )
    {
      this.CustomItemContainerGenerator.ToggleDetails( dataItem );
    }

    public Group GetGroupFromCollectionViewGroup( CollectionViewGroup collectionViewGroup )
    {
      if( collectionViewGroup == null )
        return null;

      Group group = this.CustomItemContainerGenerator.GetGroupFromCollectionViewGroup( collectionViewGroup );

      return group;
    }

    public DependencyObject GetContainerFromItem( object item )
    {
      if( item == null )
        return null;

      DependencyObject container = null;

      if( ( m_detailConfig == null ) && ( item is DataTemplate ) )
      {
        //If the container was not found in the DataGridContext's Generator and the DataGridContext does not have a source DetailConfig ( master context )
        //then check in the DataGridControl's Fixed Items
        container = m_dataGridControl.GetContainerForFixedItem( item );
      }

      if( container == null )
        container = this.CustomItemContainerGenerator.ContainerFromItem( item );

      return container;
    }

    public object GetItemFromContainer( DependencyObject container )
    {
      object item = null;

      if( m_detailConfig == null )
      {
        //If the item was not found in the DataGridContext's Generator and the DataGridContext does not have a source DetailConfig ( master context )
        //then check in the DataGridControl's Fixed Items
        item = DataGridControl.GetFixedItemFromContainer( container );
      }

      if( item == null )
      {
        item = this.CustomItemContainerGenerator.ItemFromContainer( container );
      }

      return item;
    }

    [EditorBrowsable( EditorBrowsableState.Never )]
    public DependencyObject GetContainerFromIndex( int index )
    {
      return this.CustomItemContainerGenerator.GetRealizedContainerForIndex( index );
    }

    [EditorBrowsable( EditorBrowsableState.Never )]
    public int GetIndexFromContainer( DependencyObject container )
    {
      return this.CustomItemContainerGenerator.GetRealizedIndexForContainer( container );
    }

    public CollectionViewGroup GetParentGroupFromItem( object item )
    {
      return this.GetParentGroupFromItemCore( item, false );
    }

    internal CollectionViewGroup GetParentGroupFromItemCore( object item, bool recurseDetails )
    {
      CollectionViewGroup retval = null;

      DataTemplate dataTemplate = item as DataTemplate;

      //For the master level, before interrogating the Generator, I need to check for the items presence in the Fixed regions.
      if( ( m_detailConfig == null ) && ( dataTemplate != null ) )
      {
        Xceed.Wpf.DataGrid.Views.ViewBase view = m_dataGridControl.GetView();

        //Fixed header cannot have a Parent Group.
        if( view.FixedHeaders.Contains( dataTemplate ) == true )
          return null;

        //Fixed header cannot have a Parent Group.
        if( view.FixedFooters.Contains( dataTemplate ) == true )
          return null;

      }

      if( this.CustomItemContainerGenerator.IsInUse == true )
      {
        retval = this.CustomItemContainerGenerator.GetParentGroupFromItem( item, recurseDetails );
      }

      return retval;
    }

    internal DataGridContext GetChildContext( object parentItem, DetailConfiguration detailConfiguration )
    {
      if( detailConfiguration == null )
        throw new ArgumentNullException( "detailConfiguration" );

      return this.GetChildContext( parentItem, detailConfiguration.RelationName );
    }

    public DataGridContext GetChildContext( object parentItem, string relationName )
    {
      return this.CustomItemContainerGenerator.GetChildContext( parentItem, relationName );
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate" )]
    public IEnumerable<DataGridContext> GetChildContexts()
    {
      return this.CustomItemContainerGenerator.GetChildContexts();
    }

    internal BindingPathValueExtractor GetBindingPathExtractorForColumn( Column column, object dataItem )
    {
      if( column == null )
        throw new ArgumentNullException( "column" );

      if( dataItem == null )
        return null;

      if( column == null )
        throw new ArgumentNullException( "column" );

      string xPath = null;
      PropertyPath propertyPath = null;

      // Disable warning for DisplayMemberBinding when internaly used
#pragma warning disable 618

      Binding displayMemberBinding = column.DisplayMemberBinding as Binding;

#pragma warning restore 618

      if( displayMemberBinding == null )
      {
        bool isBoundToDataGridUnboundItemProperty;

        displayMemberBinding = ItemsSourceHelper.AutoCreateDisplayMemberBinding(
          column, this, dataItem, out isBoundToDataGridUnboundItemProperty );
      }

      if( displayMemberBinding != null )
      {
        xPath = displayMemberBinding.XPath;
        propertyPath = displayMemberBinding.Path;
      }
      else
      {
        throw new DataGridInternalException( "DisplayMemberBinding is null." );
      }

      IValueConverter converter = new Xceed.Wpf.DataGrid.Converters.SourceDataConverter( ItemsSourceHelper.IsItemSupportingDBNull( dataItem ), CultureInfo.InvariantCulture );

      BindingPathValueExtractor extractorForRead = new BindingPathValueExtractor( xPath, propertyPath, false, typeof( object ), converter, null, CultureInfo.InvariantCulture );

      return extractorForRead;
    }

    //---------- INTERNAL METHODS -----------

    internal IDisposable DeferRestoreState()
    {
      return new DeferRestoreStateDisposable( this );
    }

    internal void SetAssociatedAutomationPeer()
    {
      DataGridContextAutomationPeer parentAutomationPeer = m_parentDataGridContext.Peer;

      if( parentAutomationPeer == null )
        return;

      DataGridContextAutomationPeer automationPeer = parentAutomationPeer.GetDetailPeer( m_parentItem, m_detailConfig );

      if( automationPeer != null )
      {
        this.Peer = automationPeer;
        automationPeer.DataGridContext = this;
      }
    }

    internal void UpdatePublicSelectionProperties()
    {
      if( ( m_selectedItemsStore.Count > 0 ) || ( m_selectedCellsStore.Count > 0 ) )
      {
        m_dataGridControl.AddToSelectedContexts( this );
      }
      else
      {
        m_dataGridControl.RemoveFromSelectedContexts( this );
      }


      if( m_parentItem == null )
      {
        // We are the root context ( GridControl )
        int newSelectedItemIndex;
        object newSelectedItem;
        int newSelectedColumnIndex;
        this.GetFirstSelectedItemFromStore( false, out newSelectedItemIndex, out newSelectedItem, out newSelectedColumnIndex );

        if( m_dataGridControl.SelectedIndex != newSelectedItemIndex )
        {
          m_dataGridControl.SelectedIndex = newSelectedItemIndex;
        }

        // Calculate the new SelectedItem
        if( !object.Equals( m_dataGridControl.SelectedItem, newSelectedItem ) )
        {
          m_dataGridControl.SetSkipCoerceSelectedItemCheck( true );

          try
          {
            m_dataGridControl.SelectedItem = newSelectedItem;
          }
          finally
          {
            m_dataGridControl.SetSkipCoerceSelectedItemCheck( false );
          }
        }
      }
    }

    internal void GetFirstSelectedItemFromStore( bool checkCellsStore, out int selectedItemIndex, out object selectedItem, out int selectedColumnIndex )
    {
      SelectionRangeWithItems newSelectedRangeWithItems = ( m_selectedItemsStore.Count == 0 ) ?
        SelectionRangeWithItems.Empty : m_selectedItemsStore[ 0 ];

      if( newSelectedRangeWithItems.IsEmpty )
      {
        if( checkCellsStore )
        {
          SelectionCellRangeWithItems newSelectionCellRangeWithItems = ( m_selectedCellsStore.Count == 0 ) ?
            SelectionCellRangeWithItems.Empty : m_selectedCellsStore[ 0 ];

          if( newSelectionCellRangeWithItems.ItemRange.IsEmpty )
          {
            selectedColumnIndex = -1;
            selectedItemIndex = -1;
            selectedItem = null;
          }
          else
          {
            selectedColumnIndex = newSelectionCellRangeWithItems.ColumnRange.StartIndex;
            selectedItemIndex = newSelectionCellRangeWithItems.ItemRange.StartIndex;
            selectedItem = newSelectionCellRangeWithItems.ItemRangeWithItems.GetItem( this, 0 );
          }
        }
        else
        {
          selectedColumnIndex = -1;
          selectedItemIndex = -1;
          selectedItem = null;
        }
      }
      else
      {
        selectedColumnIndex = -1;
        selectedItemIndex = newSelectedRangeWithItems.Range.StartIndex;
        selectedItem = newSelectedRangeWithItems.GetItem( this, 0 );
      }
    }

    internal ColumnBase GetDefaultImageColumn()
    {
      if( !this.DefaultImageColumnDetermined )
      {
        if( this.Items.Count > 0 )
        {
          this.DefaultImageColumnDetermined = true;
          ItemsSourceHelper.FieldDescriptor fieldDescriptor;
          object dataItem = this.Items.GetItemAt( 0 );

          foreach( ColumnBase visibleColumn in this.VisibleColumns )
          {
            if( this.ItemsSourceFieldDescriptors.TryGetValue( visibleColumn.FieldName, out fieldDescriptor ) )
            {
              if( typeof( ImageSource ).IsAssignableFrom( fieldDescriptor.DataType ) )
              {
                m_defaultImageColumn = visibleColumn;
                break;
              }
              else if( ( typeof( byte[] ).IsAssignableFrom( fieldDescriptor.DataType ) ) ||
                       ( typeof( System.Drawing.Image ).IsAssignableFrom( fieldDescriptor.DataType ) ) )
              {
                Xceed.Wpf.DataGrid.Converters.ImageConverter converter = new Xceed.Wpf.DataGrid.Converters.ImageConverter();
                object convertedValue = null;
                object rawValue = null;

                try
                {
                  if( fieldDescriptor.PropertyDescriptor == null )
                  {
                    System.Data.DataRow dataRow = dataItem as System.Data.DataRow;

                    if( dataRow == null )
                    {
                      System.Data.DataRowView dataRowView = dataItem as System.Data.DataRowView;

                      if( dataRowView != null )
                        rawValue = dataRowView[ fieldDescriptor.Name ];
                    }
                    else
                    {
                      rawValue = dataRow[ fieldDescriptor.Name ];
                    }
                  }
                  else
                  {
                    rawValue = fieldDescriptor.PropertyDescriptor.GetValue( dataItem );
                  }

                  if( rawValue != null )
                    convertedValue = converter.Convert( rawValue, typeof( ImageSource ), null, System.Globalization.CultureInfo.CurrentCulture );
                }
                catch( NotSupportedException )
                {
                  //suppress the exception, the byte[] is not an image. convertedValue will remain null
                }

                if( convertedValue != null )
                {
                  m_defaultImageColumn = visibleColumn;
                  break;
                }
              }
            }
          }
        }
      }

      return m_defaultImageColumn;
    }

    internal void EnsureResort()
    {
      if( ( !ItemsSourceHelper.IsDataView( this.Items ) ) && ( this.Items.SortDescriptions.Count > 0 ) )
      {
        SortDescriptionCollection sortDescriptions = this.Items.SortDescriptions;
        SortDescription[] sortDescriptionsCopy = new SortDescription[ sortDescriptions.Count ];

        sortDescriptions.CopyTo( sortDescriptionsCopy, 0 );
        sortDescriptions.Clear();

        foreach( SortDescription sortDescription in sortDescriptionsCopy )
        {
          sortDescriptions.Add( sortDescription );
        }
      }
    }

    internal void EnsureRegroup()
    {

      if( this.Items.GroupDescriptions.Count > 0 )
      {
        ObservableCollection<GroupDescription> groupDescriptions = this.Items.GroupDescriptions;
        GroupDescription[] groupDescriptionsCopy = new GroupDescription[ groupDescriptions.Count ];

        groupDescriptions.CopyTo( groupDescriptionsCopy, 0 );
        groupDescriptions.Clear();

        foreach( GroupDescription groupDescription in groupDescriptionsCopy )
        {
          groupDescriptions.Add( groupDescription );
        }
      }
    }

    internal static string GetColumnNameFromGroupDescription( GroupDescription groupDescription )
    {
      PropertyGroupDescription propertyGroupDescription =
        groupDescription as PropertyGroupDescription;

      if( propertyGroupDescription != null )
        return propertyGroupDescription.PropertyName;

      DataGridGroupDescription dataGridGroupDescription =
        groupDescription as DataGridGroupDescription;

      if( dataGridGroupDescription != null )
        return dataGridGroupDescription.PropertyName;

      throw new NotSupportedException( "Group descriptions other than PropertyGroupDescription or DataGridGroupDescription are not supported." );
    }

    internal static ObservableCollection<GroupDescription> GetGroupDescriptionsHelper( CollectionView collectionView )
    {
      ObservableCollection<GroupDescription> retval = collectionView.GroupDescriptions;

      ItemCollection itemCollection = collectionView as ItemCollection;

      if( itemCollection != null )
      {
        CollectionView sourceCollectionView = itemCollection.SourceCollection as CollectionView;

        if( sourceCollectionView != null )
        {
          if( sourceCollectionView.GroupDescriptions != null )
            retval = sourceCollectionView.GroupDescriptions;
        }
        else
        {
          sourceCollectionView = CollectionViewSource.GetDefaultView( itemCollection.SourceCollection ) as CollectionView;

          if( ( sourceCollectionView != null ) && ( sourceCollectionView.GroupDescriptions != null ) )
            retval = sourceCollectionView.GroupDescriptions;
        }
      }

      return retval;
    }

    internal Group GetGroupFromItem( object dataItem )
    {
      return this.CustomItemContainerGenerator.GetGroupFromItem( dataItem );
    }

    internal static void HandleColumnsCollectionChanged( NotifyCollectionChangedEventArgs e,
      ColumnCollection columns,
      ReadOnlyColumnCollection visibleColumns,
      HashedLinkedList<ColumnBase> columnsByVisiblePosition,
      SortDescriptionsSyncContext sortDescriptionsSyncContext,
      SortDescriptionCollection sortDescriptions )
    {
      if( columns == null )
        throw new ArgumentNullException( "columns" );

      if( visibleColumns == null )
        throw new ArgumentNullException( "visibleColumns" );

      if( columnsByVisiblePosition == null )
        throw new ArgumentNullException( "columnsByVisiblePosition" );

      if( sortDescriptionsSyncContext == null )
        throw new ArgumentNullException( "sortDescriptionsSyncContext" );


      bool needGeneratorHardReset = false;

      if( e.OldItems != null )
      {
        if( ( e.Action == NotifyCollectionChangedAction.Remove )
          || ( e.Action == NotifyCollectionChangedAction.Replace )
          || ( e.Action == NotifyCollectionChangedAction.Reset ) )
        {
          if( e.OldItems.Count > 0 )
            needGeneratorHardReset = true;
        }
      }

      if( sortDescriptions != null )
      {
        DataGridContext.SynchronizeSortProperties( sortDescriptionsSyncContext, sortDescriptions, columns );
      }

      DataGridContext.UpdateVisibleColumns( columns, visibleColumns, columnsByVisiblePosition, e.NewItems, null );

      // When a column is removed, we need to clear the Generator from every container to avoid problem
      // when a Column with the same field name is removed thant reinserted with 2 different instances
      if( needGeneratorHardReset )
      {
        DataGridControl dataGridControl = columns.DataGridControl;
        if( dataGridControl != null )
        {
          if( dataGridControl.CustomItemContainerGenerator != null )
            dataGridControl.CustomItemContainerGenerator.RemoveAllAndNotify();

          // We only reset the fixed region if the column is part of the master context 
          if( ( dataGridControl.DataGridContext != null ) && ( dataGridControl.DataGridContext.SourceDetailConfiguration == null ) )
            dataGridControl.ResetFixedRegions();
        }
      }
    }

    internal IDisposable InhibitQueueBringIntoView()
    {
      return m_dataGridControl.InhibitQueueBringIntoView();
    }

    internal IDisposable InhibitSetFocus()
    {
      return m_dataGridControl.InhibitSetFocus();
    }

    internal void NotifyItemsSourceChanged()
    {
      this.NotifyPropertyChanged( "HasDetails" );
    }

    internal void DelayBringIntoViewAndFocusCurrent()
    {
      m_dataGridControl.DelayBringIntoViewAndFocusCurrent();
    }

    internal void ResetViewProperties()
    {
      //Used only for the Print DataGridControl
      this.SetupViewProperties();
      this.InitializeViewProperties();
    }

    internal void SetCurrent(
      object item,
      Row containerRow,
      Nullable<int> sourceDataItemIndex,
      ColumnBase column,
      bool forceFocus,
      bool isCancelable,
      bool synchronizeSelectionWithCurrent )
    {
      // sourceDataItemIndex 
      //  null = item have to be checked if it is a dataItem
      //  -1   = the item it's not a dataItem
      //  ?    = then index of the item in the source collection.

      //verify that SetCurrent is not called successively caused by actions performed within, this is to prevent
      //exceptions caused by the accessing of resources that could be "locked" (e.g., ItemsControl.Items.DeferRefresh)
      if( m_dataGridControl.IsSetCurrentInProgress )
        throw new DataGridException( "SetCurrent cannot be invoked while another SetCurrent is in progress." );

      try
      {
        //set the flag that indicates that we are already processing a SetCurrent
        m_dataGridControl.IsSetCurrentInProgress = true;

        DataGridContext oldCurrentContext = m_dataGridControl.CurrentContext;
        DataGridContext newCurrentContext = this;

        //store the previous public current item, so I can detect a change and lift the PropertyChanged for this properties
        object oldCurrentItem = oldCurrentContext.InternalCurrentItem;
        object oldPublicCurrentItem = oldCurrentContext.CurrentItem;
        ColumnBase oldCurrentColumn = oldCurrentContext.CurrentColumn;
        Row oldCurrentRow = oldCurrentContext.CurrentRow;

        //if item is not realized or if the item passed is not a Data Item (header, footer or group), then the 
        //old current cell will be null
        Cell oldCurrentCell = ( oldCurrentRow == null )
          ? null : oldCurrentRow.Cells[ oldCurrentColumn ];

        Row newCurrentRow = ( containerRow != null )
          ? containerRow : Row.FromContainer( this.GetContainerFromItem( item ) );

        //verify that set Current is not called on a column that cannot be current
        if( newCurrentRow != null )
        {
          if( ( column != null )
            && ( ( ( column.ReadOnly )
            && ( !newCurrentRow.Cells[ column ].GetCalculatedCanBeCurrent() ) ) ) )
            throw new DataGridException( "SetCurrent cannot be invoked if the column cannot be current." );
        }

        //if item is not realized or if the item passed is not a Data Item (header, footer or group), then the 
        //new current cell will be null
        Cell newCurrentCell = ( newCurrentRow == null ) ? null : newCurrentRow.Cells[ column ];
        bool currentCellChanged = ( newCurrentCell != oldCurrentCell );

        if( ( ( item != oldCurrentItem ) || ( oldCurrentContext != newCurrentContext ) )
          && ( oldCurrentRow != null ) )
        {
          if( Row.IsCellEditorDisplayConditionsSet( oldCurrentRow, CellEditorDisplayConditions.RowIsCurrent ) )
            oldCurrentRow.RemoveDisplayEditorMatchingCondition( CellEditorDisplayConditions.RowIsCurrent );

          oldCurrentRow.SetIsCurrent( false );

          try
          {
            oldCurrentRow.EndEdit();
          }
          //an error occurred in the end edit
          catch( DataGridException )
          {
            //restore the Cell and the Row's IsCurrent Flag
            if( oldCurrentCell != null )
              oldCurrentCell.SetIsCurrent( true );

            oldCurrentRow.SetIsCurrent( true );
            throw;
          }
        }

        // For this if, the assumption is that current item hasn't changed, but current column does... (previous if will have 
        // caught the case where current item changed ).

        //if the currentCell has changed and the old current cell still exists.
        if( ( currentCellChanged ) && ( oldCurrentCell != null ) )
        {
          //clear the CellIsCurrent editor display conditions
          if( Cell.IsCellEditorDisplayConditionsSet( oldCurrentCell, CellEditorDisplayConditions.CellIsCurrent ) )
            oldCurrentCell.RemoveDisplayEditorMatchingCondition( CellEditorDisplayConditions.CellIsCurrent );

          oldCurrentCell.SetIsCurrent( false );

          try
          {
            //cancel any ongoing editing on the cell. (this has no effect if row or cell is not being edited.
            //this line can throw but no action is to be done.
            oldCurrentCell.EndEdit();
          }
          catch( DataGridException )
          {
            oldCurrentCell.SetIsCurrent( true );
            throw;
          }
        }

        if( ( item != oldCurrentItem )
          || ( oldCurrentContext != newCurrentContext ) )
        {
          DataGridCurrentChangingEventArgs currentChangingEventArgs = new DataGridCurrentChangingEventArgs( oldCurrentContext, oldCurrentItem, newCurrentContext, item, isCancelable );
          m_dataGridControl.RaiseCurrentChanging( currentChangingEventArgs );

          if( isCancelable && currentChangingEventArgs.Cancel )
          {
            // We restore the currentness on the previous row and cell.
            if( oldCurrentRow != null )
            {
              if( oldCurrentCell != null )
                oldCurrentCell.SetIsCurrent( true );

              oldCurrentRow.SetIsCurrent( true );
            }

            throw new DataGridException( "The operation has been canceled." );
          }
        }

        //If they are different, clean the previously current DataGridContext
        if( oldCurrentContext != newCurrentContext )
        {
          oldCurrentContext.SetCurrentItemHelper( null, -1 );
          oldCurrentContext.SetInternalCurrentItemHelper( null );
          oldCurrentContext.SetInternalCurrentItemNamesTree( null );
          //preserve current column on the old DataGridContext

          oldCurrentContext.SetIsCurrentHelper( false );
        }

        if( !sourceDataItemIndex.HasValue )
        {
          sourceDataItemIndex = this.Items.IndexOf( item );
        }

        // All the stuff that can throw is done
        newCurrentContext.SetInternalCurrentItemHelper( item );
        newCurrentContext.SetIsCurrentHelper( true );

        if( ( item != null ) && ( item.GetType() == typeof( GroupHeaderFooterItem ) ) )
        {
          GroupHeaderFooterItem groupHeaderItem = ( GroupHeaderFooterItem )item;
          newCurrentContext.SetInternalCurrentItemNamesTree( newCurrentContext.CustomItemContainerGenerator.GetNamesTreeFromGroup( groupHeaderItem.Group ) );
        }
        else
        {
          newCurrentContext.SetInternalCurrentItemNamesTree( null );
        }

        // change the CurrentRow and CurentColumn
        if( ( item != null ) && ( sourceDataItemIndex.Value != -1 ) )
        {
          newCurrentContext.SetCurrentItemHelper( item, sourceDataItemIndex.Value );
        }
        else
        {
          newCurrentContext.SetCurrentItemHelper( null, -1 );
        }

        newCurrentContext.SetCurrentColumnHelper( column );
        m_dataGridControl.SetCurrentDataGridContextHelper( this );

        // We must refetch the container for the current item in case EndEdit triggers a reset on the 
        // CustomItemContainerGenerator and remap this item to a container other than the one previously
        // fetched
        newCurrentRow = Row.FromContainer( newCurrentContext.GetContainerFromItem( item ) );
        newCurrentCell = ( newCurrentRow == null ) ? null : newCurrentRow.Cells[ column ];

        bool currentRowChanged = ( newCurrentRow != oldCurrentRow );

        //If there is a container for the new Row.
        if( newCurrentRow != null )
        {
          Debug.Assert( newCurrentRow.IsContainerPrepared, "The container must be prepared to be set as current and call BeginEdit." );

          //update the RowIsCurrent display condition
          if( Row.IsCellEditorDisplayConditionsSet( newCurrentRow, CellEditorDisplayConditions.RowIsCurrent ) )
            newCurrentRow.SetDisplayEditorMatchingCondition( CellEditorDisplayConditions.RowIsCurrent );

          //Update the IsCurrent flag
          newCurrentRow.SetIsCurrent( true );

          //if the current row changed, make sure to check the appropriate edition triggers.
          if( currentRowChanged == true )
          {
            //if enterEditTriggers tells to enter edition mode AND no current cell is going to be set on the new current row
            if( ( newCurrentRow.IsEditTriggerSet( EditTriggers.RowIsCurrent ) )
              && ( newCurrentCell == null ) )
            {
              //To prevent re-entrancy of the SetCurrent fonction (since Row.BeginEdit will call Cell.BeginEdit which will call SetCurrent if not current already)
              if( this.VisibleColumns.Count > 0 )
              {
                int firstFocusableColumn = DataGridScrollViewer.GetFirstVisibleFocusableColumnIndex( this, newCurrentRow );

                if( firstFocusableColumn < 0 )
                  throw new DataGridException( "Trying to edit while no cell is focusable. " );

                this.SetCurrentColumnHelper( this.VisibleColumns[ firstFocusableColumn ] );
                newCurrentCell = newCurrentRow.Cells[ m_currentColumn ];
                currentCellChanged = true;
              }
            }
          }
        }

        //If current cell exists.
        if( newCurrentCell != null )
        {
          //update the CellIsCurrent display condition
          if( Cell.IsCellEditorDisplayConditionsSet( newCurrentCell, CellEditorDisplayConditions.CellIsCurrent ) )
            newCurrentCell.SetDisplayEditorMatchingCondition( CellEditorDisplayConditions.CellIsCurrent );

          newCurrentCell.SetIsCurrent( true );

          //Then, if the Current Cell has changed or not currently in edition, process the edition conditions.
          if( currentCellChanged || !newCurrentCell.IsBeingEdited )
          {
            //if the EditTriggers dictate that we should enter edition, do it.
            if( ( newCurrentRow.IsEditTriggerSet( EditTriggers.CellIsCurrent ) )
               || ( ( newCurrentRow.IsEditTriggerSet( EditTriggers.RowIsCurrent ) ) && ( currentRowChanged ) ) )
            {
              try
              {
                newCurrentCell.BeginEdit();
              }
              catch( DataGridException )
              {
                // We swallow the exception if it occurs because the Cell was read-only.

                // Try to begin edit only on the row
                try
                {
                  newCurrentRow.BeginEdit();
                }
                catch( DataGridException )
                {
                  // We swallow the exception if it occurs because the Row was read-only.
                }
              }
            }
          }
        }

        //if there is a new CurrentItem 
        if( item != null )
        {
          //if the container from the new Current item is realized
          if( newCurrentRow != null )
          {
            // Call BringIntoView on the cell to ensure it is put in VisualTree if virtualized
            if( newCurrentCell != null )
            {
              // Ensure the Cell is into view (this will force a bring into view of the newCurrentRow as a side effect)
              newCurrentCell.BringIntoView();
            }
            else
            {
              // Ensure the item is visible visible
              newCurrentRow.BringIntoView();
            }

            using( m_dataGridControl.SelectionChangerManager.PushUpdateSelectionSource( SelectionManager.UpdateSelectionSource.None ) )
            {
              // Call this to update the Focus to the correct location
              m_dataGridControl.SetFocusHelper( newCurrentRow, column, forceFocus, true );
            }
          }
          else
          {
            m_dataGridControl.DelayBringIntoViewAndFocusCurrent();
          }
        }

        //Send notification at the end (as much as possible) to ensure that no code bound on the property changed handler will interrupt this sequence 

        if( ( synchronizeSelectionWithCurrent ) && ( !m_dataGridControl.SelectionChangerManager.IsActive ) )
        {
          m_dataGridControl.SelectionChangerManager.Begin();

          try
          {
            if( m_dataGridControl.SelectionUnit == SelectionUnit.Row )
            {
              if( newCurrentContext.CurrentItemIndex == -1 )
              {
                m_dataGridControl.SelectionChangerManager.UnselectAll();
              }
              else
              {
                m_dataGridControl.SelectionChangerManager.SelectJustThisItem( this, newCurrentContext.CurrentItemIndex, newCurrentContext.CurrentItem );
              }
            }
            else
            {
              int currentColumnVisibleIndex = ( newCurrentContext.CurrentColumn == null ) ? -1 : newCurrentContext.CurrentColumn.VisiblePosition;

              if( ( currentColumnVisibleIndex == -1 ) || ( newCurrentContext.CurrentItemIndex == -1 ) )
              {
                m_dataGridControl.SelectionChangerManager.UnselectAll();
              }
              else
              {
                m_dataGridControl.SelectionChangerManager.SelectJustThisCell( this, newCurrentContext.CurrentItemIndex, newCurrentContext.CurrentItem, currentColumnVisibleIndex );
              }
            }
          }
          finally
          {
            m_dataGridControl.SelectionChangerManager.End( false, false, false );
          }
        }

        if( this.CurrentItem != oldPublicCurrentItem )
          this.NotifyPropertyChanged( "CurrentItem" );

        if( column != oldCurrentColumn )
          this.NotifyPropertyChanged( "CurrentColumn" );

        if( ( item != oldCurrentItem )
          || ( oldCurrentContext != newCurrentContext ) )
        {
          DataGridCurrentChangedEventArgs currentChangedEventArgs = new DataGridCurrentChangedEventArgs( oldCurrentContext, oldCurrentItem, newCurrentContext, item );
          m_dataGridControl.RaiseCurrentChanged( currentChangedEventArgs );
        }
      }
      finally
      {
        m_dataGridControl.IsSetCurrentInProgress = false;
      }
    }

    internal void SetupTitleBindingForGroupLevelDescriptions()
    {
      foreach( GroupLevelDescription gld in this.GroupLevelDescriptions )
      {
        Binding titleBinding = BindingOperations.GetBinding( gld, GroupLevelDescription.TitleProperty );

        if( titleBinding == null )
        {
          ColumnBase column = this.Columns[ gld.FieldName ];

          if( column != null )
          {
            DataGridContext.SetupGroupLevelDescriptionBindings( gld, column );
          }
        }
      }
    }

    internal static void SynchronizeSortProperties( SortDescriptionsSyncContext syncContext, SortDescriptionCollection sortDescriptions, ColumnCollection columns )
    {
      if( syncContext.ProcessingSortSynchronization == true )
        return;

      //if( syncContext.IsInitializing == true )
      //{
      //  syncContext.SynchronizeSortDelayed = true;
      //  return;
      //}

      try
      {
        syncContext.ProcessingSortSynchronization = true;

        SortDescription sortDescription;
        Collection<ColumnBase> handledColumns = new Collection<ColumnBase>();

        for( int i = sortDescriptions.Count - 1; i >= 0; i-- )
        {
          sortDescription = sortDescriptions[ i ];

          foreach( ColumnBase column in columns )
          {
            string fieldName = column.FieldName;

            if( fieldName == sortDescription.PropertyName )
            {
              column.SetSortIndex( i );

              switch( sortDescription.Direction )
              {
                case ListSortDirection.Ascending:
                  column.SetSortDirection( SortDirection.Ascending );
                  break;

                case ListSortDirection.Descending:
                  column.SetSortDirection( SortDirection.Descending );
                  break;
              }

              handledColumns.Add( column );
              break;
            }
          }
        }

        foreach( ColumnBase column in columns )
        {
          if( !handledColumns.Contains( column ) )
          {
            column.SetSortIndex( -1 );
            column.SetSortDirection( SortDirection.None );
          }
        }
      }
      finally
      {
        syncContext.ProcessingSortSynchronization = false;
      }
    }

    internal static void UpdateGroupLevelDescriptions( GroupLevelDescriptionCollection groupLevelDescriptions,
      NotifyCollectionChangedEventArgs e, ObservableCollection<GroupDescription> groupDescriptions, ColumnCollection columns )
    {
      switch( e.Action )
      {
        case NotifyCollectionChangedAction.Add:
          //add all the items added into the GroupLevelDescriptions collection
          for( int i = 0; i < e.NewItems.Count; i++ )
          {
            GroupLevelDescription newInfo = DataGridContext.CreateGroupLevelDescription( columns, ( GroupDescription )e.NewItems[ i ] );

            groupLevelDescriptions.Insert( i + e.NewStartingIndex, newInfo );
          }
          break;

        case NotifyCollectionChangedAction.Remove:
          foreach( GroupDescription desc in e.OldItems )
          {
            GroupLevelDescription oldInfo = DataGridContext.GetGroupLevelDescription( groupLevelDescriptions, desc );

            if( oldInfo != null )
            {
              groupLevelDescriptions.Remove( oldInfo );
            }
          }
          break;

        case NotifyCollectionChangedAction.Reset:

          groupLevelDescriptions.Clear();

          foreach( GroupDescription desc in groupDescriptions )
          {
            GroupLevelDescription newInfo = DataGridContext.CreateGroupLevelDescription( columns, desc );

            groupLevelDescriptions.Add( newInfo );
          }

          break;

        case NotifyCollectionChangedAction.Move:

          GroupLevelDescription movedGroupLevelDescription = groupLevelDescriptions[ e.OldStartingIndex ];
          groupLevelDescriptions.RemoveAt( e.OldStartingIndex );
          groupLevelDescriptions.Insert( e.NewStartingIndex, movedGroupLevelDescription );

          break;

        case NotifyCollectionChangedAction.Replace:

          groupLevelDescriptions.RemoveAt( e.OldStartingIndex );
          GroupLevelDescription replaceInfo = DataGridContext.CreateGroupLevelDescription( columns, ( GroupDescription )e.NewItems[ 0 ] );

          groupLevelDescriptions.Insert( e.NewStartingIndex, replaceInfo );

          break;
      }
    }

    internal static void UpdateVisibleColumns(
      ColumnCollection columns,
      ReadOnlyColumnCollection visibleColumns,
      HashedLinkedList<ColumnBase> columnsByVisiblePosition,
      IList triggeringColumns,
      ColumnVisiblePositionChangedEventArgs visiblePositionChangedArgs )
    {
      if( columns == null )
        throw new ArgumentNullException( "columns" );

      if( visibleColumns == null )
        throw new ArgumentNullException( "visibleColumns" );

      if( columnsByVisiblePosition == null )
        throw new ArgumentNullException( "columnsByVisiblePosition" );

      if( columns.ProcessingVisibleColumnsUpdate == true )
        return;

      columns.ProcessingVisibleColumnsUpdate = true;

      try
      {
        columns.NotifyVisibleColumnsUpdating();

        columnsByVisiblePosition.Clear();

        int columnCount = columns.Count;
        ColumnBase currentColumn;

        // Add the Columns which are not part of the triggering columns collection.
        for( int currentIndex = 0; currentIndex < columnCount; currentIndex++ )
        {
          currentColumn = columns[ currentIndex ];

          if( ( triggeringColumns == null ) || ( !triggeringColumns.Contains( currentColumn ) ) )
            DataGridContext.InsertColumnByVisiblePosition( columnsByVisiblePosition, currentColumn );
        }

        // Add the Columns which are part of the triggering columns collection
        // (their VisiblePosition have priority over the existing ones).
        if( triggeringColumns != null )
        {
          if( visiblePositionChangedArgs == null )
          {
            // From a ColumnCollection change
            foreach( ColumnBase column in triggeringColumns )
            {
              DataGridContext.InsertColumnByVisiblePosition( columnsByVisiblePosition, column );
            }
          }
          else
          {
            // From a Column.VisiblePosition change
            ColumnBase column = visiblePositionChangedArgs.TriggeringColumn;

            LinkedListNode<ColumnBase> oldPositionNode = null;
            LinkedListNode<ColumnBase> listNode = columnsByVisiblePosition.First;

            while( listNode != null )
            {
              if( listNode.Value.VisiblePosition == column.VisiblePosition )
              {
                oldPositionNode = listNode;
                break;
              }
              listNode = listNode.Next;
            }

            // If there were already some columns, the desired. If oldPositionNode is null, it means 
            // there were no column  at the desired VisiblePosition. We must add it at the end of the 
            // list since the new column VisiblePosition is greater than the actual columns VisiblePosition
            if( oldPositionNode != null )
            {
              if( visiblePositionChangedArgs.PositionDelta > 0 )
              {
                columnsByVisiblePosition.AddAfter( oldPositionNode, column );
              }
              else if( visiblePositionChangedArgs.PositionDelta < 0 )
              {
                columnsByVisiblePosition.AddBefore( oldPositionNode, column );
              }
            }
            else
            {
              columnsByVisiblePosition.AddLast( column );
            }
          }
        }

        // Fill the VisibleColumns collection and 
        // update all the columns' VisiblePosition, IsFirstVisible, and IsLastVisible properties.
        visibleColumns.InternalClear();

        int i = 0;
        LinkedListNode<ColumnBase> columnNode = columnsByVisiblePosition.First;

        // While is used to be sure Columns are returned in ascendant order
        while( columnNode != null )
        {
          currentColumn = columnNode.Value;
          currentColumn.VisiblePosition = i++;
          currentColumn.ClearIsFirstVisible();
          currentColumn.ClearIsLastVisible();

          if( currentColumn.Visible )
            visibleColumns.InternalAdd( currentColumn );

          columnNode = columnNode.Next;
        }

        if( visibleColumns.Count > 0 )
        {
          visibleColumns[ 0 ].SetIsFirstVisible( true );
          visibleColumns[ visibleColumns.Count - 1 ].SetIsLastVisible( true );
        }

        columns.NotifyVisibleColumnsUpdated();
      }
      finally
      {
        columns.ProcessingVisibleColumnsUpdate = false;
      }
    }

    internal static DataGridContext SafeGetDataGridContextForDataGridCollectionView(
      DataGridContext rootDataGridContext,
      DataGridCollectionViewBase targetDataGridCollectionViewBase )
    {
      DataGridContext matchingDataGridContext = null;

      DataGridCollectionViewBase rootDataGridCollectionViewBase =
        rootDataGridContext.ItemsSourceCollection as DataGridCollectionViewBase;

      if( rootDataGridCollectionViewBase != null )
      {
        if( rootDataGridCollectionViewBase == targetDataGridCollectionViewBase )
        {
          matchingDataGridContext = rootDataGridContext;
        }
        else
        {
          matchingDataGridContext = DataGridContext.SafeRecursiveGetDataGridContextForDataGridCollectionViewBase(
            rootDataGridContext.GetChildContexts(),
            targetDataGridCollectionViewBase );
        }
      }

      return matchingDataGridContext;
    }

    private static DataGridContext SafeRecursiveGetDataGridContextForDataGridCollectionViewBase(
      IEnumerable<DataGridContext> childDataGridContexts,
      DataGridCollectionViewBase targetDataGridCollectionViewBase )
    {
      foreach( DataGridContext childDataGridContext in childDataGridContexts )
      {
        // Child DataGridContext's Items cannot be anything else than a DataGridCollectionView.
        DataGridCollectionViewBase dataGridCollectionViewBase = ( DataGridCollectionViewBase )childDataGridContext.Items;

        if( dataGridCollectionViewBase == targetDataGridCollectionViewBase )
          return childDataGridContext;

        // Deep down.
        DataGridContext matchingDataGridContext = DataGridContext.SafeRecursiveGetDataGridContextForDataGridCollectionViewBase(
          childDataGridContext.GetChildContexts(),
          targetDataGridCollectionViewBase );

        if( matchingDataGridContext != null )
          return matchingDataGridContext;
      }

      return null;
    }

    internal void SaveContainerSizeState( object item, FrameworkElement container )
    {
      if( item == null )
        return;

      ContainerSizeState containerSizeState = new ContainerSizeState( container );

      if( containerSizeState.IsEmpty() == false )
      {
        m_sizeStateDictionary[ item ] = containerSizeState;

        DataGridContext.CleanContainerSizeState( container, containerSizeState );
      }
      else
      {
        m_sizeStateDictionary.Remove( item );
      }
    }

    internal void RestoreContainerSizeState( object item, FrameworkElement container )
    {
      if( item == null )
        return;

      ContainerSizeState containerSizeState;
      if( m_sizeStateDictionary.TryGetValue( item, out containerSizeState ) == true )
      {
        DataGridContext.RestoreContainerSizeWorker( container, containerSizeState );
      }
    }

    internal void ClearSizeStates()
    {
      m_sizeStateDictionary.Clear();
    }

    /// <summary>
    /// Gets the current View property value for this DataGridContext. It can either come
    /// from a local (attached) value or the value on the view (if the property is "routed").
    /// </summary>
    /// <typeparam name="T">The return value type of the requested property.</typeparam>
    /// <param name="propertyName">The property name on the View.</param>
    /// <param name="returnValue">The value of the property if found. Will be ignored if the property is not found.</param>
    /// <returns>Returns true if the returnValue has been set. False otherwise.</returns>
    internal bool GetViewPropertyValue<T>( string propertyName, ref T returnValue )
    {
      object value = null;

      if( m_viewPropertiesDescriptors != null )
      {
        PropertyDescriptor descriptor = m_viewPropertiesDescriptors[ propertyName ];

        if( descriptor != null )
          value = descriptor.GetValue( this );
      }

      if( value != null )
      {
        returnValue = ( T )value;
        return true;
      }

      return false;
    }

    //---------- PRIVATE METHODS ----------

    private void ClearViewPropertyBindings()
    {
      foreach( ViewPropertyStruct viewProperty in m_viewProperties )
      {
        BindingOperations.ClearBinding( this, viewProperty.DependencyProperty );
        this.ClearValue( viewProperty.DependencyProperty );
      }

      m_viewProperties.Clear();

      //Empty the list of ViewBindingPropertyDescriptor as well.
      if( m_viewPropertiesDescriptors != null )
      {
        m_viewPropertiesDescriptors.Clear();
        m_viewPropertiesDescriptors = null;
      }
    }

    private static GroupLevelDescription CreateGroupLevelDescription( ColumnCollection columns, GroupDescription group )
    {
      GroupLevelDescription retval = null;
      string columnName = DataGridContext.GetColumnNameFromGroupDescription( group );

      if( !string.IsNullOrEmpty( columnName ) )
      {
        retval = new GroupLevelDescription( group, columnName );

        //find the column for that property name
        ColumnBase column = columns[ columnName ];

        if( column != null )
        {
          DataGridContext.SetupGroupLevelDescriptionBindings( retval, column );
        }
        else
        {
          //if no column exists for that name, then use the name as the Title for the GroupLevelDescription
          retval.SetTitle( columnName );
        }
      }

      return retval;
    }

    private static Binding CreateViewPropertyBindingForSource( object source, DependencyProperty property )
    {
      Binding binding = new Binding();
      binding.Source = source;
      binding.Path = new PropertyPath( property );
      binding.Mode = BindingMode.TwoWay;

      return binding;
    }

    private static GroupLevelDescription GetGroupLevelDescription( GroupLevelDescriptionCollection groupLevelDescriptions, GroupDescription group )
    {
      GroupLevelDescription retval = null;
      string columnName = DataGridContext.GetColumnNameFromGroupDescription( group );

      if( string.IsNullOrEmpty( columnName ) == false )
      {
        foreach( GroupLevelDescription info in groupLevelDescriptions )
        {
          if( info.FieldName == columnName )
          {
            retval = info;
            break;
          }
        }
      }

      return retval;
    }

    internal DefaultDetailConfiguration GetDefaultDetailConfigurationForContext()
    {
      if( this.AlreadySearchedForDefaultDetailConfig )
        return m_defaultDetailConfiguration;

      m_defaultDetailConfiguration = null;
      //start the loop with the current instance.
      if( ( this.SourceDetailConfiguration != null ) && ( this.SourceDetailConfiguration.IsAutoCreated == true ) )
      {
        DataGridContext currentContext = this;

        do
        {
          DefaultDetailConfiguration defaultDetailConfig = currentContext.DefaultDetailConfiguration;
          if( defaultDetailConfig != null )
          {
            m_defaultDetailConfiguration = defaultDetailConfig;
            break;
          }

          currentContext = currentContext.ParentDataGridContext;
        }
        while( currentContext != null );
      }

      this.AlreadySearchedForDefaultDetailConfig = true;
      return m_defaultDetailConfiguration;
    }

    internal void GetRealizedItems( List<object> containers )
    {
      lock( containers )
      {
        //obtain the IItemContainerGenerator interface of the ItemContainerGenerator (since called many places and it's bothering to always cast).
        IItemContainerGenerator generator = this.CustomItemContainerGenerator;
        //exception case, I wan to ask for the Generator Interface

        //obtain the GeneratorPosition from the last item in the items collection of the Itemscontrol, this will give me the number of items that are realized.
        int itemCount = this.CustomItemContainerGenerator.ItemCount;

        GeneratorPosition lastItemGenPos = generator.GeneratorPositionFromIndex( itemCount - 1 );

        //the index of the generator position of the last item will indicate the number of realized items.
        for( int i = 0; i <= lastItemGenPos.Index; i++ )
        {
          //To obtain the Container from the Generator, I need and Index, I can obtain an index from a GeneratorPosition
          GeneratorPosition itemGenPos = new GeneratorPosition( i, 0 ); //items with offset == 0 are "realized items"
          int itemIndex = generator.IndexFromGeneratorPosition( itemGenPos );

          object item = this.CustomItemContainerGenerator.ContainerFromIndex( itemIndex );

          //add the container the the enumerable collection
          containers.Add( item );
        }

        //exception case for the master DataGridContext, take the Fixed regions as well
        if( m_detailConfig == null )
        {
          Panel fixedPanel = m_dataGridControl.FixedHeadersHostPanel;
          if( fixedPanel != null )
          {
            foreach( object container in fixedPanel.Children )
            {
              containers.Add( container );
            }
          }

          fixedPanel = m_dataGridControl.FixedFootersHostPanel;
          if( fixedPanel != null )
          {
            foreach( object container in fixedPanel.Children )
            {
              containers.Add( container );
            }
          }
        } //end if Master DataGridContext

      }
    }

    private static void InsertColumnByVisiblePosition( HashedLinkedList<ColumnBase> linkedList, ColumnBase columnToInsert )
    {
      int requestedPosition = columnToInsert.VisiblePosition;
      LinkedListNode<ColumnBase> columnNode = linkedList.First;

      LinkedListNode<ColumnBase> insertBeforeNode = null;
      ColumnBase column = null;

      // Append columns which VisiblePosition haven't explicitly set
      if( columnToInsert.VisiblePosition == Int32.MaxValue )
      {
        linkedList.AddLast( columnToInsert );
        return;
      }

      if( linkedList.Count == 0 )
      {
        linkedList.AddFirst( columnToInsert );
      }
      else
      {
        while( columnNode != null )
        {
          column = columnNode.Value;

          if( column != null )
          {
            if( column.VisiblePosition > requestedPosition )
            {
              // Insertion point found
              insertBeforeNode = columnNode;
              break;
            }

            columnNode = columnNode.Next;
          }
        }

        if( insertBeforeNode != null )
        {
          linkedList.AddBefore( insertBeforeNode, columnToInsert );
          insertBeforeNode = null;
        }
        else
        {
          linkedList.AddLast( columnToInsert );
        }
      }
    }

    private void MergeDistinctValues( DistinctValuesRequestedEventArgs args )
    {
      Debug.Assert( this.Columns[ args.AutoFilterColumnFieldName ] != null );

      ReadOnlyObservableHashList readOnlyObservableHashList = null;

      if( this.DistinctValues == null )
        return;

      // We force the creation of DistinctValues for this field in every DataGridCollectionView
      if( this.DistinctValues.TryGetValue( args.AutoFilterColumnFieldName, out readOnlyObservableHashList ) )
      {
        foreach( object item in readOnlyObservableHashList )
        {
          args.DistinctValues.Add( item );
        }
      }
    }

    public void OnGroupDescriptionsChanged( object sender, NotifyCollectionChangedEventArgs e )
    {
      this.DelayBringIntoViewAndFocusCurrent();

      DataGridContext.UpdateGroupLevelDescriptions( this.GroupLevelDescriptions, e, this.Items.GroupDescriptions, this.Columns );
    }

    public void OnSortDescriptionsChanged( object sender, NotifyCollectionChangedEventArgs e )
    {
      this.DelayBringIntoViewAndFocusCurrent();

      //No need to protect against SortDescriptions being null, since this reacts to the sort descriptions changes
      DataGridContext.SynchronizeSortProperties( this.SortDescriptionsSyncContext, this.Items.SortDescriptions, this.Columns );
    }

    private static void SetupGroupLevelDescriptionBindings( GroupLevelDescription groupLevelDescription, ColumnBase column )
    {
      //Bind the column title to the GroupLevelDescription title
      Binding titleBinding = new Binding();
      titleBinding.Path = new PropertyPath( ColumnBase.TitleProperty );
      titleBinding.Source = column;
      titleBinding.Mode = BindingMode.OneWay;

      //Bind the column title template to the GroupLevelDescription title template
      Binding titleTemplateBinding = new Binding();
      titleTemplateBinding.Path =
        new PropertyPath( ColumnBase.TitleTemplateProperty );
      titleTemplateBinding.Source = column;
      titleTemplateBinding.Mode = BindingMode.OneWay;

      //Bind the column title template selector to the GroupLevelDescription title template selector
      Binding titleTemplateSelectorBinding = new Binding();
      titleTemplateSelectorBinding.Path =
        new PropertyPath( ColumnBase.TitleTemplateSelectorProperty );
      titleTemplateSelectorBinding.Source = column;
      titleTemplateSelectorBinding.Mode = BindingMode.OneWay;

      Binding valueTemplateBinding = null;
      Binding valueTemplateSelectorBinding = null;

      Column dataColumn = column as Column;

      // If we have a Column and a ForeignKey is defined
      if( ( dataColumn != null ) && ( dataColumn.ForeignKeyConfiguration != null ) )
      {
        //Bind the column GroupValueTemplate to the GroupLevelDescription value template
        valueTemplateBinding = new Binding();
        valueTemplateBinding.Source = dataColumn;
        valueTemplateBinding.Path = new PropertyPath( "(0).(1)",
          Column.ForeignKeyConfigurationProperty,
          ForeignKeyConfiguration.DefaultGroupValueTemplateProperty );
        valueTemplateBinding.Mode = BindingMode.OneWay;
      }
      else
      {
        //Bind the column GroupValueTemplate to the GroupLevelDescription value template
        valueTemplateBinding = new Binding();
        valueTemplateBinding.Path = new PropertyPath( ColumnBase.GroupValueTemplateProperty );
        valueTemplateBinding.Source = column;
        valueTemplateBinding.Mode = BindingMode.OneWay;

        //Bind the column GroupValueTemplateSelector to the GroupLevelDescription value template selector
        valueTemplateSelectorBinding = new Binding();
        valueTemplateSelectorBinding.Path = new PropertyPath( ColumnBase.GroupValueTemplateSelectorProperty );
        valueTemplateSelectorBinding.Source = column;
        valueTemplateSelectorBinding.Mode = BindingMode.OneWay;
      }

      BindingOperations.SetBinding( groupLevelDescription, GroupLevelDescription.TitleProperty, titleBinding );
      BindingOperations.SetBinding( groupLevelDescription, GroupLevelDescription.TitleTemplateProperty, titleTemplateBinding );
      BindingOperations.SetBinding( groupLevelDescription, GroupLevelDescription.TitleTemplateSelectorProperty, titleTemplateSelectorBinding );
      BindingOperations.SetBinding( groupLevelDescription, GroupLevelDescription.ValueTemplateProperty, valueTemplateBinding );

      if( valueTemplateSelectorBinding != null )
      {
        BindingOperations.SetBinding( groupLevelDescription, GroupLevelDescription.ValueTemplateSelectorProperty, valueTemplateSelectorBinding );
      }
    }

    private void CreateStandaloneCollections()
    {
      m_columns = new ColumnCollection( this.DataGridControl, null );
      VisibilityChangedEventManager.AddListener( m_columns, this );

      m_visibleColumns = new ReadOnlyColumnCollection();
      m_columnsByVisiblePosition = new HashedLinkedList<ColumnBase>();
      m_groupLevelDescriptions = new GroupLevelDescriptionCollection();

      //in this particular case, I need to create a SortDescriptionsSyncContext to ensure I will be able to synchronize access to the sort descriptions collection.
      m_sortDescriptionsSyncContext = new SortDescriptionsSyncContext();

      //Register to the Columns Collection's CollectionChanged event to manage the Removal of the CurrentColumn.
      CollectionChangedEventManager.AddListener( m_columns, this );

      // Register to the VisibleColumnsChanged to update, if need be, the columns desired width
      // when column stretching is active and there's a column reordering.
      CollectionChangedEventManager.AddListener( m_visibleColumns, this );
    }

    private void HandleMasterColumnsCollectionChanged( NotifyCollectionChangedEventArgs e )
    {
      bool removeCurrentColumn = false;
      bool needGeneratorHardReset = false;
      ColumnBase currentColumn = this.CurrentColumn;

      if( e.Action == NotifyCollectionChangedAction.Reset )
      {
        //Reset means that collection was cleared.
        removeCurrentColumn = true;
        needGeneratorHardReset = true;
      }
      else if( ( e.Action == NotifyCollectionChangedAction.Remove ) && ( e.OldItems.Contains( currentColumn ) == true ) )
      {
        //Remove of at least the current column
        removeCurrentColumn = true;
        needGeneratorHardReset = true;
      }
      else if( ( e.Action == NotifyCollectionChangedAction.Replace ) && ( e.OldItems.Contains( currentColumn ) )
        && ( !e.NewItems.Contains( currentColumn ) ) )
      {
        //Replace in which at least the current column was "replaced" by another (current column not present in new items )
        removeCurrentColumn = true;
        needGeneratorHardReset = true;
      }

      //If we computed that current columns should be cleared
      if( removeCurrentColumn )
      {
        //The column is not in the VisibleColumns list , set CurrentColumn to null.
        this.SetCurrentColumnCore(
          DataGridContext.GetClosestColumn( currentColumn, this.ColumnsByVisiblePosition ),
          false,
          m_dataGridControl.SynchronizeSelectionWithCurrent );
      }

      // When a column is removed, we need to clear the Generator from every container to avoid problem
      // when a Column with the same field name is removed thant reinserted with 2 different instances
      if( needGeneratorHardReset )
      {
        // We only reset the fixed region if the column is part of the master context 
        if( this.SourceDetailConfiguration == null )
          this.DataGridControl.ResetFixedRegions();

        this.DataGridControl.CustomItemContainerGenerator.RemoveAllAndNotify();
      }

      this.DefaultImageColumnDetermined = false;

      DataGridContext.HandleColumnsCollectionChanged(
        e, this.Columns,
        this.VisibleColumns as ReadOnlyColumnCollection,
        this.ColumnsByVisiblePosition,
        this.SortDescriptionsSyncContext,
        this.Items.SortDescriptions );

      if( m_columns != null )
      {
        ForeignKeyConfiguration.UpdateColumnsForeignKeyConfigurations(
          m_columns.FieldNameToColumnDictionary,
          this.ItemsSourceCollection,
          this.ItemsSourceFieldDescriptors,
          this.AutoCreateForeignKeyConfigurations );
      }
    }

    internal static ColumnBase GetClosestColumn( ColumnBase reference, HashedLinkedList<ColumnBase> columnsByVisiblePosition )
    {
      LinkedListNode<ColumnBase> oldColumnNode = columnsByVisiblePosition.Find( reference );

      ColumnBase newColumn = null;

      if( oldColumnNode != null )
      {
        LinkedListNode<ColumnBase> newColumnNode = oldColumnNode.Previous;

        if( newColumnNode == null )
        {
          newColumnNode = oldColumnNode.Next;
        }

        if( newColumnNode != null )
        {
          newColumn = newColumnNode.Value;
        }
      }

      return newColumn;
    }

    private void SetupCollectionsChangeNotification()
    {
      //register for change notification of the SortDescriptions
      INotifyCollectionChanged notify = m_items.SortDescriptions;
      m_sortDescriptionsCollectionChangedHandler = new WeakEventHandler<NotifyCollectionChangedEventArgs>( this.OnSortDescriptionsChanged ).Handler;
      notify.CollectionChanged += m_sortDescriptionsCollectionChangedHandler;

      //register for change notification of the GroupDescriptions
      m_groupDescriptionsCollectionChangedHandler = new WeakEventHandler<NotifyCollectionChangedEventArgs>( this.OnGroupDescriptionsChanged ).Handler;
      m_items.GroupDescriptions.CollectionChanged += m_groupDescriptionsCollectionChangedHandler;
    }

    private void SetupViewProperties( bool initialization = false )
    {
      this.ClearViewPropertyBindings();

      Views.ViewBase view = m_dataGridControl.GetView( !initialization );
      if( view == null )
        return;

      // First pass, get the "View-Only" properties and bind them.
      IEnumerable<DependencyProperty> viewProperties = view.GetViewProperties(); // retrieve the non-shared view properties.
      foreach( DependencyProperty viewProperty in viewProperties )
      {
        if( viewProperty.ReadOnly == true )
          throw new InvalidOperationException( "An attempt was made to return a read-only view property. Dependency properties returned by ViewBase.GetViewProperties() cannot be read-only." );

        Binding viewPropertyBinding = DataGridContext.CreateViewPropertyBindingForSource( view, viewProperty );

        BindingOperations.SetBinding( this, viewProperty, viewPropertyBinding );

        //Note: we place the shared and non-shared properties in the same pool (makes no differences beyond this point ).
        m_viewProperties.Add( new ViewPropertyStruct( ViewPropertyMode.ViewOnly, viewProperty ) );

        this.NotifyPropertyChanged( viewProperty.Name );
      }

      // Second pass, get the "shared" properties and bind them.
      viewProperties = view.GetSharedProperties(); // retrieve the shared view properties.
      foreach( DependencyProperty viewProperty in viewProperties )
      {
        if( viewProperty.ReadOnly == true )
          throw new InvalidOperationException( "An attempt was made to return a read-only view property. Dependency properties returned by ViewBase.GetSharedProperties() cannot be read-only." );

        object bindingSource = view;
        if( this.SourceDetailConfiguration != null )
        {
          bindingSource = this.GetDefaultDetailConfigurationForContext();
          if( bindingSource == null )
          {
            bindingSource = this.SourceDetailConfiguration;
          }
        }

        Binding viewPropertyBinding = DataGridContext.CreateViewPropertyBindingForSource( bindingSource, viewProperty );

        BindingOperations.SetBinding( this, viewProperty, viewPropertyBinding );

        //Note: we place the shared and non-shared properties in the same pool (makes no differences beyond this point ).
        m_viewProperties.Add( new ViewPropertyStruct( ViewPropertyMode.Routed, viewProperty ) );

        this.NotifyPropertyChanged( viewProperty.Name );
      }

      // Third pass, get the "shared no fallback" properties and bind them.
      viewProperties = view.GetSharedNoFallbackProperties(); // retrieve the shared no fallback view properties.
      foreach( DependencyProperty viewProperty in viewProperties )
      {
        if( viewProperty.ReadOnly == true )
          throw new InvalidOperationException( "An attempt was made to return a read-only view property. Dependency properties returned by ViewBase.GetSharedProperties() cannot be read-only." );

        object bindingSource = view;
        if( this.SourceDetailConfiguration != null )
        {
          bindingSource = this.GetDefaultDetailConfigurationForContext();
          if( bindingSource == null )
          {
            bindingSource = this.SourceDetailConfiguration;
          }
        }

        Binding viewPropertyBinding = DataGridContext.CreateViewPropertyBindingForSource( bindingSource, viewProperty );

        BindingOperations.SetBinding( this, viewProperty, viewPropertyBinding );

        //Note: we place the shared and non-shared properties in the same pool (makes no differences beyond this point ).
        m_viewProperties.Add( new ViewPropertyStruct( ViewPropertyMode.RoutedNoFallback, viewProperty ) );

        this.NotifyPropertyChanged( viewProperty.Name );
      }
    }

    private void InitializeViewProperties()
    {
      TableflowView.SetFixedColumnSplitterTranslation( this, new TranslateTransform() );
    }

    private void OnDetailConfigRequestingDelayBringIntoViewAndFocusCurrent( object sender, EventArgs e )
    {
      if( this.IsCurrent )
        this.DelayBringIntoViewAndFocusCurrent();
    }

    private static void CleanContainerSizeState( FrameworkElement container, ContainerSizeState containerSizeState )
    {
      if( containerSizeState.Height != DependencyProperty.UnsetValue )
        container.ClearValue( FrameworkElement.HeightProperty );

      if( containerSizeState.Width != DependencyProperty.UnsetValue )
        container.ClearValue( FrameworkElement.WidthProperty );

      if( containerSizeState.MinHeight != DependencyProperty.UnsetValue )
        container.ClearValue( FrameworkElement.MinHeightProperty );

      if( containerSizeState.MinWidth != DependencyProperty.UnsetValue )
        container.ClearValue( FrameworkElement.MinWidthProperty );

      if( containerSizeState.MaxHeight != DependencyProperty.UnsetValue )
        container.ClearValue( FrameworkElement.MaxHeightProperty );

      if( containerSizeState.MaxWidth != DependencyProperty.UnsetValue )
        container.ClearValue( FrameworkElement.MaxWidthProperty );
    }

    private static void RestoreContainerSizeWorker( FrameworkElement container, ContainerSizeState sizeState )
    {
      if( sizeState.Height != DependencyProperty.UnsetValue )
        container.SetValue( FrameworkElement.HeightProperty, sizeState.Height );

      if( sizeState.Width != DependencyProperty.UnsetValue )
        container.SetValue( FrameworkElement.WidthProperty, sizeState.Width );

      if( sizeState.MinHeight != DependencyProperty.UnsetValue )
        container.SetValue( FrameworkElement.MinHeightProperty, sizeState.MinHeight );

      if( sizeState.MinWidth != DependencyProperty.UnsetValue )
        container.SetValue( FrameworkElement.MinWidthProperty, sizeState.MinWidth );

      if( sizeState.MaxHeight != DependencyProperty.UnsetValue )
        container.SetValue( FrameworkElement.MaxHeightProperty, sizeState.MaxHeight );

      if( sizeState.MaxWidth != DependencyProperty.UnsetValue )
        container.SetValue( FrameworkElement.MaxWidthProperty, sizeState.MaxWidth );
    }

    //---------- OVERRIDES ----------

    protected override void OnPropertyChanged( DependencyPropertyChangedEventArgs e )
    {
      base.OnPropertyChanged( e );

      if( ( m_viewProperties != null ) && ( m_viewProperties.Count > 0 ) )
      {
        if( m_viewProperties.Contains( new ViewPropertyStruct( e.Property ) ) == true )
        {
          this.NotifyPropertyChanged( e.Property.Name );
        }
      }
    }

    //---------- INTERFACES ----------

    #region INotifyPropertyChanged Members

    public event PropertyChangedEventHandler PropertyChanged;

    private void NotifyPropertyChanged( string propertyName )
    {
      if( this.PropertyChanged != null )
        this.PropertyChanged( this, new PropertyChangedEventArgs( propertyName ) );
    }

    #endregion

    #region IWeakEventListener Members

    bool IWeakEventListener.ReceiveWeakEvent( Type managerType, object sender, EventArgs e )
    {
      if( managerType == typeof( CollectionChangedEventManager ) )
      {
        if( sender == m_items )
        {
          this.Items_CollectionChanged( sender, ( NotifyCollectionChangedEventArgs )e );
          return true;
        }
        if( sender == this.DetailConfigurations )
        {
          this.NotifyPropertyChanged( "HasDetails" );
          return true;
        }
        else if( sender == m_columns )
        {
          //Master Level columns collection changed
          this.HandleMasterColumnsCollectionChanged( ( NotifyCollectionChangedEventArgs )e );
          return true;
        }
        else if( sender == this.VisibleColumns )
        {
          ColumnStretchMode mode = ColumnStretchMode.None;

          if( this.GetViewPropertyValue<ColumnStretchMode>( "ColumnStretchMode", ref mode ) )
          {
            if( ( mode == ColumnStretchMode.First ) || ( mode == ColumnStretchMode.Last ) )
            {
              NotifyCollectionChangedEventArgs notifyArgs = ( NotifyCollectionChangedEventArgs )e;

              if( notifyArgs.NewItems != null )
              {
                foreach( ColumnBase column in notifyArgs.NewItems )
                {
                  column.ClearValue( ColumnBase.DesiredWidthProperty );
                }
              }
            }
          }

          return true;
        }
      }
      else if( managerType == typeof( VisibilityChangedEventManager ) )
      {
        var wrappedEventArgs = ( ColumnCollection.WrappedEventEventArgs )e;

        DataGridContext.UpdateVisibleColumns( this.Columns, this.VisibleColumns as ReadOnlyColumnCollection, this.ColumnsByVisiblePosition,
          new object[] { wrappedEventArgs.WrappedSender }, wrappedEventArgs.WrappedEventArgs as ColumnVisiblePositionChangedEventArgs );

        return true;
      }
      else if( managerType == typeof( VisibleColumnsUpdatingEventManager ) )
      {
        SelectionManager selectionManager = m_dataGridControl.SelectionChangerManager;
        selectionManager.Begin();

        try
        {
          selectionManager.UnselectAllCells( this );
        }
        finally
        {
          selectionManager.End( false, false, false );
        }

        return true;
      }
      else if( managerType == typeof( VisibleColumnsUpdatedEventManager ) )
      {
        // Only force a reset if at least one Row has not used an IVirtualizingCellsHost as PART_CellsHost
        if( this.DataGridControl.ForceGeneratorReset == true )
        {
          //forces the collection view to refresh (therefore, regenerates all the items ).
          this.CustomItemContainerGenerator.RemoveAllAndNotify();

          if( this.SourceDetailConfiguration == null ) // not linked to a DetailConfiguration (master level)
          {
            //also notify the DataGridControl so it can clear the fixed regions
            m_dataGridControl.ResetFixedRegions();
          }
        }

        this.DefaultImageColumnDetermined = false;
        return true;
      }
      else if( managerType == typeof( RealizedContainersRequestedEventManager ) )
      {
        RealizedContainersRequestedEventArgs eventArgs = ( RealizedContainersRequestedEventArgs )e;
        this.GetRealizedItems( eventArgs.RealizedContainers );
        return true;
      }
      else if( managerType == typeof( DistinctValuesRequestedEventManager ) )
      {
        DistinctValuesRequestedEventArgs eventArgs = ( DistinctValuesRequestedEventArgs )e;
        this.MergeDistinctValues( eventArgs );
        return true;
      }
      else if( managerType == typeof( AllowDetailToggleChangedEventManager ) )
      {
        this.NotifyPropertyChanged( "AllowDetailToggle" );
        return true;
      }
      else if( managerType == typeof( ViewChangedEventManager ) )
      {
        this.SetupViewProperties();
        this.InitializeViewProperties();
        return true;
      }
      else if( managerType == typeof( CurrentColumnChangedEventManager ) )
      {
        this.NotifyPropertyChanged( "CurrentColumn" );
        return true;
      }
      else if( managerType == typeof( GroupConfigurationSelectorChangedEventManager ) )
      {
        this.OnGroupConfigurationSelectorChanged();
        return true;
      }
      else if( managerType == typeof( DetailVisibilityChangedEventManager ) )
      {
        this.NotifyPropertyChanged( "HasDetails" );
        return true;
      }
      else if( managerType == typeof( MaxGroupLevelsChangedEventManager ) )
      {
        this.NotifyPropertyChanged( "MaxGroupLevels" );
        return true;
      }
      else if( managerType == typeof( MaxSortLevelsChangedEventManager ) )
      {
        this.NotifyPropertyChanged( "MaxSortLevels" );
        return true;
      }
      else if( managerType == typeof( ItemPropertiesChangedEventManager ) )
      {
        this.ItemsSourceFieldDescriptors = null;
        bool autoCreateColumns = ( m_detailConfig == null ) ? m_dataGridControl.AutoCreateColumns : m_detailConfig.AutoCreateColumns;

        if( autoCreateColumns )
        {
          ItemsSourceHelper.UpdateColumnsOnItemsPropertiesChanged(
            this.DataGridControl, this.Columns, this.AutoCreateForeignKeyConfigurations,
            ( NotifyCollectionChangedEventArgs )e, ( DataGridItemPropertyCollection )sender );
        }

        return true;
      }
      else if( managerType == typeof( ItemsSourceChangeCompletedEventManager ) )
      {
        // GenerateColumnsFromItemsSourceFields is already done in DataGridControl.ProcessDelayedItemsSourceChanged
        // no need to do it here.
        this.UnhookToItemPropertiesChanged( m_dataGridControlItemsSource as DataGridCollectionViewBase );
        m_dataGridControlItemsSource = m_dataGridControl.ItemsSource;
        this.HookToItemPropertiesChanged();
        return true;
      }

      return false;
    }

    #endregion

    #region Private Fields

    private BitVector32 m_flags = new BitVector32();
    private CurrencyManager m_currencyManager;
    private List<ViewPropertyStruct> m_viewProperties = new List<ViewPropertyStruct>();
    PropertyDescriptorCollection m_viewPropertiesDescriptors; // = null
    private DefaultDetailConfiguration m_defaultDetailConfiguration; // = null
    private RecyclingManager m_recyclingManager; // = null
    private ColumnBase m_defaultImageColumn; // = null
    private int m_deferRestoreStateCount; // = 0;
    private Dictionary<object, ContainerSizeState> m_sizeStateDictionary = new Dictionary<object, ContainerSizeState>();
    private IEnumerable m_dataGridControlItemsSource;

    private NotifyCollectionChangedEventHandler m_sortDescriptionsCollectionChangedHandler;
    private NotifyCollectionChangedEventHandler m_groupDescriptionsCollectionChangedHandler;

    #endregion

    #region IDataGridContextVisitable Members

    void IDataGridContextVisitable.AcceptVisitor( IDataGridContextVisitor visitor, out bool visitWasStopped )
    {
      ( ( IDataGridContextVisitable )this.CustomItemContainerGenerator ).AcceptVisitor( visitor, out visitWasStopped );
    }

    void IDataGridContextVisitable.AcceptVisitor( int minIndex, int maxIndex, IDataGridContextVisitor visitor, out bool visitWasStopped )
    {
      ( ( IDataGridContextVisitable )this.CustomItemContainerGenerator ).AcceptVisitor( minIndex, maxIndex, visitor, out visitWasStopped );
    }

    void IDataGridContextVisitable.AcceptVisitor( int minIndex, int maxIndex, IDataGridContextVisitor visitor, DataGridContextVisitorType visitorType, out bool visitWasStopped )
    {
      ( ( IDataGridContextVisitable )this.CustomItemContainerGenerator ).AcceptVisitor( minIndex, maxIndex, visitor, visitorType, out visitWasStopped );
    }

    void IDataGridContextVisitable.AcceptVisitor( int minIndex, int maxIndex, IDataGridContextVisitor visitor, DataGridContextVisitorType visitorType, bool visitDetails, out bool visitWasStopped )
    {
      ( ( IDataGridContextVisitable )this.CustomItemContainerGenerator ).AcceptVisitor( minIndex, maxIndex, visitor, visitorType, visitDetails, out visitWasStopped );
    }

    #endregion

    #region ICustomTypeDescriptor Members

    AttributeCollection ICustomTypeDescriptor.GetAttributes()
    {
      return AttributeCollection.Empty;
    }

    string ICustomTypeDescriptor.GetClassName()
    {
      return null;
    }

    string ICustomTypeDescriptor.GetComponentName()
    {
      return null;
    }

    TypeConverter ICustomTypeDescriptor.GetConverter()
    {
      return null;
    }

    EventDescriptor ICustomTypeDescriptor.GetDefaultEvent()
    {
      return null;
    }

    PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty()
    {
      return null;
    }

    object ICustomTypeDescriptor.GetEditor( Type editorBaseType )
    {
      return null;
    }

    EventDescriptorCollection ICustomTypeDescriptor.GetEvents( Attribute[] attributes )
    {
      return EventDescriptorCollection.Empty;
    }

    EventDescriptorCollection ICustomTypeDescriptor.GetEvents()
    {
      return EventDescriptorCollection.Empty;
    }

    PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties( Attribute[] attributes )
    {
      if( attributes == null )
      {
        // We only cache the full property list.
        if( m_viewPropertiesDescriptors == null )
        {
          Xceed.Wpf.DataGrid.Views.ViewBase view = this.DataGridControl.GetView();
          Debug.Assert( view != null );

          if( view == null )
            return PropertyDescriptorCollection.Empty;

          ViewBindingPropertyDescriptor[] properties = new ViewBindingPropertyDescriptor[ m_viewProperties.Count ];
          for( int i = 0; i < m_viewProperties.Count; i++ )
          {
            properties[ i ] = new ViewBindingPropertyDescriptor( m_detailConfig,
                                                                  this.GetDefaultDetailConfigurationForContext(),
                                                                  m_dataGridControl.GetView(),
                                                                  m_viewProperties[ i ].DependencyProperty,
                                                                  m_viewProperties[ i ].ViewPropertyMode );
          }

          m_viewPropertiesDescriptors = new PropertyDescriptorCollection( properties );
        }

        return m_viewPropertiesDescriptors;
      }

      return PropertyDescriptorCollection.Empty;
    }

    PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties()
    {
      return ( ( ICustomTypeDescriptor )this ).GetProperties( null );
    }

    object ICustomTypeDescriptor.GetPropertyOwner( PropertyDescriptor pd )
    {
      return this;
    }

    #endregion

    #region Automation

    private static readonly DependencyPropertyKey AutomationPeerPropertyKey =
      DependencyProperty.RegisterReadOnly( "AutomationPeer", typeof( DataGridContextAutomationPeer ), typeof( DataGridContext ),
        new FrameworkPropertyMetadata( null, FrameworkPropertyMetadataOptions.NotDataBindable ) );

    private static readonly DependencyProperty AutomationPeerProperty = AutomationPeerPropertyKey.DependencyProperty;

    internal DataGridContextAutomationPeer Peer
    {
      get
      {
        return this.GetValue( AutomationPeerProperty ) as DataGridContextAutomationPeer;
      }
      set
      {
        this.SetValue( AutomationPeerPropertyKey, value );
      }
    }

    internal AutomationPeer PeerSource
    {
      get
      {
        if( m_parentItem == null )
          return FrameworkElementAutomationPeer.CreatePeerForElement( m_dataGridControl );

        return this.Peer;
      }
    }

    internal void RaiseIsSelectedChangedAutomationEvent( DependencyObject container, bool isSelected )
    {
      DataGridContextAutomationPeer peer = this.Peer;

      if( ( peer != null ) && ( peer.ItemPeers != null ) )
      {
        object item = this.GetItemFromContainer( container );

        if( item != null )
        {
          Xceed.Wpf.DataGrid.Automation.DataGridItemAutomationPeer itemPeer = peer.ItemPeers[ item ] as Xceed.Wpf.DataGrid.Automation.DataGridItemAutomationPeer;

          if( itemPeer != null )
            itemPeer.RaiseAutomationIsSelectedChanged( isSelected );
        }
      }
    }

    #endregion Automation

    //---------- SUB CLASSES ----------

    #region DeferRestoreStateDisposable Private Class

    private class DeferRestoreStateDisposable : IDisposable
    {
      public DeferRestoreStateDisposable( DataGridContext dataGridContext )
      {
        m_dataGridContext = dataGridContext;
        m_dataGridContext.m_deferRestoreStateCount++;
      }

      private DataGridContext m_dataGridContext;

      #region IDisposable Members

      public void Dispose()
      {
        if( m_dataGridContext == null )
          return;

        m_dataGridContext.m_deferRestoreStateCount--;

        if( m_dataGridContext.m_deferRestoreStateCount == 0 )
        {
          m_dataGridContext.DataGridControl.RestoreDataGridContextState( m_dataGridContext );
        }

        m_dataGridContext = null;
      }

      #endregion
    }

    #endregion

    #region ViewPropertyStruct Private Struct

    private struct ViewPropertyStruct
    {
      public ViewPropertyStruct( ViewPropertyMode viewPropertyMode, DependencyProperty dependencyProperty )
      {
        if( dependencyProperty == null )
          throw new ArgumentNullException( "dependencyProperty" );

        this.ViewPropertyMode = viewPropertyMode;
        this.DependencyProperty = dependencyProperty;
      }

      public ViewPropertyStruct( DependencyProperty dependencyProperty )
        : this( ViewPropertyMode.None, dependencyProperty )
      {
      }

      public ViewPropertyMode ViewPropertyMode;
      public DependencyProperty DependencyProperty;


      public override bool Equals( object obj )
      {
        ViewPropertyStruct refStruct = ( ViewPropertyStruct )obj;

        if( this.DependencyProperty == refStruct.DependencyProperty )
        {
          return true;
        }

        return false;
      }

      public override int GetHashCode()
      {
        return this.DependencyProperty.GetHashCode();
      }
    }

    #endregion

    #region ViewBindingPropertyDescriptor Private Class

    private class ViewBindingPropertyDescriptor : PropertyDescriptor
    {
      public ViewBindingPropertyDescriptor( DetailConfiguration detailConfig, DefaultDetailConfiguration defaultDetailConfig, Xceed.Wpf.DataGrid.Views.ViewBase view, DependencyProperty dp, ViewPropertyMode viewPropertyMode )
        : base( dp.Name, null )
      {
        if( dp == null )
          throw new ArgumentNullException( "dp" );

        if( view == null )
          throw new ArgumentNullException( "view" );

        if( viewPropertyMode == ViewPropertyMode.None )
          throw new ArgumentException( "viewPropertyMode cannot be ViewPropertyMode.None", "mode" );

        m_dp = dp;
        m_detailConfig = detailConfig;
        m_defaultDetailConfig = defaultDetailConfig;
        m_view = view;
        m_viewPropertyMode = viewPropertyMode;
      }

      #region DependencyProperty Read-Only Property

      public DependencyProperty DependencyProperty
      {
        get
        {
          return m_dp;
        }
      }

      private DependencyProperty m_dp; // = null

      #endregion

      #region DetailConfig Read-Only Property

      public DetailConfiguration DetailConfig
      {
        get
        {
          return m_detailConfig;
        }
      }

      private DetailConfiguration m_detailConfig;

      #endregion

      #region DefaultDetailConfig Read-Only Property

      public DefaultDetailConfiguration DefaultDetailConfig
      {
        get
        {
          return m_defaultDetailConfig;
        }
      }

      private DefaultDetailConfiguration m_defaultDetailConfig;

      #endregion

      #region View Read-Only Property

      public Xceed.Wpf.DataGrid.Views.ViewBase View
      {
        get
        {
          return m_view;
        }
      }

      private Xceed.Wpf.DataGrid.Views.ViewBase m_view;

      #endregion

      #region ViewPropertyMode Read-Only Property

      public ViewPropertyMode ViewPropertyMode
      {
        get
        {
          return m_viewPropertyMode;
        }
      }

      private ViewPropertyMode m_viewPropertyMode;

      #endregion

      public override bool CanResetValue( object component )
      {
        return false; //stubbing this so that value cannot be reset through the PropertyDescriptor
      }

      public override Type ComponentType
      {
        get
        {
          return typeof( DataGridContext );
        }
      }

      public override object GetValue( object component )
      {
        object retval = null;
        DependencyObject dependencyObject = component as DependencyObject;
        if( dependencyObject != null )
        {
          retval = dependencyObject.GetValue( m_dp );
        }

        //If there isa a DefaultDetailConfig, process it in precedence to the DetailConfig
        if( m_defaultDetailConfig != null )
        {
          //AND there is NO local value set on it.
          object localValue = m_defaultDetailConfig.ReadLocalValue( m_dp );
          if( ( localValue == DependencyProperty.UnsetValue ) && ( this.ViewPropertyMode == ViewPropertyMode.Routed ) )
          {
            //then use the View's Value.
            retval = m_view.GetValue( m_dp );
          }
        }
        //If there isa a DetailConfig
        else if( m_detailConfig != null )
        {
          //AND there is NO local value set on it.
          object localValue = m_detailConfig.ReadLocalValue( m_dp );
          if( ( localValue == DependencyProperty.UnsetValue ) && ( this.ViewPropertyMode == ViewPropertyMode.Routed ) )
          {
            //then use the View's Value.
            retval = m_view.GetValue( m_dp );
          }
        }
        return retval;
      }

      public override bool IsReadOnly
      {
        get
        {
          return m_dp.ReadOnly;
        }
      }

      public override Type PropertyType
      {
        get
        {
          return m_dp.PropertyType;
        }
      }

      public override void ResetValue( object component )
      {
        throw new InvalidOperationException();
      }

      public override void SetValue( object component, object value )
      {
        DependencyObject dependencyObject = component as DependencyObject;
        if( dependencyObject != null )
        {
          dependencyObject.SetValue( m_dp, value );
          return;
        }
        throw new DataGridInternalException();
      }

      public override bool ShouldSerializeValue( object component )
      {
        return false;
      }
    }

    #endregion

    #region DataGridContextFlags Private Enum

    [Flags]
    private enum DataGridContextFlags
    {
      AlreadySearchedForDetailConfig = 1,
      DefaultImageColumnDetermined = 2,
      IsCurrent = 4,
      IsRestoringState = 8,
      IsSavingState = 16,
    }

    #endregion
  }
}
