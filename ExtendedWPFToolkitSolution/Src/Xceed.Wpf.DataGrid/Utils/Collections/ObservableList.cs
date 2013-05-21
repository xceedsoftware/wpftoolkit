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
using System.Text;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.Collections;

namespace Xceed.Utils.Collections
{
  internal class ObservableList<T> : IList<T>, IList, ICollection, INotifyCollectionChanged
  {
    public ObservableList()
    {
      m_wrappedlist = new List<T>();
    }

    public ObservableList( IEnumerable<T> collection )
    {
      m_wrappedlist = new List<T>( collection );
    }

    public ObservableList( int capacity )
    {
      m_wrappedlist = new List<T>( capacity );
    }

    public void AddRange( IEnumerable<T> collection )
    {
      int startingIndex = m_wrappedlist.Count;

      m_wrappedlist.AddRange( collection );

      if( this.CollectionChanged != null )
      {
        List<T> newItems = new List<T>( collection );

        this.CollectionChanged( this, new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Add, newItems, startingIndex ) );
      }
    }

    public ReadOnlyCollection<T> AsReadOnly()
    {
      return m_wrappedlist.AsReadOnly();
    }

    public int BinarySearch( T item )
    {
      return m_wrappedlist.BinarySearch( item );
    }

    public int BinarySearch( T item, IComparer<T> comparer )
    {
      return m_wrappedlist.BinarySearch( item, comparer );
    }

    public int BinarySearch( int index, int count, T item, IComparer<T> comparer )
    {
      return m_wrappedlist.BinarySearch( index, count, item, comparer );
    }

    public List<TOutput> ConvertAll<TOutput>( Converter<T, TOutput> converter )
    {
      return m_wrappedlist.ConvertAll<TOutput>( converter );
    }

    public void CopyTo( T[] array )
    {
      m_wrappedlist.CopyTo( array );
    }

    public void CopyTo( int index, T[] array, int arrayIndex, int count )
    {
      m_wrappedlist.CopyTo( index, array, arrayIndex, count );
    }

    public bool Exists( Predicate<T> match )
    {
      return m_wrappedlist.Exists( match );
    }

    public T Find( Predicate<T> match )
    {
      return m_wrappedlist.Find( match );
    }

    public List<T> FindAll( Predicate<T> match )
    {
      return m_wrappedlist.FindAll( match );
    }

    public int FindIndex( Predicate<T> match )
    {
      return m_wrappedlist.FindIndex( match );
    }

    public int FindIndex( int startIndex, Predicate<T> match )
    {
      return m_wrappedlist.FindIndex( startIndex, match );
    }

    public int FindIndex( int startIndex, int count, Predicate<T> match )
    {
      return m_wrappedlist.FindIndex( startIndex, count, match );
    }

    public T FindLast( Predicate<T> match )
    {
      return m_wrappedlist.FindLast( match );
    }

    public int FindLastIndex( Predicate<T> match )
    {
      return m_wrappedlist.FindLastIndex( match );
    }

    public int FindLastIndex( int startIndex, Predicate<T> match )
    {
      return m_wrappedlist.FindLastIndex( startIndex, match );
    }

    public int FindLastIndex( int startIndex, int count, Predicate<T> match )
    {
      return m_wrappedlist.FindLastIndex( startIndex, count, match );
    }

    public void ForEach( Action<T> action )
    {
      m_wrappedlist.ForEach( action );
    }

    public List<T> GetRange( int index, int count )
    {
      return m_wrappedlist.GetRange( index, count );
    }

    public int IndexOf( T item, int index )
    {
      return m_wrappedlist.IndexOf( item, index );
    }

    public int IndexOf( T item, int index, int count )
    {
      return m_wrappedlist.IndexOf( item, index, count );
    }

    public void InsertRange( int index, IEnumerable<T> collection )
    {
      m_wrappedlist.InsertRange( index, collection );

      if( this.CollectionChanged != null )
      {
        List<object> newItems = new List<object>( ( IEnumerable<object> )collection );

        NotifyCollectionChangedEventArgs eventArgs = new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Add, newItems, index );

        this.CollectionChanged( this, eventArgs );
      }
    }

    public int LastIndexOf( T item )
    {
      return m_wrappedlist.LastIndexOf( item );
    }

    public int LastIndexOf( T item, int index )
    {
      return m_wrappedlist.LastIndexOf( item, index );
    }

    public int LastIndexOf( T item, int index, int count )
    {
      return m_wrappedlist.LastIndexOf( item, index, count );
    }

    public int RemoveAll( Predicate<T> match )
    {
      int retval = m_wrappedlist.RemoveAll( match );

      if( this.CollectionChanged != null )
      {
        // Not choices but to send a reset... since I do not know what items were removed.
        NotifyCollectionChangedEventArgs eventArgs = new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Reset );

        this.CollectionChanged( this, eventArgs );
      }

      return retval;
    }

    public void RemoveRange( int index, int count )
    {
      List<T> removedItems = this.GetRange( index, count );

      m_wrappedlist.RemoveRange( index, count );

      if( this.CollectionChanged != null )
      {
        NotifyCollectionChangedEventArgs eventArgs = new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Remove, removedItems, index );

        this.CollectionChanged( this, eventArgs );
      }
    }

    public void Reverse()
    {
      m_wrappedlist.Reverse();
    }

    public void Reverse( int index, int count )
    {
      m_wrappedlist.Reverse( index, count );
    }

    public void Sort()
    {
      m_wrappedlist.Sort();
    }

    public void Sort( IComparer<T> comparer )
    {
      m_wrappedlist.Sort( comparer );
    }

    public void Sort( Comparison<T> comparison )
    {
      m_wrappedlist.Sort( comparison );
    }

    public void Sort( int index, int count, IComparer<T> comparer )
    {
      m_wrappedlist.Sort( index, count, comparer );
    }

    #region IList<T> Members

    public int IndexOf( T item )
    {
      return m_wrappedlist.IndexOf( item );
    }

    public void Insert( int index, T item )
    {
      m_wrappedlist.Insert( index, item );

      if( this.CollectionChanged != null )
      {
        NotifyCollectionChangedEventArgs eventArgs = new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Add, item, index );

        this.CollectionChanged( this, eventArgs );
      }
    }

    public void RemoveAt( int index )
    {
      T removedItem = this[ index ];

      m_wrappedlist.RemoveAt( index );

      if( this.CollectionChanged != null )
      {
        NotifyCollectionChangedEventArgs eventArgs = new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Remove, removedItem, index );

        this.CollectionChanged( this, eventArgs );
      }
    }

    public T this[ int index ]
    {
      get
      {
        return m_wrappedlist[ index ];
      }
      set
      {
        T oldItem = m_wrappedlist[ index ];

        m_wrappedlist[ index ] = value;

        if( this.CollectionChanged != null )
        {
          NotifyCollectionChangedEventArgs eventArgs = new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Replace, value, oldItem );

          this.CollectionChanged( this, eventArgs );
        }
      }
    }

    #endregion

    #region ICollection<T> Members

    public void Add( T item )
    {
      m_wrappedlist.Add( item );

      if( this.CollectionChanged != null )
      {
        // -1 since the item is already added to the list
        NotifyCollectionChangedEventArgs eventArgs = new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Add, item, this.Count - 1 );

        this.CollectionChanged( this, eventArgs );
      }
    }

    public void Clear()
    {
      m_wrappedlist.Clear();

      if( this.CollectionChanged != null )
      {
        NotifyCollectionChangedEventArgs eventArgs = new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Reset );

        this.CollectionChanged( this, eventArgs );
      }
    }

    public bool Contains( T item )
    {
      return m_wrappedlist.Contains( item );
    }

    public void CopyTo( T[] array, int arrayIndex )
    {
      m_wrappedlist.CopyTo( array, arrayIndex );
    }

    public int Count
    {
      get
      {
        return m_wrappedlist.Count;
      }
    }

    bool ICollection<T>.IsReadOnly
    {
      get
      {
        return ( ( ICollection<T> )m_wrappedlist ).IsReadOnly;
      }
    }

    public bool Remove( T item )
    {
      bool retval = false;

      int oldIndex = m_wrappedlist.IndexOf( item );

      retval = m_wrappedlist.Remove( item );

      if( ( retval == true ) && ( this.CollectionChanged != null ) )
      {
        NotifyCollectionChangedEventArgs eventArgs = new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Remove, item, oldIndex );

        this.CollectionChanged( this, eventArgs );
      }

      return retval;
    }

    #endregion

    #region IEnumerable<T> Members

    public IEnumerator<T> GetEnumerator()
    {
      return ( ( IEnumerable<T> )m_wrappedlist ).GetEnumerator();
    }

    #endregion

    #region IEnumerable Members

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return ( ( System.Collections.IEnumerable )m_wrappedlist ).GetEnumerator();
    }

    #endregion

    #region INotifyCollectionChanged Members

    public event NotifyCollectionChangedEventHandler CollectionChanged;

    #endregion

    #region IList Members

    int IList.Add( object value )
    {
      if( IsCompatibleObject( value ) == false )
        throw new ArgumentException( "value must be of type T." );

      this.Add( ( T )value );

      return ( this.Count - 1 );
    }

    bool IList.Contains( object value )
    {
      if( IsCompatibleObject( value ) == false )
        throw new ArgumentException( "value must be of type T." );

      return this.Contains( ( T )value );
    }

    int IList.IndexOf( object value )
    {
      if( IsCompatibleObject( value ) == false )
        throw new ArgumentException( "value must be of type T." );

      return this.IndexOf( ( T )value );
    }

    void IList.Insert( int index, object value )
    {
      if( IsCompatibleObject( value ) == false )
        throw new ArgumentException( "value must be of type T." );

      this.Insert( index, ( T )value );
    }

    bool IList.IsFixedSize
    {
      get
      {
        return ( ( IList )m_wrappedlist ).IsFixedSize;
      }
    }

    bool IList.IsReadOnly
    {
      get
      {
        return ( ( IList )m_wrappedlist ).IsReadOnly;
      }
    }

    void IList.Remove( object value )
    {
      if( IsCompatibleObject( value ) == false )
        throw new ArgumentException( "value must be of type T." );

      this.Remove( ( T )value );
    }

    object IList.this[ int index ]
    {
      get
      {
        return this[ index ];
      }
      set
      {
        if( IsCompatibleObject( value ) == false )
          throw new ArgumentException( "value must be of type T." );

        this[ index ] = ( T )value;
      }
    }

    #endregion

    #region ICollection Members

    void ICollection.CopyTo( Array array, int index )
    {
      ( ( ICollection )m_wrappedlist ).CopyTo( array, index );
    }

    bool ICollection.IsSynchronized
    {
      get
      {
        return ( ( ICollection )m_wrappedlist ).IsSynchronized;
      }
    }

    object ICollection.SyncRoot
    {
      get
      {
        return ( ( ICollection )m_wrappedlist ).SyncRoot;
      }
    }

    #endregion

    private static bool IsCompatibleObject( object value )
    {
      if( !( value is T ) && ( ( value != null ) || typeof( T ).IsValueType ) )
      {
        return false;
      }
      return true;
    }

    private List<T> m_wrappedlist;

  }
}
