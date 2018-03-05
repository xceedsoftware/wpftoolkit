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
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;

namespace Xceed.Wpf.DataGrid
{
  public abstract class DataGridVirtualizingCollectionViewBase : DataGridCollectionViewBase
  {
    #region STATIC MEMBERS

    internal const int DefaultMaxRealizedItemCount = 1000;
    internal const int DefaultPageSize = 200;
    internal const double DefaultPreemptivePageQueryRatio = 0.25;

    #endregion STATIC MEMBERS

    #region CONSTRUCTORS

    internal DataGridVirtualizingCollectionViewBase( object sourceModel, Type itemType, bool autoCreateItemProperties, int pageSize, int maxRealizedItemCount )
      : base( sourceModel, null, itemType, autoCreateItemProperties, false, false )
    {
      if( itemType == null )
      {
        itemType = typeof( object );
      }

      if( pageSize < 1 )
        throw new ArgumentOutOfRangeException( "pageSize", pageSize, "pageSize must be greater than zero." );

      if( maxRealizedItemCount < 1 )
        throw new ArgumentOutOfRangeException( "maxRealizedItemCount", maxRealizedItemCount, "maxRealizedItemCount must be greater than zero." );

      if( maxRealizedItemCount < pageSize )
      {
        maxRealizedItemCount = pageSize;
      }

      m_pageSize = pageSize;
      m_maxRealizedItemCount = maxRealizedItemCount;
      m_preemptivePageQueryRatio = DataGridVirtualizingCollectionViewBase.DefaultPreemptivePageQueryRatio;
    }

    internal override DataGridCollectionViewBase CreateDetailDataGridCollectionViewBase(
      IEnumerable detailDataSource,
      DataGridDetailDescription parentDetailDescription,
      DataGridCollectionViewBase parentDataGridCollectionViewBase )
    {
      throw new NotImplementedException();
    }

    #endregion

    #region CollectionView Members

    public override bool CanGroup
    {
      get
      {
        return true;
      }
    }

    public override bool CanFilter
    {
      get
      {
        return false;
      }
    }

    public override IEnumerable SourceCollection
    {
      get
      {
        // VirtualizingDataGridCollectionView does not have a source.
        return null;
      }
    }

    public override bool Contains( object item )
    {
      if( item == null )
        return false;

      return ( this.IndexOf( item ) != -1 );
    }

    public override bool IsEmpty
    {
      get
      {
        return ( this.Count == 0 );
      }
    }

    public override int IndexOf( object item )
    {
      this.EnsureThreadAndCollectionLoaded();

      return this.RootGroup.GetGlobalIndexOf( item );
    }

    public override object GetItemAt( int index )
    {
      this.EnsureThreadAndCollectionLoaded();

      if( index < 0 )
        return null;

      return this.RootGroup.GetItemAtGlobalIndex( index );
    }

    #endregion CollectionView Members

    #region Currency management

    public override object CurrentItem
    {
      get
      {
        if( ( !this.Loaded ) || ( this.IsCurrentBeforeFirst ) || ( this.IsCurrentAfterLast ) )
          return null;

        return base.CurrentItem;
      }
    }

    public override bool MoveCurrentToPosition( int position )
    {
      if( position == this.CurrentPosition )
        return ( this.CurrentItem != null );

      this.EnsureThreadAndCollectionLoaded();
      return this.SetCurrentItem( position, true );
    }

    private bool SetCurrentItem( int newCurrentPosition, bool isCancelable )
    {
      int count = this.Count;

      if( ( newCurrentPosition < -1 ) || ( newCurrentPosition > count ) )
        throw new ArgumentOutOfRangeException( "newCurrentPosition", "The current position must be greater than -1 and less than Count." );

      object newCurrentItem = null;

      if( ( newCurrentPosition >= 0 ) && ( newCurrentPosition < count ) )
        newCurrentItem = this.GetItemAt( newCurrentPosition );

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
          int count = this.Count;

          if( beforeDeleteOperation )
          {
            Debug.Assert( count > 0 );
            count--;
          }

          bool isCurrentBeforeFirst;
          bool isCurrentAfterLast;

          if( count == 0 )
          {
            isCurrentBeforeFirst = true;
            isCurrentAfterLast = true;
          }
          else
          {
            isCurrentBeforeFirst = newCurrentPosition < 0;
            isCurrentAfterLast = newCurrentPosition >= count;
          }

#if DEBUG
          if( newCurrentItem == null )
            Debug.Assert( ( newCurrentPosition == -1 ) || ( newCurrentPosition >= ( count - 1 ) ) );
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

    private void SaveCurrentBeforeReset( out int oldCurrentPosition )
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
    }

    private void AdjustCurrencyAfterReset( int oldCurrentPosition )
    {
      if( oldCurrentPosition < 0 )
      {
        this.SetCurrentItem( -1, null, false, false );
        return;
      }

      int count = this.Count;

      if( oldCurrentPosition == int.MaxValue )
      {
        this.SetCurrentItem( count, null, false, false );
        return;
      }

      if( oldCurrentPosition >= count )
      {
        this.SetCurrentItem( count - 1, false );
        return;
      }

      this.SetCurrentItem( oldCurrentPosition, false );
    }

    #endregion Currency management

    #region IEnumerable Members

    protected override IEnumerator GetEnumerator()
    {
      this.EnsureThreadAndCollectionLoaded();

      return this.GetVirtualEnumerator();
    }

    internal abstract IEnumerator GetVirtualEnumerator();

    #endregion

    #region Count Property

    public override int Count
    {
      get
      {
        this.EnsureThreadAndCollectionLoaded();

        int count = this.RootGroup.VirtualItemCount;

        if( count > 0 )
          return count;

        return 0;
      }
    }

    #endregion Count Property

    #region DistinctValues Management

    private void ResetDistinctValues()
    {
    }

    #endregion

    #region PageSize Property

    public int PageSize
    {
      get
      {
        return m_pageSize;
      }
      set
      {
        if( m_pageSize != value )
        {
          m_pageSize = value;

          // Cannot modify this PageManager property.  Must Refresh which will recreate the CollectionViewGroupRoot
          // and the DataGridVirtualPageManagerBase taking the new pageSize into account.
          this.Refresh();

          this.OnPropertyChanged( new PropertyChangedEventArgs( "PageSize" ) );
        }
      }
    }

    #endregion PageSize Property

    #region MaxRealizedItemCount Property

    public int MaxRealizedItemCount
    {
      get
      {
        return m_maxRealizedItemCount;
      }
      set
      {
        if( m_maxRealizedItemCount != value )
        {
          m_maxRealizedItemCount = value;

          if( value < this.PageSize )
          {
            value = this.PageSize;
          }

          // Cannot modify this PageManager property.  Must Refresh which will recreate the CollectionViewGroupRoot
          // and the DataGridVirtualPageManagerBase taking the new pageSize into account.
          this.Refresh();

          this.OnPropertyChanged( new PropertyChangedEventArgs( "MaxRealizedItemCount" ) );
        }
      }
    }

    #endregion MaxRealizedItemCount Property

    #region PreemptivePageQueryRatio Property

    public double PreemptivePageQueryRatio
    {
      get
      {
        return m_preemptivePageQueryRatio;
      }
      set
      {
        if( m_preemptivePageQueryRatio != value )
        {
          m_preemptivePageQueryRatio = value;

          // Cannot modify this PageManager property.  Must Refresh which will recreate the CollectionViewGroupRoot
          // and the DataGridVirtualPageManagerBase taking the new pageSize into account.
          this.Refresh();

          this.OnPropertyChanged( new PropertyChangedEventArgs( "PreemptivePageQueryRatio" ) );
        }
      }
    }

    #endregion PreemptivePageQueryRatio Property

    #region CommitMode Property

    public CommitMode CommitMode
    {
      get
      {
        return m_commitMode;
      }
      set
      {
        if( m_commitMode != value )
        {
          m_commitMode = value;
          this.OnPropertyChanged( new PropertyChangedEventArgs( "CommitMode" ) );
        }
      }
    }

    #endregion CommitMode Property

    #region DATA VIRTUALIZATION

    public void CommitAll()
    {
      this.RootGroup.GetVirtualPageManager().CommitAll();
    }

    public event EventHandler<CommitItemsEventArgs> CommitItems;

    internal void OnCommitItems( AsyncCommitInfo asyncCommitInfo )
    {
      CommitItemsEventArgs e = new CommitItemsEventArgs( this, asyncCommitInfo );

      if( this.CommitItems != null )
        this.CommitItems( this, e );

      DataGridVirtualizingCollectionViewSourceBase source = this.ParentCollectionViewSourceBase as DataGridVirtualizingCollectionViewSourceBase;

      if( source != null )
        source.OnCommitItems( e );
    }

    public DataGridConnectionState ConnectionState
    {
      get
      {
        return m_connectionState;
      }
      private set
      {
        if( m_connectionState != value )
        {
          m_connectionState = value;
          this.RefreshConnectionState();
        }
      }
    }

    private void RefreshConnectionState()
    {
      this.OnPropertyChanged( new PropertyChangedEventArgs( "ConnectionState" ) );
      this.OnConnectionStateChanged();
    }

    public object ConnectionError
    {
      get
      {
        return m_connectionError;
      }
      private set
      {
        if( m_connectionError != value )
        {
          m_connectionError = value;
          this.RefreshConnectionError();
        }
      }
    }

    private void RefreshConnectionError()
    {
      this.OnPropertyChanged( new PropertyChangedEventArgs( "ConnectionError" ) );
      this.OnConnectionErrorChanged();
    }

    internal void RefreshConnectionStateAndError()
    {
      this.RefreshConnectionState();
      this.RefreshConnectionError();
    }

    internal void UpdateConnectionState( DataGridConnectionState connectionState, object error )
    {
      this.ConnectionState = connectionState;
      this.ConnectionError = error;
    }

    #endregion DATA VIRTUALIZATION


    #region PUBLIC METHODS

    public override void CommitNew()
    {
      // We are not calling base since the intended behavior is quite different with Virtualizing collection views.
      object currentAddItem = this.CurrentAddItem;

      if( currentAddItem == null )
        return;

      DataGridCommittingNewItemEventArgs committingNewItemEventArgs = new DataGridCommittingNewItemEventArgs( this, currentAddItem, false );

      this.RootDataGridCollectionViewBase.OnCommittingNewItem( committingNewItemEventArgs );

      if( committingNewItemEventArgs.Cancel )
        throw new DataGridException( "CommitNew was canceled." );

      if( !committingNewItemEventArgs.Handled )
        throw new InvalidOperationException( "When manually handling the item-insertion process the CreatingNewItem, CommittingNewItem, and CancelingNewItem events must all be handled." );

      // Contrarily to the data bound collection views, we do not care about the new index or new count since we will enqueue a Refresh operation.

      this.SetCurrentAddNew( null, -1 );

      this.ExecuteOrQueueSourceItemOperation( new DeferredOperation( DeferredOperation.DeferredOperationAction.Refresh, -1, null ) );
    }

    public override bool MoveCurrentTo( object item )
    {
      m_canSynchronizeSelectionWithCurrent = true;
      bool ret = base.MoveCurrentTo( item );
      m_canSynchronizeSelectionWithCurrent = false;

      return ret;
    }

    public override bool MoveCurrentToFirst()
    {
      m_canSynchronizeSelectionWithCurrent = true;
      bool ret = base.MoveCurrentToFirst();
      m_canSynchronizeSelectionWithCurrent = false;

      return ret;
    }

    public override bool MoveCurrentToLast()
    {
      m_canSynchronizeSelectionWithCurrent = true;
      bool ret = base.MoveCurrentToLast();
      m_canSynchronizeSelectionWithCurrent = false;

      return ret;
    }

    public override bool MoveCurrentToNext()
    {
      m_canSynchronizeSelectionWithCurrent = true;
      bool ret = base.MoveCurrentToNext();
      m_canSynchronizeSelectionWithCurrent = false;

      return ret;
    }

    public override bool MoveCurrentToPrevious()
    {
      m_canSynchronizeSelectionWithCurrent = true;
      bool ret = base.MoveCurrentToPrevious();
      m_canSynchronizeSelectionWithCurrent = false;

      return ret;
    }

    #endregion PUBLIC METHODS

    internal override void SetCurrentItemAndPositionCore( object currentItem, int currentPosition, bool isCurrentBeforeFirst, bool isCurrentAfterLast )
    {
      this.UpdateDataVirtualizationLockForCurrentPosition( false );

      base.SetCurrentItemAndPositionCore( currentItem, currentPosition, isCurrentBeforeFirst, isCurrentAfterLast );

      this.UpdateDataVirtualizationLockForCurrentPosition( true );
    }

    private void UpdateDataVirtualizationLockForCurrentPosition( bool applyLock )
    {
      if( base.RootGroup == null )
        return;

      if( !this.IsCurrentAfterLast && !this.IsCurrentBeforeFirst )
      {
        int currentPosition = this.CurrentPosition;

        Debug.Assert( currentPosition > -1 );

        if( applyLock )
        {
          this.RootGroup.LockGlobalIndex( currentPosition );
        }
        else
        {
          this.RootGroup.UnlockGlobalIndex( currentPosition );
        }
      }
    }


    #region DEFERRED OPERATIONS HANDLING

    internal override void ExecuteSourceItemOperation( DeferredOperation deferredOperation, out bool refreshForced )
    {
      refreshForced = false;

      switch( deferredOperation.Action )
      {
        case DeferredOperation.DeferredOperationAction.Add:
        case DeferredOperation.DeferredOperationAction.Move:
        case DeferredOperation.DeferredOperationAction.RefreshDistincValues:
          {
            Debug.Assert( false );
            break;
          }

        case DeferredOperation.DeferredOperationAction.Refresh:
        case DeferredOperation.DeferredOperationAction.Remove:
        case DeferredOperation.DeferredOperationAction.Resort:
        case DeferredOperation.DeferredOperationAction.Regroup:
          {
            this.ForceRefresh( true, false, true );
            break;
          }

        case DeferredOperation.DeferredOperationAction.Replace:
          {
            this.ReplaceSourceItem( deferredOperation.OldStartingIndex, deferredOperation.OldItems,
              deferredOperation.NewStartingIndex, deferredOperation.NewItems );
            break;
          }

        default:
          {
            base.ExecuteSourceItemOperation( deferredOperation, out refreshForced );
            break;
          }
      }
    }

    private void ReplaceSourceItem( int oldStartIndex, IList oldItems, int newStartIndex, IList newItems )
    {
      Debug.Assert( oldStartIndex == newStartIndex );
      Debug.Assert( oldItems.Count == newItems.Count );

      if( ( oldStartIndex == -1 ) || ( newStartIndex == -1 ) )
      {
        this.ForceRefresh( true, !this.Loaded, true );
        return;
      }

      int newItemCount = newItems.Count;
      int oldItemCount = oldItems.Count;
      int extraOldItemCount = oldItemCount - newItemCount;

      int currentPosition = this.CurrentPosition;

      int count = Math.Min( newItemCount, oldItemCount );

      for( int i = 0; i < count; i++ )
      {
        object oldItem = oldItems[ i ];
        object newItem = newItems[ i ];

        if( ( oldStartIndex == newStartIndex ) && ( object.Equals( oldItem, newItem ) ) )
        {
          if( this.CurrentEditItem != oldItem )
          {
            this.OnCollectionChanged( new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Replace,
              new object[] { newItem },
              new object[] { oldItem },
              oldStartIndex + i ) );
          }
        }
        else
        {
          this.OnCollectionChanged( new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Replace,
            new object[] { newItem },
            new object[] { oldItem },
            oldStartIndex + i ) );

          if( currentPosition == newStartIndex + i )
            this.SetCurrentItem( currentPosition, newItem, false, false );
        }
      }
    }

    #endregion DEFERRED OPERATIONS HANDLING

    #region INTERNAL PROPERTIES

    internal new DataGridVirtualizingCollectionViewGroupBase RootGroup
    {
      get
      {
        if( base.RootGroup == null )
          base.RootGroup = this.CreateNewRootGroup();

        return base.RootGroup as DataGridVirtualizingCollectionViewGroupBase;
      }
      set
      {
        base.RootGroup = value;
      }
    }

    internal override int SourceItemCount
    {
      get
      {
        return this.Count;
      }
    }

    internal bool CanSynchronizeSelectionWithCurrent
    {
      get
      {
        return m_canSynchronizeSelectionWithCurrent;
      }
    }

    #endregion INTERNAL PROPERTIES

    #region INTERNAL METHODS

    internal abstract DataGridVirtualizingCollectionViewGroupBase CreateNewRootGroup();

    internal override void OnBeginningEdit( DataGridItemCancelEventArgs e )
    {
      object item = e.Item;

      // We throw instead of setting e.Cancel to True because we do not want to give the developer the chance to set it back to False.
      if( item is EmptyDataItem )
        throw new DataGridException( "Cannot begin edit on an empty data item or on an item that has a pending commit async operation." );

      DataGridPageManagerBase pageManager = this.RootGroup.GetVirtualPageManager();

      if( pageManager.IsAsyncCommitQueuedForItem( item ) )
        throw new DataGridException( "Cannot begin edit on an empty data item or on an item that has a pending commit async operation." );

      base.OnBeginningEdit( e );
    }

    internal override void OnEditBegun( DataGridItemEventArgs e )
    {
      var item = e.Item;
      var pageManager = this.RootGroup.GetVirtualPageManager();

      if( !pageManager.IsItemDirty( item ) )
      {
        // First time we enter edit on this item.
        var itemProperties = this.ItemProperties;
        var count = itemProperties.Count;

        var propertyNames = new string[ count ];
        var cachedValues = new object[ count ];

        for( int i = 0; i < count; i++ )
        {
          var itemProperty = itemProperties[ i ];

          propertyNames[ i ] = PropertyRouteParser.Parse( itemProperty );
          cachedValues[ i ] = ItemsSourceHelper.GetValueFromItemProperty( itemProperty, item );
        }

        // Cache the values of the never edited before row.  This will help the developer find the corresponding row
        // in the source when times comes to commit the changes to the data source.
        pageManager.SetCachedValuesForItem( item, propertyNames, cachedValues );
      }

      base.OnEditBegun( e );
    }

    internal override void OnEditCommitted( DataGridItemEventArgs e )
    {
      var item = e.Item;
      var pageManager = this.RootGroup.GetVirtualPageManager();

      // Compare cached values with current values.  If they are the same, we can clear the old values which in turn will
      // make the item non dirty.
      var clearIsDirty = true;

      var cachedValues = pageManager.GetCachedValuesForItem( item );

      Debug.Assert( cachedValues != null );

      var itemProperties = this.ItemProperties;

      foreach( var itemProperty in itemProperties )
      {
        var currentValue = ItemsSourceHelper.GetValueFromItemProperty( itemProperty, item );

        if( !( object.Equals( currentValue, cachedValues[ itemProperty.Name ] ) ) )
        {
          clearIsDirty = false;
          break;
        }
      }

      if( clearIsDirty )
      {
        // No modification was detected.
        pageManager.ClearCachedValuesForItem( item );
      }
      else if( m_commitMode == CommitMode.EditCommitted )
      {
        pageManager.CommitAll();
      }

      base.OnEditCommitted( e );
    }

    internal override void OnEditCanceled( DataGridItemEventArgs e )
    {
      var item = e.Item;
      var pageManager = this.RootGroup.GetVirtualPageManager();

      // Compare cached values with current values.  If they are the same, we can clear the old values which in turn will
      // make the item non dirty.
      var clearIsDirty = true;

      var cachedValues = pageManager.GetCachedValuesForItem( item );

      Debug.Assert( cachedValues != null );

      var itemProperties = this.ItemProperties;

      foreach( var itemProperty in itemProperties )
      {
        var currentValue = ItemsSourceHelper.GetValueFromItemProperty( itemProperty, item );

        if( !( object.Equals( currentValue, cachedValues[ PropertyRouteParser.Parse( itemProperty ) ] ) ) )
        {
          clearIsDirty = false;
          break;
        }
      }

      if( clearIsDirty )
      {
        pageManager.ClearCachedValuesForItem( item );
      }

      base.OnEditCanceled( e );
    }

    #endregion INTERNAL METHODS

    #region INTERNAL EVENTS

    internal event EventHandler ConnectionStateChanged;

    internal void OnConnectionStateChanged()
    {
      if( this.ConnectionStateChanged != null )
        this.ConnectionStateChanged( this, EventArgs.Empty );
    }

    internal event EventHandler ConnectionErrorChanged;

    internal void OnConnectionErrorChanged()
    {
      if( this.ConnectionErrorChanged != null )
        this.ConnectionErrorChanged( this, EventArgs.Empty );
    }

    #endregion INTERNAL EVENTS


    #region PRIVATE FIELDS

    private CommitMode m_commitMode;

    private int m_pageSize;
    private double m_preemptivePageQueryRatio;
    private int m_maxRealizedItemCount;

    private object m_connectionError;
    private DataGridConnectionState m_connectionState;

    private bool m_canSynchronizeSelectionWithCurrent;

    #endregion PRIVATE FIELDS


    #region DataGridCollectionViewBase Implementation

    internal override void EnsurePosition( int globalSortedIndex )
    {
    }

    internal override void ForceRefresh( bool sendResetNotification, bool initialLoad, bool setCurrentToFirstOnInitialLoad )
    {
      if( this.Refreshing )
        throw new InvalidOperationException( "An attempt was made to refresh the DataGridVirtualizingCollectionView while it is already in the process of refreshing." );

      if( this.IsRefreshingDistinctValues )
        throw new InvalidOperationException( "An attempt was made to refresh the DataGridVirtualizingCollectionView while it is already in the process of refreshing distinct values." );

      this.SetCurrentEditItem( null );
      int oldCurrentPosition = -1;

      if( !initialLoad )
        this.SaveCurrentBeforeReset( out oldCurrentPosition );

      using( this.DeferCurrencyEvent() )
      {
        this.Refreshing = true;
        try
        {
          lock( this.SyncRoot )
          {
            lock( this.DeferredOperationManager )
            {
              this.DeferredOperationManager.ClearDeferredOperations();

              // We explicitly go through base so we do not end-up creating a RootGroup if there is none existing at the moment.
              DataGridVirtualizingCollectionViewGroupBase rootGroup = base.RootGroup as DataGridVirtualizingCollectionViewGroupBase;
              if( rootGroup != null )
              {
                DataGridPageManagerBase pageManager = rootGroup.GetVirtualPageManager();

                // The pageManager can be null when no queryable source was set yet.
                if( pageManager != null )
                {
                  // Disconnect the PageManager so that subsequent Items/Count interrogations are not processed.
                  pageManager.Disconnect();

                  // Restart all virtual lists.  The DataGridPageManagerBase will make a call to ForceRefresh once everything has been restarted if
                  // commit operations had to be made.
                  pageManager.Restart();
                }
              }

              // Ensure to clear the DistinctValues cache when refreshing
              // since the source could have changed
              this.ResetDistinctValues();

              base.RootGroup = this.CreateNewRootGroup();

              // We set the current item to -1 to prevent the developper to get an invalid position.
              // We will replace the current item to the correct one later.
              this.SetCurrentItem( -1, null, false, false );
            }
          }
        }
        finally
        {
          this.Refreshing = false;
        }

        if( initialLoad )
        {
          this.Loaded = true;
        }
        else
        {
          this.AdjustCurrencyAfterReset( oldCurrentPosition );
        }

        if( sendResetNotification )
        {
          this.OnCollectionChanged( new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Reset ) );
        }

        this.OnPropertyChanged( new PropertyChangedEventArgs( "Groups" ) );
      }
    }

    #endregion DataGridCollectionViewBase Implementation
  }
}
