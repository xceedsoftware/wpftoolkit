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
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Collections;
using System.Windows.Threading;
using System.Collections.Specialized;

namespace Xceed.Wpf.DataGrid
{
  internal abstract class DataGridPageManagerBase : VirtualPageManager, IEnumerable<object>, IEnumerable
  {
    #region STATIC MEMBERS

    private static AsyncQueryInfoWeakComparer QueryInfoWeakComparer = new AsyncQueryInfoWeakComparer();

    #endregion STATIC MEMBERS

    public DataGridPageManagerBase( DataGridVirtualizingCollectionViewBase collectionView )
      : base( collectionView.Dispatcher, collectionView.PageSize, collectionView.MaxRealizedItemCount, collectionView.PreemptivePageQueryRatio )
    {
      m_collectionView = collectionView;
      m_virtualListVSCollectionViewGroupDictionary = new Dictionary<VirtualList, DataGridVirtualizingCollectionViewGroupBase>();
    }

    #region CollectionView Property

    public DataGridVirtualizingCollectionViewBase CollectionView
    {
      get
      {
        return m_collectionView;
      }
    }

    #endregion CollectionView Property

    #region PUBLIC METHODS

    public DataGridVirtualizingCollectionViewGroupBase GetLinkedCollectionViewGroup( VirtualList virtualItemList )
    {
      DataGridVirtualizingCollectionViewGroupBase collectionViewGroup;

      m_virtualListVSCollectionViewGroupDictionary.TryGetValue( virtualItemList, out collectionViewGroup );

      return collectionViewGroup;
    }

    public int GetGlobalIndexOf( object item )
    {
      ReadOnlyCollection<VirtualList> virtualItemLists = this.ManagedLists;
      int virtualItemListsCount = virtualItemLists.Count;

      for( int i = 0; i < virtualItemListsCount; i++ )
      {
        VirtualList localList = virtualItemLists[ i ];
        int localIndex = localList.IndexOf( item );

        if( localIndex >= 0 )
        {
          Debug.Assert( m_virtualListVSCollectionViewGroupDictionary.ContainsKey( localList ) );

          DataGridVirtualizingCollectionViewGroupBase dgvcvg = m_virtualListVSCollectionViewGroupDictionary[ localList ];

          return localIndex + dgvcvg.StartGlobalIndex;
        }
      }

      return -1;
    }

    #endregion PUBLIC METHODS

    #region PROTECTED METHODS

    protected internal override void OnBuiltInAbort( VirtualPage page, AsyncQueryInfo queryInfo )
    {
      // When a built-in abort occurs, we ensure to remove
      // any AsyncQueryInfo from references since the ConnectionState
      // use this array to update its actual state
      m_asyncQueryInfosInProgress.Remove( queryInfo );

      // In case the page query was aborted when
      // VirtualPageManager.CleanUpUnused is called
      if( page.RemoveAfterOperation )
        this.RemovePage( page );

      this.UpdateConnectionState();
    }

    protected internal override void OnQueryItems( VirtualPage page, AsyncQueryInfo queryInfo )
    {
      // The VirtualPageManager is not connected to the CollectionView anymore,
      // do NOT query items since it will be done by the new VirtualPageManager
      // assigned to the same CollectionView
      if( !this.IsConnected )
        return;

      Debug.Assert( !m_asyncQueryInfosInProgress.Contains( queryInfo ) );
      m_asyncQueryInfosInProgress.Add( queryInfo );

      if( m_asyncQueryInfosInError != null )
      {
        LinkedListNode<AsyncQueryInfo> queryInfoInErrorNode = m_asyncQueryInfosInError.First;

        while( queryInfoInErrorNode != null )
        {
          if( DataGridPageManagerBase.QueryInfoWeakComparer.Equals( queryInfo, queryInfoInErrorNode.Value ) )
          {
            m_asyncQueryInfosInError.Remove( queryInfoInErrorNode );
            break;
          }

          queryInfoInErrorNode = queryInfoInErrorNode.Next;
        }

        if( m_asyncQueryInfosInError.Count == 0 )
          m_asyncQueryInfosInError = null;
      }

      this.UpdateConnectionState();
    }

    protected internal override void OnQueryItemsCompleted( VirtualPage page, AsyncQueryInfo queryInfo, object[] fetchedItems )
    {
      base.OnQueryItemsCompleted( page, queryInfo, fetchedItems );

      Debug.Assert( m_asyncQueryInfosInProgress.Contains( queryInfo ) );
      m_asyncQueryInfosInProgress.Remove( queryInfo );

      this.UpdateConnectionState();
    }

    protected internal override void OnAbortQueryItems( VirtualPage page, AsyncQueryInfo queryInfo )
    {
      base.OnAbortQueryItems( page, queryInfo );

      // It is possible that the queryInfo was removed previously
      m_asyncQueryInfosInProgress.Remove( queryInfo );

      // In case the page query was aborted when
      // VirtualPageManager.CleanUpUnused is called
      if( page.RemoveAfterOperation )
        this.RemovePage( page );

      this.UpdateConnectionState();
    }

    protected internal override void OnQueryErrorChanged( VirtualPage page, AsyncQueryInfo queryInfo )
    {
      base.OnQueryErrorChanged( page, queryInfo );

      // It is possible that m_asyncQueryInfosInProgress does not contain the queryInfo when
      // the query was aborted but the user did not stop the query and later on set the queryInfo error
      // event if the queryInfo ShouldAbort is set to True.
      Debug.Assert( ( m_asyncQueryInfosInProgress.Contains( queryInfo ) ) || ( queryInfo.ShouldAbort ) );

      object error = queryInfo.Error;

      if( error == null )
      {
        // Even if the queryInfo's ShouldAbort property is set to True, clean-up the error.
        Debug.Assert( ( m_asyncQueryInfosInError != null ) && ( m_asyncQueryInfosInError.Contains( queryInfo ) ) );

        m_asyncQueryInfosInError.Remove( queryInfo );

        if( m_asyncQueryInfosInError.Count == 0 )
          m_asyncQueryInfosInError = null;
      }
      else if( !queryInfo.ShouldAbort )
      {
        // Only add errors if the queryInfo's ShouldAbort property is set to False.
        if( m_asyncQueryInfosInError == null )
          m_asyncQueryInfosInError = new LinkedList<AsyncQueryInfo>();

        if( m_asyncQueryInfosInError.Contains( queryInfo ) )
          m_asyncQueryInfosInError.Remove( queryInfo );

        m_asyncQueryInfosInError.AddFirst( queryInfo );
      }

      this.UpdateConnectionState();
    }

    protected override void OnVirtualPageManagerRestarting()
    {
      base.OnVirtualPageManagerRestarting();

      // Keep a reference on the old CollectionViewGroupRoot of the 
      // parent CollectionView to be able to clear all references
      // of this group after the VirtualPageManager is disposed
      m_managerCollectionViewGroupRoot = m_collectionView.RootGroup;
    }

    protected override void OnVirtualPageManagerRestarted( bool shouldRefresh )
    {
      base.OnVirtualPageManagerRestarted( shouldRefresh );

      // No need for a refresh since a new VirtualPageManager will
      // be created and force QueryItemsCount and QueryItems using 
      // the same CollectionView

      this.TryDispose();
    }

    protected internal override void OnCommitItems( VirtualPage page, AsyncCommitInfo commitInfo )
    {
      base.OnCommitItems( page, commitInfo );

      Debug.Assert( !m_asyncCommitInfosInProgress.Contains( commitInfo ) );
      m_asyncCommitInfosInProgress.Add( commitInfo );

      this.UpdateConnectionState();

      Debug.WriteLineIf( VirtualPageManager.DebugDataVirtualization, "OnCommitItems for page " + page.ToString() );

      m_collectionView.Dispatcher.BeginInvoke(
        new Action<VirtualPage, AsyncCommitInfo>( this.RaiseCollectionViewOnCommitItems ),
        DispatcherPriority.Background,
        page,
        commitInfo );
    }

    protected internal override void OnCommitItemsCompleted( VirtualPage page, AsyncCommitInfo commitInfo )
    {
      Debug.Assert( m_asyncCommitInfosInProgress.Contains( commitInfo ) );
      m_asyncCommitInfosInProgress.Remove( commitInfo );
      this.UpdateConnectionState();

      base.OnCommitItemsCompleted( page, commitInfo );

      // In case the page query was aborted when
      // VirtualPageManager.CleanUpUnused is called
      if( page.RemoveAfterOperation )
        this.RemovePage( page );

      Debug.WriteLineIf( VirtualPageManager.DebugDataVirtualization, "OnCommitItemsCompleted for page " + page.ToString() );
    }

    protected internal override void OnCommitErrorChanged( VirtualPage page, AsyncCommitInfo commitInfo )
    {
      base.OnCommitErrorChanged( page, commitInfo );

      Debug.Assert( m_asyncCommitInfosInProgress.Contains( commitInfo ) );

      object error = commitInfo.Error;

      if( error == null )
      {
        Debug.Assert( ( m_asyncCommitInfosInError != null ) && ( m_asyncCommitInfosInError.Contains( commitInfo ) ) );

        m_asyncCommitInfosInError.Remove( commitInfo );

        if( m_asyncCommitInfosInError.Count == 0 )
          m_asyncCommitInfosInError = null;
      }
      else
      {
        if( m_asyncCommitInfosInError == null )
          m_asyncCommitInfosInError = new LinkedList<AsyncCommitInfo>();

        if( m_asyncCommitInfosInError.Contains( commitInfo ) )
          m_asyncCommitInfosInError.Remove( commitInfo );

        m_asyncCommitInfosInError.AddFirst( commitInfo );
      }

      this.UpdateConnectionState();
    }

    protected void UnlinkVirtualListAndCollectionViewGroup( VirtualList virtualList )
    {
      DataGridVirtualizingCollectionViewGroupBase collectionViewGroup = null;

      if( m_virtualListVSCollectionViewGroupDictionary.TryGetValue( virtualList, out collectionViewGroup ) )
      {
        CollectionChangedEventManager.RemoveListener( virtualList, collectionViewGroup );
        m_virtualListVSCollectionViewGroupDictionary.Remove( virtualList );
      }
    }

    protected override void DisposeCore()
    {
      // Remove every CollectionChanged WeakEventListener from the 
      // inner DataGridVirtualizingCollectionViewGroupBase
      // and clear the references to them
      Debug.Assert( m_virtualListVSCollectionViewGroupDictionary.Count == 0 );

      // Dispose the RootGroup for which this manager
      // was created
      if( m_managerCollectionViewGroupRoot != null )
      {
        m_managerCollectionViewGroupRoot.Dispose();
        m_managerCollectionViewGroupRoot = null;
      }

      // Force a PropertyChanged on the CollectionView
      // for properties ConnectionState and Error to force
      // bound countrol to update correctly using the values
      // affected on the CollectionView by the new 
      // VirtualPageManager
      if( m_collectionView != null )
      {
        m_collectionView.RefreshConnectionStateAndError();
        m_collectionView = null;
      }

      base.DisposeCore();
    }

    protected internal override int OnQueryItemCount( VirtualList virtualList )
    {
      try
      {
        return base.OnQueryItemCount( virtualList );
      }
      finally
      {
        this.UpdateConnectionState();
      }
    }

    #endregion PROTECTED METHODS

    #region PRIVATE METHODS

    private void TryDispose()
    {
      if( ( m_asyncCommitInfosInProgress.Count == 0 )
          && ( m_asyncCommitInfosInError == null )
          && ( m_asyncQueryInfosInError == null )
          && ( m_asyncQueryInfosInProgress.Count == 0 ) )
      {
        // At this point, all data was correctly committed.

        // This manager is no more required, we can dispose
        // it. This will force a dispose of all VirtualList,
        // their inner VirtualListTableOfContent and Pages.
        this.Dispose();
      }
    }

    private void UpdateConnectionState()
    {
      // Never update the ConnectionState when disconnected
      // from the CollectionView or Disposed since another
      // VirtualPageManager will take care or updating it
      if( !this.IsConnected || this.IsDisposed )
        return;

      DataGridConnectionState state = DataGridConnectionState.Idle;
      object error = null;

      if( m_asyncCommitInfosInError != null )
      {
        state = DataGridConnectionState.Error;
        Debug.Assert( m_asyncCommitInfosInError.Count > 0 );
        error = m_asyncCommitInfosInError.First.Value.Error;
      }
      else if( m_asyncQueryInfosInError != null )
      {
        state = DataGridConnectionState.Error;
        Debug.Assert( m_asyncQueryInfosInError.Count > 0 );
        error = m_asyncQueryInfosInError.First.Value.Error;
      }
      else if( m_asyncCommitInfosInProgress.Count > 0 )
      {
        state = DataGridConnectionState.Committing;
      }
      else if( m_asyncQueryInfosInProgress.Count > 0 )
      {
        state = DataGridConnectionState.Loading;
      }

      m_collectionView.UpdateConnectionState( state, error );
    }

    private void RaiseCollectionViewOnCommitItems( VirtualPage dispatchedPage, AsyncCommitInfo dispatchedCommitInfo )
    {
      DataGridVirtualizingCollectionViewBase collectionView = this.CollectionView as DataGridVirtualizingCollectionViewBase;

      DataGridVirtualizingCollectionViewGroupBase collectionViewGroup =
        this.GetLinkedCollectionViewGroup( dispatchedPage.ParentVirtualList ) as DataGridVirtualizingCollectionViewGroupBase;

      Debug.Assert( ( collectionViewGroup != null ) && ( collectionView != null ) );

      collectionView.OnCommitItems( dispatchedCommitInfo );
    }

    #endregion

    #region INTERNAL METHODS

    internal void LinkVirtualListAndCollectionViewGroup( VirtualList virtualItemList, DataGridVirtualizingCollectionViewGroupBase collectionViewGroup )
    {
      Debug.Assert( !m_virtualListVSCollectionViewGroupDictionary.ContainsKey( virtualItemList ) );
      Debug.Assert( !m_virtualListVSCollectionViewGroupDictionary.ContainsValue( collectionViewGroup ) );

      m_virtualListVSCollectionViewGroupDictionary.Add( virtualItemList, collectionViewGroup );

      CollectionChangedEventManager.AddListener( virtualItemList, collectionViewGroup );
    }

    internal override void OnVirtualListRestarted( VirtualList virtualList )
    {
      this.UnlinkVirtualListAndCollectionViewGroup( virtualList );
      base.OnVirtualListRestarted( virtualList );
    }

    #endregion INTERNAL METHODS

    #region PRIVATE FIELDS

    private DataGridVirtualizingCollectionViewGroupBase m_managerCollectionViewGroupRoot;
    private DataGridVirtualizingCollectionViewBase m_collectionView;
    private Dictionary<VirtualList, DataGridVirtualizingCollectionViewGroupBase> m_virtualListVSCollectionViewGroupDictionary;

    private HashSet<AsyncQueryInfo> m_asyncQueryInfosInProgress = new HashSet<AsyncQueryInfo>();
    private HashSet<AsyncCommitInfo> m_asyncCommitInfosInProgress = new HashSet<AsyncCommitInfo>();

    private LinkedList<AsyncQueryInfo> m_asyncQueryInfosInError;
    private LinkedList<AsyncCommitInfo> m_asyncCommitInfosInError;

    #endregion PRIVATE FIELDS


    #region IEnumerable<object> Members

    public IEnumerator<object> GetEnumerator()
    {
      return new DataGridPageManagerEnumerator( this );
    }

    #endregion

    #region IEnumerable Members

    IEnumerator IEnumerable.GetEnumerator()
    {
      return null;
    }

    #endregion


    #region PRIVATE NESTED CLASSES

    private class DataGridPageManagerEnumerator : IEnumerator<object>, IEnumerator
    {
      public DataGridPageManagerEnumerator( DataGridPageManagerBase dataGridPageManagerBase )
      {
        if( dataGridPageManagerBase == null )
          throw new ArgumentNullException( "dataGridPageManagerBase" );

        m_dataGridPageManagerBase = dataGridPageManagerBase;

        VirtualList[] orderedVirtualLists =
          m_dataGridPageManagerBase.m_virtualListVSCollectionViewGroupDictionary.Keys.OrderBy(
            virtualList => m_dataGridPageManagerBase.m_virtualListVSCollectionViewGroupDictionary[ virtualList ].StartGlobalIndex ).ToArray();

        m_orderedVirtualListEnumerators = new VirtualListEnumerator[ orderedVirtualLists.Length ];

        for( int i = 0; i < orderedVirtualLists.Length; i++ )
        {
          m_orderedVirtualListEnumerators[ i ] = ( VirtualListEnumerator )( ( IEnumerable )orderedVirtualLists[ i ] ).GetEnumerator();
        }
      }

      #region IEnumerator<object> Members

      public object Current
      {
        get
        {
          return m_currentItem;
        }
      }

      #endregion IEnumerator<object> Members

      #region IDisposable Members

      public void Dispose()
      {
        foreach( VirtualListEnumerator enumerator in m_orderedVirtualListEnumerators )
        {
          enumerator.Dispose();
        }

        m_orderedVirtualListEnumerators = null;
        m_dataGridPageManagerBase = null;
        m_currentItem = null;
      }

      #endregion

      #region IEnumerator Members

      public bool MoveNext()
      {
        // No need to check the VirtualPageManager's version.  The sub VirtualLists' Enumerator will take care of it.
        while( m_currentEnumeratorIndex < m_orderedVirtualListEnumerators.Length )
        {
          VirtualListEnumerator enumerator = m_orderedVirtualListEnumerators[ m_currentEnumeratorIndex ];

          enumerator.MoveNext();

          if( !enumerator.AfterEnd )
          {
            m_currentItem = enumerator.Current;
            return true;
          }
          else
          {
            // Reached the end of this enumerator.  Let's increment the currentEnumeratorIndex.
            m_currentEnumeratorIndex++;
          }
        }

        m_currentItem = null;
        return false;
      }

      public void Reset()
      {
        m_currentEnumeratorIndex = 0;
        m_currentItem = null;
      }

      object IEnumerator.Current
      {
        get
        {
          return this.Current;
        }
      }

      bool IEnumerator.MoveNext()
      {
        return this.MoveNext();
      }

      void IEnumerator.Reset()
      {
        this.Reset();
      }

      #endregion IEnumerator Members


      #region PRIVATE FIELDS

      private DataGridPageManagerBase m_dataGridPageManagerBase;
      private VirtualListEnumerator[] m_orderedVirtualListEnumerators;
      private object m_currentItem;
      private int m_currentEnumeratorIndex;

      #endregion PRIVATE FIELDS
    }

    #endregion PRIVATE NESTED CLASSES
  }
}
