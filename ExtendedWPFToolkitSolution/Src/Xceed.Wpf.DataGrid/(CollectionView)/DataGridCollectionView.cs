/************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2010-2012 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus edition at http://xceed.com/wpf_toolkit

   Visit http://xceed.com and follow @datagrid on Twitter

  **********************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Data;
using System.Windows.Threading;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Data;
using System.Globalization;
using System.Security;
using System.Security.Permissions;
using System.Threading;
using System.Windows;
using System.Xml;

using Xceed.Utils;
using Xceed.Utils.Collections;
using Xceed.Utils.Data;
using Xceed.Wpf.DataGrid.Stats;
using System.Data.Objects.DataClasses;

namespace Xceed.Wpf.DataGrid
{
  [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Design", "CA1039:ListsAreStronglyTyped" )]
  [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Design", "CA1035:ICollectionImplementationsHaveStronglyTypedMembers" )]
  [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix" )]
  public sealed partial class DataGridCollectionView : DataGridCollectionViewBase, ISupportInitializeNotification, ICancelAddNew, IBindingList
  {
    public DataGridCollectionView( IEnumerable collection )
      : this( collection, null, true, true )
    {
    }

    public DataGridCollectionView( IEnumerable collection, Type itemType )
      : this( collection, itemType, true, true )
    {
    }

    public DataGridCollectionView( IEnumerable collection, Type itemType, bool autoCreateItemProperties, bool autoCreateDetailDescriptions )
      : this( collection, itemType, autoCreateItemProperties, autoCreateDetailDescriptions, false )
    {
    }

    public DataGridCollectionView( IEnumerable collection, Type itemType, bool autoCreateItemProperties, bool autoCreateDetailDescriptions, bool autoCreateForeignKeyDescriptions )
      : base( null, collection, itemType, autoCreateItemProperties, autoCreateDetailDescriptions, autoCreateForeignKeyDescriptions )
    {
      if( collection == null )
        throw new ArgumentNullException( "collection" );
    }

    [EditorBrowsable( EditorBrowsableState.Never )]
    [Obsolete( "This constructor is obsolete and should no longer be used.", true )]
    public DataGridCollectionView( Type itemType )
      : base( null, null, null, false, false, false )
    {
      throw new NotSupportedException( "This constructor is obsolete and should no longer be used." );
    }

    private DataGridCollectionView( IEnumerable collection, DataGridDetailDescription parentDetailDescription, DataGridCollectionView rootDataGridCollectionView )
      : base( collection, parentDetailDescription, rootDataGridCollectionView )
    {
      if( collection == null )
        throw new ArgumentNullException( "collection" );

      this.UpdateChangedPropertyStatsOnly = rootDataGridCollectionView.UpdateChangedPropertyStatsOnly;
    }

    internal override void PreConstruct()
    {
      base.PreConstruct();

      m_sourceItems = new SourceItemCollection( this );
      this.RootGroup = new DataGridCollectionViewGroupRoot( this );
    }

    #region StatFunctions Property

    internal StatFunctionCollection StatFunctions
    {
      get
      {
        return m_statFunctions;
      }
    }

    StatFunctionCollection m_statFunctions; // = null

    #endregion StatFunctions Property

    #region InvalidatedStatFunctions Property

    internal List<StatFunction> InvalidatedStatFunctions
    {
      get
      {
        return m_invalidatedStatFunctions;
      }
    }

    List<StatFunction> m_invalidatedStatFunctions = new List<StatFunction>();

    #endregion

    #region UpdateChangedPropertyStatsOnly Property

    public bool UpdateChangedPropertyStatsOnly
    {
      get;
      set;
    }

    #endregion

    #region CalculateChangedPropertyStatsOnly Property

    internal bool CalculateChangedPropertyStatsOnly
    {
      get;
      set;
    }

    #endregion

    #region SourceItems Property

    public IList SourceItems
    {
      get
      {
        this.EnsureThreadAndCollectionLoaded();
        return m_sourceItems;
      }
    }

    private SourceItemCollection m_sourceItems;

    #endregion SourceItems Property

    #region View Property

    // The View property was created to continue the support of a binding where the 
    // Path property was set to View to fix an issue with VS 2008:
    // ItemsSource="{Binding Source={StaticResource cvs_orders}, Path=View}
    //
    // Since binding to a CollectionViewSource is like binding directly to 
    // the object returned by the CVS (the DataGridCollectionView), which was not 
    // the case with our older DataGridCollectionViewSource, we decided to expose "View" here
    // to support the previously recommended fix.
    [Browsable( false )]
    [EditorBrowsable( EditorBrowsableState.Never )]
    [Obsolete( "The View property is obsolete and should not be used. A Binding's Path property no longer needs to  be set to the underlying View.", true )]
    public DataGridCollectionView View
    {
      get
      {
        return this;
      }
    }

    #endregion View Property

    #region Count Property

    public override int Count
    {
      get
      {
        this.EnsureThreadAndCollectionLoaded();
        return this.RootGroup.GlobalRawItemCount;
      }
    }

    #endregion Count Property

    #region IsAddingItem Property

    [Obsolete( "The IsAddingItem property is obsolete and has been replaced by the IsAddingNew property.", false )]
    [EditorBrowsable( EditorBrowsableState.Never )]
    [Browsable( false )]
    public bool IsAddingItem
    {
      get
      {
        return this.IsAddingNew;
      }
    }

    #endregion IsAddingItem Property

    #region RawItemIndexComparer Property

    private static RawItemIndexComparer RawItemIndexComparer
    {
      get
      {
        if( m_rawItemIndexComparer == null )
          m_rawItemIndexComparer = new RawItemIndexComparer();

        return m_rawItemIndexComparer;
      }
    }

    #endregion

    #region RawItemSortComparer Property

    private RawItemSortComparer RawItemSortComparer
    {
      get
      {
        if( m_rawItemSortComparer == null )
          m_rawItemSortComparer = new RawItemSortComparer( this );

        return m_rawItemSortComparer;
      }
    }

    #endregion

    #region Insertion Property

    public bool AllowNew
    {
      get
      {
        return this.CanAddNew;
      }
    }

    #endregion

    #region RootGroup Property

    internal new DataGridCollectionViewGroupRoot RootGroup
    {
      get
      {
        return base.RootGroup as DataGridCollectionViewGroupRoot;
      }
      set
      {
        base.RootGroup = value;
      }
    }

    #endregion

    #region CanHaveDetails Property

    internal override bool CanHaveDetails
    {
      get
      {
        return true;
      }
    }

    #endregion

    #region SortedItemVersion Property

    internal int SortedItemVersion
    {
      get
      {
        return m_sortedItemVersion;
      }
    }

    #endregion

    #region SourceItemCount Property

    internal override int SourceItemCount
    {
      get
      {
        if( m_sourceItemList == null )
          return 0;

        return m_sourceItemList.Count;
      }
    }

    #endregion

    #region SyncRoot Property

    internal override object SyncRoot
    {
      get
      {
        IEnumerable enumeration = this.Enumeration;

        ICollection collection = enumeration as ICollection;

        if( collection != null )
          return collection.SyncRoot;

        if( enumeration != null )
          return enumeration;

        return base.SyncRoot;
      }
    }

    #endregion

    #region RootGroupChanged Internal Event

    internal event EventHandler RootGroupChanged;

    private void OnRootGroupChanged( EventArgs e )
    {
      if( this.RootGroupChanged != null )
        this.RootGroupChanged( this, e );
    }

    #endregion RootGroupChanged Internal Event

    public override bool MoveCurrentToPosition( int position )
    {
      if( position == this.CurrentPosition )
        return ( this.CurrentItem != null );

      this.EnsureThreadAndCollectionLoaded();
      return this.SetCurrentItem( position, true );
    }

    public override void RemoveAt( int index )
    {
      if( !this.CanRemove )
        throw new InvalidOperationException( "An attempt was made to remove an item from a source that does not support removal." );

      RawItem rawItem = this.GetRawItemAt( index );
      int sourceIndex = ( rawItem == null ) ? -1 : rawItem.Index;

      DataGridRemovingItemEventArgs removingItemEventArgs = new DataGridRemovingItemEventArgs( this, null, index, false );
      this.RootDataGridCollectionViewBase.OnRemovingItem( removingItemEventArgs );

      if( removingItemEventArgs.Cancel )
        throw new DataGridException( "RemoveAt was canceled." );

      if( !removingItemEventArgs.Handled )
        this.InternalRemoveAt( sourceIndex );

      if( ( !this.IsSourceSupportingChangeNotification ) && ( sourceIndex != -1 ) )
      {
        DeferredOperation deferredOperation = new DeferredOperation(
          DeferredOperation.DeferredOperationAction.Remove,
          sourceIndex,
          new object[] { rawItem.DataItem } );

        this.ExecuteOrQueueSourceItemOperation( deferredOperation );
      }

      DataGridItemRemovedEventArgs itemRemovedEventArgs = new DataGridItemRemovedEventArgs( this, null, index );
      this.RootDataGridCollectionViewBase.OnItemRemoved( itemRemovedEventArgs );
    }

    public override object GetItemAt( int index )
    {
      this.EnsureThreadAndCollectionLoaded();

      return this.RootGroup.GetRawItemAtGlobalSortedIndex( index ).DataItem;
    }

    public void RefreshDistinctValuesForFieldName( string fieldName )
    {
      this.ForceRefreshDistinctValuesForFieldName( fieldName, this.DistinctValues[ fieldName ].InnerObservableHashList );
    }

    internal override DataGridCollectionViewBase CreateDetailDataGridCollectionViewBase(
      IEnumerable detailDataSource,
      DataGridDetailDescription parentDetailDescription,
      DataGridCollectionViewBase rootDataGridCollectionViewBase )
    {
      Debug.Assert( rootDataGridCollectionViewBase is DataGridCollectionView );

      if( parentDetailDescription != null )
        return new DataGridCollectionView( detailDataSource, parentDetailDescription, rootDataGridCollectionViewBase as DataGridCollectionView );

      return new DataGridCollectionView( detailDataSource );
    }

    internal override void CreateDefaultCollections( DataGridDetailDescription parentDetailDescription )
    {
      base.CreateDefaultCollections( parentDetailDescription );

      m_statFunctions = ( parentDetailDescription == null ) ? new StatFunctionCollection() :
        parentDetailDescription.StatFunctions;
    }

    internal override void RefreshDistinctValuesForField( DataGridItemPropertyBase dataGridItemProperty )
    {
      if( dataGridItemProperty == null )
        return;

      if( !dataGridItemProperty.CalculateDistinctValues )
        return;

      // List containing current column distinct values, we pass the DistinctvaluesEqualityComparer if any. Null will force the HashTable to use the default one
      HashSet<object> currentColumnDistinctValues = new HashSet<object>( new EqualityComparerWrapper( dataGridItemProperty.DistinctValuesEqualityComparer ) );

      ReadOnlyObservableHashList readOnlyColumnDistinctValues = null;

      // If the key is not set in DistinctValues yet, do not calculate distinct values for this field
      if( !( ( DistinctValuesDictionary )this.DistinctValues ).InternalTryGetValue( dataGridItemProperty.Name, out readOnlyColumnDistinctValues ) )
        return;

      ObservableHashList columnDistinctValues = readOnlyColumnDistinctValues.InnerObservableHashList;

      // We use the DistinctValuesSortComparer if present, else the SortComparer for the DataGridItemProperty, else, the Comparer used is the one of the base class.
      IComparer distinctValuesSortComparer = dataGridItemProperty.DistinctValuesSortComparer;

      using( columnDistinctValues.DeferINotifyCollectionChanged() )
      {
        // If none was specified, we use the default SortComparer for the DataGridItemProperty
        if( distinctValuesSortComparer == null )
        {
          distinctValuesSortComparer = dataGridItemProperty.SortComparer;
        }

        int rowsCount = m_sourceItemList.Count; // Parse every rows to get distinct values
        int maximumDistinctValuesCount = dataGridItemProperty.MaxDistinctValues;

        for( int rowIndex = 0; rowIndex < rowsCount; rowIndex++ )
        {
          object rowItem = m_sourceItemList[ rowIndex ].DataItem;

          // If we have more than one filtered item, we need to verify if the row passes every filter except the filtered one to get
          if( this.DistinctValuesConstraint == DistinctValuesConstraint.Filtered )
          {
            if( !this.PassesAutoFilter( rowItem, dataGridItemProperty ) )
              continue;
          }

          object distinctValue = dataGridItemProperty.GetValue( rowItem );

          // Allow the user to provide the corresponding distinct value for this item
          distinctValue = dataGridItemProperty.GetDistinctValueFromItem( distinctValue );

          // Compute current value to be able to remove unused values Accept if -1 => no maximum, or less than maximum specified
          if( ( maximumDistinctValuesCount == -1 ) || ( maximumDistinctValuesCount > currentColumnDistinctValues.Count ) )
          {
            currentColumnDistinctValues.Add( distinctValue );
          }
        }

        DataGridCollectionViewBase.RemoveUnusedDistinctValues( distinctValuesSortComparer, currentColumnDistinctValues, columnDistinctValues, null );
      }
    }

    internal override bool CanCreateItemProperties()
    {
      if( base.CanCreateItemProperties() )
        return true;

      return ( m_sourceItems.Count != 0 );
    }

    internal override bool CanCreateDetailDescriptions()
    {
      if( base.CanCreateDetailDescriptions() )
        return true;

      return ( m_sourceItems.Count != 0 );
    }

    internal override int IndexOfSourceItem( object item )
    {
      if( item != null )
      {
        RawItem rawItem = this.GetFirstRawItemFromDataItem( item );
        if( rawItem != null )
        {
          return rawItem.Index;
        }
      }

      return -1;
    }

    internal override void EnsurePosition( int globalSortedIndex )
    {
      RawItem rawItem = this.GetRawItemAt( globalSortedIndex );

      this.EnsurePosition( rawItem, globalSortedIndex );
    }

    internal override void ExecuteSourceItemOperation( DeferredOperation deferredOperation, out bool refreshForced )
    {
      switch( deferredOperation.Action )
      {
        case DeferredOperation.DeferredOperationAction.Add:
          {
            refreshForced = !this.AddSourceItem(
              deferredOperation.NewStartingIndex,
              deferredOperation.NewItems,
              deferredOperation.NewSourceItemCount );

            break;
          }

        case DeferredOperation.DeferredOperationAction.Move:
          {
            refreshForced = !this.MoveSourceItem(
              deferredOperation.OldStartingIndex,
              deferredOperation.OldItems,
              deferredOperation.NewStartingIndex );

            break;
          }

        case DeferredOperation.DeferredOperationAction.Refresh:
          {
            refreshForced = false;
            this.OnProxyCollectionRefresh();
            this.ForceRefresh( true, false, true );
            break;
          }

        case DeferredOperation.DeferredOperationAction.RefreshDistincValues:
          {
            refreshForced = false;
            if( this.DistinctValuesUpdateMode == DistinctValuesUpdateMode.Manual )
            {
              //Let the AutoFilterControl (or the user) decide when to refresh DistinctValues.
              this.RaiseDistinctValuesRefreshNeeded();
            }
            else
            {
              this.ForceRefreshDistinctValues( deferredOperation.FilteredItemsChanged );
            }
            break;
          }

        case DeferredOperation.DeferredOperationAction.Regroup:
          {
            refreshForced = false;

            // Ensure to defer the CurrentItem change
            // when regrouping is completed to be sure
            // the SaveCurrent has the right values
            // when restoring
            using( this.DeferCurrencyEvent() )
            {
              int oldCurrentPosition = -1;
              RawItem oldCurrentRawItem = null;
              this.SaveCurrentBeforeReset( out oldCurrentRawItem, out oldCurrentPosition );

              SortDescriptionInfo[] sortDescriptionInfos;
              this.GroupItems();
              this.PrepareSort( out sortDescriptionInfos );
              this.SortItems( sortDescriptionInfos );

              // Ensure to send reset notifications
              this.AdjustCurrentAndSendResetNotification( oldCurrentRawItem, oldCurrentPosition );
              this.TriggerRootGroupChanged();
            }

            break;
          }

        case DeferredOperation.DeferredOperationAction.Remove:
          {
            refreshForced = !this.RemoveSourceItem(
              deferredOperation.OldStartingIndex,
              deferredOperation.OldItems.Count );

            break;
          }

        case DeferredOperation.DeferredOperationAction.Replace:
          {
            refreshForced = !this.ReplaceSourceItem(
              deferredOperation.OldStartingIndex,
              deferredOperation.OldItems,
              deferredOperation.NewStartingIndex,
              deferredOperation.NewItems );

            break;
          }

        case DeferredOperation.DeferredOperationAction.Resort:
          {
            refreshForced = false;

            // Ensure to defer the CurrentItem change
            // when resort is completed to be sure
            // the SaveCurrent has the right values
            // when restoring
            using( this.DeferCurrencyEvent() )
            {
              int oldCurrentPosition = -1;
              RawItem oldCurrentRawItem = null;
              this.SaveCurrentBeforeReset( out oldCurrentRawItem, out oldCurrentPosition );

              SortDescriptionInfo[] sortDescriptionInfos;
              this.PrepareSort( out sortDescriptionInfos );
              this.SortItems( sortDescriptionInfos );

              // Ensure to send reset notifications
              this.AdjustCurrentAndSendResetNotification( oldCurrentRawItem, oldCurrentPosition );
              this.TriggerRootGroupChanged();
            }

            break;
          }

        default:
          {
            base.ExecuteSourceItemOperation( deferredOperation, out refreshForced );
            break;
          }
      }
    }

    internal override void ProcessInvalidatedGroupStats( List<DataGridCollectionViewGroup> invalidatedGroups, bool ensurePosition )
    {
      GroupSortComparer[] groupSortComparers = this.GetGroupSortComparers();

      foreach( DataGridCollectionViewGroup group in invalidatedGroups )
      {
        group.InvokeStatFunctionsPropertyChanged( this );

        if( ensurePosition )
        {
          this.EnsurePosition( group, groupSortComparers );
        }
      }

      this.CalculateChangedPropertyStatsOnly = false;
      this.InvalidatedStatFunctions.Clear();
    }

    internal override void ForceRefresh( bool sendResetNotification, bool initialLoad, bool setCurrentToFirstOnInitialLoad )
    {
      if( this.Refreshing )
        throw new InvalidOperationException( "An attempt was made to refresh the DataGridCollectionView while it is already in the process of refreshing." );

      if( this.IsRefreshingDistinctValues )
        throw new InvalidOperationException( "An attempt was made to refresh the DataGridCollectionView while it is already in the process of refreshing distinct values." );

      this.SetCurrentEditItem( null );
      int oldCurrentPosition = -1;
      RawItem oldCurrentRawItem = null;

      // The cache of PropertyDescriptors for the StatFunctions will be recreated if 
      // needed in GetStatFunctionProperties().
      m_statisticalProperties = null;

      if( !initialLoad )
        this.SaveCurrentBeforeReset( out oldCurrentRawItem, out oldCurrentPosition );

      SortDescriptionInfo[] sortDescriptionInfos = null;

      using( this.DeferCurrencyEvent() )
      {
        this.Refreshing = true;

        try
        {
          lock( this.SyncRoot )
          {
            DeferredOperationManager deferredOperationManager = this.DeferredOperationManager;

            lock( deferredOperationManager )
            {
              deferredOperationManager.ClearDeferredOperations();
              m_lastAddCount = -1;
              bool bLoaded = false;

              IEnumerable enumeration = this.Enumeration;
              CollectionView collectionView = enumeration as CollectionView;

              if( collectionView == null )
              {
                IList list = enumeration as IList;

                if( list != null )
                {
                  int count = list.Count;
                  m_sourceItemList = new List<RawItem>( count );
                  m_dataItemToRawItemList = new Dictionary<object, object>();
                  m_filteredItemList = new List<RawItem>( count );

                  for( int i = 0; i < count; i++ )
                  {
                    object item = list[ i ];
                    RawItem rawItem = new RawItem( i, item );
                    m_sourceItemList.Add( rawItem );
                    this.AddRawItemDataItemMapping( rawItem );

                    if( this.PassesFilter( item )
                        && this.PassesAutoFilter( item, null )
                        && this.PassesFilterCriterion( rawItem.DataItem ) )
                    {
                      m_filteredItemList.Add( rawItem );
                    }
                  }

                  bLoaded = true;
                }
              }

              if( !bLoaded )
              {
                if( enumeration != null )
                {
                  int count = 128;

                  if( collectionView != null )
                  {
                    count = collectionView.Count;
                  }
                  else
                  {
                    ICollection collection = enumeration as ICollection;

                    if( collection != null )
                      count = collection.Count;
                  }

                  m_sourceItemList = new List<RawItem>( count );
                  m_dataItemToRawItemList = new Dictionary<object, object>();
                  m_filteredItemList = new List<RawItem>( count );

                  int index = 0;
                  IEnumerator enumerator = enumeration.GetEnumerator();

                  while( enumerator.MoveNext() )
                  {
                    object item = enumerator.Current;
                    RawItem rawItem = new RawItem( index, item );
                    index++;
                    m_sourceItemList.Add( rawItem );
                    this.AddRawItemDataItemMapping( rawItem );

                    if( this.PassesFilter( item )
                        && this.PassesAutoFilter( item, null )
                        && this.PassesFilterCriterion( rawItem.DataItem ) )
                    {
                      m_filteredItemList.Add( rawItem );
                    }
                  }
                }
                else
                {
                  if( m_sourceItemList == null )
                    m_sourceItemList = new List<RawItem>( 128 );

                  if( m_dataItemToRawItemList == null )
                    m_dataItemToRawItemList = new Dictionary<object, object>();

                  if( m_filteredItemList == null )
                    m_filteredItemList = new List<RawItem>( 128 );
                }
              }

              m_filteredItemList.TrimExcess();

              // We set the current item to -1 to prevent the developper to get an invalid position.
              // We will replace the current item to the correct one later.
              this.SetCurrentItem( -1, null, false, false );
              this.GroupItems();

              // This will cache the data for the sorting
              this.PrepareSort( out sortDescriptionInfos );
            }
          }

          if( this.DistinctValuesUpdateMode == DistinctValuesUpdateMode.Manual )
          {
            //Let the AutoFilterControl (or the user) decide when to refresh DistinctValues.
            this.RaiseDistinctValuesRefreshNeeded();
          }
          else
          {
            this.ForceRefreshDistinctValues( true );
          }

          this.SortItems( sortDescriptionInfos );

          unchecked
          {
            m_sortedItemVersion++;
          }
        }
        finally
        {
          this.Refreshing = false;
        }

        if( initialLoad )
        {
          this.Loaded = true;
          this.AdjustCurrencyAfterInitialLoad( setCurrentToFirstOnInitialLoad );
        }
        else
        {
          this.AdjustCurrencyAfterReset( oldCurrentRawItem, oldCurrentPosition, true );
        }

        if( sendResetNotification )
        {
          this.OnCollectionChanged( new NotifyCollectionChangedEventArgs(
            NotifyCollectionChangedAction.Reset ) );
        }

        this.TriggerRootGroupChanged();
      }
    }

    internal RawItem GetRawItemAt( int index )
    {
      this.EnsureThreadAndCollectionLoaded();
      return this.RootGroup.GetRawItemAtGlobalSortedIndex( index );
    }

    internal object GetSourceItemAt( int index )
    {
      if( m_sourceItemList == null )
        throw new ArgumentOutOfRangeException( "index", index, "index must be greater than zero and less than SourceItems.Count." );

      return m_sourceItemList[ index ].DataItem;
    }

    internal IEnumerator<RawItem> GetSourceListEnumerator()
    {
      if( m_sourceItemList == null )
        return new List<RawItem>().GetEnumerator();

      return m_sourceItemList.GetEnumerator();
    }

    internal PropertyDescriptorCollection GetStatFunctionProperties()
    {
      if( m_statisticalProperties == null )
      {
        m_statisticalProperties = new PropertyDescriptorCollection( null );

        foreach( Stats.StatFunction statFunction in m_statFunctions )
        {
          m_statisticalProperties.Add( new StatFunctionPropertyDescriptor( statFunction.ResultPropertyName ) );
        }
      }

      return m_statisticalProperties;
    }

    internal bool AddSourceItem( int startIndex, IList items, int newSourceItemCount )
    {
      if( items == null )
        throw new ArgumentNullException( "items" );

      if( ( startIndex < 0 ) || ( startIndex > m_sourceItemList.Count ) )
      {
        this.ForceRefresh( true, !this.Loaded, true );
        return false;
      }

      if( ( newSourceItemCount != -1 ) && ( newSourceItemCount == m_lastAddCount ) )
      {
        // In that case, we are reciving a second add to confirm an insertion of a previous item.
        // We will convert this add into a replace.
        return this.ReplaceSourceItem( m_lastAddIndex, items, m_lastAddIndex, items );
      }

      m_lastAddCount = newSourceItemCount;
      m_lastAddIndex = startIndex;

      int count = items.Count;
      List<RawItem> filteredItems = new List<RawItem>( count );
      RawItem[] rawItems = new RawItem[ count ];

      for( int i = 0; i < count; i++ )
      {
        object item = items[ i ];
        RawItem rawItem = new RawItem( startIndex + i, item );
        rawItems[ i ] = rawItem;

        if( this.PassesFilter( rawItem.DataItem )
            && this.PassesAutoFilter( rawItem.DataItem, null )
            && this.PassesFilterCriterion( rawItem.DataItem ) )
        {
          filteredItems.Add( rawItem );
        }
      }

      bool isLast = ( startIndex == this.SourceItemCount );
      this.AddRawItemInSourceList( startIndex, rawItems );

      if( filteredItems.Count != 0 )
      {
        this.AddRawItemInFilteredList( filteredItems, isLast );
        this.AddRawItemInGroup( filteredItems );

        DeferredOperationManager deferredOperationManager = this.DeferredOperationManager;
        foreach( RawItem rawItem in filteredItems )
        {
          deferredOperationManager.InvalidateGroupStats( rawItem.ParentGroup );
        }
      }

      this.RefreshDistinctValues( filteredItems.Count > 0 );
      return true;
    }

    internal bool RemoveSourceItem( int startIndex, int count )
    {
      m_lastAddCount = -1;

      if( ( startIndex < 0 ) || ( ( startIndex + count ) > m_sourceItemList.Count ) )
      {
        this.ForceRefresh( true, !this.Loaded, true );
        return false;
      }

      RawItem rawItem;
      RawItem[] removedItems = new RawItem[ count ];

      for( int i = 0; i < count; i++ )
      {
        rawItem = m_sourceItemList[ i + startIndex ];

        if( this.CurrentEditItem == rawItem.DataItem )
          this.SetCurrentEditItem( null );

        removedItems[ i ] = rawItem;
        this.DeferredOperationManager.InvalidateGroupStats( rawItem.ParentGroup );
      }

      int filteredItemRemovedCount = this.RemoveRawItemInFilteredList( removedItems );

      // We do the raw item list after cleaning up the filtered list to delay the resequencing of the RawItem.
      this.RemoveRawItemInSourceList( startIndex, count );

      if( filteredItemRemovedCount > 0 )
        this.RemoveRawItemInGroup( removedItems );

      this.RefreshDistinctValues( filteredItemRemovedCount > 0 );
      return true;
    }

    internal bool ReplaceSourceItem( int oldStartIndex, IList oldItems, int newStartIndex, IList newItems )
    {
      m_lastAddCount = -1;

      if( ( oldStartIndex < 0 ) || ( newStartIndex < 0 ) )
      {
        this.ForceRefresh( true, !this.Loaded, true );
        return false;
      }

      int oldItemCount = oldItems.Count;
      int newItemCount = newItems.Count;
      int sourceItemListCount = m_sourceItemList.Count;

      if( ( oldStartIndex + oldItemCount ) > sourceItemListCount
        || ( newStartIndex + newItemCount ) > sourceItemListCount )
      {
        this.ForceRefresh( true, !this.Loaded, true );
        return false;
      }

      int extraOldItemCount = oldItemCount - newItemCount;

      int count = Math.Min( newItemCount, oldItemCount );

      using( this.DeferCurrencyEvent() )
      {
        for( int i = 0; i < count; i++ )
        {
          if( oldStartIndex == newStartIndex )
          {
            RawItem rawItem = m_sourceItemList[ oldStartIndex + i ];
            object oldItem = rawItem.DataItem;
            int globalSortedIndex = rawItem.GetGlobalSortedIndex();

            // globalSortedIndex == -1 means item does not pass filter
            if( globalSortedIndex != -1 )
            {
              bool itemChanged = !object.Equals( oldItem, newItems[ i ] );

              if( ( this.CurrentEditItem == oldItem ) && ( itemChanged ) )
              {
                try
                {
                  this.CancelEdit();
                }
                catch
                {
                }
              }

              if( this.CurrentEditItem != oldItem )
              {
                object newItem = newItems[ i ];
                DeferredOperationManager deferredOperationManager = this.DeferredOperationManager;

                if( newItem != rawItem.DataItem )
                {
                  this.RemoveRawItemDataItemMapping( rawItem );
                  rawItem.SetDataItem( newItem );
                  this.AddRawItemDataItemMapping( rawItem );

                  this.AdjustCurrencyBeforeReplace( globalSortedIndex, newItem );
                }

                DataGridCollectionViewGroup oldGroup = rawItem.ParentGroup;

                // Even if the newItem == oldItem, we want to set rawItem.ParentGroup.ProtectedItems[] for the change event to be raised.
                if( ( oldGroup != null ) && ( oldGroup != this.RootGroup ) )
                {
                  oldGroup.ProtectedItems[ rawItem.SortedIndex ] = newItem;
                }

                this.OnCollectionChanged( new NotifyCollectionChangedEventArgs(
                  NotifyCollectionChangedAction.Replace, newItem, oldItem, rawItem.GetGlobalSortedIndex() ) );

                deferredOperationManager.InvalidateGroupStats( oldGroup );
                this.EnsurePosition( rawItem, globalSortedIndex );
                deferredOperationManager.InvalidateGroupStats( rawItem.ParentGroup );
              }
            }
            else
            {
              // Since it is a replace, we must re-ensure position even if current item did not passed filter
              // in case a properperty change would let it pass the filter
              this.DeferredOperationManager.InvalidateGroupStats( rawItem.ParentGroup );
              this.EnsurePosition( rawItem, globalSortedIndex );
              this.DeferredOperationManager.InvalidateGroupStats( rawItem.ParentGroup );
            }
          }
          else
          {
            Debug.Fail( "Replace should have an oldStartIndex equal to newStartIndex." );
            this.RemoveSourceItem( oldStartIndex + i, 1 );
            this.AddSourceItem( newStartIndex + i, new object[] { newItems[ i ] }, -1 );
          }
        }

        if( extraOldItemCount > 0 )
          this.RemoveSourceItem( oldStartIndex + newItemCount, extraOldItemCount );

        int extraNewItemCount = newItemCount - oldItemCount;

        if( extraNewItemCount > 0 )
        {
          object[] tempItems = new object[ extraNewItemCount ];

          for( int i = 0; i < extraNewItemCount; i++ )
          {
            tempItems[ i ] = newItems[ oldItemCount + i ];
          }

          this.AddSourceItem( newStartIndex + oldItemCount, tempItems, -1 );
        }
      }

      return true;
    }

    internal RawItem GetFirstRawItemFromDataItem( object dataItem )
    {
      this.EnsureThreadAndCollectionLoaded();

      object cached;

      if( ( m_dataItemToRawItemList != null )
        && ( m_dataItemToRawItemList.TryGetValue( dataItem, out cached ) ) )
      {
        RawItem temp = cached as RawItem;

        if( cached != null )
          return temp;

        return ( ( RawItem[] )cached )[ 0 ];
      }

      return null;
    }

    private static DataStore CreateStore( Type dataType, int initialCapacity )
    {
      if( dataType == typeof( bool ) )
      {
        return new BoolDataStore( initialCapacity );
      }
      else if( dataType == typeof( byte ) )
      {
        return new ValueTypeDataStore<byte>( initialCapacity );
      }
      else if( dataType == typeof( char ) )
      {
        return new ValueTypeDataStore<char>( initialCapacity );
      }
      else if( dataType == typeof( DateTime ) )
      {
        return new ValueTypeDataStore<DateTime>( initialCapacity );
      }
      else if( dataType == typeof( decimal ) )
      {
        return new ValueTypeDataStore<decimal>( initialCapacity );
      }
      else if( dataType == typeof( double ) )
      {
        return new ValueTypeDataStore<double>( initialCapacity );
      }
      else if( dataType == typeof( Guid ) )
      {
        return new ValueTypeDataStore<Guid>( initialCapacity );
      }
      else if( dataType == typeof( short ) )
      {
        return new ValueTypeDataStore<short>( initialCapacity );
      }
      else if( dataType == typeof( int ) )
      {
        return new ValueTypeDataStore<int>( initialCapacity );
      }
      else if( dataType == typeof( long ) )
      {
        return new ValueTypeDataStore<long>( initialCapacity );
      }
      else if( dataType == typeof( sbyte ) )
      {
        return new ValueTypeDataStore<sbyte>( initialCapacity );
      }
      else if( dataType == typeof( float ) )
      {
        return new ValueTypeDataStore<float>( initialCapacity );
      }
      else if( dataType == typeof( string ) )
      {
        return new StringDataStore( initialCapacity );
      }
      else if( dataType == typeof( TimeSpan ) )
      {
        return new ValueTypeDataStore<TimeSpan>( initialCapacity );
      }
      else if( dataType == typeof( ushort ) )
      {
        return new ValueTypeDataStore<ushort>( initialCapacity );
      }
      else if( dataType == typeof( uint ) )
      {
        return new ValueTypeDataStore<uint>( initialCapacity );
      }
      else if( dataType == typeof( ulong ) )
      {
        return new ValueTypeDataStore<ulong>( initialCapacity );
      }
      else
      {
        return new ObjectDataStore( initialCapacity );
      }
    }

    private void EnsurePosition( RawItem rawItem, int globalSortedIndex )
    {
      Debug.Assert( rawItem.GetGlobalSortedIndex() == globalSortedIndex );

      if( this.PassesFilter( rawItem.DataItem )
        && this.PassesAutoFilter( rawItem.DataItem, null )
        && this.PassesFilterCriterion( rawItem.DataItem ) )
      {
        if( globalSortedIndex == -1 )
        {
          RawItem[] rawItems = new RawItem[] { rawItem };
          this.AddRawItemInFilteredList( rawItems, false );
          this.AddRawItemInGroup( rawItems );

          this.RefreshDistinctValues( true );
          return;
        }
        else
        {
          this.RefreshDistinctValues( true );
        }
      }
      else
      {
        if( globalSortedIndex != -1 )
        {
          RawItem[] rawItems = new RawItem[] { rawItem };
          this.RemoveRawItemInFilteredList( rawItems );
          this.RemoveRawItemInGroup( rawItems );
        }

        this.RefreshDistinctValues( true );
        return;
      }

      // Verify the row is in the correct group.
      DataGridCollectionViewGroup newGroup = this.GetRawItemNewGroup( rawItem );

      if( rawItem.ParentGroup != newGroup )
      {
        using( this.DeferCurrencyEvent() )
        {
          DeferredOperationManager deferredOperationManager = this.DeferredOperationManager;
          deferredOperationManager.InvalidateGroupStats( rawItem.ParentGroup );
          deferredOperationManager.InvalidateGroupStats( newGroup );

          int newSortIndex = newGroup.BinarySearchRawItem( rawItem, this.RawItemSortComparer );

          if( newSortIndex < 0 )
          {
            newSortIndex = ~newSortIndex;
          }

          rawItem.ParentGroup.RemoveRawItemAt( rawItem.SortedIndex );
          newGroup.InsertRawItem( newSortIndex, rawItem );
          int newGlobalSortedIndex = rawItem.GetGlobalSortedIndex();
          this.AdjustCurrencyAfterMove( globalSortedIndex, newGlobalSortedIndex, 1 );
          this.OnCollectionChanged( new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Move, rawItem.DataItem, newGlobalSortedIndex, globalSortedIndex ) );
        }
      }
      else
      {
        // Verify sorting only if we have not changed group, since adding an item in a group will automatically sort it.
        // Even if we are not sorted, we must ensure the order matches the natural order of the source.
        int newSortIndex = newGroup.BinarySearchRawItem( rawItem, this.RawItemSortComparer );

        if( newSortIndex < 0 )
          newSortIndex = ~newSortIndex;

        if( newSortIndex > rawItem.SortedIndex )
          newSortIndex--;

        if( rawItem.SortedIndex != newSortIndex )
        {
          using( this.DeferCurrencyEvent() )
          {
            newGroup.MoveRawItem( rawItem.SortedIndex, newSortIndex );
            int newGlobalSortedIndex = rawItem.GetGlobalSortedIndex();
            this.AdjustCurrencyAfterMove( globalSortedIndex, newGlobalSortedIndex, 1 );
            this.OnCollectionChanged( new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Move, rawItem.DataItem, newGlobalSortedIndex, globalSortedIndex ) );
          }
        }
      }
    }

    private void EnsurePosition( DataGridCollectionViewGroup group, GroupSortComparer[] groupSortComparers )
    {
      DataGridCollectionViewGroup parentGroup = group.Parent;

      if( parentGroup == null )
        return;

      int oldSortIndex = parentGroup.ProtectedItems.IndexOf( group );

      // If the group have been removed, we have nothing to do here.
      if( oldSortIndex == -1 )
        return;

      DataGridCollectionViewGroup currentGroup = parentGroup.Parent;
      int groupLevel = 0;

      while( currentGroup != null )
      {
        groupLevel++;
        currentGroup = currentGroup.Parent;
      }

      int newSortIndex = parentGroup.BinarySearchGroup( group, groupSortComparers[ groupLevel ] );

      if( newSortIndex < 0 )
        newSortIndex = ~newSortIndex;

      if( newSortIndex > oldSortIndex )
        newSortIndex--;

      if( oldSortIndex != newSortIndex )
      {
        using( this.DeferCurrencyEvent() )
        {
          int oldGlobalSortedIndex = group.GetFirstRawItemGlobalSortedIndex();
          parentGroup.ProtectedItems.Move( oldSortIndex, newSortIndex );
          int newGlobalSortedIndex = group.GetFirstRawItemGlobalSortedIndex();

          int itemCount = group.GlobalRawItemCount;
          this.AdjustCurrencyAfterMove( oldGlobalSortedIndex, newGlobalSortedIndex, itemCount );

          List<object> items = new List<object>( 256 );
          group.GetGlobalItems( items );

          this.OnCollectionChanged( new NotifyCollectionChangedEventArgs(
            NotifyCollectionChangedAction.Move, items, newGlobalSortedIndex, oldGlobalSortedIndex ) );
        }
      }
    }

    // excludedFilteredDataGridItemProperty is used to avoid filtering the column on which we are calculating Distinct Values if null, this parameter is ignored
    private bool PassesAutoFilter( object item, DataGridItemPropertyBase excludedFilteredDataGridItemProperty )
    {
      AutoFilterMode autoFilterMode = this.AutoFilterMode;

      if( autoFilterMode == AutoFilterMode.None )
        return true;

      DistinctValuesConstraint distinctValuesConstraint = this.DistinctValuesConstraint;
      bool isRowAccepted = false;
      bool isColumnFiltered = false;

      switch( autoFilterMode )
      {
        case AutoFilterMode.And:
          // Initially, we accept the row and reject on any filter difference
          isRowAccepted = true;
          break;

        case AutoFilterMode.Or:
          // Initially, no criteria is met, we reject the row
          isRowAccepted = false;
          break;
      }

      ObservableCollection<DataGridItemPropertyBase> autoFilteItems = this.AutoFilteredDataGridItemProperties;
      int autoFilterItemsCount = autoFilteItems.Count;

      // The index of the last filtered item to calculate DistinctValuesConstraint.Filtered
      int calculateDistinctValuesIndex = autoFilterItemsCount - 1; // Count for last item

      for( int propertyIndex = 0; propertyIndex < autoFilterItemsCount; propertyIndex++ )
      {
        DataGridItemPropertyBase dataGridItemProperty = autoFilteItems[ propertyIndex ];

        // If the excluded DataGridItemProperty is found
        if( ( excludedFilteredDataGridItemProperty != null ) && ( dataGridItemProperty == excludedFilteredDataGridItemProperty ) )
          continue;

        // At least one column was filtered
        isColumnFiltered = true;

        bool isCurrentFilterAccepted = false;

        // autoFilterValues will never be null
        IList autoFilterValues = this.GetAutoFilterValues( dataGridItemProperty.Name );
        int filterCount = autoFilterValues.Count;

        if( filterCount > 0 )
        {
          // Only get value if there are autoFilterValues for this dataGridItemProperty
          object columnValue = dataGridItemProperty.GetValue( item );

          // Allow the user to provide the corresponding distinct value for this item
          columnValue = dataGridItemProperty.GetDistinctValueFromItem( columnValue );

          // Check if one of the items in filter list passes
          for( int filterIndex = 0; filterIndex < filterCount; filterIndex++ )
          {
            object autoFilterValue = autoFilterValues[ filterIndex ];

            // Use the DistinctValuesEqualityComparer for this DataGridItemProperty if specified
            IEqualityComparer distinctValuesEqualityComparer = dataGridItemProperty.DistinctValuesEqualityComparer;

            if( distinctValuesEqualityComparer != null )
            {
              if( distinctValuesEqualityComparer.Equals( autoFilterValue, columnValue ) )
              {
                // Current column correspond to Filter value
                isCurrentFilterAccepted = true;
                break;
              }
            }
            else if( ItemsSourceHelper.IsEntityFramework( autoFilterValue ) && ItemsSourceHelper.IsEntityFramework( columnValue ) )
            {
              if( ( autoFilterValue is IComparable ) && ( columnValue is IComparable ) )
              {
                IComparable autoFilterValueComparable = ( IComparable )autoFilterValue;
                IComparable columnValueComparable = ( IComparable )columnValue;

                if( autoFilterValueComparable.CompareTo( columnValueComparable ) == 0 )
                {
                  isCurrentFilterAccepted = true;
                  break;
                }
              }
              else
              {
                if( Object.Equals( autoFilterValue, columnValue ) )
                {
                  isCurrentFilterAccepted = true;
                  break;
                }
              }
            }
            else if( ObjectDataStore.CompareData( autoFilterValue, columnValue ) == 0 )
            {
              // Current column correspond to Filter value
              isCurrentFilterAccepted = true;
              break;
            }
          }
        }

        if( isCurrentFilterAccepted )
        {
          isRowAccepted = true;

          // In FilteringMode.Or, accept the row right away, do not process other columns
          if( autoFilterMode == AutoFilterMode.Or )
            break;
        }
        else
        {
          // One filtered column doesn't correspond to Filter value
          if( autoFilterMode == AutoFilterMode.And )
          {
            isRowAccepted = false;
            break;
          }
        }
      }

      // If no filter provided for any column, accept the row
      if( !isColumnFiltered )
        isRowAccepted = true;

      return isRowAccepted;
    }

    private bool PassesFilterCriterion( object item )
    {
      FilterCriteriaMode filterMode = this.FilterCriteriaMode;
      bool passesFilter = true;

      if( filterMode != FilterCriteriaMode.None )
      {
        ReadOnlyCollection<DataGridItemPropertyBase> itemProperties =
          this.FilteredCriterionItemProperties;

        int itemPropertyCount = itemProperties.Count;

        for( int i = 0; i < itemPropertyCount; i++ )
        {
          DataGridItemPropertyBase itemProperty = itemProperties[ i ];
          Debug.Assert( itemProperty.FilterCriterion != null );

          if( itemProperty.FilterCriterion == null )
            continue;

          passesFilter = itemProperty.FilterCriterion.IsMatch( itemProperty.GetValue( item ) );

          if( filterMode == FilterCriteriaMode.And )
          {
            if( !passesFilter )
              break;
          }
          else
          {
            if( passesFilter )
              break;
          }
        }
      }

      return passesFilter;
    }

    private bool SetCurrentItem( int newCurrentPosition, bool isCancelable )
    {
      DataGridCollectionViewGroupRoot rootGroup = this.RootGroup;
      int itemCount = rootGroup.GlobalRawItemCount;

      if( ( newCurrentPosition < -1 ) || ( newCurrentPosition > itemCount ) )
        throw new ArgumentOutOfRangeException( "newCurrentPosition", "The current position must be greater than -1 and less than Count." );

      // When we have no items, the current item/position must not be changed
      // We have done that to get a behavior like the microsoft BindingListCollectionView
      if( itemCount == 0 )
        return false;

      object newCurrentItem = null;

      if( ( newCurrentPosition >= 0 ) && ( newCurrentPosition < itemCount ) )
      {
        RawItem newCurrentRawItem = rootGroup.GetRawItemAtGlobalSortedIndex( newCurrentPosition );

        if( newCurrentRawItem != null )
          newCurrentItem = newCurrentRawItem.DataItem;
      }

      return this.SetCurrentItem( newCurrentPosition, newCurrentItem, isCancelable, false );
    }

    private bool SetCurrentItem( int newCurrentPosition, object newCurrentItem, bool isCancelable, bool beforeDeleteOperation )
    {
      object oldCurrentItem = this.CurrentItem;
      int oldCurrentPosition = this.CurrentPosition;
      bool oldIsCurrentBeforeFirst = this.IsCurrentBeforeFirst;
      bool oldIsCurrentAfterLast = this.IsCurrentAfterLast;

      if( ( !object.Equals( oldCurrentItem, newCurrentItem ) ) || ( oldCurrentPosition != newCurrentPosition ) )
      {
        // We raise the changing event even if we are in DeferCurrencyEvent
        CurrentChangingEventArgs currentChangingEventArgs = new CurrentChangingEventArgs( isCancelable );
        this.OnCurrentChanging( currentChangingEventArgs );

        if( ( !currentChangingEventArgs.Cancel ) || ( !currentChangingEventArgs.IsCancelable ) )
        {
          int globalRawItemCount = this.RootGroup.GlobalRawItemCount;

          if( beforeDeleteOperation )
          {
            Debug.Assert( globalRawItemCount > 0 );
            globalRawItemCount--;
          }

          bool isCurrentBeforeFirst;
          bool isCurrentAfterLast;

          if( globalRawItemCount == 0 )
          {
            isCurrentBeforeFirst = true;
            isCurrentAfterLast = true;
          }
          else
          {
            isCurrentBeforeFirst = newCurrentPosition < 0;
            isCurrentAfterLast = newCurrentPosition >= globalRawItemCount;
          }

#if DEBUG
          if( newCurrentItem == null )
            Debug.Assert( ( newCurrentPosition == -1 ) || ( newCurrentPosition >= ( globalRawItemCount - 1 ) ) );
#endif

          this.SetCurrentItemAndPositionCore(
            newCurrentItem, newCurrentPosition, isCurrentBeforeFirst, isCurrentAfterLast );

          if( !this.IsCurrencyDeferred )
          {
            if( !object.Equals( oldCurrentItem, newCurrentItem ) )
              this.OnPropertyChanged( new PropertyChangedEventArgs( "CurrentItem" ) );

            if( oldCurrentPosition != this.CurrentPosition )
              this.OnPropertyChanged( new PropertyChangedEventArgs( "CurrentPosition" ) );

            if( oldIsCurrentBeforeFirst != this.IsCurrentBeforeFirst )
              this.OnPropertyChanged( new PropertyChangedEventArgs( "IsCurrentBeforeFirst" ) );

            if( oldIsCurrentAfterLast != this.IsCurrentAfterLast )
              this.OnPropertyChanged( new PropertyChangedEventArgs( "IsCurrentAfterLast" ) );

            this.OnCurrentChanged();
          }
        }
      }

      return ( newCurrentItem != null );
    }

    private bool MoveSourceItem( int oldStartIndex, IList items, int newStartIndex )
    {
      int count = items.Count;

      if( ( oldStartIndex < 0 ) || ( oldStartIndex + count > m_sourceItemList.Count )
        || ( newStartIndex < 0 ) || ( newStartIndex > ( m_sourceItemList.Count - count ) ) )
      {
        this.ForceRefresh( true, !this.Loaded, true );
        return false;
      }

      m_lastAddCount = -1;
      RawItem[] rawItems = new RawItem[ count ];
      List<RawItem> filteredRawItems = new List<RawItem>( count );

      for( int i = 0; i < count; i++ )
      {
        RawItem rawItem = m_sourceItemList[ oldStartIndex + i ];
        rawItems[ i ] = rawItem;

        // If our parent group is null, we are filtered out.
        if( rawItem.ParentGroup != null )
          filteredRawItems.Add( rawItem );
      }

      int filteredItemCount = this.RemoveRawItemInFilteredList( rawItems );

      Debug.Assert( filteredItemCount == filteredRawItems.Count );

      m_sourceItemList.RemoveRange( oldStartIndex, count );
      m_sourceItemList.InsertRange( newStartIndex, rawItems );

      int startIndex = Math.Min( oldStartIndex, newStartIndex );
      int endIndex = Math.Max( oldStartIndex, newStartIndex ) + count;

      for( int i = startIndex; i < endIndex; i++ )
      {
        m_sourceItemList[ i ].SetIndex( i );
      }

      filteredItemCount = filteredRawItems.Count;

      if( filteredItemCount > 0 )
      {
        this.AddRawItemInFilteredList( filteredRawItems, false );

        for( int i = 0; i < filteredItemCount; i++ )
        {
          RawItem rawItem = filteredRawItems[ i ];
          this.EnsurePosition( rawItem, rawItem.GetGlobalSortedIndex() );
        }
      }

      return true;
    }

    private void AddRawItemInSourceList( int startIndex, IList<RawItem> rawItems )
    {
      m_sourceItemList.InsertRange( startIndex, rawItems );

      foreach( RawItem rawItem in rawItems )
      {
        this.AddRawItemDataItemMapping( rawItem );
      }

      int count = m_sourceItemList.Count;

      for( int i = startIndex + rawItems.Count; i < count; i++ )
      {
        m_sourceItemList[ i ].SetIndex( i );
      }
    }

    private void AddRawItemInFilteredList( IList<RawItem> rawItems, bool isLast )
    {
      // The function take for granted that all RawItem's index are sequential,
      // or if there is gap, the index contained in the gap are not already contained in the list.

      int index;

      if( isLast )
      {
        index = m_filteredItemList.Count;
      }
      else
      {
        index = m_filteredItemList.BinarySearch(
          rawItems[ 0 ], DataGridCollectionView.RawItemIndexComparer );

        Debug.Assert( index < 0 );

        if( index < 0 )
          index = ~index;

        Debug.Assert( index <= m_filteredItemList.Count );
      }

      m_filteredItemList.InsertRange( index, rawItems );
    }

    private void AddRawItemInGroup( IList<RawItem> rawItems )
    {
      int count = rawItems.Count;

      for( int i = 0; i < count; i++ )
      {
        this.AddRawItemInGroup( rawItems[ i ] );
      }
    }

    private void AddRawItemInGroup( RawItem rawItem )
    {
      using( this.DeferCurrencyEvent() )
      {
        DataGridCollectionViewGroup newGroup = this.GetRawItemNewGroup( rawItem );
        int index = newGroup.BinarySearchRawItem( rawItem, this.RawItemSortComparer );

        if( index < 0 )
          index = ~index;

        int globalIndex = newGroup.GetFirstRawItemGlobalSortedIndex() + index;
        this.AdjustCurrencyBeforeAdd( globalIndex );
        newGroup.InsertRawItem( index, rawItem );

        unchecked
        {
          m_sortedItemVersion++;
        }

        this.OnCollectionChanged( new NotifyCollectionChangedEventArgs(
          NotifyCollectionChangedAction.Add, rawItem.DataItem, globalIndex ) );
      }
    }

    private void RemoveRawItemInSourceList( int startIndex, int count )
    {
      int endIndex = startIndex + count - 1;
      for( int i = startIndex; i <= endIndex; i++ )
      {
        this.RemoveRawItemDataItemMapping( m_sourceItemList[ i ] );
      }

      m_sourceItemList.RemoveRange( startIndex, count );
      int totalCount = m_sourceItemList.Count;

      for( int i = startIndex; i < totalCount; i++ )
      {
        m_sourceItemList[ i ].SetIndex( i );
      }
    }

    private int RemoveRawItemInFilteredList( IList<RawItem> rawItems )
    {
      // The function take for granted that all RawItem's index are sequential,
      // There should not be any gap in the index sequence.

      int index = m_filteredItemList.BinarySearch(
        rawItems[ 0 ], DataGridCollectionView.RawItemIndexComparer );

      if( index < 0 )
        index = ~index;

      if( index > m_filteredItemList.Count )
        return 0;

      int lastRawItemIndex = rawItems[ rawItems.Count - 1 ].Index;

      int count = m_filteredItemList.Count;
      int countToRemove = 0;

      for( int i = index; i < count; i++ )
      {
        if( m_filteredItemList[ i ].Index <= lastRawItemIndex )
          countToRemove++;
      }

      if( countToRemove > 0 )
        m_filteredItemList.RemoveRange( index, countToRemove );

      return countToRemove;
    }

    private void RemoveRawItemInGroup( IList<RawItem> rawItems )
    {
      int count = rawItems.Count;

      for( int i = 0; i < count; i++ )
      {
        RawItem oldRawItem = rawItems[ i ];
        DataGridCollectionViewGroup parentGroup = oldRawItem.ParentGroup;

        if( parentGroup != null )
        {
          int globalSortedIndex = oldRawItem.GetGlobalSortedIndex();

          using( this.DeferCurrencyEvent() )
          {
            this.AdjustCurrencyBeforeRemove( globalSortedIndex );
            parentGroup.RemoveRawItemAt( oldRawItem.SortedIndex );

            unchecked
            {
              m_sortedItemVersion++;
            }
          }

          // In the case of a remove, the CollectionChanged must be after the CurrentChanged
          // since when the DataGridCollectionView is used with a Selector having the 
          // IsSynchronizedWithCurrent set, the Selector will sett the current position to 
          // -1 if the selected item is removed. 
          this.OnCollectionChanged( new NotifyCollectionChangedEventArgs(
            NotifyCollectionChangedAction.Remove, oldRawItem.DataItem, globalSortedIndex ) );
        }
      }
    }

    private void GroupItems()
    {
      // We set the current item to -1 to prevent the developper to get an invalid position.
      // We will replace the current item to the correct one later.
      this.SetCurrentItem( -1, null, false, false );

      // We use a new DataGridCollectionViewGroupRoot instead of doing a clear to prevent
      // some event to be raised.  And it should also be faster.
      this.RootGroup = new DataGridCollectionViewGroupRoot( this );

      ObservableCollection<GroupDescription> groupDescriptions = this.GroupDescriptions;

      int groupDescriptionCount = groupDescriptions.Count;
      int itemCount = m_filteredItemList.Count;

      DataGridCollectionViewGroupRoot rootGroup = this.RootGroup;

      if( groupDescriptionCount > 0 )
      {
        CultureInfo culture = this.Culture;

        for( int i = 0; i < groupDescriptionCount; i++ )
        {
          DataGridGroupDescription groupDescription = groupDescriptions[ i ] as DataGridGroupDescription;

          if( groupDescription != null )
            groupDescription.SetContext( this );
        }

        try
        {
          rootGroup.SetSubGroupBy( groupDescriptions[ 0 ] );
          rootGroup.CreateFixedGroupNames( 0, groupDescriptions, null );

          for( int i = 0; i < itemCount; i++ )
          {
            RawItem rawItem = m_filteredItemList[ i ];
            DataGridCollectionViewGroup currentGroup = rootGroup;

            for( int j = 1; j <= groupDescriptionCount; j++ )
            {
              // We set SortComparers to null to avoid Sorting since we are in the middle of a "batch operation"
              // The sorting will be done after the batch operation which is faster than sorting for each
              // group modification
              currentGroup = currentGroup.GetGroup( rawItem, j - 1, culture, groupDescriptions, null );
            }

            currentGroup.InsertRawItem( currentGroup.ItemCount, rawItem );
          }
        }
        finally
        {
          for( int i = 0; i < groupDescriptionCount; i++ )
          {
            DataGridGroupDescription groupDescription = groupDescriptions[ i ] as DataGridGroupDescription;

            if( groupDescription != null )
              groupDescription.SetContext( null );
          }
        }
      }
      else
      {
        for( int i = 0; i < itemCount; i++ )
        {
          rootGroup.InsertRawItem( i, m_filteredItemList[ i ] );
        }
      }

      unchecked
      {
        m_sortedItemVersion++;
      }
    }

    private void SortItems( SortDescriptionInfo[] sortDescriptionInfos )
    {
      // We set the current item to -1 to prevent the developper to get an invalid position.
      // We will replace the current item to the correct one later.
      this.SetCurrentItem( -1, null, false, false );

      DataGridCollectionViewGroupRoot rootGroup = this.RootGroup;

      if( rootGroup.IsBottomLevel )
      {
        rootGroup.SortRootRawItems( sortDescriptionInfos, m_sourceItemList );
      }
      else
      {
        // We use a new DataGridCollectionViewGroupRoot to prevent
        // some event to be raised.
        DataGridCollectionViewGroupRoot newRootGroup = new DataGridCollectionViewGroupRoot( rootGroup );

        rootGroup.SortItems(
          sortDescriptionInfos, this.GetGroupSortComparers(),
          0, m_sourceItemList, newRootGroup );

        this.RootGroup = newRootGroup;
      }

      unchecked
      {
        m_sortedItemVersion++;
      }
    }

    private void PrepareSort( out SortDescriptionInfo[] sortDescriptionInfos )
    {
      sortDescriptionInfos = null;

      lock( this.SyncRoot )
      {
        lock( this.DeferredOperationManager )
        {
          SortDescriptionCollection sortDescriptions = this.SortDescriptions;
          int sortDescriptionCount = sortDescriptions.Count;

          if( sortDescriptionCount > 0 )
          {
            int itemCount = m_sourceItemList.Count;
            sortDescriptionInfos = new SortDescriptionInfo[ sortDescriptionCount ];
            DataGridItemPropertyCollection itemProperties = this.ItemProperties;

            for( int i = 0; i < sortDescriptionCount; i++ )
            {
              SortDescription sortDescription = sortDescriptions[ i ];
              DataGridItemPropertyBase dataProperty = itemProperties[ sortDescription.PropertyName ];

              SortDescriptionInfo sortDescriptionInfo =
                new SortDescriptionInfo( dataProperty, sortDescription.Direction );

              sortDescriptionInfos[ i ] = sortDescriptionInfo;

              if( dataProperty == null )
                continue;

              // This will cache the data for the sorting
              DataStore dataStore;

              if( dataProperty.SortComparer != null )
              {
                // If we have a sort comparer, keep the element in an object data type to increase the performance.
                dataStore = DataGridCollectionView.CreateStore( typeof( object ), itemCount );
              }
              else
              {
                dataStore = DataGridCollectionView.CreateStore( dataProperty.DataType, itemCount );
              }

              sortDescriptionInfo.DataStore = dataStore;

              ISupportInitialize supportInitialize = dataProperty as ISupportInitialize;

              if( supportInitialize != null )
                supportInitialize.BeginInit();

              try
              {
                for( int j = 0; j < itemCount; j++ )
                {
                  dataStore.SetData( j, dataProperty.GetValue( m_sourceItemList[ j ].DataItem ) );
                }
              }
              finally
              {
                if( supportInitialize != null )
                  supportInitialize.EndInit();
              }
            }
          }
        }
      }
    }

    private GroupSortComparer[] GetGroupSortComparers()
    {
      SortDescriptionCollection sortDescriptions = this.SortDescriptions;
      ObservableCollection<GroupDescription> groupDescriptions = this.GroupDescriptions;

      int groupDescriptionCount = groupDescriptions.Count;
      int sortDescriptionCount = sortDescriptions.Count;
      GroupSortComparer[] sortComparers = new GroupSortComparer[ groupDescriptionCount ];

      DataGridItemPropertyCollection itemProperties = this.ItemProperties;

      for( int j = 0; j < groupDescriptionCount; j++ )
      {
        GroupDescription groupDescription = groupDescriptions[ j ];

        // Find the sortDescriptionInfo for the current sub group.
        ListSortDirection sortDirection = ListSortDirection.Ascending;
        string groupName = DataGridCollectionView.GetPropertyNameFromGroupDescription( groupDescription );
        DataGridGroupDescription dataGridGroupDescription = groupDescription as DataGridGroupDescription;
        bool defaultGroupSortAdded = false;

        List<GroupSortComparer.SortInfo> sortInfos =
          new List<GroupSortComparer.SortInfo>( sortDescriptionCount );

        for( int i = 0; i < sortDescriptionCount; i++ )
        {
          SortDescription sortDescription = sortDescriptions[ i ];
          sortDirection = sortDescription.Direction;
          string sortDescriptionPropertyName = sortDescription.PropertyName;
          DataGridItemPropertyBase itemProperty = itemProperties[ sortDescriptionPropertyName ];

          if( itemProperty != null )
          {
            string sortStatResultPropertyName = itemProperty.GroupSortStatResultPropertyName;

            if( !string.IsNullOrEmpty( sortStatResultPropertyName ) )
            {
              IComparer sortComparer = itemProperty.GroupSortStatResultComparer;

              if( sortComparer == null )
                sortComparer = StatResultComparer.Singleton;

              sortInfos.Add( new GroupSortComparer.SortInfo(
                sortStatResultPropertyName, sortDirection, sortComparer ) );
            }
          }

          if( string.Equals( groupName, sortDescriptionPropertyName ) )
          {
            sortInfos.Add( this.CreateDefaultGroupSortInfo(
              groupDescription, itemProperty, sortDirection, true ) );

            defaultGroupSortAdded = true;
          }
        }

        if( !defaultGroupSortAdded )
        {
          GroupSortComparer.SortInfo sortInfo = this.CreateDefaultGroupSortInfo(
            groupDescription, itemProperties[ groupName ], sortDirection, false );

          if( sortInfo != null )
          {
            sortInfos.Add( sortInfo );
          }
        }

        sortComparers[ j ] = new GroupSortComparer( sortInfos );
      }

      return sortComparers;
    }

    private GroupSortComparer.SortInfo CreateDefaultGroupSortInfo(
      GroupDescription groupDescription,
      DataGridItemPropertyBase itemProperty,
      ListSortDirection sortDirection,
      bool useDefaultComparer )
    {
      DataGridGroupDescription dataGridGroupDescription = groupDescription as DataGridGroupDescription;
      IComparer sortComparer = null;

      if( dataGridGroupDescription != null )
        sortComparer = dataGridGroupDescription.SortComparer;

      if( ( sortComparer == null ) && ( itemProperty != null ) )
      {
        sortComparer = itemProperty.SortComparer;
      }

      if( ( sortComparer == null ) && ( useDefaultComparer ) )
      {
        sortComparer = ObjectComparer.Singleton;
      }

      if( sortComparer == null )
        return null;

      return new GroupSortComparer.SortInfo( null, sortDirection, sortComparer );
    }

    private void AdjustCurrentAndSendResetNotification( RawItem oldCurrentRawItem, int oldCurrentPosition )
    {
      this.AdjustCurrencyAfterReset( oldCurrentRawItem, oldCurrentPosition, false );

      this.OnCollectionChanged( new NotifyCollectionChangedEventArgs(
        NotifyCollectionChangedAction.Reset ) );
    }

    private void TriggerRootGroupChanged()
    {
      // All the groups will be recreated. No need to trigger the StatFunctions PropertyChanged.
      this.DeferredOperationManager.ClearInvalidatedGroups();

      // The previous GroupItems or SortItems have changed RootGroup but have not
      // send the RootGroupChanged event. Do it here.
      this.OnRootGroupChanged( EventArgs.Empty );
    }

    private DataGridCollectionViewGroup GetRawItemNewGroup( RawItem rawItem )
    {
      ObservableCollection<GroupDescription> groupDescriptions = this.GroupDescriptions;
      int groupDescriptionCount = groupDescriptions.Count;

      if( groupDescriptionCount == 0 )
        return this.RootGroup;

      CultureInfo culture = this.Culture;
      GroupSortComparer[] sortComparers = this.GetGroupSortComparers();

      for( int i = 0; i < groupDescriptionCount; i++ )
      {
        DataGridGroupDescription groupDescription = groupDescriptions[ i ] as DataGridGroupDescription;

        if( groupDescription != null )
          groupDescription.SetContext( this );
      }

      try
      {
        DataGridCollectionViewGroup currentGroup = this.RootGroup;

        for( int j = 1; j <= groupDescriptionCount; j++ )
        {
          currentGroup = currentGroup.GetGroup( rawItem, j - 1, culture, groupDescriptions, sortComparers );
        }

        return currentGroup;
      }
      finally
      {
        for( int i = 0; i < groupDescriptionCount; i++ )
        {
          DataGridGroupDescription groupDescription = groupDescriptions[ i ] as DataGridGroupDescription;

          if( groupDescription != null )
            groupDescription.SetContext( null );
        }
      }
    }

    private void AddRawItemDataItemMapping( RawItem rawItem )
    {
      object dataItem = rawItem.DataItem;

      object cached;
      if( m_dataItemToRawItemList.TryGetValue( dataItem, out cached ) )
      {
        // rawItems will either be a RawItem or an array of RawItem.
        // For sure, after this, we will have an array of RawItem.
        RawItem temp = cached as RawItem;
        RawItem[] rawItems;

        if( temp != null )
        {
          System.Diagnostics.Debug.Assert( rawItem != temp, "It's not normal to be called twice for the same RawItem." );

          // We need to convert the entry to an array.
          rawItems = new RawItem[ 2 ] { temp, rawItem };
        }
        else
        {
          rawItems = ( RawItem[] )cached;

          int count = rawItems.Length;
          for( int i = 0; i < count; i++ )
          {
            if( rawItems[ i ] == rawItem )
            {
              System.Diagnostics.Debug.Fail( "It's not normal to be called twice for the same RawItem." );
              return;
            }
          }

          // Resize the array and add the new RawItem into it.
          Array.Resize<RawItem>( ref rawItems, rawItems.Length + 1 );
          rawItems[ rawItems.Length - 1 ] = rawItem;
        }

        m_dataItemToRawItemList[ dataItem ] = rawItems;
      }
      else
      {
        // This is the first time that we are adding a map for this
        // data item. No need for an array here.
        m_dataItemToRawItemList.Add( dataItem, rawItem );
      }
    }

    private void RemoveRawItemDataItemMapping( RawItem rawItem )
    {
      object dataItem = rawItem.DataItem;

      object cached;
      if( m_dataItemToRawItemList.TryGetValue( dataItem, out cached ) )
      {
        RawItem temp = cached as RawItem;
        if( temp != null )
        {
          // We only had one RawItem for the data item.
          // Simply remove the cached entry.
          m_dataItemToRawItemList.Remove( dataItem );
        }
        else
        {
          // Remove the RawItem from the mapping.
          List<RawItem> rawItemList = new List<RawItem>( ( RawItem[] )cached );
          if( rawItemList.Remove( rawItem ) )
          {
            // If there is only one RawItem left in the list,
            // we convert the list to store only the RawItem.
            m_dataItemToRawItemList[ dataItem ] = ( rawItemList.Count == 1 )
              ? ( object )rawItemList[ 0 ]
              : rawItemList.ToArray();
          }
        }
      }
    }

    private void SaveCurrentBeforeReset( out RawItem oldCurrentRawItem, out int oldCurrentPosition )
    {
      Debug.Assert( this.Loaded );

      if( this.IsCurrentAfterLast )
      {
        oldCurrentPosition = int.MaxValue;
      }
      else
      {
        oldCurrentPosition = this.CurrentPosition;
      }

      oldCurrentRawItem = ( oldCurrentPosition < 0 ) || ( oldCurrentPosition == int.MaxValue ) ?
        null : this.RootGroup.GetRawItemAtGlobalSortedIndex( oldCurrentPosition );
    }

    private void AdjustCurrencyAfterReset( RawItem oldCurrentRawItem, int oldCurrentPosition, bool itemsChanged )
    {
      DataGridCollectionViewGroupRoot rootGroup = this.RootGroup;

      if( ( oldCurrentPosition < 0 ) || ( rootGroup.GlobalRawItemCount == 0 ) )
      {
        this.SetCurrentItem( -1, null, false, false );
        return;
      }

      if( oldCurrentPosition == int.MaxValue )
      {
        this.SetCurrentItem( rootGroup.GlobalRawItemCount, null, false, false );
        return;
      }

      if( itemsChanged )
      {
        object oldCurrentItem = oldCurrentRawItem.DataItem;
        int newIndex = this.IndexOf( oldCurrentItem );

        if( newIndex == -1 )
        {
          this.SetCurrentItem( -1, null, false, false );
        }
        else
        {
          this.SetCurrentItem( newIndex, oldCurrentItem, false, false );
        }
      }
      else
      {
        this.SetCurrentItem(
          oldCurrentRawItem.GetGlobalSortedIndex(),
          oldCurrentRawItem.DataItem,
          false, false );
      }
    }

    private void AdjustCurrencyAfterInitialLoad( bool setCurrentToFirst )
    {
      if( this.RootGroup.GlobalRawItemCount == 0 )
      {
        this.SetCurrentItem( -1, null, false, false );
        return;
      }

      if( setCurrentToFirst )
      {
        this.SetCurrentItem( 0, this.RootGroup.GetRawItemAtGlobalSortedIndex( 0 ).DataItem, false, false );
      }
      else
      {
        this.SetCurrentItem( -1, null, false, false );
      }
    }

    private void AdjustCurrencyBeforeAdd( int index )
    {
      if( this.RootGroup.GlobalRawItemCount == 0 )
      {
        this.SetCurrentItem( -1, null, false, false );
      }
      else if( index <= this.CurrentPosition )
      {
        this.SetCurrentItem( this.CurrentPosition + 1, this.CurrentItem, false, false );
      }
    }

    private void AdjustCurrencyAfterMove( int oldIndex, int newIndex, int itemCount )
    {
      int oldRangeEnd = oldIndex + itemCount - 1;

      // Current position was in the moved range
      if( ( this.CurrentPosition >= oldIndex ) && ( this.CurrentPosition <= oldRangeEnd ) )
      {
        int offset = this.CurrentPosition - oldIndex;
        this.SetCurrentItem( newIndex + offset, this.CurrentItem, false, false );
      }
      else if( ( ( oldIndex >= this.CurrentPosition ) || ( newIndex >= this.CurrentPosition ) )
        && ( ( oldIndex <= this.CurrentPosition ) || ( newIndex <= this.CurrentPosition ) ) )
      {
        if( oldIndex < this.CurrentPosition )
        {
          this.SetCurrentItem( this.CurrentPosition - itemCount, this.CurrentItem, false, false );
        }
        else if( newIndex <= this.CurrentPosition )
        {
          this.SetCurrentItem( this.CurrentPosition + itemCount, this.CurrentItem, false, false );
        }
      }
    }

    private void AdjustCurrencyBeforeRemove( int index )
    {
      if( index < this.CurrentPosition )
      {
        this.SetCurrentItem( this.CurrentPosition - 1, this.CurrentItem, false, true );
      }
      else if( index == this.CurrentPosition )
      {
        DataGridCollectionViewGroupRoot rootGroup = this.RootGroup;

        if( this.CurrentPosition == ( rootGroup.GlobalRawItemCount - 1 ) )
        {
          index = rootGroup.GlobalRawItemCount - 2;

          if( index >= 0 )
          {
            this.SetCurrentItem( index, rootGroup.GetRawItemAtGlobalSortedIndex( index ).DataItem, false, true );
          }
          else
          {
            this.SetCurrentItem( index, null, false, true );
          }
        }
        else
        {
          this.SetCurrentItem(
            index,
            rootGroup.GetRawItemAtGlobalSortedIndex( index + 1 ).DataItem,
            false, true );
        }
      }
    }

    private void AdjustCurrencyBeforeReplace( int index, object newItem )
    {
      if( index == this.CurrentPosition )
        this.SetCurrentItem( index, newItem, false, false );
    }

    #region ICancelAddNew Members

    void ICancelAddNew.CancelNew( int itemIndex )
    {
      this.CancelNew();
    }

    void ICancelAddNew.EndNew( int itemIndex )
    {
      this.CommitNew();
    }

    #endregion

    #region ISupportInitializeNotification Members

    private event EventHandler Initialized;

    event EventHandler ISupportInitializeNotification.Initialized
    {
      add
      {
        this.Initialized = ( EventHandler )Delegate.Combine( this.Initialized, value );
      }
      remove
      {
        this.Initialized = ( EventHandler )Delegate.Remove( this.Initialized, value );
      }
    }

    private void OnInitialized( EventArgs e )
    {
      if( this.Initialized != null )
        this.Initialized( this, e );
    }

    bool ISupportInitializeNotification.IsInitialized
    {
      get
      {
        return m_batchInitializationCount == 0;
      }
    }

    #endregion ISupportInitializeNotification Members

    #region ISupportInitialize Members

    void ISupportInitialize.BeginInit()
    {
      if( m_batchInitializationCount == 0 )
      {
        Debug.Assert( m_batchDeferred == null );
        m_batchDeferred = this.DeferRefresh();
      }

      m_batchInitializationCount++;
    }

    void ISupportInitialize.EndInit()
    {
      if( m_batchInitializationCount > 0 )
        m_batchInitializationCount--;

      if( m_batchInitializationCount == 0 )
      {
        m_batchDeferred.Dispose();
        m_batchDeferred = null;
        this.OnInitialized( EventArgs.Empty );
      }
    }

    #endregion ISupportInitialize Members

    #region ICollectionView Members

    public override IEnumerable SourceCollection
    {
      get
      {
        return this.Enumeration;
      }
    }

    public override bool Contains( object item )
    {
      this.EnsureThreadAndCollectionLoaded();

      if( item != null )
      {
        RawItem rawItem = this.GetFirstRawItemFromDataItem( item );
        if( rawItem != null )
        {
          // A SortedIndex of -1 means that the RawItem is not in the m_filteredItemList.
          if( rawItem.SortedIndex > -1 )
            return true;
        }
      }

      return false;
    }

    public override bool IsEmpty
    {
      get
      {
        this.EnsureThreadAndCollectionLoaded();
        return ( this.RootGroup.GlobalRawItemCount == 0 );
      }
    }

    public override int IndexOf( object item )
    {
      this.EnsureThreadAndCollectionLoaded();

      if( item != null )
      {
        RawItem rawItem = this.GetFirstRawItemFromDataItem( item );
        if( rawItem != null )
        {
          // A SortedIndex of -1 means that the RawItem is not in the m_filteredItemList.
          if( rawItem.SortedIndex > -1 )
            return rawItem.GetGlobalSortedIndex();
        }
      }

      return -1;
    }

    #endregion ICollectionView Members

    #region IBindingList Members

    void IBindingList.AddIndex( PropertyDescriptor property )
    {
      throw new NotSupportedException( "The IBindingList.AddIndex method not supported." );
    }

    bool IBindingList.AllowEdit
    {
      get
      {
        return false;
      }
    }

    bool IBindingList.AllowRemove
    {
      get
      {
        return false;
      }
    }

    void IBindingList.ApplySort( PropertyDescriptor property, ListSortDirection direction )
    {
      throw new NotSupportedException( "The IBindingList.ApplySort method is not supported." );
    }

    int IBindingList.Find( PropertyDescriptor property, object key )
    {
      throw new NotSupportedException( "The IBindingList.Find method is not supported." );
    }

    bool IBindingList.IsSorted
    {
      get
      {
        return false;
      }
    }

    private event ListChangedEventHandler ListChanged;

    event ListChangedEventHandler IBindingList.ListChanged
    {
      add
      {
        this.ListChanged = ( ListChangedEventHandler )EventHandler.Combine( this.ListChanged, value );
      }
      remove
      {
        this.ListChanged = ( ListChangedEventHandler )EventHandler.Remove( this.ListChanged, value );
      }
    }

    void IBindingList.RemoveIndex( PropertyDescriptor property )
    {
      throw new NotSupportedException( "The IBindingList.RemoveIndex method is not supported." );
    }

    void IBindingList.RemoveSort()
    {
      throw new NotSupportedException( "The IBindingList.RemoveSort method is not supported." );
    }

    ListSortDirection IBindingList.SortDirection
    {
      get
      {
        throw new NotSupportedException( "The IBindingList.SortDirection property is not supported." );
      }
    }

    PropertyDescriptor IBindingList.SortProperty
    {
      get
      {
        throw new NotSupportedException( "The IBindingList.SortProperty property is not supported." );
      }
    }

    bool IBindingList.SupportsChangeNotification
    {
      get
      {
        return false;
      }
    }

    bool IBindingList.SupportsSearching
    {
      get
      {
        return false;
      }
    }

    bool IBindingList.SupportsSorting
    {
      get
      {
        return false;
      }
    }

    #endregion IBindingList Members

    #region IList Members

    int IList.Add( object value )
    {
      throw new NotSupportedException( "The IList.Add method is not supported." );
    }

    void IList.Clear()
    {
      throw new NotSupportedException( "The IList.Clear method is not supported." );
    }

    void IList.Insert( int index, object value )
    {
      throw new NotSupportedException( "The IList.Insert method is not supported." );
    }

    bool IList.IsFixedSize
    {
      get
      {
        return true;
      }
    }

    bool IList.IsReadOnly
    {
      get
      {
        return true;
      }
    }

    void IList.Remove( object value )
    {
      throw new NotSupportedException( "The IList.Remove method is not supported." );
    }

    void IList.RemoveAt( int index )
    {
      throw new NotSupportedException( "The IList.RemoveAt method is not supported." );
    }

    object IList.this[ int index ]
    {
      get
      {
        return this.GetItemAt( index );
      }
      set
      {
        throw new NotSupportedException( "The IList indexer is not supported." );
      }
    }

    #endregion IList Members

    #region ICollection Members

    void ICollection.CopyTo( Array array, int index )
    {
      IEnumerator enumerator = this.GetEnumerator();

      while( enumerator.MoveNext() )
      {
        array.SetValue( enumerator.Current, index );
        index++;
      }
    }

    void CopyTo( object[] array, int index )
    {
      IEnumerator enumerator = this.GetEnumerator();

      while( enumerator.MoveNext() )
      {
        array[ index ] = enumerator.Current;
        index++;
      }
    }

    bool ICollection.IsSynchronized
    {
      get
      {
        return false;
      }
    }

    object ICollection.SyncRoot
    {
      get
      {
        return this.SyncRoot;
      }
    }

    #endregion ICollection Members

    #region IEnumerable Members

    protected override IEnumerator GetEnumerator()
    {
      this.EnsureThreadAndCollectionLoaded();
      return new DataGridCollectionViewEnumerator( this );
    }

    #endregion IEnumerable Members

    private static RawItemIndexComparer m_rawItemIndexComparer;

    private RawItemSortComparer m_rawItemSortComparer;

    private int m_lastAddCount;
    private int m_lastAddIndex;

    private int m_batchInitializationCount;
    private IDisposable m_batchDeferred;

    private List<RawItem> m_sourceItemList;
    private List<RawItem> m_filteredItemList;
    private Dictionary<object, object> m_dataItemToRawItemList;

    private PropertyDescriptorCollection m_statisticalProperties;
    private List<WeakReference> m_tempStatFunctions = new List<WeakReference>();

    private int m_sortedItemVersion;

    #region Private Class StatFunctionPropertyDescriptor

    private class StatFunctionPropertyDescriptor : PropertyDescriptor
    {
      public StatFunctionPropertyDescriptor( string propertyName )
        : base( propertyName, null )
      {
      }

      public override bool CanResetValue( object component )
      {
        return false;
      }

      public override Type ComponentType
      {
        get
        {
          return typeof( DataGridCollectionViewGroup );
        }
      }

      public override object GetValue( object component )
      {
        DataGridCollectionViewGroup group = component as DataGridCollectionViewGroup;

        if( group != null )
          return group.GetStatFunctionValue( this.Name );

        return null;
      }

      public override bool IsReadOnly
      {
        get
        {
          return true;
        }
      }

      public override Type PropertyType
      {
        get
        {
          return typeof( object );
        }
      }

      public override void ResetValue( object component )
      {
        throw new NotSupportedException( "This statistical property is read-only." );
      }

      public override void SetValue( object component, object value )
      {
        throw new NotSupportedException( "This statistical property is read-only." );
      }

      public override bool ShouldSerializeValue( object component )
      {
        return false;
      }
    }

    #endregion

    #region Private Class EqualityComparerWrapper

    private class EqualityComparerWrapper : IEqualityComparer<object>
    {
      public EqualityComparerWrapper( IEqualityComparer comparer )
      {
        if( comparer == null )
        {
          m_comparer = EqualityComparer<object>.Default;
        }
        else
        {
          m_comparer = comparer;
        }
      }

      bool IEqualityComparer<object>.Equals( object x, object y )
      {
        return m_comparer.Equals( x, y );
      }

      int IEqualityComparer<object>.GetHashCode( object obj )
      {
        return m_comparer.GetHashCode( obj );
      }

      private IEqualityComparer m_comparer;
    }

    #endregion
  }
}
