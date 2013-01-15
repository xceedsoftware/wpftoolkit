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

      m_items = items;
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

    #endregion SelectionRange Property

    #region Items Property

    public object[] Items
    {
      get
      {
        return m_items;
      }
    }

    private object[] m_items;

    #endregion Items Property

    #region Length Property

    public int Length
    {
      get
      {
        return m_range.Length;
      }
    }

    #endregion Length Property

    #region PUBLIC PROPERTIES

    public bool IsEmpty
    {
      get
      {
        return m_range.IsEmpty;
      }
    }

    #endregion PUBLIC PROPERTIES

    #region PUBLIC METHODS

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
      object[] itemsToCompare = rangeWithItemsToCompare.m_items;

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
      Array.Copy( m_items, startOffset, items, 0, items.Length );

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

    #endregion PUBLIC METHODS
  }
}
