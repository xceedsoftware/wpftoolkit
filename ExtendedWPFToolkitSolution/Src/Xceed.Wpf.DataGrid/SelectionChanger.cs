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
using System.Linq;
using System.Text;
using System.Collections;
using System.Windows.Controls;
using System.Diagnostics;
using System.Windows;
using System.Collections.Specialized;
using System.Windows.Data;

namespace Xceed.Wpf.DataGrid
{
  internal class SelectionChanger
  {
    #region CONSTRUCTORS

    public SelectionChanger( DataGridContext owner )
    {
      m_owner = owner;
      m_itemsToSelect = new SelectedItemsStorage( owner, 2 );
      m_itemsToUnselect = new SelectedItemsStorage( owner, 2 );
      m_cellsToSelect = new SelectedCellsStorage( owner, 2 );
      m_cellsToUnselect = new SelectedCellsStorage( owner, 2 );
      m_toDeferSelect = new List<object>( 1 );
      m_sourceChanges = new List<SourceChangeInfo>( 2 );
    }

    #endregion CONSTRUCTORS

    #region Owner Property

    public DataGridContext Owner
    {
      get
      {
        return m_owner;
      }
    }

    private DataGridContext m_owner;

    #endregion Owner Property

    #region PUBLIC METHODS

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

        SelectedItemsStorage tempStorage = new SelectedItemsStorage( m_owner, 8 );
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

        SelectedCellsStorage tempStorage = new SelectedCellsStorage( m_owner, 8 );
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

            if( index > -1 )
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
        SelectedItemsStorage tempStorage = new SelectedItemsStorage( m_owner, 8 );
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
        SelectedCellsStorage tempStorage = new SelectedCellsStorage( m_owner, 8 );
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
      SelectedCellsStorage tempStorage = new SelectedCellsStorage( null, 1 );
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
            tempStorage = new SelectedCellsStorage( null, 1 );
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

    public void UpdateSelectedItemsInChangeOfDataGridContext( bool itemsSourceChanged )
    {
      List<SelectionRangeWithItems> removedRangeWithItems;
      List<SelectionRangeWithItems> unselectedItemsFromRemove = this.GetUnselectedItemsFromRemove( out removedRangeWithItems );
      SelectedItemsStorage ownerSelectedItems = m_owner.SelectedItemsStore;
      int count = m_sourceChanges.Count;

      for( int i = 0; i < count; i++ )
      {
        SourceChangeInfo sourceChangeInfo = m_sourceChanges[ i ];

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

      count = m_toDeferSelect.Count;

      for( int i = count - 1; i >= 0; i-- )
      {
        object item = m_toDeferSelect[ i ];
        int itemIndex = m_owner.Items.IndexOf( item );

        if( itemIndex != -1 )
        {
          if( !m_itemsToUnselect.Contains( itemIndex ) )
          {
            m_itemsToSelect.Add( new SelectionRangeWithItems( itemIndex, item ) );
          }

          m_toDeferSelect.RemoveAt( i );
        }
      }

      if( ( m_itemsToUnselect.Count > 0 ) || ( m_itemsToSelect.Count > 0 ) || ( unselectedItemsFromRemove.Count > 0 ) )
      {
        Dictionary<int, DataRow> realizedDataRows = new Dictionary<int, DataRow>();

        // Only want to update the realizedDataRows if the selection change is from user interaction and not
        // from the source being updated.  When the source is updated, the generator will recreate needed container,
        // so the old one will have the correct selection state.
        foreach( DependencyObject container in m_owner.CustomItemContainerGenerator.RealizedContainers )
        {
          DataRow dataRow = container as DataRow;

          if( ( dataRow != null ) && ( DataGridControl.GetDataGridContext( dataRow ) == m_owner ) )
          {
            realizedDataRows[ DataGridVirtualizingPanel.GetItemIndex( dataRow ) ] = dataRow;
          }
        }

        bool sourceItemIsDataRow = false;

        foreach( SelectionRangeWithItems rangeWithItems in m_itemsToUnselect )
        {
          sourceItemIsDataRow |= this.SetIsSelectedOnDataRow( realizedDataRows, rangeWithItems, false );
          ownerSelectedItems.Remove( rangeWithItems );
        }

        foreach( SelectionRangeWithItems rangeWithItems in m_itemsToSelect )
        {
          sourceItemIsDataRow |= this.SetIsSelectedOnDataRow( realizedDataRows, rangeWithItems, true );
          ownerSelectedItems.Add( rangeWithItems );
        }

        foreach( SelectionRangeWithItems rangeWithItems in unselectedItemsFromRemove )
        {
          m_itemsToUnselect.Add( rangeWithItems );
        }

        if( !sourceItemIsDataRow )
        {
          foreach( KeyValuePair<int, DataRow> realizedItemPair in realizedDataRows )
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

    public void UpdateSelectedCellsInChangeOfDataGridContext( bool itemsSourceChanged )
    {
      List<SelectionCellRangeWithItems> removedCellsRangeWithItems;
      List<SelectionCellRangeWithItems> unselectedCellsFromRemove = this.GetUnselectedCellsFromRemove( out removedCellsRangeWithItems );
      SelectedCellsStorage ownerSelectedCells = m_owner.SelectedCellsStore;
      int count = m_sourceChanges.Count;

      for( int i = 0; i < count; i++ )
      {
        SourceChangeInfo sourceChangeInfo = m_sourceChanges[ i ];

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

      foreach( SelectionCellRangeWithItems cellRangeWithItems in removedCellsRangeWithItems )
      {
        ownerSelectedCells.Remove( cellRangeWithItems );
        ownerSelectedCells.OffsetIndex( cellRangeWithItems.ItemRange.StartIndex, -cellRangeWithItems.ItemRange.Length );
      }

      if( ( m_cellsToUnselect.Count > 0 ) || ( m_cellsToSelect.Count > 0 ) || ( unselectedCellsFromRemove.Count > 0 ) )
      {
        Dictionary<int, DataRow> realizedDataRows = new Dictionary<int, DataRow>();

        // Only want to update the realizedCells if the selection change is from user interaction and not
        // from the source being updated.  When the source is updated, the generator will recreate needed container,
        // so the old one will have the correct selection state.
        foreach( DependencyObject container in m_owner.CustomItemContainerGenerator.RealizedContainers )
        {
          DataRow dataRow = container as DataRow;

          if( ( dataRow != null ) && ( DataGridControl.GetDataGridContext( dataRow ) == m_owner ) )
          {
            realizedDataRows[ DataGridVirtualizingPanel.GetItemIndex( dataRow ) ] = dataRow;
          }
        }

        // We use the ColumnsByVisiblePosition for when column are changing position, we want to have the state before the change.
        HashedLinkedList<ColumnBase> columnsByVisiblePositionLikedList = m_owner.ColumnsByVisiblePosition;
        int columnIndex = 0;
        Dictionary<ColumnBase, int> columnsVisiblePosition = new Dictionary<ColumnBase, int>( columnsByVisiblePositionLikedList.Count );
        LinkedListNode<ColumnBase> columnNode = columnsByVisiblePositionLikedList.First;

        while( columnNode != null )
        {
          columnsVisiblePosition.Add( columnNode.Value, columnIndex );
          columnIndex++;
          columnNode = columnNode.Next;
        }

        foreach( SelectionCellRangeWithItems cellRangeWithItems in m_cellsToUnselect )
        {
          this.SetIsSelectedOnDataCell( columnsVisiblePosition, realizedDataRows, cellRangeWithItems, false );
          ownerSelectedCells.Remove( cellRangeWithItems );
        }

        foreach( SelectionCellRangeWithItems cellRangeWithItems in m_cellsToSelect )
        {
          this.SetIsSelectedOnDataCell( columnsVisiblePosition, realizedDataRows, cellRangeWithItems, true );
          ownerSelectedCells.Add( cellRangeWithItems );
        }

        foreach( SelectionCellRangeWithItems cellRangeWithItems in unselectedCellsFromRemove )
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

      if( oldItemIndex != -1 )
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

          if( itemIndex == -1 )
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
      List<SelectionRangeWithItems> removedRangeWithItems;
      List<SelectionRangeWithItems> unselectedItemsFromRemove = this.GetUnselectedItemsFromRemove( out removedRangeWithItems );
      List<SelectionCellRangeWithItems> removedCellsRangeWithItems;
      List<SelectionCellRangeWithItems> unselectedCellsFromRemove = this.GetUnselectedCellsFromRemove( out removedCellsRangeWithItems );

      SelectedItemsStorage itemsToUnselect = new SelectedItemsStorage( m_itemsToUnselect );
      SelectedCellsStorage cellsToUnselect = new SelectedCellsStorage( m_cellsToUnselect );

      foreach( SelectionRangeWithItems rangeWithItems in unselectedItemsFromRemove )
      {
        itemsToUnselect.Add( rangeWithItems );
      }

      foreach( SelectionCellRangeWithItems cellRangeWithItems in unselectedCellsFromRemove )
      {
        cellsToUnselect.Add( cellRangeWithItems );
      }

      return new SelectionInfo(
        m_owner, itemsToUnselect, new SelectedItemsStorage( m_itemsToSelect ),
        cellsToUnselect, new SelectedCellsStorage( m_cellsToSelect ) );
    }

    #endregion PUBLIC METHODS

    #region PRIVATE METHODS

    private List<SelectionRangeWithItems> GetUnselectedItemsFromRemove( out List<SelectionRangeWithItems> removedRangeWithItems )
    {
      List<SelectionRangeWithItems> unselectedItemsFromRemove = new List<SelectionRangeWithItems>( 8 );
      removedRangeWithItems = new List<SelectionRangeWithItems>( 8 );

      int count = m_sourceChanges.Count;

      for( int i = 0; i < count; i++ )
      {
        SourceChangeInfo sourceChangeInfo = m_sourceChanges[ i ];

        if( ( sourceChangeInfo.Action != NotifyCollectionChangedAction.Remove ) || ( sourceChangeInfo.StartIndex == -1 ) )
          continue;

        int startIndex = sourceChangeInfo.StartIndex;
        int removedItemCount = sourceChangeInfo.Count;
        object[] removedItems = new object[ removedItemCount ];
        sourceChangeInfo.Items.CopyTo( removedItems, 0 );

        SelectionRangeWithItems rangeWithItemsToRemove = new SelectionRangeWithItems(
          new SelectionRange( startIndex, startIndex + removedItemCount - 1 ),
          removedItems );

        removedRangeWithItems.Add( rangeWithItemsToRemove );

        if( m_owner.SelectedItemsStore.Contains( rangeWithItemsToRemove ) )
        {
          unselectedItemsFromRemove.Add( rangeWithItemsToRemove );
        }
      }

      return unselectedItemsFromRemove;
    }

    private List<SelectionCellRangeWithItems> GetUnselectedCellsFromRemove( out List<SelectionCellRangeWithItems> removedCellsRangeWithItems )
    {
      removedCellsRangeWithItems = new List<SelectionCellRangeWithItems>( 8 );
      List<SelectionCellRangeWithItems> unselectedCellsFromRemove = new List<SelectionCellRangeWithItems>( 8 );

      int count = m_sourceChanges.Count;

      for( int i = 0; i < count; i++ )
      {
        SourceChangeInfo sourceChangeInfo = m_sourceChanges[ i ];

        if( ( sourceChangeInfo.Action != NotifyCollectionChangedAction.Remove ) || ( sourceChangeInfo.StartIndex == -1 ) )
          continue;

        int startIndex = sourceChangeInfo.StartIndex;
        int removedItemCount = sourceChangeInfo.Count;
        object[] removedItems = new object[ removedItemCount ];
        sourceChangeInfo.Items.CopyTo( removedItems, 0 );

        SelectionCellRangeWithItems cellRangeWithItemsToRemove = new SelectionCellRangeWithItems(
          new SelectionRange( startIndex, startIndex + removedItemCount - 1 ),
          removedItems, new SelectionRange( 0, int.MaxValue - 1 ) );

        removedCellsRangeWithItems.Add( cellRangeWithItemsToRemove );

        List<SelectionCellRangeWithItems> intersectedCellRangesWithItems =
          m_owner.SelectedCellsStore.GetIntersectedCellRangesWithItems( cellRangeWithItemsToRemove.CellRange );

        foreach( SelectionCellRangeWithItems cellRangeWithItems in intersectedCellRangesWithItems )
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
      object[] rangeItems = cellRangeWithItems.ItemRangeWithItems.Items;
      bool selectionChanged = false;
      SelectionRange columnRange = cellRangeWithItems.ColumnRange;

      if( rangeItems != null )
      {
        int itemsCount = rangeItems.Length;

        for( int i = 0; i < itemsCount; i++ )
        {
          DataRow rangeItemAsDataRow = rangeItems[ i ] as DataRow;

          if( rangeItemAsDataRow != null )
          {
            selectionChanged = true;

            foreach( DataCell dataCell in rangeItemAsDataRow.CreatedCells )
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
        SelectionRange itemRange = cellRangeWithItems.ItemRange;

        foreach( KeyValuePair<int, DataRow> realizedItemPair in realizedDataRows )
        {
          if( !itemRange.Intersect( new SelectionRange( realizedItemPair.Key ) ).IsEmpty )
          {
            foreach( DataCell dataCell in realizedItemPair.Value.CreatedCells )
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

    #endregion PRIVATE METHODS

    #region FIELDS

    private List<object> m_toDeferSelect;
    private SelectedItemsStorage m_itemsToSelect;
    private SelectedItemsStorage m_itemsToUnselect;
    private SelectedCellsStorage m_cellsToSelect;
    private SelectedCellsStorage m_cellsToUnselect;
    private List<SourceChangeInfo> m_sourceChanges;

    #endregion FIELDS

    #region SourceChangeInfo Class

    private class SourceChangeInfo
    {
      public SourceChangeInfo( NotifyCollectionChangedAction action, int startIndex, int count, IList items )
      {
        this.Action = action;
        this.StartIndex = startIndex;
        this.Count = count;
        this.Items = items;
      }

      public NotifyCollectionChangedAction Action
      {
        get;
        private set;
      }

      public int StartIndex
      {
        get;
        private set;
      }

      public int Count
      {
        get;
        private set;
      }

      public IList Items
      {
        get;
        private set;
      }
    }

    #endregion SourceChangeInfo Class
  }
}
