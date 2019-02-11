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
using System.Collections.Specialized;

namespace Xceed.Wpf.DataGrid
{
  internal static class NotifyCollectionChangedEventArgsExtensions
  {
    internal static NotifyCollectionChangedEventArgs GetRangeActionOrSelf( this NotifyCollectionChangedEventArgs source )
    {
      if( source == null )
        throw new ArgumentNullException( "source" );

      var rangeEventArgs = source as NotifyRangeCollectionChangedEventArgs;
      if( rangeEventArgs != null )
        return rangeEventArgs.OriginalEventArgs;

      return source;
    }

    internal static object GetReplacement( this NotifyCollectionChangedEventArgs source, object item )
    {
      if( source == null )
        throw new ArgumentNullException( "source" );

      var result = default( object );

      if( item != null )
      {
        var batch = source as NotifyBatchCollectionChangedEventArgs;
        if( batch != null )
        {
          if( !batch.Replacements.TryGetValue( item, out result ) )
          {
            result = null;
          }
        }
        else if( source.Action == NotifyCollectionChangedAction.Replace )
        {
          var oldItems = source.OldItems;
          var newItems = source.NewItems;

          if( ( oldItems != null ) && ( newItems != null ) )
          {
            var index = oldItems.IndexOf( item );
            if( index >= 0 )
            {
              result = newItems[ index ];
            }
          }
        }
      }

      return result;
    }
  }
}
