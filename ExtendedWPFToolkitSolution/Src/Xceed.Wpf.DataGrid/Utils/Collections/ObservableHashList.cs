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
    private const int UseListThreshold = 8;

    #endregion

    internal ObservableHashList()
    {
    }

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

    public object this[ int index ]
    {
      get
      {
        return m_items[ index ];
      }
      set
      {
        var count = this.Count;
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

    public int Add( object value )
    {
      int index = this.Count;
      this.InsertCore( index, value );

      return index;
    }

    public void Clear()
    {
      if( this.Count == 0 )
        return;

      m_set = null;
      m_items.Clear();

      this.OnCollectionChanged( new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Reset ) );
    }

    public bool Contains( object value )
    {
      if( m_set != null )
        return m_set.Contains( value );

      return m_items.Contains( value );
    }

    public int IndexOf( object value )
    {
      if( ( m_set != null ) && !m_set.Contains( value ) )
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

    #endregion

    #region ICollection Members

    public int Count
    {
      get
      {
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
        return ( ( ICollection )m_items ).SyncRoot;
      }
    }

    public void CopyTo( Array array, int index )
    {
      ( ( IList )m_items ).CopyTo( array, index );
    }

    #endregion

    #region IEnumerable Members

    public IEnumerator GetEnumerator()
    {
      return m_items.GetEnumerator();
    }

    #endregion

    #region INotifyCollectionChanged Members

    public event NotifyCollectionChangedEventHandler CollectionChanged;

    private void OnCollectionChanged( NotifyCollectionChangedEventArgs e )
    {
      var handler = this.CollectionChanged;
      if( handler == null )
        return;

      if( m_deferCount == 0 )
      {
        this.OnPropertyChanged( "Item[]" );
        this.OnPropertyChanged( "Count" );

        handler.Invoke( this, e );
      }
      else
      {
        m_hasDeferredNotifications = true;
      }
    }

    #endregion

    #region INotifyPropertyChanged Members

    public event PropertyChangedEventHandler PropertyChanged;

    private void OnPropertyChanged( string propertyName )
    {
      var handler = this.PropertyChanged;
      if( handler == null )
        return;

      handler.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
    }

    #endregion

    public IDisposable DeferINotifyCollectionChanged()
    {
      return new DeferDisposable( this );
    }

    public void Sort()
    {
      this.Sort( null );
    }

    public void Sort( IComparer comparer )
    {
      var comparerWrapper = new ComparerWrapper( ( comparer != null ) ? comparer : ObjectComparer.Singleton );
      m_items.Sort( comparerWrapper );

      this.OnCollectionChanged( new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Reset ) );
    }

    private void InsertCore( int index, object value )
    {
      var count = this.Count;
      if( ( index < 0 ) || ( index > count ) )
        throw new ArgumentOutOfRangeException( "index" );

      this.EnsureNotIn( value );

      if( ( m_set == null ) && ( count >= ObservableHashList.UseHashSetThreshold ) )
      {
        m_set = new HashSet<object>( m_items );
      }

      m_items.Insert( index, value );

      if( m_set != null )
      {
        m_set.Add( value );
      }

      this.OnCollectionChanged( new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Add, value, index ) );
    }

    private void RemoveAtCore( int index )
    {
      var count = this.Count;
      if( ( index < 0 ) || ( index >= count ) )
        throw new ArgumentOutOfRangeException( "index" );

      var value = m_items[ index ];

      m_items.RemoveAt( index );

      if( m_set != null )
      {
        if( count > ObservableHashList.UseListThreshold )
        {
          m_set.Remove( value );
        }
        else
        {
          m_set = null;
        }
      }

      this.OnCollectionChanged( new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Remove, value, index ) );
    }

    private void ReplaceCore( int index, object value )
    {
      Debug.Assert( ( index >= 0 ) && ( index < this.Count ) );

      if( m_items[ index ] == value )
        return;

      this.EnsureNotIn( value );

      var oldValue = m_items[ index ];
      m_items[ index ] = value;

      if( m_set != null )
      {
        m_set.Remove( oldValue );
        m_set.Add( value );
      }

      this.OnCollectionChanged( new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Replace, value, oldValue, index ) );
    }

    private void EnsureNotIn( object value )
    {
      if( this.Contains( value ) )
        throw new ArgumentException( string.Format( "Item has already been added. (Key being added: '{0}')", "value", ( value == null ) ? "null" : value.ToString() ) );
    }

    #region Private Fields

    private readonly List<object> m_items = new List<object>();
    private HashSet<object> m_set; //null
    private int m_deferCount;
    private bool m_hasDeferredNotifications; //false

    #endregion

    #region DeferDisposable Private Class

    private sealed class DeferDisposable : IDisposable
    {
      internal DeferDisposable( ObservableHashList owner )
      {
        Debug.Assert( owner != null );

        m_owner = owner;
        m_owner.m_deferCount++;
      }

      public void Dispose()
      {
        this.Dispose( true );
        GC.SuppressFinalize( this );
      }

      private void Dispose( bool disposing )
      {
        var target = Interlocked.Exchange( ref m_owner, null );
        if( target == null )
          return;

        target.m_deferCount--;
        if( target.m_deferCount != 0 )
          return;

        if( target.m_hasDeferredNotifications )
        {
          target.m_hasDeferredNotifications = false;
          target.OnCollectionChanged( new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Reset ) );
        }
      }

      ~DeferDisposable()
      {
        this.Dispose( false );
      }

      private ObservableHashList m_owner; // = null;
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

    #region ObservableHashListDebugView Private Class

    private sealed class ObservableHashListDebugView
    {
      internal ObservableHashListDebugView( ObservableHashList owner )
      {
        if( owner == null )
          throw new ArgumentNullException( "list" );

        m_owner = owner;
      }

      public object[] Values
      {
        get
        {
          var items = m_owner.m_items;
          if( items == null )
            return new object[ 0 ];

          return items.ToArray();
        }
      }

      private readonly ObservableHashList m_owner;
    }

    #endregion
  }
}
