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

namespace Xceed.Utils.Collections
{
  internal static class IListExtensions
  {
    internal static void Diff(
      this IList source,
      IList destination,
      ICollection<object> itemsAdded,
      ICollection<object> itemsRemoved,
      ICollection<object> itemsMoved )
    {
      if( source == null )
        throw new ArgumentNullException( "source" );

      if( destination == null )
        throw new ArgumentNullException( "destination" );

      // There is nothing to do since the caller is not interested by the result.
      if( ( itemsAdded == null ) && ( itemsRemoved == null ) && ( itemsMoved == null ) )
        return;

      IListExtensions.Diff( new IListWrapper( source ), new IListWrapper( destination ), itemsAdded, itemsRemoved, itemsMoved );
    }

    internal static void Diff<T>(
      this IList<T> source,
      IList<T> destination,
      ICollection<T> itemsAdded,
      ICollection<T> itemsRemoved,
      ICollection<T> itemsMoved )
    {
      if( source == null )
        throw new ArgumentNullException( "source" );

      if( destination == null )
        throw new ArgumentNullException( "destination" );

      // There is nothing to do since the caller is not interested by the result.
      if( ( itemsAdded == null ) && ( itemsRemoved == null ) && ( itemsMoved == null ) )
        return;

      var sourceCount = source.Count;
      var sourcePositions = new Dictionary<T, int>( sourceCount );
      var sequence = new List<int>( System.Math.Min( sourceCount, destination.Count ) );
      var isAlive = new BitArray( sourceCount, false );
      var hasNotMoved = default( BitArray );

      for( var i = 0; i < sourceCount; i++ )
      {
        sourcePositions.Add( source[ i ], i );
      }

      foreach( var item in destination )
      {
        int index;

        if( sourcePositions.TryGetValue( item, out index ) )
        {
          isAlive[ index ] = true;
          sequence.Add( index );
        }
        else if( itemsAdded != null )
        {
          itemsAdded.Add( item );
        }
      }

      // We may omit this part of the algorithm if the caller is not interested by the items that have moved.
      if( itemsMoved != null )
      {
        hasNotMoved = new BitArray( sourceCount, false );

        // The subsequence contains the position of the item that are in the destination collection and that have not moved.
        foreach( var index in LongestIncreasingSubsequence.Find( sequence ) )
        {
          hasNotMoved[ index ] = true;
        }
      }

      // We may omit this part of the algorithm if the caller is not interested by the items that have moved or were removed.
      if( ( itemsRemoved != null ) || ( itemsMoved != null ) )
      {
        for( var i = 0; i < sourceCount; i++ )
        {
          if( isAlive[ i ] )
          {
            // We check if the move collection is not null first because the bit array is null when the move collection is null.
            if( ( itemsMoved != null ) && !hasNotMoved[ i ] )
            {
              itemsMoved.Add( source[ i ] );
            }
          }
          else if( itemsRemoved != null )
          {
            itemsRemoved.Add( source[ i ] );
          }
        }
      }
    }
  }
}
