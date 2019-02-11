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
using System.Linq;
using System.Threading;

namespace Xceed.Utils.Collections
{
  internal sealed class WeakDictionary<TKey, TValue> : IDictionary<TKey, TValue>, ICollection
  {
    internal WeakDictionary()
      : this( 0, null )
    {
    }

    internal WeakDictionary( int capacity )
      : this( capacity, null )
    {
    }

    internal WeakDictionary( IEqualityComparer<TKey> comparer )
      : this( 0, comparer )
    {
    }

    internal WeakDictionary( int capacity, IEqualityComparer<TKey> comparer )
    {
      if( capacity < 0 )
        throw new ArgumentOutOfRangeException( "capacity" );

      if( capacity > 0 )
      {
        this.EnsureCapacity( capacity );
      }

      m_comparer = comparer ?? EqualityComparer<TKey>.Default;
      m_creator = WeakDictionary<TKey, TValue>.GetCreator();

      Debug.Assert( m_comparer != null );
      Debug.Assert( m_creator != null );
    }

    #region Count Property

    public int Count
    {
      get
      {
        return m_size;
      }
    }

    #endregion

    #region Keys Property

    public ICollection<TKey> Keys
    {
      get
      {
        if( m_keys == null )
        {
          m_keys = new KeyCollection( this );
        }

        return m_keys;
      }
    }

    #endregion

    #region Values Property

    public ICollection<TValue> Values
    {
      get
      {
        if( m_values == null )
        {
          m_values = new ValueCollection( this );
        }

        return m_values;
      }
    }

    #endregion

    public void Add( TKey key, TValue value )
    {
      if( object.ReferenceEquals( key, null ) )
        throw new ArgumentNullException( "key" );

      if( this.ContainsKey( key ) )
        throw new ArgumentException();

      this.AddCore( key, value );
    }

    public bool ContainsKey( TKey key )
    {
      TValue unused;

      return this.TryGetValue( key, out unused );
    }

    public bool Remove( TKey key )
    {
      if( object.ReferenceEquals( key, null ) )
        throw new ArgumentNullException( "key" );

      return this.Remove( key, default( TValue ), false );
    }

    public void Clear()
    {
      if( m_size <= 0 )
        return;

      Debug.Assert( m_buckets != null );
      Debug.Assert( m_entries != null );
      Debug.Assert( m_entriesInfo != null );
      Debug.Assert( m_entries.Length == m_entriesInfo.Length );

      for( int i = 0; i < m_buckets.Length; i++ )
      {
        m_buckets[ i ] = -1;
      }

      for( int i = 0; i < m_entries.Length; i++ )
      {
        m_entries[ i ] = null;
        m_entriesInfo[ i ] = new EntryInfo( ( i < m_entries.Length - 1 ) ? i + 1 : -1 );
      }

      m_size = 0;
      m_free = 0;

      unchecked
      {
        m_version++;
      }
    }

    public bool TryGetValue( TKey key, out TValue value )
    {
      if( object.ReferenceEquals( key, null ) )
        throw new ArgumentNullException( "key" );

      value = default( TValue );

      var bucket = default( int );
      var index = default( int );
      var previous = default( int );

      return this.FindKey( key, out bucket, out index, out previous, out value );
    }

    private bool FindKey( TKey key, out int bucket, out int index, out int previous, out TValue value )
    {
      Debug.Assert( !object.ReferenceEquals( key, null ) );

      bucket = -1;
      index = -1;
      previous = -1;
      value = default( TValue );

      if( m_size <= 0 )
        return false;

      Debug.Assert( m_buckets != null );
      Debug.Assert( m_entries != null );
      Debug.Assert( m_entriesInfo != null );

      var hashCode = m_comparer.GetHashCode( key );

      bucket = this.GetBucket( hashCode );
      Debug.Assert( ( bucket >= 0 ) && ( bucket < m_buckets.Length ) );

      index = m_buckets[ bucket ];
      previous = -1;

      while( index >= 0 )
      {
        var entryInfo = m_entriesInfo[ index ];
        if( entryInfo.HashCode == hashCode )
        {
          TKey targetKey;
          var entry = m_entries[ index ];

          Debug.Assert( !WeakDictionary<TKey, TValue>.IsFree( entry ) );

          if( entry.TryGetKeyValue( out targetKey, out value ) && m_comparer.Equals( targetKey, key ) )
            return true;
        }

        previous = index;
        index = entryInfo.Next;
      }

      bucket = -1;
      index = -1;
      previous = -1;

      return false;
    }

    private bool ContainsValue( TValue value )
    {
      if( m_size <= 0 )
        return false;

      Debug.Assert( m_entries != null );

      var comparer = EqualityComparer<TValue>.Default;

      for( int i = 0; i < m_entries.Length; i++ )
      {
        var entry = m_entries[ i ];
        if( WeakDictionary<TKey, TValue>.IsFree( entry ) )
          continue;

        TKey targetKey;
        TValue targetValue;
        if( !entry.TryGetKeyValue( out targetKey, out targetValue ) )
          continue;

        if( comparer.Equals( targetValue, value ) )
          return true;
      }

      return false;
    }

    private bool Remove( TKey key, TValue value, bool compareValue )
    {
      Debug.Assert( !object.ReferenceEquals( key, null ) );

      var bucket = default( int );
      var index = default( int );
      var previous = default( int );
      var targetValue = default( TValue );

      if( !this.FindKey( key, out bucket, out index, out previous, out targetValue ) )
        return false;

      if( compareValue && !EqualityComparer<TValue>.Default.Equals( value, targetValue ) )
        return false;

      this.RemoveCore( bucket, index, previous );

      return true;
    }

    private void AddCore( TKey key, TValue value )
    {
      Debug.Assert( !object.ReferenceEquals( key, null ) );

      this.EnsureCapacity();
      Debug.Assert( m_buckets != null );
      Debug.Assert( m_entries != null );
      Debug.Assert( m_entriesInfo != null );
      Debug.Assert( m_free >= 0 );

      var index = m_free;
      var hashCode = m_comparer.GetHashCode( key );
      var bucket = this.GetBucket( hashCode );

      m_free = m_entriesInfo[ index ].Next;
      m_entries[ index ] = m_creator.Invoke( key, value );
      m_entriesInfo[ index ] = new EntryInfo( hashCode, m_buckets[ bucket ] );
      m_buckets[ bucket ] = index;
      m_size++;

      unchecked
      {
        m_version++;
      }
    }

    private void RemoveCore( int bucket, int index, int previous )
    {
      Debug.Assert( ( bucket >= 0 ) && ( bucket < m_buckets.Length ) );
      Debug.Assert( ( index >= 0 ) && ( index < m_entries.Length ) );
      Debug.Assert( previous < m_entries.Length );

      var next = m_entriesInfo[ index ].Next;

      if( m_buckets[ bucket ] == index )
      {
        Debug.Assert( previous < 0 );

        m_buckets[ bucket ] = next;
      }
      else
      {
        Debug.Assert( previous >= 0 );
        Debug.Assert( m_entriesInfo[ previous ].Next == index );

        m_entriesInfo[ previous ] = m_entriesInfo[ previous ].SetNext( next );
      }

      m_entries[ index ] = null;
      m_entriesInfo[ index ] = new EntryInfo( m_free );
      m_free = index;
      m_size--;

      unchecked
      {
        m_version++;
      }
    }

    private int GetBucket( int hashCode )
    {
      return WeakDictionary<TKey, TValue>.GetBucket( hashCode, m_capacity );
    }

    private void EnsureCapacity()
    {
      // Make sure the collection is not full.
      if( m_size != m_capacity )
        return;

      this.EnsureCapacity( m_capacity * 2L );
      Debug.Assert( m_free >= 0 );
      Debug.Assert( m_size != m_capacity );
    }

    private void EnsureCapacity( long min )
    {
      if( ( m_capacity >= min ) && ( m_capacity > 0 ) )
        return;

      this.RemoveDeadEntries();

      if( m_size * 2L < m_capacity )
        return;

      var capacity = WeakDictionary<TKey, TValue>.FindNextSize( min );
      Debug.Assert( capacity > 0 );

      m_buckets = new int[ capacity ];

      Array.Resize( ref m_entries, capacity );
      Array.Resize( ref m_entriesInfo, capacity );

      for( int i = 0; i < m_buckets.Length; i++ )
      {
        m_buckets[ i ] = -1;
      }

      for( int i = m_capacity; i < capacity; i++ )
      {
        m_entries[ i ] = null;
        m_entriesInfo[ i ] = new EntryInfo( ( i < capacity - 1 ) ? i + 1 : -1 );
      }

      // Rehash the elements to initialize the buckets.
      for( int i = 0; i < m_capacity; i++ )
      {
        // Do not rehash free entries.
        if( WeakDictionary<TKey, TValue>.IsFree( m_entries[ i ] ) )
          continue;

        var entryInfo = m_entriesInfo[ i ];
        var bucket = WeakDictionary<TKey, TValue>.GetBucket( entryInfo.HashCode, capacity );
        Debug.Assert( ( bucket >= 0 ) && ( bucket < capacity ) );

        var index = m_buckets[ bucket ];

        m_buckets[ bucket ] = i;
        m_entriesInfo[ i ] = entryInfo.SetNext( index );
      }

      // Link the remaining free entries to the newly created free entries.
      if( m_free >= 0 )
      {
        var index = m_free;

        while( true )
        {
          var next = m_entriesInfo[ index ].Next;
          if( next < 0 )
          {
            m_entriesInfo[ index ] = new EntryInfo( m_capacity );
            break;
          }

          index = next;
        }
      }
      else
      {
        m_free = m_capacity;
      }

      m_capacity = capacity;

      unchecked
      {
        m_version++;
      }
    }

    private void RemoveDeadEntries()
    {
      if( m_capacity <= 0 )
        return;

      Debug.Assert( m_buckets != null );
      Debug.Assert( m_entries != null );
      Debug.Assert( m_entriesInfo != null );

      for( int bucket = 0; bucket < m_buckets.Length; bucket++ )
      {
        var index = m_buckets[ bucket ];
        var previous = -1;

        while( index >= 0 )
        {
          var entry = m_entries[ index ];
          var entryInfo = m_entriesInfo[ index ];
          var next = entryInfo.Next;

          Debug.Assert( !WeakDictionary<TKey, TValue>.IsFree( entry ) );

          TKey targetKey;
          TValue targetValue;

          if( !entry.TryGetKeyValue( out targetKey, out targetValue ) )
          {
            this.RemoveCore( bucket, index, previous );
          }
          else
          {
            previous = index;
          }

          index = next;
        }
      }
    }

    private static int FindNextSize( long min )
    {
      var sizes = ArrayHelper.Sizes;

      for( int i = 0; i < sizes.Length; i++ )
      {
        var size = sizes[ i ];

        if( size >= min )
          return size;
      }

      throw new InvalidOperationException( "Cannot find a larger size." );
    }

    private static int GetBucket( int hashCode, int capacity )
    {
      Debug.Assert( capacity > 0 );

      // Remove the negative sign without using Math.Abs to handle the case of int.MinValue.
      return ( hashCode & 0x7fffffff ) % capacity;
    }

    private static bool IsFree( Entry entry )
    {
      return ( entry == null );
    }

    private static Func<TKey, TValue, Entry> GetCreator()
    {
      var isKeyRef = !typeof( TKey ).IsValueType;
      var isValueRef = !typeof( TValue ).IsValueType;

      if( isKeyRef && isValueRef )
        return ( k, v ) => new WWEntry( k, v );

      if( isKeyRef )
        return ( k, v ) => new WHEntry( k, v );

      if( isValueRef )
        return ( k, v ) => new HWEntry( k, v );

      return ( k, v ) => new HHEntry( k, v );
    }

    #region IDictionary<> Members

    TValue IDictionary<TKey, TValue>.this[ TKey key ]
    {
      get
      {
        throw new NotSupportedException();
      }
      set
      {
        throw new NotSupportedException();
      }
    }

    #endregion

    #region ICollection<> Members

    bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly
    {
      get
      {
        return false;
      }
    }

    void ICollection<KeyValuePair<TKey, TValue>>.Add( KeyValuePair<TKey, TValue> item )
    {
      var key = item.Key;
      if( object.ReferenceEquals( key, null ) )
        throw new ArgumentException( "item" );

      this.Add( key, item.Value );
    }

    bool ICollection<KeyValuePair<TKey, TValue>>.Contains( KeyValuePair<TKey, TValue> item )
    {
      var key = item.Key;
      if( object.ReferenceEquals( key, null ) )
        throw new ArgumentException( "item" );

      TValue value;

      if( !this.TryGetValue( key, out value ) )
        return false;

      return EqualityComparer<TValue>.Default.Equals( item.Value, value );
    }

    void ICollection<KeyValuePair<TKey, TValue>>.CopyTo( KeyValuePair<TKey, TValue>[] array, int index )
    {
      throw new NotSupportedException();
    }

    bool ICollection<KeyValuePair<TKey, TValue>>.Remove( KeyValuePair<TKey, TValue> item )
    {
      var key = item.Key;
      if( object.ReferenceEquals( key, null ) )
        throw new ArgumentException( "item" );

      return this.Remove( key, item.Value, true );
    }

    #endregion

    #region ICollection Members

    bool ICollection.IsSynchronized
    {
      get
      {
        return false;
      }
    }

    object ICollection.SyncRoot
    {
      get
      {
        if( m_syncRoot == null )
        {
          Interlocked.CompareExchange( ref m_syncRoot, new object(), null );
          Debug.Assert( m_syncRoot != null );
        }

        return m_syncRoot;
      }
    }

    void ICollection.CopyTo( Array array, int index )
    {
      throw new NotSupportedException();
    }

    #endregion

    #region IEnumerable<> Members

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
      if( m_capacity <= 0 )
        yield break;

      Debug.Assert( m_entries != null );

      var version = m_version;

      for( int i = 0; i < m_entries.Length; i++ )
      {
        var entry = m_entries[ i ];

        if( !WeakDictionary<TKey, TValue>.IsFree( entry ) )
        {
          TKey key;
          TValue value;

          if( entry.TryGetKeyValue( out key, out value ) )
            yield return new KeyValuePair<TKey, TValue>( key, value );
        }

        if( version != m_version )
          throw new InvalidOperationException();
      }
    }

    #endregion

    #region IEnumerable Members

    IEnumerator IEnumerable.GetEnumerator()
    {
      return this.GetEnumerator();
    }

    #endregion

    private readonly IEqualityComparer<TKey> m_comparer;
    private readonly Func<TKey, TValue, Entry> m_creator;
    private ICollection<TKey> m_keys; //null
    private ICollection<TValue> m_values; //null
    private object m_syncRoot;
    private int[] m_buckets;
    private Entry[] m_entries;
    private EntryInfo[] m_entriesInfo;
    private int m_capacity; //0
    private int m_size; //0
    private int m_free = -1;
    private int m_version; //0

    #region Entry Private Class

    private abstract class Entry
    {
      internal abstract bool TryGetKey( out TKey value );
      internal abstract bool TryGetValue( out TValue value );

      internal bool TryGetKeyValue( out TKey key, out TValue value )
      {
        if( this.TryGetKey( out key ) )
          return this.TryGetValue( out value );

        value = default( TValue );

        return false;
      }
    }

    #endregion

    #region WWEntry Private Class

    private sealed class WWEntry : Entry
    {
      internal WWEntry( TKey key, TValue value )
      {
        Debug.Assert( !object.ReferenceEquals( key, null ) );

        m_key = new WeakReference( key );
        m_value = ( object.ReferenceEquals( value, null ) )
                    ? null
                    : new WeakReference( value );
      }

      internal override bool TryGetKey( out TKey value )
      {
        value = ( TKey )m_key.Target;

        return !object.ReferenceEquals( value, null );
      }

      internal override bool TryGetValue( out TValue value )
      {
        if( m_value != null )
        {
          value = ( TValue )m_value.Target;

          return !object.ReferenceEquals( value, null );
        }

        value = default( TValue );
        return true;
      }

      private readonly WeakReference m_key;
      private readonly WeakReference m_value;
    }

    #endregion

    #region WHEntry Private Class

    private sealed class WHEntry : Entry
    {
      internal WHEntry( TKey key, TValue value )
      {
        Debug.Assert( !object.ReferenceEquals( key, null ) );

        m_key = new WeakReference( key );
        m_value = value;
      }

      internal override bool TryGetKey( out TKey value )
      {
        value = ( TKey )m_key.Target;

        return !object.ReferenceEquals( value, null );
      }

      internal override bool TryGetValue( out TValue value )
      {
        value = m_value;
        return true;
      }

      private readonly WeakReference m_key;
      private readonly TValue m_value;
    }

    #endregion

    #region HWEntry Private Class

    private sealed class HWEntry : Entry
    {
      internal HWEntry( TKey key, TValue value )
      {
        m_key = key;
        m_value = ( object.ReferenceEquals( value, null ) )
                    ? null
                    : new WeakReference( value );
      }

      internal override bool TryGetKey( out TKey value )
      {
        value = m_key;
        return true;
      }

      internal override bool TryGetValue( out TValue value )
      {
        if( m_value != null )
        {
          value = ( TValue )m_value.Target;

          return !object.ReferenceEquals( value, null );
        }

        value = default( TValue );
        return true;
      }

      private readonly TKey m_key;
      private readonly WeakReference m_value;
    }

    #endregion

    #region HHEntry Private Class

    private sealed class HHEntry : Entry
    {
      internal HHEntry( TKey key, TValue value )
      {
        m_key = key;
        m_value = value;
      }

      internal override bool TryGetKey( out TKey value )
      {
        value = m_key;
        return true;
      }

      internal override bool TryGetValue( out TValue value )
      {
        value = m_value;
        return true;
      }

      private readonly TKey m_key;
      private readonly TValue m_value;
    }

    #endregion

    #region EntryInfo Private Struct

    [DebuggerDisplay( "HashCode = {HashCode}, Next = {Next}" )]
    private struct EntryInfo
    {
      internal EntryInfo( int hashCode, int next )
      {
        m_hashCode = hashCode;
        m_next = next;
      }

      internal EntryInfo( int next )
        : this( 0, next )
      {
      }

      internal int HashCode
      {
        get
        {
          return m_hashCode;
        }
      }

      internal int Next
      {
        get
        {
          return m_next;
        }
      }

      internal EntryInfo SetNext( int next )
      {
        return new EntryInfo( m_hashCode, next );
      }

      private readonly int m_hashCode;
      private readonly int m_next;
    }

    #endregion

    #region KeyCollection Private Class

    private sealed class KeyCollection : ICollection<TKey>
    {
      internal KeyCollection( WeakDictionary<TKey, TValue> owner )
      {
        Debug.Assert( owner != null );
        m_owner = owner;
      }

      public int Count
      {
        get
        {
          return m_owner.Count;
        }
      }

      bool ICollection<TKey>.IsReadOnly
      {
        get
        {
          return true;
        }
      }

      void ICollection<TKey>.Add( TKey item )
      {
        throw new NotSupportedException();
      }

      void ICollection<TKey>.Clear()
      {
        throw new NotSupportedException();
      }

      public bool Contains( TKey item )
      {
        return m_owner.ContainsKey( item );
      }

      void ICollection<TKey>.CopyTo( TKey[] array, int index )
      {
        throw new NotSupportedException();
      }

      bool ICollection<TKey>.Remove( TKey item )
      {
        throw new NotSupportedException();
      }

      public IEnumerator<TKey> GetEnumerator()
      {
        return m_owner.Select( item => item.Key ).GetEnumerator();
      }

      IEnumerator IEnumerable.GetEnumerator()
      {
        return this.GetEnumerator();
      }

      private readonly WeakDictionary<TKey, TValue> m_owner;
    }

    #endregion

    #region ValueCollection Private Class

    private sealed class ValueCollection : ICollection<TValue>
    {
      internal ValueCollection( WeakDictionary<TKey, TValue> owner )
      {
        Debug.Assert( owner != null );
        m_owner = owner;
      }

      public int Count
      {
        get
        {
          return m_owner.Count;
        }
      }

      bool ICollection<TValue>.IsReadOnly
      {
        get
        {
          return true;
        }
      }

      void ICollection<TValue>.Add( TValue item )
      {
        throw new NotSupportedException();
      }

      void ICollection<TValue>.Clear()
      {
        throw new NotSupportedException();
      }

      public bool Contains( TValue item )
      {
        return m_owner.ContainsValue( item );
      }

      void ICollection<TValue>.CopyTo( TValue[] array, int arrayIndex )
      {
        throw new NotSupportedException();
      }

      bool ICollection<TValue>.Remove( TValue item )
      {
        throw new NotSupportedException();
      }

      public IEnumerator<TValue> GetEnumerator()
      {
        return m_owner.Select( item => item.Value ).GetEnumerator();
      }

      IEnumerator IEnumerable.GetEnumerator()
      {
        return this.GetEnumerator();
      }

      private readonly WeakDictionary<TKey, TValue> m_owner;
    }

    #endregion
  }
}
