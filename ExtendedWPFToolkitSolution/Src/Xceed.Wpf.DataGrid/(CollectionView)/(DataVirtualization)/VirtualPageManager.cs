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
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Windows.Threading;

namespace Xceed.Wpf.DataGrid
{
  internal abstract class VirtualPageManager : IDisposable
  {
    internal static bool DebugDataVirtualization = false;
    private const DispatcherPriority CommitDataPriority = DispatcherPriority.Input;
    private const int EstimatedLockedPageCount = 3;

    public VirtualPageManager( Dispatcher dispatcher, int pageSize, int maxRealizedItemCount, double preemptivePageQueryRatio )
    {
      if( dispatcher == null )
        throw new ArgumentNullException( "dispatcher" );

      if( pageSize < 1 )
        throw new ArgumentOutOfRangeException( "pageSize", pageSize, "pageSize must be greater than zero." );

      if( maxRealizedItemCount < pageSize )
        throw new ArgumentOutOfRangeException( "maxRealizedItemCount", maxRealizedItemCount, "maxRealizedItemCount must be greater than or equal to pageSize." );

      m_managedLists = new List<VirtualList>();
      m_pageNodes = new LinkedList<VirtualPage>();

      this.Dispatcher = dispatcher;

      m_pageSize = pageSize;
      m_maxRealizedItemCount = maxRealizedItemCount;

      m_maxRemovablePageCount = ( m_maxRealizedItemCount / m_pageSize );

      m_preemptivePageQueryRatio = preemptivePageQueryRatio;

      this.IsConnected = true;
    }

    #region Dispatcher Property

    public Dispatcher Dispatcher
    {
      get;
      private set;
    }

    #endregion

    #region PageSize Property

    public int PageSize
    {
      get
      {
        return m_pageSize;
      }
    }

    #endregion

    #region MaxRealizedItemCount Property

    public int MaxRealizedItemCount
    {
      get
      {
        return m_maxRealizedItemCount;
      }
    }

    #endregion

    #region PreemptivePageQueryRatio Property

    public double PreemptivePageQueryRatio
    {
      get
      {
        return m_preemptivePageQueryRatio;
      }
      set
      {
        m_preemptivePageQueryRatio = value;
      }
    }

    #endregion

    #region ManagedLists Property

    public ReadOnlyCollection<VirtualList> ManagedLists
    {
      get
      {
        if( m_readOnlyManagedLists == null )
        {
          m_readOnlyManagedLists = new ReadOnlyCollection<VirtualList>( m_managedLists );
        }

        return m_readOnlyManagedLists;
      }
    }

    #endregion

    #region IsDisposed Property

    protected bool IsDisposed
    {
      get
      {
        return m_flags[ ( int )VirtualPageManagerFlags.IsDisposed ];
      }
      private set
      {
        m_flags[ ( int )VirtualPageManagerFlags.IsDisposed ] = value;
      }
    }

    #endregion

    #region EstimatedTotalPageCount Property

    internal int EstimatedTotalPageCount
    {
      get
      {
        return m_maxRemovablePageCount + VirtualPageManager.EstimatedLockedPageCount;
      }
    }

    #endregion

    #region Version Property

    internal int Version
    {
      get
      {
        return m_version;
      }
    }

    #endregion

    #region IsConnected Property

    internal bool IsConnected
    {
      get
      {
        return m_flags[ ( int )VirtualPageManagerFlags.IsConnected ];
      }
      private set
      {
        m_flags[ ( int )VirtualPageManagerFlags.IsConnected ] = value;
      }
    }

    #endregion

    #region RestartingManager Property

    private bool RestartingManager
    {
      get
      {
        return m_flags[ ( int )VirtualPageManagerFlags.RestartingManager ];
      }
      set
      {
        m_flags[ ( int )VirtualPageManagerFlags.RestartingManager ] = value;
      }
    }

    #endregion

    #region ShouldRefreshAfterRestart Property

    private bool ShouldRefreshAfterRestart
    {
      get
      {
        return m_flags[ ( int )VirtualPageManagerFlags.ShouldRefreshAfterRestart ];
      }
      set
      {
        m_flags[ ( int )VirtualPageManagerFlags.ShouldRefreshAfterRestart ] = value;
      }
    }

    #endregion

    #region LastRemovable Property

    private LinkedListNode<VirtualPage> LastRemovable
    {
      get
      {
        LinkedListNode<VirtualPage> lastRemovableNode = m_pageNodes.Last;

        while( lastRemovableNode != null )
        {
          Debug.Assert( lastRemovableNode.Value != null );

          VirtualPage page = lastRemovableNode.Value;

          if( page.IsRemovable )
            return lastRemovableNode;

          lastRemovableNode = lastRemovableNode.Previous;
        }

        return lastRemovableNode;
      }
    }

    #endregion

    protected abstract int OnQueryItemCountCore( VirtualList virtualList );

    protected virtual void OnVirtualPageManagerRestarting()
    {
    }

    protected virtual void OnVirtualPageManagerRestarted( bool shouldRefresh )
    {
    }

    protected virtual void EndRestart()
    {
      this.RestartingManager = false;

      Debug.WriteLineIf( VirtualPageManager.DebugDataVirtualization, "VirtualPageManager (" + this.GetHashCode() + ")- All virtual Lists restarted." );

      this.OnVirtualPageManagerRestarted( this.ShouldRefreshAfterRestart );
      this.ShouldRefreshAfterRestart = false;
    }

    protected internal abstract void OnBuiltInAbort( VirtualPage virtualPage, AsyncQueryInfo queryInfo );

    protected internal virtual int OnQueryItemCount( VirtualList virtualList )
    {
      return this.OnQueryItemCountCore( virtualList );
    }

    protected internal abstract void OnQueryItems( VirtualPage page, AsyncQueryInfo queryInfo );

    protected internal virtual void OnAbortQueryItems( VirtualPage page, AsyncQueryInfo queryInfo )
    {
    }

    protected internal virtual void OnQueryItemsCompleted( VirtualPage page, AsyncQueryInfo queryInfo, object[] fetchedItems )
    {
      this.IncrementVersion();

      page.ParentVirtualList.FillEmptyPage( queryInfo, fetchedItems );
    }

    protected internal virtual void OnCommitItems( VirtualPage page, AsyncCommitInfo commitInfo )
    {
    }

    protected internal virtual void OnCommitItemsCompleted( VirtualPage page, AsyncCommitInfo commitInfo )
    {
      page.ParentVirtualList.NotifyCommitComplete( commitInfo );
    }

    protected internal virtual void OnQueryErrorChanged( VirtualPage page, AsyncQueryInfo queryInfo )
    {
    }

    protected internal virtual void OnCommitErrorChanged( VirtualPage page, AsyncCommitInfo commitInfo )
    {
    }

    internal virtual void OnVirtualListRestarting( VirtualList virtualList )
    {
      Debug.Assert( m_managedLists.Contains( virtualList ) );
      Debug.Assert( this.RestartingManager, "Until CollectionViewGroups can be virtualized, we should not be restarting a leaf list on its own." );
    }

    internal virtual void OnVirtualListRestarted( VirtualList virtualList )
    {
      Debug.Assert( m_managedLists.Contains( virtualList ) );
      Debug.Assert( this.RestartingManager, "Until CollectionViewGroups can be virtualized, we should not be restarting a leaf list on its own." );

      if( this.RestartingManager )
      {
        m_restartingListsCount--;
      }

      // Make sure that no page nodes belonging to this virtual list are left in the linked list. Remove all remaining ones since after the manager is restarted, its content is completely cleared.
      LinkedListNode<VirtualPage> pageNode = m_pageNodes.Last;
      while( pageNode != null )
      {
        LinkedListNode<VirtualPage> previousNode = pageNode.Previous;

        if( pageNode.Value.ParentVirtualList == virtualList )
          throw new DataGridInternalException( "A VirtualPage was not remove from its parent VirtualList after it is restarted" );

        pageNode = previousNode;
      }

      this.IncrementVersion();

      // If the manager is restarting, no page left and no more list restarting
      if( this.RestartingManager && ( m_pageNodes.Count == 0 ) && ( m_restartingListsCount == 0 ) )
      {
        this.EndRestart();
      }
    }

    internal virtual void OnVirtualListPageRestarting( VirtualList virtualList, VirtualPage page )
    {
      Debug.Assert( m_managedLists.Contains( virtualList ) );

      LinkedListNode<VirtualPage> pageNode = m_pageNodes.Find( page );

      Debug.Assert( pageNode != null );

      // RemovePageNode takes care of either raising the AbortQueryData event or aborting the QueryData Dispatcher Operation altogether.
      // It also takes care of raising the CommitVirtualData event for loaded pages which contains modified data.
      this.QueueCommitDataOrAbortIfRequired( pageNode, false );
    }

    internal virtual void OnVirtualListPageRestarted( VirtualList virtualList, VirtualPage page )
    {
      Debug.Assert( m_managedLists.Contains( virtualList ) );
      Debug.Assert( m_pageNodes.Contains( page ) );

      this.RemovePage( page );
    }

    internal void QueueQueryData( VirtualPage page )
    {
      Debug.Assert( m_managedLists.Contains( page.ParentVirtualList ) );

      page.QueueQueryData( this.Dispatcher );
    }

    internal void QueueCommitData( VirtualPage page )
    {
      Debug.WriteLineIf( VirtualPageManager.DebugDataVirtualization, "QueueCommitData for page " + page.ToString() );
      Debug.Assert( m_managedLists.Contains( page.ParentVirtualList ) );
      Debug.Assert( page.IsFilled );
      Debug.Assert( page.IsDirty );

      if( this.RestartingManager )
      {
        this.ShouldRefreshAfterRestart = true;
      }

      page.QueueCommitData( this.Dispatcher );
    }

    internal List<LinkedListNode<VirtualPage>> GetUnlockedPendingFillNodes()
    {
      List<LinkedListNode<VirtualPage>> unlockedPendingFillNodes = new List<LinkedListNode<VirtualPage>>();

      LinkedListNode<VirtualPage> lastUnlockedPendingFillNode = m_pageNodes.Last;

      while( lastUnlockedPendingFillNode != null )
      {
        VirtualPage page = lastUnlockedPendingFillNode.Value;

        Debug.Assert( page != null );

        if( ( !page.IsLocked ) && ( !page.IsFilled ) )
        {
          unlockedPendingFillNodes.Add( lastUnlockedPendingFillNode );
        }

        lastUnlockedPendingFillNode = lastUnlockedPendingFillNode.Previous;
      }

      return unlockedPendingFillNodes;
    }

    internal void CleanUpAndDisposeUnused()
    {
      // Remove the less used unlocked pages.  This will also ask to save it.

      // Also remove all pending fill pages which are not locked wether or not we are under the max unlocked page count.
      // This is so the abort query event is raised so that the user can abort his fetching of data.

      // Start with the unlocked pending fill pages since it is mandatory to remove them all in order to abort the async data fetching.  
      // The first node in the list returned by the GetUnlockedPendingFillNodes method is the oldest one, so we can start removing from the beginning of the returned list.
      List<LinkedListNode<VirtualPage>> unlockedPendingFillNodes = this.GetUnlockedPendingFillNodes();

      int unlockedPendingFillCount = unlockedPendingFillNodes.Count;

      for( int i = 0; i < unlockedPendingFillCount; i++ )
      {
        this.QueueCommitDataOrAbortIfRequired( unlockedPendingFillNodes[ i ], true );
      }

      // Then, move on to removing the other unlocked pages not up for commit, if we are above the max item in memory limit.  There should not be any pending fill pages left which are not locked.
      int removablePageItemCount = this.GetRemovablePageItemCount();

      while( removablePageItemCount > m_maxRealizedItemCount )
      {
        LinkedListNode<VirtualPage> lastRemovable = this.LastRemovable;

        Debug.Assert( lastRemovable != null );

        removablePageItemCount -= lastRemovable.Value.Count;

        this.QueueCommitDataOrAbortIfRequired( lastRemovable, true );
      }
    }

    internal void RemovePage( VirtualPage page )
    {
      if( page.IsDisposed )
        return;

      Debug.Assert( page != null );
      Debug.Assert( !page.IsDirty );

      // A filled page is being removed.  Change the version.
      this.IncrementVersion();

      // Update the table of content of the page's ParentVirtualList
      page.ParentVirtualList.TableOfContent.RemovePage( page );

      m_pageNodes.Remove( page );

      Debug.WriteLineIf( VirtualPageManager.DebugDataVirtualization, "Removed Page: " + page.ToString() );

      // Dispose the page since it will never be reused
      page.Dispose();
    }

    internal void MovePageToFront( VirtualPage page )
    {
      // The further from the front a page is, the longer it has been since it was requested.
      Debug.Assert( page != null );

      LinkedListNode<VirtualPage> firstNode = m_pageNodes.First;

      if( firstNode.Value != page )
      {
        LinkedListNode<VirtualPage> node = m_pageNodes.Find( page );
        m_pageNodes.Remove( node );
        m_pageNodes.AddFirst( node );

        Debug.WriteLineIf( VirtualPageManager.DebugDataVirtualization, "Moved To Front: Page " + page.ToString() );
      }
    }

    internal void AddPage( VirtualPage page, PageInsertPosition insertPosition )
    {
      if( page == null )
        throw new ArgumentNullException( "page", "TODOOC: An internal error occured while paging data. Page cannot be null." );

      // We call clean-up before the call to AddFirst since if we do it afterward and the page is pending fill, we will remove it.
      this.CleanUpAndDisposeUnused();

      if( insertPosition == PageInsertPosition.Front )
      {
        m_pageNodes.AddFirst( page );
      }
      else
      {
        m_pageNodes.AddLast( page );
      }

      Debug.WriteLineIf( VirtualPageManager.DebugDataVirtualization, "Added To " + ( ( insertPosition == PageInsertPosition.Front ) ? "Front" : "Back" ) + ": Page " + page.ToString() );
    }

    internal void Restart()
    {
      if( this.RestartingManager )
        return;

      this.RestartingManager = true;

      Debug.WriteLineIf( VirtualPageManager.DebugDataVirtualization, "VirtualPageManager (" + this.GetHashCode() + ") - Restarting all virtual Lists. " );

      this.OnVirtualPageManagerRestarting();

      if( m_managedLists.Count == 0 )
      {
        // No pages, restart is completed
        this.EndRestart();
      }
      else
      {
        m_restartingListsCount = m_managedLists.Count;
        int managedListCount = m_managedLists.Count;

        // Restart all VirtualLists 
        for( int i = managedListCount - 1; i >= 0; i-- )
        {
          VirtualList virtualList = m_managedLists[ i ];
          virtualList.Restart();
        }
      }
    }

    internal void Disconnect()
    {
      this.IsConnected = false;
    }

    internal void ManageList( VirtualList virtualList )
    {
      Debug.Assert( !m_managedLists.Contains( virtualList ) );
      Debug.Assert( virtualList.VirtualPagingManager == null );

      virtualList.VirtualPagingManager = this;

      this.m_managedLists.Add( virtualList );
    }

    internal bool IsAsyncCommitQueuedForItem( object item )
    {
      LinkedListNode<VirtualPage> pageNode = m_pageNodes.First;

      while( pageNode != null )
      {
        VirtualList virtualList = pageNode.Value.ParentVirtualList;

        Debug.Assert( m_managedLists.Contains( virtualList ) );

        if( virtualList.IsAsyncCommitQueuedForItem( item ) )
        {
          Debug.Assert( !( item is EmptyDataItem ), "A commit operation should not have been queued for an EmptyDataItem." );

          return true;
        }

        pageNode = pageNode.Next;
      }

      return false;
    }

    internal bool IsItemDirty( object item )
    {
      LinkedListNode<VirtualPage> pageNode = m_pageNodes.First;

      while( pageNode != null )
      {
        VirtualList virtualList = pageNode.Value.ParentVirtualList;

        Debug.Assert( m_managedLists.Contains( virtualList ) );

        if( virtualList.IsItemDirty( item ) )
        {
          Debug.Assert( !( item is EmptyDataItem ), "An EmptyDataItem should not have been flagged as dirty." );

          return true;
        }

        pageNode = pageNode.Next;
      }

      return false;
    }

    internal void SetCachedValuesForItem( object item, string[] names, object[] values )
    {
      LinkedListNode<VirtualPage> pageNode = m_pageNodes.First;

      while( pageNode != null )
      {
        VirtualList virtualList = pageNode.Value.ParentVirtualList;

        Debug.Assert( m_managedLists.Contains( virtualList ) );

        int localIndex = virtualList.IndexOf( item );

        if( localIndex != -1 )
        {
          virtualList.SetCachedValuesForItemAtIndex( localIndex, names, values );
          return;
        }

        pageNode = pageNode.Next;
      }

      throw new InvalidOperationException( "An attempt was made to begin the edit process on an unknown item." );
    }

    internal VirtualizedItemValueCollection GetCachedValuesForItem( object item )
    {
      LinkedListNode<VirtualPage> pageNode = m_pageNodes.First;

      while( pageNode != null )
      {
        VirtualList virtualList = pageNode.Value.ParentVirtualList;

        Debug.Assert( m_managedLists.Contains( virtualList ) );

        int localIndex = virtualList.IndexOf( item );

        if( localIndex != -1 )
          return virtualList.GetCachedValuesForItemAtIndex( localIndex );

        pageNode = pageNode.Next;
      }

      return null;
    }

    internal void ClearCachedValuesForItem( object item )
    {
      LinkedListNode<VirtualPage> pageNode = m_pageNodes.First;

      while( pageNode != null )
      {
        VirtualList virtualList = pageNode.Value.ParentVirtualList;

        Debug.Assert( m_managedLists.Contains( virtualList ) );

        int localIndex = virtualList.IndexOf( item );

        if( localIndex != -1 )
        {
          virtualList.ClearCachedValuesForItemAtIndex( localIndex );
          return;
        }

        pageNode = pageNode.Next;
      }

      throw new InvalidOperationException( "An attempt was made to leave the edit process on an unknown item." );
    }

    internal void CommitAll()
    {
      List<VirtualList> currentVirtualLists = new List<VirtualList>( m_pageNodes.Count );

      LinkedListNode<VirtualPage> pageNode = m_pageNodes.First;

      // Scan all in memory pages to build a list of unique VirtualLists which currently have items loaded in memory.
      while( pageNode != null )
      {
        VirtualList virtualList = pageNode.Value.ParentVirtualList;

        Debug.Assert( m_managedLists.Contains( virtualList ) );

        if( !currentVirtualLists.Contains( virtualList ) )
        {
          currentVirtualLists.Add( virtualList );
        }

        pageNode = pageNode.Next;
      }

      int currentVirtualListCount = currentVirtualLists.Count;

      for( int i = 0; i < currentVirtualListCount; i++ )
      {
        currentVirtualLists[ i ].CommitAll();
      }
    }

    private int GetRemovablePageCount()
    {
      int removablePageCount = 0;

      LinkedListNode<VirtualPage> pageNode = m_pageNodes.Last;

      while( pageNode != null )
      {
        VirtualPage page = pageNode.Value;

        Debug.Assert( page != null );

        if( ( page != null ) && ( page.IsRemovable ) )
        {
          removablePageCount++;
        }

        pageNode = pageNode.Previous;
      }

      return removablePageCount;
    }

    private int GetRemovablePageItemCount()
    {
      int removablePageItemCount = 0;

      LinkedListNode<VirtualPage> pageNode = m_pageNodes.Last;

      while( pageNode != null )
      {
        VirtualPage page = pageNode.Value;

        Debug.Assert( page != null );

        if( ( page != null ) && ( page.IsRemovable ) )
        {
          removablePageItemCount += page.Count;
        }

        pageNode = pageNode.Previous;
      }

      return removablePageItemCount;
    }

    private void QueueCommitDataOrAbortIfRequired( LinkedListNode<VirtualPage> pageNode, bool removeAfterOperation )
    {
      VirtualPage page = pageNode.Value;

      // Update the flag in case this page must be removed after an abort or commit operation
      page.RemoveAfterOperation = removeAfterOperation;

      // The only circumstance when we should remove a page which is not removable is if we are restarting.
      Debug.Assert( ( page != null ) && ( !page.IsDisposed ) && ( ( page.IsRemovable ) || ( page.ParentVirtualList.IsRestarting ) ) );

      if( page.IsDirty )
      {
        // Don't remove pages which contains modifications.  We'll remove them from the book when they are committed, if they aren't locked.
        this.QueueCommitData( page );
      }
      else if( !page.IsFilled )
      {
        // The page is not filled, we must send abort the QueryData for this page in case it was sent
        page.AbortQueryDataOperation();
      }

      // The page must be removed after operation and it has nothing to commit and is not  currently aborting an operation. It is safe to remove it
      if( removeAfterOperation && !page.IsCommitPending && !page.IsAborting )
      {
        this.RemovePage( page );
      }
    }

    private void IncrementVersion()
    {
      unchecked
      {
        m_version++;
      }
    }

    #region IDisposable Members

    public void Dispose()
    {

      this.DisposeCore();

    }

    protected virtual void DisposeCore()
    {
      if( m_managedLists != null )
      {
        m_managedLists.Clear();
      }

      if( m_pageNodes != null )
      {
        m_pageNodes.Clear();
      }

      this.IsDisposed = true;
    }

    #endregion

    private int m_version;
    private int m_restartingListsCount;

    private BitVector32 m_flags = new BitVector32();

    private LinkedList<VirtualPage> m_pageNodes;

    private List<VirtualList> m_managedLists;
    private ReadOnlyCollection<VirtualList> m_readOnlyManagedLists;

    private int m_pageSize;
    private int m_maxRealizedItemCount;
    private double m_preemptivePageQueryRatio;
    private int m_maxRemovablePageCount;

    internal enum PageInsertPosition
    {
      Front = 0,
      Back = 1
    }

    private enum VirtualPageManagerFlags
    {
      RestartingManager = 1,
      ShouldRefreshAfterRestart = 2,
      IsDisposed = 4,
      IsConnected = 8,
    }
  }
}
