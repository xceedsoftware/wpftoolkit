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
using System.Linq;

namespace Xceed.Utils.Collections
{
  internal sealed class IListWrapper : IList<object>
  {
    internal IListWrapper( IList source )
    {
      if( source == null )
        throw new ArgumentNullException( "source" );

      m_source = source;
    }

    #region IList<> Members

    public object this[ int index ]
    {
      get
      {
        return m_source[ index ];
      }
      set
      {
        m_source[ index ] = value;
      }
    }

    public int IndexOf( object item )
    {
      return m_source.IndexOf( item );
    }

    public void Insert( int index, object item )
    {
      m_source.Insert( index, item );
    }

    public void RemoveAt( int index )
    {
      m_source.RemoveAt( index );
    }

    #endregion

    #region ICollection<> Members

    public int Count
    {
      get
      {
        return m_source.Count;
      }
    }

    bool ICollection<object>.IsReadOnly
    {
      get
      {
        return false;
      }
    }

    public void Add( object item )
    {
      m_source.Add( item );
    }

    public void Clear()
    {
      m_source.Clear();
    }

    public bool Contains( object item )
    {
      return m_source.Contains( item );
    }

    public void CopyTo( object[] array, int index )
    {
      m_source.CopyTo( array, index );
    }

    public bool Remove( object item )
    {
      var index = m_source.IndexOf( item );
      if( index < 0 )
        return false;

      m_source.RemoveAt( index );

      return true;
    }

    #endregion

    #region IEnumerable<> Members

    public IEnumerator<object> GetEnumerator()
    {
      return m_source.Cast<object>().GetEnumerator();
    }

    #endregion

    #region IEnumerable Members

    IEnumerator IEnumerable.GetEnumerator()
    {
      return m_source.GetEnumerator();
    }

    #endregion

    private readonly IList m_source;
  }
}
