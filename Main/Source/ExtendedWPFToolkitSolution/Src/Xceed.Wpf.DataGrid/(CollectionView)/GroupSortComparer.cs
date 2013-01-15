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
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

using Xceed.Utils.Data;

namespace Xceed.Wpf.DataGrid
{
  internal class GroupSortComparer : IComparer<DataGridCollectionViewGroup>
  {
    public GroupSortComparer( List<SortInfo> sortInfos )
    {
      m_sortInfos = sortInfos;
    }

    #region IComparer<DataGridCollectionViewGroup> Members

    public int Compare( DataGridCollectionViewGroup xGroup, DataGridCollectionViewGroup yGroup )
    {
      if( xGroup == null )
      {
        if( yGroup == null )
        {
          return 0;
        }
        else
        {
          return -1;
        }
      }
      else
      {
        if( yGroup == null )
        {
          return 1;
        }
      }

      ListSortDirection sortDirection = ListSortDirection.Ascending;
      int result;
      int sortInfoCount = m_sortInfos.Count;

      for( int i = 0; i < sortInfoCount; i++ )
      {
        SortInfo sortInfo = m_sortInfos[ i ];
        string statResultPropertyName = sortInfo.StatResultPropertyName;
        IComparer sortComparer = sortInfo.SortComparer;
        sortDirection = sortInfo.SortDirection;

        if( statResultPropertyName == null )
        {
          result = sortComparer.Compare( xGroup.Name, yGroup.Name );
        }
        else
        {
          result = sortComparer.Compare(
            xGroup.GetStatFunctionValue( statResultPropertyName ),
            yGroup.GetStatFunctionValue( statResultPropertyName ) );
        }

        if( result != 0 )
        {
          if( sortDirection == ListSortDirection.Descending )
            return -result;

          return result;
        }
      }

      if( sortDirection == ListSortDirection.Descending )
        return yGroup.UnsortedIndex - xGroup.UnsortedIndex;

      return xGroup.UnsortedIndex - yGroup.UnsortedIndex;
    }

    #endregion

    private List<SortInfo> m_sortInfos;

    public class SortInfo
    {
      public SortInfo( string name, ListSortDirection sortDirection, IComparer sortComparer )
      {
        if( sortComparer == null )
          throw new ArgumentNullException( "sortComparer" );

        this.StatResultPropertyName = name;
        this.SortDirection = sortDirection;
        this.SortComparer = sortComparer;
      }

      public string StatResultPropertyName
      {
        get;
        private set;
      }

      public ListSortDirection SortDirection
      {
        get;
        private set;
      }

      public IComparer SortComparer
      {
        get;
        private set;
      }
    }
  }
}
