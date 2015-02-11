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
using System.Text;
using System.Windows.Data;

namespace Xceed.Wpf.DataGrid
{
  internal class SelectAllVisitor : IDataGridContextVisitor
  {
    #region CONSTRUCTORS

    public SelectAllVisitor()
    {
    }

    #endregion CONSTRUCTORS

    #region IDataGridContextVisitor Members

    public void Visit( DataGridContext sourceContext, ref bool stopVisit )
    {
      object[] items;
      CollectionView itemsCollection = sourceContext.Items;
      int count = itemsCollection.Count;

      if( count == 0 )
        return;

      if( sourceContext.ItemsSourceCollection is DataGridVirtualizingCollectionViewBase )
      {
        items = null;
      }
      else
      {
        items = new object[ count ];

        for( int i = 0; i < count; i++ )
        {
          items[ i ] = itemsCollection.GetItemAt( i );
        }
      }

      SelectionRange itemRange = new SelectionRange( 0, count - 1 );

      if( sourceContext.DataGridControl.SelectionUnit == SelectionUnit.Row )
      {
        sourceContext.DataGridControl.SelectionChangerManager.SelectItems(
          sourceContext,
          new SelectionRangeWithItems( itemRange, items ) );
      }
      else
      {
        HashedLinkedList<ColumnBase> columnsByVisiblePosition = sourceContext.ColumnsByVisiblePosition;
        SelectedItemsStorage selectedColumnStore = new SelectedItemsStorage( null );
        SelectionRange fullColumnRange = new SelectionRange( 0, columnsByVisiblePosition.Count - 1 );
        selectedColumnStore.Add( new SelectionRangeWithItems( fullColumnRange, null ) );
        int index = 0;

        foreach( ColumnBase column in columnsByVisiblePosition )
        {
          if( !column.Visible )
          {
            selectedColumnStore.Remove( new SelectionRangeWithItems( new SelectionRange( index ), null ) );
          }

          index++;
        }

        int columnRangeCount = selectedColumnStore.Count;

        for( int i = 0; i < columnRangeCount; i++ )
        {
          sourceContext.DataGridControl.SelectionChangerManager.SelectCells(
            sourceContext,
            new SelectionCellRangeWithItems( itemRange, items, selectedColumnStore[ i ].Range ) );
        }
      }
    }

    public void Visit( DataGridContext sourceContext, int startSourceDataItemIndex, int endSourceDataItemIndex, ref bool stopVisit )
    {
      throw new NotSupportedException( "Only DataGridContexts can be visited by this visitor." );
    }

    public void Visit( DataGridContext sourceContext, int sourceDataItemIndex, object item, ref bool stopVisit )
    {
      throw new NotSupportedException( "Only DataGridContexts can be visited by this visitor." );
    }

    public void Visit( DataGridContext sourceContext, System.Windows.Data.CollectionViewGroup group, object[] namesTree, int groupLevel, bool isExpanded, bool isComputedExpanded, ref bool stopVisit )
    {
      throw new NotSupportedException( "Only DataGridContexts can be visited by this visitor." );
    }

    public void Visit( DataGridContext sourceContext, System.Windows.DataTemplate headerFooter, ref bool stopVisit )
    {
      throw new NotSupportedException( "Only DataGridContexts can be visited by this visitor." );
    }

    public void Visit( DataGridContext sourceContext, GroupHeaderFooterItem groupHeaderFooter, ref bool stopVisit )
    {
      throw new NotSupportedException( "Only DataGridContexts can be visited by this visitor." );
    }

    #endregion
  }
}
