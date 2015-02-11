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
using System.ComponentModel;

namespace Xceed.Utils.Collections
{
  // "Weak heap sort" sort m_dataIndexArray[0..length-1] to m_dataIndexArray[1..length]
  //
  // m_dataIndexArray is an array of int which points to the real value to sort.
  // The index in dataIndexArray will be used to pass as parameters to Compare.
  // That way, when implementing Compare, we use this index to directly acces the real data.
  internal abstract class IndexWeakHeapSort
  {
    protected IndexWeakHeapSort( int[] dataIndexArray )
    {
      m_dataIndexArray = dataIndexArray;
    }

    public void Sort( int length )
    {
      if( m_dataIndexArray == null )
        throw new InvalidOperationException( "A dataIndexArray must be passed at construction in order to sort." );

      if( length < 0 )
        throw new ArgumentOutOfRangeException( "length", length, "length must be greater than or equal to zero." );

      if( length >= m_dataIndexArray.Length )
        throw new ArgumentOutOfRangeException( "length", length, "length must be less than dataIndexArray.Length - 1." );

      m_reverse = new sbyte[ length + 1 ];
      int i;

      for( i = length - 1; i >= 1; i-- )
      {
        int parent = i;

        // Gparent //
        while( ( parent & 1 ) == 0 )
        {
          parent >>= 1;
        }

        parent >>= 1;
        // End GParent //

        // Merge //
        if( this.Compare( m_dataIndexArray[ parent ], m_dataIndexArray[ i ] ) < 0 )
        {
          int temp = m_dataIndexArray[ parent ];
          m_dataIndexArray[ parent ] = m_dataIndexArray[ i ];
          m_dataIndexArray[ i ] = temp;

          if( m_reverse[ i ] == 0 )
          {
            m_reverse[ i ] = 1;
          }
          else
          {
            m_reverse[ i ] = 0;
          }
        }
        // End merge //
      }

      m_dataIndexArray[ length ] = m_dataIndexArray[ 0 ];

      for( i = length - 1; i >= 2; i-- )
      {
        this.MergeForest( i );
      }

      // "Weak heap sort" sort array[0..NUM_ELEMENTS-1] to array[1..NUM_ELEMENTS]

      m_reverse = null;
    }

    public abstract int Compare( int xDataIndex, int yDataIndex );

    private void MergeForest( int m )
    {
      int x = 1;

      while( ( ( x << 1 ) + m_reverse[ x ] ) < m )
      {
        x = ( x << 1 ) + m_reverse[ x ];
      }

      do
      {
        // Merge //
        if( this.Compare( m_dataIndexArray[ m ], m_dataIndexArray[ x ] ) < 0 )
        {
          int temp = m_dataIndexArray[ m ];
          m_dataIndexArray[ m ] = m_dataIndexArray[ x ];
          m_dataIndexArray[ x ] = temp;

          if( m_reverse[ x ] == 0 )
          {
            m_reverse[ x ] = 1;
          }
          else
          {
            m_reverse[ x ] = 0;
          }
        }
        // End merge //

        x >>= 1;
      }
      while( x > 0 );
    }

    private int[] m_dataIndexArray;
    private sbyte[] m_reverse;
  }
}
