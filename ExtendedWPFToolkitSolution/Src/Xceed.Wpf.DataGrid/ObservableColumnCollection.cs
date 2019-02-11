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
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using Xceed.Utils.Wpf;

namespace Xceed.Wpf.DataGrid
{
  public class ObservableColumnCollection : ObservableCollection<ColumnBase>
  {
    #region [] Property

    public ColumnBase this[ string fieldName ]
    {
      get
      {
        if( fieldName != null )
        {
          ColumnBase value;
          if( m_fieldNameToColumn.TryGetValue( fieldName, out value ) )
            return value;
        }

        return null;
      }
    }

    #endregion

    #region SyncRoot Private Property

    private object SyncRoot
    {
      get
      {
        return ( ( ICollection )this ).SyncRoot;
      }
    }

    #endregion

    // This method has better performance than the one on the base class.
    public new bool Contains( ColumnBase item )
    {
      if( item == null )
        return false;

      return object.ReferenceEquals( item, this[ item.FieldName ] );
    }

    protected virtual void OnItemAdding( ColumnBase item )
    {
    }

    protected virtual void OnItemAdded( ColumnBase item )
    {
    }

    protected virtual void OnItemRemoving( ColumnBase item )
    {
    }

    protected virtual void OnItemRemoved( ColumnBase item )
    {
    }

    protected override void RemoveItem( int index )
    {
      var item = this[ index ];
      Debug.Assert( item != null );

      this.OnItemRemovingCore( item );

      base.RemoveItem( index );

      this.OnItemRemovedCore( item );
    }

    protected override void InsertItem( int index, ColumnBase item )
    {
      if( item == null )
        throw new ArgumentNullException( "item" );

      var fieldName = item.FieldName;
      if( string.IsNullOrEmpty( fieldName ) )
        throw new ArgumentException( "A column must have a fieldname.", "item" );

      if( m_fieldNameToColumn.ContainsKey( fieldName ) )
        throw new DataGridException( "A column with same field name already exists in collection." );

      this.OnItemAddingCore( item );

      base.InsertItem( index, item );

      this.OnItemAddedCore( item );
    }

    protected override void ClearItems()
    {
      var items = new List<ColumnBase>( this );

      foreach( var item in items )
      {
        this.OnItemRemovingCore( item );
      }

      Debug.Assert( m_fieldNameToColumn.Count == 0 );
      m_fieldNameToColumn.Clear();

      base.ClearItems();

      foreach( var item in items )
      {
        this.OnItemRemovedCore( item );
      }
    }

    protected override void SetItem( int index, ColumnBase item )
    {
      if( item == null )
        throw new ArgumentNullException( "item" );

      var fieldName = item.FieldName;
      if( string.IsNullOrEmpty( fieldName ) )
        throw new ArgumentException( "A column must have a fieldname.", "item" );

      var oldItem = this[ index ];
      Debug.Assert( oldItem != null );

      if( oldItem.FieldName != fieldName )
      {
        if( m_fieldNameToColumn.ContainsKey( fieldName ) )
          throw new DataGridException( "A column with same field name already exists in collection." );
      }

      this.OnItemRemovingCore( oldItem );
      this.OnItemAddingCore( item );

      base.SetItem( index, item );

      this.OnItemRemovedCore( oldItem );
      this.OnItemAddedCore( item );
    }

    protected override void OnPropertyChanged( PropertyChangedEventArgs e )
    {
      lock( this.SyncRoot )
      {
        if( m_deferCount != 0 )
        {
          if( m_deferPropertyChangedEventArgs == null )
          {
            m_deferPropertyChangedEventArgs = e;
          }
          else if( !string.IsNullOrEmpty( m_deferPropertyChangedEventArgs.PropertyName ) && !object.Equals( m_deferPropertyChangedEventArgs.PropertyName, e.PropertyName ) )
          {
            m_deferPropertyChangedEventArgs = new PropertyChangedEventArgs( string.Empty );
          }

          return;
        }
      }

      base.OnPropertyChanged( e );
    }

    protected override void OnCollectionChanged( NotifyCollectionChangedEventArgs e )
    {
      lock( this.SyncRoot )
      {
        if( m_deferCount != 0 )
        {
          if( m_deferCollectionChangedEventArgs == null )
          {
            m_deferCollectionChangedEventArgs = e;
          }
          else if( m_deferCollectionChangedEventArgs.Action != NotifyCollectionChangedAction.Reset )
          {
            m_deferCollectionChangedEventArgs = new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Reset );
          }

          return;
        }
      }

      base.OnCollectionChanged( e );
    }

    internal IDisposable DeferNotifications()
    {
      return new DeferredDisposable( new DeferState( this ) );
    }

    private void OnItemAddingCore( ColumnBase item )
    {
      m_fieldNameToColumn.Add( item.FieldName, item );

      this.OnItemAdding( item );
    }

    private void OnItemAddedCore( ColumnBase item )
    {
      this.OnItemAdded( item );
    }

    private void OnItemRemovingCore( ColumnBase item )
    {
      var fieldName = item.FieldName;

      this.OnItemRemoving( item );

      m_fieldNameToColumn.Remove( fieldName );
    }

    private void OnItemRemovedCore( ColumnBase item )
    {
      this.OnItemRemoved( item );
    }

    private int m_deferCount; //0
    private PropertyChangedEventArgs m_deferPropertyChangedEventArgs; //null
    private NotifyCollectionChangedEventArgs m_deferCollectionChangedEventArgs; //null

    private readonly Dictionary<string, ColumnBase> m_fieldNameToColumn = new Dictionary<string, ColumnBase>();

    #region DeferState Private Class

    private sealed class DeferState : DeferredDisposableState
    {
      internal DeferState( ObservableColumnCollection target )
      {
        Debug.Assert( target != null );
        m_target = target;
      }

      protected override object SyncRoot
      {
        get
        {
          return m_target.SyncRoot;
        }
      }

      protected override bool IsDeferred
      {
        get
        {
          return ( m_target.m_deferCount != 0 );
        }
      }

      protected override void Increment()
      {
        m_target.m_deferCount++;
      }

      protected override void Decrement()
      {
        m_target.m_deferCount--;
      }

      protected override void OnDeferEnding( bool disposing )
      {
        m_propertyChangedEventArgs = m_target.m_deferPropertyChangedEventArgs;
        m_collectionChangedEventArgs = m_target.m_deferCollectionChangedEventArgs;
        m_target.m_deferPropertyChangedEventArgs = null;
        m_target.m_deferCollectionChangedEventArgs = null;

        base.OnDeferEnding( disposing );
      }

      protected override void OnDeferEnded( bool disposing )
      {
        if( m_collectionChangedEventArgs != null )
        {
          m_target.OnCollectionChanged( m_collectionChangedEventArgs );
        }

        if( m_propertyChangedEventArgs != null )
        {
          m_target.OnPropertyChanged( m_propertyChangedEventArgs );
        }
      }

      private readonly ObservableColumnCollection m_target;
      private PropertyChangedEventArgs m_propertyChangedEventArgs;
      private NotifyCollectionChangedEventArgs m_collectionChangedEventArgs;
    }

    #endregion
  }
}
