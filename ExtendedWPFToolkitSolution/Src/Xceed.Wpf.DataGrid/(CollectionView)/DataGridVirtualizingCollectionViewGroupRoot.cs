/*************************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at http://xceed.com/wpf_toolkit

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

namespace Xceed.Wpf.DataGrid
{
  internal class DataGridVirtualizingCollectionViewGroupRoot : DataGridVirtualizingCollectionViewGroup
  {
    internal DataGridVirtualizingCollectionViewGroupRoot( DataGridVirtualizingCollectionView collectionView, bool isBottomLevel )
      : base( null, -1, 0, null, -1, isBottomLevel )
    {
      m_parentCollectionView = collectionView;
      m_virtualPageManager = new DataGridPageManager( collectionView );
    }

    protected override DataGridVirtualizingCollectionViewBase GetCollectionView()
    {
      return m_parentCollectionView;
    }

    internal override int GetGlobalIndexOf( object item )
    {
      return m_virtualPageManager.GetGlobalIndexOf( item );
    }

    internal override DataGridPageManagerBase GetVirtualPageManager()
    {
      return m_virtualPageManager;
    }

    internal override void DisposeCore()
    {
      m_parentCollectionView = null;
      m_virtualPageManager = null;
      base.DisposeCore();
    }

    private DataGridVirtualizingCollectionView m_parentCollectionView;
    private DataGridPageManager m_virtualPageManager;
  }

}
