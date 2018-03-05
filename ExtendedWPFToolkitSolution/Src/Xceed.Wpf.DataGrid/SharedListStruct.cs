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
using System.Collections.Generic;

namespace Xceed.Wpf.DataGrid
{
  // The idea behind this struct is to share the same "List" instance
  // (eg. m_internalList) across many "SharedList" instances 
  // (note: SharedList is a value type, so the reference is copied on every
  //  assignation) without having to create and copy of the list every time.
  // As long as each SharedList intances shares the same first items,
  // each instances can add new items to the list without affecting other instances.
  internal struct SharedList
  {
    public SharedList( int capacity )
    {
      m_internalList = new List<object>( capacity );
      m_localCount = 0;
    }

    public object this[ int index ]
    {
      get
      {
        if( index >= m_localCount )
          throw new ArgumentException( "Provided index is out of range", "index" );

        return m_internalList[ index ];
      }
    }

    public int Count
    {
      get
      {
        return m_localCount;
      }
    }

    public void EnsureCapacity( int capacity )
    {
      if( m_internalList.Capacity < capacity )
      {
        // Performance optimization. 
        // To avoid increasing to frequently, at least double the list capacity
        m_internalList.Capacity = Math.Max( m_internalList.Capacity * 2, capacity );
      }
    }

    public void Add( object item )
    {
      this.TrunkExcessiveContent( 1 );
      m_internalList.Add( item );
      m_localCount++;
    }

    public void AddRange( object[] e )
    {
      this.TrunkExcessiveContent( e.Length );
      m_internalList.AddRange( e );
      m_localCount += e.Length;
    }

    public object[] ToArray()
    {
      object[] newArray = new object[ m_localCount ];
      m_internalList.CopyTo( 0, newArray, 0, m_localCount );
      return newArray;
    }

    public void CopyTo( int startIndex, object[] array, int arrayIndex, int length )
    {
      if( ( startIndex < 0 )
        || ( length < 0 )
        || ( startIndex + length > m_localCount ) )
        throw new DataGridInternalException( "Specified range is invalid" );

      m_internalList.CopyTo( startIndex, array, arrayIndex, length );
    }

    private void TrunkExcessiveContent( int newListExtraCapacity )
    {
      if( m_localCount < m_internalList.Count )
      {
        // This is the scenario that should not happen very often 
        // where we loose the "same list copy" advantage. If we need
        // to add items to a list that already contains others "added" items,
        // we need to "branch" the list instance.
        var newList = new List<object>( m_localCount + newListExtraCapacity );
        for( int i = 0; i < m_localCount; i++ )
        {
          newList[ i ] = m_internalList[ i ];
        }
        m_internalList = newList;
      }
    }

    #region Fields

    private int m_localCount; //0
    private List<object> m_internalList;

    #endregion

  }
}
