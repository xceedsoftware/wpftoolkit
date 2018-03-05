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
        return new Size();

      var parentRow = this.ParentPanel.ParentRow;
      if( parentRow == null )
        return new Size();

      var desiredSize = new Size();
      var columnToVisibleWidth = columnVirtualizationManager.GetFieldNameToWidth( parentRow.LevelCache );

      //This using prevents a WeakEvent to be raised to execute UpdateChildren() in FixedCellPanel, as it is already up to date at this point.
      using( this.ParentPanel.ParentRowCells.SetIsUpdating() )
      {
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
        return new Size();

      var parentRow = this.ParentPanel.ParentRow;
      if( parentRow == null )
        return new Size();

      var dataGridContext = this.DataGridContext;
      var scrollViewer = ( dataGridContext != null ) ? dataGridContext.DataGridControl.ScrollViewer : null;

      var columnToVisibleOffset = columnVirtualizationManager.GetFieldNameToOffset( parentRow.LevelCache );
      var horizontalOffset = ( scrollViewer != null ) ? scrollViewer.HorizontalOffset : 0d;
      var fixedColumnsWidth = columnVirtualizationManager.FixedColumnsWidth;
      var compensationOffset = columnVirtualizationManager.FirstColumnCompensationOffset;
      var finalRect = new Rect( arrangeSize );

      //This using prevents a WeakEvent to be raised to execute UpdateChildren() in FixedCellPanel, as it is already up to date at this point.
      using( this.ParentPanel.ParentRowCells.SetIsUpdating() )
      {
        foreach( var cell in this.GetCellsToLayout() )
        {
          var offset = this.CalculateCellOffset( cell.ParentColumn, columnToVisibleOffset, horizontalOffset, fixedColumnsWidth, compensationOffset );

          finalRect.X = offset.X;
          finalRect.Width = cell.DesiredSize.Width;
          finalRect.Height = Math.Max( arrangeSize.Height, cell.DesiredSize.Height );

          cell.Arrange( finalRect );
        }
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

    internal override Point CalculateCellOffset( ColumnBase column )
    {
      Debug.Assert( column != null );
      var row = this.ParentPanel.ParentRow;

      if( ( column != null ) && ( row != null ) )
      {
        var dataGridContext = this.DataGridContext;
        var columnVirtualizationManager = this.ColumnVirtualizationManager;

        if( ( dataGridContext != null ) && ( columnVirtualizationManager != null ) )
        {
          var columnToVisibleOffset = columnVirtualizationManager.GetFieldNameToOffset( row.LevelCache );
          var scrollViewer = dataGridContext.DataGridControl.ScrollViewer;
          var horizontalOffset = ( scrollViewer != null ) ? scrollViewer.HorizontalOffset : 0d;
          var fixedColumnsWidth = columnVirtualizationManager.FixedColumnsWidth;
          var compensationOffset = columnVirtualizationManager.FirstColumnCompensationOffset;

          return this.CalculateCellOffset( column, columnToVisibleOffset, horizontalOffset, fixedColumnsWidth, compensationOffset );
        }
      }

      return new Point();
    }

    private Point CalculateCellOffset( ColumnBase column, IColumnInfoCollection<double> columnsOffset, double horizontalOffset, double fixedColumnsWidth, double compensationOffset )
    {
      if( column == null )
        return new Point();

      Debug.Assert( columnsOffset != null );
      Debug.Assert( columnsOffset.Contains( column ) );

      var columnOffset = columnsOffset[ column ];

      // Calculate the offset of the cell's parent column:
      //    The original offset of the Column
      //    - the horizontal offset of the ScrollViewer to scroll to the right
      //    - the width of the fixed columns since we are in the Scrolling FixedCellSubPanel
      //    + the compensation offset used in master detail to avoid scrolling when not required 
      return new Point( columnOffset - horizontalOffset - fixedColumnsWidth + compensationOffset, 0d );
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
      var parentRowCells = this.ParentPanel.ParentRowCells;
      if( parentRowCells == null )
        return visibleCells;

      return this.GetCellsToLayout( visibleCells, permanentScrollingFieldNames, parentRowCells );
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
