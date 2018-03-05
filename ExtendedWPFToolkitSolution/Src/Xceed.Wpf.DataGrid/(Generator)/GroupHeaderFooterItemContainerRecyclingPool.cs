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
using System.Windows;

namespace Xceed.Wpf.DataGrid
{
  internal sealed class GroupHeaderFooterItemContainerRecyclingPool : IEnumerable<DependencyObject>
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
      m_collection.Clear();
      m_count = 0;
    }

    internal void Enqueue( string groupBy, object template, DependencyObject container )
    {
      if( groupBy == null )
        throw new ArgumentNullException( "groupBy" );

      if( template == null )
        throw new ArgumentNullException( "template" );

      var pool = default( HeaderFooterItemContainerRecyclingPool );
      if( m_collection.TryGetValue( groupBy, out pool ) )
      {
        pool.Enqueue( template, container );
      }
      else
      {
        pool = new HeaderFooterItemContainerRecyclingPool();
        pool.Enqueue( template, container );

        m_collection.Add( groupBy, pool );
      }

      m_count++;
    }

    internal DependencyObject Dequeue( string groupBy, object template )
    {
      if( groupBy == null )
        throw new ArgumentNullException( "groupBy" );

      if( template == null )
        throw new ArgumentNullException( "template" );

      var pool = default( HeaderFooterItemContainerRecyclingPool );
      if( !m_collection.TryGetValue( groupBy, out pool ) )
        return null;

      var container = pool.Dequeue( template );
      if( container != null )
      {
        if( pool.Count == 0 )
        {
          m_collection.Remove( groupBy );
        }

        m_count--;
      }

      return container;
    }

    #region IEnumerable<> Members

    public IEnumerator<DependencyObject> GetEnumerator()
    {
      foreach( var pool in m_collection.Values )
      {
        foreach( var entry in pool )
        {
          yield return entry;
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
    private readonly Dictionary<string, HeaderFooterItemContainerRecyclingPool> m_collection = new Dictionary<string, HeaderFooterItemContainerRecyclingPool>();
  }
}
