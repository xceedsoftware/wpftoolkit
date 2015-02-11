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

namespace Xceed.Wpf.DataGrid
{
  internal class SelectedCellsStorage : ICloneable, IEnumerable<SelectionCellRangeWithItems>, IEnumerable
  {
    #region Constructors

    internal SelectedCellsStorage( DataGridContext dataGridContext )
    {
      // dataGridContext can be null for store that we don't want to auto-get Items from index
      m_dataGridContext = dataGridContext;
    }

    #endregion

    #region Count Property

    public int Count
    {
      get
      {
        return m_unsortedRanges.Count;
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
        return m_unsortedRanges[ index ].Value;
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
      Debug.Assert( m_unsortedRanges.All( r => r.Value.CellRange.Intersect( rangeWithItems.CellRange ).IsEmpty ), "Part of this range is already selected" );

      var itemRangeWithItems = rangeWithItems.ItemRangeWithItems;

      SelectedItemsStorage.UpdateSelectionRangeWithItemsFromAdd( m_dataGridContext, itemRangeWithItems, ref itemRangeWithItems );

      var newRangeWithItems = new SelectionCellRangeWithItems( itemRangeWithItems.Range, itemRangeWithItems.Items, rangeWithItems.ColumnRange );

      this.Insert( this.Count, newRangeWithItems, true );
    }

    public void Clear()
    {
      m_unsortedRanges.Clear();
      m_sortedRanges.Clear();
    }

    public bool Contains( int itemIndex, int columnIndex )
    {
      if( ( itemIndex < 0 ) || ( columnIndex < 0 ) )
        return false;

      return this.Contains( new SelectionCellRange( itemIndex, columnIndex ) );
    }

    public bool Contains( SelectionCellRange range )
    {
      if( ( m_unsortedRanges.Count <= 0 ) || ( SelectedCellsStorage.IsEmpty( range ) ) )
        return false;

      return this.Contains( new SelectionCellRangeWithItems( range.ItemRange, null, range.ColumnRange ) );
    }

    public bool Contains( SelectionCellRangeWithItems rangeWithItems )
    {
      if( ( m_unsortedRanges.Count <= 0 ) || ( SelectedCellsStorage.IsEmpty( rangeWithItems ) ) )
        return false;

      if( rangeWithItems.Length == 1 )
        return this.IndexOfOverlap( rangeWithItems ).Any();

      var store = new SelectedCellsStorage( null );
      store.Add( rangeWithItems );

      foreach( var match in this.IndexOfOverlap( rangeWithItems ) )
      {
        store.Remove( m_unsortedRanges[ match ].Value );

        if( store.Count == 0 )
          return true;
      }

      return false;
    }

    public bool Remove( SelectionCellRangeWithItems rangeWithItems )
    {
      if( rangeWithItems.CellRange.IsEmpty )
        return true;

      // The SelectionCellRange.IsEmpty should be mapped to the item range's SelectionRange.IsEmpty property.
      Debug.Assert( !rangeWithItems.ItemRange.IsEmpty );

      var matches = this.IndexOfOverlap( rangeWithItems ).OrderBy( index => index ).ToList();
      var rangeToRemove = rangeWithItems.CellRange;

      for( int i = matches.Count - 1; i >= 0; i-- )
      {
        var index = matches[ i ];
        var currentRangeWithItems = m_unsortedRanges[ index ].Value;
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
      if( m_sortedRanges.Count == 0 )
        return;

      // Used to offset index after an add or remove from the data source of the grid.
      var offsetRange = new SelectionRange( startIndex, startIndex + Math.Abs( offset ) - 1 );
      var comparer = new ItemRangeComparer( offsetRange );

      // Find the first range that is greater or that overlaps the target range.
      var index = this.FindIndex( comparer );
      if( index < 0 )
      {
        index = ~index;
      }
      else
      {
        while( index > 0 )
        {
          if( comparer.Compare( m_sortedRanges[ index - 1 ] ) != 0 )
            break;

          index--;
        }
      }

      var matches = m_sortedRanges.Skip( index ).OrderByDescending( item => item.Index ).ToList();

      // Adjust the range of all ranges greater or that overlaps the target range.
      foreach( var currentRangeWithItemsWrapper in matches )
      {
        Debug.Assert( comparer.Compare( currentRangeWithItemsWrapper ) <= 0 );

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
      return ( from match in this.IndexOfOverlap( range )
               let current = m_unsortedRanges[ match ].Value
               let intersection = current.CellRange.Intersect( range )
               select intersection.ColumnRange ).ToList();
    }

    public IEnumerable<SelectionCellRangeWithItems> GetIntersectedCellRangesWithItems( SelectionCellRange range )
    {
      return ( from match in this.IndexOfOverlap( range )
               let current = m_unsortedRanges[ match ].Value
               let intersection = current.CellRange.Intersect( range )
               select new SelectionCellRangeWithItems(
                 intersection.ItemRange,
                 current.ItemRangeWithItems.GetItems( intersection.ItemRange ),
                 intersection.ColumnRange ) ).ToList();
    }

    private void Insert( int index, SelectionCellRangeWithItems item, bool repairIndex )
    {
      var wrapper = new SelectionCellRangeWithItemsWrapper( item, index );

      m_unsortedRanges.Insert( index, wrapper );

      if( repairIndex )
      {
        this.RepairIndex( index + 1 );
      }

      if( !SelectedCellsStorage.IsEmpty( item ) )
      {
        var insertionIndex = ~this.FindIndex( new ItemComparer( wrapper ) );
        Debug.Assert( insertionIndex >= 0 );

        m_sortedRanges.Insert( insertionIndex, wrapper );
      }
    }

    private void RemoveAt( int index, bool repairIndex )
    {
      var wrapper = m_unsortedRanges[ index ];
      Debug.Assert( wrapper.Index == index );

      if( !SelectedCellsStorage.IsEmpty( wrapper.Value ) )
      {
        var removalIndex = this.FindIndex( new ItemComparer( wrapper ) );
        Debug.Assert( removalIndex >= 0 );

        m_sortedRanges.RemoveAt( removalIndex );
      }

      m_unsortedRanges.RemoveAt( index );

      if( repairIndex )
      {
        this.RepairIndex( index );
      }
    }

    private void RepairIndex( int index )
    {
      Debug.Assert( index >= 0 );

      for( int i = index; i < m_unsortedRanges.Count; i++ )
      {
        m_unsortedRanges[ i ].Index = i;
      }
    }

    private IEnumerable<int> IndexOfOverlap( SelectionCellRange target )
    {
      if( ( m_sortedRanges.Count <= 0 ) || SelectedCellsStorage.IsEmpty( target ) )
        yield break;

      var comparer = new ItemRangeComparer( target.ItemRange );

      var index = this.FindIndex( comparer );
      if( index < 0 )
        yield break;

      while( index > 0 )
      {
        if( comparer.Compare( m_sortedRanges[ index - 1 ] ) != 0 )
          break;

        index--;
      }

      for( int i = index; i < m_sortedRanges.Count; i++ )
      {
        var wrapper = m_sortedRanges[ i ];
        if( comparer.Compare( wrapper ) != 0 )
          break;

        var currentRangeWithItems = wrapper.Value;
        var overlap = target.Intersect( currentRangeWithItems.CellRange );

        if( !SelectedCellsStorage.IsEmpty( overlap ) )
          yield return wrapper.Index;
      }
    }

    private IEnumerable<int> IndexOfOverlap( SelectionCellRangeWithItems target )
    {
      var targetRange = target.CellRange;
      var targetRangeWithItems = target.ItemRangeWithItems;
      var targetItemRange = targetRange.ItemRange;

      foreach( var match in this.IndexOfOverlap( targetRange ) )
      {
        var currentRangeWithItems = m_unsortedRanges[ match ].Value;
        var overlap = targetItemRange.Intersect( currentRangeWithItems.ItemRange );

        Debug.Assert( !overlap.IsEmpty );

        if( targetRangeWithItems.IsItemsEqual( overlap, currentRangeWithItems.ItemRangeWithItems ) )
          yield return match;
      }
    }

    private int FindIndex( ISelectionRangeComparer comparer )
    {
      Debug.Assert( comparer != null );

      var lowerBound = 0;
      var upperBound = m_sortedRanges.Count - 1;

      while( lowerBound <= upperBound )
      {
        var middle = lowerBound + ( upperBound - lowerBound ) / 2;
        var compare = comparer.Compare( m_sortedRanges[ middle ] );

        if( compare < 0 )
        {
          if( middle == lowerBound )
            return ~middle;

          upperBound = middle - 1;
        }
        else if( compare > 0 )
        {
          if( middle == upperBound )
            return ~( middle + 1 );

          lowerBound = middle + 1;
        }
        else
        {
          return middle;
        }
      }

      return ~0;
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

      for( int i = 0; i < m_unsortedRanges.Count; i++ )
      {
        copy.Insert( i, m_unsortedRanges[ i ].Value, false );
      }

      return copy;
    }

    #endregion

    #region IEnumerable<SelectionCellRangeWithItems> Members

    public IEnumerator<SelectionCellRangeWithItems> GetEnumerator()
    {
      return m_unsortedRanges.Select( item => item.Value ).GetEnumerator();
    }

    #endregion

    #region IEnumerable Members

    IEnumerator IEnumerable.GetEnumerator()
    {
      return this.GetEnumerator();
    }

    #endregion

    #region Private Fields

    private readonly List<SelectionCellRangeWithItemsWrapper> m_unsortedRanges = new List<SelectionCellRangeWithItemsWrapper>();
    private readonly List<SelectionCellRangeWithItemsWrapper> m_sortedRanges = new List<SelectionCellRangeWithItemsWrapper>();

    #endregion

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

    #region ISelectionRangeComparer Private Interface

    private interface ISelectionRangeComparer
    {
      int Compare( SelectionCellRangeWithItemsWrapper item );
    }

    #endregion

    #region ItemComparer Private Class

    private sealed class ItemComparer : ISelectionRangeComparer
    {
      internal ItemComparer( SelectionCellRangeWithItemsWrapper target )
      {
        if( target == null )
          throw new ArgumentNullException( "target" );

        if( SelectedCellsStorage.IsEmpty( target.Value ) )
          throw new ArgumentException( "The selection range must not be empty.", "target" );

        m_target = target;
      }

      public int Compare( SelectionCellRangeWithItemsWrapper item )
      {
        if( m_target == item )
          return 0;

        var xri = m_target.Value;
        var yri = item.Value;

        int compare;

        compare = ItemComparer.Compare( xri.ItemRange, yri.ItemRange );
        if( compare != 0 )
        {
          Debug.Assert( ( ( compare < 0 ) && ItemRangeComparer.Compare( xri.ItemRange, item ) <= 0 )
                     || ( ( compare > 0 ) && ItemRangeComparer.Compare( xri.ItemRange, item ) >= 0 ) );

          return compare;
        }

        Debug.Assert( ItemRangeComparer.Compare( xri.ItemRange, item ) == 0 );

        compare = ItemComparer.Compare( xri.CellRange.ColumnRange, yri.CellRange.ColumnRange );
        if( compare != 0 )
          return compare;

        Debug.Assert( xri.CellRange == yri.CellRange );

        return ( m_target.Index - item.Index );
      }

      private static int Compare( SelectionRange xr, SelectionRange yr )
      {
        int xs, xe, ys, ye;
        ItemComparer.GetBounds( xr, out xs, out xe );
        ItemComparer.GetBounds( yr, out ys, out ye );

        if( xs < ys )
        {
          return -1;
        }
        else if( xs > ys )
        {
          return 1;
        }

        Debug.Assert( !xr.Intersect( yr ).IsEmpty );

        if( xe < ye )
        {
          return -1;
        }
        else if( xe > ye )
        {
          return 1;
        }

        Debug.Assert( xr == yr );
        return 0;
      }

      private static void GetBounds( SelectionRange range, out int startIndex, out int endIndex )
      {
        if( range.StartIndex <= range.EndIndex )
        {
          startIndex = range.StartIndex;
          endIndex = range.EndIndex;
        }
        else
        {
          endIndex = range.StartIndex;
          startIndex = range.EndIndex;
        }
      }

      private readonly SelectionCellRangeWithItemsWrapper m_target;
    }

    #endregion

    #region ItemRangeComparer Private Class

    private sealed class ItemRangeComparer : ISelectionRangeComparer
    {
      internal ItemRangeComparer( SelectionRange target )
      {
        if( target.IsEmpty )
          throw new ArgumentException( "The selection range must not be empty.", "target" );

        m_target = target;
      }

      public int Compare( SelectionCellRangeWithItemsWrapper item )
      {
        return ItemRangeComparer.Compare( m_target, item );
      }

      internal static int Compare( SelectionRange range, SelectionCellRangeWithItemsWrapper wrapper )
      {
        var itemRange = wrapper.Value.ItemRange;

        if( range < itemRange )
          return -1;

        if( range > itemRange )
          return 1;

        Debug.Assert( !range.Intersect( itemRange ).IsEmpty );

        return 0;
      }

      private readonly SelectionRange m_target;
    }

    #endregion
  }
}
