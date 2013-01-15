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
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Xceed.Wpf.DataGrid
{
  internal class RangeSelectionVisitor : IDataGridContextVisitor
  {
    public RangeSelectionVisitor( SelectionRange[] selectedColumns )
    {
      m_selectedColumns = selectedColumns;
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

          Debug.WriteLine( "Selection : Adding cell : (" + startSourceDataItemIndex.ToString() + " - " + endSourceDataItemIndex.ToString() + ") - ("
             + intersectionSelectionRange.StartIndex.ToString() + " - " + intersectionSelectionRange.EndIndex.ToString() + ")" );

          selectionChangerManager.SelectCells(
            sourceContext,
            new SelectionCellRangeWithItems( new SelectionRange( startSourceDataItemIndex, endSourceDataItemIndex ), null, intersectionSelectionRange ) );
        }
      }
      else
      {
        Debug.WriteLine( "Selection : Adding item : " + startSourceDataItemIndex.ToString() + " - " + endSourceDataItemIndex.ToString() );

        selectionChangerManager.SelectItems(
          sourceContext,
          new SelectionRangeWithItems( new SelectionRange( startSourceDataItemIndex, endSourceDataItemIndex ), null ) );
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
  }
}
