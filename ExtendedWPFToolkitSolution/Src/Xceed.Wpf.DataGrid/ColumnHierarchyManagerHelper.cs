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
using System.Diagnostics;
using System.Linq;

namespace Xceed.Wpf.DataGrid
{
  internal static class ColumnHierarchyManagerHelper
  {
    internal static IDisposable DeferColumnsUpdate( DataGridContext dataGridContext )
    {
      Debug.Assert( dataGridContext != null );

      return dataGridContext.ColumnManager.DeferUpdate();
    }

    internal static IDisposable DeferColumnsUpdate( DetailConfiguration detailConfiguration )
    {
      Debug.Assert( detailConfiguration != null );

      return detailConfiguration.ColumnManager.DeferUpdate();
    }

    internal static bool MoveColumnBefore( DataGridContext dataGridContext, ColumnBase current, ColumnBase next )
    {
      Debug.Assert( dataGridContext != null );

      return ColumnHierarchyManagerHelper.MoveColumnBefore( dataGridContext.ColumnManager, current, next );
    }

    internal static bool MoveColumnBefore( DetailConfiguration detailConfiguration, ColumnBase current, ColumnBase next )
    {
      Debug.Assert( detailConfiguration != null );

      return ColumnHierarchyManagerHelper.MoveColumnBefore( detailConfiguration.ColumnManager, current, next );
    }

    internal static bool MoveColumnAfter( DataGridContext dataGridContext, ColumnBase current, ColumnBase previous )
    {
      Debug.Assert( dataGridContext != null );

      return ColumnHierarchyManagerHelper.MoveColumnAfter( dataGridContext.ColumnManager, current, previous );
    }

    internal static bool MoveColumnAfter( DetailConfiguration detailConfiguration, ColumnBase current, ColumnBase previous )
    {
      Debug.Assert( detailConfiguration != null );

      return ColumnHierarchyManagerHelper.MoveColumnAfter( detailConfiguration.ColumnManager, current, previous );
    }

    internal static bool MoveColumnUnder( DataGridContext dataGridContext, ColumnBase current, ColumnBase parent )
    {
      Debug.Assert( dataGridContext != null );

      return ColumnHierarchyManagerHelper.MoveColumnUnder( dataGridContext.ColumnManager, dataGridContext.Columns, current, parent );
    }

    internal static bool MoveColumnUnder( DetailConfiguration detailConfiguration, ColumnBase current, ColumnBase parent )
    {
      Debug.Assert( detailConfiguration != null );

      return ColumnHierarchyManagerHelper.MoveColumnUnder( detailConfiguration.ColumnManager, detailConfiguration.Columns, current, parent );
    }

    private static bool MoveColumnBefore( ColumnHierarchyManager columnsLayout, ColumnBase current, ColumnBase next )
    {
      Debug.Assert( columnsLayout != null );

      var currentLocation = columnsLayout.GetColumnLocationFor( current );
      if( currentLocation == null )
        return false;

      var pivotLocation = columnsLayout.GetColumnLocationFor( next );
      if( pivotLocation == null )
        return false;

      if( !currentLocation.CanMoveBefore( pivotLocation ) )
        return false;

      currentLocation.MoveBefore( pivotLocation );
      return true;
    }

    private static bool MoveColumnAfter( ColumnHierarchyManager columnsLayout, ColumnBase current, ColumnBase previous )
    {
      Debug.Assert( columnsLayout != null );

      var currentLocation = columnsLayout.GetColumnLocationFor( current );
      if( currentLocation == null )
        return false;

      var pivotLocation = columnsLayout.GetColumnLocationFor( previous );
      if( pivotLocation == null )
        return false;

      if( !currentLocation.CanMoveAfter( pivotLocation ) )
        return false;

      currentLocation.MoveAfter( pivotLocation );
      return true;
    }

    private static bool MoveColumnUnder( ColumnHierarchyManager columnsLayout, ColumnCollection columns, ColumnBase current, ColumnBase parent )
    {
      Debug.Assert( columnsLayout != null );

      var currentLocation = columnsLayout.GetColumnLocationFor( current );
      if( currentLocation == null )
        return false;

      var pivotLocation = default( ColumnHierarchyManager.ILocation );

      // Move the column under the orphan section.
      if( parent == null )
      {
        var columnCollection = current.ContainingCollection;
        if( columnCollection == null )
          return false;

        var mergedColumnCollection = default( ColumnCollection );

        if( columnCollection == columns )
        {
          mergedColumnCollection =  null;
        }

        if( mergedColumnCollection == null )
          return false;

        var levelMarkers = columnsLayout.GetLevelMarkersFor( mergedColumnCollection );
        if( levelMarkers == null )
          return false;

        pivotLocation = levelMarkers.Orphan;
      }
      else
      {
        pivotLocation = columnsLayout.GetColumnLocationFor( parent );
      }

      if( pivotLocation == null )
        return false;

      if( !currentLocation.CanMoveUnder( pivotLocation ) )
        return false;

      currentLocation.MoveUnder( pivotLocation );
      return true;
    }
  }
}
