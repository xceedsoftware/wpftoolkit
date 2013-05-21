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

namespace Xceed.Wpf.DataGrid
{
  internal class DataGridPageManager : DataGridPageManagerBase
  {
    #region CONSTRUCTORS

    public DataGridPageManager( DataGridVirtualizingCollectionView collectionView )
      : base( collectionView )
    {
    }

    #endregion CONSTRUCTORS

    #region DATA VIRTUALIZATION

    protected override int OnQueryItemCountCore( VirtualList virtualList )
    {
      if( !this.IsConnected )
        return 0;

      DataGridVirtualizingCollectionView collectionView = this.CollectionView as DataGridVirtualizingCollectionView;

      // The VirtualPageManager was Disposed
      if( collectionView == null )
        return 0;

      DataGridVirtualizingCollectionViewGroup collectionViewGroup = this.GetLinkedCollectionViewGroup( virtualList ) as DataGridVirtualizingCollectionViewGroup;
      Debug.Assert( ( collectionViewGroup != null ) && ( collectionView != null ) );

      return collectionView.OnQueryItemCount( collectionViewGroup );
    }

    protected internal override void OnQueryItems( VirtualPage page, AsyncQueryInfo queryInfo )
    {
      base.OnQueryItems( page, queryInfo );

      DataGridVirtualizingCollectionView collectionView = this.CollectionView as DataGridVirtualizingCollectionView;

      // The VirtualPageManager was Disposed
      if( collectionView == null )
        return;

      DataGridVirtualizingCollectionViewGroup collectionViewGroup = this.GetLinkedCollectionViewGroup( page.ParentVirtualList ) as DataGridVirtualizingCollectionViewGroup;

      Debug.Assert( ( collectionViewGroup != null ) && ( collectionView != null ) );

      collectionView.OnQueryItems( queryInfo, collectionViewGroup );
    }

    protected internal override void OnAbortQueryItems( VirtualPage page, AsyncQueryInfo queryInfo )
    {
      DataGridVirtualizingCollectionView collectionView = this.CollectionView as DataGridVirtualizingCollectionView;

      // The VirtualPageManager was Disposed
      if( collectionView == null )
        return;

      DataGridVirtualizingCollectionViewGroup collectionViewGroup = this.GetLinkedCollectionViewGroup( page.ParentVirtualList ) as DataGridVirtualizingCollectionViewGroup;

      collectionView.OnAbortQueryItems( queryInfo, collectionViewGroup );

      base.OnAbortQueryItems( page, queryInfo );
    }

    protected internal override void OnQueryItemsCompleted( VirtualPage page, AsyncQueryInfo queryInfo, object[] fetchedItems )
    {
      DataGridVirtualizingCollectionView collectionView = this.CollectionView as DataGridVirtualizingCollectionView;

      // The VirtualPageManager was Disposed
      if( collectionView == null )
        return;

      using( collectionView.DeferRefresh() )
      {
        base.OnQueryItemsCompleted( page, queryInfo, fetchedItems );
      }
    }

    #endregion DATA VIRTUALIZATION
  }
}
