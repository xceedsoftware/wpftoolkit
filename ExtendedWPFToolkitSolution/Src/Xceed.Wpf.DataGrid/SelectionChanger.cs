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
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Windows.Data;

namespace Xceed.Wpf.DataGrid
{
  internal class SelectionChanger
  {
    #region Static Fields

    private static readonly SelectedItemsStorage EmptyItemsStore = new SelectedItemsStorage( null );
    private static readonly SelectedCellsStorage EmptyCellsStore = new SelectedCellsStorage( null );

    #endregion

    internal SelectionChanger( DataGridContext owner )
    {
      m_owner = owner;
      m_itemsToSelect = new SelectedItemsStorage( owner );
      m_itemsToUnselect = new SelectedItemsStorage( owner );
      m_cellsToSelect = new SelectedCellsStorage( owner );
      m_cellsToUnselect = new SelectedCellsStorage( owner );
      m_toDeferSelect = new List<object>( 1 );
      m_sourceChanges = new List<SourceChangeInfo>( 2 );
    }

    #region Owner Property

    internal DataGridContext Owner
    {
      get
      {
        return m_owner;
      }
    }

    private readonly DataGridContext m_owner;

    #endregion

    public bool SelectItems( SelectionRangeWithItems rangeWithItems )
    {
      SelectionRange range = rangeWithItems.Range;

      if( !( m_owner.ItemsSourceCollection is DataGridVirtualizingCollectionViewBase ) )
      {
        if( range.IsEmpty )
        {
          foreach( object item in rangeWithItems.Items )
          {
            if( !m_toDeferSelect.Contains( item ) )
              m_toDeferSelect.Add( item );
          }

          return false;
        }
      }
      else
      {
        if( range.IsEmpty )
          throw new ArgumentException( "rangeWithItems.Range can't be empty when we are using a DataGridVirtualizingCollectionView", "rangeWithItems" );
      }

      if( rangeWithItems.Length == 1 )
      {
        if( !m_itemsToUnselect.Remove( rangeWithItems ) )
        {
          if( m_owner.SelectedItemsStore.Contains( rangeWithItems ) )
            return false;

          if( m_itemsToSelect.Contains( range ) )
            return false;

          this.m_itemsToSelect.Add( rangeWithItems );
        }

        return true;
      }
      else
      {
        bool selectionChanged = m_itemsToUnselect.Remove( rangeWithItems );

        SelectedItemsStorage tempStorage = new SelectedItemsStorage( m_owner );
        tempStorage.Add( rangeWithItems );

        // Remove the currently selected item from the new range to select
        foreach( SelectionRangeWithItems existingSelectionRangeWithItems in m_owner.SelectedItemsStore )
        {
          tempStorage.Remove( existingSelectionRangeWithItems );
        }

        // Remove the pending item to be selected from the new range to select
        foreach( SelectionRangeWithItems existingSelectionRangeWithItems in m_itemsToSelect )
        {
          tempStorage.Remove( existingSelectionRangeWithItems );
        }

        if( tempStorage.Count > 0 )
        {
          selectionChanged = true;

          foreach( SelectionRangeWithItems rangeWithItemsToAdd in tempStorage )
          {
            m_itemsToSelect.Add( rangeWithItemsToAdd );
          }
        }

        return selectionChanged;
      }
    }

    public bool SelectCells( SelectionCellRangeWithItems cellRangeWithItems )
    {
      SelectionRange itemRange = cellRangeWithItems.ItemRange;

      if( itemRange.IsEmpty )
        throw new ArgumentException( "cellRangeWithItems.ItemRange can't be empty", "cellRangeWithItems" );

      if( cellRangeWithItems.Length == 1 )
      {
        if( !m_cellsToUnselect.Remove( cellRangeWithItems ) )
        {
          if( m_owner.SelectedCellsStore.Contains( cellRangeWithItems ) )
            return false;

          if( m_cellsToSelect.Contains( cellRangeWithItems.CellRange ) )
            return false;

          this.m_cellsToSelect.Add( cellRangeWithItems );
        }

        return true;
      }
      else
      {
        bool selectionChanged = m_cellsToUnselect.Remove( cellRangeWithItems );

        SelectedCellsStorage tempStorage = new SelectedCellsStorage( m_owner );
        tempStorage.Add( cellRangeWithItems );

        // Remove the currently selected item from the new range to select
        foreach( SelectionCellRangeWithItems existingSelectionCellRangeWithItems in m_owner.SelectedCellsStore )
        {
          tempStorage.Remove( existingSelectionCellRangeWithItems );
        }

        // Remove the pending item to be selected from the new range to select
        foreach( SelectionCellRangeWithItems existingSelectionCellRangeWithItems in m_cellsToSelect )
        {
          tempStorage.Remove( existingSelectionCellRangeWithItems );
        }

        if( tempStorage.Count > 0 )
        {
          selectionChanged = true;

          foreach( SelectionCellRangeWithItems cellRangeWithItemsToAdd in tempStorage )
          {
            m_cellsToSelect.Add( cellRangeWithItemsToAdd );
          }
        }

        return selectionChanged;
      }
    }

    public bool UnselectItems( SelectionRangeWithItems rangeWithItems )
    {
      if( !( m_owner.ItemsSourceCollection is DataGridVirtualizingCollectionViewBase ) )
      {
        if( m_toDeferSelect.Count > 0 )
        {
          foreach( object item in rangeWithItems.Items )
          {
            m_toDeferSelect.Remove( item );
          }
        }
      }

      SelectionRange range = rangeWithItems.Range;

      if( range.IsEmpty )
      {
        // We have no index we have to remove based on item
        bool selectionChanged = false;

        List<SelectionRangeWithItems> itemsRangeToRemove = new List<SelectionRangeWithItems>();
        List<object> itemsToUnselect = new List<object>( rangeWithItems.Items );
        int count = itemsToUnselect.Count;

        for( int i = count - 1; i >= 0; i-- )
        {
          object itemToUnselect = itemsToUnselect[ i ];
          bool selectionAdded = false;

          foreach( SelectionRangeWithItems existingSelectionRangeWithItems in m_itemsToSelect )
          {
            int index = Array.IndexOf( existingSelectionRangeWithItems.Items, itemToUnselect );

            if( index > -1 )
            {
              selectionAdded = true;

              itemsRangeToRemove.Add(
                new SelectionRangeWithItems(
                  existingSelectionRangeWithItems.Range.GetIndexFromItemOffset( index ),
                  itemToUnselect ) );
            }
          }

          if( selectionAdded )
          {
            itemsToUnselect.RemoveAt( i );
          }
        }

        // Remove the currently unselected item from the new range to select
        foreach( SelectionRangeWithItems itemRangeToRemove in itemsRangeToRemove )
        {
          selectionChanged |= m_itemsToSelect.Remove( itemRangeToRemove );
        }

        count = itemsToUnselect.Count;

        for( int i = 0; i < count; i++ )
        {
          object itemToUnselect = itemsToUnselect[ i ];

          foreach( SelectionRangeWithItems existingSelectionRangeWithItems in m_owner.SelectedItemsStore )
          {
            int index = Array.IndexOf( existingSelectionRangeWithItems.Items, itemToUnselect );

            if( index >= 0 )
            {
              index = existingSelectionRangeWithItems.Range.GetIndexFromItemOffset( index );

              if( !m_itemsToUnselect.Contains( index ) )
              {
                selectionChanged = true;
                m_itemsToUnselect.Add( new SelectionRangeWithItems( index, itemToUnselect ) );
              }
            }
          }
        }

        return selectionChanged;
      }

      if( range.Length == 1 )
      {
        if( !m_itemsToSelect.Remove( rangeWithItems ) )
        {
          if( !m_owner.SelectedItemsStore.Contains( range ) )
            return false;

          if( m_itemsToUnselect.Contains( range ) )
            return false;

          m_itemsToUnselect.Add( rangeWithItems );
        }

        return true;
      }
      else
      {
        SelectedItemsStorage tempStorage = new SelectedItemsStorage( m_owner );
        tempStorage.Add( rangeWithItems );

        // Remove the currently selected item from the new range to select
        foreach( SelectionRangeWithItems existingSelectionRangeWithItems in m_itemsToSelect )
        {
          if( !range.Intersect( existingSelectionRangeWithItems.Range ).IsEmpty )
          {
            tempStorage.Remove( existingSelectionRangeWithItems );
          }
        }

        bool selectionChanged = m_itemsToSelect.Remove( rangeWithItems );

        if( tempStorage.Count > 0 )
        {
          selectionChanged = true;

          foreach( SelectionRangeWithItems rangeWithItemsToAdd in tempStorage )
          {
            Debug.Assert( !m_itemsToUnselect.Contains( rangeWithItemsToAdd.Range ) );
            m_itemsToUnselect.Add( rangeWithItemsToAdd );
          }
        }

        return selectionChanged;
      }
    }

    public bool UnselectCells( SelectionCellRangeWithItems cellRangeWithItems )
    {
      SelectionRange itemRange = cellRangeWithItems.ItemRange;
      SelectionRange columnRange = cellRangeWithItems.ColumnRange;
      SelectionCellRange cellRange = cellRangeWithItems.CellRange;

      if( itemRange.IsEmpty )
      {
        // We have no index we have to remove based on item
        bool selectionChanged = false;

        List<SelectionCellRangeWithItems> cellsRangeToRemove = new List<SelectionCellRangeWithItems>();
        List<object> itemsToUnselect = new List<object>( cellRangeWithItems.ItemRangeWithItems.Items );
        int count = itemsToUnselect.Count;

        for( int i = count - 1; i >= 0; i-- )
        {
          object itemToUnselect = itemsToUnselect[ i ];

          foreach( SelectionCellRangeWithItems existingSelectionCellRangeWithItems in m_cellsToSelect )
          {
            SelectionRange columnIntersection = columnRange.Intersect( existingSelectionCellRangeWithItems.ColumnRange );

            if( columnIntersection.IsEmpty )
              continue;

            int index = Array.IndexOf( existingSelectionCellRangeWithItems.ItemRangeWithItems.Items, itemToUnselect );

            if( index > -1 )
            {
              cellsRangeToRemove.Add(
                new SelectionCellRangeWithItems(
                  new SelectionRange( existingSelectionCellRangeWithItems.ItemRange.GetIndexFromItemOffset( index ) ),
                  new object[] { itemToUnselect },
                  columnIntersection ) );
            }
          }
        }

        // Remove the currently unselected item from the new range to select
        foreach( SelectionCellRangeWithItems cellRangeToRemove in cellsRangeToRemove )
        {
          selectionChanged |= m_cellsToSelect.Remove( cellRangeToRemove );
        }

        count = itemsToUnselect.Count;

        for( int i = 0; i < count; i++ )
        {
          object itemToUnselect = itemsToUnselect[ i ];

          foreach( SelectionCellRangeWithItems existingSelectionCellRangeWithItems in m_owner.SelectedCellsStore )
          {
            SelectionRange columnIntersection = columnRange.Intersect( existingSelectionCellRangeWithItems.ColumnRange );

            if( columnIntersection.IsEmpty )
              continue;

            int index = Array.IndexOf( existingSelectionCellRangeWithItems.ItemRangeWithItems.Items, itemToUnselect );

            if( index > -1 )
            {
              index = existingSelectionCellRangeWithItems.ItemRange.GetIndexFromItemOffset( index );

              SelectionCellRange cellRangeTemp = new SelectionCellRange(
                new SelectionRange( existingSelectionCellRangeWithItems.ItemRange.GetIndexFromItemOffset( index ) ),
                columnIntersection );

              if( !m_cellsToUnselect.Contains( cellRangeTemp ) )
              {
                selectionChanged = true;

                m_cellsToUnselect.Add( new SelectionCellRangeWithItems(
                  cellRangeTemp.ItemRange, new object[] { itemToUnselect }, cellRangeTemp.ColumnRange ) );
              }
            }
          }
        }

        return selectionChanged;
      }

      if( cellRangeWithItems.Length == 1 )
      {
        if( !m_cellsToSelect.Remove( cellRangeWithItems ) )
        {
          if( !m_owner.SelectedCellsStore.Contains( cellRange ) )
            return false;

          if( m_cellsToUnselect.Contains( cellRange ) )
            return false;

          m_cellsToUnselect.Add( cellRangeWithItems );
        }

        return true;
      }
      else
      {
        SelectedCellsStorage tempStorage = new SelectedCellsStorage( m_owner );
        tempStorage.Add( cellRangeWithItems );

        // Remove the currently selected item from the new range to select
        foreach( SelectionCellRangeWithItems existingSelectionCellRangeWithItems in m_cellsToSelect )
        {
          tempStorage.Remove( existingSelectionCellRangeWithItems );
        }

        bool selectionChanged = m_cellsToSelect.Remove( cellRangeWithItems );

        if( tempStorage.Count > 0 )
        {
          selectionChanged = true;

          foreach( SelectionCellRangeWithItems cellRangeWithItemsToAdd in tempStorage )
          {
            Debug.Assert( !m_cellsToUnselect.Contains( cellRangeWithItemsToAdd.CellRange ) );
            m_cellsToUnselect.Add( cellRangeWithItemsToAdd );
          }
        }

        return selectionChanged;
      }
    }

    public bool SelectJustThisItem( int itemIndex, object item )
    {
      bool selectionDone = true;
      m_toDeferSelect.Clear();

      SelectionRangeWithItems rangeWithItemsToSelect = new SelectionRangeWithItems( itemIndex, item );
      SelectionRange range = rangeWithItemsToSelect.Range;

      if( m_itemsToSelect.Contains( range ) )
        selectionDone = false;

      m_itemsToSelect.Clear();
      SelectedItemsStorage selectedItemsInChange = m_owner.SelectedItemsStore;

      if( selectedItemsInChange.Contains( range ) )
      {
        if( !m_itemsToUnselect.Contains( range ) )
          selectionDone = false;

        m_itemsToUnselect.Clear();

        foreach( SelectionRangeWithItems selectedRangeWithItems in selectedItemsInChange )
        {
          m_itemsToUnselect.Add( selectedRangeWithItems );
        }

        m_itemsToUnselect.Remove( rangeWithItemsToSelect );
      }
      else
      {
        m_itemsToSelect.Add( rangeWithItemsToSelect );
        m_itemsToUnselect.Clear();

        foreach( SelectionRangeWithItems selectedRangeWithItems in selectedItemsInChange )
        {
          m_itemsToUnselect.Add( selectedRangeWithItems );
        }
      }

      this.UnselectAllCells();
      return selectionDone;
    }

    public bool SelectJustThisCell( int itemIndex, object item, int columnIndex )
    {
      bool selectionDone = true;
      m_toDeferSelect.Clear();

      SelectionCellRangeWithItems rangeWithItemsToSelect =
        new SelectionCellRangeWithItems( itemIndex, item, columnIndex );

      SelectionCellRange cellRange = rangeWithItemsToSelect.CellRange;

      if( m_cellsToSelect.Contains( cellRange ) )
        selectionDone = false;

      m_cellsToSelect.Clear();
      SelectedCellsStorage selectedCellsInChange = m_owner.SelectedCellsStore;

      if( selectedCellsInChange.Contains( cellRange ) )
      {
        if( !m_cellsToUnselect.Contains( cellRange ) )
          selectionDone = false;

        m_cellsToUnselect.Clear();

        foreach( SelectionCellRangeWithItems selectedCellRangeWithItems in selectedCellsInChange )
        {
          m_cellsToUnselect.Add( selectedCellRangeWithItems );
        }

        m_cellsToUnselect.Remove( rangeWithItemsToSelect );
      }
      else
      {
        m_cellsToSelect.Add( rangeWithItemsToSelect );
        m_cellsToUnselect.Clear();

        foreach( SelectionCellRangeWithItems selectedCellRangeWithItems in selectedCellsInChange )
        {
          m_cellsToUnselect.Add( selectedCellRangeWithItems );
        }
      }

      this.UnselectAllItems();
      return selectionDone;
    }

    public bool SelectItemCells(
      int itemIndex,
      object item,
      HashedLinkedList<ColumnBase> columnsByVisiblePosition,
      bool preserveSelection )
    {
      bool selectionDone = true;
      int columnsCount = columnsByVisiblePosition.Count;
      m_toDeferSelect.Clear();

      SelectionCellRangeWithItems rangeWithItemsToSelect =
        new SelectionCellRangeWithItems(
        new SelectionRange( itemIndex ),
        new object[] { item },
        new SelectionRange( 0, columnsCount - 1 ) );

      SelectionCellRange cellRange = rangeWithItemsToSelect.CellRange;

      m_cellsToSelect.Clear();
      SelectedCellsStorage selectedCellsInChange = m_owner.SelectedCellsStore;

      // Remove all currently selected Cells from the new selectionRange
      // to avoid duplicate SelectionCellRange
      SelectedCellsStorage tempStorage = new SelectedCellsStorage( null );
      tempStorage.Add( rangeWithItemsToSelect );

      for( int i = 0; i < selectedCellsInChange.Count; i++ )
      {
        tempStorage.Remove( selectedCellsInChange[ i ] );
      }

      foreach( ColumnBase column in columnsByVisiblePosition )
      {
        if( !column.Visible )
          tempStorage.Remove( new SelectionCellRangeWithItems( itemIndex, item, column.VisiblePosition ) );
      }

      int tempStorageCount = tempStorage.Count;

      // All Cells are already selected
      if( tempStorageCount == 0 )
      {
        if( !m_cellsToUnselect.Contains( cellRange ) )
          selectionDone = false;

        m_cellsToUnselect.Clear();

        if( !preserveSelection )
        {
          foreach( SelectionCellRangeWithItems selectedCellRangeWithItems in selectedCellsInChange )
          {
            m_cellsToUnselect.Add( selectedCellRangeWithItems );
          }
        }

        m_cellsToUnselect.Remove( rangeWithItemsToSelect );
      }
      else
      {
        // Add each range to selection
        for( int i = 0; i < tempStorageCount; i++ )
        {
          m_cellsToSelect.Add( tempStorage[ i ] );
        }

        m_cellsToUnselect.Clear();

        if( !preserveSelection )
        {
          foreach( SelectionCellRangeWithItems selectedCellRangeWithItems in selectedCellsInChange )
          {
            tempStorage = new SelectedCellsStorage( null );
            tempStorage.Add( selectedCellRangeWithItems );
            tempStorage.Remove( rangeWithItemsToSelect );
            tempStorageCount = tempStorage.Count;

            for( int i = 0; i < tempStorageCount; i++ )
            {
              m_cellsToUnselect.Add( tempStorage[ i ] );
            }
          }
        }
      }

      if( !preserveSelection )
        this.UnselectAllItems();

      return selectionDone;
    }

    public void UnselectAllItems()
    {
      m_toDeferSelect.Clear();
      m_itemsToSelect.Clear();
      m_itemsToUnselect.Clear();

      foreach( SelectionRangeWithItems selectedRangeWithItems in m_owner.SelectedItemsStore )
      {
        m_itemsToUnselect.Add( selectedRangeWithItems );
      }
    }

    public void UnselectAllCells()
    {
      m_cellsToSelect.Clear();
      m_cellsToUnselect.Clear();

      foreach( SelectionCellRangeWithItems selectedCellRangeWithItems in m_owner.SelectedCellsStore )
      {
        m_cellsToUnselect.Add( selectedCellRangeWithItems );
      }
    }

    public void Cleanup()
    {
      m_itemsToSelect.Clear();
      m_itemsToUnselect.Clear();
      m_cellsToSelect.Clear();
      m_cellsToUnselect.Clear();
    }

    public void CleanupDeferSelection()
    {
      m_toDeferSelect.Clear();
    }

    public void UpdateSelectedItemsInChangeOfDataGridContext()
    {
      var removedRangeWithItems = new List<SelectionRangeWithItems>();
      var unselectedItemsFromRemove = this.GetUnselectedItemsFromRemove( removedRangeWithItems );
      var ownerSelectedItems = m_owner.SelectedItemsStore;

      for( int i = 0; i < m_sourceChanges.Count; i++ )
      {
        var sourceChangeInfo = m_sourceChanges[ i ];

        switch( sourceChangeInfo.Action )
        {
          case NotifyCollectionChangedAction.Add:
            {
              if( sourceChangeInfo.StartIndex != -1 )
              {
                ownerSelectedItems.OffsetIndex( sourceChangeInfo.StartIndex, sourceChangeInfo.Count );
              }

              break;
            }

          case NotifyCollectionChangedAction.Remove:
            break;

          default:
            throw new NotSupportedException( "Only Add and Remove are supported." );
        }
      }

      foreach( SelectionRangeWithItems rangeWithItems in removedRangeWithItems )
      {
        ownerSelectedItems.Remove( rangeWithItems );
        ownerSelectedItems.OffsetIndex( rangeWithItems.Range.StartIndex, -rangeWithItems.Length );
      }

      for( int i = m_toDeferSelect.Count - 1; i >= 0; i-- )
      {
        object item = m_toDeferSelect[ i ];
        int itemIndex = m_owner.Items.IndexOf( item );

        if( itemIndex >= 0 )
        {
          if( !m_itemsToUnselect.Contains( itemIndex ) )
          {
            m_itemsToSelect.Add( new SelectionRangeWithItems( itemIndex, item ) );
          }

          m_toDeferSelect.RemoveAt( i );
        }
      }

      if( ( m_itemsToUnselect.Count > 0 ) || ( m_itemsToSelect.Count > 0 ) || ( unselectedItemsFromRemove.Any() ) )
      {
        var realizedDataRows = new Dictionary<int, DataRow>();

        // Only want to update the realizedDataRows if the selection change is from user interaction and not
        // from the source being updated.  When the source is updated, the generator will recreate needed container,
        // so the old one will have the correct selection state.
        foreach( var container in m_owner.CustomItemContainerGenerator.RealizedContainers )
        {
          var dataRow = container as DataRow;

          if( ( dataRow != null ) && ( DataGridControl.GetDataGridContext( dataRow ) == m_owner ) )
          {
            realizedDataRows[ DataGridVirtualizingPanel.GetItemIndex( dataRow ) ] = dataRow;
          }
        }

        var sourceItemIsDataRow = false;

        foreach( var rangeWithItems in m_itemsToUnselect )
        {
          sourceItemIsDataRow |= this.SetIsSelectedOnDataRow( realizedDataRows, rangeWithItems, false );
          ownerSelectedItems.Remove( rangeWithItems );
        }

        foreach( var rangeWithItems in m_itemsToSelect )
        {
          sourceItemIsDataRow |= this.SetIsSelectedOnDataRow( realizedDataRows, rangeWithItems, true );
          ownerSelectedItems.Add( rangeWithItems );
        }

        foreach( var rangeWithItems in unselectedItemsFromRemove )
        {
          m_itemsToUnselect.Add( rangeWithItems );
        }

        if( !sourceItemIsDataRow )
        {
          foreach( var realizedItemPair in realizedDataRows )
          {
            if( ownerSelectedItems.Contains( new SelectionRange( realizedItemPair.Key ) ) )
            {
              realizedItemPair.Value.SetIsSelected( true );
            }
            else
            {
              realizedItemPair.Value.SetIsSelected( false );
            }
          }
        }
      }
    }

    public void UpdateSelectedCellsInChangeOfDataGridContext()
    {
      var removedCellsRangeWithItems = new List<SelectionCellRangeWithItems>();
      var unselectedCellsFromRemove = this.GetUnselectedCellsFromRemove( removedCellsRangeWithItems );
      var ownerSelectedCells = m_owner.SelectedCellsStore;

      for( int i = 0; i < m_sourceChanges.Count; i++ )
      {
        var sourceChangeInfo = m_sourceChanges[ i ];

        switch( sourceChangeInfo.Action )
        {
          case NotifyCollectionChangedAction.Add:
            {
              if( sourceChangeInfo.StartIndex != -1 )
              {
                ownerSelectedCells.OffsetIndex( sourceChangeInfo.StartIndex, sourceChangeInfo.Count );
              }

              break;
            }

          case NotifyCollectionChangedAction.Remove:
            break;

          default:
            throw new NotSupportedException( "Only Add and Remove are supported." );
        }
      }

      foreach( var cellRangeWithItems in removedCellsRangeWithItems )
      {
        ownerSelectedCells.Remove( cellRangeWithItems );
        ownerSelectedCells.OffsetIndex( cellRangeWithItems.ItemRange.StartIndex, -cellRangeWithItems.ItemRange.Length );
      }

      if( ( m_cellsToUnselect.Count > 0 ) || ( m_cellsToSelect.Count > 0 ) || ( unselectedCellsFromRemove.Any() ) )
      {
        var realizedDataRows = new Dictionary<int, DataRow>();

        // Only want to update the realizedCells if the selection change is from user interaction and not
        // from the source being updated.  When the source is updated, the generator will recreate needed container,
        // so the old one will have the correct selection state.
        foreach( var container in m_owner.CustomItemContainerGenerator.RealizedContainers )
        {
          var dataRow = container as DataRow;

          if( ( dataRow != null ) && ( DataGridControl.GetDataGridContext( dataRow ) == m_owner ) )
          {
            realizedDataRows[ DataGridVirtualizingPanel.GetItemIndex( dataRow ) ] = dataRow;
          }
        }

        // We use the ColumnsByVisiblePosition for when column are changing position, we want to have the state before the change.
        var columnsByVisiblePositionLikedList = m_owner.ColumnsByVisiblePosition;
        var columnIndex = 0;
        var columnsVisiblePosition = new Dictionary<ColumnBase, int>( columnsByVisiblePositionLikedList.Count );
        var columnNode = columnsByVisiblePositionLikedList.First;

        while( columnNode != null )
        {
          columnsVisiblePosition.Add( columnNode.Value, columnIndex );
          columnIndex++;
          columnNode = columnNode.Next;
        }

        foreach( var cellRangeWithItems in m_cellsToUnselect )
        {
          this.SetIsSelectedOnDataCell( columnsVisiblePosition, realizedDataRows, cellRangeWithItems, false );
          ownerSelectedCells.Remove( cellRangeWithItems );
        }

        foreach( var cellRangeWithItems in m_cellsToSelect )
        {
          this.SetIsSelectedOnDataCell( columnsVisiblePosition, realizedDataRows, cellRangeWithItems, true );
          ownerSelectedCells.Add( cellRangeWithItems );
        }

        foreach( var cellRangeWithItems in unselectedCellsFromRemove )
        {
          m_cellsToUnselect.Add( cellRangeWithItems );
        }
      }
    }

    public void UpdateSelectionAfterSourceDataItemAdded( NotifyCollectionChangedEventArgs e )
    {
      int newStartingIndex = e.NewStartingIndex;

      // if newStartingIndex == -1, we take for granted that the newly added items are at the end of the list.
      // In that case, we have nothing to do.
      if( newStartingIndex == -1 )
        return;

      IList addedItemsList = e.NewItems;
      int addedItemsCount = addedItemsList.Count;

      m_sourceChanges.Add( new SourceChangeInfo( e.Action, newStartingIndex, addedItemsCount, addedItemsList ) );

      m_itemsToSelect.OffsetIndex( newStartingIndex, addedItemsCount );
      m_itemsToUnselect.OffsetIndex( newStartingIndex, addedItemsCount );
      m_cellsToSelect.OffsetIndex( newStartingIndex, addedItemsCount );
      m_cellsToUnselect.OffsetIndex( newStartingIndex, addedItemsCount );
    }

    public void UpdateSelectionAfterSourceDataItemRemoved( NotifyCollectionChangedEventArgs e )
    {
      int oldStartingIndex = e.OldStartingIndex;
      IList removedItemsList = e.OldItems;
      int removedItemsCount = removedItemsList.Count;

      m_sourceChanges.Add( new SourceChangeInfo( e.Action, oldStartingIndex, removedItemsCount, removedItemsList ) );

      if( oldStartingIndex == -1 )
      {
        foreach( object item in removedItemsList )
        {
          SelectionRangeWithItems rangeWithItemsToRemove = new SelectionRangeWithItems( SelectionRange.Empty, new object[] { item } );

          m_itemsToSelect.Remove( rangeWithItemsToRemove );
          m_itemsToUnselect.Remove( rangeWithItemsToRemove );

          SelectionCellRangeWithItems cellRangeWithItemsToRemove = new SelectionCellRangeWithItems(
            SelectionRange.Empty, new object[] { item }, new SelectionRange( 0, int.MaxValue - 1 ) );

          m_cellsToSelect.Remove( cellRangeWithItemsToRemove );
          m_cellsToUnselect.Remove( cellRangeWithItemsToRemove );
        }

        // Seek out in a max range of removedItemsCount the new position for actually selected item.
        m_itemsToSelect.OffsetIndexBasedOnSourceNewIndex( -removedItemsCount );
        m_itemsToUnselect.OffsetIndexBasedOnSourceNewIndex( -removedItemsCount );
        m_cellsToSelect.OffsetIndexBasedOnSourceNewIndex( -removedItemsCount );
        m_cellsToUnselect.OffsetIndexBasedOnSourceNewIndex( -removedItemsCount );
      }
      else
      {
        SelectionRange itemRange = new SelectionRange( oldStartingIndex, oldStartingIndex + removedItemsCount - 1 );
        SelectionRangeWithItems rangeWithItemsToRemove = new SelectionRangeWithItems( itemRange, null );
        m_itemsToSelect.Remove( rangeWithItemsToRemove );
        m_itemsToUnselect.Remove( rangeWithItemsToRemove );
        m_itemsToSelect.OffsetIndex( oldStartingIndex, -removedItemsCount );
        m_itemsToUnselect.OffsetIndex( oldStartingIndex, -removedItemsCount );

        SelectionCellRangeWithItems cellRangeWithItemsToRemove = new SelectionCellRangeWithItems(
          itemRange, null, new SelectionRange( 0, int.MaxValue - 1 ) );

        m_cellsToSelect.Remove( cellRangeWithItemsToRemove );
        m_cellsToUnselect.Remove( cellRangeWithItemsToRemove );
        m_cellsToSelect.OffsetIndex( oldStartingIndex, -removedItemsCount );
        m_cellsToUnselect.OffsetIndex( oldStartingIndex, -removedItemsCount );
      }
    }

    public void UpdateSelectionAfterSourceDataItemReplaced( NotifyCollectionChangedEventArgs e )
    {
      Debug.Assert( e.OldItems.Count == e.NewItems.Count );
      Debug.Assert( e.OldStartingIndex == e.NewStartingIndex );

      SelectedItemsStorage selectedItemsStorage = m_owner.SelectedItemsStore;
      SelectedCellsStorage selectedCellsStorage = m_owner.SelectedCellsStore;
      int oldItemIndex = e.OldStartingIndex;
      IList oldItems = e.OldItems;
      IList newItems = e.NewItems;
      int replacedItemCount = oldItems.Count;
      int cellRangeCount = selectedCellsStorage.Count;

      if( oldItemIndex >= 0 )
      {
        int itemIndex = oldItemIndex;

        for( int i = 0; i < replacedItemCount; i++ )
        {
          object newItem = newItems[ i ];

          if( selectedItemsStorage.Contains( itemIndex ) )
          {
            this.UnselectItems( new SelectionRangeWithItems( itemIndex, oldItems[ i ] ) );
            this.SelectItems( new SelectionRangeWithItems( itemIndex, newItem ) );
          }

          SelectionCellRange replacedCellRange = new SelectionCellRange(
            new SelectionRange( itemIndex ), new SelectionRange( 0, int.MaxValue - 1 ) );

          for( int j = 0; j < cellRangeCount; j++ )
          {
            SelectionCellRangeWithItems cellRangeWithItems = selectedCellsStorage[ j ];
            SelectionCellRange cellRange = cellRangeWithItems.CellRange;

            if( !cellRange.Intersect( replacedCellRange ).IsEmpty )
            {
              object[] items = cellRangeWithItems.ItemRangeWithItems.Items;

              if( items != null )
              {
                items[ cellRange.ItemRange.GetOffsetFromItemIndex( itemIndex ) ] = newItem;
              }
            }
          }

          itemIndex++;
        }
      }
      else
      {
        CollectionView sourceItems = m_owner.Items;

        for( int i = 0; i < replacedItemCount; i++ )
        {
          object newItem = newItems[ i ];
          int itemIndex = sourceItems.IndexOf( newItem );

          if( itemIndex < 0 )
            continue;

          if( selectedItemsStorage.Contains( itemIndex ) )
          {
            this.UnselectItems( new SelectionRangeWithItems( itemIndex, oldItems[ i ] ) );
            this.SelectItems( new SelectionRangeWithItems( itemIndex, newItem ) );
          }

          SelectionCellRange replacedCellRange = new SelectionCellRange(
            new SelectionRange( itemIndex ), new SelectionRange( 0, int.MaxValue - 1 ) );

          for( int j = 0; j < cellRangeCount; j++ )
          {
            SelectionCellRangeWithItems cellRangeWithItems = selectedCellsStorage[ j ];
            SelectionCellRange cellRange = cellRangeWithItems.CellRange;

            if( !cellRange.Intersect( replacedCellRange ).IsEmpty )
            {
              object[] items = cellRangeWithItems.ItemRangeWithItems.Items;

              if( items != null )
              {
                items[ cellRange.ItemRange.GetOffsetFromItemIndex( itemIndex ) ] = newItem;
              }
            }
          }
        }
      }
    }

    public SelectionInfo GetSelectionInfo()
    {
      var unselectedItemsFromRemove = this.GetUnselectedItemsFromRemove();
      var unselectedCellsFromRemove = this.GetUnselectedCellsFromRemove();

      var itemsToUnselect = ( ( m_itemsToUnselect.Count > 0 ) || unselectedItemsFromRemove.Any() ) ? ( SelectedItemsStorage )m_itemsToUnselect.Clone() : SelectionChanger.EmptyItemsStore;
      var itemsToSelect = ( m_itemsToSelect.Count > 0 ) ? ( SelectedItemsStorage )m_itemsToSelect.Clone() : SelectionChanger.EmptyItemsStore;
      var cellsToUnselect = ( ( m_cellsToUnselect.Count > 0 ) || unselectedCellsFromRemove.Any() ) ? ( SelectedCellsStorage )m_cellsToUnselect.Clone() : SelectionChanger.EmptyCellsStore;
      var cellsToSelect = ( m_cellsToSelect.Count > 0 ) ? ( SelectedCellsStorage )m_cellsToSelect.Clone() : SelectionChanger.EmptyCellsStore;

      foreach( var rangeWithItems in unselectedItemsFromRemove )
      {
        itemsToUnselect.Add( rangeWithItems );
      }

      foreach( var cellRangeWithItems in unselectedCellsFromRemove )
      {
        cellsToUnselect.Add( cellRangeWithItems );
      }

      return new SelectionInfo( m_owner, itemsToUnselect, itemsToSelect, cellsToUnselect, cellsToSelect );
    }

    private IEnumerable<SelectionRangeWithItems> GetUnselectedItemsFromRemove()
    {
      return this.GetUnselectedItemsFromRemove( null );
    }

    private IEnumerable<SelectionRangeWithItems> GetUnselectedItemsFromRemove( ICollection<SelectionRangeWithItems> removedRangeWithItems )
    {
      var store = m_owner.SelectedItemsStore;
      if( ( store.Count <= 0 ) || ( m_sourceChanges.Count <= 0 ) )
        return Enumerable.Empty<SelectionRangeWithItems>();

      var unselectedItemsFromRemove = new List<SelectionRangeWithItems>();

      for( int i = 0; i < m_sourceChanges.Count; i++ )
      {
        var sourceChangeInfo = m_sourceChanges[ i ];
        if( ( sourceChangeInfo.Action != NotifyCollectionChangedAction.Remove ) || ( sourceChangeInfo.StartIndex == -1 ) )
          continue;

        var startIndex = sourceChangeInfo.StartIndex;
        var removedItemCount = sourceChangeInfo.Count;
        var removedItems = new object[ removedItemCount ];

        sourceChangeInfo.Items.CopyTo( removedItems, 0 );

        var range = new SelectionRange( startIndex, startIndex + removedItemCount - 1 );
        var rangeWithItemsToRemove = new SelectionRangeWithItems( range, removedItems );

        if( removedRangeWithItems != null )
        {
          removedRangeWithItems.Add( rangeWithItemsToRemove );
        }

        if( store.Contains( rangeWithItemsToRemove ) )
        {
          unselectedItemsFromRemove.Add( rangeWithItemsToRemove );
        }
      }

      return unselectedItemsFromRemove;
    }

    private IEnumerable<SelectionCellRangeWithItems> GetUnselectedCellsFromRemove()
    {
      return this.GetUnselectedCellsFromRemove( null );
    }

    private IEnumerable<SelectionCellRangeWithItems> GetUnselectedCellsFromRemove( ICollection<SelectionCellRangeWithItems> removedCellsRangeWithItems )
    {
      var store = m_owner.SelectedCellsStore;
      if( ( store.Count <= 0 ) || ( m_sourceChanges.Count <= 0 ) )
        return Enumerable.Empty<SelectionCellRangeWithItems>();

      var unselectedCellsFromRemove = new List<SelectionCellRangeWithItems>();

      for( int i = 0; i < m_sourceChanges.Count; i++ )
      {
        var sourceChangeInfo = m_sourceChanges[ i ];
        if( ( sourceChangeInfo.Action != NotifyCollectionChangedAction.Remove ) || ( sourceChangeInfo.StartIndex == -1 ) )
          continue;

        var startIndex = sourceChangeInfo.StartIndex;
        var removedItemCount = sourceChangeInfo.Count;
        var range = new SelectionCellRange( new SelectionRange( startIndex, startIndex + removedItemCount - 1 ), new SelectionRange( 0, int.MaxValue - 1 ) );

        if( removedCellsRangeWithItems != null )
        {
          var removedItems = new object[ removedItemCount ];
          sourceChangeInfo.Items.CopyTo( removedItems, 0 );

          removedCellsRangeWithItems.Add( new SelectionCellRangeWithItems( range.ItemRange, removedItems, range.ColumnRange ) );
        }

        foreach( var cellRangeWithItems in store.GetIntersectedCellRangesWithItems( range ) )
        {
          unselectedCellsFromRemove.Add( cellRangeWithItems );
        }
      }

      return unselectedCellsFromRemove;
    }

    private bool SetIsSelectedOnDataRow( Dictionary<int, DataRow> realizedDataRows, SelectionRangeWithItems rangeWithItems, bool selected )
    {
      int rangeLength = rangeWithItems.Length;
      object[] rangeItems = rangeWithItems.Items;
      bool selectionChanged = false;

      if( rangeItems != null )
      {
        for( int i = 0; i < rangeLength; i++ )
        {
          DataRow rangeItemAsDataRow = rangeItems[ i ] as DataRow;

          if( rangeItemAsDataRow != null )
          {
            selectionChanged = true;
            rangeItemAsDataRow.SetIsSelected( selected );
          }
          else
          {
            // We take for granted that no item will be a DataRow
            break;
          }
        }
      }

      return selectionChanged;
    }

    private void SetIsSelectedOnDataCell(
      Dictionary<ColumnBase, int> columnsVisiblePosition,
      Dictionary<int, DataRow> realizedDataRows,
      SelectionCellRangeWithItems cellRangeWithItems,
      bool selected )
    {
      var rangeItems = cellRangeWithItems.ItemRangeWithItems.Items;
      var selectionChanged = false;
      var columnRange = cellRangeWithItems.ColumnRange;

      if( rangeItems != null )
      {
        int itemsCount = rangeItems.Length;

        for( int i = 0; i < itemsCount; i++ )
        {
          var rangeItemAsDataRow = rangeItems[ i ] as DataRow;

          if( rangeItemAsDataRow != null )
          {
            selectionChanged = true;

            foreach( var dataCell in rangeItemAsDataRow.CreatedCells )
            {
              if( dataCell.IsContainerVirtualized )
                continue;

              int columnPosition;

              if( !columnsVisiblePosition.TryGetValue( dataCell.ParentColumn, out columnPosition ) )
                continue;

              if( !columnRange.Intersect( new SelectionRange( columnPosition ) ).IsEmpty )
              {
                dataCell.SetIsSelected( selected );
              }
            }
          }
          else
          {
            // We take for granted that no item will be a DataRow
            break;
          }
        }
      }

      if( !selectionChanged )
      {
        var itemRange = cellRangeWithItems.ItemRange;

        foreach( var realizedItemPair in realizedDataRows )
        {
          if( !itemRange.Intersect( new SelectionRange( realizedItemPair.Key ) ).IsEmpty )
          {
            foreach( var dataCell in realizedItemPair.Value.CreatedCells )
            {
              if( dataCell.IsContainerVirtualized )
                continue;

              int columnPosition;

              if( !columnsVisiblePosition.TryGetValue( dataCell.ParentColumn, out columnPosition ) )
                continue;

              if( !columnRange.Intersect( new SelectionRange( columnPosition ) ).IsEmpty )
              {
                dataCell.SetIsSelected( selected );
              }
            }
          }
        }
      }
    }

    private readonly List<object> m_toDeferSelect;
    private readonly SelectedItemsStorage m_itemsToSelect;
    private readonly SelectedItemsStorage m_itemsToUnselect;
    private readonly SelectedCellsStorage m_cellsToSelect;
    private readonly SelectedCellsStorage m_cellsToUnselect;
    private readonly List<SourceChangeInfo> m_sourceChanges;

    #region SourceChangeInfo Private Class

    private sealed class SourceChangeInfo
    {
      internal SourceChangeInfo( NotifyCollectionChangedAction action, int startIndex, int count, IList items )
      {
        this.Action = action;
        this.StartIndex = startIndex;
        this.Count = count;
        this.Items = items;
      }

      internal NotifyCollectionChangedAction Action
      {
        get;
        private set;
      }

      internal int StartIndex
      {
        get;
        private set;
      }

      internal int Count
      {
        get;
        private set;
      }

      internal IList Items
      {
        get;
        private set;
      }
    }

    #endregion
  }
}
