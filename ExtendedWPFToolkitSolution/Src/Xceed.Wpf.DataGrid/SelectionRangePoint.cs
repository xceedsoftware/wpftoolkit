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
using System.Linq;

namespace Xceed.Wpf.DataGrid
{
  internal sealed class SelectionRangePoint
  {
    private SelectionRangePoint()
    {
    }

    private SelectionRangePoint( DataGridContext dataGridContext, object item, int itemIndex, int columnPosition )
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

    private SelectionRangePoint( Group group )
    {
      if( group == null )
        throw new ArgumentNullException();

      this.Group = group;
      this.DataGridContext = group.DataGridContext;
      this.ColumnIndex = -1;
    }

    public object Item
    {
      get;
      private set;
    }

    public int ColumnIndex
    {
      get;
      set;
    }

    public Group Group
    {
      get;
      private set;
    }

    public DataGridContext DataGridContext
    {
      get;
      private set;
    }

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
          m_globalIndex = this.DataGridContext.DataGridControl.GetGlobalGeneratorIndexFromItem( this.DataGridContext, this.Item );
        }

        return m_globalIndex.Value;
      }
    }

    public bool GetIsSelected( SelectionUnit selectionUnit )
    {
      if( selectionUnit == SelectionUnit.Cell )
      {
        return this.DataGridContext.SelectedCellsStore.Contains( this.ItemIndex, this.ColumnIndex );
      }
      else if( selectionUnit == SelectionUnit.Row )
      {
        if( this.ItemIndex >= 0 )
        {
          return this.DataGridContext.SelectedItemsStore.Contains( this.ItemIndex );
        }
      }

      return false;
    }

    public static SelectionRangePoint TryCreateRangePoint( DataGridContext dataGridContext, object item, int itemIndex, int columnIndex )
    {
      if( ( dataGridContext == null ) || ( item == null ) )
        return null;

      if( !( item is GroupHeaderFooterItem ) )
        return new SelectionRangePoint( dataGridContext, item, itemIndex, columnIndex );

      var dataGridControl = dataGridContext.DataGridControl;
      if( ( dataGridControl == null ) )
        return null;

      var group = dataGridContext.GetGroupFromCollectionViewGroup( ( ( GroupHeaderFooterItem )item ).Group );

      return SelectionRangePoint.TryCreateRangePoint( group );
    }

    public static SelectionRangePoint TryCreateRangePoint( Group group )
    {
      if( group != null )
        return new SelectionRangePoint( group );

      return null;
    }

    public static SelectionRangePoint TryCreateFromCurrent( DataGridContext dataGridContext )
    {
      if( dataGridContext == null )
        return null;

      var dataGridControl = dataGridContext.DataGridControl;
      if( dataGridControl == null )
        return null;

      var column = dataGridContext.CurrentColumn;
      int columnIndex = ( column != null ) ? column.VisiblePosition : -1;

      var oldPosition = SelectionRangePoint.TryCreateRangePoint( dataGridContext, dataGridContext.CurrentItem, dataGridContext.CurrentItemIndex, columnIndex );
      if( oldPosition == null )
      {
        oldPosition = SelectionRangePoint.TryCreateRangePoint( dataGridContext, dataGridContext.InternalCurrentItem, -1, columnIndex );
      }

      return oldPosition;
    }

    public SelectionRangeWithItems ToSelectionRangeWithItems()
    {
      if( this.Item != null )
      {
        return new SelectionRangeWithItems( this.ItemIndex, this.Item );
      }
      else
      {
        var group = this.Group;
        var dataGridContext = group.DataGridContext;
        var groupList = group.GetLeafItems().ToArray();
        int firstIndex = dataGridContext.Items.IndexOf( groupList[ 0 ] );
        int lastIndex = dataGridContext.Items.IndexOf( groupList[ groupList.Length - 1 ] );

        return new SelectionRangeWithItems( new SelectionRange( firstIndex, lastIndex ), groupList );
      }
    }

    private int? m_globalIndex;
    private int? m_localIndex;
  }
}
