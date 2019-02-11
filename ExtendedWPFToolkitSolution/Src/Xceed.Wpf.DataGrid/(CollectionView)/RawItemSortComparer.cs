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
        return ( yRawItem == null ) ? 0 : -1;

      if( yRawItem == null )
        return 1;

      var lastSortDirection = ListSortDirection.Ascending;
      var sortDescriptions = m_collectionView.SortDescriptions;
      var sortDescriptionCount = sortDescriptions.Count;

      if( sortDescriptionCount > 0 )
      {
        var result = default( int );
        var itemProperties = m_collectionView.ItemProperties;

        for( int i = 0; i < sortDescriptionCount; i++ )
        {
          var sortDescription = sortDescriptions[ i ];
          lastSortDirection = sortDescription.Direction;

          var itemProperty = ItemsSourceHelper.GetItemPropertyFromProperty( itemProperties, sortDescription.PropertyName );
          if( itemProperty == null )
            continue;

          var supportInitialize = itemProperty as ISupportInitialize;
          var xData = default( object );
          var yData = default( object );

          if( supportInitialize != null )
          {
            supportInitialize.BeginInit();
          }

          try
          {
            xData = ItemsSourceHelper.GetValueFromItemProperty( itemProperty, xRawItem.DataItem );
            yData = ItemsSourceHelper.GetValueFromItemProperty( itemProperty, yRawItem.DataItem );

            if( itemProperty.IsSortingOnForeignKeyDescription )
            {
              var foreignKeyDescription = itemProperty.ForeignKeyDescription;

              xData = foreignKeyDescription.GetDisplayValue( xData );
              yData = foreignKeyDescription.GetDisplayValue( yData );
            }
          }
          finally
          {
            if( supportInitialize != null )
            {
              supportInitialize.EndInit();
            }
          }

          var sortComparer = itemProperty.SortComparer;

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
              return -result;

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
