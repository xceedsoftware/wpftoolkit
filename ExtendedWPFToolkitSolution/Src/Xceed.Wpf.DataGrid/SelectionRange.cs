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
  public struct SelectionRange : IComparable<SelectionRange>
  {
    public static readonly SelectionRange Empty = new SelectionRange( -1 );

    #region CONSTRUCTORS

    public SelectionRange( int startIndex, int endIndex )
    {
      if( ( startIndex < 0 ) || ( startIndex >= int.MaxValue ) )
        throw new ArgumentOutOfRangeException( "startIndex", "startIndex must be equal to or greater than zero and lower than int.MaxValue." );

      if( ( endIndex < 0 ) || ( endIndex >= int.MaxValue ) )
        throw new ArgumentOutOfRangeException( "endIndex", "endIndex must be equal to or greater than zero and lower than int.MaxValue." );

      m_startIndex = startIndex;
      m_endIndex = endIndex;
    }

    public SelectionRange( int index )
    {
      // Only place where we accept index = -1
      if( ( index < -1 ) || ( index >= int.MaxValue ) )
        throw new ArgumentOutOfRangeException( "index", "index must be equal to or greater than -1 and lower than int.MaxValue." );

      m_startIndex = index;
      m_endIndex = index;
    }

    #endregion

    #region StartIndex Property

    public int StartIndex
    {
      get
      {
        return m_startIndex;
      }
      set
      {
        if( ( value < 0 ) || ( value >= int.MaxValue ) )
          throw new ArgumentOutOfRangeException( "StartIndex", "StartIndex must be equal to or greater than zero and lower than int.MaxValue." );

        m_startIndex = value;
      }
    }

    private int m_startIndex;

    #endregion

    #region EndIndex Property

    public int EndIndex
    {
      get
      {
        return m_endIndex;
      }
      set
      {
        if( ( value < 0 ) || ( value >= int.MaxValue ) )
          throw new ArgumentOutOfRangeException( "EndIndex", "EndIndex must be equal to or greater than zero and lower than int.MaxValue." );

        m_endIndex = value;
      }
    }

    private int m_endIndex;

    #endregion

    #region Length Property

    public int Length
    {
      get
      {
        if( this.IsEmpty )
          return 0;

        return Math.Abs( m_endIndex - m_startIndex ) + 1;
      }
    }

    #endregion

    #region IsEmpty Property

    public bool IsEmpty
    {
      get
      {
        return ( m_startIndex == -1 ) || ( m_endIndex == -1 );
      }
    }

    #endregion

    public static bool operator >( SelectionRange range1, SelectionRange range2 )
    {
      if( range1.m_startIndex > range1.m_endIndex )
      {
        if( range2.m_startIndex < range2.m_endIndex )
        {
          return range1.m_endIndex > range2.m_endIndex;
        }
        else
        {
          return range1.m_endIndex > range2.m_startIndex;
        }
      }
      else
      {
        if( range2.m_startIndex > range2.m_endIndex )
        {
          return range1.m_startIndex > range2.m_startIndex;
        }
        else
        {
          return range1.m_startIndex > range2.m_endIndex;
        }
      }
    }

    public static bool operator <( SelectionRange range1, SelectionRange range2 )
    {
      if( range1.m_startIndex > range1.m_endIndex )
      {
        if( range2.m_startIndex < range2.m_endIndex )
        {
          return range1.m_startIndex < range2.m_startIndex;
        }
        else
        {
          return range1.m_startIndex < range2.m_endIndex;
        }
      }
      else
      {
        if( range2.m_startIndex > range2.m_endIndex )
        {
          return range1.m_endIndex < range2.m_endIndex;
        }
        else
        {
          return range1.m_endIndex < range2.m_startIndex;
        }
      }
    }

    public static bool operator ==( SelectionRange range1, SelectionRange range2 )
    {
      return range1.Equals( range2 );
    }

    public static bool operator !=( SelectionRange range1, SelectionRange range2 )
    {
      return !range1.Equals( range2 );
    }

    public SelectionRange Intersect( SelectionRange range )
    {
      int newStart;
      int newEnd;

      if( m_startIndex > m_endIndex )
      {
        if( range.m_startIndex < range.m_endIndex )
        {
          int temp = range.m_startIndex;
          range.m_startIndex = range.m_endIndex;
          range.m_endIndex = temp;
        }

        newStart = Math.Min( range.m_startIndex, m_startIndex );
        newEnd = Math.Max( range.m_endIndex, m_endIndex );

        if( newEnd > newStart )
          return SelectionRange.Empty;
      }
      else
      {
        if( range.m_startIndex > range.m_endIndex )
        {
          int temp = range.m_startIndex;
          range.m_startIndex = range.m_endIndex;
          range.m_endIndex = temp;
        }

        newStart = Math.Max( range.m_startIndex, m_startIndex );
        newEnd = Math.Min( range.m_endIndex, m_endIndex );

        if( newStart > newEnd )
          return SelectionRange.Empty;
      }

      return new SelectionRange( newStart, newEnd );
    }

    public SelectionRange[] Exclude( SelectionRange rangeToExclude )
    {
      if( rangeToExclude.IsEmpty )
        return new SelectionRange[] { this };

      var rangeIntersection = this.Intersect( rangeToExclude );
      if( rangeIntersection.IsEmpty )
        return new SelectionRange[] { this };

      if( m_startIndex > m_endIndex )
      {
        Debug.Assert( rangeIntersection.StartIndex >= rangeIntersection.EndIndex );

        if( rangeIntersection.EndIndex > m_endIndex )
        {
          var newRange = this;
          newRange.StartIndex = rangeIntersection.EndIndex - 1;

          if( rangeIntersection.StartIndex < m_startIndex )
          {
            return new SelectionRange[] 
              { 
                new SelectionRange( m_startIndex, rangeIntersection.StartIndex + 1 ),
                newRange 
              };
          }
          else
          {
            return new SelectionRange[] { newRange };
          }
        }
        else
        {
          if( rangeIntersection.StartIndex < m_startIndex )
          {
            var newRange = this;
            newRange.EndIndex = rangeIntersection.StartIndex + 1;
            return new SelectionRange[] { newRange };
          }
        }
      }
      else
      {
        Debug.Assert( rangeIntersection.StartIndex <= rangeIntersection.EndIndex );

        if( rangeIntersection.StartIndex > m_startIndex )
        {
          SelectionRange newRange = this;
          newRange.EndIndex = rangeIntersection.StartIndex - 1;

          if( rangeIntersection.EndIndex < m_endIndex )
          {
            return new SelectionRange[]
              { 
                newRange,
                new SelectionRange( rangeIntersection.EndIndex + 1, m_endIndex )
              };
          }
          else
          {
            return new SelectionRange[] { newRange };
          }
        }
        else
        {
          if( rangeIntersection.EndIndex < m_endIndex )
          {
            SelectionRange newRange = this;
            newRange.StartIndex = rangeIntersection.EndIndex + 1;
            return new SelectionRange[] { newRange };
          }
        }
      }

      return new SelectionRange[ 0 ];
    }

    public override bool Equals( object obj )
    {
      if( !( obj is SelectionRange ) )
        return false;

      var selectionRange = ( SelectionRange )obj;

      return ( selectionRange.m_startIndex == m_startIndex )
        && ( selectionRange.m_endIndex == m_endIndex );
    }

    public override int GetHashCode()
    {
      return ( ( m_startIndex >> 16 ) | ( m_startIndex << 16 ) ) ^ m_endIndex;
    }

    public int GetIndexFromItemOffset( int itemOffset )
    {
      if( m_startIndex > m_endIndex )
      {
        return m_startIndex - itemOffset;
      }
      else
      {
        return m_startIndex + itemOffset;
      }
    }

    public int GetOffsetFromItemIndex( int itemIndex )
    {
      if( m_startIndex > m_endIndex )
      {
        return m_startIndex - itemIndex;
      }
      else
      {
        return itemIndex - m_startIndex;
      }
    }

    #region IComparable<SelectionRange> Members

    public int CompareTo( SelectionRange other )
    {
      if( this < other )
        return -1;

      if( this > other )
        return 1;

      return 0;
    }

    #endregion
  }
}
