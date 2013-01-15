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
using System.Collections.Specialized;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Xceed.Wpf.DataGrid
{
  internal class DataGridVirtualizingCollectionViewGroupRoot : DataGridVirtualizingCollectionViewGroup
  {
    #region CONSTRUCTORS

    internal DataGridVirtualizingCollectionViewGroupRoot( DataGridVirtualizingCollectionView collectionView, bool isBottomLevel )
      : base( null, -1, 0, null, -1, isBottomLevel )
    {
      m_parentCollectionView = collectionView;
      m_virtualPageManager = new DataGridPageManager( collectionView );
    }

    #endregion CONSTRUCTORS

    #region INTERNAL METHODS

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

    #endregion

    #region PROTECTED METHODS

    protected override DataGridVirtualizingCollectionViewBase GetCollectionView()
    {
      return m_parentCollectionView;
    }

    #endregion PROTECTED METHODS

    #region PRIVATE FILEDS

    private DataGridVirtualizingCollectionView m_parentCollectionView;
    private DataGridPageManager m_virtualPageManager;

    #endregion PRIVATE FIELDS
  }

}
