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
  public struct SelectionCellRange
  {
    public static readonly SelectionCellRange Empty = new SelectionCellRange( -1, -1 );

    #region CONSTRUCTORS

    public SelectionCellRange( int itemIndex, int columnIndex )
    {
      if( ( itemIndex == -1 || columnIndex == -1 ) && ( itemIndex != -1 || columnIndex != -1 ) )
        throw new ArgumentException( "itemIndex and columnIndex must be equal if the value of one is -1." );

      if( ( itemIndex < -1 ) || ( itemIndex >= int.MaxValue ) )
        throw new ArgumentOutOfRangeException( "itemIndex", "itemIndex must be equal to or greater than -1 and lower than int.MaxValue." );

      if( ( columnIndex < -1 ) || ( columnIndex >= int.MaxValue ) )
        throw new ArgumentOutOfRangeException( "columnIndex", "columnIndex must be equal to or greater than -1 and lower than int.MaxValue." );

      m_itemRange = new SelectionRange( itemIndex );
      m_columnRange = new SelectionRange( columnIndex );
    }

    public SelectionCellRange( int itemStartIndex, int columnStartIndex, int itemEndIndex, int columnEndIndex )
    {
      if( ( itemStartIndex < 0 ) || ( itemStartIndex >= int.MaxValue ) )
        throw new ArgumentOutOfRangeException( "itemStartIndex", "itemStartIndex must be equal to or greater than zero and lower than int.MaxValue." );

      if( ( itemEndIndex < 0 ) || ( itemEndIndex >= int.MaxValue ) )
        throw new ArgumentOutOfRangeException( "itemEndIndex", "itemEndIndex must be equal to or greater than zero and lower than int.MaxValue." );

      if( ( columnStartIndex < 0 ) || ( columnStartIndex >= int.MaxValue ) )
        throw new ArgumentOutOfRangeException( "columnStartIndex", "columnStartIndex must be equal to or greater than zero and lower than int.MaxValue." );

      if( ( columnEndIndex < 0 ) || ( columnEndIndex >= int.MaxValue ) )
        throw new ArgumentOutOfRangeException( "columnEndIndex", "columnEndIndex must be equal to or greater than zero and lower than int.MaxValue." );

      m_itemRange = new SelectionRange( itemStartIndex, itemEndIndex );
      m_columnRange = new SelectionRange( columnStartIndex, columnEndIndex );
    }

    public SelectionCellRange( SelectionRange itemRange, SelectionRange columnRange )
    {
      m_itemRange = itemRange;
      m_columnRange = columnRange;
    }

    #endregion

    #region ItemRange Property

    public SelectionRange ItemRange
    {
      get
      {
        return m_itemRange;
      }
      set
      {
        m_itemRange = value;
      }
    }

    private SelectionRange m_itemRange;

    #endregion

    #region ColumnRange Property

    public SelectionRange ColumnRange
    {
      get
      {
        return m_columnRange;
      }
      set
      {
        m_columnRange = value;
      }
    }

    private SelectionRange m_columnRange;

    #endregion

    #region Length Property

    public int Length
    {
      get
      {
        return m_itemRange.Length * m_columnRange.Length;
      }
    }

    #endregion

    #region IsEmpty Property

    public bool IsEmpty
    {
      get
      {
        return m_itemRange.IsEmpty;
      }
    }

    #endregion

    public static bool operator ==( SelectionCellRange range1, SelectionCellRange range2 )
    {
      return range1.Equals( range2 );
    }

    public static bool operator !=( SelectionCellRange range1, SelectionCellRange range2 )
    {
      return !range1.Equals( range2 );
    }

    public SelectionCellRange Intersect( SelectionCellRange range )
    {
      SelectionCellRange cellRangeIntersection = SelectionCellRange.Empty;

      SelectionRange itemRange = range.ItemRange;
      SelectionRange itemRangeIntersection = m_itemRange.Intersect( itemRange );

      if( itemRangeIntersection.IsEmpty )
        return SelectionCellRange.Empty;

      SelectionRange columnRange = range.ColumnRange;
      SelectionRange columnRangeIntersection = m_columnRange.Intersect( columnRange );

      if( columnRangeIntersection.IsEmpty )
        return SelectionCellRange.Empty;

      return new SelectionCellRange( itemRangeIntersection, columnRangeIntersection );
    }

    public SelectionCellRange[] Exclude( SelectionCellRange cellRangeToExclude )
    {
      if( cellRangeToExclude.IsEmpty )
        return new SelectionCellRange[] { this };

      SelectionCellRange cellRangeIntersection = this.Intersect( cellRangeToExclude );

      if( cellRangeIntersection.IsEmpty )
        return new SelectionCellRange[] { this };

      SelectionRange[] itemRanges = m_itemRange.Exclude( cellRangeToExclude.ItemRange );
      SelectionRange[] columnRanges = m_columnRange.Exclude( cellRangeToExclude.ColumnRange );
      SelectionCellRange[] cellRanges = new SelectionCellRange[ itemRanges.Length + columnRanges.Length ];
      int index = 0;

      foreach( SelectionRange itemRange in itemRanges )
      {
        cellRanges[ index ] = new SelectionCellRange( itemRange, m_columnRange );
        index++;
      }

      foreach( SelectionRange columnRange in columnRanges )
      {
        cellRanges[ index ] = new SelectionCellRange( cellRangeIntersection.ItemRange, columnRange );
        index++;
      }

      Debug.Assert( index == cellRanges.Length );
      return cellRanges;
    }

    public override bool Equals( object obj )
    {
      if( !( obj is SelectionCellRange ) )
        return false;

      SelectionCellRange selectionRange = ( SelectionCellRange )obj;

      return ( selectionRange.m_itemRange == m_itemRange )
        && ( selectionRange.m_columnRange == m_columnRange );
    }

    public override int GetHashCode()
    {
      return ( m_itemRange.GetHashCode() ^ m_columnRange.GetHashCode() );
    }
  }
}
