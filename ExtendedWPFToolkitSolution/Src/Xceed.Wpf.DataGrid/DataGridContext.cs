/*************************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at http://xceed.com/wpf_toolkit

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using Xceed.Utils.Collections;
using Xceed.Wpf.DataGrid.Views;

namespace Xceed.Wpf.DataGrid
{
  public sealed class DataGridContext : DependencyObject, INotifyPropertyChanged, IWeakEventListener, IDataGridContextVisitable, ICustomTypeDescriptor
  {
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

      m_selectedItemsStore = new SelectedItemsStorage( this );
      m_selectedCellsStore = new SelectedCellsStorage( this );
      m_selectedItemsRanges = new SelectionItemRangeCollection( m_selectedItemsStore );
      m_selectedCellRanges = new SelectionCellRangeCollection( m_selectedCellsStore );
      m_selectedItems = new SelectionItemCollection( m_selectedItemsStore );

      CollectionChangedEventManager.AddListener( m_items, this );

      var dataGridCollectionViewBase = m_items as DataGridCollectionViewBase;
      if( dataGridCollectionViewBase != null )
      {
        PreBatchCollectionChangedEventManager.AddListener( dataGridCollectionViewBase, this );
        PostBatchCollectionChangedEventManager.AddListener( dataGridCollectionViewBase, this );
      }

      //if this is a DataGridCollectionView
      var dataGridCollectionView = m_items as DataGridCollectionView;
      if( dataGridCollectionView != null )
      {
        //Set its dataGridContext so it can propagate a DeferRefresh to details' DataGridCollectionView's
        dataGridCollectionView.PrepareRootContextForDeferRefresh( this );
      }

      //If the detailConfiguration is null then we are in the master...
      if( m_detailConfig == null )
      {
        var columns = new ColumnCollection( dataGridControl, null );

        m_columnManager = new ColumnHierarchyManager( columns );
        m_itemsSourcePropertyDescriptions = dataGridControl.ItemsSourcePropertyDescriptions;
        m_itemPropertyMap = dataGridControl.ItemPropertyMap;
        m_groupLevelDescriptions = new GroupLevelDescriptionCollection();

        //in this particular case, I need to create a SortDescriptionsSyncContext to ensure I will be able to synchronize access to the sort descriptions collection.
        m_sortDescriptionsSyncContext = new SortDescriptionsSyncContext();

        //Register to the Columns Collection's CollectionChanged event to manage the Removal of the CurrentColumn.
        CollectionChangedEventManager.AddListener( m_columnManager.Columns, this );

        // Register to the VisibleColumnsChanged to update, if need be, the columns desired width when column stretching is active and there's a column reordering.
        CollectionChangedEventManager.AddListener( m_columnManager.VisibleColumns, this );

        CollectionChangedEventManager.AddListener( m_items.SortDescriptions, this );
        CollectionChangedEventManager.AddListener( m_items.GroupDescriptions, this );

        GroupConfigurationSelectorChangedEventManager.AddListener( m_dataGridControl, this );
        AllowDetailToggleChangedEventManager.AddListener( m_dataGridControl, this );
        MaxGroupLevelsChangedEventManager.AddListener( m_dataGridControl, this );
        MaxSortLevelsChangedEventManager.AddListener( m_dataGridControl, this );
        ItemsSourceChangeCompletedEventManager.AddListener( m_dataGridControl, this );

        this.HookToItemPropertiesChanged();
      }
      //Detail DataGridContext
      else
      {
        m_detailConfig.AddDataGridContext( this );

        m_columnManager = m_detailConfig.ColumnManager;
        m_itemsSourcePropertyDescriptions = m_detailConfig.ItemsSourcePropertyDescriptions;
        m_itemPropertyMap = m_detailConfig.ItemPropertyMap;

        //only listen to the detail grid config current column changed if the detail grid config is synchronized.
        CurrentColumnChangedEventManager.AddListener( m_detailConfig, this );

        GroupConfigurationSelectorChangedEventManager.AddListener( m_detailConfig, this );
        AllowDetailToggleChangedEventManager.AddListener( m_detailConfig, this );

        // Register to the VisibleColumnsChanged to update, if need be, the columns desired width
        // when column stretching is active and there's a column reordering.
        CollectionChangedEventManager.AddListener( m_detailConfig.VisibleColumns, this );

        MaxGroupLevelsChangedEventManager.AddListener( m_detailConfig, this );
        MaxSortLevelsChangedEventManager.AddListener( m_detailConfig, this );

        m_detailConfig.DetailConfigurations.DataGridControl = dataGridControl;
      }

      Debug.Assert( m_columnManager != null );
      Debug.Assert( m_itemsSourcePropertyDescriptions != null );
      Debug.Assert( m_itemPropertyMap != null );

      ColumnsLayoutChangingEventManager.AddListener( m_columnManager, this );
      ColumnsLayoutChangedEventManager.AddListener( m_columnManager, this );
      DetailVisibilityChangedEventManager.AddListener( this.DetailConfigurations, this );
      RealizedContainersRequestedEventManager.AddListener( this.Columns, this );
      DistinctValuesRequestedEventManager.AddListener( this.Columns, this );

      this.SetupViewProperties();
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
    }

    private void Items_CollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
    {
      if( m_deferSelectionChangedOnItemsCollectionChanged != null )
      {
        m_deferSelectionChangedOnItemsCollectionChanged.Queue( e );
      }
      else
      {
        this.UpdateSelectionAfterSourceCollectionChanged( e );
      }
    }

    private void UpdateSelectionAfterSourceCollectionChanged( NotifyCollectionChangedEventArgs e )
    {
      if( m_dataGridControl.SelectedIndexPropertyNeedCoerce )
      {
        m_dataGridControl.CoerceValue( DataGridControl.SelectedIndexProperty );
      }

      if( m_dataGridControl.SelectedItemPropertyNeedCoerce )
      {
        m_dataGridControl.CoerceValue( DataGridControl.SelectedItemProperty );
      }

      m_dataGridControl.SelectionChangerManager.UpdateSelectionAfterSourceCollectionChanged( this, e );

    }

    internal void CleanDataGridContext()
    {
      m_currencyManager.CleanManager();

      if( m_items != null )
      {
        CollectionChangedEventManager.RemoveListener( m_items, this );

        var dataGridCollectionViewBase = this.ItemsSourceCollection as DataGridCollectionViewBase;
        if( dataGridCollectionViewBase != null )
        {
          dataGridCollectionViewBase.Dispose();
        }
      }

      if( m_detailConfig == null )
      {
        GroupConfigurationSelectorChangedEventManager.RemoveListener( m_dataGridControl, this );
        AllowDetailToggleChangedEventManager.RemoveListener( m_dataGridControl, this );
        MaxGroupLevelsChangedEventManager.RemoveListener( m_dataGridControl, this );
        MaxSortLevelsChangedEventManager.RemoveListener( m_dataGridControl, this );
        ItemsSourceChangeCompletedEventManager.RemoveListener( m_dataGridControl, this );

        CollectionChangedEventManager.RemoveListener( m_items.SortDescriptions, this );
        CollectionChangedEventManager.RemoveListener( m_items.GroupDescriptions, this );

        this.UnhookToItemPropertiesChanged( m_dataGridControlItemsSource as DataGridCollectionViewBase );
      }
      else
      {
        CurrentColumnChangedEventManager.RemoveListener( m_detailConfig, this );
        GroupConfigurationSelectorChangedEventManager.RemoveListener( m_detailConfig, this );
        AllowDetailToggleChangedEventManager.RemoveListener( m_detailConfig, this );
        CollectionChangedEventManager.RemoveListener( m_detailConfig.VisibleColumns, this );
        MaxGroupLevelsChangedEventManager.RemoveListener( m_detailConfig, this );
        MaxSortLevelsChangedEventManager.RemoveListener( m_detailConfig, this );

        m_detailConfig.RemoveDataGridContext( this );
      }

      this.ClearSizeStates();

      ColumnsLayoutChangingEventManager.RemoveListener( m_columnManager, this );
      ColumnsLayoutChangedEventManager.RemoveListener( m_columnManager, this );
      CollectionChangedEventManager.RemoveListener( this.DetailConfigurations, this );
      DetailVisibilityChangedEventManager.RemoveListener( this.DetailConfigurations, this );
      RealizedContainersRequestedEventManager.RemoveListener( this.Columns, this );
      DistinctValuesRequestedEventManager.RemoveListener( this.Columns, this );

      ViewChangedEventManager.RemoveListener( m_dataGridControl, this );

      var columnVirtualizationManager = this.ColumnVirtualizationManager;
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

    private readonly DataGridContext m_parentDataGridContext; // = null

    #endregion

    #region ParentItem Read-Only Property

    public object ParentItem
    {
      get
      {
        return m_parentItem;
      }
    }

    private readonly object m_parentItem;

    #endregion

    #region AllowDetailToggle Property

    public bool AllowDetailToggle
    {
      get
      {
        if( m_detailConfig != null )
        {
          var defaultDetailConfig = this.GetDefaultDetailConfigurationForContext();
          if( defaultDetailConfig != null )
            return defaultDetailConfig.AllowDetailToggle;

          return m_detailConfig.AllowDetailToggle;
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
        var dataGridCollectionViewBase = this.ItemsSourceCollection as DataGridCollectionViewBase;
        if( dataGridCollectionViewBase == null )
          return null;

        return null;
      }
    }

    #endregion

    #region Columns Read-Only Property

    public ColumnCollection Columns
    {
      get
      {
        return m_columnManager.Columns;
      }
    }

    #endregion

    #region Items Property

    public CollectionView Items
    {
      get
      {
        return m_items;
      }
    }

    private readonly CollectionView m_items;

    #endregion

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

        this.SetCurrentColumnCore( value, true, m_dataGridControl.SynchronizeSelectionWithCurrent, AutoScrollCurrentItemSourceTriggers.CurrentColumnChanged );
      }
    }

    internal void SetCurrentColumnCore( ColumnBase column, bool isCancelable, bool synchronizeSelectionWithCurrent, AutoScrollCurrentItemSourceTriggers trigger )
    {
      this.SetCurrent( this.InternalCurrentItem, null, null, column, false, isCancelable, synchronizeSelectionWithCurrent, trigger );
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

        this.SetCurrentItemCore( value, true, m_dataGridControl.SynchronizeSelectionWithCurrent, AutoScrollCurrentItemSourceTriggers.CurrentItemChanged );
      }
    }

    internal void SetCurrentItemCore( object item, bool isCancelable, bool synchronizeSelectionWithCurrent, AutoScrollCurrentItemSourceTriggers trigger )
    {
      this.SetCurrent( item, null, null, this.CurrentColumn, false, isCancelable, synchronizeSelectionWithCurrent, trigger );
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

        this.SetCurrentItemIndexCore( value, true, m_dataGridControl.SynchronizeSelectionWithCurrent, AutoScrollCurrentItemSourceTriggers.CurrentItemChanged );
      }
    }

    internal void SetCurrentItemIndexCore( int index, bool isCancelable, bool synchronizeSelectionWithCurrent, AutoScrollCurrentItemSourceTriggers trigger )
    {
      this.SetCurrent( this.Items.GetItemAt( index ), null, index, this.CurrentColumn, false, isCancelable, synchronizeSelectionWithCurrent, trigger );
    }

    private void SetCurrentItem( object dataItem, int sourceDataItemIndex )
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
      var handler = this.CurrentItemChanged;
      if( handler == null )
        return;

      handler.Invoke( this, EventArgs.Empty );
    }

    private object m_currentItem; // = null
    private int m_currentItemIndex = -1;

    #endregion

    #region DefaultGroupConfiguration Read-Only Property

    public GroupConfiguration DefaultGroupConfiguration
    {
      get
      {
        if( m_detailConfig != null )
        {
          var defaultDetailConfig = this.GetDefaultDetailConfigurationForContext();
          if( defaultDetailConfig != null )
            return defaultDetailConfig.DefaultGroupConfiguration;

          return m_detailConfig.DefaultGroupConfiguration;
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
          return m_detailConfig.DetailConfigurations;

        return null;
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
          var defaultDetailConfig = this.GetDefaultDetailConfigurationForContext();
          if( defaultDetailConfig != null )
            return defaultDetailConfig.Footers;

          return m_detailConfig.Footers;
        }

        var view = m_dataGridControl.GetView();
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

    private readonly GroupLevelDescriptionCollection m_groupLevelDescriptions; //null

    #endregion

    #region GroupConfigurationSelector Property

    public GroupConfigurationSelector GroupConfigurationSelector
    {
      get
      {
        if( m_detailConfig != null )
        {
          var defaultDetailConfig = this.GetDefaultDetailConfigurationForContext();
          if( defaultDetailConfig != null )
            return defaultDetailConfig.GroupConfigurationSelector;

          return m_detailConfig.GroupConfigurationSelector;
        }

        return m_dataGridControl.GroupConfigurationSelector;
      }
    }

    private void OnGroupConfigurationSelectorChanged()
    {
      var handler = this.GroupConfigurationSelectorChanged;
      if( handler == null )
        return;

      handler.Invoke( this, EventArgs.Empty );
    }

    internal event EventHandler GroupConfigurationSelectorChanged;

    #endregion

    #region HasDetails Read-Only Property

    public bool HasDetails
    {
      get
      {
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
          var defaultDetailConfig = this.GetDefaultDetailConfigurationForContext();
          if( defaultDetailConfig != null )
            return defaultDetailConfig.Headers;

          return m_detailConfig.Headers;
        }

        var view = m_dataGridControl.GetView();
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
        return m_flags[ ( int )DataGridContextFlags.IsCurrent ];
      }
    }

    internal void SetIsCurrent( bool value )
    {
      m_flags[ ( int )DataGridContextFlags.IsCurrent ] = value;
    }

    #endregion

    #region ItemContainerStyle

    public Style ItemContainerStyle
    {
      get
      {
        if( m_detailConfig != null )
        {
          var defaultDetailConfig = this.GetDefaultDetailConfigurationForContext();
          if( defaultDetailConfig != null )
            return defaultDetailConfig.ItemContainerStyle;

          return m_detailConfig.ItemContainerStyle;
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
          var defaultDetailConfig = this.GetDefaultDetailConfigurationForContext();
          if( defaultDetailConfig != null )
            return defaultDetailConfig.ItemContainerStyleSelector;

          return m_detailConfig.ItemContainerStyleSelector;
        }

        return m_dataGridControl.ItemContainerStyleSelector;
      }
    }

    #endregion

    #region DataGridControl Read-Only Property

    public DataGridControl DataGridControl
    {
      get
      {
        return m_dataGridControl;
      }
    }

    private readonly DataGridControl m_dataGridControl;

    #endregion

    #region SelectedItems Read-Only Property

    public IList<object> SelectedItems
    {
      get
      {
        return m_selectedItems;
      }
    }

    private readonly SelectionItemCollection m_selectedItems;

    #endregion

    #region SelectedItemRanges Read-Only Property

    public IList<SelectionRange> SelectedItemRanges
    {
      get
      {
        return m_selectedItemsRanges;
      }
    }

    private readonly SelectionItemRangeCollection m_selectedItemsRanges;

    #endregion

    #region SelectedCellRanges Read-Only Property

    public IList<SelectionCellRange> SelectedCellRanges
    {
      get
      {
        return m_selectedCellRanges;
      }
    }

    private readonly SelectionCellRangeCollection m_selectedCellRanges;

    #endregion

    #region SourceDetailConfiguration Read-Only Property

    internal DetailConfiguration SourceDetailConfiguration
    {
      get
      {
        return m_detailConfig;
      }
    }

    private readonly DetailConfiguration m_detailConfig;

    #endregion

    #region VisibleColumns Read-Only Property

    public ReadOnlyObservableCollection<ColumnBase> VisibleColumns
    {
      get
      {
        return m_columnManager.VisibleColumns;
      }
    }

    #endregion

    #region AutoCreateForeignKeyConfigurations Internal Property

    internal bool AutoCreateForeignKeyConfigurations
    {
      get
      {
        if( m_detailConfig != null )
          return m_detailConfig.AutoCreateForeignKeyConfigurations;

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
          var defaultDetailConfig = this.GetDefaultDetailConfigurationForContext();
          if( defaultDetailConfig != null )
            return defaultDetailConfig.MaxSortLevels;

          return m_detailConfig.MaxSortLevels;
        }

        return m_dataGridControl.MaxSortLevels;
      }
    }

    #endregion

    #region MaxGroupLevels Property

    public int MaxGroupLevels
    {
      get
      {
        if( m_detailConfig != null )
        {
          var defaultDetailConfig = this.GetDefaultDetailConfigurationForContext();
          if( defaultDetailConfig != null )
            return defaultDetailConfig.MaxGroupLevels;

          return m_detailConfig.MaxGroupLevels;
        }

        return m_dataGridControl.MaxGroupLevels;
      }
    }

    #endregion

    //--------- INTERNAL PROPERTIES ---------

    #region RootDataGridContext Read-Only Property

    internal DataGridContext RootDataGridContext
    {
      get
      {
        return this.DataGridControl.DataGridContext;
      }
    }

    #endregion

    #region SelectedItemsStore Read-Only Property

    internal SelectedItemsStorage SelectedItemsStore
    {
      get
      {
        return m_selectedItemsStore;
      }
    }

    private readonly SelectedItemsStorage m_selectedItemsStore;

    #endregion

    #region SelectedItemsStore Read-Only Property

    internal SelectedCellsStorage SelectedCellsStore
    {
      get
      {
        return m_selectedCellsStore;
      }
    }

    private readonly SelectedCellsStorage m_selectedCellsStore;

    #endregion

    #region ColumnsByVisiblePosition Read-Only Property

    internal HashedLinkedList<ColumnBase> ColumnsByVisiblePosition
    {
      get
      {
        return m_columnManager.ColumnsByVisiblePosition;
      }
    }

    #endregion

    #region ColumnStretchingManager Property

    internal ColumnStretchingManager ColumnStretchingManager
    {
      get
      {
        if( m_columnStretchingManager == null )
        {
          m_columnStretchingManager = new ColumnStretchingManager( this );
        }

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
        var currentRow = this.CurrentRow;
        if( currentRow == null )
          return null;

        return currentRow.Cells[ this.CurrentColumn ];
      }
    }

    #endregion

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

    #endregion

    #region CustomItemContainerGenerator Read-Only Property

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

    private CustomItemContainerGenerator m_generator;

    #endregion

    #region InternalCurrentItem Read-Only Property

    internal object InternalCurrentItem
    {
      get
      {
        return m_internalCurrentItem;
      }
    }

    private void SetInternalCurrentItem( object value )
    {
      m_internalCurrentItem = value;
    }

    private object m_internalCurrentItem; //null

    #endregion

    #region ItemsSourcePropertyDescriptions Property

    internal PropertyDescriptionRouteDictionary ItemsSourcePropertyDescriptions
    {
      get
      {
        return m_itemsSourcePropertyDescriptions;
      }
    }

    private readonly PropertyDescriptionRouteDictionary m_itemsSourcePropertyDescriptions;

    #endregion

    #region ItemPropertyMap Internal Property

    internal DataGridItemPropertyMap ItemPropertyMap
    {
      get
      {
        return m_itemPropertyMap;
      }
    }

    private readonly DataGridItemPropertyMap m_itemPropertyMap;

    #endregion

    #region ToggleColumnSortCommand Internal Property

    internal ToggleColumnSortCommand ToggleColumnSortCommand
    {
      get
      {
        if( m_toggleColumnSortCommand == null )
        {
          m_toggleColumnSortCommand = new DataGridContextToggleColumnSortCommand( this );
        }

        Debug.Assert( m_toggleColumnSortCommand != null );

        return m_toggleColumnSortCommand;
      }
    }

    private ToggleColumnSortCommand m_toggleColumnSortCommand;

    #endregion

    #region UpdateColumnSortCommand Internal Property

    internal UpdateColumnSortCommand UpdateColumnSortCommand
    {
      get
      {
        if( m_updateColumnSortCommand == null )
        {
          m_updateColumnSortCommand = new DataGridContextUpdateColumnSortCommand( this );
        }

        Debug.Assert( m_updateColumnSortCommand != null );

        return m_updateColumnSortCommand;
      }
    }

    private UpdateColumnSortCommand m_updateColumnSortCommand;

    #endregion

    #region AddGroupCommand Internal Property

    internal ColumnAddGroupCommand AddGroupCommand
    {
      get
      {
        if( m_addGroupCommand == null )
        {
          m_addGroupCommand = new DataGridContextAddGroupCommand( this );
        }

        Debug.Assert( m_addGroupCommand != null );

        return m_addGroupCommand;
      }
    }

    private ColumnAddGroupCommand m_addGroupCommand;

    #endregion

    #region SortDescriptionsSyncContext Read-Only Property

    internal SortDescriptionsSyncContext SortDescriptionsSyncContext
    {
      get
      {
        var dataGridCollectionViewBase = this.ItemsSourceCollection as DataGridCollectionViewBase;
        if( dataGridCollectionViewBase != null )
          return dataGridCollectionViewBase.DataGridSortDescriptions.SyncContext;

        Debug.Assert( m_sortDescriptionsSyncContext != null );
        return m_sortDescriptionsSyncContext;
      }
    }

    private readonly SortDescriptionsSyncContext m_sortDescriptionsSyncContext; //null

    #endregion

    #region DefaultDetailConfiguration Property

    internal DefaultDetailConfiguration DefaultDetailConfiguration
    {
      get
      {
        if( m_detailConfig != null )
          return m_detailConfig.DefaultDetailConfiguration;

        return m_dataGridControl.DefaultDetailConfiguration;
      }
    }

    #endregion

    #region ColumnVirtualizationManager Property

    internal ColumnVirtualizationManager ColumnVirtualizationManager
    {
      get
      {
        var columnVirtualizationManager = this.GetColumnVirtualizationManagerOrNull();
        if( columnVirtualizationManager == null )
        {
          columnVirtualizationManager = this.DataGridControl.GetView().CreateColumnVirtualizationManager( this );
        }

        // We must ensure the manager is up to date before using it
        columnVirtualizationManager.Update();

        return columnVirtualizationManager;
      }
    }

    internal ColumnVirtualizationManager GetColumnVirtualizationManagerOrNull()
    {
      return ColumnVirtualizationManager.GetColumnVirtualizationManager( this );
    }

    #endregion

    #region ColumnManagerRowConfiguration Read-Only Internal Property

    internal ColumnManagerRowConfiguration ColumnManagerRowConfiguration
    {
      get
      {
        if( m_detailConfig != null )
          return m_detailConfig.ColumnManagerRowConfiguration;

        return this.DataGridControl.ColumnManagerRowConfiguration;
      }
    }

    #endregion

    #region FixedHeaderFooterViewPortSize Property

    internal Size FixedHeaderFooterViewPortSize
    {
      get
      {
        return m_fixedHeaderFooterViewPortSize;
      }
      set
      {
        if( Size.Equals( value, m_fixedHeaderFooterViewPortSize ) )
          return;

        m_fixedHeaderFooterViewPortSize = value;

        this.OnPropertyChanged( "FixedHeaderFooterViewPortSize" );
      }
    }

    private Size m_fixedHeaderFooterViewPortSize = new Size();

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

      var defaultDetailConfig = dataGridContext.GetDefaultDetailConfigurationForContext();
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

    #endregion

    #region SelectionChanged Event

    internal event EventHandler<SelectionChangedInternalEventArgs> SelectionChangedInternal;

    internal void InvokeSelectionChanged( SelectionInfo selectionInfo )
    {

      if( this.SelectionChangedInternal != null )
      {
        this.SelectionChangedInternal( this, new SelectionChangedInternalEventArgs( selectionInfo ) );
      }

      Debug.Assert( selectionInfo.DataGridContext == this );

      if( ( selectionInfo.RemovedItems.Count == 0 ) && ( selectionInfo.AddedItems.Count == 0 ) )
        return;

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

    #region IsAFlattenDetail Internal Property

    internal bool IsAFlattenDetail
    {
      get
      {
        return ( m_detailConfig != null )
            && ( this.AreDetailsFlatten );
      }
    }

    #endregion

    #region AreDetailsFlatten Internal Property

    internal bool AreDetailsFlatten
    {
      get
      {
        return this.DataGridControl.AreDetailsFlatten;
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

    #region ColumnManager Internal Property

    internal ColumnHierarchyManager ColumnManager
    {
      get
      {
        return m_columnManager;
      }
    }

    private readonly ColumnHierarchyManager m_columnManager;

    #endregion

    #region CanIncreaseColumnWidth Property

    internal bool CanIncreaseColumnWidth
    {
      get
      {
        return m_canIncreaseColumnWidth;
      }
      set
      {
        m_canIncreaseColumnWidth = value;
      }
    }

    private bool m_canIncreaseColumnWidth = true;

    #endregion

    internal void SetCurrentColumnAndChangeSelection( ColumnBase newCurrentColumn )
    {
      // Since SetCurrentColumnCore can be aborted, we do it before changing the selection.
      this.SetCurrentColumnCore( newCurrentColumn, true, false, AutoScrollCurrentItemSourceTriggers.Navigation );

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

        var oldPosition = SelectionRangePoint.TryCreateFromCurrent( this );
        var newPosition = SelectionRangePoint.TryCreateRangePoint( this, item, dataRowIndex, newCurrentColumn.VisiblePosition );

        m_dataGridControl.SelectionChangerManager.UpdateSelection( oldPosition, newPosition, true, rowIsBeingEdited, SelectionManager.UpdateSelectionSource.Navigation );
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
      return ( this.CustomItemContainerGenerator.IsGroupExpanded( group ) == true );
    }

    internal bool? IsGroupExpanded( CollectionViewGroup group, bool recurseDetails )
    {
      return this.CustomItemContainerGenerator.IsGroupExpanded( group, recurseDetails );
    }

    public bool ToggleGroupExpansion( CollectionViewGroup group )
    {
      return this.CustomItemContainerGenerator.ToggleGroupExpansion( group );
    }

    public bool ExpandGroup( CollectionViewGroup group )
    {
      return this.CustomItemContainerGenerator.ExpandGroup( group );
    }

    internal bool ExpandGroup( CollectionViewGroup group, bool recurseDetails )
    {
      return this.CustomItemContainerGenerator.ExpandGroup( group, recurseDetails );
    }

    public bool CollapseGroup( CollectionViewGroup group )
    {
      return this.CustomItemContainerGenerator.CollapseGroup( group );
    }

    internal bool CollapseGroup( CollectionViewGroup group, bool recurseDetails )
    {
      return this.CustomItemContainerGenerator.CollapseGroup( group, recurseDetails );
    }

    public bool AreDetailsExpanded( object dataItem )
    {
      return this.CustomItemContainerGenerator.AreDetailsExpanded( dataItem );
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

      var container = default( DependencyObject );

      if( ( m_detailConfig == null ) && ( item is DataTemplate ) )
      {
        //If the container was not found in the DataGridContext's Generator and the DataGridContext does not have a source DetailConfig ( master context )
        //then check in the DataGridControl's Fixed Items
        container = m_dataGridControl.GetContainerForFixedItem( item );
      }

      if( container == null )
      {
        container = this.CustomItemContainerGenerator.ContainerFromItem( item );
      }

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
        Views.ViewBase view = m_dataGridControl.GetView();

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

    internal IEnumerable<DataGridContext> GetChildContextsCore()
    {
      return this.CustomItemContainerGenerator.GetChildContextsCore();
    }

    internal BindingPathValueExtractor GetBindingPathExtractorForColumn( Column column, object dataItem )
    {
      if( column == null )
        throw new ArgumentNullException( "column" );

      if( dataItem == null )
        return null;

      if( column == null )
        throw new ArgumentNullException( "column" );

      var xPath = default( string );
      var propertyPath = default( PropertyPath );

      // Disable warning for DisplayMemberBinding when internaly used
#pragma warning disable 618
      var displayMemberBinding = column.DisplayMemberBinding as Binding;
#pragma warning restore 618

      if( displayMemberBinding == null )
      {
        displayMemberBinding = ItemsSourceHelper.CreateDefaultBinding(
                                 ItemsSourceHelper.CreateOrGetPropertyDescriptionFromColumn( this, column, dataItem.GetType() ) );
      }

      if( displayMemberBinding == null )
        throw new DataGridInternalException( "DisplayMemberBinding is null.", m_dataGridControl );

      xPath = displayMemberBinding.XPath;
      propertyPath = displayMemberBinding.Path;

      var converter = new Xceed.Wpf.DataGrid.Converters.SourceDataConverter( ItemsSourceHelper.IsItemSupportingDBNull( dataItem ), CultureInfo.InvariantCulture );

      return new BindingPathValueExtractor( xPath, propertyPath, false, typeof( object ), converter, null, CultureInfo.InvariantCulture );
    }

    public IDisposable DeferColumnsUpdate()
    {
      return ColumnHierarchyManagerHelper.DeferColumnsUpdate( this );
    }

    public bool MoveColumnBefore( ColumnBase current, ColumnBase next )
    {
      return ColumnHierarchyManagerHelper.MoveColumnBefore( this, current, next );
    }

    public bool MoveColumnAfter( ColumnBase current, ColumnBase previous )
    {
      return ColumnHierarchyManagerHelper.MoveColumnAfter( this, current, previous );
    }

    public bool MoveColumnUnder( ColumnBase current, ColumnBase parent )
    {
      return ColumnHierarchyManagerHelper.MoveColumnUnder( this, current, parent );
    }

    //---------- INTERNAL METHODS -----------

    internal IDisposable DeferRestoreState()
    {
      return new DeferRestoreStateDisposable( this );
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
      var newSelectedRangeWithItems = ( m_selectedItemsStore.Count == 0 ) ? SelectionRangeWithItems.Empty : m_selectedItemsStore[ 0 ];

      try
      {
        if( newSelectedRangeWithItems.IsEmpty )
        {
          if( checkCellsStore )
          {
            var newSelectionCellRangeWithItems = ( m_selectedCellsStore.Count == 0 ) ? SelectionCellRangeWithItems.Empty : m_selectedCellsStore[ 0 ];

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
      catch( ArgumentOutOfRangeException )
      {
        selectedColumnIndex = -1;
        selectedItemIndex = -1;
        selectedItem = null;
      }
    }

    internal ColumnBase GetMatchingColumn( DataGridContext sourceContext, ColumnBase sourceColumn )
    {
      if( ( sourceContext == null ) || ( sourceColumn == null ) )
        return null;

      if( sourceContext == this )
        return sourceColumn;

      var collectionView = sourceContext.ItemsSourceCollection as DataGridCollectionViewBase;
      if( collectionView == null )
        return null;

      var sourceItemProperty = ItemsSourceHelper.GetItemPropertyFromProperty( collectionView.ItemProperties, sourceColumn.FieldName );
      if( sourceItemProperty == null )
        return null;

      var masterContext = sourceContext.RootDataGridContext;
      var masterItemProperty = sourceItemProperty;

      if( masterContext != sourceContext )
      {
        if( !sourceContext.ItemPropertyMap.TryGetMasterItemProperty( sourceItemProperty, out masterItemProperty ) )
          return null;
      }

      Debug.Assert( masterItemProperty != null );
      if( masterItemProperty == null )
        return null;

      var currentItemProperty = masterItemProperty;
      if( masterContext != this )
      {
        if( !this.ItemPropertyMap.TryGetDetailItemProperty( masterItemProperty, out currentItemProperty ) )
          return null;
      }

      Debug.Assert( currentItemProperty != null );
      if( currentItemProperty == null )
        return null;

      var columnName = PropertyRouteParser.Parse( currentItemProperty );
      if( string.IsNullOrEmpty( columnName ) )
        return null;

      return this.Columns[ columnName ];
    }

    internal ColumnBase GetDefaultImageColumn()
    {
      if( !this.DefaultImageColumnDetermined )
      {
        if( this.Items.Count > 0 )
        {
          this.DefaultImageColumnDetermined = true;

          var dataItem = this.Items.GetItemAt( 0 );

          foreach( var column in this.VisibleColumns )
          {
            var key = PropertyRouteParser.Parse( column.FieldName );
            if( key == null )
              continue;

            var propertyDescriptionRoute = default( PropertyDescriptionRoute );
            if( !m_itemsSourcePropertyDescriptions.TryGetValue( key, out propertyDescriptionRoute ) )
              continue;

            var propertyDescription = propertyDescriptionRoute.Current;
            var dataType = propertyDescription.DataType;

            if( typeof( ImageSource ).IsAssignableFrom( dataType ) )
            {
              m_defaultImageColumn = column;
              break;
            }
            else if( ( typeof( byte[] ).IsAssignableFrom( dataType ) ) ||
                     ( typeof( System.Drawing.Image ).IsAssignableFrom( dataType ) ) )
            {
              var converter = new Xceed.Wpf.DataGrid.Converters.ImageConverter();
              var convertedValue = default( object );
              var rawValue = default( object );

              try
              {
                if( propertyDescription.PropertyDescriptor == null )
                {
                  var dataRow = dataItem as System.Data.DataRow;
                  if( dataRow == null )
                  {
                    var dataRowView = dataItem as System.Data.DataRowView;
                    if( dataRowView != null )
                    {
                      rawValue = dataRowView[ propertyDescription.Name ];
                    }
                  }
                  else
                  {
                    rawValue = dataRow[ propertyDescription.Name ];
                  }
                }
                else
                {
                  rawValue = propertyDescription.PropertyDescriptor.GetValue( dataItem );
                }

                if( rawValue != null )
                {
                  convertedValue = converter.Convert( rawValue, typeof( ImageSource ), null, CultureInfo.CurrentCulture );
                }
              }
              catch( NotSupportedException )
              {
                //suppress the exception, the byte[] is not an image. convertedValue will remain null
              }

              if( convertedValue != null )
              {
                m_defaultImageColumn = column;
                break;
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

    internal IDisposable SetQueueBringIntoViewRestrictions( AutoScrollCurrentItemSourceTriggers trigger )
    {
      return m_dataGridControl.SetQueueBringIntoViewRestrictions( trigger );
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
      this.OnPropertyChanged( "HasDetails" );
    }

    internal void DelayBringIntoViewAndFocusCurrent( AutoScrollCurrentItemSourceTriggers trigger )
    {
      m_dataGridControl.DelayBringIntoViewAndFocusCurrent( trigger );
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
      bool synchronizeSelectionWithCurrent,
      AutoScrollCurrentItemSourceTriggers trigger )
    {
      // sourceDataItemIndex 
      //  null = item have to be checked if it is a dataItem
      //  -1   = the item it's not a dataItem
      //  ?    = then index of the item in the source collection.

      //verify that SetCurrent is not called successively caused by actions performed within, this is to prevent
      //exceptions caused by the accessing of resources that could be "locked" (e.g., ItemsControl.Items.DeferRefresh)
      if( m_dataGridControl.IsSetCurrentInProgress )
        throw new DataGridException( "SetCurrent cannot be invoked while another SetCurrent is in progress.", m_dataGridControl );

      try
      {
        //set the flag that indicates that we are already processing a SetCurrent
        m_dataGridControl.IsSetCurrentInProgress = true;

        var oldCurrentContext = m_dataGridControl.CurrentContext;
        var newCurrentContext = this;

        //store the previous public current item, so I can detect a change and lift the PropertyChanged for this properties
        var oldCurrentItem = oldCurrentContext.InternalCurrentItem;
        var oldPublicCurrentItem = oldCurrentContext.CurrentItem;
        var oldCurrentColumn = oldCurrentContext.CurrentColumn;
        var oldCurrentRow = oldCurrentContext.CurrentRow;

        //if item is not realized or if the item passed is not a Data Item (header, footer or group), then the old current cell will be null
        var oldCurrentCell = ( oldCurrentRow == null ) ? null : oldCurrentRow.Cells[ oldCurrentColumn ];

        var newCurrentRow = ( containerRow != null ) ? containerRow : Row.FromContainer( this.GetContainerFromItem( item ) );
        var newCurrentCell = default( Cell );

        //verify that set Current is not called on a column that cannot be current
        if( newCurrentRow != null )
        {
          newCurrentCell = newCurrentRow.Cells[ column ];
          if( ( column != null ) && column.ReadOnly && ( newCurrentCell != null ) && !newCurrentCell.GetCalculatedCanBeCurrent() )
            throw new DataGridException( "SetCurrent cannot be invoked if the column cannot be current.", m_dataGridControl );
        }

        var currentCellChanged = ( newCurrentCell != oldCurrentCell );

        if( ( ( item != oldCurrentItem ) || ( oldCurrentContext != newCurrentContext ) ) && ( oldCurrentRow != null ) )
        {
          if( Row.IsCellEditorDisplayConditionsSet( oldCurrentRow, CellEditorDisplayConditions.RowIsCurrent ) )
          {
            oldCurrentRow.RemoveDisplayEditorMatchingCondition( CellEditorDisplayConditions.RowIsCurrent );
          }

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
            {
              oldCurrentCell.SetIsCurrent( true );
            }

            oldCurrentRow.SetIsCurrent( true );
            throw;
          }
        }

        //For this if, the assumption is that current item hasn't changed, but current column has... (previous if will have caught the case where current item changed).

        //if the currentCell has changed and the old current cell still exists.
        if( ( currentCellChanged ) && ( oldCurrentCell != null ) )
        {
          //clear the CellIsCurrent editor display conditions
          if( Cell.IsCellEditorDisplayConditionsSet( oldCurrentCell, CellEditorDisplayConditions.CellIsCurrent ) )
          {
            oldCurrentCell.RemoveDisplayEditorMatchingCondition( CellEditorDisplayConditions.CellIsCurrent );
          }

          oldCurrentCell.SetIsCurrent( false );

          try
          {
            //cancel any ongoing editing on the cell. (this has no effect if row or cell is not being edited. This line can throw but no action is to be done.
            oldCurrentCell.EndEdit();
          }
          catch( DataGridException )
          {
            oldCurrentCell.SetIsCurrent( true );
            throw;
          }
        }

        if( ( item != oldCurrentItem ) || ( oldCurrentContext != newCurrentContext ) )
        {
          var currentChangingEventArgs = new DataGridCurrentChangingEventArgs( oldCurrentContext, oldCurrentItem, newCurrentContext, item, isCancelable );
          m_dataGridControl.RaiseCurrentChanging( currentChangingEventArgs );

          if( isCancelable && currentChangingEventArgs.Cancel )
          {
            // We restore the currentness on the previous row and cell.
            if( oldCurrentRow != null )
            {
              if( oldCurrentCell != null )
              {
                oldCurrentCell.SetIsCurrent( true );
              }

              oldCurrentRow.SetIsCurrent( true );
            }

            throw new DataGridException( "The operation has been canceled.", m_dataGridControl );
          }
        }

        //If they are different, clean the previously current DataGridContext
        if( oldCurrentContext != newCurrentContext )
        {
          oldCurrentContext.SetCurrentItem( null, -1 );
          oldCurrentContext.SetInternalCurrentItem( null );
          //preserve current column on the old DataGridContext

          oldCurrentContext.SetIsCurrent( false );
        }

        if( !sourceDataItemIndex.HasValue )
        {
          sourceDataItemIndex = this.Items.IndexOf( item );
        }

        // All the stuff that can throw is done
        newCurrentContext.SetInternalCurrentItem( item );
        newCurrentContext.SetIsCurrent( true );

        // change the CurrentRow and CurentColumn
        if( ( item != null ) && ( sourceDataItemIndex.Value != -1 ) )
        {
          newCurrentContext.SetCurrentItem( item, sourceDataItemIndex.Value );
        }
        else
        {
          newCurrentContext.SetCurrentItem( null, -1 );
        }

        newCurrentContext.SetCurrentColumnHelper( column );
        m_dataGridControl.SetCurrentDataGridContextHelper( this );

        // We must refetch the container for the current item in case EndEdit triggers a reset on the 
        // CustomItemContainerGenerator and remap this item to a container other than the one previously fetched
        newCurrentRow = Row.FromContainer( newCurrentContext.GetContainerFromItem( item ) );
        newCurrentCell = ( newCurrentRow == null ) ? null : newCurrentRow.Cells[ column ];

        bool currentRowChanged = ( newCurrentRow != oldCurrentRow );

        //If there is a container for the new Row.
        if( newCurrentRow != null )
        {
          Debug.Assert( newCurrentRow.IsContainerPrepared, "The container must be prepared to be set as current and call BeginEdit." );

          //update the RowIsCurrent display condition
          if( Row.IsCellEditorDisplayConditionsSet( newCurrentRow, CellEditorDisplayConditions.RowIsCurrent ) )
          {
            newCurrentRow.SetDisplayEditorMatchingCondition( CellEditorDisplayConditions.RowIsCurrent );
          }

          //Update the IsCurrent flag
          newCurrentRow.SetIsCurrent( true );

          //if the current row changed, make sure to check the appropriate edition triggers.
          if( currentRowChanged == true )
          {
            //if EditTriggers tells to enter editing mode AND no current cell is set on the new current row
            if( ( newCurrentRow.IsEditTriggerSet( EditTriggers.RowIsCurrent ) ) && ( newCurrentCell == null ) )
            {
              //To prevent re-entrancy of the SetCurrent fonction (since Row.BeginEdit will call Cell.BeginEdit which will call SetCurrent if not current already)
              if( this.VisibleColumns.Count > 0 )
              {
                var firstFocusableColumn = NavigationHelper.GetFirstVisibleFocusableColumnIndex( this, newCurrentRow );
                if( firstFocusableColumn < 0 )
                  throw new DataGridException( "Trying to edit while no cell is focusable. ", m_dataGridControl );

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
          {
            newCurrentCell.SetDisplayEditorMatchingCondition( CellEditorDisplayConditions.CellIsCurrent );
          }

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
            if( m_dataGridControl.IsBringIntoViewAllowed( trigger ) )
            {
              if( !m_dataGridControl.IsBringIntoViewDelayed )
              {
                if( newCurrentCell != null )
                {
                  newCurrentCell.BringIntoView();
                }
                else
                {
                  // Ensure the item is visible visible
                  newCurrentRow.BringIntoView();
                }
              }
              else
              {
                m_dataGridControl.DelayBringIntoViewAndFocusCurrent( trigger );
              }
            }

            using( m_dataGridControl.SelectionChangerManager.PushUpdateSelectionSource( SelectionManager.UpdateSelectionSource.None ) )
            {
              // Call this to update the Focus to the correct location
              m_dataGridControl.SetFocusHelper( newCurrentRow, column, forceFocus, true );
            }
          }
          else
          {
            m_dataGridControl.DelayBringIntoViewAndFocusCurrent( trigger );
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
            m_dataGridControl.SelectionChangerManager.End( false, false );
          }
        }

        if( this.CurrentItem != oldPublicCurrentItem )
        {
          this.OnPropertyChanged( "CurrentItem" );
        }

        if( column != oldCurrentColumn )
        {
          this.OnPropertyChanged( "CurrentColumn" );
        }

        if( ( item != oldCurrentItem ) || ( oldCurrentContext != newCurrentContext ) )
        {
          var currentChangedEventArgs = new DataGridCurrentChangedEventArgs( oldCurrentContext, oldCurrentItem, newCurrentContext, item );
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

    internal static DataGridContext SafeGetDataGridContextForDataGridCollectionView(
      DataGridContext rootDataGridContext,
      DataGridCollectionViewBase targetDataGridCollectionViewBase )
    {
      DataGridContext matchingDataGridContext = null;

      DataGridCollectionViewBase rootDataGridCollectionViewBase = rootDataGridContext.ItemsSourceCollection as DataGridCollectionViewBase;

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

    private static void UnselectAllCells( DataGridContext dataGridContext )
    {
      if( dataGridContext == null )
        return;

      var selectionManager = dataGridContext.DataGridControl.SelectionChangerManager;
      selectionManager.Begin();

      try
      {
        selectionManager.UnselectAllCells( dataGridContext );
      }
      finally
      {
        selectionManager.End( false, false );
      }
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
      foreach( Views.ViewBase.ViewPropertyStruct viewProperty in m_viewProperties )
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

    private Binding CreateViewPropertyBinding( Views.ViewBase view, Views.ViewBase.ViewPropertyStruct viewProperty )
    {
      var dependencyProperty = viewProperty.DependencyProperty;

      if( this.IsAFlattenDetail )
      {
        switch( viewProperty.FlattenDetailBindingMode )
        {
          case FlattenDetailBindingMode.None:
            return null;

          case FlattenDetailBindingMode.MasterOneWay:
            {
              var rootDataGridContext = this.RootDataGridContext;
              Debug.Assert( ( rootDataGridContext != null ) && ( this != rootDataGridContext ) );
              return DataGridContext.CreateViewPropertyBindingForSource( rootDataGridContext, dependencyProperty, BindingMode.OneWay );
            }

          default:
            break;
        }
      }

      if( viewProperty.ViewPropertyMode == ViewPropertyMode.ViewOnly )
        return DataGridContext.CreateViewPropertyBindingForSource( view, dependencyProperty );

      var detailConfiguration = this.SourceDetailConfiguration;
      if( detailConfiguration != null )
      {
        var defaultDetailConfiguration = this.GetDefaultDetailConfigurationForContext();
        if( defaultDetailConfiguration != null )
          return DataGridContext.CreateViewPropertyBindingForSource( defaultDetailConfiguration, dependencyProperty );

        return DataGridContext.CreateViewPropertyBindingForSource( detailConfiguration, dependencyProperty );
      }

      return DataGridContext.CreateViewPropertyBindingForSource( view, dependencyProperty );
    }

    private static Binding CreateViewPropertyBindingForSource( object source, DependencyProperty property )
    {
      return DataGridContext.CreateViewPropertyBindingForSource( source, property, BindingMode.TwoWay );
    }

    private static Binding CreateViewPropertyBindingForSource( object source, DependencyProperty property, BindingMode bindingMode )
    {
      Binding binding = new Binding();
      binding.Source = source;
      binding.Path = new PropertyPath( property );
      binding.Mode = bindingMode;

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

    internal static void InsertColumnByVisiblePosition( HashedLinkedList<ColumnBase> linkedList, ColumnBase columnToInsert )
    {
      int requestedPosition = columnToInsert.VisiblePosition;
      LinkedListNode<ColumnBase> columnNode = linkedList.First;

      LinkedListNode<ColumnBase> insertBeforeNode = null;
      ColumnBase column = null;

      // Append columns which VisiblePosition haven't explicitly set
      if( columnToInsert.VisiblePosition == int.MaxValue )
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

      var distinctValues = this.DistinctValues;
      if( distinctValues == null )
        return;

      var readOnlyObservableHashList = default( ReadOnlyObservableHashList );

      // We force the creation of DistinctValues for this field in every DataGridCollectionView
      if( distinctValues.TryGetValue( args.AutoFilterColumnFieldName, out readOnlyObservableHashList ) )
      {
        foreach( var item in readOnlyObservableHashList )
        {
          args.DistinctValues.Add( item );
        }
      }
    }

    private void OnGroupDescriptionsChanged( object sender, NotifyCollectionChangedEventArgs e )
    {
      this.DelayBringIntoViewAndFocusCurrent( AutoScrollCurrentItemSourceTriggers.GroupChanged );

      DataGridContext.UpdateGroupLevelDescriptions( this.GroupLevelDescriptions, e, this.Items.GroupDescriptions, this.Columns );
    }

    private void OnSortDescriptionsChanged( object sender, NotifyCollectionChangedEventArgs e )
    {
      this.DelayBringIntoViewAndFocusCurrent( AutoScrollCurrentItemSourceTriggers.SortChanged );

      var updateColumnSortCommand = this.UpdateColumnSortCommand;
      if( updateColumnSortCommand.CanExecute() )
      {
        updateColumnSortCommand.Execute();
      }
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
      Binding valueStringFormatBinding = null;
      Binding valueStringFormatCultureBinding = null;

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

        //Bind the column GroupValueStringFormat to the GroupLevelDescription string format
        valueStringFormatBinding = new Binding();
        valueStringFormatBinding.Path = new PropertyPath( ColumnBase.GroupValueStringFormatProperty );
        valueStringFormatBinding.Source = column;
        valueStringFormatBinding.Mode = BindingMode.OneWay;

        //Bind the column DefaultCulture to the GroupLevelDescription string format culture
        valueStringFormatCultureBinding = new Binding();
        valueStringFormatCultureBinding.Path = new PropertyPath( ColumnBase.DefaultCultureProperty );
        valueStringFormatCultureBinding.Source = column;
        valueStringFormatCultureBinding.Mode = BindingMode.OneWay;
        valueStringFormatCultureBinding.TargetNullValue = CultureInfo.CurrentCulture;
      }

      BindingOperations.SetBinding( groupLevelDescription, GroupLevelDescription.TitleProperty, titleBinding );
      BindingOperations.SetBinding( groupLevelDescription, GroupLevelDescription.TitleTemplateProperty, titleTemplateBinding );
      BindingOperations.SetBinding( groupLevelDescription, GroupLevelDescription.TitleTemplateSelectorProperty, titleTemplateSelectorBinding );
      BindingOperations.SetBinding( groupLevelDescription, GroupLevelDescription.ValueTemplateProperty, valueTemplateBinding );

      if( valueStringFormatBinding != null )
      {
        BindingOperations.SetBinding( groupLevelDescription, GroupLevelDescription.ValueStringFormatProperty, valueStringFormatBinding );
      }

      if( valueStringFormatCultureBinding != null )
      {
        BindingOperations.SetBinding( groupLevelDescription, GroupLevelDescription.ValueStringFormatCultureProperty, valueStringFormatCultureBinding );
      }

      if( valueTemplateSelectorBinding != null )
      {
        BindingOperations.SetBinding( groupLevelDescription, GroupLevelDescription.ValueTemplateSelectorProperty, valueTemplateSelectorBinding );
      }
    }

    private void HandleMasterColumnsCollectionChanged( NotifyCollectionChangedEventArgs e )
    {
      var removeCurrentColumn = false;
      var needGeneratorHardReset = false;
      var currentColumn = this.CurrentColumn;

      //If( e.Action == Add ), there is nothing to do.

      if( e.Action == NotifyCollectionChangedAction.Reset )
      {
        var columns = this.Columns;

        removeCurrentColumn = ( columns == null ) || ( !columns.Contains( currentColumn ) );
        needGeneratorHardReset = true;
      }
      else if( e.Action == NotifyCollectionChangedAction.Remove )
      {
        //Remove of at least the current column
        if( e.OldItems.Contains( currentColumn ) )
        {
          removeCurrentColumn = true;
          needGeneratorHardReset = true;
        }
      }
      else if( e.Action == NotifyCollectionChangedAction.Replace )
      {
        //Replace in which at least the current column was "replaced" by another (current column not present in new items )
        if( ( e.OldItems.Contains( currentColumn ) ) && ( !e.NewItems.Contains( currentColumn ) ) )
        {
          removeCurrentColumn = true;
          needGeneratorHardReset = true;
        }
      }
      //Nothing to do because the way to change the visible position of a column is through its VisiblePosition property.
      else if( e.Action == NotifyCollectionChangedAction.Move )
      {
        return;
      }

      var closestColumn = default( ColumnBase );

      //Select the current column's closest visible column while its possible
      if( removeCurrentColumn )
      {
        closestColumn = DataGridContext.GetClosestColumn( currentColumn, this.ColumnsByVisiblePosition );
      }

      // When a column is removed, we need to clear the Generator from every container to avoid problems
      // if the column is added again, but using a different instance (i.e. a new column with the same fieldname).
      if( needGeneratorHardReset )
      {
        // We only reset the fixed region if the column is part of the master context 
        if( this.SourceDetailConfiguration == null )
        {
          this.DataGridControl.ResetFixedRegions();
        }

        this.DataGridControl.CustomItemContainerGenerator.RemoveAllAndNotify();
      }

      this.DefaultImageColumnDetermined = false;

      var updateColumnSortCommand = this.UpdateColumnSortCommand;
      if( updateColumnSortCommand.CanExecute() )
      {
        this.UpdateColumnSortCommand.Execute();
      }

      if( removeCurrentColumn )
      {
        // Now that the columns collections have been updated, it is time to set the new current column if necessary.
        // The new current column that was identified earlier may have been removed from the visible columns after the collections update.
        if( ( closestColumn != null ) && !this.Columns.Contains( closestColumn ) )
        {
          closestColumn = null;
        }

        this.SetCurrentColumnCore( closestColumn, false, m_dataGridControl.SynchronizeSelectionWithCurrent, AutoScrollCurrentItemSourceTriggers.ColumnsCollectionChanged );
      }

      ForeignKeyConfiguration.UpdateColumnsForeignKeyConfigurations( this.Columns,
                                                                     this.ItemsSourceCollection,
                                                                     m_itemsSourcePropertyDescriptions,
                                                                     this.AutoCreateForeignKeyConfigurations );
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

    private void SetupViewProperties()
    {
      this.ClearViewPropertyBindings();

      Views.ViewBase view = m_dataGridControl.GetView();
      if( view == null )
        return;

      var viewProperties = view.GetViewProperties()
                             .Concat( view.GetSharedProperties() )
                             .Concat( view.GetSharedNoFallbackProperties() );

      foreach( var viewProperty in viewProperties )
      {
        var dependencyProperty = viewProperty.DependencyProperty;
        Debug.Assert( !dependencyProperty.ReadOnly );

        Binding binding = this.CreateViewPropertyBinding( view, viewProperty );
        if( binding != null )
        {
          BindingOperations.SetBinding( this, dependencyProperty, binding );
        }

        //Note: we place the shared and non-shared properties in the same pool (makes no differences beyond this point ).
        m_viewProperties.Add( viewProperty );

        this.OnPropertyChanged( dependencyProperty.Name );
      }
    }

    private void InitializeViewProperties()
    {
      if( !this.IsAFlattenDetail )
      {
        TableflowView.SetFixedColumnSplitterTranslation( this, new TranslateTransform() );
      }
    }

    private void HookToItemPropertiesChanged()
    {
      var collectionView = this.ItemsSourceCollection as DataGridCollectionViewBase;
      if( collectionView == null )
        return;

      this.RegisterItemProperties( collectionView.ItemProperties );
    }

    private void UnhookToItemPropertiesChanged( DataGridCollectionViewBase collectionView )
    {
      if( collectionView == null )
        return;

      this.UnregisterItemProperties( collectionView.ItemProperties );
    }

    private void UpdateColumns()
    {
      Debug.Assert( m_detailConfig == null );

      ItemsSourceHelper.ResetPropertyDescriptions( m_itemsSourcePropertyDescriptions, m_itemPropertyMap, m_dataGridControl, m_dataGridControlItemsSource );

      if( m_dataGridControl.AutoCreateColumns )
      {
        ItemsSourceHelper.UpdateColumnsFromPropertyDescriptions( m_columnManager, m_dataGridControl.DefaultCellEditors, m_itemsSourcePropertyDescriptions, this.AutoCreateForeignKeyConfigurations );
      }
    }

    private void RegisterItemProperties( DataGridItemPropertyCollection itemProperties )
    {
      if( itemProperties == null )
        return;

      CollectionChangedEventManager.AddListener( itemProperties, this );

      foreach( var itemProperty in itemProperties )
      {
        this.RegisterItemProperty( itemProperty );
      }
    }

    private void UnregisterItemProperties( DataGridItemPropertyCollection itemProperties )
    {
      if( itemProperties == null )
        return;

      foreach( var itemProperty in itemProperties )
      {
        this.UnregisterItemProperty( itemProperty );
      }

      CollectionChangedEventManager.RemoveListener( itemProperties, this );
    }

    private void RegisterItemProperty( DataGridItemPropertyBase itemProperty )
    {
      if( itemProperty == null )
        return;

      PropertyChangedEventManager.AddListener( itemProperty, this, DataGridItemPropertyBase.ItemPropertiesInternalPropertyName );
      this.RegisterItemProperties( itemProperty.ItemPropertiesInternal );
    }

    private void UnregisterItemProperty( DataGridItemPropertyBase itemProperty )
    {
      if( itemProperty == null )
        return;

      this.UnregisterItemProperties( itemProperty.ItemPropertiesInternal );
      PropertyChangedEventManager.RemoveListener( itemProperty, this, DataGridItemPropertyBase.ItemPropertiesInternalPropertyName );
    }

    private void OnItemPropertiesCollectionChanged( DataGridItemPropertyCollection collection, NotifyCollectionChangedEventArgs e )
    {
      Debug.Assert( m_detailConfig == null );

      var rootCollection = ItemsSourceHelper.GetRootCollection( collection );
      if( rootCollection == null )
        return;

      if( rootCollection == ( ( DataGridCollectionViewBase )m_dataGridControlItemsSource ).ItemProperties )
      {
        if( e.Action == NotifyCollectionChangedAction.Reset )
          throw new NotSupportedException();

        if( e.Action == NotifyCollectionChangedAction.Move )
          return;

        if( e.OldItems != null )
        {
          foreach( DataGridItemPropertyBase itemProperty in e.OldItems )
          {
            this.UnregisterItemProperty( itemProperty );
          }
        }

        if( e.NewItems != null )
        {
          foreach( DataGridItemPropertyBase itemProperty in e.NewItems )
          {
            this.RegisterItemProperty( itemProperty );
          }
        }

        this.UpdateColumns();
      }
      else
      {
        Debug.Fail( "The collection is not linked to the collection view's item properties." );
        this.UnregisterItemProperties( collection );
      }
    }

    private void OnItemPropertyPropertyChanged( DataGridItemPropertyBase itemProperty, PropertyChangedEventArgs e )
    {
      Debug.Assert( m_detailConfig == null );

      var rootCollection = ItemsSourceHelper.GetRootCollection( itemProperty );
      if( rootCollection == null )
        return;

      if( rootCollection != ( ( DataGridCollectionViewBase )m_dataGridControlItemsSource ).ItemProperties )
      {
        Debug.Fail( "The collection is not linked to the detail configuration's item properties." );
        this.UnregisterItemProperty( itemProperty );
        return;
      }

      if( string.IsNullOrEmpty( e.PropertyName ) || ( e.PropertyName == DataGridItemPropertyBase.ItemPropertiesInternalPropertyName ) )
      {
        var itemProperties = itemProperty.ItemPropertiesInternal;
        if( itemProperties != null )
        {
          this.UnregisterItemProperties( itemProperties );
          this.RegisterItemProperties( itemProperties );
          this.UpdateColumns();
        }
      }
    }

    private void OnDetailConfigRequestingDelayBringIntoViewAndFocusCurrent( object sender, RequestingDelayBringIntoViewAndFocusCurrentEventArgs e )
    {
      if( !this.IsCurrent )
        return;

      this.DelayBringIntoViewAndFocusCurrent( e.Trigger );
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
      this.OnPropertyChanged( e.Property.Name );
    }

    //---------- INTERFACES ----------

    #region INotifyPropertyChanged Members

    public event PropertyChangedEventHandler PropertyChanged;

    private void OnPropertyChanged( string propertyName )
    {
      var handler = this.PropertyChanged;
      if( handler == null )
        return;

      handler.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
    }

    #endregion

    #region IWeakEventListener Members

    bool IWeakEventListener.ReceiveWeakEvent( Type managerType, object sender, EventArgs e )
    {
      return this.OnReceiveWeakEvent( managerType, sender, e );
    }

    private bool OnReceiveWeakEvent( Type managerType, object sender, EventArgs e )
    {
      if( managerType == typeof( CollectionChangedEventManager ) )
      {
        var eventArgs = ( NotifyCollectionChangedEventArgs )e;

        if( sender == m_items )
        {
          var args = eventArgs.GetRangeActionOrSelf();

          this.Items_CollectionChanged( sender, args );
        }
        if( sender == this.DetailConfigurations )
        {
          this.OnPropertyChanged( "HasDetails" );
        }
        else if( sender == m_columnManager.Columns )
        {
          //Master Level columns collection changed
          this.HandleMasterColumnsCollectionChanged( eventArgs );
        }
        else if( sender == this.VisibleColumns )
        {
          var mode = ColumnStretchMode.None;

          if( this.GetViewPropertyValue<ColumnStretchMode>( "ColumnStretchMode", ref mode ) )
          {
            if( ( mode == ColumnStretchMode.First ) || ( mode == ColumnStretchMode.Last ) )
            {
              if( eventArgs.NewItems != null )
              {
                foreach( ColumnBase column in eventArgs.NewItems )
                {
                  column.ClearValue( ColumnBase.DesiredWidthProperty );
                }
              }
            }
          }
        }
        else if( sender == m_items.SortDescriptions )
        {
          this.OnSortDescriptionsChanged( sender, eventArgs );
        }
        else if( sender == m_items.GroupDescriptions )
        {
          this.OnGroupDescriptionsChanged( sender, eventArgs );
        }
        else if( sender is DataGridItemPropertyCollection )
        {
          this.OnItemPropertiesCollectionChanged( ( DataGridItemPropertyCollection )sender, eventArgs );
        }
      }
      else if( managerType == typeof( PropertyChangedEventManager ) )
      {
        var eventArgs = ( PropertyChangedEventArgs )e;

        if( sender is DataGridItemPropertyBase )
        {
          this.OnItemPropertyPropertyChanged( ( DataGridItemPropertyBase )sender, eventArgs );
        }
      }
      else if( managerType == typeof( ColumnsLayoutChangingEventManager ) )
      {
        DataGridContext.UnselectAllCells( this );
      }
      else if( managerType == typeof( ColumnsLayoutChangedEventManager ) )
      {
        // Only force a reset if at least one Row has not used an IVirtualizingCellsHost as PART_CellsHost
        if( m_dataGridControl.ForceGeneratorReset )
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
      }
      else if( managerType == typeof( PreBatchCollectionChangedEventManager ) )
      {
        Debug.Assert( m_deferSelectionChangedOnItemsCollectionChanged == null );

        // When using DataGridVirtualizingCollectionViewBase, we should keep the "Replace" action
        // in order to call UpdateSelectionAfterSourceCollectionChanged with a Replace instead of a Reset.
        if( m_deferSelectionChangedOnItemsCollectionChanged == null && !( sender is DataGridVirtualizingCollectionViewBase ) )
        {
          m_deferSelectionChangedOnItemsCollectionChanged = new DeferSelectionChangedOnItemsCollectionChangedDisposable( this );
        }
      }
      else if( managerType == typeof( PostBatchCollectionChangedEventManager ) )
      {
        var disposable = m_deferSelectionChangedOnItemsCollectionChanged;
        if( disposable != null )
        {
          m_deferSelectionChangedOnItemsCollectionChanged = null;
          disposable.Dispose();
        }
      }
      else if( managerType == typeof( RealizedContainersRequestedEventManager ) )
      {
        var eventArgs = ( RealizedContainersRequestedEventArgs )e;
        this.GetRealizedItems( eventArgs.RealizedContainers );
      }
      else if( managerType == typeof( DistinctValuesRequestedEventManager ) )
      {
        var eventArgs = ( DistinctValuesRequestedEventArgs )e;
        this.MergeDistinctValues( eventArgs );
      }
      else if( managerType == typeof( AllowDetailToggleChangedEventManager ) )
      {
        this.OnPropertyChanged( "AllowDetailToggle" );
      }
      else if( managerType == typeof( ViewChangedEventManager ) )
      {
        this.SetupViewProperties();
        this.InitializeViewProperties();
      }
      else if( managerType == typeof( CurrentColumnChangedEventManager ) )
      {
        this.OnPropertyChanged( "CurrentColumn" );
      }
      else if( managerType == typeof( GroupConfigurationSelectorChangedEventManager ) )
      {
        this.OnGroupConfigurationSelectorChanged();
      }
      else if( managerType == typeof( DetailVisibilityChangedEventManager ) )
      {
        this.OnPropertyChanged( "HasDetails" );
      }
      else if( managerType == typeof( MaxGroupLevelsChangedEventManager ) )
      {
        this.OnPropertyChanged( "MaxGroupLevels" );
      }
      else if( managerType == typeof( MaxSortLevelsChangedEventManager ) )
      {
        this.OnPropertyChanged( "MaxSortLevels" );
      }
      else if( managerType == typeof( ItemsSourceChangeCompletedEventManager ) )
      {
        var dataGridCollectionViewBase = m_dataGridControlItemsSource as DataGridCollectionViewBase;

        // GenerateColumnsFromItemsSourceFields is already done in DataGridControl.ProcessDelayedItemsSourceChanged no need to do it here.
        this.UnhookToItemPropertiesChanged( dataGridCollectionViewBase );

        if( dataGridCollectionViewBase != null )
        {
          PreBatchCollectionChangedEventManager.RemoveListener( dataGridCollectionViewBase, this );
          PostBatchCollectionChangedEventManager.RemoveListener( dataGridCollectionViewBase, this );
        }

        m_dataGridControlItemsSource = m_dataGridControl.ItemsSource;

        dataGridCollectionViewBase = m_dataGridControlItemsSource as DataGridCollectionViewBase;
        if( dataGridCollectionViewBase != null )
        {
          PreBatchCollectionChangedEventManager.AddListener( dataGridCollectionViewBase, this );
          PostBatchCollectionChangedEventManager.AddListener( dataGridCollectionViewBase, this );
        }

        this.HookToItemPropertiesChanged();

        //If this is the master level DataGridContext
        if( m_parentDataGridContext == null )
        {
          //And if this is a DataGridCollectionView
          DataGridCollectionView collectionView = m_dataGridControlItemsSource as DataGridCollectionView;
          if( collectionView != null )
          {
            //Set its dataGridContext so it can propagate a DeferRefresh to details' DataGridCollectionViews
            collectionView.PrepareRootContextForDeferRefresh( this );
          }
        }
      }
      else
      {
        return false;
      }

      return true;
    }

    #endregion

    #region Private Fields

    private BitVector32 m_flags = new BitVector32();
    private readonly CurrencyManager m_currencyManager;
    private readonly ICollection<Views.ViewBase.ViewPropertyStruct> m_viewProperties = new List<Views.ViewBase.ViewPropertyStruct>();
    private PropertyDescriptorCollection m_viewPropertiesDescriptors; // = null
    private DefaultDetailConfiguration m_defaultDetailConfiguration; // = null
    private ColumnBase m_defaultImageColumn; // = null
    private int m_deferRestoreStateCount; // = 0;
    private readonly Dictionary<object, ContainerSizeState> m_sizeStateDictionary = new Dictionary<object, ContainerSizeState>();
    private IEnumerable m_dataGridControlItemsSource;
    private DeferSelectionChangedOnItemsCollectionChangedDisposable m_deferSelectionChangedOnItemsCollectionChanged; // = null

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
          var view = m_dataGridControl.GetView();
          Debug.Assert( view != null );

          if( view == null )
            return PropertyDescriptorCollection.Empty;

          var properties = new ViewBindingPropertyDescriptor[ m_viewProperties.Count ];
          var defaultDetailConfiguration = this.GetDefaultDetailConfigurationForContext();
          var index = 0;

          foreach( var property in m_viewProperties )
          {
            properties[ index ] = new ViewBindingPropertyDescriptor( m_detailConfig, defaultDetailConfiguration, view, property.DependencyProperty, property.ViewPropertyMode );
            index++;
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

    //---------- SUB CLASSES ----------

    #region DeferRestoreStateDisposable Private Class

    private sealed class DeferRestoreStateDisposable : IDisposable
    {
      internal DeferRestoreStateDisposable( DataGridContext dataGridContext )
      {
        if( dataGridContext == null )
          throw new ArgumentNullException( "dataGridContext" );

        m_dataGridContext = new WeakReference( dataGridContext );

        Interlocked.Increment( ref dataGridContext.m_deferRestoreStateCount );
      }

      void IDisposable.Dispose()
      {
        this.Dispose( true );
        GC.SuppressFinalize( this );
      }

      private void Dispose( bool disposing )
      {
        var wr = Interlocked.Exchange( ref m_dataGridContext, null );
        if( wr == null )
          return;

        var dataGridContext = ( DataGridContext )wr.Target;
        if( dataGridContext == null )
          return;

        if( Interlocked.Decrement( ref dataGridContext.m_deferRestoreStateCount ) != 0 )
          return;

        Debug.Assert( disposing );

        var dataGridControl = dataGridContext.DataGridControl;
        dataGridControl.RestoreDataGridContextState( dataGridContext );
      }

      ~DeferRestoreStateDisposable()
      {
        this.Dispose( false );
      }

      private WeakReference m_dataGridContext;
    }

    #endregion

    #region DeferSelectionChangedOnItemsCollectionChangedDisposable Private Class

    private sealed class DeferSelectionChangedOnItemsCollectionChangedDisposable : IDisposable
    {
      internal DeferSelectionChangedOnItemsCollectionChangedDisposable( DataGridContext dataGridContext )
      {
        if( dataGridContext == null )
          throw new ArgumentNullException( "dataGridContext" );

        m_dataGridContext = new WeakReference( dataGridContext );
      }

      internal void Queue( NotifyCollectionChangedEventArgs e )
      {
        if( e == null )
          return;

        if( m_eventArgs != null )
        {
          m_eventArgs = NotifyBatchCollectionChangedEventArgs.Combine( m_eventArgs, e );
        }
        else
        {
          m_eventArgs = e;
        }
      }

      public void Dispose()
      {
        this.Dispose( true );
        GC.SuppressFinalize( this );
      }

      private void Dispose( bool disposing )
      {
        if( !disposing )
          return;

        var wr = Interlocked.Exchange( ref m_dataGridContext, null );
        if( wr == null )
          return;

        var eventArgs = m_eventArgs;
        if( eventArgs == null )
          return;

        var dataGridContext = ( DataGridContext )wr.Target;
        if( dataGridContext == null )
          return;

        dataGridContext.UpdateSelectionAfterSourceCollectionChanged( eventArgs );
      }

      private WeakReference m_dataGridContext;
      private NotifyCollectionChangedEventArgs m_eventArgs; //null
    }

    #endregion

    #region ViewBindingPropertyDescriptor Private Class

    private class ViewBindingPropertyDescriptor : PropertyDescriptor
    {
      public ViewBindingPropertyDescriptor( DetailConfiguration detailConfig, DefaultDetailConfiguration defaultDetailConfig, Views.ViewBase view, DependencyProperty dp, ViewPropertyMode viewPropertyMode )
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

      public Views.ViewBase View
      {
        get
        {
          return m_view;
        }
      }

      private Views.ViewBase m_view;

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
        if( m_viewPropertyMode == ViewPropertyMode.Routed )
        {
          var source = ( DependencyObject )m_defaultDetailConfig ?? m_detailConfig;

          //If there is a DefaultDetailConfig, process it in precedence to the DetailConfig.
          if( source != null )
          {
            var valueSource = DependencyPropertyHelper.GetValueSource( source, m_dp );

            switch( valueSource.BaseValueSource )
            {
              // Pick the value on the view.
              case BaseValueSource.Default:
              case BaseValueSource.Inherited:
              case BaseValueSource.Unknown:
                return m_view.GetValue( m_dp );

              // Pick the value on the source component itself.
              default:
                break;
            }
          }
        }

        var dependencyObject = component as DependencyObject;
        if( dependencyObject != null )
          return dependencyObject.GetValue( m_dp );

        return null;
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
        var dependencyObject = component as DependencyObject;
        if( dependencyObject == null )
          throw new ArgumentException( "SetValue was called for an object which is not a DependencyObject.", component.ToString() );

        dependencyObject.SetValue( m_dp, value );
      }

      public override bool ShouldSerializeValue( object component )
      {
        return false;
      }
    }

    #endregion

    #region DataGridContextToggleColumnSortCommand Private Class

    private sealed class DataGridContextToggleColumnSortCommand : ToggleColumnSortCommand
    {
      #region Constructor

      internal DataGridContextToggleColumnSortCommand( DataGridContext dataGridContext )
        : base()
      {
        ToggleColumnSortCommand.ThrowIfNull( dataGridContext, "dataGridContext" );

        m_dataGridContext = new WeakReference( dataGridContext );
      }

      #endregion

      #region Properties

      #region CanSort Protected Property

      protected override bool CanSort
      {
        get
        {
          var dataGridContext = this.DataGridContext;
          if( this.DataGridContext == null )
            return false;

          return this.DataGridContext.Items.CanSort;
        }
      }

      #endregion

      #region Columns Protected Property

      protected override ColumnCollection Columns
      {
        get
        {
          return this.DataGridContext.Columns;
        }
      }

      #endregion

      #region DataGridContext Protected Property

      protected override DataGridContext DataGridContext
      {
        get
        {
          return ( m_dataGridContext != null ) ? m_dataGridContext.Target as DataGridContext : null;
        }
      }

      private readonly WeakReference m_dataGridContext;

      #endregion

      #region MaxSortLevels Protected Property

      protected override int MaxSortLevels
      {
        get
        {
          return this.DataGridContext.MaxSortLevels;
        }
      }

      #endregion

      #region SortDescriptions Protected Property

      protected override SortDescriptionCollection SortDescriptions
      {
        get
        {
          return this.DataGridContext.Items.SortDescriptions;
        }
      }

      #endregion

      #endregion

      #region Methods Override

      protected override bool CanExecuteCore( ColumnBase column, bool resetSort )
      {
        var dataGridContext = this.DataGridContext;
        if( dataGridContext == null )
          return false;

        return base.CanExecuteCore( column, resetSort );
      }

      protected override void ValidateToggleColumnSort()
      {
        var dataGridContext = this.DataGridContext;
        if( dataGridContext == null )
          return;

        Debug.Assert( !dataGridContext.IsAFlattenDetail, "A flatten detail should not be able to toggle the column sort direction." );
      }

      protected override SortDescriptionsSyncContext GetSortDescriptionsSyncContext()
      {
        var dataGridContext = this.DataGridContext;

        ToggleColumnSortCommand.ThrowIfNull( dataGridContext, "dataGridContext" );
        return dataGridContext.SortDescriptionsSyncContext;
      }

      protected override void ValidateSynchronizationContext( SynchronizationContext synchronizationContext )
      {
        var dataGridContext = this.DataGridContext;
        ToggleColumnSortCommand.ThrowIfNull( dataGridContext, "dataGridContext" );

        if( !synchronizationContext.Own )
          throw new DataGridInternalException( "The column is already being processed.", dataGridContext.DataGridControl );
      }

      protected override void DeferRestoreStateOnLevel( Disposer disposer )
      {
        var dataGridContext = this.DataGridContext;
        ToggleColumnSortCommand.ThrowIfNull( dataGridContext, "dataGridContext" );

        ToggleColumnSortCommand.DeferRestoreStateOnLevel( disposer, dataGridContext );
      }

      protected override IDisposable SetQueueBringIntoViewRestrictions( AutoScrollCurrentItemSourceTriggers triggers )
      {
        var dataGridContext = this.DataGridContext;
        if( dataGridContext == null )
          return null;

        return this.DataGridContext.SetQueueBringIntoViewRestrictions( triggers );
      }

      protected override bool TryDeferResortSourceDetailConfiguration( out IDisposable defer )
      {
        var dataGridContext = this.DataGridContext;
        ToggleColumnSortCommand.ThrowIfNull( dataGridContext, "dataGridContext" );

        return this.TryDeferResort( dataGridContext.SourceDetailConfiguration, out defer );
      }

      protected override IDisposable DeferResortHelperItemsSourceCollection()
      {
        var dataGridContext = this.DataGridContext;
        ToggleColumnSortCommand.ThrowIfNull( dataGridContext, "dataGridContext" );

        return this.DeferResortHelper( dataGridContext.ItemsSourceCollection, dataGridContext.Items );
      }

      protected override void UpdateColumnSort()
      {
        var dataGridContext = this.DataGridContext;
        Debug.Assert( dataGridContext != null );

        base.UpdateColumnSort();
      }

      #endregion
    }

    #endregion

    #region DataGridContextUpdateColumnSortCommand Private Class

    private sealed class DataGridContextUpdateColumnSortCommand : UpdateColumnSortCommand
    {
      #region Validation Methods

      private static void ThrowIfDetailDataGridContext( DataGridContext dataGridContext, string paramName )
      {
        Debug.Assert( dataGridContext != null );

        if( dataGridContext.ParentDataGridContext != null )
          throw new ArgumentException( "The DataGridContext is not the topmost DataGridContext.", paramName );
      }

      #endregion

      #region Constructor

      internal DataGridContextUpdateColumnSortCommand( DataGridContext dataGridContext )
      {
        DataGridContextUpdateColumnSortCommand.ThrowIfNull( dataGridContext, "dataGridContext" );

        m_dataGridContext = new WeakReference( dataGridContext );
      }

      #endregion

      #region SortDescriptionsSyncContext Protected Property

      protected override SortDescriptionsSyncContext SortDescriptionsSyncContext
      {
        get
        {
          var dataGridContext = this.DataGridContext;
          if( dataGridContext == null )
            return null;

          return dataGridContext.SortDescriptionsSyncContext;
        }
      }

      #endregion

      #region DataGridContext Private Property

      private DataGridContext DataGridContext
      {
        get
        {
          return m_dataGridContext.Target as DataGridContext;
        }
      }

      private readonly WeakReference m_dataGridContext;

      #endregion

      protected override bool CanExecuteCore()
      {
        return ( this.DataGridContext != null );
      }

      protected override void ExecuteCore()
      {
        var dataGridContext = this.DataGridContext;
        if( dataGridContext == null )
          return;

        var detailConfiguration = dataGridContext.SourceDetailConfiguration;
        if( detailConfiguration != null )
        {
          this.Update( detailConfiguration );
        }
        // The current DataGridContext is the top most DataGridContext;
        else
        {
          this.Update( dataGridContext );
        }
      }

      private void Update( DataGridContext dataGridContext )
      {
        ColumnSortCommand.ThrowIfNull( dataGridContext, "dataGridContext" );
        DataGridContextUpdateColumnSortCommand.ThrowIfDetailDataGridContext( dataGridContext, "dataGridContext" );

        // The sort order of the flatten details is driven by the master.  A sort order change
        // on the master must be reflected in the details.
        if( dataGridContext.AreDetailsFlatten )
        {
          foreach( var detailConfiguration in dataGridContext.DetailConfigurations )
          {
            this.Update( detailConfiguration, true );
          }
        }

        using( var synchronizationContext = this.StartSynchronizing( dataGridContext.SortDescriptionsSyncContext ) )
        {
          this.SynchronizeColumnSort(
            synchronizationContext,
            dataGridContext.Items.SortDescriptions,
            dataGridContext.Columns );
        }
      }

      private void Update( DetailConfiguration detailConfiguration )
      {
        this.Update( detailConfiguration, false );
      }

      private void Update( DetailConfiguration detailConfiguration, bool recursive )
      {
        ColumnSortCommand.ThrowIfNull( detailConfiguration, "detailConfiguration" );

        if( recursive )
        {
          foreach( var children in detailConfiguration.DetailConfigurations )
          {
            this.Update( children, true );
          }
        }

        var command = detailConfiguration.UpdateColumnSortCommand;
        if( command.CanExecute() )
        {
          command.Execute();
        }
      }
    }

    #endregion

    #region DataGridContextAddGroupCommand Private Class

    private sealed class DataGridContextAddGroupCommand : ColumnAddGroupCommand
    {
      #region Constructor

      internal DataGridContextAddGroupCommand( DataGridContext dataGridContext )
      {
        DataGridContextAddGroupCommand.ThrowIfNull( dataGridContext, "dataGridContext" );

        m_dataGridContext = new WeakReference( dataGridContext );
      }

      #endregion

      #region GroupDescriptions Protected Property

      protected override ObservableCollection<GroupDescription> GroupDescriptions
      {
        get
        {
          var dataGridContext = this.DataGridContext;
          if( dataGridContext == null )
            return null;

          return dataGridContext.Items.GroupDescriptions;
        }
      }

      #endregion

      #region DataGridContext Private Property

      private DataGridContext DataGridContext
      {
        get
        {
          return m_dataGridContext.Target as DataGridContext;
        }
      }

      private readonly WeakReference m_dataGridContext;

      #endregion

      protected override string GetColumnName( ColumnBase column )
      {
        var dataGridContext = this.DataGridContext;
        if( ( dataGridContext == null ) || ( column == null ) )
          return null;

        var itemPropertyMap = dataGridContext.ItemPropertyMap;
        if( ( itemPropertyMap != null ) && itemPropertyMap.IsMapping )
        {
          string fieldName;
          if( DataGridItemPropertyMapHelper.TryGetDetailColumnName( itemPropertyMap, column.FieldName, out fieldName ) )
            return fieldName;
        }
        else
        {
          return column.FieldName;
        }

        return null;
      }

      protected override GroupDescription GetGroupDescription( ColumnBase column )
      {
        if( this.DataGridContext == null )
          return null;

        return base.GetGroupDescription( column );
      }

      protected override GroupConfiguration GetGroupConfiguration( ColumnBase column )
      {
        if( this.DataGridContext == null )
          return null;

        return base.GetGroupConfiguration( column );
      }

      protected override bool CanExecuteCore( ColumnBase column, int index )
      {
        if( this.DataGridContext == null )
          return false;

        return base.CanExecuteCore( column, index );
      }

      protected override void ExecuteCore( ColumnBase column, int index )
      {
        var dataGridContext = this.DataGridContext;
        if( dataGridContext == null )
          return;

        using( dataGridContext.SetQueueBringIntoViewRestrictions( AutoScrollCurrentItemSourceTriggers.CollectionViewCurrentItemChanged ) )
        {
          base.ExecuteCore( column, index );
        }
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
