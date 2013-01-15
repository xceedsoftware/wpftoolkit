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
using System.Diagnostics;
using System.Windows.Data;

namespace Xceed.Wpf.DataGrid
{
  internal class SelectedCellsStorage : IEnumerable
  {
    #region CONSTRUCTORS

    internal SelectedCellsStorage( DataGridContext dataGridContext, int capacity )
    {
      // dataGridContext can be null for store that we don't want to auto-get Items from index
      m_dataGridContext = dataGridContext;
      m_list = new List<SelectionCellRangeWithItems>( capacity );
    }

    internal SelectedCellsStorage( SelectedCellsStorage collection )
    {
      m_dataGridContext = collection.m_dataGridContext;
      m_list = new List<SelectionCellRangeWithItems>( collection.m_list );
      m_cellsCount = collection.m_cellsCount;
    }

    #endregion CONSTRUCTORS

    #region PUBLIC PROPERTIES

    public int Count
    {
      get
      {
        return m_list.Count;
      }
    }

    public int CellsCount
    {
      get
      {
        return m_cellsCount;
      }
    }

    public DataGridContext DataGridContext
    {
      get
      {
        return m_dataGridContext;
      }
    }

    public SelectionCellRangeWithItems this[ int index ]
    {
      get
      {
        return m_list[ index ];
      }
      set
      {
        SelectionCellRangeWithItems oldRange = m_list[ index ];
        m_list[ index ] = value;
        m_cellsCount += value.Length - oldRange.Length;
      }
    }

    #endregion PUBLIC PROPERTIES

    #region IEnumerable

    IEnumerator IEnumerable.GetEnumerator()
    {
      return m_list.GetEnumerator();
    }

    #endregion IEnumerable

    #region PUBLIC METHODS

    public int Add( SelectionCellRangeWithItems cellRangeWithItems )
    {


      m_cellsCount += cellRangeWithItems.Length;
      SelectionRangeWithItems itemRangeWithItems = cellRangeWithItems.ItemRangeWithItems;

      SelectedItemsStorage.UpdateSelectionRangeWithItemsFromAdd(
        m_dataGridContext, itemRangeWithItems, ref itemRangeWithItems );

      m_list.Add( new SelectionCellRangeWithItems(
        itemRangeWithItems.Range, itemRangeWithItems.Items, cellRangeWithItems.ColumnRange ) );

      return m_list.Count - 1;
    }

    public void Insert( int index, SelectionCellRangeWithItems cellRangeWithItems )
    {
      Debug.Assert( !this.Contains( cellRangeWithItems.CellRange ) );
      m_cellsCount += cellRangeWithItems.Length;

      SelectionRangeWithItems itemRangeWithItems = cellRangeWithItems.ItemRangeWithItems;

      SelectedItemsStorage.UpdateSelectionRangeWithItemsFromAdd(
        m_dataGridContext, itemRangeWithItems, ref itemRangeWithItems );

      m_list.Insert(
        index, new SelectionCellRangeWithItems(
          itemRangeWithItems.Range, itemRangeWithItems.Items, cellRangeWithItems.ColumnRange ) );
    }

    public void RemoveAt( int index )
    {
      m_cellsCount -= m_list[ index ].Length;
      m_list.RemoveAt( index );
    }

    public void Clear()
    {
      m_cellsCount = 0;
      m_list.Clear();
    }

    public SelectionCellRangeWithItems[] ToArray()
    {
      return m_list.ToArray();
    }

    public bool Contains( int itemIndex, int columnIndex )
    {
      if( ( itemIndex < 0 ) || ( columnIndex < 0 ) )
        return false;

      SelectionCellRange cellRange = new SelectionCellRange( itemIndex, columnIndex );
      int count = m_list.Count;

      for( int i = 0; i < count; i++ )
      {
        if( !m_list[ i ].CellRange.Intersect( cellRange ).IsEmpty )
          return true;
      }

      return false;
    }

    public bool Contains( SelectionCellRange cellRange )
    {
      int count = m_list.Count;

      int cellRangeLength = cellRange.Length;

      // Ensure every Cells are present in the SelectedCellsStorage
      if( cellRangeLength > 1 )
      {
        SelectedCellsStorage cellStorage = new SelectedCellsStorage( null, cellRangeLength );
        cellStorage.Add( new SelectionCellRangeWithItems( cellRange.ItemRange,
          null,
          cellRange.ColumnRange ) );

        for( int i = 0; i < count; i++ )
        {
          cellStorage.Remove( m_list[ i ] );

          if( cellStorage.Count == 0 )
            return true;
        }
      }
      else
      {
        for( int i = 0; i < count; i++ )
        {
          if( !m_list[ i ].CellRange.Intersect( cellRange ).IsEmpty )
            return true;
        }
      }

      return false;
    }

    public bool Contains( SelectionCellRangeWithItems cellRangeWithItemsToCompare )
    {
      int count = m_list.Count;
      SelectionCellRange cellRangeToCompare = cellRangeWithItemsToCompare.CellRange;

      int cellRangeLength = cellRangeToCompare.Length;
      // If there is more than one Cell in the range, ensure 
      // the range is completely contained within this SelectedCellsStorage
      if( cellRangeLength > 1 )
      {
        SelectedCellsStorage cellStorage = new SelectedCellsStorage( null, cellRangeLength );
        cellStorage.Add( cellRangeWithItemsToCompare );

        // The range is completely contained when the store 
        // is empty after removing all ranges contained within
        // this SelectedCellsStorage
        for( int i = 0; i < count; i++ )
        {
          SelectionCellRangeWithItems cellRangeWithItems = m_list[ i ];

          cellStorage.Remove( cellRangeWithItems );

          if( cellStorage.Count == 0 )
            return true;
        }
      }
      else
      {
        for( int i = 0; i < count; i++ )
        {
          SelectionCellRangeWithItems rangeWithItem = m_list[ i ];
          SelectionCellRange rangeIntersection = rangeWithItem.CellRange.Intersect( cellRangeToCompare );

          if( !rangeIntersection.IsEmpty )
          {
            if( cellRangeWithItemsToCompare.ItemRangeWithItems.IsItemsEqual(
              rangeIntersection.ItemRange, rangeWithItem.ItemRangeWithItems ) )
            {
              return true;
            }
          }
        }
      }

      return false;
    }

    public bool Remove( SelectionCellRangeWithItems cellRangeWithItemsToRemove )
    {
      if( cellRangeWithItemsToRemove.CellRange.IsEmpty )
        return true;

      bool removed = false;
      SelectionCellRange cellRangeToRemove = cellRangeWithItemsToRemove.CellRange;
      object[] itemsToRemove = cellRangeWithItemsToRemove.ItemRangeWithItems.Items;
      int itemsToRemoveCount = ( itemsToRemove == null ) ? 0 : itemsToRemove.Length;
      int itemOffsetToRemove = 0;
      bool itemRangeToRemoveIsEmpty = cellRangeToRemove.ItemRange.IsEmpty;

      do
      {
        for( int i = m_list.Count - 1; i >= 0; i-- )
        {
          SelectionCellRangeWithItems cellRangeWithItems = m_list[ i ];
          SelectionRange itemRange = cellRangeWithItems.ItemRange;
          SelectionCellRange cellRange = cellRangeWithItems.CellRange;
          SelectionRangeWithItems itemRangeWithItems = cellRangeWithItems.ItemRangeWithItems;
          SelectionCellRange cellRangeIntersection;
          object[] oldRangeItems = itemRangeWithItems.Items;

          if( itemRangeToRemoveIsEmpty )
          {
            int itemOffset = Array.IndexOf( oldRangeItems, itemsToRemove[ itemOffsetToRemove ] );

            if( itemOffset == -1 )
              continue;

            int itemIndex = itemRange.GetIndexFromItemOffset( itemOffset );

            cellRangeIntersection = new SelectionCellRange(
              new SelectionRange( itemIndex, itemIndex ),
              cellRangeWithItems.ColumnRange.Intersect( cellRangeToRemove.ColumnRange ) );
          }
          else
          {
            cellRangeIntersection = cellRange.Intersect( cellRangeToRemove );

            if( cellRangeIntersection.IsEmpty )
              continue;

            if( !itemRangeWithItems.IsItemsEqual(
              cellRangeIntersection.ItemRange, cellRangeWithItemsToRemove.ItemRangeWithItems ) )
            {
              continue;
            }
          }

          removed = true;
          m_cellsCount -= cellRangeIntersection.Length;
          SelectionCellRange[] newCellRanges = cellRange.Exclude( cellRangeIntersection );

          if( newCellRanges.Length == 0 )
          {
            m_list.RemoveAt( i );
          }
          else
          {
            SelectionCellRange newCellRange = newCellRanges[ 0 ];

            m_list[ i ] = new SelectionCellRangeWithItems(
              newCellRange.ItemRange, itemRangeWithItems.GetItems( newCellRange.ItemRange ), newCellRange.ColumnRange );

            for( int j = 1; j < newCellRanges.Length; j++ )
            {
              newCellRange = newCellRanges[ j ];

              m_list.Insert( i + j, new SelectionCellRangeWithItems(
                newCellRange.ItemRange, itemRangeWithItems.GetItems( newCellRange.ItemRange ), newCellRange.ColumnRange ) );
            }
          }
        }

        itemOffsetToRemove++;
      } while( ( itemRangeToRemoveIsEmpty ) && ( itemOffsetToRemove < itemsToRemoveCount ) );

      return removed;
    }

    public void OffsetIndex( int itemStartIndex, int offset )
    {
      // Used to offset index after an add or remove from the data source of the grid.

      SelectionRange offsetRange = new SelectionRange(
        itemStartIndex, itemStartIndex + Math.Abs( offset ) - 1 );

      for( int i = this.Count - 1; i >= 0; i-- )
      {
        SelectionCellRangeWithItems cellRangeWithItems = m_list[ i ];
        SelectionRange itemRange = cellRangeWithItems.ItemRange;
        SelectionRange columnRange = cellRangeWithItems.ColumnRange;

        if( offsetRange > itemRange )
          continue;

        SelectionRange itemRangeIntersection = itemRange.Intersect( offsetRange );
        object[] originalItems = cellRangeWithItems.ItemRangeWithItems.Items;

        if( !itemRangeIntersection.IsEmpty )
        {
          // Should only happen when adding since when we remove data from the source, we remove the
          // the range from the list.
          Debug.Assert( offset > 0 );

          // Offset the index higher than the start index of the new added item
          SelectionRange topRange;
          SelectionRange bottomRange;

          if( itemRange.StartIndex > itemRange.EndIndex )
          {
            if( itemRangeIntersection.EndIndex == itemRange.EndIndex )
            {
              SelectionRange newItemRange = new SelectionRange( itemRange.StartIndex + offset, itemRange.EndIndex + offset );
              m_list[ i ] = new SelectionCellRangeWithItems( newItemRange, originalItems, columnRange );
              continue;
            }
            else
            {
              int bottomRangeEndIndex = itemStartIndex + offset;
              bottomRange = new SelectionRange( itemStartIndex - 1, itemRange.EndIndex );
              topRange = new SelectionRange( itemRange.Length - bottomRange.Length - 1 + bottomRangeEndIndex, bottomRangeEndIndex );
            }
          }
          else
          {
            if( itemRangeIntersection.StartIndex == itemRange.StartIndex )
            {
              SelectionRange newItemRange = new SelectionRange( itemRange.StartIndex + offset, itemRange.EndIndex + offset );
              m_list[ i ] = new SelectionCellRangeWithItems( newItemRange, originalItems, columnRange );
              continue;
            }
            else
            {
              int bottomRangeStartIndex = itemStartIndex + offset;
              topRange = new SelectionRange( itemRange.StartIndex, itemStartIndex - 1 );
              bottomRange = new SelectionRange( bottomRangeStartIndex, itemRange.Length - topRange.Length - 1 + bottomRangeStartIndex );
            }
          }

          object[] topItems = null;
          object[] bottomItems = null;

          if( originalItems != null )
          {
            topItems = new object[ topRange.Length ];
            Array.Copy( originalItems, 0, topItems, 0, topItems.Length );
            bottomItems = new object[ bottomRange.Length ];
            Array.Copy( originalItems, topItems.Length, bottomItems, 0, bottomItems.Length );
          }

          m_list[ i ] = new SelectionCellRangeWithItems( topRange, topItems, columnRange );
          m_list.Insert( i + 1, new SelectionCellRangeWithItems( bottomRange, bottomItems, columnRange ) );
        }
        else
        {
          // Offset the index by the count added
          SelectionRange newItemRange = new SelectionRange( itemRange.StartIndex + offset, itemRange.EndIndex + offset );
          m_list[ i ] = new SelectionCellRangeWithItems( newItemRange, originalItems, columnRange );
        }
      }
    }

    public void OffsetIndexBasedOnSourceNewIndex( int maxOffset )
    {
      if( m_dataGridContext == null )
        throw new InvalidOperationException( "We must have a DataGridContext to find the new index." );

      CollectionView sourceItems = m_dataGridContext.Items;

      for( int i = this.Count - 1; i >= 0; i-- )
      {
        SelectionCellRangeWithItems cellRangeWithItems = m_list[ i ];
        SelectionRangeWithItems itemRangeWithItems = cellRangeWithItems.ItemRangeWithItems;
        object[] items = itemRangeWithItems.Items;

        if( items == null )
          throw new InvalidOperationException( "We should have items to find the new index." );

        object item = items[ 0 ];
        SelectionRange itemRange = itemRangeWithItems.Range;
        int startIndex = Math.Min( itemRange.StartIndex, itemRange.EndIndex );

        if( maxOffset < 0 )
        {
          for( int j = 0; j >= maxOffset; j-- )
          {
            if( object.Equals( sourceItems.GetItemAt( startIndex ), item ) )
            {
              if( j != 0 )
              {
                SelectionRange newItemRange = new SelectionRange( itemRange.StartIndex + j, itemRange.EndIndex + j );
                m_list[ i ] = new SelectionCellRangeWithItems( newItemRange, items, cellRangeWithItems.ColumnRange );
              }

              break;
            }

            startIndex--;

            if( startIndex < 0 )
              break;
          }
        }
        else
        {
          int sourceItemCount = sourceItems.Count;

          for( int j = 0; j <= maxOffset; j++ )
          {
            if( object.Equals( sourceItems.GetItemAt( startIndex ), item ) )
            {
              if( j != 0 )
              {
                SelectionRange newItemRange = new SelectionRange( itemRange.StartIndex + j, itemRange.EndIndex + j );
                m_list[ i ] = new SelectionCellRangeWithItems( newItemRange, items, cellRangeWithItems.ColumnRange );
              }

              break;
            }

            startIndex++;

            if( startIndex >= sourceItemCount )
              break;
          }
        }
      }
    }

    public List<SelectionRange> GetIntersectedColumnRanges( SelectionCellRange cellRange )
    {
      List<SelectionRange> intersectionRanges = new List<SelectionRange>();
      int count = m_list.Count;

      for( int i = 0; i < count; i++ )
      {
        SelectionCellRangeWithItems range = m_list[ i ];
        SelectionCellRange cellRangeIntersection = range.CellRange.Intersect( cellRange );

        if( !cellRangeIntersection.IsEmpty )
        {
          intersectionRanges.Add( cellRangeIntersection.ColumnRange );
        }
      }

      return intersectionRanges;
    }

    public List<SelectionCellRangeWithItems> GetIntersectedCellRangesWithItems( SelectionCellRange cellRange )
    {
      List<SelectionCellRangeWithItems> intersectionCellRanges = new List<SelectionCellRangeWithItems>();
      int count = m_list.Count;

      for( int i = 0; i < count; i++ )
      {
        SelectionCellRangeWithItems range = m_list[ i ];
        SelectionCellRange cellRangeIntersection = range.CellRange.Intersect( cellRange );

        if( !cellRangeIntersection.IsEmpty )
        {
          intersectionCellRanges.Add(
            new SelectionCellRangeWithItems(
              cellRangeIntersection.ItemRange,
              range.ItemRangeWithItems.GetItems( cellRangeIntersection.ItemRange ),
              cellRangeIntersection.ColumnRange ) );
        }
      }

      return intersectionCellRanges;
    }

    #endregion PUBLIC METHODS

    private List<SelectionCellRangeWithItems> m_list;
    private DataGridContext m_dataGridContext;
    private int m_cellsCount;
  }
}
