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
  internal class SelectionItemRangeCollection : IList<SelectionRange>, IList
  {
    public SelectionItemRangeCollection( SelectedItemsStorage list )
    {
      m_list = list;
    }

    public SelectionRange this[ int index ]
    {
      get
      {
        return m_list[ index ].Range;
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
        return m_list.Count;
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

    int IList.Add( object item )
    {
      this.Add( ( SelectionRange )item );
      return m_list.Count - 1;
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
      int count = m_list.Count;

      for( int i = 0; i < count; i++ )
      {
        array.SetValue( m_list[ i ].Range, arrayIndex );
        arrayIndex++;
      }
    }

    #endregion ICollection Members

    #region IList<SelectionRange> Members

    public int IndexOf( SelectionRange item )
    {
      int count = m_list.Count;

      for( int i = 0; i < count; i++ )
      {
        if( m_list[ i ].Range == item )
          return i;
      }

      return -1;
    }

    public void Insert( int index, SelectionRange item )
    {
      if( ( index < 0 ) || ( index > m_list.Count ) )
        throw new ArgumentOutOfRangeException( "index", index, "index must be greater than or equal to zero and less than or equal to Count." );

      DataGridContext dataGridContext = m_list.DataGridContext;
      SelectionManager selectionManager = dataGridContext.DataGridControl.SelectionChangerManager;
      selectionManager.Begin();

      try
      {
        selectionManager.SelectItems( dataGridContext, new SelectionRangeWithItems( item, null ) );
      }
      finally
      {
        selectionManager.End( false, true, true );
      }
    }

    public void RemoveAt( int index )
    {
      if( ( index < 0 ) || ( index >= m_list.Count ) )
        throw new ArgumentOutOfRangeException( "index", index, "index must be greater than or equal to zero and less than Count." );

      DataGridContext dataGridContext = m_list.DataGridContext;
      SelectionManager selectionManager = dataGridContext.DataGridControl.SelectionChangerManager;
      selectionManager.Begin();

      try
      {
        selectionManager.UnselectItems( dataGridContext, new SelectionRangeWithItems( this[ index ], null ) );
      }
      finally
      {
        selectionManager.End( false, true, true );
      }
    }

    #endregion

    #region ICollection<SelectionRange> Members

    public void Add( SelectionRange item )
    {
      DataGridContext dataGridContext = m_list.DataGridContext;
      DataGridControl dataGridControl = dataGridContext.DataGridControl;

      if( dataGridControl.SelectionUnit == SelectionUnit.Cell )
        throw new InvalidOperationException( "Can't add item when SelectionUnit is Cell." );

      SelectionManager selectionManager = dataGridControl.SelectionChangerManager;
      selectionManager.Begin();

      try
      {
        selectionManager.SelectItems( dataGridContext, new SelectionRangeWithItems( item, null ) );
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

    public bool Contains( SelectionRange item )
    {
      return this.IndexOf( item ) != -1;
    }

    internal bool Contains( int itemIndex )
    {
      int count = m_list.Count;

      for( int i = 0; i < count; i++ )
      {
        SelectionRange range = m_list[ i ].Range;

        int startIndex = range.StartIndex;
        int endIndex = range.EndIndex;

        if( startIndex > endIndex )
        {
          startIndex = range.EndIndex;
          endIndex = range.StartIndex;
        }

        if( ( startIndex == itemIndex )
          || ( endIndex == itemIndex )
          || ( ( startIndex < itemIndex ) && ( itemIndex < endIndex ) ) )
        {
          return true;
        }
      }

      return false;
    }

    public void CopyTo( SelectionRange[] array, int arrayIndex )
    {
      int count = m_list.Count;

      for( int i = 0; i < count; i++ )
      {
        array[ arrayIndex ] = m_list[ i ].Range;
        arrayIndex++;
      }
    }

    public bool Remove( SelectionRange item )
    {
      DataGridContext dataGridContext = m_list.DataGridContext;
      SelectionManager selectionManager = dataGridContext.DataGridControl.SelectionChangerManager;
      selectionManager.Begin();

      try
      {
        return selectionManager.UnselectItems( dataGridContext, new SelectionRangeWithItems( item, null ) );
      }
      finally
      {
        selectionManager.End( false, true, true );
      }
    }

    #endregion

    #region IEnumerable<SelectionRange> Members

    public IEnumerator<SelectionRange> GetEnumerator()
    {
      // We use a foreach to get the exception when the list is changed
      foreach( SelectionRangeWithItems rangeWithItems in m_list )
      {
        yield return rangeWithItems.Range;
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
