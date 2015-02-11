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
  // Panel used as children of the FixedCellPanel that only layout horizontally.
  internal class FixedCellSubPanel : Panel
  {
    public FixedCellSubPanel( FixedCellPanel parentPanel )
    {
      if( parentPanel == null )
        throw new ArgumentNullException( "parentPanel" );

      m_parentPanel = parentPanel;
    }

    #region DataGridContext Internal Property

    internal DataGridContext DataGridContext
    {
      get
      {
        return m_parentPanel.DataGridContext;
      }
    }

    #endregion

    #region ParentFixedCellPanel Internal Property

    internal FixedCellPanel ParentPanel
    {
      get
      {
        return m_parentPanel;
      }
    }

    private readonly FixedCellPanel m_parentPanel;

    #endregion

    #region ColumnVirtualizationManager Internal Property

    internal TableViewColumnVirtualizationManagerBase ColumnVirtualizationManager
    {
      get
      {
        var dataGridContext = this.DataGridContext;
        if( dataGridContext == null )
          return null;

        return ( TableViewColumnVirtualizationManagerBase )dataGridContext.ColumnVirtualizationManager;
      }
    }

    #endregion

    protected override UIElementCollection CreateUIElementCollection( FrameworkElement logicalParent )
    {
      // We make sure that the element added to this panel won't have this panel as logical parent.
      // We want the logical parent of these element to be the FixedCellPanel itself. This is handled in the UICellCollection class.
      return new UIElementCollection( this, null );
    }

    // Measure horizontally.
    protected override Size MeasureOverride( Size constraint )
    {
      // This can be null when the parent Row is not prepared yet.
      var columnVirtualizationManager = this.ColumnVirtualizationManager;
      if( columnVirtualizationManager == null )
        return this.DesiredSize;

      var desiredSize = new Size();
      var fieldNameToWidth = columnVirtualizationManager.GetFieldNameToWidth( m_parentPanel.ParentRow.LevelCache );

      foreach( var cell in this.GetVisibleCells() )
      {
        Debug.Assert( !Row.GetIsTemplateCell( cell ), "No template Cells should be added to FixedCellPanel" );

        cell.Measure( new Size( fieldNameToWidth[ cell.FieldName ], constraint.Height ) );

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
      // This can be null when the parent Row is not prepared yet.
      var columnVirtualizationManager = this.ColumnVirtualizationManager;
      if( columnVirtualizationManager == null )
        return this.DesiredSize;

      var columnToVisibleOffset = columnVirtualizationManager.GetFieldNameToOffset( m_parentPanel.ParentRow.LevelCache );
      var finalRect = new Rect( arrangeSize );

      foreach( var cell in this.GetVisibleCells() )
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
      var columnVirtualizationManager = this.ColumnVirtualizationManager;
      if( columnVirtualizationManager == null )
        return new string[ 0 ];

      return columnVirtualizationManager.GetFixedFieldNames( m_parentPanel.ParentRow.LevelCache );
    }

    protected IEnumerable<Cell> GetVisibleCells()
    {
      VirtualizingCellCollection collection = this.ParentPanel.ParentRowCells;

      if( collection == null )
        yield break;

      foreach( string fieldName in this.GetVisibleFieldsName() )
      {
        yield return collection.GetCell( fieldName, false );
      }
    }
  }
}
