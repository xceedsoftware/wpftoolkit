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

namespace Xceed.Wpf.DataGrid
{
  internal sealed class SelectionItemCollection : IList<object>, IList
  {
    internal SelectionItemCollection( SelectedItemsStorage collection )
    {
      m_storage = collection;
    }

    private bool FindItem( int index, DataGridContext dataGridContext, int itemsCount, out SelectionRangeWithItems range, out int offset )
    {
      var rangesCount = m_storage.Count;

      range = SelectionRangeWithItems.Empty;
      offset = index;

      for( int i = 0; i < rangesCount; i++ )
      {
        if( offset < 0 )
          break;

        range = m_storage[ i ];
        var length = range.Length;

        if( offset < length )
        {
          var startAt = range.Range.StartIndex;
          var endAt = range.Range.EndIndex;

          if( startAt > endAt )
          {
            if( startAt - offset >= itemsCount )
              break;
          }
          else
          {
            if( startAt + offset >= itemsCount )
              break;
          }

          Debug.Assert( ( offset >= 0 ) && ( offset < length ) );

          return true;
        }
        else
        {
          offset -= length;
        }
      }

      return false;
    }

    private IEnumerable<object> EnumerateItems( SelectionRangeWithItems range, DataGridContext dataGridContext, int itemsCount )
    {
      var startAt = range.Range.StartIndex;
      var endAt = range.Range.EndIndex;
      var min = Math.Min( startAt, endAt );
      var max = Math.Max( startAt, endAt );

      if( startAt > endAt )
      {
        var upperLimit = Math.Min( startAt, itemsCount - 1 );

        for( int i = upperLimit; i >= endAt; i-- )
        {
          object item;

          try
          {
            item = range.GetItem( dataGridContext, startAt - i );
          }
          catch( ArgumentOutOfRangeException )
          {
            continue;
          }

          yield return item;
        }
      }
      else
      {
        var upperLimit = Math.Min( endAt, itemsCount - 1 );

        for( int i = startAt; i <= upperLimit; i++ )
        {
          object item;

          try
          {
            item = range.GetItem( dataGridContext, i - startAt );
          }
          catch( ArgumentOutOfRangeException )
          {
            break;
          }

          yield return item;
        }
      }
    }

    #region IList<> Members

    public object this[ int index ]
    {
      get
      {
        if( ( index < 0 ) || ( index >= m_storage.ItemsCount ) )
          throw new ArgumentOutOfRangeException( "index", "The index must be equal or greater than zero and less than Count." );

        var dataGridContext = m_storage.DataGridContext;
        var itemsCount = dataGridContext.Items.Count;
        var range = SelectionRangeWithItems.Empty;
        var offset = default( int );

        if( this.FindItem( index, dataGridContext, itemsCount, out range, out offset ) )
        {
          try
          {
            return range.GetItem( dataGridContext, offset );
          }
          catch( ArgumentOutOfRangeException )
          {
            // return null
          }
        }

        return null;
      }
      set
      {
        throw new NotSupportedException();
      }
    }

    public int IndexOf( object item )
    {
      var rangesCount = m_storage.Count;
      if( rangesCount <= 0 )
        return -1;

      var dataGridContext = m_storage.DataGridContext;
      var itemsCount = default( int? );
      var offset = 0;

      for( int i = 0; i < rangesCount; i++ )
      {
        var range = m_storage[ i ];
        if( range.Items != null )
        {
          var index = 0;

          if( !itemsCount.HasValue )
          {
            itemsCount = dataGridContext.Items.Count;
          }

          foreach( var target in this.EnumerateItems( range, dataGridContext, itemsCount.Value ) )
          {
            if( object.Equals( target, item ) )
              return offset + index;

            index++;
          }
        }

        offset += range.Length;
      }

      return -1;
    }

    public void Insert( int index, object item )
    {
      if( ( index < 0 ) || ( index > m_storage.ItemsCount ) )
        throw new ArgumentOutOfRangeException( "index", index, "index must be greater than or equal to zero and less than or equal to Count." );

      var dataGridContext = m_storage.DataGridContext;
      var dataGridControl = dataGridContext.DataGridControl;

      if( dataGridControl.SelectionUnit == SelectionUnit.Cell )
        throw new InvalidOperationException( "Can't add item when SelectionUnit is Cell." );

      var selectionManager = dataGridControl.SelectionChangerManager;
      selectionManager.Begin();

      try
      {
        selectionManager.SelectItems( dataGridContext, new SelectionRangeWithItems( dataGridContext.Items.IndexOf( item ), item ) );
      }
      finally
      {
        selectionManager.End( true, true );
      }
    }

    public void RemoveAt( int index )
    {
      if( ( index < 0 ) || ( index >= m_storage.ItemsCount ) )
        throw new ArgumentOutOfRangeException( "index", index, "index must be greater than or equal to zero and less than Count." );

      var dataGridContext = m_storage.DataGridContext;
      var itemsCount = dataGridContext.Items.Count;
      var range = SelectionRangeWithItems.Empty;
      var offset = default( int );

      if( !this.FindItem( index, dataGridContext, itemsCount, out range, out offset ) )
        return;

      object item;
      int itemIndex;

      try
      {
        item = range.GetItem( dataGridContext, offset );
        itemIndex = range.Range.GetIndexFromItemOffset( offset );
      }
      catch( ArgumentOutOfRangeException )
      {
        return;
      }

      var selectionManager = dataGridContext.DataGridControl.SelectionChangerManager;
      selectionManager.Begin();

      try
      {
        selectionManager.UnselectItems( dataGridContext, new SelectionRangeWithItems( itemIndex, item ) );
      }
      finally
      {
        selectionManager.End( true, true );
      }
    }

    #endregion

    #region IList Members

    object IList.this[ int index ]
    {
      get
      {
        return this[ index ];
      }
      set
      {
        this[ index ] = value;
      }
    }

    bool IList.IsReadOnly
    {
      get
      {
        return ( ( ICollection<object> )this ).IsReadOnly;
      }
    }

    bool IList.IsFixedSize
    {
      get
      {
        return false;
      }
    }

    int IList.Add( object item )
    {
      this.Add( item );
      return this.Count - 1;
    }

    void IList.Clear()
    {
      this.Clear();
    }

    void IList.Insert( int index, object item )
    {
      this.Insert( index, item );
    }

    void IList.Remove( object item )
    {
      this.Remove( item );
    }

    void IList.RemoveAt( int index )
    {
      this.RemoveAt( index );
    }

    bool IList.Contains( object item )
    {
      return this.Contains( item );
    }

    int IList.IndexOf( object item )
    {
      return this.IndexOf( item );
    }

    #endregion

    #region ICollection<> Members

    public int Count
    {
      get
      {
        return m_storage.ItemsCount;
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
      this.Insert( m_storage.ItemsCount, item );
    }

    public void Clear()
    {
      var dataGridContext = m_storage.DataGridContext;
      var selectionManager = dataGridContext.DataGridControl.SelectionChangerManager;
      selectionManager.Begin();

      try
      {
        selectionManager.UnselectAllItems( dataGridContext );
      }
      finally
      {
        selectionManager.End( true, true );
      }
    }

    public bool Contains( object item )
    {
      return ( this.IndexOf( item ) >= 0 );
    }

    public bool Remove( object item )
    {
      var dataGridContext = m_storage.DataGridContext;
      var dataGridControl = dataGridContext.DataGridControl;

      if( dataGridControl.SelectionUnit == SelectionUnit.Cell )
        throw new InvalidOperationException( "Can't remove item when SelectionUnit is Cell." );

      var selectionManager = dataGridControl.SelectionChangerManager;
      selectionManager.Begin();

      try
      {
        return selectionManager.UnselectItems( dataGridContext, new SelectionRangeWithItems( SelectionRange.Empty, new object[] { item } ) );
      }
      finally
      {
        selectionManager.End( true, true );
      }
    }

    public void CopyTo( object[] array, int arrayIndex )
    {
      ( ( ICollection )this ).CopyTo( array, arrayIndex );
    }

    #endregion

    #region ICollection Members

    int ICollection.Count
    {
      get
      {
        return this.Count;
      }
    }

    object ICollection.SyncRoot
    {
      get
      {
        return m_storage;
      }
    }

    bool ICollection.IsSynchronized
    {
      get
      {
        return false;
      }
    }

    void ICollection.CopyTo( Array array, int arrayIndex )
    {
      using( var enumerator = this.GetEnumerator() )
      {
        while( enumerator.MoveNext() )
        {
          array.SetValue( enumerator.Current, arrayIndex );
          arrayIndex++;
        }
      }
    }

    #endregion

    #region IEnumerable<> Members

    public IEnumerator<object> GetEnumerator()
    {
      if( m_storage.Count <= 0 )
        yield break;

      var dataGridContext = m_storage.DataGridContext;
      var itemsCount = dataGridContext.Items.Count;

      // We use a foreach to get the exception when the collection is changed.
      foreach( var range in m_storage )
      {
        foreach( var item in this.EnumerateItems( range, dataGridContext, itemsCount ) )
        {
          yield return item;
        }
      }
    }

    #endregion

    #region IEnumerable Members

    IEnumerator IEnumerable.GetEnumerator()
    {
      return this.GetEnumerator();
    }

    #endregion

    private readonly SelectedItemsStorage m_storage;
  }
}
