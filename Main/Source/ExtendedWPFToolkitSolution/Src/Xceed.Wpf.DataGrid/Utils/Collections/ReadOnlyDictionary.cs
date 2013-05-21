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
using System.Collections;

namespace Xceed.Utils.Collections
{
  internal class ReadOnlyDictionary<Key, Value> : IDictionary<Key, Value>
  {
    #region IDictionary<Key,Value> Members

    public virtual void Add( Key key, Value value )
    {
      throw new NotSupportedException();
    }

    public virtual bool ContainsKey( Key key )
    {
      return m_dictionary.ContainsKey( key );
    }

    public virtual ICollection<Key> Keys
    {
      get
      {
        return m_dictionary.Keys;
      }
    }

    public virtual bool Remove( Key key )
    {
      throw new NotSupportedException();
    }

    public virtual bool TryGetValue( Key key, out Value value )
    {
      return m_dictionary.TryGetValue( key, out value );
    }

    public virtual ICollection<Value> Values
    {
      get
      {
        return m_dictionary.Values;
      }
    }

    public virtual Value this[ Key key ]
    {
      get
      {
        return m_dictionary[ key ];
      }
      set
      {
        throw new NotSupportedException();
      }
    }

    #endregion

    #region ICollection<KeyValuePair<Key,Value>> Members

    public virtual void Add( KeyValuePair<Key, Value> item )
    {
      throw new NotSupportedException();
    }

    public virtual void Clear()
    {
      throw new NotSupportedException();
    }

    public virtual bool Contains( KeyValuePair<Key, Value> item )
    {
      return m_dictionary.Contains( item );
    }

    public void CopyTo( KeyValuePair<Key, Value>[] array, int arrayIndex )
    {
      throw new NotSupportedException();
    }

    public int Count
    {
      get
      {
        return m_dictionary.Count;
      }
    }

    public bool IsReadOnly
    {
      get
      {
        return true;
      }
    }

    public virtual bool Remove( KeyValuePair<Key, Value> item )
    {
      throw new NotSupportedException();
    }

    #endregion

    #region IEnumerable<KeyValuePair<Key,Value>> Members

    public virtual IEnumerator<KeyValuePair<Key, Value>> GetEnumerator()
    {
      throw new NotSupportedException();
    }

    #endregion

    #region IEnumerable Members

    IEnumerator IEnumerable.GetEnumerator()
    {
      throw new NotSupportedException();
    }

    #endregion

    #region INTERNAL METHODS

    internal virtual void InternalAdd( Key key, Value value )
    {
      m_dictionary.Add( key, value );
    }

    internal virtual void InternalClear()
    {
      m_dictionary.Clear();
    }

    internal virtual bool InternalRemove( Key key )
    {
      return m_dictionary.Remove( key );
    }

    internal virtual void InternalSet( Key key, Value value )
    {
      m_dictionary[ key ] = value;
    }

    #endregion

    #region PRIVATE FIELDS

    Dictionary<Key, Value> m_dictionary = new Dictionary<Key, Value>();

    #endregion
  }
}
