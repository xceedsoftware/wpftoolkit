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
  internal class SelectedItemsStorage : ICloneable, IEnumerable<SelectionRangeWithItems>, IEnumerable
  {
    #region Constructors

    internal SelectedItemsStorage( DataGridContext dataGridContext )
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
        return m_unsortedRanges[ index ].Value;
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
      m_unsortedRanges.Clear();
      m_sortedRanges.Clear();
    }

    public SelectionRange[] ToSelectionRangeArray()
    {
      return ( from item in m_unsortedRanges
               select item.Value.Range ).ToArray();
    }

    public bool Contains( int itemIndex )
    {
      return this.Contains( new SelectionRange( itemIndex ) );
    }

    public bool Contains( SelectionRange range )
    {
      if( range.IsEmpty )
        return false;

      return ( this.FindIndex( new RangeComparer( range ) ) >= 0 );
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
      if( m_sortedRanges.Count == 0 )
        return;

      // Used to offset index after an add or remove from the data source of the grid.
      var offsetRange = new SelectionRange( startIndex, startIndex + Math.Abs( offset ) - 1 );
      var comparer = new RangeComparer( offsetRange );

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

    private void Insert( int index, SelectionRangeWithItems item, bool repairIndex )
    {
      var wrapper = new SelectionRangeWithItemsWrapper( item, index );

      m_unsortedRanges.Insert( index, wrapper );

      if( repairIndex )
      {
        this.RepairIndex( index + 1 );
      }

      if( !item.IsEmpty )
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

      if( !wrapper.Value.IsEmpty )
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
      if( rangeWithItems.IsEmpty )
        yield break;

      var targetRange = rangeWithItems.Range;
      var comparer = new RangeComparer( targetRange );

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
        var overlap = targetRange.Intersect( currentRangeWithItems.Range );

        if( !overlap.IsEmpty && ( rangeWithItems.IsItemsEqual( overlap, currentRangeWithItems ) ) )
          yield return wrapper.Index;
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

    #region ICloneable Members

    public object Clone()
    {
      var copy = new SelectedItemsStorage( m_dataGridContext );

      for( int i = 0; i < m_unsortedRanges.Count; i++ )
      {
        copy.Insert( i, m_unsortedRanges[ i ].Value, false );
      }

      copy.m_itemsCount = m_itemsCount;

      return copy;
    }

    #endregion

    #region IEnumerable<SelectionRangeWithItems> Members

    public IEnumerator<SelectionRangeWithItems> GetEnumerator()
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

    private readonly List<SelectionRangeWithItemsWrapper> m_unsortedRanges = new List<SelectionRangeWithItemsWrapper>();
    private readonly List<SelectionRangeWithItemsWrapper> m_sortedRanges = new List<SelectionRangeWithItemsWrapper>();

    #endregion

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

    #region ISelectionRangeComparer Private Interface

    private interface ISelectionRangeComparer
    {
      int Compare( SelectionRangeWithItemsWrapper item );
    }

    #endregion

    #region ItemComparer Private Class

    private sealed class ItemComparer : ISelectionRangeComparer
    {
      internal ItemComparer( SelectionRangeWithItemsWrapper target )
      {
        if( target == null )
          throw new ArgumentNullException( "target" );

        if( target.Value.IsEmpty )
          throw new ArgumentException( "The selection range must not be empty.", "target" );

        m_target = target;
      }

      public int Compare( SelectionRangeWithItemsWrapper item )
      {
        if( item == m_target )
          return 0;

        var xr = m_target.Value.Range;
        var yr = item.Value.Range;

        int xs, xe, ys, ye;
        ItemComparer.GetBounds( xr, out xs, out xe );
        ItemComparer.GetBounds( yr, out ys, out ye );

        if( xs < ys )
        {
          Debug.Assert( RangeComparer.Compare( xr, item ) <= 0 );
          return -1;
        }
        else if( xs > ys )
        {
          Debug.Assert( RangeComparer.Compare( xr, item ) >= 0 );
          return 1;
        }

        Debug.Assert( RangeComparer.Compare( xr, item ) == 0 );

        if( xe < ye )
        {
          return -1;
        }
        else if( xe > ye )
        {
          return 1;
        }

        Debug.Assert( xr == yr );

        return ( m_target.Index - item.Index );
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

      private readonly SelectionRangeWithItemsWrapper m_target;
    }

    #endregion

    #region RangeComparer Private Class

    private sealed class RangeComparer : ISelectionRangeComparer
    {
      internal RangeComparer( SelectionRange target )
      {
        if( target.IsEmpty )
          throw new ArgumentException( "The selection range must not be empty.", "target" );

        m_target = target;
      }

      public int Compare( SelectionRangeWithItemsWrapper item )
      {
        return RangeComparer.Compare( m_target, item );
      }

      internal static int Compare( SelectionRange range, SelectionRangeWithItemsWrapper wrapper )
      {
        var itemRange = wrapper.Value.Range;

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
