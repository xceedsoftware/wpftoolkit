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
using System.Windows;
using System.Windows.Controls;
using Xceed.Wpf.DataGrid.Print;

namespace Xceed.Wpf.DataGrid.Views
{
  internal class VirtualizingFixedCellSubPanel : FixedCellSubPanel, IPrintInfo
  {
    public VirtualizingFixedCellSubPanel( FixedCellPanel parentPanel )
      : base( parentPanel )
    {
    }

    // Measure horizontally.
    protected override Size MeasureOverride( Size constraint )
    {
      Size desiredSize = new Size();

      DataGridContext dataGridContext = this.DataGridContext;

      // DataGridContext can be null when a the parent Row was not prepared
      if( dataGridContext == null )
        return this.DesiredSize;

      TableViewColumnVirtualizationManager columnVirtualizationManager = dataGridContext.ColumnVirtualizationManager as TableViewColumnVirtualizationManager;
      Dictionary<string, double> columnToVisibleWidth = columnVirtualizationManager.FieldNameToWidth;

      foreach( Cell cell in this.GetCellsToLayout() )
      {
        double width;

        if( columnToVisibleWidth.TryGetValue( cell.FieldName, out width ) )
        {
          cell.Measure( new Size( width, constraint.Height ) );
        }
        else
        {
          // The cell will be hidden.
          cell.Measure( Size.Empty );
        }

        if( cell.DesiredSize.Height > desiredSize.Height )
        {
          desiredSize.Height = cell.DesiredSize.Height;
        }
      }

      desiredSize.Width = columnVirtualizationManager.ScrollingColumnsWidth;

      return desiredSize;
    }

    // Arrange horizontally.
    protected override Size ArrangeOverride( Size arrangeSize )
    {
      DataGridContext dataGridContext = this.DataGridContext;

      // DataGridContext can be null when the parent Row was not prepared
      if( dataGridContext == null )
        return this.DesiredSize;

      TableViewColumnVirtualizationManager columnVirtualizationManager = dataGridContext.ColumnVirtualizationManager as TableViewColumnVirtualizationManager;
      Dictionary<string, double> columnToVisibleOffset = columnVirtualizationManager.FieldNameToOffset;

      ScrollViewer parentScrollViewer = dataGridContext.DataGridControl.ScrollViewer;
      double horizontalOffset = parentScrollViewer.HorizontalOffset;
      double fixedColumnVisibleWidth = columnVirtualizationManager.FixedColumnsWidth;
      double firstColumnCompensationOffset = columnVirtualizationManager.FirstColumnCompensationOffset;
      Rect finalRect = new Rect( arrangeSize );

      foreach( Cell cell in this.GetCellsToLayout() )
      {
        // Calculate the offset of the Cell:
        //    The original offset of the Column
        //    - the horizontal offset of the ScrollViewer to scroll to the right
        //    - the width of the fixed columns since we are in the Scrolling FixedCellSubPanel
        //    + the compensation offset used in master detail to avoid scrolling when not required 

        double cellOffset = columnToVisibleOffset[ cell.FieldName ]
          - horizontalOffset
          - fixedColumnVisibleWidth
          + firstColumnCompensationOffset;

        finalRect.X = cellOffset;
        finalRect.Width = cell.DesiredSize.Width;
        finalRect.Height = Math.Max( arrangeSize.Height, cell.DesiredSize.Height );

        cell.Arrange( finalRect );
      }

      return arrangeSize;
    }

    protected override IEnumerable<string> GetVisibleFieldsName()
    {
      DataGridContext dataGridContext = this.DataGridContext;
      if( dataGridContext == null )
        return new string[ 0 ];

      TableViewColumnVirtualizationManager columnVirtualizationManager =
        dataGridContext.ColumnVirtualizationManager as TableViewColumnVirtualizationManager;

      return columnVirtualizationManager.ScrollingFieldNames;
    }

    private IEnumerable<Cell> GetCellsToLayout()
    {
      HashSet<Cell> layoutedCells = new HashSet<Cell>( this.GetVisibleCells() );
      CellCollection collection = this.ParentPanel.ParentRowCells;

      if( collection != null )
      {
        // Retrieve the cells that aren't in view anymore but needs to be measured
        // and arranged for different purpose.  In case of a BringIntoView, the cell
        // must be layouted in order to find out the target location.  In case of a
        // non recyclable cell, the cell must be arranged out of view.
        foreach( string fieldName in this.ParentPanel.PermanentScrollingFieldNames )
        {
          Cell cell = collection[ fieldName ];

          if( ( cell != null ) && ( !layoutedCells.Contains( cell ) ) )
          {
            layoutedCells.Add( cell );
          }
        }
      }

      return layoutedCells;
    }
  }
}
