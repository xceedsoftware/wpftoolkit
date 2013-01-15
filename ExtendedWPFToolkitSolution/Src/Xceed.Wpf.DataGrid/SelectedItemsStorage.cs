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
  internal class SelectedItemsStorage : IEnumerable
  {
    #region CONSTRUCTORS

    internal SelectedItemsStorage( DataGridContext dataGridContext, int capacity )
    {
      // dataGridContext can be null for store that we don't want to auto-get Items from index
      m_dataGridContext = dataGridContext;
      m_list = new List<SelectionRangeWithItems>( capacity );
    }

    internal SelectedItemsStorage( SelectedItemsStorage collection )
    {
      m_dataGridContext = collection.m_dataGridContext;
      m_list = new List<SelectionRangeWithItems>( collection.m_list );
      m_itemsCount = collection.m_itemsCount;
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

    public int ItemsCount
    {
      get
      {
        return m_itemsCount;
      }
    }

    public DataGridContext DataGridContext
    {
      get
      {
        return m_dataGridContext;
      }
    }

    public SelectionRangeWithItems this[ int index ]
    {
      get
      {
        return m_list[ index ];
      }
      set
      {
        SelectionRangeWithItems oldRangeWithItems = m_list[ index ];
        m_list[ index ] = value;
        m_itemsCount += value.Length - oldRangeWithItems.Length;
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

    public static void UpdateSelectionRangeWithItemsFromAdd(
      DataGridContext dataGridContext,
      SelectionRangeWithItems newRangeWithItems,
      ref SelectionRangeWithItems lastRangeWithItems )
    {
      // Only work for adding at the end of an existing range.

      object[] newRangeItems = newRangeWithItems.Items;

      if( ( ( dataGridContext == null )
        || ( ( dataGridContext.ItemsSourceCollection == null ) && ( newRangeItems == null ) )
        || ( dataGridContext.ItemsSourceCollection is DataGridVirtualizingCollectionViewBase ) ) )
      {
        lastRangeWithItems = new SelectionRangeWithItems(
          new SelectionRange( lastRangeWithItems.Range.StartIndex, newRangeWithItems.Range.EndIndex ),
          null );

        return;
      }

      object[] rangeToUpdateItems = lastRangeWithItems.Items;
      object[] newItems;
      int newItemsStartPosition;

      if( newRangeWithItems == lastRangeWithItems )
      {
        if( newRangeItems != null )
          return;

        newItemsStartPosition = 0;
        newItems = new object[ lastRangeWithItems.Length ];
      }
      else
      {
        newItemsStartPosition = lastRangeWithItems.Length;
        newItems = new object[ lastRangeWithItems.Length + newRangeWithItems.Length ];
        Array.Copy( rangeToUpdateItems, newItems, newItemsStartPosition );
      }

      CollectionView items = ( dataGridContext == null ) ? null : dataGridContext.Items;
      int rangeToUpdateStartIndex = lastRangeWithItems.Range.StartIndex;
      int rangeToUpdateEndIndex = lastRangeWithItems.Range.EndIndex;
      int newRangeStartIndex = newRangeWithItems.Range.StartIndex;
      int newRangeEndIndex = newRangeWithItems.Range.EndIndex;

      // If new range have no items set, found the items and set it.
      if( newRangeItems == null )
      {
        if( newRangeEndIndex > newRangeStartIndex )
        {
          for( int i = newRangeStartIndex; i <= newRangeEndIndex; i++ )
          {
            newItems[ newItemsStartPosition ] = items.GetItemAt( i );
            newItemsStartPosition++;
          }
        }
        else
        {
          for( int i = newRangeStartIndex; i >= newRangeEndIndex; i-- )
          {
            newItems[ newItemsStartPosition ] = items.GetItemAt( i );
            newItemsStartPosition++;
          }
        }
      }
      else
      {
        for( int i = 0; i < newRangeItems.Length; i++ )
        {
          newItems[ newItemsStartPosition ] = newRangeItems[ i ];
          newItemsStartPosition++;
        }
      }

      lastRangeWithItems = new SelectionRangeWithItems(
        new SelectionRange( rangeToUpdateStartIndex, newRangeEndIndex ),
        newItems );
    }

    public int Add( SelectionRangeWithItems rangeWithItems )
    {
      Debug.Assert( !this.Contains( rangeWithItems.Range ) );

      m_itemsCount += rangeWithItems.Length;
      int count = m_list.Count;

      if( count > 0 )
      {
        int lastIndex = count - 1;
        SelectionRangeWithItems lastRangeWithItems = m_list[ lastIndex ];
        SelectionRange lastRange = lastRangeWithItems.Range;

        if( ( lastRange.EndIndex + 1 ) == rangeWithItems.Range.StartIndex )
        {
          Debug.Assert( rangeWithItems.Range.EndIndex > lastRange.EndIndex );

          SelectedItemsStorage.UpdateSelectionRangeWithItemsFromAdd(
            m_dataGridContext, rangeWithItems, ref lastRangeWithItems );

          m_list[ lastIndex ] = lastRangeWithItems;
          return lastIndex;
        }
        else if( ( lastRange.EndIndex - 1 ) == rangeWithItems.Range.StartIndex )
        {
          Debug.Assert( rangeWithItems.Range.EndIndex < lastRange.EndIndex );

          SelectedItemsStorage.UpdateSelectionRangeWithItemsFromAdd(
            m_dataGridContext, rangeWithItems, ref lastRangeWithItems );

          m_list[ lastIndex ] = lastRangeWithItems;
          return lastIndex;
        }
      }

      SelectedItemsStorage.UpdateSelectionRangeWithItemsFromAdd(
        m_dataGridContext, rangeWithItems, ref rangeWithItems );

      m_list.Add( rangeWithItems );
      return m_list.Count - 1;
    }

    public void Insert( int index, SelectionRangeWithItems rangeWithItems )
    {
      Debug.Assert( !this.Contains( rangeWithItems.Range ) );
      m_itemsCount += rangeWithItems.Length;

      SelectedItemsStorage.UpdateSelectionRangeWithItemsFromAdd(
        m_dataGridContext, rangeWithItems, ref rangeWithItems );

      m_list.Insert( index, rangeWithItems );
    }

    public void RemoveAt( int index )
    {
      m_itemsCount -= m_list[ index ].Length;
      m_list.RemoveAt( index );
    }

    public void Clear()
    {
      m_itemsCount = 0;
      m_list.Clear();
    }

    public SelectionRangeWithItems[] ToArray()
    {
      return m_list.ToArray();
    }

    public SelectionRange[] ToSelectionRangeArray()
    {
      int count = m_list.Count;
      SelectionRange[] selections = new SelectionRange[ count ];

      for( int i = 0; i < count; i++ )
      {
        selections[ i ] = m_list[ i ].Range;
      }

      return selections;
    }

    public bool Contains( int itemIndex )
    {
      SelectionRange itemRange = new SelectionRange( itemIndex );
      int count = m_list.Count;

      for( int i = 0; i < count; i++ )
      {
        if( !m_list[ i ].Range.Intersect( itemRange ).IsEmpty )
          return true;
      }

      return false;
    }

    public bool Contains( SelectionRange itemRange )
    {
      int count = m_list.Count;

      for( int i = 0; i < count; i++ )
      {
        if( !m_list[ i ].Range.Intersect( itemRange ).IsEmpty )
          return true;
      }

      return false;
    }

    public bool Contains( SelectionRangeWithItems rangeWithItemsToCompare )
    {
      int count = m_list.Count;
      SelectionRange rangeToCompare = rangeWithItemsToCompare.Range;

      for( int i = 0; i < count; i++ )
      {
        SelectionRangeWithItems rangeWithItem = m_list[ i ];
        SelectionRange rangeIntersection = rangeWithItem.Range.Intersect( rangeToCompare );

        if( !rangeIntersection.IsEmpty )
        {
          if( rangeWithItemsToCompare.IsItemsEqual( rangeIntersection, rangeWithItem ) )
            return true;
        }
      }

      return false;
    }

    public bool Remove( SelectionRangeWithItems rangeWithItemsToRemove )
    {
      bool removed = false;
      SelectionRange rangeToRemove = rangeWithItemsToRemove.Range;
      object[] itemsToRemove = rangeWithItemsToRemove.Items;
      int itemsToRemoveCount = ( itemsToRemove == null ) ? 0 : itemsToRemove.Length;
      int itemOffsetToRemove = 0;
      bool rangeToRemoveIsEmpty = rangeToRemove.IsEmpty;

      do
      {
        for( int i = m_list.Count - 1; i >= 0; i-- )
        {
          SelectionRangeWithItems rangeWithItems = m_list[ i ];
          SelectionRange range = rangeWithItems.Range;
          SelectionRange rangeIntersection;
          object[] oldRangeItems = rangeWithItems.Items;

          if( rangeToRemoveIsEmpty )
          {
            int itemOffset = Array.IndexOf( oldRangeItems, itemsToRemove[ itemOffsetToRemove ] );

            if( itemOffset == -1 )
              continue;

            rangeIntersection = new SelectionRange( range.GetIndexFromItemOffset( itemOffset ) );
          }
          else
          {
            rangeIntersection = range.Intersect( rangeToRemove );

            if( rangeIntersection.IsEmpty )
              continue;

            if( !rangeWithItems.IsItemsEqual( rangeIntersection, rangeWithItemsToRemove ) )
              continue;
          }

          removed = true;
          m_itemsCount -= rangeIntersection.Length;
          SelectionRange[] newRanges = range.Exclude( rangeIntersection );

          if( newRanges.Length == 0 )
          {
            m_list.RemoveAt( i );
          }
          else
          {
            SelectionRange newRange = newRanges[ 0 ];
            m_list[ i ] = new SelectionRangeWithItems( newRange, rangeWithItems.GetItems( newRange ) );

            if( newRanges.Length > 1 )
            {
              Debug.Assert( newRanges.Length == 2 );
              newRange = newRanges[ 1 ];
              m_list.Insert( i + 1, new SelectionRangeWithItems( newRange, rangeWithItems.GetItems( newRange ) ) );
            }
          }
        }

        itemOffsetToRemove++;
      } while( ( rangeToRemoveIsEmpty ) && ( itemOffsetToRemove < itemsToRemoveCount ) );

      return removed;
    }

    public void OffsetIndex( int startIndex, int offset )
    {
      // Used to offset index after an add or remove from the data source of the grid.

      SelectionRange offsetRange = new SelectionRange(
        startIndex, startIndex + Math.Abs( offset ) - 1 );

      for( int i = this.Count - 1; i >= 0; i-- )
      {
        SelectionRangeWithItems rangeWithItems = m_list[ i ];
        SelectionRange range = rangeWithItems.Range;

        if( offsetRange > range )
          continue;

        SelectionRange rangeIntersection = range.Intersect( offsetRange );
        object[] originalItems = rangeWithItems.Items;

        if( !rangeIntersection.IsEmpty )
        {
          // Should only happen when adding since when we remove data from the source, we remove the
          // the range from the list.
          Debug.Assert( offset > 0 );

          // Offset the index higher than the start index of the new added item
          SelectionRange topRange;
          SelectionRange bottomRange;

          if( range.StartIndex > range.EndIndex )
          {
            if( rangeIntersection.EndIndex == range.EndIndex )
            {
              SelectionRange newRange = new SelectionRange( range.StartIndex + offset, range.EndIndex + offset );
              m_list[ i ] = new SelectionRangeWithItems( newRange, originalItems );
              continue;
            }
            else
            {
              int bottomRangeEndIndex = startIndex + offset;
              bottomRange = new SelectionRange( startIndex - 1, range.EndIndex );
              topRange = new SelectionRange( range.Length - bottomRange.Length - 1 + bottomRangeEndIndex, bottomRangeEndIndex );
            }
          }
          else
          {
            if( rangeIntersection.StartIndex == range.StartIndex )
            {
              SelectionRange newRange = new SelectionRange( range.StartIndex + offset, range.EndIndex + offset );
              m_list[ i ] = new SelectionRangeWithItems( newRange, originalItems );
              continue;
            }
            else
            {
              int bottomRangeStartIndex = startIndex + offset;
              topRange = new SelectionRange( range.StartIndex, startIndex - 1 );
              bottomRange = new SelectionRange( bottomRangeStartIndex, range.Length - topRange.Length - 1 + bottomRangeStartIndex );
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

          m_list[ i ] = new SelectionRangeWithItems( topRange, topItems );
          m_list.Insert( i + 1, new SelectionRangeWithItems( bottomRange, bottomItems ) );
        }
        else
        {
          // Offset the index by the count added
          SelectionRange newRange = new SelectionRange( range.StartIndex + offset, range.EndIndex + offset );
          m_list[ i ] = new SelectionRangeWithItems( newRange, originalItems );
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
        SelectionRangeWithItems rangeWithItems = m_list[ i ];
        object[] items = rangeWithItems.Items;

        if( items == null )
          throw new InvalidOperationException( "We should have items to find the new index." );

        object item = items[ 0 ];
        SelectionRange range = rangeWithItems.Range;
        int startIndex = Math.Min( range.StartIndex, range.EndIndex );

        if( maxOffset < 0 )
        {
          for( int j = 0; j >= maxOffset; j-- )
          {
            if( object.Equals( sourceItems.GetItemAt( startIndex ), item ) )
            {
              if( j != 0 )
              {
                SelectionRange newRange = new SelectionRange( range.StartIndex + j, range.EndIndex + j );
                m_list[ i ] = new SelectionRangeWithItems( newRange, items );
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
                SelectionRange newRange = new SelectionRange( range.StartIndex + j, range.EndIndex + j );
                m_list[ i ] = new SelectionRangeWithItems( newRange, items );
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

    #endregion PUBLIC METHODS

    private List<SelectionRangeWithItems> m_list;
    private DataGridContext m_dataGridContext;
    private int m_itemsCount;
  }
}
