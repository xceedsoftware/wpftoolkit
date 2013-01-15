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
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Collections;
using System.Windows.Data;
using System.Diagnostics;
using System.Windows.Documents;
using System.Collections.Specialized;
using System.Windows;

namespace Xceed.Wpf.DataGrid
{
  internal class DataGridVirtualizingCollectionViewGroup : DataGridVirtualizingCollectionViewGroupBase, IWeakEventListener
  {
    #region STATIC MEMBERS

    private static List<DataGridGroupInfo> EmptyDataGridInfoList = new List<DataGridGroupInfo>( 0 );

    #endregion STATIC MEMBERS


    #region CONSTRUCTORS

    internal DataGridVirtualizingCollectionViewGroup(
      object name,
      int initialItemsCount,
      int startGlobalIndex,
      DataGridVirtualizingCollectionViewGroup parent,
      int level,
      bool isBottomLevel )
      : base( name, initialItemsCount, startGlobalIndex, parent, level, isBottomLevel )
    {
    }

    #endregion CONSTRUCTORS

    #region DATA VIRTUALIZATION

    internal override int QueryItemCount()
    {
      DataGridVirtualizingCollectionView collectionView = this.GetCollectionView() as DataGridVirtualizingCollectionView;
      return collectionView.OnQueryItemCount( this );
    }

    internal override ObservableCollection<object> QuerySubCollectionViewGroupList( GroupDescription subGroupBy, int nextLevel, bool nextLevelIsBottom )
    {
      DataGridVirtualizingCollectionView collectionView = this.GetCollectionView() as DataGridVirtualizingCollectionView;

      Debug.Assert( collectionView != null );

      List<GroupNameCountPair> subGroupInfos = collectionView.OnQueryGroups( this );
      int subGroupCount = subGroupInfos.Count;

      // Create the collection of sub CollectionViewGroups
      ObservableCollection<object> subCollectionViewGroupList = new ObservableCollection<object>();

      int runningCount = this.StartGlobalIndex;
      for( int i = 0; i < subGroupCount; i++ )
      {
        object subGroupName = subGroupInfos[ i ].Name;
        int subGroupItemCount = subGroupInfos[ i ].ItemCount;

        subCollectionViewGroupList.Add(
          new DataGridVirtualizingCollectionViewGroup( subGroupName, subGroupItemCount, runningCount, this, nextLevel, nextLevelIsBottom ) );

        runningCount += subGroupItemCount;
      }

      return subCollectionViewGroupList;
    }

    #endregion DATA VIRTUALIZATION


    #region INTERNAL PROPERTIES

    internal List<DataGridGroupInfo> GroupPath
    {
      get
      {
        if( m_groupPath == null )
          m_groupPath = this.BuildGroupPath();

        return m_groupPath;
      }
    }

    #endregion INTERNAL PROPERTIES


    #region PRIVATE METHODS

    private List<DataGridGroupInfo> BuildGroupPath()
    {
      DataGridVirtualizingCollectionViewGroup parent = this.Parent as DataGridVirtualizingCollectionViewGroup;

      if( parent == null )
        return new List<DataGridGroupInfo>();

      DataGridVirtualizingCollectionViewBase collectionView = this.GetCollectionView();

      ObservableCollection<GroupDescription> groupDescriptions = collectionView.GroupDescriptions;

      int level = this.Level;

      Debug.Assert( this.Level != -1, "A DataGridCollectionViewGroupRoot should have returned a new List since its parent should have been null." );

      List<DataGridGroupInfo> groupPath = new List<DataGridGroupInfo>( parent.GroupPath );

      Debug.Assert( groupDescriptions.Count > level );

      if( groupDescriptions.Count > level )
        groupPath.Add( new DataGridGroupInfo( groupDescriptions[ level ], this ) );

      return groupPath;
    }

    #endregion PRIVATE METHODS

    #region PRIVATE FIELDS

    private List<DataGridGroupInfo> m_groupPath;

    #endregion PRIVATE FIELDS
  }
}
