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
using System.ComponentModel;
using System.Collections.Generic;
using System.Text;

using Xceed.Utils.Collections;
using System.Collections;
using Xceed.Utils.Data;

namespace Xceed.Wpf.DataGrid
{
  internal class DataGridCollectionViewSort : IndexWeakHeapSort
  {
    public DataGridCollectionViewSort( int[] dataIndexArray, SortDescriptionInfo[] sortDescriptionInfos )
      : base( dataIndexArray )
    {
      m_sortDescriptionInfos = sortDescriptionInfos;
    }

    public override int Compare( int xDataIndex, int yDataIndex )
    {
      ListSortDirection lastSortDirection = ListSortDirection.Ascending;

      if( m_sortDescriptionInfos != null )
      {
        int result = 0;
        int count = m_sortDescriptionInfos.Length;

        for( int i = 0; i < count; i++ )
        {
          SortDescriptionInfo sortDescriptionInfo = m_sortDescriptionInfos[ i ];

          lastSortDirection = sortDescriptionInfo.SortDirection;

          if( sortDescriptionInfo.Property == null )
            continue;

          IComparer sortComparer = sortDescriptionInfo.SortComparer;
          DataStore dataStore = sortDescriptionInfo.DataStore;

          if( sortComparer != null )
          {
            result = sortComparer.Compare( dataStore.GetData( xDataIndex ), dataStore.GetData( yDataIndex ) );
          }
          else
          {
            result = dataStore.Compare( xDataIndex, yDataIndex );
          }

          if( result != 0 )
          {
            if( lastSortDirection == ListSortDirection.Descending )
              result = -result;

            return result;
          }
        }
      }

      if( lastSortDirection == ListSortDirection.Descending )
        return yDataIndex - xDataIndex;

      return xDataIndex - yDataIndex;
    }

    private SortDescriptionInfo[] m_sortDescriptionInfos;
  }
}
