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
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Xceed.Wpf.DataGrid;

namespace Xceed.Utils.Collections
{
  [Serializable]
  [DebuggerTypeProxy( typeof( Xceed.Utils.Collections.ObservableHashList.ObservableHashListDebugView ) )]
  [DebuggerDisplay( "Count = {Count}" )]
  [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Design", "CA1039:ListsAreStronglyTyped" )]
  [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Design", "CA1035:ICollectionImplementationsHaveStronglyTypedMembers" )]
  [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix" )]
  public sealed class ObservableHashList : IList, INotifyCollectionChanged, INotifyPropertyChanged
  {
    #region CONSTANTS

    private const int UseHashSetThreshold = 10;

    #endregion

    #region INTERNAL CONSTRUCTOR

    internal ObservableHashList()
    {
    }

    #endregion

    #region IList Members

    public bool IsFixedSize
    {
      get
      {
        return false;
      }
    }

    public bool IsReadOnly
    {
      get
      {
        return false;
      }
    }

    public int Add( object value )
    {
      int index = this.Count;
      this.InsertCore( index, value );

      return index;
    }

    public void Clear()
    {
      m_items = null;
      m_set = null;
      this.OnPropertyChanged( "Item[]" );
      this.OnPropertyChanged( "Count" );
      this.FireCollectionChanged( new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Reset ) );
    }

    public bool Contains( object value )
    {
      if( m_set != null )
        return m_set.Contains( value );

      if( m_items != null )
        return m_items.Contains( value );

      return false;
    }

    public int IndexOf( object value )
    {
      if( m_items == null )
        return -1;

      return m_items.IndexOf( value );
    }

    public void Insert( int index, object value )
    {
      this.InsertCore( index, value );
    }

    public void Remove( object value )
    {
      int index = this.IndexOf( value );
      if( index < 0 )
        return;

      this.RemoveAtCore( index );
    }

    public void RemoveAt( int index )
    {
      this.RemoveAtCore( index );
    }

    public object this[ int index ]
    {
      get
      {
        if( ( index < 0 ) || ( index >= this.Count ) )
          throw new ArgumentOutOfRangeException( "index" );

        Debug.Assert( m_items != null );

        return m_items[ index ];
      }
      set
      {
        int count = this.Count;
        if( ( index < 0 ) || ( index > count ) )
          throw new ArgumentOutOfRangeException( "index" );

        if( index == count )
        {
          this.InsertCore( index, value );
        }
        else
        {
          this.ReplaceCore( index, value );
        }
      }
    }

    #endregion

    #region ICollection Members

    public int Count
    {
      get
      {
        if( m_items == null )
          return 0;

        return m_items.Count;
      }
    }

    public bool IsSynchronized
    {
      get
      {
        return false;
      }
    }

    public object SyncRoot
    {
      get
      {
        if( m_syncRoot == null )
        {
          Interlocked.CompareExchange<object>( ref m_syncRoot, new object(), null );
        }

        return m_syncRoot;
      }
    }

    public void CopyTo( Array array, int index )
    {
      if( m_items == null )
        return;

      IList items = m_items;
      items.CopyTo( array, index );
    }

    #endregion

    #region IEnumerable Members

    public IEnumerator GetEnumerator()
    {
      if( m_items == null )
        return Enumerable.Empty<object>().GetEnumerator();

      return m_items.GetEnumerator();
    }

    #endregion

    #region INotifyCollectionChanged Members

    public event NotifyCollectionChangedEventHandler CollectionChanged;

    private void FireCollectionChanged( NotifyCollectionChangedEventArgs args )
    {
      if( m_deferOperationCount == 0 )
      {
        var handler = this.CollectionChanged;
        if( handler == null )
          return;

        handler.Invoke( this, args );
      }
      else
      {
        m_deferOperationCount = m_deferOperationCount.DeferOperation();
      }
    }

    #endregion

    #region INotifyPropertyChanged Members

    public event PropertyChangedEventHandler PropertyChanged;

    private void OnPropertyChanged( string propertyName )
    {
      this.OnPropertyChanged( new PropertyChangedEventArgs( propertyName ) );
    }

    private void OnPropertyChanged( PropertyChangedEventArgs e )
    {
      if( m_deferOperationCount != 0 )
        return;

      var handler = this.PropertyChanged;
      if( handler == null )
        return;

      handler.Invoke( this, e );
    }

    #endregion

    public IDisposable DeferINotifyCollectionChanged()
    {
      return new ObservableHashList.DeferOperationNotifications( this );
    }

    public void Sort()
    {
      this.Sort( null );
    }

    public void Sort( IComparer comparer )
    {
      if( m_items == null )
        return;

      var comparerWrapper = new ComparerWrapper( ( comparer != null ) ? comparer : ObjectComparer.Singleton );
      m_items.Sort( comparerWrapper );

      this.FireCollectionChanged( new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Reset ) );
    }

    private void InsertCore( int index, object value )
    {
      int count = this.Count;
      if( ( index < 0 ) || ( index > count ) )
        throw new ArgumentOutOfRangeException( "index" );

      if( m_items == null )
      {
        Debug.Assert( count == 0 );
        m_items = new List<object>() { value };
      }
      else
      {
        if( ( m_set == null ) && ( count >= ObservableHashList.UseHashSetThreshold ) )
          m_set = new HashSet<object>( m_items );

        if( this.Contains( value ) )
          throw new ArgumentException( string.Format( "Item has already been added. (Key being added: '{0}')", "value", ( value == null ) ? "null" : value.ToString() ) );

        m_items.Insert( index, value );

        if( m_set != null )
        {
          var result = m_set.Add( value );
          Debug.Assert( result );
        }
      }

      this.OnPropertyChanged( "Item[]" );
      this.OnPropertyChanged( "Count" );
      this.FireCollectionChanged( new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Add, value, index ) );
    }

    private void RemoveAtCore( int index )
    {
      int count = this.Count;
      if( ( index < 0 ) || ( index >= count ) )
        throw new ArgumentOutOfRangeException( "index" );

      Debug.Assert( m_items != null );

      object value = m_items[ index ];

      if( count > 1 )
      {
        m_items.RemoveAt( index );
      }
      else
      {
        m_items = null;
      }

      if( m_set != null )
      {
        if( count > ObservableHashList.UseHashSetThreshold )
        {
          Debug.Assert( m_items != null );

          m_set.Remove( value );
        }
        else
        {
          m_set = null;
        }
      }

      this.OnPropertyChanged( "Item[]" );
      this.OnPropertyChanged( "Count" );
      this.FireCollectionChanged( new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Remove, value, index ) );
    }

    private void ReplaceCore( int index, object value )
    {
      Debug.Assert( ( index >= 0 ) && ( index < this.Count ) );
      Debug.Assert( m_items != null );

      if( m_items[ index ] == value )
        return;

      if( this.Contains( value ) )
        throw new ArgumentException( string.Format( "Item has already been added. (Key being added: '{0}')", "value", ( value == null ) ? "null" : value.ToString() ) );

      var oldValue = m_items[ index ];
      m_items[ index ] = value;

      if( m_set != null )
      {
        m_set.Remove( oldValue );
        m_set.Add( value );
      }

      this.OnPropertyChanged( "Item[]" );
      this.OnPropertyChanged( "Count" );
      this.FireCollectionChanged( new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Replace, value, oldValue, index ) );
    }

    #region Private Fields

    private List<object> m_items; // = null
    private HashSet<object> m_set; // = null
    private DeferCount m_deferOperationCount;
    private object m_syncRoot; // = null

    #endregion

    #region DeferOperationNotifications Class

    private class DeferOperationNotifications : IDisposable
    {
      public DeferOperationNotifications( ObservableHashList owner )
      {
        Debug.Assert( owner != null );

        m_owner = owner;
        m_owner.m_deferOperationCount++;
      }

      public void Dispose()
      {
        this.Dispose( true );
        GC.SuppressFinalize( this );
      }

      private void Dispose( bool disposing )
      {
        var target = m_owner;
        if( target == null )
          return;

        target = Interlocked.CompareExchange( ref m_owner, null, target );
        if( target == null )
          return;

        target.m_deferOperationCount--;

        if( target.m_deferOperationCount.HasDeferredOperation && ( target.m_deferOperationCount == 0 ) )
        {
          // We always force a Reset after any deferred operation
          target.m_deferOperationCount = new DeferCount();
          target.OnPropertyChanged( "Item[]" );
          target.OnPropertyChanged( "Count" );
          target.FireCollectionChanged( new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Reset ) );
        }
      }

      ~DeferOperationNotifications()
      {
        this.Dispose( false );
      }

      private ObservableHashList m_owner; // = null;
    }

    #endregion

    #region Debugger Helper Class

    internal class ObservableHashListDebugView
    {
      public ObservableHashListDebugView( ObservableHashList list )
      {
        if( list == null )
          throw new ArgumentNullException( "list" );

        m_list = list;
      }

      public object[] Values
      {
        get
        {
          var items = m_list.m_items;
          if( items == null )
            return new object[ 0 ];

          return items.ToArray();
        }
      }

      private readonly ObservableHashList m_list;
    }

    #endregion

    #region ComparerWrapper Private Class

    private sealed class ComparerWrapper : IComparer<object>
    {
      internal ComparerWrapper( IComparer comparer )
      {
        Debug.Assert( comparer != null );

        m_comparer = comparer;
      }

      public int Compare( object x, object y )
      {
        return m_comparer.Compare( x, y );
      }

      private readonly IComparer m_comparer;
    }

    #endregion

    #region DeferCount Private Nested Type

    private struct DeferCount
    {
      private const uint DeferredOperationMask = 1u << 31;
      private const uint CountMask = ~DeferCount.DeferredOperationMask;

      private DeferCount( int count, bool hasDeferredOperation )
      {
        if( count < 0 )
          throw new ArgumentException( "count" );

        uint data = ( uint )count;
        if( hasDeferredOperation )
        {
          data |= DeferCount.DeferredOperationMask;
        }

        this.Data = data;
      }

      public int Count
      {
        get
        {
          return Convert.ToInt32( this.Data & DeferCount.CountMask );
        }
      }

      public bool HasDeferredOperation
      {
        get
        {
          return ( this.Data & DeferCount.DeferredOperationMask ) != 0u;
        }
      }

      public DeferCount DeferOperation()
      {
        if( this.HasDeferredOperation )
          return this;

        return new DeferCount( this.Count, true );
      }

      public static DeferCount operator ++( DeferCount source )
      {
        return source + 1;
      }

      public static DeferCount operator --( DeferCount source )
      {
        return source - 1;
      }

      public static DeferCount operator +( DeferCount source, int value )
      {
        if( value == 0 )
          return source;

        return new DeferCount( source.Count + value, source.HasDeferredOperation );
      }

      public static DeferCount operator -( DeferCount source, int value )
      {
        return source + ( -value );
      }

      public static implicit operator int( DeferCount source )
      {
        return source.Count;
      }

      public static implicit operator DeferCount( int count )
      {
        return new DeferCount( count, false );
      }

      private readonly uint Data;
    }

    #endregion
  }
}
