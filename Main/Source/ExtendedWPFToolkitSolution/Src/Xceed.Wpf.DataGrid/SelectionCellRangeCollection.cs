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
  internal class SelectionCellRangeCollection : IList<SelectionCellRange>, IList
  {
    public SelectionCellRangeCollection( SelectedCellsStorage list )
    {
      m_list = list;
    }

    public SelectionCellRange this[ int index ]
    {
      get
      {
        return m_list[ index ].CellRange;
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
        this[ index ] = ( SelectionCellRange )value;
      }
    }

    int IList.Add( object item )
    {
      this.Add( ( SelectionCellRange )item );
      return m_list.Count - 1;
    }

    void IList.Clear()
    {
      this.Clear();
    }

    void IList.Insert( int index, object item )
    {
      this.Insert( index, ( SelectionCellRange )item );
    }

    void IList.Remove( object item )
    {
      this.Remove( ( SelectionCellRange )item );
    }

    void IList.RemoveAt( int index )
    {
      this.RemoveAt( index );
    }

    bool IList.Contains( object item )
    {
      return this.Contains( ( SelectionCellRange )item );
    }

    int IList.IndexOf( object item )
    {
      return this.IndexOf( ( SelectionCellRange )item );
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
        array.SetValue( m_list[ i ].CellRange, arrayIndex );
        arrayIndex++;
      }
    }

    #endregion ICollection Members

    #region IList<SelectionRange> Members

    public int IndexOf( SelectionCellRange item )
    {
      int count = m_list.Count;

      for( int i = 0; i < count; i++ )
      {
        if( m_list[ i ].CellRange == item )
          return i;
      }

      return -1;
    }

    public void Insert( int index, SelectionCellRange item )
    {
      if( ( index < 0 ) || ( index > m_list.Count ) )
        throw new ArgumentOutOfRangeException( "index", index, "index must be greater than or equal to zero and less than or equal to Count." );

      DataGridContext dataGridContext = m_list.DataGridContext;
      SelectionManager selectionManager = dataGridContext.DataGridControl.SelectionChangerManager;
      selectionManager.Begin();

      try
      {
        selectionManager.SelectCells( dataGridContext, new SelectionCellRangeWithItems( item.ItemRange, null, item.ColumnRange ) );
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
        SelectionCellRange cellRange = this[ index ];
        selectionManager.UnselectCells( dataGridContext, new SelectionCellRangeWithItems( cellRange.ItemRange, null, cellRange.ColumnRange ) );
      }
      finally
      {
        selectionManager.End( false, true, true );
      }
    }

    #endregion

    #region ICollection<SelectionRange> Members

    public void Add( SelectionCellRange item )
    {
      DataGridContext dataGridContext = m_list.DataGridContext;
      DataGridControl dataGridControl = dataGridContext.DataGridControl;

      if( dataGridControl.SelectionUnit == SelectionUnit.Row )
        throw new InvalidOperationException( "Can't add cell range when SelectionUnit is Row." );

      SelectionManager selectionManager = dataGridControl.SelectionChangerManager;
      selectionManager.Begin();

      try
      {
        selectionManager.SelectCells( dataGridContext, new SelectionCellRangeWithItems( item.ItemRange, null, item.ColumnRange ) );
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
        selectionManager.UnselectAllCells( dataGridContext );
      }
      finally
      {
        selectionManager.End( false, true, true );
      }
    }

    public bool Contains( SelectionCellRange item )
    {
      return this.IndexOf( item ) != -1;
    }

    public void CopyTo( SelectionCellRange[] array, int arrayIndex )
    {
      int count = m_list.Count;

      for( int i = 0; i < count; i++ )
      {
        array[ arrayIndex ] = m_list[ i ].CellRange;
        arrayIndex++;
      }
    }

    public bool Remove( SelectionCellRange item )
    {
      DataGridContext dataGridContext = m_list.DataGridContext;
      SelectionManager selectionManager = dataGridContext.DataGridControl.SelectionChangerManager;
      selectionManager.Begin();

      try
      {
        return selectionManager.UnselectCells( dataGridContext, new SelectionCellRangeWithItems( item.ItemRange, null, item.ColumnRange ) );
      }
      finally
      {
        selectionManager.End( false, true, true );
      }
    }

    #endregion

    #region IEnumerable<SelectionRange> Members

    public IEnumerator<SelectionCellRange> GetEnumerator()
    {
      // We use a foreach to get the exception when the list is changed
      foreach( SelectionCellRangeWithItems cellRangeWithItems in m_list )
      {
        yield return cellRangeWithItems.CellRange;
      }
    }

    #endregion

    #region IEnumerable Members

    IEnumerator IEnumerable.GetEnumerator()
    {
      return this.GetEnumerator();
    }

    #endregion

    private SelectedCellsStorage m_list;
  }
}
