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
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Collections;

namespace Xceed.Wpf.DataGrid
{
  internal class SelectionItemCollection : IList<object>, IList
  {
    public SelectionItemCollection( SelectedItemsStorage list )
    {
      m_list = list;
    }

    public object this[ int index ]
    {
      get
      {
        if( ( index < 0 ) || ( index >= m_list.ItemsCount ) )
          throw new ArgumentOutOfRangeException( "index", "The index must be equal or greater than zero and less than Count." );

        int oldIndexOffset = 0;
        int indexOffset = 0;
        int count = m_list.Count;

        for( int i = 0; i < count; i++ )
        {
          SelectionRangeWithItems rangeWithItems = m_list[ i ];
          indexOffset += rangeWithItems.Length;

          if( index < indexOffset )
            return rangeWithItems.GetItem( m_list.DataGridContext, index - oldIndexOffset );

          oldIndexOffset = indexOffset;
        }

        throw new ArgumentOutOfRangeException( "index", "The index must be less than Count." );
      }
      set
      {
        throw new NotSupportedException();
      }
    }

    public int Count
    {
      get
      {
        return m_list.ItemsCount;
      }
    }

    public bool IsReadOnly
    {
      get
      {
        return false;
      }
    }

    public bool IsFixedSize
    {
      get
      {
        return false;
      }
    }

    #region IList Members

    int IList.Add( object item )
    {
      this.Add( item );
      return m_list.ItemsCount - 1;
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

    #endregion IList Members

    #region ICollection Members

    object ICollection.SyncRoot
    {
      get
      {
        return m_list;
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
      DataGridContext dataGridContext = m_list.DataGridContext;
      int count = m_list.Count;

      for( int i = 0; i < count; i++ )
      {
        SelectionRangeWithItems rangeWithItems = m_list[ i ];
        int rangeLength = rangeWithItems.Length;

        for( int j = 0; j < rangeLength; j++ )
        {
          array.SetValue( rangeWithItems.GetItem( dataGridContext, j ), arrayIndex );
          arrayIndex++;
        }
      }
    }

    #endregion ICollection Members

    #region IList<object> Members

    public int IndexOf( object item )
    {
      int indexOffset = 0;
      int count = m_list.Count;

      for( int i = 0; i < count; i++ )
      {
        SelectionRangeWithItems rangeWithItems = m_list[ i ];
        object[] items = rangeWithItems.Items;

        if( items != null )
        {
          int index = Array.IndexOf<object>( items, item );

          if( index != -1 )
            return indexOffset + index;
        }

        indexOffset += rangeWithItems.Length;
      }

      return -1;
    }

    public void Insert( int index, object item )
    {
      if( ( index < 0 ) || ( index > m_list.ItemsCount ) )
        throw new ArgumentOutOfRangeException( "index", index, "index must be greater than or equal to zero and less than or equal to Count." );

      DataGridContext dataGridContext = m_list.DataGridContext;
      SelectionManager selectionManager = dataGridContext.DataGridControl.SelectionChangerManager;
      selectionManager.Begin();

      try
      {
        selectionManager.SelectItems( dataGridContext, 
          new SelectionRangeWithItems( dataGridContext.Items.IndexOf( item ), item ) );
      }
      finally
      {
        selectionManager.End( false, true, true );
      }
    }

    public void RemoveAt( int index )
    {
      if( ( index < 0 ) || ( index >= m_list.ItemsCount ) )
        throw new ArgumentOutOfRangeException( "index", index, "index must be greater than or equal to zero and less than Count." );

      int oldIndexOffset = 0;
      int indexOffset = 0;
      int count = m_list.Count;
      SelectionRangeWithItems rangeWithItemsFound = SelectionRangeWithItems.Empty;

      for( int i = 0; i < count; i++ )
      {
        SelectionRangeWithItems rangeWithItems = m_list[ i ];
        indexOffset += rangeWithItems.Length;

        if( index < indexOffset )
        {
          rangeWithItemsFound = rangeWithItems; // .GetItem( dataGridContext, index - oldIndexOffset );
          break;
        }

        oldIndexOffset = indexOffset;
      }

      if( !rangeWithItemsFound.IsEmpty )
      {
        DataGridContext dataGridContext = m_list.DataGridContext;
        SelectionManager selectionManager = dataGridContext.DataGridControl.SelectionChangerManager;
        selectionManager.Begin();

        try
        {
          selectionManager.UnselectItems( dataGridContext, 
            new SelectionRangeWithItems(
              rangeWithItemsFound.Range.GetIndexFromItemOffset( index - oldIndexOffset ),
              rangeWithItemsFound.GetItem( dataGridContext, index - oldIndexOffset ) ) );
        }
        finally
        {
          selectionManager.End( false, true, true );
        }
      }
    }

    #endregion

    #region ICollection<object> Members

    public void Add( object item )
    {
      DataGridContext dataGridContext = m_list.DataGridContext;
      DataGridControl dataGridControl = dataGridContext.DataGridControl;

      if( dataGridControl.SelectionUnit == SelectionUnit.Cell )
        throw new InvalidOperationException( "Can't add item when SelectionUnit is Cell." ); 

      SelectionManager selectionManager = dataGridControl.SelectionChangerManager;
      selectionManager.Begin();

      try
      {
        selectionManager.SelectItems( dataGridContext, 
          new SelectionRangeWithItems( dataGridContext.Items.IndexOf( item ), item ) );
      }
      finally
      {
        selectionManager.End( false, true, true );
      }
    }

    public void Clear()
    {
      DataGridContext dataGridContext = m_list.DataGridContext;
      SelectionManager selectionManager = dataGridContext.DataGridControl.SelectionChangerManager;
      selectionManager.Begin();

      try
      {
        selectionManager.UnselectAllItems( dataGridContext );
      }
      finally
      {
        selectionManager.End( false, true, true );
      }
    }

    public bool Contains( object item )
    {
      return this.IndexOf( item ) != -1;
    }

    public void CopyTo( object[] array, int arrayIndex )
    {
      DataGridContext dataGridContext = m_list.DataGridContext;
      int count = m_list.Count;

      for( int i = 0; i < count; i++ )
      {
        SelectionRangeWithItems rangeWithItems = m_list[ i ];
        int rangeLength = rangeWithItems.Length;

        for( int j = 0; j < rangeLength; j++ )
        {
          array[ arrayIndex ] = rangeWithItems.GetItem( dataGridContext, j );
          arrayIndex++;
        }
      }
    }

    public bool Remove( object item )
    {
      DataGridContext dataGridContext = m_list.DataGridContext;
      SelectionManager selectionManager = dataGridContext.DataGridControl.SelectionChangerManager;
      selectionManager.Begin();

      try
      {
        return selectionManager.UnselectItems( dataGridContext, new SelectionRangeWithItems( SelectionRange.Empty, new object[] { item } ) );
      }
      finally
      {
        selectionManager.End( false, true, true );
      }
    }

    #endregion

    #region IEnumerable<SelectionRange> Members

    public IEnumerator<object> GetEnumerator()
    {
      DataGridContext dataGridContext = m_list.DataGridContext;

      // We use a foreach to get the exception when the list is changed
      foreach( SelectionRangeWithItems rangeWithItems in m_list )
      {
        int rangeLength = rangeWithItems.Length;

        for( int j = 0; j < rangeLength; j++ )
        {
          yield return rangeWithItems.GetItem( dataGridContext, j );
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

    private SelectedItemsStorage m_list;
  }
}
