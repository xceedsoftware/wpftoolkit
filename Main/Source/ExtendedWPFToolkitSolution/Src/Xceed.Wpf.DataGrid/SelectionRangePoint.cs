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
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace Xceed.Wpf.DataGrid
{
  internal class SelectionRangePoint
  {
    public object Item { get; private set; }
    public int ColumnIndex { get; set; }
    public DataGridContext DataGridContext { get; private set; }

    public int ItemIndex
    {
      get
      {
        if( m_localIndex == null )
        {
          m_localIndex = this.DataGridContext.Items.IndexOf( this.Item );
        }

        return m_localIndex.Value;
      }

    }

    public int ItemGlobalIndex
    {
      get
      {
        if( m_globalIndex == null )
        {
          m_globalIndex = this.DataGridContext.DataGridControl
            .GetGlobalGeneratorIndexFromItem( this.DataGridContext, this.Item );
        }

        return m_globalIndex.Value;
      }
    }

    public bool GetIsSelected( SelectionUnit selectionUnit )
    {
      if(selectionUnit == SelectionUnit.Cell)
      {
        if(this.ItemIndex != -1 && this.ColumnIndex != -1)
        {
          return this.DataGridContext.SelectedCellsStore.Contains( this.ItemIndex, this.ColumnIndex );
        }
      }
      else if(selectionUnit == SelectionUnit.Row)
      {
        if(this.ItemIndex != -1)
        {
          return this.DataGridContext.SelectedItemsStore.Contains( this.ItemIndex );
        }
      }

      return false;
    }

    private SelectionRangePoint() { }

    public SelectionRangePoint( DataGridContext dataGridContext, object item, int itemIndex, int columnPosition )
    {
      if( dataGridContext == null )
        throw new ArgumentNullException();

      if( item == null )
        throw new ArgumentNullException();

      m_localIndex = itemIndex;
      this.Item = item;
      this.ColumnIndex = columnPosition;
      this.DataGridContext = dataGridContext;
    }

    public static SelectionRangePoint TryCreateRangePoint( DataGridContext dataGridContext, object item, int itemIndex, int columnIndex )
    {
      if( dataGridContext != null && item != null )
        return new SelectionRangePoint( dataGridContext, item, itemIndex, columnIndex );

      return null;
    }

    public static SelectionRangePoint TryCreateFromCurrent( DataGridContext dataGridContext )
    {
      if( dataGridContext == null )
        return null;

      var column = dataGridContext.CurrentColumn;
      int columnIndex = ( column != null ) ? column.VisiblePosition : -1;

      var oldPosition = SelectionRangePoint.TryCreateRangePoint(
        dataGridContext, dataGridContext.CurrentItem,
        dataGridContext.CurrentItemIndex, columnIndex );

      return oldPosition;
    }



    public SelectionRangePoint CreateCopy()
    {
      var copy = new SelectionRangePoint();

      copy.m_globalIndex = this.m_globalIndex;
      copy.m_localIndex = this.m_localIndex;
      copy.Item = this.Item;
      copy.ColumnIndex = this.ColumnIndex;
      copy.DataGridContext = this.DataGridContext;

      return copy;
    }

    public SelectionRangeWithItems ToSelectionRangeWithItems()
    {
      return new SelectionRangeWithItems( this.ItemIndex, this.Item );
    }


    private int? m_globalIndex;
    private int? m_localIndex;
  }
}
