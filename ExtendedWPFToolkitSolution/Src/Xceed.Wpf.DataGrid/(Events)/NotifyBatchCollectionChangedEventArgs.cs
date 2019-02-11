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
using System.Collections.Specialized;
using System.Diagnostics;

namespace Xceed.Wpf.DataGrid
{
  internal sealed class NotifyBatchCollectionChangedEventArgs : NotifyCollectionChangedEventArgs
  {
    private NotifyBatchCollectionChangedEventArgs( NotifyCollectionChangedAction action )
      : base( action )
    {
    }

    private NotifyBatchCollectionChangedEventArgs( NotifyCollectionChangedAction action, IList changedItems )
      : base( action, changedItems )
    {
    }

    private NotifyBatchCollectionChangedEventArgs( NotifyCollectionChangedAction action, IList changedItems, int startingIndex )
      : base( action, changedItems, startingIndex )
    {
    }

    private NotifyBatchCollectionChangedEventArgs( NotifyCollectionChangedAction action, IList changedItems, int index, int oldIndex )
      : base( action, changedItems, index, oldIndex )
    {
    }

    private NotifyBatchCollectionChangedEventArgs( NotifyCollectionChangedAction action, IList newItems, IList oldItems )
      : base( action, newItems, oldItems )
    {
    }

    private NotifyBatchCollectionChangedEventArgs( NotifyCollectionChangedAction action, IList newItems, IList oldItems, int startingIndex )
      : base( action, newItems, oldItems, startingIndex )
    {
    }

    #region Replacements Property

    internal IDictionary<object, object> Replacements
    {
      get
      {
        if( m_fromTo == null )
        {
          m_fromTo = new Dictionary<object, object>( 0 );
          m_toFrom = new Dictionary<object, object>( 0 );
        }

        return m_fromTo;
      }
    }

    #endregion

    internal static NotifyBatchCollectionChangedEventArgs Combine( NotifyCollectionChangedEventArgs x, NotifyCollectionChangedEventArgs y )
    {
      return NotifyBatchCollectionChangedEventArgs.Combine(
               NotifyBatchCollectionChangedEventArgs.Create( x ),
               NotifyBatchCollectionChangedEventArgs.Create( y ) );
    }

    private static NotifyBatchCollectionChangedEventArgs Combine( NotifyBatchCollectionChangedEventArgs x, NotifyBatchCollectionChangedEventArgs y )
    {
      if( x == null )
        return y;

      if( y == null )
        return x;

      var replacements = default( BiMap );
      var result = default( NotifyBatchCollectionChangedEventArgs );

      if( x.Action == y.Action )
      {
        switch( x.Action )
        {
          case NotifyCollectionChangedAction.Replace:
            {
              replacements = NotifyBatchCollectionChangedEventArgs.Merge(
                               NotifyBatchCollectionChangedEventArgs.Map( x.OldItems, x.NewItems ),
                               NotifyBatchCollectionChangedEventArgs.Map( y.OldItems, y.NewItems ) );
              if( replacements == null )
                return null;

              result = new NotifyBatchCollectionChangedEventArgs( NotifyCollectionChangedAction.Reset );
            }
            break;

          case NotifyCollectionChangedAction.Add:
          case NotifyCollectionChangedAction.Remove:
          case NotifyCollectionChangedAction.Move:
            result = new NotifyBatchCollectionChangedEventArgs( NotifyCollectionChangedAction.Reset );
            break;

          case NotifyCollectionChangedAction.Reset:
            result = new NotifyBatchCollectionChangedEventArgs( NotifyCollectionChangedAction.Reset );
            replacements = NotifyBatchCollectionChangedEventArgs.Merge( x.m_fromTo, y.m_fromTo );
            break;

          default:
            throw new NotSupportedException();
        }
      }
      else
      {
        if( x.Action == NotifyCollectionChangedAction.Replace )
        {
          replacements = NotifyBatchCollectionChangedEventArgs.Merge( NotifyBatchCollectionChangedEventArgs.Map( x.OldItems, x.NewItems ), null );
        }
        else if( ( x.Action == NotifyCollectionChangedAction.Reset ) && ( x.m_fromTo != null ) )
        {
          replacements = new BiMap( x.m_fromTo, x.m_toFrom );
        }

        switch( y.Action )
        {
          case NotifyCollectionChangedAction.Add:
            {
              if( replacements != null )
              {
                var fromTo = replacements.FromTo;
                var toFrom = replacements.ToFrom;

                foreach( var item in y.NewItems )
                {
                  object to;

                  if( fromTo.TryGetValue( item, out to ) )
                  {
                    fromTo.Remove( item );
                    toFrom.Remove( to );
                  }
                }
              }
            }
            break;

          case NotifyCollectionChangedAction.Remove:
            {
              if( replacements != null )
              {
                var fromTo = replacements.FromTo;
                var toFrom = replacements.ToFrom;

                foreach( var item in y.OldItems )
                {
                  object from;

                  if( toFrom.TryGetValue( item, out from ) )
                  {
                    fromTo.Remove( from );
                    toFrom.Remove( item );
                  }
                }
              }
            }
            break;

          case NotifyCollectionChangedAction.Replace:
            {
              var mapping = NotifyBatchCollectionChangedEventArgs.Map( y.OldItems, y.NewItems );

              if( replacements != null )
              {
                replacements = NotifyBatchCollectionChangedEventArgs.Merge( replacements.FromTo, mapping );
              }
              else
              {
                replacements = NotifyBatchCollectionChangedEventArgs.Merge( mapping, null );
              }
            }
            break;

          case NotifyCollectionChangedAction.Move:
            break;

          case NotifyCollectionChangedAction.Reset:
            {
              if( replacements != null )
              {
                replacements = NotifyBatchCollectionChangedEventArgs.Merge( replacements.FromTo, y.m_fromTo );
              }
              else
              {
                replacements = NotifyBatchCollectionChangedEventArgs.Merge( y.m_fromTo, null );
              }
            }
            break;

          default:
            throw new NotSupportedException();
        }

        result = new NotifyBatchCollectionChangedEventArgs( NotifyCollectionChangedAction.Reset );
      }

      if( ( result != null ) && ( replacements != null ) )
      {
        result.m_fromTo = replacements.FromTo;
        result.m_toFrom = replacements.ToFrom;
      }

      return result;
    }

    private static NotifyBatchCollectionChangedEventArgs Create( NotifyCollectionChangedEventArgs e )
    {
      if( e == null )
        return null;

      var batch = e as NotifyBatchCollectionChangedEventArgs;
      if( batch != null )
        return batch;

      switch( e.Action )
      {
        case NotifyCollectionChangedAction.Add:
          return new NotifyBatchCollectionChangedEventArgs( e.Action, e.NewItems, e.NewStartingIndex );

        case NotifyCollectionChangedAction.Remove:
          return new NotifyBatchCollectionChangedEventArgs( e.Action, e.OldItems, e.OldStartingIndex );

        case NotifyCollectionChangedAction.Move:
          return new NotifyBatchCollectionChangedEventArgs( e.Action, e.OldItems, e.NewStartingIndex, e.OldStartingIndex );

        case NotifyCollectionChangedAction.Replace:
          return new NotifyBatchCollectionChangedEventArgs( e.Action, e.NewItems, e.OldItems, e.OldStartingIndex );

        case NotifyCollectionChangedAction.Reset:
          return new NotifyBatchCollectionChangedEventArgs( e.Action );

        default:
          throw new NotSupportedException();
      }
    }

    private static BiMap Merge( IDictionary<object, object> x, IDictionary<object, object> y )
    {
      var fromTo = new Dictionary<object, object>();
      var toFrom = new Dictionary<object, object>();

      if( x != null )
      {
        foreach( var entry in x )
        {
          if( object.ReferenceEquals( entry.Key, entry.Value ) )
            continue;

          fromTo.Add( entry.Key, entry.Value );
          toFrom.Add( entry.Value, entry.Key );
        }
      }

      if( y != null )
      {
        foreach( var entry in y )
        {
          if( object.ReferenceEquals( entry.Key, entry.Value ) )
            continue;

          // Keep only the initial and final value.  For example, (A -> B -> C) becomes (A -> C).
          var from = default( object );
          if( toFrom.TryGetValue( entry.Key, out from ) )
          {
            toFrom.Remove( entry.Key );

            if( object.ReferenceEquals( from, entry.Value ) )
            {
              fromTo.Remove( from );
            }
            else
            {
              fromTo[ from ] = entry.Value;
              toFrom[ entry.Value ] = from;
            }
          }
          else
          {
            // There is no need to check if the entry exists, since it should not or the same instance
            // is use multiple time in the data source, which is not supported by the grid.
            fromTo.Add( entry.Key, entry.Value );
            toFrom.Add( entry.Value, entry.Key );
          }
        }
      }

      if( fromTo.Count <= 0 )
        return null;

      return new BiMap( fromTo, toFrom );
    }

    private static IDictionary<object, object> Map( IList oldItems, IList newItems )
    {
      Debug.Assert( oldItems != null );
      Debug.Assert( newItems != null );
      Debug.Assert( oldItems.Count == newItems.Count );

      var items = new Dictionary<object, object>();
      var count = oldItems.Count;

      for( int i = 0; i < count; i++ )
      {
        items.Add( oldItems[ i ], newItems[ i ] );
      }

      return items;
    }

    private IDictionary<object, object> m_fromTo;
    private IDictionary<object, object> m_toFrom;

    #region BiMap Private Class

    private sealed class BiMap
    {
      internal BiMap( IDictionary<object, object> fromTo, IDictionary<object, object> toFrom )
      {
        this.FromTo = fromTo;
        this.ToFrom = toFrom;
      }

      internal readonly IDictionary<object, object> FromTo;
      internal readonly IDictionary<object, object> ToFrom;
    }

    #endregion
  }
}
