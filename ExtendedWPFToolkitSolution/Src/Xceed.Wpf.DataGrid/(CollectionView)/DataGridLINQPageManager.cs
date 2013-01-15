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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Xceed.Wpf.DataGrid
{
  internal class DataGridLINQPageManager : DataGridPageManagerBase
  {
    #region CONSTRUCTORS

    public DataGridLINQPageManager( DataGridVirtualizingQueryableCollectionView collectionView, object syncRoot, bool supportsPrimaryKeyOptimizations )
      : base( collectionView )
    {
      m_supportsPrimaryKeyOptimizations = supportsPrimaryKeyOptimizations;
      m_syncRoot = syncRoot;
    }

    #endregion CONSTRUCTORS

    #region DATA VIRTUALIZATION

    protected override int OnQueryItemCountCore( VirtualList virtualList )
    {
      if( !this.IsConnected )
        return 0;

      DataGridVirtualizingQueryableCollectionViewGroup collectionViewGroup =
        this.GetLinkedCollectionViewGroup( virtualList ) as DataGridVirtualizingQueryableCollectionViewGroup;

      Debug.Assert( collectionViewGroup != null );

      return collectionViewGroup.QueryItemCount();
    }

    protected internal override void OnQueryItems( VirtualPage page, AsyncQueryInfo queryInfo )
    {
      base.OnQueryItems( page, queryInfo );

      DataGridVirtualizingQueryableCollectionViewGroup collectionViewGroup =
        this.GetLinkedCollectionViewGroup( page.ParentVirtualList ) as DataGridVirtualizingQueryableCollectionViewGroup;

      IQueryable queryableToUse;

      int virtualItemCount = collectionViewGroup.VirtualItemCount;

      bool queryableIsReversed;

      if( ( !m_supportsPrimaryKeyOptimizations ) || ( queryInfo.StartIndex < ( virtualItemCount / 2 ) ) )
      {
        queryableIsReversed = false;
        queryableToUse = collectionViewGroup.Queryable.Slice( queryInfo.StartIndex, queryInfo.RequestedItemCount );
      }
      else
      {
        queryableIsReversed = true;

        int reversedStartIndex = virtualItemCount - ( queryInfo.StartIndex + queryInfo.RequestedItemCount );

        queryableToUse = collectionViewGroup.ReversedQueryable.Slice( reversedStartIndex, queryInfo.RequestedItemCount );
      }

      System.Threading.ThreadPool.QueueUserWorkItem( new System.Threading.WaitCallback( this.AsyncGatherItems ), new object[] { queryInfo, queryableToUse, queryableIsReversed } );
    }


    private void AsyncGatherItems( object workItem )
    {
      object[] parameters = ( object[] )workItem;

      AsyncQueryInfo queryInfo = parameters[ 0 ] as AsyncQueryInfo;

      if( queryInfo.ShouldAbort )
        return;

      IQueryable queryable = ( IQueryable )parameters[ 1 ];

      object[] items = new object[ queryInfo.RequestedItemCount ];

      System.Collections.IEnumerator enumerator;

      lock( m_syncRoot )
      {
        // We reverify here since a reset could have been issued while we were waiting on the lock statement.
        if( ( queryInfo.ShouldAbort ) || ( !this.IsConnected ) || ( this.IsDisposed ) )
          return;

        try
        {
          Debug.WriteLineIf( VirtualPageManager.DebugDataVirtualization,"Beginning Provider Execute for page at start index: " + queryInfo.StartIndex.ToString() );

          enumerator = queryable.GetEnumerator();

          Debug.WriteLineIf( VirtualPageManager.DebugDataVirtualization,"Ended Provider Execute for page at start index: " + queryInfo.StartIndex.ToString() );

          int i = 0;

          while( enumerator.MoveNext() )
          {
            object current = enumerator.Current;

            if( current != null )
              items[ i ] = enumerator.Current;

            i++;
          }
        }
        catch( Exception exception )
        {
          // TimeOut exeception or other.
          queryInfo.AbortQuery();
          queryInfo.Error = exception.Message;
          return;
        }
      }

      bool queryableWasReversed = ( bool )parameters[ 2 ];

      if( queryableWasReversed )
        Array.Reverse( items );

      queryInfo.EndQuery( items );
    }

    protected internal override void OnQueryItemsCompleted( VirtualPage page, AsyncQueryInfo queryInfo, object[] fetchedItems )
    {
      DataGridVirtualizingQueryableCollectionView collectionView = this.CollectionView as DataGridVirtualizingQueryableCollectionView;

      // The VirtualPageManager was Disposed
      if( collectionView == null )
        return;

      using( collectionView.DeferRefresh() )
      {
        base.OnQueryItemsCompleted( page, queryInfo, fetchedItems );
      }
    }

    protected override void DisposeCore()
    {
      m_syncRoot = null;
      base.DisposeCore();
    }

    #endregion DATA VIRTUALIZATION

    #region PRIVATE FIELDS

    private object m_syncRoot;
    private bool m_supportsPrimaryKeyOptimizations;

    #endregion PRIVATE FIELDS
  }
}
