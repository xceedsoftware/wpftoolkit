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
using System.Windows.Data;
using Xceed.Utils.Collections;
using Xceed.Utils.Data;

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

    private DataGridCollectionView( IEnumerable collection, DataGridDetailDescription parentDetailDescription, DataGridCollectionView parentDataGridCollectionView )
      : base( collection, parentDetailDescription, parentDataGridCollectionView.RootDataGridCollectionViewBase )
    {
      if( collection == null )
        throw new ArgumentNullException( "collection" );

      //If already deferring refreshes while a detail grid is expanded.
      if( parentDataGridCollectionView.InDeferRefresh )
      {
        //If count is 0, it means this child collection view represents the first detail of its parent item, so create the parent's list of disposable.
        if( parentDataGridCollectionView.DetailDeferRefreshes.Count == 0 )
        {
          parentDataGridCollectionView.DetailDeferRefreshes.Add( new List<IDisposable>() );
        }

        //Need to defer refresh on the detail CollectionView as well, and for as many "defers" there are presently at the master level.
        foreach( List<IDisposable> detailDisposables in parentDataGridCollectionView.DetailDeferRefreshes )
        {
          detailDisposables.Add( this.DeferRefresh() );
        }

        //If Refresh() has been called on the root CollectionView, do it also for the detail one.
        if( parentDataGridCollectionView.NeedsRefresh )
        {
          this.Refresh();
        }
      }
    }

    internal override void PreConstruct()
    {
      base.PreConstruct();

      m_sourceItemList = new List<RawItem>( 128 );
      m_filteredItemList = new List<RawItem>( 128 );

      m_sourceItems = new SourceItemCollection( this );
      this.RootGroup = new DataGridCollectionViewGroupRoot( this );
    }

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
      int sourceIndex;
      object dataItem;

      if( rawItem != null )
      {
        sourceIndex = rawItem.Index;
        dataItem = rawItem.DataItem;
      }
      else
      {
        sourceIndex = -1;
        dataItem = null;
      }

      DataGridRemovingItemEventArgs removingItemEventArgs = new DataGridRemovingItemEventArgs( this, dataItem, index, false );
      this.RootDataGridCollectionViewBase.OnRemovingItem( removingItemEventArgs );

      if( removingItemEventArgs.Cancel )
        throw new DataGridException( "RemoveAt was canceled." );

      if( !removingItemEventArgs.Handled )
        this.InternalRemoveAt( sourceIndex );

      if( ( !this.IsSourceSupportingChangeNotification ) && ( sourceIndex != -1 ) )
      {
        DeferredOperation deferredOperation = new DeferredOperation( DeferredOperation.DeferredOperationAction.Remove, sourceIndex, new object[] { dataItem } );

        this.ExecuteOrQueueSourceItemOperation( deferredOperation );
      }

      DataGridItemRemovedEventArgs itemRemovedEventArgs = new DataGridItemRemovedEventArgs( this, dataItem, index );
      this.RootDataGridCollectionViewBase.OnItemRemoved( itemRemovedEventArgs );
    }

    public override object GetItemAt( int index )
    {
      this.EnsureThreadAndCollectionLoaded();

      return this.RootGroup.GetRawItemAtGlobalSortedIndex( index ).DataItem;
    }

    public void ResetItem( object item )
    {
      DeferredOperation deferredOperation = new DeferredOperation( DeferredOperation.DeferredOperationAction.ResetItem, item );
      this.ExecuteOrQueueSourceItemOperation( deferredOperation );
    }

    public void ResetItems( IList items )
    {
      DeferredOperation deferredOperation = new DeferredOperation( DeferredOperation.DeferredOperationAction.ResetItem, items );
      this.ExecuteOrQueueSourceItemOperation( deferredOperation );
    }

    public void RefreshUnboundItemProperties( object item = null )
    {
      DeferredOperation deferredOperation = new DeferredOperation( DeferredOperation.DeferredOperationAction.RefreshUnboundItemProperties, item );
      this.ExecuteOrQueueSourceItemOperation( deferredOperation );
    }

    public void RefreshUnboundItemProperties( IList items )
    {
      DeferredOperation deferredOperation = new DeferredOperation( DeferredOperation.DeferredOperationAction.RefreshUnboundItemProperties, items );
      this.ExecuteOrQueueSourceItemOperation( deferredOperation );
    }

    internal override DataGridCollectionViewBase CreateDetailDataGridCollectionViewBase(
      IEnumerable detailDataSource,
      DataGridDetailDescription parentDetailDescription,
      DataGridCollectionViewBase parentDataGridCollectionViewBase )
    {
      Debug.Assert( parentDataGridCollectionViewBase is DataGridCollectionView );

      if( parentDetailDescription != null )
        return new DataGridCollectionView( detailDataSource, parentDetailDescription, parentDataGridCollectionViewBase as DataGridCollectionView );

      return new DataGridCollectionView( detailDataSource );
    }

    internal override void CreateDefaultCollections( DataGridDetailDescription parentDetailDescription )
    {
      base.CreateDefaultCollections( parentDetailDescription );
    }

    internal override bool CanCreateItemProperties()
    {
      if( base.CanCreateItemProperties() )
        return true;

      return ( m_sourceItems.Count != 0 );
    }

    internal override int IndexOfSourceItem( object item )
    {
      if( item != null )
      {
        // The source index of raw items may not be up-to-date if some operations were deferred.
        if( !this.DeferredOperationManager.HasPendingOperations )
        {
          // Use the RawItem/DataItem mapping instead of looping through the list for efficency.
          var rawItem = this.GetFirstRawItemFromDataItem( item );

          if( rawItem != null )
            return rawItem.Index;
        }

        IList list = this.Enumeration as IList;
        if( list != null )
          return list.IndexOf( item );

        var index = 0;
        foreach( var obj in this.Enumeration )
        {
          if( obj == item )
            return index;

          index++;
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
            refreshForced = !this.AddSourceItem( deferredOperation.NewStartingIndex, deferredOperation.NewItems, deferredOperation.NewSourceItemCount );
            break;
          }

        case DeferredOperation.DeferredOperationAction.Move:
          {
            refreshForced = !this.MoveSourceItem( deferredOperation.OldStartingIndex, deferredOperation.OldItems, deferredOperation.NewStartingIndex );
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
            break;
          }

        case DeferredOperation.DeferredOperationAction.Regroup:
          {
            refreshForced = false;

            // Ensure to defer the CurrentItem change when regrouping is completed to be sure the SaveCurrent has the right values when restoring
            using( this.DeferCurrencyEvent() )
            {
              var oldCurrentPosition = -1;
              var oldCurrentRawItem = default( RawItem );

              this.SaveCurrentBeforeReset( out oldCurrentRawItem, out oldCurrentPosition );

              var sortDescriptionInfos = default( IList<SortDescriptionInfo> );

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
            refreshForced = !this.RemoveSourceItem( deferredOperation.OldStartingIndex, deferredOperation.OldItems.Count );
            break;
          }

        case DeferredOperation.DeferredOperationAction.Replace:
          {
            refreshForced = !this.ReplaceSourceItem( deferredOperation.OldStartingIndex, deferredOperation.OldItems,
                                                     deferredOperation.NewStartingIndex, deferredOperation.NewItems );
            break;
          }

        case DeferredOperation.DeferredOperationAction.Resort:
          {
            refreshForced = false;

            // Ensure to defer the CurrentItem change when resort is completed to be sure the SaveCurrent has the right values when restoring
            using( this.DeferCurrencyEvent() )
            {
              var oldCurrentPosition = -1;
              var oldCurrentRawItem = default( RawItem );

              this.SaveCurrentBeforeReset( out oldCurrentRawItem, out oldCurrentPosition );

              var sortDescriptionInfos = default( IList<SortDescriptionInfo> );

              this.PrepareSort( out sortDescriptionInfos );
              this.SortItems( sortDescriptionInfos );

              // Ensure to send reset notifications
              this.AdjustCurrentAndSendResetNotification( oldCurrentRawItem, oldCurrentPosition );

              //If there is no grouping, there is no group changes, so no GroupChanged to raise.
              if( !this.RootGroup.IsBottomLevel )
              {
                this.TriggerRootGroupChanged();
              }
            }
            break;
          }

        case DeferredOperation.DeferredOperationAction.ResetItem:
          {
            refreshForced = false;
            if( deferredOperation.DataItem != null )
            {
              this.ResetSourceItem( deferredOperation.DataItem );
            }
            else
            {
              this.ResetSourceItems( deferredOperation.NewItems );
            }
            m_currentChildCollectionView = null;
            break;
          }

        case DeferredOperation.DeferredOperationAction.RefreshUnboundItemProperties:
          {
            refreshForced = false;
            if( deferredOperation.NewItems != null )
            {
              this.RefreshUnboundItemPropertiesCore( deferredOperation.NewItems );
            }
            else
            {
              this.RefreshUnboundItemPropertiesCore( deferredOperation.DataItem );
            }
            m_currentChildCollectionView = null;
            break;
          }

        default:
          {
            base.ExecuteSourceItemOperation( deferredOperation, out refreshForced );
            break;
          }
      }
    }

    internal override void ForceRefresh( bool sendResetNotification, bool initialLoad, bool setCurrentToFirstOnInitialLoad )
    {
      if( this.Refreshing )
        throw new InvalidOperationException( "An attempt was made to refresh the DataGridCollectionView while it is already in the process of refreshing." );

      if( this.IsRefreshingDistinctValues )
        throw new InvalidOperationException( "An attempt was made to refresh the DataGridCollectionView while it is already in the process of refreshing distinct values." );

      this.SetCurrentEditItem( null );

      var oldCurrentPosition = -1;
      var oldCurrentRawItem = default( RawItem );

      if( !initialLoad )
      {
        this.SaveCurrentBeforeReset( out oldCurrentRawItem, out oldCurrentPosition );
      }

      var sortDescriptionInfos = default( IList<SortDescriptionInfo> );

      using( this.DeferCurrencyEvent() )
      {
        this.Refreshing = true;

        try
        {
          lock( this.SyncRoot )
          {
            var deferredOperationManager = this.DeferredOperationManager;

            lock( deferredOperationManager )
            {
              deferredOperationManager.ClearDeferredOperations();
              m_lastAddCount = -1;

              var bLoaded = false;
              var enumeration = this.Enumeration;
              var collectionView = enumeration as CollectionView;

              if( collectionView == null )
              {
                var list = enumeration as IList;

                if( list != null )
                {
                  int count = list.Count;

                  m_sourceItemList.Clear();
                  m_filteredItemList.Clear();
                  m_dataItemToRawItemMap.Clear();

                  for( int i = 0; i < count; i++ )
                  {
                    var item = list[ i ];
                    var rawItem = new RawItem( i, item );

                    m_sourceItemList.Add( rawItem );
                    this.AddRawItemDataItemMapping( rawItem );

                    if( this.PassesFilter( item ) )
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
                  var count = 128;

                  if( collectionView != null )
                  {
                    count = collectionView.Count;
                  }
                  else
                  {
                    var collection = enumeration as ICollection;
                    if( collection != null )
                    {
                      count = collection.Count;
                    }
                  }

                  m_sourceItemList.Clear();
                  m_filteredItemList.Clear();
                  m_dataItemToRawItemMap.Clear();

                  var index = 0;
                  var enumerator = enumeration.GetEnumerator();

                  while( enumerator.MoveNext() )
                  {
                    var item = enumerator.Current;
                    var rawItem = new RawItem( index, item );

                    index++;
                    m_sourceItemList.Add( rawItem );

                    this.AddRawItemDataItemMapping( rawItem );

                    if( this.PassesFilter( item ))
                    {
                      m_filteredItemList.Add( rawItem );
                    }
                  }
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
          this.OnCollectionChanged( new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Reset ) );
        }

        this.TriggerRootGroupChanged();
      }
    }

    internal override void ClearGroupSortComparers()
    {
      m_groupSortComparers.Clear();
      m_groupSortComparers.TrimExcess();
    }

    internal RawItem GetRawItemAt( int index )
    {
      this.EnsureThreadAndCollectionLoaded();
      return this.RootGroup.GetRawItemAtGlobalSortedIndex( index );
    }

    internal object GetSourceItemAt( int index )
    {
      if( index > m_sourceItemList.Count - 1 )
        throw new ArgumentOutOfRangeException( "index", index, "index must be greater than zero and less than SourceItems.Count." );

      return m_sourceItemList[ index ].DataItem;
    }

    internal IEnumerator<RawItem> GetSourceListEnumerator()
    {
      return m_sourceItemList.GetEnumerator();
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

      for( int i = 0; i < items.Count; i++ )
      {
        var index = startIndex + i;
        var item = items[ i ];
        var rawItem = new RawItem( index, item );

        if( this.PassesFilter( item ) )
        {
          this.AddRawItemInSourceList( index, rawItem );
          this.AddRawItemInFilteredList( rawItem );
          this.AddRawItemInGroup( rawItem );
        }
        else
        {
          this.AddRawItemInSourceList( index, rawItem );
        }
      }

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

      for( int i = 0; i < count; i++ )
      {
        var rawItem = m_sourceItemList[ startIndex ];

        if( this.CurrentEditItem == rawItem.DataItem )
        {
          this.SetCurrentEditItem( null );
        }

        if( this.RemoveRawItemInFilteredList( rawItem ) )
        {
          this.RemoveRawItemInSourceList( startIndex );
          this.RemoveRawItemInGroup( rawItem );
        }
        else
        {
          this.RemoveRawItemInSourceList( startIndex );
        }
      }

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

      if( ( oldStartIndex + oldItemCount ) > sourceItemListCount || ( newStartIndex + newItemCount ) > sourceItemListCount )
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
            object newItem = newItems[ i ];

            // globalSortedIndex == -1 means item does not pass filter
            if( globalSortedIndex != -1 )
            {
              bool itemChanged = !object.Equals( oldItem, newItem );
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
                if( newItem != oldItem )
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

                this.OnCollectionChanged( new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Replace, newItem, oldItem, rawItem.GetGlobalSortedIndex() ) );

                this.EnsurePosition( rawItem, globalSortedIndex );

                DataGridCollectionViewGroup newGroup = rawItem.ParentGroup;
                bool movedToDifferentParentGroup = oldGroup != newGroup;
              }
            }
            else
            {
              if( newItem != oldItem )
              {
                this.RemoveRawItemDataItemMapping( rawItem );
                rawItem.SetDataItem( newItem );
                this.AddRawItemDataItemMapping( rawItem );
              }

              // Since it is a replace, we must re-ensure position and invalidate stats function (no need to invalidate the old group, since the old item is filtered
              // out, and thus has no parent group), even if it is an item being replaced by itself, in case a property changed would now let it pass the filter.
              this.EnsurePosition( rawItem, globalSortedIndex );
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
        {
          this.RemoveSourceItem( oldStartIndex + newItemCount, extraOldItemCount );
        }

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

      return m_dataItemToRawItemMap[ dataItem ];
    }

    internal int GetGlobalSortedIndexFromDataItem( object dataItem )
    {
      RawItem rawItem = this.GetFirstRawItemFromDataItem( dataItem );
      int globalSortedIndex = rawItem.GetGlobalSortedIndex();
      if( globalSortedIndex == -1 )
      {
        return rawItem.SortedIndex;
      }

      return globalSortedIndex;
    }

    internal IEnumerable<object> GetSortedFilteredDataItems()
    {
      foreach( RawItem item in this.RootGroup.RawItems )
      {
        yield return item.DataItem;
      }
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

      var dataItem = rawItem.DataItem;

      if( this.PassesFilter( dataItem ) )
      {
        if( globalSortedIndex == -1 )
        {
          this.AddRawItemInFilteredList( rawItem );
          this.AddRawItemInGroup( rawItem );
          return;
        }
      }
      else
      {
        if( globalSortedIndex != -1 )
        {
          this.RemoveRawItemInFilteredList( rawItem );
          this.RemoveRawItemInGroup( rawItem );
        }

        return;
      }

      // Verify the row is in the correct group.
      var newGroup = this.GetRawItemNewGroup( rawItem );
      var currentGroup = rawItem.ParentGroup;

      if( currentGroup != newGroup )
      {
        using( this.DeferCurrencyEvent() )
        {         
          var newSortIndex = newGroup.BinarySearchRawItem( rawItem, this.RawItemSortComparer );
          if( newSortIndex < 0 )
          {
            newSortIndex = ~newSortIndex;
          }

          currentGroup.RemoveRawItemAt( rawItem.SortedIndex );
          newGroup.InsertRawItem( newSortIndex, rawItem );

          var newGlobalSortedIndex = rawItem.GetGlobalSortedIndex();
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
        {
          newSortIndex = ~newSortIndex;
        }

        if( newSortIndex > rawItem.SortedIndex )
        {
          newSortIndex--;
        }

        if( rawItem.SortedIndex != newSortIndex )
        {
          using( this.DeferCurrencyEvent() )
          {
            newGroup.MoveRawItem( rawItem.SortedIndex, newSortIndex );

            var newGlobalSortedIndex = rawItem.GetGlobalSortedIndex();
            this.AdjustCurrencyAfterMove( globalSortedIndex, newGlobalSortedIndex, 1 );

            this.OnCollectionChanged( new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Move, rawItem.DataItem, newGlobalSortedIndex, globalSortedIndex ) );
          }
        }
      }
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

          this.SetCurrentItemAndPositionCore( newCurrentItem, newCurrentPosition, isCurrentBeforeFirst, isCurrentAfterLast );

          if( !this.IsCurrencyDeferred )
          {
            if( !object.Equals( oldCurrentItem, newCurrentItem ) )
            {
              this.OnPropertyChanged( new PropertyChangedEventArgs( "CurrentItem" ) );
            }

            if( oldCurrentPosition != this.CurrentPosition )
            {
              this.OnPropertyChanged( new PropertyChangedEventArgs( "CurrentPosition" ) );
            }

            if( oldIsCurrentBeforeFirst != this.IsCurrentBeforeFirst )
            {
              this.OnPropertyChanged( new PropertyChangedEventArgs( "IsCurrentBeforeFirst" ) );
            }

            if( oldIsCurrentAfterLast != this.IsCurrentAfterLast )
            {
              this.OnPropertyChanged( new PropertyChangedEventArgs( "IsCurrentAfterLast" ) );
            }

            this.OnCurrentChanged();
          }
        }
      }

      return ( newCurrentItem != null );
    }

    private bool MoveSourceItem( int oldStartIndex, IList items, int newStartIndex )
    {
      var count = items.Count;

      if( ( oldStartIndex < 0 ) || ( oldStartIndex + count > m_sourceItemList.Count ) || ( newStartIndex < 0 ) || ( newStartIndex > ( m_sourceItemList.Count - count ) ) )
      {
        this.ForceRefresh( true, !this.Loaded, true );
        return false;
      }

      m_lastAddCount = -1;

      if( oldStartIndex < newStartIndex )
      {
        for( int i = count - 1; i >= 0; i-- )
        {
          var oldIndex = oldStartIndex + i;
          var newIndex = newStartIndex + i;

          Debug.Assert( ( oldIndex >= 0 ) && ( oldIndex < m_sourceItemList.Count ) );
          Debug.Assert( ( newIndex >= 0 ) && ( newIndex < m_sourceItemList.Count ) );

          var rawItem = m_sourceItemList[ oldIndex ];
          var wasFiltered = this.RemoveRawItemInFilteredList( rawItem );

          m_sourceItemList.RemoveAt( oldIndex );
          m_sourceItemList.Insert( newIndex, rawItem );

          for( int j = oldIndex; j <= newIndex; j++ )
          {
            m_sourceItemList[ j ].SetIndex( j );
          }

          if( wasFiltered )
          {
            this.AddRawItemInFilteredList( rawItem );
            this.EnsurePosition( rawItem, rawItem.GetGlobalSortedIndex() );
          }
        }
      }
      else if( oldStartIndex > newStartIndex )
      {
        for( int i = 0; i < count; i++ )
        {
          var oldIndex = oldStartIndex + i;
          var newIndex = newStartIndex + i;

          Debug.Assert( ( oldIndex >= 0 ) && ( oldIndex < m_sourceItemList.Count ) );
          Debug.Assert( ( newIndex >= 0 ) && ( newIndex < m_sourceItemList.Count ) );

          var rawItem = m_sourceItemList[ oldIndex ];
          var wasFiltered = this.RemoveRawItemInFilteredList( rawItem );

          m_sourceItemList.RemoveAt( oldIndex );
          m_sourceItemList.Insert( newIndex, rawItem );

          for( int j = newIndex; j <= oldIndex; j++ )
          {
            m_sourceItemList[ j ].SetIndex( j );
          }

          if( wasFiltered )
          {
            this.AddRawItemInFilteredList( rawItem );
            this.EnsurePosition( rawItem, rawItem.GetGlobalSortedIndex() );
          }
        }
      }

      return true;
    }

    private bool ResetSourceItem( object dataItem )
    {
      var rawItem = default( RawItem );

      //Verify if the item is in the same detail CollectionView as the previous item was found in.
      if( m_currentChildCollectionView != null )
      {
        rawItem = m_currentChildCollectionView.GetFirstRawItemFromDataItem( dataItem );

        //If the item was found, refresh it.
        if( rawItem != null )
        {
          int globalSortedIndex = rawItem.GetGlobalSortedIndex();
          m_currentChildCollectionView.EnsurePosition( rawItem, globalSortedIndex );
          return true;
        }
      }

      //If the item has not been found, try to find it in the current CollectionView.
      rawItem = this.GetFirstRawItemFromDataItem( dataItem );
      m_currentChildCollectionView = null;

      //If the item was found, refresh it.
      if( rawItem != null )
      {
        this.EnsurePosition( rawItem, rawItem.GetGlobalSortedIndex() );
        return true;
      }

      //If the item was not found, look for it in an expended detail.
      foreach( var detailContext in this.DataGridContext.GetChildContextsCore() )
      {
        var detailCollectionView = detailContext.Items as DataGridCollectionView;
        if( detailCollectionView == null )
          continue;

        if( detailCollectionView.ResetSourceItem( dataItem ) )
        {
          //keep a reference to the detail CollectionView the item was found in, in case following items belong to the same detail.
          m_currentChildCollectionView = detailCollectionView;
          return true;
        }
      }

      return false;
    }

    private void ResetSourceItems( IList items )
    {
      foreach( object dataItem in items )
      {
        this.ResetSourceItem( dataItem );
      }
    }

    private void AddRawItemInSourceList( int index, RawItem rawItem )
    {
      Debug.Assert( ( index >= 0 ) && ( index <= m_sourceItemList.Count ) );
      Debug.Assert( rawItem != null );

      m_sourceItemList.Insert( index, rawItem );

      this.AddRawItemDataItemMapping( rawItem );

      for( var i = m_sourceItemList.Count - 1; i > index; i-- )
      {
        m_sourceItemList[ i ].SetIndex( i );
      }
    }

    private void AddRawItemInFilteredList( RawItem rawItem )
    {
      Debug.Assert( rawItem != null );

      // The function take for granted that all RawItem's index are sequential,
      // or if there is gap, the index contained in the gap are not already contained in the list.
      if( ( m_filteredItemList.Count == 0 ) || ( m_filteredItemList.Last().Index < rawItem.Index ) )
      {
        m_filteredItemList.Add( rawItem );
      }
      else
      {
        var index = m_filteredItemList.BinarySearch( rawItem, DataGridCollectionView.RawItemIndexComparer );
        Debug.Assert( index < 0 );

        if( index < 0 )
        {
          index = ~index;
        }

        m_filteredItemList.Insert( index, rawItem );
      }
    }

    private void AddRawItemInGroup( RawItem rawItem )
    {
      Debug.Assert( rawItem != null );

      using( this.DeferCurrencyEvent() )
      {
        var newGroup = this.GetRawItemNewGroup( rawItem );
        var index = newGroup.BinarySearchRawItem( rawItem, this.RawItemSortComparer );

        if( index < 0 )
        {
          index = ~index;
        }

        var globalIndex = newGroup.GetFirstRawItemGlobalSortedIndex() + index;

        this.AdjustCurrencyBeforeAdd( globalIndex );

        newGroup.InsertRawItem( index, rawItem );

        unchecked
        {
          m_sortedItemVersion++;
        }

        this.OnCollectionChanged( new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Add, rawItem.DataItem, globalIndex ) );
      }
    }

    private void RemoveRawItemInSourceList( int index )
    {
      Debug.Assert( ( index >= 0 ) && ( index < m_sourceItemList.Count ) );

      this.RemoveRawItemDataItemMapping( m_sourceItemList[ index ] );

      m_sourceItemList.RemoveAt( index );

      for( int i = m_sourceItemList.Count - 1; i >= index; i-- )
      {
        m_sourceItemList[ i ].SetIndex( i );
      }
    }

    private bool RemoveRawItemInFilteredList( RawItem rawItem )
    {
      Debug.Assert( rawItem != null );

      // The function take for granted that all RawItem's index are sequential,
      // There should not be any gap in the index sequence.
      var index = m_filteredItemList.BinarySearch( rawItem, DataGridCollectionView.RawItemIndexComparer );
      if( index < 0 )
        return false;

      Debug.Assert( ( index >= 0 ) && ( index < m_filteredItemList.Count ) );

      m_filteredItemList.RemoveAt( index );

      return true;
    }

    private void RemoveRawItemInGroup( RawItem rawItem )
    {
      Debug.Assert( rawItem != null );

      var parentGroup = rawItem.ParentGroup;
      if( parentGroup == null )
        return;

      var globalSortedIndex = rawItem.GetGlobalSortedIndex();

      using( this.DeferCurrencyEvent() )
      {
        this.AdjustCurrencyBeforeRemove( globalSortedIndex );
        parentGroup.RemoveRawItemAt( rawItem.SortedIndex );

        unchecked
        {
          m_sortedItemVersion++;
        }
      }

      // In the case of a remove, the CollectionChanged must be after the CurrentChanged since when the DataGridCollectionView is used with a Selector having the 
      // IsSynchronizedWithCurrent set, the Selector will set the current position to -1 if the selected item is removed. 
      this.OnCollectionChanged( new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Remove, rawItem.DataItem, globalSortedIndex ) );
    }

    private bool RefreshUnboundItemPropertiesCore( object dataItem = null )
    {
      if( dataItem != null )
      {
        //Verify if the item is in the same detail CollectionView as the previous item was found in.
        if( m_currentChildCollectionView != null && m_currentChildCollectionView.m_sourceItems.Contains( dataItem ) )
        {
          m_currentChildCollectionView.ItemProperties.RefreshUnboundItemProperty( dataItem );
          return true;
        }

        //Since the item was not found in the previous detail, it means items are not sequential, thus no need to keep looking into this detail.
        m_currentChildCollectionView = null;

        //Try to find it in the current CollectionView.
        if( m_sourceItems.Contains( dataItem ) )
        {
          this.ItemProperties.RefreshUnboundItemProperty( dataItem );
          return true;
        }

        //If not, try to find it in an expended detail
        foreach( DataGridContext detailContext in this.DataGridContext.GetChildContextsCore() )
        {
          DataGridCollectionView detailCollectionView = detailContext.Items as DataGridCollectionView;
          if( detailCollectionView != null )
          {
            if( detailCollectionView.RefreshUnboundItemPropertiesCore( dataItem ) )
            {
              m_currentChildCollectionView = detailCollectionView;
              return true;
            }
          }
        }

        return false;
      }

      //If dataItem is null, it means every item must be Refreshed.  Let's start with the level we're on.
      foreach( RawItem rawItem in m_sourceItemList )
      {
        this.ItemProperties.RefreshUnboundItemProperty( rawItem.DataItem );
      }

      //Refresh every expended detail of the parent
      foreach( DataGridContext detailContext in this.DataGridContext.GetChildContextsCore() )
      {
        DataGridCollectionView detailCollectionView = detailContext.Items as DataGridCollectionView;
        if( detailCollectionView != null )
        {
          detailCollectionView.RefreshUnboundItemPropertiesCore();
        }
      }

      return true;
    }

    private void RefreshUnboundItemPropertiesCore( IList items )
    {
      foreach( object dataItem in items )
      {
        this.RefreshUnboundItemPropertiesCore( dataItem );
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
          {
            groupDescription.SetContext( this );
          }
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
              // The sorting will be done after the batch operation which is faster than sorting for each group modification
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
            {
              groupDescription.SetContext( null );
            }
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

    private void SortItems( IList<SortDescriptionInfo> sortDescriptionInfos )
    {
      // We set the current item to -1 to prevent the developper to get an invalid position. We will replace the current item to the correct one later.
      this.SetCurrentItem( -1, null, false, false );

      var rootGroup = this.RootGroup;
      if( rootGroup.IsBottomLevel )
      {
        rootGroup.SortRootRawItems( sortDescriptionInfos, m_sourceItemList );
      }
      else
      {
        // We use a new DataGridCollectionViewGroupRoot to prevent some event to be raised.
        var newRootGroup = new DataGridCollectionViewGroupRoot( rootGroup );

        rootGroup.SortItems( sortDescriptionInfos, this.GetGroupSortComparers(), 0, m_sourceItemList, newRootGroup );

        this.RootGroup = newRootGroup;
      }

      unchecked
      {
        m_sortedItemVersion++;
      }
    }

    private void SortGroups()
    {
      //This method assumes the that the RootGroup is not the bottom level group, i.e. there DataGrid is currently grouped.
      this.RootGroup.SortGroups( this.GetGroupSortComparers(), 0 );

      unchecked
      {
        m_sortedItemVersion++;
      }
    }

    private void ResortGroups()
    {
      using( this.DeferCurrencyEvent() )
      {
        object oldCurrentDataItem = this.CurrentItem;
        int oldCurrentPosition = this.CurrentPosition;

        this.SortGroups();

        RawItem currentRawItem = null;

        if( oldCurrentDataItem != null )
        {
          currentRawItem = this.GetFirstRawItemFromDataItem( oldCurrentDataItem );
        }

        if( currentRawItem == null )
        {
          if( ( oldCurrentPosition > 0 ) && ( oldCurrentPosition < this.RootGroup.GlobalRawItemCount ) )
          {
            currentRawItem = this.RootGroup.GetRawItemAtGlobalSortedIndex( oldCurrentPosition );
          }
        }

        if( currentRawItem != null )
        {
          this.SetCurrentItem( currentRawItem.GetGlobalSortedIndex(), oldCurrentDataItem, false, false );
        }

        this.OnCollectionChanged( new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Reset ) );
      }
    }

    private void PrepareSort( out IList<SortDescriptionInfo> sortDescriptionInfos )
    {
      sortDescriptionInfos = new List<SortDescriptionInfo>();

      lock( this.SyncRoot )
      {
        lock( this.DeferredOperationManager )
        {
          var itemCount = m_sourceItemList.Count;

          foreach( var sortDescription in this.SortDescriptions )
          {
            var itemProperty = ItemsSourceHelper.GetItemPropertyFromProperty( this.ItemProperties, sortDescription.PropertyName );
            var sortDescriptionInfo = new SortDescriptionInfo( itemProperty, sortDescription.Direction );

            sortDescriptionInfos.Add( sortDescriptionInfo );

            if( itemProperty == null )
              continue;

            // This will cache the data for the sorting
            var dataStore = default( DataStore );
            var isSortingOnForeignKeyDescription = false;

            var foreignKeyDescription = itemProperty.ForeignKeyDescription;
            if( foreignKeyDescription != null )
            {
              var dataType = foreignKeyDescription.GetDataType();
              if( dataType != null )
              {
                dataStore = DataGridCollectionView.CreateStore( dataType, itemCount );
                isSortingOnForeignKeyDescription = true;
              }
            }

            itemProperty.IsSortingOnForeignKeyDescription = isSortingOnForeignKeyDescription;

            //Create the DataStore if it has not been created yet.
            if( !isSortingOnForeignKeyDescription )
            {
              if( itemProperty.SortComparer != null )
              {
                // If we have a sort comparer, keep the element in an object data type to increase the performance.
                dataStore = DataGridCollectionView.CreateStore( typeof( object ), itemCount );
              }
              else
              {
                dataStore = DataGridCollectionView.CreateStore( itemProperty.DataType, itemCount );
              }
            }

            sortDescriptionInfo.DataStore = dataStore;

            var supportInitialize = itemProperty as ISupportInitialize;
            if( supportInitialize != null )
            {
              supportInitialize.BeginInit();
            }

            try
            {
              if( isSortingOnForeignKeyDescription )
              {
                for( int i = 0; i < itemCount; i++ )
                {
                  var value = ItemsSourceHelper.GetValueFromItemProperty( itemProperty, m_sourceItemList[ i ].DataItem );

                  dataStore.SetData( i, foreignKeyDescription.GetDisplayValue( value ) );
                }
              }
              else
              {
                for( int i = 0; i < itemCount; i++ )
                {
                  var value = ItemsSourceHelper.GetValueFromItemProperty( itemProperty, m_sourceItemList[ i ].DataItem );

                  dataStore.SetData( i, value );
                }
              }
            }
            finally
            {
              if( supportInitialize != null )
              {
                supportInitialize.EndInit();
              }
            }
          }
        }
      }
    }

    private List<GroupSortComparer> GetGroupSortComparers()
    {
      if( m_groupSortComparers.Count > 0 )
        return m_groupSortComparers;

      var sortDescriptions = this.SortDescriptions;

      foreach( var groupDescription in this.GroupDescriptions )
      {
        // Find all the SortInfos for the current sub group.
        var groupDescriptionName = DataGridCollectionView.GetPropertyNameFromGroupDescription( groupDescription );
        var sortInfos = new List<GroupSortComparer.SortInfo>( sortDescriptions.Count + 1 );

        foreach( var sortDescription in sortDescriptions )
        {
          var sortDirection = sortDescription.Direction;
          var sortDescriptionName = sortDescription.PropertyName;
          var itemProperty = ItemsSourceHelper.GetItemPropertyFromProperty( this.ItemProperties, sortDescriptionName );

          if( string.Equals( groupDescriptionName, sortDescriptionName ) )
          {
            var foreignKeyDescription = ( itemProperty != null ) ? itemProperty.ForeignKeyDescription : null;
            if( foreignKeyDescription != null )
            {
              if( foreignKeyDescription.ItemsSource != null
                && ( !string.IsNullOrWhiteSpace( foreignKeyDescription.DisplayMemberPath ) || ( foreignKeyDescription.ForeignKeyConverter != null ) ) )
              {
                sortInfos.Add( new GroupSortComparer.SortInfo( foreignKeyDescription, sortDirection, this.GetSortComparer( groupDescription, itemProperty ) ) );
              }
            }

            sortInfos.Add( new GroupSortComparer.SortInfo( sortDirection, this.GetSortComparer( groupDescription, itemProperty ) ) );
          }
        }

        m_groupSortComparers.Add( new GroupSortComparer( sortInfos ) );
      }

      return m_groupSortComparers;
    }

    private IComparer GetSortComparer( GroupDescription groupDescription, DataGridItemPropertyBase itemProperty )
    {
      var dataGridGroupDescription = groupDescription as DataGridGroupDescription;
      var sortComparer = default( IComparer );

      if( dataGridGroupDescription != null )
      {
        sortComparer = dataGridGroupDescription.SortComparer;
      }

      if( ( sortComparer == null ) && ( itemProperty != null ) )
      {
        sortComparer = itemProperty.SortComparer;
      }

      if( sortComparer == null )
      {
        sortComparer = ObjectComparer.Singleton;
      }

      return sortComparer;
    }

    private void AdjustCurrentAndSendResetNotification( RawItem oldCurrentRawItem, int oldCurrentPosition )
    {
      this.AdjustCurrencyAfterReset( oldCurrentRawItem, oldCurrentPosition, false );

      this.OnCollectionChanged( new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Reset ) );
    }

    private void TriggerRootGroupChanged()
    {
      // All the groups will be recreated. No need to trigger the StatFunctions PropertyChanged.
      this.DeferredOperationManager.ClearInvalidatedGroups();

      this.OnPropertyChanged( new PropertyChangedEventArgs( DataGridCollectionViewBase.RootGroupPropertyName ) );

      // Flag that the Collection of groups itself has changed.
      this.OnPropertyChanged( new PropertyChangedEventArgs( DataGridCollectionViewBase.GroupsPropertyName ) );
    }

    private DataGridCollectionViewGroup GetRawItemNewGroup( RawItem rawItem )
    {
      ObservableCollection<GroupDescription> groupDescriptions = this.GroupDescriptions;
      int groupDescriptionCount = groupDescriptions.Count;

      if( groupDescriptionCount == 0 )
        return this.RootGroup;

      CultureInfo culture = this.Culture;
      List<GroupSortComparer> sortComparers = this.GetGroupSortComparers();

      for( int i = 0; i < groupDescriptionCount; i++ )
      {
        DataGridGroupDescription groupDescription = groupDescriptions[ i ] as DataGridGroupDescription;

        if( groupDescription != null )
        {
          groupDescription.SetContext( this );
        }
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
          {
            groupDescription.SetContext( null );
          }
        }
      }
    }

    private void AddRawItemDataItemMapping( RawItem rawItem )
    {
      m_dataItemToRawItemMap.Add( rawItem.DataItem, rawItem );
    }

    private void RemoveRawItemDataItemMapping( RawItem rawItem )
    {
      m_dataItemToRawItemMap.Remove( rawItem.DataItem, rawItem );
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

      oldCurrentRawItem = ( oldCurrentPosition < 0 ) || ( oldCurrentPosition == int.MaxValue ) ? null : this.RootGroup.GetRawItemAtGlobalSortedIndex( oldCurrentPosition );
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
        this.SetCurrentItem( oldCurrentRawItem.GetGlobalSortedIndex(), oldCurrentRawItem.DataItem, false, false );
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
          this.SetCurrentItem( index, rootGroup.GetRawItemAtGlobalSortedIndex( index + 1 ).DataItem, false, true );
        }
      }
    }

    private void AdjustCurrencyBeforeReplace( int index, object newItem )
    {
      if( index == this.CurrentPosition )
      {
        this.SetCurrentItem( index, newItem, false, false );
      }
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
      {
        this.Initialized( this, e );
      }
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
      {
        m_batchInitializationCount--;
      }

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
        var rawItem = this.GetFirstRawItemFromDataItem( item );
        if( rawItem != null )
        {
          // A SortedIndex of -1 means that the RawItem is not in the m_filteredItemList.
          if( rawItem.SortedIndex >= 0 )
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
    private List<GroupSortComparer> m_groupSortComparers = new List<GroupSortComparer>();

    private int m_lastAddCount;
    private int m_lastAddIndex;

    private int m_batchInitializationCount;
    private IDisposable m_batchDeferred;

    private List<RawItem> m_sourceItemList;
    private List<RawItem> m_filteredItemList;
    private readonly RawItemMap m_dataItemToRawItemMap = new RawItemMap();

    private int m_sortedItemVersion;
    private DataGridCollectionView m_currentChildCollectionView;

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
