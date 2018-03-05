/*************************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at http://xceed.com/wpf_toolkit

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;

namespace Xceed.Wpf.DataGrid
{
  internal class DataGridVirtualizingCollectionViewGroup : DataGridVirtualizingCollectionViewGroupBase, IWeakEventListener
  {
    private static List<DataGridGroupInfo> EmptyDataGridInfoList = new List<DataGridGroupInfo>( 0 );

    internal DataGridVirtualizingCollectionViewGroup( object name, int initialItemsCount, int startGlobalIndex, DataGridVirtualizingCollectionViewGroup parent, int level, bool isBottomLevel )
      : base( name, initialItemsCount, startGlobalIndex, parent, level, isBottomLevel )
    {
    }

    #region GroupPath Property

    internal List<DataGridGroupInfo> GroupPath
    {
      get
      {
        if( m_groupPath == null )
        {
          m_groupPath = this.BuildGroupPath();
        }

        return m_groupPath;
      }
    }

    #endregion

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

        subCollectionViewGroupList.Add( new DataGridVirtualizingCollectionViewGroup( subGroupName, subGroupItemCount, runningCount, this, nextLevel, nextLevelIsBottom ) );

        runningCount += subGroupItemCount;
      }

      return subCollectionViewGroupList;
    }

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
      {
        groupPath.Add( new DataGridGroupInfo( groupDescriptions[ level ], this ) );
      }

      return groupPath;
    }

    private List<DataGridGroupInfo> m_groupPath;
  }
}
