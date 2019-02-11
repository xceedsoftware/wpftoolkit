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
using System.ComponentModel;
using System.Collections;
using System.Linq.Expressions;

namespace Xceed.Wpf.DataGrid
{
  internal class DataGridVirtualizingQueryableCollectionViewGroupRoot : DataGridVirtualizingQueryableCollectionViewGroup
  {
    #region CONSTRUCTORS

    internal DataGridVirtualizingQueryableCollectionViewGroupRoot( 
      DataGridVirtualizingQueryableCollectionView collectionView, 
      object pageManagerSyncRoot, 
      bool isBottomLevel )
      : base( null, -1, 0, null, -1, isBottomLevel )
    {
      m_parentCollectionView = collectionView;
      m_queryableSource = m_parentCollectionView.QueryableSource;

      if( m_queryableSource != null )
      {
        m_primaryKeyPropertyNames = m_queryableSource.FindPrimaryKeys();
        // Primary key optimizations are only possible when only one primary key is identified in the source.
        // When dealing with sources defining more than one primary key, we would need to know in which order
        bool supportsPrimaryKeyOptimizations = ( ( m_primaryKeyPropertyNames != null ) && ( m_primaryKeyPropertyNames.Length == 1 ) );

        m_virtualPageManager = new DataGridLINQPageManager( collectionView,
          pageManagerSyncRoot, 
          supportsPrimaryKeyOptimizations );
      }
    }


    #endregion CONSTRUCTORS

    #region PROTECTED METHODS

    protected override DataGridVirtualizingCollectionViewBase GetCollectionView()
    {
      return m_parentCollectionView;
    }

    #endregion PROTECTED METHODS

    #region INTERNAL PROPERTIES

    public string[] PrimaryKeys
    {
      get
      {
        return m_primaryKeyPropertyNames;
      }
    }

    #endregion INTERNAL PROPERTIES

    #region INTERNAL METHODS

    internal override IQueryable CreateUnsortedFilteredQueryable()
    {
      return m_queryableSource;
    }

    internal override int GetGlobalIndexOf( object item )
    {
      return m_virtualPageManager.GetGlobalIndexOf( item );
    }

    internal override DataGridPageManagerBase GetVirtualPageManager()
    {
      return m_virtualPageManager;
    }

    internal object[] GetItemPropertyDistinctValues( DataGridItemPropertyBase dataGridItemProperty )
    {
      if( m_queryableSource == null )
        return new object[ 0 ];

      IQueryable distinctQueryable = m_queryableSource.GetSubGroupsAndCountsQueryable( dataGridItemProperty.Name, false, ListSortDirection.Ascending );

      List<object> distinctValuesAndCounts = new List<object>();
      try
      {
        System.Collections.IEnumerator enumerator = distinctQueryable.GetEnumerator();

        while( enumerator.MoveNext() )
        {
          QueryableExtensions.IQueryableGroupNameCountPair current = enumerator.Current as QueryableExtensions.IQueryableGroupNameCountPair;

          if( current != null )
            distinctValuesAndCounts.Add( current.GroupName );
        }

        return distinctValuesAndCounts.ToArray();
      }
      catch
      {
        // TimeOut exception on the connection or other.
        return new object[ 0 ];
      }
    }

    internal override void DisposeCore()
    {
      m_parentCollectionView = null;
      m_queryableSource = null;
      m_virtualPageManager = null;
      base.DisposeCore();
    }

    #endregion INTERNAL METHODS

    #region PRIVATE FIELDS

    private DataGridVirtualizingQueryableCollectionView m_parentCollectionView;

    private IQueryable m_queryableSource;

    private string[] m_primaryKeyPropertyNames;

    private DataGridLINQPageManager m_virtualPageManager;

    #endregion PRIVATE FIELDS
  }
}
