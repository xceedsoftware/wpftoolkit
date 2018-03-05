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
using System.Diagnostics;

namespace Xceed.Wpf.DataGrid
{
  internal class RangeSelectionVisitor : IDataGridContextVisitor
  {
    public RangeSelectionVisitor( SelectionRange[] selectedColumns, bool unselect )
    {
      m_selectedColumns = selectedColumns;
      m_unselect = unselect;
    }

    #region IDataGridContextVisitor Members

    public void Visit( DataGridContext sourceContext, ref bool stopVisit )
    {
      throw new NotSupportedException( "The RangeSelectionVisitor is only capable of handling data items block." );
    }

    public void Visit( DataGridContext sourceContext, int startSourceDataItemIndex, int endSourceDataItemIndex, ref bool stopVisit )
    {
      SelectionManager selectionChangerManager = sourceContext.DataGridControl.SelectionChangerManager;

      if( m_selectedColumns != null )
      {
        int columnCount = sourceContext.Columns.Count;

        if( columnCount == 0 )
          return;

        SelectionRange contextColumnMaxRange = new SelectionRange( 0, columnCount - 1 );

        for( int i = 0; i < m_selectedColumns.Length; i++ )
        {
          SelectionRange selectionRange = m_selectedColumns[ i ];
          SelectionRange intersectionSelectionRange = selectionRange.Intersect( contextColumnMaxRange );

          if( intersectionSelectionRange.IsEmpty )
            continue;


          var cellRange = new SelectionCellRangeWithItems( new SelectionRange( startSourceDataItemIndex, endSourceDataItemIndex ), null, intersectionSelectionRange );

          if( m_unselect )
          {
            selectionChangerManager.UnselectCells( sourceContext, cellRange );
          }
          else
          {
            selectionChangerManager.SelectCells( sourceContext, cellRange );
          }
        }
      }
      else
      {
        var itemRange = new SelectionRangeWithItems( new SelectionRange( startSourceDataItemIndex, endSourceDataItemIndex ), null );

        if( m_unselect )
        {
          selectionChangerManager.UnselectItems( sourceContext, itemRange );
        }
        else
        {
          selectionChangerManager.SelectItems( sourceContext, itemRange );
        }
      }
    }

    public void Visit( DataGridContext sourceContext, int sourceDataItemIndex, object item, ref bool stopVisit )
    {
      throw new NotSupportedException( "The RangeSelectionVisitor is only capable of handling data items block." );
    }

    public void Visit( DataGridContext sourceContext, System.Windows.Data.CollectionViewGroup group, object[] namesTree, int groupLevel, bool isExpanded, bool isComputedExpanded, ref bool stopVisit )
    {
      throw new NotSupportedException( "The RangeSelectionVisitor is only capable of handling data items block." );
    }

    public void Visit( DataGridContext sourceContext, System.Windows.DataTemplate headerFooter, ref bool stopVisit )
    {
      throw new NotSupportedException( "The RangeSelectionVisitor is only capable of handling data items block." );
    }

    public void Visit( DataGridContext sourceContext, GroupHeaderFooterItem groupHeaderFooter, ref bool stopVisit )
    {
      throw new NotSupportedException( "The RangeSelectionVisitor is only capable of handling data items block." );
    }

    #endregion

    private SelectionRange[] m_selectedColumns;
    private bool m_unselect;
  }
}
