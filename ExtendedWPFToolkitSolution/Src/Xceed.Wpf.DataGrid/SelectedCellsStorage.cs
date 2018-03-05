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
  internal class SelectedCellsStorage : ICloneable, IEnumerable<SelectionCellRangeWithItems>, IEnumerable
  {
    internal SelectedCellsStorage( DataGridContext dataGridContext )
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

    public SelectionCellRangeWithItems this[ int index ]
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

    public void Add( SelectionCellRangeWithItems rangeWithItems )
    {
      Debug.Assert( m_ranges.All( r => r.Value.CellRange.Intersect( rangeWithItems.CellRange ).IsEmpty ), "Part of this range is already selected" );

      var itemRangeWithItems = rangeWithItems.ItemRangeWithItems;

      SelectedItemsStorage.UpdateSelectionRangeWithItemsFromAdd( m_dataGridContext, itemRangeWithItems, ref itemRangeWithItems );

      var newRangeWithItems = new SelectionCellRangeWithItems( itemRangeWithItems.Range, itemRangeWithItems.Items, rangeWithItems.ColumnRange );

      this.Insert( this.Count, newRangeWithItems, true );
    }

    public void Clear()
    {
      m_ranges.Clear();
      m_map.Clear();
    }

    public bool Contains( int itemIndex, int columnIndex )
    {
      if( ( itemIndex < 0 ) || ( itemIndex >= int.MaxValue ) || ( columnIndex < 0 ) || ( columnIndex >= int.MaxValue ) )
        return false;

      return this.Contains( new SelectionCellRange( itemIndex, columnIndex ) );
    }

    public bool Contains( SelectionCellRange range )
    {
      if( ( m_ranges.Count <= 0 ) || ( SelectedCellsStorage.IsEmpty( range ) ) )
        return false;

      return this.Contains( new SelectionCellRangeWithItems( range.ItemRange, null, range.ColumnRange ) );
    }

    public bool Contains( SelectionCellRangeWithItems rangeWithItems )
    {
      if( ( m_ranges.Count <= 0 ) || ( SelectedCellsStorage.IsEmpty( rangeWithItems ) ) )
        return false;

      if( rangeWithItems.Length == 1 )
        return this.IndexOfOverlap( rangeWithItems ).Any();

      var store = new SelectedCellsStorage( null );
      store.Add( rangeWithItems );

      foreach( var match in this.IndexOfOverlap( rangeWithItems ) )
      {
        store.Remove( m_ranges[ match ].Value );

        if( store.Count == 0 )
          return true;
      }

      return false;
    }

    public bool Remove( SelectionCellRangeWithItems rangeWithItems )
    {
      if( SelectedCellsStorage.IsEmpty( rangeWithItems ) )
        return true;

      // It improves performance to leave early and prevent empty enumerators creation.
      if( m_ranges.Count <= 0 )
        return false;

      // The SelectionCellRange.IsEmpty should be mapped to the item range's SelectionRange.IsEmpty property.
      Debug.Assert( !rangeWithItems.ItemRange.IsEmpty );

      var matches = this.IndexOfOverlap( rangeWithItems ).OrderBy( index => index ).ToList();
      var rangeToRemove = rangeWithItems.CellRange;

      for( int i = matches.Count - 1; i >= 0; i-- )
      {
        var index = matches[ i ];
        var currentRangeWithItems = m_ranges[ index ].Value;
        var currentRange = currentRangeWithItems.CellRange;
        var overlap = rangeToRemove.Intersect( currentRange );

        Debug.Assert( !overlap.IsEmpty );

        var newRanges = currentRange.Exclude( overlap );

        if( newRanges.Length == 0 )
        {
          this.RemoveAt( index, true );
        }
        else
        {
          var currentRangeItems = currentRangeWithItems.ItemRangeWithItems;

          this[ index ] = new SelectionCellRangeWithItems( newRanges[ 0 ].ItemRange, currentRangeItems.GetItems( newRanges[ 0 ].ItemRange ), newRanges[ 0 ].ColumnRange );

          if( newRanges.Length > 1 )
          {
            for( int j = 1; j < newRanges.Length; j++ )
            {
              this.Insert( index + j, new SelectionCellRangeWithItems( newRanges[ j ].ItemRange, currentRangeItems.GetItems( newRanges[ j ].ItemRange ), newRanges[ j ].ColumnRange ), false );
            }

            this.RepairIndex( index + 1 );
          }
        }
      }

      return ( matches.Count > 0 );
    }

    public void OffsetIndex( int startIndex, int offset )
    {
      if( m_map.Count == 0 )
        return;

      // Used to offset index after an add or remove from the data source of the grid.
      var offsetRange = new SelectionRange( startIndex, startIndex + Math.Abs( offset ) - 1 );

      var entries = m_map.GetEntriesWithin( new RSTree2D<SelectionCellRangeWithItemsWrapper>.Area( startIndex, int.MaxValue - startIndex, 0, int.MaxValue ) ).OrderByDescending( e => e.Item.Index ).ToList();

      // Adjust the range of all ranges greater or that overlaps the target range.
      foreach( var entry in entries )
      {
        var currentRangeWithItemsWrapper = entry.Item;
        var currentRangeIndex = currentRangeWithItemsWrapper.Index;
        var currentRangeWithItems = currentRangeWithItemsWrapper.Value;
        var currentRange = currentRangeWithItems.ItemRange;
        var currentRangeItems = currentRangeWithItems.ItemRangeWithItems.Items;

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
              this[ currentRangeIndex ] = new SelectionCellRangeWithItems(
                                            new SelectionRange( currentRange.StartIndex + offset, currentRange.EndIndex + offset ),
                                            currentRangeItems,
                                            currentRangeWithItems.ColumnRange );
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
              this[ currentRangeIndex ] = new SelectionCellRangeWithItems(
                                            new SelectionRange( currentRange.StartIndex + offset, currentRange.EndIndex + offset ),
                                            currentRangeItems,
                                            currentRangeWithItems.ColumnRange );
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

          this[ currentRangeIndex ] = new SelectionCellRangeWithItems( topRange, topItems, currentRangeWithItems.ColumnRange );
          this.Insert( currentRangeIndex + 1, new SelectionCellRangeWithItems( bottomRange, bottomItems, currentRangeWithItems.ColumnRange ), true );
        }
        // The range is greater.
        else
        {
          this[ currentRangeIndex ] = new SelectionCellRangeWithItems(
                                        new SelectionRange( currentRange.StartIndex + offset, currentRange.EndIndex + offset ),
                                        currentRangeItems,
                                        currentRangeWithItems.ColumnRange );
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
        var rangeItems = rangeWithItems.ItemRangeWithItems.Items;

        if( rangeItems == null )
          throw new InvalidOperationException( "We should have items to find the new index." );

        var item = rangeItems[ 0 ];
        var range = rangeWithItems.ItemRange;
        var startIndex = range.StartIndex;

        if( maxOffset < 0 )
        {
          for( int j = 0; j >= maxOffset; j-- )
          {
            if( object.Equals( sourceItems.GetItemAt( startIndex ), item ) )
            {
              if( j != 0 )
              {
                this[ i ] = new SelectionCellRangeWithItems( new SelectionRange( range.StartIndex + j, range.EndIndex + j ), rangeItems, rangeWithItems.ColumnRange );
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
                this[ i ] = new SelectionCellRangeWithItems( new SelectionRange( range.StartIndex + j, range.EndIndex + j ), rangeItems, rangeWithItems.ColumnRange );
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

    public IEnumerable<SelectionRange> GetIntersectedColumnRanges( SelectionCellRange range )
    {
      // It improves performance to leave early and prevent empty enumerators creation.
      if( m_ranges.Count <= 0 )
        return Enumerable.Empty<SelectionRange>();

      return ( from match in this.IndexOfOverlap( range )
               let current = m_ranges[ match ].Value
               let intersection = current.CellRange.Intersect( range )
               select intersection.ColumnRange ).ToList();
    }

    public IEnumerable<SelectionCellRangeWithItems> GetIntersectedCellRangesWithItems( SelectionCellRange range )
    {
      // It improves performance to leave early and prevent empty enumerators creation.
      if( m_ranges.Count <= 0 )
        return Enumerable.Empty<SelectionCellRangeWithItems>();

      return ( from match in this.IndexOfOverlap( range )
               let current = m_ranges[ match ].Value
               let intersection = current.CellRange.Intersect( range )
               select new SelectionCellRangeWithItems(
                 intersection.ItemRange,
                 current.ItemRangeWithItems.GetItems( intersection.ItemRange ),
                 intersection.ColumnRange ) ).ToList();
    }

    private static RSTree2D<SelectionCellRangeWithItemsWrapper>.Area GetArea( SelectionCellRange range )
    {
      if( range.IsEmpty )
        return RSTree2D<SelectionCellRangeWithItemsWrapper>.Area.Empty;

      var itemRange = range.ItemRange;
      var columnRange = range.ColumnRange;
      int rs, re, cs, ce;

      if( itemRange.StartIndex <= itemRange.EndIndex )
      {
        rs = itemRange.StartIndex;
        re = itemRange.EndIndex;
      }
      else
      {
        rs = itemRange.EndIndex;
        re = itemRange.StartIndex;
      }

      if( columnRange.StartIndex <= columnRange.EndIndex )
      {
        cs = columnRange.StartIndex;
        ce = columnRange.EndIndex;
      }
      else
      {
        cs = columnRange.EndIndex;
        ce = columnRange.StartIndex;
      }

      return new RSTree2D<SelectionCellRangeWithItemsWrapper>.Area( rs, re - rs + 1, cs, ce - cs + 1 );
    }

    private static RSTree2D<SelectionCellRangeWithItemsWrapper>.Area GetArea( SelectionCellRangeWithItems range )
    {
      return SelectedCellsStorage.GetArea( range.CellRange );
    }

    private void Insert( int index, SelectionCellRangeWithItems item, bool repairIndex )
    {
      var wrapper = new SelectionCellRangeWithItemsWrapper( item, index );

      m_ranges.Insert( index, wrapper );

      if( repairIndex )
      {
        this.RepairIndex( index + 1 );
      }

      if( !SelectedCellsStorage.IsEmpty( item ) )
      {
        m_map.Add( SelectedCellsStorage.GetArea( item ), wrapper );
      }
    }

    private void RemoveAt( int index, bool repairIndex )
    {
      var wrapper = m_ranges[ index ];
      var rangeWithItems = wrapper.Value;

      Debug.Assert( wrapper.Index == index );

      if( !SelectedCellsStorage.IsEmpty( rangeWithItems ) )
      {
        var removed = m_map.Remove( SelectedCellsStorage.GetArea( rangeWithItems ), wrapper );
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

    private IEnumerable<int> IndexOfOverlap( SelectionCellRange range )
    {
      // It improves performance to leave early.
      if( m_map.Count <= 0 )
        yield break;

      foreach( var entry in m_map.GetEntriesWithin( SelectedCellsStorage.GetArea( range ) ) )
      {
        var candidate = entry.Item;
        var target = candidate.Value;
        var overlap = range.Intersect( target.CellRange );

        Debug.Assert( !SelectedCellsStorage.IsEmpty( overlap ) );

        if( !SelectedCellsStorage.IsEmpty( overlap ) )
          yield return candidate.Index;
      }
    }

    private IEnumerable<int> IndexOfOverlap( SelectionCellRangeWithItems target )
    {
      var targetRange = target.CellRange;
      var targetRangeWithItems = target.ItemRangeWithItems;
      var targetItemRange = targetRange.ItemRange;

      foreach( var match in this.IndexOfOverlap( targetRange ) )
      {
        var currentRangeWithItems = m_ranges[ match ].Value;
        var overlap = targetItemRange.Intersect( currentRangeWithItems.ItemRange );

        Debug.Assert( !overlap.IsEmpty );

        if( targetRangeWithItems.IsItemsEqual( overlap, currentRangeWithItems.ItemRangeWithItems ) )
          yield return match;
      }
    }

    private static bool IsEmpty( SelectionCellRange range )
    {
      return ( range.ItemRange.IsEmpty )
          || ( range.ColumnRange.IsEmpty );
    }

    private static bool IsEmpty( SelectionCellRangeWithItems rangeWithItems )
    {
      return ( rangeWithItems.ItemRange.IsEmpty )
          || ( rangeWithItems.ColumnRange.IsEmpty );
    }

    #region ICloneable Members

    public object Clone()
    {
      var copy = new SelectedCellsStorage( m_dataGridContext );

      for( int i = 0; i < m_ranges.Count; i++ )
      {
        copy.Insert( i, m_ranges[ i ].Value, false );
      }

      return copy;
    }

    #endregion

    #region IEnumerable<SelectionCellRangeWithItems> Members

    public IEnumerator<SelectionCellRangeWithItems> GetEnumerator()
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

    private readonly List<SelectionCellRangeWithItemsWrapper> m_ranges = new List<SelectionCellRangeWithItemsWrapper>();
    private readonly RSTree2D<SelectionCellRangeWithItemsWrapper> m_map = new RSTree2D<SelectionCellRangeWithItemsWrapper>();

    #region SelectionCellRangeWithItemsWrapper Private Class

    // We are wrapping the SelectionCellRangeWithItems structure inside a class so our collections
    // may target the same object.
    private sealed class SelectionCellRangeWithItemsWrapper
    {
      internal SelectionCellRangeWithItemsWrapper( SelectionCellRangeWithItems value, int index )
      {
        m_value = value;
        m_index = index;
      }

      internal SelectionCellRangeWithItems Value
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

      private readonly SelectionCellRangeWithItems m_value;
      private int m_index;
    }

    #endregion
  }
}
