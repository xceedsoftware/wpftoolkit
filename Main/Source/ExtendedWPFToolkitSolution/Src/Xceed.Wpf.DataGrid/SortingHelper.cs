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
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Diagnostics;
using System.Windows.Data;
using System.Collections;

namespace Xceed.Wpf.DataGrid
{
  internal static class SortingHelper
  {
    private static void ApplyColumnSort( DataGridContext dataGridContext, SortDescriptionCollection sortDescriptions, ColumnCollection columns, ColumnBase column, SortDirection sortDirection )
    {
      if( column == null )
        throw new ArgumentNullException( "column" );

      Debug.Assert( ( dataGridContext != null ) || ( column.ParentDetailConfiguration != null ) );

      if( ( dataGridContext == null ) && ( column.ParentDetailConfiguration == null ) )
        throw new DataGridInternalException( "DataGridContext or ParentDetailConfiguration cannot be null." );

      if( !columns.Contains( column ) )
        throw new ArgumentException( "The specified column is not part of the DataGridContext.", "column" );

      string fieldName = column.FieldName;

      bool canSort = ( dataGridContext != null ) ? dataGridContext.Items.CanSort : true;

      if( ( !string.IsNullOrEmpty( fieldName ) ) && ( canSort == true ) )
      {
        SortDescription existingSort;
        int sortDescriptionIndex = sortDescriptions.Count;

        for( int i = sortDescriptions.Count - 1; i >= 0; i-- )
        {
          existingSort = sortDescriptions[ i ];

          if( existingSort.PropertyName == fieldName )
          {
            if( ( ( existingSort.Direction == ListSortDirection.Ascending ) && ( sortDirection == SortDirection.Ascending ) ) ||
                ( ( existingSort.Direction == ListSortDirection.Descending ) && ( sortDirection == SortDirection.Descending ) ) )
            {
              // Nothing to change!
              sortDescriptionIndex = -1;
            }
            else
            {
              sortDescriptionIndex = i;
              sortDescriptions.Remove( existingSort );
            }

            break;
          }
        }

        int maxDescriptions = ( dataGridContext != null ) ? dataGridContext.MaxSortLevels 
          : column.ParentDetailConfiguration.MaxSortLevels;

        if( ( maxDescriptions != -1 ) && ( sortDescriptions.Count >= maxDescriptions ) )
        {
          // Cannot insert sort description since it would go beyond the max sort description count.
          sortDescriptionIndex = -1;
          sortDirection = SortDirection.None;
        }

        if( ( sortDescriptionIndex > -1 ) && ( sortDirection != SortDirection.None ) )
        {
          SortDescription sortDescription = new SortDescription( fieldName,
            ( sortDirection == SortDirection.Ascending ) ? ListSortDirection.Ascending : ListSortDirection.Descending );

          sortDescriptions.Insert( sortDescriptionIndex, sortDescription );
          column.SetSortIndex( sortDescriptionIndex );
          column.SetSortDirection( sortDirection );
        }

        SortingHelper.SynchronizeSortIndexes( sortDescriptions, columns );
      }
    }

    private static IDisposable DeferResortHelper( 
      IEnumerable itemsSourceCollection, 
      CollectionView collectionView, 
      SortDescriptionCollection sortDescriptions )
    {
      IDisposable resortDisposable = null;
      DataGridSortDescriptionCollection dataGridSortDescriptions = sortDescriptions as DataGridSortDescriptionCollection;

      if( dataGridSortDescriptions != null )
      {
        // We are in a detail
        resortDisposable = dataGridSortDescriptions.DeferResort();
      }
      else
      {
        Debug.Assert( collectionView != null, "We must have a CollectionView when we are not processing a Detail" );

        DataGridCollectionViewBase dataGridCollectionView = itemsSourceCollection as DataGridCollectionViewBase;

        if( dataGridCollectionView != null )
        {
          resortDisposable = dataGridCollectionView.DataGridSortDescriptions.DeferResort();
        }
        else
        {
          resortDisposable = collectionView.DeferRefresh();
        }
      }
      Debug.Assert( resortDisposable != null );

      return resortDisposable;
    }

    private static void SynchronizeSortIndexes( SortDescriptionCollection sortDescriptions, ColumnCollection columns )
    {
      SortDescription sortDescription;
      Collection<ColumnBase> handledColumns = new Collection<ColumnBase>();

      for( int i = sortDescriptions.Count - 1; i >= 0; i-- )
      {
        sortDescription = sortDescriptions[ i ];

        foreach( ColumnBase column in columns )
        {
          string fieldName = column.FieldName;

          if( fieldName == sortDescription.PropertyName )
          {
            column.SetSortIndex( i );
            handledColumns.Add( column );
            break;
          }
        }
      }

      foreach( ColumnBase column in columns )
      {
        if( !handledColumns.Contains( column ) )
        {
          column.SetSortIndex( -1 );
          column.SetSortDirection( SortDirection.None );
        }
      }
    }

    internal static void ToggleColumnSort(
      DataGridContext dataGridContext,
      SortDescriptionCollection sortDescriptions,
      ColumnCollection columns,
      ColumnBase column,
      bool shiftUnpressed )
    {
      if( column == null )
        throw new ArgumentNullException( "column" );

      Debug.Assert( ( dataGridContext != null ) || ( column.ParentDetailConfiguration != null ) );

      if( ( dataGridContext == null ) && ( column.ParentDetailConfiguration == null ) )
        throw new DataGridInternalException( "DataGridContext or ParentDetailConfiguration can't be null." );

      DataGridContext parentDataGridContext = ( dataGridContext == null ) ? null : dataGridContext.ParentDataGridContext;

      // Defer the RestoreState of each DataGridContext of the same level
      // to ensure all the DataGridContext will be correctly restored once
      // all of them are completely resorted
      HashSet<IDisposable> deferRestoreStateDisposable = new HashSet<IDisposable>();

      if( parentDataGridContext != null )
      {
        foreach( DataGridContext childContext in parentDataGridContext.GetChildContexts() )
        {
          deferRestoreStateDisposable.Add( childContext.DeferRestoreState() );
        }
      }

      IDisposable deferResortHelper = ( dataGridContext == null ) ? null :
        SortingHelper.DeferResortHelper( dataGridContext.ItemsSourceCollection, dataGridContext.Items, sortDescriptions );

      //this will ensure that all DataGridCollectionViews mapped to this SortDescriptions collection will only refresh their sorting once!
      SortDirection newSortDirection = column.SortDirection;
      if( ( shiftUnpressed ) &&
          ( ( column.SortIndex == -1 ) || ( sortDescriptions.Count > 1 ) ) )
        sortDescriptions.Clear();

      switch( newSortDirection )
      {
        case SortDirection.None:
          newSortDirection = SortDirection.Ascending;
          break;
        case SortDirection.Ascending:
          newSortDirection = SortDirection.Descending;
          break;

        case SortDirection.Descending:
          newSortDirection = SortDirection.None;
          break;
      }

      SortingHelper.ApplyColumnSort( dataGridContext, sortDescriptions, columns, column, newSortDirection );

      if( deferResortHelper != null )
      {
        //end of the DeferResort(), any DataGridCollectionView listening to the SortDescriptions instance will refresh its sorting!
        deferResortHelper.Dispose();
      }

      foreach( IDisposable disposable in deferRestoreStateDisposable )
      {
        try
        {
          // Try/Catch to ensure all contexts are restored
          disposable.Dispose();
        }
        catch( Exception )
        {
        }
      }

      deferRestoreStateDisposable.Clear();
    }
  }
}
