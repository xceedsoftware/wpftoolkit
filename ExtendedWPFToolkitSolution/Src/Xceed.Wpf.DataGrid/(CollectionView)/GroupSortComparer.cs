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

      var sortDirection = ListSortDirection.Ascending;
      int result;

      foreach( SortInfo sortInfo in m_sortInfos )
      {
        var foreignKeyDescription = sortInfo.ForeignKeyDescription;
        var sortComparer = sortInfo.SortComparer;
        sortDirection = sortInfo.SortDirection;

        if( foreignKeyDescription != null )
        {
          result = sortComparer.Compare( foreignKeyDescription.GetDisplayValue( xGroup.Name ), foreignKeyDescription.GetDisplayValue( yGroup.Name ) );
        }
        else
        {
          result = sortComparer.Compare( xGroup.Name, yGroup.Name );
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
      public SortInfo( ListSortDirection sortDirection, IComparer sortComparer )
      {
        if( sortComparer == null )
          throw new ArgumentNullException( "sortComparer" );

        this.SortDirection = sortDirection;
        this.SortComparer = sortComparer;
      }

      public SortInfo( DataGridForeignKeyDescription foreignKeyDescription, ListSortDirection sortDirection, IComparer sortComparer )
      {
        if( sortComparer == null )
          throw new ArgumentNullException( "sortComparer" );

        this.ForeignKeyDescription = foreignKeyDescription;
        this.SortDirection = sortDirection;
        this.SortComparer = sortComparer;
      }


      #region ForeignKeyDescription Property

      internal DataGridForeignKeyDescription ForeignKeyDescription
      {
        get;
        private set;
      }

      #endregion

      #region SortDirection Property

      public ListSortDirection SortDirection
      {
        get;
        private set;
      }

      #endregion

      #region SortComparer Property

      public IComparer SortComparer
      {
        get;
        private set;
      }

      #endregion
    }
  }
}
