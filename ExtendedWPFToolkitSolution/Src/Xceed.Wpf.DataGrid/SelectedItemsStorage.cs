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
using System.Diagnostics;
using System.Linq;
using Xceed.Utils.Collections;

namespace Xceed.Wpf.DataGrid
{
  internal class SelectedItemsStorage : ICloneable, IEnumerable<SelectionRangeWithItems>, IEnumerable
  {
    internal SelectedItemsStorage( DataGridContext dataGridContext )
    {
      // dataGridContext can be null for store that we don't want to auto-get Items from index
      m_dataGridContext = dataGridContext;
    }

    #region Count Property

    public int Count
    {
      get
      {
        return m_ranges.Count;
      }
    }

    #endregion

    #region ItemsCount Property

    public int ItemsCount
    {
      get
      {
        return m_itemsCount;
      }
    }

    private int m_itemsCount;

    #endregion

    #region DataGridContext Property

    public DataGridContext DataGridContext
    {
      get
      {
        return m_dataGridContext;
      }
    }

    private readonly DataGridContext m_dataGridContext;

    #endregion

    #region [] Property

    public SelectionRangeWithItems this[ int index ]
    {
      get
      {
        return m_ranges[ index ].Value;
      }
      private set
      {
        this.RemoveAt( index, false );
        this.Insert( index, value, false );
      }
    }

    #endregion

    public static void UpdateSelectionRangeWithItemsFromAdd(
      DataGridContext dataGridContext,
      SelectionRangeWithItems newRangeWithItems,
      ref SelectionRangeWithItems lastRangeWithItems )
    {
      // Only work for adding at the end of an existing range.

      object[] newRangeItems = newRangeWithItems.Items;

      if( ( dataGridContext == null )
        || ( ( dataGridContext.ItemsSourceCollection == null ) && ( newRangeItems == null ) )
        || ( dataGridContext.ItemsSourceCollection is DataGridVirtualizingCollectionViewBase ) )
      {
        lastRangeWithItems = new SelectionRangeWithItems( new SelectionRange( lastRangeWithItems.Range.StartIndex, newRangeWithItems.Range.EndIndex ), null );
        return;
      }

      SharedList newItemsList;

      if( newRangeWithItems == lastRangeWithItems )
      {
        if( newRangeItems != null )
          return;

        newItemsList = new SharedList( lastRangeWithItems.Length );
      }
      else
      {
        int minNewCapacity = lastRangeWithItems.Length + newRangeWithItems.Length;

        // Optimization, re-use the ItemsList if available.
        if( lastRangeWithItems.SharedList != null )
        {
          newItemsList = lastRangeWithItems.SharedList.Value;
          newItemsList.EnsureCapacity( minNewCapacity );
        }
        else
        {
          newItemsList = new SharedList( minNewCapacity );
          newItemsList.AddRange( lastRangeWithItems.Items );
        }
      }

      var rangeToUpdateStartIndex = lastRangeWithItems.Range.StartIndex;
      var newRangeEndIndex = newRangeWithItems.Range.EndIndex;

      // If new range have no items set, found the items and set it.
      if( newRangeItems == null )
      {
        var items = dataGridContext.Items;
        var newRangeStartIndex = newRangeWithItems.Range.StartIndex;

        if( newRangeEndIndex > newRangeStartIndex )
        {
          for( int i = newRangeStartIndex; i <= newRangeEndIndex; i++ )
          {
            newItemsList.Add( items.GetItemAt( i ) );
          }
        }
        else
        {
          for( int i = newRangeStartIndex; i >= newRangeEndIndex; i-- )
          {
            newItemsList.Add( items.GetItemAt( i ) );
          }
        }
      }
      else
      {
        newItemsList.AddRange( newRangeItems );
      }

      lastRangeWithItems = new SelectionRangeWithItems( new SelectionRange( rangeToUpdateStartIndex, newRangeEndIndex ), newItemsList );
    }

    public void Add( SelectionRangeWithItems rangeWithItems )
    {
      Debug.Assert( !this.Contains( rangeWithItems.Range ) );

      m_itemsCount += rangeWithItems.Length;

      if( this.Count > 0 )
      {
        var lastIndex = this.Count - 1;
        var lastRangeWithItems = this[ lastIndex ];
        var lastRange = lastRangeWithItems.Range;

        if( ( lastRange.EndIndex + 1 ) == rangeWithItems.Range.StartIndex )
        {
          Debug.Assert( rangeWithItems.Range.EndIndex > lastRange.EndIndex );

          SelectedItemsStorage.UpdateSelectionRangeWithItemsFromAdd( m_dataGridContext, rangeWithItems, ref lastRangeWithItems );

          this[ lastIndex ] = lastRangeWithItems;
          return;
        }
        else if( ( lastRange.EndIndex - 1 ) == rangeWithItems.Range.StartIndex )
        {
          Debug.Assert( rangeWithItems.Range.EndIndex < lastRange.EndIndex );

          SelectedItemsStorage.UpdateSelectionRangeWithItemsFromAdd( m_dataGridContext, rangeWithItems, ref lastRangeWithItems );

          this[ lastIndex ] = lastRangeWithItems;
          return;
        }
      }

      SelectedItemsStorage.UpdateSelectionRangeWithItemsFromAdd( m_dataGridContext, rangeWithItems, ref rangeWithItems );

      this.Insert( this.Count, rangeWithItems, true );
    }

    public void Clear()
    {
      m_itemsCount = 0;
      m_ranges.Clear();
      m_map.Clear();
    }

    public SelectionRange[] ToSelectionRangeArray()
    {
      return ( from item in m_ranges
               select item.Value.Range ).ToArray();
    }

    public bool Contains( int itemIndex )
    {
      if( ( itemIndex < -1 ) || ( itemIndex >= int.MaxValue ) )
        return false;

      return this.Contains( new SelectionRange( itemIndex ) );
    }

    public bool Contains( SelectionRange range )
    {
      return m_map.Overlaps( SelectedItemsStorage.GetArea( range ) );
    }

    public bool Contains( SelectionRangeWithItems rangeWithItems )
    {
      return this.IndexOfOverlap( rangeWithItems ).Any();
    }

    public bool Remove( SelectionRangeWithItems rangeWithItems )
    {
      if( rangeWithItems.IsEmpty )
        return this.RemoveEmptyRange( rangeWithItems );

      return this.RemoveRangeWithItems( rangeWithItems );
    }

    public void OffsetIndex( int startIndex, int offset )
    {
      if( m_map.Count == 0 )
        return;

      // Used to offset index after an add or remove from the data source of the grid.
      var offsetRange = new SelectionRange( startIndex, startIndex + Math.Abs( offset ) - 1 );

      var entries = m_map.GetEntriesWithin( new RSTree1D<SelectionRangeWithItemsWrapper>.Area( startIndex, int.MaxValue - startIndex ) ).OrderByDescending( e => e.Item.Index ).ToList();

      // Adjust the range of all ranges greater or that overlaps the target range.
      foreach( var entry in entries )
      {
        var currentRangeWithItemsWrapper = entry.Item;
        var currentRangeIndex = currentRangeWithItemsWrapper.Index;
        var currentRangeWithItems = currentRangeWithItemsWrapper.Value;
        var currentRange = currentRangeWithItems.Range;
        var currentRangeItems = currentRangeWithItems.Items;

        Debug.Assert( !( offsetRange > currentRange ) );

        var currentRangeIntersection = currentRange.Intersect( offsetRange );

        // The range overlaps.
        if( !currentRangeIntersection.IsEmpty )
        {
          // Should only happen when adding since when we remove data from the source, we remove the
          // the range from the list.
          Debug.Assert( offset > 0 );

          SelectionRange topRange;
          SelectionRange bottomRange;

          if( currentRange.StartIndex > currentRange.EndIndex )
          {
            if( currentRangeIntersection.EndIndex == currentRange.EndIndex )
            {
              this[ currentRangeIndex ] = new SelectionRangeWithItems(
                                            new SelectionRange( currentRange.StartIndex + offset, currentRange.EndIndex + offset ),
                                            currentRangeItems );
              continue;
            }
            else
            {
              topRange = new SelectionRange( currentRange.StartIndex + offset, startIndex + offset );
              bottomRange = new SelectionRange( startIndex - 1, currentRange.EndIndex );
            }
          }
          else
          {
            if( currentRangeIntersection.StartIndex == currentRange.StartIndex )
            {
              this[ currentRangeIndex ] = new SelectionRangeWithItems(
                                            new SelectionRange( currentRange.StartIndex + offset, currentRange.EndIndex + offset ),
                                            currentRangeItems );
              continue;
            }
            else
            {
              topRange = new SelectionRange( currentRange.StartIndex, startIndex - 1 );
              bottomRange = new SelectionRange( startIndex + offset, currentRange.EndIndex + offset );
            }
          }

          object[] topItems = null;
          object[] bottomItems = null;

          if( currentRangeItems != null )
          {
            topItems = new object[ topRange.Length ];
            Array.Copy( currentRangeItems, 0, topItems, 0, topItems.Length );

            bottomItems = new object[ bottomRange.Length ];
            Array.Copy( currentRangeItems, topItems.Length, bottomItems, 0, bottomItems.Length );
          }

          this[ currentRangeIndex ] = new SelectionRangeWithItems( topRange, topItems );
          this.Insert( currentRangeIndex + 1, new SelectionRangeWithItems( bottomRange, bottomItems ), true );
        }
        // The range is greater.
        else
        {
          this[ currentRangeIndex ] = new SelectionRangeWithItems(
                                        new SelectionRange( currentRange.StartIndex + offset, currentRange.EndIndex + offset ),
                                        currentRangeItems );
        }
      }
    }

    public void OffsetIndexBasedOnSourceNewIndex( int maxOffset )
    {
      if( m_dataGridContext == null )
        throw new InvalidOperationException( "We must have a DataGridContext to find the new index." );

      if( maxOffset == 0 )
        return;

      var sourceItems = m_dataGridContext.Items;

      for( int i = this.Count - 1; i >= 0; i-- )
      {
        var rangeWithItems = this[ i ];
        var rangeItems = rangeWithItems.Items;

        if( rangeItems == null )
          throw new InvalidOperationException( "We should have items to find the new index." );

        var item = rangeItems[ 0 ];
        var range = rangeWithItems.Range;
        var startIndex = range.StartIndex;

        if( maxOffset < 0 )
        {
          for( int j = 0; j >= maxOffset; j-- )
          {
            if( object.Equals( sourceItems.GetItemAt( startIndex ), item ) )
            {
              if( j != 0 )
              {
                this[ i ] = new SelectionRangeWithItems( new SelectionRange( range.StartIndex + j, range.EndIndex + j ), rangeItems );
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
          var sourceItemCount = sourceItems.Count;

          for( int j = 0; j <= maxOffset; j++ )
          {
            if( object.Equals( sourceItems.GetItemAt( startIndex ), item ) )
            {
              if( j != 0 )
              {
                this[ i ] = new SelectionRangeWithItems( new SelectionRange( range.StartIndex + j, range.EndIndex + j ), rangeItems );
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

    private static RSTree1D<SelectionRangeWithItemsWrapper>.Area GetArea( SelectionRange range )
    {
      if( range.IsEmpty )
        return RSTree1D<SelectionRangeWithItemsWrapper>.Area.Empty;

      return new RSTree1D<SelectionRangeWithItemsWrapper>.Area( Math.Min( range.StartIndex, range.EndIndex ), range.Length );
    }

    private static RSTree1D<SelectionRangeWithItemsWrapper>.Area GetArea( SelectionRangeWithItems range )
    {
      return SelectedItemsStorage.GetArea( range.Range );
    }

    private void Insert( int index, SelectionRangeWithItems item, bool repairIndex )
    {
      var wrapper = new SelectionRangeWithItemsWrapper( item, index );

      m_ranges.Insert( index, wrapper );

      if( repairIndex )
      {
        this.RepairIndex( index + 1 );
      }

      if( !item.IsEmpty )
      {
        m_map.Add( SelectedItemsStorage.GetArea( item ), wrapper );
      }
    }

    private void RemoveAt( int index, bool repairIndex )
    {
      var wrapper = m_ranges[ index ];
      var rangeWithItems = wrapper.Value;

      Debug.Assert( wrapper.Index == index );

      if( !rangeWithItems.IsEmpty )
      {
        var removed = m_map.Remove( SelectedItemsStorage.GetArea( rangeWithItems ), wrapper );
        Debug.Assert( removed, "Failed to remove the selection range." );

        // Since there should be only a single instance of the wrapper within the collection, try an altenate strategy.
        if( !removed )
        {
          var entry = m_map.FirstOrDefault( e => e.Item == wrapper );
          if( entry.Item == wrapper )
          {
            removed = m_map.Remove( entry );
          }

          Debug.Assert( removed, "Failed to find the selection range." );
        }
      }

      m_ranges.RemoveAt( index );

      if( repairIndex )
      {
        this.RepairIndex( index );
      }
    }

    private void RepairIndex( int index )
    {
      Debug.Assert( index >= 0 );

      for( int i = index; i < m_ranges.Count; i++ )
      {
        m_ranges[ i ].Index = i;
      }
    }

    private bool RemoveEmptyRange( SelectionRangeWithItems rangeWithItems )
    {
      Debug.Assert( rangeWithItems.IsEmpty );

      bool removed = false;

      var itemsToRemove = rangeWithItems.Items;
      var itemsToRemoveCount = ( itemsToRemove == null ) ? 0 : itemsToRemove.Length;
      var itemOffsetToRemove = 0;

      do
      {
        for( int i = this.Count - 1; i >= 0; i-- )
        {
          var currentRangeWithItems = this[ i ];
          var currentRange = currentRangeWithItems.Range;

          var itemOffset = Array.IndexOf( currentRangeWithItems.Items, itemsToRemove[ itemOffsetToRemove ] );
          if( itemOffset == -1 )
            continue;

          var overlap = new SelectionRange( currentRange.GetIndexFromItemOffset( itemOffset ) );

          removed = true;
          m_itemsCount -= overlap.Length;

          var newRanges = currentRange.Exclude( overlap );

          if( newRanges.Length == 0 )
          {
            this.RemoveAt( i, true );
          }
          else
          {
            this[ i ] = new SelectionRangeWithItems( newRanges[ 0 ], currentRangeWithItems.GetItems( newRanges[ 0 ] ) );

            if( newRanges.Length > 1 )
            {
              Debug.Assert( newRanges.Length == 2 );
              this.Insert( i + 1, new SelectionRangeWithItems( newRanges[ 1 ], currentRangeWithItems.GetItems( newRanges[ 1 ] ) ), true );
            }
          }
        }

        itemOffsetToRemove++;
      }
      while( itemOffsetToRemove < itemsToRemoveCount );

      return removed;
    }

    private bool RemoveRangeWithItems( SelectionRangeWithItems rangeWithItems )
    {
      Debug.Assert( !rangeWithItems.IsEmpty );

      var rangeToRemove = rangeWithItems.Range;
      var matches = this.IndexOfOverlap( rangeWithItems ).OrderBy( item => item ).ToList();

      for( int i = matches.Count - 1; i >= 0; i-- )
      {
        var index = matches[ i ];
        var currentRangeWithItems = this[ index ];
        var currentRange = currentRangeWithItems.Range;
        var overlap = rangeToRemove.Intersect( currentRange );

        Debug.Assert( !overlap.IsEmpty );

        m_itemsCount -= overlap.Length;

        var newRanges = currentRange.Exclude( overlap );

        if( newRanges.Length == 0 )
        {
          this.RemoveAt( index, true );
        }
        else
        {
          this[ index ] = new SelectionRangeWithItems( newRanges[ 0 ], currentRangeWithItems.GetItems( newRanges[ 0 ] ) );

          if( newRanges.Length > 1 )
          {
            Debug.Assert( newRanges.Length == 2 );
            this.Insert( index + 1, new SelectionRangeWithItems( newRanges[ 1 ], currentRangeWithItems.GetItems( newRanges[ 1 ] ) ), true );
          }
        }
      }

      return ( matches.Count > 0 );
    }

    private IEnumerable<int> IndexOfOverlap( SelectionRangeWithItems rangeWithItems )
    {
      foreach( var entry in m_map.GetEntriesWithin( SelectedItemsStorage.GetArea( rangeWithItems ) ) )
      {
        var candidate = entry.Item;
        var target = candidate.Value;
        var overlap = rangeWithItems.Range.Intersect( target.Range );

        Debug.Assert( !overlap.IsEmpty );

        if( !overlap.IsEmpty && ( rangeWithItems.IsItemsEqual( overlap, target ) ) )
          yield return candidate.Index;
      }
    }

    #region ICloneable Members

    public object Clone()
    {
      var copy = new SelectedItemsStorage( m_dataGridContext );

      for( int i = 0; i < m_ranges.Count; i++ )
      {
        copy.Insert( i, m_ranges[ i ].Value, false );
      }

      copy.m_itemsCount = m_itemsCount;

      return copy;
    }

    #endregion

    #region IEnumerable<SelectionRangeWithItems> Members

    public IEnumerator<SelectionRangeWithItems> GetEnumerator()
    {
      return m_ranges.Select( item => item.Value ).GetEnumerator();
    }

    #endregion

    #region IEnumerable Members

    IEnumerator IEnumerable.GetEnumerator()
    {
      return this.GetEnumerator();
    }

    #endregion

    private readonly List<SelectionRangeWithItemsWrapper> m_ranges = new List<SelectionRangeWithItemsWrapper>();
    private readonly RSTree1D<SelectionRangeWithItemsWrapper> m_map = new RSTree1D<SelectionRangeWithItemsWrapper>();

    #region SelectionRangeWithItemsWrapper Private Class

    // We are wrapping the SelectionRangeWithItems structure inside a class so our collections
    // may target the same object.
    private sealed class SelectionRangeWithItemsWrapper
    {
      internal SelectionRangeWithItemsWrapper( SelectionRangeWithItems value, int index )
      {
        m_value = value;
        m_index = index;
      }

      internal SelectionRangeWithItems Value
      {
        get
        {
          return m_value;
        }
      }

      internal int Index
      {
        get
        {
          return m_index;
        }
        set
        {
          m_index = value;
        }
      }

      private readonly SelectionRangeWithItems m_value;
      private int m_index;
    }

    #endregion
  }
}
