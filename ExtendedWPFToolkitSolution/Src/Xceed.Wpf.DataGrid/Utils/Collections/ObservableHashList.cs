/************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2010-2012 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus edition at http://xceed.com/wpf_toolkit

   Visit http://xceed.com and follow @datagrid on Twitter

  **********************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Collections.Specialized;
using Xceed.Wpf.DataGrid;
using Xceed.Utils.Data;
using System.Diagnostics;
using System.ComponentModel;

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
    #region INTERNAL CONSTRUCTOR

    internal ObservableHashList()
    {
    }

    #endregion

    #region IList Members

    public int Add( object value )
    {
      if( !m_hashSet.Add( value ) )
      {
        string strValue = ( value == null ) ? "null" : value.ToString();
        throw new ArgumentException( "Item has already been added. (Key being added: '" + strValue + "')", "value" );
      }

      int index = m_list.Add( value );

      this.OnPropertyChanged( "Item[]" );
      this.OnPropertyChanged( "Count" );
      this.FireCollectionChanged( new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Add, value, index ) );
      return index;
    }

    public void Clear()
    {
      m_list.Clear();
      m_hashSet.Clear();
      this.OnPropertyChanged( "Item[]" );
      this.OnPropertyChanged( "Count" );
      this.FireCollectionChanged( new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Reset ) );
    }

    public bool Contains( object value )
    {
      return m_hashSet.Contains( value );
    }

    public int IndexOf( object value )
    {
      return m_list.IndexOf( value );
    }

    public void Insert( int index, object value )
    {
      if( !m_hashSet.Add( value ) )
      {
        string strValue = ( value == null ) ? "null" : value.ToString();
        throw new ArgumentException( "Item has already been added. (Key being added: '" + strValue + "')", "value" );
      }

      m_list.Insert( index, value );
      this.OnPropertyChanged( "Item[]" );
      this.OnPropertyChanged( "Count" );
      this.FireCollectionChanged( new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Add, value, index ) );
    }

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

    public void Remove( object value )
    {
      int index = this.IndexOf( value );

      if( index >= 0 )
      {
        m_hashSet.Remove( value );
        m_list.RemoveAt( index );
        this.OnPropertyChanged( "Item[]" );
        this.OnPropertyChanged( "Count" );
        this.FireCollectionChanged( new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Remove, value, index ) );
      }
    }

    public void RemoveAt( int index )
    {
      if( ( index < 0 ) || ( index >= this.Count ) )
        throw new ArgumentOutOfRangeException( "index" );

      object value = m_list[ index ];
      m_hashSet.Remove( value );
      m_list.RemoveAt( index );
      this.OnPropertyChanged( "Item[]" );
      this.OnPropertyChanged( "Count" );
      this.FireCollectionChanged( new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Remove, value, index ) );
    }

    public object this[ int index ]
    {
      get
      {
        return m_list[ index ];
      }
      set
      {
        object oldValue = m_list[ index ];
        m_list[ index ] = value;
        this.OnPropertyChanged( "Item[]" );
        this.OnPropertyChanged( "Count" );
        this.FireCollectionChanged( new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Replace, value, oldValue, index ) );
      }
    }

    #endregion

    #region ICollection Members

    public void CopyTo( Array array, int index )
    {
      m_list.CopyTo( array, index );
    }

    public int Count
    {
      get
      {
        return m_list.Count;
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
        return m_list.SyncRoot;
      }
    }

    #endregion

    #region IEnumerable Members

    public IEnumerator GetEnumerator()
    {
      return m_list.GetEnumerator();
    }

    #endregion

    #region INotifyCollectionChanged Members

    public event NotifyCollectionChangedEventHandler CollectionChanged;

    private void FireCollectionChanged( NotifyCollectionChangedEventArgs args )
    {
      if( m_deferOperationCount == 0 )
      {
        if( this.CollectionChanged != null )
        {
          this.CollectionChanged( this, args );
        }
      }
      else
      {
        m_operationsCount++;
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
      if( m_deferOperationCount == 0 )
      {
        if( this.PropertyChanged != null )
        {
          this.PropertyChanged( this, e );
        }
      }
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
      if( comparer == null )
        comparer = ObjectComparer.Singleton;

      lock( this.SyncRoot )
      {
        m_list.Sort( comparer );
        this.FireCollectionChanged( new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Reset ) );
      }
    }

    private HashSet<object> m_hashSet = new HashSet<object>();
    private ArrayList m_list = new ArrayList();
    private int m_deferOperationCount;
    private int m_operationsCount;

    #region DeferOperationNotifications class

    private class DeferOperationNotifications : IDisposable
    {
      public DeferOperationNotifications( ObservableHashList list )
      {
        m_innerList = list;
        m_innerList.m_deferOperationCount++;
      }

      public void Dispose()
      {
        if( m_innerList == null )
          return;

        m_innerList.m_deferOperationCount--;

        if( ( m_innerList.m_deferOperationCount == 0 ) && ( m_innerList.m_operationsCount > 0 ) )
        {
          // We always force a Reset after any deferred operation
          m_innerList.OnPropertyChanged( "Item[]" );
          m_innerList.OnPropertyChanged( "Count" );
          m_innerList.FireCollectionChanged( new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Reset ) );
          m_innerList.m_operationsCount = 0;
        }
        m_innerList = null;
      }

      private ObservableHashList m_innerList; // = null;
    }

    #endregion

    #region Debugger helper classes

    internal class ObservableHashListDebugView
    {
      private ObservableHashList m_hashList;

      public ObservableHashListDebugView( ObservableHashList hashlist )
      {
        if( hashlist == null )
        {
          throw new ArgumentNullException( "hashlist" );
        }
        m_hashList = hashlist;
      }

      public object[] Values
      {
        get
        {
          object[] values = new object[ m_hashList.m_hashSet.Count ];

          int i = 0;
          foreach( object key in m_hashList.m_hashSet )
          {
            values[ i ] = key;
            i++;
          }

          return values;
        }
      }
    }

    #endregion
  }
}
