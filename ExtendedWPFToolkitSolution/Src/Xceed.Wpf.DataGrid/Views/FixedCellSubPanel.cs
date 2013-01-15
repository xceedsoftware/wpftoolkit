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
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using Xceed.Wpf.DataGrid.Print;

namespace Xceed.Wpf.DataGrid.Views
{
  // Panel used as children of the FixedCellPanel that only layout horizontally.
  internal class FixedCellSubPanel : Panel, IPrintInfo
  {
    public FixedCellSubPanel( FixedCellPanel parentPanel )
    {
      if( parentPanel == null )
        throw new ArgumentNullException( "parentPanel" );

      m_parentPanel = parentPanel;
    }

    #region DataGridContext Property

    internal DataGridContext DataGridContext
    {
      get
      {
        return m_parentPanel.DataGridContext;
      }
    }

    #endregion

    #region ParentFixedCellPanel Property

    internal FixedCellPanel ParentPanel
    {
      get
      {
        return m_parentPanel;
      }
    }

    private readonly FixedCellPanel m_parentPanel;

    #endregion

    protected override UIElementCollection CreateUIElementCollection( FrameworkElement logicalParent )
    {
      // We make sure that the element added to this panel won't have this panel as 
      // logical parent. We want the logical parent of these element to be the 
      // FixedCellPanel itself. This is handled in the UICellCollection class.
      return new UIElementCollection( this, null );
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

      foreach( Cell cell in this.GetVisibleCells() )
      {
        Debug.Assert( !Row.GetIsTemplateCell( cell ), "No template Cells should be added to FixedCellPanel" );

        cell.Measure( new Size( columnVirtualizationManager.FieldNameToWidth[ cell.FieldName ], constraint.Height ) );

        if( cell.DesiredSize.Height > desiredSize.Height )
        {
          desiredSize.Height = cell.DesiredSize.Height;
        }
      }

      desiredSize.Width = columnVirtualizationManager.FixedColumnsWidth;

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
      Rect finalRect = new Rect( arrangeSize );

      foreach( Cell cell in this.GetVisibleCells() )
      {
        // Never add Template Cells to FixedCellPanel
        if( Row.GetIsTemplateCell( cell ) )
          continue;

        // Calculate the offset of the Cell:
        //    The original offset of the Column
        //    - the horizontal offset of the ScrollViewer to scroll to the right
        //    - the width of the fixed columns since we are in the Scrolling FixedCellSubPanel
        //    + the compensation offset used in master detail to avoid scrolling when not required 

        double cellOffset = columnToVisibleOffset[ cell.FieldName ];

        finalRect.X = cellOffset;
        finalRect.Width = cell.DesiredSize.Width;
        finalRect.Height = Math.Max( arrangeSize.Height, cell.DesiredSize.Height );

        cell.Arrange( finalRect );
      }

      return arrangeSize;
    }

    protected virtual IEnumerable<string> GetVisibleFieldsName()
    {
      DataGridContext dataGridContext = this.DataGridContext;
      if( dataGridContext == null )
        return new string[ 0 ];

      TableViewColumnVirtualizationManager columnVirtualizationManager =
        dataGridContext.ColumnVirtualizationManager as TableViewColumnVirtualizationManager;

      return columnVirtualizationManager.FixedFieldNames;
    }

    protected IEnumerable<Cell> GetVisibleCells()
    {
      CellCollection collection = this.ParentPanel.ParentRowCells;

      if( collection != null )
      {
        foreach( string fieldName in this.GetVisibleFieldsName() )
        {
          yield return collection[ fieldName ];
        }
      }
    }

    #region IPrintInfo Members

    double IPrintInfo.GetPageRightOffset( double horizontalOffset, double viewportWidth )
    {
      Cell firstCellInView = null;
      double offset = 0.0d;
      double accumulatedWidth = 0.0d;

      foreach( Cell cell in this.GetVisibleCells() )
      {
        double cellWidth = cell.ActualWidth;

        offset += cellWidth;

        // The cell is in view.
        if( offset > horizontalOffset )
        {
          if( firstCellInView == null )
          {
            firstCellInView = cell;
          }

          accumulatedWidth += cellWidth;

          if( accumulatedWidth > viewportWidth )
          {
            // If the first cell is larger than the viewPort, break.
            if( cell != firstCellInView )
            {
              accumulatedWidth -= cellWidth;
            }

            break;
          }
        }
      }

      return accumulatedWidth + horizontalOffset;
    }

    void IPrintInfo.UpdateElementVisibility( double horizontalOffset, double viewportWidth, object state )
    {
      IDictionary<string, object> visibilityState = ( IDictionary<string, object> )state;
      Debug.Assert( visibilityState != null );

      double accumulatedWidth = 0.0d;
      bool firstCellInViewportFound = false;
      double viewportLimitsHorizontalOffset = horizontalOffset + viewportWidth;

      foreach( Cell cell in this.GetVisibleCells() )
      {
        Visibility cellVisibility = cell.Visibility;
        if( cellVisibility == Visibility.Collapsed )
          continue;

        accumulatedWidth += cell.ActualWidth;

        bool isPastViewportStart = accumulatedWidth > horizontalOffset;
        bool isBeforeViewportEnd = isPastViewportStart && ( accumulatedWidth <= ( viewportLimitsHorizontalOffset ) );

        bool isFirstInViewport = ( isPastViewportStart ) && ( !firstCellInViewportFound );

        if( isFirstInViewport )
          firstCellInViewportFound = true;

        if( isPastViewportStart )
        {
          // The cell is past the Viewport's start.
          if( ( isBeforeViewportEnd ) || ( isFirstInViewport ) )
          {
            // Either the cell is completely visible in the Viewport
            // or we face a cell which is the first in the Viewport but does not completely fit in the Viewport.
            FixedCellSubPanel.RestoreCellVisibility( cell, visibilityState );
          }
          else if( cellVisibility == Visibility.Visible )
          {
            // The cell is past the Viewport's end.
            FixedCellSubPanel.HideCell( cell, visibilityState );
          }
        }
        else if( cellVisibility == Visibility.Visible )
        {
          // The cell is before the Viewport's start.
          FixedCellSubPanel.HideCell( cell, visibilityState );
        }
      }
    }

    object IPrintInfo.CreateElementVisibilityState()
    {
      return new Dictionary<string, object>();
    }

    private static void HideCell( Cell cell, IDictionary<string, object> visibilityState )
    {
      visibilityState[ cell.FieldName ] = cell.ReadLocalValue( Cell.VisibilityProperty );
      cell.Visibility = Visibility.Hidden;
    }

    private static void RestoreCellVisibility( Cell cell, IDictionary<string, object> visibilityState )
    {
      string fieldName = cell.FieldName;
      object storedVisibility;

      if( !visibilityState.TryGetValue( fieldName, out storedVisibility ) )
        return;

      // Restore Visibility
      if( storedVisibility is Visibility )
      {
        cell.Visibility = ( Visibility )storedVisibility;
      }
      else
      {
        cell.ClearValue( Cell.VisibilityProperty );
      }

      visibilityState.Remove( fieldName );
    }

    #endregion
  }
}
