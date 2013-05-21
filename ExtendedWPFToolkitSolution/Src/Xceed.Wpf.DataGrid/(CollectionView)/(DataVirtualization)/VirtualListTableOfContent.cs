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
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Collections.ObjectModel;

namespace Xceed.Wpf.DataGrid
{
  internal class VirtualListTableOfContent : IDisposable
  {
    internal VirtualListTableOfContent( int initialCapacity )
    {
      m_virtualPages = new List<VirtualPage>();
      m_readonlyVirtualPages = new ReadOnlyCollection<VirtualPage>( m_virtualPages );

      m_objectVersusIndexDictionary = new Dictionary<object, int>( initialCapacity );
      m_indexVersusPageDictionary = new Dictionary<int, VirtualPage>( initialCapacity );
    }

    public void AddPage( VirtualPage page )
    {
      Debug.Assert( !m_virtualPages.Contains( page ) );

      m_virtualPages.Add( page );

      int itemCount = page.Count;

      for( int i = 0; i < itemCount; i++ )
      {
        VirtualizedItemInfo virtualizedItemInfo = page[ i ];

        Debug.Assert( !m_objectVersusIndexDictionary.ContainsKey( virtualizedItemInfo.DataItem ) );
        Debug.Assert( !m_indexVersusPageDictionary.ContainsKey( virtualizedItemInfo.Index ) );

        m_objectVersusIndexDictionary.Add( virtualizedItemInfo.DataItem, virtualizedItemInfo.Index );
        m_indexVersusPageDictionary.Add( virtualizedItemInfo.Index, page );
      }
    }

    public void RemovePage( VirtualPage page )
    {
      Debug.Assert( m_virtualPages.Contains( page ) );

      int itemCount = page.Count;

      for( int i = 0; i < itemCount; i++ )
      {
        VirtualizedItemInfo virtualizedItemInfo = page[ i ];

        Debug.Assert( m_objectVersusIndexDictionary.ContainsKey( virtualizedItemInfo.DataItem ) );
        Debug.Assert( m_indexVersusPageDictionary.ContainsKey( virtualizedItemInfo.Index ) );

        Debug.Assert( m_indexVersusPageDictionary[ virtualizedItemInfo.Index ] == page );

        m_objectVersusIndexDictionary.Remove( virtualizedItemInfo.DataItem );
        m_indexVersusPageDictionary.Remove( virtualizedItemInfo.Index );
      }

      m_virtualPages.Remove( page );
    }

    public int IndexOf( object item )
    {
      int index;

      if( m_objectVersusIndexDictionary.TryGetValue( item, out index ) )
        return index;

      return -1;
    }

    public bool TryGetPageForItem( object item, out VirtualPage page )
    {
      page = null;

      int index;

      if( m_objectVersusIndexDictionary.TryGetValue( item, out index ) )
        return m_indexVersusPageDictionary.TryGetValue( index, out page );

      return false;
    }

    public bool TryGetPageForSourceIndex( int sourceIndex, out VirtualPage page )
    {
      return m_indexVersusPageDictionary.TryGetValue( sourceIndex, out page );
    }

    public bool ContainsPageForItem( object item )
    {
      int index;

      if( m_objectVersusIndexDictionary.TryGetValue( item, out index ) )
        return m_indexVersusPageDictionary.ContainsKey( index );

      return false;
    }

    public bool ContainsPageForSourceIndex( int sourceIndex )
    {
      return m_indexVersusPageDictionary.ContainsKey( sourceIndex );
    }

    public int Count
    {
      get
      {
        Debug.Assert( m_objectVersusIndexDictionary.Count == m_indexVersusPageDictionary.Count );
        return m_objectVersusIndexDictionary.Count;
      }
    }

    public ReadOnlyCollection<VirtualPage> VirtualPages
    {
      get
      {
        return m_readonlyVirtualPages;
      }
    }

    private Dictionary<object, int> m_objectVersusIndexDictionary;
    private Dictionary<int, VirtualPage> m_indexVersusPageDictionary;

    private List<VirtualPage> m_virtualPages;
    private ReadOnlyCollection<VirtualPage> m_readonlyVirtualPages;

    #region IDisposable Members

    public void Dispose()
    {
      while( m_virtualPages.Count > 0 )
      {
        // Remove the page from every Dictionaries and
        // also from m_virtualPages
        VirtualPage page = m_virtualPages[ 0 ];
        this.RemovePage( page );
        page.Dispose();
      }

      Debug.Assert( m_objectVersusIndexDictionary.Count == 0 );
      Debug.Assert( m_indexVersusPageDictionary.Count == 0 );
      Debug.Assert( m_virtualPages.Count == 0 );

      m_objectVersusIndexDictionary.Clear();
      m_indexVersusPageDictionary.Clear();
      m_virtualPages.Clear();

    }

    #endregion
  }
}
