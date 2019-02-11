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

namespace Xceed.Utils.Collections
{
  internal abstract class FenwickTree<T>
  {
    #region Constructor

    protected FenwickTree( int capacity )
    {
      if( capacity < 0 )
        throw new ArgumentException( "The capacity must be greater than or equal to zero.", "capacity" );

      if( capacity > 0 )
      {
        m_collection = new T[ capacity + 1 ];
      }
    }

    #endregion

    #region Count Property

    public int Count
    {
      get
      {
        if( m_collection != null )
          return m_collection.Length - 1;

        return 0;
      }
    }

    #endregion

    #region [] Property

    public T this[ int index ]
    {
      get
      {
        return this.GetRunningSum( index, index );
      }
      set
      {
        var current = this[ index ];
        if( object.Equals( current, value ) )
          return;

        var diff = this.Substract( value, current );
        var x = FenwickTree<T>.ConvertIndex( index );

        while( x < m_collection.Length )
        {
          m_collection[ x ] = this.Add( m_collection[ x ], diff );
          x += FenwickTree<T>.FindLastDigit( x );
        }
      }
    }

    #endregion

    public T GetRunningSum( int index )
    {
      return this.GetRunningSum( 0, index );
    }

    public T GetRunningSum( int startIndex, int endIndex )
    {
      if( ( startIndex < 0 ) || ( startIndex >= this.Count ) )
        throw new ArgumentOutOfRangeException( "startIndex", startIndex, "The index must be greater or equal to zero and lesser than the collection Count." );

      if( ( endIndex < 0 ) || ( endIndex >= this.Count ) )
        throw new ArgumentOutOfRangeException( "endIndex", endIndex, "The index must be greater or equal to zero and lesser than the collection Count." );

      if( startIndex > endIndex )
        throw new ArgumentException( "The start index must be lesser than or equal to the end index.", "startIndex" );

      var x = FenwickTree<T>.ConvertIndex( endIndex );
      var y = FenwickTree<T>.ConvertIndex( startIndex - 1 );
      var sum = default( T );

      while( x > y )
      {
        sum = this.Add( sum, m_collection[ x ] );
        x -= FenwickTree<T>.FindLastDigit( x );
      }

      while( y > x )
      {
        sum = this.Substract( sum, m_collection[ y ] );
        y -= FenwickTree<T>.FindLastDigit( y );
      }

      return sum;
    }

    protected abstract T Add( T x, T y );
    protected abstract T Substract( T x, T y );

    private static int ConvertIndex( int value )
    {
      return value + 1;
    }

    private static int FindLastDigit( int value )
    {
      return ( value & -value );
    }

    #region Private Fields

    private T[] m_collection; //null

    #endregion
  }
}
