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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Windows.Threading;

namespace Xceed.Wpf.DataGrid
{
  internal class VirtualPage : List<VirtualizedItemInfo>, IDisposable
  {
    internal static VirtualPage CreateEmptyPage( VirtualList parentVirtualList, int startSourceIndex, int entryCount )
    {
      if( parentVirtualList == null )
        throw new ArgumentNullException( "parentVirtualList" );

      if( startSourceIndex < 0 )
        throw new ArgumentOutOfRangeException( "startSourceIndex", startSourceIndex, "startSourceIndex must be greater than or equal to zero." );

      if( entryCount < 0 )
        throw new ArgumentOutOfRangeException( "entryCount", entryCount, "entryCount must be greater than or equal to zero." );

      EmptyDataItem[] emptyDataItems = new EmptyDataItem[ entryCount ];
      for( int i = 0; i < entryCount; i++ )
      {
        emptyDataItems[ i ] = new EmptyDataItem( startSourceIndex + i, parentVirtualList );
      }

      VirtualPage emptyDataItemPage = new VirtualPage( parentVirtualList, startSourceIndex, emptyDataItems );
      emptyDataItemPage.IsFilled = false;

      return emptyDataItemPage;
    }

    private VirtualPage( VirtualList parentVirtualList, int startSourceIndex, object[] dataItems )
      : base( dataItems.Length )
    {
      if( parentVirtualList == null )
        throw new ArgumentNullException( "parentVirtualList" );

      if( startSourceIndex < 0 )
        throw new ArgumentOutOfRangeException( "startSourceIndex", startSourceIndex, "startSourceIndex must be greater than or equal to zero." );

      m_startDataIndex = startSourceIndex;
      m_asyncCommitInfoList = new List<AsyncCommitInfo>();

      m_parentVirtualList = parentVirtualList;

      int length = dataItems.Length;
      for( int i = 0; i < length; i++ )
      {
        Debug.Assert( dataItems[ i ] != null );
        this.Add( new VirtualizedItemInfo( m_startDataIndex + i, dataItems[ i ] ) );
      }

      this.IsFilled = true;
    }

    #region RemoveAfterOperation Property

    public bool RemoveAfterOperation
    {
      get;
      set;
    }

    #endregion

    #region IsAborting Property

    public bool IsAborting
    {
      get;
      private set;
    }

    #endregion

    #region IsRestarting Property

    public bool IsRestarting
    {
      get;
      private set;
    }

    #endregion

    #region IsEmpty Property

    public bool IsEmpty
    {
      get
      {
        return ( this.Count == 0 );
      }
    }

    #endregion

    #region IsFilled Property

    public bool IsFilled
    {
      get
      {
        return m_flags[ ( int )VirtualItemPageFlags.IsFilled ];
      }
      private set
      {
        // Wether we are setting PendingFill to True or to False, we should not be touching this property if we are pending commit.
        Debug.Assert( !this.IsCommitPending );

        m_flags[ ( int )VirtualItemPageFlags.IsFilled ] = value;
      }
    }

    #endregion

    #region IsDisposed Read-Only Property

    public bool IsDisposed
    {
      get
      {
        return m_flags[ ( int )VirtualItemPageFlags.IsDisposed ];
      }
      private set
      {
        m_flags[ ( int )VirtualItemPageFlags.IsDisposed ] = value;
      }
    }

    #endregion

    #region IsCommitPending Property

    public bool IsCommitPending
    {
      get
      {
        return m_asyncCommitInfoList.Count > 0;
      }
    }

    #endregion

    #region IsDirty Property

    public bool IsDirty
    {
      get
      {
        if( !this.IsFilled )
          return false;

        int count = this.Count;

        for( int i = 0; i < count; i++ )
        {
          if( this[ i ].IsDirty )
            return true;
        }

        return false;
      }
    }

    #endregion

    #region IsLocked Property

    public bool IsLocked
    {
      get
      {
        return ( m_lockCount > 0 );
      }
    }

    #endregion

    #region IsRemovable Property

    public bool IsRemovable
    {
      get
      {
        // A page which IsPendingFill is removable since it hasn't even been filled yet.
        return ( ( !this.IsLocked ) && ( !this.IsCommitPending ) );
      }
    }

    #endregion

    #region ParentVirtualList Property

    public VirtualList ParentVirtualList
    {
      get
      {
        return m_parentVirtualList;
      }
    }

    #endregion

    #region StartDataIndex Property

    public int StartDataIndex
    {
      get
      {
        return m_startDataIndex;
      }
    }

    #endregion

    #region EndDataIndex Property

    public int EndDataIndex
    {
      get
      {
        return m_startDataIndex + ( this.Count - 1 );
      }
    }

    #endregion

    public VirtualizedItemInfo GetVirtualizedItemInfoAtIndex( int sourceIndex )
    {
      int index = this.SourceIndexToPageEntryIndex( sourceIndex );

      if( index != -1 )
        return this[ index ];

      return null;
    }

#if DEBUG
    public override string ToString()
    {
      string representation = string.Empty;

      if( !this.IsFilled )
      {
        representation += "Fill Pending - ";
      }

      if( this.IsEmpty )
      {
        representation += m_startDataIndex.ToString() + " - EMPTY.";
      }
      else
      {
        representation += m_startDataIndex.ToString() + " - " + this.EndDataIndex.ToString();
      }

      return representation;
    }
#endif

    public object[] ToItemArray()
    {
      int count = this.Count;

      object[] items = new object[ count ];

      for( int i = 0; i < count; i++ )
      {
        items[ i ] = this[ i ].DataItem;
      }

      return items;
    }

    internal bool LockPage()
    {
      m_lockCount++;

      // Returns True if the page just became locked (and is not up for removal) or False if the call simply incremented the lock count.
      return m_lockCount == 1;
    }

    internal bool UnlockPage()
    {
      if( m_lockCount > 0 )
      {
        m_lockCount--;
      }
      else
      {
        // Safety Net.  We return False, even though the lock count is reinitialized to zero, since the page did not just become unlocked.
        // This can occur when the DataGridVirtualizingCollectionView hits ForceRefresh method restarts the VirtualItemBook, thus getting rid
        // of locked and unlocked pages and THEN, the Generator's WeakEventListener of CollectionChanged cleans up its containers which in turn
        // tries to unlock a source index which isn't locked at all (since it was cleared when the virtual item book reseted).
        m_lockCount = 0;
        return false;
      }

      // Returns True if the page just became unlocked (and is up for removal) or False if the call simply decremented the lock count.
      return m_lockCount == 0;
    }

    internal void AbortQueryDataOperation()
    {
      Debug.Assert( !this.IsDisposed );

      if( this.IsFilled )
        throw new InvalidOperationException( "An attempt was made to abort a query that has already completed." );

      if( m_asyncQueryInfo != null )
      {
        // This method can be reentered by another thread, while it's already being processed, with the result that the AbortQuery delegate may not be trigged by the CollectionView,
        // and hence the client code not prevented it must abort the query/operation/connection.
        lock( this )
        {
          if( this.IsAborting )
            return;

          this.IsAborting = true;
        }

        m_asyncQueryInfo.AbortQuery();
      }
    }

    internal void QueueQueryData( Dispatcher dispatcher )
    {
      Debug.Assert( !this.IsDisposed );
      Debug.Assert( m_asyncQueryInfo == null );

      m_asyncQueryInfo = new AsyncQueryInfo(
        dispatcher,
        new Action<AsyncQueryInfo>( this.AsyncQueryInfo_BeginQueryItems ),
        new Action<AsyncQueryInfo>( this.AsyncQueryInfo_AbortQueryItems ),
        new Action<AsyncQueryInfo, object[]>( this.AsyncQueryInfo_EndQueryItems ),
        new Action<AsyncQueryInfo>( this.AsyncQueryInfo_QueryErrorChanged ),
        new Action<AsyncQueryInfo>( this.AsyncQueryInfo_BuiltInAbort ),
        m_startDataIndex,
        this.Count );

      m_asyncQueryInfo.QueueQuery();
    }

    internal void EndQueryItems( AsyncQueryInfo asyncQueryInfo, object[] items )
    {
      Debug.Assert( !this.IsDisposed );

      // This can occur when the user notify us that the QueryData is completed for an AsyncQueryInfo.StartIndex that refers to a Page which does exists
      // but that was removed, then re-created thus creating another asyncQueryInfo and queuing another QueryData.
      // The only way to get rid of this situation would be to keep a ref to the queued asyncQueryInfo even if we get rid of the page and re-link the same instance to the newly
      // created page.  This optimization could be done in a future version.  For now, let's return and the second asyncQueryInfo will take care of filling the newly created page.
      if( m_asyncQueryInfo != asyncQueryInfo )
      {
        Debug.Assert( false );

        return;
      }

      if( this.IsFilled )
        throw new InvalidOperationException( "An attempt was made to fill a virtual page that is already filled." );

      if( items == null )
        throw new ArgumentNullException( "items" );

      for( int i = 0; i < items.Length; i++ )
      {
        VirtualizedItemInfo virtualizedItemInfo = this[ i ];

        Debug.Assert( virtualizedItemInfo.DataItem is EmptyDataItem );
        Debug.Assert( virtualizedItemInfo.Index == ( m_startDataIndex + i ) );

        this[ i ].DataItem = items[ i ];
      }

      this.IsFilled = true;

      Debug.WriteLineIf( VirtualPageManager.DebugDataVirtualization, "Page Filled - " + this.ToString() );
    }

    internal void QueueCommitData( Dispatcher dispatcher )
    {
      Debug.Assert( !this.IsDisposed );

      int count = this.Count;

      List<VirtualizedItemInfo> dirtyItemInfoNotAlreadyPendingCommit = new List<VirtualizedItemInfo>( count );

      for( int i = 0; i < count; i++ )
      {
        VirtualizedItemInfo virtualizedItemInfo = this[ i ];

        if( ( virtualizedItemInfo.IsDirty ) && ( !this.IsAsyncCommitInfoQueuedForItem( virtualizedItemInfo.DataItem ) ) )
        {
          Debug.WriteLineIf( VirtualPageManager.DebugDataVirtualization, "QueueCommitData for page " + this.ToString() + " and index " + virtualizedItemInfo.Index );

          dirtyItemInfoNotAlreadyPendingCommit.Add( virtualizedItemInfo );
        }
      }

      if( dirtyItemInfoNotAlreadyPendingCommit.Count > 0 )
      {
        AsyncCommitInfo asyncCommitInfo = new AsyncCommitInfo(
          dispatcher,
          new Action<AsyncCommitInfo>( this.AsyncCommitInfo_BeginCommitItems ),
          new Action<AsyncCommitInfo>( this.AsyncCommitInfo_EndCommitItems ),
          new Action<AsyncCommitInfo>( this.AsyncCommitInfo_CommitErrorChanged ),
          dirtyItemInfoNotAlreadyPendingCommit.ToArray() );

        m_asyncCommitInfoList.Add( asyncCommitInfo );
        asyncCommitInfo.BeginCommit();
      }
    }

    internal bool IsAsyncCommitInfoQueuedForItem( object item )
    {
      int pendingAsyncCommitInfoCount = m_asyncCommitInfoList.Count;

      for( int i = 0; i < pendingAsyncCommitInfoCount; i++ )
      {
        VirtualizedItemInfo[] pendingCommitVirtualizedItemInfos = m_asyncCommitInfoList[ i ].VirtualizedItemInfos;

        foreach( VirtualizedItemInfo pendingCommitVirtualizedItemInfo in pendingCommitVirtualizedItemInfos )
        {
          if( pendingCommitVirtualizedItemInfo.DataItem == item )
            return true;
        }
      }

      return false;
    }

    internal void EndCommitItems( AsyncCommitInfo asyncCommitInfo )
    {
      Debug.Assert( !this.IsDisposed );
      Debug.Assert( m_asyncCommitInfoList.Contains( asyncCommitInfo ) );

      VirtualizedItemInfo[] commitedItemInfos = asyncCommitInfo.VirtualizedItemInfos;

      for( int i = 0; i < commitedItemInfos.Length; i++ )
      {
        commitedItemInfos[ i ].OldValues = null;
      }

      m_asyncCommitInfoList.Remove( asyncCommitInfo );

      asyncCommitInfo.Dispose();
    }

    internal void Restart()
    {
      if( !this.IsRestarting )
      {
        Debug.WriteLineIf( VirtualPageManager.DebugDataVirtualization, "Restart VirtualPage requested for " + this.ToString() );

        this.IsRestarting = true;
        this.ParentVirtualList.OnVirtualPageRestarting( this );
      }

      // The page has finished commiting or aborting
      if( ( !this.IsCommitPending ) && ( !this.IsAborting ) )
      {
        this.EndRestart();
      }
    }

    internal void EndRestart()
    {
      // VirtualPage will be disposed by the VirtualPageManager when it removes the page from is m_pageNodeList
      Debug.WriteLineIf( VirtualPageManager.DebugDataVirtualization, "Restart VirtualPage completed for " + this.ToString() );

      this.IsRestarting = false;
      this.ParentVirtualList.OnVirtualPageRestarted( this );
    }

    private void AsyncQueryInfo_BuiltInAbort( AsyncQueryInfo queryInfo )
    {
      if( this.IsDisposed )
        return;

      Debug.Assert( ( this.ParentVirtualList != null ) || ( this.ParentVirtualList.PagingManager != null ) );

      this.ParentVirtualList.PagingManager.OnBuiltInAbort( this, queryInfo );
      this.IsAborting = false;
    }

    private void AsyncQueryInfo_BeginQueryItems( AsyncQueryInfo queryInfo )
    {
      if( this.IsDisposed )
        return;

      Debug.Assert( ( this.ParentVirtualList != null ) || ( this.ParentVirtualList.PagingManager != null ) );

      this.ParentVirtualList.PagingManager.OnQueryItems( this, queryInfo );
    }

    private void AsyncQueryInfo_AbortQueryItems( AsyncQueryInfo queryInfo )
    {
      if( this.IsDisposed )
        return;

      Debug.Assert( ( this.ParentVirtualList != null ) || ( this.ParentVirtualList.PagingManager != null ) );

      this.ParentVirtualList.PagingManager.OnAbortQueryItems( this, queryInfo );

      this.IsAborting = false;

      // If the page was removed, it was also disposed. This case means the page was not restarting
      if( !this.RemoveAfterOperation && this.ParentVirtualList.IsRestarting )
      {
        this.Restart();
      }
    }

    private void AsyncQueryInfo_EndQueryItems( AsyncQueryInfo queryInfo, object[] fetchedItems )
    {
      if( this.IsDisposed )
        return;

      Debug.Assert( ( this.ParentVirtualList != null ) || ( this.ParentVirtualList.PagingManager != null ) );
      Debug.Assert( !this.IsAborting );

      // The page can be removed when the CollectionView raises a CollectionChanged and the selection is consequently updated, so make sure to lock it until the update operation is done.
      this.LockPage();

      this.ParentVirtualList.PagingManager.OnQueryItemsCompleted( this, queryInfo, fetchedItems );

      this.UnlockPage();

      if( this.ParentVirtualList.IsRestarting )
      {
        this.Restart();
      }
    }

    private void AsyncQueryInfo_QueryErrorChanged( AsyncQueryInfo queryInfo )
    {
      if( this.IsDisposed )
        return;

      Debug.Assert( ( this.ParentVirtualList != null ) || ( this.ParentVirtualList.PagingManager != null ) );

      this.ParentVirtualList.PagingManager.OnQueryErrorChanged( this, queryInfo );

      if( this.ParentVirtualList.IsRestarting )
      {
        this.Restart();
      }
    }

    private void AsyncCommitInfo_BeginCommitItems( AsyncCommitInfo commitInfo )
    {
      if( this.IsDisposed )
        return;

      Debug.Assert( ( this.ParentVirtualList != null ) || ( this.ParentVirtualList.PagingManager != null ) );

      this.ParentVirtualList.PagingManager.OnCommitItems( this, commitInfo );
    }

    private void AsyncCommitInfo_EndCommitItems( AsyncCommitInfo commitInfo )
    {
      if( this.IsDisposed )
        return;

      Debug.Assert( ( this.ParentVirtualList != null ) || ( this.ParentVirtualList.PagingManager != null ) );

      this.ParentVirtualList.PagingManager.OnCommitItemsCompleted( this, commitInfo );

      // Make sure the page has not been removed by the previous call before accessing the parent virtual list.
      if( !this.IsDisposed && this.ParentVirtualList.IsRestarting )
      {
        this.Restart();
      }
    }

    private void AsyncCommitInfo_CommitErrorChanged( AsyncCommitInfo commitInfo )
    {
      if( this.IsDisposed )
        return;

      Debug.Assert( ( this.ParentVirtualList != null ) || ( this.ParentVirtualList.PagingManager != null ) );

      this.ParentVirtualList.PagingManager.OnCommitErrorChanged( this, commitInfo );

      if( this.ParentVirtualList.IsRestarting )
      {
        this.Restart();
      }
    }

    private int SourceIndexToPageEntryIndex( int sourceIndex )
    {
      if( ( this.Count != 0 ) && ( ( sourceIndex >= m_startDataIndex ) && ( sourceIndex <= this.EndDataIndex ) ) )
        return sourceIndex - m_startDataIndex;

      return -1;
    }

    private bool ContainsItem( object item )
    {
      int count = this.Count;

      for( int i = 0; i < count; i++ )
      {
        if( this[ i ].DataItem == item )
          return true;
      }

      return false;
    }

    #region IDisposable Members

    public void Dispose()
    {
      Debug.Assert( !this.IsDisposed );
      Debug.Assert( ( m_asyncCommitInfoList != null ) && ( m_asyncCommitInfoList.Count == 0 ), "Some async commit are not completed while disposing VirtualPage" );

      if( m_asyncQueryInfo != null )
      {
        // We must dispose the AsyncQueryInfo to be sure it does not root this VirtualPage instance
        m_asyncQueryInfo.Dispose();
        m_asyncQueryInfo = null;
      }

      this.Clear();
      m_parentVirtualList = null;

      this.IsDisposed = true;

    }

    #endregion

    private BitVector32 m_flags;

    private int m_startDataIndex;
    private int m_lockCount;

    private VirtualList m_parentVirtualList;

    public AsyncQueryInfo m_asyncQueryInfo;
    private List<AsyncCommitInfo> m_asyncCommitInfoList;

    [Flags]
    private enum VirtualItemPageFlags
    {
      IsFilled = 1,
      IsDisposed = 2,
    }
  }
}
