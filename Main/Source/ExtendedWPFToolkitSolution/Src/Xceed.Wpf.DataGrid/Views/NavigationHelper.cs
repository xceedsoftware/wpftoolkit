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
using System.Windows;
using System.Windows.Input;

namespace Xceed.Wpf.DataGrid.Views
{
  internal static class NavigationHelper
  {
    internal static int GetFirstVisibleFocusableColumnIndex( DataGridContext dataGridContext )
    {
      if( dataGridContext == null )
        return -1;

      return NavigationHelper.GetFirstVisibleFocusableColumnIndex( dataGridContext, dataGridContext.CurrentRow );
    }

    internal static int GetFirstVisibleFocusableColumnIndex( DataGridContext dataGridContext, Row targetRow )
    {
      if( dataGridContext == null )
        return -1;

      if( dataGridContext.IsAFlattenDetail )
        return NavigationHelper.GetNextVisibleFocusableDetailColumnIndexFromMasterColumnIndex( dataGridContext, targetRow, int.MinValue );

      return NavigationHelper.GetNextVisibleFocusableColumnIndex( dataGridContext, targetRow, int.MinValue );
    }

    internal static int GetLastVisibleFocusableColumnIndex( DataGridContext dataGridContext )
    {
      if( dataGridContext == null )
        return -1;

      return NavigationHelper.GetLastVisibleFocusableColumnIndex( dataGridContext, dataGridContext.CurrentRow );
    }

    internal static int GetLastVisibleFocusableColumnIndex( DataGridContext dataGridContext, Row targetRow )
    {
      if( dataGridContext == null )
        return -1;

      if( dataGridContext.IsAFlattenDetail )
        return NavigationHelper.GetPreviousVisibleFocusableDetailColumnIndexFromMasterColumnIndex( dataGridContext, targetRow, int.MaxValue );

      return NavigationHelper.GetPreviousVisibleFocusableColumnIndex( dataGridContext, targetRow, int.MaxValue );
    }

    internal static int GetNextVisibleFocusableColumnIndex( DataGridContext dataGridContext, Row targetRow, ColumnBase targetColumn )
    {
      if( ( dataGridContext == null ) || ( targetRow == null ) || ( targetColumn == null ) )
        return -1;

      var columns = dataGridContext.VisibleColumns;
      if( ( columns == null ) || ( columns.Count <= 0 ) )
        return -1;

      var columnIndex = columns.IndexOf( targetColumn );
      if( columnIndex < 0 )
        return -1;

      if( !dataGridContext.IsAFlattenDetail )
        return NavigationHelper.GetNextVisibleFocusableColumnIndex( dataGridContext, targetRow, columnIndex + 1 );

      var columnMap = dataGridContext.ItemPropertyMap;
      var masterColumnName = default( string );

      if( !columnMap.TryGetColumnFieldName( targetColumn, out masterColumnName ) )
        return -1;

      var masterDataGridContext = dataGridContext.RootDataGridContext;
      var masterColumn = masterDataGridContext.Columns[ masterColumnName ];

      if( masterColumn == null )
        return -1;

      var masterColumnIndex = masterDataGridContext.VisibleColumns.IndexOf( masterColumn );
      if( masterColumnIndex < 0 )
        return -1;

      return NavigationHelper.GetNextVisibleFocusableDetailColumnIndexFromMasterColumnIndex( dataGridContext, targetRow, masterColumnIndex + 1 );
    }

    internal static int GetPreviousVisibleFocusableColumnIndex( DataGridContext dataGridContext, Row targetRow, ColumnBase targetColumn )
    {
      if( ( dataGridContext == null ) || ( targetRow == null ) || ( targetColumn == null ) )
        return -1;

      var columns = dataGridContext.VisibleColumns;
      if( ( columns == null ) || ( columns.Count <= 0 ) )
        return -1;

      var columnIndex = columns.IndexOf( targetColumn );
      if( columnIndex < 0 )
        return -1;

      if( !dataGridContext.IsAFlattenDetail )
        return NavigationHelper.GetPreviousVisibleFocusableColumnIndex( dataGridContext, targetRow, columnIndex - 1 );

      var columnMap = dataGridContext.ItemPropertyMap;
      var masterColumnName = default( string );

      if( !columnMap.TryGetColumnFieldName( targetColumn, out masterColumnName ) )
        return -1;

      var masterDataGridContext = dataGridContext.RootDataGridContext;
      var masterColumn = masterDataGridContext.Columns[ masterColumnName ];

      if( masterColumn == null )
        return -1;

      var masterColumnIndex = masterDataGridContext.VisibleColumns.IndexOf( masterColumn );
      if( masterColumnIndex < 0 )
        return -1;

      return NavigationHelper.GetPreviousVisibleFocusableDetailColumnIndexFromMasterColumnIndex( dataGridContext, targetRow, masterColumnIndex - 1 );
    }

    internal static bool MoveFocusLeft( DataGridContext dataGridContext )
    {
      var dataGridControl = NavigationHelper.GetDataGridControl( dataGridContext );
      if( dataGridControl == null )
        return false;

      var leftToRight = ( dataGridControl.FlowDirection == FlowDirection.LeftToRight );

      return NavigationHelper.MoveFocus( dataGridControl, dataGridContext, leftToRight );
    }

    internal static bool MoveFocusRight( DataGridContext dataGridContext )
    {
      var dataGridControl = NavigationHelper.GetDataGridControl( dataGridContext );
      if( dataGridControl == null )
        return false;

      var leftToRight = ( dataGridControl.FlowDirection == FlowDirection.LeftToRight );

      return NavigationHelper.MoveFocus( dataGridControl, dataGridContext, !leftToRight );
    }

    internal static bool MoveFocusToNextVisibleColumn( DataGridContext dataGridContext )
    {
      var dataGridControl = NavigationHelper.GetDataGridControl( dataGridContext );
      if( dataGridControl == null )
        return false;

      Debug.Assert( dataGridControl.CurrentContext == dataGridContext );

      return NavigationHelper.MoveFocusToNextVisibleColumn( dataGridContext, dataGridContext.CurrentRow, dataGridContext.CurrentColumn );
    }

    internal static bool MoveFocusToPreviousVisibleColumn( DataGridContext dataGridContext )
    {
      var dataGridControl = NavigationHelper.GetDataGridControl( dataGridContext );
      if( dataGridControl == null )
        return false;

      Debug.Assert( dataGridControl.CurrentContext == dataGridContext );

      return NavigationHelper.MoveFocusToPreviousVisibleColumn( dataGridContext, dataGridContext.CurrentRow, dataGridContext.CurrentColumn );
    }

    internal static bool MoveFocusToFirstVisibleColumn( DataGridContext dataGridContext )
    {
      var dataGridControl = NavigationHelper.GetDataGridControl( dataGridContext );
      if( dataGridControl == null )
        return false;

      Debug.Assert( dataGridControl.CurrentContext == dataGridContext );

      return NavigationHelper.MoveFocusToFirstVisibleColumn( dataGridContext, dataGridContext.CurrentRow );
    }

    internal static bool MoveFocusToLastVisibleColumn( DataGridContext dataGridContext )
    {
      var dataGridControl = NavigationHelper.GetDataGridControl( dataGridContext );
      if( dataGridControl == null )
        return false;

      Debug.Assert( dataGridControl.CurrentContext == dataGridContext );

      return NavigationHelper.MoveFocusToLastVisibleColumn( dataGridContext, dataGridContext.CurrentRow );
    }

    internal static bool HandleTabKey(
      DataGridControl dataGridControl,
      DataGridContext dataGridContext,
      FrameworkElement source,
      KeyboardDevice device )
    {
      if( ( dataGridControl == null ) || !dataGridControl.IsKeyboardFocusWithin )
        return false;

      Debug.Assert( dataGridControl.CurrentContext == dataGridContext );

      if( !dataGridControl.IsBeingEdited )
      {
        Debug.Assert( dataGridContext != null );
        return false;
      }

      if( NavigationHelper.ValidateTabKeyHandleIsWithin( dataGridControl, source, device ) )
        return false;

      var columns = dataGridContext.VisibleColumns;
      if( columns.Count <= 0 )
        return false;

      bool handled;

      if( dataGridContext.CurrentColumn == null )
      {
        handled = NavigationHelper.MoveFocusToFirstVisibleColumn( dataGridContext );
      }
      else
      {
        if( ( device.Modifiers & ModifierKeys.Shift ) == ModifierKeys.Shift )
        {
          handled = NavigationHelper.MoveFocusToPreviousVisibleColumn( dataGridContext );
        }
        else
        {
          handled = NavigationHelper.MoveFocusToNextVisibleColumn( dataGridContext );
        }
      }

      if( !handled )
        throw new DataGridException( "Trying to edit while no cell is focusable.", dataGridControl );

      return true;
    }

    private static int GetNextVisibleFocusableColumnIndex( DataGridContext dataGridContext, Row targetRow, int startIndex )
    {
      if( ( dataGridContext == null ) || ( targetRow == null ) )
        return -1;

      var columns = dataGridContext.VisibleColumns;
      if( ( columns == null ) || ( columns.Count <= 0 ) )
        return -1;

      var cells = targetRow.Cells;

      for( int i = Math.Max( 0, startIndex ); i < columns.Count; i++ )
      {
        var targetColumn = columns[ i ];

        if( cells[ targetColumn ].GetCalculatedCanBeCurrent() )
          return i;
      }

      return -1;
    }

    private static int GetPreviousVisibleFocusableColumnIndex( DataGridContext dataGridContext, Row targetRow, int startIndex )
    {
      if( ( dataGridContext == null ) || ( targetRow == null ) )
        return -1;

      var columns = dataGridContext.VisibleColumns;
      if( ( columns == null ) || ( columns.Count <= 0 ) )
        return -1;

      var cells = targetRow.Cells;

      for( int i = Math.Min( columns.Count - 1, startIndex ); i >= 0; i-- )
      {
        var targetColumn = columns[ i ];

        if( cells[ targetColumn ].GetCalculatedCanBeCurrent() )
          return i;
      }

      return -1;
    }

    private static int GetNextVisibleFocusableDetailColumnIndexFromMasterColumnIndex( DataGridContext dataGridContext, Row targetRow, int startIndex )
    {
      if( ( dataGridContext == null ) || ( targetRow == null ) )
        return -1;

      Debug.Assert( dataGridContext.IsAFlattenDetail );

      var detailVisibleColumns = dataGridContext.VisibleColumns;
      if( ( detailVisibleColumns == null ) || ( detailVisibleColumns.Count <= 0 ) )
        return -1;

      var masterDataGridContext = dataGridContext.RootDataGridContext;
      var masterVisibleColumns = masterDataGridContext.VisibleColumns;

      var detailColumns = dataGridContext.Columns;
      var detailPropertyMap = dataGridContext.ItemPropertyMap;
      var cells = targetRow.Cells;

      for( int i = Math.Max( 0, startIndex ); i < masterVisibleColumns.Count; i++ )
      {
        var masterColumn = masterVisibleColumns[ i ];
        Debug.Assert( masterColumn != null );

        var detailColumnName = default( string );
        if( detailPropertyMap.TryGetItemPropertyName( masterColumn, out detailColumnName ) )
        {
          var detailColumn = detailColumns[ detailColumnName ];

          if( detailColumn != null )
          {
            var detailColumnIndex = detailVisibleColumns.IndexOf( detailColumn );

            if( ( detailColumnIndex >= 0 ) && ( cells[ detailColumn ].GetCalculatedCanBeCurrent() ) )
              return detailColumnIndex;
          }
        }
      }

      return -1;
    }

    private static int GetPreviousVisibleFocusableDetailColumnIndexFromMasterColumnIndex( DataGridContext dataGridContext, Row targetRow, int startIndex )
    {
      if( ( dataGridContext == null ) || ( targetRow == null ) )
        return -1;

      Debug.Assert( dataGridContext.IsAFlattenDetail );

      var detailVisibleColumns = dataGridContext.VisibleColumns;
      if( ( detailVisibleColumns == null ) || ( detailVisibleColumns.Count <= 0 ) )
        return -1;

      var masterDataGridContext = dataGridContext.RootDataGridContext;
      var masterVisibleColumns = masterDataGridContext.VisibleColumns;

      var detailColumns = dataGridContext.Columns;
      var detailPropertyMap = dataGridContext.ItemPropertyMap;
      var cells = targetRow.Cells;

      for( int i = Math.Min( masterVisibleColumns.Count - 1, startIndex ); i >= 0; i-- )
      {
        var masterColumn = masterVisibleColumns[ i ];
        Debug.Assert( masterColumn != null );

        var detailColumnName = default( string );
        if( detailPropertyMap.TryGetItemPropertyName( masterColumn, out detailColumnName ) )
        {
          var detailColumn = detailColumns[ detailColumnName ];

          if( detailColumn != null )
          {
            var detailColumnIndex = detailVisibleColumns.IndexOf( detailColumn );

            if( ( detailColumnIndex >= 0 ) && ( cells[ detailColumn ].GetCalculatedCanBeCurrent() ) )
              return detailColumnIndex;
          }
        }
      }

      return -1;
    }

    private static bool ValidateTabKeyHandleIsWithin(
      DataGridControl dataGridControl,
      FrameworkElement source,
      KeyboardDevice device )
    {
      if( ( dataGridControl == null ) || ( source == null ) )
        return false;

      var nextControl = NavigationHelper.PredictNextElement( source, device );
      if( nextControl == null )
        return false;

      //If the original source is not a control (e.g. the cells panel instead of a cell), columns will be used to move focus.
      var cell = Cell.FindFromChild( nextControl );
      if( ( cell == null ) || ( cell.ParentColumn != dataGridControl.CurrentColumn ) )
        return false;

      return object.Equals( cell.ParentRow.DataContext, dataGridControl.CurrentItemInEdition );
    }

    private static bool MoveFocus( DataGridControl dataGridControl, DataGridContext dataGridContext, bool moveLeft )
    {
      Debug.Assert( dataGridControl != null );
      Debug.Assert( dataGridControl.CurrentContext == dataGridContext );

      bool isFocusWithin = dataGridControl.IsKeyboardFocusWithin;
      bool isBeingEdited = ( isFocusWithin && dataGridControl.IsBeingEdited );

      var navigationBehavior = dataGridControl.NavigationBehavior;

      // Process key even if NavigationBehavior is RowOnly when the grid is being edited.
      if( ( navigationBehavior == NavigationBehavior.CellOnly )
        || ( ( navigationBehavior == NavigationBehavior.RowOnly ) && isBeingEdited )
        || ( ( navigationBehavior == NavigationBehavior.RowOrCell ) && isFocusWithin && ( dataGridContext.CurrentColumn != null ) ) )
      {
        if( moveLeft )
          return NavigationHelper.MoveFocusToPreviousVisibleColumn( dataGridContext, dataGridContext.CurrentRow, dataGridContext.CurrentColumn );

        return NavigationHelper.MoveFocusToNextVisibleColumn( dataGridContext, dataGridContext.CurrentRow, dataGridContext.CurrentColumn );
      }

      return false;
    }

    private static bool MoveFocusToNextVisibleColumn( DataGridContext dataGridContext, Row targetRow, ColumnBase targetColumn )
    {
      if( ( dataGridContext == null ) || ( targetRow == null ) )
        return false;

      var columnIndex = NavigationHelper.GetNextVisibleFocusableColumnIndex( dataGridContext, targetRow, targetColumn );
      if( columnIndex < 0 )
        return NavigationHelper.MoveFocusToFirstVisibleColumn( dataGridContext, targetRow );

      var columns = dataGridContext.VisibleColumns;
      Debug.Assert( columns != null );

      NavigationHelper.SetCurrentColumnAndChangeSelection( dataGridContext, columns[ columnIndex ] );

      return true;
    }

    private static bool MoveFocusToPreviousVisibleColumn( DataGridContext dataGridContext, Row targetRow, ColumnBase targetColumn )
    {
      if( ( dataGridContext == null ) || ( targetRow == null ) )
        return false;

      var columnIndex = NavigationHelper.GetPreviousVisibleFocusableColumnIndex( dataGridContext, targetRow, targetColumn );
      if( columnIndex < 0 )
        return NavigationHelper.MoveFocusToLastVisibleColumn( dataGridContext, targetRow );

      var columns = dataGridContext.VisibleColumns;
      Debug.Assert( columns != null );

      NavigationHelper.SetCurrentColumnAndChangeSelection( dataGridContext, columns[ columnIndex ] );

      return true;
    }

    private static bool MoveFocusToFirstVisibleColumn( DataGridContext dataGridContext, Row targetRow )
    {
      if( ( dataGridContext == null ) || ( targetRow == null ) )
        return false;

      var columnIndex = NavigationHelper.GetFirstVisibleFocusableColumnIndex( dataGridContext, targetRow );
      if( columnIndex < 0 )
        return false;

      var columns = dataGridContext.VisibleColumns;
      Debug.Assert( columns != null );

      NavigationHelper.SetCurrentColumnAndChangeSelection( dataGridContext, columns[ columnIndex ] );

      return true;
    }

    private static bool MoveFocusToLastVisibleColumn( DataGridContext dataGridContext, Row targetRow )
    {
      if( ( dataGridContext == null ) || ( targetRow == null ) )
        return false;

      var columnIndex = NavigationHelper.GetLastVisibleFocusableColumnIndex( dataGridContext, targetRow );
      if( columnIndex < 0 )
        return false;

      var columns = dataGridContext.VisibleColumns;
      Debug.Assert( columns != null );

      NavigationHelper.SetCurrentColumnAndChangeSelection( dataGridContext, columns[ columnIndex ] );

      return true;
    }

    private static DataGridControl GetDataGridControl( DataGridContext dataGridContext )
    {
      if( dataGridContext != null )
        return dataGridContext.DataGridControl;

      return null;
    }

    private static DependencyObject PredictNextElement( FrameworkElement source, KeyboardDevice device )
    {
      foreach( var direction in NavigationHelper.GetFocusNavigationDirections( source, device ) )
      {
        var target = source.PredictFocus( direction );
        if( target != null )
          return target;
      }

      return null;
    }

    private static IEnumerable<FocusNavigationDirection> GetFocusNavigationDirections( FrameworkElement source, KeyboardDevice device )
    {
      if( source == null )
        yield break;

      //In the case of a ListBox set with Cycle or Contained navigation mode, we must move in the other direction if on the first or last item,
      //since PredictFocus will throw is we use FocusNavigationDirection.First/Last.
      if( ( device.Modifiers & ModifierKeys.Shift ) == ModifierKeys.Shift )
      {
        yield return FocusNavigationDirection.Left;
        yield return FocusNavigationDirection.Up;

        var navigationMode = ( KeyboardNavigationMode )source.GetValue( KeyboardNavigation.TabNavigationProperty );
        if( navigationMode == KeyboardNavigationMode.Cycle || navigationMode == KeyboardNavigationMode.Contained )
        {
          yield return FocusNavigationDirection.Right;
          yield return FocusNavigationDirection.Down;
        }
      }
      else
      {
        yield return FocusNavigationDirection.Right;
        yield return FocusNavigationDirection.Down;

        var navigationMode = ( KeyboardNavigationMode )source.GetValue( KeyboardNavigation.TabNavigationProperty );
        if( navigationMode == KeyboardNavigationMode.Cycle || navigationMode == KeyboardNavigationMode.Contained )
        {
          yield return FocusNavigationDirection.Left;
          yield return FocusNavigationDirection.Up;
        }
      }
    }

    private static void SetCurrentColumnAndChangeSelection( DataGridContext dataGridContext, ColumnBase column )
    {
      Debug.Assert( dataGridContext != null );
      Debug.Assert( column != null );

      try
      {
        dataGridContext.SetCurrentColumnAndChangeSelection( column );
      }
      catch( DataGridException )
      {
        // We swallow the exception if it occurs because of a validation error or Cell was read-only or
        // any other GridException.
      }
    }
  }
}
