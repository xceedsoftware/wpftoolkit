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

namespace Xceed.Wpf.DataGrid
{
  internal struct SelectionRangeWithItems
  {
    public static readonly SelectionRangeWithItems Empty = new SelectionRangeWithItems( new SelectionRange( -1 ), null );

    public SelectionRangeWithItems( int itemIndex, object item )
      : this( new SelectionRange( itemIndex ), new object[] { item } )
    {
    }

    public SelectionRangeWithItems( SelectionRange range, object[] items )
    {
      if( ( items != null ) && ( items.Length != range.Length ) && ( !range.IsEmpty ) )
        throw new ArgumentException( "selectionRange and items must have the same length." );

      m_items = ( items != null ) ? new OptimizedItemsList( items ) : null;
      m_range = range;
    }

    internal SelectionRangeWithItems( SelectionRange range, SharedList items )
    {
      if( ( items.Count != range.Length ) && ( !range.IsEmpty ) )
        throw new ArgumentException( "selectionRange and items must have the same length." );

      m_items = new OptimizedItemsList( items );
      m_range = range;
    }

    #region SelectionRange Property

    public SelectionRange Range
    {
      get
      {
        return m_range;
      }
    }

    private SelectionRange m_range;

    #endregion

    #region Items Property

    public object[] Items
    {
      get
      {
        if( m_items == null )
          return null;

        return m_items.ItemsArray;
      }
    }

    private OptimizedItemsList m_items;

    #endregion

    #region ItemsList Property

    internal SharedList? SharedList
    {
      get
      {
        if( m_items == null )
          return null;

        return m_items.SharedList;
      }
    }

    #endregion

    #region Length Property

    public int Length
    {
      get
      {
        return m_range.Length;
      }
    }

    #endregion

    #region IsEmpty Property

    public bool IsEmpty
    {
      get
      {
        return m_range.IsEmpty;
      }
    }

    #endregion

    public static bool operator ==( SelectionRangeWithItems rangeWithItems1, SelectionRangeWithItems rangeWithItems2 )
    {
      if( rangeWithItems1.Range != rangeWithItems2.Range )
        return false;

      return rangeWithItems1.IsItemsEqual( rangeWithItems1.Range, rangeWithItems2 );
    }

    public static bool operator !=( SelectionRangeWithItems rangeWithItems1, SelectionRangeWithItems rangeWithItems2 )
    {
      if( rangeWithItems1.Range != rangeWithItems2.Range )
        return true;

      return !rangeWithItems1.IsItemsEqual( rangeWithItems1.Range, rangeWithItems2 );
    }

    public bool IsItemsEqual( SelectionRange rangeIntersection, SelectionRangeWithItems rangeWithItemsToCompare )
    {
      OptimizedItemsList itemsToCompare = rangeWithItemsToCompare.m_items;

      if( ( m_items == null ) || ( itemsToCompare == null ) )
        return true;

      SelectionRange rangeToCompare = rangeWithItemsToCompare.Range;
      int startIndex = Math.Min( rangeIntersection.StartIndex, rangeIntersection.EndIndex );
      int itemIndex1 = Math.Abs( startIndex - m_range.StartIndex );
      int itemIndex2 = Math.Abs( startIndex - rangeToCompare.StartIndex );
      bool inversedRange1 = m_range.StartIndex > m_range.EndIndex;
      bool inversedRange2 = rangeToCompare.StartIndex > rangeToCompare.EndIndex;
      int count = rangeIntersection.Length;

      for( int i = 0; i < count; i++ )
      {
        if( !object.Equals( m_items[ itemIndex1 ], itemsToCompare[ itemIndex2 ] ) )
          return false;

        itemIndex1 = inversedRange1 ? itemIndex1 - 1 : itemIndex1 + 1;
        itemIndex2 = inversedRange2 ? itemIndex2 - 1 : itemIndex2 + 1;
      }

      return true;
    }

    public object GetItem( DataGridContext dataGridContext, int offset )
    {
      if( m_items != null )
        return m_items[ offset ];

      int rangeStart = m_range.StartIndex;
      int rangeEnd = m_range.EndIndex;

      if( rangeStart > rangeEnd )
      {
        return dataGridContext.Items.GetItemAt( rangeStart - offset );
      }
      else
      {
        return dataGridContext.Items.GetItemAt( rangeStart + offset );
      }
    }

    public object[] GetItems( SelectionRange range )
    {
      if( m_items == null )
        return null;

      if( range.IsEmpty )
        return new object[ 0 ];

      int startOffset;
      bool reverseOrder;

      SelectionRange rangeIntersection = m_range.Intersect( range );

      if( m_range.StartIndex > m_range.EndIndex )
      {
        startOffset = m_range.StartIndex - rangeIntersection.StartIndex;
        reverseOrder = range.StartIndex < range.EndIndex;
      }
      else
      {
        startOffset = rangeIntersection.StartIndex - m_range.StartIndex;
        reverseOrder = range.StartIndex > range.EndIndex;
      }

      object[] items = new object[ rangeIntersection.Length ];
      m_items.CopyItemsToArray( startOffset, items, 0, items.Length );

      if( reverseOrder )
      {
        Array.Reverse( items );
      }

      return items;
    }

    public override int GetHashCode()
    {
      return m_range.GetHashCode();
    }

    public override bool Equals( object obj )
    {
      if( !( obj is SelectionRangeWithItems ) )
        return false;

      return ( ( SelectionRangeWithItems )obj ) == this;
    }

    #region OptimizedItemsList Private Class

    // Performance optimization.
    // This wrapper class allows to store items as an array (exclusive)OR as a SharedList.
    // This allows a uniform API to access the items, whether they are stored as an Array or as a SharedList. 
    //
    // The SharedList allows us to be more efficient when we need to add items to the existing ones. 
    // The Array allows the return of a value to the SelectionRangeWithItems.Items property more efficiently.
    private class OptimizedItemsList
    {
      public OptimizedItemsList( SharedList list )
      {
        m_itemsArray = null;
        m_itemsList = list;
      }

      public OptimizedItemsList( object[] items )
      {
        if( items == null )
          throw new ArgumentNullException( "items" );

        m_itemsArray = items;
        m_itemsList = null;
      }

      public object this[ int index ]
      {
        get
        {
          if( m_itemsArray != null )
          {
            return m_itemsArray[ index ];
          }
          else
          {
            Debug.Assert( m_itemsList != null );
            return m_itemsList.Value[ index ];
          }
        }
      }

      public SharedList? SharedList
      {
        get
        {
          return m_itemsList;
        }
      }

      public object[] ItemsArray
      {
        get
        {
          //Convert the list to an array if needed.
          if( m_itemsList != null )
          {
            Debug.Assert( m_itemsArray == null );
            m_itemsArray = m_itemsList.Value.ToArray();
            m_itemsList = null;
          }

          return m_itemsArray;
        }
      }

      public void CopyItemsToArray( int index, object[] array, int arrayIndex, int length )
      {
        if( m_itemsArray != null )
        {
          Array.Copy( m_itemsArray, index, array, arrayIndex, length );
        }
        else
        {
          Debug.Assert( m_itemsList != null );
          m_itemsList.Value.CopyTo( index, array, arrayIndex, length );
        }
      }

      private object[] m_itemsArray;
      private SharedList? m_itemsList;
    }

    #endregion
  }
}
