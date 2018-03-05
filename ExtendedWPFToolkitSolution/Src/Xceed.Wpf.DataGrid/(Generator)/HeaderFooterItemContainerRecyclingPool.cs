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
using System.Diagnostics;
using System.Windows;

namespace Xceed.Wpf.DataGrid
{
  internal sealed class HeaderFooterItemContainerRecyclingPool : IEnumerable<DependencyObject>
  {
    internal int Count
    {
      get
      {
        return m_count;
      }
    }

    internal void Clear()
    {
      if( m_count <= 0 )
        return;

      m_collection.Clear();
      m_count = 0;
      m_version++;
    }

    internal void Enqueue( object item, DependencyObject container )
    {
      if( item == null )
        throw new ArgumentNullException( "item" );

      if( container == null )
        throw new ArgumentNullException( "container" );

      var entry = default( Entry );

      if( !m_collection.TryGetValue( item, out entry ) )
      {
        entry = new Entry();
      }

      entry.Push( container );

      m_collection[ item ] = entry;
      m_count++;
      m_version++;
    }

    internal DependencyObject Dequeue( object item )
    {
      if( item == null )
        throw new ArgumentNullException( "item" );

      if( m_collection.Count <= 0 )
        return null;

      var entry = default( Entry );

      if( !m_collection.TryGetValue( item, out entry ) )
        return null;

      Debug.Assert( entry.Count > 0 );
      var container = entry.Pop();
      Debug.Assert( container != null );

      if( entry.Count == 0 )
      {
        m_collection.Remove( item );
      }
      else
      {
        m_collection[ item ] = entry;
      }

      m_count--;
      m_version++;

      return container;
    }

    #region IEnumerable<> Members

    public IEnumerator<DependencyObject> GetEnumerator()
    {
      var version = m_version;

      foreach( var entry in m_collection.Values )
      {
        for( int i = entry.Count - 1; i >= 0; i-- )
        {
          if( version != m_version )
            throw new InvalidOperationException();

          yield return entry[ i ];
        }
      }
    }

    #endregion

    #region IEnumerable Members

    IEnumerator IEnumerable.GetEnumerator()
    {
      return this.GetEnumerator();
    }

    #endregion

    private int m_count; //0
    private int m_version; //0
    private readonly Dictionary<object, Entry> m_collection = new Dictionary<object, Entry>();

    #region Entry Private Struct

    private struct Entry
    {
      internal int Count
      {
        get
        {
          return m_count;
        }
      }

      internal DependencyObject this[ int index ]
      {
        get
        {
          Debug.Assert( ( index >= 0 ) && ( index < m_count ) );
          return m_containers[ index ];
        }
      }

      internal void Push( DependencyObject container )
      {
        Debug.Assert( container != null );

        if( m_containers == null )
        {
          Array.Resize( ref m_containers, 1 );
        }
        else if( m_count == m_containers.Length )
        {
          Debug.Assert( m_containers.Length > 0 );
          Array.Resize( ref m_containers, m_containers.Length * 2 );
        }

        m_containers[ m_count ] = container;
        m_count++;
      }

      internal DependencyObject Pop()
      {
        Debug.Assert( m_count > 0 );
        Debug.Assert( m_containers != null );

        m_count--;

        var container = m_containers[ m_count ];
        m_containers[ m_count ] = default( DependencyObject );

        return container;
      }

      private int m_count; //0
      private DependencyObject[] m_containers; //null
    }

    #endregion
  }
}
