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
using System.Diagnostics;
using System.Text;

namespace Xceed.Wpf.DataGrid
{
  internal class VirtualList : IList, IList<object>, INotifyCollectionChanged, IDisposable
  {
    public VirtualList( VirtualPageManager pagingManager )
      : this( pagingManager, -1 )
    {
    }

    public VirtualList( VirtualPageManager pagingManager, int virtualCount )
    {
      if( pagingManager == null )
        throw new ArgumentNullException( "pagingManager" );

      pagingManager.ManageList( this );

      m_tableOfContent = new VirtualListTableOfContent( 8 );

      m_virtualCount = virtualCount;
    }

    #region VirtualPagingManager Property

    public VirtualPageManager PagingManager
    {
      get
      {
        return m_pagingManager;
      }
    }

    #endregion

    #region VirtualCount Property

    public int VirtualCount
    {
      get
      {
        if( this.IsDisposed )
          return 0;

        if( m_virtualCount == -1 )
          this.QueryAndSetVirtualCount();

        return m_virtualCount;
      }
    }

    #endregion

    #region TableOfContent Property

    internal VirtualListTableOfContent TableOfContent
    {
      get
      {
        return m_tableOfContent;
      }
    }

    #endregion

    #region VirtualPagingManager Property

    internal VirtualPageManager VirtualPagingManager
    {
      get
      {
        return m_pagingManager;
      }
      set
      {
        // null is an acceptable value when disposing the list
        if( ( m_pagingManager != null ) && ( value != null ) )
          throw new InvalidOperationException( "An attempt was made to set a VirtualPageManager when one has already been provided." );

        m_pagingManager = value;
      }
    }

    #endregion

    #region IsDisposed Property

    internal bool IsDisposed
    {
      get
      {
        return m_flags[ ( int )VirtualItemBookFlags.Disposed ];
      }
      private set
      {
        m_flags[ ( int )VirtualItemBookFlags.Disposed ] = value;
      }
    }

    #endregion

    #region IsRestarting Property

    internal bool IsRestarting
    {
      get
      {
        return m_flags[ ( int )VirtualItemBookFlags.Restarting ];
      }
      private set
      {
        m_flags[ ( int )VirtualItemBookFlags.Restarting ] = value;
      }
    }

    #endregion

    #region HasPagePendingCommit Property

    internal bool HasPagePendingCommit
    {
      get
      {
        foreach( VirtualPage page in m_tableOfContent.VirtualPages )
        {
          if( page.IsCommitPending )
            return true;
        }

        return false;
      }
    }

    #endregion

#if DEBUG
    public override string ToString()
    {
      StringBuilder builder = new StringBuilder();

      ReadOnlyCollection<VirtualPage> virtualPages = m_tableOfContent.VirtualPages;
      int pageCount = virtualPages.Count;

      for( int i = 0; i < pageCount; i++ )
      {
        VirtualPage page = virtualPages[ i ];

        builder.Append( i.ToString() + ": Page " + page.ToString() + Environment.NewLine );
      }

      return builder.ToString();
    }
#endif

    internal int IndexOf( object item )
    {
      if( ( item == null ) || ( this.IsDisposed ) )
        return -1;

      EmptyDataItem emptyDataItem = item as EmptyDataItem;

      if( emptyDataItem != null )
      {
        if( ( emptyDataItem.ParentVirtualList == this ) && ( emptyDataItem.Index < m_virtualCount ) )
          return emptyDataItem.Index;

        return -1;
      }

      return m_tableOfContent.IndexOf( item );
    }

    internal bool IsPageDirty( int index )
    {
      VirtualPage page = this.GetPageOrDefaultForItemIndex( index, true );

      return ( page == null ) ? false : page.IsDirty;
    }

    internal bool IsItemDirty( object item )
    {
      int localIndex = this.IndexOf( item );

      if( localIndex == -1 )
        return false;

      VirtualizedItemInfo virtualizedItemInfo = this.GetVirtualizedItemInfoAtIndex( localIndex, false, true );

      return ( virtualizedItemInfo == null ) ? false : virtualizedItemInfo.IsDirty;
    }

    internal VirtualizedItemValueCollection GetCachedValuesForItemAtIndex( int index )
    {
      VirtualizedItemInfo virtualizedItemInfo = this.GetVirtualizedItemInfoAtIndex( index, false, true );

      return ( virtualizedItemInfo == null ) ? null : virtualizedItemInfo.OldValues;
    }

    internal void SetCachedValuesForItemAtIndex( int index, string[] names, object[] values )
    {
      VirtualizedItemInfo virtualizedItemInfo = this.GetVirtualizedItemInfoAtIndex( index, false, true );

      if( virtualizedItemInfo == null )
        throw new ArgumentOutOfRangeException( "index", index, "No VirtualizedItemInfo can be found at the specified index." );

      virtualizedItemInfo.OldValues = new VirtualizedItemValueCollection( names, values );
    }

    internal void ClearCachedValuesForItemAtIndex( int index )
    {
      VirtualizedItemInfo virtualizedItemInfo = this.GetVirtualizedItemInfoAtIndex( index, false, true );

      if( virtualizedItemInfo == null )
        throw new ArgumentOutOfRangeException( "index", index, "No VirtualizedItemInfo can be found at the specified index." );

      virtualizedItemInfo.OldValues = null;
    }

    internal object GetItemAt( int index )
    {
      if( ( index < 0 ) || ( index >= m_virtualCount ) )
        throw new ArgumentOutOfRangeException( "Index", index, "index must be greater than or equal to zero and less than count." );

      VirtualizedItemInfo virtualizedItemInfo = this.GetVirtualizedItemInfoAtIndex( index, true, false );
      return virtualizedItemInfo.DataItem;
    }

    internal VirtualizedItemInfo GetVirtualizedItemInfoAtIndex( int index, bool createPageIfLineNotFound, bool preventMovePageToFront )
    {
      VirtualizedItemInfo virtualizedItemInfo = null;

      VirtualPage page = this.GetPageOrDefaultForItemIndex( index, preventMovePageToFront );

      if( page != null )
      {
        virtualizedItemInfo = page.GetVirtualizedItemInfoAtIndex( index );

        Debug.Assert( virtualizedItemInfo != null );
      }
      else if( createPageIfLineNotFound )
      {
        page = this.CreateNewPage( index );

        virtualizedItemInfo = page.GetVirtualizedItemInfoAtIndex( index );
        m_pagingManager.AddPage( page, VirtualPageManager.PageInsertPosition.Front );
      }

      return virtualizedItemInfo;
    }

    internal void LockPageForLocalIndex( int sourceIndex )
    {
      Debug.Assert( m_tableOfContent.ContainsPageForSourceIndex( sourceIndex ) );
      Debug.Assert( this.GetPageOrDefaultForItemIndex( sourceIndex, true ) != null );

      VirtualPage page;
      if( m_tableOfContent.TryGetPageForSourceIndex( sourceIndex, out page ) )
      {
        if( page.LockPage() )
        {
          Debug.WriteLineIf( VirtualPageManager.DebugDataVirtualization, "List: " + this.GetHashCode().ToString() + " - LOCKING PAGE: " + page.ToString() + " for index: " + sourceIndex.ToString() + " NEW LOCKED PAGES COUNT: " + this.GetLockedPageCount().ToString() );
        }

        this.PreEmptiveLoadPages( sourceIndex, page );
      }
    }

    internal void UnlockPageForLocalIndex( int sourceIndex )
    {
      if( this.IsDisposed )
        return;

      VirtualPage page;

      if( m_tableOfContent.TryGetPageForSourceIndex( sourceIndex, out page ) )
      {
        if( page.UnlockPage() )
        {
          Debug.Assert( this.GetLockedPageCount() >= 0 );
          Debug.WriteLineIf( VirtualPageManager.DebugDataVirtualization, "List: " + this.GetHashCode().ToString() + " - UN-LOCKING PAGE: " + page.ToString() + " for index: " + sourceIndex.ToString() + " NEW LOCKED PAGES COUNT: " + this.GetLockedPageCount().ToString() );

          m_pagingManager.CleanUpAndDisposeUnused();
        }
      }
    }

    private void PreEmptiveLoadPages( int sourceIndex, VirtualPage page )
    {
      // The VirtualList is disposed or part of a PagingManager that will be disposed (only disconnected when dispose is required)
      if( !this.PagingManager.IsConnected )
        return;

      Debug.Assert( !this.IsDisposed );

      double preemptivePageQueryRatio = m_pagingManager.PreemptivePageQueryRatio;
      int pageSize = m_pagingManager.PageSize;

      double pageRatio = ( preemptivePageQueryRatio > 0.5 ) ? 0.5 : ( preemptivePageQueryRatio < 0.0 ) ? 0 : preemptivePageQueryRatio;

      double boundariesItemCount = ( pageRatio * pageSize );

      int preEmptivePageStartIndex = -1;

      if( ( page.StartDataIndex > 0 ) && ( sourceIndex < ( page.StartDataIndex + boundariesItemCount ) ) )
      {
        // Pre emptively load the previous page.
        preEmptivePageStartIndex = page.StartDataIndex - pageSize;
      }
      else if( ( page.EndDataIndex < ( m_virtualCount - 1 ) ) && ( sourceIndex > ( page.EndDataIndex - boundariesItemCount ) ) )
      {
        // Pre emptively load the next page.
        preEmptivePageStartIndex = page.EndDataIndex + 1;
      }

      if( preEmptivePageStartIndex != -1 )
      {
        VirtualPage preEmptivePage = null;

        // We do not want to move the pre-emptive page to the front if it is already created since it does not count as a legitimate user-acess.
        preEmptivePage = this.GetPageOrDefaultForItemIndex( preEmptivePageStartIndex, true );

        if( preEmptivePage == null )
        {
          // The pre-emptive page is not yet created. Let's do it and add it to the back since it is not really accessed at the moment.
          preEmptivePage = this.CreateNewPage( preEmptivePageStartIndex );
          m_pagingManager.AddPage( preEmptivePage, VirtualPageManager.PageInsertPosition.Back );
        }
      }
    }

    internal bool IsAsyncCommitQueuedForItem( object item )
    {
      VirtualPage virtualPage;

      if( m_tableOfContent.TryGetPageForItem( item, out virtualPage ) )
        return virtualPage.IsAsyncCommitInfoQueuedForItem( item );

      return false;
    }

    internal void CommitAll()
    {
      foreach( VirtualPage page in m_tableOfContent.VirtualPages )
      {
        Debug.Assert( page != null );
        Debug.Assert( page.ParentVirtualList == this );

        if( page.IsDirty )
        {
          m_pagingManager.QueueCommitData( page );
        }
      }
    }

    internal void Restart()
    {
      if( this.IsRestarting )
        return;

      Debug.WriteLineIf( VirtualPageManager.DebugDataVirtualization, "Restart VirtualList requested, checking for pages to commit or abort..." );

      this.IsRestarting = true;
      m_pagingManager.OnVirtualListRestarting( this );

      // We must keep a copy since restarting can remove pages from table of content
      int virtualPagesCount = m_tableOfContent.VirtualPages.Count;

      if( virtualPagesCount == 0 )
      {
        this.EndRestart();
      }
      else
      {
        // Restart every pages this VirtualList contains.
        // Keep a reference to the pages that need to restart in order to know when this VirtualList is restarted
        m_restartingPages.AddRange( m_tableOfContent.VirtualPages );

        for( int i = virtualPagesCount - 1; i >= 0; i-- )
        {
          VirtualPage page = m_tableOfContent.VirtualPages[ i ];
          Debug.Assert( !page.IsDisposed );

          if( !page.IsRestarting )
          {
            page.Restart();
          }
        }
      }
    }

    internal void OnVirtualPageRestarting( VirtualPage page )
    {
      // Notify the VirtualPageManager that this page is restarted to ensure it commits its data or aborts the QueryItems if already invoked
      Debug.Assert( m_restartingPages.Contains( page ) );
      m_pagingManager.OnVirtualListPageRestarting( this, page );
    }

    internal void OnVirtualPageRestarted( VirtualPage page )
    {
      Debug.Assert( m_restartingPages.Contains( page ) );

      // The page is restarted, remove it from the restarting pages
      m_restartingPages.Remove( page );

      // Notify the manager that this page is restarted in order to let it remove it from its m_pageNodes and also from this VirtualList TableOfContent.
      // NOTE: We do not remove it from the TableOfContent immediately to avoid have to insert a condition in  VirtualPageManager.RemovePage since this method
      // used widely to ensure a page is removed from the TableOfContent and from the m_pageNodes list.
      m_pagingManager.OnVirtualListPageRestarted( this, page );

      // Ensure all restarted pages completed their commit or abort operation before notifying that this list is restarted
      if( m_restartingPages.Count == 0 )
      {
        Debug.WriteLineIf( VirtualPageManager.DebugDataVirtualization, "Cleared VirtualList" );
        this.EndRestart();
      }
    }

    internal void FillEmptyPage( AsyncQueryInfo asyncQueryInfo, object[] fetchedItems )
    {
      // The VirtualList is disposed or part of a PagingManager that will be disposed (only disconnected when dispose is required)
      if( !this.PagingManager.IsConnected )
        return;

      Debug.Assert( !this.IsDisposed );
      Debug.Assert( !asyncQueryInfo.IsDisposed );

      // We do not want to move the page we are about to fill to the front since it does not count as a legitimate user-acess.  It will get moved to the front when one of its item is accessed.
      VirtualPage page = null;
      page = this.GetPageOrDefaultForItemIndex( asyncQueryInfo.StartIndex, true );

      // Although extremely rare, this situation could occur if we are calling RemovePageNode and the QueryData Dispatcher Operation 
      // which has been asyncronously invoked in CreateNewPage is raising the QueryData event at the exact moment when we
      // try to abort the dispatcher operation.  This means that the customer will have queued an async request for data
      // for a page we no longer care about, and have already removed from the Table of Content and our LinkedList.
      // This should NOT occur if the user did not abort the request and called the AsyncQueryInfo EndQuery method since AsyncQueryInfo should
      // not have invoked the EndQueryAction if its ShouldAbort property was set to true.
      if( page == null )
        return;

      Debug.Assert( !page.IsFilled );
      Debug.Assert( this.GetPageStartingIndexForItemIndex( asyncQueryInfo.StartIndex ) == asyncQueryInfo.StartIndex );

      Debug.Assert( fetchedItems.Length <= page.Count );

      if( fetchedItems.Length == page.Count )
      {
        object[] oldItems = page.ToItemArray();

        m_tableOfContent.RemovePage( page );

        page.EndQueryItems( asyncQueryInfo, fetchedItems );

        m_tableOfContent.AddPage( page );

        Debug.WriteLineIf( VirtualPageManager.DebugDataVirtualization, "Replaced TOC items/index for page: " + page.ToString() );

        this.OnCollectionChanged( new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Replace, fetchedItems, oldItems, asyncQueryInfo.StartIndex ) );
      }
      else
      {
        // The expected count was not met.  Maybe the user told us the source was bigger than it really is, or maybe there were delete operations made on the source since the last restart.
        // Let's refresh the CollectionView. This will restart the VirtualItemBook and raise the CollectionView's OnCollectionChanged Reset notification.
        this.OnCollectionChanged( new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Reset ) );
      }
    }

    internal void NotifyCommitComplete( AsyncCommitInfo asyncCommitInfo )
    {
      if( asyncCommitInfo.VirtualizedItemInfos.Length < 1 )
        throw new DataGridInternalException( "VirualizedItemInfos is empty." );

      int indexForItemInPage = asyncCommitInfo.VirtualizedItemInfos[ 0 ].Index;

      // We do not want to move the page we are about to flag has committed to the front since it does not count as a legitimate user-access.
      // It will get moved to the front when one of its items is accessed.
      VirtualPage page = null;
      page = this.GetPageOrDefaultForItemIndex( indexForItemInPage, true );

      if( page == null )
        throw new InvalidOperationException( "An attempt was made to retrieve a page does not exist." );

      if( ( !this.HasPagePendingCommit ) || ( !page.IsCommitPending ) )
        throw new InvalidOperationException( "An attempt was made to commit a page that does not have a pending commit operation." );

      Debug.Assert( page.IsDirty );

      page.EndCommitItems( asyncCommitInfo );

      // If we no longer have any pages pending commit.
      if( !this.HasPagePendingCommit )
      {
        // CleanUp and queue a request to fill empty pages from the start of the queue.
        m_pagingManager.CleanUpAndDisposeUnused();

        // This is a failsafe, to make sure that during the clean-up other commit were not queued.
        if( !this.HasPagePendingCommit )
        {
          if( !this.IsRestarting )
          {
            // After the call to cleanup, there should only be LOCKED pending fill pages remaining.  Those are the one to refetch.
            List<VirtualPage> lockedPages = this.GetLockedPages();
            int lockedPageCount = lockedPages.Count;

            for( int i = 0; i < lockedPageCount; i++ )
            {
              VirtualPage lockedPage = lockedPages[ i ];

              if( lockedPage.IsFilled )
                continue;

              // The locked page has been created while commit was pending.  Let's queue its query data operation.
              m_pagingManager.QueueQueryData( lockedPage );
            }
          }
          else
          {
            this.Restart();
          }
        }
      }
    }

    private void EndRestart()
    {
      Debug.Assert( m_restartingPages.Count == 0 );

      m_virtualCount = -1;
      this.IsRestarting = false;

      Debug.WriteLineIf( VirtualPageManager.DebugDataVirtualization, "VirtualList restarted" );

      m_pagingManager.OnVirtualListRestarted( this );

      this.Dispose();
    }

    private int GetPageStartingIndexForItemIndex( int itemIndex )
    {
      int pageSize = m_pagingManager.PageSize;
      return ( itemIndex / pageSize ) * pageSize;
    }

    private VirtualPage CreateNewPage( int itemIndex )
    {
      Debug.Assert( !m_tableOfContent.ContainsPageForSourceIndex( itemIndex ) );

      int pageStartIndex = this.GetPageStartingIndexForItemIndex( itemIndex );

      int pageSize = m_pagingManager.PageSize;

      int expectedItemCount = Math.Min( pageSize, ( m_virtualCount - pageStartIndex ) );
      expectedItemCount = Math.Max( 0, expectedItemCount );

      VirtualPage page = VirtualPage.CreateEmptyPage( this, pageStartIndex, expectedItemCount );

      m_tableOfContent.AddPage( page );

      // If we have a pending commit page, this brandly new created page will get its query data queued when we are notified of a commit completed and that we no longer have any pages awaiting commiting.
      if( !this.HasPagePendingCommit )
      {
        m_pagingManager.QueueQueryData( page );
      }

      Debug.WriteLineIf( VirtualPageManager.DebugDataVirtualization, "Creating VirtualItemPlaceHolder for page: " + page.ToString() );

      return page;
    }

    private VirtualPage GetPageOrDefaultForItemIndex( int index, bool preventMoveToFront )
    {
      VirtualPage page = null;

      if( m_tableOfContent.TryGetPageForSourceIndex( index, out page ) )
      {
        if( !preventMoveToFront )
          m_pagingManager.MovePageToFront( page );
      }

      return page;
    }

    private int GetLockedPageCount()
    {
      int lockedPageCount = 0;

      foreach( VirtualPage page in m_tableOfContent.VirtualPages )
      {
        Debug.Assert( page != null );

        if( ( page != null ) && ( page.IsLocked ) )
          lockedPageCount++;
      }

      return lockedPageCount;
    }

    private List<VirtualPage> GetLockedPages()
    {
      List<VirtualPage> lockedPages = new List<VirtualPage>();

      foreach( VirtualPage page in m_tableOfContent.VirtualPages )
      {
        Debug.Assert( page != null );

        if( ( page != null ) && ( page.IsLocked ) )
        {
          lockedPages.Add( page );
        }
      }

      return lockedPages;
    }

    private void QueryAndSetVirtualCount()
    {
      int count = m_pagingManager.OnQueryItemCount( this );

      Debug.WriteLineIf( VirtualPageManager.DebugDataVirtualization, ( count != -1 ) ? "QUERY VIRTUAL COUNT: " + count.ToString() : "QUERY VIRTUAL COUNT NOT HANDLED, SETTING COUNT TO ZERO." );

      if( count == -1 )
      {
        count = 0;
      }

      m_virtualCount = count;
    }

    private void OnCollectionChanged( NotifyCollectionChangedEventArgs e )
    {
      var handler = this.CollectionChanged;
      if( handler == null )
        return;

      handler.Invoke( this, e );
    }

    #region IList<object> Members

    int IList<object>.IndexOf( object item )
    {
      return this.IndexOf( item );
    }

    void IList<object>.Insert( int index, object item )
    {
      throw new NotImplementedException();
    }

    void IList<object>.RemoveAt( int index )
    {
      throw new NotImplementedException();
    }

    object IList<object>.this[ int index ]
    {
      get
      {
        return this.GetItemAt( index );
      }
      set
      {
        throw new NotImplementedException();
      }
    }

    #endregion

    #region ICollection<object> Members

    void ICollection<object>.Add( object item )
    {
      throw new NotImplementedException();
    }

    void ICollection<object>.Clear()
    {
      throw new NotImplementedException();
    }

    bool ICollection<object>.Contains( object item )
    {
      return ( this.IndexOf( item ) != -1 );
    }

    void ICollection<object>.CopyTo( object[] array, int arrayIndex )
    {
      throw new NotImplementedException();
    }

    int ICollection<object>.Count
    {
      get
      {
        return this.VirtualCount;
      }
    }

    bool ICollection<object>.IsReadOnly
    {
      get
      {
        return true;
      }
    }

    bool ICollection<object>.Remove( object item )
    {
      throw new NotImplementedException();
    }

    #endregion

    #region IEnumerable<object> Members

    IEnumerator<object> IEnumerable<object>.GetEnumerator()
    {
      return new VirtualListEnumerator( this );
    }

    #endregion

    #region IEnumerable Members

    IEnumerator IEnumerable.GetEnumerator()
    {
      return ( ( IEnumerable<object> )this ).GetEnumerator();
    }

    #endregion

    #region INotifyCollectionChanged Members

    public event NotifyCollectionChangedEventHandler CollectionChanged;

    #endregion

    #region IList Members

    int IList.Add( object value )
    {
      throw new NotImplementedException();
    }

    void IList.Clear()
    {
      throw new NotImplementedException();
    }

    bool IList.Contains( object value )
    {
      return this.IndexOf( value ) != -1;
    }

    int IList.IndexOf( object value )
    {
      return this.IndexOf( value );
    }

    void IList.Insert( int index, object value )
    {
      throw new NotImplementedException();
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
      throw new NotImplementedException();
    }

    void IList.RemoveAt( int index )
    {
      throw new NotImplementedException();
    }

    object IList.this[ int index ]
    {
      get
      {
        return this.GetItemAt( index );
      }
      set
      {
        throw new NotImplementedException();
      }
    }

    #endregion

    #region ICollection Members

    void ICollection.CopyTo( Array array, int index )
    {
      throw new NotImplementedException();
    }

    int ICollection.Count
    {
      get
      {
        return this.VirtualCount;
      }
    }

    bool ICollection.IsSynchronized
    {
      get
      {
        throw new NotImplementedException();
      }
    }

    object ICollection.SyncRoot
    {
      get
      {
        throw new NotImplementedException();
      }
    }

    #endregion

    #region IDisposable Members

    public void Dispose()
    {

      if( m_tableOfContent != null )
      {
        Debug.Assert( m_tableOfContent.VirtualPages.Count == 0 );

        m_tableOfContent.Dispose();
        m_tableOfContent = null;
      }

      m_pagingManager = null;
      this.IsDisposed = true;

    }

    #endregion

    private List<VirtualPage> m_restartingPages = new List<VirtualPage>();
    private VirtualPageManager m_pagingManager;
    private BitVector32 m_flags;
    private int m_virtualCount;
    private VirtualListTableOfContent m_tableOfContent;

    [Flags]
    private enum VirtualItemBookFlags
    {
      Restarting = 1,
      Disposed = 2,
    }
  }
}
