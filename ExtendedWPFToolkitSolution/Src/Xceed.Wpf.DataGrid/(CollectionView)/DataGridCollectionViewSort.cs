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
using System.ComponentModel;
using Xceed.Utils.Collections;

namespace Xceed.Wpf.DataGrid
{
  internal class DataGridCollectionViewSort : IndexWeakHeapSort
  {
    public DataGridCollectionViewSort( int[] dataIndexArray, IList<SortDescriptionInfo> sortDescriptionInfos )
      : base( dataIndexArray )
    {
      m_sortDescriptionInfos = sortDescriptionInfos;
    }

    public override int Compare( int xDataIndex, int yDataIndex )
    {
      var lastSortDirection = ListSortDirection.Ascending;

      if( m_sortDescriptionInfos != null )
      {
        foreach( var sortDescriptionInfo in m_sortDescriptionInfos )
        {
          lastSortDirection = sortDescriptionInfo.SortDirection;

          if( sortDescriptionInfo.Property == null )
            continue;

          var sortComparer = sortDescriptionInfo.SortComparer;
          var dataStore = sortDescriptionInfo.DataStore;
          var compare = ( sortComparer != null )
                          ? sortComparer.Compare( dataStore.GetData( xDataIndex ), dataStore.GetData( yDataIndex ) )
                          : dataStore.Compare( xDataIndex, yDataIndex );

          if( compare != 0 )
            return ( lastSortDirection == ListSortDirection.Descending ) ? -compare : compare;
        }
      }

      if( lastSortDirection == ListSortDirection.Descending )
        return yDataIndex - xDataIndex;

      return xDataIndex - yDataIndex;
    }

    private readonly IList<SortDescriptionInfo> m_sortDescriptionInfos;
  }
}
