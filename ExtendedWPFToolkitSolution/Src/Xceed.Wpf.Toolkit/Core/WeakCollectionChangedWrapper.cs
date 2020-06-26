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
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Specialized;
using Xceed.Wpf.Toolkit.Core.Utilities;

namespace Xceed.Wpf.Toolkit.Core
{
  internal class WeakCollectionChangedWrapper : IList, ICollection, INotifyCollectionChanged
  {
    private WeakEventListener<NotifyCollectionChangedEventArgs> _innerListListener;
    private IList _innerList;

    public WeakCollectionChangedWrapper( IList sourceList )
    {
      _innerList = sourceList;
      var notifyList = _innerList as INotifyCollectionChanged;
      if( notifyList != null )
      {
        _innerListListener = new WeakEventListener<NotifyCollectionChangedEventArgs>( OnInnerCollectionChanged );
        CollectionChangedEventManager.AddListener( notifyList, _innerListListener );
      }
    }

    public event NotifyCollectionChangedEventHandler CollectionChanged;

    private void OnInnerCollectionChanged( object sender, NotifyCollectionChangedEventArgs args )
    {
      if( this.CollectionChanged != null )
      {
        this.CollectionChanged( this, args );
      }
    }

    internal void ReleaseEvents()
    {
      if( _innerListListener != null )
      {
        CollectionChangedEventManager.RemoveListener( ( INotifyCollectionChanged )_innerList, _innerListListener );
        _innerListListener = null;
      }
    }

    #region IList Members

    int IList.Add( object value )
    {
      return _innerList.Add( value );
    }

    void IList.Clear()
    {
      _innerList.Clear();
    }

    bool IList.Contains( object value )
    {
      return _innerList.Contains( value );
    }

    int IList.IndexOf( object value )
    {
      return _innerList.IndexOf( value );
    }

    void IList.Insert( int index, object value )
    {
      _innerList.Insert( index, value );
    }

    bool IList.IsFixedSize
    {
      get { return _innerList.IsFixedSize; }
    }

    bool IList.IsReadOnly
    {
      get { return _innerList.IsReadOnly; }
    }

    void IList.Remove( object value )
    {
      _innerList.Remove( value );
    }

    void IList.RemoveAt( int index )
    {
      _innerList.RemoveAt( index );
    }

    object IList.this[ int index ]
    {
      get { return _innerList[ index ]; }
      set { _innerList[ index ] = value; }
    }
    #endregion

    #region ICollection Members

    void ICollection.CopyTo( Array array, int index )
    {
      _innerList.CopyTo( array, index );
    }

    int ICollection.Count
    {
      get { return _innerList.Count; }
    }

    bool ICollection.IsSynchronized
    {
      get { return _innerList.IsSynchronized; }
    }

    object ICollection.SyncRoot
    {
      get { return _innerList.SyncRoot; }
    }

    #endregion

    #region IEnumerable Members

    IEnumerator IEnumerable.GetEnumerator()
    {
      return _innerList.GetEnumerator();
    }

    #endregion

  }
}
