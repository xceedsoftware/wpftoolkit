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
  internal class RawItemSortComparer : IComparer<RawItem>
  {
    public RawItemSortComparer( DataGridCollectionView collectionView )
    {
      m_collectionView = collectionView;
    }

    #region IComparer<RawItem> Members

    public int Compare( RawItem xRawItem, RawItem yRawItem )
    {
      if( xRawItem == null )
      {
        if( yRawItem == null )
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
        if( yRawItem == null )
        {
          return 1;
        }
      }

      ListSortDirection lastSortDirection = ListSortDirection.Ascending;
      SortDescriptionCollection sortDescriptions = m_collectionView.SortDescriptions;
      int sortDescriptionCount = sortDescriptions.Count;

      if( sortDescriptionCount > 0 )
      {
        int result;
        DataGridItemPropertyCollection itemProperties = m_collectionView.ItemProperties;

        for( int i = 0; i < sortDescriptionCount; i++ )
        {
          SortDescription sortDescription = sortDescriptions[ i ];
          lastSortDirection = sortDescription.Direction;
          DataGridItemPropertyBase dataProperty = itemProperties[ sortDescription.PropertyName ];

          if( dataProperty == null )
            continue;

          ISupportInitialize supportInitialize = dataProperty as ISupportInitialize;
          object xData = null;
          object yData = null;

          if( supportInitialize != null )
            supportInitialize.BeginInit();

          try
          {
            xData = dataProperty.GetValue( xRawItem.DataItem );
            yData = dataProperty.GetValue( yRawItem.DataItem );
          }
          finally
          {
            if( supportInitialize != null )
              supportInitialize.EndInit();
          }

          IComparer sortComparer = dataProperty.SortComparer;

          if( sortComparer != null )
          {
            result = sortComparer.Compare( xData, yData );
          }
          else
          {
            result = ObjectDataStore.CompareData( xData, yData );
          }

          if( result != 0 )
          {
            if( lastSortDirection == ListSortDirection.Descending )
              result = - result;

            return result;
          }
        }
      }

      if( lastSortDirection == ListSortDirection.Descending )
        return yRawItem.Index - xRawItem.Index;

      return xRawItem.Index - yRawItem.Index;
    }

    #endregion

    private DataGridCollectionView m_collectionView;
  }
}
