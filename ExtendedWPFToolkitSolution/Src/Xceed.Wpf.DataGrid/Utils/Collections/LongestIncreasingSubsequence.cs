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
using System.Collections.Generic;
using System.Diagnostics;

namespace Xceed.Utils.Collections
{
  internal static class LongestIncreasingSubsequence
  {
    internal static IList<int> Find( IList<int> values )
    {
      if( values == null )
        throw new ArgumentNullException( "values" );

      if( values.Count <= 0 )
        return new int[ 0 ];

      var maximum = 1;
      var predecessors = new int[ values.Count ];
      var positions = new int[ values.Count ];

      predecessors[ 0 ] = -1;
      positions[ 0 ] = 0;

      for( var i = 1; i < values.Count; i++ )
      {
        // We have found an element that extend the longest subsequence found so far.
        if( values[ i ] > values[ positions[ maximum - 1 ] ] )
        {
          predecessors[ i ] = positions[ maximum - 1 ];
          positions[ maximum ] = i;
          maximum++;
        }
        else
        {
          var index = LongestIncreasingSubsequence.FindLeastIndex( i, values, positions, maximum );

          // This is the lowest value found so far.
          if( index < 0 )
          {
            predecessors[ i ] = -1;
            positions[ 0 ] = i;
          }
          // We have found an element that could help to find new future subsequences.
          else
          {
            Debug.Assert( index + 1 < maximum );

            predecessors[ i ] = positions[ index ];
            positions[ index + 1 ] = i;
          }
        }
      }

      var result = new int[ maximum ];
      var position = positions[ maximum - 1 ];

      for( var i = maximum - 1; i >= 0; i-- )
      {
        result[ i ] = values[ position ];
        position = predecessors[ position ];
      }

      return result;
    }

    private static int FindLeastIndex( int index, IList<int> values, int[] positions, int maximum )
    {
      Debug.Assert( ( values != null ) && ( values.Count > 0 ) );
      Debug.Assert( ( positions != null ) && ( maximum <= positions.Length ) );
      Debug.Assert( ( index >= 0 ) && ( index < values.Count ) );

      var lower = 0;
      var upper = maximum - 1;
      var value = values[ index ];

      while( lower <= upper )
      {
        var middle = lower + ( upper - lower ) / 2;
        var compare = value.CompareTo( values[ positions[ middle ] ] );

        if( compare <= 0 )
        {
          if( middle == 0 )
            break;

          if( lower == middle )
            return middle - 1;

          upper = middle - 1;
        }
        else
        {
          if( upper == middle )
            return middle;

          lower = middle + 1;
        }
      }

      return -1;
    }
  }
}
