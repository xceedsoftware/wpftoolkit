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

namespace Xceed.Wpf.DataGrid
{
  internal struct SelectionCellRangeWithItems
  {
    public static readonly SelectionCellRangeWithItems Empty = new SelectionCellRangeWithItems( 
      SelectionRange.Empty, null, SelectionRange.Empty );

    public SelectionCellRangeWithItems( int itemIndex, object item, int columnIndex )
      : this( new SelectionRange( itemIndex ), new object[] { item }, new SelectionRange( columnIndex ) )
    {
    }

    public SelectionCellRangeWithItems( SelectionRange itemRange, object[] items, SelectionRange columnRange )
    {
      if( ( items != null ) && ( items.Length != itemRange.Length ) ) 
        throw new ArgumentException( "itemRange and items must have the same length." );

      m_itemRangeWithItems = new SelectionRangeWithItems( itemRange, items );
      m_columnRange = columnRange;
    }

    #region ItemRangeWithItems Property

    public SelectionRangeWithItems ItemRangeWithItems
    {
      get
      {
        return m_itemRangeWithItems;
      }
    }

    SelectionRangeWithItems m_itemRangeWithItems;

    #endregion ItemRangeWithItems Property

    #region ItemRange Property

    public SelectionRange ItemRange
    {
      get
      {
        return m_itemRangeWithItems.Range;
      }
    }

    #endregion ItemRange Property

    #region ColumnRange Property

    public SelectionRange ColumnRange
    {
      get
      {
        return m_columnRange;
      }
    }

    SelectionRange m_columnRange;

    #endregion ColumnRange Property

    #region CellRange Property

    public SelectionCellRange CellRange
    {
      get
      {
        return new SelectionCellRange( m_itemRangeWithItems.Range, m_columnRange );
      }
    }

    #endregion CellRange Property

    #region Length Property

    public int Length
    {
      get
      {
        return m_itemRangeWithItems.Range.Length * m_columnRange.Length;
      }
    }

    #endregion Length Property

    #region PUBLIC METHODS

    public static bool operator ==( SelectionCellRangeWithItems rangeWithItems1, SelectionCellRangeWithItems rangeWithItems2 )
    {
      return
        ( rangeWithItems1.m_columnRange == rangeWithItems2.m_columnRange )
        && ( rangeWithItems1.m_itemRangeWithItems == rangeWithItems2.m_itemRangeWithItems );
    }

    public static bool operator !=( SelectionCellRangeWithItems rangeWithItems1, SelectionCellRangeWithItems rangeWithItems2 )
    {
      return
        ( rangeWithItems1.m_columnRange != rangeWithItems2.m_columnRange )
        || ( rangeWithItems1.m_itemRangeWithItems != rangeWithItems2.m_itemRangeWithItems );
    }

    public override int GetHashCode()
    {
      return ( m_itemRangeWithItems.GetHashCode() ^ m_columnRange.GetHashCode() );
    }

    public override bool Equals( object obj )
    {
      if( !( obj is SelectionCellRangeWithItems ) )
        return false;

      return ( ( SelectionCellRangeWithItems )obj ) == this;
    }

    #endregion PUBLIC METHODS
  }
}
