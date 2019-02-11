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
using System.ComponentModel;
using System.Collections;

using Xceed.Utils.Data;
using System.Windows.Data;

namespace Xceed.Wpf.DataGrid
{
  internal class SortDescriptionInfo
  {
    public SortDescriptionInfo( DataGridItemPropertyBase property, ListSortDirection direction )
    {
      m_property = property;
      m_direction = direction;
    }

    #region SortDirection Property

    public ListSortDirection SortDirection
    {
      get
      {
        return m_direction;
      }
    }

    #endregion

    #region Property Property

    public DataGridItemPropertyBase Property
    {
      get
      {
        return m_property;
      }
    }

    #endregion

    #region DataStore Property

    public DataStore DataStore
    {
      get
      {
        return m_dataStore;
      }
      set
      {
        m_dataStore = value;
      }
    }

    #endregion

    #region SortComparer Property

    public IComparer SortComparer
    {
      get
      {
        if( m_property == null )
          return null;

        return m_property.SortComparer;
      }
    }

    #endregion

    public bool IsReverseOf( SortDescriptionInfo sortDescriptionInfo )
    {
      if( sortDescriptionInfo.m_property == m_property )
      {
        switch( sortDescriptionInfo.m_direction )
        {
          case ListSortDirection.Ascending:
            return m_direction == ListSortDirection.Descending;

          case ListSortDirection.Descending:
            return m_direction == ListSortDirection.Ascending;
        }
      }

      return false;
    }

    public override bool Equals( object obj )
    {
      SortDescriptionInfo sortDescriptionInfo = obj as SortDescriptionInfo;

      if( sortDescriptionInfo == null )
        return false;

      if( ( sortDescriptionInfo.m_direction == m_direction ) &&
        ( sortDescriptionInfo.m_property == m_property ) )
      {
        return true;
      }

      return false;
    }

    public override int GetHashCode()
    {
      return ( m_property.GetHashCode() ) ^ ( ( int )m_direction << 14 );
    }

    private ListSortDirection m_direction; // Initialize in constructor
    private DataGridItemPropertyBase m_property; // Initialize in constructor
    private DataStore m_dataStore; // = null
  }
}
