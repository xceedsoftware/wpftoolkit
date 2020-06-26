/*************************************************************************************
   
   Toolkit for WPF

   Copyright (C) 2007-2020 Xceed Software Inc.

   This program is provided to you under the terms of the XCEED SOFTWARE, INC.
   COMMUNITY LICENSE AGREEMENT (for non-commercial use) as published at 
   https://github.com/xceedsoftware/wpftoolkit/blob/master/license.md 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at https://xceed.com/xceed-toolkit-plus-for-wpf/

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

using System;
using System.Collections.Generic;

namespace Xceed.Wpf.AvalonDock.Controls
{
  internal class FullWeakDictionary<K, V> where K : class
  {
    #region Members

    private List<WeakReference> _keys = new List<WeakReference>();
    private List<WeakReference> _values = new List<WeakReference>();

    #endregion

    #region Constructors

    public FullWeakDictionary()
    {
    }

    #endregion

    #region Public Methods

    public V this[ K key ]
    {
      get
      {
        V valueToReturn;
        if( !GetValue( key, out valueToReturn ) )
          throw new ArgumentException();
        return valueToReturn;
      }
      set
      {
        SetValue( key, value );
      }
    }

    public bool ContainsKey( K key )
    {
      CollectGarbage();
      return -1 != _keys.FindIndex( k => k.GetValueOrDefault<K>() == key );
    }

    public void SetValue( K key, V value )
    {
      CollectGarbage();
      int vIndex = _keys.FindIndex( k => k.GetValueOrDefault<K>() == key );
      if( vIndex > -1 )
        _values[ vIndex ] = new WeakReference( value );
      else
      {
        _values.Add( new WeakReference( value ) );
        _keys.Add( new WeakReference( key ) );
      }
    }

    public bool GetValue( K key, out V value )
    {
      CollectGarbage();
      int vIndex = _keys.FindIndex( k => k.GetValueOrDefault<K>() == key );
      value = default( V );
      if( vIndex == -1 )
        return false;
      value = _values[ vIndex ].GetValueOrDefault<V>();
      return true;
    }

    void CollectGarbage()
    {
      int vIndex = 0;

      do
      {
        vIndex = _keys.FindIndex( vIndex, k => !k.IsAlive );
        if( vIndex >= 0 )
        {
          _keys.RemoveAt( vIndex );
          _values.RemoveAt( vIndex );
        }
      }
      while( vIndex >= 0 );

      vIndex = 0;
      do
      {
        vIndex = _values.FindIndex( vIndex, v => !v.IsAlive );
        if( vIndex >= 0 )
        {
          _values.RemoveAt( vIndex );
          _keys.RemoveAt( vIndex );
        }
      }
      while( vIndex >= 0 );
    }

    #endregion
  }
}
