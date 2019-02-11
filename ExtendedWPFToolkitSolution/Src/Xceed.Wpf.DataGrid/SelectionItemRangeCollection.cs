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

namespace Xceed.Wpf.DataGrid
{
  internal sealed class SelectionItemRangeCollection : IList<SelectionRange>, IList
  {
    internal SelectionItemRangeCollection( SelectedItemsStorage collection )
    {
      Debug.Assert( collection != null );

      m_storage = collection;
    }

    #region IList<> Members

    public SelectionRange this[ int index ]
    {
      get
      {
        return m_storage[ index ].Range;
      }
      set
      {
        throw new NotSupportedException();
      }
    }

    public int IndexOf( SelectionRange item )
    {
      var count = m_storage.Count;

      for( int i = 0; i < count; i++ )
      {
        if( m_storage[ i ].Range == item )
          return i;
      }

      return -1;
    }

    public void Insert( int index, SelectionRange item )
    {
      if( ( index < 0 ) || ( index > m_storage.Count ) )
        throw new ArgumentOutOfRangeException( "index", index, "index must be greater than or equal to zero and less than or equal to Count." );

      var dataGridContext = m_storage.DataGridContext;
      var dataGridControl = dataGridContext.DataGridControl;

      if( dataGridControl.SelectionUnit == SelectionUnit.Cell )
        throw new InvalidOperationException( "Can't add item when SelectionUnit is Cell." );

      if( !( dataGridContext.ItemsSourceCollection is DataGridVirtualizingCollectionViewBase ) )
      {
        var minIndex = Math.Min( item.StartIndex, item.EndIndex );
        var maxIndex = Math.Max( item.StartIndex, item.EndIndex );

        if( ( minIndex < 0 ) || ( maxIndex >= dataGridContext.Items.Count ) )
          throw new ArgumentException( "The selection range targets items outside of the data source.", "item" );
      }

      var selectionManager = dataGridControl.SelectionChangerManager;
      selectionManager.Begin();

      try
      {
        selectionManager.SelectItems( dataGridContext, new SelectionRangeWithItems( item, null ) );
      }
      finally
      {
        selectionManager.End( true, true );
      }
    }

    public void RemoveAt( int index )
    {
      if( ( index < 0 ) || ( index >= m_storage.Count ) )
        throw new ArgumentOutOfRangeException( "index", index, "index must be greater than or equal to zero and less than Count." );

      var dataGridContext = m_storage.DataGridContext;
      var selectionManager = dataGridContext.DataGridControl.SelectionChangerManager;
      selectionManager.Begin();

      try
      {
        selectionManager.UnselectItems( dataGridContext, new SelectionRangeWithItems( this[ index ], null ) );
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
        this[ index ] = ( SelectionRange )value;
      }
    }

    bool IList.IsReadOnly
    {
      get
      {
        return ( ( ICollection<SelectionRange> )this ).IsReadOnly;
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
      this.Add( ( SelectionRange )item );
      return this.Count - 1;
    }

    void IList.Clear()
    {
      this.Clear();
    }

    void IList.Insert( int index, object item )
    {
      this.Insert( index, ( SelectionRange )item );
    }

    void IList.Remove( object item )
    {
      this.Remove( ( SelectionRange )item );
    }

    void IList.RemoveAt( int index )
    {
      this.RemoveAt( index );
    }

    bool IList.Contains( object item )
    {
      return this.Contains( ( SelectionRange )item );
    }

    int IList.IndexOf( object item )
    {
      return this.IndexOf( ( SelectionRange )item );
    }

    #endregion

    #region ICollection<> Members

    public int Count
    {
      get
      {
        return m_storage.Count;
      }
    }

    bool ICollection<SelectionRange>.IsReadOnly
    {
      get
      {
        return false;
      }
    }

    public void Add( SelectionRange item )
    {
      this.Insert( m_storage.Count, item );
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

    public bool Contains( SelectionRange item )
    {
      return ( this.IndexOf( item ) >= 0 );
    }

    public bool Remove( SelectionRange item )
    {
      var dataGridContext = m_storage.DataGridContext;
      var dataGridControl = dataGridContext.DataGridControl;

      if( dataGridControl.SelectionUnit == SelectionUnit.Cell )
        throw new InvalidOperationException( "Can't remove item when SelectionUnit is Cell." );

      if( !( dataGridContext.ItemsSourceCollection is DataGridVirtualizingCollectionViewBase ) )
      {
        var minIndex = Math.Min( item.StartIndex, item.EndIndex );
        var maxIndex = Math.Max( item.StartIndex, item.EndIndex );

        if( ( minIndex < 0 ) || ( maxIndex >= dataGridContext.Items.Count ) )
          throw new ArgumentException( "The selection range targets items outside of the data source.", "item" );
      }

      var selectionManager = dataGridControl.SelectionChangerManager;
      selectionManager.Begin();

      try
      {
        return selectionManager.UnselectItems( dataGridContext, new SelectionRangeWithItems( item, null ) );
      }
      finally
      {
        selectionManager.End( true, true );
      }
    }

    public void CopyTo( SelectionRange[] array, int arrayIndex )
    {
      ( ( ICollection )this ).CopyTo( array, arrayIndex );
    }

    internal bool Contains( int itemIndex )
    {
      return m_storage.Contains( itemIndex );
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
      var count = m_storage.Count;

      for( int i = 0; i < count; i++ )
      {
        array.SetValue( m_storage[ i ].Range, arrayIndex );
        arrayIndex++;
      }
    }

    #endregion

    #region IEnumerable<> Members

    public IEnumerator<SelectionRange> GetEnumerator()
    {
      return ( from item in m_storage
               select item.Range ).GetEnumerator();
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
