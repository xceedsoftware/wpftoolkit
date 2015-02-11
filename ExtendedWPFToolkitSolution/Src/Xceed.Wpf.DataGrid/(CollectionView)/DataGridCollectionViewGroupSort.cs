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
using System.ComponentModel;
using System.Collections.Generic;
using System.Text;

using Xceed.Utils.Collections;
using System.Collections.ObjectModel;
using Xceed.Utils.Data;

namespace Xceed.Wpf.DataGrid
{
  internal class DataGridCollectionViewGroupSort : IndexWeakHeapSort
  {
    public DataGridCollectionViewGroupSort( int[] dataIndexArray, GroupSortComparer groupSortedComparer, DataGridCollectionViewGroup[] protectedItems )
      : base( dataIndexArray )
    {
      if( groupSortedComparer == null )
        throw new ArgumentNullException( "groupSortedComparer" );

      m_groupSortedComparer = groupSortedComparer;
      m_groups = protectedItems;
    }

    public override int Compare( int xDataIndex, int yDataIndex )
    {
      DataGridCollectionViewGroup xGroup = m_groups[ xDataIndex ];
      DataGridCollectionViewGroup yGroup = m_groups[ yDataIndex ];

      return m_groupSortedComparer.Compare( xGroup, yGroup );
    }

    private GroupSortComparer m_groupSortedComparer;
    private DataGridCollectionViewGroup[] m_groups;
  }
}
