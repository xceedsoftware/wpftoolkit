/*************************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at http://xceed.com/wpf_toolkit

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Xceed.Utils.Collections;

namespace Xceed.Wpf.DataGrid
{
  public sealed class DataGridVirtualizingQueryableCollectionView : DataGridVirtualizingCollectionViewBase
  {
    public DataGridVirtualizingQueryableCollectionView()
      : this( null, true, DataGridVirtualizingCollectionViewBase.DefaultPageSize, DataGridVirtualizingCollectionViewBase.DefaultMaxRealizedItemCount )
    {
    }

    public DataGridVirtualizingQueryableCollectionView( IQueryable queryableSource )
      : this( queryableSource, true, DataGridVirtualizingCollectionViewBase.DefaultPageSize, DataGridVirtualizingCollectionViewBase.DefaultMaxRealizedItemCount )
    {
    }

    public DataGridVirtualizingQueryableCollectionView( IQueryable queryableSource, bool autoCreateItemProperties, int pageSize, int maxRealizedItemCount )
      : base( queryableSource, null, autoCreateItemProperties, pageSize, maxRealizedItemCount )
    {
      m_pageManagerSyncRoot = new object();
    }

    #region QueryableSource PROPERTY

    public IQueryable QueryableSource
    {
      get
      {
        return this.ModelSource as IQueryable;
      }
    }

    #endregion QueryableSource PROPERTY

    #region INTERNAL METHODS

    internal override DataGridVirtualizingCollectionViewGroupBase CreateNewRootGroup()
    {
      bool rootIsBottomLevel = ( this.GroupDescriptions == null ) ? true : ( this.GroupDescriptions.Count == 0 );

      return new DataGridVirtualizingQueryableCollectionViewGroupRoot( this, m_pageManagerSyncRoot, rootIsBottomLevel );
    }

    internal override System.Collections.IEnumerator GetVirtualEnumerator()
    {
      return ( ( DataGridVirtualizingQueryableCollectionViewGroupRoot )this.RootGroup ).GetVirtualPageManager().GetEnumerator();
    }

    #endregion

    #region PRIVATE FIELDS

    private object m_pageManagerSyncRoot;

    #endregion
  }
}
