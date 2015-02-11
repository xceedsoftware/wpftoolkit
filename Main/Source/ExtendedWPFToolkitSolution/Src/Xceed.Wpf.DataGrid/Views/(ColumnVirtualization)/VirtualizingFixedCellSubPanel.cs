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
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Xceed.Wpf.DataGrid.Views
{
  internal class VirtualizingFixedCellSubPanel : FixedCellSubPanel
  {
    public VirtualizingFixedCellSubPanel( FixedCellPanel parentPanel )
      : base( parentPanel )
    {
    }

    // Measure horizontally.
    protected override Size MeasureOverride( Size constraint )
    {
      // This can be null when the parent Row is not prepared yet.
      var columnVirtualizationManager = this.ColumnVirtualizationManager;
      if( columnVirtualizationManager == null )
        return this.DesiredSize;

      var desiredSize = new Size();
      var columnToVisibleWidth = columnVirtualizationManager.GetFieldNameToWidth( this.ParentPanel.ParentRow.LevelCache );

      foreach( var cell in this.GetCellsToLayout() )
      {
        double width;

        if( columnToVisibleWidth.TryGetValue( cell.FieldName, out width ) )
        {
          cell.Measure( new Size( width, constraint.Height ) );
        }
        else
        {
          // The cell will be hidden.
          cell.Measure( new Size() );
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
      // This can be null when the parent Row is not prepared yet.
      var columnVirtualizationManager = this.ColumnVirtualizationManager;
      if( columnVirtualizationManager == null )
        return this.DesiredSize;

      var columnToVisibleOffset = columnVirtualizationManager.GetFieldNameToOffset( this.ParentPanel.ParentRow.LevelCache );

      ScrollViewer parentScrollViewer = this.DataGridContext.DataGridControl.ScrollViewer;
      double horizontalOffset = parentScrollViewer.HorizontalOffset;
      double fixedColumnVisibleWidth = columnVirtualizationManager.FixedColumnsWidth;
      double firstColumnCompensationOffset = columnVirtualizationManager.FirstColumnCompensationOffset;
      Rect finalRect = new Rect( arrangeSize );

      foreach( var cell in this.GetCellsToLayout() )
      {
        // Calculate the offset of the Cell:
        //    The original offset of the Column
        //    - the horizontal offset of the ScrollViewer to scroll to the right
        //    - the width of the fixed columns since we are in the Scrolling FixedCellSubPanel
        //    + the compensation offset used in master detail to avoid scrolling when not required 

        double cellOffset = columnToVisibleOffset[ cell.FieldName ] - horizontalOffset - fixedColumnVisibleWidth + firstColumnCompensationOffset;

        finalRect.X = cellOffset;
        finalRect.Width = cell.DesiredSize.Width;
        finalRect.Height = Math.Max( arrangeSize.Height, cell.DesiredSize.Height );

        cell.Arrange( finalRect );
      }

      return arrangeSize;
    }

    protected override IEnumerable<string> GetVisibleFieldsName()
    {
      var columnVirtualizationManager = this.ColumnVirtualizationManager;
      if( columnVirtualizationManager == null )
        return new string[ 0 ];

      return columnVirtualizationManager.GetScrollingFieldNames( this.ParentPanel.ParentRow.LevelCache );
    }

    private IEnumerable<Cell> GetCellsToLayout()
    {
      var visibleCells = this.GetVisibleCells();

      // Check out if there are any out of view cells that needs to be layouted for different purpose.  In case of a BringIntoView, the cell
      // must be layouted in order to find out the target location.  In case of a non recyclable cell, the cell must be arranged out of view.
      var permanentScrollingFieldNames = this.ParentPanel.PermanentScrollingFieldNames;
      if( !permanentScrollingFieldNames.Any() )
        return visibleCells;

      // Make sure we have access to the collection containing the additional cells to be layouted.
      var cellsCollection = this.ParentPanel.ParentRowCells;
      if( cellsCollection == null )
        return visibleCells;

      return this.GetCellsToLayout( visibleCells, permanentScrollingFieldNames, cellsCollection );
    }

    private IEnumerable<Cell> GetCellsToLayout( IEnumerable<Cell> visibleCells, IEnumerable<string> permanentScrollingFieldNames, VirtualizingCellCollection cellsCollection )
    {
      List<string> unhandledCells = permanentScrollingFieldNames.ToList();

      foreach( var cell in visibleCells )
      {
        unhandledCells.Remove( cell.FieldName );

        yield return cell;
      }

      foreach( var fieldName in unhandledCells )
      {
        var cell = cellsCollection.GetCell( fieldName, false );
        Debug.Assert( cell != null );

        yield return cell;
      }
    }
  }
}
